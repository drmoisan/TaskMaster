# Code Review — utilitiescs-test-determinism (Issue #811)

- Date: 2026-09-08T11-30
- Branch: `bug/utilitiescs-test-determinism-780-803-594-811`
- Head: `3805ca89f0c72e2bb448726695f18f420bd260f7`
- Base: `bb1c7d4b60f7b782227956f36859314d5c47bb03`
- Source under review: 20 changed `.cs` / `.csproj` files; 48 further documentation and evidence
  files reviewed by direct reading

Blocking findings in this artifact: **0**. (The single blocking finding for this branch is F-1 in
`policy-audit.2026-09-08T11-30.md`; it is a process gap, not a code defect.)

## 1. Overall Assessment

The change is well-scoped and the technique is correct for the problem class. Each of the three
defects is treated at the level where the nondeterminism originates rather than suppressed at the
test level: a guard with no consumer is deleted, wall-clock deadlines that must survive in
production are placed under an injectable `TimeProvider`, and a process-wide output dependency is
converted into an explicit parameter. Notably, the change removes an existing serialization stopgap
and an existing timing tolerance rather than adding new ones, which is the direction the repository
policy asks for.

The production risk profile is low. Every new parameter is optional and trailing; every default
reproduces prior behavior exactly (`null` `TimeProvider` resolves to the system clock, `null`
`TextWriter` resolves to `Console.Out`, `null` `etl` resolves to the extracted
`DefaultTableEtl`). The one intentional behavior change — an `InvalidOperationException` replacing
a `NullReferenceException` one statement later — is a strict diagnostic improvement on a path that
already failed.

## 2. Design and Architecture

### 2.1 Deleting the `TryAddValuesAsync` window — correct

`DictionaryExtensions.cs` now passes the caller's token straight to `Task.Run`. This reviewer
verified the two load-bearing claims independently rather than accepting them:

- `TryAddValuesAsync` has zero production call sites. A repository-wide search over `*.cs` returns
  exactly four hits: the definition at `DictionaryExtensions.cs:169` and three references in
  `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs` (two test method names and one
  invocation). No production caller can be affected.
- The token passed to `Task.Run` can only cancel work that has not started, so a window that
  cancels after 500 ms could only ever convert scheduling latency into a spurious
  `TaskCanceledException`. Deleting it removes a failure mode without removing a guarantee.

The change also removes a per-call undisposed linked `CancellationTokenSource` and its timer. The
replacement comment states the invariant (bounded compare-and-swap loop, no I/O, cannot hang)
rather than restating the code, which is the correct comment style.

### 2.2 `TimeProvider` seams — used substantively, not decoratively

This was checked specifically. The seams are load-bearing, not ornamental:

- `OlTableExtensions.Etl.cs` converts three `TimeoutAfter(ms, attempts)` calls to
  `TimeoutAfter(ms, timeProvider)`. The `attempts` local is deleted rather than left dangling, so
  the inert retry parameter is genuinely gone from this path.
- The seam is exercised by real behavior change in tests: `EtlAsync_DeadlineExpires_...` advances a
  fake clock by exactly 250 ms and observes the `catch (TimeoutException)` block execute, driving
  the `EtlAsync` method-span line rate from 0.7000 to 0.9796. A decorative seam would not have
  moved coverage into a previously unreachable catch block.
- `ArmingBarrierTimeProvider` is a genuine correctness requirement, not convenience.
  `FakeTimeProvider` arms timers relative to its current time, so advancing before the production
  code creates its timer arms that timer past an already-elapsed deadline and the awaiting test
  hangs. The barrier forwards `CreateTimer` to the inner provider and completes a
  `TaskCompletionSource` afterwards, so the test can `await barrier.Armed` before calling
  `Advance`. `TaskCreationOptions.RunContinuationsAsynchronously` is correct here: it prevents a
  timer-creating production thread from being hijacked to run the test continuation inline.
  `TrySetResult` rather than `SetResult` is correct because the production loop may arm one more
  timer than the test drives.
- `DfDeedleEtlTimeoutTests` demonstrates the mechanism is understood rather than copied: it uses
  two independent test-owned gates (`gateA` inside `Columns.Add("SentOn")`, `gateB` inside
  `GetNextRow`) plus an explicit `ReArm()` between them, so the identity of each armed timer is
  determined by the test rather than by a thread-pool race. Without that, the number of `Armed`
  signals preceding the ETL deadline would itself be nondeterministic.

Both gates are released in `finally`, so no orphaned `Task.Run` body outlives the test. That
matters in a `[DoNotParallelize]`-free assembly; leaked blocked pool threads would degrade
subsequent classes.

### 2.3 Delegate parameters replacing mutable statics — correct, and it narrows the public surface

Replacing `internal static Func<...> TableEtlInvoker` and `StoreTableEtlInvoker` with an optional
`etl` parameter and a `private static readonly DefaultTableEtl` is the right call. It is
preference 2 in the `.claude/rules/csharp.md` DI-seam order (injectable delegate for a single call
path), the default remains safe and deterministic, and it converts two mutable process-wide fields
into one immutable private one. The `Stores` overload correctly forwards `etl` to the `Store`
overload, so the injected delegate reaches the inner call.

Constructor injection — preference 1 — is unavailable because all touched members are `static`
extension or factory methods. A trailing optional parameter is the smallest seam that works. This
is a correct application of the rule rather than a shortcut around it.

### 2.4 The null-snapshot guard — correctly placed

The guard sits between the `EtlAsync` call and the `LogDfTiming` statement that dereferences
`tableSnapshot.Item1`, and therefore ahead of the pre-existing `ValidateRequiredEmailColumns`
guard, which inspects `Item2` only and would not have caught this. The message names the folder and
states the mechanism ("the table ETL timed out or was cancelled before returning any rows"), which
satisfies the "fail fast and explicitly" requirement and gives the `QfcDatamodel.FrameBuilding.cs`
boundary something attributable to log.

One observation, not a defect: the guard tests `tableSnapshot.data is null` while the two lines
below still read `tableSnapshot.Item1` / `.Item2`. Mixing the named and positional forms of the
same tuple within four lines is slightly inconsistent. The positional reads are pre-existing lines
and switching them would have added diff noise, so the choice is defensible, but a future edit
should unify on the named form.

### 2.5 `TextWriter` seams — correct, with one asymmetry

`PrintTree`, both `PrettyPrint` overloads, `EnumerateTable` and the new `GFG.Run` all take the
writer as a parameter and resolve `null` to `Console.Out`. `EnumerateTable` hoists the resolution
into a local `target` once rather than repeating `(writer ?? Console.Out)` three times, which is
the better form; `PrintTree` repeats the expression once per call and also forwards the original
nullable `writer` on recursion, which is correct (resolving at each level would be equivalent but
allocates nothing either way). The asymmetry is cosmetic.

`GFG.Main` keeping its `String[] args` parameter while delegating to `Run(Console.Out)` preserves
the existing entry-point shape. Reasonable.

## 3. Test Quality

### 3.1 Strengths

- Assertions are specific. `ThrowAsync<InvalidOperationException>().WithMessage("*Inbox*")` will
  fail if the code regresses to `NullReferenceException` or drops the folder name, which is exactly
  what a generic `Should().Throw<Exception>()` would have missed.
- `MockBehavior.Strict` is used on `Row`, `Table`, `Column`, `TableView` and `Explorer` in
  `DfDeedleEtlTimeoutTests`, so an unanticipated production call fails the test rather than
  returning a silent default.
- `SilentProgressTracker` overrides all three `Report` members to no-ops, removing progress
  reporting as a timing or output variable. This is a deliberate isolation choice, not incidental.
- The green-path tests use an un-advanced `FakeTimeProvider` rather than a widened deadline. This is
  the distinction AC5 turns on and the change gets it right: the deadline is not made larger, it is
  made unreachable by a clock the test owns.
- The retired `Returns(120)` tolerance is verified gone from the final tree, not merely absent from
  the added lines. Checking the final state as well as the diff is the correct verification shape.

### 3.2 Findings

**C-1 (Minor).** `OlTableExtensions_Tests.EnumerateTable_WritesFormattedOutputToSuppliedWriterAndMovesToStart`
performs two Acts: it asserts on the supplied-writer path, then invokes the null-writer path with
`FluentActions.Invoking(...).Should().NotThrow()` before the final `Verify`. The in-code comment
states the reason ("this file is at its line ceiling"). This trades test isolation for compliance
with the 500-line cap on an already-1846-line file. The trade is understandable but the better
resolution is a small new test file, which would satisfy both constraints. Recommend as follow-up.

**C-2 (Minor).** `Main_RunsSampleScenarioWithoutThrowing` is now a bare `NotThrow` assertion. It
does still cover the `Main` to `Run(Console.Out)` default branch, and the substantive assertions
moved to `Run_WritesScenarioToSuppliedWriter`, so the pair is complete. But the surviving test's
name promises "runs sample scenario" while asserting only absence of an exception. The doc comment
above it states this accurately, so the intent is documented; the name is slightly stronger than
the assertion.

**C-3 (Minor).** `PrettyPrint_NullWriter_WritesToConsoleWithoutThrowing` and the null-writer half
of C-1 write to the real `Console.Out` during a now-parallel-eligible run. This is harmless because
after this change no test captures `Console.Out` — but that safety is a property of the whole
assembly, not of these tests, and nothing enforces it. If a future test reintroduces a
`Console.Out` capture, these two null-writer paths become new noise sources. A one-line note in the
class header of each would make the dependency explicit.

**C-4 (Observation).** `DfDeedleEtlTimeoutTests` and `OlTableExtensionsEtlClockTests` both carry
`[DoNotParallelize]` with a stated reason ("drives a real `Task.Run` gate"). That is the correct
default for gate-driving classes and it is consistent with the pre-existing
`DfDeedleQfcColumnTimeoutTests`. Net effect on the serialized set: minus three classes (attributes
removed) plus two classes (new gate-driving classes), so the serialized set shrinks by one.

**C-5 (Observation).** `DfDeedle_COM_Tests` still swaps `DfDeedle.MessageBoxInvoker` in four tests.
The class header now asserts it "has no reader on any parallel-phase path". This reviewer verified
that claim: the only production readers are `DfDeedle.QfcColumns.cs:27,170,194`, reached by exactly
three test classes, of which two are `[DoNotParallelize]` and the third is `DfDeedle_COM_Tests`
itself (serialised internally by `ClassLevel` scope). The claim holds today. It is not enforced by
any test or analyzer, and it rests on the MSTest phase-ordering assumption that `spec.md` Risk 3
explicitly labels documentation-sourced and not run-verified — the same assumption the AC3 seam
work was intended to stop relying on. Recommend the class header state the dependency ("this holds
only while both reader classes remain `[DoNotParallelize]`") so a future edit cannot silently break
it.

## 4. Directed Assessment: the three removed `[DoNotParallelize]` attributes

The reviewer was asked to reach an independent verdict on three specific questions. The findings
below are this reviewer's own; they do not adopt the executor's classification.

### 4.1 Is removing the three attributes a legitimate consequence of AC3, or unnecessary widening?

**Legitimate, and required by the criterion's own scope definition.**

`spec.md` line 55 defines the AC3 work as: "introduce a `TextWriter` seam on the four production
members whose output is captured by tests, convert those tests to their own writer, **and remove the
`[DoNotParallelize]` serialization stopgap that currently suppresses the race**." The removal is not
an incidental side effect; it is the deliverable. `spec.md` line 62 gives the reason: with the
attribute in place, the AC4 gate would exercise the stopgap rather than the seam, and the seam would
be unproven. That reasoning is sound. A fix that leaves the suppressant in place is untestable by
construction.

Three of four attributes were removed. The fourth, on `OlTableExtensions_Tests`, was deliberately
retained under the conservative fallback that `spec.md` Mitigation 4 pre-authorised, and its comment
was rewritten to stop claiming the console as its reason ("The console reason for this attribute was
removed by the `TextWriter` seam under #811. It is retained because this class has not been soaked
under class-level parallelism and ten of its tests drive the 2000 ms `GetTableInViewAsync` window").
That is the correct judgment: the class is 1846 lines of COM mocks with a documented untested
parallel-safety profile, and the comment now states a true reason rather than a stale one. Retaining
it is not an inconsistency; it is the risk-weighted subset.

Scope check: each of the three removals is confined to the file that owned the captured output, and
each is paired with the seam conversion in the same file. No unrelated class lost an attribute. This
is not widening.

### 4.2 Can raising parallel density reasonably be judged to have increased the surfacing probability of the `ILGlobals` race?

**No — not on any evidence available, and the mechanism argues against it. The claim is not
supportable in either direction beyond "scheduling was perturbed".**

The reasoning, which this reviewer derived rather than inherited:

1. **The instantaneous concurrency is capped by the worker count, not by the class count.**
   `AssemblyInfo.cs:18-21` sets `Workers = 0`, which resolved to 24 on the run host. During the
   parallel phase, at most 24 classes execute at once regardless of how many are eligible.

2. **The eligible pool is already an order of magnitude larger than the cap.** A search for
   `[TestClass]` across `UtilitiesCS.Test` returns more than 400 files, against roughly 35 classes
   carrying `[DoNotParallelize]`. The parallel phase is worker-saturated throughout; it is not
   starved for runnable classes. Adding three more classes to a pool of ~400 does not raise the
   number of classes executing concurrently, because that number was already pinned at 24.

3. **What actually changes is the dispatch order and the parallel-phase duration.** Three short,
   purely in-memory classes (a stack sample, a DataFrame pretty-printer, a DASL string parser) move
   from the sequential tail into the saturated pool. Their combined runtime is a negligible fraction
   of a 51-to-74-second run over 7162 tests. The perturbation to when `ILGlobals_Tests` and
   `MethodBodyReader_Tests` happen to be co-scheduled is real but undirected: it can as easily
   reduce the overlap as increase it. There is no argument, and no measurement in the evidence, that
   establishes a direction.

4. **The race's participants and window are untouched.** Both racing classes were parallel-eligible
   before this change and are parallel-eligible after. Neither is in the write set. Neither gained
   nor lost an attribute. The vulnerable window — the interval in `LoadOpCodes()` between the
   reassignment at line 123 and the completion of the fill loop at line 137 — is byte-identical.

5. **Counter-consideration, stated fairly.** The three moved classes do consume worker slots they
   previously did not, which marginally extends the saturated window. If one insists on a
   directional claim, that is the only mechanism available, and its magnitude is on the order of
   three short classes out of more than four hundred — well under one percent, against a race that
   was already surfacing at an unquantified base rate. The evidence does not measure the base rate:
   the two baseline runs (P0-T10, P0-T12) did not exhibit it, but two single runs of an
   intermittent failure establish nothing about its frequency. Attributing run 7 to this change is
   therefore speculation, and the executor's own hedged wording ("can therefore alter the
   probability that any latent race surfaces") is the most that the evidence supports.

**Verdict: the AC4 failure is wholly attributable to a pre-existing defect. This change did not
create it, did not make its participants newly concurrent, and did not measurably increase its
exposure. What this change did do is bring the defect to light by running the suite ten times,
which is the gate working as designed.**

### 4.3 Blocking or Non-blocking for the AC4 shortfall?

**Non-blocking**, for the following reasons, weighed against the strongest contrary argument.

The strongest argument for Blocking is that the item's stated purpose (`spec.md` Context) is "so the
required `mstest-coverage` check stops failing on unrelated pull requests", and with the `ILGlobals`
race unfixed the check can still fail intermittently. On that reading the business objective is not
achieved and the branch should not merge.

That argument does not survive scrutiny for three reasons:

1. **The branch is a strict improvement, and blocking it makes the stated problem worse, not
   better.** Three failure modes are removed with strong evidence — 130 of 130 tracked assertions
   `Passed` across ten runs, including both intermittent-failure sentinels. Holding the fix hostage
   to a fourth, unrelated defect leaves four failure modes in `main` instead of one.

2. **Fixing `ILGlobals` here would violate the repository's own bugfix discipline.** CLAUDE.md's
   Bugfix Workflow requires the minimal targeted fix and states: "If you uncover deeper design
   problems, open a new issue instead of widening scope." The `ILGlobals` fix touches two production
   files and two test classes that are outside the declared 20-path write set, and doing it properly
   would need its own RED-first regression test for a race that is not deterministically
   reproducible. That is a separate item.

3. **The shortfall is reported honestly and no re-run-until-green was attempted.** AC4 is left
   unchecked in `spec.md`, the ten-run artifact states "AC4 IS NOT SATISFIED" in its verdict line,
   and the issue-update artifact records 4 of 5 delivered against a plan that predicted 5 of 5,
   explicitly refusing to write the predicted value. Given that AC4 exists precisely to stop
   re-run-until-green, an executor that re-ran until green would deserve a blocking finding; one
   that reported the failure does not.

**What is Blocking is the disposal of the finding, not the finding itself.** The `ILGlobals` race
exists only as prose inside a feature folder that is archived at merge, with no
`docs/features/potential/` entry and no issue. The executor's stated reason — "authoring it was not
in this plan's write set" — is contradicted by the same plan task having authored two other
`docs/features/potential/` entries. That is finding F-1 in `policy-audit.2026-09-08T11-30.md` and it
is the one item that must be closed before merge. It costs one file.

## 5. Findings Summary

| ID | Severity | Summary | Location |
|---|---|---|---|
| C-1 | Minor | One test performs two Acts to respect a 500-line ceiling | `OlTableExtensions_Tests.cs`, `EnumerateTable_WritesFormattedOutputToSuppliedWriterAndMovesToStart` |
| C-2 | Minor | Test name is stronger than its surviving assertion | `StackGeek_Tests.Main_RunsSampleScenarioWithoutThrowing` |
| C-3 | Minor | Two null-writer tests write to real `Console.Out` under an unenforced assembly-wide assumption | `PrettyPrint_Tests`, `OlTableExtensions_Tests` |
| C-4 | Observation | New gate-driving classes correctly `[DoNotParallelize]`; serialized set shrinks by one | `DfDeedleEtlTimeoutTests.cs`, `OlTableExtensionsEtlClockTests.cs` |
| C-5 | Observation | `MessageBoxInvoker` safety claim is true but unenforced and rests on an unverified ordering assumption | `DfDeedle_COM_Tests.cs` header, `DfDeedle.cs:60` |
| C-6 | Observation | Named and positional tuple access mixed within four lines | `DfDeedle.cs`, null-snapshot guard and `LogDfTiming` |

No finding in this artifact is Blocking. None requires a code change before merge.

## 6. Recommendations

1. Close F-1 from the policy audit: file the `ILGlobals` race as a
   `docs/features/potential/` entry and promote it to an issue. Candidate fixes, in order of
   preference: initialise both tables in a static constructor or via `Lazy<OpCode[]>` so they are
   published once and immutably; or take a `lock` inside `LoadOpCodes()` and around the reads; or,
   as an interim stopgap only, add `[DoNotParallelize]` to `ILGlobals_Tests` and
   `MethodBodyReader_Tests`. The first is preferred because `LoadOpCodes()` is idempotent and the
   tables are conceptually constant.
2. Split the two Acts in C-1 into a separate small test file, which also reduces a file that is
   1346 lines over the cap.
3. Add the enforcement caveat described in C-5 to the `DfDeedle_COM_Tests` class header.
4. Consider following the `spec.md` rollout list item to soak `OlTableExtensions_Tests` without
   `[DoNotParallelize]` as separate work, now that its console reason is genuinely gone.
