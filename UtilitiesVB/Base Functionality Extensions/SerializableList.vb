Imports System.IO
Imports System.Windows.Forms
Imports Newtonsoft.Json
Imports System.Reflection
Imports System



<Serializable>
Public Class SerializableList2(Of T)
    Implements IList(Of T)

    Private _innerList As List(Of T)
    Private _lazyLoader As IEnumerable(Of T)
    Private _filename As String
    Private _folderpath As String
    Private _filepath As String

    Public Sub New()
        _innerList = New List(Of T)
    End Sub

    Public Sub New(listOfT As List(Of T))
        _innerList = listOfT
    End Sub

    Public Sub New(IEnumerableOfT As IEnumerable(Of T))
        _lazyLoader = IEnumerableOfT
    End Sub

    Public Sub Serialize()
        If Filepath <> "" Then
            Serialize(Filepath)
        End If
    End Sub

    Public Sub Serialize(filepath As String)
        Me.Filepath = filepath

        Throw New NotImplementedException
    End Sub

    Public Sub ToCSV(filepath As String)

    End Sub

    'Public Sub Deserialize()
    '    If Filepath <> "" Then
    '        Deserialize(Filepath)
    '    End If
    'End Sub
    '
    'Public Sub Deserialize(filepath As String)
    '    Dim _csvSerializer = New CsvSerializer.Serializer
    '    Dim listObj As Object = Nothing
    '    Dim shouldExecute As Boolean = True
    '    Me.Filepath = filepath
    '    Try
    '        Using csvStream As New FileStream(path:=filepath, mode:=FileMode.Open)
    '            listObj = _csvSerializer.Deserialize(csvStream)
    '        End Using
    '    Catch ex As Exception
    '        MsgBox("Error accessing file." & ex.Message)
    '        shouldExecute = False
    '    End Try
    '    If shouldExecute Then
    '        _innerList = TryCast(listObj, List(Of T))
    '        If _innerList Is Nothing Then
    '            MsgBox("Cannot convert file " & filepath & "to List(Of T)")
    '        End If
    '    End If

    'End Sub

    Private Sub ensureList()
        If _innerList Is Nothing Then _innerList = New List(Of T)(_lazyLoader)
    End Sub

    Public Function IndexOf(ByVal item As T) As Integer Implements IList(Of T).IndexOf
        ensureList()
        Return _innerList.IndexOf(item)
    End Function

    Public Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert
        ensureList()
        _innerList.Insert(index, item)
    End Sub

    Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt
        ensureList()
        _innerList.RemoveAt(index)
    End Sub

    Default Public Property Item(ByVal index As Integer) As T Implements IList(Of T).Item
        Get
            ensureList()
            Return _innerList(index)
        End Get
        Set(ByVal value As T)
            ensureList()
            _innerList(index) = value
        End Set
    End Property

    Public Sub Add(ByVal item As T) Implements IList(Of T).Add
        ensureList()
        _innerList.Add(item)
    End Sub

    Public Sub Clear() Implements IList(Of T).Clear
        ensureList()
        _innerList.Clear()
    End Sub

    Public Function Contains(ByVal item As T) As Boolean Implements IList(Of T).Contains 'Implements ICollection(Of T).Contains
        ensureList()
        Return _innerList.Contains(item)
    End Function

    Public Sub CopyTo(ByVal array As T(), ByVal arrayIndex As Integer) Implements IList(Of T).CopyTo
        ensureList()
        _innerList.CopyTo(array, arrayIndex)
    End Sub

    Public ReadOnly Property Count As Integer Implements IList(Of T).Count
        Get
            ensureList()
            Return _innerList.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean Implements IList(Of T).IsReadOnly
        Get
            Return False
        End Get
    End Property

    Public Property Filepath As String
        Get
            If _filepath = "" Then
                If _filename = "" And _folderpath = "" Then
                    MsgBox("Filepath is empty")
                ElseIf _filename = "" Then
                    MsgBox("Folderpath has a value but Filename is empty")
                Else
                    MsgBox("Filename has a value but Folderpath is empty")
                End If
            End If
            Return _filepath
        End Get
        Set(value As String)
            _filepath = value
            _folderpath = Path.GetDirectoryName(_filepath)
            _filename = Path.GetFileName(_filepath)
        End Set
    End Property

    Public Property Folderpath As String
        Get
            Return _folderpath
        End Get
        Set(value As String)
            _folderpath = value
            If _filename <> "" Then
                _filepath = Path.Combine(_folderpath, _filename)
            End If
        End Set
    End Property

    Public Property Filename As String
        Get
            Return _filename
        End Get
        Set(value As String)
            _filename = value
            If _folderpath <> "" Then
                _filepath = Path.Combine(_folderpath, _filename)
            End If
        End Set
    End Property

    Public Function Remove(ByVal item As T) As Boolean Implements IList(Of T).Remove
        ensureList()
        Return _innerList.Remove(item)
    End Function

    Public Function GetEnumerator() As System.Collections.IEnumerator Implements IEnumerable.GetEnumerator
        ensureList()
        Return _innerList.GetEnumerator()
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        ensureList()
        Return _innerList.GetEnumerator()
    End Function


End Class
