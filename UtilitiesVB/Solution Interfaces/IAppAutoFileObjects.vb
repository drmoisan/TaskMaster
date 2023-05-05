Imports UtilitiesCS

Public Interface IAppAutoFileObjects
    Property Conversation_Weight As Long
    Property LngConvCtPwr As Long
    Property MaxRecents As Long
    Property RecentsList As IRecentsList(Of String)
    Property CTFList As CtfIncidenceList
    Property CommonWords As ISerializableList(Of String)
    Property SuggestionFilesLoaded As Boolean
    Property SmithWatterman_MatchScore As Integer
    Property SmithWatterman_MismatchScore As Integer
    Property SmithWatterman_GapPenalty As Integer
End Interface
