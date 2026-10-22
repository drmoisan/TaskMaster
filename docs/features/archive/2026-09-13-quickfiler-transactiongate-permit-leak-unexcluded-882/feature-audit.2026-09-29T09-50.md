# Feature Audit — Issue #882 (QuickFiler `TransactionGate` bounded acquisition)

- Date: 2026-09-29 (artifact stamp `2026-09-29T09-50`, authoring stamp; no shell clock was available to this review)
- Work Mode: `full-bug` (marker `- Work Mode: full-bug` at `issue.md` line 6)
- Acceptance-criteria source: `spec.md` v1.1 **only**, section `## Acceptance Criteria`, criteria AC1 to AC12. `issue.md` carries superseded wording and is not an AC source in this mode. No `user-story.md` exists, which is correct for `full-bug` and is not a gap.
- Branch: `bug/quickfiler-transactiongate-permit-leak-unexcluded-882`
- Head: `865a473f9e3b0f6322859d0e8e3ccd776ffb40d3`
- Base: `177b6d78e1b2408e5aedbd794cef3aad6b7fb372`
- Verdict: **PASS** — 12 of 12 acceptance criteria PASS, 0 blocking findings

## Summary

All twelve acceptance criteria are delivered and were re-verified by the reviewer directly against the delivered worktree with Read, Grep and Glob (the Bash tool was not used, at the caller's direction). Where a criterion depends on a git-derived measurement, the executor's committed figure is used and is labelled `(executor git-derived)`; every figure derivable from file content was re-derived by the reviewer and matches. The executor had already checked every criterion `- [x]` in `spec.md`; the reviewer's evaluation agrees with all twelve, so no check-off edit was required and no criterion text was altered.

## Verification Method

- Both changed C# files were read in full.
- Every `BeginTransactionAsync` reference in `QuickFiler.Test` was enumerated (24 hits across 6 files).
- The feature folder was enumerated by Glob (44 files) and scanned for raw tool documents (`*.trx`, `*.xml`, `*.coverage`, `*.coveragexml`, `*.json`: none) and for host-identity tokens (account name, `<drive>:\Users`, `<drive>:/Users`, `/c/Users/`: 0 hits).
- The two committed JaCoCo projections were re-summed by the reviewer and match the committed summaries exactly.
- The branch head was read from the ref file (`865a473f9e3b…`) and matches the caller's value; the last reflog entry for that commit is `docs(882): check off P4-T26 in the plan`.

## Acceptance Criteria Inventory

| AC | Criterion (abridged) | State in `spec.md` at review start |
|---|---|---|
| AC1 | Acquisition bounded; parameterless `WaitAsync()` gone; boolean outcome branched on | `[x]` |
| AC2 | `TimeoutException` naming `TransactionGate`, the bound, the token; no construction or counter on failure | `[x]` |
| AC3 | Default 120000 ms; `TimeSpan` entry point; pre-check before wait; acquisitions counted only on success | `[x]` |
| AC4 | New test: `[Timeout(GateTimeoutMs)]`, no `DoNotParallelize`, no `Install`, zero-bound probe while holding, `TimeoutException` with token, no sleep/delay/retry/elapsed-time | `[x]` |
| AC5 | Counter difference == 1 while holding; own `Dispose` in `try` without `SemaphoreFullException`; `finally` disposal; production round trip | `[x]` |
| AC6 | Seven pre-existing tests unmodified and passing; no consuming file edited | `[x]` |
| AC7 | `.csproj` untouched; no file added/removed; anchored diff equals the declared write set; test file <= 500 lines | `[x]` |
| AC8 | Determinism criterion reproduced and both clauses shown satisfied | `[x]` |
| AC9 | Compile-level fail-before-exception dossier | `[x]` |
| AC10 | Full toolchain single pass; suite count = baseline + 1; Markdown projections only | `[x]` |
| AC11 | No shipped production file; write set inside `QuickFiler.Test/` and the feature folder | `[x]` |
| AC12 | No raw trx/coverage document; no host path, account or host name in committed text | `[x]` |

## Acceptance Criteria Evaluation

### AC1 — Acquisition is bounded — **PASS**

Fixture file, reviewer full read: `TransactionGate.WaitAsync()` (parameterless) occurs 0 times; the only acquisition is `bool acquired = await TransactionGate.WaitAsync(bound).ConfigureAwait(false);` at line 178, branched on at line 179 (`if (!acquired)`). The executor's `fixture-structure-gates.md` records the same counts (0 / 1 / 1 / 1).

### AC2 — Failure type, message and no side effects on failure — **PASS**

Lines 181-185 throw `new TimeoutException(...)` whose message begins with the literal `TRANSACTIONGATE_ACQUIRE_TIMEOUT`, names `UiThreadDispatcherFixture.TransactionGate`, and states the bound as `bound.TotalMilliseconds.ToString("0", CultureInfo.InvariantCulture)` followed by ` ms`. The throw at 181 precedes the acquisitions increment at 188 and the construction at 189; `_transactionReleases` is written only in `ReleaseTransactionGate` (line 112), which is unreachable on the failure path. `new UiThreadDispatcherTransaction()` occurs exactly once in the file (189). The new test's `TransactionAcquisitions − TransactionReleases == 1` assertion, evaluated after the failed probe, Passed in both runs, which is the runtime confirmation.

### AC3 — Bound value, `TimeSpan` entry point, counter placement — **PASS**

`internal const int TransactionGateAcquireTimeoutMs = 120000;` at line 146; `internal static async Task<UiThreadDispatcherTransaction> BeginTransactionAsync(TimeSpan bound)` at lines 169-171; the parameterless overload (155-160) delegates with `TimeSpan.FromMilliseconds(TransactionGateAcquireTimeoutMs)`. The contended pre-check is at 173-176, before the wait at 178. The acquisitions increment at 188 is reached only after the `if (!acquired)` block. Line order 175 < 178 < 181 < 188 < 189 is strictly increasing (reviewer-verified; matches `qa-post-format-audit.md`).

### AC4 — The regression test's shape and prohibitions — **PASS**

Test file lines 396-456, method `BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing` in the existing class `QfcItemController_UiThreadDispatcherFixtureTests`:

- `[TestMethod]` (405) and `[Timeout(GateTimeoutMs)]` (406), `GateTimeoutMs` being the class's existing `60000` constant at line 33. `[TestMethod]` and `[Timeout(GateTimeoutMs)]` each occur 8 times in the file (7 + 1).
- `DoNotParallelize`: 0 occurrences in the file.
- Acquires through `UiThreadDispatcherFixture.BeginTransactionAsync()` (410-412) and never calls `Install` (no `.Install(` between lines 407 and 456; the file's last `.Install(` is at 334).
- Probes `UiThreadDispatcherFixture.BeginTransactionAsync(TimeSpan.Zero)` (418-419) inside the `try`, before any disposal, so the test still holds the permit.
- `await probe.Should().ThrowAsync<TimeoutException>(...).WithMessage("*TRANSACTIONGATE_ACQUIRE_TIMEOUT*")` (422-427).
- `Thread.Sleep`, `Task.Delay`, `Stopwatch`, `[Retry`: 0 occurrences each; no elapsed-time assertion exists.

Both runs report the test `Passed` (`pass-after-scoped-run.md`, `mstest-test-result-summary.md`).

### AC5 — Counter assertion, own disposal, safety net, production round trip — **PASS**

- Lines 428-433: `(TransactionAcquisitions − TransactionReleases).Should().Be(1, ...)` while holding.
- Lines 440-445: `Action dispose = () => transaction.Dispose(); dispose.Should().NotThrow<SemaphoreFullException>(...)` inside the `try`.
- Lines 447-450: unconditional `transaction.Dispose()` in `finally` (idempotent by the `_disposed` guard, proven by R5).
- Lines 452-455: `roundTrip = await UiThreadDispatcherFixture.BeginTransactionAsync()` then `roundTrip.Dispose()`; the only `TimeSpan.Zero` in the file is the probe at 419.

`UiThreadDispatcherFixture.TransactionReleases` occurs twice in the file (the #743 test and this one) and `roundTrip.Dispose();` twice (R5 and this one), matching the executor's counts.

### AC6 — Pre-existing tests unmodified and passing; no consuming file edited — **PASS**

- Test-file numstat against the base: `62 0` (62 added, 0 deleted) (executor git-derived, `test-structure-gates.md`). Zero deleted lines and an insertion after line 394 mean the seven pre-existing methods are byte-unchanged; the reviewer confirms their content matches the spec's line citations (R4 at 204-264, R5 at 271-312, the #743 counter-balance test at 355-394).
- All seven Passed in the scoped run (8/8) and the full run (1469/1469).
- The anchored `git diff --name-status` at head lists exactly two paths under `QuickFiler.Test/` (executor git-derived, `qa-post-commit-verification.md`); `QfcFormControllerUndoHandoffTests.cs`, `QfcHomeControllerRunAsyncTests.cs` (all partials), `QfcItemController.InitializationTests.Part2.cs` and `WpfUiDispatcherTests.cs` are absent from the diff, and the reviewer's enumeration shows each still calls `BeginTransactionAsync()` unchanged.

### AC7 — Project file untouched, write set exact, test file under 500 lines — **PASS**

- `git diff --numstat 177b6d78e… -- "*.csproj"` is empty (executor git-derived, `qa-footprint-scope.md`); no `.csproj` appears in the anchored diff.
- The anchored diff at head lists 48 paths: the two C# files, `spec.md`, the plan, `issue.md`, the two research records, the two `.claude/agent-memory/orchestrator/` paths, and 40 evidence artifacts. Under the plan's recorded decomposition (P0-T9 `BASE-DIFF-PATHS` — the seven paths already on the branch before execution — plus the Write Set), every path is accounted for and nothing else is present. The two agent-memory paths were on the branch before the plan ran and are not deliverables; the reviewer scanned them and found no host-identity token. The literal phrase "exactly the write-set paths" is satisfied under that decomposition, which the plan (E6, P0-T9, P4-T9) defines.
- Test file: 458 lines (reviewer full read; `LINES-FT=458`), at most 500. Fixture file: 342.

### AC8 — Determinism criterion reproduced and satisfied — **PASS**

The plan's section "Determinism criterion (reproduced verbatim from spec.md, Determinism Ruling; AC8)" quotes the two-clause criterion verbatim and argues each clause: clause (i) — `WaitAsync(TimeSpan)` completes the instant the permit is available, and the test reaches the failure branch with `TimeSpan.Zero` while holding, which returns without blocking; clause (ii) — expiry of the 120000 ms bound is reported as a `TimeoutException` test failure and is never the means by which a test reaches its expected state. The corollary (the test never lets the bound elapse) holds by construction. The reviewer's independent reading of the delivered code agrees on both clauses.

### AC9 — Compile-level fail-before dossier — **PASS**

Exactly one file matches `evidence/regression-testing/fail-before-exception.*.md`: `fail-before-exception.2026-09-29T09-06.md`. It carries `WhyFailingRunImpossible:` (the overload did not exist on the pre-fix tree, so the test cannot compile, load or execute), `ExpectedExitCode: 1`, `EXIT_CODE: 1`, `Build FAILED.`, and the quoted diagnostic `error CS1501: No overload for method 'BeginTransactionAsync' takes 1 arguments` at `FixtureTests.cs(418,68)`, which the reviewer confirms is the probe line. The absence proof cites the P0-T15 baseline counts (`BeginTransactionAsync(TimeSpan bound)` = 0, `TransactionGate.WaitAsync()` = 1 on the base tree).

### AC10 — Full toolchain single pass, evidenced by projections only — **PASS**

| Step | Result | Artifact |
|---|---|---|
| `dotnet tool run csharpier check .` | exit 0, `Checked 1623 files in 6099ms.`, `DRIFT-FILES: NONE` | `qa-gates/qa-csharpier-check.md` |
| Analyzer rebuild | exit 0, `0 Warning(s)`, `0 Error(s)`, `DLL-FRESH=True` | `qa-gates/qa-analyzer-rebuild.md` |
| Nullable rebuild (`TreatWarningsAsErrors`) | exit 0, `0 Warning(s)`, `0 Error(s)`, `DLL-FRESH=True` | `qa-gates/qa-nullable-rebuild.md` |
| `QuickFiler.Test` under `TaskMaster.cli.runsettings` | exit 0, 1469 total / 1469 passed / 0 failed = baseline 1468 + 1 | `qa-gates/mstest-test-result-summary.md`, `qa-coverage-test-run.md` |

`qa-loop-closure.md`: `ITERATIONS: 1`, `LOOP: CLEAN PASS`. The committed evidence consists of the Markdown projections named in the spec's "Committed evidence" subsection (`mstest-test-result-summary.md`, `coverage-jacoco-projection.md`, `coverage-summary.md` under `qa-gates/`, with baseline counterparts); no raw document is committed (Glob confirms no `*.trx`/`*.xml`/`*.coverage` under the feature folder).

### AC11 — No shipped production file modified — **PASS**

Every non-feature-folder path in the anchored diff is either under `QuickFiler.Test/Controllers/` (the two C# files) or under `.claude/agent-memory/orchestrator/` (documentation, pre-existing on the branch, not add-in code). No path under `QuickFiler/`, `UtilitiesCS/`, `TaskMaster/` or any other production project appears.

### AC12 — No raw document; no host identity in committed text — **PASS**

- Raw documents: the anchored diff contains no path ending in `.trx`, `.coverage`, `.coveragexml`, `.cobertura.xml`, `.jacoco.xml` and none named `coverage.xml` (executor, `qa-footprint-scope.md`); the reviewer's Glob over the feature folder for XML/trx/coverage/JSON returns nothing.
- Host identity: the reviewer's Grep over the feature folder for the account name, `<drive>:\Users`, `<drive>:/Users` and `/c/Users/` returns 0 hits. The executor's corrected five-pattern scan (`qa-hygiene-scan.md`) reports 0 hits for account token, host name, worktree root, drive-letter shape and Git-Bash shape, with positive controls of 2941 / 2939 / 1472 / 2939 against the gitignored trx proving each pattern can hit. Committed commands use `<repo-root>` and the placeholders the spec lists.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/spec.md
- Total AC items: 12
- Checked off (delivered): 12
- Remaining (unchecked): 0
- Items remaining: none
```

| AC | Verdict | AC | Verdict |
|---|---|---|---|
| AC1 | PASS | AC7 | PASS |
| AC2 | PASS | AC8 | PASS |
| AC3 | PASS | AC9 | PASS |
| AC4 | PASS | AC10 | PASS |
| AC5 | PASS | AC11 | PASS |
| AC6 | PASS | AC12 | PASS |

Totals: **12 PASS, 0 PARTIAL, 0 FAIL, 0 unverified.**

## Acceptance Criteria Check-off

All twelve criteria were already `- [x]` in `spec.md` when the review began (reviewer read of lines 228-239). The reviewer's evaluation agrees with every one, so no criterion was checked, unchecked or reworded by this review, and `spec.md` is unchanged by the review.

## Baseline Comparison

| Dimension | Baseline (`177b6d78e`) | Head (`865a473f9`) |
|---|---|---|
| `QuickFiler.Test` tests passed / failed | 1468 / 0 | 1469 / 0 |
| Fixture class tests | 7 | 8 |
| `TransactionGate.WaitAsync()` (unbounded) in the fixture | 1 | 0 |
| `TransactionGate.WaitAsync(bound)` in the fixture | 0 | 1 |
| `TRANSACTIONGATE_ACQUIRE_TIMEOUT` under `QuickFiler.Test/` (`*.cs`) | 0 | 3 (1 fixture, 2 test file) |
| Fixture file lines | 304 | 342 |
| Test file lines | 396 | 458 |
| `QuickFiler.Test.csproj` | unchanged | unchanged |
| First-party line coverage exercised by `QuickFiler.Test` alone (scope-limited observation) | 24.40% (15170/62182) | 24.42% (15182/62182) |
| First-party branch coverage, same scope | 23.20% (3763/16222) | 23.20% (3763/16222) |

The +12 covered-line movement is spread across `UtilitiesCS` (+15 covered) and `QuickFiler` (−3 covered) with identical denominators and no instrumented file changed; it is run-to-run variation, not an effect of the change, which lives entirely in the uninstrumented test assembly.

## Residual Items

None blocking. Seven non-blocking findings (NB-1 to NB-7), all procedural or plan-text matters rather than delivery defects, and five informational notes are recorded in `policy-audit.2026-09-29T09-50.md`; four informational code observations are recorded in `code-review.2026-09-29T09-50.md`. The follow-ups the spec already records (a `CancellationToken`-observing overload; the two per-instance unbounded `WaitAsync()` calls in `BreadcrumbUiThreadDispatchTests.cs` line 391 and `BreadcrumbPopupBoundaryCoverageTests.cs` line 305) remain owed and are not defects of this change.

## Verdict

**PASS. 0 blocking findings. 12 of 12 acceptance criteria delivered and verified.**

No remediation is required and no `remediation-inputs` artifact is produced.
