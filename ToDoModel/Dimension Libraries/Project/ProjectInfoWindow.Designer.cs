using System;
using System.Diagnostics;

namespace ToDoModel
{
    [Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
    public partial class ProjectInfoWindow : System.Windows.Forms.Form
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
            components = new System.ComponentModel.Container();
            SplitContainer1 = new System.Windows.Forms.SplitContainer();
            olvProjInfo = new BrightIdeasSoftware.ObjectListView();
            olvProjInfo.KeyUp += new System.Windows.Forms.KeyEventHandler(olvProjInfo_KeyUp);
            olvProjInfo.CellEditStarting += new BrightIdeasSoftware.CellEditEventHandler(olvProjInfo_CellEditStarting);
            olvProjInfo.CellEditFinishing += new BrightIdeasSoftware.CellEditEventHandler(olvProjInfo_CellEditFinishing);
            OlvProjectID = new BrightIdeasSoftware.OLVColumn();
            OlvProjectName = new BrightIdeasSoftware.OLVColumn();
            OlvProgramName = new BrightIdeasSoftware.OLVColumn();
            BTN_CANCEL = new System.Windows.Forms.Button();
            BTN_CANCEL.Click += new EventHandler(BTN_CANCEL_Click);
            BTN_OK = new System.Windows.Forms.Button();
            BTN_OK.Click += new EventHandler(BTN_OK_Click);
            ProjectInfoBindingSource = new System.Windows.Forms.BindingSource(components);
            ((System.ComponentModel.ISupportInitialize)SplitContainer1).BeginInit();
            SplitContainer1.Panel1.SuspendLayout();
            SplitContainer1.Panel2.SuspendLayout();
            SplitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)olvProjInfo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ProjectInfoBindingSource).BeginInit();
            SuspendLayout();
            // 
            // SplitContainer1
            // 
            SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            SplitContainer1.Location = new System.Drawing.Point(0, 0);
            SplitContainer1.Name = "SplitContainer1";
            SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // SplitContainer1.Panel1
            // 
            SplitContainer1.Panel1.Controls.Add(olvProjInfo);
            // 
            // SplitContainer1.Panel2
            // 
            SplitContainer1.Panel2.Controls.Add(BTN_CANCEL);
            SplitContainer1.Panel2.Controls.Add(BTN_OK);
            SplitContainer1.Size = new System.Drawing.Size(800, 450);
            SplitContainer1.SplitterDistance = 399;
            SplitContainer1.TabIndex = 0;
            // 
            // olvProjInfo
            // 
            olvProjInfo.AllColumns.Add(OlvProjectID);
            olvProjInfo.AllColumns.Add(OlvProjectName);
            olvProjInfo.AllColumns.Add(OlvProgramName);
            olvProjInfo.AllowColumnReorder = true;
            olvProjInfo.AllowDrop = true;
            olvProjInfo.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            olvProjInfo.CellEditUseWholeCell = false;
            olvProjInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { OlvProjectID, OlvProjectName, OlvProgramName });
            olvProjInfo.Cursor = System.Windows.Forms.Cursors.Default;
            olvProjInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            olvProjInfo.HasCollapsibleGroups = false;
            olvProjInfo.HideSelection = false;
            olvProjInfo.Location = new System.Drawing.Point(0, 0);
            olvProjInfo.Name = "olvProjInfo";
            olvProjInfo.ShowGroups = false;
            olvProjInfo.Size = new System.Drawing.Size(800, 399);
            olvProjInfo.TabIndex = 0;
            olvProjInfo.UseCompatibleStateImageBehavior = false;
            olvProjInfo.View = System.Windows.Forms.View.Details;
            // 
            // OlvProjectID
            // 
            OlvProjectID.AspectName = "ProjectID";
            OlvProjectID.Groupable = false;
            OlvProjectID.Text = "Project ID";
            OlvProjectID.Width = 166;
            // 
            // OlvProjectName
            // 
            OlvProjectName.AspectName = "ProjectName";
            OlvProjectName.Groupable = false;
            OlvProjectName.Text = "Project Name";
            OlvProjectName.Width = 318;
            // 
            // OlvProgramName
            // 
            OlvProgramName.AspectName = "ProgramName";
            OlvProgramName.Groupable = false;
            OlvProgramName.Text = "Program Name";
            OlvProgramName.Width = 283;
            // 
            // BTN_CANCEL
            // 
            BTN_CANCEL.Anchor = System.Windows.Forms.AnchorStyles.None;
            BTN_CANCEL.Location = new System.Drawing.Point(440, 12);
            BTN_CANCEL.Name = "BTN_CANCEL";
            BTN_CANCEL.Size = new System.Drawing.Size(94, 23);
            BTN_CANCEL.TabIndex = 1;
            BTN_CANCEL.Text = "CANCEL";
            BTN_CANCEL.UseVisualStyleBackColor = true;
            // 
            // BTN_OK
            // 
            BTN_OK.Anchor = System.Windows.Forms.AnchorStyles.None;
            BTN_OK.Location = new System.Drawing.Point(312, 12);
            BTN_OK.Name = "BTN_OK";
            BTN_OK.Size = new System.Drawing.Size(94, 23);
            BTN_OK.TabIndex = 0;
            BTN_OK.Text = "OK";
            BTN_OK.UseVisualStyleBackColor = true;
            // 
            // ProjectInfoBindingSource
            // 
            ProjectInfoBindingSource.DataSource = typeof(ProjectInfo);
            // 
            // ProjectInfoWindow
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6.0f, 13.0f);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(SplitContainer1);
            Name = "ProjectInfoWindow";
            Text = "ProjectInfoWindow";
            SplitContainer1.Panel1.ResumeLayout(false);
            SplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SplitContainer1).EndInit();
            SplitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)olvProjInfo).EndInit();
            ((System.ComponentModel.ISupportInitialize)ProjectInfoBindingSource).EndInit();
            Load += new EventHandler(ProjectInfoWindow_Load);
            Resize += new EventHandler(ProjectInfoWindow_Resize);
            ResumeLayout(false);

        }

        internal System.Windows.Forms.SplitContainer SplitContainer1;
        internal System.Windows.Forms.BindingSource ProjectInfoBindingSource;
        internal System.Windows.Forms.Button BTN_CANCEL;
        internal System.Windows.Forms.Button BTN_OK;
        internal BrightIdeasSoftware.ObjectListView olvProjInfo;
        internal BrightIdeasSoftware.OLVColumn OlvProjectID;
        internal BrightIdeasSoftware.OLVColumn OlvProjectName;
        internal BrightIdeasSoftware.OLVColumn OlvProgramName;
    }
}