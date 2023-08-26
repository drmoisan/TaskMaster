using System;
using System.Diagnostics;

namespace TaskVisualization
{
    [Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
    public partial class TaskViewer3 : System.Windows.Forms.Form
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
            this.Frame1 = new System.Windows.Forms.Panel();
            this.XlScBullpin = new System.Windows.Forms.Label();
            this.XlScToday = new System.Windows.Forms.Label();
            this.XlScWaiting = new System.Windows.Forms.Label();
            this.XlScUnprocessed = new System.Windows.Forms.Label();
            this.XlScNews = new System.Windows.Forms.Label();
            this.XlScEmail = new System.Windows.Forms.Label();
            this.XlScReadingbusiness = new System.Windows.Forms.Label();
            this.XlScCalls = new System.Windows.Forms.Label();
            this.XlScInternet = new System.Windows.Forms.Label();
            this.XlScPreread = new System.Windows.Forms.Label();
            this.XlScMeeting = new System.Windows.Forms.Label();
            this.XlScPersonal = new System.Windows.Forms.Label();
            this.XlTopic = new System.Windows.Forms.Label();
            this.XlProject = new System.Windows.Forms.Label();
            this.XlPeople = new System.Windows.Forms.Label();
            this.XlContext = new System.Windows.Forms.Label();
            this.ShortcutWaitingFor = new System.Windows.Forms.Button();
            this.CbxBullpin = new System.Windows.Forms.CheckBox();
            this.CbxToday = new System.Windows.Forms.CheckBox();
            this.CbxFlagAsTask = new System.Windows.Forms.CheckBox();
            this.ShortcutEmail = new System.Windows.Forms.Button();
            this.ShortcutNews = new System.Windows.Forms.Button();
            this.ShortcutUnprocessed = new System.Windows.Forms.Button();
            this.ShortcutReadingBusiness = new System.Windows.Forms.Button();
            this.ShortcutCalls = new System.Windows.Forms.Button();
            this.ShortcutInternet = new System.Windows.Forms.Button();
            this.ShortcutPreRead = new System.Windows.Forms.Button();
            this.ShortcutMeeting = new System.Windows.Forms.Button();
            this.LblTopic = new System.Windows.Forms.Label();
            this.ShortcutPersonal = new System.Windows.Forms.Button();
            this.LblProject = new System.Windows.Forms.Label();
            this.LblPeople = new System.Windows.Forms.Label();
            this.LblContext = new System.Windows.Forms.Label();
            this.TopicSelection = new System.Windows.Forms.Label();
            this.ProjectSelection = new System.Windows.Forms.Label();
            this.PeopleSelection = new System.Windows.Forms.Label();
            this.CategorySelection = new System.Windows.Forms.Label();
            this.OKButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.LblTaskname = new System.Windows.Forms.Label();
            this.TaskName = new System.Windows.Forms.TextBox();
            this.LblPriority = new System.Windows.Forms.Label();
            this.LblKbf = new System.Windows.Forms.Label();
            this.LblDuration = new System.Windows.Forms.Label();
            this.PriorityBox = new System.Windows.Forms.ComboBox();
            this.KbSelector = new System.Windows.Forms.ComboBox();
            this.Duration = new System.Windows.Forms.TextBox();
            this.DtDuedate = new System.Windows.Forms.DateTimePicker();
            this.LblDuedate = new System.Windows.Forms.Label();
            this.LblReminder = new System.Windows.Forms.Label();
            this.DtReminder = new System.Windows.Forms.DateTimePicker();
            this.XlTaskname = new System.Windows.Forms.Label();
            this.XlImportance = new System.Windows.Forms.Label();
            this.XlKanban = new System.Windows.Forms.Label();
            this.XlWorktime = new System.Windows.Forms.Label();
            this.XlOk = new System.Windows.Forms.Label();
            this.XlCancel = new System.Windows.Forms.Label();
            this.XlReminder = new System.Windows.Forms.Label();
            this.XlDuedate = new System.Windows.Forms.Label();
            this.Frame1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Frame1
            // 
            this.Frame1.Controls.Add(this.XlScBullpin);
            this.Frame1.Controls.Add(this.XlScToday);
            this.Frame1.Controls.Add(this.XlScWaiting);
            this.Frame1.Controls.Add(this.XlScUnprocessed);
            this.Frame1.Controls.Add(this.XlScNews);
            this.Frame1.Controls.Add(this.XlScEmail);
            this.Frame1.Controls.Add(this.XlScReadingbusiness);
            this.Frame1.Controls.Add(this.XlScCalls);
            this.Frame1.Controls.Add(this.XlScInternet);
            this.Frame1.Controls.Add(this.XlScPreread);
            this.Frame1.Controls.Add(this.XlScMeeting);
            this.Frame1.Controls.Add(this.XlScPersonal);
            this.Frame1.Controls.Add(this.XlTopic);
            this.Frame1.Controls.Add(this.XlProject);
            this.Frame1.Controls.Add(this.XlPeople);
            this.Frame1.Controls.Add(this.XlContext);
            this.Frame1.Controls.Add(this.ShortcutWaitingFor);
            this.Frame1.Controls.Add(this.CbxBullpin);
            this.Frame1.Controls.Add(this.CbxToday);
            this.Frame1.Controls.Add(this.CbxFlagAsTask);
            this.Frame1.Controls.Add(this.ShortcutEmail);
            this.Frame1.Controls.Add(this.ShortcutNews);
            this.Frame1.Controls.Add(this.ShortcutUnprocessed);
            this.Frame1.Controls.Add(this.ShortcutReadingBusiness);
            this.Frame1.Controls.Add(this.ShortcutCalls);
            this.Frame1.Controls.Add(this.ShortcutInternet);
            this.Frame1.Controls.Add(this.ShortcutPreRead);
            this.Frame1.Controls.Add(this.ShortcutMeeting);
            this.Frame1.Controls.Add(this.LblTopic);
            this.Frame1.Controls.Add(this.ShortcutPersonal);
            this.Frame1.Controls.Add(this.LblProject);
            this.Frame1.Controls.Add(this.LblPeople);
            this.Frame1.Controls.Add(this.LblContext);
            this.Frame1.Controls.Add(this.TopicSelection);
            this.Frame1.Controls.Add(this.ProjectSelection);
            this.Frame1.Controls.Add(this.PeopleSelection);
            this.Frame1.Controls.Add(this.CategorySelection);
            this.Frame1.Location = new System.Drawing.Point(14, 358);
            this.Frame1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Frame1.Name = "Frame1";
            this.Frame1.Size = new System.Drawing.Size(1140, 619);
            this.Frame1.TabIndex = 0;
            // 
            // XlScBullpin
            // 
            this.XlScBullpin.AutoSize = true;
            this.XlScBullpin.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlScBullpin.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlScBullpin.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlScBullpin.Location = new System.Drawing.Point(876, 527);
            this.XlScBullpin.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlScBullpin.Name = "XlScBullpin";
            this.XlScBullpin.Size = new System.Drawing.Size(31, 30);
            this.XlScBullpin.TabIndex = 38;
            this.XlScBullpin.Text = "B";
            // 
            // XlScToday
            // 
            this.XlScToday.AutoSize = true;
            this.XlScToday.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlScToday.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlScToday.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlScToday.Location = new System.Drawing.Point(704, 527);
            this.XlScToday.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlScToday.Name = "XlScToday";
            this.XlScToday.Size = new System.Drawing.Size(30, 30);
            this.XlScToday.TabIndex = 37;
            this.XlScToday.Text = "T";
            // 
            // XlScWaiting
            // 
            this.XlScWaiting.AutoSize = true;
            this.XlScWaiting.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlScWaiting.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlScWaiting.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlScWaiting.Location = new System.Drawing.Point(40, 490);
            this.XlScWaiting.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlScWaiting.Name = "XlScWaiting";
            this.XlScWaiting.Size = new System.Drawing.Size(39, 30);
            this.XlScWaiting.TabIndex = 36;
            this.XlScWaiting.Text = "W";
            // 
            // XlScUnprocessed
            // 
            this.XlScUnprocessed.AutoSize = true;
            this.XlScUnprocessed.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlScUnprocessed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlScUnprocessed.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlScUnprocessed.Location = new System.Drawing.Point(838, 490);
            this.XlScUnprocessed.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlScUnprocessed.Name = "XlScUnprocessed";
            this.XlScUnprocessed.Size = new System.Drawing.Size(33, 30);
            this.XlScUnprocessed.TabIndex = 35;
            this.XlScUnprocessed.Text = "U";
            // 
            // XlScNews
            // 
            this.XlScNews.AutoSize = true;
            this.XlScNews.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlScNews.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlScNews.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlScNews.Location = new System.Drawing.Point(568, 490);
            this.XlScNews.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlScNews.Name = "XlScNews";
            this.XlScNews.Size = new System.Drawing.Size(33, 30);
            this.XlScNews.TabIndex = 34;
            this.XlScNews.Text = "N";
            // 
            // XlScEmail
            // 
            this.XlScEmail.AutoSize = true;
            this.XlScEmail.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlScEmail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlScEmail.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlScEmail.Location = new System.Drawing.Point(332, 490);
            this.XlScEmail.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlScEmail.Name = "XlScEmail";
            this.XlScEmail.Size = new System.Drawing.Size(31, 30);
            this.XlScEmail.TabIndex = 33;
            this.XlScEmail.Text = "E";
            // 
            // XlScReadingbusiness
            // 
            this.XlScReadingbusiness.AutoSize = true;
            this.XlScReadingbusiness.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlScReadingbusiness.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlScReadingbusiness.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlScReadingbusiness.Location = new System.Drawing.Point(860, 415);
            this.XlScReadingbusiness.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlScReadingbusiness.Name = "XlScReadingbusiness";
            this.XlScReadingbusiness.Size = new System.Drawing.Size(33, 30);
            this.XlScReadingbusiness.TabIndex = 32;
            this.XlScReadingbusiness.Text = "R";
            // 
            // XlScCalls
            // 
            this.XlScCalls.AutoSize = true;
            this.XlScCalls.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlScCalls.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlScCalls.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlScCalls.Location = new System.Drawing.Point(602, 415);
            this.XlScCalls.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlScCalls.Name = "XlScCalls";
            this.XlScCalls.Size = new System.Drawing.Size(33, 30);
            this.XlScCalls.TabIndex = 31;
            this.XlScCalls.Text = "C";
            // 
            // XlScInternet
            // 
            this.XlScInternet.AutoSize = true;
            this.XlScInternet.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlScInternet.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlScInternet.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlScInternet.Location = new System.Drawing.Point(332, 413);
            this.XlScInternet.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlScInternet.Name = "XlScInternet";
            this.XlScInternet.Size = new System.Drawing.Size(20, 30);
            this.XlScInternet.TabIndex = 30;
            this.XlScInternet.Text = "I";
            // 
            // XlScPreread
            // 
            this.XlScPreread.AutoSize = true;
            this.XlScPreread.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlScPreread.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlScPreread.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlScPreread.Location = new System.Drawing.Point(860, 337);
            this.XlScPreread.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlScPreread.Name = "XlScPreread";
            this.XlScPreread.Size = new System.Drawing.Size(31, 30);
            this.XlScPreread.TabIndex = 29;
            this.XlScPreread.Text = "P";
            // 
            // XlScMeeting
            // 
            this.XlScMeeting.AutoSize = true;
            this.XlScMeeting.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlScMeeting.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlScMeeting.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlScMeeting.Location = new System.Drawing.Point(602, 337);
            this.XlScMeeting.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlScMeeting.Name = "XlScMeeting";
            this.XlScMeeting.Size = new System.Drawing.Size(36, 30);
            this.XlScMeeting.TabIndex = 28;
            this.XlScMeeting.Text = "M";
            // 
            // XlScPersonal
            // 
            this.XlScPersonal.AutoSize = true;
            this.XlScPersonal.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlScPersonal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlScPersonal.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlScPersonal.Location = new System.Drawing.Point(332, 335);
            this.XlScPersonal.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlScPersonal.Name = "XlScPersonal";
            this.XlScPersonal.Size = new System.Drawing.Size(31, 30);
            this.XlScPersonal.TabIndex = 27;
            this.XlScPersonal.Text = "P";
            // 
            // XlTopic
            // 
            this.XlTopic.AutoSize = true;
            this.XlTopic.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlTopic.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlTopic.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlTopic.Location = new System.Drawing.Point(12, 258);
            this.XlTopic.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlTopic.Name = "XlTopic";
            this.XlTopic.Size = new System.Drawing.Size(30, 30);
            this.XlTopic.TabIndex = 22;
            this.XlTopic.Text = "T";
            // 
            // XlProject
            // 
            this.XlProject.AutoSize = true;
            this.XlProject.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlProject.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlProject.Location = new System.Drawing.Point(12, 188);
            this.XlProject.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlProject.Name = "XlProject";
            this.XlProject.Size = new System.Drawing.Size(31, 30);
            this.XlProject.TabIndex = 21;
            this.XlProject.Text = "P";
            // 
            // XlPeople
            // 
            this.XlPeople.AutoSize = true;
            this.XlPeople.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlPeople.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlPeople.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlPeople.Location = new System.Drawing.Point(12, 115);
            this.XlPeople.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlPeople.Name = "XlPeople";
            this.XlPeople.Size = new System.Drawing.Size(31, 30);
            this.XlPeople.TabIndex = 20;
            this.XlPeople.Text = "P";
            // 
            // XlContext
            // 
            this.XlContext.AutoSize = true;
            this.XlContext.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlContext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlContext.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlContext.Location = new System.Drawing.Point(12, 48);
            this.XlContext.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlContext.Name = "XlContext";
            this.XlContext.Size = new System.Drawing.Size(33, 30);
            this.XlContext.TabIndex = 17;
            this.XlContext.Text = "C";
            // 
            // ShortcutWaitingFor
            // 
            this.ShortcutWaitingFor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShortcutWaitingFor.BackColor = System.Drawing.Color.DarkMagenta;
            this.ShortcutWaitingFor.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.ShortcutWaitingFor.Location = new System.Drawing.Point(26, 473);
            this.ShortcutWaitingFor.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ShortcutWaitingFor.Name = "ShortcutWaitingFor";
            this.ShortcutWaitingFor.Size = new System.Drawing.Size(252, 65);
            this.ShortcutWaitingFor.TabIndex = 19;
            this.ShortcutWaitingFor.Text = "Waiting For";
            this.ShortcutWaitingFor.UseVisualStyleBackColor = false;
            this.ShortcutWaitingFor.Click += new System.EventHandler(this.ShortcutWaitingFor_Click);
            // 
            // CbxBullpin
            // 
            this.CbxBullpin.AutoSize = true;
            this.CbxBullpin.Location = new System.Drawing.Point(848, 558);
            this.CbxBullpin.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.CbxBullpin.Name = "CbxBullpin";
            this.CbxBullpin.Size = new System.Drawing.Size(221, 29);
            this.CbxBullpin.TabIndex = 18;
            this.CbxBullpin.Text = "BULLPIN Priorities";
            this.CbxBullpin.UseVisualStyleBackColor = true;
            this.CbxBullpin.CheckedChanged += new System.EventHandler(this.CbxBullpin_CheckedChanged);
            // 
            // CbxToday
            // 
            this.CbxToday.AutoSize = true;
            this.CbxToday.Location = new System.Drawing.Point(584, 558);
            this.CbxToday.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.CbxToday.Name = "CbxToday";
            this.CbxToday.Size = new System.Drawing.Size(214, 29);
            this.CbxToday.TabIndex = 17;
            this.CbxToday.Text = "Complete TODAY";
            this.CbxToday.UseVisualStyleBackColor = true;
            this.CbxToday.CheckedChanged += new System.EventHandler(this.CbxToday_CheckedChanged);
            // 
            // CbxFlagAsTask
            // 
            this.CbxFlagAsTask.AutoSize = true;
            this.CbxFlagAsTask.Checked = true;
            this.CbxFlagAsTask.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CbxFlagAsTask.Location = new System.Drawing.Point(320, 558);
            this.CbxFlagAsTask.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.CbxFlagAsTask.Name = "CbxFlagAsTask";
            this.CbxFlagAsTask.Size = new System.Drawing.Size(225, 29);
            this.CbxFlagAsTask.TabIndex = 16;
            this.CbxFlagAsTask.Text = "Flag For Follow Up";
            this.CbxFlagAsTask.UseVisualStyleBackColor = true;
            this.CbxFlagAsTask.CheckedChanged += new System.EventHandler(this.CbxFlag_CheckedChanged);
            // 
            // ShortcutEmail
            // 
            this.ShortcutEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShortcutEmail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.ShortcutEmail.Location = new System.Drawing.Point(320, 473);
            this.ShortcutEmail.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ShortcutEmail.Name = "ShortcutEmail";
            this.ShortcutEmail.Size = new System.Drawing.Size(252, 65);
            this.ShortcutEmail.TabIndex = 15;
            this.ShortcutEmail.Text = "Email";
            this.ShortcutEmail.UseVisualStyleBackColor = false;
            this.ShortcutEmail.Click += new System.EventHandler(this.ShortcutEmail_Click);
            // 
            // ShortcutNews
            // 
            this.ShortcutNews.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShortcutNews.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.ShortcutNews.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ShortcutNews.Location = new System.Drawing.Point(584, 473);
            this.ShortcutNews.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ShortcutNews.Name = "ShortcutNews";
            this.ShortcutNews.Size = new System.Drawing.Size(252, 65);
            this.ShortcutNews.TabIndex = 14;
            this.ShortcutNews.Text = "News | Articles | Other";
            this.ShortcutNews.UseVisualStyleBackColor = false;
            this.ShortcutNews.Click += new System.EventHandler(this.ShortcutReadingNews_Click);
            // 
            // ShortcutUnprocessed
            // 
            this.ShortcutUnprocessed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShortcutUnprocessed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.ShortcutUnprocessed.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ShortcutUnprocessed.Location = new System.Drawing.Point(848, 473);
            this.ShortcutUnprocessed.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ShortcutUnprocessed.Name = "ShortcutUnprocessed";
            this.ShortcutUnprocessed.Size = new System.Drawing.Size(252, 65);
            this.ShortcutUnprocessed.TabIndex = 13;
            this.ShortcutUnprocessed.Text = "Unprocessed > 2min";
            this.ShortcutUnprocessed.UseVisualStyleBackColor = false;
            this.ShortcutUnprocessed.Click += new System.EventHandler(this.ShortcutUnprocessed_Click);
            // 
            // ShortcutReadingBusiness
            // 
            this.ShortcutReadingBusiness.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShortcutReadingBusiness.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.ShortcutReadingBusiness.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ShortcutReadingBusiness.Location = new System.Drawing.Point(848, 396);
            this.ShortcutReadingBusiness.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ShortcutReadingBusiness.Name = "ShortcutReadingBusiness";
            this.ShortcutReadingBusiness.Size = new System.Drawing.Size(252, 65);
            this.ShortcutReadingBusiness.TabIndex = 12;
            this.ShortcutReadingBusiness.Text = "Reading - Business";
            this.ShortcutReadingBusiness.UseVisualStyleBackColor = false;
            this.ShortcutReadingBusiness.Click += new System.EventHandler(this.ShortcutReadingBusiness_Click);
            // 
            // ShortcutCalls
            // 
            this.ShortcutCalls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShortcutCalls.BackColor = System.Drawing.Color.Blue;
            this.ShortcutCalls.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.ShortcutCalls.Location = new System.Drawing.Point(584, 396);
            this.ShortcutCalls.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ShortcutCalls.Name = "ShortcutCalls";
            this.ShortcutCalls.Size = new System.Drawing.Size(252, 65);
            this.ShortcutCalls.TabIndex = 11;
            this.ShortcutCalls.Text = "Calls";
            this.ShortcutCalls.UseVisualStyleBackColor = false;
            this.ShortcutCalls.Click += new System.EventHandler(this.ShortcutCalls_Click);
            // 
            // ShortcutInternet
            // 
            this.ShortcutInternet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShortcutInternet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.ShortcutInternet.Location = new System.Drawing.Point(320, 396);
            this.ShortcutInternet.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ShortcutInternet.Name = "ShortcutInternet";
            this.ShortcutInternet.Size = new System.Drawing.Size(252, 65);
            this.ShortcutInternet.TabIndex = 10;
            this.ShortcutInternet.Text = "Internet";
            this.ShortcutInternet.UseVisualStyleBackColor = false;
            this.ShortcutInternet.Click += new System.EventHandler(this.ShortcutInternet_Click);
            // 
            // ShortcutPreRead
            // 
            this.ShortcutPreRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShortcutPreRead.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.ShortcutPreRead.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ShortcutPreRead.Location = new System.Drawing.Point(848, 319);
            this.ShortcutPreRead.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ShortcutPreRead.Name = "ShortcutPreRead";
            this.ShortcutPreRead.Size = new System.Drawing.Size(252, 65);
            this.ShortcutPreRead.TabIndex = 9;
            this.ShortcutPreRead.Text = "PreRead";
            this.ShortcutPreRead.UseVisualStyleBackColor = false;
            this.ShortcutPreRead.Click += new System.EventHandler(this.ShortcutPreRead_Click);
            // 
            // ShortcutMeeting
            // 
            this.ShortcutMeeting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShortcutMeeting.BackColor = System.Drawing.Color.Blue;
            this.ShortcutMeeting.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.ShortcutMeeting.Location = new System.Drawing.Point(584, 319);
            this.ShortcutMeeting.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ShortcutMeeting.Name = "ShortcutMeeting";
            this.ShortcutMeeting.Size = new System.Drawing.Size(252, 65);
            this.ShortcutMeeting.TabIndex = 8;
            this.ShortcutMeeting.Text = "Meeting";
            this.ShortcutMeeting.UseVisualStyleBackColor = false;
            this.ShortcutMeeting.Click += new System.EventHandler(this.ShortcutMeeting_Click);
            // 
            // LblTopic
            // 
            this.LblTopic.AutoSize = true;
            this.LblTopic.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblTopic.Location = new System.Drawing.Point(50, 258);
            this.LblTopic.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.LblTopic.Name = "LblTopic";
            this.LblTopic.Size = new System.Drawing.Size(134, 30);
            this.LblTopic.TabIndex = 7;
            this.LblTopic.Text = "Topic Tag:";
            // 
            // ShortcutPersonal
            // 
            this.ShortcutPersonal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShortcutPersonal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.ShortcutPersonal.Location = new System.Drawing.Point(320, 319);
            this.ShortcutPersonal.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ShortcutPersonal.Name = "ShortcutPersonal";
            this.ShortcutPersonal.Size = new System.Drawing.Size(252, 65);
            this.ShortcutPersonal.TabIndex = 1;
            this.ShortcutPersonal.Text = "PERSONAL";
            this.ShortcutPersonal.UseVisualStyleBackColor = false;
            this.ShortcutPersonal.Click += new System.EventHandler(this.ShortcutPersonal_Click);
            // 
            // LblProject
            // 
            this.LblProject.AutoSize = true;
            this.LblProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblProject.Location = new System.Drawing.Point(50, 188);
            this.LblProject.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.LblProject.Name = "LblProject";
            this.LblProject.Size = new System.Drawing.Size(157, 30);
            this.LblProject.TabIndex = 6;
            this.LblProject.Text = "Project Flag:";
            // 
            // LblPeople
            // 
            this.LblPeople.AutoSize = true;
            this.LblPeople.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblPeople.Location = new System.Drawing.Point(50, 117);
            this.LblPeople.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.LblPeople.Name = "LblPeople";
            this.LblPeople.Size = new System.Drawing.Size(156, 30);
            this.LblPeople.TabIndex = 5;
            this.LblPeople.Text = "People Flag:";
            // 
            // LblContext
            // 
            this.LblContext.AutoSize = true;
            this.LblContext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblContext.Location = new System.Drawing.Point(50, 48);
            this.LblContext.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.LblContext.Name = "LblContext";
            this.LblContext.Size = new System.Drawing.Size(164, 30);
            this.LblContext.TabIndex = 4;
            this.LblContext.Text = "Context Flag:";
            // 
            // TopicSelection
            // 
            this.TopicSelection.BackColor = System.Drawing.SystemColors.Window;
            this.TopicSelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TopicSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TopicSelection.Location = new System.Drawing.Point(320, 256);
            this.TopicSelection.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.TopicSelection.Name = "TopicSelection";
            this.TopicSelection.Size = new System.Drawing.Size(778, 44);
            this.TopicSelection.TabIndex = 3;
            this.TopicSelection.Text = "[Other Topics Flagged]";
            this.TopicSelection.Click += new System.EventHandler(this.TopicSelection_Click);
            // 
            // ProjectSelection
            // 
            this.ProjectSelection.BackColor = System.Drawing.SystemColors.Window;
            this.ProjectSelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ProjectSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProjectSelection.Location = new System.Drawing.Point(320, 187);
            this.ProjectSelection.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ProjectSelection.Name = "ProjectSelection";
            this.ProjectSelection.Size = new System.Drawing.Size(778, 44);
            this.ProjectSelection.TabIndex = 2;
            this.ProjectSelection.Text = "[Projects Flagged]";
            this.ProjectSelection.Click += new System.EventHandler(this.ProjectSelection_Click);
            // 
            // PeopleSelection
            // 
            this.PeopleSelection.BackColor = System.Drawing.SystemColors.Window;
            this.PeopleSelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PeopleSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PeopleSelection.Location = new System.Drawing.Point(320, 115);
            this.PeopleSelection.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.PeopleSelection.Name = "PeopleSelection";
            this.PeopleSelection.Size = new System.Drawing.Size(778, 44);
            this.PeopleSelection.TabIndex = 1;
            this.PeopleSelection.Text = "[Assigned People Flagged]";
            this.PeopleSelection.Click += new System.EventHandler(this.PeopleSelection_Click);
            // 
            // CategorySelection
            // 
            this.CategorySelection.BackColor = System.Drawing.SystemColors.Window;
            this.CategorySelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CategorySelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CategorySelection.Location = new System.Drawing.Point(320, 46);
            this.CategorySelection.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.CategorySelection.Name = "CategorySelection";
            this.CategorySelection.Size = new System.Drawing.Size(778, 44);
            this.CategorySelection.TabIndex = 0;
            this.CategorySelection.Text = "[Category Label]";
            this.CategorySelection.Click += new System.EventHandler(this.CategorySelection_Click);
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OKButton.Location = new System.Drawing.Point(276, 1019);
            this.OKButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(290, 110);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel_Button.Location = new System.Drawing.Point(591, 1019);
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(290, 110);
            this.Cancel_Button.TabIndex = 2;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
            // 
            // LblTaskname
            // 
            this.LblTaskname.AutoSize = true;
            this.LblTaskname.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblTaskname.Location = new System.Drawing.Point(24, 44);
            this.LblTaskname.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.LblTaskname.Name = "LblTaskname";
            this.LblTaskname.Size = new System.Drawing.Size(186, 30);
            this.LblTaskname.TabIndex = 1;
            this.LblTaskname.Text = "Name Of Task:";
            // 
            // TaskName
            // 
            this.TaskName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TaskName.ForeColor = System.Drawing.SystemColors.WindowText;
            this.TaskName.Location = new System.Drawing.Point(24, 81);
            this.TaskName.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.TaskName.Name = "TaskName";
            this.TaskName.Size = new System.Drawing.Size(1116, 37);
            this.TaskName.TabIndex = 2;
            this.TaskName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TaskName_KeyDown);
            this.TaskName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TaskName_KeyPress);
            this.TaskName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TaskName_KeyUp);
            // 
            // LblPriority
            // 
            this.LblPriority.AutoSize = true;
            this.LblPriority.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblPriority.Location = new System.Drawing.Point(24, 160);
            this.LblPriority.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.LblPriority.Name = "LblPriority";
            this.LblPriority.Size = new System.Drawing.Size(147, 30);
            this.LblPriority.TabIndex = 3;
            this.LblPriority.Text = "Importance:";
            // 
            // LblKbf
            // 
            this.LblKbf.AutoSize = true;
            this.LblKbf.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblKbf.Location = new System.Drawing.Point(24, 213);
            this.LblKbf.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.LblKbf.Name = "LblKbf";
            this.LblKbf.Size = new System.Drawing.Size(107, 30);
            this.LblKbf.TabIndex = 5;
            this.LblKbf.Text = "Kanban:";
            // 
            // LblDuration
            // 
            this.LblDuration.AutoSize = true;
            this.LblDuration.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblDuration.Location = new System.Drawing.Point(24, 277);
            this.LblDuration.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.LblDuration.Name = "LblDuration";
            this.LblDuration.Size = new System.Drawing.Size(146, 30);
            this.LblDuration.TabIndex = 7;
            this.LblDuration.Text = "Work Time:";
            // 
            // PriorityBox
            // 
            this.PriorityBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PriorityBox.FormattingEnabled = true;
            this.PriorityBox.Items.AddRange(new object[] {
            "High",
            "Normal",
            "Low"});
            this.PriorityBox.Location = new System.Drawing.Point(240, 158);
            this.PriorityBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.PriorityBox.Name = "PriorityBox";
            this.PriorityBox.Size = new System.Drawing.Size(238, 33);
            this.PriorityBox.TabIndex = 4;
            this.PriorityBox.SelectedIndexChanged += new System.EventHandler(this.PriorityBox_SelectedIndexChanged);
            // 
            // KbSelector
            // 
            this.KbSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.KbSelector.FormattingEnabled = true;
            this.KbSelector.Items.AddRange(new object[] {
            "Backlog",
            "Planned",
            "InProgress",
            "Complete"});
            this.KbSelector.Location = new System.Drawing.Point(240, 213);
            this.KbSelector.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.KbSelector.Name = "KbSelector";
            this.KbSelector.Size = new System.Drawing.Size(238, 33);
            this.KbSelector.TabIndex = 6;
            this.KbSelector.SelectedIndexChanged += new System.EventHandler(this.KbSelector_SelectedIndexChanged);
            // 
            // Duration
            // 
            this.Duration.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Duration.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Duration.Location = new System.Drawing.Point(240, 271);
            this.Duration.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Duration.Name = "Duration";
            this.Duration.Size = new System.Drawing.Size(238, 37);
            this.Duration.TabIndex = 8;
            // 
            // DtDuedate
            // 
            this.DtDuedate.Checked = false;
            this.DtDuedate.CustomFormat = "MM/dd/yyyy hh:mm tt";
            this.DtDuedate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.DtDuedate.Location = new System.Drawing.Point(776, 160);
            this.DtDuedate.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.DtDuedate.Name = "DtDuedate";
            this.DtDuedate.ShowCheckBox = true;
            this.DtDuedate.Size = new System.Drawing.Size(364, 31);
            this.DtDuedate.TabIndex = 10;
            // 
            // LblDuedate
            // 
            this.LblDuedate.AutoSize = true;
            this.LblDuedate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblDuedate.Location = new System.Drawing.Point(620, 160);
            this.LblDuedate.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.LblDuedate.Name = "LblDuedate";
            this.LblDuedate.Size = new System.Drawing.Size(128, 30);
            this.LblDuedate.TabIndex = 9;
            this.LblDuedate.Text = "Due Date:";
            // 
            // LblReminder
            // 
            this.LblReminder.AutoSize = true;
            this.LblReminder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblReminder.Location = new System.Drawing.Point(620, 219);
            this.LblReminder.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.LblReminder.Name = "LblReminder";
            this.LblReminder.Size = new System.Drawing.Size(132, 30);
            this.LblReminder.TabIndex = 11;
            this.LblReminder.Text = "Reminder:";
            // 
            // DtReminder
            // 
            this.DtReminder.Checked = false;
            this.DtReminder.CustomFormat = "MM/dd/yyyy hh:mm tt";
            this.DtReminder.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.DtReminder.Location = new System.Drawing.Point(776, 215);
            this.DtReminder.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.DtReminder.Name = "DtReminder";
            this.DtReminder.ShowCheckBox = true;
            this.DtReminder.Size = new System.Drawing.Size(364, 31);
            this.DtReminder.TabIndex = 12;
            // 
            // XlTaskname
            // 
            this.XlTaskname.AutoSize = true;
            this.XlTaskname.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlTaskname.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlTaskname.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlTaskname.Location = new System.Drawing.Point(14, 44);
            this.XlTaskname.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlTaskname.Name = "XlTaskname";
            this.XlTaskname.Size = new System.Drawing.Size(33, 30);
            this.XlTaskname.TabIndex = 13;
            this.XlTaskname.Text = "N";
            // 
            // XlImportance
            // 
            this.XlImportance.AutoSize = true;
            this.XlImportance.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlImportance.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlImportance.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlImportance.Location = new System.Drawing.Point(12, 158);
            this.XlImportance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlImportance.Name = "XlImportance";
            this.XlImportance.Size = new System.Drawing.Size(20, 30);
            this.XlImportance.TabIndex = 14;
            this.XlImportance.Text = "I";
            // 
            // XlKanban
            // 
            this.XlKanban.AutoSize = true;
            this.XlKanban.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlKanban.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlKanban.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlKanban.Location = new System.Drawing.Point(12, 213);
            this.XlKanban.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlKanban.Name = "XlKanban";
            this.XlKanban.Size = new System.Drawing.Size(31, 30);
            this.XlKanban.TabIndex = 15;
            this.XlKanban.Text = "K";
            // 
            // XlWorktime
            // 
            this.XlWorktime.AutoSize = true;
            this.XlWorktime.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlWorktime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlWorktime.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlWorktime.Location = new System.Drawing.Point(12, 275);
            this.XlWorktime.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlWorktime.Name = "XlWorktime";
            this.XlWorktime.Size = new System.Drawing.Size(39, 30);
            this.XlWorktime.TabIndex = 16;
            this.XlWorktime.Text = "W";
            // 
            // XlOk
            // 
            this.XlOk.AutoSize = true;
            this.XlOk.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlOk.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlOk.Location = new System.Drawing.Point(382, 1058);
            this.XlOk.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlOk.Name = "XlOk";
            this.XlOk.Size = new System.Drawing.Size(34, 30);
            this.XlOk.TabIndex = 23;
            this.XlOk.Text = "O";
            // 
            // XlCancel
            // 
            this.XlCancel.AutoSize = true;
            this.XlCancel.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlCancel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlCancel.Location = new System.Drawing.Point(682, 1058);
            this.XlCancel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlCancel.Name = "XlCancel";
            this.XlCancel.Size = new System.Drawing.Size(33, 30);
            this.XlCancel.TabIndex = 24;
            this.XlCancel.Text = "C";
            // 
            // XlReminder
            // 
            this.XlReminder.AutoSize = true;
            this.XlReminder.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlReminder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlReminder.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlReminder.Location = new System.Drawing.Point(610, 219);
            this.XlReminder.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlReminder.Name = "XlReminder";
            this.XlReminder.Size = new System.Drawing.Size(33, 30);
            this.XlReminder.TabIndex = 25;
            this.XlReminder.Text = "R";
            // 
            // XlDuedate
            // 
            this.XlDuedate.AutoSize = true;
            this.XlDuedate.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.XlDuedate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XlDuedate.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.XlDuedate.Location = new System.Drawing.Point(610, 158);
            this.XlDuedate.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.XlDuedate.Name = "XlDuedate";
            this.XlDuedate.Size = new System.Drawing.Size(33, 30);
            this.XlDuedate.TabIndex = 26;
            this.XlDuedate.Text = "D";
            // 
            // TaskViewer3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1165, 1175);
            this.Controls.Add(this.XlDuedate);
            this.Controls.Add(this.XlReminder);
            this.Controls.Add(this.XlCancel);
            this.Controls.Add(this.XlOk);
            this.Controls.Add(this.XlWorktime);
            this.Controls.Add(this.XlKanban);
            this.Controls.Add(this.XlImportance);
            this.Controls.Add(this.XlTaskname);
            this.Controls.Add(this.DtReminder);
            this.Controls.Add(this.LblReminder);
            this.Controls.Add(this.LblDuedate);
            this.Controls.Add(this.DtDuedate);
            this.Controls.Add(this.Duration);
            this.Controls.Add(this.KbSelector);
            this.Controls.Add(this.PriorityBox);
            this.Controls.Add(this.LblDuration);
            this.Controls.Add(this.LblKbf);
            this.Controls.Add(this.LblPriority);
            this.Controls.Add(this.TaskName);
            this.Controls.Add(this.LblTaskname);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.Frame1);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "TaskViewer3";
            this.Text = "Change Flagged Email Into Task";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TaskViewer_KeyDown);
            this.Frame1.ResumeLayout(false);
            this.Frame1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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