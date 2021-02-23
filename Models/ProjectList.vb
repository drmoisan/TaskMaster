Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.Serialization
Imports System.Text


<Serializable()>
Public Class ProjectList

    Public ProjectDictionary As Dictionary(Of String, String)

    Public Sub New(ByVal dictProjectList As Dictionary(Of String, String))
        Me.ProjectDictionary = dictProjectList
    End Sub

    Public Sub New()

    End Sub

    Public Sub ToCSV(FileName As String)
        Dim csv As String = String.Join(Environment.NewLine, ProjectDictionary.[Select](Function(d) $"{d.Key};{d.Value};"))
        System.IO.File.WriteAllText(FileName, csv)
    End Sub

End Class

Public Class CsvSerializer(Of T As {Class, New})
    Implements IFormatter

    Private ReadOnly _properties As List(Of PropertyInfo)
    Private _ignoreEmptyLines As Boolean = True
    Private _ignoreReferenceTypesExceptString As Boolean = True
    Private _newlineReplacement As String = (ChrW(&H254)).ToString()
    Private _replacement As String = (ChrW(&H255)).ToString()
    Private _rowNumberColumnTitle As String = "RowNumber"
    Private _separator As Char = ","c
    Private _useLineNumbers As Boolean = True

    Public Property IgnoreEmptyLines As Boolean
        Get
            Return _ignoreEmptyLines
        End Get
        Set(ByVal value As Boolean)
            _ignoreEmptyLines = value
        End Set
    End Property

    Public Property IgnoreReferenceTypesExceptString As Boolean
        Get
            Return _ignoreReferenceTypesExceptString
        End Get
        Set(ByVal value As Boolean)
            _ignoreReferenceTypesExceptString = value
        End Set
    End Property

    Public Property NewlineReplacement As String
        Get
            Return _newlineReplacement
        End Get
        Set(ByVal value As String)
            _newlineReplacement = value
        End Set
    End Property

    Public Property Replacement As String
        Get
            Return _replacement
        End Get
        Set(ByVal value As String)
            _replacement = value
        End Set
    End Property

    Public Property RowNumberColumnTitle As String
        Get
            Return _rowNumberColumnTitle
        End Get
        Set(ByVal value As String)
            _rowNumberColumnTitle = value
        End Set
    End Property

    Public Property Separator As Char
        Get
            Return _separator
        End Get
        Set(ByVal value As Char)
            _separator = value
        End Set
    End Property

    Public Property UseEofLiteral As Boolean

    Public Property UseLineNumbers As Boolean
        Get
            Return _useLineNumbers
        End Get
        Set(ByVal value As Boolean)
            _useLineNumbers = value
        End Set
    End Property

    Public Property UseTextQualifier As Boolean

    Public Sub New()
        UseTextQualifier = False
        UseEofLiteral = False
        Dim type = GetType(T)
        Dim properties = type.GetProperties(BindingFlags.[Public] Or BindingFlags.Instance Or BindingFlags.GetProperty Or BindingFlags.SetProperty)
        Dim q = properties.AsQueryable()

        If IgnoreReferenceTypesExceptString Then
            q = q.Where(Function(a) a.PropertyType.IsValueType OrElse a.PropertyType.Name = "String")
        End If

        Dim r = From a In q Where a.GetCustomAttribute(Of CsvIgnoreAttribute)() Is Nothing Order By a.Name Select a
        _properties = r.ToList()
    End Sub

    Public Property Binder As SerializationBinder Implements IFormatter.Binder
    Public Property Context As StreamingContext Implements IFormatter.Context
    Public Property SurrogateSelector As ISurrogateSelector Implements IFormatter.SurrogateSelector

    Public Function Deserialize(ByVal serializationStream As Stream) As Object Implements IFormatter.Deserialize
        Dim columns As String()
        Dim rows As String()

        Try

            Using sr = New StreamReader(serializationStream)
                columns = sr.ReadLine().Split(Separator)
                Dim contents = sr.ReadToEnd()
                Dim lineEnding = If(contents.IndexOf(vbCr) = -1, vbLf, vbCrLf)
                rows = contents.Split(New String() {lineEnding}, StringSplitOptions.None)
            End Using

        Catch ex As Exception
            Throw New InvalidCsvFormatException("The CSV File is Invalid. See Inner Exception for more inoformation.", ex)
        End Try

        Dim data = New List(Of T)()

        For row = 0 To rows.Length - 1
            Dim line = rows(row)

            If IgnoreEmptyLines AndAlso String.IsNullOrWhiteSpace(line) Then
                Continue For
            End If

            If Not IgnoreEmptyLines AndAlso String.IsNullOrWhiteSpace(line) Then
                Throw New InvalidCsvFormatException(String.Format("Error: Empty line at line number: {0}", row))
            End If

            Dim parts = line.Split(Separator)
            Dim firstColumnIndex = If(UseLineNumbers, 2, 1)

            If parts.Length = firstColumnIndex AndAlso parts(firstColumnIndex - 1) IsNot Nothing AndAlso parts(firstColumnIndex - 1) = "EOF" Then
                Exit For
            End If

            Dim datum = New T()
            Dim start = If(UseLineNumbers, 1, 0)

            For i = start To parts.Length - 1
                Dim value = parts(i)
                Dim column = columns(i)

                If column.Equals(RowNumberColumnTitle) AndAlso Not _properties.Any(Function(a) a.Name.Equals(RowNumberColumnTitle)) Then
                    Continue For
                End If

                value = value.Replace(Replacement, Separator.ToString()).Replace(NewlineReplacement, Environment.NewLine).Trim()
                Dim p = _properties.FirstOrDefault(Function(a) a.Name.Equals(column, StringComparison.InvariantCultureIgnoreCase))

                If p Is Nothing Then
                    Continue For
                End If

                If UseTextQualifier Then

                    If value.IndexOf("""") = 0 Then
                        value = value.Substring(1)
                    End If

                    If value(value.Length - 1).ToString() = """" Then
                        value = value.Substring(0, value.Length - 1)
                    End If
                End If

                Dim converter = TypeDescriptor.GetConverter(p.PropertyType)
                Dim convertedvalue = converter.ConvertFrom(value)
                p.SetValue(datum, convertedvalue)
            Next

            data.Add(datum)
        Next

        Return data
    End Function

    Public Sub Serialize(ByVal stream As Stream, ByVal graph As Object) Implements IFormatter.Serialize
        Dim sb = New StringBuilder()
        Dim values = New List(Of String)()

        If Separator <> ","c Then
            sb.AppendLine("sep=" & Separator)
        End If

        Dim data = CType(graph, List(Of T))
        sb.AppendLine(GetHeader())
        Dim row = 1

        For Each item In data
            values.Clear()

            If UseLineNumbers Then
                values.Add(Math.Min(System.Threading.Interlocked.Increment(row), row - 1).ToString())
            End If

            For Each p In _properties
                Dim raw = p.GetValue(item)
                Dim value = If(raw Is Nothing, "", raw.ToString().Replace(Separator.ToString(), Replacement).Replace(Environment.NewLine, NewlineReplacement))

                If UseTextQualifier Then
                    value = String.Format("""{0}""", value)
                End If

                values.Add(value)
            Next

            sb.AppendLine(String.Join(Separator.ToString(), values.ToArray()))
        Next

        If UseEofLiteral Then
            values.Clear()

            If UseLineNumbers Then
                values.Add(Math.Min(System.Threading.Interlocked.Increment(row), row - 1).ToString())
            End If

            values.Add("EOF")
            sb.AppendLine(String.Join(Separator.ToString(), values.ToArray()))
        End If

        Using sw = New StreamWriter(stream)
            sw.Write(sb.ToString().Trim())
        End Using
    End Sub

    Private Function GetHeader() As String
        Dim csvDisplayHeaderAttributes = New List(Of CsvDisplayHeaderAttribute)()

        For Each [property] In _properties
            Dim attribute = CType([property].GetCustomAttributes(GetType(CsvDisplayHeaderAttribute), False).FirstOrDefault(), CsvDisplayHeaderAttribute)
            csvDisplayHeaderAttributes.Add(New CsvDisplayHeaderAttribute With {
                    .DisplayName = If(attribute Is Nothing, [property].Name, If(String.IsNullOrEmpty(attribute.DisplayName), [property].Name, attribute.DisplayName)),
                    .Order = If(attribute Is Nothing, Integer.MaxValue, attribute.Order)
                })
        Next

        Dim header = csvDisplayHeaderAttributes.OrderBy(Function(x) x.Order).[Select](Function(x) x.DisplayName)

        If UseLineNumbers Then
            header = New String() {RowNumberColumnTitle}.Union(header)
        End If

        Return String.Join(Separator.ToString(), header.ToArray())
    End Function
End Class

Public Class CsvIgnoreAttribute
    Inherits Attribute
End Class

Public Class CsvDisplayHeaderAttribute
    Inherits Attribute

    Public Property DisplayName As String
    Public Property Order As Integer
End Class

Public Class InvalidCsvFormatException
    Inherits Exception

    Public Sub New(ByVal message As String)
        MyBase.New(message)
    End Sub

    Public Sub New(ByVal message As String, ByVal ex As Exception)
        MyBase.New(message, ex)
    End Sub
End Class

