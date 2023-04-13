Imports UtilitiesVB
Imports UtilitiesCS

Public Class AppAutoFileObjects
    Implements IAppAutoFileObjects

    Private _suggestionFilesLoaded As Boolean = False
    Private _smithWatterman_MatchScore As Integer
    Private _smithWatterman_MismatchScore As Integer
    Private _smithWatterman_GapPenalty As Integer
    Private _recentsList As IRecentsList(Of String)
    Private _parent As IApplicationGlobals

    Public Sub New(ParentInstance As ApplicationGlobals)
        _parent = ParentInstance
    End Sub

    Public Property LngConvCtPwr As Long Implements IAppAutoFileObjects.LngConvCtPwr
        Get
            Return My.Settings.ConversationExponent
        End Get
        Set(value As Long)
            My.Settings.ConversationExponent = value
            My.Settings.Save()
        End Set
    End Property

    Public Property Conversation_Weight As Long Implements IAppAutoFileObjects.Conversation_Weight
        Get
            Return My.Settings.ConversationWeight
        End Get
        Set(value As Long)
            My.Settings.ConversationWeight = value
            My.Settings.Save()
        End Set
    End Property

    Public Property SuggestionFilesLoaded As Boolean Implements IAppAutoFileObjects.SuggestionFilesLoaded
        Get
            Return _suggestionFilesLoaded
        End Get
        Set(value As Boolean)
            _suggestionFilesLoaded = value
        End Set
    End Property

    Public Property SmithWatterman_MatchScore As Integer Implements IAppAutoFileObjects.SmithWatterman_MatchScore
        Get
            Return My.Settings.SmithWatterman_MatchScore
        End Get
        Set(value As Integer)
            My.Settings.SmithWatterman_MatchScore = value
            My.Settings.Save()
        End Set
    End Property

    Public Property SmithWatterman_MismatchScore As Integer Implements IAppAutoFileObjects.SmithWatterman_MismatchScore
        Get
            Return My.Settings.SmithWatterman_MismatchScore
        End Get
        Set(value As Integer)
            My.Settings.SmithWatterman_MismatchScore = value
            My.Settings.Save()
        End Set
    End Property

    Public Property SmithWatterman_GapPenalty As Integer Implements IAppAutoFileObjects.SmithWatterman_GapPenalty
        Get
            Return My.Settings.SmithWatterman_GapPenalty
        End Get
        Set(value As Integer)
            My.Settings.SmithWatterman_GapPenalty = value
            My.Settings.Save()
        End Set
    End Property

    Public Property MaxRecents As Long Implements IAppAutoFileObjects.MaxRecents
        Get
            Return My.Settings.MaxRecents
        End Get
        Set(value As Long)
            My.Settings.MaxRecents = value
            My.Settings.Save()
        End Set
    End Property

    Public Property RecentsList As IRecentsList(Of String) Implements IAppAutoFileObjects.RecentsList
        Get
            If _recentsList Is Nothing Then
                _recentsList = New RecentsList(Of String)(My.Settings.FileName_Recents, _parent.FS.FldrFlow, max:=MaxRecents)
            End If
            Return _recentsList
        End Get
        Set(value As IRecentsList(Of String))
            _recentsList = value
            With _recentsList
                If .Folderpath = "" Then
                    .Folderpath = _parent.FS.FldrFlow
                    .Filename = My.Settings.FileName_Recents
                End If
            End With
            _recentsList.Serialize()
        End Set
    End Property


End Class
