﻿Partial Class TaskMasterRibbon
    Inherits Microsoft.Office.Tools.Ribbon.RibbonBase

    <System.Diagnostics.DebuggerNonUserCode()> _
    Public Sub New(ByVal container As System.ComponentModel.IContainer)
        MyClass.New()

        'Required for Windows.Forms Class Composition Designer support
        If (container IsNot Nothing) Then
            container.Add(Me)
        End If

    End Sub

    <System.Diagnostics.DebuggerNonUserCode()> _
    Public Sub New()
        MyBase.New(Globals.Factory.GetRibbonFactory())

        'This call is required by the Component Designer.
        InitializeComponent()

    End Sub

    'Component overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Component Designer
    'It can be modified using the Component Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(TaskMasterRibbon))
        Me.Tab1 = Me.Factory.CreateRibbonTab
        Me.Group1 = Me.Factory.CreateRibbonGroup
        Me.TaskMenu = Me.Factory.CreateRibbonMenu
        Me.btn_RefreshMax = Me.Factory.CreateRibbonButton
        Me.btn_TreeView = Me.Factory.CreateRibbonButton
        Me.btn_SplitToDoID = Me.Factory.CreateRibbonButton
        Me.Btn_TreeListView = Me.Factory.CreateRibbonButton
        Me.but_Dictionary = Me.Factory.CreateRibbonButton
        Me.but_CompressIDs = Me.Factory.CreateRibbonButton
        Me.Tab1.SuspendLayout()
        Me.Group1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Tab1
        '
        Me.Tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office
        Me.Tab1.ControlId.OfficeId = "TabTasks"
        Me.Tab1.Groups.Add(Me.Group1)
        Me.Tab1.Label = "TabTasks"
        Me.Tab1.Name = "Tab1"
        '
        'Group1
        '
        Me.Group1.Items.Add(Me.TaskMenu)
        Me.Group1.Label = "Group1"
        Me.Group1.Name = "Group1"
        '
        'TaskMenu
        '
        Me.TaskMenu.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.TaskMenu.Image = CType(resources.GetObject("TaskMenu.Image"), System.Drawing.Image)
        Me.TaskMenu.Items.Add(Me.btn_RefreshMax)
        Me.TaskMenu.Items.Add(Me.btn_TreeView)
        Me.TaskMenu.Items.Add(Me.btn_SplitToDoID)
        Me.TaskMenu.Items.Add(Me.Btn_TreeListView)
        Me.TaskMenu.Items.Add(Me.but_Dictionary)
        Me.TaskMenu.Items.Add(Me.but_CompressIDs)
        Me.TaskMenu.ItemSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.TaskMenu.Label = "Menu1"
        Me.TaskMenu.Name = "TaskMenu"
        Me.TaskMenu.ShowImage = True
        '
        'btn_RefreshMax
        '
        Me.btn_RefreshMax.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.btn_RefreshMax.Label = "Refresh IDList"
        Me.btn_RefreshMax.Name = "btn_RefreshMax"
        Me.btn_RefreshMax.OfficeImageId = "AccessRefreshAllLists"
        Me.btn_RefreshMax.ShowImage = True
        '
        'btn_TreeView
        '
        Me.btn_TreeView.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.btn_TreeView.Label = "Load Tree View"
        Me.btn_TreeView.Name = "btn_TreeView"
        Me.btn_TreeView.OfficeImageId = "BaselineSave"
        Me.btn_TreeView.ShowImage = True
        '
        'btn_SplitToDoID
        '
        Me.btn_SplitToDoID.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.btn_SplitToDoID.Label = "Split ToDoID"
        Me.btn_SplitToDoID.Name = "btn_SplitToDoID"
        Me.btn_SplitToDoID.OfficeImageId = "ConvertTextToTable"
        Me.btn_SplitToDoID.ShowImage = True
        '
        'Btn_TreeListView
        '
        Me.Btn_TreeListView.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.Btn_TreeListView.Label = "Load Tree 2"
        Me.Btn_TreeListView.Name = "Btn_TreeListView"
        Me.Btn_TreeListView.OfficeImageId = "BaselineSave"
        Me.Btn_TreeListView.ShowImage = True
        '
        'but_Dictionary
        '
        Me.but_Dictionary.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.but_Dictionary.Label = "Revise Dictionary"
        Me.but_Dictionary.Name = "but_Dictionary"
        Me.but_Dictionary.OfficeImageId = "EditQuery"
        Me.but_Dictionary.ShowImage = True
        '
        'but_CompressIDs
        '
        Me.but_CompressIDs.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.but_CompressIDs.Label = "Compress IDs"
        Me.but_CompressIDs.Name = "but_CompressIDs"
        Me.but_CompressIDs.OfficeImageId = "ReviewCombineRevisions"
        Me.but_CompressIDs.ShowImage = True
        '
        'TaskMasterRibbon
        '
        Me.Name = "TaskMasterRibbon"
        Me.RibbonType = "Microsoft.Outlook.Explorer"
        Me.Tabs.Add(Me.Tab1)
        Me.Tab1.ResumeLayout(False)
        Me.Tab1.PerformLayout()
        Me.Group1.ResumeLayout(False)
        Me.Group1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Tab1 As Microsoft.Office.Tools.Ribbon.RibbonTab
    Friend WithEvents Group1 As Microsoft.Office.Tools.Ribbon.RibbonGroup
    Friend WithEvents TaskMenu As Microsoft.Office.Tools.Ribbon.RibbonMenu
    Friend WithEvents btn_RefreshMax As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents btn_TreeView As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents btn_SplitToDoID As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents Btn_TreeListView As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents but_Dictionary As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents but_CompressIDs As Microsoft.Office.Tools.Ribbon.RibbonButton
End Class

Partial Class ThisRibbonCollection

    <System.Diagnostics.DebuggerNonUserCode()> _
    Friend ReadOnly Property Ribbon1() As TaskMasterRibbon
        Get
            Return Me.GetRibbon(Of TaskMasterRibbon)()
        End Get
    End Property
End Class
