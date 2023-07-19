Public Interface IFileSystemFolderPaths
    ReadOnly Property FldrAppData As String
    ReadOnly Property FldrFlow As String
    ReadOnly Property FldrMyD As String
    ReadOnly Property FldrPreReads As String
    ReadOnly Property FldrRoot As String
    ReadOnly Property FldrStaging As String
    ReadOnly Property FldrPythonStaging As String
    Sub Reload()
    ReadOnly Property Filenames As IAppStagingFilenames
End Interface
