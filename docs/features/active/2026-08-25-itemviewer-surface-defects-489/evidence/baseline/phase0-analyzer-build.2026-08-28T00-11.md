# Phase 0 — Analyzer Build Baseline (P0-T11) — re-run, supersedes 2026-08-27T23-26

Timestamp: 2026-08-28T00-11
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal /fl "/flp:LogFile=docs\features\active\itemviewer-surface-defects-489\evidence\qa-gates\phase0-analyzer-build.2026-08-28T00-09.msbuild.txt;Verbosity=normal"
EXIT_CODE: 0
ExpectedExitCode: 0

BaselineAnalyzerWarningCount: 5

## Supersession

This artifact supersedes `evidence/baseline/phase0-analyzer-build.2026-08-27T23-26.md`, which
recorded `EXIT_CODE: 1` and `0 Warning(s) / 10 Error(s)`. That failure was the inherited
`CS0006` analyzer version skew described in the superseded artifact: the `<Analyzer Include>`
HintPaths in the project files name `Meziantou.Analyzer.3.0.156` and
`Roslynator.Analyzers.4.16.0`, while `packages.config` declares `3.0.174` and `4.16.1`.

The skew is **pre-existing repository state on `origin/main` and on the epic integration branch,
not a defect of this feature, and out of scope for it**. It was cleared for this worktree only by
placing the two named package folders under the worktree's gitignored `packages/` directory
(`.gitignore:349`). **No tracked file was changed**: no `.csproj`, no `packages.config`, no
`<Analyzer Include>` entry was edited by this feature, and none may be. `git status --porcelain`
was empty immediately before this run.

The superseded artifact is retained as the audit record of the blocked first attempt.

## What the build reported

- `Build succeeded.` with `5 Warning(s)` and `0 Error(s)` (log lines 5696, 5723, 5724).
- Time elapsed 00:00:14.19. Eighteen distinct `*.csproj` projects reported a `(Rebuild target)`
  entry in the log.
- All five warnings are the same diagnostic, one per project, raised by
  `packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`:
  "The project contains a packages.config file, which is not supported by System.Reactive v7.0
  or later." The five projects are `UtilitiesCS`, `UtilitiesCS.Test`, `ToDoModel`, `QuickFiler`
  and `TaskMaster`. It is a package-authoring advisory about `packages.config`, not a Roslyn
  analyzer diagnostic, and it is pre-existing repository state unrelated to this feature.
- Zero occurrences of any `CS`, `CA`, `IDE`, `MA` or `RCS` diagnostic code.

## Non-vacuity proof

Occurrences of the literal `Skipping target "CoreCompile"` in
`evidence/qa-gates/phase0-analyzer-build.2026-08-28T00-09.msbuild.txt`: **0**.

The count was taken over the whole 5728-line log. `CoreCompile` appears 75 times in the log and
the `Csc` task was invoked for every project, so the analyzers genuinely ran. `/t:Rebuild` is
load-bearing here: a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project
and runs no analyzers. A `csc.exe` occurrence count is **not** an acceptable substitute for this
literal count and is not used as one.

## Log artifact

`evidence/qa-gates/phase0-analyzer-build.2026-08-28T00-09.msbuild.txt` exists on disk, is 5728
lines, and `git check-ignore -v <path>` exits `1` (no ignore rule matches), which proves the
directory-creation step ran, that no `MSB1029` occurred, and that the log is committable. The
extension is `.msbuild.txt` and not `.log`, because `.gitignore:84` is `*.log`.

All absolute path prefixes in the log were redacted before commit: 10267 occurrences of the
worktree root to `<repo-root>` and 36 occurrences of the main checkout root to
`<main-checkout-root>` (the latter arise from `.editorconfig` inheritance walking out of the
worktree). No account name and no machine name remains in the log; the only absolute paths left
are `C:\Program Files\...` tool locations, which identify neither.

Output Summary: The analyzer gate **passes**. `msbuild TaskMaster.sln /t:Rebuild` with
`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exited `0` with `Build succeeded.`,
`5 Warning(s)`, `0 Error(s)` across eighteen projects. `BaselineAnalyzerWarningCount: 5`; all five
are the identical pre-existing `System.Reactive` `packages.config` advisory, one per project, and
none is a Roslyn analyzer diagnostic. The gate is non-vacuous: the literal
`Skipping target "CoreCompile"` occurs **0** times in the `/v:normal` file log. The log exists
under `evidence/qa-gates/` with a `.msbuild.txt` extension and is not gitignored. This run
supersedes the 2026-08-27T23-26 artifact, which failed `CS0006` on an inherited analyzer version
skew that has since been cleared in this worktree without changing any tracked file.
