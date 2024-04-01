using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Globalization;
using System.IO;


namespace SVGControl
{
    public class SvgResourceConverter: TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            // we only know how to convert from to a string
            return typeof(string) == destinationType;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType)
            {
                // just use the benchmark name
                if (value is null)
                    return "(none)";
                else
                {
                    System.Collections.DictionaryEntry benchmark = (System.Collections.DictionaryEntry)value;
                    return benchmark.Key;
                }
            }
            return "(none)";
        }
    }
}
