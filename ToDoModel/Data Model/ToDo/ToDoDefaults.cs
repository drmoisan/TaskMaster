using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;

using UtilitiesCS;

namespace ToDoModel
{

    public class ToDoDefaults
    {
        public ToDoDefaults()
        {
            PrefixList = new List<IPrefix>() 
            { 
                new PrefixItem(key: "People", value: Properties.Settings.Default.Prefix_People, color: OlCategoryColor.olCategoryColorDarkGray), 
                new PrefixItem(key: "Project", value: Properties.Settings.Default.Prefix_Project, color: OlCategoryColor.olCategoryColorTeal), 
                new PrefixItem(key: "Topic", value: Properties.Settings.Default.Prefix_Topic, color: OlCategoryColor.olCategoryColorDarkTeal), 
                new PrefixItem(key: "Context", value: Properties.Settings.Default.Prefix_Context, color: OlCategoryColor.olCategoryColorNone), 
                new PrefixItem(key: "Today", value: Properties.Settings.Default.Prefix_Today, color: OlCategoryColor.olCategoryColorDarkRed), 
                new PrefixItem(key: "Bullpin", value: Properties.Settings.Default.Prefix_Bullpin, color: OlCategoryColor.olCategoryColorOrange), 
                new PrefixItem(key: "KB", value: Properties.Settings.Default.Prefix_KB, color: OlCategoryColor.olCategoryColorRed) 
            };
            DefaultTaskLength = Properties.Settings.Default.Default_Task_Length;
        }

        public int DefaultTaskLength { get; private set; }

        public List<IPrefix> PrefixList { get; private set; }

    }

    public class PrefixItem : IPrefix
    {

        public PrefixItem(string key, string value, OlCategoryColor color)
        {
            Key = key;
            Value = value;
            Color = color;
        }

        public string Key { get; set; }

        public string Value { get; set; }

        public OlCategoryColor Color { get; set; }
    }
}