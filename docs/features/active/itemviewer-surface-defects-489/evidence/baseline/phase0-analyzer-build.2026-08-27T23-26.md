# Phase 0 — Analyzer Build Baseline (P0-T11)

Timestamp: 2026-08-27T23-26
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal /fl "/flp:LogFile=docs\features\active\itemviewer-surface-defects-489\evidence\qa-gates\phase0-analyzer-build.2026-08-27T23-22.msbuild.txt;Verbosity=normal"
EXIT_CODE: 1

BaselineAnalyzerWarningCount: 0

## ACCEPTANCE NOT MET — this task is recorded but NOT checked off

P0-T11's acceptance requires `EXIT_CODE: 0`. The observed exit code is `1`. Under the plan's
fail-closed evidence rule the verdict for this task is BLOCKED, never PASS, and the plan checkbox for
`[P0-T11]` is left unchecked.

## What the build reported

- `Build FAILED.` with `0 Warning(s)` and `10 Error(s)` (log lines 543-544).
- All ten errors are `CS0006: Metadata file ... could not be found`, five raised by
  `VBFunctions/VBFunctions.csproj` and five by `UtilitiesCS/UtilitiesCS.csproj`. The missing files are
  the Meziantou.Analyzer 3.0.156 analyzer DLL and the four Roslynator.Analyzers 4.16.0 analyzer DLLs.
- Every other project in the solution reported FAILED transitively, because they depend on
  `UtilitiesCS`. `QuickFiler` and `QuickFiler.Test` never reached `CoreCompile`.

## Root cause — an inherited analyzer version skew, not caused by this feature

The `<Analyzer Include>` items and the `<Import>` / `<Error Condition>` items in the same project
files name different versions of the same two packages:

| Item | Version named | Present in packages/? |
|---|---|---|
| Import of Meziantou.Analyzer.props, and its Error Condition | 3.0.174 | yes |
| Analyzer Include for Meziantou.Analyzer.dll | 3.0.156 | **no** |
| packages.config id Roslynator.Analyzers | 4.16.1 | yes |
| Analyzer Include for the four Roslynator DLLs | 4.16.0 | **no** |

`packages/` contains exactly `Meziantou.Analyzer.3.0.174/` and `Roslynator.Analyzers.4.16.1/`, and
`QuickFiler/packages.config` declares version 3.0.174 and version 4.16.1. `nuget restore` (P0-T8)
exited 0 reporting that all packages listed in packages.config are already installed, so the restore
is complete and correct; the skew is between packages.config and the Analyzer Include HintPaths,
which a NuGet upgrade bumped on one side only.

The condition is **inherited, not introduced here**. A name-only diff of every project file against
the branch base `69e8317152c0a9ee6ee6e65db0ef81f6906189b1` returns **zero** paths, so no project file
on this branch differs from the branch base. There is direct precedent in the history: commit
`46ca9210 fix(build): repair NuGet upgrade fallout blocking CI` performed exactly this repair for the
previous bump, moving the same five Analyzer Include lines from Meziantou.Analyzer 3.0.138 and
Roslynator.Analyzers 4.15.0 to 3.0.156 and 4.16.0. The subsequent bump to 3.0.174 and 4.16.1 landed
without the matching repair.

## Why no remedy was applied

Repairing this requires editing the Analyzer Include items in the affected project files. That is
prohibited here on three independent grounds and was **not** done:

1. Phase 0's own preamble: no production, test, or project file may be edited in this phase.
2. This feature's scope lock: `spec.md` requires `QuickFiler/QuickFiler.csproj` to be absent from the
   diff, and permits `QuickFiler.Test/QuickFiler.Test.csproj` to gain exactly two Compile Include
   entries and nothing else. `UtilitiesCS.csproj` and `VBFunctions.csproj` are outside this feature's
   file set entirely, and no `UtilitiesCS` file may appear in the diff.
3. The atomic-execution contract forbids performing work not described by the plan. A repo-wide
   build-configuration repair is an independent outcome, not a micro-action of this task.

`scripts/vscode/Sync-PackageReferences.ps1` exists and reconciles project references against
packages.config. It was **not** run, because it mutates project files across the solution. It is
recorded here as the likely remedy for whoever owns the fix, not as an action taken.

## Non-vacuity of the analyzer invocation

- Occurrences of the literal `Skipping target "CoreCompile"` in the `/v:normal` log: **0**.
- The only `Skipping target` occurrences in the log are 8 of `Skipping target "CopyMSTestV2Resources"`,
  which is unrelated.
- `CoreCompile` was entered on 4 projects before the failure, so `/t:Rebuild` did invalidate the
  up-to-date check as intended. The zero count establishes that the command shape is non-vacuous; the
  build nevertheless failed for the reason above, so the warning count below is not a usable baseline.

## BaselineAnalyzerWarningCount interpretation

`BaselineAnalyzerWarningCount:` is recorded as `0` because that is the integer the log reports
(`0 Warning(s)`). It is **not** a meaningful analyzer-warning baseline: the compile aborted at
`CoreCompile` on the two root projects, and `QuickFiler`, `QuickFiler.Test` and every other downstream
project were never compiled, so no analyzer ran over this feature's files. Any later comparison
against this figure — spec AC49, which requires the post-change analyzer warning count to be not
greater than the Phase 0 baseline — would be comparing against a floor of zero produced by a failed
build, and would be unsatisfiable rather than merely strict. This must be re-baselined once the
analyzer reference skew is repaired.

## Log artifact

The `/v:normal` file log named by the LogFile parameter exists on disk at
`docs/features/active/itemviewer-surface-defects-489/evidence/qa-gates/phase0-analyzer-build.2026-08-27T23-22.msbuild.txt`
(548 lines). It is **not** matched by `git check-ignore`, which exits 1 for it, so it is committable;
the `.msbuild.txt` extension was used precisely because `.gitignore:84` is `*.log`. Absolute path
prefixes in the log were redacted before commit: the worktree root to `<repo-root>` and the parent
checkout root to `<main-checkout-root>`. The second prefix appears because `.editorconfig` inheritance
walks up out of the worktree, so csc receives an analyzerconfig argument for the parent checkout's
`.editorconfig` as well as the worktree's. After redaction the log contains zero occurrences of the
account name and zero of the machine name.

Output Summary: The analyzer build **FAILED** with `EXIT_CODE: 1`, `0 Warning(s)` and `10 Error(s)`,
all `CS0006` from an inherited analyzer version skew: the Analyzer Include HintPaths name
Meziantou.Analyzer 3.0.156 and Roslynator.Analyzers 4.16.0 while packages.config and `packages/`
carry 3.0.174 and 4.16.1. Zero project files differ from the branch base, so the breakage is
pre-existing and belongs to the repository, not to this feature. `Skipping target "CoreCompile"`
occurs **0** times, so the command shape is non-vacuous. P0-T11's acceptance condition `EXIT_CODE: 0`
is **not met**; the task is recorded and left unchecked, and the recorded warning count of `0` is not
a usable baseline because no analyzer ran over this feature's files.
