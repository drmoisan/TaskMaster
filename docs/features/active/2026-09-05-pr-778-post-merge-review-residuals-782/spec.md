# pr-778-post-merge-review-residuals (Refactor Spec)

- **Issue:** #782
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-05
- **Status:** Draft — authored 2026-09-05 from issue.md acceptance criteria AC1-AC9 and the research
  record research/research.2026-09-05T16-10.md. Not yet planned, not yet executed.
- **Version:** 1.0

## Sources of Record

- Requirements source: issue.md in this feature folder (work mode `full-feature`, AC1-AC9).
- Finding source: the section "Post-merge code review (three-phase, 2026-09-05)" in
  pr-778-review-source.md in this feature folder, a verbatim local copy of the PR #778 body.
  Finding identifiers C01-C26, S2-1, S3-1 through S3-9, S4-1, S4-2 refer to that section.
- Verification source: research/research.2026-09-05T16-10.md in this feature folder. Every line
  number, line count, and member set quoted below is taken from that record. Where this spec and
  issue.md disagree, this spec states the reason and the orchestrator scope decision that resolved it.

### Formatting convention (do not "fix")

A downstream tool derives this delivery's change footprint by harvesting backtick-delimited
repository paths from this document. Every file this delivery creates or modifies appears at least
once as an inline code span. Every path cited only for context — a file this delivery reads but does
not touch — is deliberately written as plain prose without backticks. Do not add backticks to those
citations.

## Intent & Outcomes

PR #778 changed `UtilitiesCS/Threading/UiThread.cs` so that the static `Dispatcher` accessor throws
`InvalidOperationException` instead of returning null. The three-phase post-merge review confirmed
the fix and found no functional regression. It also produced a set of residuals that are individually
small and that no single one of them justifies a separate delivery: two latent defects in the new
code, a set of test-hygiene and documentation nits, and a group of internal inconsistencies in the
#584 feature folder's audit and evidence artifacts.

This delivery consolidates all of them into one Refactor pass so that none is lost when the #584
feature folder is archived. Observable outcomes:

- The `UiThread.Dispatcher` getter reads its backing field once, carries XML documentation, and
  throws a single shared message that names only the public `Init()` entry point and states the
  UI-thread requirement.
- `UiThread.Init()` can be retried after a failed `Initialize()`, so the remedy the message names is
  actionable.
- All `UtilitiesCS.Test` manipulation of the `UiThread._dispatcher` static goes through one
  disposable install scope with one reflection acquisition.
- No test file in the touched set exceeds the 500-line limit, and no test leaves an unshut dispatcher
  on a pooled MTA worker thread.
- Comments and reason strings describe the current synchronous `InvalidOperationException`
  mechanism rather than the pre-#778 `NullReferenceException` mechanism.
- The #584 feature folder's audits and evidence are internally consistent, schema-conformant, and
  neutral in tone.

## Behavioral Contract

This section states what the changed production surfaces must do after the change. Behavior on the
initialized path is unchanged; only the uninitialized path and the message text change.

### The shared message constant

`UtilitiesCS/Threading/UiThread.cs` gains one member:

```csharp
internal const string DispatcherNotInitializedMessage =
    "The UI dispatcher has not been captured. Call UiThread.Init() on the UI (STA) thread during host startup before reading UiThread.Dispatcher.";
```

Placement: inside `UiThread`, adjacent to the `Dispatcher` property (currently lines 135-149), so the
constant and its thrower are read together. Accessibility `internal` is sufficient for every
consumer and is the accessibility CLAUDE.md § C#5.2 prefers for non-public API. `const` rather than
`static readonly` because the value is a compile-time literal with no initialization-order concern.

The text reconciles three findings simultaneously:

| Finding | Requirement | How the text satisfies it |
|---|---|---|
| C06 | name only the public `Init()`, not the private `Initialize()` | the literal contains `UiThread.Init()` and does not contain `UiThread.Initialize()` |
| C09 (message half) | state the STA / UI-thread requirement | the clause "on the UI (STA) thread during host startup" |
| C20 | one constant shared by both throw sites | the literal is domain-neutral and names no caller-specific operation |

A rejected alternative: a new holder type `UiThreadMessages`. It adds a file and a csproj
`<Compile Include>` entry for one string, and `UtilitiesCS/Threading/UiThread.cs` (172 lines
measured) has ample headroom under the 500-line limit even after the C08 XML documentation and the
C05 comment are added.

### `UiThread.Dispatcher`

Invariant: **the getter never returns null, and it never observes a value other than the one it
tested.**

After the change the getter must:

1. Read the static backing field exactly once into a local (C02).
2. Throw `InvalidOperationException(DispatcherNotInitializedMessage)` when that local is null.
3. Return that same local otherwise.
4. Not lazily call `Init()`. A two-line comment above the throw must state the reason: `Initialize()`
   constructs and shows a hidden WinForms `SyncContextForm` and must run on the UI thread, so a lazy
   `Init()` from an arbitrary reader is deliberately avoided here even though the sibling
   `UiSyncContext` and `AutoScaleFactor` accessors do self-heal (C05).
5. Carry `<summary>`, `<remarks>` documenting the deliberate non-lazy contract, and
   `<exception cref="InvalidOperationException">` XML documentation. The file currently carries zero
   `///` comments (C08).
6. Keep its declared type non-nullable `Dispatcher` and keep its private setter. This is not a public
   signature change.

Accept-to-throw trace for one value. TaskMaster/ThisAddIn.cs calls `UiThread.Init(...)` on the
Outlook STA thread during startup; `Initialize()` assigns the captured dispatcher D to the backing
field. A later reader enters the getter, copies the field into the local — observing D — tests the
local, and returns D. No second read occurs, so a concurrent null write landing after the test
cannot cause a null return; the caller receives D or the exception, never null. On the uninitialized
path the local is null, the getter throws with the shared constant, and the exception propagates to
the caller unchanged. The getter absorbs nothing and introduces no new catch.

### `UiThread.Init()`

Signature, parameter names, and default values are unchanged.

Invariant: **a failed initialization must not permanently consume the single-shot latch.**

- `Init()` continues to gate `Initialize()` behind the single-shot latch (currently line 36,
  `if (_loaded.CheckAndSetFirstCall)`). The latch must continue to be checked and set **before**
  `Initialize()` runs, so two concurrent callers cannot both enter `Initialize()`.
- When `Initialize()` throws, `Init()` re-arms the latch by assigning a fresh
  `ThreadSafeSingleShotGuard` to the backing field and rethrows the original exception unchanged, so
  a subsequent `Init()` retries initialization (C03). The latch field is not `readonly` (currently
  line 46), so reassignment is legal. The re-arm idiom already exists twice in the same assembly, in
  UtilitiesCS/Threading/IdleActionQueue.cs and UtilitiesCS/Threading/ApplicationIdleTimer.cs.
- The broad catch is permitted by the General Code Change Policy only because it immediately
  rethrows. It must carry a comment stating that it exists to re-arm the latch, not to absorb the
  failure.
- No deterministic unit test covers this branch, because `Initialize()` shows a WinForms window and
  cannot be forced to throw from a test without introducing a new production seam, which is out of
  scope. The delivery's code-review artifact must record that reason. See AC2.
- `Init()` still performs no apartment-state check. Making it reject non-STA callers is out of scope
  and is promoted separately under AC8.

### `WpfDispatcherYield`

`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` (77 lines measured) contains exactly one
`throw`, at lines 62-67. C20's phrase "both throws" refers to that one plus the one in
`UtilitiesCS/Threading/UiThread.cs` — two files, one throw each, in the same `UtilitiesCS` assembly,
which is what makes a single `internal` constant viable.

After the change:

- Dispatcher-selection behavior is unchanged. The type still prefers the dispatcher affinitized to
  the calling thread and falls back to the injected provider, whose production default is
  `() => UtilitiesCS.UiThread.Dispatcher` (line 46).
- The local `dispatcher is null` guard throws `InvalidOperationException(UiThread.DispatcherNotInitializedMessage)`.
- **The domain-specific tail "before yielding folder tree work" is removed.** This loss is intended
  (scope decision SD5) and is pinned by an acceptance criterion and by the C20 `WithMessage`
  assertion, so a reviewer does not read it as a regression. Two facts bound the impact: the guard is
  unreachable on the production path, because the production fallback provider throws from
  `UiThread.Dispatcher` first with the same message; and the guard therefore covers only injected
  providers, which are typed `Func<Dispatcher?>` and exist only in tests.
- The comment at lines 53-59 is corrected. Its final clause ("UiThread.Dispatcher is set-once state
  ... and is null outside a live host") is false after PR #778. The replacement must state that the
  production fallback provider throws directly and that the local guard covers injected providers.

## Scope

### Write Set — production files (5)

| File | Change (one line) | Findings |
|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 172 lines measured. Add the shared message constant, single-read getter, non-lazy comment, XML docs, and the `Initialize()` failure re-arm in `Init()`. | C02, C03, C05, C06, C08, C09-message, C20 |
| `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` | 77 lines measured. Correct the comment at lines 53-59 and route the single throw at lines 62-67 through the shared constant. | C20 |
| `UtilitiesCS/Threading/ProgressTracker.cs` | Pass the captured `UiDispatcher` local (line 33) into the `Invoke` lambda instead of re-reading the static at line 39. The unrelated viewer-dispatcher read later in the same file is not changed. | C23 |
| `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | Pass the captured `UiDispatcher` local (line 33) into the `InvokeAsync` lambda instead of re-reading the static at line 39. | C23 |
| `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | Remove the now-dead `dispatcher != null` comparisons at lines 72 and 115. The three `UiThread.Dispatcher` mentions in XML-doc prose (lines 54 and 93) are not edited. | C01 |

### Write Set — test files (10, of which 2 are new)

| File | Change (one line) | Findings |
|---|---|---|
| `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs` | **New.** The single `internal IDisposable` install scope for the `UiThread._dispatcher` static, holding the only reflection acquisition in the assembly. | C12, C13 |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | 179 lines measured. Host the populated-branch sentinel on a dedicated STA thread with shutdown; move the field null guard into the helper and use expression-bodied throw lambdas; assert `*UiThread.Init()*`; migrate to the install scope; refresh the stale XML-doc prose at line 113. | C06, C10, C11, C12, C13 |
| `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | 514 lines measured. Split: this file keeps the class attributes as two separate lines and the first 17 tests plus `CapturingProgressTracker`, and becomes `public partial class`. Projected 271 lines. | C15, C16 |
| `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs` | **New.** The second partial part: the P74 region's 7 tests. Projected 260 lines. Its `_dispatcher` reflection site then migrates to the install scope, and the C26 synchronous sibling test is added here. | C12, C13, C16, C26 |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | 206 lines measured. Migrate the reflection site at lines 138-142 to the install scope; add the asynchronous C26 test. | C12, C13, C26 |
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | 348 lines measured. Rewrite the three P27-T2 passages to describe the synchronous `InvalidOperationException` path; reimplement `ForceDispatcherNull` / `RestoreDispatcher` on top of the install scope. | C12, C13, C19 |
| `UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs` | 241 lines measured. Add a `[TestCleanup]` that drains queued entries, resets the subscribe guard, cancels the pending unsubscribe, and unsubscribes the heartbeat handler; add `[DoNotParallelize]` to the class. | C14 |
| `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | 201 lines measured. Add a `WithMessage` assertion to `YieldAsync_WithoutDispatcher_RemainsStrict`; add the C21 production-fallback test. | C20, C21 |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | 320 lines measured. Replace the local `FieldInfo` and both null-conditional reads with `UiThreadDispatcherFixture.Current`; retype the snapshot field; delete the two "avoid WindowsBase" comment clauses at lines 29 and 53, retaining the accurate paragraph at lines 33-37. | C18, C25 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 393 lines measured. Correct the false clause "neither of which can complete an InvokeAsync" at lines 124-125 while preserving the accurate description of the parked-dispatcher case. | S2-1 |

### Write Set — build configuration (1)

| File | Change (one line) |
|---|---|
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | Add exactly one `<Compile Include>` entry for each new file: the Threading entry adjacent to the existing Threading\ProgressTracker_Tests.cs entry (currently line 477), and the TestHelpers entry adjacent to the two existing TestHelpers entries (currently lines 74-75). |

### Write Set — #584 feature folder documentation (4)

All four live under docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/.

| File | Change (one line) | Findings |
|---|---|---|
| `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` | Set Status to the merged state; reconcile the three disagreeing file lists against the six-file Write Set; replace the three call-site figures. | S3-6, S3-7 |
| `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/policy-audit.2026-09-04T04-05.md` | Soften the ordering sentence at line 115; correct the formatter command cell at line 229; amend row 3.1 at line 123; label the Appendix B entry at line 421 as a reference command rather than a transcript; add a section 8 gap entry after line 244; correct "34" to "38" at line 68; replace the evaluative span at line 111; record the S3-9 disposition. | S3-1, S3-2, S3-3, S3-8, S3-9 |
| `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/feature-audit.2026-09-04T04-05.md` | Soften the ordering sentence at lines 37-39; correct the formatter command cell at line 149; replace the evaluative spans at lines 117 and 119. | S3-1, S3-2, S3-8 |
| `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/code-review.2026-09-04T04-05.md` | Replace the evaluative spans at lines 22 and 191; record the S3-9 disposition against the open recommendation at line 85. | S3-8, S3-9 |

### Write Set — #584 feature folder evidence (19)

Four files change for reasons other than S3-5:

| File | Change (one line) | Findings |
|---|---|---|
| `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p1-t4-expect-fail.md` | Soften the ordering sentence at line 48. | S3-1 |
| `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p3-t1-analyzer-build.md` | Soften the ordering sentence at lines 30-31. | S3-1 |
| `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p2-t3-file-size.md` | Replace the evaluative span at line 42. | S3-8 |
| `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/issue-updates/issue-584.2026-09-02T09-02.md` | Insert an in-place naming note after line 3. No rename, no change to the existing `Timestamp:` value. | S3-4 |

#### S3-5 member set (15 files)

Scope decision SD3 widens S3-5 from the three files named in issue.md to all fifteen files whose
`EXIT_CODE:` value deviates from the schema's single-integer form. Rationale: a value that is not a
single integer is not machine-readable, so the collector cannot render the row; correcting three of
fifteen would leave the defect class live while letting AC3 read as resolved. The member set below is
taken verbatim from Numeric Derivation Evidence claim 4 of the research record, which derives it by
two distinct queries over the complete evidence subtree and compares the deviating and conforming
member sets against the independently established 37-file population.

Each of the following is edited to carry a single integer on the `EXIT_CODE:` line, with any
qualifying prose or per-command breakdown moved to a line below the field. Where the true exit code
is a non-zero value that the gate expects, `ExpectedExitCode: <int>` is added alongside it so the
collector normalizes the row to pass; that applies to the no-match grep gate in
`p3-t5-no-timing-tokens.md`, whose real exit code is 1. For `p0-t6-mcp-probe.md` no process ran, so
the honest normalization is a single integer plus a prose line recording that the MCP transport
returned no exit code.

All fifteen live under docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/.

Empty value with a following bullet list (11):

- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t6-quickfiler-tests.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p2-t2-nullforgiving-removed.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p1-t5-donotparallelize.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t1-format.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p3-t5-no-timing-tokens.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/other/p3-t4-progresstrackerasync-unmodified.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/other/p5-t10-footprint.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t13-parallel-bucket-census.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t14-reflective-dispatcher-census.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t5-toolchain-resolution.md`

Integer followed by parenthetical prose, or a non-numeric token (4):

- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t2-uithread-rederivation.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t3-progresstrackerasync-rederivation.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t4-test-rederivation.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t6-mcp-probe.md`

The three files named in issue.md (`p0-t6-mcp-probe.md`, `p1-t5-donotparallelize.md`,
`p3-t5-no-timing-tokens.md`) are a subset of this set.

### Evidence outputs for this delivery

This delivery's own gate evidence is written under
`docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/` in the canonical
sub-paths defined by the evidence-and-timestamp-conventions skill: baseline evidence under
evidence/baseline/, regression evidence under evidence/regression-testing/, gate evidence including
the coverage summary under evidence/qa-gates/, and the issue-update mirror under
evidence/issue-updates/. Any instruction to write these artifacts to artifacts/baselines/,
artifacts/qa/, artifacts/coverage/, or artifacts/evidence/ must be rejected and replaced with the
canonical path, recording `EVIDENCE_LOCATION_OVERRIDE_REJECTED: <supplied path> replaced with
<canonical path>`.

## The Shared Test Seam (C12/C13)

### Why reflection remains

UtilitiesCS/Properties/AssemblyInfo.cs grants `InternalsVisibleTo("UtilitiesCS.Test")`, but the
`_dispatcher` backing field is `private`, and an `InternalsVisibleTo` grant does not expose private
members. Reflection is therefore still required. The purpose of the seam is to reduce six
independently written reflection sites to one acquisition with one uniform failure mode, not to
eliminate reflection. Adding an `internal` test-only member to the production `UiThread` type is the
alternative; it is rejected because it puts test scaffolding into a production type, and issue.md's
C12/C13 wording explicitly permits the `UtilitiesCS.Test/TestHelpers/` landing site.

### Design

`UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs` declares
`internal sealed class UiThreadDispatcherScope : IDisposable` with this surface:

| Member | Contract |
|---|---|
| private static readonly `FieldInfo` | Resolved once in a static initializer. The resolution asserts the field is non-null with a stated reason, mirroring the `ResolveDispatcherField` idiom in the QuickFiler.Test fixture, so a rename of `_dispatcher` raises `TypeInitializationException` and **fails** every consuming test rather than degrading to a silent no-op. |
| `internal static Dispatcher? Current { get; }` | Reads the field directly, bypassing the throwing property getter, so a test can observe the uninitialized state without triggering the guard. |
| `internal static UiThreadDispatcherScope Install(Dispatcher? replacement)` | Captures the prior field value, writes `replacement`, returns the scope. |
| `internal static UiThreadDispatcherScope InstallNull()` | Convenience for `Install(null)`, replacing the private `ForceDispatcherNull` helpers. |
| `void Dispose()` | Restores the captured prior value. **This must restore a null prior value as well** — the prior is stored in a nullable field, not tested for null before restoring, so disposal always returns the static to exactly the value observed at install time. Disposal is idempotent: a second call is a no-op. |

The scope is deliberately not internally synchronized. Serialization of writers is provided by
`[DoNotParallelize]` on every class that installs a value, which is the existing repository model.
The scope's XML documentation must state that dependence explicitly so a future caller does not
assume thread safety.

### Migrating sites

Exactly four `UtilitiesCS.Test` reflection sites migrate. Each replaces its local `GetField` call,
its hand-rolled capture and `SetValue`, and its `try` / `finally` restore with a `using` statement
over the scope:

| # | File after this delivery | Site before this delivery |
|---|---|---|
| 1 | `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | the `DispatcherField()` helper at lines 125-131 and both consuming tests |
| 2 | `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs` | `ProgressTracker_Tests.cs` lines 421-426, which move into the new partial part by the C16 split before the migration runs |
| 3 | `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | lines 138-142 |
| 4 | `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | lines 144-145, consumed by the private `ForceDispatcherNull` (lines 165-171) and `RestoreDispatcher` (lines 184-187) helpers, which are reimplemented over the scope |

### The QuickFiler.Test side (C18)

`QuickFiler.Test` cannot use `UiThreadDispatcherScope`: the scope is `internal` to `UtilitiesCS.Test`,
and UtilitiesCS/Properties/AssemblyInfo.cs grants `InternalsVisibleTo` only to
`DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, and `ToDoModel.Test` — there is no grant to
`QuickFiler.Test`, so it also cannot read the private field through an internal seam on `UiThread`.

Instead, `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` reads the value through
`UiThreadDispatcherFixture.Current`, an `internal static Dispatcher` accessor already present in the
same assembly at QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs. That
file is not modified by this delivery. Mechanical consequences:

- `EmailMoveMonitorTests` is in namespace `QuickFiler.Helper_Classes.Tests`, so the migration needs
  `using QuickFiler.Controllers.Tests;` or a qualified reference.
- `Current` returns `System.Windows.Threading.Dispatcher`, so the snapshot field is retyped from
  `object` to `Dispatcher`. WindowsBase is already referenced by QuickFiler.Test.csproj, so no
  reference is added.
- The rename-safety property C18 asks for comes from the fixture's own field resolution, which
  asserts the field exists inside a static initializer. After the change, a rename of `_dispatcher`
  fails the class instead of passing vacuously on `null == null`.
- The two "avoid WindowsBase" comment clauses (C25) are deleted in the same edit, because the same
  migration makes their premise visibly false.

After this delivery the repository contains exactly two `GetField("_dispatcher", ...)` acquisitions:
one in `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs` and one in the QuickFiler.Test
fixture named above.

## The File Split (C16/C15)

`UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` is 514 lines, over the 500-line limit stated in
CLAUDE.md § 4 and .claude/rules/general-code-change.md. No rule-level pre-existing or baseline
exemption exists; the "baseline + 1" clause the #584 evidence relies on is plan-local and cannot
waive a CLAUDE.md rule under the Policy Compliance Order.

**Shape: `partial class`.** Repository precedent is direct and current — UtilitiesCS.Test's
`TimeOutTask_Tests` is split across four files, of which only one carries `[TestClass]` and
`[DoNotParallelize]` and the other three declare `public partial class TimeOutTask_Tests` with no
attributes. `[TestClass]` is not `AllowMultiple`, so applying it to two parts is a compile error and
the attributes must stay on one part only. `partial` also preserves every fully-qualified test name,
which two separate classes would not; several of those names are recorded verbatim in committed #584
evidence artifacts.

| Part | File | Contents | Projected lines |
|---|---|---|---|
| A | `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | `[TestClass]` and `[DoNotParallelize]` on two separate lines (C15, replacing the comma-combined form at line 14); `public partial class ProgressTracker_Tests`; the 17 tests currently at lines 17-266; the `CapturingProgressTracker` nested class currently at lines 81-95, which every test in both parts uses. | 271 |
| B | `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs` | `public partial class ProgressTracker_Tests` with no attributes; the whole "P74 — ProgressTracker core Report/child/root-close behaviour" region currently at lines 270-512, i.e. 7 tests including three `[STATestMethod]` members and the `_dispatcher` reflection site; plus the C26 synchronous sibling test. | 260 plus the new test |

Projected counts are the research record's exact arithmetic over the current file, not estimates:
part A is 268 retained source lines plus two closing braces plus one line from expanding the combined
attribute; part B is 243 moved source lines plus a 15-line preamble plus two closing braces. Both
have more than 200 lines of headroom, so neither a CSharpier re-wrap nor the added C26 test can push
either over 500.

Part B additionally needs `using System.Reflection;`, `using System.Windows.Forms;`, and
`using System.Windows.Threading;`. `[STATestMethod]` ships with the pinned MSTest packages and needs
no new using directive.

**Ordering (scope decision SD6).** The split runs **before** the C12/C13 migration, so the line-count
arithmetic above remains the measured one and the plan's task-level assertions are stable. Migrating
first would shrink the original file to roughly 508 lines — still over the limit, so the split is
mandatory either way — and would then require the migration to be re-applied to the new file.

## Non-Goals

Paths in this section are deliberately unbackticked because this delivery does not modify them. Do
not add backticks here.

- **The C09 behavioral follow-up.** Making `UiThread.Init()` reject non-STA callers is a production
  behavior change that breaks the live worker-thread `UiThread.Init(false)` call in
  QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs. Only the message half of C09 is
  delivered here. The behavioral half is promoted as its own entry under AC8; the research record
  section D carries a drafted body, a blast-radius enumeration, and a recommended
  `bug` / `full-bug` classification for that entry.
- **Finding S4-1** — stale notes under .claude/agent-memory/task-researcher/ that describe
  `UiThread.Dispatcher` as "permanently null in tests" and as producing NREs. Recorded as an upstream
  follow-up for the drm-copilot repository.
- **The S3-1 request to define `Timestamp:` semantics** in the evidence-and-timestamp-conventions
  skill. The skill under .claude/skills/ specifies only `Timestamp: <ISO-8601>` and defines no
  semantics for which instant it denotes. Recorded as the same upstream follow-up.
- Both of the two items above live under .claude/, which is overwritten by push-down from
  drm-copilot. Any edit made in this repository is silently lost. This delivery must not modify
  anything under .claude/.
- **Findings needing no action:** C04 (pre-existing non-blocking latch race, untouched by PR #778),
  C07 (expression-bodied getter; premise refuted, `.editorconfig` preference is silent), C17
  (class-level `[DoNotParallelize]` is defensible per plan rationale and repository precedent), C22
  (`ProgressTrackerPane` double read; setter is private and set-once, no production path can swap
  between reads), C24 (`WpfUiDispatcher`; exception-type change only), S4-2 (evidence-scope
  observation; CI ran every test assembly).
- **The `IUiDispatcher` seam conversion** replacing the remaining direct reads of
  `UiThread.Dispatcher` across production files. Out of scope here and tracked elsewhere; this
  delivery only corrects the figure the #584 spec quotes for it.
- **Adding the `.claude` worktree-exclusion guard to** scripts/vscode/Invoke-MSTest.ps1, and the
  wrong-filename docstring in the same script. Both are separate PowerShell production changes; if
  wanted, promote them as their own entries rather than folding them in.
- **artifacts/csharp/coverage.xml** is deliberately not produced. See the Constraints section.

## Constraints

1. **No temporary files in tests.** The General Unit Test Policy prohibits creating or using
   temporary files in tests, with no currently approved exceptions. This binds the C10 STA sentinel,
   the C21 fresh-thread test, and the C14 cleanup.
2. **STA sentinel discipline.** Any test that obtains a real `Dispatcher` must do so on a dedicated
   STA thread, must call `BeginInvokeShutdown` on that dispatcher, and must join the thread in a
   `finally` block, so no dispatcher outlives the test. Two verified in-repo patterns exist: the
   `StaDispatcherHost` nested class in `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`
   (lines 172-199), which is the closest match to what C10 asks for, and the inline
   `Thread` / `SetApartmentState` / `Join` form with exception capture in
   `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` (lines 132-199). Global STA is
   intentionally disabled in this repository; STA is opt-in per UtilitiesCS.Test/test.runsettings.
3. **C21's thread must be fresh.** The C21 test must reach the production fallback provider, which
   requires a thread whose `Dispatcher.FromThread` is null while the `UiThread` static is null. On a
   pooled MSTest worker, `Dispatcher.FromThread` returns non-null if any earlier test on that same
   thread ever touched `CurrentDispatcher` — the exact hazard C10 fixes. The test must therefore run
   its Act on a dedicated fresh thread that never touches `CurrentDispatcher`, and join it.
   `[DoNotParallelize]` alone does not remove that coupling.
4. **C14 pairs with serialization (scope decision SD7).** The C14 cleanup unsubscribes the heartbeat
   handler. `ApplicationIdleTimer.Unsubscribe` calls `Stop()` when the invocation list empties, and
   `Stop()` touches process-global `System.Windows.Forms.Application.Idle` and
   `ApplicationIdleTimer.Guard` state shared with `IdleAsyncQueue_Tests` and
   UtilitiesCS.Test/Threading/ApplicationIdleTimer_Tests.cs. `[DoNotParallelize]` must therefore be
   added to `IdleActionQueue_Tests` in the same edit, matching the precedent that
   ApplicationIdleTimer_Tests already sets.
5. **500-line limit.** Every touched test file must end under 500 lines. This is a hard CLAUDE.md
   rule with no baseline exemption.
6. **csproj registration.** Every new file must be registered as exactly one `<Compile Include>`
   entry in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. Duplicate `<Compile Include>` entries are a
   known past defect in this project (CS2002, issue #394), so the plan must assert exactly one entry
   per new file. Follow the file's conventions: four-space indent, one self-closing element per line,
   Windows backslash separators, appended adjacent to the sibling entry rather than sorted.
7. **Evidence files are edited in place.** No #584 evidence file is renamed. No existing `Timestamp:`
   value is altered. The S3-4 remedy is an inserted note, not a rename or a re-stamp.
8. **Message-change grep discipline.** The C06/C09 message change breaks exactly one assertion,
   `UtilitiesCS.Test/Threading/UiThread_Tests.cs` line 152. Grep for `UiThread.Initialize()` across
   all projects before and after; the other occurrences at UiThread_Tests.cs line 113,
   IdleAsyncQueue_Tests.cs line 156, and WpfDispatcherYieldTests.cs line 122 are prose, not
   assertions. The line 113 occurrence is refreshed because the file is already in the write set. The
   IdleAsyncQueue_Tests.cs passage at lines 155-160 is not factually wrong and is deliberately left;
   record that as a decision, not an omission.
9. **Bugfix workflow scope.** This is a Refactor. The bugfix workflow's failing-regression-test-first
   requirement applies only to C10 and C02. Both are latent-window defects: C10's hazard is a leaked
   dispatcher that only manifests when a later test on the same pooled thread resolves
   `Dispatcher.FromThread`, and C02's is a torn double read of a non-volatile static. A deterministic
   in-suite failing test is likely to be structurally impossible for both. If so, record a
   fail-before-exception dossier under this feature's evidence/regression-testing/ sub-path
   rather than asserting a fail-before run that did not happen. That route is the one the
   evidence-and-timestamp-conventions skill prescribes.
10. **Test assemblies to run.** This delivery touches the UtilitiesCS, TaskMaster, UtilitiesCS.Test,
    and QuickFiler.Test projects, so at minimum UtilitiesCS.Test.dll,
    QuickFiler.Test.dll, and TaskMaster.Test.dll must be run. Naming all nine test assemblies avoids
    finding S4-2 recurring. Four shell-icon test classes stall on the local workstation for
    environmental reasons that reproduce against main; exclude them with a TestCaseFilter and rely
    on CI, and expect the `TryAddValuesAsync` flake tracked as issue #780.
11. **Coverage evidence (scope decision SD1).** artifacts/csharp/coverage.xml is deliberately not
    produced for this delivery. The repository coverage pipeline emits Cobertura while the
    feature-review coverage hook parses JaCoCo, so the path requires a throwaway conversion; and the
    hook applies a fixed repository-wide line floor that would force a FAIL verdict for a shortfall
    that pre-exists on origin/main. Coverage evidence is instead a compact package-level JaCoCo
    summary committed under this feature's evidence/qa-gates/ sub-path. AC9's operative requirement is that
    changed-line coverage does not decrease.
12. **Coverage figures must be re-derived before they are quoted.** Orchestrator scope decision SD1
    reports first-party line and branch coverage figures for the branch base. Those figures are not
    derived in research/research.2026-09-05T16-10.md and carry no Numeric Derivation Evidence, so
    this spec does not assert them and no acceptance criterion depends on them. Any artifact that
    quotes a coverage figure must re-measure it and record the derivation.

## Corrections to issue.md Encoded Here

These are places where the requirements source is superseded. Each is an orchestrator scope
decision, not a unilateral change.

| # | issue.md text | Correction |
|---|---|---|
| SD3 | S3-5 covers "the three named evidence files" | S3-5 covers all fifteen deviating files enumerated above. |
| SD4 | (silent) | The C06 assertion changes but the test method name `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` is deliberately **not** renamed. Its fully-qualified name is quoted verbatim inside a TestCaseFilter expression in a committed #584 regression-testing evidence artifact; renaming would make that recorded command resolve to zero tests. The residual naming inaccuracy is recorded in this delivery's code-review artifact. |
| SD5 | (silent) | The shared constant drops the `WpfDispatcherYield` message's "before yielding folder tree work" tail. Intended; pinned by AC10. |
| SD7 | "add a `TestCleanup` to `IdleActionQueue_Tests`" | The cleanup is implemented in full **and** `[DoNotParallelize]` is added to the class. |
| SD8 | Test Conditions: "`InitializeAsync` with null dispatcher throws synchronously" | **Incorrect.** `ProgressTrackerAsync.InitializeAsync` is `public async Task<ProgressTrackerAsync>`, so the guarded read faults the returned task rather than throwing at the call site. The C26 test must use `Func<Task> act = () => tracker.InitializeAsync();` with `await act.Should().ThrowAsync<InvalidOperationException>()`. A synchronous assertion would fail. A second test asserts the genuinely synchronous throw from `ProgressTracker.Initialize()`, which is not async; that also closes C26's second named gap. |
| SD9 | S3-9: the follow-up "is satisfied by C26 in this delivery" | **Incorrect.** #584 finding F5 asks for synchronization around the existing unsynchronized reflective mutation of `UiThread._dispatcher`. That is discharged by C12/C13, the single shared install scope that all four `UtilitiesCS.Test` sites migrate to, not by C26, which adds a new test and changes no existing mutation. The artifact note must cite C12/C13 and may cite C26 as adjacent coverage. It must also record that the follow-up was verifiably never promoted: no potential entry and no active feature folder covers it, and the two recommendations that asked for it remain open. |
| SD10 | S3-7: "reconcile the call-site counts to the grep-verified figure" | The #584 spec document carries **49 live reads across 25 production files**, with the derivation cited. The PR #778 review body states 49 reads in 26 files. The artifact records the 25-versus-26 divergence rather than silently adopting either figure; the review body does not publish its member set, so the source of the extra file cannot be established. |

## Items Requiring Re-derivation at Planning Time (SD11)

The research record did not verify the two items below. The plan must re-derive each before writing
any assertion that depends on it. Do not carry these forward as established facts.

1. **The #584 spec document's acceptance-criteria block state (S3-6).** The PR review body states that all
   seven of that spec's acceptance criteria are checked while its Status still reads "Draft". The
   research did not read the AC checkboxes line by line, because the S3-6 remedy is a Status change
   either way. If the plan asserts the AC state in an audit amendment, it must read the AC block
   first.
2. **Two line references into the #584 plan file.** The S3-2 section 8 gap entry is expected to cite
   that plan's P4-T1 rationale, and the C16 discussion cites its "baseline + 1" file-size clause. Both
   line numbers come from the PR review body and were not re-verified; the plan file was not read.
   Confirm both before quoting them.

## Traceability

Every in-scope finding identifier, the file it changes, and the acceptance criterion that covers it.

| ID | File(s) changed | AC |
|---|---|---|
| C01 | `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | AC4 |
| C02 | `UtilitiesCS/Threading/UiThread.cs` | AC1 |
| C03 | `UtilitiesCS/Threading/UiThread.cs` | AC2 |
| C05 | `UtilitiesCS/Threading/UiThread.cs` | AC2 |
| C06 | `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | AC2, AC10, AC11 |
| C08 | `UtilitiesCS/Threading/UiThread.cs` | AC2 |
| C09-message | `UtilitiesCS/Threading/UiThread.cs` | AC2, AC10 |
| C10 | `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | AC1 |
| C11 | `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | AC2 |
| C12 | `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | AC5 |
| C13 | `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | AC5 |
| C14 | `UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs` | AC2 |
| C15 | `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | AC2, AC6 |
| C16 | `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | AC1, AC6 |
| C18 | `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | AC1, AC5 |
| C19 | `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | AC1 |
| C20 | `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | AC1, AC10 |
| C21 | `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | AC2, AC7 |
| C23 | `UtilitiesCS/Threading/ProgressTracker.cs`, `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | AC4 |
| C25 | `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | AC2 |
| C26 | `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs` | AC2, AC7 |
| S2-1 | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | AC2 |
| S3-1 | `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p1-t4-expect-fail.md`, `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p3-t1-analyzer-build.md`, `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/feature-audit.2026-09-04T04-05.md`, `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/policy-audit.2026-09-04T04-05.md` | AC3 (artifact softening); AC8 (the `Timestamp:` semantics request, upstream) |
| S3-2 | `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/policy-audit.2026-09-04T04-05.md`, `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/feature-audit.2026-09-04T04-05.md` | AC1, AC12 |
| S3-3 | `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/policy-audit.2026-09-04T04-05.md` | AC3 |
| S3-4 | `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/issue-updates/issue-584.2026-09-02T09-02.md` | AC3 |
| S3-5 | the fifteen evidence files enumerated in the S3-5 member set above | AC3 |
| S3-6 | `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` | AC3, AC12 |
| S3-7 | `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` | AC3 |
| S3-8 | `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/feature-audit.2026-09-04T04-05.md`, `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/code-review.2026-09-04T04-05.md`, `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/policy-audit.2026-09-04T04-05.md`, `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p2-t3-file-size.md` | AC3 |
| S3-9 | `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/code-review.2026-09-04T04-05.md`, `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/policy-audit.2026-09-04T04-05.md` | AC3 |

## Acceptance Criteria

- [ ] AC1: Each of the seven Should-fix findings is resolved as this spec specifies — C10 (sentinel
      obtained on a dedicated STA thread and shut down in a `finally`, populated-branch test
      retained), C02 (getter reads the backing field exactly once), C18 (order-independence guard
      reads through `UiThreadDispatcherFixture.Current`), C19 (the P27-T2 docstring, Act comment, and
      `NotThrow` reason all describe the synchronous `InvalidOperationException` path), C20 (comment
      corrected, both throw sites routed through the shared constant, `WithMessage` assertion added),
      C16 (partial-class split), S3-2 (both formatter command cells corrected to the scoped six-path
      form, row 3.1 amended, Appendix B labelled as a reference command, section 8 gap entry added).
      **Evidence:** the branch diff for each named file, plus a passing run of `UtilitiesCS.Test` and
      `QuickFiler.Test` recorded under this feature's evidence/qa-gates/ sub-path.
- [ ] AC2: Each of the fourteen in-scope code and test nits — C03, C05, C06, C08, C09 (message half),
      C11, C12, C13, C14, C15, C21, C25, C26, S2-1 — is resolved, or its omission is recorded with a
      stated reason in this delivery's code-review artifact. The C03 clause is satisfied when
      `UtilitiesCS/Threading/UiThread.cs` contains a catch around `Initialize()` that assigns a fresh
      single-shot guard and rethrows the original exception unchanged, and the code-review artifact
      records why no unit test covers that branch. **Evidence:** one diff hunk per identifier, mapped
      by the traceability table; the code-review artifact for any omission.
- [ ] AC3: Each of the eight in-scope documentation and evidence nits is resolved in the #584 feature
      folder, with these amendments: S3-5 is applied to all fifteen files in the S3-5 member set
      above, not only the three named in issue.md (SD3); S3-9's note cites C12/C13 as the discharging
      item and records that the follow-up was never promoted (SD9); S3-7 states 49 live reads across
      25 production files with the derivation cited and records the review body's 26-file figure as a
      divergence (SD10); S3-1 covers only the four artifact softenings, the `Timestamp:`-semantics
      request being out of scope under AC8. **Evidence:** a grep over the #584 evidence subtree in
      which every `EXIT_CODE:` line matches a single signed integer and nothing else; a `git diff`
      over the #584 folder listing exactly the files named in the Write Set sections above; a grep
      over the four audit artifacts returning zero occurrences of the six evaluative spans S3-8
      names.
- [ ] AC4: The two optional refuted-item cleanups are applied. `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`
      contains no `dispatcher != null` comparison and its two XML-doc mentions of `UiThread.Dispatcher`
      are unchanged; `UtilitiesCS/Threading/ProgressTracker.cs` and
      `UtilitiesCS/Threading/ProgressTrackerAsync.cs` each pass the captured `UiDispatcher` local into
      the marshalling lambda and no longer re-read the static inside it. **Evidence:** the diff for
      the three files, plus a grep confirming zero remaining `UiThread.Dispatcher` reads inside those
      two lambdas.
- [ ] AC5: `UtilitiesCS.Test` contains exactly one acquisition of a `FieldInfo` for
      `UiThread._dispatcher`, in `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs`, and the
      four former sites listed in the migrating-sites table all use that scope.
      `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` contains no `FieldInfo` for
      `_dispatcher`. At least one migrated test in `UtilitiesCS.Test/Threading/UiThread_Tests.cs`
      installs a non-null dispatcher over a null prior value and asserts, after the scope is
      disposed, that the static is null again. **Evidence:** a grep over all `*.cs` files in the
      repository for the single-line token `"_dispatcher"` returning exactly two hits — the new scope and the unchanged
      QuickFiler.Test fixture — and the named restore test passing. The conjunction
      `GetField("_dispatcher"` is not used as the evidence method, because CSharpier wraps every
      acquisition so that `GetField(` and `"_dispatcher",` never share a line, and a line-oriented
      search for the conjunction therefore returns zero lines whatever the executor does.
- [ ] AC6: `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` and
      `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs` are each strictly under 500
      lines, both are registered as exactly one `<Compile Include>` entry in
      `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, both declare the same `partial class` with the
      `[TestClass]` and `[DoNotParallelize]` attributes on separate lines in exactly one part, and
      every test method that existed in the pre-split file is still discovered and passing under its
      original fully-qualified name. **Evidence:** a line-count artifact for both files; the csproj
      diff; a before-and-after test-name list from the `UtilitiesCS.Test` run.
- [ ] AC7: Three new tests exist and each fails if its corresponding throw is removed and passes on
      the current code — the C21 test that reaches the production fallback provider from a dedicated
      fresh thread with no dispatcher, the C26 asynchronous test asserting
      `ThrowAsync<InvalidOperationException>` from `ProgressTrackerAsync.InitializeAsync`, and the
      C26 synchronous sibling asserting the direct throw from `ProgressTracker.Initialize()`.
      **Evidence:** a fail-before / pass-after artifact under this feature's
      evidence/regression-testing/ sub-path recording each test's result with the guard temporarily
      removed and restored.
- [ ] AC8: The C09 behavioral follow-up (making `UiThread.Init()` reject non-STA callers) is promoted
      as its own potential entry through the promotion lifecycle and carries a GitHub issue number;
      and the S4-1 stale agent-memory notes together with the S3-1 request to define `Timestamp:`
      semantics are both recorded as upstream follow-ups for the drm-copilot repository. Neither is
      fixed in this repository. **Evidence:** the promoted entry file plus its issue URL, and the
      upstream follow-up record in this delivery's artifacts; plus a `git diff --stat` showing zero
      changed files under .claude/.
- [ ] AC9: The full C# toolchain passes in a single final pass — CSharpier format then check,
      analyzer build, nullable build, and the test run with coverage over the named assemblies — and
      changed-line coverage does not decrease. A package-level coverage summary is committed under
      this feature's evidence/qa-gates/ sub-path; artifacts/csharp/coverage.xml is not produced
      (SD1). **Evidence:** one gate artifact per toolchain step with its exact command and exit code,
      plus the changed-line coverage figure with its derivation.
- [ ] AC10: `UtilitiesCS/Threading/UiThread.cs` declares exactly one `internal const string` message
      constant whose value is the text stated in the Behavioral Contract section; both throw sites —
      the one in that file and the one in `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` —
      reference it, and no `InvalidOperationException` message literal for this precondition remains
      anywhere in `UtilitiesCS`. The `WpfDispatcherYield` message's former "before yielding folder
      tree work" tail is intentionally gone; that loss is recorded in this delivery's code-review
      artifact as an accepted, reviewed change rather than a regression, and is pinned by the C20
      `WithMessage` assertion in
      `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`. **Evidence:** a grep for
      "before yielding folder tree work" returning zero hits in `UtilitiesCS`; a grep for
      `UiThread.Initialize()` returning zero hits in any message literal or assertion; the passing
      `WithMessage` assertion.
- [ ] AC11: The test method `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize`
      in `UtilitiesCS.Test/Threading/UiThread_Tests.cs` retains that exact name while its assertion
      changes to `*UiThread.Init()*`, and this delivery's code-review artifact records the residual
      naming inaccuracy and the reason the name is retained: the fully-qualified name is quoted inside
      a TestCaseFilter expression in a committed #584 regression-testing evidence artifact, and renaming would
      make that recorded command resolve to zero tests (SD4). **Evidence:** a grep confirming the
      method name is unchanged and the asserted wildcard is `*UiThread.Init()*`; the code-review
      artifact entry.
- [ ] AC12: Neither of the two items listed under "Items Requiring Re-derivation at Planning Time" is
      asserted in any artifact without a fresh derivation recorded in this delivery's evidence —
      specifically the #584 spec document's acceptance-criteria block state used by S3-6, and the two line
      references into the #584 plan file used by the S3-2 section 8 entry and the C16 rationale. If a
      re-derivation is not performed, the corresponding assertion is omitted rather than carried
      forward. **Evidence:** a re-derivation artifact under this feature's evidence/baseline/
      sub-path quoting the current text at each location, or an explicit record that the assertion
      was dropped.

## Toolchain

Run in this exact order; restart from step 1 if any step fails or changes files.

1. `dotnet tool run csharpier format .`, verified with `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe` over the explicit test-assembly paths with the /EnableCodeCoverage switch, subject to
   the assembly list and TestCaseFilter constraints stated in Constraints 10.
