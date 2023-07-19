Imports UtilitiesVB

<Serializable()>
Public Class ToDoProjectInfoEntry
    Implements IEquatable(Of IToDoProjectInfoEntry),
        IComparable, IComparable(Of IToDoProjectInfoEntry), IToDoProjectInfoEntry

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

        Return other IsNot Nothing AndAlso ProjectName.Equals(other.ProjectName)
    End Function

    Public Overrides Function Equals(obj As Object) As Boolean Implements IToDoProjectInfoEntry.Equals
        If obj Is Nothing Then Return False

        Dim other As ToDoProjectInfoEntry = TryCast(obj, ToDoProjectInfoEntry)
        Return other IsNot Nothing AndAlso Equals(other)
    End Function

    Public Function CompareTo(other As IToDoProjectInfoEntry) As Integer Implements IComparable(Of IToDoProjectInfoEntry).CompareTo, IToDoProjectInfoEntry.CompareTo
        If other Is Nothing Then
            Return 1
        Else
            Dim x As Integer = String.CompareOrdinal(ProjectID, other.ProjectID)
            If x = 0 Then
                If ProjectID.Length < other.ProjectID.Length Then
                    x = -1
                ElseIf ProjectID.Length > other.ProjectID.Length Then
                    x = 1
                End If
            End If
            Return x
            'Return Me.ProjectID.CompareTo(other.ProjectID)
        End If
    End Function

    Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
        If obj Is Nothing Then Return 1
        Dim other As IToDoProjectInfoEntry = TryCast(obj, IToDoProjectInfoEntry)

        If other IsNot Nothing Then
            Return CompareTo(other)
        Else
            Throw New ArgumentException("Object cannot be cast to IToDoProjectInfoEntry")
        End If
    End Function

    Public Function ToCSV() As String Implements IToDoProjectInfoEntry.ToCSV
        Return ProjectID + "," + ProjectName + "," + ProgramName
    End Function

End Class
