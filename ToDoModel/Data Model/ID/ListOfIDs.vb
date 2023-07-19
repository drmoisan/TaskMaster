Imports System.IO
Imports System.Numerics
Imports System.Runtime.Serialization.Formatters.Binary
Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB

<Serializable()>
Public Class ListOfIDs
    Implements IListOfIDs

    Private _usedIDList As List(Of String)
    Private _maxIDLength As Long
    Private _filepath As String = ""

    Public Sub New(ByVal listUsedID As List(Of String))
        UsedIDList = listUsedID
    End Sub

    Public Sub New(FilePath As String, OlApp As Application)
        LoadFromFile(FilePath:=FilePath, OlApp:=OlApp)
    End Sub

    Public Sub New()
        _usedIDList = New List(Of String)
    End Sub

    Public Shared Function LoadFromFile(FilePath As String, OlApp As Application) As ListOfIDs
        Dim tmpIDList As ListOfIDs = New ListOfIDs

        If File.Exists(FilePath) Then
            Dim deserializer As New BinaryFormatter
            Try
                Using TestFileStream As Stream = File.OpenRead(FilePath)
                    tmpIDList = CType(deserializer.Deserialize(TestFileStream), ListOfIDs)
                End Using

            Catch ex As UnauthorizedAccessException
                tmpIDList = ProcessFileError(OlApp,
                    "Unexpected File Access Error. Recreate the list?")

            Catch ex As IOException
                tmpIDList = ProcessFileError(OlApp,
                    "Unexpected IO Error. Is IDList File Corrupt?")

            Catch ex As InvalidCastException
                tmpIDList = ProcessFileError(OlApp,
                    "File exists but cannot cast to ListOfIDs. Recreate the list?")
            End Try

        Else
            tmpIDList = ProcessFileError(OlApp,
                "File " & FilePath & " does not exist. Recreate the List?")
        End If

        tmpIDList.Filepath = FilePath
        Return tmpIDList
    End Function

    Private Shared Function ProcessFileError(OlApp As Application, msg As String) As ListOfIDs
        Dim tmpIDList As ListOfIDs = New ListOfIDs()
        Dim result As MsgBoxResult = MsgBox(msg, vbYesNo)
        If result = MsgBoxResult.Yes Then
            tmpIDList.RefreshIDList(OlApp)
        Else
            MsgBox("Returning an empty list of ToDoIDs")
        End If
        Return tmpIDList
    End Function

    Public Sub RefreshIDList(Application As Application) Implements IListOfIDs.RefreshIDList
        Dim unused As New Object
        Dim _dataModel As New TreeOfToDoItems
        Dim _toDoList As List(Of Object)
        UsedIDList = New List(Of String)

        _toDoList = _dataModel.GetToDoList(TreeOfToDoItems.LoadOptions.vbLoadAll, Application)

        For Each _objItem As Object In _toDoList
            Dim strID As String = CustomFieldID_GetValue(_objItem, "ToDoID")
            If UsedIDList.Contains(strID) = False And strID.Length <> 0 Then
                UsedIDList.Add(strID)
                If strID.Length > _maxIDLength Then _maxIDLength = strID.Length
            End If
        Next
    End Sub

    ''' <summary>
    ''' Function Invokes the DataModel_ToDoTree.ReNumberIDs() method at the root level which 
    ''' recursively calls DataModel_ToDoTree.ReNumberChildrenIDs() and then invokes the
    ''' ListOfIDs.Save() Method
    ''' </summary>
    ''' <param name="OlApp">Pointer to Outlook Application</param>
    Public Sub CompressToDoIDs(OlApp As Application) Implements IListOfIDs.CompressToDoIDs
        Dim _dataModel As New TreeOfToDoItems()
        _dataModel.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadAll, OlApp)
        _dataModel.ReNumberIDs(Me)
    End Sub

    Public ReadOnly Property MaxIDLength As Long Implements IListOfIDs.MaxIDLength
        Get
            If _maxIDLength = 0 Then
                Dim maxLen As Long = 0
                For Each strID As String In UsedIDList
                    If strID.Length > maxLen Then
                        maxLen = strID.Length
                    End If
                Next
                _maxIDLength = maxLen
            End If
            Return _maxIDLength

        End Get
    End Property

    Public Property UsedIDList As List(Of String) Implements IListOfIDs.UsedIDList
        Get
            Return _usedIDList
        End Get
        Set(value As List(Of String))
            _usedIDList = value
        End Set
    End Property

    Public Property Filepath As String Implements IListOfIDs.Filepath
        Get
            Return _filepath
        End Get
        Set(value As String)
            _filepath = value
        End Set
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
        If strMaxID.Length > _maxIDLength Then
            _maxIDLength = strMaxID.Length
            My.Settings.MaxIDLength = _maxIDLength
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
        If strMaxID.Length > _maxIDLength Then
            _maxIDLength = strMaxID.Length
            My.Settings.MaxIDLength = _maxIDLength
            My.Settings.Save()
        End If

        Return strMaxID
    End Function

    Public Sub Save(Filepath As String) Implements IListOfIDs.Save
        If Not Directory.Exists(Path.GetDirectoryName(Filepath)) Then
            Directory.CreateDirectory(Path.GetDirectoryName(Filepath))
        End If

        Dim serializer As New BinaryFormatter
        Using TestFileStream As Stream = File.Create(Filepath)
            serializer.Serialize(TestFileStream, Me)
        End Using

        Me.Filepath = Filepath
    End Sub

    Public Sub Save() Implements IListOfIDs.Save
        If Filepath.Length > 0 Then
            Dim serializer As New BinaryFormatter
            Using TestFileStream As Stream = File.Create(Filepath)
                serializer.Serialize(TestFileStream, Me)
            End Using
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