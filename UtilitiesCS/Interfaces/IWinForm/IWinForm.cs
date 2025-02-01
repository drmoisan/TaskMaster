using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace QuickFiler.Interfaces
{
    public interface IWinForm : IForm, IContainerControl, IComponent, IDisposable, IContainerControlLocal  { }
    //public interface IWinForm: IForm, IContainerControl, IArrangedElement, IComponent, IDisposable {   }
        
    //public interface  IArrangedElement: IComponent, IDisposable
    //{
    //    Rectangle Bounds { get; }

    //    Rectangle DisplayRectangle { get; }

    //    bool ParticipatesInLayout { get; }

    //    PropertyStore Properties { get; }

    //    IArrangedElement Container { get; }

    //    ArrangedElementCollection Children { get; }

    //    void SetBounds(Rectangle bounds, BoundsSpecified specified);

    //    Size GetPreferredSize(Size proposedSize);

    //    void PerformLayout(IArrangedElement affectedElement, string propertyName);
    //}


}
