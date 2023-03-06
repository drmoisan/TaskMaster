Class SurroundingClass
    <XmlRoot("PersonenListe")>
    <XmlInclude(GetType(Person))>
    Public Class PersonalList
        <XmlArray("PersonenArray")>
        <XmlArrayItem("PersonObjekt")>
        Public Persons As List(Of Person) = New List(Of Person)()
        <XmlElement("Listname")>
        Public Property Listname As String

        Public Sub New()
        End Sub

        Public Sub New(ByVal name As String)
            Me.Listname = name
        End Sub

        Public Sub AddPerson(ByVal person As Person)
            Persons.Add(person)
        End Sub
    End Class

    <XmlType("Person")>
    <XmlInclude(GetType(SpecialPerson)), XmlInclude(GetType(SuperPerson))>
    Public Class Person
        <XmlAttribute("PersID", DataType:="string")>
        Public Property ID As String
        <XmlElement("Name")>
        Public Property Name As String
        <XmlElement("City")>
        Public Property City As String
        <XmlElement("Age")>
        Public Property Age As Integer

        Public Sub New()
        End Sub

        Public Sub New(ByVal name As String, ByVal city As String, ByVal age As Integer, ByVal id As String)
            Me.Name = name
            Me.City = city
            Me.Age = age
            Me.ID = id
        End Sub
    End Class

    <XmlType("SpecialPerson")>
    Public Class SpecialPerson
        Inherits Person

        <XmlElement("SpecialInterests")>
        Public Property Interests As String

        Public Sub New()
        End Sub

        Public Sub New(ByVal name As String, ByVal city As String, ByVal age As Integer, ByVal id As String, ByVal interests As String)
            Me.Name = name
            Me.City = city
            Me.Age = age
            Me.ID = id
            Me.Interests = interests
        End Sub
    End Class

    <XmlType("SuperPerson")>
    Public Class SuperPerson
        Inherits Person

        <XmlArray("Skills")>
        <XmlArrayItem("Skill")>
        Public Property Skills As List(Of String)
        <XmlElement("Alias")>
        Public Property [Alias] As String

        Public Sub New()
            Skills = New List(Of String)()
        End Sub

        Public Sub New(ByVal name As String, ByVal city As String, ByVal age As Integer, ByVal id As String, ByVal skills As String(), ByVal [alias] As String)
            Skills = New List(Of String)()
            Me.Name = name
            Me.City = city
            Me.Age = age
            Me.ID = id

            For Each item As String In skills
                Me.Skills.Add(item)
            Next

            Me.[Alias] = [alias]
        End Sub
    End Class

    Private Shared Sub Main(ByVal args As String())
        Dim personen As PersonalList = New PersonalList()
        personen.Listname = "Friends"
        Dim normPerson As Person = New Person()
        normPerson.ID = "0"
        normPerson.Name = "Max Man"
        normPerson.City = "Capitol City"
        normPerson.Age = 33
        Dim specPerson As SpecialPerson = New SpecialPerson()
        specPerson.ID = "1"
        specPerson.Name = "Albert Einstein"
        specPerson.City = "Ulm"
        specPerson.Age = 36
        specPerson.Interests = "Physics"
        Dim supPerson As SuperPerson = New SuperPerson()
        supPerson.ID = "2"
        supPerson.Name = "Superman"
        supPerson.[Alias] = "Clark Kent"
        supPerson.City = "Metropolis"
        supPerson.Age = Integer.MaxValue
        supPerson.Skills.Add("fly")
        supPerson.Skills.Add("strong")
        personen.AddPerson(normPerson)
        personen.AddPerson(specPerson)
        personen.AddPerson(supPerson)
        Dim personTypes As Type() = {GetType(Person), GetType(SpecialPerson), GetType(SuperPerson)}
        Dim serializer As XmlSerializer = New XmlSerializer(GetType(PersonalList), personTypes)
        Dim fs As FileStream = New FileStream("Personenliste.xml", FileMode.Create)
        serializer.Serialize(fs, personen)
        fs.Close()
        personen = Nothing
        fs = New FileStream("Personenliste.xml", FileMode.Open)
        personen = CType(serializer.Deserialize(fs), PersonalList)
        serializer.Serialize(Console.Out, personen)
        Console.ReadLine()
    End Sub
End Class