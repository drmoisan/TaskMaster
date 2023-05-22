
// Imports System.IO
// Imports UtilitiesVB

// Public Module CTFIncidenceModule
// Public CTF_Inc() As CTF_Incidence
// Public CTF_Inc_Ct As Integer = 0

// Public Sub CTF_Inc_Position_ADD(Inc_Num As Integer, CTF_Map As Conversation_To_Folder)
// Dim i, j As Integer                                                                                                     'Variables to hold loop counters
// Dim added As Boolean

// added = False

// If My.Settings.MaxFolders_ConvID = 1 Then                                                                                           'If the MaxFolders is 1 then do the second check
// If CTF_Map.Email_Conversation_Count > CTF_Inc(Inc_Num).Email_Conversation_Count(1) Then                             'If the conversation count is more than the folder stored,
// Call CTF_Incidence_SET(Inc_Num, 1, 1, CTF_Map)                                                                  'then call the subroutine to replace the value
// End If

// Else                                                                                                                    'SECTION FOR WHEN MaxFolders IS MORE THAN 1

// If CTF_Inc(Inc_Num).Folder_Count < My.Settings.MaxFolders_ConvID Then
// CTF_Inc(Inc_Num).Folder_Count = CTF_Inc(Inc_Num).Folder_Count + 1                                               'If folder count is less than max, increase count
// End If

// For i = 1 To My.Settings.MaxFolders_ConvID - 1                                                                                  'Sorting routine to insert the new value in sequential order
// If CTF_Map.Email_Conversation_Count > CTF_Inc(Inc_Num).Email_Conversation_Count(i) Then                         'from largest folder count to least folder count. Items that
// For j = My.Settings.MaxFolders_ConvID - 1 To i Step -1                                                                  'have a lower count than all items up to the max will not be added
// CTF_Inc(Inc_Num).Email_Conversation_Count(j + 1) = CTF_Inc(Inc_Num).Email_Conversation_Count(j)
// CTF_Inc(Inc_Num).EmailFolder(j + 1) = CTF_Inc(Inc_Num).EmailFolder(j)
// Next j
// CTF_Inc(Inc_Num).Email_Conversation_Count(i) = CTF_Map.Email_Conversation_Count
// CTF_Inc(Inc_Num).EmailFolder(i) = CTF_Map.EmailFolder
// added = True
// Exit For
// End If
// Next i

// If added = False Then

// If CTF_Map.Email_Conversation_Count > CTF_Inc(Inc_Num).Email_Conversation_Count(My.Settings.MaxFolders_ConvID) Then
// CTF_Inc(Inc_Num).Email_Conversation_Count(My.Settings.MaxFolders_ConvID) = CTF_Map.Email_Conversation_Count
// CTF_Inc(Inc_Num).EmailFolder(My.Settings.MaxFolders_ConvID) = CTF_Map.EmailFolder
// End If

// End If

// End If
// End Sub



// Public Function CTF_Incidence_FIND(ConvID As String) As Integer
// Dim i As Integer

// CTF_Incidence_FIND = 0

// For i = 1 To CTF_Inc_Ct
// If CTF_Inc(i).Email_Conversation_ID = ConvID Then
// CTF_Incidence_FIND = i
// Exit For
// End If
// Next i

// End Function

// Public Sub CTF_Incidence_INIT(Inc_Num As Integer)
// Dim i As Integer

// For i = 1 To My.Settings.MaxFolders_ConvID                                                      'Loop through the number of Folders we are saving
// CTF_Inc(Inc_Num).Folder_Count = 0
// CTF_Inc(Inc_Num).Email_Conversation_Count(i) = 0                                'Set count to 0 so any value wins
// CTF_Inc(Inc_Num).EmailFolder(i) = "==============================="            'Set Folder name to lines so that they will not be accepted if they show up in selection list
// Next i
// End Sub

// Public Sub CTF_Incidence_SET(Inc_Num As Integer, Inc_Position As Integer, Folder_Count As Integer, Map As Conversation_To_Folder)
// CTF_Inc(Inc_Num).Folder_Count = Folder_Count
// CTF_Inc(Inc_Num).Email_Conversation_ID = Map.Email_Conversation_ID
// CTF_Inc(Inc_Num).Email_Conversation_Count(Inc_Position) = Map.Email_Conversation_Count
// CTF_Inc(Inc_Num).EmailFolder(Inc_Position) = Map.EmailFolder
// End Sub



// '****************************************************************************************************************************************************
// '****This Subroutine Writes to the File System the conversation id's with the Folders that have the most emails from the conversation in them********
// '****************************************************************************************************************************************************

// Public Sub CTF_Incidence_Text_File_WRITE(FolderPaths As IFileSystemFolderPaths)

// Dim listOutput As New List(Of String)
// listOutput.Add("This file contains a mapping of folders to email conversations based on incidence")

// Dim i, j As Integer
// For i = 1 To CTF_Inc_Ct
// listOutput.Add(CTF_Inc(i).Email_Conversation_ID)
// listOutput.Add(CTF_Inc(i).Folder_Count)
// For j = 1 To CTF_Inc(i).Folder_Count
// listOutput.Add(CTF_Inc(i).EmailFolder(j))
// listOutput.Add(CTF_Inc(i).Email_Conversation_Count(j))
// Next j
// Next i

// Dim filepath As String = Path.Combine(FolderPaths.FldrPythonStaging, My.Settings.File_CTF_Inc)
// Using sw As New StreamWriter(filepath, False, System.Text.Encoding.ASCII)
// For Each line In listOutput
// sw.WriteLine(line)
// Next
// End Using

// End Sub

// Public Sub CTF_Incidence_Text_File_READ(FolderPaths As IFileSystemFolderPaths)

// 'INITIALIZE VARIABLES
// Dim i As Integer
// CTF_Inc_Ct = 0
// ReDim CTF_Inc(0)
// Dim filepath As String = Path.Combine(FolderPaths.FldrPythonStaging, My.Settings.File_CTF_Inc)

// 'OPEN FILE IF IT EXISTS AND READ IT IN
// If File.Exists(filepath) Then
// Dim filecontents = System.IO.File.ReadAllLines(filepath, Text.Encoding.ASCII)
// Dim lines As New Queue(Of String)(filecontents.Skip(1))
// Dim listCTF As New List(Of CTF_Incidence)
// listCTF.Add(New CTF_Incidence())

// While lines.Count > 0
// Dim tmpCTF_Inc As New CTF_Incidence()
// With tmpCTF_Inc
// .Email_Conversation_ID = lines.Dequeue()
// .Folder_Count = lines.Dequeue()
// For i = 1 To .Folder_Count
// .EmailFolder(i) = lines.Dequeue()
// .Email_Conversation_Count(i) = lines.Dequeue()
// Next
// End With
// listCTF.Add(tmpCTF_Inc)
// End While
// 'ReDim CTF_Inc(listCTF.Count)
// CTF_Inc = listCTF.ToArray()
// CTF_Inc_Ct = listCTF.Count
// 'Need to set CTF_Inc_Ct
// Else
// MsgBox("Index file not found. Please run indexer.", vbCritical)
// End If


// End Sub

// End Module
