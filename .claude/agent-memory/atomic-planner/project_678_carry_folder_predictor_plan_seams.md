---
name: project-678-carry-folder-predictor-plan-seams
description: Issue #678 minor-audit planning seams - coverage runner throws twice before writing, Cobertura .//line double-counts, FolderHandlingTests.cs at 498/500 and non-partial, tool manifest at repo root, fail-closed CreateGate reflection helper, non-relocatable lambda-body construction sites
metadata:
  type: project
---

Planning seams found while authoring the atomic plan for issue #678 (carry the folder predictor from
the confidence gate to the item controller, `QuickFiler` + `QuickFiler.Test`, work mode
`minor-audit`).

**Why:** each one silently breaks an acceptance condition that reads as correct, and none is visible
from the issue body or the research document.

**How to apply:** re-verify each against the tree before reusing, then encode the guard.

## `Invoke-MSTestWithCoverage.ps1` has TWO throws before it writes the post-processed file

`Invoke-DotnetCoverageCollection` throws on a non-zero coverage exit code (`:235-237`) **and**
`Assert-CoberturaLineCoverageThreshold` throws below 80 percent (`Helpers.ps1:487-490`). Both run
before `Set-Content` at `:343`. Either leaves the UNFILTERED report on disk, so a filtered baseline
compared against an unfiltered post-change run reports a phantom double-digit regression. The
memory entry [[project_494_threshold_reconciliation_plan_seams]] records the red-run half; the
threshold half is additional and fires even on a fully green suite.

Observed success-case literal: `Done. Coverage artifact:` (`:344`) is printed only after
post-processing, the threshold assert, and the on-disk write all succeed. It is the only stdout
token that proves the file is post-processed. Deterministic fallback: dot-source
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and call
`ConvertTo-KoverageCoberturaXml` yourself, writing to `coverage/coverage.postprocessed.cobertura.xml`
(`coverage/*` is gitignored at `.gitignore:144`).

## Never count Cobertura `.//line`

`Get-CoberturaClassLineSummary` (`Helpers.ps1:162-260`) exists because Cobertura repeats every source
line under `<methods>/<method>/<lines>` and again in the class-level rollup (issue #441). A
descendant-axis count double-counts every line. Use the helper. It exposes `LineMap`, `TotalLines`,
`CoveredLines`, `TotalBranches`, `CoveredBranches`; `LineMap[n].Hits` is the per-line figure for a
changed-line intersection. `Merge-CoberturaClassesByFilename` already collapses async state machines
to one entry per file in a post-processed document.

Package `name` and the six root attributes (`line-rate`, `branch-rate`, `lines-covered`,
`lines-valid`, `branches-covered`, `branches-valid`) are confirmed to exist post-processing
(`Helpers.ps1:418`, `:442-447`). Per-package counter attributes are NOT confirmed - derive them.

The first-party allowlist is computed at run time from every non-test `*.csproj`/`*.vbproj`/`*.fsproj`
(`Helpers.ps1:4-48`). In this tree it is exactly:
`QuickFiler,SVGControl,Tags,TaskMaster,TaskTree,TaskVisualization,ToDoModel,UtilitiesCS,VBFunctions`.
Assert SUBSET plus a positive control, not exact equality: a package appears only if its assembly
was loaded and instrumented.

## `QfcItemController.FolderHandlingTests.cs` cannot hold a new test

It ends at line 498 (2 lines of headroom) and its class declaration at `:19` is NOT `partial`. The
research's §6.4 instruction to add adoption tests to that file is unsatisfiable. New tests go in
`QfcItemController.FolderHandlingTests.Part2.cs`, `partial` is added at `:19` with no second
`[TestClass]` on the new part (precedent: `QfcItemController.InitializationTests.cs:30`), and
`QuickFiler.Test/QuickFiler.Test.csproj` needs a `<Compile Include>` entry - both projects use
explicit item lists. See [[project_legacy_csproj_explicit_compile_include]].

Production headroom in the same cycle: `QfcItemController.ViewerSetup.cs` ends at 499,
`QfcItemController.Initialization.cs` at 489, `QfcCollectionController.cs` at 2446 (and is not
`partial`; the declaration is at `:22`), `QfcQueue.cs` at 610.

## The tool manifest is `dotnet-tools.json` at the repository root

There is no `.config/dotnet-tools.json` in this tree. A Phase 0 task that reads the pinned CSharpier
version from `.config/dotnet-tools.json` asserts against a path that does not exist.

## `format .` versus AC-scope confinement

`dotnet tool run csharpier format .` is repo-wide, so on a tree with pre-existing drift it rewrites
files outside the issue's allowed prefixes and breaks a scope-confinement AC. Pair the mandated
command with a `git status --porcelain` before/after observation and a restoration clause for any
rewritten path outside scope, then let the `check .` gate report a residual subset as
`REMEDIATION-REQUIRED` rather than editing out-of-scope files. See
[[csharpier-repowide-format-breaks-zero-diff-acs]].

## Widening `scoreLoader` detonates a fail-closed reflection helper across three files

`QfcStreamingDequeueConfidenceGateTests.CreateGate` (`:26-83`) looks the gate constructor up by an
EXACT type array that repeats the delegate shape at `:54`, then asserts
`constructor.Should().NotBeNull("the gate must expose the nine-parameter testable constructor seam")`
at `:65-67`. Its own comment at `:43-47` records that it was deliberately made to fail CLOSED after a
descending-fallback version failed open (issue #446). So widening the `scoreLoader` delegate without
updating `:54` fails EVERY test in the partial class, not just one.

The blast radius is three files, because Part2 (9 sites) and Part3 (6 sites) call the first
`CreateGate` overload with inline two-value lambdas such as `(mail, token) => Task.FromResult((950L, ""))`
(main file has 16 more). Grepping for `scoreLoader` or `Task<(long` finds NOTHING in Part2/Part3 —
the lambdas are untyped at the call site — so a census built from those tokens silently omits both
files.

## `QfcQueue.cs:405` is not a relocatable unit

The `new QfcItemController(` at `:405` sits inside a `SelectAwait` lambda inside
`LoadControllersViewersAsync`, declared at `:380` with its six parameters one per line. A plan delta
that says to move "the construction block at `:405`" into a new partial part is unsatisfiable; the
movable member is the enclosing method at `:380`. Same trap for a constructor parameter: in
`QfcItemController.Initialization.cs` (489 lines) the `predeterminedFolder` ctor at `:86-96` gains a
parameter that cannot move alone — the whole ctor plus its doc block at `:78-85` is the unit.

## Near-cap test files that a one-argument edit can breach

`QfcCollectionControllerTests.cs` 499, `QfcHomeControllerIterationTests.cs` 497,
`QfcStreamingDequeueConfidenceGateTests.cs` 468 and `QfcFormControllerTests.cs` 827 (already over,
so it must not grow at all). None of the first, second or fourth is `partial` (declarations at
`:24`, `:26`, `:20`). CSharpier rewraps a call that crosses the print width, so adding one argument
can add four lines. Budget relocation of whole `[TestMethod]` members up front.

## `TestResults/` IS gitignored — but only a bracket-class grep proves it

`.gitignore:39` is `[Tt]est[Rr]esult*/`. A regex search for `[Tt]est[Rr]esult` does NOT match that
literal line (the char class consumes the wrong characters) and returns only `TestResult.xml` at
`:44`, which would wrongly suggest the D7 TRX directories break the unscoped porcelain gates in
P2-T11/P2-T15. Search for `esult` instead. See [[gitignore-bracket-classes-defeat-literal-grep]].

## Research-versus-issue conflicts resolved in the plan

The issue body cites `QfcHomeControllerRunAsyncHighConfidenceTests.cs:246`/`:277` as needing rewrite;
both are inside high-confidence-DISABLED tests (declared `:217` and `:257`) and must be preserved.
The enabled-mode rewrites are `QfcHomeControllerIssue218Tests.cs:178`/`:256` and
`QfcHomeControllerRunAsyncHighConfidenceTests.cs:192-201`. A separate `Times.Never` at `:207` sits
inside the enabled test and pins the UNFILTERED batch - it stays valid after the change and must not
be swept up in the rewrite.
