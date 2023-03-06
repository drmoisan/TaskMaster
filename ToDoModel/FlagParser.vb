''' <summary>
''' Class converts color categories to flags relevant to People, Projects, Topics, Context, etc
''' </summary>
Public Class FlagParser

    Private ReadOnly _people As New FlagDetails(My.Settings.Prefix_People)
    Private ReadOnly _projects As New FlagDetails(My.Settings.Prefix_Project)
    Private ReadOnly _topics As New FlagDetails(My.Settings.Prefix_Topic)
    Private ReadOnly _context As New FlagDetails(My.Settings.Prefix_Context)
    Private ReadOnly _kb As New FlagDetails(My.Settings.Prefix_KB)
    Public other As String = ""
    Public today As Boolean = False
    Public bullpin As Boolean = False

    ''' <summary>
    ''' Constructor for the FlagParser class accepts a comma delimited string containing 
    ''' color categories and initializes
    ''' </summary>
    ''' <param name="strCats_All"></param>
    ''' <param name="DeleteSearchSubString"></param>
    Public Sub New(ByRef strCats_All As String, Optional DeleteSearchSubString As Boolean = False)
        If strCats_All Is Nothing Then strCats_All = ""
        'Splitter(strCats_All, DeleteSearchSubString)
        InitFromString(strCats_All)
    End Sub

    ''' <summary>
    ''' Function tests to see if a string begins with a prefix
    ''' </summary>
    ''' <param name="test_string"></param>
    ''' <param name="prefix"></param>
    ''' <returns>True if present. False if not present.</returns>
    Private Function PrefixPresent(test_string As String, prefix As String) As Boolean
        Return Left(test_string, prefix.Length) = prefix
    End Function

    Private Sub InitFromString(ByRef strCats_All As String)
        Dim list_categories As List(Of String) = SplitToList(strCats_All, ",")
        _people.List = FindMatches(list_categories, _people.prefix)
        _projects.List = FindMatches(list_categories, _projects.prefix)
        _topics.List = FindMatches(list_categories, _topics.prefix)
        _context.List = FindMatches(list_categories, _context.prefix)
        _kb.List = FindMatches(list_categories, _kb.prefix)

        list_categories = list_categories.Except(_people.ListWithPrefix) _
                                         .Except(_projects.ListWithPrefix) _
                                         .Except(_topics.ListWithPrefix) _
                                         .Except(_context.ListWithPrefix) _
                                         .Except(_kb.ListWithPrefix) _
                                         .ToList()

        If list_categories.Contains(My.Settings.Prefix_Today) Then
            today = True
            Dim unused1 = list_categories.Remove(My.Settings.Prefix_Today)
        Else
            today = False
        End If

        If list_categories.Contains(My.Settings.Prefix_Bullpin) Then
            bullpin = True
            Dim unused = list_categories.Remove(My.Settings.Prefix_Bullpin)
        Else
            bullpin = False
        End If

        other = If(list_categories.Count > 0, String.Join(", ", list_categories), "")

    End Sub

    Public Property KB(Optional IncludePrefix As Boolean = False) As String
        Get
            Return If(IncludePrefix, _kb.WithPrefix, _kb.NoPrefix)
        End Get
        Set(value As String)
            _kb.List = SplitToList(value, ",", _kb.prefix)
        End Set
    End Property

    ''' <summary>
    ''' Property accesses a private instance of FlagDetails. 
    ''' SET splits a comma delimited String to a list excluding 
    ''' the prefix which is passed to the FlagDetails class.
    ''' </summary>
    ''' <param name="IncludePrefix">Determines whether GET includes the category prefix</param>
    ''' <returns>A string containing a comma separated Context names</returns>
    Public Property Context(Optional IncludePrefix As Boolean = False) As String
        Get
            Return If(IncludePrefix, _context.WithPrefix, _context.NoPrefix)
        End Get
        Set(value As String)
            _context.List = SplitToList(value, ",", _context.prefix)
        End Set
    End Property

    Public ReadOnly Property ContextList As List(Of String)
        Get
            Return _context.List
        End Get
    End Property

    ''' <summary>
    ''' Property accesses a private instance of FlagDetails. 
    ''' SET splits a comma delimited String to a list excluding 
    ''' the prefix which is passed to the FlagDetails class.
    ''' </summary>
    ''' <param name="IncludePrefix">Determines whether GET includes the category prefix</param>
    ''' <returns>A string containing a comma separated Project names</returns>
    Public Property Projects(Optional IncludePrefix As Boolean = False) As String
        Get
            Return If(IncludePrefix, _projects.WithPrefix, _projects.NoPrefix)
        End Get
        Set(value As String)
            _projects.List = SplitToList(value, ",", _projects.prefix)
        End Set
    End Property

    Public ReadOnly Property ProjectList As List(Of String)
        Get
            Return _projects.List
        End Get
    End Property

    ''' <summary>
    ''' Property accesses a private instance of FlagDetails. 
    ''' SET splits a comma delimited String to a list excluding 
    ''' the prefix which is passed to the FlagDetails class.
    ''' </summary>
    ''' <param name="IncludePrefix">Determines whether GET includes the category prefix</param>
    ''' <returns>A string containing a comma separated Topic names</returns>
    Public Property Topics(Optional IncludePrefix As Boolean = False) As String
        Get
            Return If(IncludePrefix, _topics.WithPrefix, _topics.NoPrefix)
        End Get
        Set(value As String)
            _topics.List = SplitToList(value, ",", _topics.prefix)
        End Set
    End Property

    Public ReadOnly Property TopicList As List(Of String)
        Get
            Return _topics.List
        End Get
    End Property

    ''' <summary>
    ''' Property accesses a private instance of FlagDetails. 
    ''' SET splits a comma delimited String to a list excluding 
    ''' the prefix which is passed to the FlagDetails class.
    ''' </summary>
    ''' <param name="IncludePrefix">Determines whether GET includes the category prefix</param>
    ''' <returns>A string containing a comma separated Topic names</returns>
    Public Property People(Optional IncludePrefix As Boolean = False) As String
        Get
            Return If(IncludePrefix, _people.WithPrefix, _people.NoPrefix)
        End Get
        Set(value As String)
            _people.List = SplitToList(value, ",", _people.prefix)
        End Set
    End Property

    Public ReadOnly Property PeopleList As List(Of String)
        Get
            Return _people.List
        End Get
    End Property

    Private Function AppendDetails(base As String, details As FlagDetails, wtag As Boolean) As String
        Return If(details.WithPrefix.Length = 0, base, If(wtag, base & ", " & details.WithPrefix, base & ", " & details.NoPrefix))
    End Function

    ''' <summary>
    ''' Function recombines flag settings in one comma delimited string representing color categories
    ''' </summary>
    ''' <returns>A string containing color categories</returns>
    Public Function Combine(Optional wtag As Boolean = True) As String
        Dim string_return As String = ""
        string_return = AppendDetails(string_return, _people, wtag)
        string_return = AppendDetails(string_return, _projects, wtag)
        string_return = AppendDetails(string_return, _topics, wtag)
        string_return = AppendDetails(string_return, _context, wtag)
        string_return = AppendDetails(string_return, _kb, wtag)

        If today Then string_return = string_return & ", " & "Tag A Top Priority Today"
        If bullpin Then string_return = string_return & ", " & "Tag Bullpin Priorities"


        If string_return.Length > 2 Then
            string_return = Right(string_return, string_return.Length - 2)
        End If

        Return string_return
    End Function

    ''' <summary>
    ''' Subroutine extracts flag settings from color categories and loads to internal variables
    ''' </summary>
    ''' <param name="strCats_All">String containing comma delimited color categories</param>
    ''' <param name="DeleteSearchSubString"></param>
    Public Sub Splitter(ByRef strCats_All As String, Optional DeleteSearchSubString As Boolean = False)
        _people.WithPrefix = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag PPL "), ", ", DeleteSearchSubString:=DeleteSearchSubString)
        other = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag PPL "), ", ", True)

        _projects.WithPrefix = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag PROJECT "), ", ", DeleteSearchSubString:=DeleteSearchSubString)
        other = SubStr_w_Delimeter(other, AddWildcards("Tag PROJECT "), ", ", True)

        Dim strTemp As String = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag Bullpin Priorities"), ", ", DeleteSearchSubString:=False)
        other = SubStr_w_Delimeter(other, AddWildcards("Tag Bullpin Priorities"), ", ", True)
        bullpin = strTemp <> ""

        strTemp = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag A Top Priority Today"), ", ", DeleteSearchSubString:=False)
        other = SubStr_w_Delimeter(other, AddWildcards("Tag A Top Priority Today"), ", ", True)
        today = strTemp <> ""

        _topics.WithPrefix = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag TOPIC "), ", ", DeleteSearchSubString:=DeleteSearchSubString)
        other = SubStr_w_Delimeter(other, AddWildcards("Tag TOPIC "), ", ", True)

        KB = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag KB "), ", ", DeleteSearchSubString:=DeleteSearchSubString)
        other = SubStr_w_Delimeter(other, AddWildcards("Tag KB "), ", ", True)

        Context = other

    End Sub

    ''' <summary>
    ''' Function adds wildcards to a seach string
    ''' </summary>
    ''' <param name="strOriginal">A search string</param>
    ''' <param name="b_Leading">If true, a wildcard is added at the beginning</param>
    ''' <param name="b_Trailing">If true, a wildcard is added at the end</param>
    ''' <param name="charWC">Character representing wildcard. Default is *</param>
    ''' <returns>A search string with wildcards added</returns>
    Public Function AddWildcards(ByVal strOriginal As String, Optional b_Leading As Boolean = True,
    Optional b_Trailing As Boolean = True, Optional charWC As String = "*") As String

        Dim strTemp As String
        strTemp = strOriginal
        If b_Leading Then strTemp = charWC & strTemp
        If b_Trailing Then strTemp &= charWC

        AddWildcards = strTemp

    End Function

    Private Function SplitToList(MainString As String,
                                 Delimiter As String,
                                 Optional ReplaceString As String = "XXXXX") As List(Of String)
        Dim list_return As List(Of String)
        If MainString Is Nothing Then
            list_return = New List(Of String)
        ElseIf MainString = "" Then
            list_return = New List(Of String)
        Else
            list_return = MainString.Split(Delimiter) _
                                    .Select(Function(x) x _
                                    .Replace(ReplaceString, "").Trim) _
                                    .ToList()
        End If
        Return list_return
    End Function

    Private Function FindMatches(source As List(Of String),
                                 substring As String,
                                 Optional return_nonmatches As Boolean = False
                                 ) As List(Of String)

        Dim list_return = If(return_nonmatches,
            source.Where(
                Function(x) x.IndexOf(substring, StringComparison.OrdinalIgnoreCase
                ) = -1).Select(Function(x) x).ToList(),
            source.Where(
                Function(x) x.IndexOf(substring, StringComparison.OrdinalIgnoreCase
                ) <> -1).Select(Function(x) x.Replace(substring, "")).ToList())
        Return list_return

    End Function

    Public Function SubStr_MatchList_w_Delimiter(
        MainString As String,
        SubString As String,
        Delimiter As String,
        Optional bNotSearchStr As Boolean = False,
        Optional DeleteSearchSubString As Boolean = True
        ) As List(Of String)

        Dim str_array = MainString.Split(Delimiter)
        Dim filtered_array = SearchArry4Str(str_array, SubString, bNotSearchStr,
                                            DeleteSearchSubString:=DeleteSearchSubString)
        Dim match_list As List(Of String) = TryCast(filtered_array, List(Of String))
        Return match_list
    End Function

    ''' <summary>
    ''' Extract: Function accepts a comma delimited string and converts to an array of strings
    ''' Transform: Function selects members of the array that match the substring
    ''' Load: Function returns a comma delimited string containing matching elements
    ''' </summary>
    ''' <param name="strMainString">A comma delimited string that will be searched</param>
    ''' <param name="strSubString">Target substring to find</param>
    ''' <param name="strDelimiter">String used as delimiter</param>
    ''' <param name="bNotSearchStr">Boolean flag that inverts the search to return 
    ''' elements that don't match</param>
    ''' <param name="DeleteSearchSubString">Boolean that determines if return value 
    ''' eliminates substring from each match</param>
    ''' <returns></returns>
    Public Function SubStr_w_Delimeter(strMainString As String, strSubString As String, strDelimiter As String, Optional bNotSearchStr As Boolean = False, Optional DeleteSearchSubString As Boolean = False) As String
        Dim varTempStrAry As Object
        Dim varFiltStrAry As Object
        Dim strTempStr As String

        varTempStrAry = strMainString.Split(strDelimiter)
        varFiltStrAry = SearchArry4Str(varTempStrAry, strSubString, bNotSearchStr, DeleteSearchSubString:=DeleteSearchSubString)
        strTempStr = Condense_Variant_To_Str(varFiltStrAry)

        SubStr_w_Delimeter = strTempStr

    End Function

    ''' <summary>
    ''' Function accepts a pointer to a string array and searches for a substring.
    ''' It returns a pointer to a new string array containing matches 
    ''' </summary>
    ''' <param name="varStrArry">Pointer to the string array to search</param>
    ''' <param name="SearchStr$">Target substring to search</param>
    ''' <param name="bNotSearchStr">Boolean flag that inverts the search to return 
    ''' any element that doesn't match</param>
    ''' <param name="DeleteSearchSubString">Boolean that removes </param>
    ''' <returns>Pointer to a string array with elements that match the criteria</returns>
    Public Function SearchArry4Str(ByRef varStrArry As Object, Optional SearchStr$ = "", Optional bNotSearchStr As Boolean = False, Optional DeleteSearchSubString As Boolean = False) As Object
        Dim m_Find As String
        Dim m_Wildcard As Boolean

        Dim strCats() As String
        Dim i As Integer
        Dim intFoundCt As Integer
        Dim boolFound As Boolean
        Dim strTemp As String
        Dim strSearchNoWC As String

        If Len(Trim$(SearchStr)) <> 0 Then

            ReDim strCats(0)
            m_Find = SearchStr

            'Make lower case
            m_Find = LCase$(m_Find)

            'Standardize characters used as wildcards
            m_Find = Replace(m_Find, "%", "*")

            'Determine if wildcards are present in search string
            m_Wildcard = InStr(m_Find, "*")

            intFoundCt = 0

            'Remove wildcards from the string
            strSearchNoWC = Replace(SearchStr, "*", "")

            'Loop through the array to find substring
            For i = LBound(varStrArry) To UBound(varStrArry)
                boolFound = False

                'Skip over blank entries
                If varStrArry(i) <> "" Then
                    boolFound = If(m_Wildcard,
                        If(bNotSearchStr = False, DirectCast(LCase$(varStrArry(i)) Like m_Find, Boolean), DirectCast(Not LCase$(varStrArry(i)) Like m_Find, Boolean)),
                        If(bNotSearchStr = False, DirectCast(LCase$(varStrArry(i)) = m_Find, Boolean), DirectCast(Not LCase$(varStrArry(i)) = m_Find, Boolean)))
                End If

                If boolFound Then
                    intFoundCt += 1
                    ReDim Preserve strCats(intFoundCt)
                    strTemp = varStrArry(i)
                    If DeleteSearchSubString Then strTemp = Replace(strTemp, strSearchNoWC, "", , , vbTextCompare)
                    strCats(intFoundCt) = strTemp
                End If
            Next i

            SearchArry4Str = If(intFoundCt = 0, "", strCats)

        Else
            SearchArry4Str = varStrArry
        End If


    End Function

    ''' <summary>
    ''' Function accepts a pointer to a string array and collapses into a comma delimited string
    ''' </summary>
    ''' <param name="varAry">Pointer to string array</param>
    ''' <returns>A comma delimited string</returns>
    Public Function Condense_Variant_To_Str(varAry As Object) As String
        Dim strTempStr As String = ""
        Dim i As Integer

        If IsArray(varAry) Then
            For i = 1 To UBound(varAry)
                strTempStr = strTempStr & ", " & varAry(i)
            Next i
            If strTempStr <> "" Then strTempStr = Right(strTempStr, Len(strTempStr) - 2)
        Else
            strTempStr = varAry
        End If

        Condense_Variant_To_Str = strTempStr

    End Function

End Class


Public Class FlagDetails
    Private _list As RestrictedList(Of String)
    Public prefix As String

    Public Sub New()
    End Sub

    Public Sub New(prefix As String)
        Me.prefix = prefix
    End Sub

    Public Property List As List(Of String)
        Get
            Return _list
        End Get
        Set(value As List(Of String))
            Dim TmpList As List(Of String)
            If value Is Nothing Then
                TmpList = New List(Of String)
            ElseIf value.Count = 0 Then
                TmpList = value
            ElseIf Strings.Left(value(0), prefix.Length) = prefix Then
                TmpList = value.Select(Function(x) x.Replace(prefix, "")).ToList()
            Else
                TmpList = value
            End If
            _list = New RestrictedList(Of String)(TmpList, Me)
            ListChange_Refresh()
        End Set
    End Property

    Public ReadOnly Property ListWithPrefix() As List(Of String)
        Get
            Return _list.Select(Function(x) prefix & x).ToList()
        End Get
    End Property

    Private Sub ListChange_Refresh()
        WithPrefix = String.Join(", ", _list.Select(Function(x) prefix & x))
        NoPrefix = String.Join(", ", _list)
    End Sub

    Public Property WithPrefix As String

    Public Property NoPrefix As String

    Private NotInheritable Class RestrictedList(Of T) : Inherits List(Of T)
        'Implements ICloneable

        Private ReadOnly outer As FlagDetails

        Public Sub New(ByVal wrapped_list As List(Of T), outer As FlagDetails)
            MyBase.New(wrapped_list)
            If wrapped_list Is Nothing Then
                Throw New ArgumentNullException("wrapped_list")
            End If
            Me.outer = outer
        End Sub

        Public Overloads Sub Add(ByVal item As T)
            MyBase.Add(item)
            outer.ListChange_Refresh()
        End Sub

        Public Overloads Sub Remove(ByVal item As T)
            Dim unused = MyBase.Remove(item)
            outer.ListChange_Refresh()
        End Sub

        'Public Function ToClonedList() As List(Of T)
        '    Dim ClonedList As List(Of T) = TryCast(Me.Clone(), List(Of T))
        '    Return ClonedList
        'End Function

        'Private Function Clone() As Object Implements ICloneable.Clone
        '    Return MyBase.MemberwiseClone()
        'End Function
    End Class

End Class

