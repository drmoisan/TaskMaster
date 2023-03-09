Partial Class TaskRibbonViewer
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(TaskRibbonViewer))
        Me.Tab1 = Me.Factory.CreateRibbonTab
        Me.Group1 = Me.Factory.CreateRibbonGroup
        Me.BtnLoadTree = Me.Factory.CreateRibbonButton
        Me.BtnFlagTask = Me.Factory.CreateRibbonButton
        Me.ViewMenu = Me.Factory.CreateRibbonMenu
        Me.BtnHideHeadersNoChildren = Me.Factory.CreateRibbonButton
        Me.UtilitiesMenu = Me.Factory.CreateRibbonMenu
        Me.BtnRefreshIDList = Me.Factory.CreateRibbonButton
        Me.BtnSplitToDoID = Me.Factory.CreateRibbonButton
        Me.BtnReviseDictionary = Me.Factory.CreateRibbonButton
        Me.BtnCompressIDs = Me.Factory.CreateRibbonButton
        Me.BtnHookToggle = Me.Factory.CreateRibbonButton
        Me.BtnMigrateIDs = Me.Factory.CreateRibbonButton
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
        Me.Group1.Items.Add(Me.BtnLoadTree)
        Me.Group1.Items.Add(Me.BtnFlagTask)
        Me.Group1.Items.Add(Me.ViewMenu)
        Me.Group1.Items.Add(Me.UtilitiesMenu)
        Me.Group1.Label = "Task Master"
        Me.Group1.Name = "Group1"
        '
        'BtnLoadTree
        '
        Me.BtnLoadTree.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.BtnLoadTree.Label = "Load Tree"
        Me.BtnLoadTree.Name = "BtnLoadTree"
        Me.BtnLoadTree.OfficeImageId = "OutlineShowDetail"
        Me.BtnLoadTree.ShowImage = True
        '
        'BtnFlagTask
        '
        Me.BtnFlagTask.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.BtnFlagTask.Label = "Flag Task"
        Me.BtnFlagTask.Name = "BtnFlagTask"
        Me.BtnFlagTask.OfficeImageId = "FlagMessage"
        Me.BtnFlagTask.ShowImage = True
        '
        'ViewMenu
        '
        Me.ViewMenu.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.ViewMenu.Items.Add(Me.BtnHideHeadersNoChildren)
        Me.ViewMenu.ItemSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.ViewMenu.Label = "View"
        Me.ViewMenu.Name = "ViewMenu"
        Me.ViewMenu.OfficeImageId = "FindDialog"
        Me.ViewMenu.ShowImage = True
        '
        'BtnHideHeadersNoChildren
        '
        Me.BtnHideHeadersNoChildren.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.BtnHideHeadersNoChildren.Label = "Hide Empty Headers"
        Me.BtnHideHeadersNoChildren.Name = "BtnHideHeadersNoChildren"
        Me.BtnHideHeadersNoChildren.OfficeImageId = "ReviewShowOrHideComment"
        Me.BtnHideHeadersNoChildren.ShowImage = True
        '
        'UtilitiesMenu
        '
        Me.UtilitiesMenu.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.UtilitiesMenu.Image = CType(resources.GetObject("UtilitiesMenu.Image"), System.Drawing.Image)
        Me.UtilitiesMenu.Items.Add(Me.BtnRefreshIDList)
        Me.UtilitiesMenu.Items.Add(Me.BtnSplitToDoID)
        Me.UtilitiesMenu.Items.Add(Me.BtnReviseDictionary)
        Me.UtilitiesMenu.Items.Add(Me.BtnCompressIDs)
        Me.UtilitiesMenu.Items.Add(Me.BtnHookToggle)
        Me.UtilitiesMenu.Items.Add(Me.BtnMigrateIDs)
        Me.UtilitiesMenu.ItemSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.UtilitiesMenu.Label = "Utilities"
        Me.UtilitiesMenu.Name = "UtilitiesMenu"
        Me.UtilitiesMenu.ShowImage = True
        '
        'BtnRefreshIDList
        '
        Me.BtnRefreshIDList.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.BtnRefreshIDList.Label = "Refresh IDList"
        Me.BtnRefreshIDList.Name = "BtnRefreshIDList"
        Me.BtnRefreshIDList.OfficeImageId = "AccessRefreshAllLists"
        Me.BtnRefreshIDList.ShowImage = True
        '
        'BtnSplitToDoID
        '
        Me.BtnSplitToDoID.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.BtnSplitToDoID.Label = "Split ToDoID"
        Me.BtnSplitToDoID.Name = "BtnSplitToDoID"
        Me.BtnSplitToDoID.OfficeImageId = "ConvertTextToTable"
        Me.BtnSplitToDoID.ShowImage = True
        '
        'BtnReviseDictionary
        '
        Me.BtnReviseDictionary.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.BtnReviseDictionary.Label = "Revise Dictionary"
        Me.BtnReviseDictionary.Name = "BtnReviseDictionary"
        Me.BtnReviseDictionary.OfficeImageId = "EditQuery"
        Me.BtnReviseDictionary.ShowImage = True
        '
        'BtnCompressIDs
        '
        Me.BtnCompressIDs.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.BtnCompressIDs.Label = "Compress IDs"
        Me.BtnCompressIDs.Name = "BtnCompressIDs"
        Me.BtnCompressIDs.OfficeImageId = "ReviewCombineRevisions"
        Me.BtnCompressIDs.ShowImage = True
        '
        'BtnHookToggle
        '
        Me.BtnHookToggle.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.BtnHookToggle.Label = "UnHook Events"
        Me.BtnHookToggle.Name = "BtnHookToggle"
        Me.BtnHookToggle.OfficeImageId = "PositionAbsoluteMarks"
        Me.BtnHookToggle.ShowImage = True
        '
        'BtnMigrateIDs
        '
        Me.BtnMigrateIDs.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.BtnMigrateIDs.Label = "MigrateToDoIDs"
        Me.BtnMigrateIDs.Name = "BtnMigrateIDs"
        Me.BtnMigrateIDs.ShowImage = True
        '
        'TaskRibbonViewer
        '
        Me.Name = "TaskRibbonViewer"
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
    Friend WithEvents UtilitiesMenu As Microsoft.Office.Tools.Ribbon.RibbonMenu
    Friend WithEvents BtnRefreshIDList As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents BtnSplitToDoID As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents BtnLoadTree As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents BtnReviseDictionary As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents BtnCompressIDs As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents BtnMigrateIDs As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents BtnHookToggle As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents BtnFlagTask As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents ViewMenu As Microsoft.Office.Tools.Ribbon.RibbonMenu
    Friend WithEvents BtnHideHeadersNoChildren As Microsoft.Office.Tools.Ribbon.RibbonButton
End Class

Partial Class ThisRibbonCollection

    <System.Diagnostics.DebuggerNonUserCode()>
    Friend ReadOnly Property Ribbon_TM() As TaskRibbonViewer
        Get
            Return Me.GetRibbon(Of TaskRibbonViewer)()
        End Get
    End Property
End Class
