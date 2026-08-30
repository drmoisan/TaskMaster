---
name: analyzer-hintpath-skew-breaks-all-four-gates
description: A NuGet bump that updates packages.config but not the <Analyzer Include> HintPaths fails msbuild with CS0006, and /t:Rebuild then cascades it into the nullable, vstest and coverage baselines
metadata:
  type: project
---

A NuGet analyzer bump can land on `packages.config` + the `<Import>`/`<Error Condition>` items while
leaving the `<Analyzer Include>` HintPaths on the OLD version. `csc` is handed a path that does not
exist and the build dies `CS0006: Metadata file ... could not be found`.

Observed 2026-08-27 on `epic/quickfiler-bug-family-integration`: HintPaths named
`Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0` while `packages.config` and
`packages/` carried `3.0.174` and `4.16.1`. 10 errors, 0 warnings, raised by `UtilitiesCS.csproj`
and `VBFunctions.csproj`; every dependent project then reported FAILED transitively.

**Why:** `nuget restore` reports "All packages listed in packages.config are already installed" and
exits 0, so the restore gate looks healthy and gives no hint. The skew is between two *different*
places in the same `.csproj`. Precedent: commit `46ca9210 fix(build): repair NuGet upgrade fallout
blocking CI` performed exactly this repair for the previous bump (3.0.138/4.15.0 -> 3.0.156/4.16.0),
so this recurs on every bump that is not followed by the repair.

**How to apply:**
- Diagnose in one command: compare `grep 'Analyzer Include' *.csproj` against `ls packages/`. If the
  versions differ, this is the bug — do not chase the compile error.
- **One failure becomes four.** Both msbuild gates use `/t:Rebuild`, which *cleans* `bin/Debug`
  before failing. So the analyzer gate, the nullable gate, the vstest gate (assembly now absent:
  "The test source file ... was not found") and the repo coverage gate (only assemblies with no
  `UtilitiesCS` dependency survive — here just `SVGControl.Test`) all fail from the single cause.
  Diagnose once and cross-reference; do not re-investigate each.
- An executor under a Phase-0 "no project file may be edited" rule, or a feature scope-lock that
  excludes `UtilitiesCS`/`VBFunctions`, **must not** repair it — record and escalate.
  `scripts/vscode/Sync-PackageReferences.ps1` is the likely remedy but mutates csproj files
  solution-wide.
- **Third option, better than escalating a blocked gate (confirmed 2026-08-30, issue #644):** provision
  the HintPath-named versions into the gitignored `packages/` tree instead of editing anything:
  `nuget install Meziantou.Analyzer -Version 3.0.156 -OutputDirectory packages -DependencyVersion Ignore`
  and the same for `Roslynator.Analyzers -Version 4.16.0`. `packages/` is gitignored, so this changes
  ZERO tracked files (`git status --porcelain -- '*.csproj' '*.config' '*.props' '*.targets'` stays
  empty) and the mandated msbuild command then runs unmodified — 0 errors. This is the same class of
  action as installing the repo-local SDK or running the packages.config restore, both of which a cold
  worktree needs anyway. Verify it is pre-existing first: `git show origin/main:UtilitiesCS/UtilitiesCS.csproj`
  carried the identical skew, and `git diff --name-only origin/main...HEAD -- '*.csproj'` proved the
  branch never touched the skewed projects.
- **It only bites a COLD worktree.** A long-lived worktree accumulates package dirs across bumps and
  `nuget restore` never deletes superseded ones, so the old 3.0.156/4.16.0 folders are still present and
  the stale HintPaths still resolve. That is why a prior cycle on the same branch built fine and a fresh
  agent worktree fails: do not read the failure as a regression introduced by the current change.
- Never record the resulting `0 Warning(s)` as a baseline. It is the absence of a measurement, and a
  later "warning count not greater than baseline" gate would compare against a floor of zero.
  Likewise a `Skipped: 0` from a 1-assembly coverage run poisons any later equality gate, because a
  healthy full run reports 3 (the live `[Ignore]` tests in `UtilitiesCS.Test`).

Related: [[project_analyzer_version_skew_fresh_worktree]],
[[project_incremental_build_vacuous_baseline]], [[project_build_test_env]]
