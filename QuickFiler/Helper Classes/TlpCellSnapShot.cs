using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;

namespace QuickFiler
{
    public class TlpCellStates: Dictionary<string, TlpCellSnapShotList>
    {
        public TlpCellStates(): base() {}

        public TlpCellStates(IEnumerable<KeyValuePair<string, TlpCellSnapShotList>> collection) : base() 
        {
            foreach (var kvp in collection)
                this.Add(kvp.Key, kvp.Value);
        }

        public TlpCellStates(IEnumerable<KeyValuePair<string, List<TlpCellSnapShot>>> collection) : base()
        {
            foreach (var kvp in collection)
                this.Add(kvp.Key, new TlpCellSnapShotList(kvp.Value));
        }

        public bool TryAddState(string stateName)
        {
            if (this.ContainsKey(stateName))
                return false;
            else
            {
                this.Add(stateName, new TlpCellSnapShotList());
                return true;
            }
        }

        public bool TryAddState(string stateName, List<TlpCellSnapShot> snapShots)
        {
            if (this.ContainsKey(stateName))
                return false;
            else
            {
                this.Add(stateName, new TlpCellSnapShotList(snapShots));
                return true;
            }
        }

    }

    public class TlpCellSnapShotList: List<TlpCellSnapShot>
    {
        public TlpCellSnapShotList() : base() { }
        public TlpCellSnapShotList(IEnumerable<TlpCellSnapShot> collection) : base(collection) { }
        
        public void ApplyState(Control root)
        {
            this.ForEach(s => s.ApplyState(root));
        }
    }

    public class TlpCellSnapShot
    {
        public TlpCellSnapShot() { }

        public TlpCellSnapShot(TableLayoutPanel tlp, Control control) 
        { 
            SnapCell(tlp, control);
        }

        public void SnapCell(TableLayoutPanel tlp, Control control)
        {
            TlpName = tlp.Name;
            ControlName = control.Name;
            Cell = tlp.GetCellPosition(control);
            
            RowSpan = tlp.GetRowSpan(control);
            
            RowStyles = new List<RowStyle>();
            for (int i = Cell.Row; i < Cell.Row + RowSpan; i++)
                RowStyles.Add(tlp.RowStyles[i]);
            
            ColumnSpan = tlp.GetColumnSpan(control);
            ColumnStyles = new List<ColumnStyle>();
            for (int i = Cell.Column; i < Cell.Column + ColumnSpan; i++)
                ColumnStyles.Add(tlp.ColumnStyles[i]);

            Enabled = control.Enabled;
            Visible = control.Visible;
            if (ControlName.StartsWith("LblAc") && control is Label)
            {
                AcceleratorText = ((Label)control).Text;
            }
        }

        private string _tlpName;
        public string TlpName { get => _tlpName; set => _tlpName = value; }

        private string _controlName;
        public string ControlName { get => _controlName; set => _controlName = value; }

        private string _acceleratorText;
        public string AcceleratorText { get => _acceleratorText; set => _acceleratorText = value; }

        private TableLayoutPanelCellPosition _cell;
        public TableLayoutPanelCellPosition Cell { get => _cell; set => _cell = value; }
        public int Row { get => _cell.Row; set => _cell.Row = value; }
        public int Column { get => _cell.Column; set => _cell.Column = value; }
        
        private List<RowStyle> _rowStyles;
        public List<RowStyle> RowStyles { get => _rowStyles; set => _rowStyles = value; }

        private int _rowSpan;
        public int RowSpan { get => _rowSpan; set => _rowSpan = value; }

        private int _columnSpan;
        public int ColumnSpan { get => _columnSpan; set => _columnSpan = value; }

        private List<ColumnStyle> _columnStyles;
        public List<ColumnStyle> ColumnStyles { get => _columnStyles; set => _columnStyles = value; }

        private bool _enabled;
        public bool Enabled { get => _enabled; set => _enabled = value; }

        private bool _visible;
        public bool Visible { get => _visible; set => _visible = value; }
        
        public void ApplyState(Control root)
        {
            var tlp = root.Controls.Find(TlpName, true).FirstOrDefault() as TableLayoutPanel;
            for (int i = Cell.Row; i < Cell.Row + RowSpan; i++)
                tlp.RowStyles[i] = RowStyles[i - Cell.Row].Clone();
            //tlp.RowStyles[Cell.Row] = RowStyles.Clone();

            for (int i = Cell.Column; i < Cell.Column + ColumnSpan; i++)
                tlp.ColumnStyles[i] = ColumnStyles[i - Cell.Column].Clone();
            //tlp.ColumnStyles[Cell.Column] = ColumnStyles.Clone();

            if (!ControlName.IsNullOrEmpty())
            {
                var control = root.Controls.Find(ControlName, true).FirstOrDefault();
                control.Enabled = Enabled;
                control.Visible = Visible;
                if (control.Parent != tlp)
                {
                    control.Parent = tlp;
                }
                tlp.SetCellPosition(control, Cell);
                tlp.SetRowSpan(control, RowSpan);
                tlp.SetColumnSpan(control, ColumnSpan);

                if (ControlName.StartsWith("LblAc") && control is Label)
                {
                    ((Label)control).Text = AcceleratorText;
                }
            }
        }
    }
}
