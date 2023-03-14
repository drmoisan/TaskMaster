Imports Microsoft.Office.Interop.Outlook
Imports Microsoft.Office.Interop

Public Class cInfoMail
    Public Subject As String
    Private _endDate As Date
    Public StartDate As Date
    Private _durationSec As Long
    Public SentTo As String
    Public SentCC As String
    Public SentFrom As String
    Public Body As String
    Public Importance As Outlook.OlImportance
    Public Categories As String
    Public strAction As String
    Public strProcName As String
    Private col As Collection
    Private dict As Dictionary(Of String, Long)


    Private Sub SortDictionary()
        Dim i As Long
        Dim key As Object
        Dim objSortedList As Object

        objSortedList = CreateObject("System.Collections.SortedList")
        With objSortedList
            'With CreateObject("System.Collections.SortedList")
            For Each key In dict
                .Add(key, dict(key))
            Next
            dict.Clear()
            For i = .Keys.Count - 1 To 0 Step -1
                dict.Add(.GetKey(i), .item(.GetKey(i)))
            Next
        End With
    End Sub

    Public Sub dict_new()
        dict = New Dictionary(Of String, Long)
    End Sub
    Public Sub dict_add(strKey As String, lngVal As Long)
        'col.ADD lngVal, strKey
        dict.Add(strKey, lngVal)
    End Sub

    Public ReadOnly Property dict_ct() As Integer
        Get
            Return dict.Count
        End Get
    End Property

    Public ReadOnly Property dict_strSum() As String
        Get
            Dim i As Integer
            Dim key As String
            i = 0
            If dict.Count = 0 Then
                dict_strSum = ""
            Else
                SortDictionary()
                'Sort_Collections.sort col, New Sort_CReverseComparator
                dict_strSum = "Grouped Apps: "
                For Each key In dict.Keys
                    i = i + 1
                    If i < 3 Then
                        If i > 1 Then dict_strSum = dict_strSum & " | "
                        dict_strSum = dict_strSum & key & " " & Format(dict(key) / 60, "#,##0.0") & " min"
                    End If
                Next key
            End If
        End Get
    End Property

    Public Sub dict_upORadd(strKey As String, lngVal As Long)
        If dict.ContainsKey(strKey) Then
            dict(strKey) = dict(strKey) + lngVal
        Else
            dict.Add(strKey, lngVal)
        End If
    End Sub
    Friend Function Init(Optional lcl_Subject As String = "",
                        Optional lcl_EndDate As Date = Nothing,
                        Optional lcl_StartDate As Date = Nothing,
                        Optional lcl_DurationSec As Long = 0,
                        Optional lcl_SentTo As String = "",
                        Optional lcl_SentCC As String = "",
                        Optional lcl_SentFrom As String = "",
                        Optional lcl_Body As String = "",
                        Optional lcl_Importance As Outlook.OlImportance = OlImportance.olImportanceNormal,
                        Optional lcl_Categories As Outlook.Categories = Nothing,
                        Optional lcl_strAction As String = "")


        On Error Resume Next

        Subject = lcl_Subject
        EndDate = lcl_EndDate
        StartDate = lcl_StartDate
        DurationSec = lcl_DurationSec
        SentTo = lcl_SentTo
        SentCC = lcl_SentCC
        SentFrom = lcl_SentFrom
        Body = lcl_Body
        Importance = lcl_Importance
        Categories = lcl_Categories.ToString()
        strAction = lcl_strAction

        If Err.Number = 0 Then
            Init = 1
        Else
            Init = 0
        End If
    End Function

    Friend Function Init_wMail(
            OlMail As MailItem,
            Optional OlEndTime As Date = Nothing,
            Optional lngDurationSec As Long = 0,
            Optional stringAction As String = "") As Boolean


        On Error Resume Next

        Subject = OlMail.Subject
        If OlEndTime <> Nothing Then EndDate = OlEndTime
        If lngDurationSec Then DurationSec = lngDurationSec
        SentTo = OlMail.To
        SentCC = OlMail.CC
        SentFrom = OlMail.Sender.ToString()
        Body = OlMail.Body
        Importance = OlMail.Importance
        Categories = OlMail.Categories
        If stringAction <> "" Then strAction = stringAction

        If Err.Number = 0 Then
            Init_wMail = True
        Else
            Init_wMail = False
            Debug.WriteLine(Err.Description)
            Err.Clear()
        End If

    End Function

    Public Property EndDate As Date
        Get
            Return _endDate
        End Get
        Set(value As Date)
            _endDate = value
            StartDate = DateAdd("s", -_durationSec, _endDate)
        End Set
    End Property

    Public Property DurationSec As Long
        Get
            Return _durationSec
        End Get
        Set(value As Long)
            _durationSec = value
            StartDate = DateAdd("s", -_durationSec, _endDate)
        End Set
    End Property

    Public Shadows ReadOnly Property ToString As String
        Get
            Dim strTemp As String
            Dim lngSeconds As Double
            Dim lngSeconds2 As Double
            Dim lngMinutes As Double
            Dim lngMinutes2 As Double

            lngSeconds = DateDiff("s", StartDate, _endDate)
            lngMinutes = Math.Round((lngSeconds / 60) - 0.5, 0)
            lngSeconds2 = lngSeconds - lngMinutes * 60
            lngMinutes2 = lngSeconds / 60

            If strAction = "EventLog" Then
                strTemp = Format(StartDate, "General Date") &
                    " TO " & Format(_endDate, "h:mm:ss AM/PM") &
                    "| DUR: " &
                    lngMinutes & " minutes " & lngSeconds2 & " seconds" &
                    " |" & Format(lngMinutes2, "##0.0000") &
                    " | " & "APP: " & Subject &
                    " | " & "PROC: " & strProcName

            ElseIf strAction = "ToDo" Then
                strTemp = "|" & Format(_endDate, "General Date") &
                    "| Duration: " &
                    lngMinutes & " minutes " & lngSeconds2 & " seconds" &
                    " |" & Format(lngMinutes2, "##0.0000") &
                    " | Subject: " & Subject
            Else
                strTemp = "|" & Format(_endDate, "General Date") &
                    "| Duration: " &
                    lngMinutes & " minutes " & lngSeconds2 & " seconds" &
                    " |" & Format(lngMinutes2, "##0.0000") &
                    "| Action: " & strAction &
                    " | Subject: " & Subject &
                    " | From: " & SentFrom &
                    " | To: " & SentTo
            End If
            ToString = strTemp
        End Get

    End Property

End Class
