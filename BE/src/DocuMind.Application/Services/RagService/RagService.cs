using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.Rag;
using DocuMind.Application.Interface.IRag;
using DocuMind.Core.Interfaces.IEmbedding;
using DocuMind.Core.Interfaces.ILLM;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Core.Interfaces.IVectorDb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocuMind.Application.Services.Rag
{
    public class RagService : IRagService
    {

        private readonly IEmbeddingService _embeddingService;
        private readonly IChatSessionRepository _chatRepository;
        private readonly IVectorDbService _vectorDbService;
        private readonly ILlmService _llmService;
        private readonly ILogger<RagService> _logger;
        private const int TOP_K = 10;
        private const float SCORE_THRESHOLD = 0.6f;


        public RagService(
            IEmbeddingService embeddingService,
            IVectorDbService vectorDbService,
            ILlmService llmService,
            IChatSessionRepository chatRepository,
            ILogger<RagService> logger)
        {
            _chatRepository = chatRepository;
            _embeddingService = embeddingService;
            _vectorDbService = vectorDbService;
            _llmService = llmService;
            _logger = logger;

            //Configurable RAG parameters
        /*   _topK = int.Parse(configuration["RagSettings:TopK"] ?? "5");
            _scoreThreshold = float.Parse(configuration["RagSettings:ScoreThreshold"] ?? "0.5");*/
        }
        public async Task<ServiceResult<RagDto>> AskQuestionAsync(string question, List<int> documentIds, int sessionId, CancellationToken cancellationToken = default)
        {
            var stopWatch = Stopwatch.StartNew();

            var questionEmbedding = await _embeddingService.EmbedChunkAsync(question, cancellationToken);

            var searchResults = await _vectorDbService.SearchSimilarAsync(questionEmbedding, documentIds, TOP_K);

            // Filter results based on score threshold
            var relevantResults = searchResults
                .Where(r => r.Score >= SCORE_THRESHOLD)
                .ToList();

            if(relevantResults.Count == 0)
            {
                _logger.LogWarning("No relevant chunks found");
                return ServiceResult<RagDto>.Fail("No relevant information found to answer the question.");
               
            }

            // Step 3: Get conversation history
            var recentMessages = await _chatRepository.GetWithRecentMessagesAsync(sessionId, 10);
            var conversationHistory = recentMessages?.Messages
                .Select(m => $"{(m.IsUser ? "User" : "Bot")}: {m.Content}")
                .ToList();

            // Step 4: Build context
            var context = BuildContext(relevantResults);

            // Step 5: Create prompt
            var prompt = BuildPrompt(question, context, conversationHistory);

            // Step 6: Generate answer
            _logger.LogDebug("Generating answer with Gemini...");
            var answer = await _llmService.AskAsync(prompt, cancellationToken);

            stopWatch.Stop();

            var returnDto = new RagDto
            {
                Answer = answer,
                ProcessingTimeMs = stopWatch.ElapsedMilliseconds
            };

            return ServiceResult<RagDto>.Ok(returnDto);
        }

        private string BuildContext(List<SearchResult> searchResults)
        {
            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine("Thông tin từ tài liệu:");
            contextBuilder.AppendLine();

            for (int i = 0; i < searchResults.Count; i++)
            {
                var result = searchResults[i];
                contextBuilder.AppendLine($"[Nguồn {i + 1}]");
                contextBuilder.AppendLine(result.ChunkText);
                contextBuilder.AppendLine();
            }

            return contextBuilder.ToString();
        }

        private string BuildPrompt(string question, string documentContext, List<string>? conversationHistory)
        {
            var promptBuilder = new StringBuilder();

            // System instruction
            promptBuilder.AppendLine("Bạn là trợ lý AI thông minh, chuyên phân tích tài liệu và trả lời câu hỏi.");
            promptBuilder.AppendLine(); 

            if(conversationHistory != null && conversationHistory.Count > 0)
            {
                promptBuilder.AppendLine("Lịch sử hội thoại gần đây:");
                foreach (var message in conversationHistory)
                {
                    promptBuilder.AppendLine(message);
                }
                promptBuilder.AppendLine();
            }   
            // Document context
            promptBuilder.AppendLine(documentContext);

            // Instructions
            promptBuilder.AppendLine("Hướng dẫn:");
            promptBuilder.AppendLine("- Chỉ trả lời dựa trên thông tin trong tài liệu");
            promptBuilder.AppendLine("- Nếu không có thông tin, nói rõ 'Thông tin này không có trong tài liệu'");
            promptBuilder.AppendLine("- Trả lời bằng tiếng Việt, ngắn gọn và chính xác");
            promptBuilder.AppendLine();

            // Question
            promptBuilder.AppendLine($"Câu hỏi: {question}");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Trả lời:");

            return promptBuilder.ToString();
        }
    }
}
