Imports System.IO
Imports UtilitiesVB

Public Module Module1
    Private CTF_Inc_Ct As Integer
    Private CTF_Inc() As CTF_Incidence2

    Public Sub CTF_Incidence_Text_File_READ1(FolderPaths As IFileSystemFolderPaths)



        'INITIALIZE VARIABLES
        Dim i As Integer
        CTF_Inc_Ct = 0
        ReDim CTF_Inc(0)
        Dim filepath As String = Path.Combine(FolderPaths.FldrPythonStaging, My.Settings.File_CTF_Inc)

        'OPEN FILE IF IT EXISTS AND READ IT IN
        If File.Exists(filepath) Then
            Dim filecontents = System.IO.File.ReadAllLines(filepath, Text.Encoding.ASCII)
            Dim lines As New Queue(Of String)(filecontents.Skip(1))
            Dim listCTF As New List(Of CTF_Incidence2)
            listCTF.Add(New CTF_Incidence2())

            While lines.Count > 0
                Dim tmpCTF_Inc As New CTF_Incidence2()
                With tmpCTF_Inc
                    .Email_Conversation_ID = lines.Dequeue()
                    .Folder_Count = lines.Dequeue()
                    For i = 1 To .Folder_Count
                        .Email_Folder(i) = lines.Dequeue()
                        .Email_Conversation_Count(i) = lines.Dequeue()
                    Next
                End With
                listCTF.Add(tmpCTF_Inc)
            End While
            'ReDim CTF_Inc(listCTF.Count)
            CTF_Inc = listCTF.ToArray()
            CTF_Inc_Ct = listCTF.Count
            'Need to set CTF_Inc_Ct
        Else
            MsgBox("Index file not found. Please run indexer.", vbCritical)
        End If


    End Sub
End Module
