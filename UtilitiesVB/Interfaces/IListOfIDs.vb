Imports System.Numerics
Imports Microsoft.Office.Interop.Outlook

Public Interface IListOfIDs
    ReadOnly Property MaxIDLength As Long
    Sub RePopulate(Appliation As Application)
    Sub Save()
    Sub Save(FileName_IDList As String)
    Function ConvertToBase(nbase As Integer, num As BigInteger, Optional intMinDigits As Integer = 2) As String
    Function ConvertToDecimal(nbase As Integer, strBase As String) As BigInteger
    Function FlattenArry(varBranch() As Object) As String
    Function GetMaxToDoID() As String
    Function GetNextAvailableToDoID(strSeed As String) As String
End Interface
