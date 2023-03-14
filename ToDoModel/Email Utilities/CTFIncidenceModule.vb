Public Module CTFIncidenceModule
    Public CTF_Inc() As CTF_Incidence
    Public CTF_Inc_Ct As Integer = 0

    Private Sub CTF_Inc_Position_ADD(Inc_Num As Integer, CTF_Map As Conversation_To_Folder)
        Dim i, j As Integer                                                                                                     'Variables to hold loop counters
        Dim added As Boolean

        added = False

        If My.Settings.MaxFolders_ConvID = 1 Then                                                                                           'If the MaxFolders is 1 then do the second check
            If CTF_Map.Email_Conversation_Count > CTF_Inc(Inc_Num).Email_Conversation_Count(1) Then                             'If the conversation count is more than the folder stored,
                Call CTF_Incidence_SET(Inc_Num, 1, 1, CTF_Map)                                                                  'then call the subroutine to replace the value
            End If

        Else                                                                                                                    'SECTION FOR WHEN MaxFolders IS MORE THAN 1

            If CTF_Inc(Inc_Num).Folder_Count < My.Settings.MaxFolders_ConvID Then
                CTF_Inc(Inc_Num).Folder_Count = CTF_Inc(Inc_Num).Folder_Count + 1                                               'If folder count is less than max, increase count
            End If

            For i = 1 To My.Settings.MaxFolders_ConvID - 1                                                                                  'Sorting routine to insert the new value in sequential order
                If CTF_Map.Email_Conversation_Count > CTF_Inc(Inc_Num).Email_Conversation_Count(i) Then                         'from largest folder count to least folder count. Items that
                    For j = My.Settings.MaxFolders_ConvID - 1 To i Step -1                                                                  'have a lower count than all items up to the max will not be added
                        CTF_Inc(Inc_Num).Email_Conversation_Count(j + 1) = CTF_Inc(Inc_Num).Email_Conversation_Count(j)
                        CTF_Inc(Inc_Num).Email_Folder(j + 1) = CTF_Inc(Inc_Num).Email_Folder(j)
                    Next j
                    CTF_Inc(Inc_Num).Email_Conversation_Count(i) = CTF_Map.Email_Conversation_Count
                    CTF_Inc(Inc_Num).Email_Folder(i) = CTF_Map.Email_Folder
                    added = True
                    Exit For
                End If
            Next i

            If added = False Then

                If CTF_Map.Email_Conversation_Count > CTF_Inc(Inc_Num).Email_Conversation_Count(My.Settings.MaxFolders_ConvID) Then
                    CTF_Inc(Inc_Num).Email_Conversation_Count(My.Settings.MaxFolders_ConvID) = CTF_Map.Email_Conversation_Count
                    CTF_Inc(Inc_Num).Email_Folder(My.Settings.MaxFolders_ConvID) = CTF_Map.Email_Folder
                End If

            End If

        End If
    End Sub



    Private Function CTF_Incidence_FIND(ConvID As String) As Integer
        Dim i As Integer

        CTF_Incidence_FIND = 0

        For i = 1 To CTF_Inc_Ct
            If CTF_Inc(i).Email_Conversation_ID = ConvID Then
                CTF_Incidence_FIND = i
                Exit For
            End If
        Next i

    End Function

    Private Sub CTF_Incidence_INIT(Inc_Num As Integer)
        Dim i As Integer

        For i = 1 To My.Settings.MaxFolders_ConvID                                                      'Loop through the number of Folders we are saving
            CTF_Inc(Inc_Num).Folder_Count = 0
            CTF_Inc(Inc_Num).Email_Conversation_Count(i) = 0                                'Set count to 0 so any value wins
            CTF_Inc(Inc_Num).Email_Folder(i) = "==============================="            'Set Folder name to lines so that they will not be accepted if they show up in selection list
        Next i
    End Sub

    Private Sub CTF_Incidence_SET(Inc_Num As Integer, Inc_Position As Integer, Folder_Count As Integer, Map As Conversation_To_Folder)
        CTF_Inc(Inc_Num).Folder_Count = Folder_Count
        CTF_Inc(Inc_Num).Email_Conversation_ID = Map.Email_Conversation_ID
        CTF_Inc(Inc_Num).Email_Conversation_Count(Inc_Position) = Map.Email_Conversation_Count
        CTF_Inc(Inc_Num).Email_Folder(Inc_Position) = Map.Email_Folder
    End Sub



    '****************************************************************************************************************************************************
    '****This Subroutine Writes to the File System the conversation id's with the Folders that have the most emails from the conversation in them********
    '****************************************************************************************************************************************************

    Public Sub CTF_Incidence_Text_File_WRITE()
        Dim LOC_TXT_FILE As String
        Dim a
        Dim i, j As Integer



        Dim objShell = CreateObject("Shell.Application")
        Dim objFSO = CreateObject("Scripting.FileSystemObject")

        DELETE_TextFile(File_CTF_Inc, FileSystem_FLOW & "\Combined\data\")
        LOC_TXT_FILE = FileSystem_FLOW & "\Combined\data\" & File_CTF_Inc
        a = objFSO.CreateTextFile(LOC_TXT_FILE, True)
        Dim unused5 = a.WriteLine("This file contains a mapping of folders to email conversations based on incidence")

        For i = 1 To CTF_Inc_Ct
            Dim unused4 = a.WriteLine(CTF_Inc(i).Email_Conversation_ID)
            Dim unused3 = a.WriteLine(CTF_Inc(i).Folder_Count)
            For j = 1 To CTF_Inc(i).Folder_Count
                Dim unused2 = a.WriteLine(CTF_Inc(i).Email_Folder(j))
                Dim unused1 = a.WriteLine(CTF_Inc(i).Email_Conversation_Count(j))
            Next j
        Next i

        Dim unused = a.Close


    End Sub

    Public Sub CTF_Incidence_Text_File_READ()

        Dim objFSO As Object       ' Computer's file system object.
        Dim LOC_TXT_FILE As String
        Dim oFS
        Dim Temp As String
        Dim i As Integer

        'INITIALIZE VARIABLES
        objShell = CreateObject("Shell.Application")
        objFSO = CreateObject("Scripting.FileSystemObject")
        CTF_Inc_Ct = 0
        ReDim CTF_Inc(0)
        LOC_TXT_FILE = FileSystem_FLOW & "\Combined\data\" & File_CTF_Inc

        'OPEN FILE IF IT EXISTS AND READ IT IN
        If objFSO.FileExists(LOC_TXT_FILE) = True Then
            oFS = objFSO.OpenTextFile(LOC_TXT_FILE)

            Temp = oFS.ReadLine
            Do Until oFS.AtEndOfStream
                CTF_Inc_Ct += 1
                ReDim Preserve CTF_Inc(CTF_Inc_Ct)
                CTF_Inc(CTF_Inc_Ct).Email_Conversation_ID = oFS.ReadLine
                CTF_Inc(CTF_Inc_Ct).Folder_Count = oFS.ReadLine
                For i = 1 To CTF_Inc(CTF_Inc_Ct).Folder_Count
                    CTF_Inc(CTF_Inc_Ct).Email_Folder(i) = oFS.ReadLine
                    CTF_Inc(CTF_Inc_Ct).Email_Conversation_Count(i) = oFS.ReadLine
                Next i
            Loop
        Else
            Temp = MsgBox("Index file not found. Please run indexer.", vbCritical)
        End If
        Dim unused = oFS.Close

    End Sub

End Module
