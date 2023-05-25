using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS
{
    public static class WinFormsExtensions
    {
        /// <summary>
        /// Traverses all controls on a form recursively and performs an action defined by "action" parameter
        /// </summary>
        /// <example>   
        /// this.ForAllControls(c =>
        /// {
        ///     if (c.GetType() == typeof(TextBox)) 
        ///     {
        ///         c.TextChanged += C_TextChanged;
        ///     }
        /// });
        /// </example>
        /// <param name="parent">Topmost control on a form to traverse</param>
        /// <param name="action"></param>
        public static void ForAllControls(this Control parent, Action<Control> action)
        {
            foreach (Control c in parent.Controls)
            {
                action(c);
                ForAllControls(c, action);
            }
        }

        public static IEnumerable<Control> GetAllChildren(this Control root)
        {
            var stack = new Stack<Control>();
            stack.Push(root);

            while (stack.Any())
            {
                var next = stack.Pop();
                foreach (Control child in next.Controls)
                    stack.Push(child);
                yield return next;
            }
        }
    }
}
