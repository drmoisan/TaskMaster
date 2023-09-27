using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

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
                ForAllControls(c, action);
            }
            action(parent);
        }

        public static void ForAllControls(this Control parent, Action<Control> action, IList<Control> except)
        {
            if (!except.Contains(parent))
            {
                foreach (Control c in parent.Controls)
                {
                    ForAllControls(c, action, except);
                }
                action(parent);
            }
        }

        public static void ForAllControls<T>(this Control parent, T value, Action<Control, T> action, IList<Control> except)
        {
            if (!except.Contains(parent))
            {
                foreach (Control c in parent.Controls)
                {
                    ForAllControls(c, value, action, except);
                }
                action(parent, value);
            }
        }

        public static void ForAllControls<T>(this Control parent, T value, Func<Control, T, T> function, IList<Control> except)
        {
            if (!except.Contains(parent))
            {
                T seedValue = function(parent, value);
                foreach (Control c in parent.Controls)
                {
                    ForAllControls(c, seedValue, function, except);
                }
            }
            
        }

        public static void ForAllControls<T>(this Control parent, T value, Func<Control, T, T> function)
        {
            T seedValue = function(parent, value);
            foreach (Control c in parent.Controls)
            {
                ForAllControls(c, seedValue, function);
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

        public static IEnumerable<Control> GetAllChildren(this Control root, IList<Control> except)
        {
            var stack = new Stack<Control>();
            if (!except.Contains(root))
                stack.Push(root);

            while (stack.Any())
            {
                var next = stack.Pop();
                foreach (Control child in next.Controls)
                    if (!except.Contains(child))
                        stack.Push(child);
                yield return next;
            }
        }

        public static bool IsRegistered(this EventHandler handler, 
                                        Delegate prospectiveHandler) => 
            handler != null && 
            handler.GetInvocationList()
                   .Any(existingHandler => existingHandler == prospectiveHandler);

        
        public static T Clone<T>(this T controlToClone)
            where T : Control
        {
            PropertyInfo[] controlProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            T instance = Activator.CreateInstance<T>();

            var excluded = new List<string>() { "WindowTarget", "Name", "Parent" };
            
            foreach (PropertyInfo propInfo in controlProperties)
            {
                if (propInfo.CanWrite)
                {
                    //if (propInfo.Name != "WindowTarget")
                    if (!excluded.Contains(propInfo.Name))
                        propInfo.SetValue(instance, propInfo.GetValue(controlToClone, null), null);
                }
            }

            return instance;
        }
        
        public static (EventHandlerList EventHandlerList, object Object) GetEventHandlerList(this object control, string eventName)
        {
            eventName = "Event" + eventName;
            FieldInfo f1 = typeof(Control).GetField(eventName,
                               BindingFlags.Static | BindingFlags.NonPublic);

            object obj = f1.GetValue(control);
            PropertyInfo pi = control.GetType().GetProperty("Events",
                                              BindingFlags.NonPublic | BindingFlags.Instance);

            return ((EventHandlerList)pi.GetValue(control, null),obj);
        }
        
        public static void RemoveEventHandlers(this Control control, string eventName)
        { 
            (var list, var obj) = control.GetEventHandlerList(eventName);
            list.RemoveHandler(obj, list[obj]);
        }

    }
}
