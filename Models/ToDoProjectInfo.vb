Imports System.Numerics
Imports BrightIdeasSoftware
Imports System.Collections
Imports System.IO
Imports Microsoft.Office.Interop.Outlook
Imports System.Collections.Generic
Imports System.Linq
Imports System.Collections.ObjectModel
Imports System.Diagnostics
Imports Microsoft.Office.Core
Imports System.Linq.Expressions
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Globalization

<Serializable()>
Public Class ProjectInfoEntry
    Implements IEquatable(Of ProjectInfoEntry), IComparable(Of ProjectInfoEntry)

    Public Property ProjectName As String
    Public Property ProjectID As String
    Public Property ProgramName As String

    Public Sub New(ByVal ProjName As String, ProjID As String, ProgName As String)
        ProjectName = ProjName
        ProjectID = ProjID
        ProgramName = ProgName
    End Sub

    Public Overloads Function Equals(other As ProjectInfoEntry) As Boolean Implements IEquatable(Of ProjectInfoEntry).Equals
        If other Is Nothing Then Return False
        Return (Me.ProjectName.Equals(other.ProjectName))
    End Function

    Public Overrides Function Equals(obj As Object) As Boolean
        If obj Is Nothing Then Return False

        Dim other As ProjectInfoEntry = TryCast(obj, ProjectInfoEntry)
        If other Is Nothing Then
            Return False
        Else
            Return Equals(other)
        End If
    End Function

    Public Function CompareTo(other As ProjectInfoEntry) As Integer Implements IComparable(Of ProjectInfoEntry).CompareTo
        If other Is Nothing Then
            Return 1
        Else
            Dim x As Integer = String.CompareOrdinal(Me.ProjectID, other.ProjectID)
            If x = 0 Then
                If Me.ProjectID.Length < other.ProjectID.Length Then
                    x = -1
                ElseIf Me.ProjectID.Length > other.ProjectID.Length Then
                    x = 1
                End If
            End If
            Return x
            'Return Me.ProjectID.CompareTo(other.ProjectID)
        End If
    End Function

    Public Function ToCSV() As String
        Return ProjectID + "," + ProjectName + "," + ProgramName
    End Function
End Class

<Serializable()>
Public Class ProjectInfo
    Inherits List(Of ProjectInfoEntry)
    Public pFileName As String = ""

    Public Sub Save(FileName_IDList As String)
        If Not Directory.Exists(Path.GetDirectoryName(FileName_IDList)) Then
            Directory.CreateDirectory(Path.GetDirectoryName(FileName_IDList))
        End If
        Dim TestFileStream As Stream = File.Create(FileName_IDList)
        Dim serializer As New BinaryFormatter
        serializer.Serialize(TestFileStream, Me)
        TestFileStream.Close()
        pFileName = FileName_IDList
    End Sub

    Public Sub Save()
        If pFileName.Length > 0 Then
            Dim TestFileStream As Stream = File.Create(pFileName)
            Dim serializer As New BinaryFormatter
            serializer.Serialize(TestFileStream, Me)
            TestFileStream.Close()
        Else
            MsgBox("Can't save. IDList FileName not set yet")
        End If
    End Sub

    Public Function Contains_ProjectName(StrProjectName As String) As Boolean
        Return Me.Any(Function(p) String.Equals(p.ProjectName, StrProjectName, StringComparison.CurrentCulture))
    End Function

    Public Function Find_ByProjectName(StrProjectName As String) As List(Of ProjectInfoEntry)
        Return Me.Where(Function(p) String.Equals(p.ProjectName, StrProjectName, StringComparison.CurrentCulture)).ToList()
    End Function

    Public Function Contains_ProjectID(StrProjectID As String) As Boolean
        Return Me.Any(Function(p) String.Equals(p.ProjectID, StrProjectID, StringComparison.Ordinal))
    End Function

    Public Function Find_ByProjectID(StrProjectID As String) As List(Of ProjectInfoEntry)
        Return Me.Where(Function(p) String.Equals(p.ProjectID, StrProjectID, StringComparison.CurrentCulture)).ToList()
    End Function

    Public Function Contains_ProgramName(StrProgramName As String) As Boolean
        Return Me.Any(Function(p) String.Equals(p.ProgramName, StrProgramName, StringComparison.CurrentCulture))
    End Function

    Public Function Find_ByProgramName(StrProgramName As String) As List(Of ProjectInfoEntry)
        Return Me.Where(Function(p) String.Equals(p.ProgramName, StrProgramName, StringComparison.CurrentCulture)).ToList()
    End Function
End Class

'<Serializable()>
'Public Class ProjectInfo2
'    Inherits List(Of ProjectInfoEntry)
'    Public pFileName As String = ""

'    Public Sub Save(FileName_IDList As String)
'        If Not Directory.Exists(Path.GetDirectoryName(FileName_IDList)) Then
'            Directory.CreateDirectory(Path.GetDirectoryName(FileName_IDList))
'        End If
'        Dim TestFileStream As Stream = File.Create(FileName_IDList)
'        Dim serializer As New BinaryFormatter
'        serializer.Serialize(TestFileStream, Me)
'        TestFileStream.Close()
'        pFileName = FileName_IDList
'    End Sub

'    Public Sub Save()
'        If pFileName.Length > 0 Then
'            Dim TestFileStream As Stream = File.Create(pFileName)
'            Dim serializer As New BinaryFormatter
'            serializer.Serialize(TestFileStream, Me)
'            TestFileStream.Close()
'        Else
'            MsgBox("Can't save. IDList FileName not set yet")
'        End If
'    End Sub
'End Class


'<Serializable()>
'Public Class ProjectInfo
'    Implements IList(Of ProjectInfoEntry)
'    Private pList As List(Of ProjectInfoEntry)
'    Public pFileName As String = ""

'    Public Sub New()
'        pList = New List(Of ProjectInfoEntry)
'    End Sub
'    Public Sub Save(FileName_IDList As String)
'        If Not Directory.Exists(Path.GetDirectoryName(FileName_IDList)) Then
'            Directory.CreateDirectory(Path.GetDirectoryName(FileName_IDList))
'        End If
'        Dim TestFileStream As Stream = File.Create(FileName_IDList)
'        Dim serializer As New BinaryFormatter
'        serializer.Serialize(TestFileStream, Me)
'        TestFileStream.Close()
'        pFileName = FileName_IDList
'    End Sub

'    Public Sub Save()
'        If pFileName.Length > 0 Then
'            Dim TestFileStream As Stream = File.Create(pFileName)
'            Dim serializer As New BinaryFormatter
'            serializer.Serialize(TestFileStream, Me)
'            TestFileStream.Close()
'        Else
'            MsgBox("Can't save. IDList FileName not set yet")
'        End If
'    End Sub
'    Default Public Property Item(index As Integer) As ProjectInfoEntry Implements IList(Of ProjectInfoEntry).Item
'        Get
'            Return pList.Item(index)
'        End Get
'        Set(value As ProjectInfoEntry)
'            pList.Item(index) = value
'        End Set
'    End Property

'    Public ReadOnly Property Count As Integer Implements ICollection(Of ProjectInfoEntry).Count
'        Get
'            Return pList.Count
'        End Get
'    End Property

'    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of ProjectInfoEntry).IsReadOnly
'        Get
'            Return False
'        End Get
'    End Property

'    Public Sub Insert(index As Integer, item As ProjectInfoEntry) Implements IList(Of ProjectInfoEntry).Insert
'        pList.Insert(index, item)
'    End Sub

'    Public Sub RemoveAt(index As Integer) Implements IList(Of ProjectInfoEntry).RemoveAt
'        pList.RemoveAt(index)
'    End Sub

'    Public Sub Add(item As ProjectInfoEntry) Implements ICollection(Of ProjectInfoEntry).Add
'        pList.Add(item)
'    End Sub

'    Public Sub Clear() Implements ICollection(Of ProjectInfoEntry).Clear
'        pList.Clear()
'    End Sub

'    Public Sub CopyTo(array() As ProjectInfoEntry, arrayIndex As Integer) Implements ICollection(Of ProjectInfoEntry).CopyTo
'        pList.CopyTo(array, arrayIndex)
'    End Sub

'    Public Function IndexOf(item As ProjectInfoEntry) As Integer Implements IList(Of ProjectInfoEntry).IndexOf
'        Return pList.IndexOf(item)
'    End Function

'    Public Function Contains(item As ProjectInfoEntry) As Boolean Implements ICollection(Of ProjectInfoEntry).Contains
'        Return pList.Contains(item)
'    End Function

'    Public Function Remove(item As ProjectInfoEntry) As Boolean Implements ICollection(Of ProjectInfoEntry).Remove
'        Return pList.Remove(item)
'    End Function

'    Public Function GetEnumerator() As IEnumerator(Of ProjectInfoEntry) Implements IEnumerable(Of ProjectInfoEntry).GetEnumerator
'        Return pList.GetEnumerator
'    End Function

'    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
'        Return pList.GetEnumerator
'    End Function
'End Class