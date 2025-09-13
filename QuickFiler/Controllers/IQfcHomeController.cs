using QuickFiler.Interfaces;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    public interface IQfcHomeController: IFilerHomeController
    {
        IQfcDatamodel DataModel { get; }        
        IQfcHomeController Init();
        void Iterate();
        void Iterate2();
        Task IterateQueueAsync();        
        void SwapStopWatch();
        Task WriteMetricsAsync(string filename);
        bool WorkerComplete { get; }
    }
}