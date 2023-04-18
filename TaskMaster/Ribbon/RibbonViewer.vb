'TODO:  Follow these steps to enable the Ribbon (XML) item:

'1: Copy the following code block into the ThisAddin, ThisWorkbook, or ThisDocument class.

'Protected Overrides Function CreateRibbonExtensibilityObject() As Microsoft.Office.Core.IRibbonExtensibility
'    Return New Ribbon()
'End Function

'2. Create callback methods in the "Ribbon Callbacks" region of this class to handle user
'   actions, such as clicking a button. Note: if you have exported this Ribbon from the
'   Ribbon designer, move your code from the event handlers to the callback methods and
'   modify the code to work with the Ribbon extensibility (RibbonX) programming model.

'3. Assign attributes to the control tags in the Ribbon XML file to identify the appropriate callback methods in your code.

'For more information, see the Ribbon XML documentation in the Visual Studio Tools for Office Help.

<Runtime.InteropServices.ComVisible(True)>
Public Class RibbonViewer
    Implements Office.IRibbonExtensibility

    Private _ribbon As Office.IRibbonUI
    Private _controller As RibbonController

    Public Sub New(Controller As RibbonController)
        _controller = Controller
    End Sub

    Public Sub SetController(Controller As RibbonController)
        _controller = Controller
    End Sub

    Public Function GetCustomUI(ByVal ribbonID As String) As String Implements Office.IRibbonExtensibility.GetCustomUI
        Return GetResourceText("TaskMaster.RibbonViewer.xml")
    End Function

#Region "Ribbon Callbacks"
    'Create callback methods here. For more information about adding callback methods, visit https://go.microsoft.com/fwlink/?LinkID=271226
    Public Sub Ribbon_Load(ByVal ribbonUI As Office.IRibbonUI)
        Me._ribbon = ribbonUI
        _controller.SetViewer(Me)
    End Sub

    Public Sub BtnLoadTree_Click(ByVal control As Office.IRibbonControl)
        _controller.LoadTaskTree()
    End Sub

    Public Sub FlagAsTask_Click(ByVal control As Office.IRibbonControl)
        _controller.FlagAsTask()
    End Sub

    Public Sub BtnHideHeadersNoChildren_Click(ByVal control As Office.IRibbonControl)
        _controller.HideHeadersNoChildren()
    End Sub

    Public Sub BtnRefreshIDList_Click(ByVal control As Office.IRibbonControl)
        _controller.RefreshIDList()
    End Sub

    Public Sub BtnSplitToDoID_Click(ByVal control As Office.IRibbonControl)
        _controller.SplitToDoID()
    End Sub

    Public Sub BtnReviseProjectInfo_Click(ByVal control As Office.IRibbonControl)
        _controller.ReviseProjectInfo()
    End Sub

    Public Sub BtnCompressIDs_Click(ByVal control As Office.IRibbonControl)
        _controller.CompressIDs()
    End Sub

    Public Sub BtnHookToggle_Click(ByVal control As Office.IRibbonControl)
        _controller.ToggleEventsHook(_ribbon)
    End Sub

    Public Function GetHookButtonText(control As Office.IRibbonControl) As String
        Return _controller.GetHookButtonText(control)
    End Function

    Public Sub BtnMigrateIDs_Click(ByVal control As Office.IRibbonControl)
        MsgBox("Not Implemented")
    End Sub

    Public Sub QuickFilerOld_Click(ByVal control As Office.IRibbonControl)
        _controller.LoadQuickFilerOld()
    End Sub

    Public Sub QuickFiler_Click(ByVal control As Office.IRibbonControl)
        _controller.LoadQuickFiler()
    End Sub

#End Region

#Region "Helpers"

    Private Shared Function GetResourceText(ByVal resourceName As String) As String
        Dim asm As Reflection.Assembly = Reflection.Assembly.GetExecutingAssembly()
        Dim resourceNames() As String = asm.GetManifestResourceNames()
        For i As Integer = 0 To resourceNames.Length - 1
            If String.Compare(resourceName, resourceNames(i), StringComparison.OrdinalIgnoreCase) = 0 Then
                Using resourceReader As IO.StreamReader = New IO.StreamReader(asm.GetManifestResourceStream(resourceNames(i)))
                    If resourceReader IsNot Nothing Then
                        Return resourceReader.ReadToEnd()
                    End If
                End Using
            End If
        Next
        Return Nothing
    End Function

#End Region

End Class
