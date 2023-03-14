Imports System.IO
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook
Imports Microsoft.VisualBasic.FileIO
Imports Newtonsoft.Json
Imports ToDoModel
Imports UtilitiesVB

Public Class ApplicationGlobals
    Implements IApplicationGlobals

    Private ReadOnly _fs As AppFileSystemFolderPaths
    Private ReadOnly _olObjects As AppOlObjects
    Private ReadOnly _toDoObjects As AppToDoObjects

    Public Sub New(OlApp As Application)
        _fs = New AppFileSystemFolderPaths
        _olObjects = New AppOlObjects(OlApp)
        _toDoObjects = New AppToDoObjects(Me)
    End Sub

    Public ReadOnly Property FS As IFileSystemFolderPaths Implements IApplicationGlobals.FS
        Get
            Return _fs
        End Get
    End Property

    Public ReadOnly Property Ol As IOlObjects Implements IApplicationGlobals.Ol
        Get
            Return _olObjects
        End Get
    End Property

    Public ReadOnly Property TD As IToDoObjects Implements IApplicationGlobals.TD
        Get
            Return _toDoObjects
        End Get
    End Property

#Region "Legacy Definitions and Constants"


#End Region

End Class
