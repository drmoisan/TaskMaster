
namespace UtilitiesCS
{

    public interface IToDoObj<T>
    {
        string Filename { get; set; }
        string Filepath { get; set; }
        string Folderpath { get; set; }
        T Item { get; }
        void LoadFromFile(string Folderpath, Microsoft.Office.Interop.Outlook.Application OlApp);
    }
}