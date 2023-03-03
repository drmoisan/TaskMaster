Imports UtilitiesVB

<Serializable()>
Public Class ToDoProjectInfoEntry
    Implements IEquatable(Of IToDoProjectInfoEntry),
        IComparable(Of IToDoProjectInfoEntry), IToDoProjectInfoEntry

    Public Property ProjectName As String Implements IToDoProjectInfoEntry.ProjectName
    Public Property ProjectID As String Implements IToDoProjectInfoEntry.ProjectID
    Public Property ProgramName As String Implements IToDoProjectInfoEntry.ProgramName

    Public Sub New(ByVal ProjName As String, ProjID As String, ProgName As String)
        ProjectName = ProjName
        ProjectID = ProjID
        ProgramName = ProgName
    End Sub

    Public Overloads Function Equals(other As IToDoProjectInfoEntry) _
        As Boolean Implements IEquatable(Of IToDoProjectInfoEntry).Equals,
        IToDoProjectInfoEntry.Equals

        If other Is Nothing Then
            Return False
        Else
            Return (Me.ProjectName.Equals(other.ProjectName))
        End If
    End Function

    Public Overrides Function Equals(obj As Object) As Boolean Implements IToDoProjectInfoEntry.Equals
        If obj Is Nothing Then Return False

        Dim other As ToDoProjectInfoEntry = TryCast(obj, ToDoProjectInfoEntry)
        If other Is Nothing Then
            Return False
        Else
            Return Equals(other)
        End If
    End Function

    Public Function CompareTo(other As IToDoProjectInfoEntry) As Integer Implements IComparable(Of IToDoProjectInfoEntry).CompareTo, IToDoProjectInfoEntry.CompareTo
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

    Public Function ToCSV() As String Implements IToDoProjectInfoEntry.ToCSV
        Return ProjectID + "," + ProjectName + "," + ProgramName
    End Function
End Class
