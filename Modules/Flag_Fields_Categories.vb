Imports System
Imports System.Collections
Imports System.Diagnostics
Imports System.Windows.Forms
Imports Microsoft.Office.Interop.Outlook


Module Flag_Fields_Categories
    Public Sub CCOCatList_Load()
        Globals.ThisAddIn.CCOCatList = New List(Of String)

        With Globals.ThisAddIn.CCOCatList
            .Add("Tag PROJECT 31 Conectados")
            .Add("Tag PROJECT 31 Digital Architecture")
            .Add("Tag PROJECT 31 Digital Center Expansion BCC")
            .Add("Tag PROJECT 31 eGTM")
            .Add("Tag PROJECT 31 iVend")
            .Add("Tag PROJECT 31 Perfect Store")
            .Add("Tag PROJECT 31 RM Analytics Simulator")
            .Add("Tag PROJECT 31 White Swan TPM TPO")
            .Add("Tag PROJECT 31Marketing Consumer DNA")
            .Add("Tag PROJECT 31Marketing Mix Modeling and ROI Media MMM")
            .Add("Tag PROJECT 32 Marketing As A Function")
            .Add("Tag PROJECT 33 GBS")
            .Add("Tag PROJECT 33 POS Central Design")
            .Add("Tag PROJECT 33 RIC Route Enginnering Center")
            .Add("Tag PROJECT 33 Working A&M")
            .Add("Tag PROJECT 34 GTM Brazil - Ilumina")
            .Add("Tag PROJECT 35 CCO Org Design and Functions")
            .Add("Tag PROJECT 36 Coronavirus Response")
            .Add("Tag PROJECT 37 OT Capability")
            .Add("Tag PROJECT 41 Functional - Duties")
            .Add("Tag PROJECT 41 Functional - Learning Agenda")
            .Add("Tag PROJECT 41 Functional - UEFA UCLF Champions League 2020")
            .Add("Tag PROJECT 42 Planning - AOP 2019")
            .Add("Tag PROJECT 42 Planning - AOP 2020")
            .Add("Tag PROJECT 42 Planning - AOP 2021")
            .Add("Tag PROJECT 42 Planning - COVID Replan")
            .Add("Tag PROJECT 42 Planning - POR")
            .Add("Tag PROJECT 42 Planning - PSP 2020")
            .Add("Tag PROJECT 43 Other CCO")
            .Add("Tag PROJECT 43 Represent Sector Globally")
            .Add("Tag PROJECT 43 Routine - 1w1")
            .Add("Tag PROJECT 43 Routine - Approvals")
            .Add("Tag PROJECT 43 Routine - Improve Personal Efficiency / Effectiveness")
            .Add("Tag PROJECT 43 Routine - LATAM Excom Duties")
            .Add("Tag PROJECT 43 Routine - Market Visits / Stakeholder Wiring")
            .Add("Tag PROJECT 43 Routine - Other Tasks")
            .Add("Tag PROJECT 43 Routine - People Process")
            .Add("Tag PROJECT 43 Routine - Plan Calendar")
            .Add("Tag PROJECT 43 Routine - Reading")
            .Add("Tag PROJECT 43 Routine - Team Leadership Duties")
            .Add("Tag PROJECT 43 Routine - Training")
            .Add("Tag PROJECT 98 Atlantic")
            .Add("Tag PROJECT 98 Capacity Investments")
            .Add("Tag PROJECT 98 Corporate Audit")
            .Add("Tag PROJECT 98 Costa Rica Strategy")
            .Add("Tag PROJECT 98 Dominican Republic TAP")
            .Add("Tag PROJECT 98 Gira Caricam")
            .Add("Tag PROJECT 98 Gira Caricam 2019")
            .Add("Tag PROJECT 98 Guatemala Turnaround")
            .Add("Tag PROJECT 98 H1 Travel Agenda")
            .Add("Tag PROJECT 98 Imports Agile Stage Gate")
            .Add("Tag PROJECT 98 INSEAD IPP Integrating Performance and Progress")
            .Add("Tag PROJECT 98 ISCP Horizon")
            .Add("Tag PROJECT 98 Kendall Awards")
            .Add("Tag PROJECT 98 MDM")
            .Add("Tag PROJECT 98 Oats War Games")
            .Add("Tag PROJECT 98 OT / CATMAN CAPABILITY")
            .Add("Tag PROJECT 98 Other Caricam")
            .Add("Tag PROJECT 98 Parallel Imports")
            .Add("Tag PROJECT 98 Planning - AOP 2019")
            .Add("Tag PROJECT 98 Ramon Visit To Guatemala")
            .Add("Tag PROJECT 98 Revenue Management SLAM Panama")
            .Add("Tag PROJECT 98 SAP Leveling")
            .Add("Tag PROJECT 98 Splenda Panama DTS")
            .Add("Tag PROJECT 98 Visits 181106 Brian Newman Johannes Guatemala")
            .Add("Tag PROJECT Personal - Other")
            .Add("Tag PROJECT Relocation")
            .Add("Tag PROJECT Taxes 2016")
            .Add("Tag PROJECT Taxes 2017")
            .Add("Tag PROJECT Taxes 2018")
            .Add("Tag PPL Monica Bauer")
            .Add("Tag PPL Martin Ribichich")
            .Add("Tag PPL Luz Gossmann")
            .Add("Tag PPL Isaias Martinez")
            .Add("Tag PPL Claudia Olivos")
            .Add("Tag PPL Veronica Riojas")
            .Add("Tag PPL Bennett Price")
            .Add("Tag PPL Alejandro Puig")
            .Add("Tag PPL Hernan Tantardini")
            .Add("Tag PPL Dani Cachich")
            .Add("Tag PPL Raul Cortes")
            .Add("Tag PPL Maria Emiliana Rodriguez")
            .Add("Tag PPL Gabriela Alves")
            .Add("Tag PPL Claus Hanspach")
            .Add("Tag PPL Luis Ruben Gutierrez")
            .Add("Tag PPL Karla Halley")
            .Add("Tag PPL Adalberto Aguilar")
            .Add("Tag PPL Todd Squarek")
            .Add("Tag PPL Gabriela Garciacortes")
            .Add("Tag PPL Lily Zaidman")
            .Add("Tag PPL Erich Gamper")
            .Add("Tag PPL Georgina Rodriguez")
            .Add("Tag PPL Ana Henao")
            .Add("Tag PPL Eduardo Real")
            .Add("Tag PPL Salvador Hernandez")
            .Add("Tag PPL Omar Gonzalez")
            .Add("Tag PPL Arnab Sinha")
            .Add("Tag PPL Parinya Kitjatanapan")
            .Add("Tag PPL Lucas Ciccarelli")
            .Add("Tag PPL Ivonne Dominguez")
            .Add("Tag PPL Carlos DeLascurain")
            .Add("Tag PPL Carlos SanchezAbril")
            .Add("Tag PPL Gerardo Diaz de Leon")
            .Add("Tag PPL Cristina Lou")
            .Add("Tag PPL Eric Melis")
            .Add("Tag PPL Harry Walsh")
            .Add("Tag PPL LATAM EXCOM")
            .Add("Tag PPL Carolina Moreno")
            .Add("Tag PPL Rodolfo Portillo")
            .Add("Tag PPL Triana Vazquez")
            .Add("Tag PPL Janine Wacklawski")
            .Add("Tag PPL Victor Hugo Gonzalez")
            .Add("Tag PPL Vikram Somaya")
            .Add("Tag PPL Andrew Motz")
            .Add("Tag PPL Joao Campos")
            .Add("Tag PPL Federico Lamberti")
            .Add("Tag PPL Raul Bribiesca")
            .Add("Tag PPL Dennis Furnis")
            .Add("Tag PPL Beatriz Tilkian")
            .Add("Tag PPL Erica Diago")
            .Add("Tag PPL Begona Aristy")
            .Add("Tag PPL Jose Cota")
            .Add("Tag PPL Michelle Luciano")
            .Add("Tag PPL Alejandra Noble")
            .Add("Tag PPL Carlos Quintana")
            .Add("Tag PPL Santiago Cota")
            .Add("Tag PPL Carlos Pacheco")
            .Add("Tag PPL Juan Zapiain")
            .Add("Tag PPL Fernando Pliego")
            .Add("Tag PPL David Kahn")
            .Add("Tag PPL Ian Kahn")
            .Add("Tag PPL Marco Viramontes")
            .Add("Tag PPL Mauricio Nakaie")
            .Add("Tag PPL Hernan Bazan")
            .Add("Tag PPL Axelle Olivares")
            .Add("Tag PPL Alejandro Romo")
            .Add("Tag PPL Arturo Aviladominguez")
            .Add("Tag PPL JoseMaria Chepe Mora")
            .Add("Tag PPL Lalo Vidargas")
            .Add("Tag PPL Dora Blanco")
            .Add("Tag PPL Felipe Santanaruiz")
            .Add("Tag PPL Joao Goncalves")
            .Add("Tag PPL Paulina Corona")
            .Add("Tag PPL Paula Santilli")
            .Add("Tag PPL Gonzalo Muente")
            .Add("Tag PPL Ricardo Arias")
            .Add("Tag PPL Leonardo Aio")
            .Add("Tag PPL Pedro Escalante")
            .Add("Tag PPL Mauricio Degaray")
            .Add("Tag PPL Michal Geller")
            .Add("Tag PPL Raul Pedrique")
            .Add("Tag PPL Miguel Olivares")
            .Add("Tag PPL Chufi Zarate")
            .Add("Tag PPL Carlos Eboli")
            .Add("Tag PPL Erick Scheel")
            .Add("Tag PPL Hector Ariel")
            .Add("Tag PPL Paula Velazquez")
            .Add("Tag PPL Manny Pineiro")
            .Add("Tag PPL Ana Fleury")
            .Add("Tag PPL Gibu Thomas")
            .Add("Tag PPL Ezequiel Gomez")
            .Add("Tag PPL Yolanda Lebron")
            .Add("Tag PPL PWC")
            .Add("Tag PPL Salomon Levy")
            .Add("Tag PPL Juan Aude")
            .Add("Tag PPL Catalina Casco")
            .Add("Tag PPL Carlos Gonzalez")
            .Add("Tag PPL Mike Roper")
            .Add("Tag PPL Steve Gibson")
            .Add("Tag PPL Ricardo Pimenta")
            .Add("Tag PPL Lorena Segal")
            .Add("Tag PPL Marjorie Delacruz")
            .Add("Tag PPL Cristina Leo")
            .Add("Tag PPL Ergun Gunay -> APAC")
            .Add("Tag PPL Juancarlos Haro")
            .Add("Tag PPL Ricardo Cuellar")
            .Add("Tag PPL Roberto Martinez")
            .Add("Tag PPL Glenda Pfirter")
            .Add("Tag PPL Mimi Chen")
            .Add("Tag PPL Lilly Yip")
            .Add("Tag PPL Lucia Weymann")
            .Add("Tag PPL Celso Borges")
            .Add("Tag PPL Paulina Castillo")
            .Add("Tag PPL Valeria Rivas")
            .Add("Tag PPL Paco Cotera")
            .Add("Tag PPL Carmela Rivero")
            .Add("Tag PPL Monica Tenorio")
            .Add("Tag PPL Fernando Mateos")


        End With

    End Sub

    Public Function Category_Create(strPrefix As String, strNewCat As String) As Category

        Dim objNameSpace As Outlook.NameSpace
        Dim objCategory As Category
        Dim OlColor As OlCategoryColor
        Dim strTemp As String

        On Error GoTo ErrorHandler


        If strNewCat <> "" Then
            objNameSpace = Globals.ThisAddIn._OlNS
            If strPrefix <> "" Then
                If Len(strNewCat) > Len(strPrefix) Then
                    If Left(strNewCat, Len(strPrefix)) <> strPrefix Then
                        strTemp = strPrefix & strNewCat
                    Else
                        strTemp = strNewCat
                    End If
                Else
                    strTemp = strPrefix & strNewCat
                End If
            Else
                strTemp = strNewCat
            End If

            If strPrefix = "Tag PPL " Then
                OlColor = OlCategoryColor.olCategoryColorDarkGray
            ElseIf strPrefix = "Tag PROJECT " Then
                OlColor = OlCategoryColor.olCategoryColorTeal
            ElseIf strPrefix = "Tag TOPIC " Then
                OlColor = OlCategoryColor.olCategoryColorDarkTeal
            Else
                OlColor = OlCategoryColor.olCategoryColorNone
            End If

            On Error Resume Next
            objCategory = objNameSpace.Categories.Add(strTemp, OlColor,
            OlCategoryShortcutKey.olCategoryShortcutKeyNone)

            If Err.Number = 0 Then
                Category_Create = objCategory
            Else
                Err.Clear()

                For Each objCategory In objNameSpace.Categories
                    If objCategory.Name = strTemp Then
                        Category_Create = objCategory
                        Exit Function
                    End If
                Next objCategory
            End If
        Else
            Category_Create = Nothing
        End If

        Exit Function

ErrorHandler:
        MsgBox("Error, can't create category " & strTemp & ". " & Err.Description)
        Category_Create = Nothing
    End Function

End Module
