using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    internal class QfcExplorerController : IQfcExplorerController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool BlShowInConversations { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void ExplConvView_Cleanup()
        {
            throw new NotImplementedException();
        }

        public void ExplConvView_ReturnState()
        {
            throw new NotImplementedException();
        }

        public void ExplConvView_ToggleOff()
        {
            throw new NotImplementedException();
        }

        public void ExplConvView_ToggleOn()
        {
            throw new NotImplementedException();
        }

        public void OpenQFItem(object ObjItem)
        {
            throw new NotImplementedException();
        }
    }
}
