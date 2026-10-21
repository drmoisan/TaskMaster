# Feature Audit — issue #670 (`bug/qfc-initializewebviewasync-fault-is-unobserved-670`)

- **Timestamp:** 2026-09-01T20-41
- **HEAD:** `bb4dbaade9c9a90c0e1e5c61ea78041aa0c1892f`
- **Base (merge base with `origin/main`):** `988d35a8f8eb7436cc46a9f6424db917ed93807a`
- **Work mode:** `full-bug` (marker at `issue.md:12`)
- **AC source:** `spec.md` **only** — not `issue.md`; no `user-story.md` exists for this feature
- **Verdict:** PASS — 14 of 14 acceptance criteria verified; 0 unchecked; 0 blocking findings

## Method

Each criterion was verified against the working tree and the committed evidence, not against the
executor's summary claims. Where a criterion states a specific measurement, the measurement was
re-taken in this session. Where a criterion depends on a test outcome, the outcome was confirmed
against the committed TRX and the full-suite run record. Coverage figures were re-parsed from the
committed Cobertura documents by this reviewer.

All 14 criteria were already checked `[x]` by the executor on arrival. The task was to confirm each
check-off is earned. **All 14 are earned. No criterion was unchecked and `spec.md` was not
modified.**

## Criterion-by-Criterion Verification

### AC1 — New partial file, declarations, no `#nullable`, one csproj entry — **PASS**

- File exists at `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` (41 lines).
- Declares `namespace QuickFiler.Controllers` (line 4) and `internal partial class QfcItemController`
  (line 6).
- `grep -n "#nullable"` returns **exit 1, zero matches** — no directive present, as required.
- `grep -c "QfcItemController.WebViewFaultBoundary.cs" QuickFiler/QuickFiler.csproj` returns **1** —
  exactly one `<Compile Include>` entry, inserted after the `ViewerSetup.cs` entry at `:333`.
- `git diff --stat` on `QuickFiler.Test/QuickFiler.Test.csproj` returns **empty** — unchanged, as the
  criterion requires.
- Build succeeds (AC10 stages 2 and 3, exit 0).

### AC2 — Sink declaration and default log4net delegate — **PASS**

Lines 13-17 declare
`internal System.Action<string, System.Exception> WebViewInitializationErrorSink { get; set; }` with
default `(message, exception) => logger.Error(message, exception)`. The CSharpier-formatted layout
splits the generic argument list across lines but the declaration is exactly as specified.

The message-first overload is confirmed structurally: `logger` is
`private static readonly log4net.ILog` at `QfcItemController.cs:30`, and the exception-first form
does not exist on `log4net.ILog`. Both msbuild stages compile the file with zero coded diagnostics,
which is the criterion's own stated proof.

### AC3 — Guard structure — **PASS**

Lines 24-39 declare `internal async Task InitializeWebViewGuardedAsync()` which:

- awaits `InitializeWebViewAsync()` inside a `try` (lines 26-29);
- catches `OperationCanceledException` without invoking the sink (lines 30-34), with a comment
  recording that cooperative teardown cancellation is expected;
- catches `Exception ex` and invokes
  `WebViewInitializationErrorSink("WebView2 initialization failed.", ex)` (lines 35-38);
- does not rethrow — no `throw` statement appears anywhere in the file.

Both arms are confirmed executed by the coverage data: line 30 has `hits=1` and lines 35-37 have
`hits=1`.

### AC4 — Core fault test exists and passes — **PASS**

`InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault` is present
in `Part3.cs`. It asserts `NotThrowAsync` on the awaited guard and
`captured.Should().BeOfType<WebViewSentinelException>()`. Reported passed in the full-suite run
(`evidence/qa-gates/p4-t9-failure-set.md`) and in the isolated run
(`evidence/regression-testing/p3-t10-new-tests.trx`).

Assertion strength is independently established: under the P3-T5 mutation the `NotThrowAsync`
assertion still passes and only the sink assertion fails, so both halves of the criterion are
genuinely load-bearing rather than one masking the other.

### AC5 — Default-delegate test exists and passes — **PASS**

`WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing` is present in `Part3.cs`. It
constructs a bare `HarnessController` and makes **no sink assignment**, so the default log4net-backed
lambda is the code under test rather than a double — which is precisely what the criterion demands.
Reported passed.

### AC6 — Pump-hosted site-192 test exists and passes — **PASS**

`InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink` is present in `Part3.cs`,
carries `[Timeout(PumpTimeoutMs)]`, drives `Initialize(async: false)` through `WinFormsPumpHost`, and
asserts the sink received a `WebViewSentinelException`. The sink is installed during Arrange, before
dispatch, as the spec's risk mitigation requires. Reported passed.

### AC7 — Three call sites name the guarded member; no `.Unwrap()`, `ContinueWith`, or `await` — **PASS**

`grep -n` for both member names in `QfcItemController.Initialization.cs` returns:

| Line | Text | Classification |
| --- | --- | --- |
| 165 | `// InitializeWebViewAsync through the viewer's WPF dispatcher; both require a live message` | comment |
| 192 | `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewGuardedAsync);` | **guarded call site** |
| 193 | `//Task.Run(() => InitializeWebViewAsync());` | comment |
| 200 | `` // `await InitializeWebViewAsync()` is not completable in a unit test (the CoreWebView2 `` | comment |
| 256 | `await InitializeWebViewAsync();` | **unguarded, deliberate (AC8)** |
| 288 | `_ = InitializeWebViewGuardedAsync();` | **guarded call site** |
| 324 | `_ = InitializeWebViewGuardedAsync();` | **guarded call site** |
| 345 | `//    _ = InitializeWebViewAsync();` | comment |

Exactly three executable call sites name the guarded member, at lines 192, 288 and 324 as specified.
None introduces `.Unwrap()`, `ContinueWith`, or `await` — verified from the diff, which shows each as
a single-token substitution on an otherwise identical line (`+3/-3` total).

### AC8 — Line 256 unguarded and `ViewerSetup.cs` unmodified — **PASS**

- Line 256 reads `await InitializeWebViewAsync();`, calling the unguarded member. Confirmed by direct
  read.
- `git diff --stat 988d35a8...HEAD -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
  returns **empty output** — zero changed lines.
- `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` remains at `:47` and
  `internal async Task InitializeWebViewAsync()` at `:48`, both confirmed by direct read.
- The file is 499 lines, unchanged from baseline.

This is a deliberate design decision, not an omission, and the criterion is satisfied in both its
halves.

### AC9 — Three pinned pre-existing tests pass with unchanged bodies — **PASS**

All three report passed in the **full-suite** run of 6938 tests
(`evidence/qa-gates/p4-t9-failure-set.md`), which is a stronger observation than an isolated run
because it demonstrates they pass alongside the whole suite under the runsettings' parallelism.

The "bodies unchanged" half is confirmed structurally rather than by inspection alone: both changed
test files are **pure additions** — `Part3.cs` is `+100/-0` and `InitializationTests.cs` is `+52/-0`
in `git diff --numstat`. A modified method body would necessarily have produced deleted lines. Zero
deletions across both files proves no existing test body or assertion was altered.

`InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` is the substantive pin: it
asserts `InitializeAsync` **throws**. Had line 256 been routed through the guard, the fault would
have been contained and this test would have failed. Its passing is behavioural confirmation that the
fix was not applied over-broadly, corroborating AC8 independently of the source check.

### AC10 — Clean four-stage toolchain pass in order — **PASS** (with disclosed substitution)

Stages 1-4 ran in order as tasks P4-T1 through P4-T5 with no failure and no restart. Exit code 0 at
every stage. `/t:Rebuild` used in both msbuild stages; `/p:Nullable=enable` absent from both, as the
criterion explicitly requires.

I independently re-ran the read-only formatting verification:
`dotnet tool run csharpier check .` → `Checked 1567 files in 4686ms`, exit 0, matching the recorded
file count exactly.

Non-vacuity: the analyzer stage records 75 `CoreCompile:` executions and the nullable stage 67, so
compilation genuinely occurred in both and a diagnostic would have surfaced. The formatting gate is
demonstrably capable of failing — the same command exited 1 earlier in the run against the
unformatted new file.

Stage 4 substituted `Invoke-MSTestWithCoverage.ps1` for the literal
`vstest.console.exe ... /EnableCodeCoverage`. The runner wraps `dotnet-coverage collect` around the
same vstest binary over the same nine assemblies, and the substitution is required to obtain the
Cobertura document that AC13 and AC14 depend on. This is disclosed in the evidence record with its
rationale and is recorded as finding **PA-1** (non-blocking) in the policy audit. The substance of
AC10 — one uninterrupted, ordered, clean pass — is satisfied.

### AC11 — Every touched file at or below 500 lines — **PASS**

Measured with `awk 'END{print NR}'`:

| File | Lines | Verdict |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` | 41 | PASS |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` | 489 | PASS |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 498 | PASS |

The criterion enumerates these three files. The delivery also touched a fourth,
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs`, which the criterion does not
name. It measures **261 lines** and satisfies the ceiling as well, so the criterion's substantive
requirement holds across the actual touched set, not only the enumerated subset. The enumeration gap
is recorded as finding **PA-2** (non-blocking) in the policy audit.

`Part3.cs` at 498 of 500 leaves two lines of headroom; noted as CR-3 in the code review for future
maintainers. It is not a violation.

### AC12 — No determinism-banned API in the new tests — **PASS**

A sweep of both changed test files and the new production file for `Thread.Sleep`, `Task.Delay`,
`SpinWait`, `SpinUntil`, `DateTime.Now`, `DateTime.UtcNow`, `Stopwatch`, `.Wait()`, `.Result` and
`WaitOne` returned **zero hits**.

The pump-hosted test's only wait is `await observed.Task` on a
`TaskCompletionSource<Exception>` completed from the sink callback, exactly as the criterion
specifies. `WinFormsPumpHost` was separately swept for `Thread.Sleep`, `Task.Delay`, `SpinWait`,
`SpinUntil`, `DoEvents` and `while (true)`: zero hits, so no polling is introduced indirectly through
the harness either. No new test starts a live Outlook worker, a real WebView2 runtime, or any
external process; `HarnessController` exists specifically to exercise these members without live
WinForms/Outlook infrastructure.

### AC13 — New file reaches ≥ 90% line coverage — **PASS**

Parsed from `evidence/qa-gates/postchange.cobertura.xml`: the class row for
`QuickFiler\Controllers\QfcItemController.WebViewFaultBoundary.cs` carries `line-rate=0.923077`, and
a count of its class-level `<line>` nodes gives **12 covered of 13 = 92.3077%**, clearing the 90%
new-module rule. Branch coverage on the file is 100% (`branch-rate=1`).

The single uncovered line is **line 29**, the closing brace of the `try` block, reachable only when
`InitializeWebViewAsync()` returns successfully. The spec states in advance (Risks; Test Strategy
§Edge cases) that the success path requires a live CoreWebView2 runtime and is not coverable in a
unit test. This is a disclosed, anticipated limitation rather than untested behaviour, and it was
stated plainly rather than papered over — which the spec explicitly committed to doing.

The file is absent from the baseline Cobertura document (0 matching class rows), confirming it is
genuinely new code subject to the new-module rule.

### AC14 — Repository-wide line coverage does not regress — **PASS**

Re-parsed independently from both committed documents:

| Measure | Baseline | Post-change | Delta |
| --- | --- | --- | --- |
| `lines-covered` | 54983 | 54988 | **+5** |
| `lines-valid` | 64393 | 64406 | +13 |
| Line percentage | 85.3866% | 85.3771% | −0.0095 pp |
| `branches-covered` / `branches-valid` | 13115 / 16516 (79.4078%) | 13120 / 16524 (79.3997%) | −0.0081 pp |
| `POSTPROCESSED` | yes | yes | comparable |

Absolute covered lines **rose by 5**. The ratio moved down by 0.0095 percentage points purely because
the denominator grew by 13 — the new file's measurable lines, 12 of which are covered. No previously
covered line became uncovered. Both documents are in the same post-processing state, so their
denominators are directly comparable and no normalization is required.

The three changed lines in `Initialization.cs` sit in members that carry no coverage attribute and are
executed by existing tests before and after, so there is no changed-line regression.

**Storage-location note.** The criterion text says both artifacts are stored under
`evidence/coverage/`. They are not: they are at `evidence/baseline/baseline.cobertura.xml` and
`evidence/qa-gates/postchange.cobertura.xml`. This is **correct**, not a shortfall. `coverage` is not
a canonical evidence kind — I verified the canonical list directly at
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md:15-20` — and the executor recorded a
properly formed `EVIDENCE_LOCATION_OVERRIDE_REJECTED` entry at
`evidence/other/p4-t26-ac14-path-override.md`. `Test-Path` confirms no `evidence/coverage/` directory
was created. The criterion text was correctly left unedited per `acceptance-criteria-tracking` rule 3.
AC14's substantive requirement — no regression against the pre-edit baseline — is met.

## Baseline Comparison Summary

The defect described in `spec.md` §Actual is resolved. Of the four production call sites:

| Call site | Before | After | Fault observed? |
| --- | --- | --- | --- |
| `Initialization.cs:192` | `InvokeAsync(InitializeWebViewAsync)` | `InvokeAsync(InitializeWebViewGuardedAsync)` | yes — routed to sink |
| `Initialization.cs:256` | `await InitializeWebViewAsync()` | unchanged | yes — propagates to caller (AC8) |
| `Initialization.cs:288` | `_ = InitializeWebViewAsync()` | `_ = InitializeWebViewGuardedAsync()` | yes — routed to sink |
| `Initialization.cs:324` | `_ = InitializeWebViewAsync()` | `_ = InitializeWebViewGuardedAsync()` | yes — routed to sink |

All four paths now observe a fault. Fire-and-forget latency is preserved on the three that had it, and
the already-observed path retains its existing propagation semantics.

Out-of-scope items were correctly left alone and are documented in `spec.md` §Scope & Non-Goals:
`EfcItemController.cs:97`/`:153`, the `TaskScheduler.UnobservedTaskException` backstop, and the
`void Initialize(bool)` → `async Task` conversion. None appears in the branch diff.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md
- Total AC items: 14
- Checked off (delivered): 14
- Remaining (unchecked): 0
- Items remaining: none
```

No criterion was unchecked by this review. `spec.md` was not modified: all 14 items arrived checked
and all 14 were verified as earned, so no `- [x]` → `- [ ]` transition was warranted.

## Verdict

**PASS. 0 blocking findings.** Four non-blocking observations (CR-1 through CR-4) are recorded in
`code-review.2026-09-01T20-41.md` and two (PA-1, PA-2) in `policy-audit.2026-09-01T20-41.md`. None
requires remediation before merge, and no `remediation-inputs` artifact is produced.
