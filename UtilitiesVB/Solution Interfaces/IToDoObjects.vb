
Public Interface IToDoObjects
    ReadOnly Property DictPPL As Dictionary(Of String, String)
    ReadOnly Property DictPPL_Filename As String
    Sub DictPPL_Save()
    ReadOnly Property DictRemap As Dictionary(Of String, String)
    ReadOnly Property IDList As IListOfIDs
    Sub IDList_Refresh()
    ReadOnly Property Parent As IApplicationGlobals
    ReadOnly Property ProjInfo As IProjectInfo
    ReadOnly Property ProjInfo_Filename As String
    ReadOnly Property FnameDictRemap As String
    ReadOnly Property FnameIDList As String

End Interface
