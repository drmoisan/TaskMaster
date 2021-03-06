﻿Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Numerics
Imports BrightIdeasSoftware
Imports System.Collections
Imports System.IO
Imports Microsoft.Office.Interop.Outlook
Imports System.Collections.Generic
Imports System.Linq
Imports System.Collections.ObjectModel
Imports System.Diagnostics
Imports Microsoft.Office.Core
Imports System.Linq.Expressions
Imports System.Runtime.Serialization.Formatters.Binary


Public Class ToDoItem
    Private OlObject As Object
    Private _ToDoID As String = ""
    Public _TaskSubject As String = ""
    Public _MetaTaskSubject As String = ""
    Public _MetaTaskLvl As String = ""
    Private _TagContext As String = ""
    Private _TagProgram As String = ""
    Private _TagProject As String = ""
    Private _TagPeople As String = ""
    Private _TagTopic As String = ""
    Private _Priority As Outlook.OlImportance
    Private _TaskCreateDate As Date
    Private _StartDate As Date
    Private _Complete As Boolean
    Private _KB As String = ""

    Public Sub New(OlMail As Outlook.MailItem)
        OlObject = OlMail
        If OlMail.TaskSubject.Length <> 0 Then
            _TaskSubject = OlMail.TaskSubject
        Else
            _TaskSubject = OlMail.Subject
        End If
        _TagContext = CustomField("TagContext")
        _TagProgram = CustomField("TagProgram")
        _TagProject = CustomField("TagProject")
        _TagPeople = CustomField("TagPeople")
        _TagTopic = CustomField("TagTopic")
        _KB = CustomField("KBF")
        _Priority = OlMail.Importance
        _TaskCreateDate = OlMail.CreationTime
        _StartDate = OlMail.TaskStartDate
        _Complete = (OlMail.FlagStatus = OlFlagStatus.olFlagComplete)
    End Sub
    Public Sub New(OlTask As Outlook.TaskItem)
        OlObject = OlTask

        _TaskSubject = OlTask.Subject
        _TagContext = CustomField("TagContext")
        _TagProgram = CustomField("TagProgram")
        _TagProject = CustomField("TagProject")
        _TagPeople = CustomField("TagPeople")
        _TagTopic = CustomField("TagTopic")
        _KB = CustomField("KBF")
        _Priority = OlTask.Importance
        _TaskCreateDate = OlTask.CreationTime
        _StartDate = OlTask.StartDate
        _Complete = OlTask.Complete
    End Sub
    Public Sub New(OlMail As Outlook.MailItem, OnDemand As Boolean)
        OlObject = OlMail
        If OnDemand = False Then
            If OlMail.TaskSubject.Length <> 0 Then
                _TaskSubject = OlMail.TaskSubject
            Else
                _TaskSubject = OlMail.Subject
            End If
            _TagContext = CustomField("TagContext")
            _TagProgram = CustomField("TagProgram")
            _TagProject = CustomField("TagProject")
            _TagPeople = CustomField("TagPeople")
            _TagTopic = CustomField("TagTopic")
            _KB = CustomField("KBF")
            _Priority = OlMail.Importance
            _TaskCreateDate = OlMail.CreationTime
            _StartDate = OlMail.TaskStartDate
            _Complete = (OlMail.FlagStatus = OlFlagStatus.olFlagComplete)
        End If
    End Sub
    Public Sub New(OlTask As Outlook.TaskItem, OnDemand As Boolean)
        OlObject = OlTask
        If OnDemand = False Then
            _TaskSubject = OlTask.Subject
            _TagContext = CustomField("TagContext")
            _TagProgram = CustomField("TagProgram")
            _TagProject = CustomField("TagProject")
            _TagPeople = CustomField("TagPeople")
            _TagTopic = CustomField("TagTopic")
            _KB = CustomField("KBF")
            _Priority = OlTask.Importance
            _TaskCreateDate = OlTask.CreationTime
            _StartDate = OlTask.StartDate
            _Complete = OlTask.Complete
        End If
    End Sub
    Public Sub New(Item As Object, OnDemand As Boolean)

        OlObject = Item
        If OnDemand = False Then
            MsgBox("Coding Error: New ToDoItem() is overloaded. Only supply the OnDemand variable if you want to load values on demand")
        End If
    End Sub
    Public Sub New(strID As String)
        _ToDoID = strID
    End Sub

    Public ReadOnly Property TaskCreateDate As Date
        Get
            TaskCreateDate = _TaskCreateDate
        End Get
    End Property

    Public Property StartDate As Date
        Get
            Return _TaskCreateDate
        End Get
        Set(value As Date)
            _TaskCreateDate = value
        End Set
    End Property

    Public Property Priority As Outlook.OlImportance
        Get

            If OlObject Is Nothing Then
                _Priority = OlImportance.olImportanceNormal
            ElseIf TypeOf OlObject Is MailItem Then
                Dim OlMail As MailItem = OlObject
                _Priority = OlMail.Importance
            ElseIf TypeOf OlObject Is TaskItem Then
                Dim OlTask As TaskItem = OlObject
                _Priority = OlTask.Importance
            End If
            Return _Priority
        End Get
        Set(value As Outlook.OlImportance)
            _Priority = value
            If OlObject Is Nothing Then
            ElseIf TypeOf OlObject Is MailItem Then
                Dim OlMail As MailItem = OlObject
                OlMail.Importance = _Priority
                OlMail.Save()
            ElseIf TypeOf OlObject Is TaskItem Then
                Dim OlTask As TaskItem = OlObject
                OlTask.Importance = _Priority
                OlTask.Save()
            End If
        End Set
    End Property

    Public Property Complete As Boolean
        Get
            If OlObject Is Nothing Then
                _Complete = False
            ElseIf TypeOf OlObject Is MailItem Then
                Dim OlMail As MailItem = OlObject
                If (OlMail.FlagStatus = OlFlagStatus.olFlagComplete) Then
                    _Complete = True
                Else
                    _Complete = False
                End If
            ElseIf TypeOf OlObject Is TaskItem Then
                Dim OlTask As TaskItem = OlObject
                _Complete = OlTask.Complete
            End If
            Return _Complete
        End Get
        Set(value As Boolean)
            If OlObject Is Nothing Then
            ElseIf TypeOf OlObject Is MailItem Then
                Dim OlMail As MailItem = OlObject
                If value = True Then
                    OlMail.FlagStatus = OlFlagStatus.olFlagComplete
                Else
                    OlMail.FlagStatus = OlFlagStatus.olFlagMarked
                End If
                OlMail.Save()
            ElseIf TypeOf OlObject Is TaskItem Then
                Dim OlTask As TaskItem = OlObject
                OlTask.Complete = value
                OlTask.Save()
            End If
            _Complete = value
        End Set
    End Property

    Public Property TaskSubject As String
        Get
            If _TaskSubject.Length = 0 Then
                If OlObject Is Nothing Then
                    _TaskSubject = ""
                ElseIf TypeOf OlObject Is MailItem Then
                    Dim OlMail As MailItem = OlObject
                    _TaskSubject = OlMail.TaskSubject
                ElseIf TypeOf OlObject Is TaskItem Then
                    Dim OlTask As TaskItem = OlObject
                    _TaskSubject = OlTask.Subject
                Else
                    _TaskSubject = ""
                End If
            End If
            Return _TaskSubject
        End Get
        Set(value As String)
            _TaskSubject = value
            If OlObject Is Nothing Then
            ElseIf TypeOf OlObject Is MailItem Then
                Dim OlMail As MailItem = OlObject
                OlMail.TaskSubject = _TaskSubject
                OlMail.Save()
            ElseIf TypeOf OlObject Is TaskItem Then
                Dim OlTask As TaskItem = OlObject
                OlTask.Subject = _TaskSubject
                OlTask.Save()
            End If
        End Set
    End Property

    Public Property TagPeople As String
        Get
            If _TagPeople.Length <> 0 Then
                Return _TagPeople
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _TagPeople = CustomField("TagPeople")
                Return _TagPeople
            End If
        End Get
        Set(value As String)
            _TagPeople = value
            If Not OlObject Is Nothing Then
                CustomField("TagPeople") = value
            End If
        End Set
    End Property

    Public Property TagProject As String
        Get
            If _TagProject.Length <> 0 Then
                Return _TagProject
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _TagProject = CustomField("TagProject")
                Return _TagProject
            End If

        End Get
        Set(value As String)
            _TagProject = value
            If Not OlObject Is Nothing Then
                CustomField("TagProject") = value
            End If
        End Set
    End Property

    Public Property TagProgram As String
        Get
            If _TagProgram.Length <> 0 Then
                Return _TagProgram
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _TagProgram = CustomField("TagProgram", OlUserPropertyType.olKeywords)
                Return _TagProgram
            End If

        End Get
        Set(value As String)
            _TagProgram = value
            If Not OlObject Is Nothing Then
                CustomField("TagProgram", OlUserPropertyType.olKeywords) = value
            End If
        End Set
    End Property

    Public Property TagContext As String
        Get
            If _TagContext.Length <> 0 Then
                Return _TagContext
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _TagContext = CustomField("TagContext")
                Return _TagContext
            End If

        End Get
        Set(value As String)
            _TagContext = value
            If Not OlObject Is Nothing Then
                CustomField("TagContext") = value
            End If
        End Set
    End Property

    Public Property TagTopic As String
        Get
            If _TagTopic.Length <> 0 Then
                Return _TagTopic
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _TagTopic = CustomField("TagTopic")
                Return _TagTopic
            End If

        End Get
        Set(value As String)
            _TagTopic = value
            If Not OlObject Is Nothing Then
                CustomField("TagTopic") = value
            End If
        End Set
    End Property
    Public Property KB As String
        Get
            If _KB.Length <> 0 Then
                Return _KB
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _KB = CustomField("KBF")
                Return _TagTopic
            End If

        End Get
        Set(value As String)
            _KB = value
            If Not OlObject Is Nothing Then
                CustomField("KBF") = value
            End If
        End Set
    End Property
    Public Property ToDoID As String
        Get
            If _ToDoID.Length <> 0 Then
                Return _ToDoID
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _ToDoID = CustomField("ToDoID")
                Return _ToDoID
            End If
        End Get
        Set(strID As String)
            _ToDoID = strID
            If Not OlObject Is Nothing Then
                CustomField("ToDoID") = strID
                SplitID()
            End If
        End Set
    End Property

    Public Sub SplitID()
        Dim strField As String = ""
        Dim strFieldValue As String = ""
        Try
            Dim strToDoID As String = ToDoID
            Dim strToDoID_Len As Long = strToDoID.Length
            If strToDoID_Len > 0 Then
                Dim maxlen As Long = Globals.ThisAddIn.IDList.MaxIDLength

                For i = 2 To maxlen Step 2
                    strField = "ToDoIdLvl" & (i / 2)
                    strFieldValue = "00"
                    If i <= strToDoID_Len Then
                        strFieldValue = Mid(strToDoID, i - 1, 2)
                    End If
                    CustomField(strField) = strFieldValue
                Next
            End If
        Catch
            Debug.WriteLine("Error in Split_ToDoID")
            Debug.WriteLine(Err.Description)
            Debug.WriteLine("Field Name is " & strField)
            Debug.WriteLine("Field Value is " & strFieldValue)
            Stop
        End Try
    End Sub

    Public Property MetaTaskLvl As String
        Get
            If _MetaTaskLvl.Length <> 0 Then
                Return _MetaTaskLvl
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _MetaTaskLvl = CustomField("Meta Task Level")
                Return _MetaTaskLvl
            End If
        End Get
        Set(strLvl As String)
            _MetaTaskLvl = strLvl
            If Not OlObject Is Nothing Then
                CustomField("Meta Task Level") = strLvl
            End If
        End Set
    End Property

    Public Property MetaTaskSubject As String
        Get
            If _MetaTaskSubject.Length <> 0 Then
                Return _MetaTaskSubject
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _MetaTaskSubject = CustomField("Meta Task Subject")
                Return _MetaTaskSubject
            End If
        End Get
        Set(strID As String)
            _MetaTaskSubject = strID
            If Not OlObject Is Nothing Then
                'CustomFieldID_Set("Meta Task Subject", strID, SpecificItem:=OlObject)
                CustomField("Meta Task Subject") = strID
            End If
        End Set
    End Property

    Public Sub SwapIDPrefix(strPrefixOld, strPrefixNew)

    End Sub

    Public Function GetItem() As Object
        Return OlObject
    End Function

    Public ReadOnly Property InFolder() As String
        Get
            Return OlObject.Parent.FolderPath
        End Get
    End Property

    Public Property CustomField(FieldName As String, Optional ByVal OlFieldType As Outlook.OlUserPropertyType = Outlook.OlUserPropertyType.olText)
        Get
            Dim objProperty As Outlook.UserProperty = OlObject.UserProperties.Find(FieldName)
            If objProperty Is Nothing Then
                Return ""
            Else
                If IsArray(objProperty.Value) Then
                    Return FlattenArry(objProperty.Value)
                Else
                    Return objProperty.Value
                End If
            End If
        End Get
        Set(value)
            Dim objProperty As Outlook.UserProperty = OlObject.UserProperties.Find(FieldName)
            If objProperty Is Nothing Then
                Try
                    objProperty = OlObject.UserProperties.Add(FieldName, OlFieldType)
                    objProperty.Value = value
                    OlObject.Save()
                Catch e As System.Exception
                    Debug.WriteLine("Exception in Set User Property: " & FieldName)
                    Debug.WriteLine(e.Message)
                    Debug.WriteLine(e.Source)
                    Debug.WriteLine(e.StackTrace)

                End Try
            Else
                objProperty.Value = value
                OlObject.Save()
            End If

        End Set

    End Property

    Private Function FlattenArry(varBranch() As Object) As String
        Dim i As Integer
        Dim strTemp As String

        strTemp = ""

        For i = 0 To UBound(varBranch)
            If IsArray(varBranch(i)) Then
                strTemp = strTemp & ", " & FlattenArry(varBranch(i))
            Else
                strTemp = strTemp & ", " & varBranch(i)
            End If
        Next i
        If strTemp.Length <> 0 Then strTemp = Right(strTemp, Len(strTemp) - 2)
        FlattenArry = strTemp
    End Function

End Class

