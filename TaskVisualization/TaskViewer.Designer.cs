using System;
using System.Diagnostics;

namespace TaskVisualization
{
    [Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
    public partial class TaskViewer : System.Windows.Forms.Form
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
            Frame1 = new System.Windows.Forms.Panel();
            XlScBullpin = new System.Windows.Forms.Label();
            XlScToday = new System.Windows.Forms.Label();
            XlScWaiting = new System.Windows.Forms.Label();
            XlScUnprocessed = new System.Windows.Forms.Label();
            XlScNews = new System.Windows.Forms.Label();
            XlScEmail = new System.Windows.Forms.Label();
            XlScReadingbusiness = new System.Windows.Forms.Label();
            XlScCalls = new System.Windows.Forms.Label();
            XlScInternet = new System.Windows.Forms.Label();
            XlScPreread = new System.Windows.Forms.Label();
            XlScMeeting = new System.Windows.Forms.Label();
            XlScPersonal = new System.Windows.Forms.Label();
            XlTopic = new System.Windows.Forms.Label();
            XlProject = new System.Windows.Forms.Label();
            XlPeople = new System.Windows.Forms.Label();
            XlContext = new System.Windows.Forms.Label();
            ShortcutWaitingFor = new System.Windows.Forms.Button();
            ShortcutWaitingFor.Click += new EventHandler(ShortcutWaitingFor_Click);
            CbxBullpin = new System.Windows.Forms.CheckBox();
            CbxBullpin.CheckedChanged += new EventHandler(CbxBullpin_CheckedChanged);
            CbxToday = new System.Windows.Forms.CheckBox();
            CbxToday.CheckedChanged += new EventHandler(CbxToday_CheckedChanged);
            CbxFlagAsTask = new System.Windows.Forms.CheckBox();
            CbxFlagAsTask.CheckedChanged += new EventHandler(CbxFlag_CheckedChanged);
            ShortcutEmail = new System.Windows.Forms.Button();
            ShortcutEmail.Click += new EventHandler(ShortcutEmail_Click);
            ShortcutNews = new System.Windows.Forms.Button();
            ShortcutNews.Click += new EventHandler(ShortcutReadingNews_Click);
            ShortcutUnprocessed = new System.Windows.Forms.Button();
            ShortcutUnprocessed.Click += new EventHandler(ShortcutUnprocessed_Click);
            ShortcutReadingBusiness = new System.Windows.Forms.Button();
            ShortcutReadingBusiness.Click += new EventHandler(ShortcutReadingBusiness_Click);
            ShortcutCalls = new System.Windows.Forms.Button();
            ShortcutCalls.Click += new EventHandler(ShortcutCalls_Click);
            ShortcutInternet = new System.Windows.Forms.Button();
            ShortcutInternet.Click += new EventHandler(ShortcutInternet_Click);
            ShortcutPreRead = new System.Windows.Forms.Button();
            ShortcutPreRead.Click += new EventHandler(ShortcutPreRead_Click);
            ShortcutMeeting = new System.Windows.Forms.Button();
            ShortcutMeeting.Click += new EventHandler(ShortcutMeeting_Click);
            LblTopic = new System.Windows.Forms.Label();
            ShortcutPersonal = new System.Windows.Forms.Button();
            ShortcutPersonal.Click += new EventHandler(ShortcutPersonal_Click);
            LblProject = new System.Windows.Forms.Label();
            LblPeople = new System.Windows.Forms.Label();
            LblContext = new System.Windows.Forms.Label();
            TopicSelection = new System.Windows.Forms.Label();
            TopicSelection.Click += new EventHandler(TopicSelection_Click);
            ProjectSelection = new System.Windows.Forms.Label();
            ProjectSelection.Click += new EventHandler(ProjectSelection_Click);
            PeopleSelection = new System.Windows.Forms.Label();
            PeopleSelection.Click += new EventHandler(PeopleSelection_Click);
            CategorySelection = new System.Windows.Forms.Label();
            CategorySelection.Click += new EventHandler(CategorySelection_Click);
            OKButton = new System.Windows.Forms.Button();
            OKButton.Click += new EventHandler(OKButton_Click);
            Cancel_Button = new System.Windows.Forms.Button();
            Cancel_Button.Click += new EventHandler(Cancel_Button_Click);
            LblTaskname = new System.Windows.Forms.Label();
            TaskName = new System.Windows.Forms.TextBox();
            TaskName.KeyDown += new System.Windows.Forms.KeyEventHandler(TaskName_KeyDown);
            TaskName.KeyUp += new System.Windows.Forms.KeyEventHandler(TaskName_KeyUp);
            TaskName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(TaskName_KeyPress);
            LblPriority = new System.Windows.Forms.Label();
            LblKbf = new System.Windows.Forms.Label();
            LblDuration = new System.Windows.Forms.Label();
            PriorityBox = new System.Windows.Forms.ComboBox();
            PriorityBox.SelectedIndexChanged += new EventHandler(PriorityBox_SelectedIndexChanged);
            KbSelector = new System.Windows.Forms.ComboBox();
            KbSelector.SelectedIndexChanged += new EventHandler(KbSelector_SelectedIndexChanged);
            Duration = new System.Windows.Forms.TextBox();
            DtDuedate = new System.Windows.Forms.DateTimePicker();
            LblDuedate = new System.Windows.Forms.Label();
            LblReminder = new System.Windows.Forms.Label();
            DtReminder = new System.Windows.Forms.DateTimePicker();
            XlTaskname = new System.Windows.Forms.Label();
            XlImportance = new System.Windows.Forms.Label();
            XlKanban = new System.Windows.Forms.Label();
            XlWorktime = new System.Windows.Forms.Label();
            XlOk = new System.Windows.Forms.Label();
            XlCancel = new System.Windows.Forms.Label();
            XlReminder = new System.Windows.Forms.Label();
            XlDuedate = new System.Windows.Forms.Label();
            Frame1.SuspendLayout();
            SuspendLayout();
            // 
            // Frame1
            // 
            Frame1.Controls.Add(XlScBullpin);
            Frame1.Controls.Add(XlScToday);
            Frame1.Controls.Add(XlScWaiting);
            Frame1.Controls.Add(XlScUnprocessed);
            Frame1.Controls.Add(XlScNews);
            Frame1.Controls.Add(XlScEmail);
            Frame1.Controls.Add(XlScReadingbusiness);
            Frame1.Controls.Add(XlScCalls);
            Frame1.Controls.Add(XlScInternet);
            Frame1.Controls.Add(XlScPreread);
            Frame1.Controls.Add(XlScMeeting);
            Frame1.Controls.Add(XlScPersonal);
            Frame1.Controls.Add(XlTopic);
            Frame1.Controls.Add(XlProject);
            Frame1.Controls.Add(XlPeople);
            Frame1.Controls.Add(XlContext);
            Frame1.Controls.Add(ShortcutWaitingFor);
            Frame1.Controls.Add(CbxBullpin);
            Frame1.Controls.Add(CbxToday);
            Frame1.Controls.Add(CbxFlagAsTask);
            Frame1.Controls.Add(ShortcutEmail);
            Frame1.Controls.Add(ShortcutNews);
            Frame1.Controls.Add(ShortcutUnprocessed);
            Frame1.Controls.Add(ShortcutReadingBusiness);
            Frame1.Controls.Add(ShortcutCalls);
            Frame1.Controls.Add(ShortcutInternet);
            Frame1.Controls.Add(ShortcutPreRead);
            Frame1.Controls.Add(ShortcutMeeting);
            Frame1.Controls.Add(LblTopic);
            Frame1.Controls.Add(ShortcutPersonal);
            Frame1.Controls.Add(LblProject);
            Frame1.Controls.Add(LblPeople);
            Frame1.Controls.Add(LblContext);
            Frame1.Controls.Add(TopicSelection);
            Frame1.Controls.Add(ProjectSelection);
            Frame1.Controls.Add(PeopleSelection);
            Frame1.Controls.Add(CategorySelection);
            Frame1.Location = new System.Drawing.Point(7, 186);
            Frame1.Name = "Frame1";
            Frame1.Size = new System.Drawing.Size(570, 322);
            Frame1.TabIndex = 0;
            // 
            // XlScBullpin
            // 
            XlScBullpin.AutoSize = true;
            XlScBullpin.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlScBullpin.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlScBullpin.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlScBullpin.Location = new System.Drawing.Point(438, 274);
            XlScBullpin.Name = "XlScBullpin";
            XlScBullpin.Size = new System.Drawing.Size(17, 16);
            XlScBullpin.TabIndex = 38;
            XlScBullpin.Text = "B";
            // 
            // XlScToday
            // 
            XlScToday.AutoSize = true;
            XlScToday.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlScToday.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlScToday.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlScToday.Location = new System.Drawing.Point(352, 274);
            XlScToday.Name = "XlScToday";
            XlScToday.Size = new System.Drawing.Size(17, 16);
            XlScToday.TabIndex = 37;
            XlScToday.Text = "T";
            // 
            // XlScWaiting
            // 
            XlScWaiting.AutoSize = true;
            XlScWaiting.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlScWaiting.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlScWaiting.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlScWaiting.Location = new System.Drawing.Point(20, 255);
            XlScWaiting.Name = "XlScWaiting";
            XlScWaiting.Size = new System.Drawing.Size(21, 16);
            XlScWaiting.TabIndex = 36;
            XlScWaiting.Text = "W";
            // 
            // XlScUnprocessed
            // 
            XlScUnprocessed.AutoSize = true;
            XlScUnprocessed.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlScUnprocessed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlScUnprocessed.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlScUnprocessed.Location = new System.Drawing.Point(419, 255);
            XlScUnprocessed.Name = "XlScUnprocessed";
            XlScUnprocessed.Size = new System.Drawing.Size(18, 16);
            XlScUnprocessed.TabIndex = 35;
            XlScUnprocessed.Text = "U";
            // 
            // XlScNews
            // 
            XlScNews.AutoSize = true;
            XlScNews.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlScNews.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlScNews.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlScNews.Location = new System.Drawing.Point(284, 255);
            XlScNews.Name = "XlScNews";
            XlScNews.Size = new System.Drawing.Size(18, 16);
            XlScNews.TabIndex = 34;
            XlScNews.Text = "N";
            // 
            // XlScEmail
            // 
            XlScEmail.AutoSize = true;
            XlScEmail.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlScEmail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlScEmail.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlScEmail.Location = new System.Drawing.Point(166, 255);
            XlScEmail.Name = "XlScEmail";
            XlScEmail.Size = new System.Drawing.Size(17, 16);
            XlScEmail.TabIndex = 33;
            XlScEmail.Text = "E";
            // 
            // XlScReadingbusiness
            // 
            XlScReadingbusiness.AutoSize = true;
            XlScReadingbusiness.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlScReadingbusiness.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlScReadingbusiness.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlScReadingbusiness.Location = new System.Drawing.Point(430, 216);
            XlScReadingbusiness.Name = "XlScReadingbusiness";
            XlScReadingbusiness.Size = new System.Drawing.Size(18, 16);
            XlScReadingbusiness.TabIndex = 32;
            XlScReadingbusiness.Text = "R";
            // 
            // XlScCalls
            // 
            XlScCalls.AutoSize = true;
            XlScCalls.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlScCalls.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlScCalls.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlScCalls.Location = new System.Drawing.Point(301, 216);
            XlScCalls.Name = "XlScCalls";
            XlScCalls.Size = new System.Drawing.Size(17, 16);
            XlScCalls.TabIndex = 31;
            XlScCalls.Text = "C";
            // 
            // XlScInternet
            // 
            XlScInternet.AutoSize = true;
            XlScInternet.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlScInternet.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlScInternet.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlScInternet.Location = new System.Drawing.Point(166, 215);
            XlScInternet.Name = "XlScInternet";
            XlScInternet.Size = new System.Drawing.Size(11, 16);
            XlScInternet.TabIndex = 30;
            XlScInternet.Text = "I";
            // 
            // XlScPreread
            // 
            XlScPreread.AutoSize = true;
            XlScPreread.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlScPreread.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlScPreread.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlScPreread.Location = new System.Drawing.Point(430, 175);
            XlScPreread.Name = "XlScPreread";
            XlScPreread.Size = new System.Drawing.Size(17, 16);
            XlScPreread.TabIndex = 29;
            XlScPreread.Text = "P";
            // 
            // XlScMeeting
            // 
            XlScMeeting.AutoSize = true;
            XlScMeeting.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlScMeeting.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlScMeeting.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlScMeeting.Location = new System.Drawing.Point(301, 175);
            XlScMeeting.Name = "XlScMeeting";
            XlScMeeting.Size = new System.Drawing.Size(19, 16);
            XlScMeeting.TabIndex = 28;
            XlScMeeting.Text = "M";
            // 
            // XlScPersonal
            // 
            XlScPersonal.AutoSize = true;
            XlScPersonal.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlScPersonal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlScPersonal.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlScPersonal.Location = new System.Drawing.Point(166, 174);
            XlScPersonal.Name = "XlScPersonal";
            XlScPersonal.Size = new System.Drawing.Size(17, 16);
            XlScPersonal.TabIndex = 27;
            XlScPersonal.Text = "P";
            // 
            // XlTopic
            // 
            XlTopic.AutoSize = true;
            XlTopic.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlTopic.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlTopic.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlTopic.Location = new System.Drawing.Point(6, 134);
            XlTopic.Name = "XlTopic";
            XlTopic.Size = new System.Drawing.Size(17, 16);
            XlTopic.TabIndex = 22;
            XlTopic.Text = "T";
            // 
            // XlProject
            // 
            XlProject.AutoSize = true;
            XlProject.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlProject.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlProject.Location = new System.Drawing.Point(6, 98);
            XlProject.Name = "XlProject";
            XlProject.Size = new System.Drawing.Size(17, 16);
            XlProject.TabIndex = 21;
            XlProject.Text = "P";
            // 
            // XlPeople
            // 
            XlPeople.AutoSize = true;
            XlPeople.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlPeople.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlPeople.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlPeople.Location = new System.Drawing.Point(6, 60);
            XlPeople.Name = "XlPeople";
            XlPeople.Size = new System.Drawing.Size(17, 16);
            XlPeople.TabIndex = 20;
            XlPeople.Text = "P";
            // 
            // XlContext
            // 
            XlContext.AutoSize = true;
            XlContext.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlContext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlContext.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlContext.Location = new System.Drawing.Point(6, 25);
            XlContext.Name = "XlContext";
            XlContext.Size = new System.Drawing.Size(17, 16);
            XlContext.TabIndex = 17;
            XlContext.Text = "C";
            // 
            // ShortcutWaitingFor
            // 
            ShortcutWaitingFor.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ShortcutWaitingFor.BackColor = System.Drawing.Color.DarkMagenta;
            ShortcutWaitingFor.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            ShortcutWaitingFor.Location = new System.Drawing.Point(13, 246);
            ShortcutWaitingFor.Name = "ShortcutWaitingFor";
            ShortcutWaitingFor.Size = new System.Drawing.Size(126, 34);
            ShortcutWaitingFor.TabIndex = 19;
            ShortcutWaitingFor.Text = "Waiting For";
            ShortcutWaitingFor.UseVisualStyleBackColor = false;
            // 
            // CbxBullpin
            // 
            CbxBullpin.AutoSize = true;
            CbxBullpin.Location = new System.Drawing.Point(424, 290);
            CbxBullpin.Name = "CbxBullpin";
            CbxBullpin.Size = new System.Drawing.Size(113, 17);
            CbxBullpin.TabIndex = 18;
            CbxBullpin.Text = "BULLPIN Priorities";
            CbxBullpin.UseVisualStyleBackColor = true;
            // 
            // CbxToday
            // 
            CbxToday.AutoSize = true;
            CbxToday.Location = new System.Drawing.Point(292, 290);
            CbxToday.Name = "CbxToday";
            CbxToday.Size = new System.Drawing.Size(110, 17);
            CbxToday.TabIndex = 17;
            CbxToday.Text = "Complete TODAY";
            CbxToday.UseVisualStyleBackColor = true;
            // 
            // CbxFlagAsTask
            // 
            CbxFlagAsTask.AutoSize = true;
            CbxFlagAsTask.Checked = true;
            CbxFlagAsTask.CheckState = System.Windows.Forms.CheckState.Checked;
            CbxFlagAsTask.Location = new System.Drawing.Point(160, 290);
            CbxFlagAsTask.Name = "CbxFlagAsTask";
            CbxFlagAsTask.Size = new System.Drawing.Size(114, 17);
            CbxFlagAsTask.TabIndex = 16;
            CbxFlagAsTask.Text = "Flag For Follow Up";
            CbxFlagAsTask.UseVisualStyleBackColor = true;
            // 
            // ShortcutEmail
            // 
            ShortcutEmail.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ShortcutEmail.BackColor = System.Drawing.Color.FromArgb(255, 192, 128);
            ShortcutEmail.Location = new System.Drawing.Point(160, 246);
            ShortcutEmail.Name = "ShortcutEmail";
            ShortcutEmail.Size = new System.Drawing.Size(126, 34);
            ShortcutEmail.TabIndex = 15;
            ShortcutEmail.Text = "Email";
            ShortcutEmail.UseVisualStyleBackColor = false;
            // 
            // ShortcutNews
            // 
            ShortcutNews.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ShortcutNews.BackColor = System.Drawing.Color.FromArgb(255, 255, 128);
            ShortcutNews.ForeColor = System.Drawing.SystemColors.ControlText;
            ShortcutNews.Location = new System.Drawing.Point(292, 246);
            ShortcutNews.Name = "ShortcutNews";
            ShortcutNews.Size = new System.Drawing.Size(126, 34);
            ShortcutNews.TabIndex = 14;
            ShortcutNews.Text = "News | Articles | Other";
            ShortcutNews.UseVisualStyleBackColor = false;
            // 
            // ShortcutUnprocessed
            // 
            ShortcutUnprocessed.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ShortcutUnprocessed.BackColor = System.Drawing.Color.FromArgb(255, 255, 128);
            ShortcutUnprocessed.ForeColor = System.Drawing.SystemColors.ControlText;
            ShortcutUnprocessed.Location = new System.Drawing.Point(424, 246);
            ShortcutUnprocessed.Name = "ShortcutUnprocessed";
            ShortcutUnprocessed.Size = new System.Drawing.Size(126, 34);
            ShortcutUnprocessed.TabIndex = 13;
            ShortcutUnprocessed.Text = "Unprocessed > 2min";
            ShortcutUnprocessed.UseVisualStyleBackColor = false;
            // 
            // ShortcutReadingBusiness
            // 
            ShortcutReadingBusiness.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ShortcutReadingBusiness.BackColor = System.Drawing.Color.FromArgb(255, 255, 128);
            ShortcutReadingBusiness.ForeColor = System.Drawing.SystemColors.ControlText;
            ShortcutReadingBusiness.Location = new System.Drawing.Point(424, 206);
            ShortcutReadingBusiness.Name = "ShortcutReadingBusiness";
            ShortcutReadingBusiness.Size = new System.Drawing.Size(126, 34);
            ShortcutReadingBusiness.TabIndex = 12;
            ShortcutReadingBusiness.Text = "Reading - Business";
            ShortcutReadingBusiness.UseVisualStyleBackColor = false;
            // 
            // ShortcutCalls
            // 
            ShortcutCalls.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ShortcutCalls.BackColor = System.Drawing.Color.Blue;
            ShortcutCalls.ForeColor = System.Drawing.SystemColors.ButtonFace;
            ShortcutCalls.Location = new System.Drawing.Point(292, 206);
            ShortcutCalls.Name = "ShortcutCalls";
            ShortcutCalls.Size = new System.Drawing.Size(126, 34);
            ShortcutCalls.TabIndex = 11;
            ShortcutCalls.Text = "Calls";
            ShortcutCalls.UseVisualStyleBackColor = false;
            // 
            // ShortcutInternet
            // 
            ShortcutInternet.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ShortcutInternet.BackColor = System.Drawing.Color.FromArgb(255, 128, 0);
            ShortcutInternet.Location = new System.Drawing.Point(160, 206);
            ShortcutInternet.Name = "ShortcutInternet";
            ShortcutInternet.Size = new System.Drawing.Size(126, 34);
            ShortcutInternet.TabIndex = 10;
            ShortcutInternet.Text = "Internet";
            ShortcutInternet.UseVisualStyleBackColor = false;
            // 
            // ShortcutPreRead
            // 
            ShortcutPreRead.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ShortcutPreRead.BackColor = System.Drawing.Color.FromArgb(255, 255, 128);
            ShortcutPreRead.ForeColor = System.Drawing.SystemColors.ControlText;
            ShortcutPreRead.Location = new System.Drawing.Point(424, 166);
            ShortcutPreRead.Name = "ShortcutPreRead";
            ShortcutPreRead.Size = new System.Drawing.Size(126, 34);
            ShortcutPreRead.TabIndex = 9;
            ShortcutPreRead.Text = "PreRead";
            ShortcutPreRead.UseVisualStyleBackColor = false;
            // 
            // ShortcutMeeting
            // 
            ShortcutMeeting.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ShortcutMeeting.BackColor = System.Drawing.Color.Blue;
            ShortcutMeeting.ForeColor = System.Drawing.SystemColors.ButtonFace;
            ShortcutMeeting.Location = new System.Drawing.Point(292, 166);
            ShortcutMeeting.Name = "ShortcutMeeting";
            ShortcutMeeting.Size = new System.Drawing.Size(126, 34);
            ShortcutMeeting.TabIndex = 8;
            ShortcutMeeting.Text = "Meeting";
            ShortcutMeeting.UseVisualStyleBackColor = false;
            // 
            // LblTopic
            // 
            LblTopic.AutoSize = true;
            LblTopic.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            LblTopic.Location = new System.Drawing.Point(25, 134);
            LblTopic.Name = "LblTopic";
            LblTopic.Size = new System.Drawing.Size(73, 16);
            LblTopic.TabIndex = 7;
            LblTopic.Text = "Topic Tag:";
            // 
            // ShortcutPersonal
            // 
            ShortcutPersonal.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ShortcutPersonal.BackColor = System.Drawing.Color.FromArgb(255, 128, 0);
            ShortcutPersonal.Location = new System.Drawing.Point(160, 166);
            ShortcutPersonal.Name = "ShortcutPersonal";
            ShortcutPersonal.Size = new System.Drawing.Size(126, 34);
            ShortcutPersonal.TabIndex = 1;
            ShortcutPersonal.Text = "PERSONAL";
            ShortcutPersonal.UseVisualStyleBackColor = false;
            // 
            // LblProject
            // 
            LblProject.AutoSize = true;
            LblProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            LblProject.Location = new System.Drawing.Point(25, 98);
            LblProject.Name = "LblProject";
            LblProject.Size = new System.Drawing.Size(82, 16);
            LblProject.TabIndex = 6;
            LblProject.Text = "Project Flag:";
            // 
            // LblPeople
            // 
            LblPeople.AutoSize = true;
            LblPeople.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            LblPeople.Location = new System.Drawing.Point(25, 61);
            LblPeople.Name = "LblPeople";
            LblPeople.Size = new System.Drawing.Size(84, 16);
            LblPeople.TabIndex = 5;
            LblPeople.Text = "People Flag:";
            // 
            // LblContext
            // 
            LblContext.AutoSize = true;
            LblContext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            LblContext.Location = new System.Drawing.Point(25, 25);
            LblContext.Name = "LblContext";
            LblContext.Size = new System.Drawing.Size(84, 16);
            LblContext.TabIndex = 4;
            LblContext.Text = "Context Flag:";
            // 
            // TopicSelection
            // 
            TopicSelection.BackColor = System.Drawing.SystemColors.Window;
            TopicSelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            TopicSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            TopicSelection.Location = new System.Drawing.Point(160, 133);
            TopicSelection.Name = "TopicSelection";
            TopicSelection.Size = new System.Drawing.Size(390, 24);
            TopicSelection.TabIndex = 3;
            TopicSelection.Text = "[Other Topics Flagged]";
            // 
            // ProjectSelection
            // 
            ProjectSelection.BackColor = System.Drawing.SystemColors.Window;
            ProjectSelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            ProjectSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            ProjectSelection.Location = new System.Drawing.Point(160, 97);
            ProjectSelection.Name = "ProjectSelection";
            ProjectSelection.Size = new System.Drawing.Size(390, 24);
            ProjectSelection.TabIndex = 2;
            ProjectSelection.Text = "[Projects Flagged]";
            // 
            // PeopleSelection
            // 
            PeopleSelection.BackColor = System.Drawing.SystemColors.Window;
            PeopleSelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            PeopleSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            PeopleSelection.Location = new System.Drawing.Point(160, 60);
            PeopleSelection.Name = "PeopleSelection";
            PeopleSelection.Size = new System.Drawing.Size(390, 24);
            PeopleSelection.TabIndex = 1;
            PeopleSelection.Text = "[Assigned People Flagged]";
            // 
            // CategorySelection
            // 
            CategorySelection.BackColor = System.Drawing.SystemColors.Window;
            CategorySelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            CategorySelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            CategorySelection.Location = new System.Drawing.Point(160, 24);
            CategorySelection.Name = "CategorySelection";
            CategorySelection.Size = new System.Drawing.Size(390, 24);
            CategorySelection.TabIndex = 0;
            CategorySelection.Text = "[Category Label]";
            // 
            // OKButton
            // 
            OKButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            OKButton.Location = new System.Drawing.Point(138, 530);
            OKButton.Name = "OKButton";
            OKButton.Size = new System.Drawing.Size(145, 57);
            OKButton.TabIndex = 1;
            OKButton.Text = "OK";
            OKButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            Cancel_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            Cancel_Button.Location = new System.Drawing.Point(297, 530);
            Cancel_Button.Name = "Cancel_Button";
            Cancel_Button.Size = new System.Drawing.Size(145, 57);
            Cancel_Button.TabIndex = 2;
            Cancel_Button.Text = "Cancel";
            Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // LblTaskname
            // 
            LblTaskname.AutoSize = true;
            LblTaskname.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            LblTaskname.Location = new System.Drawing.Point(12, 23);
            LblTaskname.Name = "LblTaskname";
            LblTaskname.Size = new System.Drawing.Size(97, 16);
            LblTaskname.TabIndex = 1;
            LblTaskname.Text = "Name Of Task:";
            // 
            // TaskName
            // 
            TaskName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            TaskName.ForeColor = System.Drawing.SystemColors.WindowText;
            TaskName.Location = new System.Drawing.Point(12, 42);
            TaskName.Name = "TaskName";
            TaskName.Size = new System.Drawing.Size(560, 22);
            TaskName.TabIndex = 2;
            // 
            // LblPriority
            // 
            LblPriority.AutoSize = true;
            LblPriority.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            LblPriority.Location = new System.Drawing.Point(12, 83);
            LblPriority.Name = "LblPriority";
            LblPriority.Size = new System.Drawing.Size(77, 16);
            LblPriority.TabIndex = 3;
            LblPriority.Text = "Importance:";
            // 
            // LblKbf
            // 
            LblKbf.AutoSize = true;
            LblKbf.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            LblKbf.Location = new System.Drawing.Point(12, 111);
            LblKbf.Name = "LblKbf";
            LblKbf.Size = new System.Drawing.Size(56, 16);
            LblKbf.TabIndex = 5;
            LblKbf.Text = "Kanban:";
            // 
            // LblDuration
            // 
            LblDuration.AutoSize = true;
            LblDuration.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            LblDuration.Location = new System.Drawing.Point(12, 144);
            LblDuration.Name = "LblDuration";
            LblDuration.Size = new System.Drawing.Size(76, 16);
            LblDuration.TabIndex = 7;
            LblDuration.Text = "Work Time:";
            // 
            // PriorityBox
            // 
            PriorityBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            PriorityBox.FormattingEnabled = true;
            PriorityBox.Items.AddRange(new object[] { "High", "Normal", "Low" });
            PriorityBox.Location = new System.Drawing.Point(120, 82);
            PriorityBox.Name = "PriorityBox";
            PriorityBox.Size = new System.Drawing.Size(121, 21);
            PriorityBox.TabIndex = 4;
            // 
            // KbSelector
            // 
            KbSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            KbSelector.FormattingEnabled = true;
            KbSelector.Items.AddRange(new object[] { "Backlog", "Planned", "InProgress", "Complete" });
            KbSelector.Location = new System.Drawing.Point(120, 111);
            KbSelector.Name = "KbSelector";
            KbSelector.Size = new System.Drawing.Size(121, 21);
            KbSelector.TabIndex = 6;
            // 
            // Duration
            // 
            Duration.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            Duration.ForeColor = System.Drawing.SystemColors.WindowText;
            Duration.Location = new System.Drawing.Point(120, 141);
            Duration.Name = "Duration";
            Duration.Size = new System.Drawing.Size(121, 22);
            Duration.TabIndex = 8;
            // 
            // DtDuedate
            // 
            DtDuedate.Checked = false;
            DtDuedate.CustomFormat = "MM/dd/yyyy hh:mm tt";
            DtDuedate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            DtDuedate.Location = new System.Drawing.Point(388, 83);
            DtDuedate.Name = "DtDuedate";
            DtDuedate.ShowCheckBox = true;
            DtDuedate.Size = new System.Drawing.Size(184, 20);
            DtDuedate.TabIndex = 10;
            // 
            // LblDuedate
            // 
            LblDuedate.AutoSize = true;
            LblDuedate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            LblDuedate.Location = new System.Drawing.Point(310, 83);
            LblDuedate.Name = "LblDuedate";
            LblDuedate.Size = new System.Drawing.Size(67, 16);
            LblDuedate.TabIndex = 9;
            LblDuedate.Text = "Due Date:";
            // 
            // LblReminder
            // 
            LblReminder.AutoSize = true;
            LblReminder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            LblReminder.Location = new System.Drawing.Point(310, 114);
            LblReminder.Name = "LblReminder";
            LblReminder.Size = new System.Drawing.Size(69, 16);
            LblReminder.TabIndex = 11;
            LblReminder.Text = "Reminder:";
            // 
            // DtReminder
            // 
            DtReminder.Checked = false;
            DtReminder.CustomFormat = "MM/dd/yyyy hh:mm tt";
            DtReminder.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            DtReminder.Location = new System.Drawing.Point(388, 112);
            DtReminder.Name = "DtReminder";
            DtReminder.ShowCheckBox = true;
            DtReminder.Size = new System.Drawing.Size(184, 20);
            DtReminder.TabIndex = 12;
            // 
            // XlTaskname
            // 
            XlTaskname.AutoSize = true;
            XlTaskname.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlTaskname.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlTaskname.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlTaskname.Location = new System.Drawing.Point(7, 23);
            XlTaskname.Name = "XlTaskname";
            XlTaskname.Size = new System.Drawing.Size(18, 16);
            XlTaskname.TabIndex = 13;
            XlTaskname.Text = "N";
            // 
            // XlImportance
            // 
            XlImportance.AutoSize = true;
            XlImportance.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlImportance.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlImportance.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlImportance.Location = new System.Drawing.Point(6, 82);
            XlImportance.Name = "XlImportance";
            XlImportance.Size = new System.Drawing.Size(11, 16);
            XlImportance.TabIndex = 14;
            XlImportance.Text = "I";
            // 
            // XlKanban
            // 
            XlKanban.AutoSize = true;
            XlKanban.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlKanban.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlKanban.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlKanban.Location = new System.Drawing.Point(6, 111);
            XlKanban.Name = "XlKanban";
            XlKanban.Size = new System.Drawing.Size(16, 16);
            XlKanban.TabIndex = 15;
            XlKanban.Text = "K";
            // 
            // XlWorktime
            // 
            XlWorktime.AutoSize = true;
            XlWorktime.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlWorktime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlWorktime.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlWorktime.Location = new System.Drawing.Point(6, 143);
            XlWorktime.Name = "XlWorktime";
            XlWorktime.Size = new System.Drawing.Size(21, 16);
            XlWorktime.TabIndex = 16;
            XlWorktime.Text = "W";
            // 
            // XlOk
            // 
            XlOk.AutoSize = true;
            XlOk.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlOk.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlOk.Location = new System.Drawing.Point(191, 550);
            XlOk.Name = "XlOk";
            XlOk.Size = new System.Drawing.Size(18, 16);
            XlOk.TabIndex = 23;
            XlOk.Text = "O";
            // 
            // XlCancel
            // 
            XlCancel.AutoSize = true;
            XlCancel.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlCancel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlCancel.Location = new System.Drawing.Point(341, 550);
            XlCancel.Name = "XlCancel";
            XlCancel.Size = new System.Drawing.Size(17, 16);
            XlCancel.TabIndex = 24;
            XlCancel.Text = "C";
            // 
            // XlReminder
            // 
            XlReminder.AutoSize = true;
            XlReminder.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlReminder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlReminder.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlReminder.Location = new System.Drawing.Point(305, 114);
            XlReminder.Name = "XlReminder";
            XlReminder.Size = new System.Drawing.Size(18, 16);
            XlReminder.TabIndex = 25;
            XlReminder.Text = "R";
            // 
            // XlDuedate
            // 
            XlDuedate.AutoSize = true;
            XlDuedate.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            XlDuedate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            XlDuedate.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            XlDuedate.Location = new System.Drawing.Point(305, 82);
            XlDuedate.Name = "XlDuedate";
            XlDuedate.Size = new System.Drawing.Size(18, 16);
            XlDuedate.TabIndex = 26;
            XlDuedate.Text = "D";
            // 
            // TaskViewer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6.0f, 13.0f);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(584, 611);
            Controls.Add(XlDuedate);
            Controls.Add(XlReminder);
            Controls.Add(XlCancel);
            Controls.Add(XlOk);
            Controls.Add(XlWorktime);
            Controls.Add(XlKanban);
            Controls.Add(XlImportance);
            Controls.Add(XlTaskname);
            Controls.Add(DtReminder);
            Controls.Add(LblReminder);
            Controls.Add(LblDuedate);
            Controls.Add(DtDuedate);
            Controls.Add(Duration);
            Controls.Add(KbSelector);
            Controls.Add(PriorityBox);
            Controls.Add(LblDuration);
            Controls.Add(LblKbf);
            Controls.Add(LblPriority);
            Controls.Add(TaskName);
            Controls.Add(LblTaskname);
            Controls.Add(Cancel_Button);
            Controls.Add(OKButton);
            Controls.Add(Frame1);
            Name = "TaskViewer";
            Text = "Change Flagged Email Into Task";
            Frame1.ResumeLayout(false);
            Frame1.PerformLayout();
            KeyDown += new System.Windows.Forms.KeyEventHandler(TaskViewer_KeyDown);
            ResumeLayout(false);
            PerformLayout();

        }

        internal System.Windows.Forms.Panel Frame1;
        internal System.Windows.Forms.Label LblTopic;
        internal System.Windows.Forms.Label LblProject;
        internal System.Windows.Forms.Label LblPeople;
        internal System.Windows.Forms.Label LblContext;
        internal System.Windows.Forms.Label TopicSelection;
        internal System.Windows.Forms.Label ProjectSelection;
        internal System.Windows.Forms.Label PeopleSelection;
        internal System.Windows.Forms.Label CategorySelection;
        internal System.Windows.Forms.Button OKButton;
        internal System.Windows.Forms.Button Cancel_Button;
        internal System.Windows.Forms.Button ShortcutNews;
        internal System.Windows.Forms.Button ShortcutUnprocessed;
        internal System.Windows.Forms.Button ShortcutReadingBusiness;
        internal System.Windows.Forms.Button ShortcutCalls;
        internal System.Windows.Forms.Button ShortcutInternet;
        internal System.Windows.Forms.Button ShortcutPreRead;
        internal System.Windows.Forms.Button ShortcutMeeting;
        internal System.Windows.Forms.Button ShortcutPersonal;
        internal System.Windows.Forms.Button ShortcutEmail;
        internal System.Windows.Forms.Button ShortcutWaitingFor;
        internal System.Windows.Forms.CheckBox CbxBullpin;
        internal System.Windows.Forms.CheckBox CbxToday;
        internal System.Windows.Forms.CheckBox CbxFlagAsTask;
        internal System.Windows.Forms.Label LblTaskname;
        internal System.Windows.Forms.TextBox TaskName;
        internal System.Windows.Forms.Label LblPriority;
        internal System.Windows.Forms.Label LblKbf;
        internal System.Windows.Forms.Label LblDuration;
        internal System.Windows.Forms.ComboBox PriorityBox;
        internal System.Windows.Forms.ComboBox KbSelector;
        internal System.Windows.Forms.TextBox Duration;
        internal System.Windows.Forms.DateTimePicker DtDuedate;
        internal System.Windows.Forms.Label LblDuedate;
        internal System.Windows.Forms.Label LblReminder;
        internal System.Windows.Forms.DateTimePicker DtReminder;
        internal System.Windows.Forms.Label XlTopic;
        internal System.Windows.Forms.Label XlProject;
        internal System.Windows.Forms.Label XlPeople;
        internal System.Windows.Forms.Label XlContext;
        internal System.Windows.Forms.Label XlTaskname;
        internal System.Windows.Forms.Label XlImportance;
        internal System.Windows.Forms.Label XlKanban;
        internal System.Windows.Forms.Label XlWorktime;
        internal System.Windows.Forms.Label XlOk;
        internal System.Windows.Forms.Label XlCancel;
        internal System.Windows.Forms.Label XlReminder;
        internal System.Windows.Forms.Label XlDuedate;
        internal System.Windows.Forms.Label XlScBullpin;
        internal System.Windows.Forms.Label XlScToday;
        internal System.Windows.Forms.Label XlScWaiting;
        internal System.Windows.Forms.Label XlScUnprocessed;
        internal System.Windows.Forms.Label XlScNews;
        internal System.Windows.Forms.Label XlScEmail;
        internal System.Windows.Forms.Label XlScReadingbusiness;
        internal System.Windows.Forms.Label XlScCalls;
        internal System.Windows.Forms.Label XlScInternet;
        internal System.Windows.Forms.Label XlScPreread;
        internal System.Windows.Forms.Label XlScMeeting;
        internal System.Windows.Forms.Label XlScPersonal;
    }
}