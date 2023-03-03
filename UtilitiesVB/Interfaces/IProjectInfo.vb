Public Interface IProjectInfo
    Sub Save()
    Sub Save(FileName_IDList As String)
    Function Contains_ProgramName(StrProgramName As String) As Boolean
    Function Contains_ProjectID(StrProjectID As String) As Boolean
    Function Contains_ProjectName(StrProjectName As String) As Boolean
    Function Find_ByProgramName(StrProgramName As String) As List(Of IToDoProjectInfoEntry)
    Function Find_ByProjectID(StrProjectID As String) As List(Of IToDoProjectInfoEntry)
    Function Find_ByProjectName(StrProjectName As String) As List(Of IToDoProjectInfoEntry)
    Function Programs_ByProjectNames(StrProjectNames As String) As String
End Interface
