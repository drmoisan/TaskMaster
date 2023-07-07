using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickFiler.Controllers
{
    internal class QfcTipsDetails : IQfcTipsDetails
    {
        public QfcTipsDetails(System.Windows.Forms.Label LabelControl)
        {
            _labelControl = LabelControl;
            _parentType = ResolveParentType();
            if (_parentType == typeof(TableLayoutPanel))
            {
                _tlp = (TableLayoutPanel)_labelControl.Parent;
                _columnNumber = _tlp.GetColumn(_labelControl);
                _columnWidth = _tlp.ColumnStyles[_columnNumber].Width;
            }
            else
            {
                _panel = (System.Windows.Forms.Panel)_labelControl.Parent;
            }
            _state = IQfcTipsDetails.ToggleState.On;
        }

        public Type ResolveParentType()
        {
            if (_labelControl.Parent == null)
            {
                throw new ArgumentException($"The parent of {nameof(LabelControl)} is null. " +
                $"Must be of type {typeof(TableLayoutPanel)}");
            }
            else if (!new List<Type>{typeof(TableLayoutPanel),
                                    typeof(System.Windows.Forms.Panel)}
                                    .Contains(
                                    _labelControl.Parent.GetType()))
            {
                throw new ArgumentException($"The parent of {nameof(LabelControl)} must " +
                                            $"be of type {typeof(TableLayoutPanel)} but it is of " +
                                            $"type {LabelControl.Parent.GetType()}");
            }
            return _labelControl.Parent.GetType();
        }

        private System.Windows.Forms.Label _labelControl;
        private TableLayoutPanel _tlp;
        private System.Windows.Forms.Panel _panel;
        private int _columnNumber;
        private System.Single _columnWidth;
        private IQfcTipsDetails.ToggleState _state;
        private Type _parentType;

        public System.Windows.Forms.Label LabelControl { get => _labelControl; }
        public TableLayoutPanel TLP { get => _tlp; }
        public int ColumnNumber { get => _columnNumber; }
        public float ColumnWidth { get => _columnWidth; }
        

        public void Toggle()
        {
            if (_state == IQfcTipsDetails.ToggleState.Off)
            {
                Toggle(IQfcTipsDetails.ToggleState.On);
            }
            else
            {
                Toggle(IQfcTipsDetails.ToggleState.Off);
            }
        }

        public void Toggle(bool sharedColumn)
        {
            if (_state == IQfcTipsDetails.ToggleState.Off)
            {
                Toggle(IQfcTipsDetails.ToggleState.On, sharedColumn);
            }
            else
            {
                Toggle(IQfcTipsDetails.ToggleState.Off, sharedColumn);
            }
        }

        public void Toggle(IQfcTipsDetails.ToggleState desiredState, bool sharedColumn)
        {
            if (desiredState == IQfcTipsDetails.ToggleState.Off)
            {
                _labelControl.Visible = false;
                _labelControl.Enabled = false;
                if (_parentType == typeof(TableLayoutPanel) && ((_tlp.RowCount == 1)|(sharedColumn)))
                    _tlp.ColumnStyles[_columnNumber].Width = 0;
            }
            else
            {
                _labelControl.Visible = true;
                _labelControl.Enabled = true;
                if (_parentType == typeof(TableLayoutPanel) && ((_tlp.RowCount == 1) | (sharedColumn)))
                    _tlp.ColumnStyles[_columnNumber].Width = _columnWidth;
            }
            _state = desiredState;
        }

        public void Toggle(IQfcTipsDetails.ToggleState desiredState)
        {
            if (desiredState == IQfcTipsDetails.ToggleState.Off)
            {
                _labelControl.Visible = false;
                _labelControl.Enabled = false;
                if (_parentType == typeof(TableLayoutPanel)&&(_tlp.RowCount==1))
                    _tlp.ColumnStyles[_columnNumber].Width = 0;
            }
            else
            {
                _labelControl.Visible = true;
                _labelControl.Enabled = true;
                if (_parentType == typeof(TableLayoutPanel) && (_tlp.RowCount == 1))
                    _tlp.ColumnStyles[_columnNumber].Width = _columnWidth;
            }
            _state = desiredState;
        }
    }
}
