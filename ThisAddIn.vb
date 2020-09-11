Option Explicit On
Imports Microsoft.Office.Interop.Outlook
Imports System.Diagnostics
Imports System.IO
Imports System

Public Class ThisAddIn

    Public UsedIDList As List(Of String) = New List(Of String)

    Private ribTM As TaskMasterRibbon

    Private Sub ThisAddIn_Startup() Handles Me.Startup
        If Globals.ThisAddIn.UsedIDList.Count = 0 Then
            Globals.ThisAddIn.UsedIDList_Load()
        End If
        Access_Ribbons_By_Explorer()
    End Sub

    Private Sub ThisAddIn_Shutdown() Handles Me.Shutdown

    End Sub

    Private Sub Access_Ribbons_By_Explorer()
        Dim ribbonCollection As ThisRibbonCollection = Globals.Ribbons _
            (Globals.ThisAddIn.Application.ActiveExplorer())
        ribTM = ribbonCollection.Ribbon1                                    'Grab handle on on Ribbon


    End Sub

    Public Function GetItemsCol_ToDo() As Collection
        Dim OlItems As Items
        Dim colItems As Collection
        Dim objItem As Object
        Dim OlFolder As Folder
        Dim oStore As Outlook.Store

        colItems = New Collection
        For Each oStore In Globals.ThisAddIn.Application.Session.Stores
            OlFolder = oStore.GetDefaultFolder(OlDefaultFolders.olFolderToDo)
            OlItems = OlFolder.Items
            For Each objItem In OlItems
                'Debug.Print objItem.Subject
                colItems.Add(objItem)
            Next objItem
        Next
        GetItemsCol_ToDo = colItems
    End Function

    Public Function RefreshToDoID_Max() As Long
        Dim colItems As Collection
        Dim colItems_NoID As Collection
        Dim objItem As Object
        Dim lngTmp As Long
        Dim lngMax As Long
        Dim strTmp As String
        Dim msgResponse As MsgBoxResult


        On Error Resume Next

        colItems = GetItemsCol_ToDo()

        colItems_NoID = New Collection
        lngMax = 0

        For Each objItem In colItems

            strTmp = CustomFieldID_GetValue(objItem, "ToDoID")
            If strTmp.Length = 0 Then
                colItems_NoID.Add(objItem)
            Else
                lngTmp = ConvertToDecimal(125, Left(strTmp, 2))
                If Err.Number <> 0 Then
                    Debug.WriteLine("Error can't convert variable strTmp = " & strTmp & " to Long")
                    Debug.WriteLine("Adding item to list of items to renumber")
                    Err.Clear()
                    colItems_NoID.Add(objItem)
                Else
                    If lngTmp > lngMax Then lngMax = lngTmp
                End If
            End If
        Next objItem

        If colItems_NoID.Count > 0 Then
            msgResponse = MsgBox(colItems_NoID.Count & " Items with no ToDoID. Assign them IDs now?", vbYesNo)
            If msgResponse = vbYes Then
                For Each objItem In colItems_NoID
                    Debug.WriteLine(objItem.Subject)
                    lngMax += 1
                    CustomFieldID_Set("ToDoID", ConvertToBase(125, lngMax), SpecificItem:=objItem)
                Next objItem
            End If
        End If

        Dim filename_UsedIDList As String = "C:\Users\03311352\Documents\UsedIDList.csv"
        Using sw As StreamWriter = New StreamWriter(filename_UsedIDList)
            For Each objItem In colItems
                strTmp = CustomFieldID_GetValue(objItem, "ToDoID")
                sw.WriteLine(strTmp)
            Next
        End Using


        Debug.WriteLine(Err.Description)


        My.Settings.MaxToDo = lngMax
        My.Settings.Save()
        Debug.WriteLine(lngMax)
        Debug.WriteLine(My.Settings.MaxToDo.ToString)
    End Function
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
            MsgBox("Unsupported object type")
        End If

        If objProperty Is Nothing Then
            CustomFieldID_GetValue = ""
        Else
            If IsArray(objProperty.Value) Then
                CustomFieldID_GetValue = FlattenArry(objProperty.Value)
            Else
                CustomFieldID_GetValue = objProperty.Value
            End If
        End If

        OlMail = Nothing
        OlTask = Nothing
        OlAppt = Nothing
        objProperty = Nothing

    End Function

    Public Function FlattenArry(varBranch() As Object) As String
        Dim i As Integer
        Dim strTemp As String

        strTemp = ""

        For i = 0 To UBound(varBranch)
            If IsArray(varBranch(i)) Then
                strTemp = strTemp & ", " & FlattenArry(varBranch(i))
            Else
                strTemp = strTemp & ", " & varBranch(i)
            End If
        Next i
        If strTemp.Length <> 0 Then strTemp = Right(strTemp, Len(strTemp) - 2)
        FlattenArry = strTemp
    End Function

    Public Function CustomFieldID_Set(ByVal UserDefinedFieldName As String,
                               Optional ByVal Value As String = "",
                               Optional ByVal IsCustomEntry As Boolean = False,
                               Optional ByRef SpecificItem As Object = Nothing,
                               Optional ByVal olUPType As Outlook.OlUserPropertyType =
                               Outlook.OlUserPropertyType.olText) As Boolean

        Dim myCollection As Object
        Dim Msg As Outlook.MailItem
        Dim oTask As Outlook.TaskItem
        Dim oMail As Outlook.MailItem
        Dim OlAppointment As Outlook.AppointmentItem
        Dim objProperty As Outlook.UserProperty


        Try
            If Not SpecificItem Is Nothing Then
                If TypeOf SpecificItem Is MailItem Then
                    oMail = SpecificItem
                    objProperty = oMail.UserProperties.Find(UserDefinedFieldName)
                    If objProperty Is Nothing Then objProperty = oMail.UserProperties.Add(UserDefinedFieldName, olUPType)
                    objProperty.Value = Value
                    oMail.Save()
                End If
                If TypeOf SpecificItem Is TaskItem Then
                    oTask = SpecificItem
                    objProperty = oTask.UserProperties.Find(UserDefinedFieldName)
                    If objProperty Is Nothing Then objProperty = oTask.UserProperties.Add(UserDefinedFieldName, olUPType)
                    objProperty.Value = Value
                    oTask.Save()
                End If
                If TypeOf SpecificItem Is Outlook.AppointmentItem Then
                    OlAppointment = SpecificItem
                    objProperty = OlAppointment.UserProperties.Find(UserDefinedFieldName)
                    If objProperty Is Nothing Then objProperty = OlAppointment.UserProperties.Add(UserDefinedFieldName, olUPType)
                    objProperty.Value = Value
                    OlAppointment.Save()
                End If
            End If
            CustomFieldID_Set = True
        Catch
            Debug.WriteLine("Exception caught: ", Err)
            CustomFieldID_Set = False
            Err.Clear()
        Finally
            Msg = Nothing
            objProperty = Nothing
            myCollection = Nothing
            oTask = Nothing
            oMail = Nothing
            OlAppointment = Nothing
        End Try

    End Function

    Public Function GetListOfItemsInView_ToDo() As List(Of Object)
        Dim OlItems As Items
        Dim objView As View
        Dim OlFolder As Folder
        Dim strFilter As String
        Dim oStore As Outlook.Store
        Dim objItem As Object


        objView = Application.ActiveExplorer.CurrentView
        strFilter = "@SQL=" & objView.Filter

        OlItems = Nothing
        For Each oStore In Application.Session.Stores
            OlFolder = oStore.GetDefaultFolder(OlDefaultFolders.olFolderToDo)
            If strFilter = "@SQL=" Then
                OlItems = OlFolder.Items
            Else
                OlItems = OlFolder.Items.Restrict(strFilter)
            End If
        Next
        Dim ListObjects As List(Of Object) = New List(Of Object)
        For Each objItem In OlItems
            ListObjects.Add(objItem)
        Next
        'GetItemsInView_ToDo = OlItems
        Return ListObjects
    End Function
    Public Function GetItemsInView_ToDo() As Items
        Dim OlItems As Items
        Dim objView As View
        Dim OlFolder As Folder
        Dim strFilter As String
        Dim oStore As Outlook.Store

        objView = Application.ActiveExplorer.CurrentView
        strFilter = "@SQL=" & objView.Filter

        OlItems = Nothing
        For Each oStore In Application.Session.Stores
            OlFolder = oStore.GetDefaultFolder(OlDefaultFolders.olFolderToDo)
            If strFilter = "@SQL=" Then
                OlItems = OlFolder.Items
            Else
                OlItems = OlFolder.Items.Restrict(strFilter)
            End If
        Next
        GetItemsInView_ToDo = OlItems
    End Function

    Public Function IsChild(strParent As String, strChild As String) As Integer
        Dim i As Integer = 0
        Dim count As Integer = 0
        Dim unbroken As Boolean = True
        For i = 1 To strParent.Length / 2
            If unbroken Then
                If Mid(strParent, i * 2 - 1, 2) = Mid(strChild, i * 2 - 1, 2) Then
                    count = i
                Else
                    unbroken = False
                End If
            End If
        Next
        IsChild = count
    End Function
    Public Function FindParent(itms As Collection, strChild As String) As Object
        Dim strParent As String

        Try
            strParent = Left(strChild, strChild.Length - 2)
            FindParent = itms(strParent)
        Catch
            FindParent = Nothing
            Err.Clear()
        End Try

    End Function
    Public Function Refresh_ToDoID_Splits()
        Dim objItem As Object
        Dim OlItems As Items = GetItemsInView_ToDo()
        For Each objItem In OlItems
            Split_ToDoID(objItem)
        Next
    End Function
    Public Function Split_ToDoID(objItem As Object)
        Dim i As Integer
        Dim strField As String = ""
        Dim strFieldValue As String = ""

        Try
            Dim strToDoID As String = CustomFieldID_GetValue(objItem, "ToDoID")
            Dim intDepth As Integer = strToDoID.Length / 2

            For i = 3 To intDepth
                strField = "ToDoIdLvl" & i
                strFieldValue = Mid(strToDoID, i * 2 - 1, 2)
                CustomFieldID_Set(strField, strFieldValue, SpecificItem:=objItem)
            Next
        Catch
            Debug.WriteLine("Error in Split_ToDoID")
            Debug.WriteLine(Err.Description)
            Debug.WriteLine("Field Name is " & strField)
            Debug.WriteLine("Field Value is " & strFieldValue)
            Stop
        End Try

    End Function

    Public Sub UsedIDList_Append(strID As String)
        'Append an item to the UsedIDList and write to disk
        UsedIDList.Add(strID)
        Dim filename_UsedIDList As String = "C:\Users\03311352\Documents\UsedIDList.csv"
        Using sw As StreamWriter = File.AppendText(filename_UsedIDList)
            sw.WriteLine(strID)
        End Using
    End Sub

    Public Sub UsedIDList_Load()
        'Load the used ID list
        Dim filename_UsedIDList As String = "C:\Users\03311352\Documents\UsedIDList.csv"

        Try
            Using sr As StreamReader = New StreamReader(filename_UsedIDList)
                While (sr.Peek > -1)
                    UsedIDList.Add(sr.ReadLine)
                End While
            End Using
        Catch
            Debug.WriteLine(Err.Description)
        End Try

    End Sub
End Class
