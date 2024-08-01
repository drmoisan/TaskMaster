using System;
using System.Diagnostics;

namespace ToDoModel
{
    [Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
    public partial class ProjectViewer : System.Windows.Forms.Form
    {

        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is not null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SplitContainer1 = new System.Windows.Forms.SplitContainer();
            this.OlvProjectData = new BrightIdeasSoftware.ObjectListView();
            this.OlvProjectID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvProjectName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvProgramName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.button1 = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.ProjectInfoBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.OlvProgramID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).BeginInit();
            this.SplitContainer1.Panel1.SuspendLayout();
            this.SplitContainer1.Panel2.SuspendLayout();
            this.SplitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OlvProjectData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ProjectInfoBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // SplitContainer1
            // 
            this.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer1.Location = new System.Drawing.Point(0, 0);
            this.SplitContainer1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SplitContainer1.Name = "SplitContainer1";
            this.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // SplitContainer1.Panel1
            // 
            this.SplitContainer1.Panel1.Controls.Add(this.OlvProjectData);
            // 
            // SplitContainer1.Panel2
            // 
            this.SplitContainer1.Panel2.Controls.Add(this.button1);
            this.SplitContainer1.Panel2.Controls.Add(this.ButtonCancel);
            this.SplitContainer1.Panel2.Controls.Add(this.ButtonOk);
            this.SplitContainer1.Size = new System.Drawing.Size(1600, 865);
            this.SplitContainer1.SplitterDistance = 766;
            this.SplitContainer1.SplitterWidth = 8;
            this.SplitContainer1.TabIndex = 0;
            // 
            // OlvProjectData
            // 
            this.OlvProjectData.AllColumns.Add(this.OlvProjectID);
            this.OlvProjectData.AllColumns.Add(this.OlvProjectName);
            this.OlvProjectData.AllColumns.Add(this.OlvProgramName);
            this.OlvProjectData.AllColumns.Add(this.OlvProgramID);
            this.OlvProjectData.AllowColumnReorder = true;
            this.OlvProjectData.AllowDrop = true;
            this.OlvProjectData.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.OlvProjectData.CellEditUseWholeCell = false;
            this.OlvProjectData.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.OlvProjectID,
            this.OlvProjectName,
            this.OlvProgramName,
            this.OlvProgramID});
            this.OlvProjectData.Cursor = System.Windows.Forms.Cursors.Default;
            this.OlvProjectData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OlvProjectData.HasCollapsibleGroups = false;
            this.OlvProjectData.HideSelection = false;
            this.OlvProjectData.Location = new System.Drawing.Point(0, 0);
            this.OlvProjectData.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.OlvProjectData.Name = "OlvProjectData";
            this.OlvProjectData.ShowGroups = false;
            this.OlvProjectData.Size = new System.Drawing.Size(1600, 766);
            this.OlvProjectData.TabIndex = 0;
            this.OlvProjectData.UseCompatibleStateImageBehavior = false;
            this.OlvProjectData.View = System.Windows.Forms.View.Details;
            this.OlvProjectData.CellEditFinishing += new BrightIdeasSoftware.CellEditEventHandler(this.OlvProjInfo_CellEditFinishing);
            this.OlvProjectData.CellEditStarting += new BrightIdeasSoftware.CellEditEventHandler(this.OlvProjInfo_CellEditStarting);
            this.OlvProjectData.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OlvProjInfo_KeyUp);
            // 
            // OlvProjectID
            // 
            this.OlvProjectID.AspectName = "ProjectID";
            this.OlvProjectID.Groupable = false;
            this.OlvProjectID.Text = "Project ID";
            this.OlvProjectID.Width = 166;
            // 
            // OlvProjectName
            // 
            this.OlvProjectName.AspectName = "ProjectName";
            this.OlvProjectName.Groupable = false;
            this.OlvProjectName.Text = "Project Name";
            this.OlvProjectName.Width = 318;
            // 
            // OlvProgramName
            // 
            this.OlvProgramName.AspectName = "ProgramName";
            this.OlvProgramName.Groupable = false;
            this.OlvProgramName.Text = "Program Name";
            this.OlvProgramName.Width = 283;
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button1.Location = new System.Drawing.Point(1022, 23);
            this.button1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(188, 44);
            this.button1.TabIndex = 2;
            this.button1.Text = "NEW";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ButtonCancel.Location = new System.Drawing.Point(742, 23);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(188, 44);
            this.ButtonCancel.TabIndex = 1;
            this.ButtonCancel.Text = "CANCEL";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonOk
            // 
            this.ButtonOk.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ButtonOk.Location = new System.Drawing.Point(486, 23);
            this.ButtonOk.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(188, 44);
            this.ButtonOk.TabIndex = 0;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // ProjectInfoBindingSource
            // 
            this.ProjectInfoBindingSource.DataSource = typeof(ToDoModel.ProjectData);
            // 
            // OlvProgramID
            // 
            this.OlvProgramID.AspectName = "ProgramID";
            this.OlvProgramID.Text = "Program ID";
            // 
            // ProjectViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1600, 865);
            this.Controls.Add(this.SplitContainer1);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "ProjectViewer";
            this.Text = "ProjectInfoWindow";
            this.Resize += new System.EventHandler(this.ProjectInfoWindow_Resize);
            this.SplitContainer1.Panel1.ResumeLayout(false);
            this.SplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).EndInit();
            this.SplitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.OlvProjectData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ProjectInfoBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        internal System.Windows.Forms.SplitContainer SplitContainer1;
        internal System.Windows.Forms.BindingSource ProjectInfoBindingSource;
        internal System.Windows.Forms.Button ButtonCancel;
        internal System.Windows.Forms.Button ButtonOk;
        internal BrightIdeasSoftware.ObjectListView OlvProjectData;
        internal BrightIdeasSoftware.OLVColumn OlvProjectID;
        internal BrightIdeasSoftware.OLVColumn OlvProjectName;
        internal BrightIdeasSoftware.OLVColumn OlvProgramName;
        internal System.Windows.Forms.Button button1;
        internal BrightIdeasSoftware.OLVColumn OlvProgramID;
    }
}