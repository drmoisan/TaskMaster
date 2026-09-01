# Feature Audit — Issue #648 (`bug/wpfuidispatchertests-ungated-static-swap-648`)

- Timestamp: 2026-09-01T14-06
- Branch HEAD: `08868ba0ddc6036a49c3cdaf95b6993315b30aec`
- Baseline: `origin/main` at `c7b4f08f6d80296840f9a351042cb2113892e95f` (verified as the merge base)
- Work Mode: `minor-audit` (`issue.md:12`)
- AC source: `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/issue.md`,
  section `## Acceptance Criteria`, AC-1 through AC-7
- `spec.md` and `user-story.md` are correctly absent for this mode. Not a finding.

## Verdict

**All seven acceptance criteria PASS. Blocking findings: 0.** No checkbox was unchecked; every
criterion's own evidence was re-verified independently and none was contradicted.

## Acceptance-Criteria Evaluation

### AC-1 — Single reflection owner — **PASS**

Requirement: after the change, the quoted literal `"_dispatcher"` appears on exactly one line beneath
`QuickFiler.Test/`, in `QfcItemController.UiThreadDispatcherFixture.cs`; the baseline is two lines.

Reviewer measurement (not read from evidence):

```
git grep -n -F '"_dispatcher"' -- 'QuickFiler.Test/*.cs'
  QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136

git grep -n -F '"_dispatcher"' -- '*.cs'
  QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136
  UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:144
  UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:138
  UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:422
```

Exactly one line beneath `QuickFiler.Test/`, and it is the fixture. Tree-wide count is 4, down from
the recorded baseline of 5; the three remaining out-of-assembly lines are exactly the three the issue
declares outside this fix's reach (accepted residual R-2 of #493, overlapping #584). Corroborating
evidence: `evidence/regression-testing/p1-t5-ac1-single-owner.md`,
`evidence/baseline/p0-t16-structural-counts.md`.

### AC-2 — No reflection remains in the test file — **PASS**

Requirement: `WpfUiDispatcherTests.cs` contains no `GetField`, no `SetValue`, and no
`using System.Reflection;`.

Reviewer measurement: `git grep -n -E 'GetField|SetValue|using System\.Reflection;' --
QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` returns no match (exit 1). The diff confirms
`using System.Reflection;` and `using UtilitiesCS;` were both removed from the header, the latter
because it existed only to supply `typeof(UiThread)`. Corroborating evidence:
`evidence/regression-testing/p1-t4-ac2-no-reflection.md`.

### AC-3 — Swap routed through the shared fixture — **PASS**

Requirement: the test obtains its gate from `UiThreadDispatcherFixture.BeginTransactionAsync()`,
installs through the returned `UiThreadDispatcherTransaction`, restores by disposing that transaction
rather than by writing the field, and is declared `async Task`.

Verified against the head source:

- Gate obtained: `WpfUiDispatcherTests.cs:59` calls `UiThreadDispatcherFixture.BeginTransactionAsync()`;
  the task is awaited at `:60`.
- Installed through the transaction: `transaction.Install(dispatcher)` at `:63`.
- Restored by disposal: `transaction.Dispose()` at `:95`, in the inner `finally`, textually above the
  `ShutdownDispatcher` call at `:100` in the outer `finally`. No field write remains anywhere in the
  file.
- Declared `async Task`: `public async Task Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread()`
  at `:50`.

The gate acquisition is expressed as two statements rather than the single expression P1-T2 directed.
AC-3 is phrased over the routing, not over a specific expression shape, so the criterion is satisfied
either way; the deviation is separately adjudicated and accepted in
`code-review.2026-09-01T14-06.md` CR-1, where the reviewer independently reproduced the CSharpier
behaviour that forced it. Corroborating evidence:
`evidence/regression-testing/p1-t6-ac3-fixture-routing.md`.

The substance behind AC-3 — actual participation in the lock protocol rather than mere absence of
reflection — was checked separately and holds in full: `TransactionGate` is held across the entire
install-to-restore span, `FieldLock` covers the whole read-modify-write inside `Exchange`, and the
restore is `CompareExchange`'s `ReferenceEquals` compare-then-write rather than an unconditional
write. See `code-review.2026-09-01T14-06.md` CR-2 for the per-obligation table.

### AC-4 — Behavior preserved — **PASS**

Requirement: the test still asserts that `Invoke`, `InvokeAsync`, and `BeginInvoke` each execute their
delegate on the dispatcher's own thread, and the body of `Construction_YieldsAnIUiDispatcher` is
unchanged.

- `sut.Invoke(...)` at `:69` with `invokeThreadId.Should().Be(dispatcherThreadId);` at `:70`.
- `sut.InvokeAsync(...)` at `:74-76`, completion at `:77`, assertion at `:78`.
- `sut.BeginInvoke(...)` at `:84-88` with the `ManualResetEventSlim` signal at `:87`/`:89` and the
  assertion at `:91`.
- `dispatcherThreadId` is still read from `dispatcher.Thread.ManagedThreadId` at `:65`, so the
  assertions still compare against the dispatcher's own thread rather than against a proxy.
- `Construction_YieldsAnIUiDispatcher` at `:23-30`: the branch diff contains no added and no removed
  line inside that method. The only change adjacent to it is the new `GateTimeoutMs` field at `:21`,
  which sits above the method's `[TestMethod]` attribute.

The three assertions are unchanged in substance from the pre-change file; only their indentation moved,
by one level, because they are now nested inside the transaction's `try`. Corroborating evidence:
`evidence/regression-testing/p1-t7-ac4-behavior-preserved.md`.

### AC-5 — Tests green with no regression — **PASS**

Requirement: a scoped run restricted to `WpfUiDispatcherTests` reports zero failures with both tests
passing, and a full `QuickFiler.Test.dll` run reports zero failures with a passed count no lower than
the Phase 0 baseline.

| Run | Recorded | Reviewer-verified |
|---|---|---|
| Scoped, baseline (P0-T14) | `Total tests: 2`, `Passed: 2`, `Test Run Successful.`, exit 0 | — |
| Scoped, post-implementation (P1-T8) | `Total tests: 2`, `Passed: 2`, exit 0 | — |
| Scoped, final QC (P2-T5) | `Total tests: 2`, `Passed: 2`, exit 0 | yes — rerun independently at review time: `Passed Construction_YieldsAnIUiDispatcher [31 ms]`, `Passed Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread [40 ms]`, `Test Run Successful.`, exit 0 |
| Full suite, baseline (P0-T13) | `Total tests: 1285`, `Passed: 1285`, exit 0 | — |
| Full suite, final QC (P2-T6) | `Total tests: 1285`, `Passed: 1285`, exit 0 | — |
| Full solution, coverage-enabled | 6,925 tests, both runs `Test Run Successful.` | — |

Post count equals the baseline count exactly at every level; no test was added, removed, skipped, or
renamed. The reviewer additionally ran the five dispatcher-touching test classes together under the
repository runsettings (`Scope=ClassLevel`, `Workers=0` resolving to 24 workers) to exercise the newly
introduced `TransactionGate` contention: 47/47 passed in 1.45 s, no deadlock and no starvation.

One honest limitation, already stated in the evidence and repeated here: a green run under class-level
parallelization does not prove the race is eliminated. It shows the gated path is stable under that
scope. The elimination argument rests on the structural fact that the ungated writer no longer exists
inside `QuickFiler.Test`, which AC-1 and AC-2 measure directly.

### AC-6 — Scope boundary held — **PASS**

Requirement: the branch diff against `origin/main` changes exactly one `.cs` path,
`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`, and changes no path beneath `UtilitiesCS.Test/`
or `UtilitiesCS/`.

Reviewer measurement over the full 58-path diff:

- Paths ending in `.cs`: exactly one, `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`.
- Paths beneath `UtilitiesCS/` or `UtilitiesCS.Test/`: zero.
- The three named out-of-scope mutators (`ProgressTracker_Tests.cs`, `ProgressTrackerAsync_Tests.cs`,
  `IdleAsyncQueue_Tests.cs`) are untouched; the `git grep` output under AC-1 shows them still at their
  baseline line numbers 422, 138, and 144.
- The consumed fixture, `QfcItemController.UiThreadDispatcherFixture.cs`, is not in the diff. It was
  consumed unchanged, as the issue directed.

The remaining 57 paths are 45 feature-folder Markdown files, 2 feature-folder JaCoCo XML files, and 10
tracked `.claude/agent-memory/**` Markdown files written by the planner, executor, orchestrator and
researcher during the run. AC-6 constrains `.cs` paths and the two `UtilitiesCS` trees only, so none of
these affects the criterion. The agent-memory files are tracked by repository design
(`.gitignore:351`) and are recorded here for completeness rather than as a deviation.

Note on the post-execution coverage substitution: it deleted two `.cobertura.xml` paths, added two
`.jacoco.xml` paths and one Markdown record, and touched no `.cs` path and no `UtilitiesCS` path. The
reviewer re-measured AC-6 against HEAD `08868ba0` — after the substitution — rather than against the
`8d933975` measurement the evidence records, and both clauses still hold.

Corroborating evidence: `evidence/qa-gates/p2-t13-ac6-scope-boundary.md`,
`evidence/baseline/p0-t17-scope-boundary.md`.

### AC-7 — Toolchain green and evidence complete — **PASS**

Requirement, in two halves.

**Toolchain half.** CSharpier check, analyzer rebuild, and nullable rebuild each report zero errors
and introduce no new finding relative to the Phase 0 baseline.

| Gate | Baseline | Head | Delta |
|---|---|---|---|
| `dotnet tool run csharpier check .` | exit 0, `SourceScopedDrift: none` | exit 0, `SourceScopedDrift: none`, `ComparedDrift` set-equal to baseline | none |
| `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0, `0 Error(s)`, 5 warnings | exit 0, `0 Error(s)`, 5 warnings | none; no diagnostic names `Controllers\WpfUiDispatcherTests.cs` |
| `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` | exit 0, `0 Error(s)`, 5 warnings | exit 0, `0 Error(s)`, 5 warnings | none; no diagnostic names `Controllers\WpfUiDispatcherTests.cs` |

Both msbuild gates use `/t:Rebuild`, not `/t:Build`, so `CoreCompile` actually ran and the gates are
not vacuous. Neither adds `/p:Nullable=enable`, matching `.github/workflows/ci.yml`. The warning count
is flat at 5 on both sides of both gates, so the change introduces no new diagnostic.

**Evidence half.** The canonical evidence tree carries the Phase 0 baseline artifacts, the Phase 2
final-QC artifacts, and a fail-before record.

- Phase 0: all 19 named artifacts present (18 Markdown plus the coverage XML, the latter now the
  JaCoCo projection rather than the raw Cobertura it was at check-off time).
- Phase 2: all named QC artifacts present.
- Fail-before record: `evidence/regression-testing/fail-before-exception.2026-09-01T14-16.md`, a
  schema-valid `fail-before-exception` dossier rather than a recorded failing run. AC-7 explicitly
  permits that substitution. The dossier carries all seven required fields plus the alternative-proof
  section, sets `SearchResult:` to `none` with an explicit statement that the search preceded the
  dossier's own creation, and names P1-T4, P1-T5, P1-T8, P2-T5, P2-T6 and the six #493 regression
  tests while correctly excluding P2-T7 and P2-T8 as non-behavior-preservation tasks.
- The reviewer verified the dossier's load-bearing external claim rather than accepting it:
  `.github/workflows/_mstest-coverage.yml:83` invokes `vstest.console.exe` with `/EnableCodeCoverage
  /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` and no `/Settings:`, and a
  repository-wide grep for `Settings:` across `.github/workflows/` returns nothing. The race is
  therefore genuinely dormant in the only run that gates merge, which is what makes the absence of a
  deterministic red run defensible rather than convenient.

**Caveat, non-blocking.** The `Timestamp:` values on all 42 evidence artifacts are synthetic rather
than observed; see `policy-audit.2026-09-01T14-06.md` finding F-2. This does not defeat AC-7, which
requires the artifacts to exist and to record the gate outcomes, and every gate outcome was
cross-checked and holds. It does mean the timestamps cannot be used to establish the order in which
the gates ran.

## Baseline Comparison Summary

| Dimension | `origin/main` `c7b4f08f` | HEAD `08868ba0` | Direction |
|---|---|---|---|
| Ungated writers of `UiThread._dispatcher` inside `QuickFiler.Test` | 1 | 0 | fixed |
| Reflection owners of that static beneath `QuickFiler.Test/` | 2 | 1 | fixed |
| Restore semantics at this call site | unconditional write | `ReferenceEquals` compare-then-write | fixed |
| Locks held at this call site | none | `TransactionGate` + `FieldLock` | fixed |
| `WpfUiDispatcherTests.cs` line count | 88 | 104 | within the 500-line limit |
| `QuickFiler.Test` tests passed | 1285 | 1285 | flat |
| Analyzer warnings (solution) | 5 | 5 | flat |
| Nullable-gate warnings (solution) | 5 | 5 | flat |
| Repo-wide line coverage (9 first-party packages) | 85.376% | 85.373% | -0.003 pt, inside the measurement band |
| Repo-wide branch coverage (same packages) | 79.715% | 79.694% | -0.021 pt, inside the measurement band |

Both coverage figures remain above the `.claude/rules/general-unit-test.md` floors of 85% line and
75% branch. The movement is confined to the `UtilitiesCS` package against an identical denominator and
cannot originate in the changed file, whose assembly is stripped from the document before the root
counters are recomputed.

## Residual Risk Carried Forward (declared in the issue, unchanged by this branch)

- **R-2 of #493** — the three cross-assembly mutators in `UtilitiesCS.Test` still write the same
  process-wide static without any lock. No test-side lock inside `QuickFiler.Test` can reach them.
  Explicitly out of this issue's boundary and tracked against #584. Confirmed still present at
  `ProgressTracker_Tests.cs:422`, `ProgressTrackerAsync_Tests.cs:138`, `IdleAsyncQueue_Tests.cs:144`.
- The conditional restore introduced here interacts correctly with that residual: if one of those
  cross-assembly writers replaces the static while this transaction holds it, `CompareExchange` skips
  the restore rather than clobbering the newer value. That is a strict improvement over the
  pre-change unconditional write.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/issue.md
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none
```

No checkbox was modified by this review. All seven were already `[x]` on arrival, and independent
verification confirmed each rather than contradicting any, so none was unchecked.
