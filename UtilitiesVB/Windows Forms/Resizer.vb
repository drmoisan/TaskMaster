'-------------------------------------------------------------------------------
' Resizer
' This class is used to dynamically resize and reposition all controls on a form.
' Container controls are processed recursively so that all controls on the form
' are handled.
'
' Usage:
'  Resizing functionality requires only three lines of code on a form:
'
'  1. Create a form-level reference to the Resize class:
'     Dim myResizer as Resizer
'
'  2. In the Form_Load event, call the  Resizer class FIndAllControls method:
'     myResizer.FindAllControls(Me)
'
'  3. In the Form_Resize event, call the  Resizer class ResizeAllControls method:
'     myResizer.ResizeAllControls(Me)
'
'-------------------------------------------------------------------------------
Imports System.Diagnostics
Imports System.Drawing
Imports System.Windows.Forms

Public Class Resizer

    '----------------------------------------------------------
    ' ControlInfo
    ' Structure of original state of all processed controls
    '----------------------------------------------------------
    Private Structure ControlInfo
        Public name As String
        Public parentName As String
        Public leftOffsetPercent As Double
        Public topOffsetPercent As Double
        Public heightPercent As Double
        Public originalHeight As Integer
        Public originalWidth As Integer
        Public widthPercent As Double
        Public originalFontSize As Single
        Public ResizeType As ResizeDimensions
    End Structure

    Public Enum ResizeDimensions
        None = 0                '0b00000
        Position_Top = 1        '0b00001
        Position_Left = 2       '0b00010
        Position = 3            '0b00011
        Size_Width = 4          '0b00100
        Size_Height = 8         '0b01000
        Size = 12               '0b01100
        Font = 16               '0b10000
        All = 31                '0b11111
    End Enum

    '-------------------------------------------------------------------------
    ' ctrlDict
    ' Dictionary of (control name, control info) for all processed controls
    '-------------------------------------------------------------------------
    Private ReadOnly ctrlDict As New Dictionary(Of String, ControlInfo)

    '----------------------------------------------------------------------------------------
    ' FindAllControls
    ' Recursive function to process all controls contained in the initially passed
    ' control container and store it in the Control dictionary
    '----------------------------------------------------------------------------------------
    Public Sub FindAllControls(thisCtrl As Control, Optional strLeader As String = "")

        '-- If the current control has a parent, store all original relative position
        '-- and size information in the dictionary.
        '-- Recursively call FindAllControls for each control contained in the
        '-- current Control
        If strLeader.Length = 0 Then
            Debug.WriteLine(thisCtrl.Name)
        End If
        strLeader &= " "
        For Each ctl As Control In thisCtrl.Controls
            Try
                If Not IsNothing(ctl.Parent) Then
                    If Not ctl.Name.Length = 0 Then
                        Dim parentHeight = ctl.Parent.Height
                        Dim parentWidth = ctl.Parent.Width

                        Dim c As New ControlInfo With {
                            .name = ctl.Name,
                            .parentName = ctl.Parent.Name,
                            .topOffsetPercent = Convert.ToDouble(ctl.Top) / Convert.ToDouble(parentHeight),
                            .leftOffsetPercent = Convert.ToDouble(ctl.Left) / Convert.ToDouble(parentWidth),
                            .heightPercent = Convert.ToDouble(ctl.Height) / Convert.ToDouble(parentHeight),
                            .widthPercent = Convert.ToDouble(ctl.Width) / Convert.ToDouble(parentWidth),
                            .originalFontSize = ctl.Font.Size,
                            .originalHeight = ctl.Height,
                            .originalWidth = ctl.Width,
                            .ResizeType = ResizeDimensions.All
                        }
                        ctrlDict.Add(c.name, c)
                        Debug.WriteLine(strLeader & c.name)
                    Else
                        Debug.WriteLine("")
                    End If
                End If

            Catch ex As Exception
                Debug.Print(ex.Message)
            End Try

            If ctl.Controls.Count > 0 Then
                FindAllControls(ctl, strLeader)
            End If

        Next '-- For Each

    End Sub

    Public Sub PrintDict()
        For Each key In ctrlDict.Keys
            Debug.WriteLine(key & " " & ctrlDict(key).ResizeType)
        Next
    End Sub

    Public Function SetResizeDimensions(thisCtrl As Control, ResizeType As ResizeDimensions, IncludeChildren As Boolean) As Boolean
        '-- Get the current control's info from the control info dictionary
        Dim success As Boolean = True
        Dim c As New ControlInfo

        If thisCtrl.Name.Length > 0 Then
            success = ctrlDict.TryGetValue(thisCtrl.Name, c)
            If success Then
                c.ResizeType = ResizeType
                ctrlDict(thisCtrl.Name) = c
            End If
        End If
        If IncludeChildren Then
            For Each ctl As Control In thisCtrl.Controls
                If success Then
                    success = SetResizeDimensions(ctl, ResizeType, True)
                End If
            Next
        End If
        Return success
    End Function

    '----------------------------------------------------------------------------------------
    ' ResizeAllControls
    ' Recursive function to resize and reposition all controls contained in the Control
    ' dictionary
    '----------------------------------------------------------------------------------------
    Public Sub ResizeAllControls(thisCtrl As Control)

        Dim fontRatioW As Single
        Dim fontRatioH As Single
        Dim fontRatio As Single
        Dim f As Font

        '-- Resize and reposition all controls in the passed control
        For Each ctl As Control In thisCtrl.Controls
            Try
                If Not IsNothing(ctl.Parent) Then
                    Dim parentHeight = ctl.Parent.Height
                    Dim parentWidth = ctl.Parent.Width

                    Dim c As New ControlInfo

                    Dim ret As Boolean = False
                    Try
                        '-- Get the current control's info from the control info dictionary
                        If ctl.Name.Length > 0 Then
                            ret = ctrlDict.TryGetValue(ctl.Name, c)

                            '-- If found, adjust the current control based on control relative
                            '-- size and position information stored in the dictionary
                            If ret Then
                                '-- Size
                                If c.ResizeType And ResizeDimensions.Size_Width Then ctl.Width = Int(parentWidth * c.widthPercent)
                                If c.ResizeType And ResizeDimensions.Size_Height Then ctl.Height = Int(parentHeight * c.heightPercent)

                                '-- Position
                                If c.ResizeType And ResizeDimensions.Position_Top Then ctl.Top = Int(parentHeight * c.topOffsetPercent)
                                If c.ResizeType And ResizeDimensions.Position_Left Then ctl.Left = Int(parentWidth * c.leftOffsetPercent)

                                '-- Font
                                If c.ResizeType And ResizeDimensions.Font Then
                                    f = ctl.Font
                                    fontRatioW = ctl.Width / c.originalWidth
                                    fontRatioH = ctl.Height / c.originalHeight
                                    fontRatio = (fontRatioW + fontRatioH) / 2 '-- average change in control Height and Width
                                    ctl.Font = New Font(f.FontFamily, c.originalFontSize * fontRatio, f.Style)
                                End If
                            End If
                        End If
                    Catch
                    End Try
                End If
            Catch ex As Exception
            End Try

            '-- Recursive call for controls contained in the current control
            If ctl.Controls.Count > 0 Then
                ResizeAllControls(ctl)
            End If

        Next '-- For Each
    End Sub

End Class
