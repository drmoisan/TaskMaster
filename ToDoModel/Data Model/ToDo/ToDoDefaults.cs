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
                new PrefixItem(prefixType: PrefixTypeEnum.People, key: "People", value: Properties.Settings.Default.Prefix_People, color: OlCategoryColor.olCategoryColorDarkGray), 
                new PrefixItem(prefixType : PrefixTypeEnum.Project, key: "Project", value: Properties.Settings.Default.Prefix_Project, color: OlCategoryColor.olCategoryColorTeal), 
                new PrefixItem(prefixType : PrefixTypeEnum.Topic, key: "Topic", value: Properties.Settings.Default.Prefix_Topic, color: OlCategoryColor.olCategoryColorDarkTeal), 
                new PrefixItem(prefixType : PrefixTypeEnum.Context, key: "Context", value: Properties.Settings.Default.Prefix_Context, color: OlCategoryColor.olCategoryColorNone), 
                new PrefixItem(prefixType : PrefixTypeEnum.Today, key: "Today", value: Properties.Settings.Default.Prefix_Today, color: OlCategoryColor.olCategoryColorDarkRed), 
                new PrefixItem(prefixType : PrefixTypeEnum.Bullpin, key: "Bullpin", value: Properties.Settings.Default.Prefix_Bullpin, color: OlCategoryColor.olCategoryColorOrange), 
                new PrefixItem(prefixType : PrefixTypeEnum.KB, key: "KB", value: Properties.Settings.Default.Prefix_KB, color: OlCategoryColor.olCategoryColorRed) 
            };
            DefaultTaskLength = Properties.Settings.Default.Default_Task_Length;
        }

        public static ToDoDefaults Instance { get; } = new ToDoDefaults();

        public int DefaultTaskLength { get; private set; }

        public List<IPrefix> PrefixList { get; private set; }

    }

    public class PrefixItem : IPrefix
    {

        public PrefixItem(PrefixTypeEnum prefixType, string key, string value, OlCategoryColor color)
        {
            PrefixType = prefixType;
            Key = key;
            Value = value;
            Color = color;
        }

        private PrefixTypeEnum _prefixType;
        public PrefixTypeEnum PrefixType { get => _prefixType; set => _prefixType = value; }

        private string _key;
        public string Key { get => _key; set => _key = value; }

        private string _value;
        public string Value { get => _value; set => this._value = value; }

        private OlCategoryColor _color;
        public OlCategoryColor Color { get => _color; set => _color = value; }
    }
}