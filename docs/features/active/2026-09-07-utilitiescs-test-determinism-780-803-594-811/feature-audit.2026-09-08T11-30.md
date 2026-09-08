# Feature Audit — utilitiescs-test-determinism (Issue #811)

- Date: 2026-09-08T11-30
- Branch: `bug/utilitiescs-test-determinism-780-803-594-811`
- Head: `3805ca89f0c72e2bb448726695f18f420bd260f7`
- Base: `bb1c7d4b60f7b782227956f36859314d5c47bb03`
- Work mode: `full-bug` (marker at `issue.md` line 12)
- Acceptance-criteria source: **`spec.md` only**, lines 292-296

Blocking findings in this artifact: **0**. (The branch total is 1, recorded as F-1 in
`policy-audit.2026-09-08T11-30.md`.)

## AC Source Resolution

`issue.md` line 12 records `- Work Mode: full-bug`. Per
`.claude/skills/acceptance-criteria-tracking/SKILL.md`, `full-bug` resolves the acceptance-criteria
source to `spec.md` only. `issue.md` lines 67-71 carry an AC-shaped checkbox list with identical
wording; it is **not** the authority under this work mode and was not audited as such. No
`user-story.md` exists, which is correct for `full-bug`.

`spec.md` has exactly five criteria under `## Acceptance Criteria` at lines 292-296.

## Acceptance Criteria Evaluation

| AC | Criterion (abbreviated) | Verdict | Checkbox state in `spec.md` |
|---|---|---|---|
| AC1 | `TryAddValuesAsync` no longer cancels on a fixed wall-clock window; the test passes deterministically under 24-worker parallel coverage runs | PASS | `[x]` — correct |
| AC2 | `DfDeedle_COM_Tests` no longer mutates process-wide static seams another class can observe; the null snapshot element is guarded with a descriptive failure | PASS | `[x]` — correct |
| AC3 | The `Console.Out` races are removed by eliminating the shared-console dependency | PASS | `[x]` — correct |
| AC4 | A full nine-assembly `/InIsolation` run with `TestCategory!=LiveOutlook` reports zero failures on ten consecutive runs, recorded as evidence | **FAIL** | `[ ]` — correct |
| AC5 | No test is stabilized by a sleep, a retry, or a timing tolerance | PASS | `[x]` — correct |

No checkbox required a change. Every criterion this reviewer evaluated PASS was already checked;
the one criterion evaluated FAIL was already unchecked. This reviewer made no edit to `spec.md`.

### AC1 — PASS

Both halves verified independently against the post-change source rather than against the executor's
report.

Mechanism: `UtilitiesCS/Extensions/DictionaryExtensions.cs` no longer creates a linked token source
and no longer calls `CancelAfter(500)`. The body is now
`return await Task.Run(() => dictionary.TryAddValues(key, value), token);`, so cancellation is
governed solely by the caller's token. No `TimeProvider` was needed because the window guarded
nothing: a token supplied to `Task.Run` can only cancel work that has not yet started, and the body
is a bounded compare-and-swap loop that performs no I/O.

Blast radius verified: a repository-wide search over `*.cs` for `TryAddValuesAsync` returns four
hits — the definition at `DictionaryExtensions.cs:169`, one invocation at
`DictionaryExtensions_Tests.cs:244`, one at `:266`, and two test method names. There are **zero
production call sites**, so the deletion cannot alter production behavior. The spec's claim on this
point is confirmed rather than assumed.

Determinism verified: `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` — the
sentinel test named in issue #780 — read `Passed` in all ten AC4 runs under 24 class-level workers
with coverage instrumentation (`evidence/regression-testing/p8-t6-ac4-ten-run.md`, exhaustive
per-run check, not a sample).

Surviving contract locked: the new
`TryAddValuesAsync_PreCancelledToken_ThrowsTaskCanceledAndLeavesValueUnchanged` asserts that a
pre-cancelled caller token still produces `TaskCanceledException` and leaves the dictionary value
untouched, so a future edit cannot silently drop cancellation altogether.

RED-first note: `spec.md` states, and this reviewer accepts, that AC1 admits no deterministic
fail-before test — the original failure is load-dependent and reproducing it would require a sleep,
which AC5 forbids. The RED artifact for AC1 is therefore the contract-shape test rather than a
reproduction, and the spec says so in advance rather than after the fact.

### AC2 — PASS

**Clause 1, static-seam mutation.** The two ETL delegate statics are gone.
`DfDeedle.TableEtlInvoker` and `DfDeedle.StoreTableEtlInvoker` are replaced by a single
`private static readonly DefaultTableEtl` plus an optional `etl` parameter on
`GetEmailDataInView(Explorer, ...)` and both `FromDefaultFolder` overloads. The three tests in
`DfDeedle_COM_Tests` that previously swapped and restored those statics now pass `etl:` directly and
their `try`/`finally` blocks are deleted. Mutation of process-wide ETL state by that class is
eliminated, not merely restored.

`DfDeedle.MessageBoxInvoker` survives as a mutated static in four tests. `spec.md` line 135 settles
this deliberately, and this reviewer verified the exposure argument rather than accepting it: the
only production readers are `DfDeedle.QfcColumns.cs:27,170,194`, and a repository-wide search shows
exactly three test classes reach those paths — `DfDeedle_COM_Tests` itself (whose tests are
serialised by `ExecutionScope.ClassLevel`), `DfDeedleQfcColumnTimeoutTests` and the new
`DfDeedleEtlTimeoutTests`, both `[DoNotParallelize]`. The criterion's qualifier "in a way another
class can observe" is therefore satisfied. The residual is that this holds only while both reader
classes retain their attribute, and nothing enforces that; recorded as C-5 in
`code-review.2026-09-08T11-30.md`, not as an AC shortfall.

**Clause 2, the null-snapshot guard.** `DfDeedle.GetEmailDataInViewAsync` now tests
`tableSnapshot.data is null` immediately after the `EtlAsync` call and throws
`InvalidOperationException` naming the folder and stating that the table ETL timed out or was
cancelled before returning rows. The guard is placed before the `LogDfTiming` statement that
dereferences `tableSnapshot.Item1` — the statement the CI stack trace reported — and therefore also
before the pre-existing `ValidateRequiredEmailColumns` guard, which inspects the column map only and
would not have caught this.

RED-first evidence exists for AC2 and is the strongest evidence in the item:
`evidence/regression-testing/p2-t4-ac2-fail-before.md` and
`fail-before-exception.2026-09-08T09-50.md` record the pre-fix `NullReferenceException`;
`p3-t2-ac2-pass-after.md` records the post-fix `InvalidOperationException`. The assertion names the
exception type explicitly and matches the folder name (`WithMessage("*Inbox*")`), so a regression to
`NullReferenceException` fails the test rather than passing a generic throw assertion.

Sentinel determinism: `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` — the
test named in #803 and #594 — read `Passed` in all ten runs, now driven by an un-advanced
`FakeTimeProvider` so no deadline on its path can fire under host load.

### AC3 — PASS

The criterion says "two" races; `spec.md` lines 59-64 records the discrepancy and derives four
capture-and-assert sites twice by two independent search strategies. This reviewer confirmed all
four are converted in the diff:

| Site | Conversion | `[DoNotParallelize]` |
|---|---|---|
| `StackGeek_Tests.Main_RunsSampleScenarioWithoutThrowing` | split into a null-writer default test plus `Run_WritesScenarioToSuppliedWriter` asserting on a `StringWriter` | removed |
| `PrettyPrint_Tests.DataFramePrettyHelpers_RenderRowsMarkdownAndConsoleOutput` | renamed `...AndWriterOutput`; passes `writer` to both `PrettyPrint` overloads | removed |
| `DASLFilterParserTests.PrintTree_WritesIndentedTreeToConsole` | renamed `...ToSuppliedWriter`; passes `writer` to `PrintTree` | removed |
| `OlTableExtensions_Tests.EnumerateTable_WritesFormattedOutputAndMovesToStart` | renamed `...ToSuppliedWriterAndMovesToStart`; passes `output` to `EnumerateTable` | retained, with a corrected rationale |

The fifth site, the propagator `NLogTraceWriter_Test`, had its `Console.Out` save at
`TestInitialize` and its restoring `TestCleanup` deleted entirely, together with the now-unused
`originalOut` field. That removes the mechanism by which it could capture a sibling class's writer
and later install it process-wide.

In every one of the four converted tests, no `Console.SetOut` call and no `Console.Out` read remains.
The shared-console dependency is eliminated, which is what the criterion asks for, rather than
serialised.

Retention of the fourth attribute is not a shortfall. `spec.md` Mitigation 4 pre-authorises exactly
this conservative fallback for `OlTableExtensions_Tests`, on the ground that removing the attribute
from an 1846-line COM-mock class with ten tests driving a 2000 ms window carries risk unrelated to
the console. The comment was rewritten so it no longer claims a reason that is now false. The class
is still converted to the seam, which is the criterion's substance.

Determinism verified: all four converted tests read `Passed` in all ten AC4 runs, three of them now
running inside the parallel set rather than the serialized tail.

### AC4 — FAIL

The criterion requires zero failures on ten consecutive full nine-assembly `/InIsolation` runs.
Nine runs were clean. Run 7 reported `passed`=7161 against `total`=7162.

The failing test was
`UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.MethodBodyReader_Tests.GetBodyCode_ReturnsConcatenatedInstructions`,
failing on `Expected bodyCode ... to contain "ldstr"`.

This reviewer verified the cause independently by reading the source, and separately corroborated it
from the failure output. The full derivation, including why the observed single-entry degradation
and the unresolved `0x70……`-range metadata token uniquely identify a mid-initialisation read of
`ILGlobals.singleByteOpCodes`, is recorded as F-1 in `policy-audit.2026-09-08T11-30.md`. Summary:
`ILGlobals.LoadOpCodes()` reassigns the table to an all-default array at line 123 before filling it
at line 137, with no lock or `Lazy<T>`; two test classes call it and neither carries
`[DoNotParallelize]`.

Attribution, reached independently and stated in full in section 4.2 of
`code-review.2026-09-08T11-30.md`: the defect is pre-existing and wholly outside this item. Neither
racing class is in the write set; neither gained nor lost a parallelism attribute; the vulnerable
window is byte-identical. The change did move three classes into the parallel set, but instantaneous
concurrency is capped at 24 workers and the eligible pool already exceeds 400 classes, so the pool
was worker-saturated before and after; the three short in-memory classes perturb dispatch order
without raising density. No directional increase in surfacing probability can be established, and
the evidence does not measure the base rate.

Disposition: **the AC4 shortfall is Non-blocking for merge.** The branch removes three failure modes
and leaves one; blocking it would leave four in `main`. Fixing `ILGlobals` inside this item would
breach the minimal-fix requirement of CLAUDE.md's Bugfix Workflow. What is Blocking is that the
finding has no durable follow-up artifact (F-1).

Process quality note: the executor did not re-run until green. `p8-t4-ac4-runs.md` states "No re-run
was performed: re-running until green is what AC4 exists to prevent, and doing it here would destroy
the evidence." That is the correct discipline and is worth recording, because the opposite behavior
would itself have been a blocking finding.

### AC5 — PASS

The criterion constrains technique, not only outcome. Verified against the diff independently of the
executor's search:

- Zero `Thread.Sleep`, `Task.Delay`, `CancelAfter`, `SpinWait`, `Stopwatch`, timed `.Wait(n)`,
  `new CancellationTokenSource(<int>)` or `Returns(120)` occurrences among the added lines.
- Zero `catch` blocks in the two new test files; both use `finally` only, to release gates.
- The three surviving `.Wait` references (`gateA.Wait`, `gateB.Wait`, `gate.Wait`) are
  `ManualResetEventSlim.Wait()` **method groups passed as delegate arguments**, invoked inside mock
  callbacks so the production code blocks until the test itself releases the gate in `finally`. An
  untimed wait cannot mask a timing defect: it places no bound on how long the code under test may
  take, and the outcome is decided by the injected clock rather than elapsed wall time. This is the
  same pattern the pre-existing `DfDeedleQfcColumnTimeoutTests` already uses.
- The pre-existing tolerance at `OlTableExtensions_Tests.cs:960-963` — `GetRowCount()` returning 120
  with the comment "so the timeout cannot fire under test-host contention" — is **retired**, not
  merely absent from added lines. Both tokens count 0 in the final tree; each counted 1 at the base
  commit. It is replaced by an un-advanced `FakeTimeProvider`.

The distinction that matters here is between widening a deadline and controlling the clock. The
change consistently does the latter: green-path tests use a clock that never moves, so the deadline
is unreachable rather than enlarged, and production timing is untouched because a `null`
`timeProvider` resolves to `TimeProvider.System`.

## Baseline Comparison

| Dimension | Baseline (`bb1c7d4b`) | Head (`3805ca89`) | Direction |
|---|---|---|---|
| Repository line coverage | 86.0109% | 86.0424% | improved |
| Repository branch coverage | 66.3058% | 66.3978% | improved |
| Uncovered lines, `OlTableExtensions.Etl.cs` | 26 | 12 | improved |
| Uncovered lines, `DfDeedle.cs` | 2 | 0 | improved |
| `EtlAsync` method-span line rate | 0.7000 | 0.9796 | improved |
| Mutable process-wide statics on `DfDeedle` | 3 | 1 | improved |
| `[DoNotParallelize]` classes among the four console victims | 4 | 1 | improved |
| Serialized classes, net across the change | n/a | minus 3, plus 2 | net minus 1 |
| Timing tolerances in the touched tests | 1 | 0 | improved |
| `DateTime.Now` calls on the ETL log path | 1 | 0 | improved |
| Known intermittent failure modes in `UtilitiesCS.Test` | 4 (3 in scope, `ILGlobals` latent) | 1 (`ILGlobals`) | improved |
| Analyzer warnings, full solution | 0 | 0 | held |
| Nullable / warnings-as-errors diagnostics | 0 | 0 | held |
| Files over the 500-line cap among the write set | 3 | 3 (all held or shrank) | held |
| Test total | not recorded at base | 7162 | +9 added tests |

## Deliverables Against `spec.md` Item List

`spec.md` enumerates a 20-item write set (8 production files, 11 test files, 1 project file). The
diffstat confirms exactly those 20 source paths changed and no others. No file outside the declared
write set was modified. The three new files listed at items 11, 12 and 14 exist and are wired into
the project via three `<Compile Include>` items, which is required because these are legacy non-SDK
`packages.config` projects where an unlisted `.cs` file is silently not compiled.

Non-goals held: issue #592 is untouched; the 250 ms per-row budget is unchanged; the 2000 ms
`GetTableInViewAsync` window is unchanged; the inert `(int, int)` `TimeoutAfter` overloads remain in
`TimeOutTask.cs`, which is byte-identical at 1011 lines; `EtlAsync`'s tuple contract is unchanged;
the roughly 24 unrestored `DebugTextWriter` aggressors are untouched. Two of these were filed as
`docs/features/potential/` follow-up entries.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/spec.md
- Total AC items: 5
- Checked off (delivered): 4
- Remaining (unchecked): 1
- Items remaining: AC4: A full nine-assembly /InIsolation run with TestCategory!=LiveOutlook reports zero failures on ten consecutive runs, recorded as evidence.
```

Newly checked off by this reviewer: none. All four PASS criteria were already checked, and the one
FAIL criterion was already unchecked. The AC state in `spec.md` is accurate as delivered.

## Verdict

**Feature verdict: PASS with one outstanding acceptance criterion and one blocking process finding.**

4 of 5 acceptance criteria are met with verified evidence. AC4 is not met, for a reason this
reviewer independently attributes to a pre-existing defect outside the write set. The single
blocking item before merge is F-1: file and promote the `ILGlobals` race so the finding survives the
merge. Remediation inputs are recorded in `remediation-inputs.2026-09-08T11-30.md`.
