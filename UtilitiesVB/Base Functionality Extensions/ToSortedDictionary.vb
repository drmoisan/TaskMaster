Imports System.Runtime.CompilerServices

Public Module ToSortedDictionary
    <Extension()>
    Public Function ToSortedDictionary(Of K, V)(ByVal existing As Dictionary(Of K, V)) As SortedDictionary(Of K, V)
        Return New SortedDictionary(Of K, V)(existing)
    End Function

    Public Function SearchSortedDictKeys(
        source_dict As SortedDictionary(Of String, Boolean),
        search_string As String) _
        As SortedDictionary(Of String, Boolean)

        Dim filtered_cats = (From x In source_dict
                             Where x.Key.Contains(search_string)
                             Select x).ToDictionary(
                             Function(x) x.Key,
                             Function(x) x.Value)
        Return New SortedDictionary(Of String, Boolean)(filtered_cats)
    End Function
End Module
