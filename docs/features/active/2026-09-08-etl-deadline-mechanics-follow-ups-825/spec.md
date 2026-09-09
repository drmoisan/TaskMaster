# 2026-09-08-etl-deadline-mechanics-follow-ups (Spec)

- **Issue:** #825
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-09
- **Status:** Ready for planning
- **Version:** 1.0

> **Work mode is `full-bug`.** This document is the sole authoritative acceptance-criteria source
> for issue #825. No user-story.md exists for this feature and none is to be created; the AC
> check-off protocol in .claude/skills/acceptance-criteria-tracking/SKILL.md reads this file only.

> **Path formatting is deliberate — do not "fix" it.** The Write Set section below is the only
> place in this document that uses Markdown inline code spans for repository file paths. Everywhere
> else, file names and line citations are written as plain prose (for example: OlTableExtensions.Etl.cs
> line 84). A downstream tool derives this feature's change footprint by harvesting backtick-delimited
> path tokens, so adding backticks to a path outside the Write Set widens the apparent blast radius
> and can block a sibling feature from scheduling. Inline code spans around C# identifiers
> (`EtlAsync`, `TimeProvider`, `timeoutMs`) are not paths and are unaffected.

> **Evidence location.** Every artifact produced while delivering this feature (coverage baselines,
> fail-before records, build logs, QA gates) is written under
> docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/&lt;kind&gt;/ per
> .claude/skills/evidence-and-timestamp-conventions/SKILL.md. Those paths are deliberately not
> backticked: generated evidence is not part of the change footprint.

## Write Set

Files this feature creates or modifies. This list is the change footprint.

- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`
- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs`
- `UtilitiesCS/Threading/TimeOutTask.cs`
- `UtilitiesCS/Extensions/DfDeedle.cs`
- `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`
- `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs`
- `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs` (new file)
- `UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs`
- `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs` (timer-ordering update only; added 2026-09-09, see AC6)
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

UtilitiesCS/UtilitiesCS.csproj is **not** in the write set: this feature adds no new production
source file, and the legacy project's explicit compile-item list therefore needs no new entry.

## Context

Residual ETL deadline mechanics left in place by issue #811, which placed the 250 ms per-row ETL
deadline and the 1000 ms dataframe-transform deadline under an injectable `TimeProvider` and added a
descriptive guard for the null-snapshot path. Each item below was deliberately excluded from #811 to
keep that fix's blast radius to a bugfix. None is a regression introduced by #811.

Environment:
- OS/version: Windows 11 Pro 10.0.26200 locally; GitHub Actions windows runner in CI
- Runtime: .NET Framework 4.8.1 test host, VSTest 18.9.0, MSTest 4.4.0, class-level parallelism
- Command/flags used: `vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation`
- Data source or fixture: mocked `Table`, `MAPIFolder` and `Explorer` objects; no live Outlook

Impact / Severity:
- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low: none of these blocks a check today. Items 1 and 2 are latent robustness risks on slow stores;
items 3 to 6 are maintainability and clarity debt.

### Authority of the research record

The research artifact
docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/research/2026-09-08T23-55-etl-deadline-mechanics-research.md
re-derived every line citation in the current tree and corrects the issue text in several places.
**Where the issue and the research disagree, the research wins**, and the corrected statement is the
one recorded here. The corrections adopted are:

1. **The factory seam is not threaded from `DfDeedle`.** The issue states the 2000 ms window "is
   reached from `DfDeedle.GetEmailDataInViewAsync` and is seamed by a
   `Func<int, CancellationTokenSource>` factory". The first half is true; the second is false.
   DfDeedle.cs line 148 calls `GetTableInViewAsync(token, 0)` with two arguments, and
   `GetEmailDataInViewAsync` (declared DfDeedle.cs lines 128-136) has no parameter able to carry a
   factory. The factory seam is unreachable from the production path.
2. **`TimeProviderTaskExtensions.CreateCancellationTokenSource(TimeProvider, TimeSpan)` is present**
   in `Microsoft.Bcl.TimeProvider` 10.0.11, the version referenced by both projects, consumed from
   the package's lib/net462 folder per the hint path in UtilitiesCS.csproj line 97. The member is
   declared at line 199 of the Microsoft.Bcl.TimeProvider.xml documentation file shipped beside that
   assembly. The
   research read the shipped XML documentation file, **not the IL**; see the precondition in
   Proposed Fix.
3. **`EtlAsync` has five consumers, not "every consumer"** — one production (DfDeedle.cs line 172)
   and four test. The issue's blast-radius argument for deferring the contract change overstated it.
4. **Only one test arms the real 2000 ms window**, not ten. Four tests in `OlTableExtensions_Tests`
   reference `GetTableInViewAsync` (lines 1238, 1267, 1324, 1646); of those only line 1646 arms a
   real `new CancellationTokenSource(2000)`. The "ten" claim appears both in the issue and in the
   class comment at OlTableExtensions_Tests.cs lines 18-20; both are wrong.
5. **A `LogTableTiming` call sits between the swallow and the suppressed return** in `EtlAsync`
   (Etl.cs lines 127-130). It is null-safe on the timeout path, but any edit to that region must
   account for it.

### An additional production defect the issue does not name

OlTableExtensions.TableAccess.cs line 106 hard-codes the literal `2000` on the `TimeoutException`
retry branch instead of propagating the caller's `timeoutMs`, while the `TaskCanceledException`
branch at line 85 does propagate it. The comment at lines 100-102 records this as deliberate
preservation of prior behaviour. Making the retry honour `timeoutMs` is what actually puts that
deadline under caller and test control, and it is deterministically observable through the existing
factory seam. It is in scope for item 2.

## Repro & Evidence

These are design residuals rather than reproducible failures. The reproduction procedure is to read
the cited lines; only item 2's slow-store behaviour has an observable runtime symptom, and it
requires a real Outlook store slow enough to exceed the deadline.

Expected:
1. The per-row ETL budget is either justified by measurement or explicitly recorded as unmeasured,
   with the capture route named.
2. Every wall-clock deadline on the `GetEmailDataInViewAsync` path is under test control, including
   the deadline used by the retry.
3. `EtlAsync`'s declared return type admits the value it can actually return; no null is forced
   through a null-forgiving suppression.
4. Dead code and stale documentation do not outlive the thing they describe.
5. `[DoNotParallelize]` is present only where a documented, verified reason requires it.

Actual (as corrected by the research):
1. The budget `250 * rowCount` is computed at OlTableExtensions.Etl.cs line 84 inside `EtlAsync`
   (live) and at line 149 inside `EtlAsyncOld` (dead). In the by-row branch the value is applied
   twice independently (Etl.cs lines 247 and 261), so a one-row folder gets 250 ms per hop rather
   than 250 ms in total. The number rests on no recorded measurement.
2. `GetTableInViewAsync` (OlTableExtensions.TableAccess.cs lines 32-119) defaults `timeoutMs` to
   2000 on line 36 and, absent a supplied factory, arms the deadline with
   `new CancellationTokenSource(ms)` at TimeOutTask.cs lines 52-54 — the system clock. The
   production caller supplies neither `timeoutMs` nor a factory, so the deadline is not under test
   control from the production path, and the retry at TableAccess.cs line 106 is pinned to a literal.
3. On deadline expiry `EtlAsync` swallows the `TimeoutException` (Etl.cs lines 119-125), calls
   `tokenSource.Cancel()`, and returns `(data!, columnDictionary)` at line 131 with `data` null,
   forcing a null through a null-forgiving suppression into a non-nullable tuple element.
   `EtlAsyncOld` already declares the nullable shape (Etl.cs lines 134-137), so the corrected shape
   has an in-repo precedent.
4. The `(int, int)` `TimeoutAfter` overloads at TimeOutTask.cs lines 824-849 and 924-940 wrap a call
   to the proxy-returning `(int, TimeProvider?)` overload in a `catch (TimeoutException)` that can
   never execute. `repeatAttempts` is never consulted and the "attempts remaining" log line is
   unreachable. Also recorded as Finding 1 of issue #798's evidence/other/followup-promotions.md.
5. DfDeedle.QfcColumns.cs line 96 names `TableEtlInvoker` in the present tense. No declaration of
   that member exists anywhere in the solution.
6. `[DoNotParallelize]` is retained on `OlTableExtensions_Tests` (line 21) with a comment (lines
   18-20) whose stated reason is factually wrong, and `TimeOutTask_Tests` (line 10) carries the
   attribute with no recorded reason at all.

## Scope & Non-Goals

### In scope

- **Item 1** — record why the 250 ms per-row budget is unchanged, and the recipe that would produce
  the measurement. Production edit: comment only, in OlTableExtensions.Etl.cs.
- **Item 2** — thread `TimeProvider` from `DfDeedle.GetEmailDataInViewAsync` into
  `GetTableInViewAsync` and make the `TimeoutException` retry honour the caller's `timeoutMs`.
  Files: OlTableExtensions.TableAccess.cs, DfDeedle.cs, plus tests.
- **Item 3** — change the `EtlAsync` tuple's first element to `object[,]?` and delete the
  null-forgiving suppression. Files: OlTableExtensions.Etl.cs, plus a test doc comment.
- **Item 4** — delete the two inert `(int, int)` `TimeoutAfter` overloads, `EtlAsyncOld`, its single
  test, and the two test callers of the overloads. Files: TimeOutTask.cs, OlTableExtensions.Etl.cs,
  OlTableExtensions_Tests.cs, TimeOutTask_Tests.cs.
- **Item 5** — correct the stale doc comment at DfDeedle.QfcColumns.cs line 96.
- **Item 6** — correct the `OlTableExtensions_Tests` class comment and remove its
  `[DoNotParallelize]` after item 2 lands, in this same feature; supply a documented, verified reason
  for the attribute on `TimeOutTask_Tests`.
- New regression tests in a new test file, with its compile item added to the test project.

### Non-goals

The paths in this section are deliberately written without backticks; see the banner at the top of
this document. Do not add code spans to them.

- **Changing the value of the 250 ms per-row budget.** No measurement is obtainable in this
  environment, and substituting a differently-guessed constant would replace one unjustified number
  with another. Explicitly excluded.
- **Resolving the TimeOutTask.cs 500-line cap violation.** Deleting the two overloads removes 43
  lines (1011 to 968). The file remains far over the repository cap. Splitting it is a separate
  change with its own review surface and is not attempted here.
- **Rethrowing the `TimeoutException` from `EtlAsync`.** Considered and rejected; see Proposed Fix,
  item 3.
- **Any soak run.** A soak is not used as evidence for anything in this feature; see Proposed Fix,
  item 6.
- **Retiring the `timeoutSourceFactory` parameter.** It stays, unchanged, for source compatibility
  with the direct-caller test that uses it.
- **Sibling-owned files.** These are off limits and must not appear in this feature's diff:
  QuickFiler/Controllers/QfcItemController.FolderHandling.cs (813);
  QuickFiler/Controllers/QfcHomeController.cs, UtilitiesCS/Threading/ProgressViewer.cs and their
  tests (821); UtilitiesCS/OutlookObjects/Store/StoreWrapperController*.cs,
  QuickFiler/Viewers/Breadcrumb* (823); UtilitiesCS/NewtonsoftHelpers/SDIL Reader/** and
  UtilitiesCS.Test/Properties/AssemblyInfo.cs (824);
  UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs (817); .editorconfig,
  BannedSymbols.txt, the two Console.WriteLine diagnostics described below, and every Console.SetOut
  restore in test classes (826).
- **Two of the three tests that reach the 2000 ms window through DfDeedle.** UtilitiesCS.Test/
  Extensions/DfDeedleEtlTimeoutTests.cs and UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs already
  supply a `FakeTimeProvider` to `GetEmailDataInViewAsync` (DfDeedleEtlTimeoutTests.cs lines 153 and
  199, DfDeedle_COM_Tests.cs line 478). Once DfDeedle forwards that provider,
  DfDeedleEtlTimeoutTests.cs line 187 and DfDeedle_COM_Tests.cs line 473 become deterministic with no
  edit, because neither counts arming signals. Both must stay out of the diff, and DfDeedle_COM_Tests.cs
  must not appear in the diff at all.
  **Amended 2026-09-09.** The test at DfDeedleEtlTimeoutTests.cs line 135 is the exception and does
  need an edit; it is no longer covered by this non-goal. That test drives the call with an
  `ArmingBarrierTimeProvider` (constructed at line 141, passed at line 153) and consumes arming
  signals in a fixed order, awaiting at line 158, re-arming at line 159, awaiting again at line 164
  and advancing 250 ms at line 165. Threading the provider into `GetTableInViewAsync` makes the
  table-acquisition deadline create a timer on that same barrier — table acquisition at DfDeedle.cs
  line 148 runs before AddQfcColumnsAsync at line 168 and EtlAsync at line 172 — so the acquisition
  timer becomes the first signal and every later expectation shifts by one. The barrier's `Armed`
  latch (UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs line 47 signals on every
  `CreateTimer`) drops a signal whenever two timers arm inside one await window, so the ordering
  cannot be recovered by adding awaits alone: one test-owned gate per timer is required. The edit is
  therefore a deliberate, scoped timer-ordering update, listed in the Write Set and bounded by AC6.
- **Rewriting historical records.** The past-tense mention of the removed static in
  UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs line 212 is historically accurate and needs
  no change; every occurrence under docs/features/** is a historical record and must not be
  rewritten.

### Ownership boundary with sibling feature 826

`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` is shared with sibling feature
826, which executes in wave 1 after this feature merges.

- **Feature 826 owns** the two diagnostics `Console.WriteLine($"Task timed out on try {counter}");`
  at lines 79 and 97 of that file.
- **This feature owns** the deadline window and the timeout mechanics of `GetTableInViewAsync`
  (lines 32-119).
- Both `Console.WriteLine` statements must be left **byte-identical, including leading indentation**
  (line 79 is indented 20 spaces; line 97 is indented 16). Any reflow of the enclosing blocks would
  alter them and would collide with 826's change.
- **Corollary constraint:** this feature must preserve the `counter` variable and both `catch`
  blocks (`catch (TaskCanceledException)` opened at line 71 and `catch (TimeoutException)` opened at
  line 95). Feature 826's change depends on all three existing.
- **The survival of these two lines in the diff is deliberate.** A reviewer must not read
  unmodified `Console.WriteLine` diagnostics in a file this feature otherwise edits as an oversight
  or an incomplete cleanup. They are out of this feature's ownership.

Cross-feature interaction, recorded as information and **not** a blocker: feature 826 will extend
BannedSymbols.txt to cover `TimeoutAfter` and `new CancellationTokenSource(int)`. Deleting the inert
overloads in item 4 reduces the surface 826 must ban. No sequencing dependency is created in either
direction beyond this feature merging first.

## Root Cause Analysis

Items 1 to 3 are consequences of a deadline design that predates the `TimeProvider` seam: deadlines
were expressed as integer millisecond budgets handed to `new CancellationTokenSource(int)` and to
timer-armed proxies, with no way for a caller to substitute a clock. #811 introduced the seam and
adopted it at two of the four deadlines on the `GetEmailDataInViewAsync` path; item 2 is the
remaining gap and item 3 is the return-shape consequence of a deadline that must report failure
without an exception. Items 4 and 5 are residue from that seam's incremental adoption across #798
and #811: the `(int, int)` overloads were superseded by `(int, TimeProvider?)` and the
`TableEtlInvoker` static was superseded by a default-delegate plus optional-parameter pair, but
neither predecessor was removed. Item 6 is a conservative choice made without the evidence that
would have shown the stated reason to be wrong.

## Proposed Fix

### The invariant this change establishes

**After this change, every wall-clock deadline reachable from `DfDeedle.GetEmailDataInViewAsync` —
including the table-acquisition deadline in `GetTableInViewAsync` and both of its retry attempts —
is armed on the `TimeProvider` the caller supplied, is measured against the caller's `timeoutMs`
rather than against any literal, and `EtlAsync`'s declared return type admits the null it can
return, so no null-forgiving suppression exists anywhere on that path.**

### Trace of one accepted value

Take `timeoutMs = 750` supplied by a direct caller (or, equivalently, a `FakeTimeProvider` supplied
by `DfDeedle`). The path below has no guard anywhere between the accept point and the point where
the caller's value is discarded.

1. **Accept point.** `GetTableInViewAsync` (TableAccess.cs lines 32-38) accepts `timeoutMs` and
   validates nothing about it — no range check, no null check, no clock check. It forwards the value
   to `TimeOutTask.RunWithTimeout` at lines 57-64.
2. **Throw point.** `RunWithTimeout` converts the value into a deadline source at TimeOutTask.cs
   lines 52-54 (`timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))`), links it with the
   caller's token at lines 55-58, and runs the work at line 63. When the deadline expires the linked
   token cancels and the awaited `Task.Run` faults. The factory itself is invoked at line 52,
   **outside** the `try` opened at line 61, so an exception originating there escapes `RunWithTimeout`
   uncaught.
3. **Current absorption point.** For the ordinary expiry, TimeOutTask.cs lines 65-83 absorb the
   `TaskCanceledException`; with `strict: false` (TableAccess.cs line 62) and `maxAttempts: 1`, lines
   85-92 absorb anything else and the method returns `default`. Where an exception does escape and
   reaches TableAccess.cs line 95, the `catch (TimeoutException)` re-enters `GetTableInViewAsync` at
   lines 103-108 passing the **literal 2000** at line 106. The caller's 750 is discarded there. That
   location cannot report the caller's intent because it no longer has it: the value was dropped one
   line earlier, and the second attempt then runs on a deadline neither the caller nor any test can
   observe or control.
4. **Where the fix acts.** Line 106 passes `timeoutMs`, and the deadline source produced at
   TimeOutTask.cs lines 52-54 is now built from the caller's `TimeProvider` when no explicit factory
   is supplied. Both attempts are then governed by the same caller-visible value on the same
   caller-supplied clock, and a test can assert the value the second attempt used.

**Why neither half suffices alone.** Threading the `TimeProvider` without fixing line 106 leaves the
retry armed on a literal that no caller supplied — the injected clock would govern attempt one and a
hard-coded 2000 ms would govern attempt two, which is a worse state than today because it looks
controlled. Fixing line 106 without threading the `TimeProvider` leaves both attempts on the system
clock, so the value is consistent but still unobservable and still wall-clock dependent.

**Catches that must NOT be widened.** An implementer must not "make it safe" by broadening any of
these:
- `catch (TimeoutException)` in `EtlAsync` (Etl.cs lines 119-125) keeps swallowing and keeps calling
  `tokenSource.Cancel()`. The live test
  `OlTableExtensionsEtlClockTests.EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource`
  (line 105) pins exactly this behaviour with `data.Should().BeNull()` at line 144 and the
  cancellation assertion at line 145; both assertions stay.
- The consumer guard in `DfDeedle` (`if (tableSnapshot.data is null)` at line 182, throwing
  `InvalidOperationException` at lines 184-188) is retained verbatim. It must not be converted into
  a `catch`.
- `catch (System.Exception e)` in `RunWithTimeout` (TimeOutTask.cs lines 85-92) is neither narrowed
  nor widened, and `strict` stays `false` at TableAccess.cs line 62.
- Neither `catch` block in `GetTableInViewAsync` may be removed (see the ownership boundary above).

### Design summary (what changes where)

**Item 1 — 250 ms per-row budget: unchanged, with the reason recorded.** The expression
`250 * rowCount` at OlTableExtensions.Etl.cs line 84 keeps its value. A comment at that site records
three facts: (a) the value rests on no recorded measurement; (b) no measurement is obtainable in this
environment — there is no benchmark harness in any project file, no ETL duration in the 99 evidence
files of #798 and #811, and the fixtures are Moq objects whose `GetRowCount`, `GetNextRow` and
`GetArray` return in microseconds, so anything measured against them would characterise Moq rather
than COM; (c) the capture route that would produce the measurement in a live Outlook session already
exists and is enabled — `LogTableTiming` emits `rowCount`, `columnCount` and `elapsedMs` on the
"EtlAsync complete" line at Etl.cs lines 127-130, and TaskMaster/log4net.config line 4 sets
`<level value="ALL" />` on the root logger, so a live add-in run already records the pairing in
logs\debug_yyyy-MM-dd.log. Collecting those lines across folders of differing `rowCount` is the
recipe; it is outside this repository's automated environment. Substituting another guessed constant
is out of scope. Note that the second occurrence of the constant, at Etl.cs line 149, disappears with
the item 4 deletion of `EtlAsyncOld`, so after this feature only the live occurrence remains.

**Item 2 — unify on `TimeProvider` (option b), and fix the retry literal.**

- `GetTableInViewAsync` gains a **trailing optional** parameter `TimeProvider? timeProvider = null`,
  placed after `timeoutSourceFactory`.
- Inside the method the deadline source is resolved once:
  the supplied `timeoutSourceFactory` when non-null, otherwise a factory derived from the clock —
  `ms => (timeProvider ?? TimeProvider.System).CreateCancellationTokenSource(TimeSpan.FromMilliseconds(ms))`.
  That resolved factory is what is passed to `RunWithTimeout` and to both recursive retries. An
  explicitly supplied factory therefore still wins, which keeps the existing direct-caller test
  working unchanged.
- The `TimeoutException` retry at TableAccess.cs lines 103-108 passes `timeoutMs` instead of the
  literal `2000`, and the rationale comment at lines 100-102 is replaced with one that records why
  the caller's value is now propagated.
- Both recursions also forward `timeProvider`.
- DfDeedle.cs line 148 becomes
  `await activeExplorer.GetTableInViewAsync(token, 0, timeProvider: timeProvider)`, completing the
  pattern already established at DfDeedle.cs lines 168 and 177 for `AddQfcColumnsAsync` and
  `EtlAsync`. This is the whole of the production change needed to reach the three DfDeedle-path
  tests, because all three already pass a `FakeTimeProvider`.
- `TimeOutTask.RunWithTimeout` is **not** changed for this item. Its existing
  `timeoutSourceFactory` parameter is the mechanism; the `TimeProvider` is the seam callers thread.

**Precondition — prove the API by compiling, not by reading documentation.** The research verified
`TimeProviderTaskExtensions.CreateCancellationTokenSource(TimeProvider, TimeSpan)` by reading the
XML documentation file shipped beside the net462 assembly (member declared at line 199), **not the
IL**, and no in-repo call site exists. Before the implementation commits to this API it must be
proven at build time. The settling experiment named by the research is one line —
`_ = TimeProvider.System.CreateCancellationTokenSource(TimeSpan.FromMilliseconds(1));` inside any
existing `UtilitiesCS` method — followed by the analyzer build; a clean compile confirms the member,
`CS1061` refutes it. An equivalent compile-time proof (for example, compiling the real call site
first and capturing the build log) is acceptable. This worktree has **no restored packages
directory**, so `nuget restore` (or the MSBuild restore target) must run before any build.
If the member proves absent, fall back to a `TimeProvider`-driven factory implemented in-repo
(construct a plain `CancellationTokenSource` and cancel it from a timer created on the provider) and
record the fallback; do not silently revert to option (a).

**Considered and not chosen — option (a), a test-supplied never-cancelling factory.** The seam
already permits it with zero production change, and there is an in-repo precedent at
OlTableExtensions_Tests.cs line 1300. It was rejected because of the correction above: the factory is
unreachable from the production path, so option (a) cannot reach the three tests that go through
DfDeedle (DfDeedleEtlTimeoutTests.cs lines 148 and 194, DfDeedle_COM_Tests.cs line 473) without a
production signature change on `GetEmailDataInViewAsync` of the same size as option (b) — and it
would leave two seam types on one call path while option (b) leaves one seam threaded end to end.
Option (b)'s cost is the pre-.NET 8 `CancelAfter` caveat documented at lines 211-214 of the package
XML; see Risks.

**Item 3 — change the declared tuple element (shape (i)); do not rethrow.**

- `EtlAsync`'s return type becomes
  `Task<(object[,]? data, Dictionary<string, int> columnInfo)>` (Etl.cs line 66).
- The null-forgiving suppression at line 131 is deleted: `return (data, columnDictionary);`.
- The `catch (TimeoutException)` at lines 119-125 is unchanged, so no exception type changes and no
  consumer observes a behavioural difference.
- The `DfDeedle` guard at lines 182-189 is **kept**. It remains the point at which the failure is
  named; it is now a null check against a type that admits null rather than against a lie.
- `EtlAsyncOld` already declared this exact shape (Etl.cs lines 134-137), so the pattern has an
  in-repo precedent — one that this feature then deletes, which makes adopting it on the live method
  the way the precedent survives.
- The doc comment on `EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource`
  (OlTableExtensionsEtlClockTests.cs lines 99-103) is updated: it currently describes "a null data
  array returned through a null-forgiving suppression", and the suppression no longer exists. The
  test body and both assertions are unchanged.

**Considered and not chosen — shape (ii), nullable element plus a rethrown `TimeoutException`.**
Rejected on two grounds. First, (i) removes the defect the issue actually names — a null forced
through a suppression into a non-nullable element — while (ii) additionally changes observable
behaviour at a boundary this feature does not own: `DfDeedle` is the only production consumer and its
guard, added by #811, is the agreed reporting point. Second, (ii) forces a deliberate rewrite (not a
deletion) of the contract test at OlTableExtensionsEtlClockTests.cs line 105: its `await` at line 141
and `data.Should().BeNull()` at line 144 stop making sense, and whether `tokenSource.Cancel()` should
still run under a rethrow becomes an open design question with no evidence in this feature to settle
it. Rewriting a contract test to accommodate a change is the failure mode that test was added to
prevent. If a future change does want the rethrow, it should be its own issue, with the cancellation
question answered explicitly.

**Item 4 — delete the inert overloads and the dead method together.**

- Delete `TimeoutAfter<TResult>(this Task<TResult>, int, int)` at TimeOutTask.cs lines 824-849 and
  `TimeoutAfter(this Task, int, int)` at lines 924-940.
- Delete `EtlAsyncOld` at OlTableExtensions.Etl.cs lines 134-171 (its call at line 160 is the only
  production caller of a deleted overload).
- Delete `EtlAsyncOld_WithBinaryAndObjectFields_ReturnsTransformedData` at OlTableExtensions_Tests.cs
  lines 984-1015.
- Delete the two test callers: `TimeoutAfter_GenericTask_WithRepeatAttempts_ReturnsResult`
  (TimeOutTask_Tests.cs line 191, call at 197) and
  `TimeoutAfter_NonGenericTask_WithRepeatAttempts_CompletesSuccessfully` (line 204, call at 210).

**Unreachability mechanism, stated precisely.** All three exits of the inner `(int, TimeProvider?)`
overload return a `Task`; none throws synchronously. (1) `return task` at TimeOutTask.cs line 873
when the task is already complete or the timeout is infinite. (2) `tcs.SetException(new
TimeoutException()); return tcs.Task` at lines 883-884 for a zero timeout — the exception is *placed
into* the completion source, faulting the proxy, and control returns normally. (3) `return tcs.Task`
at line 921 after arming a timer whose callback calls `TrySetException(new TimeoutException())` at
line 895 (line 982 in the non-generic pair), on a timer thread, after the method has already
returned. Consequently the assignment at line 834 never throws, the `catch (TimeoutException)` at
line 836 (line 932 in the non-generic pair) never runs, `repeatAttempts` is never read, and the
"attempts remaining" warning at line 838 is unreachable.

**Honest accounting of the file-size effect.** The deletion removes 43 lines, taking TimeOutTask.cs
from 1011 lines to 968. The repository cap in .claude/rules/general-code-change.md is 500 lines.
This is a **reduction, not a resolution**, of the cap violation, and no acceptance criterion in this
feature claims otherwise.

**Structural note.** `TimeOutTask_Tests` is one `partial` class spread across four files; the
`[TestClass]` and `[DoNotParallelize]` attributes at TimeOutTask_Tests.cs lines 9-10 govern all four.
Deleting two `[TestMethod]`s from one partial file does not affect the others.

**Item 5 — correct the stale doc comment.** DfDeedle.QfcColumns.cs line 96 currently reads
"(CS1769); the same constraint already applies to the `<c>TableEtlInvoker</c>` seam." Replace
`TableEtlInvoker` with `DefaultTableEtl` (declared DfDeedle.cs lines 67-70) or with the `etl`
parameter on `GetEmailDataInView` (DfDeedle.cs line 74, resolved at line 83). The CS1769 rationale
the sentence appeals to remains true of that pair — `DefaultTableEtl` is declared over `object` for
exactly that reason, documented at DfDeedle.cs lines 62-66 — so only the name changes. One line,
comment only. The past-tense mention at DfDeedleEtlTimeoutTests.cs line 212 is historically accurate
and is not touched; the occurrences under docs/features/** are historical records and are not
touched.

**Item 6 — correct the comment now; remove the attribute after item 2 lands, in this feature, with
no soak.**

- The class comment at OlTableExtensions_Tests.cs lines 18-20 is wrong on its face: it cites a
  population of tests driving the 2000 ms window that the code does not contain. Four tests call
  `GetTableInViewAsync` (lines 1238, 1267, 1324, 1646); three of them never reach the real window
  (1238 throws at TableAccess.cs line 49 before `RunWithTimeout`; 1324 has a pre-cancelled token and
  throws at TimeOutTask.cs line 50; 1267 supplies a test-held source through the factory at line
  1300, which makes its `timeoutMs: 5` inert), and only line 1646 arms a real
  `new CancellationTokenSource(2000)`.
- The class has **no shared mutable state**: no `[ClassInitialize]`/`[TestInitialize]`/
  `[ClassCleanup]`/`[TestCleanup]`/`[AssemblyInitialize]`, no mutable static fields (its only statics
  are six pure helpers), no `Console.SetOut` and no captured `TextWriter`, no static `TimeProvider`,
  and no `Thread.Sleep`, `Task.Delay`, `DateTime.Now` or `Stopwatch`. The usual argument for the
  attribute does not apply.
- The **actual hazard is a wall-clock deadline under thread-pool contention**: the test at line 1646
  asserts `callCount.Should().Be(1)` (line 1680) while a genuine 2000 ms deadline governs the
  `Task.Run` at TimeOutTask.cs line 63. Under class-level parallelism with `Workers = 0` and a
  saturated pool, a work item not dequeued within 2000 ms cancels the linked token and triggers the
  retry recursion at TableAccess.cs line 82, producing `callCount == 2`.
- **A soak is not a defensible basis for removal.** Ten green runs sample one machine and one suite
  composition; the failure probability rises with unrelated future test additions and slower CI
  hardware, so a soak establishes nothing durable. Repository policy also forbids stabilising a test
  with timing tolerance. Item 2 eliminates the hazard instead: once test 1646 passes a
  `FakeTimeProvider` that never advances, no wall-clock deadline governs it and the attribute has no
  remaining justification. It is then removed, and the comment is replaced with one that records
  what changed.
- **Attributes that stay, with their reasons intact.** `OlTableExtensionsEtlClockTests` (attribute at
  line 21, reason at line 20) and DfDeedleEtlTimeoutTests (attribute at line 24, reason at lines
  22-23) both state accurately that they drive a real `Task.Run` gate; both keep their attributes and
  their comments. `OlTableExtensionsRetryTests` has no attribute and nothing to remove.
- **`TimeOutTask_Tests` (line 10) carries the attribute with no recorded reason.** This feature must
  either supply a documented, verified reason or remove the attribute. The verified fact is that the
  class contains real wall-clock races — `Task.Delay(200)` raced against `TimeoutAfter(10)` at lines
  30-33, and `Task.Delay(50)` raced against `TimeoutAfter(0)` at lines 43-46 — which is a genuine
  reason to document. **Document it**; do not remove the attribute. (Converting those tests to an
  injected clock is a larger change and is not in scope.)

### Boundaries and invariants to preserve

- Production timing is unchanged when no `TimeProvider` is supplied: `timeProvider ?? TimeProvider.System`.
- The two `Console.WriteLine` diagnostics, the `counter` variable and both `catch` blocks in
  `GetTableInViewAsync` survive byte-identical (see the ownership boundary section).
- `GetTableInViewAsync`'s public non-null return contract and its `return table!` at line 118 are
  unchanged; the null-on-timeout latent condition documented at lines 116-117 is not addressed here.
- `EtlAsync` still swallows its `TimeoutException` and still cancels the supplied token source.
- `TimeOutTask.RunWithTimeout`'s signatures, `strict` semantics and retry behaviour are unchanged.

### Dependencies or blocked work

- A NuGet restore in this worktree before any build (there is no packages directory here).
- The compile-time proof of `CreateCancellationTokenSource` gates the item 2 implementation.
- Item 6's attribute removal is gated on item 2 landing in the same feature.
- Sibling feature 826 executes in wave 1 after this feature merges; no work here waits on it.

### Files/modules to change

See the Write Set section. In summary: three production files carry behaviour changes
(TableAccess.cs, Etl.cs, TimeOutTask.cs), two carry comment or single-argument changes (DfDeedle.cs,
DfDeedle.QfcColumns.cs), four existing test files are edited (OlTableExtensions_Tests.cs,
OlTableExtensionsEtlClockTests.cs, TimeOutTask_Tests.cs and, per the 2026-09-09 AC6 amendment,
DfDeedleEtlTimeoutTests.cs), one test file is added, and the test project file gains one compile item.

### Functions/classes impacted

`OlTableExtensions.GetTableInViewAsync`, `OlTableExtensions.EtlAsync`,
`OlTableExtensions.EtlAsyncOld` (deleted), `TimeOutTask.TimeoutAfter` `(int, int)` overloads
(deleted), `DfDeedle.GetEmailDataInViewAsync`, `DfDeedle.AddQfcColumnsAsync` (doc comment only),
`OlTableExtensions_Tests` (class attribute, comment, four reflection call sites, one deleted test),
`OlTableExtensionsEtlClockTests` (one doc comment), `TimeOutTask_Tests` (two deleted tests, one added
comment).

### Data flow and validation changes

The only data-flow change is that a `TimeProvider` reference now travels one hop further:
`GetEmailDataInViewAsync` -> `GetTableInViewAsync` -> the deadline-source factory -> `RunWithTimeout`.
No validation is added or removed. No new value type is introduced; if one becomes necessary, note
that both projects target v4.8.1 with no `IsExternalInit` polyfill, so `init` accessors, `record` and
`record struct` fail with CS0518 — use a plain `readonly struct`, following the working precedent at
DfDeedle.cs lines 262-292.

### Error handling and logging updates

No exception type, message or logging level changes. The `logger.Warn` for "attempts remaining"
disappears with the deleted dead overloads; it was unreachable, so no log output changes at runtime.
`LogTableTiming` calls are untouched, including the one at Etl.cs lines 127-130 that sits between the
swallow and the return.

### Rollback / feature-flag considerations

None. Every change is either a comment, a deletion of unreachable code, or an optional parameter that
defaults to today's behaviour. Rollback is a revert of the merge commit.

### Technical specifications

**Inputs/outputs and formats.** `GetTableInViewAsync(Explorer, CancellationToken, int counter, int
timeoutMs = 2000, Func<int, CancellationTokenSource>? timeoutSourceFactory = null, TimeProvider?
timeProvider = null)`. `EtlAsync` returns
`Task<(object[,]? data, Dictionary<string, int> columnInfo)>`.

**Required configuration keys and defaults.** None. `timeoutMs` keeps its 2000 default;
`timeProvider` defaults to null, meaning the system clock.

**Backward-compatibility expectations.** Every new parameter is **optional and trailing**, so every
in-solution caller that binds by ordinary overload resolution stays source-compatible without edit —
this is a hard requirement of the design, not a convenience. Two categories of caller are exceptions
and must be updated deliberately:
- **Reflection binders.** `OlTableExtensions_Tests.InvokeAsyncResult` (declared lines 1820-1842)
  binds `GetTableInViewAsync` by an explicit parameter-`Type[]`. The four call sites at lines 1238,
  1267, 1324 and 1646 each pass that array literally, so each must gain `typeof(TimeProvider)` and a
  corresponding argument. This is a binding change, not a source-compatibility break.
- **Deleted members.** The `(int, int)` `TimeoutAfter` overloads and `EtlAsyncOld` are removed
  outright. Deleting an overload while a caller remains produces a hard overload-resolution error
  (there is no `int`-to-`TimeProvider` conversion, so nothing can silently rebind) — a loud failure,
  not a silent behaviour change. All four known callers are deleted in the same change.

**Performance constraints.** None measured and none asserted. The per-row budget is unchanged; the
deadline mechanism changes from `new CancellationTokenSource(ms)` to a provider-created source, which
is one additional object per table acquisition and is not on a hot path.

## Assumptions, Constraints, Dependencies

- **Assumptions.** (a) `TimeProviderTaskExtensions.CreateCancellationTokenSource` arms its
  cancellation through the supplied provider's `CreateTimer`, so a `FakeTimeProvider` that never
  advances never cancels — inferred from the shipped XML documentation and **verified empirically by
  the new `ArmingBarrierTimeProvider` test**, which cannot pass unless a timer is created on the
  injected provider. (b) The `catch (TimeoutException)` retry branch in `GetTableInViewAsync` is
  reachable in production. See the reachability note below.
- **Constraints.** Legacy non-SDK MSBuild projects (`ToolsVersion="15.0"`, no `Sdk=` attribute, no
  globbing) with explicit compile items: any new C# source file must get its own `<Compile Include>`
  entry or it silently does not compile. Add only the new entry; do not reorder or reformat the rest
  of the project file, so the fan-in merge is a clean union. Both projects target v4.8.1 with no
  `IsExternalInit` polyfill. No `[ExcludeFromCodeCoverage]` exists on any in-scope file, so every
  touched line is in the coverage denominator. `Thread.Sleep` and `Task.Delay` are banned symbols
  (RS0030, currently at `suggestion` severity) and must not appear in new test code. The 500-line cap
  applies to production and test code alike, which is why new tests go in a new file.
- **External dependencies.** `Microsoft.Bcl.TimeProvider` 10.0.11 (already referenced by both
  projects) and `Microsoft.Extensions.TimeProvider.Testing` (already used for `FakeTimeProvider`). No
  new package.

**Reachability note (spec-author analysis, not from the research, and not verified by a test).**
`GetTableInViewAsync` calls `RunWithTimeout` with `strict: false` and `maxAttempts: 1`. With
`strict: false`, TimeOutTask.cs lines 85-92 log and swallow any exception from the work item, and
lines 65-83 absorb `TaskCanceledException`; `token.ThrowIfCancellationRequested()` at line 67 raises
`OperationCanceledException`, which the `catch (TaskCanceledException)` at TableAccess.cs line 71
does not catch. On that reading, neither `catch` block in `GetTableInViewAsync` is entered by the
ordinary expiry path, and the existing tests are consistent with it (line 1324 asserts that
`OperationCanceledException` propagates; line 1267 asserts no synthetic retry occurs). This is an
observation about a pre-existing condition, **not** something this feature fixes and **not** an
acceptance criterion. The executor must record the observation as evidence under the feature's
evidence folder (kind: other) and must record the follow-up as a handoff for the epic to file
**after this feature merges**. **No promotion record is written on this branch.** Writing one would
create a file under docs/features/potential/promoted/, which AC20 forbids; the two obligations
cannot both hold on the same branch, and the on-branch write is the one that is dropped. It must
not be acted on here. It does bear on test design: see Test Strategy.

## Data / API / Config Impact

- **User-facing or API changes:** none at the add-in level. Two internal extension-method overloads
  are deleted and one public extension method gains a trailing optional parameter.
- **Data or migration considerations:** none.
- **Logging/telemetry updates:** none. Existing `LogTableTiming` and `LogDfTiming` payloads are
  unchanged, and item 1 depends on them continuing to emit `rowCount` and `elapsedMs`.
- **Compatibility notes:** no CLI flags, config schemas or versioned contracts are involved.

## Test Strategy

All new tests go in a **new file**, `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs`,
with a matching compile item added to the test project. They must not be added to
OlTableExtensions_Tests.cs: that file is 1846 lines, its own comment at line 1640 records that it is
at its ceiling, and the repository cap is 500 lines for production and test code alike. Edits to that
file in this feature are deletions and in-place updates only.

### Regression tests to add

1. **`GetTableInViewAsync_InjectedClock_ArmsAcquisitionDeadlineOnInjectedProvider`** — proves the
   acquisition deadline is armed on the caller's clock. Mechanism: `ArmingBarrierTimeProvider`
   wrapping a `FakeTimeProvider` (the helper at UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs
   signals its `Armed` task after forwarding `CreateTimer`). The test awaits `barrier.Armed`, which
   can only complete if a timer was created on the injected provider; it then advances the fake clock
   past `timeoutMs` and awaits the call. The established precedent for this pattern is
   DfDeedleEtlTimeoutTests.cs lines 158-165. No wall-clock wait is involved. This test doubles as the
   empirical verification of assumption (a) above.
2. **`GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000`** — proves the retry
   honours the caller's value. Mechanism: a test-supplied `timeoutSourceFactory` that records every
   `ms` argument it receives, throws `TimeoutException` on its first invocation, and returns a
   never-cancelling `new CancellationTokenSource()` thereafter. The factory is invoked at
   TimeOutTask.cs line 52, outside the `try` at line 61, so the throw escapes `RunWithTimeout` and
   enters the `catch (TimeoutException)` at TableAccess.cs line 95; the recursion at line 103 then
   invokes the factory again. The test calls with a distinctive `timeoutMs` (for example 750) and
   asserts the second recorded value is 750. It fails against the pre-change file, where the recorded
   value is 2000. **The factory is being used here as an exception-injection point**, deliberately
   and with the reachability note above in view: it is the only mechanism found in the current tree
   that reaches that branch deterministically. If the executor finds an equivalent deterministic
   mechanism that reaches the branch without injecting an exception through the factory, it may be
   substituted, and the substitution must be recorded in the evidence folder.
3. **`GetTableInViewAsync_ExplicitFactorySupplied_TakesPrecedenceOverTimeProvider`** — proves the
   resolution order and protects the existing direct-caller seam. Mechanism: supply both a recording
   factory and a `FakeTimeProvider`; assert the factory was invoked and the provider created no
   timer.
4. **`GetTableInViewAsync_NoTimeProviderSupplied_UsesSystemClockAndCompletes`** — the
   default-path guard: with both optional parameters omitted and a `GetTable` that returns
   immediately, the call returns the table. Protects production timing from an accidental change.

### Existing tests updated

- OlTableExtensions_Tests.cs lines 1238, 1267, 1324, 1646 — reflection `Type[]` arrays gain
  `typeof(TimeProvider)`. Line 1646 additionally passes a `FakeTimeProvider`, which is what removes
  the wall-clock hazard that justifies `[DoNotParallelize]` on that class.
- OlTableExtensionsEtlClockTests.cs lines 99-103 — doc comment only; assertions unchanged.
- OlTableExtensions_Tests.cs lines 984-1015 and TimeOutTask_Tests.cs lines 191-214 — deleted with the
  code they cover.
- DfDeedle_COM_Tests.cs and DfDeedleEtlTimeoutTests.cs line 187 — **no change**. Their existing
  `FakeTimeProvider` arguments become effective on the table-acquisition deadline automatically once
  DfDeedle forwards the provider, and neither consumes arming signals. Their continued passing is
  itself evidence that item 2 is wired correctly.
- DfDeedleEtlTimeoutTests.cs line 135 — **timer-ordering update only**, per the amended non-goal
  above and AC6. `BuildExplorer` gains one leading gate parameter invoked inside its `GetTable` setup,
  and the test gains a third `ManualResetEventSlim` plus one further await/re-arm pair so that exactly
  one timer arms inside each await window: acquisition, then column-add, then the ETL hop. The two
  other tests in the class pass an empty lambda for the new gate. No assertion in the class changes,
  and the historically accurate past-tense mention at line 212 is not touched.

### Determinism requirements

No `Thread.Sleep`, no `Task.Delay`, no `DateTime.Now`, no `Stopwatch`, no retry loops and no timing
tolerances in any new or edited test. Every deadline in the new tests is driven by `FakeTimeProvider`
or `ArmingBarrierTimeProvider`, or is neutralised by a never-cancelling `CancellationTokenSource`
supplied through the factory seam. Gates use `ManualResetEventSlim` and are always released in a
`finally` block so an orphaned `Task.Run` body cannot outlive the test.

### Edge cases and negative scenarios

- Pre-cancelled token still propagates `OperationCanceledException` (existing test at line 1324).
- A supplied factory overrides the clock (new test 3).
- A never-advancing clock produces no cancellation (new test 4 and the two green-path DfDeedle tests).
- The retry path is exercised with a distinct `timeoutMs` (new test 2).
- `EtlAsync`'s expiry path still returns a null data element and cancels the token source (existing
  test at OlTableExtensionsEtlClockTests.cs line 105, unchanged).

### Coverage obligation

- Repository-wide line coverage must remain at or above 80% against the **testable denominator**
  defined in CLAUDE.md § UT2 (COM/VSTO/WinForms/Outlook-Interop exemptions). No baseline exists in
  this feature folder yet, so the merge-base figure must be captured first and both figures reported;
  if the raw pre-change figure is already below the floor, the blocking obligation is that this
  change does not lower it.
- New or changed code targets 90% or better.
- No regression in coverage on changed lines.
- **Deleting well-covered code moves the percentage.** The two `TimeoutAfter` overloads, `EtlAsyncOld`
  and their three tests are covered today; removing them changes both numerator and denominator.
  Baseline coverage (at the merge base) and post-change coverage must therefore both be captured
  under the feature's evidence folder (kind: coverage and kind: baseline) and compared explicitly, so
  the delta is attributed to the deletion rather than to a regression.

### Toolchain commands (run in this exact order; restart from step 1 on any failure or auto-fix)

```
nuget restore TaskMaster.sln
dotnet tool restore
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /InIsolation
```

Both MSBuild gates use `/t:Rebuild`; a warm `/t:Build` returns exit 0 with `CoreCompile` skipped and
runs no analyzers. Non-vacuity must be demonstrated from the captured build log (no
`Skipping target "CoreCompile"` entries for the projects in the write set).

### Manual validation

None required. The live-Outlook ETL duration capture described under item 1 is explicitly **not**
part of this feature's validation; it is recorded as the recipe for a future measurement.

## Acceptance Criteria

Each criterion is decidable PASS or FAIL by inspecting the named file, the diff, a named test result,
or a named captured artifact.

**Item 1 — the 250 ms per-row budget**

- [ ] **AC1** — `EtlAsync` still computes its budget as `250 * rowCount`; the diff for
      OlTableExtensions.Etl.cs contains no change to that expression or its numeric literal.
- [ ] **AC2** — A comment at that site records (a) that the value rests on no recorded measurement,
      (b) that no measurement is obtainable in this environment, and (c) the live-Outlook capture
      route by name: the `LogTableTiming` "EtlAsync complete" payload carrying `rowCount` and
      `elapsedMs`, and the log4net root level `ALL`. All three facts must be present.
- [ ] **AC3** — After the change, a case-sensitive search of OlTableExtensions.Etl.cs for
      `250 * rowCount` returns no hit outside the body of `EtlAsync`.

**Item 2 — the residual 2000 ms window**

- [ ] **AC4** — `GetTableInViewAsync` declares `TimeProvider? timeProvider = null` as its last
      parameter, positioned after `timeoutSourceFactory`, and no caller outside
      OlTableExtensions_Tests.cs and DfDeedle.cs was edited in order to keep compiling.
- [ ] **AC5** — Test `GetTableInViewAsync_InjectedClock_ArmsAcquisitionDeadlineOnInjectedProvider`
      exists in `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs`, uses
      `ArmingBarrierTimeProvider`, and passes. Its `await barrier.Armed` cannot complete unless the
      deadline timer was created on the injected provider.
- [ ] **AC6** — DfDeedle.cs line 148 passes its `timeProvider` to `GetTableInViewAsync`;
      DfDeedle_COM_Tests.cs is absent from this feature's diff; DfDeedleEtlTimeoutTests.cs appears in
      the diff only as the bounded timer-ordering update described in Test Strategy, adding no
      assertion and removing none; and both classes still pass.
      **Amended 2026-09-09** to remove an obligation that could not be met. The original wording
      required DfDeedleEtlTimeoutTests.cs to be absent from the diff while still passing. Threading
      the provider into `GetTableInViewAsync` inserts the table-acquisition timer as the first arming
      signal on the `ArmingBarrierTimeProvider` that the test at line 135 consumes in a fixed order,
      and the barrier's latch drops a signal whenever two timers arm inside one await window. The
      test's `barrier.Advance(250)` at line 165 would then run before the 250 ms ETL timer exists,
      firing nothing; because the assertion at lines 168-172 sits inside the `try`, the `finally` at
      lines 174-179 never releases the gates, so the failure mode is a hang rather than a clean
      failure. No implementation choice avoids this: a deadline under the caller's clock is a timer on
      that clock. The alternative — not forwarding the provider from DfDeedle.cs line 148 — was
      rejected because it falsifies this criterion's first clause and the AC34 invariant, and leaves
      the production table-acquisition deadline on the system clock, which is the defect item 2 exists
      to close.
- [ ] **AC7** — Test `GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000` passes, and
      a fail-before record showing it failing against the pre-change file is captured under the
      feature evidence folder (kind: regression-testing).
- [ ] **AC8** — The availability of
      `TimeProviderTaskExtensions.CreateCancellationTokenSource(TimeProvider, TimeSpan)` was proven
      **by a compile, not assumed**: a captured MSBuild log under the feature evidence folder (kind:
      other or build) shows `UtilitiesCS` compiling successfully with a real call site present. A
      prose assertion of availability, or a citation of the package XML alone, is a FAIL.
- [ ] **AC9** — The `TaskCanceledException` retry branch still propagates `timeoutMs` and
      `timeoutSourceFactory` and now also propagates `timeProvider`; the `TimeoutException` retry
      branch contains no numeric literal timeout argument.
- [ ] **AC10** — Test `GetTableInViewAsync_ExplicitFactorySupplied_TakesPrecedenceOverTimeProvider`
      passes, demonstrating that a supplied factory still overrides the clock-derived one.

**Item 3 — the `EtlAsync` tuple contract**

- [ ] **AC11** — `EtlAsync` declares
      `Task<(object[,]? data, Dictionary<string, int> columnInfo)>`, and a search of
      OlTableExtensions.Etl.cs for the null-forgiving return `data!` returns no hit.
- [ ] **AC12** — `EtlAsync` still catches and swallows `TimeoutException` and still calls
      `tokenSource.Cancel()`; `EtlAsync` throws no new exception type. The `DfDeedle` null guard and
      its `InvalidOperationException` message are unchanged in the diff.
- [ ] **AC13** — `OlTableExtensionsEtlClockTests.EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource`
      passes with its body unchanged, including `data.Should().BeNull()` and the
      `IsCancellationRequested` assertion.
- [ ] **AC14** — The doc comment on that test no longer describes a null-forgiving suppression.

**Item 4 — the inert `(int, int)` overloads**

- [ ] **AC15** — TimeOutTask.cs declares no `TimeoutAfter` overload with an `int repeatAttempts`
      parameter, and contains no `catch (TimeoutException)` clause inside a `TimeoutAfter` method.
- [ ] **AC16** — A repository-wide search of C# source files for `EtlAsyncOld` returns no hit.
- [ ] **AC17** — TimeOutTask_Tests.cs no longer contains
      `TimeoutAfter_GenericTask_WithRepeatAttempts_ReturnsResult` or
      `TimeoutAfter_NonGenericTask_WithRepeatAttempts_CompletesSuccessfully`, and the remaining
      `TimeOutTask` tests across all four partial files pass.
- [ ] **AC18** — The delivered change makes no claim, in code comment, commit message or PR body,
      that the TimeOutTask.cs 500-line cap violation is resolved; the reduction is described as a
      reduction.

**Item 5 — the stale doc comment**

- [ ] **AC19** — A search of DfDeedle.QfcColumns.cs for `TableEtlInvoker` returns no hit, the
      corrected sentence names `DefaultTableEtl` or the `etl` parameter on `GetEmailDataInView`, and
      the CS1769 rationale sentence is retained.
- [ ] **AC20** — DfDeedle_COM_Tests.cs and every file under docs/features/** other than this
      feature's own documents are absent from the diff, and DfDeedleEtlTimeoutTests.cs appears only as
      the bounded timer-ordering update AC6 permits.
      **Amended 2026-09-09** for the same measured reason recorded under AC6. The
      docs/features/** clause is unchanged and remains the binding one: no promotion record, no
      potential entry, no epic manifest edit and no sibling feature folder may appear in this
      feature's diff.

**Item 6 — `[DoNotParallelize]`**

- [ ] **AC21** — `[DoNotParallelize]` is absent from `OlTableExtensions_Tests`, and its class comment
      no longer asserts any population of tests driving the 2000 ms window and no longer states that
      a soak is pending; it states instead that the wall-clock hazard was removed by the item 2
      change.
- [ ] **AC22** — The test at OlTableExtensions_Tests.cs line 1646 supplies a `FakeTimeProvider`, so
      no test in that class arms a real `CancellationTokenSource` on the system clock. Verified by a
      search of the file for `new CancellationTokenSource(` with a numeric argument returning no hit.
- [ ] **AC23** — `OlTableExtensionsEtlClockTests` and DfDeedleEtlTimeoutTests retain their
      `[DoNotParallelize]` attributes and their existing reason comments.
- [ ] **AC24** — `TimeOutTask_Tests` retains `[DoNotParallelize]` and a comment immediately above the
      attribute records the verified reason, naming the wall-clock races in the class (the
      `Task.Delay(200)` versus `TimeoutAfter(10)` test and the `Task.Delay(50)` versus
      `TimeoutAfter(0)` test).
- [ ] **AC25** — No acceptance criterion in this feature is satisfied by repeated passing runs: the
      change record contains no soak, and the justification for AC21 is the item 2 change, not
      observed green runs.

**Ownership boundary**

- [ ] **AC26** — `git diff` for OlTableExtensions.TableAccess.cs shows no added and no removed line
      containing `Console.WriteLine`; both diagnostics survive byte-identical including their leading
      indentation.
- [ ] **AC27** — The `counter` variable and both `catch` blocks in `GetTableInViewAsync` still exist.
- [ ] **AC28** — No file listed as sibling-owned in the Non-goals section appears in this feature's
      diff.

**Process, build and coverage**

- [ ] **AC29** — All new tests are in
      `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs`, that file has a
      `<Compile Include>` entry in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, and the diff for that
      project file adds only that entry (no reordering, no reformatting).
- [ ] **AC30** — OlTableExtensions_Tests.cs gains no new `[TestMethod]`; its diff contains deletions
      and in-place edits only.
- [ ] **AC31** — The new test file contains no `Thread.Sleep`, `Task.Delay`, `DateTime.Now`,
      `Stopwatch`, retry loop or timing tolerance.
- [ ] **AC32** — A full toolchain pass is recorded: `csharpier check` clean, both MSBuild gates
      passing with `/t:Rebuild`, and the test run green. The captured build log shows no
      `Skipping target "CoreCompile"` entry for any project in the Write Set.
- [ ] **AC33** — Baseline (merge-base) and post-change coverage are both captured under the feature
      evidence folder; coverage on changed lines does not regress; new and changed code is at 90% or
      better; the repository-wide figure against the CLAUDE.md § UT2 testable denominator is recorded
      and reported, with the deletion-driven delta explicitly attributed.
- [ ] **AC34** — The delivered implementation matches the invariant sentence and the four-step trace
      recorded in Proposed Fix: a reviewer can follow `timeoutMs` from the accept point through the
      throw point and the absorption point to the retry, and confirm the caller's value is no longer
      discarded.
- [ ] **AC35** — The reachability observation recorded under Assumptions is captured as an evidence
      artifact under this feature's own evidence/other/ folder, and the follow-up is recorded in that
      same artifact as a handoff for the epic to file **after this feature merges**. No promotion
      record, potential entry or any other file outside this feature's own folder is written under
      docs/features/** on this branch, and no code change in this feature acts on the observation.
      **Amended 2026-09-09** to remove a contradiction with AC20: the original wording required
      filing through the promotion lifecycle, which writes a record under
      docs/features/potential/promoted/ and would have put a file under docs/features/** that is not
      one of this feature's own documents, falsifying AC20 on the same branch. The evidence artifact
      plus the deferred handoff discharges the observation without the on-branch write.

## Risks & Mitigations

- **Thread-pool contention while `[DoNotParallelize]` is removed.** Removing the attribute from
  `OlTableExtensions_Tests` before the wall-clock deadline is neutralised would reintroduce the
  load-dependent failure described in item 6. Mitigation: AC22 requires that no test in the class
  arms a real timed `CancellationTokenSource` before AC21 is checked off; the two criteria must be
  verified in that order, and the attribute removal is the last edit in the feature.
- **The pre-.NET 8 `CancelAfter` caveat.** The package XML at lines 211-214 records that on
  pre-.NET 8 runtimes, calling `CancelAfter(TimeSpan)` on a source produced by
  `CreateCancellationTokenSource` does not terminate the original delay timer. net481 is pre-.NET 8.
  No current code calls `CancelAfter` on such a source, so this is a constraint on future edits, not
  a present defect. Mitigation: the implementation must not introduce a `CancelAfter` call on the
  provider-created source, and the constraint must be recorded in a comment at the creation site.
- **`CreateCancellationTokenSource` behaves differently from the documented shape.** The evidence is
  a shipped XML documentation file, not IL. Mitigation: AC8 requires a compile-time proof, and the
  `ArmingBarrierTimeProvider` test in AC5 provides the runtime proof that the injected provider owns
  the timer. If either fails, the fallback named in Proposed Fix (an in-repo provider-driven factory)
  is used and recorded.
- **Fan-in merge on the shared project file.** `UtilitiesCS.Test/UtilitiesCS.Test.csproj` is a
  legacy project with an explicit, ordered compile-item list, and sibling features in the same wave
  may add entries to it. Mitigation: add exactly one `<Compile Include>` line and change nothing
  else, so the merge is a clean union (AC29). UtilitiesCS/UtilitiesCS.csproj is not touched at all
  because no new production file is added — which also means that splitting TimeOutTask.cs, which
  would require a new production compile item, stays out of scope.
- **Fan-in merge on the shared source file.** OlTableExtensions.TableAccess.cs is edited by this
  feature and by sibling 826. Mitigation: the ownership boundary section defines the split precisely,
  AC26 and AC27 make the boundary machine-checkable, and this feature merges first.
- **Reflection-bound tests break silently at run time rather than at compile time.** Adding a
  parameter changes the `Type[]` those tests pass. Mitigation: all four call sites are enumerated in
  Test Strategy; the failure mode is a loud `MissingMethodException` at run time, not a silent skip.
- **Deleting covered code moves the coverage percentage.** Mitigation: AC33 requires both figures and
  an explicit attribution of the delta.
- **Item 2's retry test depends on an exception-injection use of the factory seam.** If a reviewer
  judges that mechanism unacceptable, AC7 cannot be met as written. Mitigation: Test Strategy permits
  an equivalent deterministic mechanism, provided the substitution is recorded; and the reachability
  note explains why no other mechanism was found in the current tree.

## Rollout & Follow-up

- **Rollout.** Ordinary branch, PR and merge. No flag, no migration, no operational step. This
  feature merges before sibling feature 826 executes in wave 1.
- **Post-fix follow-ups.** Each is recorded here and in an evidence artifact under this feature's own
  evidence/other/ folder, and each is handed to the epic to file through the issue-promotion
  lifecycle **after this feature merges**. None is filed on this branch: a promotion writes a record
  under docs/features/potential/promoted/, which AC20 forbids in this feature's diff. The follow-ups
  are:
  1. Capture real ETL durations against folder size from a live Outlook session using the recipe in
     item 1, and revisit the 250 ms per-row budget with that measurement in hand.
  2. Reduce TimeOutTask.cs below the 500-line cap; this feature takes it from 1011 to 968 lines only.
  3. The reachability observation in Assumptions concerning the two `catch` blocks in
     `GetTableInViewAsync` under `strict: false` (AC35).
  4. Convert the `TimeOutTask_Tests` wall-clock races to an injected clock, which would then allow
     that class's `[DoNotParallelize]` to be removed rather than documented.
- **Links.** Issue: https://github.com/drmoisan/TaskMaster/issues/825. Predecessors: #798, #811.
  Sibling in the same wave: 826. Research record:
  docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/research/2026-09-08T23-55-etl-deadline-mechanics-research.md
