Imports Newtonsoft.Json
Imports System.IO
Imports ToDoModel
Imports UtilitiesVB
Imports UtilitiesCS

Public Class AppToDoObjects
    Implements IToDoObjects

    Private _projInfo As ProjectInfo
    Private _dictPPL As Dictionary(Of String, String)
    Private _IDList As ListOfIDs
    Private ReadOnly _parent As ApplicationGlobals
    Private _dictRemap As Dictionary(Of String, String)
    Private _catFilters As SerializableList(Of String)

    Public Sub New(ParentInstance As ApplicationGlobals)
        _parent = ParentInstance
    End Sub

    Public ReadOnly Property Parent As IApplicationGlobals Implements IToDoObjects.Parent
        Get
            Return _parent
        End Get
    End Property

    Public ReadOnly Property ProjInfo_Filename As String Implements IToDoObjects.ProjInfo_Filename
        Get
            Return My.Settings.FileName_ProjInfo
        End Get
    End Property

    Public ReadOnly Property ProjInfo As IProjectInfo Implements IToDoObjects.ProjInfo
        Get
            If _projInfo Is Nothing Then
                _projInfo = LoadToDoProjectInfo(Path.Combine(Parent.FS.FldrAppData, My.Settings.FileName_ProjInfo))
            End If
            Return _projInfo
        End Get
    End Property


    Public ReadOnly Property DictPPL_Filename As String Implements IToDoObjects.DictPPL_Filename
        Get
            Return My.Settings.FilenameDictPpl
        End Get
    End Property

    Public ReadOnly Property DictPPL As Dictionary(Of String, String) Implements IToDoObjects.DictPPL
        Get
            If _dictPPL Is Nothing Then
                _dictPPL = LoadDictJSON(Parent.FS.FldrStaging, DictPPL_Filename)
            End If
            Return _dictPPL
        End Get
    End Property

    Public Sub DictPPL_Save() Implements IToDoObjects.DictPPL_Save
        File.WriteAllText(
            Path.Combine(Parent.FS.FldrStaging, DictPPL_Filename),
            JsonConvert.SerializeObject(_dictPPL, Formatting.Indented))
    End Sub

    Public ReadOnly Property FnameIDList As String Implements IToDoObjects.FnameIDList
        Get
            Return My.Settings.FileName_IDList
        End Get
    End Property

    Public ReadOnly Property IDList As IListOfIDs Implements IToDoObjects.IDList
        Get
            If _IDList Is Nothing Then
                _IDList = New ListOfIDs(
                    Path.Combine(Parent.FS.FldrAppData,
                                 My.Settings.FileName_IDList),
                    _parent.Ol.App)
            End If
            Return _IDList
        End Get
    End Property

    Public ReadOnly Property FnameDictRemap As String Implements IToDoObjects.FnameDictRemap
        Get
            Return My.Settings.FileName_DictRemap
        End Get
    End Property

    Public ReadOnly Property DictRemap As Dictionary(Of String, String) Implements IToDoObjects.DictRemap
        Get
            If _dictRemap Is Nothing Then
                _dictRemap = LoadDictCSV(Parent.FS.FldrStaging, My.Settings.FileName_DictRemap)
            End If
            Return _dictRemap
        End Get
    End Property

    Public ReadOnly Property CategoryFilters As ISerializableList(Of String) Implements IToDoObjects.CategoryFilters
        Get
            If _catFilters Is Nothing Then
                Dim _catFilters = New SerializableList(Of String)
                With _catFilters
                    .Filename = My.Settings.FileName_CategoryFilters
                    .Folderpath = Parent.FS.FldrAppData
                    If File.Exists(.Folderpath) Then
                        .Deserialize()
                    Else
                        Dim tempList = New SerializableList(Of String)(CCOCatList_Load())
                        tempList.Folderpath = .Folderpath
                        _catFilters = tempList
                        .Serialize()
                    End If
                End With
            End If
            Return _catFilters
        End Get
    End Property

    Private Function LoadDictCSV(fpath As String,
                                 filename As String) _
                                 As Dictionary(Of String, String)
        Dim dict As Dictionary(Of String, String) = UtilitiesVB.LoadDictCSV(fpath, filename.Split(".")(0) & ".csv")
        If dict IsNot Nothing Then WriteDictJSON(dict, Path.Combine(fpath, filename))
        Return dict
    End Function

    Private Function LoadDictJSON(fpath As String,
                                  filename As String) _
                                  As Dictionary(Of String, String)

        Dim filepath As String = Path.Combine(fpath, filename)
        Dim dict As Dictionary(Of String, String) = Nothing
        Dim response As MsgBoxResult = MsgBoxResult.Ignore

        Try
            dict = JsonConvert.DeserializeObject(Of Dictionary(Of String, String)) _
                    (File.ReadAllText(Path.Combine(Parent.FS.FldrStaging, DictPPL_Filename)))
        Catch ex As FileNotFoundException
            response = MsgBox(filepath & "not found. Load from CSV?", vbYesNo)
        Catch ex As System.Exception
            response = MsgBox(filepath & "encountered a problem. " & ex.Message & "Load from CSV?", vbYesNo)
        Finally
            If response = vbYes Then
                dict = LoadDictCSV(fpath, filename)
            ElseIf response = vbNo Then
                response = MsgBox("Start a new blank dictionary?", vbYesNo)
                If response = vbYes Then
                    dict = New Dictionary(Of String, String)
                Else
                    Throw New ArgumentNullException("Cannot proceed without dictionary: " & filename)
                End If
            End If
        End Try
        Return dict
    End Function

    Public Sub WriteDictJSON(dict As Dictionary(Of String, String), filepath As String)
        File.WriteAllText(filepath, JsonConvert.SerializeObject(dict, Formatting.Indented))
    End Sub
End Class
