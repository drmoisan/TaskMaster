Public Interface IFileSystemFolderPaths
    ReadOnly Property AppData As String
    ReadOnly Property Flow As String
    ReadOnly Property MyD As String
    ReadOnly Property PreReads As String
    ReadOnly Property Root As String
    ReadOnly Property StagingPath As String
    Sub Reload()
End Interface
