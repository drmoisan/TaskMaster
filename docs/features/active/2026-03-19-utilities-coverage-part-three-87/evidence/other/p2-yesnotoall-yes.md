# P2-T11 Evidence: YesNoToAll.cs Yes Path Test

Test Method: ShowDialog_SeamInvokesRespondYes_ReturnsYesResponse
Test File: UtilitiesCS.Test\Dialogs\YesNoToAll_Tests.cs
Test Class: YesNoToAll_Tests

## Scenario

Injects MyBox.DialogInvoker lambda that calls YesNoToAll.RespondYes() then returns DialogResult.OK.
Calls YesNoToAll.ShowDialog("Test message").
Asserts result is YesNoToAllResponse.Yes.

## Coverage Result

File: UtilitiesCS\Dialogs\YesNoToAll.cs
Line-rate: 1.0 (100%)
Threshold: >= 0.80
Status: PASS
