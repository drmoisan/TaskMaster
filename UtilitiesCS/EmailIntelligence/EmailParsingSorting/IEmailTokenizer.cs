using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.EmailIntelligence
{
    public interface IEmailTokenizer
    {
        IEnumerable<string> Tokenize(IItemInfo msg);
        IEnumerable<string> Tokenize(object obj, IApplicationGlobals globals);
        Task<string[]> TokenizeAsync(object obj, IApplicationGlobals globals, CancellationToken cancel);
    }
}