using System;
using System.Collections.Generic;
// -------------------------------------------------------------------------------
// Resizer
// This class is used to dynamically resize and reposition all controls on a form.
// Container controls are processed recursively so that all controls on the form
// are handled.
// 
// Usage:
// Resizing functionality requires only three lines of code on a form:
// 
// 1. Create a form-level reference to the Resize class:
// Dim myResizer as Resizer
// 
// 2. In the Form_Load event, call the  Resizer class FIndAllControls method:
// myResizer.FindAllControls(Me)
// 
// 3. In the Form_Resize event, call the  Resizer class ResizeAllControls method:
// myResizer.ResizeAllControls(Me)
// 
// -------------------------------------------------------------------------------
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace UtilitiesVB
{

    public class Resizer
    {

        // ----------------------------------------------------------
        // ControlInfo
        // Structure of original state of all processed controls
        // ----------------------------------------------------------
        private struct ControlInfo
        {
            public string name;
            public string parentName;
            public double leftOffsetPercent;
            public double topOffsetPercent;
            public double heightPercent;
            public int originalHeight;
            public int originalWidth;
            public double widthPercent;
            public float originalFontSize;
            public ResizeDimensions ResizeType;
        }

        public enum ResizeDimensions
        {
            None = 0,                // 0b00000
            Position_Top = 1,        // 0b00001
            Position_Left = 2,       // 0b00010
            Position = 3,            // 0b00011
            Size_Width = 4,          // 0b00100
            Size_Height = 8,         // 0b01000
            Size = 12,               // 0b01100
            Font = 16,               // 0b10000
            All = 31                // 0b11111
        }

        // -------------------------------------------------------------------------
        // ctrlDict
        // Dictionary of (control name, control info) for all processed controls
        // -------------------------------------------------------------------------
        private readonly Dictionary<string, ControlInfo> ctrlDict = new Dictionary<string, ControlInfo>();

        // ----------------------------------------------------------------------------------------
        // FindAllControls
        // Recursive function to process all controls contained in the initially passed
        // control container and store it in the Control dictionary
        // ----------------------------------------------------------------------------------------
        public void FindAllControls(Control thisCtrl, string strLeader = "")
        {

            // -- If the current control has a parent, store all original relative position
            // -- and size information in the dictionary.
            // -- Recursively call FindAllControls for each control contained in the
            // -- current Control
            if (strLeader.Length == 0)
            {
                Debug.WriteLine(thisCtrl.Name);
            }
            strLeader += " ";
            foreach (Control ctl in thisCtrl.Controls)
            {
                try
                {
                    if (!(ctl.Parent == null))
                    {
                        if (!(ctl.Name.Length == 0))
                        {
                            int parentHeight = ctl.Parent.Height;
                            int parentWidth = ctl.Parent.Width;

                            var c = new ControlInfo()
                            {
                                name = ctl.Name,
                                parentName = ctl.Parent.Name,
                                topOffsetPercent = Convert.ToDouble(ctl.Top) / Convert.ToDouble(parentHeight),
                                leftOffsetPercent = Convert.ToDouble(ctl.Left) / Convert.ToDouble(parentWidth),
                                heightPercent = Convert.ToDouble(ctl.Height) / Convert.ToDouble(parentHeight),
                                widthPercent = Convert.ToDouble(ctl.Width) / Convert.ToDouble(parentWidth),
                                originalFontSize = ctl.Font.Size,
                                originalHeight = ctl.Height,
                                originalWidth = ctl.Width,
                                ResizeType = ResizeDimensions.All
                            };
                            ctrlDict.Add(c.name, c);
                            Debug.WriteLine(strLeader + c.name);
                        }
                        else
                        {
                            Debug.WriteLine("");
                        }
                    }
                }

                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                }

                if (ctl.Controls.Count > 0)
                {
                    FindAllControls(ctl, strLeader);
                }

            } // -- For Each

        }

        public void PrintDict()
        {
            foreach (var key in ctrlDict.Keys)
                Debug.WriteLine(key + " " + ((int)ctrlDict[key].ResizeType).ToString());
        }

        public bool SetResizeDimensions(Control thisCtrl, ResizeDimensions ResizeType, bool IncludeChildren)
        {
            // -- Get the current control's info from the control info dictionary
            bool success = true;
            var c = new ControlInfo();

            if (thisCtrl.Name.Length > 0)
            {
                success = ctrlDict.TryGetValue(thisCtrl.Name, out c);
                if (success)
                {
                    c.ResizeType = ResizeType;
                    ctrlDict[thisCtrl.Name] = c;
                }
            }
            if (IncludeChildren)
            {
                foreach (Control ctl in thisCtrl.Controls)
                {
                    if (success)
                    {
                        success = SetResizeDimensions(ctl, ResizeType, true);
                    }
                }
            }
            return success;
        }

        // ----------------------------------------------------------------------------------------
        // ResizeAllControls
        // Recursive function to resize and reposition all controls contained in the Control
        // dictionary
        // ----------------------------------------------------------------------------------------
        public void ResizeAllControls(Control thisCtrl)
        {

            float fontRatioW;
            float fontRatioH;
            float fontRatio;
            Font f;

            // -- Resize and reposition all controls in the passed control
            foreach (Control ctl in thisCtrl.Controls)
            {
                try
                {
                    if (!(ctl.Parent == null))
                    {
                        int parentHeight = ctl.Parent.Height;
                        int parentWidth = ctl.Parent.Width;

                        var c = new ControlInfo();

                        bool ret = false;
                        try
                        {
                            // -- Get the current control's info from the control info dictionary
                            if (ctl.Name.Length > 0)
                            {
                                ret = ctrlDict.TryGetValue(ctl.Name, out c);

                                // -- If found, adjust the current control based on control relative
                                // -- size and position information stored in the dictionary
                                if (ret)
                                {
                                    // -- Size
                                    if (Conversions.ToBoolean(c.ResizeType & ResizeDimensions.Size_Width))
                                        ctl.Width = (int)Math.Round(Conversion.Int(parentWidth * c.widthPercent));
                                    if (Conversions.ToBoolean(c.ResizeType & ResizeDimensions.Size_Height))
                                        ctl.Height = (int)Math.Round(Conversion.Int(parentHeight * c.heightPercent));

                                    // -- Position
                                    if (Conversions.ToBoolean(c.ResizeType & ResizeDimensions.Position_Top))
                                        ctl.Top = (int)Math.Round(Conversion.Int(parentHeight * c.topOffsetPercent));
                                    if (Conversions.ToBoolean(c.ResizeType & ResizeDimensions.Position_Left))
                                        ctl.Left = (int)Math.Round(Conversion.Int(parentWidth * c.leftOffsetPercent));

                                    // -- Font
                                    if (Conversions.ToBoolean(c.ResizeType & ResizeDimensions.Font))
                                    {
                                        f = ctl.Font;
                                        fontRatioW = (float)(ctl.Width / (double)c.originalWidth);
                                        fontRatioH = (float)(ctl.Height / (double)c.originalHeight);
                                        fontRatio = (fontRatioW + fontRatioH) / 2f; // -- average change in control Height and Width
                                        ctl.Font = new Font(f.FontFamily, c.originalFontSize * fontRatio, f.Style);
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                catch (Exception ex)
                {
                }

                // -- Recursive call for controls contained in the current control
                if (ctl.Controls.Count > 0)
                {
                    ResizeAllControls(ctl);
                }

            } // -- For Each
        }

    }
}