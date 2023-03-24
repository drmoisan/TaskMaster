Imports Microsoft.Office.Interop.Outlook

Friend Class AcceleratorParser
    Private _parent As QfcGroupOperationsLegacy

    Friend Sub New(Parent As QfcGroupOperationsLegacy)
        _parent = Parent
    End Sub

    Friend Sub ParseAndExecute(strToParse As String, _intActiveSelection As Integer)

        Dim intNewSelection As Integer
        Dim blExpanded As Boolean = False

        If AnythingToParse(strToParse) Then
            Dim idxLastNum As Integer = GetFinalNumericIndex(strToParse)
            If SelectionDetected(idxLastNum) Then
                intNewSelection = GetFinalNumeric(strToParse, idxLastNum)
                If _parent.IsSelectionBelowMax(intNewSelection) Then
                    If IsChange(intNewSelection, _intActiveSelection) Then
                        If IsAnythingActive(_intActiveSelection) Then blExpanded = _parent.ToggleOffActiveItem(blExpanded)
                        If intNewSelection > 0 Then
                            _parent.ActivateByIndex(intNewSelection, blExpanded)
                        End If
                    End If

                    If AdditionalInstructions(idxLastNum, strToParse) Then
                        Dim strCommand As String = ExtractInstruction(idxLastNum, strToParse)
                        _parent.ResetAcceleratorSilently()
                        Dim QF As QfcController = _parent.TryGetQfc(_intActiveSelection)

                        Select Case strCommand
                            Case "O"
                                _parent.toggleAcceleratorDialogue()
                                QF.KB(strCommand)
                                _parent.Parent.OpenQFMail(QF.Mail)
                            Case "C"
                                _parent.toggleAcceleratorDialogue()
                                QF.KB(strCommand)
                            Case "T"
                                _parent.toggleAcceleratorDialogue()
                                QF.KB(strCommand)
                            Case "F"
                                _parent.toggleAcceleratorDialogue()
                                QF.KB(strCommand)
                            Case "D"
                                _parent.toggleAcceleratorDialogue()
                                QF.KB(strCommand)
                            Case "X"
                                _parent.toggleAcceleratorDialogue()
                                QF.KB(strCommand)
                            Case "R"
                                _parent.toggleAcceleratorDialogue()
                                QF.KB(strCommand)
                            Case "A"
                                QF.KB(strCommand)
                            Case "W"
                                QF.KB(strCommand)
                            Case "M"
                                QF.KB(strCommand)
                            Case "E"
                                If QF.blExpanded Then
                                    _parent.MoveDownPix(_intActiveSelection + 1, QF.frm.Height * -0.5)
                                    QF.ExpandCtrls1()
                                Else
                                    _parent.MoveDownPix(_intActiveSelection + 1, QF.frm.Height)
                                    QF.ExpandCtrls1()
                                End If
                            Case Else
                        End Select
                    End If
                Else
                    _parent.ResetAcceleratorSilently()
                End If

            Else
                blExpanded = _parent.ToggleOffActiveItem(blExpanded)
            End If
        Else
            blExpanded = _parent.ToggleOffActiveItem(blExpanded)
        End If


    End Sub

    Private Function ExtractInstruction(idxLastNum As Integer, strToParse As String) As String
        Return UCase(Mid(strToParse, idxLastNum + 1, 1))
    End Function

    Private Function AdditionalInstructions(idxLastNum As Integer, strToParse As String) As Boolean
        If strToParse.Length > idxLastNum Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function IsAnythingActive(ActiveSelection As Integer) As Boolean
        If ActiveSelection <> 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function AnythingToParse(strToParse As String) As Boolean
        If strToParse <> "" Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function SelectionDetected(idxLastNum As Integer) As Boolean
        If idxLastNum > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function IsChange(intNewSelection As Integer, ActiveSelection As Integer) As Boolean
        If intNewSelection <> ActiveSelection Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function GetFinalNumeric(strToParse As String, idxLastNum As Integer) As Integer
        If idxLastNum > 0 Then
            'Get last digit 
            'TODO: Add support for multiple digit numbers 
            Return CInt(Mid(strToParse, 1, idxLastNum))
        Else
            Return 0
        End If
    End Function

    ''' <summary>
    ''' Gets the index of the last number in a string. Returns 0 if none is found
    ''' </summary>
    ''' <param name="strToParse"></param>
    ''' <returns></returns>
    Private Function GetFinalNumericIndex(strToParse As String) As Integer
        Dim i As Integer
        Dim intLastNum As Integer = 0
        Dim intLen As Integer = Len(strToParse)

        For i = 1 To intLen
            If IsNumeric(Mid(strToParse, i, 1)) Then
                intLastNum = i
            Else
                Exit For
            End If
        Next i
        Return intLastNum
    End Function

End Class
