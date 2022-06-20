Public Class Flags
    Private _People As String = ""
    Public _Projects As String = ""
    Public _Topics As String = ""
    Public Context As String = ""
    Public KB As String = ""
    Public Other As String = ""
    Public Today As Boolean = False
    Public Bullpin As Boolean = False

    Public Sub New(ByRef strCats_All As String, Optional DeleteSearchSubString As Boolean = False)
        Splitter(strCats_All, DeleteSearchSubString)
    End Sub

    Public Property Projects(Optional IncludePrefix As Boolean = False) As String
        Get
            Dim Prefix As String = "Tag PROJECT "
            Dim strReturn As String = _Projects

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
            _Projects = strReturn
        End Set
    End Property

    Public Property Topics(Optional IncludePrefix As Boolean = False) As String
        Get
            Dim Prefix As String = "Tag TOPIC "
            Dim strReturn As String = _Topics

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
            _Topics = strReturn
        End Set
    End Property

    Public Property People(Optional IncludePrefix As Boolean = False) As String
        Get
            Dim Prefix As String = "Tag PPL "
            Dim strReturn As String = _People

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
            _People = strReturn
        End Set
    End Property

    Public Function Combine() As String
        Dim strTmp As String = ""
        If _People.Length > 0 Then
            strTmp = strTmp & ", " & _People
        End If

        If _Projects.Length > 0 Then
            strTmp = strTmp & ", " & _Projects
        End If

        If _Topics.Length > 0 Then
            strTmp = strTmp & ", " & _Topics
        End If

        If Context.Length > 0 Then
            strTmp = strTmp & ", " & Context
        End If

        If KB.Length > 0 Then
            strTmp = strTmp & ", " & KB
        End If

        If Today = True Then
            strTmp = strTmp & ", " & "Tag A Top Priority Today"
        End If

        If Bullpin = True Then
            strTmp = strTmp & ", " & "Tag Bullpin Priorities"
        End If

        If strTmp.Length > 2 Then
            strTmp = Right(strTmp, strTmp.Length - 2)
        End If

        Return strTmp
    End Function

    Public Sub Splitter(ByRef strCats_All As String, Optional DeleteSearchSubString As Boolean = False)
        _People = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag PPL "), ", ", DeleteSearchSubString:=DeleteSearchSubString)
        Other = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag PPL "), ", ", True)

        _Projects = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag PROJECT "), ", ", DeleteSearchSubString:=DeleteSearchSubString)
        Other = SubStr_w_Delimeter(Other, AddWildcards("Tag PROJECT "), ", ", True)

        Dim strTemp As String = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag Bullpin Priorities"), ", ", DeleteSearchSubString:=False)
        Other = SubStr_w_Delimeter(Other, AddWildcards("Tag Bullpin Priorities"), ", ", True)
        If strTemp <> "" Then
            Bullpin = True
        Else
            Bullpin = False
        End If

        strTemp = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag A Top Priority Today"), ", ", DeleteSearchSubString:=False)
        Other = SubStr_w_Delimeter(Other, AddWildcards("Tag A Top Priority Today"), ", ", True)
        If strTemp <> "" Then
            Today = True
        Else
            Today = False
        End If

        _Topics = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag TOPIC "), ", ", DeleteSearchSubString:=DeleteSearchSubString)
        Other = SubStr_w_Delimeter(Other, AddWildcards("Tag TOPIC "), ", ", True)

        KB = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag KB "), ", ", DeleteSearchSubString:=DeleteSearchSubString)
        Other = SubStr_w_Delimeter(Other, AddWildcards("Tag KB "), ", ", True)

        Context = Other

    End Sub

    Public Function AddWildcards(ByVal strOriginal As String, Optional b_Leading As Boolean = True,
    Optional b_Trailing As Boolean = True, Optional charWC As String = "*") As String

        Dim strTemp As String
        strTemp = strOriginal
        If b_Leading Then strTemp = charWC & strTemp
        If b_Trailing Then strTemp = strTemp & charWC

        AddWildcards = strTemp

    End Function

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

            m_Find = LCase$(m_Find)                             'Make lower case
            m_Find = Replace(m_Find, "%", "*")                  'Standardize characters used as wildcards
            m_Wildcard = (InStr(m_Find, "*"))                   'Determine if wildcards are present in search string
            intFoundCt = 0
            strSearchNoWC = Replace(SearchStr, "*", "")         'Remove wildcards from the string

            For i = LBound(varStrArry) To UBound(varStrArry)    'Loop through the array to find substring
                boolFound = False
                If varStrArry(i) <> "" Then                       'Skip over blank entries
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
