# P5-T3 — Final QC step 3: Nullable / Type-Check Gate (remediation cycle 1, issue #614)

Timestamp: 2026-08-26T22-25

Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

`/p:Nullable=enable` was NOT added. `/t:Rebuild` was used; `/t:Build` was NOT substituted. This is
character-for-character the command in `.github/workflows/ci.yml`'s nullable step apart from the
target, which the repo policy requires to be `/t:Rebuild` on a warm local worktree.

EXIT_CODE: 0

## Output Summary

- `5 Warning(s)` / `0 Error(s)`; `Time Elapsed 00:00:11.24`.
- **Zero `CS86xx` nullable-flow diagnostics** across the entire log.
- Compilation genuinely occurred: 36 `csc.exe` invocations.
- Zero non-System.Reactive warnings; the 5 warnings are the pre-existing `packages.config`
  advisories, identical to the P0-T8 baseline.

## Nullable-specific notes for the edited files

`QuickFiler/Controllers/EfcSelectionGuard.cs` carries `#nullable enable` and therefore participates
in nullable analysis under this gate. Two null-forgiving operators are load-bearing and are the
reason the file compiles clean on .NET Framework 4.8.1, whose reference assemblies carry no
`[NotNullWhen]` annotation on `string.IsNullOrWhiteSpace`:

- `string value = selection!;` after the `string.IsNullOrWhiteSpace(selection)` guard — the
  file's pre-existing precedent, retained in both predicates.
- `ArchiveStemContract.TryMakeArchiveRelative(value, archiveRoot!, out _)` after the
  `!string.IsNullOrWhiteSpace(archiveRoot)` conjunct — required for the same reason; without it the
  call would raise CS8604 and, under `/p:TreatWarningsAsErrors=true`, break this gate.

`QuickFiler/Controllers/EfcFormController.cs` does not carry `#nullable enable` and is unaffected.
No `init`, `record`, or `record struct` construct was introduced (net48 has no `IsExternalInit`).
