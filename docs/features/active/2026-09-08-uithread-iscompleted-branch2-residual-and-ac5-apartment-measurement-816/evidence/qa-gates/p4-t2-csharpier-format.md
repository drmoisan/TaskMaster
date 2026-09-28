# P4-T2 — Formatting step of the final QC toolchain

Timestamp: 2026-09-13T23-38

Command: `dotnet tool run csharpier format .` (run with the working directory set to the worktree
root), bracketed by two content-hash captures:

```
Get-FileHash -Algorithm SHA256 -LiteralPath UtilitiesCS\Threading\UiThread.cs, UtilitiesCS.Test\Threading\UiThread_Tests.cs, UtilitiesCS.Test\Threading\UiThreadInitContract_Tests.cs, UtilitiesCS.Test\Threading\UiThreadApartmentMeasurement_Tests.cs | Select-Object -ExpandProperty Hash > coverage\logs\p4-t2-hashes-before.log
Get-FileHash -Algorithm SHA256 -LiteralPath UtilitiesCS\Threading\UiThread.cs, UtilitiesCS.Test\Threading\UiThread_Tests.cs, UtilitiesCS.Test\Threading\UiThreadInitContract_Tests.cs, UtilitiesCS.Test\Threading\UiThreadApartmentMeasurement_Tests.cs | Select-Object -ExpandProperty Hash > coverage\logs\p4-t2-hashes-after.log
```

EXIT_CODE: 0

Output Summary:

This command rewrites tracked source and exits zero whether or not it changed anything, and its
printed figure is a processed count rather than a changed count, so two observations beyond the
exit code are recorded.

### Observation 1 — the verbatim final summary line the command printed

```
Formatted 1634 files in 6017ms.
```

That figure is the number of files CSharpier processed, not the number it rewrote, which is why it
cannot serve as the changed/unchanged observation on its own.

### Observation 2 — the four before and after content hashes

`Get-FileHash` emits one row per `-LiteralPath` operand in the order given, and both captures used
the identical operand order stated below, so the pairing is unambiguous.

| # | File (operand order) | Before | After | Pair equal |
|---|---|---|---|---|
| 1 | `UtilitiesCS\Threading\UiThread.cs` | `43BDAE415E68A7C866B1F4E0D0474E2177C62C998A7F254C84F95184A19ACB16` | `43BDAE415E68A7C866B1F4E0D0474E2177C62C998A7F254C84F95184A19ACB16` | **yes** |
| 2 | `UtilitiesCS.Test\Threading\UiThread_Tests.cs` | `B12538A63F7EF19450409B4CCB07FDE8164136959207DB2AC51DE15D12C56F28` | `B12538A63F7EF19450409B4CCB07FDE8164136959207DB2AC51DE15D12C56F28` | **yes** |
| 3 | `UtilitiesCS.Test\Threading\UiThreadInitContract_Tests.cs` | `65119417DFDB0F35ADDFFDAA6A7BB3A005BC951E44F2780205FB778D62F8DA30` | `65119417DFDB0F35ADDFFDAA6A7BB3A005BC951E44F2780205FB778D62F8DA30` | **yes** |
| 4 | `UtilitiesCS.Test\Threading\UiThreadApartmentMeasurement_Tests.cs` | `973F8C05D0EE11E46C3850666FE3834AA200EEE8EE7AED23D224EE2EC448C6E0` | `973F8C05D0EE11E46C3850666FE3834AA200EEE8EE7AED23D224EE2EC448C6E0` | **yes** |

Both captures contain exactly **four** hash lines, so neither FAIL condition on the line count is
met and no operand shifted the pairing silently.

**All four pairs are equal: the formatter changed no owned file.** This phase therefore does not
restart from P4-T2, and no changed file is named here because there is none.

The reason the formatter had nothing to do is recorded in the P2-T4 artifact: an interim
`dotnet tool run csharpier check .` during Phase 2 reported one line-ending difference in the newly
created test file, that file was converted to CRLF to match its siblings, and a second interim check
returned exit code 0 with zero lines matching `Was not formatted`. The equal hashes here are
therefore attributable to a tree that was already formatted, not to a formatter that failed to run —
the `Formatted 1634 files` line is the evidence that it did run.

Only the `Hash` property was selected, never `Path`, because `Get-FileHash` reports `Path` as an
absolute host path and this plan commits no absolute host path.

The project file is outside the hash span because `.csharpierignore` excludes `*.csproj` from
formatting, so a hash change there could not be attributed to this command.
