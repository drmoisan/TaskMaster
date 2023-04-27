using System.IO;
using Outlook = Microsoft.Office.Interop.Outlook;
using UtilitiesVB;

namespace TaskMaster
{

    public class ToDoObj<T> : IToDoObj<T>
    {

        private string _filename;
        private string _folderpath;
        private string _filepath;
        public delegate T LoadToDoObj(string Folderpath, Outlook.Application OlApp);
        private LoadToDoObj _loadFunction;
        private T _item;

        public ToDoObj(string FileName, string FolderPath, LoadToDoObj LoadFunction)
        {
            _filename = FileName;
            _folderpath = FolderPath;
            _filepath = Path.Combine(FolderPath, FileName);
            _loadFunction = LoadFunction;
        }

        public ToDoObj(string Filepath, LoadToDoObj LoadFunction)
        {
            _filepath = Filepath;
            _filename = Path.GetFileName(Filepath);
            _folderpath = Path.GetDirectoryName(Filepath);

            _loadFunction = LoadFunction;
        }

        public void LoadFromFile(string Folderpath, Outlook.Application OlApp)
        {
            _item = _loadFunction(Folderpath, OlApp);
        }

        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
                _filepath = Path.Combine(_folderpath, _filename);
            }
        }

        public string Folderpath
        {
            get
            {
                return _folderpath;
            }
            set
            {
                _folderpath = value;
                _filepath = Path.Combine(_folderpath, _filename);
            }
        }

        public string Filepath
        {
            get
            {
                return _filepath;
            }
            set
            {
                _filepath = value;
                _filename = Path.GetFileName(value);
                _folderpath = Path.GetDirectoryName(value);
            }
        }

        public T Item
        {
            get
            {
                return _item;
            }
        }
    }
}