Imports Microsoft.Office.Interop
Imports UtilitiesVB

Public Module GetFields
    Public Function CustomFieldID_GetValue(objItem As Object, ByVal UserDefinedFieldName As String) As String
        Dim OlMail As Outlook.MailItem
        Dim OlTask As Outlook.TaskItem
        Dim OlAppt As Outlook.AppointmentItem
        Dim objProperty As Outlook.UserProperty


        If TypeOf objItem Is Outlook.MailItem Then
            OlMail = objItem
            objProperty = OlMail.UserProperties.Find(UserDefinedFieldName)

        ElseIf TypeOf objItem Is Outlook.TaskItem Then
            OlTask = objItem
            objProperty = OlTask.UserProperties.Find(UserDefinedFieldName)
        ElseIf TypeOf objItem Is Outlook.AppointmentItem Then
            OlAppt = objItem
            objProperty = OlAppt.UserProperties.Find(UserDefinedFieldName)
        Else
            objProperty = Nothing
            Dim unused = MsgBox("Unsupported object type")
        End If

        CustomFieldID_GetValue = If(objProperty Is Nothing,
            "",
            If(IsArray(objProperty.Value), FlattenArry(objProperty.Value), DirectCast(objProperty.Value, String)))
    End Function
End Module
