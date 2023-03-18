Imports UtilitiesVB

Public Module Smith_Watterman
    'Global Const Match_Score = 1
    'Global Const Mismatch_Score = 0
    'Global Const Gap_penalty = -1
    'Global Const Word_Match_Optimal = 5
    Public Enum SW_Options
        ByWords = 0
        ByLetters = 1
    End Enum


    Public Function StrToChars(strTmp As String) As Object
        Dim buff() As String
        Dim i As Integer

        ReDim buff(Len(strTmp) - 1)
        For i = 1 To Len(strTmp)
            buff(i - 1) = Mid(strTmp, i, 1)
        Next

        StrToChars = buff

    End Function

    Public Function SW_Calc(Str_X As String, Str_Y As String, ByRef Matrix(,) As Object, AFSettings As IAppAutoFileObjects, Optional SWOptions As SW_Options = SW_Options.ByWords) As Integer
        Dim LenX, LenY, x, y, calcA, calcB, calcC, tempa, tempB, tempC As Integer
        Dim words_X, words_Y As Object
        Dim maxSmith_Watterman As Integer
        Dim flatcsv() As String

        '    StopWatch_SW.reStart
        On Error GoTo ErrorHandler
        If SWOptions = SW_Options.ByWords Then
            words_X = Split(Str_X, " ")
            words_Y = Split(Str_Y, " ")
        ElseIf SWOptions = SW_Options.ByLetters Then
            words_X = StrToChars(Str_X)
            words_Y = StrToChars(Str_Y)
        Else
            'Default is bywords
            words_X = Split(Str_X, " ")
            words_Y = Split(Str_Y, " ")
        End If

        LenX = UBound(words_X)
        LenY = UBound(words_Y)
        ReDim Matrix(LenX + 3, LenY + 3)
        ReDim flatcsv(LenY + 3)

        '*********************************
        '**********Initialize*************
        For x = 3 To LenX + 3
            Matrix(x, 1) = words_X(x - 3)
        Next x

        For y = 3 To LenY + 3
            Matrix(1, y) = words_Y(y - 3)
        Next y

        For x = 2 To LenX + 3
            Matrix(x, 2) = 0
        Next x

        For y = 2 To LenY + 3
            Matrix(2, y) = 0
        Next y
        '*********************************

        '*********************************

        For x = 3 To LenX + 3
            For y = 3 To LenY + 3
                calcA = Matrix(x - 1, y - 1)
                If Matrix(x, 1) = Matrix(1, y) Then
                    calcA = calcA + AFSettings.SmithWatterman_MatchScore * Len(Matrix(x, 1))
                Else
                    calcA = calcA + AFSettings.SmithWatterman_MismatchScore
                End If

                calcB = Matrix(x, y - 1) + AFSettings.SmithWatterman_GapPenalty * Len(Matrix(1, y))
                calcC = Matrix(x - 1, y) + AFSettings.SmithWatterman_GapPenalty * Len(Matrix(x, 1))
                tempa = max(0, calcA, calcB, calcC)
                Matrix(x, y) = tempa
                If tempa > maxSmith_Watterman Then maxSmith_Watterman = tempa
            Next y
        Next x

        For y = 1 To LenY + 3
            flatcsv(y) = ""
            For x = 1 To LenX + 2
                flatcsv(y) = flatcsv(y) & Matrix(x, y) & ", "
            Next x
            flatcsv(y) = flatcsv(y) & Matrix(LenX + 3, y)
        Next y

        'Call Printout(flatcsv)
        'MsgBox (maxSmith_Watterman & " of " & Max(LenX + 1, LenY + 1))
        SW_Calc = maxSmith_Watterman

        '    StopWatch_SW.Pause
        Exit Function
ErrorHandler:
        MsgBox(Err.Description)
        Stop
        Err.Clear()
        Resume

    End Function



    Function max(ParamArray values() As Object) As Object
        Dim maxValue, Value As Object
        maxValue = values(0)
        For Each Value In values
            If Value > maxValue Then maxValue = Value
        Next
        max = maxValue
    End Function





End Module
