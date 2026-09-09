# Phase 1 — Write Set line counts after the cleanup regression tests

Timestamp: 2026-09-09T12-52
Task: [P1-T5]

Command: the `[P0-T13]` line-count command, re-run unchanged.

```text
pwsh -NoProfile -Command '@("QuickFiler/Controllers/QfcHomeController.cs","QuickFiler/Controllers/EfcHomeController.cs","UtilitiesCS/Threading/ProgressViewer.cs","UtilitiesCS/Threading/ProgressPane.cs","QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs","QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs","UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs","UtilitiesCS.Test/Threading/ProgressPane_Tests.cs") | ForEach-Object { "{0} {1}" -f $_, (Get-Content -LiteralPath $_).Count }'
```

EXIT_CODE: 0

Verbatim output:

```text
QuickFiler/Controllers/QfcHomeController.cs 498
QuickFiler/Controllers/EfcHomeController.cs 445
UtilitiesCS/Threading/ProgressViewer.cs 92
UtilitiesCS/Threading/ProgressPane.cs 61
QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs 192
QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs 492
UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs 352
UtilitiesCS.Test/Threading/ProgressPane_Tests.cs 192
```

## Budget check for the two files this phase edited

| File | Baseline | Now | Budget | Within budget |
|---|---|---|---|---|
| `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` | 159 | **192** | at most 200 | yes, 8 lines spare |
| `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs` | 459 | **492** | at most 495 | yes, 3 lines spare |

Both are also below the repository's 500-line ceiling. The other six files are unchanged at their
baseline counts, confirming this phase touched only the two test files it was scoped to.

Output Summary: `QfcHomeControllerCleanupTests.cs` is 192 lines, at most 200 as required.
`EfcHomeControllerLifecycleTests.cs` is 492 lines, at most 495 as required. The remaining margin on
the second file is 3 lines against its phase budget and 8 lines against the 500-line ceiling;
`[P6-T13]` re-checks both after the final CSharpier pass, which is the last write to any `.cs` file.
