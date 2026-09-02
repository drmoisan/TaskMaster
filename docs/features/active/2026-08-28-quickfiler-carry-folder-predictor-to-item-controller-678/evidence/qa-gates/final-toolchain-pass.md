# P2-T12 — Final toolchain clean-pass declaration (AC19)

Timestamp: 2026-09-02T00-28

## The five commands of the final pass, in order

### 1. Format apply (P2-T1)

- Timestamp: 2026-09-02T00-05
- Command: `dotnet tool run csharpier format .`
- EXIT_CODE: 0
- Output Summary: `Formatted 1574 files in 2132ms.` `git status --porcelain` taken immediately before
  and immediately after the command was **identical**, so the command rewrote no path. Because
  CSharpier prints a processed-file count rather than a rewritten-file count, and exits 0 either
  way, the before-and-after tree observation is what distinguishes a clean run from a repairing one.
  Detail: `evidence/qa-gates/csharpier-format.md`.

### 2. Format verify (P2-T2) — AC19 gate 1

- Timestamp: 2026-09-02T00-06
- Command: `dotnet tool run csharpier check .`
- EXIT_CODE: 0
- Output Summary: `Checked 1574 files in 4846ms.` No file was reported as needing formatting; the
  reported set is empty. This is a read-only command whose exit code is a real signal.
  Detail: `evidence/qa-gates/csharpier-check.md`.

### 3. Analyzer build (P2-T3) — AC19 gate 2

- Timestamp: 2026-09-02T00-07
- Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- EXIT_CODE: 0
- Output Summary: `5 Warning(s)`, `0 Error(s)`. The warning count equals the
  `BASELINE_ANALYZER_SUMMARY` count of 5 and all five are the same uncoded System.Reactive
  `packages.config` warnings; no coded diagnostic of any kind was emitted. `CoreCompile:` ran 63
  times, so the gate was not vacuous. Detail: `evidence/qa-gates/analyzer-build.md`.

### 4. Nullable build (P2-T4) — AC19 gate 3

- Timestamp: 2026-09-02T00-08
- Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 0
- Output Summary: `5 Warning(s)`, `0 Error(s)`. No `CS86` diagnostic was reported, matching the empty
  P0-T7 baseline enumeration. `CoreCompile:` ran 71 times.
  Detail: `evidence/qa-gates/nullable-build.md`.

### 5. MSTest run with coverage (P2-T5) — AC19 gate 4

- Timestamp: 2026-09-02T00-10
- Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .`
- EXIT_CODE: 0
- Output Summary: `Test Run Successful.` `Total tests: 6946`, `Passed: 6946`, `Failed: 0`,
  `Skipped: 0`, `Total time: 30.1704 Seconds`. The run printed the literal
  `Done. Coverage artifact:`, so the coverage document on disk is post-processed. Post-change
  repository-wide line coverage 85.41 %, branch coverage 79.45 %.
  Detail: `evidence/qa-gates/mstest-coverage-run.md`.

These five cover the four AC19 gates — format verification, analyzer build, nullable build and the
MSTest run — plus the format-apply step that precedes them. Each carries its own `Timestamp:`,
`Command:`, `EXIT_CODE:` and `Output Summary:` above and in its own artifact.

## All five ran in the same uninterrupted pass

The five commands above were executed in sequence with no source edit between them. Nothing was
changed after the format-apply step and before the test run, so the tree the analyzer build compiled
is the tree the test run exercised and the tree `csharpier check` verified.

**P2-T1 left no net change under `QuickFiler/` or `QuickFiler.Test/` during that pass.** Its
before-and-after `git status --porcelain` outputs were identical, so it rewrote no path at all,
inside or outside those prefixes. The restoration carve-out the Phase 2 preamble defines — a path
P2-T1 rewrote outside the two prefixes and then restored with `git checkout <base-ref> --`, which is
listed by name and does not falsify this clause — **is not engaged, because no path was rewritten
and none was restored**. There is nothing to list under it.

## Loop restarts: 2

The Phase 2 loop ran three times. Both restarts were triggered by a source change this executor made
in response to a gate's own finding, which is exactly what the restart rule is for.

### Restart 1 — triggered by the P2-T7 coverage measurement

**Reason.** The first pass completed all five commands cleanly, but the P2-T7 per-member coverage
measurement recorded `QfcQueue.ItemControllerFactory`, a **new** member, at 1/12 executable lines
covered (8.33 %), failing AC20's 90 % new-member threshold. The cause was the seam's parameter type:
it took the concrete `QfcItemGroup`, whose `ItemViewer` member is the concrete WinForms `ItemViewer`,
so the production default could not be invoked without a live window.

**Change made.** The seam's viewer parameter was narrowed from `QfcItemGroup` to the `IItemViewer`
interface, and its mail-item argument passed separately, so a test can invoke the default with a Moq
double. `ItemControllerFactory_OnAFreshQueue_HasANonNullProductionDefault` was replaced by
`ItemControllerFactory_DefaultInvocation_BuildsControllerCarryingTheHandler`, which invokes the
default and asserts the constructed controller received the carried handler. The member moved from
1/12 (8.33 %) to **11/11 (100 %)**.

**Files changed:** `QuickFiler/Controllers/QfcQueue.Enqueue.cs`,
`QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs`. Both under the two in-scope prefixes, so
this is a genuine net change and the restart was mandatory.

### Restart 2 — triggered by the P2-T8 attribute-invariant gate

**Reason.** P2-T8 reported **1 added line** carrying the token `ExcludeFromCodeCoverage`, against a
required count of 0. The line was not an attribute application: it was an XML documentation comment
in `QuickFiler/Controllers/QfcCollectionController.CarrierLoad.cs` that quoted the token while
explaining why that part carries no attribute of its own. The gate is a plain token search over diff
lines and cannot distinguish the two.

**Change made.** The comment was reworded to name the attribute in prose without quoting its token,
and now records why. An independent census of attribute applications on both sides was added to the
artifact as a second measurement immune to prose mentions; it reports 46 on each side. The gate now
reports 0 added and 0 removed.

**Files changed:** `QuickFiler/Controllers/QfcCollectionController.CarrierLoad.cs`. Under an in-scope
prefix, so the restart was mandatory.

### Why neither restart is a gate being talked around

In both cases a gate reported a real finding and the code was changed to satisfy it, then the whole
loop was re-run from P2-T1. Neither gate was reinterpreted, weakened or waived. The second case is
worth stating plainly: the alternative to rewording the comment was to declare the finding a false
positive and pass anyway, which would have established that a documentation mention can sit in the
diff and be dismissed — removing the gate's ability to discriminate for every future change.

## Outstanding gate failure, not resolved by this pass

AC19's four gates all pass. **AC20 does not fully pass.** Its clause "every new or modified member
reaches at least 90 % line coverage" fails for two members, `QfcQueue.EnqueueAsync` (0/46) and
`QfcQueue.LoadControllersViewersAsync` (0/24), both COM- and WinForms-bound and both uncovered before
this change as well. The full figures, the argument that no regression occurred, and the reason the
shortfall was not resolved are in `evidence/qa-gates/coverage-delta.md`. AC20 is left unchecked in
`issue.md`.
