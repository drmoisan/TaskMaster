Public Interface IToDoProjectInfoEntry
    Property ProgramName As String
    Property ProjectID As String
    Property ProjectName As String
    Function CompareTo(other As IToDoProjectInfoEntry) As Integer
    Function Equals(obj As Object) As Boolean
    Function Equals(other As IToDoProjectInfoEntry) As Boolean
    Function ToCSV() As String
End Interface
