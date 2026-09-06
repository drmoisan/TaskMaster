---
name: 781-review-residuals
description: "#781 breadcrumb guard review: PASS/0 blocking; ItemViewer is [ExcludeFromCodeCoverage] so ItemViewer.Breadcrumb.cs emits ZERO Cobertura class elements; the executor's coverage-delta blamed deleted QuickFiler tests when all movement was in untouched UtilitiesCS classes"
metadata:
  type: project
---

Review of `bug/breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781`
(base `main` @ `a007f72e`, head `4f74aa39`), work mode `minor-audit`. Outcome: PASS, 0 blocking,
8/8 AC, no remediation-inputs.

## Durable facts about this file family

`QuickFiler/Viewers/ItemViewer.cs` line 20 carries `[ExcludeFromCodeCoverage]` on the
`ItemViewer` **partial class declaration**. The attribute applies to the whole type, so
`ItemViewer.Breadcrumb.cs`, `ItemViewer.Designer.cs`, and every other part emit **zero**
`<class>` elements in Cobertura. Any AC demanding a changed-line coverage percentage on those
files is unevaluable by construction, before and after the change. `ItemViewerExpanded` is a
different type and is NOT excluded (it does appear in the Cobertura). Verify with an enumeration
of `<class filename=...>` over the document, not by inference.

The ratified basis is CLAUDE.md UT2(b) (WinForms form-derived classes). `ItemViewer` is a
`UserControl`. Do not treat this as a Coverage-Exclusion-Policy violation introduced by a branch;
check whether the query also returns 0 on the **baseline** document before scoring it.

## The finding that only a class-by-class Cobertura diff exposes

The executor's `coverage-delta` artifact explained a -2 `lines-covered` movement as "consistent
with the deletion of the two obsolete D4 tests in `QuickFiler.Test/...`". Diffing the two
Cobertura documents class by class (564 classes each) showed **zero** `QuickFiler` classes
differed — the `QuickFiler` package counters were identical at LINE missed=2376 covered=9960 —
and all three differing classes were in untouched `UtilitiesCS`:
`SegmentStopWatch` (1.0 -> 0.944954), `SubjectMapSco` (0.969466 -> 0.938931),
`OlTableExtensions` (0.885522 -> 0.912458). The real cause is run-to-run nondeterminism in
unrelated code, matching [[csharp-coverage-constants-nondeterministic]].

**Why:** the explanation was also self-contradictory — the same artifact said the old throw path
was outside the denominator, so exercising it cannot move a counter. A plausible-sounding causal
sentence in a coverage artifact is not evidence.

**How to apply:** whenever an evidence artifact attributes a coverage delta to a named cause,
diff the two Cobertura documents by `package|class|filename -> line-rate` and confirm the
movement is actually in the package the claim names. See also
[[measure-every-changed-file-not-just-the-ac-named-one]].

## Second citation trap in the same artifact

The artifact stated test assemblies are excluded "by `coverage.config`". The committed
`coverage.config` contains only seven third-party `ModulePath` excludes (Deedle, FSharp,
Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest) and **no** test-assembly entry.
The `.*\.Test\.dll$` exclusion is injected at run time by
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 99, and
`Invoke-MSTestWithCoverage.Helpers.ps1` lines 24-46 additionally omit `.Test`-suffixed
assemblies from the allowlist so `ConvertTo-KoverageCoberturaXml` strips them from numerator and
denominator. The effect is real; the citation names the wrong file.

## Residuals owed at merge (none blocking)

1. Correct both citations in `evidence/qa-gates/coverage-delta.2026-09-05T10-49.md`.
2. Promote `UtilitiesCS/Threading/UiThread.cs:100`
   (`SynchronizationContextAwaiter.IsCompleted` reference comparison -> `await
   viewer.UiSyncContext` always posts for dispatcher-built viewers) from `issue.md` prose to a
   real issue.
3. `4f74aa39` also deletes tracked `artifacts/orchestration/orchestrator-state.json` (stale #704
   state, contained an absolute host path); unmentioned in the commit subject.
4. `DrainableSynchronizationContext.Drain()` in the new test file is dead code.
5. `ThrowIfOffUiBoundary` `<remarks>` justifies via "managed thread ids are unique among live
   threads", but `Dispatcher.CheckAccess()` compares `Thread` **object references** — the guard is
   stronger than its documented rationale, and the wording invites a weaker id-based refactor.
   Note `BreadcrumbUiDispatcher.IsCurrentBoundary` documents the opposite position for its own
   (legitimately different) job.

Repo-wide C# figures this cycle: line 0.848316, branch 0.791421, `lines-valid` 64740, 9
assemblies — FAIL on the 85% floor, non-blocking (baseline 0.848347, pre-existing).
