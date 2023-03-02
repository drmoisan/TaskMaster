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
Public Class ProjectInfo
    Inherits List(Of ToDoProjectInfoEntry)
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
        Dim common = StrProjectName.Split(", ").ToList().Intersect([Select](Function(b) b.ProjectName))
        If (StrProjectName.Split(", ").ToList().Intersect([Select](Function(b) b.ProjectName)).ToList().Count > 0) Then
            Return True
        Else
            Return False
        End If
        'Return Me.Any(Function(p) String.Equals(p.ProjectName, StrProjectName, StringComparison.CurrentCulture))
    End Function

    Public Function Programs_ByProjectNames(StrProjectNames As String) As String

        Try
            Dim strTemp As String = String.Join(", ", Me.Where(Function(p) StrProjectNames.Split({", "}, StringSplitOptions.None).ToList().Contains(p.ProjectName)).Select(Function(q) q.ProgramName).Distinct())
            Return strTemp
        Catch ex As System.Exception
            Debug.WriteLine(ex.Message)
            Debug.WriteLine(ex.StackTrace)
            Return ""
        End Try

    End Function

    Public Function Find_ByProjectName(StrProjectName As String) As List(Of ToDoProjectInfoEntry)
        Return Me.Where(Function(p) String.Equals(p.ProjectName, StrProjectName, StringComparison.CurrentCulture)).ToList()
    End Function

    Public Function Contains_ProjectID(StrProjectID As String) As Boolean
        'Dim common = StrProjectID.Split(", ").ToList().Intersect([Select](Function(b) b.ProjectID))
        'Return Me.Any(StrProjectID.Split(", ").ToList().Intersect([Select](Function(b) b.ProjectID)))
        Return Me.Any(Function(p) String.Equals(p.ProjectID, StrProjectID, StringComparison.Ordinal))
    End Function

    Public Function Find_ByProjectID(StrProjectID As String) As List(Of ToDoProjectInfoEntry)
        Return Me.Where(Function(p) String.Equals(p.ProjectID, StrProjectID, StringComparison.CurrentCulture)).ToList()
    End Function

    Public Function Contains_ProgramName(StrProgramName As String) As Boolean
        Return Me.Any(Function(p) String.Equals(p.ProgramName, StrProgramName, StringComparison.CurrentCulture))
    End Function

    Public Function Find_ByProgramName(StrProgramName As String) As List(Of ToDoProjectInfoEntry)
        Return Me.Where(Function(p) String.Equals(p.ProgramName, StrProgramName, StringComparison.CurrentCulture)).ToList()
    End Function
End Class

