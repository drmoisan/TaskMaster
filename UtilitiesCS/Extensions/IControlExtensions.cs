using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuickFiler.Interfaces;

namespace UtilitiesCS.Extensions
{
    public static class IControlExtensions
    {
        public static Screen GetScreen(this IControl control)
        {
            return Screen.FromHandle(control.Handle);
        }
    }
}
