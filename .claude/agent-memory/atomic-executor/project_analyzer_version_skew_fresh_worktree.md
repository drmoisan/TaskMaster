---
name: analyzer-version-skew-fresh-worktree
description: Fresh TaskMaster worktree fails EVERY build (not just the analyzer gate) with CS0006 — 16 csproj <Analyzer Include> paths lag packages.config; it recurs after each Dependabot analyzer bump and blocks the whole plan, so surface it at P0-T13
metadata:
  type: project
---

On a clean TaskMaster worktree, the first `msbuild TaskMaster.sln ... /t:Rebuild` fails with
`error CS0006: Metadata file '..\packages\<Analyzer>.<oldVersion>\...\<X>.dll' could not be found`.

**Why:** pre-existing repo-wide skew. `packages.config` and the `<Import>`/`<Error>` restore-check lines are
bumped by Dependabot ("Bump the analyzers-dev-deps group"), but the hand-written `<Analyzer Include>` DLL
paths in **16** first-party csproj files still name the OLD versions. On dev/CI machines the old package
folders linger in the gitignored `packages/`; a clean-worktree restore installs only the current
`packages.config` versions, so the old folders are absent.

Observed instances (the pair moves with each bump; re-measure, do not assume):
- 2026-08 @ `61edc19` (epic/quickfiler-bug-family-integration): `<Analyzer Include>` names
  `Meziantou.Analyzer.3.0.156` + `Roslynator.Analyzers.4.16.0`; restore installs `3.0.174` + `4.16.1`.
  AsyncFixer 2.1.0, BannedApiAnalyzers 5.6.0, SonarAnalyzer.CSharp 10.32.0.713 all agree — only 2 skew.
- earlier: Meziantou 3.0.101, BannedApi 3.3.4, Sonar 10.27.0.140913.

**Blast radius is the whole solution, not just the analyzer gate.** `<Analyzer Include>` is unconditional, so
the plain nullable Rebuild (`/p:TreatWarningsAsErrors=true`, no analyzer properties) fails identically. Only
`VBFunctions` and `UtilitiesCS` emit the 10 diagnostics; the other 13 projects fail transitively on the broken
project reference. `SVGControl` / `SVGControl.Test` are the only projects that build (they carry no stale
reference). Consequence: `<Test>/bin/Debug` is EMPTY, so any full-suite or scoped vstest task and any coverage
baseline is unreachable, and every "analyzer Rebuild returns EXIT_CODE: 0" acceptance in the plan is
unsatisfiable. Diagnose it at the first analyzer gate rather than at the first test run.

**How to apply:**
1. Verify it is pre-existing, not yours: `git status --porcelain -- '*.csproj' '*/packages.config'` must be
   empty; then `ls -d packages/Meziantou* packages/Roslynator*` vs
   `grep -n "Analyzer Include" <proj>.csproj`. The `<Import>` at csproj line ~3 shows the NEW version while
   the `<Analyzer Include>` block shows the OLD one — that contrast is the fingerprint.
2. Remedy that touches no tracked file: install the missing OLD versions into the gitignored `packages/`
   (`nuget install Meziantou.Analyzer -Version <old> -OutputDirectory packages`, likewise Roslynator).
3. **But check the delegation prompt first.** A caller may explicitly forbid this — the #498 execution
   directive said "If a restore still produces `error CS0006` from skewed analyzer package versions, report it
   rather than working around it." In that case record the red baseline truthfully with `ExpectedExitCode: 1`
   and stop at the first task that needs a built assembly, rather than provisioning the packages.
4. The durable fix is upstream: update the 16 `<Analyzer Include>` version strings in the same commit as the
   `packages.config` bump. Worth an issue; it recurs on every analyzer bump.

**Resolved instance, 2026-08-26 @ `61edc19` (#498).** After the orchestrator provisioned the two missing
gitignored package dirs, the SAME analyzer and nullable Rebuild recipes went from `EXIT_CODE: 1` / 10 CS0006
to `EXIT_CODE: 0`, 0 errors, 5 warnings (all the uncoded `System.Reactive.PackagesConfigCheck.targets`
packages.config advisory, on QuickFiler/TaskMaster/ToDoModel/UtilitiesCS/UtilitiesCS.Test). `git status
--porcelain -- packages` stayed empty, so the ownership gate was untouched. All 9 test assemblies then
existed and the full suite ran 6482/6482 green at 84.78% repo line rate.

**Never carry a CS0006 baseline forward as the plan's baseline.** Under a Baseline-Comparison Rule, a
non-zero Phase-0 analyzer/nullable baseline permanently DEGRADES the Phase-8 gates into gates that cannot
fail, and licenses every intermediate "analyzer Rebuild returns 0" check to pass while compiling nothing.
If the provisioning gap is later fixed, re-run both gates and OVERWRITE the artifacts, deleting the
`ExpectedExitCode: 1` line — the field is what normalizes a red row to `pass` in PR-body collection.
