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
using DocuMind.Application.Interface.IIntentClassifier;
using DocuMind.Application.Interface.IPrompt;
using DocuMind.Core.Enum;
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
        private readonly IIntentClassifierService _intentClassifier;
        private readonly IPromptFactory _promptFactory;
        private readonly ILogger<RagService> _logger;

        public RagService(
            IEmbeddingService embeddingService,
            IVectorDbService vectorDbService,
            ILlmService llmService,
            IChatSessionRepository chatRepository,
            IIntentClassifierService intentClassifier,
            IPromptFactory promptFactory,
            ILogger<RagService> logger)
        {
            _chatRepository = chatRepository;
            _embeddingService = embeddingService;
            _vectorDbService = vectorDbService;
            _llmService = llmService;
            _intentClassifier = intentClassifier;
            _promptFactory = promptFactory;
            _logger = logger;
        }

        public async Task<ServiceResult<RagDto>> AskQuestionAsync(string question, List<int> documentIds, int sessionId, CancellationToken cancellationToken = default)
        {
            var stopWatch = Stopwatch.StartNew();

            // Step 1: Classify Intent
            var intent = await _intentClassifier.ClassifyIntentAsync(question, cancellationToken);
            _logger.LogInformation("Processing RAG request with intent: {Intent}", intent);

            // Step 2: Retrieve Documents with intent-aware strategy
            var questionEmbedding = await _embeddingService.EmbedChunkAsync(question, cancellationToken);
            
            // Determine search parameters based on intent
            int topK = intent switch
            {
                IntentType.SUMMARY => 20,     // Need more context for summary
                IntentType.EXPLANATION => 15, // Need moderate context for explanation
                _ => 10                       // Standard for QA
            };

            float scoreThreshold = intent switch
            {
                IntentType.SUMMARY => 0.5f,   // Looser threshold for summary
                IntentType.EXPLANATION => 0.6f,
                _ => 0.65f                    // Stricter for QA precision
            };

            var searchResults = await _vectorDbService.SearchSimilarAsync(questionEmbedding, documentIds, topK);

            // Filter results
            var relevantResults = searchResults
                .Where(r => r.Score >= scoreThreshold)
                .ToList();

            if (relevantResults.Count == 0)
            {
                _logger.LogWarning("No relevant chunks found with threshold {Threshold}", scoreThreshold);
                 // If SUMMARY and no results, try fallback with very low threshold
                if (intent == IntentType.SUMMARY)
                {
                     relevantResults = searchResults.Where(r => r.Score >= 0.4f).ToList();
                }
                
                if (relevantResults.Count == 0)
                {
                     return ServiceResult<RagDto>.Ok(new RagDto { Answer = "I couldn't find enough relevant information in the documents to answer your question." });
                }
            }

            // Step 3: Get conversation history
            var recentMessages = await _chatRepository.GetWithRecentMessagesAsync(sessionId, 5); // Reduce history to keep context focused
            var conversationHistory = recentMessages?.Messages
                .Select(m => $"{(m.IsUser ? "User" : "System")}: {m.Content}")
                .ToList();

            // Step 4: Build context
            var context = BuildContext(relevantResults);

            // Step 5: Create prompt using Factory
            var prompt = _promptFactory.GetPrompt(intent, question, context, conversationHistory);

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
            var sb = new StringBuilder();

            if (searchResults == null || searchResults.Count == 0)
            {
                return string.Empty;
            }

            for (int i = 0; i < searchResults.Count; i++)
            {
                var result = searchResults[i];
                sb.AppendLine($"[Source {i + 1}] (Score: {result.Score:F2})");
                sb.AppendLine(result.ChunkText.Trim());
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
