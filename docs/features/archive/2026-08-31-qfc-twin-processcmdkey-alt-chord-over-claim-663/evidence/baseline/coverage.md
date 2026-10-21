# Phase 0 — Baseline coverage ([P0-T13])

Timestamp: 2026-09-01T22-29

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/663-baseline.cobertura.xml`

EXIT_CODE: 0

The exit code is recorded but is not the gate, for the reason stated in the plan's reading guide: the
wrapper throws at line 236 when the inner run reports a non-zero exit, and the gate is failure-set
membership rather than exit status. The value 0 is derived from two observed facts rather than assumed.
First, the run's standard error stream is 0 bytes; the earlier aborted attempt described below wrote its
`throw` text to standard error, so a throw is observable there and none occurred. Second, the wrapper ran
past its throw site to completion, printing its terminal `Done. Coverage artifact: ...` line.

## Two runs, both recorded

**Attempt 1 — aborted, hung.** The first invocation of the identical command stalled inside
`QuickFiler.Test`. Two `WinFormsPumpHost` harness tests,
`BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` and
`BuildPumpHarness_DoesNotCreateTheWebViewChildHandles`, each failed by 60000 ms timeout, after which the
transcript stopped growing at 101,859 bytes and never advanced past the `QuickFiler.Test` parallelization
banner. Hang was distinguished from slowness by measurement rather than by assumption: the `testhost`
process CPU counter moved from 26.52 to 26.53 across a 60-second sample while the transcript byte count
stayed identical. Only this run's own process chain was terminated — `dotnet-coverage` PID 79344,
`testhost` PID 9652 and `vstest.console` PID 132968, all with a start time matching this run. Two
`vstest.console` processes over 24 hours old and one `dotnet` process of the same age were present in the
process table and were deliberately **not** touched: they belong to another session and are not part of
this chain. The aborted attempt reported `MSTest with coverage failed with exit code -1`.

**Attempt 2 — the recorded run.** The identical command was re-run once, with no intervening change to
any file. It completed, discovering the same 9 assemblies and reporting 6927 of 6927 passed. This is the
load-flaky class described for pump-host and dispatcher tests: the failures were wall-clock timeouts
rather than assertions, and the same tests pass on the retry. The retry is not a toolchain-loop restart,
because no file changed between the two invocations.

## Runner summary of the recorded run, verbatim

```
Test Run Successful.
Total tests: 6927
     Passed: 6927
 Total time: 28.7813 Seconds
```

```
Code coverage results: <repo-root>\coverage\663-baseline.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
Done. Coverage artifact: <repo-root>\coverage\663-baseline.cobertura.xml
```

## BASELINE_COVERAGE_FAILURE_SET

Verbatim list of every failing test name this instrumented run reports:

```
(empty)
```

**BASELINE_COVERAGE_FAILURE_SET = { } (the empty set).**

This set is captured separately from the `[P0-T12]` BASELINE_FAILURE_SET because instrumentation adds
load-driven failures, so the two sets are not interchangeable. On this pair of runs both happen to be
empty, but they remain separately sourced: `[P4-T6]` is compared against this set and `[P2-T3]`,
`[P3-T3]` and `[P4-T5]` are compared against the `[P0-T12]` set.

## Out-of-band post-processing

Command:

```
pwsh -NoProfile -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; $raw = Get-Content -LiteralPath "coverage/663-baseline.cobertura.xml" -Raw -Encoding UTF8; $p = ConvertTo-KoverageCoberturaXml -XmlContent $raw -RepoRoot (Get-Location).Path; Set-Content -LiteralPath "coverage/663-baseline.processed.cobertura.xml" -Value $p -Encoding UTF8 -NoNewline'
```

EXIT_CODE: 0

Because this run had no failing test, the wrapper reached its own Koverage post-processing block at lines
333 through 344 and had already post-processed the document in place. The out-of-band application was
performed anyway, as the plan requires, and is safe: `ConvertTo-KoverageCoberturaXml` is idempotent with
respect to the `<sources>` injection it performs, guarded at line 430. The output is byte-identical in
size to its input, 10,790,242 bytes, which corroborates the idempotence.

## Root-level figures, read from the post-processed document

| Attribute | Value |
|---|---|
| `line-rate` | **0.853866** |
| `branch-rate` | **0.794064** |
| `lines-covered` | **54977** |
| `lines-valid` | **64386** |
| `branches-covered` | **13110** |
| `branches-valid` | **16510** |

## BASELINE_CLASS_LINE_RATE

The post-processed document **does** contain a `class` element whose `filename` attribute ends with
`QfcFormKeyHandler.cs`. Exactly one such element exists.

- `name` attribute, verbatim: `QuickFiler.Controllers.QfcFormKeyHandler`
- `filename` attribute, verbatim: `QuickFiler\Controllers\QfcFormKeyHandler.cs`
- `line-rate` attribute: **1**
- `branch-rate` attribute: `1`

**BASELINE_CLASS_LINE_RATE = 1**

**Document kind BASELINE_CLASS_LINE_RATE was taken from: the POST-PROCESSED document**
`coverage/663-baseline.processed.cobertura.xml`. The `[P0-T13]` fallback to the raw pre-processing
document did not apply, because the class element survived the pruning. `[P4-T7]` must therefore take its
comparison figure from the post-processed document as well, so the two figures are commensurable.

## Required observation — no `ClaimsAltChord` method element under `QfcFormKeyHandler.cs`

The `class` element whose `filename` ends with `QfcFormKeyHandler.cs` contains exactly **one** `method`
element in the post-processed document:

```
method name="IsAltKeyCommand" line-rate="1"
```

There is **no** `method` element named `ClaimsAltChord` under it. That is the expected pre-change state:
the member does not exist at branch head.

## Measured reading — `ClaimsAltChord` under a class whose `filename` ends with `EfcViewer.cs`

Measured, not predicted. The post-processed document contains **zero** `class` elements whose `filename`
attribute ends with `EfcViewer.cs`. There is therefore no `method` element named `ClaimsAltChord` under
such a class: the reading is **absent**.

The plan states that either answer is admissible and that neither fails this task, because two mechanisms
pull in opposite directions. The absent reading is uninformative on the pruning question, because the
class-level `[ExcludeFromCodeCoverage]` attribute at QuickFiler/Viewers/EfcViewer.cs line 20 produces the
same result as pruning would. The pruning question is nevertheless settled independently and in the
affirmative by the clause above: the `QuickFiler` `<package>` survived, since the `<class>` element whose
`filename` ends with `QfcFormKeyHandler.cs` is present in the post-processed document. Consistent with
that, the observed absence is attributable to the attribute-based exclusion rather than to package
pruning.

## Artifact disposition

Both coverage documents stay under the gitignored `coverage` directory and are not committed:

- `coverage/663-baseline.cobertura.xml` — 10,790,242 bytes
- `coverage/663-baseline.processed.cobertura.xml` — 10,790,242 bytes

Every absolute worktree path in this artifact is rendered as `<repo-root>`.

Output Summary: The instrumented repository-wide baseline run hung on its first attempt inside
`QuickFiler.Test` after two 60-second pump-host timeouts, was diagnosed as hung by a flat testhost CPU
sample, had only its own process chain terminated, and completed on one identical retry with 6927 of 6927
tests passed. BASELINE_COVERAGE_FAILURE_SET is the empty set. Read from the post-processed Cobertura
document: root `line-rate` 0.853866, `branch-rate` 0.794064, `lines-covered` 54977, `lines-valid` 64386,
`branches-covered` 13110, `branches-valid` 16510. The class `QuickFiler.Controllers.QfcFormKeyHandler`
is present with `line-rate` 1, so BASELINE_CLASS_LINE_RATE is 1, taken from the post-processed document.
That class carries one method element, `IsAltKeyCommand`, and no `ClaimsAltChord`. No class whose
`filename` ends with `EfcViewer.cs` exists in the document, so the `EfcViewer` `ClaimsAltChord` reading is
absent.
