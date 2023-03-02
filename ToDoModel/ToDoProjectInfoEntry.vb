
<Serializable()>
Public Class ToDoProjectInfoEntry
    Implements IEquatable(Of ToDoProjectInfoEntry), IComparable(Of ToDoProjectInfoEntry)

    Public Property ProjectName As String
    Public Property ProjectID As String
    Public Property ProgramName As String

    Public Sub New(ByVal ProjName As String, ProjID As String, ProgName As String)
        ProjectName = ProjName
        ProjectID = ProjID
        ProgramName = ProgName
    End Sub

    Public Overloads Function Equals(other As ToDoProjectInfoEntry) As Boolean Implements IEquatable(Of ToDoProjectInfoEntry).Equals
        If other Is Nothing Then Return False
        Return (Me.ProjectName.Equals(other.ProjectName))
    End Function

    Public Overrides Function Equals(obj As Object) As Boolean
        If obj Is Nothing Then Return False

        Dim other As ToDoProjectInfoEntry = TryCast(obj, ToDoProjectInfoEntry)
        If other Is Nothing Then
            Return False
        Else
            Return Equals(other)
        End If
    End Function

    Public Function CompareTo(other As ToDoProjectInfoEntry) As Integer Implements IComparable(Of ToDoProjectInfoEntry).CompareTo
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
