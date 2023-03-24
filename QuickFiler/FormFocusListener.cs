
namespace QuickFiler
{
    public class FormFocusListener
    {
        public event ChangeFocusEventHandler ChangeFocus;

        public delegate void ChangeFocusEventHandler(bool gotFocus);

        public bool ChangeFocusMessage
        {
            set
            {
                ChangeFocus?.Invoke(value);
            }

        }

    }
}