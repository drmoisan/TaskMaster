using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.Interfaces
{
    public interface IFlagChangeGroup
    {
        BlockingCollection<IFlagChangeItem> FlagChangeItems { get; set; }

        Task ProcessGroupAsync(CancellationToken cancel = default);
        bool TryEnqueue(string classifierName, IEnumerable<string> original, IEnumerable<string> revised);
    }
}