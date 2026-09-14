# File-Size Audit — [P4-T12]

Timestamp: 2026-09-14T11-45

Command:

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$paths = @("UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs","UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs","TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs","TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs","TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs","TaskMaster/ThisAddIn.cs")
foreach ($p in $paths) { Write-Output ($p + " LINES=" + @(Get-Content -LiteralPath $p).Count) }
'
```

The `Set-Location` prefix is mechanically necessary and measurement-neutral: this executor was launched
without worktree isolation, so its inherited working directory is a different checkout and every
repository-relative path in the plan would otherwise resolve into the wrong tree. It is disclosed here
rather than omitted.

EXIT_CODE: 0

Output Summary:

```
UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs LINES=458
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs LINES=383
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs LINES=361
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs LINES=466
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs LINES=123
TaskMaster/ThisAddIn.cs LINES=318
```

Acceptance Condition: MET. Every `LINES=` value is at most 500, so no split is required and no write-set
amendment is recorded.

## Headroom handed forward to Phase 5

This audit is taken BEFORE the first format pass. `[P5-T2]` is the first CSharpier run over the Revision R5
and Phase 3 edits, and `[P5-T8]` repeats this audit afterwards. The margins against the 500-line ceiling as
measured here are:

| File | Lines | Headroom |
|---|---|---|
| `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` | 466 | 34 |
| `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` | 458 | 42 |
| `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs` | 383 | 117 |
| `TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs` | 361 | 139 |
| `TaskMaster/ThisAddIn.cs` | 318 | 182 |
| `TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs` | 123 | 377 |

The two files with the least headroom are the ones Phase 5 must watch. `AssemblyBindingFallback.cs` grew
from 289 lines to 458 in `[P3-T1]`, which implemented the four ladder rungs and three private helpers in
place of the behaviour-empty seams. `NetstandardBindChildDomainTests.cs` was already at 466 when Phase 3
began and no Phase 3 or Phase 4 task edits it.

## Post-Format Line Counts

Recorded by `[P5-T8]` after the final format pass at `[P5-T2]`, which rewrote
`UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` and
`TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs`.

Timestamp: 2026-09-14T11-54

Command: the `[P4-T12]` command, repeated verbatim, run from `<worktree-root>` via
`Set-Location -LiteralPath <worktree-root>`.

EXIT_CODE: 0

```
UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs LINES=455
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs LINES=383
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs LINES=364
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs LINES=466
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs LINES=123
TaskMaster/ThisAddIn.cs LINES=318
```

Every `LINES=` value is at most 500, so the acceptance condition is met. No file was split, no
write-set amendment was required, and no `csproj` registration changed.

| File | Pre-format | Post-format | Headroom |
|---|---|---|---|
| `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` | 466 | 466 | 34 |
| `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` | 458 | 455 | 45 |
| `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs` | 383 | 383 | 117 |
| `TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs` | 361 | 364 | 136 |
| `TaskMaster/ThisAddIn.cs` | 318 | 318 | 182 |
| `TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs` | 123 | 123 | 377 |

The format pass did not push either watched file over the ceiling. It reduced
`AssemblyBindingFallback.cs` by three lines and added three to `ChildDomainBindProbe.cs`, and
it left `NetstandardBindChildDomainTests.cs` unchanged at 466, that file not having been
rewritten by the formatter. The largest post-format file is therefore
`NetstandardBindChildDomainTests.cs` at 466 lines with 34 lines of headroom. No hand-formatting
was applied against CSharpier output at any point.
