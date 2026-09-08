# [P5-T2] Final QC loop, step 1 verification — formatting check

Timestamp: 2026-09-08T02-37

Command: `dotnet tool run csharpier check .` (run after the SDK preamble)

EXIT_CODE: 0

## Output Summary

```
Checked 1611 files in 6632ms.
```

## Arithmetic

The baseline value is located in `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/baseline/p0-t9-csharpier-check.md` by its `BASELINE_CHECKED_FILES:` token.

| Quantity | Value |
|---|---|
| `BASELINE_CHECKED_FILES:` recorded by [P0-T9] | 1608 |
| Files this delivery creates | 3 |
| Expected checked-file count | 1611 |
| Observed checked-file count | 1611 |
| Difference | 0 |

The plus-three is the three files this delivery creates: `UtilitiesCS/Threading/IUiCaptureSource.cs`, `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`, and `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs`. The expected value is derived from the recorded token rather than from any figure tabled in the plan, so a baseline correction would propagate without editing the task.

`coverage/` and `TestResults/` are git-ignored and CSharpier 1.2.6 honours `.gitignore`, so neither the derived coverage settings file nor any results tree enters this count.

TOOLCHAIN_LOOP_PASS: 1
