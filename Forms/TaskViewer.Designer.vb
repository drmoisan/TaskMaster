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
        Me.Cat_WaitingFor = New System.Windows.Forms.Button()
        Me.cbx_bullpin = New System.Windows.Forms.CheckBox()
        Me.cbx_today = New System.Windows.Forms.CheckBox()
        Me.cbxFlag = New System.Windows.Forms.CheckBox()
        Me.Cat_Email = New System.Windows.Forms.Button()
        Me.Cat_ReadingOther = New System.Windows.Forms.Button()
        Me.Cat_Unprocessed = New System.Windows.Forms.Button()
        Me.Cat_ReadingBusiness = New System.Windows.Forms.Button()
        Me.Cat_Calls = New System.Windows.Forms.Button()
        Me.Cat_Internet = New System.Windows.Forms.Button()
        Me.Cat_PreRead = New System.Windows.Forms.Button()
        Me.Cat_Agenda = New System.Windows.Forms.Button()
        Me.lbl_topic = New System.Windows.Forms.Label()
        Me.Cat_Deskwork = New System.Windows.Forms.Button()
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
        Me.Frame1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Frame1
        '
        Me.Frame1.Controls.Add(Me.Cat_WaitingFor)
        Me.Frame1.Controls.Add(Me.cbx_bullpin)
        Me.Frame1.Controls.Add(Me.cbx_today)
        Me.Frame1.Controls.Add(Me.cbxFlag)
        Me.Frame1.Controls.Add(Me.Cat_Email)
        Me.Frame1.Controls.Add(Me.Cat_ReadingOther)
        Me.Frame1.Controls.Add(Me.Cat_Unprocessed)
        Me.Frame1.Controls.Add(Me.Cat_ReadingBusiness)
        Me.Frame1.Controls.Add(Me.Cat_Calls)
        Me.Frame1.Controls.Add(Me.Cat_Internet)
        Me.Frame1.Controls.Add(Me.Cat_PreRead)
        Me.Frame1.Controls.Add(Me.Cat_Agenda)
        Me.Frame1.Controls.Add(Me.lbl_topic)
        Me.Frame1.Controls.Add(Me.Cat_Deskwork)
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
        Me.Cat_Email.Text = "@ Email"
        Me.Cat_Email.UseVisualStyleBackColor = False
        '
        'Cat_ReadingOther
        '
        Me.Cat_ReadingOther.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Cat_ReadingOther.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Cat_ReadingOther.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Cat_ReadingOther.Location = New System.Drawing.Point(292, 246)
        Me.Cat_ReadingOther.Name = "Cat_ReadingOther"
        Me.Cat_ReadingOther.Size = New System.Drawing.Size(126, 34)
        Me.Cat_ReadingOther.TabIndex = 14
        Me.Cat_ReadingOther.Text = "News | Articles | Other"
        Me.Cat_ReadingOther.UseVisualStyleBackColor = False
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
        Me.Cat_Calls.Text = "@Calls"
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
        Me.Cat_Internet.Text = "@Internet"
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
        'Cat_Agenda
        '
        Me.Cat_Agenda.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Cat_Agenda.BackColor = System.Drawing.Color.Blue
        Me.Cat_Agenda.ForeColor = System.Drawing.SystemColors.ButtonFace
        Me.Cat_Agenda.Location = New System.Drawing.Point(292, 166)
        Me.Cat_Agenda.Name = "Cat_Agenda"
        Me.Cat_Agenda.Size = New System.Drawing.Size(126, 34)
        Me.Cat_Agenda.TabIndex = 8
        Me.Cat_Agenda.Text = "@ Meeting"
        Me.Cat_Agenda.UseVisualStyleBackColor = False
        '
        'lbl_topic
        '
        Me.lbl_topic.AutoSize = True
        Me.lbl_topic.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_topic.Location = New System.Drawing.Point(10, 134)
        Me.lbl_topic.Name = "lbl_topic"
        Me.lbl_topic.Size = New System.Drawing.Size(73, 16)
        Me.lbl_topic.TabIndex = 7
        Me.lbl_topic.Text = "Topic Tag:"
        '
        'Cat_Deskwork
        '
        Me.Cat_Deskwork.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Cat_Deskwork.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.Cat_Deskwork.Location = New System.Drawing.Point(160, 166)
        Me.Cat_Deskwork.Name = "Cat_Deskwork"
        Me.Cat_Deskwork.Size = New System.Drawing.Size(126, 34)
        Me.Cat_Deskwork.TabIndex = 1
        Me.Cat_Deskwork.Text = "PERSONAL"
        Me.Cat_Deskwork.UseVisualStyleBackColor = False
        '
        'lbl_project
        '
        Me.lbl_project.AutoSize = True
        Me.lbl_project.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_project.Location = New System.Drawing.Point(10, 98)
        Me.lbl_project.Name = "lbl_project"
        Me.lbl_project.Size = New System.Drawing.Size(82, 16)
        Me.lbl_project.TabIndex = 6
        Me.lbl_project.Text = "Project Flag:"
        '
        'lbl_people
        '
        Me.lbl_people.AutoSize = True
        Me.lbl_people.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_people.Location = New System.Drawing.Point(10, 61)
        Me.lbl_people.Name = "lbl_people"
        Me.lbl_people.Size = New System.Drawing.Size(87, 16)
        Me.lbl_people.TabIndex = 5
        Me.lbl_people.Text = "Assigned To:"
        '
        'lbl_context
        '
        Me.lbl_context.AutoSize = True
        Me.lbl_context.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_context.Location = New System.Drawing.Point(10, 25)
        Me.lbl_context.Name = "lbl_context"
        Me.lbl_context.Size = New System.Drawing.Size(54, 16)
        Me.lbl_context.TabIndex = 4
        Me.lbl_context.Text = "Context:"
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
        Me.lbl_taskname.TabIndex = 5
        Me.lbl_taskname.Text = "Name Of Task:"
        '
        'task_name
        '
        Me.task_name.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.task_name.ForeColor = System.Drawing.SystemColors.WindowText
        Me.task_name.Location = New System.Drawing.Point(12, 42)
        Me.task_name.Name = "task_name"
        Me.task_name.Size = New System.Drawing.Size(560, 22)
        Me.task_name.TabIndex = 6
        '
        'lbl_priority
        '
        Me.lbl_priority.AutoSize = True
        Me.lbl_priority.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_priority.Location = New System.Drawing.Point(12, 83)
        Me.lbl_priority.Name = "lbl_priority"
        Me.lbl_priority.Size = New System.Drawing.Size(77, 16)
        Me.lbl_priority.TabIndex = 7
        Me.lbl_priority.Text = "Importance:"
        '
        'lbl_kbf
        '
        Me.lbl_kbf.AutoSize = True
        Me.lbl_kbf.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_kbf.Location = New System.Drawing.Point(12, 111)
        Me.lbl_kbf.Name = "lbl_kbf"
        Me.lbl_kbf.Size = New System.Drawing.Size(56, 16)
        Me.lbl_kbf.TabIndex = 8
        Me.lbl_kbf.Text = "Kanban:"
        '
        'lbl_duration
        '
        Me.lbl_duration.AutoSize = True
        Me.lbl_duration.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_duration.Location = New System.Drawing.Point(12, 144)
        Me.lbl_duration.Name = "lbl_duration"
        Me.lbl_duration.Size = New System.Drawing.Size(60, 16)
        Me.lbl_duration.TabIndex = 9
        Me.lbl_duration.Text = "Duration:"
        '
        'Priority_Box
        '
        Me.Priority_Box.FormattingEnabled = True
        Me.Priority_Box.Items.AddRange(New Object() {"High", "Normal", "Low"})
        Me.Priority_Box.Location = New System.Drawing.Point(120, 82)
        Me.Priority_Box.Name = "Priority_Box"
        Me.Priority_Box.Size = New System.Drawing.Size(121, 21)
        Me.Priority_Box.TabIndex = 10
        '
        'kb_selector
        '
        Me.kb_selector.FormattingEnabled = True
        Me.kb_selector.Items.AddRange(New Object() {"Backlog", "Planned", "InProgress", "Complete"})
        Me.kb_selector.Location = New System.Drawing.Point(120, 111)
        Me.kb_selector.Name = "kb_selector"
        Me.kb_selector.Size = New System.Drawing.Size(121, 21)
        Me.kb_selector.TabIndex = 11
        '
        'duration
        '
        Me.duration.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.duration.ForeColor = System.Drawing.SystemColors.WindowText
        Me.duration.Location = New System.Drawing.Point(120, 141)
        Me.duration.Name = "duration"
        Me.duration.Size = New System.Drawing.Size(121, 22)
        Me.duration.TabIndex = 12
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
        Me.dt_duedate.TabIndex = 13
        '
        'lbl_duedate
        '
        Me.lbl_duedate.AutoSize = True
        Me.lbl_duedate.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_duedate.Location = New System.Drawing.Point(310, 83)
        Me.lbl_duedate.Name = "lbl_duedate"
        Me.lbl_duedate.Size = New System.Drawing.Size(67, 16)
        Me.lbl_duedate.TabIndex = 14
        Me.lbl_duedate.Text = "Due Date:"
        '
        'lbl_reminder
        '
        Me.lbl_reminder.AutoSize = True
        Me.lbl_reminder.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_reminder.Location = New System.Drawing.Point(310, 114)
        Me.lbl_reminder.Name = "lbl_reminder"
        Me.lbl_reminder.Size = New System.Drawing.Size(69, 16)
        Me.lbl_reminder.TabIndex = 15
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
        Me.dt_reminder.TabIndex = 16
        '
        'TaskViewer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(584, 611)
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
    Friend WithEvents Cat_ReadingOther As Windows.Forms.Button
    Friend WithEvents Cat_Unprocessed As Windows.Forms.Button
    Friend WithEvents Cat_ReadingBusiness As Windows.Forms.Button
    Friend WithEvents Cat_Calls As Windows.Forms.Button
    Friend WithEvents Cat_Internet As Windows.Forms.Button
    Friend WithEvents Cat_PreRead As Windows.Forms.Button
    Friend WithEvents Cat_Agenda As Windows.Forms.Button
    Friend WithEvents Cat_Deskwork As Windows.Forms.Button
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
End Class
