
namespace ToDoModel
{

    public static class IDListUtilities
    {
        // Public Function LoadIDList(FilePath As String, Application As Application) As ListOfIDsLegacy
        // Dim IDList As ListOfIDsLegacy
        // If File.Exists(FilePath) Then

        // Dim deserializer As New BinaryFormatter
        // Try
        // Using TestFileStream As Stream = File.OpenRead(FilePath)
        // IDList = CType(deserializer.Deserialize(TestFileStream), ListOfIDsLegacy)
        // End Using
        // Catch ex As UnauthorizedAccessException
        // Dim unused1 = MsgBox("Unexpected Access Error. Duplicate Instance Running?")
        // Throw ex
        // Catch ex As IOException
        // Dim unused = MsgBox("Unexpected IO Error. Is IDList File Corrupt?")
        // Throw ex
        // End Try

        // IDList.Filepath = FilePath

        // Return IDList
        // Else
        // IDList = New ListOfIDsLegacy(New List(Of String))
        // IDList.RefreshIDList(Application)
        // IDList.Save(FilePath)
        // Return IDList
        // End If
        // End Function

        // Public Function RefreshIDList(FilePath As String, Application As Application) As ListOfIDsLegacy
        // Dim _idList As ListOfIDsLegacy = New ListOfIDsLegacy(New List(Of String))
        // _idList.RefreshIDList(Application)
        // _idList.Save(FilePath)
        // Return _idList
        // End Function

        // ''' <summary>
        // ''' Function Invokes the DataModel_ToDoTree.ReNumberIDs() method at the root level which 
        // ''' recursively calls DataModel_ToDoTree.ReNumberChildrenIDs() and then invokes the
        // ''' ListOfIDsLegacy.Save() Method
        // ''' </summary>
        // ''' <param name="IDList">Pointer to active instance of ListOfIDsLegacy Class</param>
        // ''' <param name="Application">Pointer to Outlook Application</param>
        // ''' <param name="DebugFolderPath">Optional path to output csv for debugging if supplied</param>
        // Public Sub CompressToDoIDs(ByRef IDList As ListOfIDsLegacy,
        // ByRef Application As Application,
        // Optional DebugFolderPath As String = "")
        // 'DOC: Add documentation to CompressToDoIDs
        // 'TESTING: Add integration testing for CompressToDoIDs
        // 'DONE: Move CompressToDoIDs to either a Module or include in ToDoTree DataModel
        // Dim _dataModel As New TreeOfToDoItems()
        // 'QUESTION: Does DataModel_ToDoTree.LoadOptions.vbLoadAll require all items to be visible in the current view?
        // _dataModel.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadAll, Application)

        // If DebugFolderPath <> "" Then
        // _dataModel.WriteTreeToCSVDebug(Path.Combine(DebugFolderPath, "DebugTreeDump_Pre.csv"))
        // End If

        // _dataModel.ReNumberIDs(IDList)

        // If DebugFolderPath <> "" Then
        // _dataModel.WriteTreeToCSVDebug(Path.Combine(DebugFolderPath, "DebugTreeDump_Post.csv"))
        // End If

        // End Sub

    }
}