using System.Threading;
using System.Threading.Tasks;
using DocuMind.Core.Enum;

namespace DocuMind.Application.Interface.IIntentClassifier
{
    public interface IIntentClassifierService
    {
        Task<IntentType> ClassifyIntentAsync(string question, CancellationToken cancellationToken = default);
    }
}
