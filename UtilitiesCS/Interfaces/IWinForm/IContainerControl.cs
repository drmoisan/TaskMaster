using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuickFiler.Interfaces
{
    public interface IContainerControlLocal: IScrollableControl
    {
        Control ActiveControl { get; set; }
        SizeF AutoScaleDimensions { get; set; }
        AutoScaleMode AutoScaleMode { get; set; }
        AutoValidate AutoValidate { get; set; }        
        SizeF CurrentAutoScaleDimensions { get; }
        Form ParentForm { get; }

        event EventHandler AutoValidateChanged;

        void PerformAutoScale();
        bool Validate();
        bool Validate(bool checkAutoValidate);
        bool ValidateChildren();
        bool ValidateChildren(ValidationConstraints validationConstraints);
    }
}