# Phase 6 — Toolchain step 3: nullable gate

Timestamp: 2026-09-09T14-41

Task: [P6-T4]

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

This is the [P0-T10] command verbatim, including `/t:Rebuild` and adding no `/p:Nullable=enable`.
A normal-verbosity file log was written to the session scratchpad so the D6 non-vacuity observation
could be derived mechanically; the log is a local run output and is not committed.

EXIT_CODE: 0

Verbatim `Build succeeded.` block:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

NULLABLE-WARNINGS: 0
NULLABLE-ERRORS: 0
NULLABLE-PROJECTS-COMPILED: 18
SKIPPED-CORECOMPILE-OCCURRENCES: 0

Comparison against [P0-T10]: `BASELINE-NULLABLE-WARNINGS` was 0 and `BASELINE-NULLABLE-ERRORS` was
0, so both post-change values are less than or equal to their baseline counterparts.
`BASELINE-NULLABLE-PROJECTS-COMPILED` was 18 and `NULLABLE-PROJECTS-COMPILED` is 18, so they are
equal as D6 requires. The literal `Skipping target "CoreCompile"` occurs zero times.

## What this gate confirms

This is the confirming gate for the specification's R3.7 prediction of no new CS86xx diagnostic and
for the D18 and R3.6 constraint on the test file.

- `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` carries `#nullable enable` on line 1, so its
  CS86xx diagnostics are promoted to errors here. The two new
  `throw new ArgumentNullException(nameof(...));` statements and the retained `== null` comparison
  form raised none. Flow analysis narrows both parameters to non-null on the fall-through path, so
  `_owners[itemViewer] = popupIsOpen;` stayed clean, which is the D17 prediction.
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` also carries `#nullable enable`, so
  the replacement `HashSet<StoreWrapper>` field raised no CS86xx either.
- `QuickFiler/Viewers/QfcFormViewer.cs`, `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` and
  `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs` are nullable-oblivious and can
  raise no CS86xx. In particular the test file gained no `#nullable enable`, so its two literal
  null arguments raised no CS8625, which is the diagnostic D18 exists to avoid.

D7 check: zero occurrences of MSB3061 and zero of MSB3021.

Output Summary: Solution-wide nullable rebuild with warnings treated as errors passed at exit 0
with 0 warnings and 0 errors, equal to the baseline on both counters. 18 distinct projects compiled
with zero skipped `CoreCompile` targets. The R3.7 no-new-diagnostic prediction is confirmed.
