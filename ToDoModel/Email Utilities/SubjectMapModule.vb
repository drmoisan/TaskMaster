Imports Microsoft.Office.Interop
Imports UtilitiesVB
Imports System.IO
Imports Microsoft.VisualStudio.Services.Graph.Constants

Friend Module SubjectMapModule
    Public SubjectMapCt As Long = 0
    Public SubjectMap() As Subject_Mapping
    Public WordList() As String
    Public WordCount As Integer
    Public Structure Subject_Incidence
        Public Val As Integer
        Public fldr As String
    End Structure

    Public Subject_Inc() As Subject_Incidence
    Public Subject_Inc_Ct As Integer

    Sub Subject_MAP_Text_File_READ(fs As IFileSystemFolderPaths)

        SubjectMapCt = 0
        ReDim SubjectMap(0)

        Dim fileContents() As String = CSV_Read(filename:=fs.Filenames.SubjectMap,
                                                fileaddress:=fs.FldrPythonStaging,
                                                SkipHeaders:=True)
        Dim rowQueue As New Queue(Of String)(fileContents)

        While rowQueue.Count > 0
            SubjectMapCt += 1
            ReDim Preserve SubjectMap(SubjectMapCt)
            SubjectMap(SubjectMapCt).Email_Folder = rowQueue.Dequeue()
            SubjectMap(SubjectMapCt).Email_Subject = StripCommonWords(rowQueue.Dequeue())
            SubjectMap(SubjectMapCt).Email_Subject_Count = CInt(rowQueue.Dequeue())
        End While

    End Sub

    Sub Common_Words_Text_File_READ(fs As IFileSystemFolderPaths)
        Dim fileContents() As String = CSV_Read(filename:=fs.Filenames.CommonWords,
                                                fileaddress:=fs.FldrPythonStaging,
                                                SkipHeaders:=False)
        Dim i As Integer = 0
        ReDim WordList(fileContents.Length)
        WordList(0) = ""
        WordCount = fileContents.Length
        For Each row As String In fileContents
            i += 1
            WordList(i) = row
        Next
    End Sub

    Sub Subject_Map_Add(Subj As String, FolderName As String)
        Dim Subject_Map_Idx As Integer


        'Check to see if any mapping exists. If not, add the first entry
        If SubjectMapCt = 0 Then
            SubjectMapCt = 1
            ReDim Preserve SubjectMap(1)
            Call Subject_Map_Set(Subj, 1, FolderName, 1)

            'Else, find the item and insert it
        Else
            Subject_Map_Idx = Subject_Map_Find(Subj, FolderName)              'Find a matching pair

            'If it doesn't exist, add an entry. If it does exist, increase the count
            If Subject_Map_Idx = 0 Then
                SubjectMapCt = SubjectMapCt + 1                             'Increase the max count
                ReDim Preserve SubjectMap(SubjectMapCt)                               'Add another slot to the array
                Call Subject_Map_Set(Subj, 1, FolderName, SubjectMapCt)     'Set the value to the last spot in the array
            Else
                SubjectMap(Subject_Map_Idx).Email_Subject_Count = SubjectMap(Subject_Map_Idx).Email_Subject_Count + 1
            End If

        End If
    End Sub


    Sub Subject_Map_Set(Subj As String, SubjCt As Integer, FolderName As String, Subject_Map_Idx As Integer)
        SubjectMap(Subject_Map_Idx).Email_Folder = FolderName
        SubjectMap(Subject_Map_Idx).Email_Subject = Subj
        SubjectMap(Subject_Map_Idx).Email_Subject_Count = SubjCt
    End Sub

    Function Subject_Map_Find(Subj As String, FolderName As String) As Integer
        Dim i As Integer
        Dim Subject_Map_Idx As Integer

        Subject_Map_Idx = 0                                     'Initialize

        'Loop to try and find an entry that matches the subject and Folder Name
        For i = 1 To SubjectMapCt
            If SubjectMap(i).Email_Subject = Subj And SubjectMap(i).Email_Folder = FolderName Then
                Subject_Map_Idx = i
                Exit For
            End If
        Next i

        Subject_Map_Find = Subject_Map_Idx

    End Function

    Sub Subject_Inc_Add(FolderName As String, Val As Integer)
        Dim Subject_Inc_Idx As Integer


        'Check to see if any mapping exists. If not, add the first entry
        If Subject_Inc_Ct = 0 Then
            Subject_Inc_Ct = 1
            ReDim Preserve Subject_Inc(1)
            Subject_Inc(Subject_Inc_Ct).fldr = FolderName
            Subject_Inc(Subject_Inc_Ct).Val = Val

            'Else, find the item and insert it
        Else
            Subject_Inc_Idx = Subject_Inc_Find(FolderName)                            'Find a matching pair

            'If it doesn't exist, add an entry. If it does exist, increase the count
            If Subject_Inc_Idx = 0 Then
                Subject_Inc_Ct = Subject_Inc_Ct + 1                                         'Increase the max count
                ReDim Preserve Subject_Inc(Subject_Inc_Ct)                                  'Add another slot to the array
                Subject_Inc(Subject_Inc_Ct).fldr = FolderName
                Subject_Inc(Subject_Inc_Ct).Val = Val

            Else
                Subject_Inc(Subject_Inc_Idx).Val = Subject_Inc(Subject_Inc_Idx).Val + Val
            End If

        End If
    End Sub



    Function Subject_Inc_Find(FolderName As String) As Integer
        Dim i As Integer
        Dim Subject_Inc_Idx As Integer

        Subject_Inc_Idx = 0                                     'Initialize

        'Loop to try and find an entry that matches the subject and Folder Name
        For i = 1 To Subject_Inc_Ct
            If Subject_Inc(i).fldr = FolderName Then
                Subject_Inc_Idx = i
                Exit For
            End If
        Next i

        Subject_Inc_Find = Subject_Inc_Idx

    End Function



End Module
