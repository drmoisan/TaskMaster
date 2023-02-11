<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class NewEMailAsTask
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
        Me.Category_Selection = New System.Windows.Forms.Label()
        Me.People_Selection = New System.Windows.Forms.Label()
        Me.Project_Selection = New System.Windows.Forms.Label()
        Me.Topic_Selection = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.OK_Button = New System.Windows.Forms.Button()
        Me.Cancel_Button = New System.Windows.Forms.Button()
        Me.Cat_Deskwork = New System.Windows.Forms.Button()
        Me.Cat_Agenda = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Cat_ReadingBusiness = New System.Windows.Forms.Button()
        Me.Cat_Calls = New System.Windows.Forms.Button()
        Me.Cat_Internet = New System.Windows.Forms.Button()
        Me.Cat_Unprocessed = New System.Windows.Forms.Button()
        Me.Cat_ReadingOther = New System.Windows.Forms.Button()
        Me.Cat_Email = New System.Windows.Forms.Button()
        Me.cbxFlag = New System.Windows.Forms.CheckBox()
        Me.cbxTODAY = New System.Windows.Forms.CheckBox()
        Me.cbxBULLPIN = New System.Windows.Forms.CheckBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Task_Name = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Priority_Box = New System.Windows.Forms.ComboBox()
        Me.KB_Selector = New System.Windows.Forms.ComboBox()
        Me.Duration = New System.Windows.Forms.TextBox()
        Me.DT_DueDate = New System.Windows.Forms.DateTimePicker()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.DT_Reminder = New System.Windows.Forms.DateTimePicker()
        Me.Cat_WaitingFor = New System.Windows.Forms.Button()
        Me.Frame1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Frame1
        '
        Me.Frame1.Controls.Add(Me.Cat_WaitingFor)
        Me.Frame1.Controls.Add(Me.cbxBULLPIN)
        Me.Frame1.Controls.Add(Me.cbxTODAY)
        Me.Frame1.Controls.Add(Me.cbxFlag)
        Me.Frame1.Controls.Add(Me.Cat_Email)
        Me.Frame1.Controls.Add(Me.Cat_ReadingOther)
        Me.Frame1.Controls.Add(Me.Cat_Unprocessed)
        Me.Frame1.Controls.Add(Me.Cat_ReadingBusiness)
        Me.Frame1.Controls.Add(Me.Cat_Calls)
        Me.Frame1.Controls.Add(Me.Cat_Internet)
        Me.Frame1.Controls.Add(Me.Button1)
        Me.Frame1.Controls.Add(Me.Cat_Agenda)
        Me.Frame1.Controls.Add(Me.Label4)
        Me.Frame1.Controls.Add(Me.Cat_Deskwork)
        Me.Frame1.Controls.Add(Me.Label3)
        Me.Frame1.Controls.Add(Me.Label2)
        Me.Frame1.Controls.Add(Me.Label1)
        Me.Frame1.Controls.Add(Me.Topic_Selection)
        Me.Frame1.Controls.Add(Me.Project_Selection)
        Me.Frame1.Controls.Add(Me.People_Selection)
        Me.Frame1.Controls.Add(Me.Category_Selection)
        Me.Frame1.Location = New System.Drawing.Point(7, 186)
        Me.Frame1.Name = "Frame1"
        Me.Frame1.Size = New System.Drawing.Size(570, 322)
        Me.Frame1.TabIndex = 0
        '
        'Category_Selection
        '
        Me.Category_Selection.BackColor = System.Drawing.SystemColors.Window
        Me.Category_Selection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Category_Selection.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Category_Selection.Location = New System.Drawing.Point(160, 24)
        Me.Category_Selection.Name = "Category_Selection"
        Me.Category_Selection.Size = New System.Drawing.Size(390, 24)
        Me.Category_Selection.TabIndex = 0
        Me.Category_Selection.Text = "[Category Label]"
        '
        'People_Selection
        '
        Me.People_Selection.BackColor = System.Drawing.SystemColors.Window
        Me.People_Selection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.People_Selection.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.People_Selection.Location = New System.Drawing.Point(160, 60)
        Me.People_Selection.Name = "People_Selection"
        Me.People_Selection.Size = New System.Drawing.Size(390, 24)
        Me.People_Selection.TabIndex = 1
        Me.People_Selection.Text = "[Assigned People Flagged]"
        '
        'Project_Selection
        '
        Me.Project_Selection.BackColor = System.Drawing.SystemColors.Window
        Me.Project_Selection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Project_Selection.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Project_Selection.Location = New System.Drawing.Point(160, 97)
        Me.Project_Selection.Name = "Project_Selection"
        Me.Project_Selection.Size = New System.Drawing.Size(390, 24)
        Me.Project_Selection.TabIndex = 2
        Me.Project_Selection.Text = "[Projects Flagged]"
        '
        'Topic_Selection
        '
        Me.Topic_Selection.BackColor = System.Drawing.SystemColors.Window
        Me.Topic_Selection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Topic_Selection.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Topic_Selection.Location = New System.Drawing.Point(160, 133)
        Me.Topic_Selection.Name = "Topic_Selection"
        Me.Topic_Selection.Size = New System.Drawing.Size(390, 24)
        Me.Topic_Selection.TabIndex = 3
        Me.Topic_Selection.Text = "[Other Topics Flagged]"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(10, 25)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(54, 16)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "Context:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(10, 61)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(87, 16)
        Me.Label2.TabIndex = 5
        Me.Label2.Text = "Assigned To:"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(10, 98)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(82, 16)
        Me.Label3.TabIndex = 6
        Me.Label3.Text = "Project Flag:"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.Location = New System.Drawing.Point(10, 134)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(73, 16)
        Me.Label4.TabIndex = 7
        Me.Label4.Text = "Topic Tag:"
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
        'Button1
        '
        Me.Button1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Button1.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Button1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Button1.Location = New System.Drawing.Point(424, 166)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(126, 34)
        Me.Button1.TabIndex = 9
        Me.Button1.Text = "@ Meeting"
        Me.Button1.UseVisualStyleBackColor = False
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
        'cbxTODAY
        '
        Me.cbxTODAY.AutoSize = True
        Me.cbxTODAY.Location = New System.Drawing.Point(292, 290)
        Me.cbxTODAY.Name = "cbxTODAY"
        Me.cbxTODAY.Size = New System.Drawing.Size(110, 17)
        Me.cbxTODAY.TabIndex = 17
        Me.cbxTODAY.Text = "Complete TODAY"
        Me.cbxTODAY.UseVisualStyleBackColor = True
        '
        'cbxBULLPIN
        '
        Me.cbxBULLPIN.AutoSize = True
        Me.cbxBULLPIN.Location = New System.Drawing.Point(424, 290)
        Me.cbxBULLPIN.Name = "cbxBULLPIN"
        Me.cbxBULLPIN.Size = New System.Drawing.Size(113, 17)
        Me.cbxBULLPIN.TabIndex = 18
        Me.cbxBULLPIN.Text = "BULLPIN Priorities"
        Me.cbxBULLPIN.UseVisualStyleBackColor = True
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.Location = New System.Drawing.Point(12, 23)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(97, 16)
        Me.Label5.TabIndex = 5
        Me.Label5.Text = "Name Of Task:"
        '
        'Task_Name
        '
        Me.Task_Name.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Task_Name.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Task_Name.Location = New System.Drawing.Point(12, 42)
        Me.Task_Name.Name = "Task_Name"
        Me.Task_Name.Size = New System.Drawing.Size(560, 22)
        Me.Task_Name.TabIndex = 6
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(12, 83)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(77, 16)
        Me.Label6.TabIndex = 7
        Me.Label6.Text = "Importance:"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.Location = New System.Drawing.Point(12, 111)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(56, 16)
        Me.Label7.TabIndex = 8
        Me.Label7.Text = "Kanban:"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.Location = New System.Drawing.Point(12, 144)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(60, 16)
        Me.Label8.TabIndex = 9
        Me.Label8.Text = "Duration:"
        '
        'Priority_Box
        '
        Me.Priority_Box.FormattingEnabled = True
        Me.Priority_Box.Location = New System.Drawing.Point(120, 82)
        Me.Priority_Box.Name = "Priority_Box"
        Me.Priority_Box.Size = New System.Drawing.Size(121, 21)
        Me.Priority_Box.TabIndex = 10
        '
        'KB_Selector
        '
        Me.KB_Selector.FormattingEnabled = True
        Me.KB_Selector.Location = New System.Drawing.Point(120, 111)
        Me.KB_Selector.Name = "KB_Selector"
        Me.KB_Selector.Size = New System.Drawing.Size(121, 21)
        Me.KB_Selector.TabIndex = 11
        '
        'Duration
        '
        Me.Duration.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Duration.ForeColor = System.Drawing.SystemColors.WindowText
        Me.Duration.Location = New System.Drawing.Point(120, 141)
        Me.Duration.Name = "Duration"
        Me.Duration.Size = New System.Drawing.Size(121, 22)
        Me.Duration.TabIndex = 12
        '
        'DT_DueDate
        '
        Me.DT_DueDate.CustomFormat = "MM/dd/yyyy hh:mm tt"
        Me.DT_DueDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.DT_DueDate.Location = New System.Drawing.Point(388, 83)
        Me.DT_DueDate.Name = "DT_DueDate"
        Me.DT_DueDate.ShowCheckBox = True
        Me.DT_DueDate.Size = New System.Drawing.Size(184, 20)
        Me.DT_DueDate.TabIndex = 13
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label9.Location = New System.Drawing.Point(310, 83)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(67, 16)
        Me.Label9.TabIndex = 14
        Me.Label9.Text = "Due Date:"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label10.Location = New System.Drawing.Point(310, 114)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(69, 16)
        Me.Label10.TabIndex = 15
        Me.Label10.Text = "Reminder:"
        '
        'DT_Reminder
        '
        Me.DT_Reminder.CustomFormat = "MM/dd/yyyy hh:mm tt"
        Me.DT_Reminder.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.DT_Reminder.Location = New System.Drawing.Point(388, 112)
        Me.DT_Reminder.Name = "DT_Reminder"
        Me.DT_Reminder.ShowCheckBox = True
        Me.DT_Reminder.Size = New System.Drawing.Size(184, 20)
        Me.DT_Reminder.TabIndex = 16
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
        'NewEMailAsTask
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(584, 611)
        Me.Controls.Add(Me.DT_Reminder)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.DT_DueDate)
        Me.Controls.Add(Me.Duration)
        Me.Controls.Add(Me.KB_Selector)
        Me.Controls.Add(Me.Priority_Box)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Task_Name)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Cancel_Button)
        Me.Controls.Add(Me.OK_Button)
        Me.Controls.Add(Me.Frame1)
        Me.Name = "NewEMailAsTask"
        Me.Text = "Change Flagged Email Into Task"
        Me.Frame1.ResumeLayout(False)
        Me.Frame1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Frame1 As Windows.Forms.Panel
    Friend WithEvents Label4 As Windows.Forms.Label
    Friend WithEvents Label3 As Windows.Forms.Label
    Friend WithEvents Label2 As Windows.Forms.Label
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents Topic_Selection As Windows.Forms.Label
    Friend WithEvents Project_Selection As Windows.Forms.Label
    Friend WithEvents People_Selection As Windows.Forms.Label
    Friend WithEvents Category_Selection As Windows.Forms.Label
    Friend WithEvents OK_Button As Windows.Forms.Button
    Friend WithEvents Cancel_Button As Windows.Forms.Button
    Friend WithEvents Cat_ReadingOther As Windows.Forms.Button
    Friend WithEvents Cat_Unprocessed As Windows.Forms.Button
    Friend WithEvents Cat_ReadingBusiness As Windows.Forms.Button
    Friend WithEvents Cat_Calls As Windows.Forms.Button
    Friend WithEvents Cat_Internet As Windows.Forms.Button
    Friend WithEvents Button1 As Windows.Forms.Button
    Friend WithEvents Cat_Agenda As Windows.Forms.Button
    Friend WithEvents Cat_Deskwork As Windows.Forms.Button
    Friend WithEvents Cat_Email As Windows.Forms.Button
    Friend WithEvents Cat_WaitingFor As Windows.Forms.Button
    Friend WithEvents cbxBULLPIN As Windows.Forms.CheckBox
    Friend WithEvents cbxTODAY As Windows.Forms.CheckBox
    Friend WithEvents cbxFlag As Windows.Forms.CheckBox
    Friend WithEvents Label5 As Windows.Forms.Label
    Friend WithEvents Task_Name As Windows.Forms.TextBox
    Friend WithEvents Label6 As Windows.Forms.Label
    Friend WithEvents Label7 As Windows.Forms.Label
    Friend WithEvents Label8 As Windows.Forms.Label
    Friend WithEvents Priority_Box As Windows.Forms.ComboBox
    Friend WithEvents KB_Selector As Windows.Forms.ComboBox
    Friend WithEvents Duration As Windows.Forms.TextBox
    Friend WithEvents DT_DueDate As Windows.Forms.DateTimePicker
    Friend WithEvents Label9 As Windows.Forms.Label
    Friend WithEvents Label10 As Windows.Forms.Label
    Friend WithEvents DT_Reminder As Windows.Forms.DateTimePicker
End Class
