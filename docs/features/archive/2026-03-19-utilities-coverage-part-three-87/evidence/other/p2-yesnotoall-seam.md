# P2-T10 Evidence: YesNoToAll.cs Seam Analysis

File: UtilitiesCS\Dialogs\YesNoToAll.cs
Decision: Seam Not Required

## Rationale

YesNoToAll.ShowDialog delegates to MyBox.ShowDialog(string, string, BoxIcon,
IList<DelegateButton>) — overload 1. That overload was patched in P2-T4 to use
MyBox.DialogInvoker instead of _viewer.ShowDialog().

Tests can inject MyBox.DialogInvoker with a lambda that:
  1. Calls the internal YesNoToAll.RespondYes() (or RespondNo/RespondYesToAll) to
     simulate a delegate-button click that sets YesNoToAll.Response.
  2. Returns DialogResult.OK to unblock ShowDialog.

This pattern covers all executable lines in ShowDialog without requiring a
separate seam property on YesNoToAll itself.

## Uncovered Members Closed by Test-Only Changes (P2-T11, P2-T12, P2-T13)

- YesNoToAll.ShowDialog body (all 9 executable lines via MyBox.DialogInvoker injection)
