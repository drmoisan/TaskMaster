# Static Shape Checks — [P4-T8]

Timestamp: 2026-09-14T11-41

Command:

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$cs = "TaskMaster/ThisAddIn.cs"
$cfg = "TaskMaster/app.config"
Write-Output ("STATIC_CTOR=" + @(Select-String -LiteralPath $cs -SimpleMatch -CaseSensitive -Pattern "static ThisAddIn()").Count)
Write-Output ("INSTALL_CALL=" + @(Select-String -LiteralPath $cs -SimpleMatch -CaseSensitive -Pattern "AssemblyBindingFallback.Install();").Count)
Write-Output ("EXCLUDE_ATTR=" + @(Select-String -LiteralPath $cs -SimpleMatch -CaseSensitive -Pattern "ExcludeFromCodeCoverage").Count)
Write-Output ("NETSTANDARD_IDENTITY=" + @(Select-String -LiteralPath $cfg -CaseSensitive -Pattern "name=.netstandard.").Count)
Write-Output ("OLD_VERSION_2_1=" + @(Select-String -LiteralPath $cfg -CaseSensitive -Pattern "oldVersion=.0\.0\.0\.0-2\.1\.0\.0.").Count)
Write-Output ("NEW_VERSION_2_0=" + @(Select-String -LiteralPath $cfg -CaseSensitive -Pattern "newVersion=.2\.0\.0\.0.").Count)
Write-Output ("FSHARP_REDIRECT=" + @(Select-String -LiteralPath $cfg -CaseSensitive -Pattern "oldVersion=.0\.0\.0\.0-11\.0\.0\.0.").Count)
'
```

The `Set-Location` prefix is mechanically necessary and measurement-neutral: this executor was launched
without worktree isolation, so its inherited working directory is a different checkout and every
repository-relative path in the plan would otherwise resolve into the wrong tree. It is disclosed here
rather than omitted.

EXIT_CODE: 0

Output Summary:

```
STATIC_CTOR=1
INSTALL_CALL=1
EXCLUDE_ATTR=1
NETSTANDARD_IDENTITY=1
OLD_VERSION_2_1=1
NEW_VERSION_2_0=1
FSHARP_REDIRECT=1
```

Acceptance Condition: MET. `STATIC_CTOR=1`, `INSTALL_CALL=1`, `EXCLUDE_ATTR` is 1 which meets the "at
least 1" floor, `NETSTANDARD_IDENTITY=1`, `OLD_VERSION_2_1=1`, `NEW_VERSION_2_0=1` and
`FSHARP_REDIRECT=1`.

`FSHARP_REDIRECT=1` is the unchanged-baseline control. The `FSharp.Core` redirect in
`TaskMaster/app.config` survived this work untouched; `[P3-T4]` inserted the new `dependentAssembly` block
after it and changed no line of it.

## Post-Format Shape Checks

Re-run by `[P5-T8]` after the final format pass at `[P5-T2]`. The re-run is required because
`[P5-T2]` rewrites tracked source across the whole tree and CSharpier 1.2.6 processes
`packages.config` and `*.xml` as well as `*.cs`, so a shape check taken before the format pass
describes the Phase 4 state rather than the terminal one.

Timestamp: 2026-09-14T11-54

Command: the `[P4-T8]` command, repeated verbatim, run from `<worktree-root>` via
`Set-Location -LiteralPath <worktree-root>`.

EXIT_CODE: 0

```
STATIC_CTOR=1
INSTALL_CALL=1
EXCLUDE_ATTR=1
NETSTANDARD_IDENTITY=1
OLD_VERSION_2_1=1
NEW_VERSION_2_0=1
FSHARP_REDIRECT=1
```

All seven values are identical to the pre-format reading above, so the format pass altered
none of the shapes these checks assert. `STATIC_CTOR=1`, `INSTALL_CALL=1`, `EXCLUDE_ATTR` at 1
meeting the "at least 1" floor, `NETSTANDARD_IDENTITY=1`, `OLD_VERSION_2_1=1`,
`NEW_VERSION_2_0=1` and `FSHARP_REDIRECT=1` are the same seven values `[P4-T8]` requires, so
the acceptance condition is met on the terminal state as well as on the Phase 4 state. In
particular `FSHARP_REDIRECT=1` confirms the `FSharp.Core` redirect survived the format pass
untouched, and `[P5-T4]` corroborates that independently with a deletion count of 0 for
`TaskMaster/app.config`.
