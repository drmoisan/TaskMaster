#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVGControl
{
    public class SvgResourceConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            // we only know how to convert from to a string
            return typeof(string) == destinationType;
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType
        )
        {
            if (typeof(string) == destinationType)
            {
                // just use the benchmark name
                if (value is null)
                    return "(none)";
                else
                {
                    ISvgResource resource = (ISvgResource)value;
                    // ISvgResource.Name is nullable (SvgResource's parameterless constructor
                    // never assigns it); this preserves the pre-existing behavior of returning
                    // whatever Name currently holds, including null, without a new fallback.
                    return resource.Name!;
                }
            }
            return "(none)";
        }
    }
}
