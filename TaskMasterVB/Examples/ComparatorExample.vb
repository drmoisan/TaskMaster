Public Class ComparatorExample

End Class

Public Class CityInfo
    Public Sub New(ByVal name As String, ByVal country As String, ByVal pop2010 As Integer)
        City = name
        Me.Country = country
        Me.Population = pop2010
    End Sub

    Public ReadOnly Property City As String

    Public ReadOnly Property Country As String

    Public ReadOnly Property Population As Integer

    Public Shared Function CompareByName(ByVal city1 As CityInfo, ByVal city2 As CityInfo) As Integer
        Return String.Compare(city1.City, city2.City)
    End Function

    Public Shared Function CompareByPopulation(ByVal city1 As CityInfo, ByVal city2 As CityInfo) As Integer
        Return city1.Population.CompareTo(city2.Population)
    End Function

    Public Shared Function CompareByNames(ByVal city1 As CityInfo, ByVal city2 As CityInfo) As Integer
        Return String.Compare(city1.Country & city1.City, city2.Country & city2.City)
    End Function
End Class

Public Class Example
    Public Shared Sub Main()
        Dim NYC As New CityInfo("New York City", "United States of America", 8175133)
        Dim Det As New CityInfo("Detroit", "United States of America", 713777)
        Dim Paris As New CityInfo("Paris", "France", 2193031)
        Dim cities As CityInfo() = {NYC, Det, Paris}
        DisplayArray(cities)
        Array.Sort(cities, AddressOf CityInfo.CompareByName)
        DisplayArray(cities)
        Array.Sort(cities, AddressOf CityInfo.CompareByPopulation)
        DisplayArray(cities)
        Array.Sort(cities, AddressOf CityInfo.CompareByNames)
        DisplayArray(cities)
    End Sub

    Private Shared Sub DisplayArray(ByVal cities As CityInfo())
        Console.WriteLine("{0,-20} {1,-25} {2,10}", "City", "Country", "Population")

        For Each city In cities
            Console.WriteLine("{0,-20} {1,-25} {2,10:N0}", city.City, city.Country, city.Population)
        Next

        Console.WriteLine()
    End Sub
End Class
