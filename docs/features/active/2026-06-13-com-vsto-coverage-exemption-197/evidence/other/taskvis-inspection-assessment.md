# TaskVisualization Assess-by-Inspection Determination (P9-T4)

Timestamp: 2026-06-13T13-46

Scope: per Phase 9 directive, `FlagChangeGroup` and `EditFilterController` are assessed by
per-member inspection before annotating. A `using Microsoft.Office.Interop.Outlook` (or
`System.Windows.Forms`) directive alone is NOT determinative; the test is whether every member
is genuinely Outlook/WinForms-bound with no testable pure-logic seam.

## EditFilterController.cs

Determination: FULLY WinForms/Outlook-bound. No testable pure-logic seam.

Evidence (per member):
- Constructors (`EditFilterController(IApplicationGlobals)`, `(IApplicationGlobals, Action<...>)`,
  `(IApplicationGlobals, FilterEntry)`): all call `Initialize()`/`InitializeFactory()`.
- `Initialize()` / `InitializeFactory()`: construct `EditFilterViewer` (WinForms Form), read
  `_globals.Ol.NamespaceMAPI.Categories` (live Outlook MAPI), set viewer control text, and call
  `_viewer.Show()`.
- `DeleteFilterDialog(...)` (static): constructs the controller, calls `InitializeFactory()`,
  and `viewer.ShowDialog()` (modal WinForms dialog).
- `SelectItems(...)`: constructs `TagViewer` (WinForms) and calls `ShowDialog()`.
- `SetUpDeleteDialog()`: empty body.
- All event handlers (`CategorySelection_Click`, `PeopleSelection_Click`, `ProjectSelection_Click`,
  `TopicSelection_Click`, `FoldersSelected_Click`, `BtnCancel_Click`, `BtnOk_Click`) and
  `RegisterEventHandlers()`: wire/handle WinForms control Click events and call viewer methods.

Treatment applied: class-level `[ExcludeFromCodeCoverage]` + `using System.Diagnostics.CodeAnalysis;`.
No method body, signature, or public API changed.

## FlagChangeGroup.cs

Determination: PARTIALLY bound — contains both Outlook-bound members and one testable
pure-logic seam (`TryEnqueue`). Per the scope-change rule, method-level annotation is applied to
the genuinely Outlook-bound members only; the pure-logic seam remains measured.

Evidence (per member):
- Constructor `FlagChangeGroup(IApplicationGlobals globals, MailItem item)`: takes a live Outlook
  `MailItem`. Outlook-bound -> EXEMPT (method-level).
- `ProcessGroupAsync(CancellationToken)`: calls `MailItemHelper.FromMailItemAsync(Item, ...)` on a
  live MailItem and `helper.TokenizeAsync()`. Outlook-bound -> EXEMPT (method-level).
- `TryProcessFlagItemAsync(...)`: operates on the MailItemHelper backed by the live MailItem.
  Outlook-bound -> EXEMPT (method-level).
- `ProcessFlagItemAsync(...)`: invokes `Globals.AF.Manager` classifier train/untrain and
  `classifier.Serialize()`. Outlook/classifier-I/O-bound -> EXEMPT (method-level).
- `TryEnqueue(string classifierName, IEnumerable<string> original, IEnumerable<string> revised)`:
  PURE LOGIC. Calls the `CompareTo` collection-diff extension and adds in-memory `FlagChangeItem`
  objects to the `BlockingCollection`. No Outlook or WinForms dependency. -> NOT EXEMPT (preserved
  testable seam, mirrors the IDList method-level approach).
- Properties (`Globals`, `Item`, `Subject`, `FlagChangeItems`): simple accessors; left measured.

Treatment applied: `using System.Diagnostics.CodeAnalysis;` added; method-level
`[ExcludeFromCodeCoverage]` on the `MailItem` constructor, `ProcessGroupAsync`,
`TryProcessFlagItemAsync`, and `ProcessFlagItemAsync`. `TryEnqueue` and the property accessors are
NOT annotated. No method body, signature, or public API changed.

## Summary
- `EditFilterController`: class-level exemption applied.
- `FlagChangeGroup`: method-level exemption applied to 4 Outlook-bound members; `TryEnqueue`
  preserved as a measured pure-logic seam.
- No scope-change escalation required: both determinations are accommodated by the plan's P9-T4
  branches (class-level when fully bound, method-level when a testable seam exists).
