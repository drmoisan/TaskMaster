# P11-T4 — Analyzer build, final QC (loop iteration 1)

Timestamp: 2026-08-28T02-18
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal /fl "/flp:LogFile=docs\features\active\itemviewer-surface-defects-489\evidence\qa-gates\p11-t4-analyzer-build.2026-08-28T02-17.msbuild.txt;Verbosity=normal"
EXIT_CODE: 0

FinalAnalyzerWarningCount: 5

Loop iteration: **1**.

## What the build reported

- `Build succeeded.` (log line 11848) with `5 Warning(s)` (line 11875) and `0 Error(s)` (line 11876).
- Time elapsed 00:00:15.28.
- The log carries **50** `(Rebuild target)` entries across the solution and its eighteen projects.
- **Zero** occurrences of any `CS`, `CA`, `IDE`, `MA` or `RCS` diagnostic code anywhere in the log.
  No Roslyn analyzer diagnostic was raised.

### The five warnings

All five are the same non-Roslyn diagnostic, one per project, raised by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`:

> The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
> later. Please migrate to PackageReference.

The five projects are `UtilitiesCS`, `UtilitiesCS.Test`, `ToDoModel`, `QuickFiler` and `TaskMaster`.
The warning text appears **10** times in the log because MSBuild prints each once inline during the
build and once again in the end-of-build warning summary; the deduplicated MSBuild count is `5`,
which is the figure recorded as `FinalAnalyzerWarningCount:`.

This is a package-authoring advisory about `packages.config`, not an analyzer finding, and it is
pre-existing repository state unrelated to this feature. It is the identical set P0-T11 recorded.

## Acceptance

**(a) `EXIT_CODE: 0`.** Observed `0`.

**(b) The `.msbuild.txt` log exists under `evidence/qa-gates/` and is not gitignored.**
`evidence/qa-gates/p11-t4-analyzer-build.2026-08-28T02-17.msbuild.txt` exists on disk at 11880 lines
and 2415963 bytes after redaction. `git check-ignore -v <path>` exits `1`, meaning no ignore rule
matches it, so the file is committable. The extension is `.msbuild.txt` and **not** `.log`, because
`.gitignore:84` is `*.log` and a `.log` artifact under `FEATURE/evidence/` could never be committed
while every porcelain gate still reported clean.

**(c) `FinalAnalyzerWarningCount:` is not greater than `BaselineAnalyzerWarningCount:`.**
`5` is not greater than `5`, the value recorded in
`evidence/baseline/phase0-analyzer-build.2026-08-28T00-11.md`. The two sets are not merely equal in
size but identical in content: the same five projects raise the same single `System.Reactive`
advisory in both runs, and both runs raise zero Roslyn diagnostics.

## Redaction

Every absolute path prefix in the log was replaced before the artifact was committed:

- 13631 occurrences of the worktree root replaced with `<repo-root>`.
- 36 further occurrences of the main checkout root replaced with `<main-checkout-root>`. These arise
  from `.editorconfig` inheritance walking above the worktree.

Post-redaction verification on the file as it now stands on disk: **0** occurrences of the account
name and **0** occurrences of the machine name, both searched case-insensitively; 13631 `<repo-root>`
tokens present. Line endings are unchanged — 11880 CRLF terminators and **0** bare LF — and the file
carries no BOM, exactly as MSBuild wrote it. The redaction touched path prefixes only; no diagnostic
text was altered, and the `5 Warning(s)` / `0 Error(s)` lines and the `Skipping target` search space
P11-T5 counts over are untouched. The only absolute paths remaining are `C:\Program Files\…` tool
locations, which identify neither the account nor the machine.

## Loop consequence

The stage passed and rewrote no source file. The build writes only into `bin/` and `obj/`, which are
gitignored, plus the evidence log. No restart is triggered; the loop proceeds to P11-T5.

Output Summary: The analyzer gate **passes** at loop iteration 1. `msbuild TaskMaster.sln /t:Rebuild`
with `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exited `0` with `Build succeeded.`,
`5 Warning(s)` and `0 Error(s)` in 15.28 seconds over 50 `(Rebuild target)` entries.
`FinalAnalyzerWarningCount: 5` is **not greater than** `BaselineAnalyzerWarningCount: 5`, and the two
sets are identical in content: the same pre-existing `System.Reactive` `packages.config` advisory
raised once each by `UtilitiesCS`, `UtilitiesCS.Test`, `ToDoModel`, `QuickFiler` and `TaskMaster`.
Zero `CS`, `CA`, `IDE`, `MA` or `RCS` diagnostics appear anywhere in the log. The `/v:normal` file log
exists under `evidence/qa-gates/` with a `.msbuild.txt` extension, is not matched by
`git check-ignore`, and has been redacted to `<repo-root>` and `<main-checkout-root>` with zero
residual account or machine identifiers.
