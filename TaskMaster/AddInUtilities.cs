using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TaskVisualization;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskMaster
{
    [ComVisible(true)]
    public interface IAddInUtilities
    {
        void MaximizeQuickFilerWindow();
        void LaunchQuickFiler();
        void LaunchSortEmail();
        void LaunchFlagAsTask();
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [ExcludeFromCodeCoverage]
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

        public void LaunchQuickFiler()
        {
            if (_globals is not null)
            {
                _ = _ribbonController.LoadQuickFilerAsync();
            }
        }

        public void LaunchSortEmail()
        {
            if (_globals is not null)
            {
                _ = _ribbonController.SortEmailAsync();
            }
        }

        public void LaunchFlagAsTask()
        {
            if (_globals is not null)
            {
                _ribbonController.FlagAsTask();
            }
        }
    }
}
