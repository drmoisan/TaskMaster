Partial Class EmailRibbonViewer
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
        Me.TabOlMail = Me.Factory.CreateRibbonTab
        Me.Group2 = Me.Factory.CreateRibbonGroup
        Me.FlagAsTask = Me.Factory.CreateRibbonButton
        Me.BTN_FlagTask = Me.Factory.CreateRibbonButton
        Me.TabOlMail.SuspendLayout()
        Me.Group2.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabOlMail
        '
        Me.TabOlMail.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office
        Me.TabOlMail.ControlId.OfficeId = "TabMail"
        Me.TabOlMail.Groups.Add(Me.Group2)
        Me.TabOlMail.Label = "TabMail"
        Me.TabOlMail.Name = "TabOlMail"
        '
        'Group2
        '
        Me.Group2.Items.Add(Me.FlagAsTask)
        Me.Group2.Label = "Task Master"
        Me.Group2.Name = "Group2"
        '
        'FlagAsTask
        '
        Me.FlagAsTask.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.FlagAsTask.Label = "Flag Task"
        Me.FlagAsTask.Name = "FlagAsTask"
        Me.FlagAsTask.OfficeImageId = "FlagMessage"
        Me.FlagAsTask.ShowImage = True
        '
        'BTN_FlagTask
        '
        Me.BTN_FlagTask.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge
        Me.BTN_FlagTask.Label = "Flag Task"
        Me.BTN_FlagTask.Name = "BTN_FlagTask"
        Me.BTN_FlagTask.OfficeImageId = "FlagMessage"
        Me.BTN_FlagTask.ShowImage = True
        '
        'EmailRibbonViewer
        '
        Me.Name = "EmailRibbonViewer"
        Me.RibbonType = "Microsoft.Outlook.Explorer"
        Me.Tabs.Add(Me.TabOlMail)
        Me.TabOlMail.ResumeLayout(False)
        Me.TabOlMail.PerformLayout()
        Me.Group2.ResumeLayout(False)
        Me.Group2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabOlMail As Microsoft.Office.Tools.Ribbon.RibbonTab
    Friend WithEvents Group2 As Microsoft.Office.Tools.Ribbon.RibbonGroup
    Friend WithEvents FlagAsTask As Microsoft.Office.Tools.Ribbon.RibbonButton
    Friend WithEvents BTN_FlagTask As Microsoft.Office.Tools.Ribbon.RibbonButton
End Class

Partial Class ThisRibbonCollection

    <System.Diagnostics.DebuggerNonUserCode()>
    Friend ReadOnly Property Ribbon_EM() As EmailRibbonViewer
        Get
            Return Me.GetRibbon(Of EmailRibbonViewer)()
        End Get
    End Property
End Class
