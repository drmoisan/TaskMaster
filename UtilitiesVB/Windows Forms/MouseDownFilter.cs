using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UtilitiesVB
{

    public class MouseDownFilter : IMessageFilter
    {

        public event EventHandler FormClicked;
        private readonly int WM_LBUTTONDOWN = 0x201;
        private readonly Form form = null;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

        public MouseDownFilter(Form f)
        {
            form = f;
        }

        private bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN)
            {

                if (Form.ActiveForm is not null && Form.ActiveForm.Equals(form))
                {
                    OnFormClicked();
                }
            }

            return false;
        }

        bool IMessageFilter.PreFilterMessage(ref Message m) => PreFilterMessage(ref m);

        protected void OnFormClicked()
        {
            FormClicked?.Invoke(form, EventArgs.Empty);
        }


    }
}