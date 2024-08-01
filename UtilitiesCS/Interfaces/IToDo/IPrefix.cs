using System.Collections.Generic;

namespace UtilitiesCS
{
    //public interface OldIPrefix
    //{
    //    string People { get; set; }
    //    string Project { get; set; }
    //    string Topic { get; set; }
    //    string Context { get; set; }
    //    string Today { get; set; }
    //    string Bullpin { get; set; }
    //    string KB { get; set; }
    //    Dictionary<string, string> elements { get; set; }
    //    int Default_Task_Length { get; set; }
    //}

    public enum PrefixTypeEnum
    {
        People,
        Project,
        Topic,
        Context,
        Today,
        Bullpin,
        KB,
        Other
    }
    
    public interface IPrefix
    {
        PrefixTypeEnum PrefixType { get; set; }
        string Key { get; set; }
        string Value { get; set; }
        Microsoft.Office.Interop.Outlook.OlCategoryColor Color { get; set; }
        string OlUserFieldName { get; set; }
    }
}