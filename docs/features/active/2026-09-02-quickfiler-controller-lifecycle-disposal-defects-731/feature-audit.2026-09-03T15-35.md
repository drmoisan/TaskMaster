# Feature Audit — issue #731 (quickfiler-controller-lifecycle-disposal-defects)

- Timestamp: 2026-09-03T15-35
- Branch: `bug/quickfiler-controller-lifecycle-disposal-defects-731` @ `c55bfad2`
- Diff base (merge-base with `origin/main`, independently recomputed): `35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e`
- Work mode: **`full-bug`** — marker `- Work Mode: full-bug` at `issue.md:12`
- AC source: **`spec.md` only**, section `## Acceptance Criteria`, 19 checkboxes at lines 217-235.
  `user-story.md` does not exist and is not required in this mode; `issue.md` is not an AC source in
  this mode.

## Method

Every criterion below was evaluated against the delivered source, the branch diff and the raw
coverage documents. The executor's check-offs were **not inherited**. Where a criterion is backed by
a numeric claim, the number was re-derived by this reviewer from primary data — `git` for diffs and
line counts, `System.Xml.XmlDocument` over the two Cobertura documents for coverage — rather than
read from an evidence artifact. Where a criterion is backed by a behavioural claim, the assertion in
the test and the corresponding production statement were both read.

Total AC items: 19. Verified PASS: 19. PARTIAL: 0. FAIL: 0. UNVERIFIED: 0.

## Acceptance Criteria Evaluation

### AC1 (spec.md:217) — Per-owner comments on all three monitor initializers — **PASS**

Verified in source, not via an artifact. All three comments exist immediately above their field
initializer:

| Owner | Comment line | Initializer line |
|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | 84 | 85 |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 104 | 105 |
| `QuickFiler/Controllers/QfcQueue.cs` | 41 | 42 |

Each contains, verbatim, all four required elements: that the instance is deliberately per-owner
("Deliberately one monitor instance per owner, not a shared singleton"); the at-most-one-action
dispatch ("BeforeItemMove dispatches at most one action per MailItem via FirstOrDefault"); the
instance-scoped teardown ("UnhookAll is instance-scoped and clears the whole hook list"); and both
citations ("issue #731 finding 1, issue #620").

The two claims the comments make were independently checked against `EmailMoveMonitor.cs`:
`FirstOrDefault` single-action dispatch at `:216-223`, instance-scoped `UnhookAll` at `:189-204`.
The comments are accurate, not merely present.

### AC2 (spec.md:218) — Structural topology pin — **PASS**

`QuickFiler.Test/Controllers/QfcMoveMonitorTopologyTests.cs`, two methods:

- `EachOwnerDeclaresExactlyOneEmailMoveMonitorInitializer` reads each of the three owner sources and
  asserts exactly one occurrence of `= new EmailMoveMonitor();` in whitespace-normalised text.
- `NoTypeDeclaresMoreThanOneEmailMoveMonitorField` reflects over the assembly containing
  `EmailMoveMonitor`, asserts a per-type maximum of one `IEmailMoveMonitor`-typed declared field,
  and asserts exactly three declaring types in aggregate.

Discrimination verified in both directions, as the criterion requires. A fourth owner raises the
aggregate to 4 and fails `HaveCount(3)`. A collapse to a shared singleton drops at least one owner's
source count to 0 and fails `Be(1)`, and simultaneously drops the aggregate below 3. Neither failure
mode is vacuous.

Fail-before status is correctly handled by a schema-valid exception dossier at
`evidence/regression-testing/fail-before-exception.finding1-topology-pin.md` rather than a fabricated
failing run, because a comment-only change has no defect state to reproduce. Pass-after recorded at
`evidence/regression-testing/finding1-topology-pin-pass.md`.

### AC3 (spec.md:219) — `_moveMonitor` field name unchanged; reflection-based injectors still pass — **PASS**

The field name is unchanged on all three owner types; the branch diff touches no line containing the
declaration itself (the additions are the comment lines above them). Verified by direct search: all
three declarations read `private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();`.

Existing reflection-based injectors: the full suite reports 6995/6995 passed with 0 failed and 0
skipped, against a baseline of 6985/6985/0/0 over an identical nine-assembly set. A broken reflection
target would surface as a failure, not a skip; none occurred.

### AC4 (spec.md:220) — Stale `EmailMoveMonitor` class comment corrected — **PASS**

Diff verified. Removed:

```
// TODO: Determine what EmailMoveMonitor was supposed to be used for. It is now malfunctioning. Temprorarily disabling.
```

Replaced by a five-line description of the class's actual responsibility, its instance-scoped hook
list, and the reason each owner constructs its own monitor, citing issue #731 finding 1. Neither
"malfunctioning" nor "disabling" nor any equivalent survives anywhere in the file.

The correction is factually warranted, which I checked rather than assumed: the class has 16 live
production call sites (8 `HookItem`, 2 `UnhookAll`, 6 `UnhookItem`) across the three owners and their
partials. It is not disabled.

### AC5 (spec.md:221) — `CompleteAdding()` before any disposal; disposal deferred to a fault-reading continuation — **PASS**

Verified statement by statement at `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`:

| Requirement | Line(s) | Observation |
|---|---|---|
| `CompleteAdding()` on the undo queue | 225 | `undoQueue?.CompleteAdding();` |
| ...before **any** disposal | 225 vs 233, 245 | Both `Dispose()` sites are strictly after; the immediate one at `:233` and the deferred one at `:245`. |
| Disposal only after the consumer completes | 238-248 | `undoConsumer.ContinueWith(..., TaskScheduler.Default)` — the continuation cannot run before the antecedent completes. |
| Continuation reads the antecedent's fault | 241 | `if (antecedent.Exception is not null)` — reading `Task.Exception` marks the fault observed. |
| Continuation logs the fault | 243 | `logger.Error("Undo consumer faulted.", antecedent.Exception);` |

The `undoConsumer is null` branch (`:231-235`) disposes immediately; there is by construction no
consumer to wait for, so the "only after the consumer task has completed" clause is satisfied
vacuously and correctly.

Behaviourally pinned by `Cleanup_WithRunningConsumer_CompletesAddingBeforeDisposing` (asserts
`IsAddingCompleted` is true **and** the queue is still usable, proving the ordering) and by
`Cleanup_WithFaultedConsumer_ObservesAndLogsTheFault` (asserts the continuation reaches
`RanToCompletion` and the queue ends disposed). The *log* half of "reads and logs" is verified by
source inspection at `:243`; there is no logger seam on this type, which is a reasonable limit and
is noted in the code review rather than counted against the criterion.

### AC6 (spec.md:222) — No synchronous wait anywhere on the teardown path — **PASS**

Read the whole of `QfcFormController.SetupDisposal.cs`: no `Task.Wait`, no `.Result`, no
`Thread.Sleep`, no wall-clock timeout, and no `TimeSpan`-bounded wait of any kind. The deferral
mechanism is `ContinueWith` on `TaskScheduler.Default`, which schedules rather than blocks.

Pinned twice: `Cleanup_SourceContainsNoSynchronousWait` scans the file for all four banned literals
(a forward guard, honestly labelled as such in its `<summary>` and confirmed passing pre-fix in the
fail-before artifact), and `Cleanup_WithParkedConsumer_ReturnsWithoutWaiting` asserts behaviourally
that `Cleanup()` returns while `consumer.IsCompleted` is still false.

The rejection of the synchronous-wait design is well grounded: the caller `Cleanup();` at
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:93` sits inside `ActionCancelAsync`
immediately after `await _formViewer.UiSyncContext` at `:89`, so it runs on the UI thread and a
`Wait` there would deadlock against the dispatcher hop in `ProcessUndoItemAsync`. I traced this call
site directly.

### AC7 (spec.md:223) — New `QfcFormControllerCleanupTests` with a genuine RED regression test plus three further paths — **PASS**

File created at `QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs`, 399 lines, 7 methods.

Required regression test: `Cleanup_WithRunningConsumer_ConsumerReachesRanToCompletion`. RED evidence
at `evidence/regression-testing/finding2-cleanup-fail-before.md`, `EXIT_CODE: 1` with
`ExpectedExitCode: 1`, carrying the real diagnostic:

```
System.ObjectDisposedException: The collection has been disposed. Object name: 'BlockingCollection'.
   at System.Collections.Concurrent.BlockingCollection`1.CheckDisposed()
   at System.Collections.Concurrent.BlockingCollection`1.get_IsCompleted()
   at QuickFiler.Controllers.QfcFormController.<UndoConsumer>d__102.MoveNext() in
QuickFiler\Controllers\QfcFormController.Actions.cs:line 322
```

That stack is exactly the defect finding 2 describes — the consumer's next `IsCompleted` evaluation
against a queue disposed underneath it. The reproduction is genuine, not a fabricated assertion
failure. GREEN evidence at `evidence/regression-testing/finding2-cleanup-pass-after.md`, 7/7 passing.

The three additional required paths are all present:

| Required path | Method |
|---|---|
| Null-consumer | `Cleanup_WithNullConsumerTask_DisposesQueueAndDoesNotThrow` |
| Parked-consumer non-blocking | `Cleanup_WithParkedConsumer_ReturnsWithoutWaiting` |
| Completion before disposal | `Cleanup_WithRunningConsumer_CompletesAddingBeforeDisposing` |

Two beyond the criterion: `Cleanup_CalledTwice_DoesNotThrow` and
`Cleanup_WithFaultedConsumer_ObservesAndLogsTheFault`.

Three of the seven failed pre-fix, four passed pre-fix as forward guards. The fail-before artifact
records which is which and why, rather than presenting all seven as reproductions. That distinction
is stated correctly.

### AC8 (spec.md:224) — `QfcFormControllerSeamTests` unmodified, line count unchanged — **PASS**

Independently measured: **496 lines at the merge-base and 496 lines at HEAD**
(`[System.IO.File]::ReadAllLines().Count` against both the base blob and the working file). The file
does not appear in the branch diff at all — verified against my own full `--numstat` listing, not
against the executor's name-status capture.

### AC9 (spec.md:225) — Both dead constructor parameters and the guard removed; neither stored — **PASS**

`QuickFiler/Controllers/QfcRemainingQueueAdmission.cs`, base 48 lines → HEAD 40 lines, 0 insertions
/ 8 deletions. Removed by the diff: the `IApplicationGlobals globals` parameter, the
`Func<MailItem, CancellationToken, Task<long>> scoreLoader` parameter, the four-line
`if (scoreLoader is null) throw new ArgumentNullException(...)` guard, and the now-unused
`using UtilitiesCS;`.

Post-state read directly: the sole constructor declares exactly `addToQueue`, `hookItem`,
`removeFromQueue`; the type declares exactly the three matching readonly fields. Neither removed
parameter is stored anywhere.

Independently observed side effect: this file's line coverage rose from 92.00% (23/25) to 100.00%
(20/20), because the two previously uncovered lines were the removed guard and its throw.

### AC10 (spec.md:226) — Sole production construction site updated, scoring lambda removed, test factory updated, solution compiles clean — **PASS**

Production call site at `QuickFiler/Controllers/QfcDatamodel.cs:353-359`: the diff removes exactly
`_globals,` and `async (m, t) => (await ScoreRemainingQueueMailItemAsync(m, t)).Score,`. The lambda
is gone entirely. `ScoreRemainingQueueMailItemAsync` itself remains and is independently used, so it
is not orphaned.

Test factory `CreateQueueAdmission` in `QfcDatamodelTests.cs` reduced from five parameters to two;
all five call sites updated in the same hunk set.

"No remaining reference to the removed parameters" is established by compilation, not by search: both
`/t:Rebuild` gates return `EXIT_CODE: 0` with **0 warnings and 0 errors**. A residual reference to a
deleted parameter is a compile error, so a clean full rebuild is conclusive.

### AC11 (spec.md:227) — Issue-#233 intent re-pinned structurally, original replaced not deleted — **PASS**

`TryQueueRemainingMailItemAsync_HighConfidenceEnabled_IgnoresThresholdAtAdmission` was **replaced**
by `QfcRemainingQueueAdmission_DeclaresNoScoringDelegate` within a single diff hunk — the old method
signature and body are removed and the new one added in place. It was not deleted outright.

The new test asserts both required structural properties: the sole constructor declares no
scoring-delegate parameter, and the type declares no scoring-delegate field.

Rationale carried: the assertion message constant reads
`"issue #233: Threshold scoring belongs to dequeue-time enforcement."`, containing the original
sentence verbatim.

Discrimination proven: `evidence/regression-testing/finding3-admission-pin-fail-before.md` shows the
test failing pre-fix with the real reflected parameter list naming `scoreLoader` as the offending
item.

**Note, not a deduction.** The assertion matches one exact delegate type,
`Func<MailItem, CancellationToken, Task<long>>`, rather than any scoring-delegate shape. A
differently shaped scorer would evade it. Recorded as code-review finding CR-3. The criterion is
nonetheless met: the replacement is stronger than what it replaced in the dimension that matters —
the old test proved the scorer was not called on one path, the new one proves the type cannot score
at all, which also subsumes the four sibling tests that lost their throwing-scorer pins in the same
edit.

### AC12 (spec.md:228) — Sole read via `Volatile.Read`; field not `volatile`; writes and declaration unchanged — **PASS**

The entire diff to this region is one line:

```
-                if (removespecificcontrolgroupcounter > 1)
+                if (Volatile.Read(ref removespecificcontrolgroupcounter) > 1)
```

Verified from the diff that the field declaration and both `Interlocked` sites are byte-unchanged —
they do not appear in the diff at all. Post-state read directly: declaration `:911`,
`Interlocked.Increment` `:915`, `Volatile.Read` guard `:993`, `Interlocked.Decrement` `:1010`. The
field carries no `volatile` modifier.

The `volatile` avoidance is substantively justified, not merely asserted: passing a `volatile` field
by `ref` to `Interlocked` produces CS0420, and the nullable gate runs with
`/p:TreatWarningsAsErrors=true`, which would turn two clean lines into two build errors. That gate
returns `EXIT_CODE: 0` with 0 warnings on the delivered form.

### AC13 (spec.md:229) — Structural proxy test with an explicit not-a-proof disclaimer — **PASS**

`ReentrancyCounterSoleReadGoesThroughVolatileRead` in
`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.Volatile.cs`, a partial-class
continuation of the type that already owns this counter's test surface, exactly as the spec directs.

Four assertions: `Volatile.Read(ref removespecificcontrolgroupcounter)` present; the bare
`if (removespecificcontrolgroupcounter >` form absent; both `Interlocked` call sites present; and
`volatile int removespecificcontrolgroupcounter` absent.

The disclaimer is explicit and unhedged, in `<remarks>` at `:67-76`:

> This assertion is a STRUCTURAL PROXY for the memory-ordering fix and is explicitly NOT a proof that
> the race is eliminated.

It goes on to explain why a deterministic test is impossible and why a thread-racing test would
violate the repository's determinism rule. This is exactly what the criterion asks for.

RED evidence at `evidence/regression-testing/finding4-volatile-fail-before.md`, `EXIT_CODE: 1`, with
the real assertion diagnostic.

### AC14 (spec.md:230) — Existing issue-#286 reentrancy tests pass unchanged — **PASS**

`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` shows a diff of exactly
1 insertion / 1 deletion, and that single change is `public class` → `public partial class`. No test
method, assertion, fixture or field-name constant is altered. Line count is 498 at both the
merge-base and HEAD.

`evidence/regression-testing/finding4-volatile-pass-after.md` records
`RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter` and
`RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter` passing, and the full
suite is green. This is the expected result — a memory-visibility fix changes no single-threaded
observable behaviour.

### AC15 (spec.md:231) — SetupDisposal coverage re-measured, recorded, compared against the #683 baseline, residual assigned to #683 — **PASS with a recorded evidence-kind deviation**

**Re-derived by this reviewer** from `coverage/baseline.cobertura.processed.xml` and
`coverage/postchange.cobertura.processed.xml` using a separator-anchored per-filename line map over
`./lines/line` and `./methods/method/lines/line` with max-hits de-duplication (the `.//line`
descendant axis was avoided, per the known Cobertura double-count trap):

| Measure | Issue-#683 baseline | Post-change | Change |
|---|---|---|---|
| Total measured lines | 157 | 182 | +25 |
| Covered lines | 111 | 136 | +25 |
| **Uncovered lines** | **46** | **46** | **0** |
| Whole-file line coverage | 70.70% | **74.73%** | +4.03 pts |

Every figure reproduces `evidence/qa-gates/setupdisposal-coverage.md` exactly.

Written comparison against the #683 baseline: present, in a table, at
`evidence/qa-gates/setupdisposal-coverage.md:36-41`. Residual assignment: present at `:47-49`.

**Critically, no artifact overstates this as closing the #683 gap.** The artifact states at `:45`:
"The improvement is therefore a side effect of covering the new code, not a reduction of the
pre-existing #683 gap. All 46 lines that were uncovered before this change remain uncovered after
it." I verified that claim arithmetically — the uncovered count is identical at 46 and the covered
count rose by exactly the number of added lines — and I found no place in `spec.md`, the plan, the
AC-traceability artifact or any evidence file that describes the movement as gap closure. The
criterion's own final sentence, that reaching any percentage is **not** a criterion, is respected
throughout.

**Recorded deviation.** The criterion names an `evidence/coverage` directory. That kind is not in the
canonical scheme, which `evidence-and-timestamp-conventions` declares non-overridable and which
recognises only `baseline/`, `regression-testing/`, `qa-gates/`, `issue-updates/`, `other/` and
`remediation-baseline/`. The artifact was therefore written to `evidence/qa-gates/` — the correct
kind for output of the mandatory final QA gate — and the substitution recorded as
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` (`evidence/qa-gates/ac-traceability.md:65`). The reviewer
concurs: writing to the spec's literal path would itself have been a policy violation.

### AC16 (spec.md:232) — New test files registered in the project file and present in the built assembly — **PASS with a recorded count deviation**

`QuickFiler.Test/QuickFiler.Test.csproj`, 3 insertions:

```
<Compile Include="Controllers\QfcCollectionControllerDefects468Tests.Volatile.cs" />
<Compile Include="Controllers\QfcMoveMonitorTopologyTests.cs" />
<Compile Include="Controllers\QfcFormControllerCleanupTests.cs" />
```

Presence in the built assembly is proven by execution, which is the strongest available evidence:
each of the three types appears in a filtered `/TestCaseFilter` run recorded in the
regression-testing artifacts, and all ten of their methods are inside the 6995-test full run. A file
absent from the assembly cannot be selected by a filter.

**Recorded deviation.** The criterion says "Both new test files", anticipating two. Three were
created and registered. The third,
`QfcCollectionControllerDefects468Tests.Volatile.cs`, exists because its host file sits at 498 lines,
two below the 500-line ceiling, and could not absorb the finding-4 proxy. Splitting into a
partial-class continuation is the correct response to the ceiling rather than an evasion of it, and
the deviation is disclosed at `evidence/qa-gates/ac-traceability.md:67`. The criterion's substance —
registration plus confirmed presence — is met for all three files.

### AC17 (spec.md:233) — Full toolchain passes in a single uninterrupted pass in the documented order — **PASS**

| Order | Gate | Command | Result | Artifact timestamp |
|---|---|---|---|---|
| 1 | Format | `dotnet tool run csharpier format .` | `EXIT_CODE: 0` | 14:27 |
| 1b | Format verify | `dotnet tool run csharpier check .` | `EXIT_CODE: 0`, 1577 files, 0 unformatted | 14:28 |
| 2 | Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m … /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | `EXIT_CODE: 0`, 0 warnings, 0 errors | 14:29 |
| 3 | Type check | `msbuild TaskMaster.sln /t:Rebuild /m … /p:TreatWarningsAsErrors=true` | `EXIT_CODE: 0`, 0 warnings, 0 errors | 14:30 |
| 4 | Tests + coverage | vstest console over 9 assemblies with `coverage.config` | `EXIT_CODE: 0`, 6995/6995 | 14:33 |

Order and monotonicity verified from the artifact timestamps; no restart is recorded and no source
edit falls inside the 14:27-14:33 window. Both msbuild gates correctly use `/t:Rebuild`, and the
nullable gate correctly omits `/p:Nullable=enable`, matching `.github/workflows/ci.yml` and
CLAUDE.md § C#1.

**Advisory attached, not a deduction.** A CSharpier probe mutated and restored `QfcQueue.cs` at
14:37-14:38, after the pass concluded, and only `csharpier check` was re-run. Recorded as policy
audit finding PA-3. I verified the restoration was faithful: the `QuickFiler` rows of the
pre-probe numstat capture (`coverage/p5t1-numstat-after.txt`, 14:27) are byte-identical to the live
numstat at HEAD, including `2  0  QuickFiler/Controllers/QfcQueue.cs`, and the 14:38 re-check reports
the same 1577 files with 0 unformatted. The mutation was confined to one comment and one blank line,
which cannot alter IL, analyzer diagnostics, nullable flow or test outcomes. The criterion's
substance holds.

### AC18 (spec.md:234) — No newly failing or newly skipped tests; no public API surface change — **PASS**

| Metric | Baseline | Post-change |
|---|---|---|
| Total | 6985 | 6995 |
| Passed | 6985 | 6995 |
| Failed | 0 | 0 |
| Skipped | 0 | 0 |
| Assemblies | 9 | 9, identical set |

Zero failed and zero skipped on both runs, so no test is newly failing and none is newly skipped.
The +10 delta equals the planned net-new count exactly; I recounted the `[TestMethod]` attributes in
the five touched test files and reproduce 2 + 7 + 1 + (1 − 1) = 10, which confirms no pre-existing
test was silently removed to make room.

The identical nine-assembly set closes the failure mode where a second run collects over a smaller
suite while still clearing a total-count bar.

Public API: the only signature change is `QfcRemainingQueueAdmission`'s constructor. The type is
`internal sealed` at `:8` and the constructor is `internal` at `:14`, so nothing public moved. No
other public member is added, removed or altered anywhere in the diff.

### AC19 (spec.md:235) — Excluded metrics files untouched; `QfcCollectionController` not split; its diff limited — **PASS**

**Excluded files.** `QfcHomeController.Metrics.cs` and `EfcHomeController.Metrics.cs` are absent from
my own independently generated full branch `--numstat`. Zero paths in the 54-path diff have a
filename ending in `Metrics.cs`. Verified against my listing, not the executor's.

**Not split.** `QfcCollectionController.cs` remains one file. No new file bearing that stem exists in
the diff.

**Diff bound.** The file's numstat row is `3  1`. Decomposed by reading the diff: one changed
statement (the `Volatile.Read` guard rewrite, 1 insertion + 1 deletion), one comment line
(1 insertion), and one blank line above that comment (1 insertion).

The blank line is formatter-mandated, and I verified this claim rather than accepting it. Two probe
logs the executor retained (`coverage/blankline-probe.log`, `blankline-probe2.log`) show
`dotnet tool run csharpier check` reporting `Was not formatted` with an `Expected: Around Line 40`
block containing the blank line — once for the plain comment form and again for the `///`
doc-comment form. The criterion's content bound, "one statement and one comment line," is exactly
what landed; the third insertion is whitespace the formatter requires and cannot be avoided without
a compensating deletion elsewhere.

Line count moved 2327 → 2329, which is not a split and not material growth. The file remains over the
repository's 500-line ceiling as pre-existing, spec-disclosed debt (policy audit finding PA-2).

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/spec.md
- Total AC items: 19
- Checked off (delivered): 19
- Remaining (unchecked): 0
- Items remaining: none
```

All 19 checkboxes at `spec.md:217-235` were already `[x]` at HEAD. This reviewer verified each one
independently and confirms every check-off is warranted. No checkbox was altered by this review; none
needed to be.

Three carry a disclosed deviation (AC15 evidence kind, AC16 file count, AC17 post-pass probe) and one
carries a scope note (AC11 delegate-type specificity). All four deviations were disclosed by the
executor before this review, are correctly reasoned, and none defeats the criterion's substance.

## Baseline Comparison

| Dimension | Merge-base `35583f7c` | HEAD `c55bfad2` | Assessment |
|---|---|---|---|
| Suite | 6985 / 6985 / 0 / 0 | 6995 / 6995 / 0 / 0 | Improved; delta exactly as planned |
| Repo-wide C# line coverage | 85.4194% | 85.4146% | −0.0048 pts, inside tool nondeterminism, above the 85% floor |
| Repo-wide C# branch coverage | 79.5094% | 79.5168% | Improved; above the 75% floor |
| `QfcFormController.SetupDisposal.cs` | 70.70%, 46 uncovered | 74.73%, 46 uncovered | Improved; pre-existing gap untouched and still owned by #683 |
| `QfcRemainingQueueAdmission.cs` | 92.00% | 100.00% | Improved |
| `QfcQueue.cs` | 50.32% | 50.32% | Unchanged (comment-only edit) |
| `EmailMoveMonitor.cs` | 44.03% | 44.03% | Unchanged (comment-only edit) |
| Analyzer warnings | 0 | 0 | Unchanged |
| Nullable errors | 0 | 0 | Unchanged |
| CSharpier unformatted files | 0 of 1574 | 0 of 1577 | Unchanged; +3 files checked |
| Unobserved undo-consumer fault | present | logged | Improved |
| Files over the 500-line ceiling among those touched | 2 (505, 2327) | 2 (507, 2329) | Marginally worse; pre-existing, non-goal, follow-up recorded |

## Verdict

**PASS — 19 of 19 acceptance criteria satisfied, 0 Blocking findings.**

The delivered change closes all four code findings of issue #731 and discharges the finding-5
evidence obligation. The two most consequential design decisions — not sharing the monitor, and not
marking the counter `volatile` — each reject the issue's first-listed option, and each rejection is
supported by evidence this reviewer re-verified against source and against the toolchain's own
constraints rather than accepting on assertion.

The three items the delegating prompt asked to be scrutinised rather than accepted were each
examined against the artifacts and ruled on independently:

1. **The changed-line gate observed nothing.** Confirmed, and the disclosure is adequate — stated in
   bold and repeated three times, with the explicit sentence that it "did not find that there was no
   regression." One correction to the prompt: neither `[P5-T5]` nor `[P5-T6]` actually deferred to
   that gate. `[P5-T5]` recorded `Absolute floor result: PASS` and `[P5-T6]` took Branch A, so both
   deferring branches went untaken and an admissible repository-wide comparison does exist and
   passed. The gap is narrower than described: a per-changed-line observation is unavailable, a
   repository-wide one is not.
2. **The coverage improvement is not gap closure.** Confirmed arithmetically — uncovered count
   identical at 46, covered count up by exactly the 25 added lines. No artifact, check-off or
   document anywhere in the feature folder overstates it. AC15's own text disclaims a percentage
   target.
3. **The `[ExcludeFromCodeCoverage]` policy tension.** Ruled **Not Blocking**, and specifically **not
   Blocking for this change**: it is a pre-existing repository condition introduced by commit
   `a564add0` (2026-06-13, `#197`) under CLAUDE.md's maintainer-ratified COM/VSTO exemption, which
   names the attribute mechanism explicitly. CLAUDE.md sits at authority level 1 and
   `.claude/rules/general-unit-test.md` at level 3; and the rules file's Blocking clause is textually
   scoped to coverage-config `exclude` glob entries, which a C# source attribute is not. It belongs
   in a separate documentation-reconciliation issue, **not** in issue #731's remediation loop. The
   full ruling, including the material consequence for finding 4's verifiability, is in policy audit
   finding PA-1.

Nine advisory findings are recorded across the policy audit and code review. None blocks merge. The
recommended pre-PR actions — regenerate the stale `pr_context` artifacts, promote the three declared
follow-ups, and correct one inaccurate clause in `file-size-audit.md` — are housekeeping, not
remediation.

No `remediation-inputs` artifact was produced, because no finding is Blocking.
