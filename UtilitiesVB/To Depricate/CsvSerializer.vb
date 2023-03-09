Imports System.ComponentModel
Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization
Imports System.Text

Public Class CsvSerializer(Of T As {Class, New})
    Implements IFormatter

    Private ReadOnly _properties As List(Of PropertyInfo)

    Public Property IgnoreEmptyLines As Boolean = True

    Public Property IgnoreReferenceTypesExceptString As Boolean = True

    Public Property NewlineReplacement As String = ChrW(&H254).ToString()

    Public Property Replacement As String = ChrW(&H255).ToString()

    Public Property RowNumberColumnTitle As String = "RowNumber"

    Public Property Separator As Char = ","c

    Public Property UseEofLiteral As Boolean

    Public Property UseLineNumbers As Boolean = True

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
            Dim unused3 = sb.AppendLine("sep=" & Separator)
        End If

        Dim data = CType(graph, List(Of T))
        Dim unused2 = sb.AppendLine(GetHeader())
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

            Dim unused1 = sb.AppendLine(String.Join(Separator.ToString(), values.ToArray()))
        Next

        If UseEofLiteral Then
            values.Clear()

            If UseLineNumbers Then
                values.Add(Math.Min(System.Threading.Interlocked.Increment(row), row - 1).ToString())
            End If

            values.Add("EOF")
            Dim unused = sb.AppendLine(String.Join(Separator.ToString(), values.ToArray()))
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


