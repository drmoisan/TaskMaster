//using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using UtilitiesCS;

//[assembly: InternalsVisibleTo("QuickFiler.Test")]
namespace UtilitiesCS
{
    public class QfcTipsDetails : IQfcTipsDetails
    {
        private QfcTipsDetails(System.Windows.Forms.Label labelControl, SynchronizationContext uiContext) 
        { 
            _labelControl = labelControl;
            _uiContext = uiContext;
        }

        public QfcTipsDetails(System.Windows.Forms.Label labelControl)
        {
            _labelControl = labelControl;
            _parentType = ResolveParentType();
            SetParentProperties(_parentType);
            if (labelControl.Visible)
            {
                _state = Enums.ToggleState.On;
            }
            else
            {
                _state = Enums.ToggleState.Off;
            }
        }

        private void SetParentProperties(Type parentType)
        {
            if (parentType == typeof(TableLayoutPanel))
            {
                _tlp = (TableLayoutPanel)_labelControl.Parent;
                _columnNumber = _tlp.GetColumn(_labelControl);
                _columnWidth = _tlp.ColumnStyles[_columnNumber].Width;
            }
            else
            {
                _panel = (System.Windows.Forms.Panel)_labelControl.Parent;
            }
        }

        public static async ValueTask<IQfcTipsDetails> CreateAsync(System.Windows.Forms.Label labelControl, SynchronizationContext uiContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var tip = new QfcTipsDetails(labelControl, uiContext);
            await tip.InitializeAsync(token);
            return tip;
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

        internal async Task InitializeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            _token = token;
            await _uiContext;
            _parentType = ResolveParentType();
            SetParentProperties(_parentType);
            if (LabelControl.Visible)
            {
                _state = Enums.ToggleState.On;
            }
            else
            {
                _state = Enums.ToggleState.Off;
            }
        }

        private System.Windows.Forms.Panel _panel;
        private Enums.ToggleState _state;
        private Type _parentType;
        private SynchronizationContext _uiContext;
        private CancellationToken _token;

        private System.Windows.Forms.Label _labelControl;
        public System.Windows.Forms.Label LabelControl { get => _labelControl; internal set => _labelControl = value; }
        
        private TableLayoutPanel _tlp;
        public TableLayoutPanel TLP { get => _tlp; }
        
        private int _columnNumber;
        public int ColumnNumber { get => _columnNumber; }

        private bool _isNavColumn = false;
        public bool IsNavColumn { get => _isNavColumn; set => _isNavColumn = value; }

        private System.Single _columnWidth;
        public float ColumnWidth { get => _columnWidth; }        

        public void Toggle()
        {
            if (_state.HasFlag(Enums.ToggleState.On))
            {
                Toggle(Enums.ToggleState.Off);
            }
            else
            {
                Toggle(Enums.ToggleState.On);
            }
        }

        public void Toggle(bool sharedColumn)
        {
            if (_state.HasFlag(Enums.ToggleState.On))
            {
                Toggle(Enums.ToggleState.Off, sharedColumn);
            }
            else
            {
                Toggle(Enums.ToggleState.On, sharedColumn);
            }
        }

        public void Toggle(Enums.ToggleState desiredState, bool sharedColumn)
        {
            if (desiredState.HasFlag(Enums.ToggleState.On))
            {
                _labelControl.Visible = true;
                _labelControl.Enabled = true;
                if (_parentType == typeof(TableLayoutPanel) && (!IsNavColumn) && ((_tlp.RowCount == 1) | (sharedColumn)))
                    _tlp.ColumnStyles[_columnNumber].Width = _columnWidth;
            }
            else
            {
                _labelControl.Visible = false;
                _labelControl.Enabled = false;
                if (_parentType == typeof(TableLayoutPanel) && (!IsNavColumn) && ((_tlp.RowCount == 1) | (sharedColumn)))
                    _tlp.ColumnStyles[_columnNumber].Width = 0;
            }
            _state = desiredState;
        }

        public void Toggle(Enums.ToggleState desiredState)
        {
            if (desiredState.HasFlag(Enums.ToggleState.On))
            {
                _labelControl.Visible = true;
                _labelControl.Enabled = true;
                if (_parentType == typeof(TableLayoutPanel) && (_tlp.RowCount == 1))
                    _tlp.ColumnStyles[_columnNumber].Width = _columnWidth;
            }
            else
            {
                _labelControl.Visible = false;
                _labelControl.Enabled = false;
                if (_parentType == typeof(TableLayoutPanel) && (_tlp.RowCount == 1))
                    _tlp.ColumnStyles[_columnNumber].Width = 0;
            }
            _state = desiredState;
        }

        public async Task ToggleAsync(Enums.ToggleState desiredState)
        {
            _token.ThrowIfCancellationRequested();
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() => Toggle(desiredState));
            //if (desiredState.HasFlag(Enums.ToggleState.On))
            //{
            //    await _uiContext;
            //    _labelControl.Visible = true;
            //    _labelControl.Enabled = true;
            //    if (_parentType == typeof(TableLayoutPanel) && (_tlp.RowCount == 1))
            //        _tlp.ColumnStyles[_columnNumber].Width = _columnWidth;
            //}
            //else
            //{
            //    await _uiContext;
            //    _labelControl.Visible = false;
            //    _labelControl.Enabled = false;
            //    if (_parentType == typeof(TableLayoutPanel) && (_tlp.RowCount == 1))
            //        _tlp.ColumnStyles[_columnNumber].Width = 0;
            //}
            //_state = desiredState;
        }

        public async Task ToggleAsync(Enums.ToggleState desiredState, bool sharedColumn)
        {
            _token.ThrowIfCancellationRequested();
            await UIThreadExtensions.UiDispatcher.InvokeAsync(()=>Toggle(desiredState, sharedColumn));
        }
    }
}
