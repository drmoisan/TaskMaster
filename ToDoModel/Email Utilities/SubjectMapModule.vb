Imports Microsoft.Office.Interop
Imports UtilitiesVB

Friend Module SubjectMapModule
    'Not Implemented
    Public Subject_Map_Ct As Long = 0
    Public Subject_Map() As Subject_Mapping

    Sub Subject_MAP_Text_File_READ(FolderPaths As IFileSystemFolderPaths)
        Throw New NotImplementedException
    End Sub

    Sub Common_Words_Text_File_READ(FolderPaths As IFileSystemFolderPaths)
        Throw New NotImplementedException
    End Sub

    Public Function GetOutlookFolder(ByVal FolderPath As String, OlApp As Outlook.Application) As Outlook.Folder
        Dim TestFolder As Outlook.Folder
        Dim FoldersArray As Object
        Dim i As Integer

        If Left(FolderPath, 2) = "\\" Then
            FolderPath = Right(FolderPath, Len(FolderPath) - 2)
        End If
        'Convert folderpath to array
        FoldersArray = Split(FolderPath, "\")
        TestFolder = OlApp.Session.Folders.Item(FoldersArray(0))
        If Not TestFolder Is Nothing Then
            For i = 1 To UBound(FoldersArray, 1)
                Dim SubFolders As Outlook.Folders
                SubFolders = TestFolder.Folders
                TestFolder = SubFolders.Item(FoldersArray(i))
                If TestFolder Is Nothing Then
                    Return Nothing
                End If
            Next
        End If

        Return TestFolder

    End Function
    Public Function OlFolderlist_GetAll(OlObjects As IOlObjects) As String()

        Dim resultList = New List(Of String)
        Dim fldrEmailRoot As Outlook.Folder

        fldrEmailRoot = GetOutlookFolder(OlObjects.ArchiveRootPath, OlObjects.App)
        OlFolder_GetDescendants(resultList, fldrEmailRoot.Folders, fldrEmailRoot.FolderPath)
        OlFolderlist_GetAll = resultList.ToArray()
    End Function

    Private Sub OlFolder_GetDescendants(ByRef ResultList As List(Of String),
                                        ByRef Children As Outlook.Folders,
                                        ByRef RootPath As String)

        For Each child As Outlook.Folder In Children
            Dim fPath As String = child.FolderPath
            fPath = Right(fPath, Len(fPath) - Len(RootPath) - 1)
            ResultList.Add(fPath)
            OlFolder_GetDescendants(ResultList, child.Folders, RootPath)
        Next

    End Sub

End Module
