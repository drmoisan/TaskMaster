<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class TaskViewer
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Frame1 = New System.Windows.Forms.Panel()
        Me.xl_sc_bullpin = New System.Windows.Forms.Label()
        Me.xl_sc_today = New System.Windows.Forms.Label()
        Me.xl_sc_waiting = New System.Windows.Forms.Label()
        Me.xl_sc_unprocessed = New System.Windows.Forms.Label()
        Me.xl_sc_news = New System.Windows.Forms.Label()
        Me.xl_sc_email = New System.Windows.Forms.Label()
        Me.xl_sc_readingbusiness = New System.Windows.Forms.Label()
        Me.xl_sc_calls = New System.Windows.Forms.Label()
        Me.xl_sc_internet = New System.Windows.Forms.Label()
        Me.xl_sc_preread = New System.Windows.Forms.Label()
        Me.xl_sc_meeting = New System.Windows.Forms.Label()
        Me.xl_sc_personal = New System.Windows.Forms.Label()
        Me.xl_topic = New System.Windows.Forms.Label()
        Me.xl_project = New System.Windows.Forms.Label()
        Me.xl_people = New System.Windows.Forms.Label()
        Me.xl_context = New System.Windows.Forms.Label()
        Me.Cat_WaitingFor = New System.Windows.Forms.Button()
        Me.cbx_bullpin = New System.Windows.Forms.CheckBox()
        Me.cbx_today = New System.Windows.Forms.CheckBox()
        Me.cbxFlag = New System.Windows.Forms.CheckBox()
        Me.Cat_Email = New System.Windows.Forms.Button()
        Me.Cat_News = New System.Windows.Forms.Button()
        Me.Cat_Unprocessed = New System.Windows.Forms.Button()
        Me.Cat_ReadingBusiness = New System.Windows.Forms.Button()
        Me.Cat_Calls = New System.Windows.Forms.Button()
        Me.Cat_Internet = New System.Windows.Forms.Button()
        Me.Cat_PreRead = New System.Windows.Forms.Button()
        Me.Cat_Meeting = New System.Windows.Forms.Button()
        Me.lbl_topic = New System.Windows.Forms.Label()
        Me.Cat_Personal = New System.Windows.Forms.Button()
        Me.lbl_project = New System.Windows.Forms.Label()
        Me.lbl_people = New System.Windows.Forms.Label()
        Me.lbl_context = New System.Windows.Forms.Label()
        Me.topic_selection = New System.Windows.Forms.Label()
        Me.project_selection = New System.Windows.Forms.Label()
        Me.people_selection = New System.Windows.Forms.Label()
        Me.category_selection = New System.Windows.Forms.Label()
        Me.OK_Button = New System.Windows.Forms.Button()
        Me.Cancel_Button = New System.Windows.Forms.Button()
        Me.lbl_taskname = New System.Windows.Forms.Label()
        Me.task_name = New System.Windows.Forms.TextBox()
        Me.lbl_priority = New System.Windows.Forms.Label()
        Me.lbl_kbf = New System.Windows.Forms.Label()
        Me.lbl_duration = New System.Windows.Forms.Label()
        Me.Priority_Box = New System.Windows.Forms.ComboBox()
        Me.kb_selector = New System.Windows.Forms.ComboBox()
        Me.duration = New System.Windows.Forms.TextBox()
        Me.dt_duedate = New System.Windows.Forms.DateTimePicker()
        Me.lbl_duedate = New System.Windows.Forms.Label()
        Me.lbl_reminder = New System.Windows.Forms.Label()
        Me.dt_reminder = New System.Windows.Forms.DateTimePicker()
        Me.xl_taskname = New System.Windows.Forms.Label()
        Me.xl_importance = New System.Windows.Forms.Label()
        Me.xl_kanban = New System.Windows.Forms.Label()
        Me.xl_worktime = New System.Windows.Forms.Label()
        Me.xl_ok = New System.Windows.Forms.Label()
        Me.xl_cancel = New System.Windows.Forms.Label()
        Me.xl_reminder = New System.Windows.Forms.Label()
        Me.xl_duedate = New System.Windows.Forms.Label()
        Me.Frame1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Frame1
        '
        Me.Frame1.Controls.Add(Me.xl_sc_bullpin)
        Me.Frame1.Controls.Add(Me.xl_sc_today)
        Me.Frame1.Controls.Add(Me.xl_sc_waiting)
        Me.Frame1.Controls.Add(Me.xl_sc_unprocessed)
        Me.Frame1.Controls.Add(Me.xl_sc_news)
        Me.Frame1.Controls.Add(Me.xl_sc_email)
        Me.Frame1.Controls.Add(Me.xl_sc_readingbusiness)
        Me.Frame1.Controls.Add(Me.xl_sc_calls)
        Me.Frame1.Controls.Add(Me.xl_sc_internet)
        Me.Frame1.Controls.Add(Me.xl_sc_preread)
        Me.Frame1.Controls.Add(Me.xl_sc_meeting)
        Me.Frame1.Controls.Add(Me.xl_sc_personal)
        Me.Frame1.Controls.Add(Me.xl_topic)
        Me.Frame1.Controls.Add(Me.xl_project)
        Me.Frame1.Controls.Add(Me.xl_people)
        Me.Frame1.Controls.Add(Me.xl_context)
        Me.Frame1.Controls.Add(Me.Cat_WaitingFor)
        Me.Frame1.Controls.Add(Me.cbx_bullpin)
        Me.Frame1.Controls.Add(Me.cbx_today)
        Me.Frame1.Controls.Add(Me.cbxFlag)
        Me.Frame1.Controls.Add(Me.Cat_Email)
        Me.Frame1.Controls.Add(Me.Cat_News)
        Me.Frame1.Controls.Add(Me.Cat_Unprocessed)
        Me.Frame1.Controls.Add(Me.Cat_ReadingBusiness)
        Me.Frame1.Controls.Add(Me.Cat_Calls)
        Me.Frame1.Controls.Add(Me.Cat_Internet)
        Me.Frame1.Controls.Add(Me.Cat_PreRead)
        Me.Frame1.Controls.Add(Me.Cat_Meeting)
        Me.Frame1.Controls.Add(Me.lbl_topic)
        Me.Frame1.Controls.Add(Me.Cat_Personal)
        Me.Frame1.Controls.Add(Me.lbl_project)
        Me.Frame1.Controls.Add(Me.lbl_people)
        Me.Frame1.Controls.Add(Me.lbl_context)
        Me.Frame1.Controls.Add(Me.topic_selection)
        Me.Frame1.Controls.Add(Me.project_selection)
        Me.Frame1.Controls.Add(Me.people_selection)
        Me.Frame1.Controls.Add(Me.category_selection)
        Me.Frame1.Location = New System.Drawing.Point(7, 186)
        Me.Frame1.Name = "Frame1"
        Me.Frame1.Size = New System.Drawing.Size(570, 322)
        Me.Frame1.TabIndex = 0
        '
        'xl_sc_bullpin
        '
        Me.xl_sc_bullpin.AutoSize = True
        Me.xl_sc_bullpin.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_sc_bullpin.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_sc_bullpin.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_sc_bullpin.Location = New System.Drawing.Point(438, 274)
        Me.xl_sc_bullpin.Name = "xl_sc_bullpin"
        Me.xl_sc_bullpin.Size = New System.Drawing.Size(17, 16)
        Me.xl_sc_bullpin.TabIndex = 38
        Me.xl_sc_bullpin.Text = "B"
        '
        'xl_sc_today
        '
        Me.xl_sc_today.AutoSize = True
        Me.xl_sc_today.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_sc_today.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_sc_today.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_sc_today.Location = New System.Drawing.Point(352, 274)
        Me.xl_sc_today.Name = "xl_sc_today"
        Me.xl_sc_today.Size = New System.Drawing.Size(17, 16)
        Me.xl_sc_today.TabIndex = 37
        Me.xl_sc_today.Text = "T"
        '
        'xl_sc_waiting
        '
        Me.xl_sc_waiting.AutoSize = True
        Me.xl_sc_waiting.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_sc_waiting.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_sc_waiting.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_sc_waiting.Location = New System.Drawing.Point(20, 255)
        Me.xl_sc_waiting.Name = "xl_sc_waiting"
        Me.xl_sc_waiting.Size = New System.Drawing.Size(21, 16)
        Me.xl_sc_waiting.TabIndex = 36
        Me.xl_sc_waiting.Text = "W"
        '
        'xl_sc_unprocessed
        '
        Me.xl_sc_unprocessed.AutoSize = True
        Me.xl_sc_unprocessed.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_sc_unprocessed.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_sc_unprocessed.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_sc_unprocessed.Location = New System.Drawing.Point(419, 255)
        Me.xl_sc_unprocessed.Name = "xl_sc_unprocessed"
        Me.xl_sc_unprocessed.Size = New System.Drawing.Size(18, 16)
        Me.xl_sc_unprocessed.TabIndex = 35
        Me.xl_sc_unprocessed.Text = "U"
        '
        'xl_sc_news
        '
        Me.xl_sc_news.AutoSize = True
        Me.xl_sc_news.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_sc_news.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_sc_news.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_sc_news.Location = New System.Drawing.Point(284, 255)
        Me.xl_sc_news.Name = "xl_sc_news"
        Me.xl_sc_news.Size = New System.Drawing.Size(18, 16)
        Me.xl_sc_news.TabIndex = 34
        Me.xl_sc_news.Text = "N"
        '
        'xl_sc_email
        '
        Me.xl_sc_email.AutoSize = True
        Me.xl_sc_email.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_sc_email.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_sc_email.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_sc_email.Location = New System.Drawing.Point(166, 255)
        Me.xl_sc_email.Name = "xl_sc_email"
        Me.xl_sc_email.Size = New System.Drawing.Size(17, 16)
        Me.xl_sc_email.TabIndex = 33
        Me.xl_sc_email.Text = "E"
        '
        'xl_sc_readingbusiness
        '
        Me.xl_sc_readingbusiness.AutoSize = True
        Me.xl_sc_readingbusiness.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_sc_readingbusiness.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_sc_readingbusiness.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_sc_readingbusiness.Location = New System.Drawing.Point(430, 216)
        Me.xl_sc_readingbusiness.Name = "xl_sc_readingbusiness"
        Me.xl_sc_readingbusiness.Size = New System.Drawing.Size(18, 16)
        Me.xl_sc_readingbusiness.TabIndex = 32
        Me.xl_sc_readingbusiness.Text = "R"
        '
        'xl_sc_calls
        '
        Me.xl_sc_calls.AutoSize = True
        Me.xl_sc_calls.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_sc_calls.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_sc_calls.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_sc_calls.Location = New System.Drawing.Point(301, 216)
        Me.xl_sc_calls.Name = "xl_sc_calls"
        Me.xl_sc_calls.Size = New System.Drawing.Size(17, 16)
        Me.xl_sc_calls.TabIndex = 31
        Me.xl_sc_calls.Text = "C"
        '
        'xl_sc_internet
        '
        Me.xl_sc_internet.AutoSize = True
        Me.xl_sc_internet.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_sc_internet.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_sc_internet.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_sc_internet.Location = New System.Drawing.Point(166, 215)
        Me.xl_sc_internet.Name = "xl_sc_internet"
        Me.xl_sc_internet.Size = New System.Drawing.Size(11, 16)
        Me.xl_sc_internet.TabIndex = 30
        Me.xl_sc_internet.Text = "I"
        '
        'xl_sc_preread
        '
        Me.xl_sc_preread.AutoSize = True
        Me.xl_sc_preread.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_sc_preread.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_sc_preread.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_sc_preread.Location = New System.Drawing.Point(430, 175)
        Me.xl_sc_preread.Name = "xl_sc_preread"
        Me.xl_sc_preread.Size = New System.Drawing.Size(17, 16)
        Me.xl_sc_preread.TabIndex = 29
        Me.xl_sc_preread.Text = "P"
        '
        'xl_sc_meeting
        '
        Me.xl_sc_meeting.AutoSize = True
        Me.xl_sc_meeting.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_sc_meeting.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_sc_meeting.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_sc_meeting.Location = New System.Drawing.Point(301, 175)
        Me.xl_sc_meeting.Name = "xl_sc_meeting"
        Me.xl_sc_meeting.Size = New System.Drawing.Size(19, 16)
        Me.xl_sc_meeting.TabIndex = 28
        Me.xl_sc_meeting.Text = "M"
        '
        'xl_sc_personal
        '
        Me.xl_sc_personal.AutoSize = True
        Me.xl_sc_personal.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_sc_personal.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_sc_personal.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_sc_personal.Location = New System.Drawing.Point(166, 174)
        Me.xl_sc_personal.Name = "xl_sc_personal"
        Me.xl_sc_personal.Size = New System.Drawing.Size(17, 16)
        Me.xl_sc_personal.TabIndex = 27
        Me.xl_sc_personal.Text = "P"
        '
        'xl_topic
        '
        Me.xl_topic.AutoSize = True
        Me.xl_topic.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_topic.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_topic.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_topic.Location = New System.Drawing.Point(6, 134)
        Me.xl_topic.Name = "xl_topic"
        Me.xl_topic.Size = New System.Drawing.Size(17, 16)
        Me.xl_topic.TabIndex = 22
        Me.xl_topic.Text = "T"
        '
        'xl_project
        '
        Me.xl_project.AutoSize = True
        Me.xl_project.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_project.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_project.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_project.Location = New System.Drawing.Point(6, 98)
        Me.xl_project.Name = "xl_project"
        Me.xl_project.Size = New System.Drawing.Size(17, 16)
        Me.xl_project.TabIndex = 21
        Me.xl_project.Text = "P"
        '
        'xl_people
        '
        Me.xl_people.AutoSize = True
        Me.xl_people.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_people.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_people.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_people.Location = New System.Drawing.Point(6, 60)
        Me.xl_people.Name = "xl_people"
        Me.xl_people.Size = New System.Drawing.Size(17, 16)
        Me.xl_people.TabIndex = 20
        Me.xl_people.Text = "P"
        '
        'xl_context
        '
        Me.xl_context.AutoSize = True
        Me.xl_context.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_context.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_context.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_context.Location = New System.Drawing.Point(6, 25)
        Me.xl_context.Name = "xl_context"
        Me.xl_context.Size = New System.Drawing.Size(17, 16)
        Me.xl_context.TabIndex = 17
        Me.xl_context.Text = "C"
        '
        'Cat_WaitingFor
        '
        Me.Cat_WaitingFor.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Cat_WaitingFor.BackColor = System.Drawing.Color.DarkMagenta
        Me.Cat_WaitingFor.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.Cat_WaitingFor.Location = New System.Drawing.Point(13, 246)
        Me.Cat_WaitingFor.Name = "Cat_WaitingFor"
        Me.Cat_WaitingFor.Size = New System.Drawing.Size(126, 34)
        Me.Cat_WaitingFor.TabIndex = 19
        Me.Cat_WaitingFor.Text = "Waiting For"
        Me.Cat_WaitingFor.UseVisualStyleBackColor = False
        '
        'cbx_bullpin
        '
        Me.cbx_bullpin.AutoSize = True
        Me.cbx_bullpin.Location = New System.Drawing.Point(424, 290)
        Me.cbx_bullpin.Name = "cbx_bullpin"
        Me.cbx_bullpin.Size = New System.Drawing.Size(113, 17)
        Me.cbx_bullpin.TabIndex = 18
        Me.cbx_bullpin.Text = "BULLPIN Priorities"
        Me.cbx_bullpin.UseVisualStyleBackColor = True
        '
        'cbx_today
        '
        Me.cbx_today.AutoSize = True
        Me.cbx_today.Location = New System.Drawing.Point(292, 290)
        Me.cbx_today.Name = "cbx_today"
        Me.cbx_today.Size = New System.Drawing.Size(110, 17)
        Me.cbx_today.TabIndex = 17
        Me.cbx_today.Text = "Complete TODAY"
        Me.cbx_today.UseVisualStyleBackColor = True
        '
        'cbxFlag
        '
        Me.cbxFlag.AutoSize = True
        Me.cbxFlag.Checked = True
        Me.cbxFlag.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbxFlag.Location = New System.Drawing.Point(160, 290)
        Me.cbxFlag.Name = "cbxFlag"
        Me.cbxFlag.Size = New System.Drawing.Size(114, 17)
        Me.cbxFlag.TabIndex = 16
        Me.cbxFlag.Text = "Flag For Follow Up"
        Me.cbxFlag.UseVisualStyleBackColor = True
        '
        'Cat_Email
        '
        Me.Cat_Email.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Cat_Email.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Cat_Email.Location = New System.Drawing.Point(160, 246)
        Me.Cat_Email.Name = "Cat_Email"
        Me.Cat_Email.Size = New System.Drawing.Size(126, 34)
        Me.Cat_Email.TabIndex = 15
        Me.Cat_Email.Text = "Email"
        Me.Cat_Email.UseVisualStyleBackColor = False
        '
        'Cat_News
        '
        Me.Cat_News.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Cat_News.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Cat_News.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Cat_News.Location = New System.Drawing.Point(292, 246)
        Me.Cat_News.Name = "Cat_News"
        Me.Cat_News.Size = New System.Drawing.Size(126, 34)
        Me.Cat_News.TabIndex = 14
        Me.Cat_News.Text = "News | Articles | Other"
        Me.Cat_News.UseVisualStyleBackColor = False
        '
        'Cat_Unprocessed
        '
        Me.Cat_Unprocessed.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Cat_Unprocessed.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Cat_Unprocessed.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Cat_Unprocessed.Location = New System.Drawing.Point(424, 246)
        Me.Cat_Unprocessed.Name = "Cat_Unprocessed"
        Me.Cat_Unprocessed.Size = New System.Drawing.Size(126, 34)
        Me.Cat_Unprocessed.TabIndex = 13
        Me.Cat_Unprocessed.Text = "Unprocessed > 2min"
        Me.Cat_Unprocessed.UseVisualStyleBackColor = False
        '
        'Cat_ReadingBusiness
        '
        Me.Cat_ReadingBusiness.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Cat_ReadingBusiness.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Cat_ReadingBusiness.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Cat_ReadingBusiness.Location = New System.Drawing.Point(424, 206)
        Me.Cat_ReadingBusiness.Name = "Cat_ReadingBusiness"
        Me.Cat_ReadingBusiness.Size = New System.Drawing.Size(126, 34)
        Me.Cat_ReadingBusiness.TabIndex = 12
        Me.Cat_ReadingBusiness.Text = "Reading - Business"
        Me.Cat_ReadingBusiness.UseVisualStyleBackColor = False
        '
        'Cat_Calls
        '
        Me.Cat_Calls.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Cat_Calls.BackColor = System.Drawing.Color.Blue
        Me.Cat_Calls.ForeColor = System.Drawing.SystemColors.ButtonFace
        Me.Cat_Calls.Location = New System.Drawing.Point(292, 206)
        Me.Cat_Calls.Name = "Cat_Calls"
        Me.Cat_Calls.Size = New System.Drawing.Size(126, 34)
        Me.Cat_Calls.TabIndex = 11
        Me.Cat_Calls.Text = "Calls"
        Me.Cat_Calls.UseVisualStyleBackColor = False
        '
        'Cat_Internet
        '
        Me.Cat_Internet.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Cat_Internet.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.Cat_Internet.Location = New System.Drawing.Point(160, 206)
        Me.Cat_Internet.Name = "Cat_Internet"
        Me.Cat_Internet.Size = New System.Drawing.Size(126, 34)
        Me.Cat_Internet.TabIndex = 10
        Me.Cat_Internet.Text = "Internet"
        Me.Cat_Internet.UseVisualStyleBackColor = False
        '
        'Cat_PreRead
        '
        Me.Cat_PreRead.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Cat_PreRead.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Cat_PreRead.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Cat_PreRead.Location = New System.Drawing.Point(424, 166)
        Me.Cat_PreRead.Name = "Cat_PreRead"
        Me.Cat_PreRead.Size = New System.Drawing.Size(126, 34)
        Me.Cat_PreRead.TabIndex = 9
        Me.Cat_PreRead.Text = "PreRead"
        Me.Cat_PreRead.UseVisualStyleBackColor = False
        '
        'Cat_Meeting
        '
        Me.Cat_Meeting.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Cat_Meeting.BackColor = System.Drawing.Color.Blue
        Me.Cat_Meeting.ForeColor = System.Drawing.SystemColors.ButtonFace
        Me.Cat_Meeting.Location = New System.Drawing.Point(292, 166)
        Me.Cat_Meeting.Name = "Cat_Meeting"
        Me.Cat_Meeting.Size = New System.Drawing.Size(126, 34)
        Me.Cat_Meeting.TabIndex = 8
        Me.Cat_Meeting.Text = "Meeting"
        Me.Cat_Meeting.UseVisualStyleBackColor = False
        '
        'lbl_topic
        '
        Me.lbl_topic.AutoSize = True
        Me.lbl_topic.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_topic.Location = New System.Drawing.Point(25, 134)
        Me.lbl_topic.Name = "lbl_topic"
        Me.lbl_topic.Size = New System.Drawing.Size(73, 16)
        Me.lbl_topic.TabIndex = 7
        Me.lbl_topic.Text = "Topic Tag:"
        '
        'Cat_Personal
        '
        Me.Cat_Personal.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Cat_Personal.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.Cat_Personal.Location = New System.Drawing.Point(160, 166)
        Me.Cat_Personal.Name = "Cat_Personal"
        Me.Cat_Personal.Size = New System.Drawing.Size(126, 34)
        Me.Cat_Personal.TabIndex = 1
        Me.Cat_Personal.Text = "PERSONAL"
        Me.Cat_Personal.UseVisualStyleBackColor = False
        '
        'lbl_project
        '
        Me.lbl_project.AutoSize = True
        Me.lbl_project.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_project.Location = New System.Drawing.Point(25, 98)
        Me.lbl_project.Name = "lbl_project"
        Me.lbl_project.Size = New System.Drawing.Size(82, 16)
        Me.lbl_project.TabIndex = 6
        Me.lbl_project.Text = "Project Flag:"
        '
        'lbl_people
        '
        Me.lbl_people.AutoSize = True
        Me.lbl_people.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_people.Location = New System.Drawing.Point(25, 61)
        Me.lbl_people.Name = "lbl_people"
        Me.lbl_people.Size = New System.Drawing.Size(84, 16)
        Me.lbl_people.TabIndex = 5
        Me.lbl_people.Text = "People Flag:"
        '
        'lbl_context
        '
        Me.lbl_context.AutoSize = True
        Me.lbl_context.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_context.Location = New System.Drawing.Point(25, 25)
        Me.lbl_context.Name = "lbl_context"
        Me.lbl_context.Size = New System.Drawing.Size(84, 16)
        Me.lbl_context.TabIndex = 4
        Me.lbl_context.Text = "Context Flag:"
        '
        'topic_selection
        '
        Me.topic_selection.BackColor = System.Drawing.SystemColors.Window
        Me.topic_selection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.topic_selection.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.topic_selection.Location = New System.Drawing.Point(160, 133)
        Me.topic_selection.Name = "topic_selection"
        Me.topic_selection.Size = New System.Drawing.Size(390, 24)
        Me.topic_selection.TabIndex = 3
        Me.topic_selection.Text = "[Other Topics Flagged]"
        '
        'project_selection
        '
        Me.project_selection.BackColor = System.Drawing.SystemColors.Window
        Me.project_selection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.project_selection.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.project_selection.Location = New System.Drawing.Point(160, 97)
        Me.project_selection.Name = "project_selection"
        Me.project_selection.Size = New System.Drawing.Size(390, 24)
        Me.project_selection.TabIndex = 2
        Me.project_selection.Text = "[Projects Flagged]"
        '
        'people_selection
        '
        Me.people_selection.BackColor = System.Drawing.SystemColors.Window
        Me.people_selection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.people_selection.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.people_selection.Location = New System.Drawing.Point(160, 60)
        Me.people_selection.Name = "people_selection"
        Me.people_selection.Size = New System.Drawing.Size(390, 24)
        Me.people_selection.TabIndex = 1
        Me.people_selection.Text = "[Assigned People Flagged]"
        '
        'category_selection
        '
        Me.category_selection.BackColor = System.Drawing.SystemColors.Window
        Me.category_selection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.category_selection.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.category_selection.Location = New System.Drawing.Point(160, 24)
        Me.category_selection.Name = "category_selection"
        Me.category_selection.Size = New System.Drawing.Size(390, 24)
        Me.category_selection.TabIndex = 0
        Me.category_selection.Text = "[Category Label]"
        '
        'OK_Button
        '
        Me.OK_Button.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.OK_Button.Location = New System.Drawing.Point(138, 530)
        Me.OK_Button.Name = "OK_Button"
        Me.OK_Button.Size = New System.Drawing.Size(145, 57)
        Me.OK_Button.TabIndex = 1
        Me.OK_Button.Text = "OK"
        Me.OK_Button.UseVisualStyleBackColor = True
        '
        'Cancel_Button
        '
        Me.Cancel_Button.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Cancel_Button.Location = New System.Drawing.Point(297, 530)
        Me.Cancel_Button.Name = "Cancel_Button"
        Me.Cancel_Button.Size = New System.Drawing.Size(145, 57)
        Me.Cancel_Button.TabIndex = 2
        Me.Cancel_Button.Text = "Cancel"
        Me.Cancel_Button.UseVisualStyleBackColor = True
        '
        'lbl_taskname
        '
        Me.lbl_taskname.AutoSize = True
        Me.lbl_taskname.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_taskname.Location = New System.Drawing.Point(12, 23)
        Me.lbl_taskname.Name = "lbl_taskname"
        Me.lbl_taskname.Size = New System.Drawing.Size(97, 16)
        Me.lbl_taskname.TabIndex = 1
        Me.lbl_taskname.Text = "Name Of Task:"
        '
        'task_name
        '
        Me.task_name.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.task_name.ForeColor = System.Drawing.SystemColors.WindowText
        Me.task_name.Location = New System.Drawing.Point(12, 42)
        Me.task_name.Name = "task_name"
        Me.task_name.Size = New System.Drawing.Size(560, 22)
        Me.task_name.TabIndex = 2
        '
        'lbl_priority
        '
        Me.lbl_priority.AutoSize = True
        Me.lbl_priority.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_priority.Location = New System.Drawing.Point(12, 83)
        Me.lbl_priority.Name = "lbl_priority"
        Me.lbl_priority.Size = New System.Drawing.Size(77, 16)
        Me.lbl_priority.TabIndex = 3
        Me.lbl_priority.Text = "Importance:"
        '
        'lbl_kbf
        '
        Me.lbl_kbf.AutoSize = True
        Me.lbl_kbf.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_kbf.Location = New System.Drawing.Point(12, 111)
        Me.lbl_kbf.Name = "lbl_kbf"
        Me.lbl_kbf.Size = New System.Drawing.Size(56, 16)
        Me.lbl_kbf.TabIndex = 5
        Me.lbl_kbf.Text = "Kanban:"
        '
        'lbl_duration
        '
        Me.lbl_duration.AutoSize = True
        Me.lbl_duration.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_duration.Location = New System.Drawing.Point(12, 144)
        Me.lbl_duration.Name = "lbl_duration"
        Me.lbl_duration.Size = New System.Drawing.Size(76, 16)
        Me.lbl_duration.TabIndex = 7
        Me.lbl_duration.Text = "Work Time:"
        '
        'Priority_Box
        '
        Me.Priority_Box.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.Priority_Box.FormattingEnabled = True
        Me.Priority_Box.Items.AddRange(New Object() {"High", "Normal", "Low"})
        Me.Priority_Box.Location = New System.Drawing.Point(120, 82)
        Me.Priority_Box.Name = "Priority_Box"
        Me.Priority_Box.Size = New System.Drawing.Size(121, 21)
        Me.Priority_Box.TabIndex = 4
        '
        'kb_selector
        '
        Me.kb_selector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.kb_selector.FormattingEnabled = True
        Me.kb_selector.Items.AddRange(New Object() {"Backlog", "Planned", "InProgress", "Complete"})
        Me.kb_selector.Location = New System.Drawing.Point(120, 111)
        Me.kb_selector.Name = "kb_selector"
        Me.kb_selector.Size = New System.Drawing.Size(121, 21)
        Me.kb_selector.TabIndex = 6
        '
        'duration
        '
        Me.duration.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.duration.ForeColor = System.Drawing.SystemColors.WindowText
        Me.duration.Location = New System.Drawing.Point(120, 141)
        Me.duration.Name = "duration"
        Me.duration.Size = New System.Drawing.Size(121, 22)
        Me.duration.TabIndex = 8
        '
        'dt_duedate
        '
        Me.dt_duedate.Checked = False
        Me.dt_duedate.CustomFormat = "MM/dd/yyyy hh:mm tt"
        Me.dt_duedate.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dt_duedate.Location = New System.Drawing.Point(388, 83)
        Me.dt_duedate.Name = "dt_duedate"
        Me.dt_duedate.ShowCheckBox = True
        Me.dt_duedate.Size = New System.Drawing.Size(184, 20)
        Me.dt_duedate.TabIndex = 10
        '
        'lbl_duedate
        '
        Me.lbl_duedate.AutoSize = True
        Me.lbl_duedate.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_duedate.Location = New System.Drawing.Point(310, 83)
        Me.lbl_duedate.Name = "lbl_duedate"
        Me.lbl_duedate.Size = New System.Drawing.Size(67, 16)
        Me.lbl_duedate.TabIndex = 9
        Me.lbl_duedate.Text = "Due Date:"
        '
        'lbl_reminder
        '
        Me.lbl_reminder.AutoSize = True
        Me.lbl_reminder.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_reminder.Location = New System.Drawing.Point(310, 114)
        Me.lbl_reminder.Name = "lbl_reminder"
        Me.lbl_reminder.Size = New System.Drawing.Size(69, 16)
        Me.lbl_reminder.TabIndex = 11
        Me.lbl_reminder.Text = "Reminder:"
        '
        'dt_reminder
        '
        Me.dt_reminder.Checked = False
        Me.dt_reminder.CustomFormat = "MM/dd/yyyy hh:mm tt"
        Me.dt_reminder.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dt_reminder.Location = New System.Drawing.Point(388, 112)
        Me.dt_reminder.Name = "dt_reminder"
        Me.dt_reminder.ShowCheckBox = True
        Me.dt_reminder.Size = New System.Drawing.Size(184, 20)
        Me.dt_reminder.TabIndex = 12
        '
        'xl_taskname
        '
        Me.xl_taskname.AutoSize = True
        Me.xl_taskname.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_taskname.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_taskname.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_taskname.Location = New System.Drawing.Point(7, 23)
        Me.xl_taskname.Name = "xl_taskname"
        Me.xl_taskname.Size = New System.Drawing.Size(18, 16)
        Me.xl_taskname.TabIndex = 13
        Me.xl_taskname.Text = "N"
        '
        'xl_importance
        '
        Me.xl_importance.AutoSize = True
        Me.xl_importance.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_importance.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_importance.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_importance.Location = New System.Drawing.Point(6, 82)
        Me.xl_importance.Name = "xl_importance"
        Me.xl_importance.Size = New System.Drawing.Size(11, 16)
        Me.xl_importance.TabIndex = 14
        Me.xl_importance.Text = "I"
        '
        'xl_kanban
        '
        Me.xl_kanban.AutoSize = True
        Me.xl_kanban.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_kanban.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_kanban.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_kanban.Location = New System.Drawing.Point(6, 111)
        Me.xl_kanban.Name = "xl_kanban"
        Me.xl_kanban.Size = New System.Drawing.Size(16, 16)
        Me.xl_kanban.TabIndex = 15
        Me.xl_kanban.Text = "K"
        '
        'xl_worktime
        '
        Me.xl_worktime.AutoSize = True
        Me.xl_worktime.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_worktime.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_worktime.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_worktime.Location = New System.Drawing.Point(6, 143)
        Me.xl_worktime.Name = "xl_worktime"
        Me.xl_worktime.Size = New System.Drawing.Size(21, 16)
        Me.xl_worktime.TabIndex = 16
        Me.xl_worktime.Text = "W"
        '
        'xl_ok
        '
        Me.xl_ok.AutoSize = True
        Me.xl_ok.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_ok.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_ok.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_ok.Location = New System.Drawing.Point(191, 550)
        Me.xl_ok.Name = "xl_ok"
        Me.xl_ok.Size = New System.Drawing.Size(18, 16)
        Me.xl_ok.TabIndex = 23
        Me.xl_ok.Text = "O"
        '
        'xl_cancel
        '
        Me.xl_cancel.AutoSize = True
        Me.xl_cancel.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_cancel.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_cancel.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_cancel.Location = New System.Drawing.Point(341, 550)
        Me.xl_cancel.Name = "xl_cancel"
        Me.xl_cancel.Size = New System.Drawing.Size(17, 16)
        Me.xl_cancel.TabIndex = 24
        Me.xl_cancel.Text = "C"
        '
        'xl_reminder
        '
        Me.xl_reminder.AutoSize = True
        Me.xl_reminder.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_reminder.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_reminder.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_reminder.Location = New System.Drawing.Point(305, 114)
        Me.xl_reminder.Name = "xl_reminder"
        Me.xl_reminder.Size = New System.Drawing.Size(18, 16)
        Me.xl_reminder.TabIndex = 25
        Me.xl_reminder.Text = "R"
        '
        'xl_duedate
        '
        Me.xl_duedate.AutoSize = True
        Me.xl_duedate.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.xl_duedate.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xl_duedate.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.xl_duedate.Location = New System.Drawing.Point(305, 82)
        Me.xl_duedate.Name = "xl_duedate"
        Me.xl_duedate.Size = New System.Drawing.Size(18, 16)
        Me.xl_duedate.TabIndex = 26
        Me.xl_duedate.Text = "D"
        '
        'TaskViewer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(584, 611)
        Me.Controls.Add(Me.xl_duedate)
        Me.Controls.Add(Me.xl_reminder)
        Me.Controls.Add(Me.xl_cancel)
        Me.Controls.Add(Me.xl_ok)
        Me.Controls.Add(Me.xl_worktime)
        Me.Controls.Add(Me.xl_kanban)
        Me.Controls.Add(Me.xl_importance)
        Me.Controls.Add(Me.xl_taskname)
        Me.Controls.Add(Me.dt_reminder)
        Me.Controls.Add(Me.lbl_reminder)
        Me.Controls.Add(Me.lbl_duedate)
        Me.Controls.Add(Me.dt_duedate)
        Me.Controls.Add(Me.duration)
        Me.Controls.Add(Me.kb_selector)
        Me.Controls.Add(Me.Priority_Box)
        Me.Controls.Add(Me.lbl_duration)
        Me.Controls.Add(Me.lbl_kbf)
        Me.Controls.Add(Me.lbl_priority)
        Me.Controls.Add(Me.task_name)
        Me.Controls.Add(Me.lbl_taskname)
        Me.Controls.Add(Me.Cancel_Button)
        Me.Controls.Add(Me.OK_Button)
        Me.Controls.Add(Me.Frame1)
        Me.Name = "TaskViewer"
        Me.Text = "Change Flagged Email Into Task"
        Me.Frame1.ResumeLayout(False)
        Me.Frame1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Frame1 As Windows.Forms.Panel
    Friend WithEvents lbl_topic As Windows.Forms.Label
    Friend WithEvents lbl_project As Windows.Forms.Label
    Friend WithEvents lbl_people As Windows.Forms.Label
    Friend WithEvents lbl_context As Windows.Forms.Label
    Friend WithEvents topic_selection As Windows.Forms.Label
    Friend WithEvents project_selection As Windows.Forms.Label
    Friend WithEvents people_selection As Windows.Forms.Label
    Friend WithEvents category_selection As Windows.Forms.Label
    Friend WithEvents OK_Button As Windows.Forms.Button
    Friend WithEvents Cancel_Button As Windows.Forms.Button
    Friend WithEvents Cat_News As Windows.Forms.Button
    Friend WithEvents Cat_Unprocessed As Windows.Forms.Button
    Friend WithEvents Cat_ReadingBusiness As Windows.Forms.Button
    Friend WithEvents Cat_Calls As Windows.Forms.Button
    Friend WithEvents Cat_Internet As Windows.Forms.Button
    Friend WithEvents Cat_PreRead As Windows.Forms.Button
    Friend WithEvents Cat_Meeting As Windows.Forms.Button
    Friend WithEvents Cat_Personal As Windows.Forms.Button
    Friend WithEvents Cat_Email As Windows.Forms.Button
    Friend WithEvents Cat_WaitingFor As Windows.Forms.Button
    Friend WithEvents cbx_bullpin As Windows.Forms.CheckBox
    Friend WithEvents cbx_today As Windows.Forms.CheckBox
    Friend WithEvents cbxFlag As Windows.Forms.CheckBox
    Friend WithEvents lbl_taskname As Windows.Forms.Label
    Friend WithEvents task_name As Windows.Forms.TextBox
    Friend WithEvents lbl_priority As Windows.Forms.Label
    Friend WithEvents lbl_kbf As Windows.Forms.Label
    Friend WithEvents lbl_duration As Windows.Forms.Label
    Friend WithEvents Priority_Box As Windows.Forms.ComboBox
    Friend WithEvents kb_selector As Windows.Forms.ComboBox
    Friend WithEvents duration As Windows.Forms.TextBox
    Friend WithEvents dt_duedate As Windows.Forms.DateTimePicker
    Friend WithEvents lbl_duedate As Windows.Forms.Label
    Friend WithEvents lbl_reminder As Windows.Forms.Label
    Friend WithEvents dt_reminder As Windows.Forms.DateTimePicker
    Friend WithEvents xl_topic As Windows.Forms.Label
    Friend WithEvents xl_project As Windows.Forms.Label
    Friend WithEvents xl_people As Windows.Forms.Label
    Friend WithEvents xl_context As Windows.Forms.Label
    Friend WithEvents xl_taskname As Windows.Forms.Label
    Friend WithEvents xl_importance As Windows.Forms.Label
    Friend WithEvents xl_kanban As Windows.Forms.Label
    Friend WithEvents xl_worktime As Windows.Forms.Label
    Friend WithEvents xl_ok As Windows.Forms.Label
    Friend WithEvents xl_cancel As Windows.Forms.Label
    Friend WithEvents xl_reminder As Windows.Forms.Label
    Friend WithEvents xl_duedate As Windows.Forms.Label
    Friend WithEvents xl_sc_bullpin As Windows.Forms.Label
    Friend WithEvents xl_sc_today As Windows.Forms.Label
    Friend WithEvents xl_sc_waiting As Windows.Forms.Label
    Friend WithEvents xl_sc_unprocessed As Windows.Forms.Label
    Friend WithEvents xl_sc_news As Windows.Forms.Label
    Friend WithEvents xl_sc_email As Windows.Forms.Label
    Friend WithEvents xl_sc_readingbusiness As Windows.Forms.Label
    Friend WithEvents xl_sc_calls As Windows.Forms.Label
    Friend WithEvents xl_sc_internet As Windows.Forms.Label
    Friend WithEvents xl_sc_preread As Windows.Forms.Label
    Friend WithEvents xl_sc_meeting As Windows.Forms.Label
    Friend WithEvents xl_sc_personal As Windows.Forms.Label
End Class
