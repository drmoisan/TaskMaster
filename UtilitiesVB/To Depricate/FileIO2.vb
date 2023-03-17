Imports System.IO
Imports System.Linq

Public Module FileIO2
    Public Sub DELETE_TextFile(filename As String, stagingPath As String)
        Dim filepath As String = Path.Combine(stagingPath, filename)

        If File.Exists(filepath) Then
            File.Delete(filepath)
        End If

    End Sub

    <Flags>
    Private Enum WriteOptions
        None = 0
        AppendNewLine = 1
        OpenAsAppend = 2
    End Enum

    Public Sub Write_TextFile(strFileName As String, strOutput() As String, strFileLocation As String)
        Dim filepath As String = Path.Combine(strFileLocation, strFileName)
        Dim listOutput As New List(Of String)(strOutput)
        For Each output In listOutput
            WriteUTF8(filepath, output, WriteOptions.AppendNewLine AndAlso WriteOptions.OpenAsAppend)
        Next

    End Sub

    'Public Sub Write_TextFile(strFileName As String,
    '                          enumerableOutput As IEnumerable,
    '                          strFileLocation As String,
    '                          encoding As Text.Encoding)
    '    Dim filepath As String = Path.Combine(strFileLocation, strFileName)
    '    Dim listOutput As New List(Of String)(strOutput)
    '    WriteUTF8(filepath, listOutput, WriteOptions.AppendNewLine AndAlso WriteOptions.OpenAsAppend)
    'End Sub


    Private Sub WriteUTF8(filepath As String,
                          textString As String,
                          options As WriteOptions)

        Dim asAppend As Boolean = options.HasFlag(WriteOptions.OpenAsAppend)

        Using sw As StreamWriter = New StreamWriter(
            filepath, asAppend, System.Text.Encoding.UTF8)
            If options.HasFlag(WriteOptions.AppendNewLine) Then
                sw.WriteLine(textString)
            Else
                sw.Write(textString)
            End If
            sw.Close()
        End Using

    End Sub



    Public Function CSV_ReadTxtF(filename As String, fileaddress As String, Optional SkipHeaders As Boolean = True) As String()

        Dim filepath As String = Path.Combine(fileaddress, filename)

        If File.Exists(filepath) Then
            If SkipHeaders Then
                Dim lines = File.ReadAllLines(filepath)
                Return lines.Skip(1).ToArray()
            Else
                Return File.ReadAllLines(filepath)
            End If

        Else
            Return Nothing
        End If

    End Function

    Public Function CSV_Read(filename As String, fileaddress As String, Optional SkipHeaders As Boolean = False) As String()

        Dim filepath As String = Path.Combine(fileaddress, filename)

        If File.Exists(filepath) Then
            If SkipHeaders Then
                Dim lines = File.ReadAllLines(filepath, System.Text.Encoding.UTF8)
                Return lines.Skip(1).ToArray()
            Else
                Return File.ReadAllLines(filepath)
            End If

        Else
            Return Nothing
        End If

    End Function

    Public Function CSV_SPLIT_TO_2D(str1D() As String, Optional Delimeter As String = ",", Optional zerobased As Boolean = False) As String(,)
        Dim i, j As Integer
        Dim Count As Integer
        Dim maxj As Integer

        Dim strD2_tmp(,) As String
        Dim strTmp() As String
        Dim strLine As String
        Dim intBase As Integer

        If zerobased Then
            intBase = 0
        Else
            intBase = 1
        End If

        For i = LBound(str1D) To UBound(str1D)
            strLine = str1D(i)
            Count = Len(strLine) - Len(Replace(strLine, Delimeter, ""))
            If Count > maxj Then maxj = Count
        Next i

        ReDim strD2_tmp(UBound(str1D) + intBase, maxj + intBase)

        For i = 0 To UBound(str1D)
            strTmp = Split(str1D(i), Delimeter)
            For j = 0 To UBound(strTmp)
                strD2_tmp(i + intBase, j + intBase) = strTmp(j)
            Next j
        Next i

        Return strD2_tmp

    End Function

End Module
