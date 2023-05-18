using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace UtilitiesVB
{

    public static class ExtToChar
    {
        public static char ToChar(this Keys key)
        {
            char c = Conversions.ToChar(Constants.vbNullChar);

            if (key >= Keys.A && key <= Keys.Z)
            {
                c = Strings.ChrW(Strings.AscW('a') + ((int)key - (int)Keys.A));
            }
            else if (key >= Keys.D0 && key <= Keys.D9)
            {
                c = Strings.ChrW(Strings.AscW('0') + ((int)key - (int)Keys.D0));
            }

            return c;
        }
    }
}