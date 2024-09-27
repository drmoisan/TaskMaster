using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS.ReusableTypeClasses.NewSmartSerializable.Config
{
    public partial class ConfigViewer : Form
    {
        public ConfigViewer()
        {
            InitializeComponent();
        }

        public ConfigViewer SetController(ConfigController controller)
        {
            Controller = controller;
            return this;
        }

        internal ConfigController Controller { get; set; }

        private async void ButtonSave_Click(object sender, EventArgs e)
        {
            await Controller.SaveAsync();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Controller.Cancel();
        }

        private async void ComboSpecialFolder_SelectedValueChanged(object sender, EventArgs e)
        {
            //await Controller.ChangeSpecialFolderAsync();
        }

        private async void ButtonOpen_Click(object sender, EventArgs e)
        {
            await Controller.OpenFileChooserAsync();
        }
    }
}
