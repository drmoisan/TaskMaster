# Orchestrator citation correction to research.2026-08-31T20-30.md

Timestamp: 2026-08-31T20-45
Author: orchestrator (preparation mode, issue #646)
Corrects: `research.2026-08-31T20-30.md`

## Why this correction exists

The base branch moved while the research delegation was in flight.

- At preparation start the item branch `bug/qfc-metrics-flush-writes-empty-session-file-646` was cut
  from `origin/main` at `9b6aff2e886eb86af5dfc131ebee7a2ebe1a5b6c`.
- Issue #647 (`bug/fileio2-write-retry-reports-success-on-final-failure-647`) was then merged to
  `main`, advancing `origin/main` to `2b85134b42872e405602e6064e02dc9cda6c319b`.
- The item branch was fast-forwarded `9b6aff2e..2b85134b` after the research agent had already begun
  reading the tree.

The research artifact therefore describes the **pre-merge** tree. Its structural conclusions remain
correct; its delegate signature, its writer-call text, and several of its line numbers do not. The
sibling change this item was told to defend against has already landed, so it is now present fact
rather than anticipated future state.

The citations below were re-derived by the orchestrator directly against
`2b85134b42872e405602e6064e02dc9cda6c319b` and supersede the corresponding claims in the research
artifact.

## Superseded citations

| Claim in research artifact | Status | Corrected fact at `2b85134b` |
|---|---|---|
| `MetricsFileWriter` is `Func<string, string[], string, CancellationToken, Task>` | Superseded | It is `Func<string, string[], string, CancellationToken, Task<bool>>`, declared at `QuickFiler/Controllers/QfcHomeController.Metrics.cs:28-34`. |
| The writer call is the single-line `await MetricsFileWriter(filename, lines, myDocuments, CancellationToken.None);` at line 179 | Superseded | The call is the multi-line assignment `bool metricsWritten = await MetricsFileWriter(` beginning at line 179 and closing at line 184, followed by the `if (!metricsWritten)` logging branch at lines 185-191. |
| `WriteMetricsAsync` body spans lines 107-180 | Superseded | It spans lines 107-192. |
| Test lambdas return `Task.CompletedTask` | Superseded | Every capturing lambda in the test file now returns `Task.FromResult(true)`, and the `async` one at line 359-364 returns `true`. |
| `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` at line 403; `WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter` at lines 430-449 | Superseded | They are at lines 404 and 432-450 respectively. |
| The #647 change is pending and will invalidate anchors later | Superseded | The #647 change is already merged and present. No further sibling rewrite of this call site is pending. |

## Confirmed unchanged by the merge

These research claims were re-checked and still hold at `2b85134b`:

- The default binding is `FileIO2.WriteTextFileAsync`, and it opens the target in append mode before
  its write loop, so an empty array still creates or touches the file. The root mechanism is intact.
- `WriteMetricsAsync` contains exactly one pre-existing early `return;`, at lines 131-134, in the
  `!Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var myDocuments)` guard. A second early
  return is structurally consistent with the method as written.
- The EFC precedent guard is `if (dataLines.Length == 0) { return; }` in
  `QuickFiler/Controllers/EfcHomeController.Metrics.cs`, inside `QuickFileMetrics_WRITE`.
- `BuildLooseMetricsController` remains the shared fixture and the correct construction path.
- `WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter` remains the best template for the
  new test: it is the only existing test whose assertion shape is a zero-invocation `bool` flag.
- Only the one production file and the one test file reference `MetricsFileWriter` in live code, so
  the guard cannot break an existing test. Every existing test that asserts invocation supplies
  non-empty diagnostics.

## Anchors for the implementation edit

Anchor on structure, not on line numbers. Both anchors are inside `WriteMetricsAsync`.

- **Anchor A (insert after):** the statement that computes the filtered array,
  `var lines = strOutput.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();` (line 174).
- **Anchor B (insert before):** the start of the writer invocation statement,
  `bool metricsWritten = await MetricsFileWriter(` (line 179), together with the explanatory comment
  block immediately preceding it at lines 176-178.

The guard belongs between Anchor A and that comment block.

## Scope boundary that still applies

The delegate signature and the `if (!metricsWritten)` logging branch are the delivered outcome of
issue #647. This item must not alter either. Its entire production change is the early-return guard.

## Standing instruction for the executor

Even though the sibling has landed, reconcile the branch against the current `origin/main` tip at
execution start and re-derive both anchors before editing. This preparation run has already been
invalidated once by a mid-run merge to the same method; the line numbers recorded above are accurate
as of `2b85134b` and carry no guarantee beyond it.

## Additional constraint discovered during correction

`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` is 454 lines. The General Code Change
Policy caps test files at 500 lines, leaving 46 lines of headroom. The new test must fit within that
headroom, or the file must be split. The plan must verify the post-change line count rather than
assume it.
