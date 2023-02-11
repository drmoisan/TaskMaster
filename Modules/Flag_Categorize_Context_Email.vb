Imports Microsoft.Office.Interop.Outlook

Module Flag_Categorize_Context_Email
    Public Sub Flag_Task(Optional selItems As Collection = Nothing,
                         Optional blFile As Boolean = True,
                         Optional hWndCaller As IntPtr = Nothing,
                         Optional strNameOfFunctionCalling As String = "")

        'Procedure Naming
        Dim SubNm As String
        SubNm = "Flag_Task_Init"

        Const DefaultTaskTime = 15
        Dim obj As Object
        Dim OlMail As Outlook.MailItem
        Dim OlTask As Outlook.TaskItem
        Dim OlAppointment As Outlook.AppointmentItem
        Dim OlImportance As String
        Dim EmailForm As NewEMailAsTask
        Dim objProperty As Outlook.UserProperty
        Dim File_Boolean As Integer
        Dim strTemp As String
        Dim intTempLen As Integer
        Dim strTempCats() As String
        Dim strCats_All As String
        Dim strCats_People As String
        Dim strCats_Projects As String
        Dim strCats_Topics As String
        Dim strCats_TagOther As String
        Dim strCats_Other As String
        Dim strCats_KB As String
        Dim strSubject As String
        Dim dtDueDate As Date
        Dim blToday As Boolean
        Dim blBullpin As Boolean
        Dim strContext As String
        Dim lngTotal_Work As Long
        Dim dtReminder As Date
        Dim OlPA As PropertyAccessor
        Const PA_TOTAL_WORK As String =
            "http://schemas.microsoft.com/mapi/id/{00062003-0000-0000-C000-000000000046}/81110003"

        Dim blAutoAttributePeople As Boolean
        Dim varT As Object
        Dim OlExplorer As Explorer = Globals.ThisAddIn.Application.ActiveExplorer()

        If selItems Is Nothing Then
            selItems = New Collection
            For Each obj In OlExplorer.Selection
                selItems.Add(obj)
            Next obj
        End If

        lngTotal_Work = DefaultTaskTime

        If selItems.Count > 0 Then

            obj = selItems.Item(1)
            If TypeOf obj Is Outlook.MailItem Then                                                          'Check to see if it is an email
                OlMail = obj                                                                              'If email, link the object to an MailItem type
                strCats_All = OlMail.Categories
                If OlMail.TaskSubject <> "" Then
                    strSubject = OlMail.TaskSubject
                Else
                    strSubject = OlMail.Subject
                End If
                OlImportance = OlMail.Importance
                If OlMail.ReminderTime <> DateValue("1/1/4501") Then dtReminder = OlMail.ReminderTime
                If OlMail.TaskDueDate <> DateValue("1/1/4501") Then dtDueDate = OlMail.TaskDueDate

                OlPA = OlMail.PropertyAccessor
                On Error Resume Next
                lngTotal_Work = OlPA.GetProperty(PA_TOTAL_WORK)
                If Err.Number <> 0 Then
                    Err.Clear()
                    lngTotal_Work = DefaultTaskTime
                End If

            ElseIf TypeOf obj Is Outlook.TaskItem Then
                OlTask = obj
                strCats_All = OlTask.Categories
                strSubject = OlTask.Subject
                OlImportance = OlTask.Importance
                If OlTask.ReminderTime <> DateValue("1/1/4501") Then dtReminder = OlTask.ReminderTime
                If OlTask.DueDate <> DateValue("1/1/4501") Then dtDueDate = OlTask.DueDate
                lngTotal_Work = OlTask.TotalWork
            ElseIf TypeOf obj Is Outlook.AppointmentItem Then
                OlAppointment = obj
                strCats_All = OlAppointment.Categories
                strSubject = OlAppointment.Subject
                OlImportance = OlAppointment.Importance
            End If


        End If

        'Split_Cats_Into_Groups strCats_All, strCats_People, strCats_Projects, strCats_Topics, strCats_TagOther, strCats_Other, blToday, blBullpin


        Split_Cats_Into_Groups(strCats_All:=strCats_All,
                    strCats_People:=strCats_People,
                    strCats_Projects:=strCats_Projects,
                    strCats_Topics:=strCats_Topics,
                    strCats_TagContext:=strCats_TagOther,
                    strCats_KB:=strCats_KB,
                    strCats_Other:=strCats_Other,
                    blToday:=blToday,
                    blBullpin:=blBullpin)

        EmailForm = New NewEMailAsTask                                                          'Initialize the dialogue box
        EmailForm.Init(strCats_Other, strCats_People, strCats_Projects, strCats_Topics, Priority:=OlImportance,
        Task_Name_Selected:=strSubject, selItems:=selItems, lngTotal_Work:=lngTotal_Work, hWndCaller:=hWndCaller,
        blToday:=blToday, blBullpin:=blBullpin, objItemObject:=obj, strNameOfFunctionCalling:=strNameOfFunctionCalling,
        KBStatus:=strCats_KB, dtReminder:=dtReminder, dtDueDate:=dtDueDate)

        If selItems.Count > 1 Then
            EmailForm.options = Flag_FT_Selectively
        End If
        EmailForm.Show                                                                              'Show the form
        Dim tmpOpts As FTOptionsEnum


    End Sub

End Module
