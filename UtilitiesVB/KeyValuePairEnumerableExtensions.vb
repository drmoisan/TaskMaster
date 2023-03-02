Imports System.Runtime.CompilerServices


Public Module KeyValuePairEnumerableExtensions
    ''' <summary>
    ''' Helper function used in conjunction with Linq query to enable a SortedDictionary to be filtered efficiently.
    ''' Sample usage is Dim filtered_dict = source_dict.Where(Function(x) x.Value.foo = bar).ToSortedDictionary()
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="l">IEnumerable of a KeyValuePair from a dictionary</param>
    ''' <returns>A Sorted Dictionary</returns>
    <Extension()>
    Public Function ToSortedDictionary(Of TKey, TValue)(ByVal l As IEnumerable(Of KeyValuePair(Of TKey, TValue))) As SortedDictionary(Of TKey, TValue)
        Dim result As SortedDictionary(Of TKey, TValue) = New SortedDictionary(Of TKey, TValue)()

        For Each e In l
            result(e.Key) = e.Value
        Next

        Return result
    End Function

    ''' <summary>
    ''' Helper function used in conjunction with Linq query to enable a Dictionary to be filtered efficiently.
    ''' Sample usage is Dim filtered_dict = source_dict.Where(Function(x) x.Value.foo = bar).ToDictionary()
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="l">IEnumerable of a KeyValuePair from a dictionary</param>
    ''' <returns>A Sorted Dictionary</returns>
    <Extension()>
    Public Function ToDictionary(Of TKey, TValue)(ByVal l As IEnumerable(Of KeyValuePair(Of TKey, TValue))) As Dictionary(Of TKey, TValue)
        Dim result As Dictionary(Of TKey, TValue) = New Dictionary(Of TKey, TValue)()

        For Each e In l
            result(e.Key) = e.Value
        Next

        Return result
    End Function
End Module
