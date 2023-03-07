Imports System.IO
Imports System.Numerics
Imports System.Runtime.Serialization.Formatters.Binary
Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB

<Serializable()>
Public Class ListOfIDs
    Implements IListOfIDs

    Public UsedIDList As List(Of String)
    Private PMaxIDLength As Long
    Public pFileName As String = ""

    Public Sub New(ByVal listUsedID As List(Of String))
        UsedIDList = listUsedID
    End Sub

    Public Sub RePopulate(Application As Application) Implements IListOfIDs.RePopulate
        Dim unused As New Object
        Dim DM As New TreeOfToDoItems
        Dim ToDoList As List(Of Object) = DM.GetToDoList(TreeOfToDoItems.LoadOptions.vbLoadAll, Application)
        UsedIDList = New List(Of String)

        Dim ObjItem As Object
        For Each ObjItem In ToDoList
            Dim strID As String = CustomFieldID_GetValue(ObjItem, "ToDoID")
            If UsedIDList.Contains(strID) = False And strID.Length <> 0 Then
                UsedIDList.Add(strID)
                If strID.Length > PMaxIDLength Then PMaxIDLength = strID.Length
            End If
        Next
    End Sub

    'Public Sub CondenseIDs()
    '    Dim DM As DataModel_ToDoTree = New DataModel_ToDoTree
    '    DM.LoadTree(DataModel_ToDoTree.LoadOptions.vbLoadAll)

    '    Dim ToDoTree As List(Of TreeNode(Of ToDoItem)) = DM.ListOfToDoTree
    'End Sub

    Public ReadOnly Property MaxIDLength As Long Implements IListOfIDs.MaxIDLength
        Get
            If PMaxIDLength = 0 Then
                Dim maxLen As Long = 0
                For Each strID As String In UsedIDList
                    If strID.Length > maxLen Then
                        maxLen = strID.Length
                    End If
                Next
                PMaxIDLength = maxLen
            End If
            Return PMaxIDLength

        End Get
    End Property

    Public Function GetNextAvailableToDoID(strSeed As String) As String Implements IListOfIDs.GetNextAvailableToDoID
        Dim blContinue As Boolean = True
        Dim lngMaxID As BigInteger = ConvertToDecimal(125, strSeed)
        Dim strMaxID As String = ""

        While blContinue
            lngMaxID += 1
            strMaxID = ConvertToBase(125, lngMaxID)
            If UsedIDList.Contains(strMaxID) = False Then
                blContinue = False
            End If
        End While
        UsedIDList.Add(strMaxID)
        If strMaxID.Length > PMaxIDLength Then
            PMaxIDLength = strMaxID.Length
            My.Settings.MaxIDLength = PMaxIDLength
            My.Settings.Save()
        End If
        Return strMaxID
    End Function

    Public Function GetMaxToDoID() As String Implements IListOfIDs.GetMaxToDoID
        Dim strMaxID = UsedIDList.Max()
        Dim lngMaxID As BigInteger = ConvertToDecimal(125, strMaxID)
        lngMaxID += 1
        strMaxID = ConvertToBase(125, lngMaxID)
        UsedIDList.Add(strMaxID)
        If strMaxID.Length > PMaxIDLength Then
            PMaxIDLength = strMaxID.Length
            My.Settings.MaxIDLength = PMaxIDLength
            My.Settings.Save()
        End If

        Return strMaxID
    End Function

    Public Sub Save(FileName_IDList As String) Implements IListOfIDs.Save
        If Not Directory.Exists(Path.GetDirectoryName(FileName_IDList)) Then
            Dim unused = Directory.CreateDirectory(Path.GetDirectoryName(FileName_IDList))
        End If
        Dim TestFileStream As Stream = File.Create(FileName_IDList)
        Dim serializer As New BinaryFormatter
        serializer.Serialize(TestFileStream, Me)
        TestFileStream.Close()
        pFileName = FileName_IDList
    End Sub

    Public Sub Save() Implements IListOfIDs.Save
        If pFileName.Length > 0 Then
            Dim TestFileStream As Stream = File.Create(pFileName)
            Dim serializer As New BinaryFormatter
            serializer.Serialize(TestFileStream, Me)
            TestFileStream.Close()
        Else
            Dim unused = MsgBox("Can't save. IDList FileName not set yet")
        End If
    End Sub

    Public Function ConvertToBase(nbase As Integer, ByVal num As BigInteger, Optional intMinDigits As Integer = 2) As String Implements IListOfIDs.ConvertToBase
        Dim chars As String
        Dim r As BigInteger
        Dim newNumber As String
        Dim maxBase As Integer
        Dim i As Integer

        'chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ"
        chars = "0123456789aAáÁàÀâÂäÄãÃåÅæÆbBcCçÇdDðÐeEéÉèÈêÊëËfFƒgGhHIIíÍìÌîÎïÏjJkKlLmMnNñÑoOóÓòÒôÔöÖõÕøØœŒpPqQrRsSšŠßtTþÞuUúÚùÙûÛüÜvVwWxXyYýÝÿŸzZžŽ"
        maxBase = Len(chars)

        ' check if we can convert to this base
        If nbase > maxBase Then
            ConvertToBase = ""
        Else

            ' in r we have the offset of the char that was converted to the new base
            newNumber = ""
            While num >= nbase
                r = num Mod nbase
                newNumber = Mid(chars, r + 1, 1) & newNumber
                num /= nbase
            End While

            newNumber = Mid(chars, num + 1, 1) & newNumber

            For i = 1 To Len(newNumber) Mod intMinDigits
                newNumber = 0 & newNumber
            Next i

            ConvertToBase = newNumber
        End If
    End Function

    Public Function ConvertToDecimal(nbase As Integer, ByVal strBase As String) As BigInteger Implements IListOfIDs.ConvertToDecimal
        Dim chars As String
        Dim i As Integer
        Dim intLoc As Integer
        Dim lngTmp As BigInteger

        'chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ"
        chars = "0123456789aAáÁàÀâÂäÄãÃåÅæÆbBcCçÇdDðÐeEéÉèÈêÊëËfFƒgGhHIIíÍìÌîÎïÏjJkKlLmMnNñÑoOóÓòÒôÔöÖõÕøØœŒpPqQrRsSšŠßtTþÞuUúÚùÙûÛüÜvVwWxXyYýÝÿŸzZžŽ"
        lngTmp = 0

        For i = 1 To Len(strBase)
            lngTmp *= nbase
            intLoc = InStr(chars, Mid(strBase, i, 1))
            lngTmp += intLoc - 1
        Next i

        ConvertToDecimal = lngTmp
    End Function

    Private Function CustomFieldID_GetValue(objItem As Object, ByVal UserDefinedFieldName As String) As String
        Dim OlMail As [MailItem]
        Dim OlTask As [TaskItem]
        Dim OlAppt As [AppointmentItem]
        Dim objProperty As [UserProperty]


        If objItem Is Nothing Then
            Return ""
        ElseIf TypeOf objItem Is [MailItem] Then
            OlMail = objItem
            objProperty = OlMail.UserProperties.Find(UserDefinedFieldName)

        ElseIf TypeOf objItem Is [TaskItem] Then
            OlTask = objItem
            objProperty = OlTask.UserProperties.Find(UserDefinedFieldName)
        ElseIf TypeOf objItem Is [AppointmentItem] Then
            OlAppt = objItem
            objProperty = OlAppt.UserProperties.Find(UserDefinedFieldName)
        Else
            objProperty = Nothing
            Dim unused = MsgBox("Unsupported object type")
        End If

        Return If(objProperty Is Nothing,
            "",
            If(IsArray(objProperty.Value), FlattenArry(objProperty.Value), DirectCast(objProperty.Value, String)))

        OlMail = Nothing
        OlTask = Nothing
        OlAppt = Nothing
        objProperty = Nothing

    End Function

    Public Function FlattenArry(varBranch() As Object) As String Implements IListOfIDs.FlattenArry
        Dim i As Integer
        Dim strTemp As String

        strTemp = ""

        For i = 0 To UBound(varBranch)
            strTemp = If(IsArray(varBranch(i)), strTemp & ", " & FlattenArry(varBranch(i)), DirectCast(strTemp & ", " & varBranch(i), String))
        Next i
        If strTemp.Length <> 0 Then strTemp = Right(strTemp, Len(strTemp) - 2)
        FlattenArry = strTemp
    End Function
End Class