Imports UtilitiesVB

Public Class AppStagingFilenames
    Implements IAppStagingFilenames

    Private _recentsFile As String = "9999999RecentsFile.txt"
    Private _emailMoves As String = "999999EmailMoves.tsv"
    Private _emailSession As String = "99999EmailSession.csv"
    Private _emailSessionTemp As String = "99999EmailSession_Tmp.csv"
    Private _ctfMap As String = "9999999CTF_Map.txt"
    Private _ctfInc As String = "9999999CTF_Inc.txt"
    Private _subjectMap As String = "9999999Subject_Map.txt"
    Private _commonWords As String = "9999999CommonWords.txt"
    Private _conditionalReminders As String = "999999ConditionalReminders.txt"

    Public Property ConditionalReminders As String Implements IAppStagingFilenames.ConditionalReminders
        Get
            Return _conditionalReminders
        End Get
        Set(value As String)
            _conditionalReminders = value
        End Set
    End Property

    Public Property CommonWords As String Implements IAppStagingFilenames.CommonWords
        Get
            Return _commonWords
        End Get
        Set(value As String)
            _commonWords = value
        End Set
    End Property

    Public Property SubjectMap As String Implements IAppStagingFilenames.SubjectMap
        Get
            Return _subjectMap
        End Get
        Set(value As String)
            _subjectMap = value
        End Set
    End Property

    Public Property CtfInc As String Implements IAppStagingFilenames.CtfInc
        Get
            Return _ctfInc
        End Get
        Set(value As String)
            _ctfInc = value
        End Set
    End Property

    Public Property CtfMap As String Implements IAppStagingFilenames.CtfMap
        Get
            Return _ctfMap
        End Get
        Set(value As String)
            _ctfMap = value
        End Set
    End Property

    Public Property EmailSessionTemp As String Implements IAppStagingFilenames.EmailSessionTemp
        Get
            Return _emailSessionTemp
        End Get
        Set(value As String)
            _emailSessionTemp = value
        End Set
    End Property

    Public Property EmailSession As String Implements IAppStagingFilenames.EmailSession
        Get
            Return _emailSession
        End Get
        Set(value As String)
            _emailSession = value
        End Set
    End Property

    Public Property EmailMoves As String Implements IAppStagingFilenames.EmailMoves
        Get
            Return _emailMoves
        End Get
        Set(value As String)
            _emailMoves = value
        End Set
    End Property

    Public Property RecentsFile As String Implements IAppStagingFilenames.RecentsFile
        Get
            Return _recentsFile
        End Get
        Set(value As String)
            _recentsFile = value
        End Set
    End Property
End Class
