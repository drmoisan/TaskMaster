using Microsoft.Office.Interop.Outlook;
using UtilitiesVB;

namespace TaskMaster
{

    public class ApplicationGlobals : IApplicationGlobals
    {

        private readonly AppFileSystemFolderPaths _fs;
        private readonly AppOlObjects _olObjects;
        private readonly AppToDoObjects _toDoObjects;
        private AppAutoFileObjects _autoFileObjects;

        public ApplicationGlobals(Application OlApp)
        {
            _fs = new AppFileSystemFolderPaths();
            _olObjects = new AppOlObjects(OlApp);
            _toDoObjects = new AppToDoObjects(this);
            _autoFileObjects = new AppAutoFileObjects(this);
        }

        public IFileSystemFolderPaths FS
        {
            get
            {
                return _fs;
            }
        }

        public IOlObjects Ol
        {
            get
            {
                return _olObjects;
            }
        }

        public IToDoObjects TD
        {
            get
            {
                return _toDoObjects;
            }
        }

        public IAppAutoFileObjects AF
        {
            get
            {
                return _autoFileObjects;
            }

        }

        #region Legacy Definitions and Constants


        #endregion

    }
}