''' <summary>
''' Class converts color categories to flags relevant to People, Projects, Topics, context, etc
''' </summary>
Public Class FlagParser
    Private _people As String = ""
    Private _projects As String = ""
    Private _topics As String = ""
    Public context As String = ""
    Public kb As String = ""
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
        Splitter(strCats_All, DeleteSearchSubString)
    End Sub

    ''' <summary>
    ''' Property accesses the private variable _projects
    ''' Set 
    '''     Extract: Split comma delimited String to array of project names
    '''     Transform: Iterate through array and append a prefix if not present 
    '''     Load: Recombine in string and store value in _projects
    ''' Get accesses the value stored in _projects
    ''' </summary>
    ''' <param name="IncludePrefix">Determines whether the return value includes the category prefix</param>
    ''' <returns>A string containing a comma separated Project names</returns>
    Public Property Projects(Optional IncludePrefix As Boolean = False) As String
        Get
            Dim Prefix As String = "Tag PROJECT "
            Dim strReturn As String = _projects

            If IncludePrefix = False Then
                strReturn = SubStr_w_Delimeter(strReturn, Prefix, ", ", DeleteSearchSubString:=True)
            End If
            Return strReturn
        End Get

        Set(value As String)
            Dim Prefix As String = "Tag PROJECT "

            Dim strReturn As String = ""
            If value = "" Then
                strReturn = ""
            ElseIf Left(value, Prefix.Length) <> Prefix Then
                Dim strTmp() As String = value.Split(", ")
                For i As Integer = LBound(strTmp) To UBound(strTmp)
                    strReturn = strReturn & ", " & Prefix & Trim(strTmp(i))
                Next
                If strReturn.Length > 2 Then
                    strReturn = Right(strReturn, strReturn.Length - 2)
                End If
            Else
                strReturn = value
            End If
            _projects = strReturn
        End Set
    End Property

    ''' <summary>
    ''' Property accesses the private variable _topics
    ''' Set 
    '''     Extract: Split comma delimited String to array of Topic names
    '''     Transform: Iterate through array and append a prefix if not present 
    '''     Load: Recombine in string and store value in _topics
    ''' Get accesses the value stored in _topics
    ''' </summary>
    ''' <param name="IncludePrefix">Determines whether the return value includes the category prefix</param>
    ''' <returns>A string containing a comma separated Topic names</returns>
    Public Property Topics(Optional IncludePrefix As Boolean = False) As String
        Get
            Dim Prefix As String = "Tag TOPIC "
            Dim strReturn As String = _topics

            If IncludePrefix = False Then
                strReturn = SubStr_w_Delimeter(strReturn, Prefix, ", ", DeleteSearchSubString:=True)
            End If
            Return strReturn
        End Get

        Set(value As String)
            Dim Prefix As String = "Tag TOPIC "

            Dim strReturn As String = ""
            If value = "" Then
                strReturn = ""
            ElseIf Left(value, Prefix.Length) <> Prefix Then
                Dim strTmp() As String = value.Split(", ")
                For i As Integer = LBound(strTmp) To UBound(strTmp)
                    strReturn = strReturn & ", " & Prefix & Trim(strTmp(i))
                Next
                If strReturn.Length > 2 Then
                    strReturn = Right(strReturn, strReturn.Length - 2)
                End If
            Else
                strReturn = value
            End If
            _topics = strReturn
        End Set
    End Property

    ''' <summary>
    ''' Property accesses the private variable _people
    ''' Set 
    '''     Extract: Split comma delimited String to array of People names
    '''     Transform: Iterate through array and append a prefix if not present 
    '''     Load: Recombine in string and store value in _people
    ''' Get accesses the value stored in _people
    ''' </summary>
    ''' <param name="IncludePrefix"></param>
    ''' <returns>A string containing a comma separated People names</returns>
    Public Property People(Optional IncludePrefix As Boolean = False) As String
        Get
            Dim Prefix As String = "Tag PPL "
            Dim strReturn As String = _people

            If IncludePrefix = False Then
                strReturn = SubStr_w_Delimeter(strReturn, Prefix, ", ", DeleteSearchSubString:=True)
            End If
            Return strReturn
        End Get

        Set(value As String)
            Dim Prefix As String = "Tag PPL "

            Dim strReturn As String = ""
            If value = "" Then
                strReturn = ""
            ElseIf Left(value, Prefix.Length) <> Prefix Then
                Dim strTmp() As String = value.Split(", ")
                For i As Integer = LBound(strTmp) To UBound(strTmp)
                    strReturn = strReturn & ", " & Prefix & Trim(strTmp(i))
                Next
                If strReturn.Length > 2 Then
                    strReturn = Right(strReturn, strReturn.Length - 2)
                End If
            Else
                strReturn = value
            End If
            _people = strReturn
        End Set
    End Property

    ''' <summary>
    ''' Function recombines flag settings in one comma delimited string representing color categories
    ''' </summary>
    ''' <returns>A string containing color categories</returns>
    Public Function Combine() As String
        Dim strTmp As String = ""
        If _people.Length > 0 Then
            strTmp = strTmp & ", " & _people
        End If

        If _projects.Length > 0 Then
            strTmp = strTmp & ", " & _projects
        End If

        If _topics.Length > 0 Then
            strTmp = strTmp & ", " & _topics
        End If

        If context.Length > 0 Then
            strTmp = strTmp & ", " & context
        End If

        If kb.Length > 0 Then
            strTmp = strTmp & ", " & kb
        End If

        If today = True Then
            strTmp = strTmp & ", " & "Tag A Top Priority Today"
        End If

        If bullpin = True Then
            strTmp = strTmp & ", " & "Tag Bullpin Priorities"
        End If

        If strTmp.Length > 2 Then
            strTmp = Right(strTmp, strTmp.Length - 2)
        End If

        Return strTmp
    End Function

    ''' <summary>
    ''' Subroutine extracts flag settings from color categories and loads to internal variables
    ''' </summary>
    ''' <param name="strCats_All">String containing comma delimited color categories</param>
    ''' <param name="DeleteSearchSubString"></param>
    Public Sub Splitter(ByRef strCats_All As String, Optional DeleteSearchSubString As Boolean = False)
        _people = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag PPL "), ", ", DeleteSearchSubString:=DeleteSearchSubString)
        other = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag PPL "), ", ", True)

        _projects = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag PROJECT "), ", ", DeleteSearchSubString:=DeleteSearchSubString)
        other = SubStr_w_Delimeter(other, AddWildcards("Tag PROJECT "), ", ", True)

        Dim strTemp As String = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag Bullpin Priorities"), ", ", DeleteSearchSubString:=False)
        other = SubStr_w_Delimeter(other, AddWildcards("Tag Bullpin Priorities"), ", ", True)
        If strTemp <> "" Then
            bullpin = True
        Else
            bullpin = False
        End If

        strTemp = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag A Top Priority Today"), ", ", DeleteSearchSubString:=False)
        other = SubStr_w_Delimeter(other, AddWildcards("Tag A Top Priority Today"), ", ", True)
        If strTemp <> "" Then
            today = True
        Else
            today = False
        End If

        _topics = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag TOPIC "), ", ", DeleteSearchSubString:=DeleteSearchSubString)
        other = SubStr_w_Delimeter(other, AddWildcards("Tag TOPIC "), ", ", True)

        kb = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag KB "), ", ", DeleteSearchSubString:=DeleteSearchSubString)
        other = SubStr_w_Delimeter(other, AddWildcards("Tag KB "), ", ", True)

        context = other

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
        If b_Trailing Then strTemp = strTemp & charWC

        AddWildcards = strTemp

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
        Dim i As Integer

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

            m_Find = SearchStr

            'Make lower case
            m_Find = LCase$(m_Find)

            'Standardize characters used as wildcards
            m_Find = Replace(m_Find, "%", "*")

            'Determine if wildcards are present in search string
            m_Wildcard = (InStr(m_Find, "*"))

            intFoundCt = 0

            'Remove wildcards from the string
            strSearchNoWC = Replace(SearchStr, "*", "")

            'Loop through the array to find substring
            For i = LBound(varStrArry) To UBound(varStrArry)
                boolFound = False

                'Skip over blank entries
                If varStrArry(i) <> "" Then
                    If m_Wildcard Then
                        If bNotSearchStr = False Then
                            boolFound = (LCase$(varStrArry(i)) Like m_Find)
                        Else
                            boolFound = Not (LCase$(varStrArry(i)) Like m_Find)
                        End If
                    Else
                        If bNotSearchStr = False Then
                            boolFound = (LCase$(varStrArry(i)) = m_Find)
                        Else
                            boolFound = Not (LCase$(varStrArry(i)) = m_Find)
                        End If
                    End If
                End If

                If boolFound Then
                    boolFound = False
                    intFoundCt = intFoundCt + 1
                    ReDim Preserve strCats(intFoundCt)
                    strTemp = varStrArry(i)
                    If DeleteSearchSubString Then strTemp = Replace(strTemp, strSearchNoWC, "", , , vbTextCompare)
                    strCats(intFoundCt) = strTemp
                End If
            Next i

            If intFoundCt = 0 Then
                SearchArry4Str = ""
            Else
                SearchArry4Str = strCats
            End If

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
