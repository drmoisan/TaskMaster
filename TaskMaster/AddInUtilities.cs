using System.Data;
using System.Runtime.InteropServices;
using TaskVisualization;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Threading.Tasks;

namespace TaskMaster
{
    [ComVisible(true)]
    public interface IAddInUtilities
    {
        void MaximizeQuickFilerWindow();
        Task LaunchQuickFilerAsync();
        void LaunchSortEmail();
        void LaunchFlagAsTask();
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class AddInUtilities : IAddInUtilities
    {
        public AddInUtilities() { }

        internal void SetGlobals(IApplicationGlobals globals, RibbonController ribbonController)
        {
            _globals = globals;
            _ribbonController = ribbonController;
        }
        
        private IApplicationGlobals _globals;
        private RibbonController _ribbonController;

        public void MaximizeQuickFilerWindow()
        {
            if (_globals is not null && _globals.AF.MaximizeQuickFileWindow is not null)
            {
                _globals.AF.MaximizeQuickFileWindow.Invoke();
            }
        }

        public async Task LaunchQuickFilerAsync()
        {
            if (_globals is not null)
            {
                await _ribbonController.LoadQuickFilerAsync();
                //_ = _ribbonController.LoadQuickFilerAsync();
            }
        }

        public void LaunchFlagAsTask()
        {
            if (_globals is not null)
            {
                _ribbonController.FlagAsTask();
            }
        }

        public void LaunchSortEmail()
        {
            if (_globals is not null)
            {
                _ribbonController.SortEmail();
            }
        }
    }
}
