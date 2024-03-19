using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS
{
    public interface IProgressViewer: IContainerControl, IComponent, IDisposable
    {
        public ProgressBar Bar { get; }
        public Label JobName { get; }
        public Button ButtonCancel { get; }
        public void SetCancellationTokenSource(System.Threading.CancellationTokenSource tokenSource);
        public System.Windows.Threading.Dispatcher UiDispatcher { get; set; }
        
    }
}
