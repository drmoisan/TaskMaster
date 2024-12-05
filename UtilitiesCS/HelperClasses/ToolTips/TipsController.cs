using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;

namespace TaskVisualization
{
    public class TipsController
    {
        public TipsController(Label label)
        {
            InitializeLabel(label);
        }

        public TipsController(Label label, int groupNumber)
        {
            InitializeLabel(label);
            _groupNumber = groupNumber;
        }

        private void InitializeLabel(Label label)
        {
            _labelControl = label;
            _parentType = ResolveParentType();

            if (_parentType == typeof(TableLayoutPanel))
            {
                _tlp = (TableLayoutPanel)_labelControl.Parent;
                _columnNumber = _tlp.GetColumn(_labelControl);
                _columnWidth = _tlp.ColumnStyles[_columnNumber].Width;
            }
            else
            {
                _panel = (Panel)_labelControl.Parent;
            }
            _state = Enums.ToggleState.On;
        }

        public T ResolveParent<T>(Control control) where T : Control
        {
            
            return (T)_labelControl.Parent;
        }
        
        public Type ResolveParentType()
        {
            if (_labelControl.Parent == null)
            {
                throw new ArgumentException($"The parent of {nameof(LabelControl)} is null. " +
                $"Must be of type {typeof(TableLayoutPanel)}");
            }
            else if (!new List<Type>{typeof(TableLayoutPanel), typeof(Panel)}
                                    .Contains(_labelControl.Parent.GetType()))
            {
                throw new ArgumentException($"The parent of {nameof(LabelControl)} must " +
                                            $"be of type {typeof(TableLayoutPanel)} but it is of " +
                                            $"type {LabelControl.Parent.GetType()}");
            }
            return _labelControl.Parent.GetType();
        }

        private Enums.ToggleState _state;
        private Type _parentType;

        private Label _labelControl;
        public Label LabelControl { get => _labelControl; }
        
        private TableLayoutPanel _tlp;
        public TableLayoutPanel TLP { get => _tlp; }

        private Panel _panel;

        private int _columnNumber;
        public int ColumnNumber { get => _columnNumber; }
        
        private float _columnWidth;
        public float ColumnWidth { get => _columnWidth; }

        private int _groupNumber;
        public int GroupNumber { get => _groupNumber; set => _groupNumber = value; }

        public void Toggle()
        {
            if (_state == Enums.ToggleState.Off)
            {
                Toggle(Enums.ToggleState.On);
            }
            else
            {
                Toggle(Enums.ToggleState.Off);
            }
        }

        public void Toggle(bool sharedColumn)
        {
            if (_state == Enums.ToggleState.Off)
            {
                Toggle(Enums.ToggleState.On, sharedColumn);
            }
            else
            {
                Toggle(Enums.ToggleState.Off, sharedColumn);
            }
        }

        public void Toggle(Enums.ToggleState desiredState, bool sharedColumn)
        {
            if (desiredState == Enums.ToggleState.Off)
            {
                _labelControl.Visible = false;
                _labelControl.Enabled = false;
                if (_parentType == typeof(TableLayoutPanel) && ((_tlp.RowCount == 1) | (sharedColumn)))
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

        public void Toggle(Enums.ToggleState desiredState)
        {
            if (desiredState == Enums.ToggleState.Off)
            {
                _labelControl.Visible = false;
                _labelControl.Enabled = false;
                if (_parentType == typeof(TableLayoutPanel) && (_tlp.RowCount == 1))
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

        public void ToggleColumnOnly(Enums.ToggleState desiredState)
        {
            if (desiredState == Enums.ToggleState.Off)
            {
                if (_parentType == typeof(TableLayoutPanel)) 
                    _tlp.ColumnStyles[_columnNumber].Width = 0;
            }
            else
            {
                if (_parentType == typeof(TableLayoutPanel))
                    _tlp.ColumnStyles[_columnNumber].Width = _columnWidth;
            }
            _state = desiredState;
        }
    }
}
