# [P4-T2] Phase 4 format and builds

Timestamp: 2026-09-08T02-18

Command: `git status --porcelain --untracked-files=all` (before-image)

Command: `dotnet tool run csharpier format .`

Command: `git status --porcelain --untracked-files=all` (after-image)

Command: `dotnet tool run csharpier check .`

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

## Output Summary

Before-image:

```
 M QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs
```

Formatter output:

```
Formatted 1611 files in 3149ms.
```

After-image:

```
 M QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs
```

The two images are identical. The one listed path is the sole Phase 4 Write Set file. Phases 1 through 3 were committed at `09bb952d`, `f7294d71` and `21126792`, which is why no earlier path appears.

Read-only check:

```
Checked 1611 files in 5963ms.
```

Analyzer build:

```
    0 Warning(s)
    0 Error(s)
```

Nullable build:

```
    0 Warning(s)
    0 Error(s)
```

Both builds used `/t:Rebuild`, and neither passed `/p:Nullable=enable`.

## Second pass, run after the [P4-T5] line-budget trim

PASS_2_TIMESTAMP: 2026-09-08T02-30

[P4-T5] measured `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` at 468 physical lines, 8 above the 460-line authoring budget [P2-T1] states. The overshoot was introduced by the [P3-T6] correction, which replaced the ambient-apartment arrangement in two methods with a dedicated-MTA-thread arrangement. Documentation comments in that file were trimmed to bring it back to exactly 460 lines. No executable statement and no assertion was changed by the trim, and every acceptance token of [P2-T1] through [P2-T5] still returns its required count.

A file change requires the toolchain loop to restart, so the whole sequence above was re-run.

PASS_2_BEFORE_IMAGE:

```
 M QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs
 M UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs
?? docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/qa-gates/p4-t2-format-and-builds.md
?? docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/qa-gates/p4-t3-quickfiler-tests.md
?? docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/qa-gates/p4-t4-utilitiescs-tests.md
```

PASS_2_FORMATTER_OUTPUT: `Formatted 1611 files in 3826ms.`

PASS_2_AFTER_IMAGE: byte-identical to the before-image above. The three untracked paths are this artifact and the two the sibling Phase 4 tasks wrote.

PASS_2_FORMAT_CHECK: `Checked 1611 files in 6599ms.`, exit 0.

PASS_2_ANALYZER_BUILD: exit 0, `    0 Warning(s)` and `    0 Error(s)`.

PASS_2_NULLABLE_BUILD: exit 0, `    0 Warning(s)` and `    0 Error(s)`.

PASS_2_EXIT_CODE: 0

[P4-T3] and [P4-T4] were both re-run against this state and both remained green; each records its own second pass. This second pass is the last formatter run of Phase 4, and it is the state [P4-T5] measures.
