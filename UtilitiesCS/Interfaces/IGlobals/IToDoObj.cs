using Outlook = Microsoft.Office.Interop.Outlook;
using System.Threading.Tasks;

namespace UtilitiesCS
{

    public interface IToDoObj<T>
    {
        string Filename { get; set; }
        string Filepath { get; set; }
        string Folderpath { get; set; }
        T Item { get; }
        void LoadFromFile(string Folderpath, Outlook.Application OlApp);
    }
}