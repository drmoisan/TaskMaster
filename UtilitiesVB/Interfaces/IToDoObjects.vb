
Public Interface IToDoObjects
    ReadOnly Property DictPPL As IPeopleDict
    ReadOnly Property DictRemap As Dictionary(Of String, String)
    ReadOnly Property IDList As IListOfIDs
    ReadOnly Property Parent As IApplicationGlobals
    ReadOnly Property ProjInfo As IProjectInfo
    ReadOnly Property FnameProjectInfo As String
    ReadOnly Property FnameDictPeople As String
    ReadOnly Property FnameDictRemap As String
    ReadOnly Property FnameIDList As String

End Interface
