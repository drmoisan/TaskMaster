using BrightIdeasSoftware;
using System.Windows.Forms;

namespace SVGControl
{
    partial class SvgResourceViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.imageListSmall = new System.Windows.Forms.ImageList(this.components);
            this.imageListLarge = new System.Windows.Forms.ImageList(this.components);
            this.tlp1 = new System.Windows.Forms.TableLayoutPanel();
            this.olv = new BrightIdeasSoftware.ObjectListView();
            this.resourceName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.resourceImage = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.tlp1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olv)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageListSmall
            // 
            this.imageListSmall.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageListSmall.ImageSize = new System.Drawing.Size(24, 24);
            this.imageListSmall.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // imageListLarge
            // 
            this.imageListLarge.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageListLarge.ImageSize = new System.Drawing.Size(48, 48);
            this.imageListLarge.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // tlp1
            // 
            this.tlp1.ColumnCount = 1;
            this.tlp1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp1.Controls.Add(this.olv, 0, 0);
            this.tlp1.Controls.Add(this.tableLayoutPanel1, 0, 1);
            this.tlp1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlp1.Location = new System.Drawing.Point(0, 0);
            this.tlp1.Name = "tlp1";
            this.tlp1.RowCount = 2;
            this.tlp1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlp1.Size = new System.Drawing.Size(800, 450);
            this.tlp1.TabIndex = 0;
            // 
            // olv
            // 
            this.olv.AllColumns.Add(this.resourceName);
            this.olv.AllColumns.Add(this.resourceImage);
            this.olv.CellEditUseWholeCell = false;
            this.olv.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.resourceName,
            this.resourceImage});
            this.olv.Cursor = System.Windows.Forms.Cursors.Default;
            this.olv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olv.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.olv.FullRowSelect = true;
            this.olv.HasCollapsibleGroups = false;
            this.olv.HideSelection = false;
            this.olv.LargeImageList = this.imageListLarge;
            this.olv.Location = new System.Drawing.Point(3, 3);
            this.olv.MultiSelect = false;
            this.olv.Name = "olv";
            this.olv.Size = new System.Drawing.Size(794, 344);
            this.olv.SmallImageList = this.imageListLarge;
            this.olv.TabIndex = 1;
            this.olv.UseCellFormatEvents = true;
            this.olv.UseCompatibleStateImageBehavior = false;
            this.olv.UseHotItem = true;
            this.olv.View = System.Windows.Forms.View.Details;
            // 
            // resourceName
            // 
            this.resourceName.AspectName = "Name";
            this.resourceName.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.resourceName.Text = "Name";
            this.resourceName.Width = 371;
            // 
            // resourceImage
            // 
            this.resourceImage.AspectName = "";
            this.resourceImage.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.resourceImage.Text = "Image";
            this.resourceImage.Width = 423;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.Cancel, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.Ok, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 353);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(794, 94);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // Cancel
            // 
            this.Cancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Cancel.Location = new System.Drawing.Point(400, 3);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(194, 88);
            this.Cancel.TabIndex = 2;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Ok.Location = new System.Drawing.Point(200, 3);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(194, 88);
            this.Ok.TabIndex = 0;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // SvgResourceViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tlp1);
            this.Name = "SvgResourceViewer";
            this.Text = "SvgResourceViewer";
            this.tlp1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olv)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        internal System.Windows.Forms.ImageList imageListLarge;
        internal System.Windows.Forms.ImageList imageListSmall;
        private TableLayoutPanel tlp1;
        internal ObjectListView olv;
        internal OLVColumn resourceName;
        internal OLVColumn resourceImage;
        private TableLayoutPanel tableLayoutPanel1;
        internal Button Cancel;
        internal Button Ok;
    }
}