using DocuMind.Core.Enum;
using System.Collections.Generic;

namespace DocuMind.Application.Interface.IPrompt
{
    public interface IPromptFactory
    {
        string GetPrompt(IntentType intent, string question, string context, List<string>? conversationHistory);
    }
}
