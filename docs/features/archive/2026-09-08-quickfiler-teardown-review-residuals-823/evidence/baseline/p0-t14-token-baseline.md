# Phase 0 — Token baseline for the R1, R3 and R4 removal acceptances

Timestamp: 2026-09-09T13-59

Task: [P0-T14]

Command: nine `git grep -c -F "<token>" -- <pathspec>` invocations, one per token, listed below.

EXIT_CODE: 0

Every one of the nine commands printed a count and exited 0, so every later absence acceptance
over these tokens is false-before and true-after rather than vacuous.

BASELINE-TOKEN: `_userEmailRetryAttempted;` | UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs | 1
BASELINE-TOKEN: `not per store` | UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs | 1
BASELINE-TOKEN: `Register_NullControlOrNullPredicate_IsIgnored` | QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs | 1
BASELINE-TOKEN: `Ignored when null` | QuickFiler/Viewers/QfcFormViewer.cs | 2
BASELINE-TOKEN: `(480 lines)` | QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs | 1
BASELINE-TOKEN: `is ignored rather than rejected` | QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs | 1
BASELINE-TOKEN: `ignored on the same reasoning` | QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs | 1
BASELINE-TOKEN: `registration hop runs from a form lookup` | QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs | 1
BASELINE-TOKEN: `registration hop runs from a form-lookup` | QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs | 1

Scoping notes, restating why three of the tokens are scoped to
`QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` rather than to the phrase `Ignored when null`:

- `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` does not carry the phrase `Ignored when
  null`. It carries `is ignored rather than rejected` at line 31, `ignored on the same reasoning` at
  line 35, and the form-lookup rationale at line 32. Those are the three tokens [P3-T5] removes.
- The phrase `Ignored when null` occurs twice, both in `QuickFiler/Viewers/QfcFormViewer.cs`, which
  is [P3-T6]'s file.
- The hyphenated spelling `registration hop runs from a form-lookup` is carried only by
  `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs`, inside the XML doc [P3-T1]
  rewrites. The spaced form scoped to the registry file does not cover it, so it is baselined
  separately.

No count is 0 and no command exited 1, so the stop-and-report branch of this task was not taken.

Output Summary: All nine tokens present at baseline with counts 1, 1, 1, 2, 1, 1, 1, 1 and 1
respectively. Every removal acceptance downstream is therefore discriminating.
