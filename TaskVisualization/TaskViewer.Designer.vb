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
        Me.XlScBullpin = New System.Windows.Forms.Label()
        Me.XlScToday = New System.Windows.Forms.Label()
        Me.XlScWaiting = New System.Windows.Forms.Label()
        Me.XlScUnprocessed = New System.Windows.Forms.Label()
        Me.XlScNews = New System.Windows.Forms.Label()
        Me.XlScEmail = New System.Windows.Forms.Label()
        Me.XlScReadingbusiness = New System.Windows.Forms.Label()
        Me.XlScCalls = New System.Windows.Forms.Label()
        Me.XlScInternet = New System.Windows.Forms.Label()
        Me.XlScPreread = New System.Windows.Forms.Label()
        Me.XlScMeeting = New System.Windows.Forms.Label()
        Me.XlScPersonal = New System.Windows.Forms.Label()
        Me.XlTopic = New System.Windows.Forms.Label()
        Me.XlProject = New System.Windows.Forms.Label()
        Me.XlPeople = New System.Windows.Forms.Label()
        Me.XlContext = New System.Windows.Forms.Label()
        Me.ShortcutWaitingFor = New System.Windows.Forms.Button()
        Me.CbxBullpin = New System.Windows.Forms.CheckBox()
        Me.CbxToday = New System.Windows.Forms.CheckBox()
        Me.CbxFlagAsTask = New System.Windows.Forms.CheckBox()
        Me.ShortcutEmail = New System.Windows.Forms.Button()
        Me.ShortcutNews = New System.Windows.Forms.Button()
        Me.ShortcutUnprocessed = New System.Windows.Forms.Button()
        Me.ShortcutReadingBusiness = New System.Windows.Forms.Button()
        Me.ShortcutCalls = New System.Windows.Forms.Button()
        Me.ShortcutInternet = New System.Windows.Forms.Button()
        Me.ShortcutPreRead = New System.Windows.Forms.Button()
        Me.ShortcutMeeting = New System.Windows.Forms.Button()
        Me.LblTopic = New System.Windows.Forms.Label()
        Me.ShortcutPersonal = New System.Windows.Forms.Button()
        Me.LblProject = New System.Windows.Forms.Label()
        Me.LblPeople = New System.Windows.Forms.Label()
        Me.LblContext = New System.Windows.Forms.Label()
        Me.TopicSelection = New System.Windows.Forms.Label()
        Me.ProjectSelection = New System.Windows.Forms.Label()
        Me.PeopleSelection = New System.Windows.Forms.Label()
        Me.CategorySelection = New System.Windows.Forms.Label()
        Me.OKButton = New System.Windows.Forms.Button()
        Me.Cancel_Button = New System.Windows.Forms.Button()
        Me.LblTaskname = New System.Windows.Forms.Label()
        Me.TaskName = New System.Windows.Forms.TextBox()
        Me.LblPriority = New System.Windows.Forms.Label()
        Me.LblKbf = New System.Windows.Forms.Label()
        Me.LblDuration = New System.Windows.Forms.Label()
        Me.PriorityBox = New System.Windows.Forms.ComboBox()
        Me.KbSelector = New System.Windows.Forms.ComboBox()
        Me.Duration = New System.Windows.Forms.TextBox()
        Me.DtDuedate = New System.Windows.Forms.DateTimePicker()
        Me.LblDuedate = New System.Windows.Forms.Label()
        Me.LblReminder = New System.Windows.Forms.Label()
        Me.DtReminder = New System.Windows.Forms.DateTimePicker()
        Me.XlTaskname = New System.Windows.Forms.Label()
        Me.XlImportance = New System.Windows.Forms.Label()
        Me.XlKanban = New System.Windows.Forms.Label()
        Me.XlWorktime = New System.Windows.Forms.Label()
        Me.XlOk = New System.Windows.Forms.Label()
        Me.XlCancel = New System.Windows.Forms.Label()
        Me.XlReminder = New System.Windows.Forms.Label()
        Me.XlDuedate = New System.Windows.Forms.Label()
        Me.Frame1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Frame1
        '
        Me.Frame1.Controls.Add(Me.XlScBullpin)
        Me.Frame1.Controls.Add(Me.XlScToday)
        Me.Frame1.Controls.Add(Me.XlScWaiting)
        Me.Frame1.Controls.Add(Me.XlScUnprocessed)
        Me.Frame1.Controls.Add(Me.XlScNews)
        Me.Frame1.Controls.Add(Me.XlScEmail)
        Me.Frame1.Controls.Add(Me.XlScReadingbusiness)
        Me.Frame1.Controls.Add(Me.XlScCalls)
        Me.Frame1.Controls.Add(Me.XlScInternet)
        Me.Frame1.Controls.Add(Me.XlScPreread)
        Me.Frame1.Controls.Add(Me.XlScMeeting)
        Me.Frame1.Controls.Add(Me.XlScPersonal)
        Me.Frame1.Controls.Add(Me.XlTopic)
        Me.Frame1.Controls.Add(Me.XlProject)
        Me.Frame1.Controls.Add(Me.XlPeople)
        Me.Frame1.Controls.Add(Me.XlContext)
        Me.Frame1.Controls.Add(Me.ShortcutWaitingFor)
        Me.Frame1.Controls.Add(Me.CbxBullpin)
        Me.Frame1.Controls.Add(Me.CbxToday)
        Me.Frame1.Controls.Add(Me.CbxFlagAsTask)
        Me.Frame1.Controls.Add(Me.ShortcutEmail)
        Me.Frame1.Controls.Add(Me.ShortcutNews)
        Me.Frame1.Controls.Add(Me.ShortcutUnprocessed)
        Me.Frame1.Controls.Add(Me.ShortcutReadingBusiness)
        Me.Frame1.Controls.Add(Me.ShortcutCalls)
        Me.Frame1.Controls.Add(Me.ShortcutInternet)
        Me.Frame1.Controls.Add(Me.ShortcutPreRead)
        Me.Frame1.Controls.Add(Me.ShortcutMeeting)
        Me.Frame1.Controls.Add(Me.LblTopic)
        Me.Frame1.Controls.Add(Me.ShortcutPersonal)
        Me.Frame1.Controls.Add(Me.LblProject)
        Me.Frame1.Controls.Add(Me.LblPeople)
        Me.Frame1.Controls.Add(Me.LblContext)
        Me.Frame1.Controls.Add(Me.TopicSelection)
        Me.Frame1.Controls.Add(Me.ProjectSelection)
        Me.Frame1.Controls.Add(Me.PeopleSelection)
        Me.Frame1.Controls.Add(Me.CategorySelection)
        Me.Frame1.Location = New System.Drawing.Point(7, 186)
        Me.Frame1.Name = "Frame1"
        Me.Frame1.Size = New System.Drawing.Size(570, 322)
        Me.Frame1.TabIndex = 0
        '
        'XlScBullpin
        '
        Me.XlScBullpin.AutoSize = True
        Me.XlScBullpin.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlScBullpin.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlScBullpin.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlScBullpin.Location = New System.Drawing.Point(438, 274)
        Me.XlScBullpin.Name = "xl_sc_bullpin"
        Me.XlScBullpin.Size = New System.Drawing.Size(17, 16)
        Me.XlScBullpin.TabIndex = 38
        Me.XlScBullpin.Text = "B"
        '
        'XlScToday
        '
        Me.XlScToday.AutoSize = True
        Me.XlScToday.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlScToday.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlScToday.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlScToday.Location = New System.Drawing.Point(352, 274)
        Me.XlScToday.Name = "xl_sc_today"
        Me.XlScToday.Size = New System.Drawing.Size(17, 16)
        Me.XlScToday.TabIndex = 37
        Me.XlScToday.Text = "T"
        '
        'XlScWaiting
        '
        Me.XlScWaiting.AutoSize = True
        Me.XlScWaiting.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlScWaiting.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlScWaiting.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlScWaiting.Location = New System.Drawing.Point(20, 255)
        Me.XlScWaiting.Name = "xl_sc_waiting"
        Me.XlScWaiting.Size = New System.Drawing.Size(21, 16)
        Me.XlScWaiting.TabIndex = 36
        Me.XlScWaiting.Text = "W"
        '
        'XlScUnprocessed
        '
        Me.XlScUnprocessed.AutoSize = True
        Me.XlScUnprocessed.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlScUnprocessed.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlScUnprocessed.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlScUnprocessed.Location = New System.Drawing.Point(419, 255)
        Me.XlScUnprocessed.Name = "xl_sc_unprocessed"
        Me.XlScUnprocessed.Size = New System.Drawing.Size(18, 16)
        Me.XlScUnprocessed.TabIndex = 35
        Me.XlScUnprocessed.Text = "U"
        '
        'XlScNews
        '
        Me.XlScNews.AutoSize = True
        Me.XlScNews.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlScNews.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlScNews.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlScNews.Location = New System.Drawing.Point(284, 255)
        Me.XlScNews.Name = "xl_sc_news"
        Me.XlScNews.Size = New System.Drawing.Size(18, 16)
        Me.XlScNews.TabIndex = 34
        Me.XlScNews.Text = "N"
        '
        'XlScEmail
        '
        Me.XlScEmail.AutoSize = True
        Me.XlScEmail.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlScEmail.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlScEmail.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlScEmail.Location = New System.Drawing.Point(166, 255)
        Me.XlScEmail.Name = "xl_sc_email"
        Me.XlScEmail.Size = New System.Drawing.Size(17, 16)
        Me.XlScEmail.TabIndex = 33
        Me.XlScEmail.Text = "E"
        '
        'XlScReadingbusiness
        '
        Me.XlScReadingbusiness.AutoSize = True
        Me.XlScReadingbusiness.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlScReadingbusiness.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlScReadingbusiness.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlScReadingbusiness.Location = New System.Drawing.Point(430, 216)
        Me.XlScReadingbusiness.Name = "xl_sc_readingbusiness"
        Me.XlScReadingbusiness.Size = New System.Drawing.Size(18, 16)
        Me.XlScReadingbusiness.TabIndex = 32
        Me.XlScReadingbusiness.Text = "R"
        '
        'XlScCalls
        '
        Me.XlScCalls.AutoSize = True
        Me.XlScCalls.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlScCalls.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlScCalls.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlScCalls.Location = New System.Drawing.Point(301, 216)
        Me.XlScCalls.Name = "xl_sc_calls"
        Me.XlScCalls.Size = New System.Drawing.Size(17, 16)
        Me.XlScCalls.TabIndex = 31
        Me.XlScCalls.Text = "C"
        '
        'XlScInternet
        '
        Me.XlScInternet.AutoSize = True
        Me.XlScInternet.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlScInternet.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlScInternet.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlScInternet.Location = New System.Drawing.Point(166, 215)
        Me.XlScInternet.Name = "xl_sc_internet"
        Me.XlScInternet.Size = New System.Drawing.Size(11, 16)
        Me.XlScInternet.TabIndex = 30
        Me.XlScInternet.Text = "I"
        '
        'XlScPreread
        '
        Me.XlScPreread.AutoSize = True
        Me.XlScPreread.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlScPreread.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlScPreread.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlScPreread.Location = New System.Drawing.Point(430, 175)
        Me.XlScPreread.Name = "xl_sc_preread"
        Me.XlScPreread.Size = New System.Drawing.Size(17, 16)
        Me.XlScPreread.TabIndex = 29
        Me.XlScPreread.Text = "P"
        '
        'XlScMeeting
        '
        Me.XlScMeeting.AutoSize = True
        Me.XlScMeeting.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlScMeeting.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlScMeeting.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlScMeeting.Location = New System.Drawing.Point(301, 175)
        Me.XlScMeeting.Name = "xl_sc_meeting"
        Me.XlScMeeting.Size = New System.Drawing.Size(19, 16)
        Me.XlScMeeting.TabIndex = 28
        Me.XlScMeeting.Text = "M"
        '
        'XlScPersonal
        '
        Me.XlScPersonal.AutoSize = True
        Me.XlScPersonal.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlScPersonal.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlScPersonal.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlScPersonal.Location = New System.Drawing.Point(166, 174)
        Me.XlScPersonal.Name = "xl_sc_personal"
        Me.XlScPersonal.Size = New System.Drawing.Size(17, 16)
        Me.XlScPersonal.TabIndex = 27
        Me.XlScPersonal.Text = "P"
        '
        'XlTopic
        '
        Me.XlTopic.AutoSize = True
        Me.XlTopic.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlTopic.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlTopic.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlTopic.Location = New System.Drawing.Point(6, 134)
        Me.XlTopic.Name = "xl_topic"
        Me.XlTopic.Size = New System.Drawing.Size(17, 16)
        Me.XlTopic.TabIndex = 22
        Me.XlTopic.Text = "T"
        '
        'XlProject
        '
        Me.XlProject.AutoSize = True
        Me.XlProject.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlProject.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlProject.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlProject.Location = New System.Drawing.Point(6, 98)
        Me.XlProject.Name = "xl_project"
        Me.XlProject.Size = New System.Drawing.Size(17, 16)
        Me.XlProject.TabIndex = 21
        Me.XlProject.Text = "P"
        '
        'XlPeople
        '
        Me.XlPeople.AutoSize = True
        Me.XlPeople.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlPeople.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlPeople.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlPeople.Location = New System.Drawing.Point(6, 60)
        Me.XlPeople.Name = "xl_people"
        Me.XlPeople.Size = New System.Drawing.Size(17, 16)
        Me.XlPeople.TabIndex = 20
        Me.XlPeople.Text = "P"
        '
        'XlContext
        '
        Me.XlContext.AutoSize = True
        Me.XlContext.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlContext.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlContext.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlContext.Location = New System.Drawing.Point(6, 25)
        Me.XlContext.Name = "xl_context"
        Me.XlContext.Size = New System.Drawing.Size(17, 16)
        Me.XlContext.TabIndex = 17
        Me.XlContext.Text = "C"
        '
        'ShortcutWaitingFor
        '
        Me.ShortcutWaitingFor.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ShortcutWaitingFor.BackColor = System.Drawing.Color.DarkMagenta
        Me.ShortcutWaitingFor.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.ShortcutWaitingFor.Location = New System.Drawing.Point(13, 246)
        Me.ShortcutWaitingFor.Name = "Cat_WaitingFor"
        Me.ShortcutWaitingFor.Size = New System.Drawing.Size(126, 34)
        Me.ShortcutWaitingFor.TabIndex = 19
        Me.ShortcutWaitingFor.Text = "Waiting For"
        Me.ShortcutWaitingFor.UseVisualStyleBackColor = False
        '
        'CbxBullpin
        '
        Me.CbxBullpin.AutoSize = True
        Me.CbxBullpin.Location = New System.Drawing.Point(424, 290)
        Me.CbxBullpin.Name = "cbx_bullpin"
        Me.CbxBullpin.Size = New System.Drawing.Size(113, 17)
        Me.CbxBullpin.TabIndex = 18
        Me.CbxBullpin.Text = "BULLPIN Priorities"
        Me.CbxBullpin.UseVisualStyleBackColor = True
        '
        'CbxToday
        '
        Me.CbxToday.AutoSize = True
        Me.CbxToday.Location = New System.Drawing.Point(292, 290)
        Me.CbxToday.Name = "cbx_today"
        Me.CbxToday.Size = New System.Drawing.Size(110, 17)
        Me.CbxToday.TabIndex = 17
        Me.CbxToday.Text = "Complete TODAY"
        Me.CbxToday.UseVisualStyleBackColor = True
        '
        'CbxFlagAsTask
        '
        Me.CbxFlagAsTask.AutoSize = True
        Me.CbxFlagAsTask.Checked = True
        Me.CbxFlagAsTask.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CbxFlagAsTask.Location = New System.Drawing.Point(160, 290)
        Me.CbxFlagAsTask.Name = "cbxFlag"
        Me.CbxFlagAsTask.Size = New System.Drawing.Size(114, 17)
        Me.CbxFlagAsTask.TabIndex = 16
        Me.CbxFlagAsTask.Text = "Flag For Follow Up"
        Me.CbxFlagAsTask.UseVisualStyleBackColor = True
        '
        'ShortcutEmail
        '
        Me.ShortcutEmail.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ShortcutEmail.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.ShortcutEmail.Location = New System.Drawing.Point(160, 246)
        Me.ShortcutEmail.Name = "Cat_Email"
        Me.ShortcutEmail.Size = New System.Drawing.Size(126, 34)
        Me.ShortcutEmail.TabIndex = 15
        Me.ShortcutEmail.Text = "Email"
        Me.ShortcutEmail.UseVisualStyleBackColor = False
        '
        'ShortcutNews
        '
        Me.ShortcutNews.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ShortcutNews.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.ShortcutNews.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ShortcutNews.Location = New System.Drawing.Point(292, 246)
        Me.ShortcutNews.Name = "Cat_News"
        Me.ShortcutNews.Size = New System.Drawing.Size(126, 34)
        Me.ShortcutNews.TabIndex = 14
        Me.ShortcutNews.Text = "News | Articles | Other"
        Me.ShortcutNews.UseVisualStyleBackColor = False
        '
        'ShortcutUnprocessed
        '
        Me.ShortcutUnprocessed.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ShortcutUnprocessed.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.ShortcutUnprocessed.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ShortcutUnprocessed.Location = New System.Drawing.Point(424, 246)
        Me.ShortcutUnprocessed.Name = "Cat_Unprocessed"
        Me.ShortcutUnprocessed.Size = New System.Drawing.Size(126, 34)
        Me.ShortcutUnprocessed.TabIndex = 13
        Me.ShortcutUnprocessed.Text = "Unprocessed > 2min"
        Me.ShortcutUnprocessed.UseVisualStyleBackColor = False
        '
        'ShortcutReadingBusiness
        '
        Me.ShortcutReadingBusiness.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ShortcutReadingBusiness.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.ShortcutReadingBusiness.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ShortcutReadingBusiness.Location = New System.Drawing.Point(424, 206)
        Me.ShortcutReadingBusiness.Name = "Cat_ReadingBusiness"
        Me.ShortcutReadingBusiness.Size = New System.Drawing.Size(126, 34)
        Me.ShortcutReadingBusiness.TabIndex = 12
        Me.ShortcutReadingBusiness.Text = "Reading - Business"
        Me.ShortcutReadingBusiness.UseVisualStyleBackColor = False
        '
        'ShortcutCalls
        '
        Me.ShortcutCalls.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ShortcutCalls.BackColor = System.Drawing.Color.Blue
        Me.ShortcutCalls.ForeColor = System.Drawing.SystemColors.ButtonFace
        Me.ShortcutCalls.Location = New System.Drawing.Point(292, 206)
        Me.ShortcutCalls.Name = "Cat_Calls"
        Me.ShortcutCalls.Size = New System.Drawing.Size(126, 34)
        Me.ShortcutCalls.TabIndex = 11
        Me.ShortcutCalls.Text = "Calls"
        Me.ShortcutCalls.UseVisualStyleBackColor = False
        '
        'ShortcutInternet
        '
        Me.ShortcutInternet.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ShortcutInternet.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.ShortcutInternet.Location = New System.Drawing.Point(160, 206)
        Me.ShortcutInternet.Name = "Cat_Internet"
        Me.ShortcutInternet.Size = New System.Drawing.Size(126, 34)
        Me.ShortcutInternet.TabIndex = 10
        Me.ShortcutInternet.Text = "Internet"
        Me.ShortcutInternet.UseVisualStyleBackColor = False
        '
        'ShortcutPreRead
        '
        Me.ShortcutPreRead.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ShortcutPreRead.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.ShortcutPreRead.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ShortcutPreRead.Location = New System.Drawing.Point(424, 166)
        Me.ShortcutPreRead.Name = "Cat_PreRead"
        Me.ShortcutPreRead.Size = New System.Drawing.Size(126, 34)
        Me.ShortcutPreRead.TabIndex = 9
        Me.ShortcutPreRead.Text = "PreRead"
        Me.ShortcutPreRead.UseVisualStyleBackColor = False
        '
        'ShortcutMeeting
        '
        Me.ShortcutMeeting.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ShortcutMeeting.BackColor = System.Drawing.Color.Blue
        Me.ShortcutMeeting.ForeColor = System.Drawing.SystemColors.ButtonFace
        Me.ShortcutMeeting.Location = New System.Drawing.Point(292, 166)
        Me.ShortcutMeeting.Name = "Cat_Meeting"
        Me.ShortcutMeeting.Size = New System.Drawing.Size(126, 34)
        Me.ShortcutMeeting.TabIndex = 8
        Me.ShortcutMeeting.Text = "Meeting"
        Me.ShortcutMeeting.UseVisualStyleBackColor = False
        '
        'LblTopic
        '
        Me.LblTopic.AutoSize = True
        Me.LblTopic.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTopic.Location = New System.Drawing.Point(25, 134)
        Me.LblTopic.Name = "lbl_topic"
        Me.LblTopic.Size = New System.Drawing.Size(73, 16)
        Me.LblTopic.TabIndex = 7
        Me.LblTopic.Text = "Topic Tag:"
        '
        'ShortcutPersonal
        '
        Me.ShortcutPersonal.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ShortcutPersonal.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.ShortcutPersonal.Location = New System.Drawing.Point(160, 166)
        Me.ShortcutPersonal.Name = "Cat_Personal"
        Me.ShortcutPersonal.Size = New System.Drawing.Size(126, 34)
        Me.ShortcutPersonal.TabIndex = 1
        Me.ShortcutPersonal.Text = "PERSONAL"
        Me.ShortcutPersonal.UseVisualStyleBackColor = False
        '
        'LblProject
        '
        Me.LblProject.AutoSize = True
        Me.LblProject.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblProject.Location = New System.Drawing.Point(25, 98)
        Me.LblProject.Name = "lbl_project"
        Me.LblProject.Size = New System.Drawing.Size(82, 16)
        Me.LblProject.TabIndex = 6
        Me.LblProject.Text = "Project Flag:"
        '
        'LblPeople
        '
        Me.LblPeople.AutoSize = True
        Me.LblPeople.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblPeople.Location = New System.Drawing.Point(25, 61)
        Me.LblPeople.Name = "lbl_people"
        Me.LblPeople.Size = New System.Drawing.Size(84, 16)
        Me.LblPeople.TabIndex = 5
        Me.LblPeople.Text = "People Flag:"
        '
        'LblContext
        '
        Me.LblContext.AutoSize = True
        Me.LblContext.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblContext.Location = New System.Drawing.Point(25, 25)
        Me.LblContext.Name = "lbl_context"
        Me.LblContext.Size = New System.Drawing.Size(84, 16)
        Me.LblContext.TabIndex = 4
        Me.LblContext.Text = "Context Flag:"
        '
        'TopicSelection
        '
        Me.TopicSelection.BackColor = System.Drawing.SystemColors.Window
        Me.TopicSelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.TopicSelection.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TopicSelection.Location = New System.Drawing.Point(160, 133)
        Me.TopicSelection.Name = "topic_selection"
        Me.TopicSelection.Size = New System.Drawing.Size(390, 24)
        Me.TopicSelection.TabIndex = 3
        Me.TopicSelection.Text = "[Other Topics Flagged]"
        '
        'ProjectSelection
        '
        Me.ProjectSelection.BackColor = System.Drawing.SystemColors.Window
        Me.ProjectSelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.ProjectSelection.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ProjectSelection.Location = New System.Drawing.Point(160, 97)
        Me.ProjectSelection.Name = "project_selection"
        Me.ProjectSelection.Size = New System.Drawing.Size(390, 24)
        Me.ProjectSelection.TabIndex = 2
        Me.ProjectSelection.Text = "[Projects Flagged]"
        '
        'PeopleSelection
        '
        Me.PeopleSelection.BackColor = System.Drawing.SystemColors.Window
        Me.PeopleSelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.PeopleSelection.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.PeopleSelection.Location = New System.Drawing.Point(160, 60)
        Me.PeopleSelection.Name = "people_selection"
        Me.PeopleSelection.Size = New System.Drawing.Size(390, 24)
        Me.PeopleSelection.TabIndex = 1
        Me.PeopleSelection.Text = "[Assigned People Flagged]"
        '
        'CategorySelection
        '
        Me.CategorySelection.BackColor = System.Drawing.SystemColors.Window
        Me.CategorySelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.CategorySelection.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CategorySelection.Location = New System.Drawing.Point(160, 24)
        Me.CategorySelection.Name = "category_selection"
        Me.CategorySelection.Size = New System.Drawing.Size(390, 24)
        Me.CategorySelection.TabIndex = 0
        Me.CategorySelection.Text = "[Category Label]"
        '
        'OKButton
        '
        Me.OKButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.OKButton.Location = New System.Drawing.Point(138, 530)
        Me.OKButton.Name = "OK_Button"
        Me.OKButton.Size = New System.Drawing.Size(145, 57)
        Me.OKButton.TabIndex = 1
        Me.OKButton.Text = "OK"
        Me.OKButton.UseVisualStyleBackColor = True
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
        'LblTaskname
        '
        Me.LblTaskname.AutoSize = True
        Me.LblTaskname.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTaskname.Location = New System.Drawing.Point(12, 23)
        Me.LblTaskname.Name = "lbl_taskname"
        Me.LblTaskname.Size = New System.Drawing.Size(97, 16)
        Me.LblTaskname.TabIndex = 1
        Me.LblTaskname.Text = "Name Of Task:"
        '
        'TaskName
        '
        Me.TaskName.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TaskName.ForeColor = System.Drawing.SystemColors.WindowText
        Me.TaskName.Location = New System.Drawing.Point(12, 42)
        Me.TaskName.Name = "task_name"
        Me.TaskName.Size = New System.Drawing.Size(560, 22)
        Me.TaskName.TabIndex = 2
        '
        'LblPriority
        '
        Me.LblPriority.AutoSize = True
        Me.LblPriority.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblPriority.Location = New System.Drawing.Point(12, 83)
        Me.LblPriority.Name = "lbl_priority"
        Me.LblPriority.Size = New System.Drawing.Size(77, 16)
        Me.LblPriority.TabIndex = 3
        Me.LblPriority.Text = "Importance:"
        '
        'LblKbf
        '
        Me.LblKbf.AutoSize = True
        Me.LblKbf.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblKbf.Location = New System.Drawing.Point(12, 111)
        Me.LblKbf.Name = "lbl_kbf"
        Me.LblKbf.Size = New System.Drawing.Size(56, 16)
        Me.LblKbf.TabIndex = 5
        Me.LblKbf.Text = "Kanban:"
        '
        'LblDuration
        '
        Me.LblDuration.AutoSize = True
        Me.LblDuration.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblDuration.Location = New System.Drawing.Point(12, 144)
        Me.LblDuration.Name = "lbl_duration"
        Me.LblDuration.Size = New System.Drawing.Size(76, 16)
        Me.LblDuration.TabIndex = 7
        Me.LblDuration.Text = "Work Time:"
        '
        'PriorityBox
        '
        Me.PriorityBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.PriorityBox.FormattingEnabled = True
        Me.PriorityBox.Items.AddRange(New Object() {"High", "Normal", "Low"})
        Me.PriorityBox.Location = New System.Drawing.Point(120, 82)
        Me.PriorityBox.Name = "Priority_Box"
        Me.PriorityBox.Size = New System.Drawing.Size(121, 21)
        Me.PriorityBox.TabIndex = 4
        '
        'KbSelector
        '
        Me.KbSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.KbSelector.FormattingEnabled = True
        Me.KbSelector.Items.AddRange(New Object() {"Backlog", "Planned", "InProgress", "Complete"})
        Me.KbSelector.Location = New System.Drawing.Point(120, 111)
        Me.KbSelector.Name = "kb_selector"
        Me.KbSelector.Size = New System.Drawing.Size(121, 21)
        Me.KbSelector.TabIndex = 6
        '
        'Duration
        '
        Me.Duration.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Duration.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Duration.Location = New System.Drawing.Point(120, 141)
        Me.Duration.Name = "duration"
        Me.Duration.Size = New System.Drawing.Size(121, 22)
        Me.Duration.TabIndex = 8
        '
        'DtDuedate
        '
        Me.DtDuedate.Checked = False
        Me.DtDuedate.CustomFormat = "MM/dd/yyyy hh:mm tt"
        Me.DtDuedate.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.DtDuedate.Location = New System.Drawing.Point(388, 83)
        Me.DtDuedate.Name = "dt_duedate"
        Me.DtDuedate.ShowCheckBox = True
        Me.DtDuedate.Size = New System.Drawing.Size(184, 20)
        Me.DtDuedate.TabIndex = 10
        '
        'LblDuedate
        '
        Me.LblDuedate.AutoSize = True
        Me.LblDuedate.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblDuedate.Location = New System.Drawing.Point(310, 83)
        Me.LblDuedate.Name = "lbl_duedate"
        Me.LblDuedate.Size = New System.Drawing.Size(67, 16)
        Me.LblDuedate.TabIndex = 9
        Me.LblDuedate.Text = "Due Date:"
        '
        'LblReminder
        '
        Me.LblReminder.AutoSize = True
        Me.LblReminder.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblReminder.Location = New System.Drawing.Point(310, 114)
        Me.LblReminder.Name = "lbl_reminder"
        Me.LblReminder.Size = New System.Drawing.Size(69, 16)
        Me.LblReminder.TabIndex = 11
        Me.LblReminder.Text = "Reminder:"
        '
        'DtReminder
        '
        Me.DtReminder.Checked = False
        Me.DtReminder.CustomFormat = "MM/dd/yyyy hh:mm tt"
        Me.DtReminder.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.DtReminder.Location = New System.Drawing.Point(388, 112)
        Me.DtReminder.Name = "dt_reminder"
        Me.DtReminder.ShowCheckBox = True
        Me.DtReminder.Size = New System.Drawing.Size(184, 20)
        Me.DtReminder.TabIndex = 12
        '
        'XlTaskname
        '
        Me.XlTaskname.AutoSize = True
        Me.XlTaskname.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlTaskname.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlTaskname.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlTaskname.Location = New System.Drawing.Point(7, 23)
        Me.XlTaskname.Name = "xl_taskname"
        Me.XlTaskname.Size = New System.Drawing.Size(18, 16)
        Me.XlTaskname.TabIndex = 13
        Me.XlTaskname.Text = "N"
        '
        'XlImportance
        '
        Me.XlImportance.AutoSize = True
        Me.XlImportance.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlImportance.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlImportance.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlImportance.Location = New System.Drawing.Point(6, 82)
        Me.XlImportance.Name = "xl_importance"
        Me.XlImportance.Size = New System.Drawing.Size(11, 16)
        Me.XlImportance.TabIndex = 14
        Me.XlImportance.Text = "I"
        '
        'XlKanban
        '
        Me.XlKanban.AutoSize = True
        Me.XlKanban.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlKanban.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlKanban.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlKanban.Location = New System.Drawing.Point(6, 111)
        Me.XlKanban.Name = "xl_kanban"
        Me.XlKanban.Size = New System.Drawing.Size(16, 16)
        Me.XlKanban.TabIndex = 15
        Me.XlKanban.Text = "K"
        '
        'XlWorktime
        '
        Me.XlWorktime.AutoSize = True
        Me.XlWorktime.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlWorktime.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlWorktime.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlWorktime.Location = New System.Drawing.Point(6, 143)
        Me.XlWorktime.Name = "xl_worktime"
        Me.XlWorktime.Size = New System.Drawing.Size(21, 16)
        Me.XlWorktime.TabIndex = 16
        Me.XlWorktime.Text = "W"
        '
        'XlOk
        '
        Me.XlOk.AutoSize = True
        Me.XlOk.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlOk.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlOk.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlOk.Location = New System.Drawing.Point(191, 550)
        Me.XlOk.Name = "xl_ok"
        Me.XlOk.Size = New System.Drawing.Size(18, 16)
        Me.XlOk.TabIndex = 23
        Me.XlOk.Text = "O"
        '
        'XlCancel
        '
        Me.XlCancel.AutoSize = True
        Me.XlCancel.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlCancel.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlCancel.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlCancel.Location = New System.Drawing.Point(341, 550)
        Me.XlCancel.Name = "xl_cancel"
        Me.XlCancel.Size = New System.Drawing.Size(17, 16)
        Me.XlCancel.TabIndex = 24
        Me.XlCancel.Text = "C"
        '
        'XlReminder
        '
        Me.XlReminder.AutoSize = True
        Me.XlReminder.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlReminder.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlReminder.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlReminder.Location = New System.Drawing.Point(305, 114)
        Me.XlReminder.Name = "xl_reminder"
        Me.XlReminder.Size = New System.Drawing.Size(18, 16)
        Me.XlReminder.TabIndex = 25
        Me.XlReminder.Text = "R"
        '
        'XlDuedate
        '
        Me.XlDuedate.AutoSize = True
        Me.XlDuedate.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.XlDuedate.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XlDuedate.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.XlDuedate.Location = New System.Drawing.Point(305, 82)
        Me.XlDuedate.Name = "xl_duedate"
        Me.XlDuedate.Size = New System.Drawing.Size(18, 16)
        Me.XlDuedate.TabIndex = 26
        Me.XlDuedate.Text = "D"
        '
        'TaskViewer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(584, 611)
        Me.Controls.Add(Me.XlDuedate)
        Me.Controls.Add(Me.XlReminder)
        Me.Controls.Add(Me.XlCancel)
        Me.Controls.Add(Me.XlOk)
        Me.Controls.Add(Me.XlWorktime)
        Me.Controls.Add(Me.XlKanban)
        Me.Controls.Add(Me.XlImportance)
        Me.Controls.Add(Me.XlTaskname)
        Me.Controls.Add(Me.DtReminder)
        Me.Controls.Add(Me.LblReminder)
        Me.Controls.Add(Me.LblDuedate)
        Me.Controls.Add(Me.DtDuedate)
        Me.Controls.Add(Me.Duration)
        Me.Controls.Add(Me.KbSelector)
        Me.Controls.Add(Me.PriorityBox)
        Me.Controls.Add(Me.LblDuration)
        Me.Controls.Add(Me.LblKbf)
        Me.Controls.Add(Me.LblPriority)
        Me.Controls.Add(Me.TaskName)
        Me.Controls.Add(Me.LblTaskname)
        Me.Controls.Add(Me.Cancel_Button)
        Me.Controls.Add(Me.OKButton)
        Me.Controls.Add(Me.Frame1)
        Me.Name = "TaskViewer"
        Me.Text = "Change Flagged Email Into Task"
        Me.Frame1.ResumeLayout(False)
        Me.Frame1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Frame1 As Windows.Forms.Panel
    Friend WithEvents LblTopic As Windows.Forms.Label
    Friend WithEvents LblProject As Windows.Forms.Label
    Friend WithEvents LblPeople As Windows.Forms.Label
    Friend WithEvents LblContext As Windows.Forms.Label
    Friend WithEvents TopicSelection As Windows.Forms.Label
    Friend WithEvents ProjectSelection As Windows.Forms.Label
    Friend WithEvents PeopleSelection As Windows.Forms.Label
    Friend WithEvents CategorySelection As Windows.Forms.Label
    Friend WithEvents OKButton As Windows.Forms.Button
    Friend WithEvents Cancel_Button As Windows.Forms.Button
    Friend WithEvents ShortcutNews As Windows.Forms.Button
    Friend WithEvents ShortcutUnprocessed As Windows.Forms.Button
    Friend WithEvents ShortcutReadingBusiness As Windows.Forms.Button
    Friend WithEvents ShortcutCalls As Windows.Forms.Button
    Friend WithEvents ShortcutInternet As Windows.Forms.Button
    Friend WithEvents ShortcutPreRead As Windows.Forms.Button
    Friend WithEvents ShortcutMeeting As Windows.Forms.Button
    Friend WithEvents ShortcutPersonal As Windows.Forms.Button
    Friend WithEvents ShortcutEmail As Windows.Forms.Button
    Friend WithEvents ShortcutWaitingFor As Windows.Forms.Button
    Friend WithEvents CbxBullpin As Windows.Forms.CheckBox
    Friend WithEvents CbxToday As Windows.Forms.CheckBox
    Friend WithEvents CbxFlagAsTask As Windows.Forms.CheckBox
    Friend WithEvents LblTaskname As Windows.Forms.Label
    Friend WithEvents TaskName As Windows.Forms.TextBox
    Friend WithEvents LblPriority As Windows.Forms.Label
    Friend WithEvents LblKbf As Windows.Forms.Label
    Friend WithEvents LblDuration As Windows.Forms.Label
    Friend WithEvents PriorityBox As Windows.Forms.ComboBox
    Friend WithEvents KbSelector As Windows.Forms.ComboBox
    Friend WithEvents Duration As Windows.Forms.TextBox
    Friend WithEvents DtDuedate As Windows.Forms.DateTimePicker
    Friend WithEvents LblDuedate As Windows.Forms.Label
    Friend WithEvents LblReminder As Windows.Forms.Label
    Friend WithEvents DtReminder As Windows.Forms.DateTimePicker
    Friend WithEvents XlTopic As Windows.Forms.Label
    Friend WithEvents XlProject As Windows.Forms.Label
    Friend WithEvents XlPeople As Windows.Forms.Label
    Friend WithEvents XlContext As Windows.Forms.Label
    Friend WithEvents XlTaskname As Windows.Forms.Label
    Friend WithEvents XlImportance As Windows.Forms.Label
    Friend WithEvents XlKanban As Windows.Forms.Label
    Friend WithEvents XlWorktime As Windows.Forms.Label
    Friend WithEvents XlOk As Windows.Forms.Label
    Friend WithEvents XlCancel As Windows.Forms.Label
    Friend WithEvents XlReminder As Windows.Forms.Label
    Friend WithEvents XlDuedate As Windows.Forms.Label
    Friend WithEvents XlScBullpin As Windows.Forms.Label
    Friend WithEvents XlScToday As Windows.Forms.Label
    Friend WithEvents XlScWaiting As Windows.Forms.Label
    Friend WithEvents XlScUnprocessed As Windows.Forms.Label
    Friend WithEvents XlScNews As Windows.Forms.Label
    Friend WithEvents XlScEmail As Windows.Forms.Label
    Friend WithEvents XlScReadingbusiness As Windows.Forms.Label
    Friend WithEvents XlScCalls As Windows.Forms.Label
    Friend WithEvents XlScInternet As Windows.Forms.Label
    Friend WithEvents XlScPreread As Windows.Forms.Label
    Friend WithEvents XlScMeeting As Windows.Forms.Label
    Friend WithEvents XlScPersonal As Windows.Forms.Label
End Class
