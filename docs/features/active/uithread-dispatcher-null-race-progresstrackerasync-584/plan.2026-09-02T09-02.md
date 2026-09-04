# uithread-dispatcher-null-race-progresstrackerasync (Plan)

- **Issue:** #584
- **Work Mode:** full-bug (AC source is `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md`; no `user-story.md` exists or is expected)
- **Owner:** drmoisan
- **Last Updated:** 2026-09-03
- **Status:** In execution, revised mid-flight (revision round 17; findings D1-D11 from preflight round 1, B1-B3 and NB1-NB6 from preflight round 2, C1-C3 and N1-N2 from preflight round 3, E1-E3 and N-1/N-2/N-3 from preflight round 4, F1 from preflight round 5, and G1 plus O1-O4 from preflight round 6 applied, revision round 9 (backtick-removal presentation fix for the parallel-scheduling blast-radius harvester) applied, revision round 10 (mechanical backtick-removal correction: stripped backtick-wrapping from CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md, .claude/rules/quality-tiers.md, .claude/rules/csharp.md, .claude/rules/tonality.md, .claude/agent-memory/**, .claude/, .codex/, .agents/, config/blast-radius.json, config/orchestration-routing.json, and bare config/ mentions across plan.md and spec.md; no task, AC, command, or evidence-path substance changed) applied, revision round 11 (BASE re-anchor after the orchestrator's origin/main reconciliation merge) applied, revision round 15 (scope widened to a sixth owned file after P4-T6 found a reflective consumer no census in this plan had enumerated) applied, revision round 16 (preflight round 16 findings B1, B2, NB6, NB7 applied) applied, revision round 17 (preflight round 17 non-blocking findings N1-N4 applied) applied)
- **Version:** 2.1
- **Branch:** `bug/uithread-dispatcher-null-race-progresstrackerasync-584`
- **BASE (merge base with `origin/main`):** `87cb4df338322844abfa580abea14df77e738e5c`

**BASE re-anchor (revision round 11, 2026-09-03).** The orchestrator merged `origin/main` into this
item branch to reconcile seven sibling items that had merged upstream. The merge is recorded in this
worktree's git log as `merge origin/main: Merge made by the 'ort' strategy` producing commit
`a2ef517b`. It touched none of the five source files this plan wrote at that time — the sixth was
added in revision round 15, after this merge, and the merge did not touch it either: citations 63 through 66 below
re-read the owned-file anchors against the post-merge tree and found them unchanged apart from four
`.csproj` line numbers, which are corrected in this round, and `atomic-executor` separately measured
`git diff --name-status 87cb4df338322844abfa580abea14df77e738e5c..HEAD` in this worktree as listing
exactly the four feature-folder documents and zero source paths. The merge base with
`origin/main` therefore moved from the promotion-time value `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1`
to the value stated above, which is also the current tip of `origin/main`. Every `git diff` command
and every acceptance clause in this plan is anchored to the value stated above; no command in this
plan is anchored to the superseded value. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/issue.md`
line 8 still records the promotion-time value and is deliberately NOT edited by this plan, because
that file is outside this plan's write set; citation 19 and the corresponding `CITATION:` line at the
end of this file state that explicitly. Where a "Citations re-derived in the revision pass of ..."
section below names `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1`, it records the then-current merge base
as a historical observation of an earlier round and does not assert the current BASE.

**Scope widening (revision round 15, 2026-09-03).** `atomic-executor` executed this plan from P0-T1
through P4-T5 and stopped at P4-T6. That task ran the full `QuickFiler.Test` assembly and returned
`Total tests: 1312`, `Passed: 1304`, `Failed: 8` against a P0-T11 baseline of 1312 of 1312 with an
empty `BASELINE_FAILURE_SET`. All eight failures are every `[TestMethod]` in
`QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`, and the executor attributed them to P2-T1
by an executed counterfactual rather than by inference: with BASE `UiThread.cs` restored to the
working tree and the solution rebuilt, the same eight tests passed; with the fixed file in place they
failed. That evidence is recorded in
`docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t6-quickfiler-tests.md`.

The mechanism is that this test class snapshots the process-global dispatcher through the PUBLIC
`Dispatcher` property by reflection, in its `[TestInitialize]` and `[TestCleanup]`, rather than
through the private `_dispatcher` backing field the three `UtilitiesCS.Test` sites use. P2-T1 makes
that getter throw when the field is null, and `PropertyInfo.GetValue(null)` on a throwing getter
propagates the exception, so the class's setup fails outright.

The gap that produced it is a census gap, and it is closed by a new task rather than by a narrative.
This plan ran `git grep -F '"_dispatcher"'` as the census for reflective reads of the private FIELD
(P0-T13) and correctly found three files. It never ran the corresponding census for reflective reads
of the PROPERTY name, and `spec.md`'s Risks & Mitigations rested on
`git grep -n "UiThread.Dispatcher\b"`, which matches only the literal qualified member expression and
therefore cannot match `typeof(UiThread).GetProperty("Dispatcher", ...)` at all. New task P0-T14 runs
that missing census across all nine test assemblies and repository-wide across `.cs` files, and
records its full result.

This round therefore: adds `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` as a sixth owned
file; adds P0-T14 (the property-name census) and P2-T4 (the reflection-target repair); widens the
owned-path lists and pathspecs in P4-T1, P4-T2, P4-T8, P5-T9, P5-T10, P5-T11, P5-T12, and P5-T13 to
six paths; restates P4-T6's acceptance to require `1312` of `1312` passing; and amends `spec.md`'s
AC4, Write Set, Scope, and Risks sections. It changes no task ID and renumbers nothing.

**Phase 4 first-pass marks cleared by this round.** P4-T1 through P4-T5 completed one pass and are
returned to `[ ]` in this round, because P2-T4 rewrites a tracked source file after that pass ran.
AC6 requires the toolchain to pass "in order in a single final pass", and the four steps' first-pass
artifacts describe a tree that no longer exists, so leaving them checked would let AC6 be claimed on
observations that predate the last source edit. The first pass is not erased: its outcome is recorded
in this note and P4-T8's acceptance requires the loop-closure artifact to record every pass in
chronological order, the first one included. Every
task in Phase 0, Phase 1, Phase 2 (P2-T1 through P2-T3), and Phase 3 keeps the check state the
executor left, because none of their acceptance conditions observes the file P2-T4 writes: P0-T13's
and P1-T5's censuses are pathspec-scoped to `UtilitiesCS.Test`, P2-T2's and P2-T3's spans name their
own files, P3-T4's names one file, P3-T5's diff pathspec is `UtilitiesCS UtilitiesCS.Test`, and
P3-T6 runs a different class in `QuickFiler.Test`. P3-T5's pathspec is the one span this widening
would otherwise leave short of AC5's "anywhere in the diff" wording, and P2-T4 carries the identical
seven-token filter over its own file so that gap is closed by a task rather than by a re-run of a
completed one.

**Fail-closed evidence rule:** Every baseline, QA-gate, regression, and coverage-comparison task below
names its artifact path. If a required artifact is missing or is missing any of `Timestamp:`,
`Command:`, `EXIT_CODE:`, `Output Summary:`, the outcome is BLOCKED or INCOMPLETE, never PASS.

**Evidence location invariant:** all evidence for this item is written under
`docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/` in the
sub-kinds `baseline/`, `regression-testing/`, `qa-gates/`, `issue-updates/`, and `other/`. No evidence
is written under `artifacts/`.

---

## Scope: files this plan's diff writes

Production and test source (exactly six files):

- `UtilitiesCS/Threading/UiThread.cs` — the `Dispatcher` accessor and its backing field (P2-T1).
- `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — one added `using`, one added `[TestClass]`
  (P1-T1, P1-T2).
- `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` — attribute-only addition (P1-T5).
- `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` — attribute-only addition (P1-T5).
- `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` — attribute-only addition (P1-T5).
- `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` — reflection-target-only change to the
  class's snapshot/restore helper: the private static `FieldInfo`/`PropertyInfo` cache and the two
  `GetValue(null)` call sites in `[TestInitialize]` and `[TestCleanup]` (P2-T4).

The three attribute-only files carry that change and nothing else: no assertion, no test body, no
`using`, and no member in those files is added, removed, or reordered. That constraint is what keeps
them compatible with AC4's "unmodified assertions" wording; P1-T5 states the enforceable form of it.
`QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` is held to the same standard by P2-T4: it
retargets one reflection lookup and renames the private static field that caches it, and it changes
no assertion, no test method signature, no `[TestMethod]`, and no `using` directive.

**The sixth path contains a space and MUST be quoted in every command that names it.** The directory
is spelled `Helper Classes`, so every `csharpier`, `git add`, `git commit`, `git status`, `git diff`,
`git grep`, and `wc -l` operand naming it in this plan is written as the double-quoted
`"QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"`. An unquoted operand would be split into
two words by the POSIX shell every command block in this plan runs through, and `git` would then
report `QuickFiler.Test/Helper` as an unmatched pathspec — a task failure, not a silent one, but one
that is avoided by quoting rather than diagnosed.

### Why three additional test files are in scope (re-derived this pass)

UtilitiesCS.Test/Properties/AssemblyInfo.cs line 18 declares `[assembly: Parallelize(`, so classes
in this assembly run concurrently by default. Exactly three files in `UtilitiesCS.Test` reflect over
the process-global `UiThread._dispatcher` backing field and write it:
`UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` line 144,
`UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` line 138, and
`UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` line 422. Re-derived this pass with
`git grep -n -F '"_dispatcher"' -- UtilitiesCS.Test`, which returns exactly those three lines in
exactly those three files. None of them is among the 18 files in `UtilitiesCS.Test` that already
carry `[DoNotParallelize]`.

Marking only the new test class `[DoNotParallelize]` would rest the new class's determinism on an
assumption about how the MSTest adapter orders its parallel and non-parallel buckets — an assumption
this plan cannot verify against the repository tree. This plan instead states the isolation guarantee
in a form that is verifiable against the tree by a single command: **after P1-T5, zero writers of
`UiThread._dispatcher` remain in the parallel bucket**, because every file in `UtilitiesCS.Test` that
names that field carries the `DoNotParallelize` attribute on its test class. That is what P1-T5
asserts and what P0-T13 baselines. It also removes, as a side effect, the concurrent-writer hazard that the new
negative test would otherwise share with the two existing tests that install a real dispatcher and
pump a `DispatcherFrame` (`UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` lines 150, 152,
and 162).

### Why a sixth test file is in scope (re-derived in the revision round 15 pass)

`QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` is the only file in the whole repository
that reads `UiThread.Dispatcher` reflectively through the PUBLIC property. Re-derived in this pass by
searching every tracked `.cs` file in the worktree for the literal `"Dispatcher"`, which returns five
lines in five files: this file's line 35, and four `<see cref="Dispatcher"/>` XML documentation
references (`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` line 14,
`UtilitiesCS/Threading/WpfUiDispatcher.cs` line 14, `UtilitiesCS/Threading/ThreadMonitor.cs` line 25,
and `UtilitiesCS/Threading/IUiDispatcher.cs` line 13). Only the first is a reflection site; the other
four name the WPF type in a documentation cross-reference and invoke nothing.

The broader safety net was re-derived in the same pass by searching every tracked `.cs` file for
`typeof(UiThread)`, which returns seven lines in seven files. Six of the seven take a FIELD:
`UtilitiesCS.Test/Threading/UiThread_Tests.cs` line 127 (added by P1-T2),
`UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` line 421,
`UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` line 138,
`UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` line 144,
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` line 135, and
`UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` line 469 — and the last of those
takes `_uiSyncContext`, not `_dispatcher`, so it is unrelated to this fix. The seventh is
`QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` line 34, the single `GetProperty` call.
P0-T14 runs both searches as executable commands with the repository-wide `'*.cs'` pathspec and
records their full output, so the census is auditable rather than asserted.

The repair is to change that one lookup to the field the other six sites already use. Reading
`_dispatcher` observes exactly the state the property getter reads, and observes it without invoking
the guard P2-T1 installed. The alternative — catching or tolerating the exception in the test's setup
— was rejected: it would encode into a test the premise that a null dispatcher is an acceptable
observed state, which is the premise this whole change exists to reverse, and every other consumer in
this fix treats that state as a real defect condition.

### Pre-existing file-size overage recorded, not deepened

`UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` is 514 lines at BASE, already above the
500-line limit in the rule file .claude/rules/general-code-change.md. That overage is pre-existing
and is not caused by this change. To avoid deepening it, P1-T5 adds the attribute to that one file by
extending its existing attribute list on line 14 to `[TestClass, DoNotParallelize]` rather than
adding a line. The other two files have ample headroom (347 and 205 lines) and use the repository's
prevailing two-line idiom, which is `[TestClass]` on one line and `[DoNotParallelize]` on the next
(the existing pattern in UtilitiesCS.Test/Threading/CurrentStoreContextTests.cs, lines 15-16,
re-derived this pass).

Documentation and evidence written by this plan:

- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/issue-updates/`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/other/`

Explicitly NOT written by this plan (verified this pass, see P0-T3 and P3-T4):
UtilitiesCS/Threading/ProgressTrackerAsync.cs, UtilitiesCS.Test/UtilitiesCS.Test.csproj,
UtilitiesCS/UtilitiesCS.csproj, QuickFiler.Test/QuickFiler.Test.csproj, and anything under the Claude
runtime tree at .claude/, the Codex mirror tree at .codex/, the dot-agents tree at .agents/,
config/blast-radius.json, or config/orchestration-routing.json.

QuickFiler.Test/QuickFiler.Test.csproj needs no edit for the sixth owned file: line 206 already
carries `<Compile Include="Helper Classes\EmailMoveMonitorTests.cs" />`, re-derived in the revision
round 15 pass. That project uses the same explicit `Compile Include` wiring
UtilitiesCS.Test/UtilitiesCS.Test.csproj uses, so an EXISTING file requires no project-file change
while a NEW one would; P2-T4 edits an existing file for that reason among others. The file is also
already tracked at BASE, which is what keeps the single-ref `git diff` form used in P2-T4 from being
blind to it.

### Test-file placement decision (made this pass, not deferred to the executor)

The new regression test goes into the EXISTING file `UtilitiesCS.Test/Threading/UiThread_Tests.cs`,
as a second `[TestClass]` alongside the existing `SynchronizationContextAwaiter_Tests`. Two
re-derived facts fix this decision:

1. The file is currently 104 lines. The addition specified in P1-T2 is approximately 75 lines, so the
   post-change file is approximately 180 lines — under the 500-line limit in the rule file
   .claude/rules/general-code-change.md. The alternative file
   UtilitiesCS.Test/Threading/UiThread_Dispatcher_Tests.cs is therefore NOT created.
2. UtilitiesCS.Test/UtilitiesCS.Test.csproj line 494 already carries
   `<Compile Include="Threading\UiThread_Tests.cs" />`. This project uses explicit `Compile Include`
   wiring, so reusing the existing file requires no `.csproj` edit, whereas a new file would. The
   three files P1-T5 touches are wired at the same project's lines 477
   (`Threading\ProgressTracker_Tests.cs`), 479 (`Threading\ProgressTrackerAsync_Tests.cs`), and 490
   (`Threading\IdleAsyncQueue_Tests.cs`), all re-derived this pass against the post-merge tree (each
   entry moved by +1 relative to the round-1 reading; all four are still present), so no `.csproj`
   edit is required
   for them either and UtilitiesCS.Test/UtilitiesCS.Test.csproj stays out of this plan's diff.

---

## Threshold reconciliation (recorded, applied)

CLAUDE.md (rank 1 in `policy-compliance-order`) sets repository line coverage `>= 80%` and new
module/class/method coverage `>= 90%`. The rule files .claude/rules/general-unit-test.md and
.claude/rules/quality-tiers.md (rank 3/4) set `>= 85%` line and `>= 75%` branch. This plan applies
the rank-1 CLAUDE.md figures (`>= 80%` repository line, `>= 90%` new code) and records the
divergence in P0-T12 rather than silently choosing one. The conflict is pre-existing and is NOT
resolved by this bug fix.

The enforced repository-level gate in this plan is **no regression relative to the P0 baseline**
(P4-T7). The absolute `>= 80%` figure is recorded, not gated, because the baseline is measured, not
assumed: a floor asserted against an unmeasured baseline could be unsatisfiable for reasons this
change did not cause.

---

## Acceptance criteria mapping (source: `spec.md` "## Acceptance Criteria", AC1-AC7)

Each row below lists exactly the same evidence-artifact set as the corresponding `AC-MAPPING:` line
in the Planner Internal Review record at the end of this file. The two lists are one derivation, not
two.

| AC | Implementation task | Test task | Evidence task |
|---|---|---|---|
| AC1 | P2-T1 | P1-T2, P1-T4, P3-T2 | `evidence/regression-testing/p1-t4-expect-fail.md`, `evidence/regression-testing/p3-t2-regression-green.md` |
| AC2 | P2-T1 | P2-T2, P4-T4 | `evidence/qa-gates/p2-t2-nullforgiving-removed.md`, `evidence/qa-gates/p4-t4-nullable-build.md` |
| AC3 | P0-T3 (verification, no edit) | P3-T4 | `evidence/other/p3-t4-progresstrackerasync-unmodified.md` |
| AC4 | P1-T5 (attribute-only, no assertion changed), P2-T4 (reflection-target-only, no assertion changed) | P3-T3, P3-T6, P4-T6 | `evidence/qa-gates/p1-t5-donotparallelize.md`, `evidence/regression-testing/p3-t3-at-risk-tests.md`, `evidence/regression-testing/p3-t6-quickfiler-wpfuidispatcher.md`, `evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md`, `evidence/regression-testing/p4-t6-first-pass-failure.md`, `evidence/qa-gates/p4-t6-quickfiler-tests.md` |
| AC5 | P1-T2, P1-T5, P2-T1, P2-T4 | P3-T5 | `evidence/qa-gates/p3-t5-no-timing-tokens.md`, `evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md` |
| AC6 | P4-T1, P4-T3, P4-T4 | P4-T5, P4-T6 | `evidence/qa-gates/p4-t1-format.md`, `evidence/qa-gates/p4-t2-format-check.md`, `evidence/qa-gates/p4-t3-analyzer-build.md`, `evidence/qa-gates/p4-t4-nullable-build.md`, `evidence/qa-gates/p4-t5-utilitiescs-tests.md`, `evidence/qa-gates/p4-t6-quickfiler-tests.md`, `evidence/qa-gates/p4-t8-loop-closure.md` |
| AC7 | P2-T1 | P4-T5, P4-T7 | `evidence/baseline/p0-t10-utilitiescs-tests-coverage.md`, `evidence/qa-gates/p4-t7-coverage-delta.md` |

---

## Exact source text this plan will create (quoted verbatim for gate exoneration)

The `Dispatcher` property region of `UtilitiesCS/Threading/UiThread.cs` (currently lines 135-140)
becomes exactly this, subject only to `csharpier` re-wrapping:

```csharp
        public static Dispatcher Dispatcher
        {
            get
            {
                if (_dispatcher is null)
                {
                    throw new InvalidOperationException(
                        "The UI dispatcher has not been captured. Call UiThread.Init() so that UiThread.Initialize() runs before reading UiThread.Dispatcher."
                    );
                }
                return _dispatcher;
            }
            private set => _dispatcher = value;
        }
        private static Dispatcher? _dispatcher;
```

The literal token asserted by P2-T2 is, verbatim and on one source line:
`private static Dispatcher? _dispatcher;`

The literal token whose disappearance P2-T2 asserts is, verbatim: `null!`

The two MSTest node identifiers created by P1-T2 are, verbatim:

- `UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize`
- `UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance`

The attribute text P1-T5 creates is quoted verbatim here so that the gate reads this quotation as the
executor's instruction rather than as a search for a literal that is absent from the tree today.

In `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, lines 28-29 become exactly:

```csharp
    [TestClass]
    [DoNotParallelize]
```

In `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, lines 13-14 become exactly:

```csharp
    [TestClass]
    [DoNotParallelize]
```

In `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, line 14 becomes exactly this single line,
so that the file's length does not grow past its pre-existing 514:

```csharp
    [TestClass, DoNotParallelize]
```

The single-line token asserted by P1-T5 against all four files is, verbatim: `DoNotParallelize`.
That bare token is used deliberately rather than the bracketed spelling, because it matches both the
two-line idiom and the combined attribute list above.

### The reflection-target change P2-T4 creates, quoted verbatim

`QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` lines 33-37 read exactly this today
(re-derived in the revision round 15 pass):

```csharp
        private static readonly System.Reflection.PropertyInfo DispatcherProperty =
            typeof(UiThread).GetProperty(
                "Dispatcher",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
            );
```

They become exactly this, subject only to `csharpier` re-wrapping:

```csharp
        private static readonly System.Reflection.FieldInfo DispatcherField =
            typeof(UiThread).GetField(
                "_dispatcher",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
```

Line 49 reads exactly `            _capturedDispatcher = DispatcherProperty?.GetValue(null);` today and
becomes exactly:

```csharp
            _capturedDispatcher = DispatcherField?.GetValue(null);
```

Line 58 reads exactly `            object current = DispatcherProperty?.GetValue(null);` today and
becomes exactly:

```csharp
            object current = DispatcherField?.GetValue(null);
```

Six lines are inserted immediately before the `private object _capturedDispatcher;` declaration on
line 32, and therefore immediately after the existing class comment that ends on line 31: a
`        //` spacer followed by five comment lines stating why the field is read instead of the
property. Their text is quoted verbatim here so that it is the plan, and not the executor, that fixes
it:

```csharp
        //
        // The snapshot reads the private _dispatcher backing field rather than the public
        // Dispatcher property (issue #584): the property getter now throws
        // InvalidOperationException when the field is null, and PropertyInfo.GetValue would
        // surface that as a TargetInvocationException from this class's setup and teardown.
        // Reading the field observes the same state without invoking the guard.
```

Those six inserted lines were checked in this pass against the seven-token case-insensitive filter
AC5 uses (`Thread.Sleep`, `Task.Delay`, `SpinWait`, `Retry`, `retries`, `Timeout(`, `PushFrame`) and
contain none of them, which is the same authoring constraint P1-T2 carries for its own doc comment.

The single-line tokens P2-T4's four `git grep` spans assert against that file are, verbatim:

- present exactly once after the change: `"_dispatcher"`
- absent after the change: `"Dispatcher"`
- absent after the change: `GetProperty(`
- present exactly eight times, unchanged from BASE: `[TestMethod]`

Each is a short, single-line, non-interpolated literal that this section quotes verbatim, so none of
them is wrap-fragile and none contains a placeholder. The first three are the false-before/true-after
pair for the retarget and the fourth is the no-test-added check.

The one assertion in the region P2-T4 touches MUST survive the change unaltered. It is, verbatim:

- `            current.Should().BeSameAs(_capturedDispatcher);`

No other assertion in the file is inside the region P2-T4 touches. The enforceable form of "no
assertion changed" for the file as a whole is stated in P2-T4's acceptance as two clauses: the file
still declares exactly eight `[TestMethod]` attributes (the count re-derived in this pass, and the
same eight P4-T6 reported as failing), and the BASE-anchored diff of this file contains no added line
and no removed line carrying the token `.Should()`.

---

## Shell constraints measured in this worktree (binding on every command block below)

Every command block in this plan runs through the executing agent's POSIX (git-bash) shell. Five
constraints of that shell and of the tools it invokes were measured directly in this worktree by the
reviewing executor during preflight rounds 3, 4, 5, and 6, by running the commands rather than by
reading documentation. They are recorded once here and are the reason several command shapes and
several evidence-sourcing rules below are spelled the way they are. They are environment
measurements, not repository-tree citations.

One further rule, the "TRX selection rule", is stated at the end of this section after constraint 5.
It is a plan rule rather than an environment measurement, and it is placed here because it binds on
the same seven tasks constraint 5 binds on and because stating it once centrally is what keeps it
from being present in some of those tasks and absent from others.

1. **A command whose name is `pwsh` is refused outright, in every argument shape.** The refusal is
   keyed on `pwsh` occupying the command position, not on `-Command` versus `-File`. The verbatim
   refusal text is: "this command runs pwsh in a plain command; what it reads or is handed as shell
   text cannot be shown not to run git. Refusing to run it." Consequence: no task in this plan may
   invoke `pwsh`, and the script scripts/vscode/Install-RepoDotNetSdk.ps1 cannot be run from this
   shell in any form. P0-T5 step 1 performs the same download-and-extract with POSIX utilities
   instead. No `pwsh`
   invocation appears anywhere in this plan.

2. **A command whose NAME is a quoted absolute path is refused** ("runs a command whose name is
   computed at runtime in a plain command ... Refusing to run it"). A path used as an ARGUMENT — after
   `--`, or as the value of a parameter — is not refused. Neither `vswhere.exe` nor
   `vstest.console.exe` resolves on `PATH` by bare name. Consequence: every task that runs
   `vstest.console.exe` leads its command line with a `PATH=` prefix so that the command NAME is the
   bare filename `vstest.console.exe`, and the two `dotnet-coverage collect` tasks keep the executable
   as a double-quoted ARGUMENT after `--`, which needs no prefix. The measured proof of the prefix
   form is in P0-T5 step 2.

3. **Bare `msbuild` does not resolve on `PATH` in this shell; `msbuild.exe` does.** Measured:
   `msbuild -version` returns `command not found` with exit 127, and `msbuild.exe -version` exits 0
   printing `MSBuild version 18.9.1+a81b43525 for .NET Framework`. This is a git-bash PATH-resolution
   property — MSYS bash does not append `.exe` when searching `PATH` for a bare name — and not a
   deviation from CLAUDE.md. Every switch set below is character-for-character the one CLAUDE.md
   mandates: no `/p:Nullable=enable` is ever added, and `/t:Rebuild` is always used in place of
   `/t:Build`.

4. **MSYS path conversion rewrites forward-slash switches, and is disabled per command line with a
   leading `MSYS_NO_PATHCONV=1` assignment.** This constraint supersedes an earlier round's narrower
   claim that only the executable SPELLING had to change; that claim was wrong, and it was wrong in a
   way that reading the commands could not reveal. Measured in this worktree during preflight round 4
   by running the commands:

   - `msbuild.exe` receives mangled switches. A single-letter `/m` is converted to `M:/`, `/t:Rebuild`
     to `t:Rebuild`, and `/p:...` to `p:...`, so MSBuild sees several bare operands and fails with
     `MSB1008: Only one project can be specified.` All six `msbuild.exe` command blocks in this plan
     failed this way when run without the prefix.
   - `vstest.console.exe` receives a mangled switch whenever a multi-letter `/Switch` carries NO
     colon. `/InIsolation` is converted to the msys-root path
     `C:/Program Files/Git/InIsolation`, which vstest then treats as a test source, reporting
     `The test source file ... was not found` and running zero tests. All eight vstest and
     `dotnet-coverage` invocations in this plan failed this way when run without the prefix.
     Colon-bearing switches such as `/Logger:trx`, `/ResultsDirectory:...`, `/Settings:...`, and
     `/TestCaseFilter:...` are NOT converted, which is precisely why three rounds of reading these
     command lines did not surface the defect: the only affected switch in the whole set is the one
     without a colon.
   - `msbuild.exe -version` in P0-T5 step 3 needs no prefix and does not carry one. Its only argument
     begins with `-`, not `/`, so nothing is converted. This was verified separately.

   The remedy applied throughout this plan is a single leading environment-variable assignment,
   `MSYS_NO_PATHCONV=1 `, placed on the same command line immediately before the executable name. It
   changes no switch. Where a command line already carries a `PATH=` prefix, `MSYS_NO_PATHCONV=1`
   is placed BEFORE it; the shell accepts any number of leading assignments on one command. The
   fourteen command blocks carrying the prefix are P0-T8, P0-T9, P1-T3, P3-T1, P4-T3, and P4-T4
   (`msbuild.exe`), and P0-T10, P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, P4-T5, and P4-T6 (vstest and
   `dotnet-coverage`). Because the prefix is part of the recorded command line, the acceptance clauses
   in P0-T9 and P4-T4 assert that the recorded line CONTAINS `msbuild.exe TaskMaster.sln` rather than
   that it begins with it. The substantive clauses those gates enforce — no `/p:Nullable=enable`, and
   `/t:Rebuild` rather than `/t:Build` — are unaffected and unchanged.

5. **A green `vstest.console.exe` run prints no `Failed:` line and no `Skipped:` line on its console
   output.** Measured across four green runs in this worktree during preflight round 5 — of 4783,
   1312, 41, and 2 tests, with and without `/Settings:`, and both under and outside
   `dotnet-coverage collect`. The entire summary block a successful run emits is:

   ```text
   Test Run Successful.
   Total tests: 4783
        Passed: 4783
    Total time: 12.8452 Seconds
   ```

   A search of the captured output of each of those four runs for `Failed:` or `Skipped:` returned
   zero matches. EACH of those two aggregate lines is printed only when its OWN counter is non-zero,
   and not merely when the run has failures: a run with a skip and no failure prints `Skipped:` and
   no `Failed:`, and a run with a failure and no skip prints `Failed:` and no `Skipped:`. Preflight
   round 6 confirmed that the two are independent, by running a three-test probe — one passing, one
   failing, one skipped — whose console printed both `Failed: 1` and `Skipped: 1`. That per-counter
   rule is why P1-T4's `[expect-fail]` acceptance can and does read `Failed: 1` from the console —
   P1-T4's run has a non-zero failure count by construction — while every
   green-run task in this plan cannot read a `Failed` count from there at all. Per-test pass and fail
   lines are a separate matter and ARE printed on a green run, so P3-T6's requirement that two named
   tests appear by name as passing in the console output is unaffected.

   Both omitted counts are recoverable from the TRX file that every affected command in this plan
   already writes through its `/Logger:trx` switch. The TRX carries the run's aggregate counts in a
   single element, for example:

   ```text
   <Counters total="4783" executed="4783" passed="4783" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" ... />
   ```

   **How `Failed` and `Skipped` are sourced from that element, and why `notExecuted` is prohibited.**
   `Failed` is read from the `failed` attribute of the single `<Counters .../>` element. `Skipped` is
   DERIVED from that same element as `total` minus `executed`; record `total`, `executed`, and the
   derived `Skipped` value. The `notExecuted` attribute MUST NOT be used: `vstest.console.exe`'s TRX
   logger populates only `total`, `executed`, `passed`, and `failed` on the `<Counters .../>` element
   and hard-codes every other attribute (`notExecuted`, `error`, `timeout`, `aborted`,
   `inconclusive`, ...) to `0` regardless of the run's actual outcome. Measured in preflight round 6:
   a run whose console printed `Skipped: 1` produced a TRX with `notExecuted="0"`; the derivation
   `total` minus `executed` correctly returned `1` on that run and `0` on an all-passing run. In the
   same file, the skipped test's own `<UnitTestResult ... outcome="NotExecuted">` element was present
   and correct, so it is the aggregate `<Counters .../>` element alone that mis-reports. Sourcing
   `Skipped` from `notExecuted` would therefore record a constant `0` whatever the run did — an
   acceptance value that cannot fail, which is the same defect class this constraint exists to remove.

   This plan therefore sources `Total tests` and `Passed` from the console summary block and `Failed`
   from the `failed` attribute in all seven TRX-reading tasks (P0-T10, P0-T11, P3-T2, P3-T3, P3-T6,
   P4-T5, and P4-T6), and derives `Skipped` as `total` minus `executed` in the four of those that
   record a `Skipped` count (P0-T10, P0-T11, P4-T5, and P4-T6); P3-T2, P3-T3, and P3-T6 record no
   `Skipped` figure and read only `Failed`. No command line changes: the `/Logger:trx` and
   `/ResultsDirectory:` switches every one of those tasks already carries are what produce the file,
   and each task writes to its own results directory, so no TRX can be attributed to the wrong task.
   Repeated runs of the SAME task do leave more than one `.trx` file in that task's own directory,
   which is what the TRX selection rule stated below governs. The TRX files are
   gitignored by .gitignore line 39 `[Tt]est[Rr]esult*/` and excluded from the format gate by
   .csharpierignore line 8 `*.trx` (both re-derived this pass), so reading them adds nothing to any
   porcelain, diff, or format gate in this plan.

   **Redaction rule binding on all seven of those tasks.** Record the `total`, `executed`, and
   `failed` values and the derived `Skipped` value, and identify the file they came from by its
   repository-relative results directory
   (`TestResults/p0-t10/`, `TestResults/p0-t11/`, `TestResults/p3-t2/`, `TestResults/p3-t3/`,
   `TestResults/p3-t6/`, `TestResults/p4-t5/`, `TestResults/p4-t6/`). Do NOT record the TRX file's own
   name in any evidence artifact: `vstest.console.exe` composes the default TRX filename from the host
   account name and the machine name, so recording it verbatim would place an account-name disclosure
   into committed evidence. Do NOT quote a vstest run's `Results File:` console line in any evidence
   artifact either: that line carries the full absolute host path of the same TRX, so it discloses the
   account name and the machine name as well as the worktree's absolute location. This is the same
   disclosure P4-T7's redaction rule prevents for the
   Cobertura `filename` attribute, applied to the second artifact class this plan now reads. The four
   recorded numeric values themselves carry no path.

   This constraint was invisible to preflight rounds 1 through 4. Until round 5, the `/InIsolation`
   mangling recorded in constraint 4 above made every `vstest.console.exe` invocation in this plan
   execute zero tests, so no round before round 5 had ever observed what a genuinely successful run's
   console output contains. It is the same defect class the `atomic-plan-contract` rule "Observe a
   command's success-case output before asserting over that output" exists to prevent: a value that
   documentation or intuition suggests is printed, but that the tool does not print on a successful
   run.

   Round 5's own first remedy fell into that same class and was corrected in round 6. That remedy
   sourced `Skipped` from the TRX `notExecuted` attribute, which the tool writes but never populates,
   so the recorded value would have been a constant `0` on every run. Round 6 built the three-test
   probe described above and read the attribute rather than assuming it, which is what produced the
   `total` minus `executed` derivation this constraint now states.

**TRX selection rule, binding on all seven TRX-reading tasks (P0-T10, P0-T11, P3-T2, P3-T3, P3-T6,
P4-T5, and P4-T6).** If a task's results directory holds more than one `.trx` file at the moment that
task's evidence artifact is written, read the most recently modified one, and record in that task's
artifact a line beginning `TRX SELECTED: most recently modified .trx in ` and completed by that
task's own results directory — for P4-T5, exactly
`TRX SELECTED: most recently modified .trx in TestResults/p4-t5/` — together with that file's
last-modified timestamp. Do NOT record the selected file's name: the
redaction rule in constraint 5 binds on this line too, because the default TRX filename carries the
host account name and the machine name, and the last-modified timestamp identifies the selection
without disclosing either.

This rule is stated once here rather than repeated inside individual tasks, because it applies to all
seven of them and because the plan explicitly anticipates re-running three. P3-T3's own text treats a
zero-test run as a failure of that task requiring a corrected filter and a re-run, and P4-T8's
loop-closure text restarts the Phase 4 loop from P4-T1 when a step rewrote a tracked file, which
re-runs P4-T5 and P4-T6. `vstest.console.exe` composes each default TRX filename with a timestamp and
does not overwrite an existing one — reported by the preflight round-6 reviewer, and recorded here as
a reported measurement rather than as a repository-tree citation — so a second run of the same task
leaves two `.trx` files in that task's directory. Without this rule those three tasks would have no
stated way to choose between them and the choice would fall to the executor.

### Worktree state assumed by Phase 0

The worktree in which this plan was authored and reviewed already contains `.dotnet-sdk/` (SDK
8.0.205), a completed `packages/` NuGet restore, and built `Debug` output. All three are gitignored
and none of them is committed by this plan. A FRESH worktree has none of them, so Phase 0's
bootstrap and restore steps in P0-T5 are mandatory and are written to run unconditionally where they
are cheap (`nuget restore`, `dotnet tool restore`) and conditionally where they are expensive and
idempotently detectable (the SDK bootstrap, which runs only when the first `dotnet --version` probe
fails). No task in this plan may be skipped on the assumption that a prior run already performed it.

### Phase 0 — Baseline capture, policy reads, and tree re-derivation

Phase 0 does not assume an empty `git status --porcelain`. This worktree already carries modified or
untracked files under the agent-memory tree at .claude/agent-memory/, written by the planning and
preflight delegations that ran in this same preparation cycle. That state is expected and affects no
gate in this plan: every terminal porcelain and diff gate here is pathspec-scoped to `UtilitiesCS`,
`UtilitiesCS.Test`, and the feature folder, and the two deliberately unscoped porcelain spans in
P4-T1 are compared before-against-after rather than asserted empty, precisely so that ambient state
cannot satisfy or falsify them. This plan's own commits never touch the Claude runtime tree at
.claude/, the Codex mirror tree at .codex/, the dot-agents tree at .agents/,
config/blast-radius.json, or config/orchestration-routing.json, and P5-T10 asserts that.

- [x] [P0-T1] Read the policy files in the order required by `policy-compliance-order`: CLAUDE.md, then .claude/rules/general-code-change.md, then .claude/rules/general-unit-test.md, then .claude/rules/quality-tiers.md, then .claude/rules/csharp.md, then .claude/rules/tonality.md. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:` (the six paths in the order read), and an explicit list of the files read. Acceptance: the artifact exists, lists all six paths, and its `Policy Order:` line matches the order above.

- [x] [P0-T2] Re-derive the defect site by reading `UtilitiesCS/Threading/UiThread.cs` in full. Record: the file's total line count; the line numbers of the `Dispatcher` property and its backing field; the verbatim backing-field declaration line; whether the file carries a nullable-enable directive on line 1; and the line numbers of the two lazy-initialising sibling properties `UiSyncContext` and `AutoScaleFactor`. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t2-uithread-rederivation.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the artifact records a total line count of 163, records that the backing-field line contains `null!`, records the property at lines 135-139 and the field at line 140, and records the nullable-enable directive on line 1. If any of these five recorded values differs from the value stated here, stop and report BLOCKED rather than editing, because the fix text quoted above was derived from them.

- [x] [P0-T3] Re-derive the AC3 hypothesis by reading UtilitiesCS/Threading/ProgressTrackerAsync.cs in full. Record the verbatim text and line number of the statement that assigns `UiThread.Dispatcher` to the instance field, and the verbatim text and line number of the first statement that dereferences that instance field. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t3-progresstrackerasync-rederivation.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the artifact records that line 33 is `UiDispatcher = UiThread.Dispatcher;` and that line 35 is the first dereference (`await UiDispatcher.InvokeAsync(`), and states the conclusion that a throwing getter raises at line 33 before line 35 executes, so no edit to this file is required. If line 33 is not the property read, record the actual ordering, add UtilitiesCS/Threading/ProgressTrackerAsync.cs to the write-target list in the "Scope" section of this plan, and report the overturned conclusion to the caller before proceeding.

- [x] [P0-T4] Re-derive the test-side facts by reading `UtilitiesCS.Test/Threading/UiThread_Tests.cs` in full, reading the `DispatcherField`/`ForceDispatcherNull`/`RestoreDispatcher` helper region of `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, and reading UtilitiesCS.Test/Properties/AssemblyInfo.cs. Record: the total line count of `UtilitiesCS.Test/Threading/UiThread_Tests.cs`; its namespace; its existing `using` directives; the reflection idiom used to reach the private static backing field; and whether the assembly declares class-level parallelisation. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t4-test-rederivation.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the artifact records 104 total lines, namespace `UtilitiesCS.Test.Threading`, absence of a `System.Reflection` using directive, the reflection idiom taking the field by the name `_dispatcher` with non-public static binding flags, and the presence of an assembly-level parallelisation attribute on line 18 of UtilitiesCS.Test/Properties/AssemblyInfo.cs (which is the justification for the do-not-parallelize attribute in P1-T2 and P1-T5).

- [x] [P0-T5] Resolve and record the toolchain entry points. Run these commands from the worktree root, in the order given.

  **1. Probe the .NET SDK before anything that depends on it.**

  ```text
  dotnet --version
  ```

  If this fails with an error containing `The repo-local .NET SDK is missing`, the worktree has no `.dotnet-sdk/` directory, and global.json (`"paths": [".dotnet-sdk", "$host$"]`, re-derived this pass) then resolves to no SDK at all, so every `dotnet` command in this plan — `dotnet tool restore` in this task, `dotnet tool run csharpier --version` in this task, and the formatter and format-check commands in P4-T1 and P4-T2 — fails. Bootstrap the SDK by running, from the worktree root:

  ```text
  mkdir -p .dotnet-sdk
  curl -L -o .dotnet-sdk/sdk.zip https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip
  unzip -q -o .dotnet-sdk/sdk.zip -d .dotnet-sdk
  rm -f .dotnet-sdk/sdk.zip
  ```

  then re-run `dotnet --version`. Record every command actually run and every `dotnet --version` reading taken, in the order taken.

  This is deliberately NOT an invocation of `scripts/vscode/Install-RepoDotNetSdk.ps1`, and that script MUST NOT be run here. The script can only be started through `pwsh`, and a command whose name is `pwsh` is refused outright by this worktree's shell in every argument shape — `-File` and `-Command` alike, because the guard keys on `pwsh` occupying the command position. The verbatim refusal text measured in this worktree is: "this command runs pwsh in a plain command; what it reads or is handed as shell text cannot be shown not to run git. Refusing to run it." The four POSIX commands above perform the same download-and-extract the script performs: the URL is character-for-character the one the script builds at `scripts/vscode/Install-RepoDotNetSdk.ps1` line 26 for its default `$Version` of `8.0.205` and default `$Architecture` of `x64` (re-derived this pass), and `.dotnet-sdk` at the worktree root is the same destination the script resolves at line 36 (`Join-Path $PSScriptRoot '..\..\.dotnet-sdk'`, re-derived this pass). Version `8.0.205` is also what global.json pins, so the acceptance below is unchanged by the substitution.

  The bootstrap leaves nothing in any porcelain or diff gate later in this plan: .gitignore line 350 is `.dotnet*/` (re-derived this pass), which already ignores `.dotnet-sdk/`.

  Fail-closed rule for this step: if any of the four commands exits non-zero — including `curl` failing to download and including `unzip` being unavailable in this shell, which this plan has not measured — record `SDK_BOOTSTRAP: BLOCKED` in the artifact together with the failing command and its verbatim output, and halt. Do not proceed to any later task, because no formatting gate in this plan can run without an SDK and the evidence produced would be unverifiable. Exactly one fallback is authorised, and only for a failure of the `unzip` command specifically: run `PATH="/c/Windows/System32:$PATH" tar.exe -xf .dotnet-sdk/sdk.zip -C .dotnet-sdk`, record it with its own exit code, and declare `SDK_BOOTSTRAP: BLOCKED` if it too exits non-zero. That fallback uses the libarchive `tar.exe` Windows ships in `System32`, which reads zip archives; the GNU `tar` on the shell's default `PATH` does not, which is why the `PATH=` prefix is required to select the right one. No other substitution is authorised for any of the four commands.

  **2. Resolve the `vstest.console.exe` directory** by calling `vswhere.exe` through a `PATH=` prefix, so that the command NAME is the bare filename rather than a quoted absolute path:

  ```text
  PATH="/c/Program Files (x86)/Microsoft Visual Studio/Installer:$PATH" vswhere.exe -latest -products '*' -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"
  ```

  This exact command was run in this worktree during preflight round 3 and printed `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`. The `PATH=`-prefix spelling is required, not stylistic: a command whose NAME is a quoted absolute path is refused by this worktree's shell with "runs a command whose name is computed at runtime in a plain command ... Refusing to run it", so the previously planned `"/c/Program Files (x86)/.../vswhere.exe" -latest ...` form cannot run at all. A path supplied as an ARGUMENT is not refused, which is why the `-find` pattern and the `dotnet-coverage` operands in P0-T10 and P4-T5 need no such treatment.

  Two quoting details are load-bearing. `-products` is quoted as `'*'` so the shell does not expand it as a filename glob against the worktree root. The `-find` pattern is double-quoted so its backslashes survive word expansion; an unquoted `Common7\IDE\...` would have its backslashes stripped and would match nothing. If the command prints more than one path, take the first.

  From the printed full path, derive the two values every later vstest task substitutes, by stripping the trailing `\vstest.console.exe`:

  - `<resolved-vstest-dir-native>` — the directory exactly as `vswhere.exe` printed it, backslash-spelled, for the example above `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform`. It is used ONLY inside the double-quoted operand after `--` in P0-T10 and P4-T5, where it is an argument handed to `dotnet-coverage` and must be a native Windows path.
  - `<resolved-vstest-dir>` — the same directory in the POSIX spelling this shell uses for `PATH` entries, obtained mechanically by replacing the leading `C:` with `/c` and every `\` with `/`; for the example above `/c/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform`. It is used ONLY in the `PATH="<resolved-vstest-dir>:$PATH"` prefix of P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, and P4-T6. The POSIX spelling is required there because this shell splits `PATH` on `:`, so a native `C:\...` entry would split at the drive-letter colon and resolve nothing.

  Both derivations are mechanical, so a third party re-running this task obtains the same two values from the same printed line.

  **3. Confirm the MSBuild entry point.**

  ```text
  msbuild.exe -version
  ```

  The `.exe` suffix is required and is not optional shorthand. Measured in this worktree during preflight round 3: `msbuild -version` returns `command not found` with exit 127, while `msbuild.exe -version` exits 0 and prints `MSBuild version 18.9.1+a81b43525 for .NET Framework`. MSYS bash does not append `.exe` when searching `PATH` for a bare name, so the bare spelling names nothing. No `vswhere`-based resolution step is added for MSBuild and none is needed: `msbuild.exe` is on `PATH`, and this plan invokes it by that name in P0-T8, P0-T9, P1-T3, P3-T1, P4-T3, and P4-T4 with the switch sets CLAUDE.md mandates, unchanged. This probe carries NO `MSYS_NO_PATHCONV=1` prefix and must not be given one: its single argument `-version` begins with `-`, not `/`, so MSYS path conversion has nothing to rewrite here. The six full `msbuild.exe` build commands do carry the prefix, for the reason given in constraint 4 of "Shell constraints measured in this worktree". No switch is added, removed, or altered in any of them.

  **4. Restore NuGet packages for the solution.**

  ```text
  nuget.exe restore TaskMaster.sln
  ```

  This step is required and is not redundant with `dotnet tool restore` in step 5. `dotnet tool restore` restores only the local tool manifest, which in this repository is dotnet-tools.json at the worktree root (there is no `.config/` directory, re-derived this pass); it performs no NuGet package restore. A fresh worktree has no `packages/` directory at all, and every project in this solution is `packages.config`-based — 18 `packages.config` files exist across the solution's projects, including `UtilitiesCS/packages.config` and `UtilitiesCS.Test/packages.config`, re-derived this pass. Without this restore the FIRST build task in this plan (P0-T8) fails with 37 errors, comprising `CS0246` type-not-found errors and MSBuild `.targets`-file-not-found errors from the `packages.config` import elements; that failure was reproduced directly in this worktree during preflight round 4.

  The command name is the bare `nuget.exe`, which resolves on `PATH` in this shell and therefore needs no `PATH=` prefix. It MUST NOT be spelled as a quoted absolute path: constraint 2 in "Shell constraints measured in this worktree" records that this shell refuses any command whose NAME is a quoted absolute path. It also needs no `MSYS_NO_PATHCONV=1` prefix, because neither of its two arguments begins with `/`.

  This step is CI parity, not a local deviation: .github/workflows/_build-analyzers.yml line 45 runs `nuget restore $env:SOLUTION_PATH` with `SOLUTION_PATH: TaskMaster.sln` (line 17) immediately before its analyzer build, and .github/workflows/_build-nullable.yml line 45 and .github/workflows/_mstest-coverage.yml line 45 do the same before their respective gates. All three re-derived this pass.

  The restore writes only into `packages/` at the worktree root, which .gitignore line 191 ignores as `**/[Pp]ackages/*` (re-derived this pass), so it enters no porcelain or diff gate later in this plan. The one un-ignored path under that pattern is `!**/[Pp]ackages/build/` on line 193; no `packages/build/` directory exists in this worktree after a completed restore, re-derived this pass. If a restore in a fresh worktree does produce one, record it in this artifact and report it, because it would otherwise appear as an untracked path in P4-T1's two unscoped porcelain spans — where, being present in both, it would cancel — and it lies outside every scoped gate in Phase 5.

  **5.** `dotnet tool restore`
  **6.** `dotnet tool run csharpier --version`
  **7.** `dotnet-coverage --version`

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t5-toolchain-resolution.md` with `Timestamp:`, `Command:` (every command actually run, in the order run, including every `dotnet --version` attempt and every bootstrap command when bootstrap was required), `EXIT_CODE:` (per command), and `Output Summary:` recording:

  - a `SDK_BOOTSTRAP:` field, whose value is EITHER the bootstrap outcome (the first `dotnet --version` result, the fact that the four-command POSIX bootstrap was run, the resulting `.dotnet-sdk` path, and the post-bootstrap `dotnet --version` result) OR, when the first probe already succeeded and no bootstrap was performed, the literal value `NOT REQUIRED (first probe already reported a version beginning 8.0.2)`. The second form is the correct one whenever `.dotnet-sdk/` is already present — for example in a worktree where an earlier partial run bootstrapped it — because in that case no bootstrap runs and there is no post-bootstrap reading to record. Recording the literal is not a skip: the first `dotnet --version` command is still run and still recorded under `Command:` and `EXIT_CODE:`;
  - a `NUGET_RESTORE:` field recording the exit code of step 4's `nuget.exe restore TaskMaster.sln` and the restore summary line it printed (for example the count of packages installed, or its statement that all packages are already installed);
  - the verbatim path line `vswhere.exe` printed;
  - the derived `RESOLVED_VSTEST_DIR_NATIVE:` and `RESOLVED_VSTEST_DIR:` values described in step 2;
  - the reported MSBuild version, the CSharpier version, and the `dotnet-coverage` version.

  Acceptance: the last `dotnet --version` reading recorded from step 1 reports a version beginning `8.0.2` and exits 0; `nuget.exe restore TaskMaster.sln` exits 0; `RESOLVED_VSTEST_DIR_NATIVE:` and `RESOLVED_VSTEST_DIR:` are both recorded as non-empty concrete directory paths, and the file `vstest.console.exe` exists inside that directory; `msbuild.exe -version` exits 0; `dotnet tool restore` exits 0; and the reported CSharpier version is `1.2.6` (the version pinned by dotnet-tools.json). The two recorded directory values are the substitutions for the `<resolved-vstest-dir-native>` and `<resolved-vstest-dir>` placeholders used in P0-T10, P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, P4-T5, and P4-T6; no task in this plan records or substitutes a full `vstest.console.exe` file path as a command name. If `dotnet-coverage` is absent, record `dotnet-coverage: UNAVAILABLE`, install it with `dotnet tool install --global dotnet-coverage`, re-run the version probe, and record both attempts; do not proceed to P0-T10 with an unresolved collector.

- [x] [P0-T6] Probe MCP availability by attempting one call to `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"` and `artifact_path: "docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md"`. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t6-mcp-probe.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the artifact records either the validator result or the exact string `MCP VALIDATOR UNAVAILABLE` plus the error text. This task never halts the plan: an unavailable validator is recorded and execution continues.

- [x] [P0-T7] Capture the format baseline. Run `dotnet tool run csharpier check .` from the worktree root. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t7-csharpier-check.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. The `Output Summary:` MUST enumerate, one per line under the heading `BASELINE_FORMAT_DRIFT_SET:`, every repository-relative path the command reports as unformatted, or the single line `BASELINE_FORMAT_DRIFT_SET: NONE` when it reports none. Acceptance: the artifact exists and carries a `BASELINE_FORMAT_DRIFT_SET:` block. A non-zero exit code here is a recorded baseline fact, not a failure of this task; P4-T2 and P5-T10 are both written to be satisfiable against a non-empty drift set.

- [x] [P0-T8] Capture the analyzer baseline. Run:

  ```text
  MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t8-analyzer-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the trailing `N Warning(s)` and `N Error(s)` counts printed by MSBuild. Acceptance: `EXIT_CODE: 0` and the artifact records `0 Error(s)` together with the baseline warning count (referred to below as the baseline analyzer warning count).

- [x] [P0-T9] Capture the nullable/type-check baseline. Run:

  ```text
  MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t9-nullable-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the trailing `N Error(s)` count and the quoted command line. Acceptance: `EXIT_CODE: 0`, the artifact records `0 Error(s)`, and the quoted command line contains `msbuild.exe TaskMaster.sln`, contains no `Nullable=enable` substring, and uses `/t:Rebuild` rather than `/t:Build`. The first of those three clauses is worded as `contains` rather than `begins with` because the recorded line begins with the `MSYS_NO_PATHCONV=1 ` assignment required by constraint 4 in "Shell constraints measured in this worktree"; it checks only the executable spelling this shell requires. The two substantive clauses are unchanged from CLAUDE.md and are what this gate actually enforces.

- [x] [P0-T10] Capture the `UtilitiesCS.Test` baseline run with Cobertura coverage, using the native vstest directory resolved in P0-T5:

  ```text
  MSYS_NO_PATHCONV=1 dotnet-coverage collect --output coverage/p0-t10.cobertura.xml --output-format cobertura --settings coverage.config -- "<resolved-vstest-dir-native>\vstest.console.exe" UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults/p0-t10 /TestCaseFilter:TestCategory!=LiveOutlook
  ```

  This task does NOT use the `PATH=`-prefix form that P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, and P4-T6 use. Here `vstest.console.exe` is not the command name — `dotnet-coverage` is — and the executable is an ARGUMENT after `--`, which this worktree's shell does not refuse. It DOES carry the `MSYS_NO_PATHCONV=1` prefix, which every vstest invocation in this plan carries regardless of which of the two forms it uses: the prefix suppresses the conversion of the colon-free `/InIsolation` switch, and that switch is passed through to `vstest.console.exe` identically in both forms. Without it, `/InIsolation` arrives as `C:/Program Files/Git/InIsolation`, vstest treats it as a test source, reports `The test source file ... was not found`, and runs zero tests — a run that would produce a coverage file describing nothing and a test count of zero. See constraint 4 in "Shell constraints measured in this worktree". The operand is double-quoted so the backslashes in `<resolved-vstest-dir-native>` survive word expansion and `dotnet-coverage` receives a valid native Windows path; the native spelling rather than the POSIX one is used for that same reason. P4-T5 uses this identical form, which is what keeps the two runs command-identical apart from their `--output` and `/ResultsDirectory` values, as P4-T7's comparison requires.

  Every other path in this command is written with forward slashes deliberately. Every command block in this plan is executed through a POSIX shell, which removes an unquoted backslash inside a word; a backslash-spelled `coverage\p0-t10.cobertura.xml` would therefore be created as `coveragep0-t10.cobertura.xml` at the worktree root, where .gitignore's `coverage/*` rule (line 144, re-derived this pass) does not match it and where P4-T7 could not read it. `msbuild.exe`, `vstest.console.exe`, and `dotnet-coverage` all accept forward-slash paths on Windows. Backslashes survive in exactly three places in this plan, and all three are inside double quotes, which is what preserves them: the `-find` pattern in P0-T5 step 2, and the `"<resolved-vstest-dir-native>\vstest.console.exe"` operands in this task and in P4-T5.

  This command's flag set is deliberately identical to P4-T5's. The two differ only in the `--output` filename and the `/ResultsDirectory` value. In particular neither passes `/EnableCodeCoverage`, because activating the vstest in-process collector underneath `dotnet-coverage collect` changes the loaded-module set and therefore the `lines-valid` denominator, which would make the P4-T7 comparison between these two runs not apples-to-apples.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t10-utilitiescs-tests-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. The `Output Summary:` MUST record the numeric `Total tests`, `Passed`, `Failed`, and `Skipped` counts for this run, each drawn from the source named below, together with the TRX `total` and `executed` values from which the `Skipped` figure is derived, and, read from the root `<coverage>` element of `coverage/p0-t10.cobertura.xml`, the numeric `lines-covered`, `lines-valid`, and `line-rate` attribute values (referred to below as the baseline coverage figures).

  **Where each of the four test counts is read from.** `Total tests` and `Passed` are read from the console summary block this command prints. `Failed` is read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p0-t10/`. `Skipped` is DERIVED from that same element as `total` minus `executed`; record `total`, `executed`, and the derived `Skipped` value. The `notExecuted` attribute MUST NOT be used: `vstest.console.exe`'s TRX logger populates only `total`, `executed`, `passed`, and `failed` on the `<Counters .../>` element and hard-codes every other attribute (`notExecuted`, `error`, `timeout`, `aborted`, `inconclusive`, ...) to `0` regardless of the run's actual outcome. Measured in preflight round 6: a run whose console printed `Skipped: 1` produced a TRX with `notExecuted="0"`; the derivation `total` minus `executed` correctly returned `1` on that run and `0` on an all-passing run. The console/TRX split is required, not stylistic. Measured across four green runs in this worktree during preflight round 5, a successful `vstest.console.exe` run prints no `Failed:` line and no `Skipped:` line at all; the entire summary block it emits is:

  ```text
  Test Run Successful.
  Total tests: 4783
       Passed: 4783
   Total time: 12.8452 Seconds
  ```

  The TRX carries the run's aggregate counts in one element, for example:

  ```text
  <Counters total="4783" executed="4783" passed="4783" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" ... />
  ```

  where `failed` supplies `Failed` and `total` minus `executed` supplies `Skipped`. See constraint 5 in "Shell constraints measured in this worktree", which states this rule once centrally and binds it on all seven TRX-reading tasks. `TestResults/p0-t10/` is written by this task and by no other task in this plan, so the file is located without any choice being left to the executor; if this task has been run more than once, the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection.

  **Redaction rule for the TRX reference.** Identify the TRX in the artifact by its repository-relative results directory `TestResults/p0-t10/`, and do NOT record the file's own name, and do NOT quote the run's `Results File:` console line. `vstest.console.exe` composes the default TRX filename from the host account name and the machine name, and the `Results File:` line prints that filename inside a full absolute host path, so recording either verbatim would place an account-name disclosure into committed evidence — the same disclosure P4-T7's redaction rule prevents for the Cobertura `filename` attribute. Record the `total`, `executed`, and `failed` values and the derived `Skipped` value themselves, which carry no path.

  Acceptance: the artifact records all four test counts and all three numeric coverage attribute values as concrete numbers, not placeholders; it records the `total` and `executed` values from which the `Skipped` figure was derived; and it identifies `TestResults/p0-t10/` as the results directory `Failed` and `Skipped` were read from without recording a TRX filename and without quoting a `Results File:` line. If `Failed` is non-zero, record the failing test names as `BASELINE_FAILURE_SET:` and treat that set, not zero, as the comparison target in P3-T3 and P4-T5; a pre-existing failure is a recorded baseline fact, not a blocker for this plan.

- [x] [P0-T11] Capture the `QuickFiler.Test` baseline run. Command:

  ```text
  MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p0-t11 /TestCaseFilter:TestCategory!=LiveOutlook
  ```

  The `PATH=`-prefix form is used by every task in this plan that runs `vstest.console.exe` as the command NAME (P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, P4-T6). It is required because this worktree's shell refuses a command whose name is a quoted absolute path, and `vstest.console.exe` does not resolve on the default `PATH` by bare name; see constraint 2 in "Shell constraints measured in this worktree". `<resolved-vstest-dir>` is the POSIX-spelled directory recorded in P0-T5 step 2.

  The `MSYS_NO_PATHCONV=1` assignment ahead of the `PATH=` assignment is separately required and is not decoration. Without it, MSYS path conversion rewrites the colon-free `/InIsolation` switch into `C:/Program Files/Git/InIsolation`; vstest then treats that as a test source, prints `The test source file ... was not found`, and runs zero tests while the colon-bearing switches on the same line pass through untouched. Ordering the two assignments the other way round works identically — the shell accepts any number of leading assignments — but the order shown here is used uniformly across all six tasks so the recorded command lines are comparable. See constraint 4 in "Shell constraints measured in this worktree".

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t11-quickfiler-tests.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the numeric `Total tests`, `Passed`, `Failed`, and `Skipped` counts, the TRX `total` and `executed` values from which the `Skipped` figure is derived, plus a `BASELINE_FAILURE_SET:` list when `Failed` is non-zero.

  The same sourcing rule P0-T10 states applies here unchanged. `Total tests` and `Passed` are read from the console summary block; `Failed` is read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p0-t11/`; and `Skipped` is DERIVED from that same element as `total` minus `executed`, with `total`, `executed`, and the derived `Skipped` value all recorded. The `notExecuted` attribute MUST NOT be used: `vstest.console.exe`'s TRX logger populates only `total`, `executed`, `passed`, and `failed` on the `<Counters .../>` element and hard-codes every other attribute (`notExecuted`, `error`, `timeout`, `aborted`, `inconclusive`, ...) to `0` regardless of the run's actual outcome. Measured in preflight round 6: a run whose console printed `Skipped: 1` produced a TRX with `notExecuted="0"`; the derivation `total` minus `executed` correctly returned `1` on that run and `0` on an all-passing run. The counts come from the TRX rather than the console because a green `vstest.console.exe` run prints no `Failed:` line and no `Skipped:` line at all (constraint 5 in "Shell constraints measured in this worktree", measured across four green runs in preflight round 5). Only the SOURCE of the `Failed` and `Skipped` values changes; the `BASELINE_FAILURE_SET:` mechanism keyed off a non-zero `Failed` count is unchanged, and the failing test names it lists are still taken from the run's per-test output. `TestResults/p0-t11/` is written by this task and by no other task in this plan; if this task has been run more than once, the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection. The TRX reference is subject to the same redaction rule P0-T10 states: identify the file by its repository-relative results directory only, never by its own name and never by quoting the run's `Results File:` console line, because `vstest.console.exe` composes the default TRX filename from the host account name and the machine name and prints it inside a full absolute host path.

  Acceptance: all four counts are recorded as concrete numbers, the `total` and `executed` values from which `Skipped` was derived are recorded, and the artifact identifies `TestResults/p0-t11/` as the results directory `Failed` and `Skipped` were read from without recording a TRX filename and without quoting a `Results File:` line. This assembly is baselined because the sibling audit in P3-T6 found that QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs constructs the parameterless `WpfUiDispatcher`, whose provider closes over `UiThread.Dispatcher`.

- [x] [P0-T12] Record the coverage-threshold reconciliation. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t12-threshold-reconciliation.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` naming CLAUDE.md as the rank-1 authority supplying `>= 80%` repository line coverage and `>= 90%` new-code coverage, naming .claude/rules/general-unit-test.md and .claude/rules/quality-tiers.md as the rank-3/rank-4 sources supplying `>= 85%` line and `>= 75%` branch, stating that the rank-1 figures are the ones this plan enforces, and quoting the baseline `line-rate` recorded in P0-T10. Acceptance: the artifact names CLAUDE.md explicitly as the superseding authority and quotes the concrete baseline `line-rate` value.

- [x] [P0-T13] Baseline the parallel-bucket census and the file sizes of the five files this plan writes. Run:

  ```text
  git grep -n -F '"_dispatcher"' -- UtilitiesCS.Test
  git grep -c -F DoNotParallelize -- UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  git grep -n -F "[TestClass" -- UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  wc -l UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  ```

  The counting idiom for every line-count assertion in this plan is `wc -l`, the physical newline count, used identically in P0-T13, P2-T3, and P4-T8. A before/after line-count comparison taken across two different counting idioms is incommensurable, so the idiom is fixed here rather than left to the executor. `Measure-Object -Line` MUST NOT be substituted for it: that cmdlet does not count blank lines and under-reports every file in this set by 17 to 92 lines, confirmed against this tree in this pass (`UiThread_Tests.cs` has 17 blank lines out of 104 and `ProgressTracker_Tests.cs` has 92 out of 514, so the substitution would report 87 and 422 in place of 104 and 514, tripping this task's own BLOCKED clause below and contradicting the pre-existing-overage narrative this plan records for `ProgressTracker_Tests.cs`).

  `wc -l` given more than one file prints one row per file and then a trailing `total` row. The acceptance below reads the five named per-file rows; it does not assert anything about the whole command output as a single value, and the trailing `total` row is expected and is not a defect.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t13-parallel-bucket-census.md` with `Timestamp:`, `Command:` (all four), `EXIT_CODE:` (per command), and `Output Summary:` quoting each command's output verbatim under the headings `BASELINE_DISPATCHER_WRITERS:`, `BASELINE_DONOTPARALLELIZE_COUNTS:`, `BASELINE_TESTCLASS_LINES:`, and `BASELINE_LINE_COUNTS:`. Acceptance, each value re-derived by the planner against the working tree in this pass:

  1. The first command prints exactly three lines, one each in `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` at line 144, `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` at line 422, and `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` at line 138.
  2. The second command prints nothing and exits 1, because `git grep` exits 1 on zero matches. This is the false-before half of P1-T5's gate: none of the four files carries the attribute at BASE.
  3. The third command prints exactly four lines, at `UtilitiesCS.Test/Threading/UiThread_Tests.cs:8`, `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:28`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:13`, and `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:14`.
  4. The fourth command's five per-file rows report 163 for `UtilitiesCS/Threading/UiThread.cs`, 104 for `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, 347 for `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, 205 for `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, and 514 for `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`. These five per-file rows are the recorded baseline counts referred to by P2-T3 and P4-T8; the trailing `total` row is ignored.

  If any of these values differs from the value stated here, stop and report BLOCKED rather than editing, because P1-T5's edit sites and P2-T3's size accounting were derived from them. The artifact MUST additionally carry the line `PRE-EXISTING FILE-SIZE OVERAGE: UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs 514` together with a statement that the overage exists at BASE `87cb4df338322844abfa580abea14df77e738e5c` and is not introduced by this change.

- [x] [P0-T14] Census the reflective reads of the `Dispatcher` PROPERTY and of the `UiThread` type across every test assembly and, through the two repository-wide commands below, across every `.cs` file in the repository, closing the census gap that produced the P4-T6 regression. This task is the property-name counterpart of P0-T13's field-name census, and it exists because P0-T13's `git grep -F '"_dispatcher"'` cannot match a reflective read spelled `GetProperty("Dispatcher", ...)`, and `spec.md`'s `git grep -n "UiThread.Dispatcher\b"` cannot match it either: neither search's operand appears in the file at all. Run, from the worktree root:

  ```text
  git grep -n -F '"Dispatcher"' -- QuickFiler.Test SVGControl.Test Tags.Test TaskMaster.Test TaskTree.Test TaskVisualization.Test ToDoModel.Test UtilitiesCS.Test VBFunctions.Test
  git grep -n -F 'typeof(UiThread)' -- '*.cs'
  git grep -n -F '"Dispatcher"' -- '*.cs'
  ```

  The nine pathspecs of the FIRST command are every test assembly in this repository, re-derived in the revision round 15 pass by enumerating the `*.Test.csproj` files at the worktree root: `QuickFiler.Test`, `SVGControl.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`, and `VBFunctions.Test`. The second and third commands are repository-wide over `.cs` files and are what cover production code; their `'*.cs'` pathspecs are load-bearing and are not decoration. A bare repository-wide `git grep -F '"Dispatcher"'` with no pathspec also matches this plan file and `spec.md`, both of which quote that literal in their own prose, so its acceptance would be asserting against text this plan itself writes. Limiting the second and third commands to `.cs` files removes that self-match while keeping full repository coverage.

  The second command's pathspec was widened from the nine-test-assembly list to `'*.cs'` in the revision round 16 pass, and the reason is stated here rather than left implicit. Clause 3 below draws a repository-wide conclusion about PRODUCTION code, and it rests on commands 2 and 3 together: command 3 enumerates every occurrence of the reflective name operand `"Dispatcher"` and command 2 enumerates every occurrence of the reflection entry point `typeof(UiThread)`. While command 2 was scoped to the nine test assemblies, that conclusion was broader than the census's own output supported, which is a defect in an auditable gate independently of whether the conclusion happened to be true. Widening costs nothing here: the widened command returns the same seven files the nine-assembly list returned, so clause 2's asserted count is unchanged at seven.

  No command carries an `MSYS_NO_PATHCONV=1` prefix and none needs one: no argument of any of the three begins with `/`, so MSYS path conversion has nothing to rewrite. The single-quoting of each `-F` operand and of each `'*.cs'` pathspec is load-bearing, because each contains characters the shell would otherwise consume — the double quotes in the first and third, the parentheses in the second, and the `'*.cs'` glob in the second and third, which must reach `git` unexpanded rather than being expanded against the worktree root.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t14-reflective-dispatcher-census.md` with `Timestamp:`, `Command:` (all three), `EXIT_CODE:` (per command), and `Output Summary:` quoting all three commands' complete output verbatim under the headings `REFLECTIVE_PROPERTY_NAME_HITS:`, `REFLECTIVE_UITHREAD_TYPE_HITS:`, and `REPOSITORY_WIDE_PROPERTY_NAME_HITS:`, and classifying every hit, one line per hit: as `REFLECTION` or `DOC-COMMENT` for the first and third commands, and as `FIELD:<field name>` or `PROPERTY:<property name>` for the second. Quoting the full output, rather than a count, is what lets a later reviewer see what the census returned instead of taking the conclusion on trust; that is the specific failure this task exists to prevent recurring.

  Acceptance, each value re-derived by the planner against the working tree in the revision round 15 pass, with clauses 2 and 3 re-derived again in the revision round 16 pass after the second command's pathspec was widened:

  1. The first command prints exactly two lines, in exactly two files: one in `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`, classified `DOC-COMMENT` because it is the XML `<see cref="Dispatcher"/>` reference in that file's class summary and invokes nothing; and one in `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`, classified `REFLECTION` because it is the name operand of a `GetProperty` call. The FILE SET is the assertion. A line number that differs from the round-15 readings of 14 and 35 for the same file and the same construct is not a failure of this task and is recorded as observed; a file in the printed set that is not one of those two, or either of those two missing, IS a failure and must be reported to the caller before P2-T4 runs, because it would mean a second reflective consumer exists that this revision does not repair.
  2. The second command prints exactly seven lines, in exactly seven files: `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` (classified `PROPERTY:Dispatcher`), `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (`FIELD:_dispatcher`), `UtilitiesCS.Test/Threading/UiThread_Tests.cs` (`FIELD:_dispatcher`), `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` (`FIELD:_dispatcher`), `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` (`FIELD:_dispatcher`), `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` (`FIELD:_dispatcher`), and `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` (`FIELD:_uiSyncContext`). Exactly one of the seven is classified `PROPERTY:`, and it is the file P2-T4 repairs. `UtilitiesCS.Test/Threading/UiThread_Tests.cs` appears in this set because P1-T2 added the `DispatcherField()` helper to it; it is not present at BASE, and this clause describes the tree as it stands when this task runs, not the tree at BASE. `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` is in the set but is unrelated to this fix: it reflects over `_uiSyncContext`, a different static, and is recorded so that a reviewer can see it was examined rather than overlooked. The count of seven and the seven-file set are unchanged by the revision round 16 widening of this command's pathspec to `'*.cs'`. That count was re-derived in the round 16 pass by searching every tracked `.cs` file in the worktree for the literal `typeof(UiThread)`: the result is the same seven files, because no production `.cs` file in this repository contains that literal at all, so every hit the widened command returns falls inside one of the nine test assemblies the previous pathspec named.
  3. The third command prints exactly five lines, in exactly five files: the two files clause 1 names, plus `UtilitiesCS/Threading/WpfUiDispatcher.cs`, `UtilitiesCS/Threading/ThreadMonitor.cs`, and `UtilitiesCS/Threading/IUiDispatcher.cs`. All three of the added files are classified `DOC-COMMENT`: each is an XML `<see cref="Dispatcher"/>` cross-reference to the WPF type and none invokes anything. The conclusion this clause establishes, and which the artifact must state explicitly, is that no PRODUCTION file in this repository reads `UiThread.Dispatcher` reflectively, so the blast radius of P2-T1's guard through the reflective route is confined to the single test file P2-T4 repairs. That conclusion rests on commands 2 and 3 TOGETHER, and after the round 16 widening both are repository-wide over `.cs` files, so the census's own output covers every file the conclusion speaks about: command 3 shows that the only `.cs` occurrences of the reflective name operand `"Dispatcher"` outside the one repaired test file are four documentation cross-references, and command 2 shows that the reflection entry point `typeof(UiThread)` occurs in no production file. The artifact MUST cite both commands' recorded output when it states this conclusion, rather than stating it under command 3 alone.

  If clause 1, clause 2, or clause 3 reports a file set other than the one stated, stop and report BLOCKED rather than proceeding, because P2-T4 repairs exactly one file and an unlisted reflective consumer would leave a second regression in place.

### Phase 1 — Deterministic regression test, red before the fix

- [x] [P1-T1] In `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, add `using System.Reflection;` to the existing using block, preserving the existing directives and their order (`System`, `System.Reflection`, `System.Threading`, `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`). Do not add `using System.Windows.Threading;`; the new test refers to the WPF dispatcher type by its fully-qualified name so no new type name enters this file's lookup scope. Acceptance: reading the file shows exactly one added using directive and five using directives total, in the order listed above, with no existing directive removed or reordered. No `git diff` is asserted at this point; the diff-based gates in this plan all use the single-ref working-tree form `git diff 87cb4df338322844abfa580abea14df77e738e5c -- <paths>` and are stated in P3-T5 and P4-T7.

- [x] [P1-T2] In the same file `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, append a second test class inside the existing `UtilitiesCS.Test.Threading` namespace, after the closing brace of `SynchronizationContextAwaiter_Tests`. The class is named `UiThread_Dispatcher_Tests`, carries `[TestClass]` on one line and `[DoNotParallelize]` on the next (justified by the assembly-level parallelisation attribute recorded in P0-T4 and by the fact that both tests mutate the process-global static `UiThread._dispatcher`), carries an XML doc comment containing the literal token `#584` and stating why reflection is used, and contains a private static helper returning the `FieldInfo` for `_dispatcher` plus exactly these two `[TestMethod]`s. The XML doc comment MUST NOT contain the token `DoNotParallelize`, because P1-T5 asserts an exact occurrence count of 1 for that token in this file. The XML doc comment MUST ALSO NOT contain any of the seven tokens `Thread.Sleep`, `Task.Delay`, `SpinWait`, `Retry`, `retries`, `Timeout(`, or `PushFrame`, in any letter case. P3-T5 searches the added lines of this change case-insensitively for exactly those seven tokens, and it reads added lines without distinguishing code from comment, so a doc comment explaining that the test needs no retry or sleep would trip AC5's gate on its own compliant documentation. State the rationale without those words — for example, by saying the test drives the accessor contract directly through the private backing field and is therefore deterministic without any timing construct.

  ```csharp
        [TestMethod]
        public void Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize()
        {
            // Arrange
            var field = DispatcherField();
            field.Should().NotBeNull();
            var prior = field.GetValue(null);
            field.SetValue(null, null);
            try
            {
                // Act
                Action act = () =>
                {
                    _ = UiThread.Dispatcher;
                };

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("*UiThread.Initialize()*");
            }
            finally
            {
                field.SetValue(null, prior);
            }
        }

        [TestMethod]
        public void Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance()
        {
            // Arrange
            var field = DispatcherField();
            var prior = field.GetValue(null);
            var expected = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            field.SetValue(null, expected);
            try
            {
                // Act / Assert
                UiThread.Dispatcher.Should().BeSameAs(expected);
            }
            finally
            {
                field.SetValue(null, prior);
            }
        }
  ```

  The helper is:

  ```csharp
        private static FieldInfo DispatcherField()
        {
            return typeof(UiThread).GetField(
                "_dispatcher",
                BindingFlags.NonPublic | BindingFlags.Static
            );
        }
  ```

  Acceptance: the file contains exactly two `[TestClass]` attributes; the new class contains exactly two `[TestMethod]` attributes; the new code contains no occurrence of `Thread.Sleep`, `Task.Delay`, `Thread.CurrentThread.Join`, `SpinWait`, or `Dispatcher.PushFrame`; both tests restore the captured prior field value in a `finally` block; the file's total line count is under 500; and `git grep -c -F "#584" -- UtilitiesCS.Test/Threading/UiThread_Tests.cs` prints a count of 1 or more, which is the enforceable form of the XML-doc-comment requirement stated above.

- [x] [P1-T3] Build the solution so the new test compiles against the UNFIXED production code. Run:

  ```text
  MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p1-t3-build-before-fix.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the trailing `N Warning(s)` and `N Error(s)` counts. Acceptance: `EXIT_CODE: 0` and `0 Error(s)`. This confirms the regression test's red state in P1-T4 is a runtime assertion failure and not a compile failure, which is the property that makes it a genuine fail-before.

- [x] [P1-T4] [expect-fail] Run the two new tests against the unfixed production code, using the POSIX vstest directory resolved in P0-T5:

  ```text
  MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p1-t4 /TestCaseFilter:"FullyQualifiedName=UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize|FullyQualifiedName=UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance"
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p1-t4-expect-fail.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 1`, and `Output Summary:` recording `Total tests`, `Passed`, `Failed`, the name of the failing test, and the verbatim FluentAssertions failure message. Acceptance: the run reports `Total tests: 2`, `Passed: 1`, `Failed: 1`; the single failure is `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize`; and the recorded failure message states that no exception was thrown. The positive test passing here is expected and required: it proves the reflection arrangement and the restore path work before the production change, so the red in the negative test is attributable to the defect and not to the test harness.

- [x] [P1-T5] Move every remaining writer of `UiThread._dispatcher` out of the parallel bucket, by an attribute-only edit to three existing test files. Make exactly these three edits and nothing else in these files — no `using`, no assertion, no test body, no member added, removed, or reordered:

  1. `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`: insert `    [DoNotParallelize]` immediately after the `[TestClass]` on line 28, giving the two-line form quoted verbatim in the "Exact source text this plan will create" section.
  2. `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`: insert `    [DoNotParallelize]` immediately after the `[TestClass]` on line 13, same two-line form.
  3. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`: replace line 14's `    [TestClass]` with the single line `    [TestClass, DoNotParallelize]`. The combined attribute list is used for this one file only, because the file is 514 lines at BASE and already exceeds the 500-line limit in .claude/rules/general-code-change.md; the combined form adds the attribute without adding a line, so this change does not deepen a pre-existing overage.

  Then run:

  ```text
  git grep -l -F '"_dispatcher"' -- UtilitiesCS.Test
  git grep -c -F DoNotParallelize -- UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p1-t5-donotparallelize.md` with `Timestamp:`, `Command:` (both), `EXIT_CODE:` (per command), and `Output Summary:` quoting both outputs verbatim. Acceptance:

  - The first command prints exactly these four paths and no other: `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`. The fourth is present because P1-T2 added the new class's `DispatcherField()` helper to it.
  - The second command prints exactly four lines, one per path, each reporting a count of exactly `1`. This is the enforceable form of the isolation guarantee: every file in `UtilitiesCS.Test` that names `UiThread._dispatcher` carries the do-not-parallelize attribute, so zero writers of that field remain in the parallel bucket. P0-T13 recorded the false-before state for this same command (zero matches, exit 1), so the gate is false before this task and true after it.

  `DoNotParallelize` resolves in all three files without a new `using`: each already imports `Microsoft.VisualStudio.TestTools.UnitTesting` at `IdleAsyncQueue_Tests.cs:6`, `ProgressTrackerAsync_Tests.cs:7`, and `ProgressTracker_Tests.cs:8`, re-derived this pass. This task is placed after P1-T4 deliberately: P1-T3's build and P1-T4's expect-fail run are scoped to the two new tests by an explicit `/TestCaseFilter`, so neither depends on these three files, and the first build that compiles these edits is P3-T1.

  This task changes no assertion in any of the three files, which is what keeps it compatible with AC4's "all pass, unmodified assertions" wording. Moving a class from the parallel bucket to the serial bucket can only reduce the concurrency those tests experience, so it cannot introduce a race; the possibility that it exposes a latent ordering dependency is not asserted away here but is verified empirically by P3-T3 and P4-T5.

### Phase 2 — Minimal production fix and its one reflective consumer

- [x] [P2-T1] In `UtilitiesCS/Threading/UiThread.cs`, replace the `Dispatcher` property and its backing field (lines 135-140 as re-derived in P0-T2) with the text quoted verbatim in the "Exact source text this plan will create" section above: an expression-free `get` accessor that throws `new InvalidOperationException("The UI dispatcher has not been captured. Call UiThread.Init() so that UiThread.Initialize() runs before reading UiThread.Dispatcher.")` when `_dispatcher is null` and otherwise returns `_dispatcher`; the `private set => _dispatcher = value;` accessor unchanged; and the backing field redeclared as `private static Dispatcher? _dispatcher;` with the `null!` initialiser and its trailing comment removed. Change nothing else in this file. Acceptance: the property's declared return type is still the non-nullable `Dispatcher`; the file contains exactly one `throw new InvalidOperationException(`; the file's total line count is 172 or fewer; and no other member of the file is modified.

- [x] [P2-T2] Verify the null-forgiving suppression is gone and the nullable field declaration is present. Run:

  ```text
  git grep -c -F "null!" -- UtilitiesCS/Threading/UiThread.cs
  git grep -n -F "private static Dispatcher? _dispatcher;" -- UtilitiesCS/Threading/UiThread.cs
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p2-t2-nullforgiving-removed.md` with `Timestamp:`, `Command:` (both), `EXIT_CODE:` (per command), `Output Summary:` quoting each command's output. Acceptance: the first command prints no matching line and exits 1 (`git grep` exits 1 on zero matches), and the second command prints exactly one line whose path is `UtilitiesCS/Threading/UiThread.cs`. Both commands are scoped by pathspec to this one file, so neither is affected by `null!` occurrences elsewhere in the repository. The pre-change state of the first command was exactly one match on line 140, recorded in P0-T2, so this gate is false before the edit and true after it.

- [x] [P2-T3] Account for the file-size limit in .claude/rules/general-code-change.md across all five files this plan writes. Run:

  ```text
  wc -l UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  ```

  This is character-for-character the command P0-T13 ran, for the reason stated there: the before and after counts must come from one counting idiom or the comparison is incommensurable. Read the five named per-file rows and ignore the trailing `total` row.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p2-t3-file-size.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` quoting all five reported per-file line counts alongside the corresponding baseline counts recorded in P0-T13. Acceptance:

  1. The counts for `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, and `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` are each strictly less than 500.
  2. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` is exempt from clause 1 because it is 514 lines at BASE, above the limit before this plan touches it. Its acceptance is instead that its post-change count is less than or equal to its P0-T13 baseline count plus 1. The plan's intent is a count unchanged at 514, achieved by the combined attribute list in P1-T5; the plus-one tolerance exists solely because a later `csharpier format .` pass may split that attribute list onto two lines, which is a formatter decision this plan does not control. The artifact MUST carry the line `PRE-EXISTING FILE-SIZE OVERAGE: UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` and state that the overage exists at BASE and is not introduced by this change. If the post-change count exceeds baseline plus 1, that is a real regression in this file and the task fails.

  This task predates the revision round 15 scope widening and audits the five files owned at that time. The sixth owned file's size is audited by P2-T4 for the pre-format tree and by P4-T8 for the post-format tree; no count in this task changes.

- [x] [P2-T4] Retarget the reflective dispatcher snapshot in `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` from the public `Dispatcher` property to the private `_dispatcher` backing field, so this class's `[TestInitialize]` and `[TestCleanup]` observe the same state without invoking the guard P2-T1 installed. Make exactly the four edits quoted verbatim in "The reflection-target change P2-T4 creates, quoted verbatim" above and nothing else in this file: the private static cache declaration (lines 33-37 today), the `GetValue` call in `Setup()` (line 49 today), the `GetValue` call in `Cleanup()` (line 58 today), and the six inserted comment lines before `private object _capturedDispatcher;` (line 32 today). Do NOT add a `using` directive: the file reaches `System.Reflection.FieldInfo` and `System.Reflection.BindingFlags` by their fully-qualified names exactly as it reaches `System.Reflection.PropertyInfo` today, and adding a directive would be a change beyond the reflection target. Do NOT add, remove, rename, or reorder any `[TestMethod]`, and do NOT alter any assertion, any mock setup, or the `[TestClass]`/`[DoNotParallelize]` attribute pair the file already carries on lines 21-22.

  The idiom this task adopts is the one already established for the same static in four other files: `typeof(UiThread).GetField("_dispatcher", BindingFlags.NonPublic | BindingFlags.Static)`, used by `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`'s `DispatcherField()` / `ForceDispatcherNull()` / `RestoreDispatcher()` helpers at lines 142-187, by the helper P1-T2 added to `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, by `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, and by `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`. This file spells the binding flags fully qualified because it declares no `using System.Reflection;`, which is the only difference from those four sites.

  The behavioural equivalence that makes this a narrow change rather than a weakened test is worth stating, because it is the reason no assertion needs to move. `Cleanup()` asserts `current.Should().BeSameAs(_capturedDispatcher)`. Before P2-T1 the property getter returned the field's value unconditionally, so the property read and the field read were the same observation; after P2-T1 they differ only in that the property read additionally throws when the value is null. The field read therefore preserves the assertion's meaning exactly — the static was not mutated across the test — and removes only the newly added failure mode, which is a diagnostic for production callers and not a state this bookkeeping test asserts about.

  Then run, from the worktree root:

  ```text
  git grep -c -F '"_dispatcher"' -- "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"
  git grep -c -F '"Dispatcher"' -- "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"
  git grep -c -F 'GetProperty(' -- "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"
  git grep -c -F '[TestMethod]' -- "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"
  wc -l "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"
  mkdir -p TestResults
  git diff 87cb4df338322844abfa580abea14df77e738e5c -- "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs" > TestResults/p2-t4-emailmovemonitor.diff
  grep -E -i 'Thread\.Sleep|Task\.Delay|SpinWait|Retry|retries|Timeout\(|PushFrame' TestResults/p2-t4-emailmovemonitor.diff
  grep -E '^[-+]' TestResults/p2-t4-emailmovemonitor.diff | grep -F '.Should()'
  ```

  Every operand naming the sixth owned file is double-quoted because the path contains a space; see the note in the "Scope" section. The `git diff` span uses the single-ref working-tree form anchored to BASE, for the same reason P3-T5 and P4-T7 do: this plan's first commit is P5-T9, so a two-dot `87cb4df338322844abfa580abea14df77e738e5c..HEAD` span would compare BASE against a HEAD that is identical to it across this pathspec and print nothing whatever the executor wrote, and both of the two `grep` gates below would then pass vacuously. The file is tracked at BASE, so the single-ref form's blindness to untracked files does not apply to it. The redirection target is forward-slash spelled for the reason stated in P0-T10; `TestResults/` is ignored by .gitignore line 39 `[Tt]est[Rr]esult*/` and `*.diff` files inside it enter no porcelain, diff, or format gate in this plan.

  The eighth command is deliberately NOT the two-stage `grep -E '^\+' ... | grep -E -i ...` pipeline P3-T5 uses, and the difference is deliberate rather than an inconsistency. P3-T5's first stage exists to restrict the search to added lines across a five-file diff. Here the whole diff is six added comment lines, six modified lines (the four lines of the cache declaration at 33-36 and the two `GetValue` call sites at 49 and 58), and their context, and the file ALREADY contains the token `Thread` on pre-existing context lines — `int marshalTargetThreadId = Thread.CurrentThread.ManagedThreadId;` and the surrounding body of `UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread`. Searching the whole diff rather than only its added lines is therefore the STRICTER gate here, because it would also report a banned token appearing on a context or removed line, and none of the seven tokens is present anywhere in this file at BASE: `Thread.CurrentThread` and `ThreadPool` do not contain `Thread.Sleep`, and none of `Task.Delay`, `SpinWait`, `Retry`, `retries`, `Timeout(`, or `PushFrame` occurs in the file. That last statement was re-derived by reading the file in full in the revision round 15 pass, and it is what makes the stricter form satisfiable rather than unsatisfiable.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md` with `Timestamp:`, `Command:` (all nine), `EXIT_CODE:` (per command), and `Output Summary:` quoting each command's output verbatim, recording the byte size of `TestResults/p2-t4-emailmovemonitor.diff`, and recording both the plan-stated pre-edit line count of 314 and the observed post-edit `wc -l` count. Acceptance:

  1. The first command prints one line reporting a count of exactly `1` for that path, and the second and third each print nothing and exit 1 (`git grep` exits 1 on zero matches; `-c` suppresses the row entirely for a file with no match). Together these are the false-before/true-after pair for the retarget: at BASE the file contains one `"Dispatcher"` and one `GetProperty(` and zero `"_dispatcher"`, and after this task it contains the reverse. The pre-change state of all three is recorded in P0-T14's census and in the P4-T6 failure artifact.
  2. The fourth command prints one line reporting a count of exactly `8` for that path, unchanged from BASE. This is the enforceable form of "no test method added, removed, or renamed"; eight is the count re-derived in the revision round 15 pass and is also the number of failures P4-T6 reported, which is every `[TestMethod]` in the class.
  3. `TestResults/p2-t4-emailmovemonitor.diff` is non-empty. An empty diff means this gate had nothing to inspect and the result is BLOCKED, not PASS.
  4. The eighth command prints nothing and exits 1, which is what `grep` returns when it finds no match. If it prints any line, the change violates AC5 and the offending construct must be removed before proceeding. This span is what extends AC5's coverage to the sixth owned file; P3-T5's diff pathspec is `UtilitiesCS UtilitiesCS.Test` and does not reach it.
  5. The ninth command prints nothing and its second `grep` exits 1. No added and no removed line carries the token `.Should()`, which is the enforceable form of AC4's "unmodified assertions" for this file.
  6. The post-edit `wc -l` count is exactly 320, which is strictly less than the 500-line limit in .claude/rules/general-code-change.md. The pre-edit count is 314, re-derived in the revision round 15 pass; the six inserted comment lines account for the entire difference, and the six modified lines add none. An exact figure is used rather than a ceiling because `csharpier` has not run at this point in the plan — P4-T1 is the formatter step — so the count is fully determined by the four edits this task describes, and any other value means a line was added or removed that this task does not describe. The post-format count is audited separately by P4-T8, which carries a two-line formatter tolerance for exactly the reason this clause does not need one.

  If clause 1's second or third command prints a line, the retarget is incomplete and the task fails. Do not proceed to Phase 4 with a partially retargeted file, because P4-T6 would then still report the same eight failures and the cause would be ambiguous between an incomplete edit and a second consumer.

### Phase 3 — Targeted verification of the fix and its blast radius

- [x] [P3-T1] Rebuild with analyzers after the fix. Run:

  ```text
  MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p3-t1-analyzer-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the trailing `N Warning(s)` and `N Error(s)` counts. Acceptance: `EXIT_CODE: 0`, `0 Error(s)`, and the warning count is less than or equal to the baseline analyzer warning count recorded in P0-T8.

- [x] [P3-T2] Run the two new tests against the fixed code, using the POSIX vstest directory resolved in P0-T5:

  ```text
  MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p3-t2 /TestCaseFilter:"FullyQualifiedName=UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize|FullyQualifiedName=UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance"
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p3-t2-regression-green.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording `Total tests`, `Passed`, `Failed`, and both test names with their individual outcomes. Acceptance: `EXIT_CODE: 0` as observed from the shell; `Total tests: 2` and `Passed: 2` as observed in the console summary block; and `Failed: 0` read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p3-t2/`.

  The `Failed` value is sourced from the TRX rather than the console because a green `vstest.console.exe` run prints no `Failed:` line at all — measured across four green runs in this worktree during preflight round 5, the whole summary block a successful run emits is `Test Run Successful.` followed by `Total tests`, `Passed`, and `Total time` only. See constraint 5 in "Shell constraints measured in this worktree". Both test names and their individual outcomes remain console-observed, because per-test result lines ARE printed on a green run. This task records no `Skipped` figure, so the `total` minus `executed` derivation stated in constraint 5 does not apply here; the `notExecuted` attribute is not read by this task and MUST NOT be introduced into it. `TestResults/p3-t2/` is written by this task and by no other task in this plan; if this task has been run more than once, the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection. The TRX reference is subject to the same redaction rule P0-T10 states: identify the file by its repository-relative results directory only, never by its own name and never by quoting the run's `Results File:` console line, because `vstest.console.exe` composes the default TRX filename from the host account name and the machine name and prints it inside a full absolute host path.

- [x] [P3-T3] Run the four `UtilitiesCS.Test` classes the spec names as at risk, plus the fifth class P1-T5 modifies, using the POSIX vstest directory resolved in P0-T5:

  ```text
  MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p3-t3 /TestCaseFilter:"FullyQualifiedName~UtilitiesCS.Test.Threading.IdleAsyncQueue_Tests|FullyQualifiedName~UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests|FullyQualifiedName~UtilitiesCS.Test.ProgressTracker_Tests|FullyQualifiedName~WpfDispatcherYieldTests|FullyQualifiedName~OutlookFolderTreeServiceConcurrencyTests"
  ```

  `UtilitiesCS.Test.ProgressTracker_Tests` is the fully-qualified name of the class declared in `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`; that file declares `namespace UtilitiesCS.Test` on line 12, not `UtilitiesCS.Test.Threading`, re-derived this pass. It is included here because P1-T5 modifies it, and it is not a prefix of any other class name in this filter.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p3-t3-at-risk-tests.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording `Total tests`, `Passed`, `Failed`, and the name and outcome of every executed test. `Total tests` and `Passed` are read from the console summary block; `Failed` is read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p3-t3/`, because a green run prints no aggregate `Failed:` line on the console (constraint 5 in "Shell constraints measured in this worktree"). The per-test names and outcomes remain console-observed. This task records no `Skipped` figure, so the `total` minus `executed` derivation stated in constraint 5 does not apply here; the `notExecuted` attribute is not read by this task and MUST NOT be introduced into it. `TestResults/p3-t3/` is written by this task and by no other task in this plan; if this task has been run more than once — which its own clause 1 below anticipates, since a zero-test run requires a corrected filter and a re-run — the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection. The TRX reference is subject to the same redaction rule P0-T10 states: identify the file by its repository-relative results directory only, never by its own name and never by quoting the run's `Results File:` console line. Acceptance:

  1. `Total tests` is greater than zero. A zero-test run means the filter matched nothing and proves nothing; treat it as a failure of this task and correct the filter.
  2. The executed set includes, by name, `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`, `YieldAsync_WithoutDispatcher_RemainsStrict`, `InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker`, `GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher`, and `Initialize_WithCurrentDispatcherAndScreen_InitializesViewerAndUpdatesUi` (the last is the `[STATestMethod]` at `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` line 412 that writes `UiThread._dispatcher`, re-derived this pass).
  3. The failing set is empty, or every member of it is also a member of the `BASELINE_FAILURE_SET` recorded in P0-T10. Any failing test that is not in that baseline set fails this task. If one of the five named tests appears in the failing set and also in the baseline set, record `PRE-EXISTING FAILURE: <test name>` in the artifact and report it to the caller before AC4 is marked, because AC4's wording is "all pass".

- [x] [P3-T4] Re-verify AC3 against the committed tree rather than against the P0 reading. Run:

  ```text
  git add -A -- UtilitiesCS UtilitiesCS.Test
  git status --porcelain -- UtilitiesCS/Threading/ProgressTrackerAsync.cs
  git diff --name-status --cached 87cb4df338322844abfa580abea14df77e738e5c -- UtilitiesCS/Threading/ProgressTrackerAsync.cs
  git grep -n -F "UiDispatcher = UiThread.Dispatcher;" -- UtilitiesCS/Threading/ProgressTrackerAsync.cs
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/other/p3-t4-progresstrackerasync-unmodified.md` with `Timestamp:`, `Command:` (all four), `EXIT_CODE:` (per command), `Output Summary:`. The `Output Summary:` MUST state, in one paragraph, why the fix in `UtilitiesCS/Threading/UiThread.cs` alone converts this consumer's failure mode: the property read on line 33 now throws before the dereference on line 35 is reached, so the consumer receives a self-diagnosing `InvalidOperationException` at the property-access line without a code change. Acceptance: the porcelain status command prints nothing, the `--cached` name-status diff prints nothing, and the grep prints exactly one line whose line number is 33.

  On what each span actually observes: `git status --porcelain` reports both the index and the working tree, so it is the span that observes the staged state produced by the preceding `git add`. A two-dot `git diff A..HEAD` never observes the index at all — it compares two commits — and before this plan's first commit (P5-T9) it would compare BASE against a HEAD that is identical to it across this task's pathspec — the reconciliation merge recorded in the BASE re-anchor note changed no path under `UtilitiesCS` or `UtilitiesCS.Test` relative to BASE — and so print nothing whatever the executor wrote, which is a vacuous pass. The `--cached` form used above compares the index against the named commit, so it observes the staged state directly and reports a real change to this path if one is staged. The two spans are complementary: `--cached` is blind to an unstaged working-tree edit, and porcelain is the span that catches that case.

- [x] [P3-T5] Verify AC5 across the whole change. Run:

  ```text
  mkdir -p TestResults
  git diff 87cb4df338322844abfa580abea14df77e738e5c -- UtilitiesCS UtilitiesCS.Test > TestResults/p3-t5-source.diff
  grep -E '^\+' TestResults/p3-t5-source.diff | grep -E -i 'Thread\.Sleep|Task\.Delay|SpinWait|Retry|retries|Timeout\(|PushFrame'
  ```

  The filter is a plain POSIX `grep` pipeline. It has to be: this worktree's shell refuses any command named for a PowerShell 7 host, in every argument shape (see constraint 1 in "Shell constraints measured in this worktree"), so a PowerShell-based filter would leave this gate with no runnable command at all. `grep` is available in the same shell that runs the two preceding spans. The `-i` flag supplies case-insensitive matching, and the seven-token list is the same list P1-T2's authoring constraint enumerates.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p3-t5-no-timing-tokens.md` with `Timestamp:`, `Command:` (all three), `EXIT_CODE:` (per command), and `Output Summary:` recording the byte size of `TestResults/p3-t5-source.diff` and quoting the last command's output verbatim. Acceptance: `TestResults/p3-t5-source.diff` is non-empty (an empty diff means the gate had nothing to inspect and the result is BLOCKED, not PASS), and the third command prints nothing and its second `grep` exits 1, which is what `grep` returns when it finds no match. If it prints any line, the change violates AC5 and the offending construct must be removed before proceeding.

  Two properties of the diff span are load-bearing and must not be "simplified". First, it is anchored to the BASE SHA rather than left bare, so it cannot silently degrade into a worktree-versus-index comparison. Second, it uses the **single-ref** form `git diff <SHA> -- <paths>`, which compares the working tree against that commit, and NOT the two-dot form `git diff <SHA>..HEAD`. This plan's first commit is P5-T9, so at Phase 3 the branch HEAD is still identical to BASE across `UtilitiesCS` and `UtilitiesCS.Test` — the reconciliation merge recorded in the BASE re-anchor note changed no path under either directory relative to BASE — and a two-dot span would emit an empty diff no matter what the executor wrote; the `grep` filter would then print nothing and the gate would pass vacuously. The single-ref form is blind to untracked files, which is harmless here because all five files this plan writes are already tracked at BASE (re-derived this pass from UtilitiesCS.Test/UtilitiesCS.Test.csproj lines 477, 479, 490, and 494, and from the presence of `UtilitiesCS/Threading/UiThread.cs` at BASE).

  The redirection target is written with forward slashes for the reason stated in P0-T10: a backslash-spelled `TestResults\p3-t5-source.diff` would be created as `TestResultsp3-t5-source.diff` at the worktree root, which .gitignore's `[Tt]est[Rr]esult*/` rule on line 39 does not match, which P5-T10's scoped porcelain check does not see, and which the following `grep -E '^\+' TestResults/p3-t5-source.diff` would then fail to open — producing a second, independent vacuous pass of this gate.

- [x] [P3-T6] Run the `QuickFiler.Test` class that constructs the parameterless `WpfUiDispatcher`, whose provider closes over `UiThread.Dispatcher`. This class is NOT named in `spec.md` or in the research trail; it was found during this plan's adversarial self-review by enumerating `new WpfUiDispatcher(` across the repository. Command:

  ```text
  MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p3-t6 /TestCaseFilter:"FullyQualifiedName~QuickFiler.Controllers.Tests.WpfUiDispatcherTests"
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p3-t6-quickfiler-wpfuidispatcher.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording `Total tests`, `Passed`, `Failed`, and each test name with its outcome. Acceptance: `Total tests` is 2 as observed in the console summary block; `Failed: 0` read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p3-t6/`; and both `Construction_YieldsAnIUiDispatcher` and `Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread` are listed by name as passing in the console output.

  The `Failed` value is sourced from the TRX because a green `vstest.console.exe` run prints no `Failed:` line at all, measured across four green runs in this worktree during preflight round 5 (constraint 5 in "Shell constraints measured in this worktree"). The by-name clause is unaffected by that measurement and is unchanged: the console DOES print per-test pass and fail lines on a green run, and the two names above are the two `[TestMethod]`s declared in QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs at lines 24 and 50, re-derived this pass. This task records no `Skipped` figure, so the `total` minus `executed` derivation stated in constraint 5 does not apply here; the `notExecuted` attribute is not read by this task and MUST NOT be introduced into it. `TestResults/p3-t6/` is written by this task and by no other task in this plan; if this task has been run more than once, the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection. The TRX reference is subject to the same redaction rule P0-T10 states: identify the file by its repository-relative results directory only, never by its own name and never by quoting the run's `Results File:` console line, because `vstest.console.exe` composes the default TRX filename from the host account name and the machine name and prints it inside a full absolute host path.

  The plan-time expectation is that neither is affected — the constructor only captures the provider lambda without invoking it, and the second test installs a real dispatcher through `UiThreadDispatcherFixture` before any forwarding call — but that expectation is verified by running the tests, not asserted from reading.

### Phase 4 — Final QA loop (format, analyze, type-check, test, coverage)

**Second pass, and why the first pass's marks were cleared.** P4-T1 through P4-T5 completed one pass
on 2026-09-03 and P4-T6 then failed, which is what produced the revision round 15 scope widening
recorded in the header. P2-T4 rewrites a tracked source file after that pass ran, so the four earlier
steps' artifacts describe a tree that no longer exists. AC6 requires the toolchain to pass "in order
in a single final pass", and that requirement is what the cleared marks restore: every task in this
phase runs again, in order, against the tree P2-T4 leaves. The per-step artifact paths are unchanged,
so each is overwritten by the second pass; the first pass is not lost, because P4-T8's acceptance
below requires the loop-closure artifact to record every pass in chronological order, the first one
included, and to state the first pass's outcome including P4-T6's eight failures. No task ID and no command line in this phase changed in round 15
apart from the owned-path lists in P4-T1, P4-T2, and P4-T8 and the restated acceptance in P4-T6.

- [x] [P4-T1] Format, with the formatter's write scope restricted to the six paths this plan owns. Run, from the worktree root:

  ```text
  git status --porcelain
  dotnet tool run csharpier format UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"
  git status --porcelain
  ```

  This plan's owned file set is exactly the six paths named on that command line:

  - `UtilitiesCS/Threading/UiThread.cs`
  - `UtilitiesCS.Test/Threading/UiThread_Tests.cs`
  - `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`
  - `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`
  - `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`
  - `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`

  The sixth operand is double-quoted because its directory is spelled `Helper Classes`; see the note in the "Scope" section. An unquoted operand would be split by the shell and CSharpier would be handed two paths that do not exist.

  **Option chosen for the repo-wide-drift problem, and why.** The preflight round-2 review identified that `dotnet tool run csharpier format .` formats the ENTIRE repository, so drift it repairs outside this plan's owned paths — in `QuickFiler/`, `TaskMaster/`, `ToDoModel/`, and elsewhere — would be rewritten, would not be restored by a check scoped to those two directories, would not be committed, and would be invisible to every terminal porcelain gate in this plan, which are all scoped the same way. Two remedies were available: widen the porcelain check and the `git checkout --` restoration to the whole worktree, or restrict the formatter's write scope so the unowned drift is never created. This plan takes the second. The reason is that the first remedy makes the gate depend on the whole worktree's ambient state, including tracked directories this plan has no relationship with (.claude/agent-memory/ is tracked in this repository), so a concurrent or pre-existing modification anywhere would either fail the gate or force an exclusion list that grows without bound. Restricting the formatter's write scope removes the failure mode at its source: a file the formatter is never given cannot be rewritten.

  Repository policy is preserved by this choice. CSharpier is file-based and formats exactly the paths it is given, so the six owned files receive character-for-character the formatting `csharpier format .` would have applied to them. The repo-wide obligation is discharged on the verification side, unchanged: P4-T2 still runs `dotnet tool run csharpier check .` over the whole tree, which is the same read-only, CI-parity command .github/workflows/_format-check.yml line 41 runs (`dotnet csharpier check .`, re-derived this pass). A formatting regression anywhere in the repository therefore still surfaces; what this task no longer does is silently repair a pre-existing one.

  If the multi-path invocation is rejected by the pinned CSharpier 1.2.6 CLI, run `dotnet tool run csharpier format <path>` once per owned path instead and record all six invocations. Both forms have identical write scope, so the acceptance below is unaffected.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t1-format.md` with `Timestamp:`, `Command:` (all three, or all eight if the per-path fallback was used), `EXIT_CODE:` (per command), and `Output Summary:` quoting the formatter's trailing summary line verbatim (CSharpier prints a `Formatted N files in Xms` line, where `N` is the count of files processed rather than the count rewritten, so the number alone is not evidence of a no-op), the unscoped porcelain output taken before the formatter ran, the unscoped porcelain output taken after it ran, and the single line `RESTORED_UNOWNED_FORMAT_DRIFT: NOT APPLICABLE (formatter write scope restricted to the six owned paths)`. Acceptance: `EXIT_CODE: 0` for the formatter; and the two unscoped porcelain outputs differ, if at all, only in entries for the six owned paths above. The porcelain spans are deliberately UNSCOPED here — unlike the terminal gates in P5-T10 and P5-T11, whose pathspecs exist to keep unrelated tracked state from making them unsatisfiable — because the property this task must establish is precisely that no path outside the owned five changed, and a scoped span cannot observe that. Comparing the before and after outputs, rather than asserting an empty one, is what makes the observation independent of whatever ambient modifications already existed when the task began.

  This task records a before-and-after tree observation in addition to the formatter's exit code, because a formatter rewrites tracked source and still exits 0 after rewriting: its exit code alone is identical on a clean run and on a repairing one.

- [x] [P4-T2] Verify formatting. Run `dotnet tool run csharpier check .`. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t2-format-check.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` enumerating every path the command reports as unformatted. Acceptance: none of this plan's six owned paths — `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` — appears in the reported set, and the reported set is a subset of the `BASELINE_FORMAT_DRIFT_SET` recorded in P0-T7. When that baseline set was `NONE`, this reduces to `EXIT_CODE: 0` with an empty reported set.

  The subset clause is the whole-tree half of this gate and is not slack. P4-T1 restricts the formatter's write scope to the six owned paths, so it repairs no pre-existing drift and creates none: every path this command reports must therefore already have been reported at P0-T7. A path in the reported set that is absent from `BASELINE_FORMAT_DRIFT_SET` is new drift introduced during this plan's execution and fails this task. This command is run over the whole repository (`.`), matching .github/workflows/_format-check.yml line 41 exactly, so the check retains full repository scope even though P4-T1's write scope is narrow.

- [x] [P4-T3] Analyze. Run:

  ```text
  MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t3-analyzer-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the trailing `N Warning(s)` and `N Error(s)` counts. Acceptance: `EXIT_CODE: 0`, `0 Error(s)`, and the warning count is less than or equal to the baseline analyzer warning count from P0-T8.

- [x] [P4-T4] Type-check. Run:

  ```text
  MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t4-nullable-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the trailing `N Error(s)` count and the quoted command line. Acceptance: `EXIT_CODE: 0`, `0 Error(s)`, and the quoted command line contains `msbuild.exe TaskMaster.sln`, contains no `Nullable=enable` substring, and uses `/t:Rebuild`. As in P0-T9, that first clause is worded as `contains` because the recorded line begins with the `MSYS_NO_PATHCONV=1 ` assignment, and it records only the executable spelling this shell requires; the `Nullable=enable` and `/t:Rebuild` clauses are the substantive checks and are unchanged. This gate is the one that proves AC2's real value: with the backing field now declared `Dispatcher?` in a file that opts into nullable analysis, a getter that returned the field without narrowing it would raise `CS8603` and fail here.

- [x] [P4-T5] Test `UtilitiesCS.Test` with coverage, using the native vstest directory resolved in P0-T5:

  ```text
  MSYS_NO_PATHCONV=1 dotnet-coverage collect --output coverage/p4-t5.cobertura.xml --output-format cobertura --settings coverage.config -- "<resolved-vstest-dir-native>\vstest.console.exe" UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults/p4-t5 /TestCaseFilter:TestCategory!=LiveOutlook
  ```

  This command's flag set is deliberately identical to P0-T10's; the two differ only in the `--output` filename and the `/ResultsDirectory` value. The executable operand is spelled identically to P0-T10's — the same double-quoted `<resolved-vstest-dir-native>` substitution, supplied as an ARGUMENT after `--` rather than as a command name, and therefore without the `PATH=` prefix the six direct vstest tasks carry. The `MSYS_NO_PATHCONV=1` prefix IS carried here, exactly as in P0-T10, and its presence on both sides of the pair is part of what keeps the two runs command-identical; a prefix on one side only would mean one run executed the suite and the other executed nothing. Keeping the two spellings identical is a precondition of P4-T7's comparison. In particular `/EnableCodeCoverage` is deliberately absent from both. Adding it here alone would activate a second, nested collector underneath `dotnet-coverage collect`, changing the loaded-module set and therefore the `lines-valid` denominator, and P4-T7's baseline-to-post-change comparison would no longer be a comparison of like with like. `dotnet-coverage collect` alone already produces the Cobertura file P4-T7 reads.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t5-utilitiescs-tests.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording `Total tests`, `Passed`, `Failed`, `Skipped`, the TRX `total` and `executed` values from which the `Skipped` figure is derived, and the `lines-covered`, `lines-valid`, and `line-rate` attribute values read from the root `<coverage>` element of `coverage/p4-t5.cobertura.xml`. `Total tests` and `Passed` are read from the console summary block; `Failed` is read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p4-t5/`; and `Skipped` is DERIVED from that same element as `total` minus `executed`, with `total`, `executed`, and the derived `Skipped` value all recorded. The `notExecuted` attribute MUST NOT be used: `vstest.console.exe`'s TRX logger populates only `total`, `executed`, `passed`, and `failed` on the `<Counters .../>` element and hard-codes every other attribute (`notExecuted`, `error`, `timeout`, `aborted`, `inconclusive`, ...) to `0` regardless of the run's actual outcome. Measured in preflight round 6: a run whose console printed `Skipped: 1` produced a TRX with `notExecuted="0"`; the derivation `total` minus `executed` correctly returned `1` on that run and `0` on an all-passing run. Both counts come from the TRX rather than the console because a green run prints neither aggregate line on the console (constraint 5 in "Shell constraints measured in this worktree"). The failing test names, when there are any, remain console-observed. `TestResults/p4-t5/` is written by this task and by no other task in this plan; if this task has been run more than once — which P4-T8's loop-restart text anticipates — the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection. The TRX reference is subject to the same redaction rule P0-T10 states: identify the file by its repository-relative results directory only, never by its own name and never by quoting the run's `Results File:` console line. Acceptance: the failing-test set is empty, or is a subset of the `BASELINE_FAILURE_SET` recorded in P0-T10 with no new member; `Total tests` is greater than or equal to the baseline `Total tests` plus 2; the `total` and `executed` values from which `Skipped` was derived are recorded; and all three coverage attribute values are recorded as concrete numbers.

- [x] [P4-T6] Test `QuickFiler.Test`, using the POSIX vstest directory resolved in P0-T5:

  ```text
  MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p4-t6 /TestCaseFilter:TestCategory!=LiveOutlook
  ```

  This command's flag set is deliberately identical to P0-T11's, including the `MSYS_NO_PATHCONV=1` and `PATH=` prefixes and the bare `vstest.console.exe` command name; the two differ only in the `/ResultsDirectory` value. `/EnableCodeCoverage` is deliberately absent from both: this task records no coverage figure, so the flag would have no consumer here, and its presence on one side of a baseline-to-post-change pair and not the other is exactly the asymmetry that makes two runs incomparable.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t6-quickfiler-tests.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording `Total tests`, `Passed`, `Failed`, `Skipped`, and the TRX `total` and `executed` values from which the `Skipped` figure is derived. `Total tests` and `Passed` are read from the console summary block; `Failed` is read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p4-t6/`; and `Skipped` is DERIVED from that same element as `total` minus `executed`, with `total`, `executed`, and the derived `Skipped` value all recorded. The `notExecuted` attribute MUST NOT be used: `vstest.console.exe`'s TRX logger populates only `total`, `executed`, `passed`, and `failed` on the `<Counters .../>` element and hard-codes every other attribute (`notExecuted`, `error`, `timeout`, `aborted`, `inconclusive`, ...) to `0` regardless of the run's actual outcome. Measured in preflight round 6: a run whose console printed `Skipped: 1` produced a TRX with `notExecuted="0"`; the derivation `total` minus `executed` correctly returned `1` on that run and `0` on an all-passing run. Both counts come from the TRX rather than the console because a green run prints neither aggregate line on the console (constraint 5 in "Shell constraints measured in this worktree"). The failing test names, when there are any, remain console-observed. `TestResults/p4-t6/` is written by this task and by no other task in this plan; if this task has been run more than once — which P4-T8's loop-restart text anticipates — the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection. The TRX reference is subject to the same redaction rule P0-T10 states: identify the file by its repository-relative results directory only, never by its own name and never by quoting the run's `Results File:` console line. Acceptance, restated in revision round 15 so that it names the concrete figures this run must produce rather than only a subset relation:

  1. `Total tests` equals the baseline `Total tests` recorded in P0-T11, which is `1312`. This plan adds no test to this assembly and P2-T4 adds none either, so any other value means the assembly or the filter changed and the task fails.
  2. `Passed` equals `1312` and the failing-test set is EMPTY. The subset-of-`BASELINE_FAILURE_SET` wording this clause replaces was correct in form but vacuous in substance here, because P0-T11 recorded an empty `BASELINE_FAILURE_SET` (1312 of 1312 passed at BASE): the only set that is a subset of the empty set is the empty set, and stating the figure directly is what makes the gate readable against the artifact.
  3. `Failed` equals `0`, read from the `failed` attribute of the single `<Counters .../>` element in the TRX this task writes under `TestResults/p4-t6/`.
  4. The `total` and `executed` values from which `Skipped` was derived are recorded, and the derived `Skipped` is `0`.
  5. All eight `EmailMoveMonitorTests` methods are listed by name as passing in the console output: `HookItem_FirstItemOfFolder_SubscribesBeforeItemMoveOnce_AndSharedFolderDoesNotResubscribe`, `UnhookItem_RemovesLastItemForFolder_UnsubscribesBeforeItemMoveOnlyOnLastItem`, `UnhookItem_Null_IsNoOp_NoComAccessNoMarshalInvocation`, `UnhookItem_UsesCachedEntryIds_RemovesExactlyTheMatchingEntry`, `AllComAccess_FlowsThroughInjectedMarshalDelegate`, `UnhookAll_UnsubscribesEveryFolder_AndClearsState`, `DuplicateHookOfSameItem_AndUnhookNeverHookedItem_DoNotThrowOrSpuriouslyUnsubscribe`, and `UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread`. These are exactly the eight the first pass of this task reported as failing, quoted from the preserved first-pass copy at `evidence/regression-testing/p4-t6-first-pass-failure.md`, so this clause is the direct pass-after counterpart of that recorded fail-before. Per-test pass lines ARE printed on a green run (constraint 5 in "Shell constraints measured in this worktree"), so this clause is readable from the console output.

  **Do this before running the command above.** The first pass of this task is the fail-before evidence for P2-T4. Its artifact records `Total tests: 1312`, `Passed: 1304`, `Failed: 8`, the eight test names, and an executed counterfactual isolating the cause to P2-T1. When this task is re-run, its artifact is overwritten by that re-run, so the fail-before record must be preserved under a separate name first. That preserved copy is the artifact AC4's check-off cites as the fail-before half of the sixth owned file's repair.

  The preservation MUST be conditional on the destination's absence, and an unconditional copy is a defect rather than a stylistic choice. This task can run more than once — P4-T8's clause 1 restarts the Phase 4 loop from P4-T1 whenever a step rewrites a tracked file — and on any pass after the first, `evidence/qa-gates/p4-t6-quickfiler-tests.md` holds the pass-after result of `1312` of `1312`. An unconditional copy would then overwrite the preserved fail-before record with a passing run under a filename that asserts the opposite, and because AC4's check-off cites that file as the fail-before half of the repair, the corruption would silently invalidate AC4's evidence chain rather than fail any gate. As the first action of this task, run, from the worktree root:

  ```text
  if [ -f docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p4-t6-first-pass-failure.md ]; then echo "FAIL_BEFORE_PRESERVATION: FOUND IN PLACE, LEFT UNTOUCHED"; else cp docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t6-quickfiler-tests.md docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p4-t6-first-pass-failure.md; echo "FAIL_BEFORE_PRESERVATION: CREATED FROM THE FIRST-PASS ARTIFACT"; fi
  grep -n -F 'Failed: 8' docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p4-t6-first-pass-failure.md
  ```

  The conditional is a plain POSIX `if [ -f ... ]` test in the same git-bash shell every other command block in this plan runs through; a PowerShell-hosted form is unavailable here (constraint 1 in "Shell constraints measured in this worktree"). `cp -n` is deliberately NOT used in its place: it exits 0 whether or not it copied, so the no-copy case would leave no observable at all, which is the specific failure the clause below is written to exclude.

  Both branches record an observable and neither is a silent no-op. The create branch prints `FAIL_BEFORE_PRESERVATION: CREATED FROM THE FIRST-PASS ARTIFACT`; the already-present branch prints `FAIL_BEFORE_PRESERVATION: FOUND IN PLACE, LEFT UNTOUCHED`, which records that the preserved fail-before record was found in place and left untouched. The second command then runs in EITHER branch and prints the `Failed: 8` line the preserved file carries, so the fail-before figure is verified against the file rather than assumed. Record the printed branch line and the `grep` output in this task's artifact under the heading `FAIL_BEFORE_PRESERVATION:`. If the `grep` prints nothing and exits 1, stop and report BLOCKED: a preserved file that does not carry `Failed: 8` is either a pass-after copied under the wrong name or a truncated file, and in both cases AC4's fail-before half is absent rather than merely unverified, so proceeding would let AC4 be marked on an evidence chain that does not close.

- [x] [P4-T7] Compute and record the coverage delta and the changed-line coverage. Derive the added-line set as the line numbers of `+` lines produced by:

  ```text
  git diff 87cb4df338322844abfa580abea14df77e738e5c -- UtilitiesCS/Threading/UiThread.cs
  ```

  This is the single-ref working-tree form, anchored to BASE, for the same reason stated in P3-T5: this plan's first commit is P5-T9 and the reconciliation merge recorded in the BASE re-anchor note changed nothing in this file relative to BASE, so a two-dot `87cb4df338322844abfa580abea14df77e738e5c..HEAD` span would return an empty diff at Phase 4, the added-line set would be empty, and clause (a) below could never be satisfied. `UtilitiesCS/Threading/UiThread.cs` is tracked at BASE, so the single-ref form's blindness to untracked files does not apply to it.

  Read `coverage/p4-t5.cobertura.xml`, locate the class node whose `filename` attribute ends in `Threading\UiThread.cs` or `Threading/UiThread.cs` (Cobertura emits the host path separator in that attribute value; accept either), and intersect its `<line number=...>` elements with the added-line set.

  **Redaction rule for this task's artifact.** `dotnet-coverage` writes the `filename` attribute as a full absolute host path — it begins with the drive letter and includes the user profile directory, so it contains a host account name. When recording the located class node in the evidence artifact, record ONLY the line-hit data: the `<line number=...>` values and their `hits` values. Do NOT record the `filename` attribute's absolute-path value verbatim. Identify the node in the artifact by the repository-relative path `UtilitiesCS/Threading/UiThread.cs` instead. The same rule applies to any other absolute path this task encounters while reading the Cobertura file.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t7-coverage-delta.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording, as concrete numbers: the baseline `lines-covered`, `lines-valid`, and `line-rate` from P0-T10; the post-change `lines-covered`, `lines-valid`, and `line-rate` from P4-T5; the signed difference `post-change lines-valid` minus `baseline lines-valid`; the added-line set; the intersected line numbers with their `hits` values; and the resulting changed-line coverage percentage. Both recorded `line-rate` figures MUST be labelled, verbatim, `raw unstripped dotnet-coverage line-rate for the UtilitiesCS.Test process; not the repository first-party figure CLAUDE.md's 80% refers to`. The word "single-assembly" is deliberately absent from that label: `dotnet-coverage collect` instruments the whole test host process and reports every first-party module loaded into it, not one assembly, so `UtilitiesCS` and `UtilitiesCS.Test` both contribute to `lines-valid` (which is also why P4-T7 clause (c) is a band rather than an equality). The label's purpose — marking the figure as not comparable to the repository-wide first-party percentage CLAUDE.md states — is unchanged. Acceptance:

  (a) The intersected set contains at least two line numbers. If it contains fewer, the coverage report did not resolve this file and the result is BLOCKED, not PASS.

  (b) Every intersected line has `hits` of 1 or more, giving 100% changed-line coverage, which satisfies the `>= 90%` new-code target from CLAUDE.md.

  (c) The denominator is comparable: the signed `lines-valid` difference is between 0 and 200 inclusive. Because P0-T10 and P4-T5 now run a flag-identical command, the only legitimate source of a `lines-valid` change between them is the source this plan adds — approximately six coverable lines in `UtilitiesCS/Threading/UiThread.cs` and approximately seventy-five in `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, both of which are inside the denominator because `coverage.config` excludes third-party module paths only (re-derived this pass). An exact-equality clause would be unsatisfiable for that reason and is deliberately not used. A difference outside the stated band indicates the collector's loaded-module set differed between the two runs — a mismatch of that kind has moved this denominator by tens of thousands of lines on an unchanged tree in this repository. If the difference falls outside the band, record `COVERAGE DENOMINATOR MISMATCH`, state explicitly that the repository-wide percentage comparison in clause (d) is VOID, and rest this gate on clauses (a) and (b) alone.

  (d) The post-change `line-rate` is greater than or equal to the baseline `line-rate` minus 0.005, the stated tolerance absorbing run-to-run nondeterminism in this suite. This clause is skipped and marked VOID when clause (c) recorded a denominator mismatch.

  Also record the post-change `line-rate` against the `>= 80%` repository figure from CLAUDE.md as an observation, not a gate. That observation is explicitly non-comparable to the policy figure: it is the raw, unstripped `dotnet-coverage` line rate for the `UtilitiesCS.Test` process, whereas CLAUDE.md's 80% refers to the repository's first-party testable denominator after third-party stripping. If the post-change figure is below that floor while the baseline figure was also below it, record `PRE-EXISTING FLOOR SHORTFALL` and do not treat it as caused by this change.

- [x] [P4-T8] Confirm the loop closed in a single clean pass and re-audit file sizes after the formatter ran. Re-run the P2-T3 command, extended with the sixth owned path:

  ```text
  wc -l UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"
  ```

  The first five operands are character-for-character the command P0-T13 and P2-T3 ran, for the reason stated in P0-T13: the before and after counts must come from one counting idiom or the comparison is incommensurable. The sixth operand is added by revision round 15 and is double-quoted because its directory is spelled `Helper Classes`. Its baseline for comparison is not P0-T13, which predates the scope widening and does not measure it, but the post-edit count P2-T4 records. Read the six named per-file rows and ignore the trailing `total` row.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t8-loop-closure.md` with `Timestamp:`, `Command:` (the re-run command above, plus the string `see P4-T1..P4-T7` for the loop record), `EXIT_CODE:`, and `Output Summary:` listing each of P4-T1 through P4-T7 with its recorded exit code and artifact path, stating explicitly whether any step rewrote a tracked file, and quoting the six post-format per-file line counts. Acceptance:

  1. The artifact lists all seven steps in order with their artifacts, and records that no step after P4-T1 rewrote a tracked file. If any did, the loop restarts from P4-T1 and this artifact records every pass, per clause 2.
  2. The artifact records EVERY Phase 4 pass explicitly, one entry per pass, in chronological order from the earliest to the latest. The number of entries is open-ended and this clause states no upper bound on it: clause 1 above can restart the loop from P4-T1 again, so a clause that demanded exactly two entries would become unsatisfiable on a third pass while enforcing nothing extra on the second. The FIRST entry MUST be the pass that ran on 2026-09-03 with P4-T1 through P4-T5 green and P4-T6 failing 8 of 1312, and the LAST entry MUST be the pass whose artifacts this task lists. Omitting the first pass is not acceptable: it is the fail-before evidence for P2-T4 and the reason this phase ran more than once, and an artifact recording only the final pass would misrepresent a multi-pass execution as a single clean one.
  3. The first five post-format line counts satisfy the same two clauses P2-T3 states, evaluated against the P0-T13 baseline counts.
  4. The sixth post-format line count is strictly less than 500 and is less than or equal to the post-edit count P2-T4 recorded, plus 2. The plan's intent is a count unchanged from P2-T4's; the plus-two tolerance exists solely because `csharpier` may re-wrap the retargeted `GetField(` call, which is a formatter decision this plan does not control. A count above that bound means the formatter or a later edit added lines this plan did not describe, and the task fails.

  The re-run exists because `csharpier format` in P4-T1 can change line counts, so a size audit taken only at P2-T3 and P2-T4 would describe a pre-format tree.

### Phase 5 — Acceptance criteria, documentation, and handoff

- [x] [P5-T1] Mark AC1 in `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` (the bullet beginning "AC1: `UiThread.Dispatcher` throws a named `InvalidOperationException`"). Acceptance: that bullet reads `- [x]` and the marking is accompanied by a citation of `evidence/regression-testing/p1-t4-expect-fail.md` (recording `Failed: 1`) and `evidence/regression-testing/p3-t2-regression-green.md` (recording `Passed: 2`).

- [x] [P5-T2] Mark AC2 in the same file (the bullet beginning "AC2: The `null!` null-forgiving suppression"). Acceptance: that bullet reads `- [x]` and cites `evidence/qa-gates/p2-t2-nullforgiving-removed.md` recording zero `null!` matches in `UtilitiesCS/Threading/UiThread.cs`, and `evidence/qa-gates/p4-t4-nullable-build.md` recording `0 Error(s)`.

- [x] [P5-T3] Mark AC3 in the same file (the bullet beginning "AC3: UtilitiesCS/Threading/ProgressTrackerAsync.cs is left unmodified"). Acceptance: that bullet reads `- [x]` and cites `evidence/other/p3-t4-progresstrackerasync-unmodified.md`, which must contain the empty `--cached` name-status diff for that path, the empty porcelain status for that path, and the recorded verification paragraph.

- [x] [P5-T4] Mark AC4 in the same file (the bullet beginning "AC4: No regression in"). AC4's enumerated no-regression FILE set was extended to five files in revision round 15; this task's citation list is extended to match. Two different counts appear in this task and they are not interchangeable: the no-regression FILE set holds five files, and the EVIDENCE set holds six artifacts, because the sixth owned file contributes both a fail-before and a pass-after artifact while each of the other five contributes at most one. Acceptance: that bullet reads `- [x]` and cites exactly the six artifacts listed in the AC4 row of the acceptance-criteria mapping table above — six is the count that row carries, re-derived in the revision round 16 pass against the table row itself, against the `AC-MAPPING: AC4` line at the end of this file, and against AC4's own evidence list in `spec.md` — namely: `evidence/qa-gates/p1-t5-donotparallelize.md` (recording that the change to `IdleAsyncQueue_Tests.cs` and `ProgressTrackerAsync_Tests.cs` is attribute-only and alters no assertion), `evidence/regression-testing/p3-t3-at-risk-tests.md` (five named tests executed, no failure outside the recorded `BASELINE_FAILURE_SET`), `evidence/regression-testing/p3-t6-quickfiler-wpfuidispatcher.md` (`Failed: 0`), `evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md` (recording that the change to `EmailMoveMonitorTests.cs` retargets one reflection lookup, alters no assertion, and leaves the `[TestMethod]` count at 8), and `evidence/regression-testing/p4-t6-first-pass-failure.md` together with `evidence/qa-gates/p4-t6-quickfiler-tests.md` (the fail-before 8 of 1312 and the pass-after 1312 of 1312 for that file). This bullet may not be marked while `evidence/qa-gates/p4-t6-quickfiler-tests.md` records a failure.

- [x] [P5-T5] Mark AC5 in the same file (the bullet beginning "AC5: No retry, sleep, or timing tolerance"). Acceptance: that bullet reads `- [x]` and cites `evidence/qa-gates/p3-t5-no-timing-tokens.md` recording zero matching added lines in the anchored diff over `UtilitiesCS` and `UtilitiesCS.Test`, and `evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md` recording zero matching lines in the anchored diff over the sixth owned file. The two together are what make AC5's "anywhere in the diff" wording true of the whole diff: P3-T5's pathspec covers five of the six owned files and P2-T4's covers the sixth.

- [x] [P5-T6] Mark AC6 in the same file (the bullet beginning "AC6: Full C# toolchain"). Acceptance: that bullet reads `- [x]` and cites exactly the seven artifacts listed in the AC6 row of the acceptance-criteria mapping table above: `evidence/qa-gates/p4-t1-format.md`, `evidence/qa-gates/p4-t2-format-check.md`, `evidence/qa-gates/p4-t3-analyzer-build.md`, `evidence/qa-gates/p4-t4-nullable-build.md`, `evidence/qa-gates/p4-t5-utilitiescs-tests.md`, `evidence/qa-gates/p4-t6-quickfiler-tests.md`, and `evidence/qa-gates/p4-t8-loop-closure.md`. `evidence/qa-gates/p4-t7-coverage-delta.md` is deliberately not cited here; it is AC7's evidence and is cited by P5-T7.

- [x] [P5-T7] Mark AC7 in the same file (the bullet beginning "AC7: Repository-wide line coverage does not regress"). Acceptance: that bullet reads `- [x]` and cites exactly the two artifacts listed in the AC7 row of the acceptance-criteria mapping table above — `evidence/baseline/p0-t10-utilitiescs-tests-coverage.md` for the baseline figures and `evidence/qa-gates/p4-t7-coverage-delta.md` for the comparison — quoting the concrete baseline and post-change `line-rate` values, the signed `lines-valid` difference, and the concrete changed-line coverage percentage. If P4-T7 recorded `COVERAGE DENOMINATOR MISMATCH`, this bullet is marked `- [x]` only on the strength of the changed-line clauses and the check-off text must say so explicitly.

- [x] [P5-T8] Mirror the issue update. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/issue-updates/issue-584.2026-09-02T09-02.md` containing `Timestamp:`, the exact text intended for issue #584, and `PostedAs:` set to `comment`, `body`, or `unknown`. If posting is blocked (for example `gh` is unavailable), begin the file with a `POSTING BLOCKED` header and the reason. Acceptance: the artifact exists and carries a `PostedAs:` line or a `POSTING BLOCKED` header; the plan does not halt on an unavailable `gh`.

- [x] [P5-T9] Commit the change. Run:

  ```text
  git add -- UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs" docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git commit -m "<message>" -- UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs" docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  ```

  where `<message>` summarises the accessor contract change and names issue #584. Acceptance: `git log -1 --name-only` lists all six source paths — `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, and `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` — and lists no path under .claude/, .codex/, .agents/, or config/.

  The sixth path is double-quoted in both spans because it contains a space. Unquoted, `git` would receive `QuickFiler.Test/Helper` and `Classes/EmailMoveMonitorTests.cs` as two pathspecs, neither of which matches anything, and the `git add` would fail with an unmatched-pathspec error rather than silently omitting the file — a loud failure, but one that quoting avoids entirely.

  The commit carries the same explicit pathspec as the `git add`, rather than being a bare `git commit` over the whole index. A bare commit would commit everything staged, and P3-T4 already ran `git add -A -- UtilitiesCS UtilitiesCS.Test`, which stages every modified or untracked path under those two directories rather than only this plan's own. The explicit-pathspec commit form is what actually bounds the committed footprint to the enumerated paths; the `git add` is retained ahead of it because a pathspec commit only accepts paths already known to git, so the evidence artifacts this plan creates under the feature folder must be staged first. P5-T10's porcelain span remains the backstop that reports anything left behind.

- [x] [P5-T10] Verify the committed footprint. Run:

  ```text
  git diff --name-status 87cb4df338322844abfa580abea14df77e738e5c..HEAD
  git status --porcelain -- UtilitiesCS UtilitiesCS.Test "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs" docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  ```

  The porcelain pathspec names the sixth owned file explicitly rather than the whole `QuickFiler.Test` directory. Scoping to the one file is deliberate: it is the only path this plan writes under that directory, and a directory-wide pathspec would let unrelated ambient state elsewhere in `QuickFiler.Test` make this gate unsatisfiable, which is the same failure mode the existing scoping of this span exists to avoid.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/other/p5-t10-footprint.md` with `Timestamp:`, `Command:` (both), `EXIT_CODE:` (per command), `Output Summary:` quoting both outputs verbatim. Acceptance: the anchored name-status diff lists exactly these six source paths and no other source path — `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` — apart from paths under `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/`; it lists no path in the `BASELINE_FORMAT_DRIFT_SET` recorded in P0-T7, because P4-T1's formatter write scope was restricted to the six owned paths and therefore rewrote no unowned path at all; it lists no path under .claude/, .codex/, .agents/, config/blast-radius.json, or config/orchestration-routing.json; and the porcelain output lists `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md` (modified by this plan's own check-off of P5-T9) and no path outside `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/`. P5-T11 commits both the plan file and this artifact together.

  The porcelain clause is worded that way because of the state this task actually runs in. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584` is inside this command's pathspec and contains this plan file; P5-T9 commits that whole folder, and the check-off protocol in `acceptance-criteria-tracking` marks P5-T9 `[x]` in this plan file once P5-T9 completes, so by the time this task runs the plan file is modified relative to that commit. `evidence/other/p5-t10-footprint.md` does not yet exist when these two commands run, because this task writes it from their output. An acceptance demanding that the porcelain output list "only this artifact" is therefore unsatisfiable in both directions at once, and is replaced by the clause above.

  The `git status --porcelain` span is the companion the name-status diff needs so that a file created but not yet tracked cannot escape the check. Here the two-dot form is correct and not vacuous, because P5-T9 has already committed, so `HEAD` is no longer identical to BASE.

- [x] [P5-T11] Commit the footprint artifact and confirm a clean tree. Run:

  ```text
  git add -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git commit -m "docs(584): record committed-footprint evidence" -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git status --porcelain -- UtilitiesCS UtilitiesCS.Test "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs" docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  ```

  Acceptance: the final porcelain command prints nothing. The status pathspec is scoped so that unrelated tracked state elsewhere in the worktree, including .claude/agent-memory/, cannot make this gate unsatisfiable or falsely satisfied. Revision round 15 added the sixth owned file to that pathspec by name rather than adding the whole `QuickFiler.Test` directory, for the same reason: naming the one file this plan writes keeps ambient state in that assembly out of the gate while still catching an uncommitted change to the file this plan owns there. The `git commit` carries the same explicit pathspec as the `git add`, for the reason stated in P5-T9: P3-T4 ran `git add -A -- UtilitiesCS UtilitiesCS.Test`, so a bare `git commit` here would commit whatever that left staged under those two directories, and P5-T10's name-status diff — the one span that would report it — has already run by this point.

- [x] [P5-T12] Write the acceptance-criteria status summary. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/other/p5-t12-ac-status-summary.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` listing AC1 through AC7 with, for each, its check state in `spec.md` and its evidence artifact path. Then run:

  ```text
  git add -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git commit -m "docs(584): record acceptance-criteria status summary" -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git status --porcelain -- UtilitiesCS UtilitiesCS.Test "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs" docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  ```

  Acceptance: all seven AC identifiers appear exactly once each in the artifact, every one is recorded as checked, every named artifact path exists on disk, and the final porcelain command prints nothing. The commit carries the same explicit pathspec as the `git add`, for the reason given in P5-T11.

- [ ] [P5-T13] Commit the plan's own final check-off state and confirm the tree is clean. Mark every remaining task in this plan file as `[x]`, including this task itself, then run:

  ```text
  git add -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git commit -m "docs(584): record final plan and acceptance-criteria check-off state" -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git status --porcelain -- UtilitiesCS UtilitiesCS.Test "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs" docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  ```

  Acceptance: the final `git status --porcelain` command prints nothing. The commit carries the same explicit pathspec as the `git add`, matching P5-T9, P5-T11, and P5-T12; without it this commit would sweep anything still staged from P3-T4's `git add -A -- UtilitiesCS UtilitiesCS.Test`.

  This task exists because `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md` is itself inside the pathspec of P5-T11's and P5-T12's porcelain checks, and its own check-off necessarily happens after those checks have already run. Without this task the plan terminates with its own plan file modified and uncommitted, which contradicts the repository expectation that work is not complete until the tree is clean. Marking this task `[x]` before its own commit is deliberate and is what makes the commit capture the plan's terminal state; the ordering is stated here so it is not read as a check-off ahead of evidence.

---

## Planner Adversarial Self-Review

SELF-REVIEW: RE-DERIVED THIS PASS

Citations re-derived directly against the working tree in this pass:

1. `UtilitiesCS/Threading/UiThread.cs` — read in full (163 lines). Line 1 is the nullable-enable directive; lines 135-139 are the `Dispatcher` property; line 140 is `private static Dispatcher _dispatcher = null!;` with the trailing comment. Confirms the defect and the exact replacement region.
2. `UtilitiesCS/Threading/UiThread.cs` — lines 113-125 (`UiSyncContext`) and 147-158 (`AutoScaleFactor`) re-derived as the lazy-initialising siblings, confirming the omission on `Dispatcher` is an inconsistency.
3. `UtilitiesCS/Threading/UiThread.cs` line 61 — `Dispatcher = _syncContextForm.UiDispatcher;` is the sole writer through the private setter; re-derived to confirm the setter's non-nullable parameter type is unaffected by the field retyping.
4. UtilitiesCS/Threading/SyncContextForm.cs line 30 — `public Dispatcher UiDispatcher { get; private set; } = null!;` re-derived, confirming the value assigned at `UiThread.cs:61` is statically non-null and introduces no `CS8604` at that assignment.
5. UtilitiesCS/Threading/ProgressTrackerAsync.cs — read in full (109 lines). Line 33 is `UiDispatcher = UiThread.Dispatcher;` and line 35 is `await UiDispatcher.InvokeAsync(`. AC3's "no edit required" conclusion is re-derived from the tree, not carried from the research document, and is confirmed.
6. UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs lines 57-67 — the existing `InvalidOperationException` precedent re-derived verbatim; lines 45-46 confirm the default fallback provider is `() => UtilitiesCS.UiThread.Dispatcher`.
7. `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — read in full (104 lines). Namespace `UtilitiesCS.Test.Threading`; four using directives; one `[TestClass]` (`SynchronizationContextAwaiter_Tests`); no `System.Reflection` using. Establishes the 500-line headroom decision and the exact using-block edit.
8. UtilitiesCS.Test/UtilitiesCS.Test.csproj line 494 — `<Compile Include="Threading\UiThread_Tests.cs" />` re-derived (line number corrected in revision round 11 against the post-merge tree; it was 493 before the reconciliation merge), establishing that reusing the existing test file requires no project-file edit.
9. `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` lines 141-186 — `DispatcherField`, `ForceDispatcherNull`, and `RestoreDispatcher` re-derived, giving the exact reflection idiom (`typeof(UiThread).GetField("_dispatcher", BindingFlags.NonPublic | BindingFlags.Static)`) the new test mirrors. Lines 248-289 re-derived: the at-risk test asserts `NotThrow` and `callCount == 0` with no exception-type assertion, so the type change is invisible to it.
10. `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` lines 126-190 — re-derived: this test installs a real `Dispatcher.CurrentDispatcher` into `_dispatcher` before calling `InitializeAsync()` and restores in `finally`, so it exercises only the non-null path and is unaffected.
11. UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs lines 117-142 — re-derived: `YieldAsync_WithoutDispatcher_RemainsStrict` injects two null-returning provider delegates and asserts the exception TYPE only (`ThrowAsync<InvalidOperationException>()`, no `WithMessage`). The real `UiThread.Dispatcher` property is never read by any of this class's tests.
12. UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs line 55 — re-derived: `new WpfDispatcherYield()` uses the parameterless constructor, so its fallback provider is the real property. Sibling re-check outcome: the message text differs after the fix but the exception type does not, and this test asserts neither; P3-T3 verifies empirically rather than resting on this reading.
13. UtilitiesCS.Test/Properties/AssemblyInfo.cs line 18 — the assembly-level `Parallelize(` attribute re-derived, which is the justification for the do-not-parallelize attribute on the new class and on the three existing classes P1-T5 touches.
14. **Sibling finding not present in `spec.md` or the research trail:** QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs lines 26 and 64 construct `new WpfUiDispatcher()`, whose provider closes over `UiThread.Dispatcher` (UtilitiesCS/Threading/WpfUiDispatcher.cs lines 24-25 and 37, re-derived). Line 26's constructor only captures the lambda and never invokes it; line 64 runs inside a `UiThreadDispatcherFixture` transaction that installs a real dispatcher first. Neither is expected to change outcome, but the class is now baselined in P0-T11 and verified in P3-T6 rather than left unexamined.
15. .csharpierignore — re-derived: it excludes `**/evidence/**`, `*.cobertura.xml`, `*.trx`, `*.csproj`, `*.props`, `*.targets`. Evidence artifacts written by this plan therefore cannot fail the format gate.
16. .gitignore lines 39 and 144-145 — re-derived: `[Tt]est[Rr]esult*/` ignores the `TestResults/` subdirectories this plan writes, and `coverage/*` (except `coverage/.gitkeep`) ignores the Cobertura outputs. Neither enters the committed footprint asserted in P5-T10. See citation 30 for why both patterns require the forward-slash spelling to take effect.
17. `coverage.config` — re-derived: it excludes only third-party module paths (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest). No first-party production path is excluded, so `UtilitiesCS/Threading/UiThread.cs` is in the coverage denominator and P4-T7's class-node lookup can resolve.
18. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` lines 249-272 — the AC1..AC7 bullets re-derived verbatim, giving the exact bullet-opening text each P5 check-off task must match. Line range corrected in the revision round 9 pass; see "Citations re-derived in the revision pass of 2026-09-02 (revision round 9, backtick-removal presentation fix)" below for the re-derivation, which supersedes this entry's round-1 "lines 234-257" reading — that reading predated the `## Write Set` section inserted at spec.md lines 77-86.
19. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/issue.md` line 8 — the promotion-time merge base `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1`, re-read in revision round 11 and found unchanged in that file. It is NO LONGER the anchor for any `git diff` in this plan: the reconciliation merge described in the BASE re-anchor note moved the merge base to `87cb4df338322844abfa580abea14df77e738e5c`, which was re-derived in round 11 from this worktree's git log (a `merge origin/main` producing `a2ef517b`, so `origin/main` is an ancestor of HEAD) together with the recorded `origin/main` tip, and which is the value every `git diff` in this plan now carries. `issue.md` is outside this plan's write set and is deliberately left unedited.
20. CLAUDE.md "C# Toolchain" section and .claude/rules/general-unit-test.md / .claude/rules/quality-tiers.md coverage sections — re-derived, producing the recorded 80/90 versus 85/75 conflict and the rank-1 resolution stated in "Threshold reconciliation" above.

### Citations re-derived in the revision pass of 2026-09-02 (revision round 9, backtick-removal presentation fix)

Every citation below was re-derived against the working tree in this revision pass by reading the
named file. The tree was at the then-current merge base `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1`
with no commit made by this plan; that value was superseded in revision round 11 (see the BASE
re-anchor note above) and is recorded here as a historical observation, not as the current BASE.
This round changed no command line, no task ID, no write-target file, and no evidence path. It
changed only: the removal of backtick-wrapping around scope-exclusion, precedent, and
context-reference file-path mentions in `spec.md` and this plan (round 9 itself, applied before this
pass); one incomplete backtick-removal left behind by that round at spec.md line 168 (Defect 1,
corrected in this pass); the plan's own status and version metadata; and citation 18 above, which the
`## Write Set` section insertion had already made stale before round 9 and which round 9's edits did
not touch. The prose corrections are enumerated in "Sibling regions re-checked in the revision round
9 pass" below.

58. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` lines 77-86
    — the `## Write Set` section re-derived this pass. It lists exactly five paths, in this order:
    `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`,
    `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`,
    `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, and
    `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`. This plan's own "Scope: files this plan's
    diff writes" list was re-read alongside it and names the same five paths in
    the same order. Superseded by revision round 15: both lists now carry a sixth path,
    `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`, and both were re-read against each
    other again in that round (see the round-15 sibling-region section). This entry records the
    round-9 observation of a superseded state and is retained for the audit trail.
59. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` lines
    249-272 — the then-current position of the `## Acceptance Criteria` block re-derived directly from
    the file: the heading sat at line 249, the AC1-AC7 bullets ran through line 272, and line 274 was
    `## Risks & Mitigations`. This was the corrected range that superseded citation 18's round-1
    "lines 234-257" reading, which predated the `## Write Set` section this pass re-derived as
    citation 58. Superseded again by revision round 15, which amended AC4 and three other sections of
    that file; the current range is re-derived as citation 79 below.
60. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` line 259 —
    the AC3 bullet's opening text, "AC3: UtilitiesCS/Threading/ProgressTrackerAsync.cs is left
    unmodified", re-read this pass and compared character-for-character against P5-T3's quotation of
    the same opening text (above). The two match exactly; no correction to P5-T3 was required.

### Citations re-derived in the revision pass of 2026-09-02 (preflight round 2)

Every citation below was re-derived against the working tree in this revision pass. Prior-round
verification of the same files is not relied on, because the file set this plan writes changed in
this pass and a prior pass observed a superseded scope.

21. `git grep -n '"_dispatcher"' -- UtilitiesCS.Test` returns exactly three lines:
    `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:144`,
    `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:422`, and
    `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:138`. These are the complete set of
    files in the assembly that reflect over the process-global backing field, and every one of them
    writes it via `SetValue`. This is the derivation behind P0-T13, P1-T5, and the expanded scope.
22. `[DoNotParallelize]` occurs in 18 files under `UtilitiesCS.Test`, re-derived this pass. None of
    the three files in citation 21 is among them, so all three are in the parallel bucket at BASE.
23. UtilitiesCS.Test/Threading/CurrentStoreContextTests.cs lines 15-16 — `[TestClass]` then
    `[DoNotParallelize]` on the following line, re-derived as the repository's prevailing two-line
    idiom that P1-T5 follows for two of the three files.
24. `[TestClass` occurs exactly once in each of the four files P1-T5 and P1-T2 touch, at
    `UtilitiesCS.Test/Threading/UiThread_Tests.cs:8`,
    `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:28`,
    `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:13`, and
    `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:14`. Re-derived this pass, which is what
    makes P0-T13's third acceptance clause a real four-line expectation rather than a lower bound.
25. Line counts re-derived this pass: `UtilitiesCS/Threading/UiThread.cs` 163,
    `UtilitiesCS.Test/Threading/UiThread_Tests.cs` 104,
    `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` 347,
    `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` 205, and
    `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` 514. **Sibling finding surfaced by this
    pass and not present in the prior round:** the last of these already exceeds the 500-line limit
    in .claude/rules/general-code-change.md at BASE. A naive "all five files under 500" acceptance
    would have been unsatisfiable. P1-T5 therefore uses the combined attribute list for that one
    file, and P2-T3 and P4-T8 record the overage as pre-existing and gate on non-growth instead.
26. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` line 12 declares `namespace
    UtilitiesCS.Test`, not `UtilitiesCS.Test.Threading`. Re-derived this pass; this is why P3-T3's
    added filter term is `FullyQualifiedName~UtilitiesCS.Test.ProgressTracker_Tests` and not a
    `.Threading.`-qualified spelling that would have matched nothing.
27. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` lines 411-432 —
    `Initialize_WithCurrentDispatcherAndScreen_InitializesViewerAndUpdatesUi` is the
    `[STATestMethod]` that writes `UiThread._dispatcher` at line 432 and restores it in a `finally`.
    Re-derived this pass as the named test P3-T3 requires in its executed set.
28. `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` lines 150, 152, and 162 — the
    `SetValue` write, the `InitializeAsync()` call that reads `UiThread.Dispatcher` downstream, and
    the `Dispatcher.PushFrame(frame)` pump. Re-derived this pass; this is the concrete mechanism by
    which a concurrent writer could either observe the post-fix `InvalidOperationException` or leave
    the pumped frame without an exit, and it is what P1-T5 removes from the parallel bucket.
29. UtilitiesCS.Test/UtilitiesCS.Test.csproj lines 477, 479, 490, and 494 — `Compile Include`
    entries for `Threading\ProgressTracker_Tests.cs`, `Threading\ProgressTrackerAsync_Tests.cs`,
    `Threading\IdleAsyncQueue_Tests.cs`, and `Threading\UiThread_Tests.cs`. Line numbers corrected in
    revision round 11 against the post-merge tree, where each entry sits one line lower than the
    round-2 reading of 476, 478, 489, and 493; all four entries are still present, so this citation's
    conclusion is unchanged. All four files are already wired and already tracked, so the expanded scope requires no
    `.csproj` edit and the single-ref `git diff` form used in P3-T5 and P4-T7 is not blind to any
    file this plan writes.
30. .gitignore line 39 `[Tt]est[Rr]esult*/` and line 144 `coverage/*` — re-derived this pass.
    Both are directory-scoped patterns. A file written to the worktree root as
    `TestResultsp3-t5-source.diff` or `coveragep0-t10.cobertura.xml`, which is what a POSIX shell
    produces from an unquoted backslash-spelled path, matches neither. This is the derivation behind
    the forward-slash rewrite of every path in this plan's command blocks.
31. `coverage.config` lines 12-22 — the `ModulePaths/Exclude` list names Deedle, FSharp,
    Castle.Core, FluentAssertions, Moq, Microsoft.Testing, and MSTest only. Re-derived this pass. No
    first-party assembly is excluded, so both `UtilitiesCS` and `UtilitiesCS.Test` contribute to
    `lines-valid`. That is why P4-T7's denominator clause is a bounded band rather than an equality:
    the production and test lines this plan adds legitimately move the denominator.
32. `UtilitiesCS/Threading/UiThread.cs` lines 135-140 re-read in this pass, unchanged from the prior
    round: `get => _dispatcher;` at line 137 and
    `private static Dispatcher _dispatcher = null!; // set in Initialize() before any access` at
    line 140. The replacement text quoted in this plan still matches the tree it will replace.

### Citations re-derived in the revision pass of 2026-09-02 (preflight round 3)

Every citation below was re-derived against the working tree in this revision pass. The tree is
clean at the then-current merge base `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` with no commit made
by this plan, so these observations describe the state the plan's first task will actually run in.
That merge-base value was superseded in revision round 11 (see the BASE re-anchor note above) and is
recorded here as a historical observation; the reconciliation merge changed none of the files cited
below, so the observations themselves still hold.

33. Physical line counts re-derived this pass by counting every line of each file:
    `UtilitiesCS/Threading/UiThread.cs` 163, `UtilitiesCS.Test/Threading/UiThread_Tests.cs` 104,
    `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` 347,
    `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` 205, and
    `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` 514. These are the five values P0-T13,
    P2-T3, and P4-T8 assert, and they are the values `wc -l` reports.
34. Blank-line counts re-derived this pass: `UtilitiesCS.Test/Threading/UiThread_Tests.cs` has 17
    blank lines out of 104 and `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` has 92 out of
    514. `Get-Content | Measure-Object -Line` does not count blank lines, so it would have reported
    87 and 422 for those two files against a plan asserting 104 and 514. That is the derivation
    behind the `wc -l` idiom now fixed in P0-T13, P2-T3, and P4-T8, and behind P0-T13's explicit
    prohibition on substituting `Measure-Object -Line`.
35. global.json lines 6-10 — `"paths": [".dotnet-sdk", "$host$"]` with the error message `The
    repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the
    repository root...`. Re-derived this pass. `.dotnet-sdk/` does NOT exist in this worktree, also
    re-derived this pass, so every `dotnet` command fails until the bootstrap in P0-T5 runs.
36. `scripts/vscode/Install-RepoDotNetSdk.ps1` — re-derived this pass. Line 3 defaults `$Version` to
    `8.0.205`, which is why P0-T5's acceptance reads "a version beginning `8.0.2`". This script is
    NOT invoked by the plan; the citation records the two values P0-T5's POSIX bootstrap reproduces.
37. `C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe` — confirmed present at
    that path this pass. This is what makes P0-T5 step 2 a real command rather than a documented
    shape.
38. .github/workflows/ci.yml line 21-23 delegates format checking to
    .github/workflows/_format-check.yml, whose line 41 runs `dotnet csharpier check .` after
    `dotnet tool restore` on line 37. Re-derived this pass. This is the CI-parity command P4-T2
    runs repo-wide, and it is why restricting P4-T1's formatter WRITE scope to the five owned paths
    does not narrow the repository-wide format gate.
39. `git grep -n '"_dispatcher"' -- UtilitiesCS.Test` re-derived again in this pass and unchanged:
    `IdleAsyncQueue_Tests.cs:144`, `ProgressTracker_Tests.cs:422`,
    `ProgressTrackerAsync_Tests.cs:138`. P0-T13's first acceptance clause and P1-T5's edit sites are
    still correct after this round's edits.
40. `UtilitiesCS/Threading/UiThread.cs` lines 135-140 re-read again in this pass and unchanged:
    `get => _dispatcher;` at line 137 and
    `private static Dispatcher _dispatcher = null!; // set in Initialize() before any access` at
    line 140. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` line 14 is still the sole
    `[TestClass]` in that file.

### Citations re-derived in the revision pass of 2026-09-02 (revision round 5, applying preflight findings C1-C3 and N1-N2)

Every citation below was re-derived against the working tree in this revision pass by reading the
named file. The tree was at the then-current merge base `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1`
with no commit made by this plan, so these observations describe the state the plan's first task will
actually run in. That merge-base value was superseded in revision round 11 (see the BASE re-anchor
note above) and is recorded here as a historical observation.

41. .gitignore line 350 is `.dotnet*/`. Re-derived this pass by searching the file for `dotnet`,
    which returns exactly two lines: line 143, a comment about `dotnet-coverage` Cobertura output,
    and line 350, the pattern itself. `.dotnet-sdk/` is therefore already ignored, so P0-T5 step 1's
    bootstrap cannot appear in any porcelain or diff gate later in this plan.
42. `scripts/vscode/Install-RepoDotNetSdk.ps1` line 26 builds the download URL
    `https://builds.dotnet.microsoft.com/dotnet/Sdk/$Version/dotnet-sdk-$Version-win-$Architecture.zip`,
    line 3 defaults `$Version` to `8.0.205`, and lines 5-6 default `$Architecture` to `x64`.
    Substituting those defaults gives
    `https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip`, which is
    character-for-character the URL P0-T5 step 1 passes to `curl`. Re-derived this pass by reading the
    file in full.
43. `scripts/vscode/Install-RepoDotNetSdk.ps1` line 36 resolves the default install directory as
    `Join-Path $PSScriptRoot '..\..\.dotnet-sdk'`, which from `scripts/vscode/` is `.dotnet-sdk` at
    the worktree root — the same destination P0-T5 step 1's `unzip -d .dotnet-sdk` writes to.
    Line 56 shows the script's own success marker is `.dotnet-sdk/sdk/8.0.205`, which the extracted
    archive creates. Re-derived this pass.
44. global.json lines 2-11 re-derived this pass: `"version": "8.0.205"`, `"rollForward":
    "latestFeature"`, `"paths": [".dotnet-sdk", "$host$"]`, and the `errorMessage` beginning `The
    repo-local .NET SDK is missing`. This is why P0-T5's acceptance clause — post-bootstrap
    `dotnet --version` beginning `8.0.2` and exiting 0 — is unchanged by replacing the PowerShell
    bootstrap with the POSIX one: both install the same pinned version into the same directory.

**Environment measurements applied in this pass, recorded as such and not as tree citations.** The
first three shell behaviours in "Shell constraints measured in this worktree" — the refusal of any
command named `pwsh`, the refusal of any command whose NAME is a quoted absolute path, and the
failure of bare `msbuild` against the success of `msbuild.exe` — were measured by running the
commands in this
worktree during preflight round 3. The fourth (MSYS path conversion of forward-slash switches) and
the missing-NuGet-restore failure recorded in P0-T5 step 4 were measured the same way during
preflight round 4. The fifth (a green `vstest.console.exe` run printing no `Failed:` and no
`Skipped:` line, and the TRX `<Counters .../>` element carrying both counts) was measured the same
way during preflight round 5, across four green runs of 4783, 1312, 41, and 2 tests. The planner has
no shell in its tool surface and therefore did not
re-run them in this pass; they are carried as reported measurements with their verbatim outputs, and
they are labelled as measurements rather than as citations to the repository tree. The same applies
to the `vswhere.exe` PATH-prefix invocation in P0-T5 step 2 and the path it printed. Two consequences
of those measurements that this pass derived rather than measured are stated explicitly in the plan
so a reviewer can check them: that a POSIX shell splits `PATH` on `:` and so requires the
`/c/...` spelling for the `PATH=` prefix, and that `unzip` availability in this shell is unmeasured,
which is why P0-T5 step 1 carries a fail-closed `SDK_BOOTSTRAP: BLOCKED` clause covering a failure of
any of its four commands.

### Citations re-derived in the revision pass of 2026-09-02 (revision round 6, applying preflight round 4 findings E1-E3 and N-1/N-2/N-3)

Every citation below was re-derived against the working tree in this revision pass by reading or
searching the named file. The tree was at the then-current merge base
`5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` with no commit made by this plan; that value was superseded
in revision round 11 (see the BASE re-anchor note above) and is recorded here as a historical
observation. The worktree now additionally contains gitignored `.dotnet-sdk/`, `packages/`, and
`Debug` build output left in place by the round-4 reviewer; none of them is a tracked file and none of
them changes any citation below.

45. .github/workflows/_build-analyzers.yml — line 17 sets `SOLUTION_PATH: TaskMaster.sln`, line 45
    runs `nuget restore $env:SOLUTION_PATH` as the step named "Restore solution", and line 50 runs the
    analyzer `msbuild` immediately after it. Read in full this pass. .github/workflows/_build-nullable.yml
    line 45 and .github/workflows/_mstest-coverage.yml line 45 carry the identical restore step.
    This is the CI-parity citation for the new P0-T5 step 4: every CI gate that builds this solution
    restores NuGet packages first.
46. `packages.config` exists in 18 project directories across this solution, enumerated this pass:
    `QuickFiler`, `QuickFiler.Test`, `SVGControl`, `SVGControl.Test`, `Tags`, `Tags.Test`,
    `TaskMaster`, `TaskMaster.Test`, `TaskTree`, `TaskTree.Test`, `TaskVisualization`,
    `TaskVisualization.Test`, `ToDoModel`, `ToDoModel.Test`, `UtilitiesCS`, `UtilitiesCS.Test`,
    `VBFunctions`, and `VBFunctions.Test`. Every one of them is a restore target of P0-T5 step 4, and
    the two that matter directly to this plan — `UtilitiesCS/packages.config` and
    `UtilitiesCS.Test/packages.config` — are among them.
47. .gitignore line 191 is `**/[Pp]ackages/*` and line 193 is `!**/[Pp]ackages/build/`. Re-derived
    this pass by reading lines 185-216. The restore output under `packages/` is therefore ignored, with
    the single exception of a `packages/build/` directory, which does not exist in this worktree after
    a completed restore (also re-derived this pass). P0-T5 step 4 states what to do if a fresh-worktree
    restore produces one.
48. dotnet-tools.json exists at the worktree root and no `.config/` directory exists. Re-derived this
    pass. This is why P0-T5 step 4's rationale names the root manifest rather than the conventional
    `.config/` location.
49. `UtilitiesCS/Threading/UiThread.cs` lines 135-140 re-read again in this pass and still unchanged:
    the property opens at line 135, `get => _dispatcher;` is line 137, `private set => _dispatcher = value;`
    is line 138, and line 140 is
    `private static Dispatcher _dispatcher = null!; // set in Initialize() before any access`.
    The replacement text quoted in "Exact source text this plan will create" still matches the region it
    replaces, and P0-T2's five BLOCKED-clause values are still the values the tree reports.
50. `.dotnet-sdk/dotnet.exe` is present in this worktree. Re-derived this pass. .gitignore line 350
    `.dotnet*/` ignores it (citation 41). Its presence is why P0-T5's `SDK_BOOTSTRAP:` field now accepts
    a `NOT REQUIRED` value: in this worktree the first `dotnet --version` probe succeeds and the
    four-command bootstrap never runs, so there is no post-bootstrap reading to record. In a fresh
    worktree the bootstrap does run and the first form of the field applies.

### Citations re-derived in the revision pass of 2026-09-02 (revision round 7, applying preflight round 5 finding F1)

Every citation below was re-derived against the working tree in this revision pass by reading or
searching the named file. The tree was at the then-current merge base
`5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` with no commit made by this plan; that value was superseded
in revision round 11 (see the BASE re-anchor note above) and is recorded here as a historical
observation. This round changed no command line, no task ID, no write target, and no evidence path; it
changed only how seven tasks source two numeric fields, plus one new constraint entry and its
redaction rule. (The `Skipped` half of that sourcing rule was itself wrong and was corrected in
revision round 8; see the round-8 section below.)

51. QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs — searched this pass for `[TestMethod]` and
    for public method declarations. The file declares exactly two `[TestMethod]` attributes, at lines
    23 and 48, whose methods are `public void Construction_YieldsAnIUiDispatcher()` on line 24 and
    `public async Task Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread()` on line 50.
    This is the derivation behind P3-T6's `Total tests` of 2 and behind the two names its acceptance
    requires to appear as passing in the console output; both names are re-derived here rather than
    carried forward from the round-1 citation, which recorded the two `new WpfUiDispatcher()`
    construction sites (lines 26 and 64) and not the method declarations.
52. .gitignore lines 33-50 read this pass. Line 39 is `[Tt]est[Rr]esult*/` and is directory-scoped,
    so `TestResults/p0-t10/`, `TestResults/p0-t11/`, `TestResults/p3-t2/`, `TestResults/p3-t3/`,
    `TestResults/p3-t6/`, `TestResults/p4-t5/`, and `TestResults/p4-t6/` — and every `.trx` file
    inside them — are ignored. Line 44 is the unrelated NUnit pattern `TestResult.xml`. Reading the
    TRX files therefore adds nothing to P5-T10's name-status diff or to any porcelain gate in this
    plan.
53. .csharpierignore read in full this pass (15 lines). Line 8 is `*.trx`, alongside
    `**/evidence/**` on line 4, `*.cobertura.xml` on line 5, `*.coverage` on line 6, `*.coveragexml`
    on line 7, and the project-file exclusions on lines 12-14. The TRX files this round's rule reads
    are outside the format gate, so P4-T2's repo-wide `csharpier check .` cannot report them.

### Citations re-derived in the revision pass of 2026-09-02 (revision round 8, applying preflight round 6 finding G1 and non-blocking findings O1-O4)

Every citation below was re-derived against the working tree in this revision pass by reading the
named file. The tree was at the then-current merge base `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1`
with no commit made by this plan; that value was superseded in revision round 11 (see the BASE
re-anchor note above) and is recorded here as a historical observation.
This round changed no command line, no task ID, no write-target file, and no evidence path. It
changed only the stated SOURCE of the `Skipped` field, the placement of the TRX-collision tie-break
rule, one redaction clause, the plan's own status and version metadata, and the prose corrections
enumerated in "Sibling regions re-checked in the revision round 8 pass" below.

54. `UtilitiesCS/Threading/UiThread.cs` lines 130-145 re-read this pass and still unchanged: the
    `Dispatcher` property opens at line 135, `get => _dispatcher;` is line 137,
    `private set => _dispatcher = value;` is line 138, and line 140 is
    `private static Dispatcher _dispatcher = null!; // set in Initialize() before any access`. The
    replacement text in "Exact source text this plan will create" still matches the region it
    replaces, and P0-T2's five BLOCKED-clause values are still the values the tree reports.
55. .csharpierignore read in full again this pass (15 lines). Line 8 is `*.trx`, line 4 is
    `**/evidence/**`, line 5 is `*.cobertura.xml`. Every TRX this round's rewritten sourcing rule
    reads remains outside P4-T2's repo-wide `csharpier check .`, including a second `.trx` left in a
    results directory by a re-run, because the pattern is extension-scoped and not name-scoped.
56. .gitignore lines 33-48 re-read this pass. Line 39 is `[Tt]est[Rr]esult*/` and is
    directory-scoped, so every file inside `TestResults/<task>/` is ignored however many `.trx` files
    a re-run leaves there. Line 44 is the unrelated NUnit pattern `TestResult.xml`. The TRX selection
    rule added this round therefore adds nothing to P5-T10's name-status diff or to any porcelain
    gate in this plan.
57. QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs re-searched this pass for `[TestMethod]`
    and for public method declarations: exactly two `[TestMethod]` attributes, at lines 23 and 48,
    on `public void Construction_YieldsAnIUiDispatcher()` at line 24 and
    `public async Task Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread()` at line
    50. P3-T6's `Total tests` of 2 and its two by-name clauses are unaffected by this round's edit to
    that task, which changed only its TRX-sourcing and redaction prose.

**Environment measurements applied in this pass, recorded as such and not as tree citations.** The
`<Counters .../>` behaviour stated in constraint 5 — that `vstest.console.exe`'s TRX logger populates
only `total`, `executed`, `passed`, and `failed` and hard-codes every other attribute to `0`, so that
a run whose console printed `Skipped: 1` produced a TRX carrying `notExecuted="0"` while
`total` minus `executed` returned the correct `1` — was measured by the preflight round-6 reviewer,
who built a three-test probe assembly (one passing, one failing, one skipped) and ran it through this
plan's exact command shape. The related statement that each of the `Failed:` and `Skipped:` console
lines is printed only when its own counter is non-zero comes from the same probe, which printed both.
The statement that a default TRX filename embeds a timestamp and does not overwrite an existing file
is reported by the same reviewer. The planner has no shell in its tool surface and did not re-run any
of them in this pass; they are carried as reported measurements and are labelled as such rather than
as citations to the repository tree.

### Citations re-derived in the revision pass of 2026-09-03 (revision round 11, BASE re-anchor after the orchestrator's origin/main reconciliation merge)

Every citation below was re-derived in this pass against the tree as it stands AFTER the
reconciliation merge, by reading the named file directly. No value in this section is carried forward
from an earlier round: every earlier round observed the pre-merge tree, which is a superseded state.
This round changed no task, no acceptance clause other than the BASE SHA it names, no evidence path,
and no `.csproj` file. It re-anchored the four executable `git diff` commands in the plan (P3-T4,
P3-T5, P4-T7, P5-T10) and the two prose statements of the same command shape (P1-T1, P4-T7), corrected
four `.csproj` line-number citations, and qualified the historical merge-base sentences above.

61. `.git` worktree log for `agent-a18cc3bc53f9c1d8a`, final entry — `merge origin/main: Merge made
    by the 'ort' strategy`, moving HEAD from `98c4ef16` to `a2ef517b`. Re-derived this pass. Because
    `origin/main` was merged into HEAD and no commit has been made on `origin/main` since, `origin/main`
    is an ancestor of HEAD and the merge base with `origin/main` is `origin/main` itself.
62. Recorded `origin/main` tip — `87cb4df338322844abfa580abea14df77e738e5c`. Re-derived this pass.
    Combined with citation 61, this is the derivation of the re-anchored BASE now stated in the header
    and carried by every `git diff` in this plan.
63. UtilitiesCS.Test/UtilitiesCS.Test.csproj lines 477, 479, 490, and 494 — re-derived this pass by
    reading lines 468-499 of the post-merge file. The four `Compile Include` entries are, in file
    order, `Threading\ProgressTracker_Tests.cs` (477), `Threading\ProgressTrackerAsync_Tests.cs`
    (479), `Threading\IdleAsyncQueue_Tests.cs` (490), and `Threading\UiThread_Tests.cs` (494). Each
    sits exactly one line lower than the pre-merge reading. All four are present, so the "no `.csproj`
    edit is required" conclusion in the Scope section, in P3-T5, and in citations 8 and 29 is
    unchanged; only the four line numbers moved.
64. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` — the file still ends at line 514 (line 512
    is `#endregion`, 513 closes the class, 514 closes the namespace). Re-derived this pass, which
    confirms the `PRE-EXISTING FILE-SIZE OVERAGE: ... 514` value P0-T13 requires is still the value
    the post-merge tree reports and that the overage is correctly attributed to the re-anchored BASE.
65. `UtilitiesCS/Threading/UiThread.cs` lines 133-141 re-read this pass against the post-merge tree
    and unchanged: the property opens at line 135, `get => _dispatcher;` is line 137,
    `private set => _dispatcher = value;` is line 138, and line 140 is
    `private static Dispatcher _dispatcher = null!; // set in Initialize() before any access`. The
    replacement text quoted in "Exact source text this plan will create" still matches the region it
    replaces, and P0-T2's BLOCKED-clause values are still the values the post-merge tree reports.
66. `UtilitiesCS.Test/Threading/UiThread_Tests.cs` still ends at line 104. Re-read this pass against
    the post-merge tree, confirming the 104-line baseline P0-T13 asserts and the 500-line headroom
    argument in "Test-file placement decision" are both still correct after the merge.

**Sibling regions re-checked in this pass.** Three sentences adjacent to the re-anchored spans rested
on a premise the merge invalidated — that HEAD is identical to BASE before P5-T9 commits. That is now
true only across the paths this plan reads. The narrower claim rests on two things: citations 63
through 66 above, which re-read the post-merge tree directly and found every owned-file citation
either unchanged or shifted only in the `.csproj` line numbers already corrected; and a measured
report from `atomic-executor`, which ran `git diff --name-status 87cb4df338322844abfa580abea14df77e738e5c..HEAD`
in this worktree and observed exactly the four feature-folder documents and zero source paths. The
second of those is a reported measurement, not a planner citation to the tree: the planner has no
shell in its tool surface and could not re-run that command in this pass. It is labelled as such here
for the same reason the environment measurements elsewhere in this section are. The three sentences
(in P3-T4, P3-T5, and P4-T7) are now scoped to the pathspec each one actually governs, so each states
a claim that holds against the post-merge tree. The conclusion each supports — that the two-dot form
would be vacuous and the single-ref or `--cached` form is required — is unchanged. P5-T10's own note
that the two-dot form is correct there, because P5-T9 has already committed, was re-read and needed no
change. P5-T10's acceptance already carves out paths under the feature folder, which is what the four
already-committed feature-folder documents fall under, so the re-anchored diff satisfies it.

### Citations re-derived in the revision pass of 2026-09-03 (revision round 15, scope widened to a sixth owned file after P4-T6 found a reflective consumer)

Every citation below was re-derived in this pass against the tree as it stands AFTER the executor
completed P0-T1 through P4-T5, by reading or searching the named file directly. No value in this
section is carried forward from an earlier round, and no value in it is carried forward from the
executor's own report: the executor's account of the failure was directional, and every line number,
count, and file set it named was re-derived here independently. That distinction matters in this round
specifically, because the round exists to repair a gap created by an assertion nobody re-derived.

67. `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` — read in full this pass. 314 lines.
    Namespace `QuickFiler.Helper_Classes.Tests`. Lines 21-22 carry `[TestClass]` then
    `[DoNotParallelize]`, so no attribute change is needed for this file and P1-T5's idiom is already
    present. Lines 25-31 are the class's explanatory comment. Line 32 is
    `private object _capturedDispatcher;`. Lines 33-37 are the
    `private static readonly System.Reflection.PropertyInfo DispatcherProperty` declaration and its
    `typeof(UiThread).GetProperty("Dispatcher", Public | Static)` initialiser. Line 44 is
    `[TestInitialize]`, line 49 is `_capturedDispatcher = DispatcherProperty?.GetValue(null);`, line
    53 is `[TestCleanup]`, line 58 is `object current = DispatcherProperty?.GetValue(null);`, and
    line 59 is `current.Should().BeSameAs(_capturedDispatcher);`. The file declares exactly eight
    `[TestMethod]` attributes, at lines 87, 107, 134, 147, 176, 200, 234, and 266. It declares no
    `using System.Reflection;` — its using block is lines 1-11 — which is why every reflection type it
    names is fully qualified and why P2-T4 adds no directive.
68. The same file re-searched this pass for each of the seven tokens AC5's filter matches. None of
    `Thread.Sleep`, `Task.Delay`, `SpinWait`, `Retry`, `retries`, `Timeout(`, or `PushFrame` occurs
    anywhere in it. The token `Thread` DOES occur, in `Thread.CurrentThread.ManagedThreadId` at line
    273 and in the surrounding body of
    `UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread`, but `Thread.Sleep`
    does not. This is the derivation that makes P2-T4's whole-diff token filter — stricter than
    P3-T5's added-lines-only filter — satisfiable rather than unsatisfiable.
69. `git grep`-equivalent search of every tracked `.cs` file in the worktree for the literal
    `"Dispatcher"` returns exactly five lines in five files, re-derived this pass:
    `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` line 35 (the `GetProperty` name
    operand), `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` line 14,
    `UtilitiesCS/Threading/WpfUiDispatcher.cs` line 14, `UtilitiesCS/Threading/ThreadMonitor.cs` line
    25, and `UtilitiesCS/Threading/IUiDispatcher.cs` line 13. The last four are
    `<see cref="Dispatcher"/>` XML documentation cross-references and none is a reflection site. This
    is the derivation behind P0-T14's clauses 1 and 3 and behind the claim that exactly one reflective
    property read exists in the repository.
70. Search of every tracked `.cs` file for `typeof(UiThread)` returns exactly seven lines in seven
    files, re-derived this pass: `UtilitiesCS.Test/Threading/UiThread_Tests.cs` line 127,
    `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` line 421,
    `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` line 138,
    `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` line 144,
    `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` line 34,
    `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` line 135, and
    `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` line 469. This is the derivation
    behind P0-T14's clause 2.
71. **Sibling finding surfaced by citation 70 and not present in any prior round:**
    `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` lines 461-487 were read in full
    this pass. `EnterUiContextAsync_WhenUiSyncContextPostsSynchronously_CompletesUsingDefaultAction`
    reflects over `UiThread` but takes the field `"_uiSyncContext"` at line 471, not `_dispatcher`,
    and restores it in a `finally` at line 485. It is therefore unaffected by P2-T1 and needs no
    change. It is recorded here and in P0-T14's clause 2 so that a later reviewer can see it was
    examined and excluded rather than missed — which is exactly the treatment
    `EmailMoveMonitorTests.cs` did not receive in earlier rounds.
72. `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` line 135 —
    `FieldInfo field = typeof(UiThread).GetField(` re-derived this pass. This fixture already uses the
    FIELD route, so the tests that install a real dispatcher through it are unaffected by P2-T1's
    guard, and P2-T4 moves `EmailMoveMonitorTests.cs` onto the same route its own assembly already
    uses elsewhere.
73. `QuickFiler.Test/QuickFiler.Test.csproj` line 206 —
    `<Compile Include="Helper Classes\EmailMoveMonitorTests.cs" />` re-derived this pass. The sixth
    owned file is already wired and already tracked, so P2-T4 requires no project-file edit and the
    single-ref `git diff` form P2-T4 uses is not blind to it.
74. `UtilitiesCS/Threading/UiThread.cs` re-read this pass against the POST-P2-T1 tree, which is the
    tree P2-T4 will run against and which no earlier round observed. The property now spans lines
    135-148: `get` opens at 137, the `if (_dispatcher is null)` guard is at 139, the
    `throw new InvalidOperationException(` is at 141, the message string is at 142,
    `return _dispatcher;` is at 145, `private set => _dispatcher = value;` is at 147, and the backing
    field is `private static Dispatcher? _dispatcher;` at line 149. Both AC2 evidence values — zero
    `null!` and the nullable field declaration — hold against the current tree. This is the citation
    that establishes the mechanism P2-T4 works around: the getter throws before returning, so a
    `PropertyInfo.GetValue(null)` over it propagates rather than returning null.
75. `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` lines 130-189 re-read this pass. The
    `DispatcherField()` helper at lines 142-148 is
    `typeof(UiThread).GetField("_dispatcher", BindingFlags.NonPublic | BindingFlags.Static)`, and
    `ForceDispatcherNull()` and `RestoreDispatcher(object)` at lines 165-187 are the capture/restore
    pair `spec.md`'s Test Strategy names as the repository idiom. P2-T4 mirrors this exact lookup,
    differing only in spelling the flags fully qualified because its own file declares no
    `using System.Reflection;` (citation 67).
76. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t11-quickfiler-tests.md`
    read in full this pass: `Total tests: 1312`, `Passed: 1312`, `Failed: 0`, `Skipped: 0`, TRX
    `total` 1312 and `executed` 1312, and `BASELINE_FAILURE_SET: empty`. This is the concrete figure
    P4-T6's restated acceptance now names, re-derived from the artifact rather than taken from the
    delegation prompt that reported it.
77. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t6-quickfiler-tests.md`
    read in full this pass: `EXIT_CODE: 1`, `Total tests: 1312`, `Passed: 1304`, `Failed: 8`, and the
    eight failing test names, which match one-for-one the eight `[TestMethod]` declarations citation
    67 enumerates. The artifact also records the executed counterfactual — BASE `UiThread.cs` restored
    to the working tree gives 8 passed, the fixed file gives 8 failed, same filter and same worktree —
    and states that the probe left no residue. The eight names in P4-T6's new clause 5 are quoted from
    this artifact and cross-checked against citation 67's `[TestMethod]` count.
79. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` re-derived
    this pass AFTER this round's amendments to it: `## Scope & Non-Goals` at line 56, `## Write Set`
    at line 82 with its six path bullets at lines 86-91, `## Root Cause Analysis` at line 93,
    `## Acceptance Criteria` at line 255 with the AC1-AC7 bullets running from line 257 to line 319,
    and `## Risks & Mitigations` at line 321. The AC bullet openings each P5 check-off task quotes
    were re-read against this state: AC1 at 257, AC2 at 267, AC3 at 275, AC4 at 284, AC5 at 312, AC6
    at 316, AC7 at 318. P5-T4's quoted fragment "AC4: No regression in" still matches AC4's opening
    text character-for-character after this round's rewrite of that bullet, which was checked
    explicitly rather than assumed, because this round is the one that rewrote it. AC4 is the only
    bullet whose check state this round changed, from `- [x]` to `- [ ]`; AC1, AC2, AC3, and AC5 keep
    the `- [x]` the executor left, and AC6 and AC7 remain `- [ ]`.
80. The nine test assemblies of this repository re-derived this pass by enumerating the
    `*.Test.csproj` files one directory below the worktree root: `QuickFiler.Test`, `SVGControl.Test`,
    `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`,
    `UtilitiesCS.Test`, and `VBFunctions.Test`. This was the pathspec list P0-T14's first two commands
    carried when the round 15 pass observed them. Revision round 16 widened the second command's
    pathspec to `'*.cs'`, so from that round onward the list above is carried by the first command
    only; the citation is retained in its round-15 form above and corrected here rather than rewritten.

**Sibling regions re-checked in the revision round 15 pass.**

- **Every pathspec-scoped gate in the plan was enumerated before any edit, and each was decided
  individually rather than swept.** The complete set is P0-T13 (`-- UtilitiesCS.Test`), P1-T5
  (`-- UtilitiesCS.Test` and a four-file list), P2-T2 (two spans on `UiThread.cs`), P2-T3 (a five-file
  `wc -l`), P3-T4 (`git add -A -- UtilitiesCS UtilitiesCS.Test` plus three spans on
  `ProgressTrackerAsync.cs`), P3-T5 (`git diff ... -- UtilitiesCS UtilitiesCS.Test`), P4-T1 (a
  five-path formatter write scope and two unscoped porcelain spans), P4-T2 (a five-path exclusion
  list), P4-T7 (`-- UtilitiesCS/Threading/UiThread.cs`), P4-T8 (a five-file `wc -l`), P5-T9 (a
  five-path `git add` and `git commit`), P5-T10 (a five-path name-status expectation and a scoped
  porcelain span), and P5-T11 through P5-T13 (three scoped porcelain spans). Of those, the ones whose
  correctness depends on the owned-file set are P4-T1, P4-T2, P4-T8, P5-T9, P5-T10, P5-T11, P5-T12,
  and P5-T13, and all eight were widened. The ones that are correct BECAUSE they are narrow —
  P0-T13's and P1-T5's `UtilitiesCS.Test` censuses, P2-T2's and P4-T7's single-file spans, P3-T4's
  single-file spans — were deliberately NOT widened, and the reason is recorded in the header note: a
  new `"_dispatcher"` occurrence in `QuickFiler.Test` does not enter P1-T5's count because that
  command's pathspec is `UtilitiesCS.Test`, so P1-T5's already-recorded four-line result stays true.
- **P3-T5 is the one narrow gate whose narrowness became a real gap, and it is closed by a task
  rather than by a re-run.** AC5's wording is "anywhere in the diff", and after the widening the diff
  includes a file P3-T5's pathspec does not reach. P3-T5 is complete and its artifact describes a
  real result over the five files it covers, so it is not reopened; instead P2-T4 carries the
  identical seven-token filter over the sixth file and both artifacts are cited by P5-T5 and by AC5's
  row in the mapping table. The alternative — widening P3-T5's pathspec and re-running it — was
  rejected because it would have required unchecking a Phase 3 task whose recorded result is still
  accurate for its own scope.
- **The `Total tests` comparison in P4-T5 was re-checked and needs no change.** Its clause reads
  "`Total tests` is greater than or equal to the baseline `Total tests` plus 2", the two being the
  tests P1-T2 added to `UtilitiesCS.Test`. P2-T4 adds no test to any assembly and touches no file in
  `UtilitiesCS.Test`, so that clause is unaffected by this round. P4-T7's coverage clauses read
  `UtilitiesCS/Threading/UiThread.cs` line hits and root-element attributes from a
  `UtilitiesCS.Test`-hosted run; a change confined to `QuickFiler.Test` moves neither, so P4-T7 is
  unchanged apart from running in the second pass.
- **P3-T6 was re-checked and needs no change.** It runs
  `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`, a different class in the same assembly as the
  sixth owned file. P2-T4 edits neither that class nor anything it references, and P3-T6's recorded
  `Failed: 0` therefore still describes the tree. Its two by-name clauses were re-read and are
  untouched.
- **The eight failing test names were cross-checked against the file rather than copied.** P4-T6's new
  clause 5 lists eight names taken from the first-pass artifact (citation 77); each was matched
  against the eight `[TestMethod]` declarations read directly from the file (citation 67). The two
  sets agree exactly, which is what establishes that "8 of 8" means every test in the class and not a
  subset that happens to number eight.
- **The first-pass P4-T6 artifact would have been destroyed by the re-run, and that was caught in
  this pass rather than after the fact.** P4-T6's artifact path is fixed, so a second pass overwrites
  it — and that artifact is the only fail-before record for P2-T4. P4-T6's text now requires the
  first-pass file to be copied to `evidence/regression-testing/p4-t6-first-pass-failure.md` before the
  re-run, the copy is added to AC4's evidence set in both the mapping table and the `AC-MAPPING:`
  block, and P5-T4 cites it. Without that copy, AC4's check-off would rest on a pass-after with no
  surviving fail-before.
- **AC6's "single final pass" wording was read against the Phase 4 check state, which is what
  produced the decision to clear P4-T1 through P4-T5.** Leaving them checked would have let AC6 be
  marked on four artifacts that describe a pre-P2-T4 tree. The alternative of adding a second set of
  Phase 4 task IDs was rejected because task IDs must be sequential in file order, so a re-run task
  could not be placed where it must execute.
- **The write-target set was re-read after every edit in this round.** It is now six paths:
  `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`,
  `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`,
  `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`,
  `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, and
  `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`.
  UtilitiesCS/Threading/ProgressTrackerAsync.cs remains outside it, as do
  UtilitiesCS.Test/UtilitiesCS.Test.csproj and QuickFiler.Test/QuickFiler.Test.csproj. The
  acceptance-criteria mapping table and the `AC-MAPPING:` block were re-read row by row against each
  other after the AC4 and AC5 rows changed, and they agree.
- **The space in the sixth path was treated as a shell hazard in every span that names it.** Nine
  spans name it: P4-T1's formatter invocation, P4-T8's `wc -l`, P5-T9's `git add` and `git commit`,
  and the porcelain spans in P5-T10, P5-T11, P5-T12, and P5-T13, plus the seven operands inside
  P2-T4. All are double-quoted. This was checked by re-reading each span after the edit rather than by
  assuming the edit applied uniformly.

### Citations re-derived in the revision pass of 2026-09-03 (revision round 16, applying preflight round 16 findings B1, B2, NB6, and NB7)

Every citation below was re-derived in this pass against the tree as it stands after revision round 15
and after this round's own edits to `spec.md`. No value in this section is carried forward from round
15's verification, including the values round 15 recorded correctly: that pass observed the tree
before this round edited `spec.md`, so its line numbers are evidence about a superseded state rather
than about the state this plan now asserts.

81. This plan file's acceptance-criteria mapping table, the AC4 row — re-read this pass. It lists SIX
    evidence artifacts: `evidence/qa-gates/p1-t5-donotparallelize.md`,
    `evidence/regression-testing/p3-t3-at-risk-tests.md`,
    `evidence/regression-testing/p3-t6-quickfiler-wpfuidispatcher.md`,
    `evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md`,
    `evidence/regression-testing/p4-t6-first-pass-failure.md`, and
    `evidence/qa-gates/p4-t6-quickfiler-tests.md`. The `AC-MAPPING: AC4` line at the end of this file
    was read against that row artifact-for-artifact and lists the same six in the same order. Six is
    therefore the count P5-T4's acceptance sentence must state. The round-15 numeral "five" in that
    sentence was a numeral error and not a disagreement between the table and the task, because the
    task's own enumeration already named six paths.
82. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` AC4, lines
    286-313 — re-read this pass. Its evidence list names the same six artifacts as citation 81, and
    its no-regression FILE set names five files:
    `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`,
    `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`,
    `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs`,
    `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, and
    `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`. The two counts are five FILES and six
    ARTIFACTS. Both occurrences of a numeral in P5-T4 were checked against the quantity each names
    before either was touched, and only the artifact count was changed; the file count of five is
    correct and is left alone.
83. The same `spec.md`, structural anchors re-derived this pass AFTER this round's edits to it:
    `## Scope & Non-Goals` at line 58, `## Write Set` at line 84 with its six path bullets at lines
    88-93, `## Root Cause Analysis` at line 95, `## Test Strategy` at line 224,
    `## Acceptance Criteria` at line 257 with the AC1-AC7 bullets running from line 259 to line 332,
    and `## Risks & Mitigations` at line 334. The AC bullet openings each P5 check-off task quotes are
    AC1 at 259, AC2 at 269, AC3 at 277, AC4 at 286, AC5 at 314, AC6 at 329, and AC7 at 331. P5-T5's
    quoted fragment "AC5: No retry, sleep, or timing tolerance" still matches line 314
    character-for-character after this round's edit to that bullet, which was checked explicitly rather
    than assumed, because this round is the one that edited it. Two edits in this round moved these
    numbers: the `- **Status:**` entry at lines 7-9, which grew from one line to three, and the
    eleven-line AC5 amendment note at lines 318-328. AC5 is the only bullet whose check state this round changed, from `- [x]` to `- [ ]`; AC1,
    AC2, and AC3 keep their `- [x]`, and AC4, AC6, and AC7 keep their `- [ ]`.
84. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p3-t5-no-timing-tokens.md`
    read this pass. Its recorded command is
    `git diff 87cb4df338322844abfa580abea14df77e738e5c -- UtilitiesCS UtilitiesCS.Test`, its recorded
    diff size is 5626 bytes, and it states "The diff covers exactly this plan's five owned files".
    That pathspec is the whole of NB6's substance: the artifact is accurate for the five files it
    measured and reaches none of the sixth, so AC5 could not stand checked on it alone once the write
    set grew to six.
85. P5-T5's acceptance text re-read this pass, before any edit to it was considered. It already
    requires the AC5 check-off to cite BOTH `evidence/qa-gates/p3-t5-no-timing-tokens.md` and
    `evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md`, and it already states that the two
    pathspecs together cover all six owned files. NB6's conditional instruction to make it do so is
    therefore already satisfied and P5-T5 is left unedited. The AC5 row of the mapping table and the
    `AC-MAPPING: AC5` line were read against it and both already name the same two artifacts.
86. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t6-quickfiler-tests.md`
    re-read this pass for the exact token P4-T6's preservation gate greps. The literal `Failed: 8`
    occurs three times in it: on line 23 inside the verbatim console summary block, on line 30 in the
    bulleted derivation of the TRX-sourced figure, and on line 105 in the counterfactual table row.
    A `grep -n -F 'Failed: 8'` over a copy of that file therefore prints three lines and exits 0, so
    the gate is satisfiable rather than unsatisfiable. The artifact
    also records `EXIT_CODE: 1` and TRX `<Counters .../>` values `total="1312" executed="1312"
    passed="1304" failed="8"`.
87. The evidence tree under
    `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/` was
    enumerated in full this pass. It holds thirty artifacts, ending at
    `evidence/qa-gates/p4-t6-quickfiler-tests.md`, and
    `evidence/regression-testing/p4-t6-first-pass-failure.md` is NOT among them. The conditional in
    P4-T6 therefore takes its create branch on the next execution and its found-in-place branch on
    every execution after that, which is the state both branches of the acceptance describe.
88. A `git grep`-equivalent content search over every `.cs` file in this worktree for the literal
    `typeof(UiThread)`, re-run in this pass independently of round 15's citation 70, returns exactly
    seven lines in seven files: `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` line 34,
    `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` line 144,
    `UtilitiesCS.Test/Threading/UiThread_Tests.cs` line 127,
    `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` line 421,
    `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` line 138,
    `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` line 135, and
    `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` line 469. All seven are inside the
    nine test assemblies P0-T14's previous pathspec named, and no production `.cs` file contains the
    literal. This is the derivation behind NB7's conclusion that widening command 2's pathspec to
    `'*.cs'` costs nothing, and behind clause 2's asserted count remaining seven after the widening.
89. The same search repeated for the literal `"Dispatcher"` over every `.cs` file in this worktree
    returns exactly five lines in five files:
    `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` line 35,
    `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` line 14,
    `UtilitiesCS/Threading/WpfUiDispatcher.cs` line 14, `UtilitiesCS/Threading/ThreadMonitor.cs` line
    25, and `UtilitiesCS/Threading/IUiDispatcher.cs` line 13. This agrees with round 15's citation 69
    and was re-derived rather than carried, because P0-T14's clauses 1 and 3 rest on it and this round
    edits clause 3. The set is unchanged, so neither clause's file set moves.
90. P4-T8 clause 1 re-read this pass before clause 2 was rewritten. It reads "If any did, the loop
    restarts from P4-T1 and this artifact records both passes", which is the restart mechanism clause
    2's open-ended count must accommodate and the reason a two-entry demand would be unsatisfiable on a
    third pass. Its trailing sub-clause read "this artifact records both passes", which restates the
    same two-pass count B2 removes from clause 2, so leaving it would have reinstated the defect one
    clause earlier. It now reads "this artifact records every pass, per clause 2". That is the minimum
    edit that makes the two clauses agree; clause 1's restart rule, its seven-step listing requirement,
    and its rewrote-a-tracked-file condition are unchanged.
91. Constraint 1 in "Shell constraints measured in this worktree" re-read this pass, confirming that
    no PowerShell-hosted conditional is available to P4-T6 and that a POSIX `if [ -f ... ]` test in
    git-bash is the form consistent with every other command block in this plan. P3-T5's command block
    was re-read as the in-plan precedent for a plain POSIX `grep` in the same shell.

**Sibling regions re-checked in the revision round 16 pass.**

- **Every occurrence of a numeral naming an AC4 quantity was enumerated before B1 was applied, rather
  than the first one being corrected in isolation.** P5-T4 carries two: "extended to five files",
  which names the no-regression FILE set and is correct, and "exactly the five artifacts", which names
  the EVIDENCE set and was wrong. The mapping table's AC4 row, the `AC-MAPPING: AC4` line, and
  `spec.md`'s AC4 evidence list were each counted independently and all three return six. Only the
  artifact numeral was changed, and the sentence now states both quantities explicitly so the two
  cannot be conflated again by a later reader.
- **P4-T6's clause 5 was re-read after the preservation clause was rewritten.** It quotes the eight
  test names "from the preserved first-pass copy at
  `evidence/regression-testing/p4-t6-first-pass-failure.md`". That wording is correct under both
  branches of the new conditional, because both branches leave that file present and carrying the
  first pass's result; only the create branch writes it. No edit to clause 5 is required and none was
  made.
- **AC4's evidence set was re-checked for a dependency on the preservation order.** AC4 cites the
  preserved fail-before and the pass-after as a pair, and the pass-after artifact is the same file the
  copy reads from. The conditional is placed as the FIRST action of P4-T6, before the command that
  overwrites that file, which is what keeps the pair well-defined; the acceptance now also verifies the
  preserved file's `Failed:` figure in both branches, so a preserved file that is silently a pass-after
  copy is reported as BLOCKED instead of being cited as a fail-before.
- **The three commands of P0-T14 were re-read as a set after command 2's pathspec changed.** The
  census keeps its three-command structure: command 1 remains the nine-test-assembly `"Dispatcher"`
  search whose clause 1 states a two-FILE set, command 2 is now the repository-wide `typeof(UiThread)`
  search, and command 3 remains the repository-wide `"Dispatcher"` search. Every recorded result was
  re-derived against what the widened command will actually print (citations 88 and 89) and none of the
  three clauses' file sets moved. The artifact headings `REFLECTIVE_PROPERTY_NAME_HITS:`,
  `REFLECTIVE_UITHREAD_TYPE_HITS:`, and `REPOSITORY_WIDE_PROPERTY_NAME_HITS:` and the per-hit
  classification rule are unchanged and still describe the three commands in order.
- **The `spec.md` Risks & Mitigations paragraph that narrates P0-T14 was re-read against the widened
  command.** It says P0-T14 "runs that census across all nine test assemblies and repository-wide
  across `.cs` files", which remains true of the three-command set after the widening — the widening
  moves one command from the first scope into the second, and both scopes are still exercised. No edit
  to that paragraph is required and none was made.
- **`spec.md`'s AC5 criterion text was left byte-identical.** Only the checkbox and an appended
  amendment note changed. The Evidence line under AC5 still names `p3-t5-no-timing-tokens.md` alone and
  was deliberately not extended, because P5-T5 is the task that adds the second citation at check-off
  time and pre-writing it would assert an artifact that does not yet exist.
- **`spec.md`'s `## Write Set`, `## Scope & Non-Goals`, and the Files/modules-to-change list under
  `## Proposed Fix` were re-read after the AC5 edit.** Write Set and Scope both name six files and are
  consistent with this plan's Scope section. The Files/modules-to-change list still names two files;
  it is a pre-existing non-authoritative summary, it is finding NB5, and NB5 is excluded from this
  delta, so it is recorded here as examined and deliberately left alone rather than swept.
- **Every restatement of P4-T8's two-pass requirement elsewhere in the plan was searched for before B2
  was applied, but that search was incomplete: it found three restatements and missed a fourth, which
  survived revision round 16 stale and was corrected only in revision round 17. The claim originally
  recorded here — that the count "was changed in one place and left stale in none" — was false when it
  was written.** The four restatements are:
  P4-T8 clause 2 itself, P4-T8 clause 1's trailing "records both passes" sub-clause, the
  "Phase 4 first-pass marks cleared by this round" header note near the top of this file, and the
  "Second pass, and why the first pass's marks were cleared" note that opens Phase 4, the last two of
  which said the acceptance "requires the loop-closure artifact to record both passes". All four now state the
  open-ended, chronologically ordered requirement. The header note's surrounding sentences — which
  record why P4-T1 through P4-T5 were returned to `[ ]` — are unchanged.
- **No task's checkbox state was changed in the plan by this round.** P0-T14, P2-T4, P4-T6, P4-T8, and
  every Phase 5 task were unchecked before this round and are unchecked after it; the completed tasks
  this round's edits sit next to — P2-T3, P3-T5, P3-T6, P4-T5 — were not edited. The only checkbox
  changed anywhere in this round is `spec.md`'s AC5, which NB6 requires.

### Sibling regions re-checked in the revision round 9 pass

- **The spec.md line-168 WpfDispatcherYield.cs backtick leftover was found and fixed in this
  round.** Round 9's backtick-removal pass converted every other backtick-wrapped
  scope-exclusion/precedent/context-reference path mention in `spec.md` and this plan to plain text,
  but left the `WpfDispatcherYield.cs:64-66` mention inside spec.md's "Error handling and logging
  updates" subsection (the same file already referenced in plain-text form at spec.md lines 100 and
  133) still backtick-wrapped. It is now corrected to the unbackticked, full-repository-relative-path
  form `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:64-66`, matching the spelling already
  established at those two lines. The surrounding code-identifier backticks on
  `InvalidOperationException`, `Initialize()`, and `IdleAsyncQueue.OnApplicationIdle` in the same
  passage were left untouched, because those are code-identifier citations rather than path mentions.
- **No other task in this plan quotes the AC3 or AC4 bullet text in full, so no further
  quotation-consistency fix was needed.** P5-T1, P5-T2, and P5-T5 through P5-T8 were each re-read this
  pass: every one quotes only a short opening fragment of its own AC bullet (for example P5-T4's "AC4:
  No regression in"), never the AC3 or AC4 bullet in full. P5-T3's opening-fragment quotation of AC3
  was re-derived directly (citation 60 above) and P5-T4's opening-fragment quotation of AC4 ("AC4: No
  regression in") was re-checked against spec.md's current AC4 bullet and still matches.
- **Constraint 5, the TRX selection rule, and the seven TRX-reading tasks (P0-T10, P0-T11, P3-T2,
  P3-T3, P3-T6, P4-T5, P4-T6) were read and confirmed unaffected by round 9's backtick-removal
  edits.** None of the seven task bodies, and neither the constraint-5 sourcing paragraph nor the TRX
  selection rule that follows it, mentions WpfDispatcherYield.cs or any other
  scope-exclusion/precedent/context-reference file path; each cites only its own vstest command,
  `/ResultsDirectory` value, and the `notExecuted`/`Skipped` sourcing rule already re-derived in the
  revision round 8 pass. No command line, task ID, write-target file, or evidence path in any of the
  seven changed as a result of round 9.

### Sibling regions re-checked in the revision round 10 pass (mechanical backtick-removal correction)

- **P5-T9's and P5-T10's exclusion-assertion path mentions were read and corrected in this round.**
  Both tasks were still backtick-wrapped after round 9 (confirmed against commit 12a11031), asserting
  that the committed diff and the anchored name-status diff contain no path under .claude/,
  .codex/, .agents/, or config/. On re-derivation this pass, that "no path under X" phrasing is
  itself the exclusion case the delegation's backtick-removal instruction names explicitly, so those
  spans are now plain text (.claude/, .codex/, .agents/, config/blast-radius.json,
  config/orchestration-routing.json) rather than backtick-wrapped, matching every other exclusion
  mention in this plan and in spec.md.

### Sibling regions re-checked in the revision round 8 pass

- **Every live occurrence of `notExecuted` in the plan was enumerated before any edit, and again
  after.** The pre-edit set named the attribute as the `Skipped` source in eight places: constraint
  5's sourcing paragraph, constraint 5's redaction rule, P0-T10 (three: its sourcing paragraph, its
  `where failed supplies ...` sentence, and its redaction rule), P0-T11, P4-T5, and P4-T6. All eight
  were corrected. Two further occurrences sit inside the verbatim `<Counters .../>` example blocks in
  constraint 5 and in P0-T10; those quote tool output rather than instruct the executor, and each is
  now immediately followed by the prohibition, so both were left as they stand. After the edits,
  every surviving occurrence of the token is either inside one of those illustrative XML blocks,
  inside an explicit prohibition on using the attribute, or
  inside the measured-evidence sentence recording that the probe's TRX carried `notExecuted="0"`. No
  occurrence anywhere in the plan now names it as a source for any recorded value.
- **The three tasks that read a `Failed` count but record no `Skipped` count were checked
  individually rather than swept.** P3-T2, P3-T3, and P3-T6 read only `failed`, so the `total` minus
  `executed` derivation does not apply to them; each now says so explicitly and forbids introducing
  `notExecuted`, so a later reader cannot mistake the absence for an omission. The delta added no
  `Skipped` field to any of the three, because adding one would have changed a gate rather than its
  documentation.
- **The `Output Summary:` field lists and the acceptance clauses were re-read against each other for
  all four tasks that record `Skipped`.** P0-T10, P0-T11, P4-T5, and P4-T6 now each list `total` and
  `executed` in the field list AND require them in the acceptance clause, so the derived value is
  auditable from the artifact rather than being an unverifiable single number. Recording the two
  operands is what makes the `Skipped` figure falsifiable by a third party, which is the property
  `notExecuted` lacked.
- **The tie-break rule's central form was checked against the redaction rule it now sits beside.**
  The finding's suggested wording would have had each artifact record `TRX SELECTED: <filename>`.
  That directly contradicts the redaction rule added in round 7, because the default TRX filename is
  composed from the host account name and the machine name. The rule as written records the results
  directory plus the selected file's last-modified timestamp instead, which identifies the selection
  uniquely within that directory and discloses neither. This is a sibling-invalidation catch inside
  this round's own delta.
- **The claim that per-task results directories make collisions impossible was corrected where it
  appeared.** Constraint 5's sourcing paragraph and the round-7 changelog both stated that files
  "cannot collide" because each task owns its directory. That is true across tasks and false within
  one task across re-runs, which is exactly the case O1 raised. Both sentences now say the uniqueness
  is per task and point at the selection rule for the re-run case.
- **The seven TRX-reading tasks were re-enumerated after the four per-task tie-break copies were
  replaced.** All seven — P0-T10, P0-T11, P3-T2, P3-T3, P3-T6, P4-T5, P4-T6 — now carry an explicit
  pointer to the central rule instead of a local copy of it, and the rule itself names all seven by
  task ID. Three of those pointers (P3-T3, P4-T5, P4-T6) additionally name the specific in-plan text
  that anticipates that task being re-run. The rule is stated
  in exactly one place and referenced from seven, so it can no longer be present in some tasks and
  silently absent from others. P1-T4, the eighth vstest task, reads no TRX and carries no pointer;
  the reason is stated in the round-7 section.
- **The `Failed:`/`Skipped:` trigger statement was corrected in both places it appeared**, not only
  in the one the finding named: constraint 5's measured-behaviour paragraph and the round-7
  changelog's P1-T4 rationale. Both now state the per-counter rule. P1-T4's own acceptance is
  unchanged and remains satisfiable: its run has a non-zero failure count by construction, so the
  `Failed:` line it reads is printed whatever the skip count is.
- **The "Shell constraints measured in this worktree" section header was re-read against its own new
  contents.** Its opening sentence attributed the five constraints to preflight rounds 3, 4, and 5;
  constraint 5 now also carries a round-6 measurement, so the sentence names rounds 3, 4, 5, and 6.
  The count of five is unchanged, because round 6 corrected constraint 5 rather than adding a sixth.
  The section now also states, before the numbered list, that the TRX selection rule at its end is a
  plan rule rather than an environment measurement, so the section's self-description matches what it
  contains. No existing constraint number moved, so every cross-reference elsewhere in the plan —
  which names constraint 1, 2, 4, or 5 — remains valid; each was re-read to confirm it.
- **The plan's `Status:` and `Version:` metadata were updated to name this round and its findings**,
  so the header does not report a round-7 plan while carrying round-8 text.
- **P4-T7 and P4-T8 were checked for a reason to change and required none.** P4-T7 reads Cobertura
  attributes and class-node line hits, and P4-T8 reads exit codes, artifact paths, and `wc -l` rows.
  Neither reads a TRX or a test count, so neither is affected by this round. P4-T7's own redaction
  rule was re-read and is consistent with the extended TRX redaction rule; both now prohibit
  recording an absolute host path from any artifact class this plan reads.
- **The write-target set was re-read after every edit in this round and is unchanged.** The five
  paths are `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`,
  `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`,
  `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, and
  `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`.
  UtilitiesCS/Threading/ProgressTrackerAsync.cs remains outside the write-target list. No evidence
  path and no command line changed, so the acceptance-criteria mapping table and the `AC-MAPPING:`
  block below are unaffected and were re-read row by row against each other to confirm they still
  agree.

### Sibling regions re-checked in the revision round 7 pass

- **Every task in the plan that records a `Failed` or a `Skipped` count was enumerated before any
  edit was made, not only the five the finding named.** There are eight: P0-T10, P0-T11, P1-T4,
  P3-T2, P3-T3, P3-T6, P4-T5, and P4-T6. Seven now carry the two-source rule. The eighth, P1-T4, is
  deliberately unchanged, and the reason is stated rather than assumed: P1-T4 is the `[expect-fail]`
  task, its acceptance requires `Failed: 1`, and the measured behaviour is that EACH of the `Failed:`
  and `Skipped:` console lines is printed only when its OWN counter is non-zero — independently of
  the other, as the round-6 three-test probe confirmed by printing both lines for a run that had one
  failure and one skip. A run that satisfies P1-T4's
  acceptance therefore has a non-zero failure count and prints the line P1-T4 reads, so applying the
  TRX rule there would have changed
  a gate that is already satisfiable. Constraint 5 states this explicitly so a later reader does not
  read P1-T4 as an omission.
- **Every one of the seven changed tasks was re-read to confirm it already carries `/Logger:trx` and
  a `/ResultsDirectory:` that no other task shares.** The seven directories are `TestResults/p0-t10`,
  `p0-t11`, `p3-t2`, `p3-t3`, `p3-t6`, `p4-t5`, and `p4-t6`; P1-T4's `p1-t4` is the eighth and is
  likewise unique. Because no two tasks write to the same results directory, a TRX cannot be
  attributed to the wrong TASK, and the rule needs no new command, no `LogFileName` argument, and no
  change to any existing switch. No command line in the plan was modified in this round. Uniqueness
  across tasks does not, however, bound the number of `.trx` files inside one task's own directory
  when that task is re-run; revision round 8 added the "TRX selection rule" after constraint 5 to
  cover that case for all seven tasks.
- **The P0-T10 / P4-T5 and P0-T11 / P4-T6 identity preconditions were re-checked after this round's
  edits.** This round edited only `Output Summary:` and acceptance prose, so both pairs still differ
  only in `--output` and `/ResultsDirectory` (the first pair) and in `/ResultsDirectory` alone (the
  second). P4-T7 clause (c) rests on the first of those and is unaffected.
- **Sibling invalidation caught and repaired inside this round's own delta.** The first draft of this
  round required each affected artifact to "name the TRX file" the counts were read from.
  `vstest.console.exe` composes the default TRX filename from the host account name and the machine
  name, so that requirement would have written an account-name disclosure into committed evidence —
  the same defect class P4-T7's existing redaction rule was added to prevent for the Cobertura
  `filename` attribute. All four occurrences were rewritten to identify the file by its
  repository-relative results directory only, and the rule is stated once centrally in constraint 5
  and bound on all seven tasks. The finding's specified treatment is unaffected: it named the
  directory, not the filename, as the locator.
- **The "Shell constraints measured in this worktree" header sentence was corrected from "Four
  constraints" to "Five", and every cross-reference to a numbered constraint elsewhere in the plan was
  re-read.** The surviving references name constraint 1 (P0-T5 step 1 and P3-T5), constraint 2 (P0-T5
  step 4, P0-T10, and P0-T11), and constraint 4 (P0-T5 step 3, P0-T10, and P0-T11). The new entry was
  appended as constraint 5, so no existing number moved and no reference was invalidated.
- **The tasks that record test counts but were NOT changed were checked for a reason to change
  them.** P4-T7 records only Cobertura root-element attributes and class-node line hits, and P4-T8
  records exit codes, artifact paths, and `wc -l` rows; neither reads a test count, so neither is
  touched. P3-T3's clause 1 (`Total tests` greater than zero) and P4-T5's clause comparing `Total
  tests` to the baseline plus 2 both compare console-sourced values on each side, and P4-T6's clause
  compares its console-sourced `Total tests` to P0-T11's console-sourced one, so no comparison in the
  plan now spans two different sources.
- **The failing-test NAME sets were distinguished from the `Failed` COUNT throughout.** P0-T10's and
  P0-T11's `BASELINE_FAILURE_SET:`, P3-T3's clause 3, and P4-T5's and P4-T6's subset clauses all
  operate on test names taken from per-test output, which is printed on green and red runs alike.
  Only the aggregate count moved to the TRX, so none of those mechanisms changed.
- **The write-target set was re-read after every edit in this round and is unchanged.** The five
  paths are `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`,
  `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`,
  `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, and
  `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`.
  UtilitiesCS/Threading/ProgressTrackerAsync.cs remains outside the write-target list. No evidence
  path changed, so the acceptance-criteria mapping table and the `AC-MAPPING:` block below are
  unaffected and were re-read row by row against each other to confirm they still agree.

### Sibling regions re-checked in the revision round 6 pass

- **Every `msbuild.exe` occurrence in the whole plan was enumerated, not only the six the finding
  named.** The plan contains seven: the six build command blocks (P0-T8, P0-T9, P1-T3, P3-T1, P4-T3,
  P4-T4), which now all carry the `MSYS_NO_PATHCONV=1 ` prefix, and the P0-T5 step 3 `-version` probe,
  which deliberately does not and must not. The two distinct build command lines were each replaced
  across all their occurrences in one operation — the analyzer line appears four times (P0-T8, P1-T3,
  P3-T1, P4-T3) and the nullable line twice (P0-T9, P4-T4) — so no occurrence could be missed through
  differing surrounding whitespace. The switch sets after the prefix are byte-identical to their
  pre-round-6 form: `/t:Rebuild` in all six, no `/p:Nullable=enable` anywhere, `/t:Build` nowhere.
- **Every `vstest.console.exe` and `dotnet-coverage` occurrence in the whole plan was enumerated.**
  There are eight invocations: six leading `MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe`
  (P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, P4-T6) and two leading `MSYS_NO_PATHCONV=1 dotnet-coverage collect`
  (P0-T10, P4-T5). All eight carry the prefix. The remaining textual occurrences of those names are in
  prose or in the P0-T5 step 2 `-find` pattern, none of which is an executed command line.
- **The P0-T10 / P4-T5 identity precondition was re-checked operand by operand after the prefix was
  added.** Both now begin `MSYS_NO_PATHCONV=1 dotnet-coverage collect`; both use the native directory
  spelling in the quoted operand after `--`; neither carries a `PATH=` prefix; neither carries
  `/EnableCodeCoverage`. They still differ only in `--output` and `/ResultsDirectory`, which is what
  P4-T7 clause (c) rests on. The same check was repeated for the P0-T11 / P4-T6 pair, which still
  differ only in `/ResultsDirectory`. Sibling-invalidation risk addressed here: a prefix added to one
  side of either pair and not the other would have left one run executing the suite and the other
  executing nothing, while both artifacts still recorded an exit code.
- **The acceptance clauses that read the recorded command line were re-checked.** P0-T9 and P4-T4 were
  the only two, and both were reworded from "begins" to "contains". Both retain the two substantive
  clauses verbatim. No other acceptance clause anywhere in the plan asserts a position within a
  recorded command line.
- **P0-T5's step numbering was re-checked against every cross-reference in the plan.** Inserting the
  NuGet restore as step 4 renumbered the former steps 4, 5, and 6 to 5, 6, and 7. Every surviving
  cross-reference in the plan names step 1, step 2, or step 3 — none names a renumbered step — so no
  reference was invalidated. The two references to step 4 both refer to the new restore step.
- **N-3 checked and no change required.** No task in this plan asserts an equality against a specific
  analyzer warning count. P0-T8 records the observed baseline count as data; P3-T1 and P4-T3 each
  require only that the observed count be less than or equal to that recorded baseline. A measured
  baseline of 5 satisfies those clauses exactly as any other value would, so the plan carries no
  number that the environment could falsify.
- **No `pwsh` was reintroduced.** The only occurrences of that token in the plan are constraint 1 of
  "Shell constraints measured in this worktree", P0-T5 step 1's prohibition on running the PowerShell
  bootstrap script, and P3-T5's rationale for using a `grep` pipeline. None is a command. The three new
  or rewritten command lines this round adds — `nuget.exe restore TaskMaster.sln` and the fourteen
  prefixed invocations — introduce no shell host.
- **The write-target set was re-read after every edit in this round and is unchanged.** This round
  touched command prefixes, one new toolchain step, two acceptance wordings, one evidence-redaction
  rule, and prose. It added no file to the diff and removed none.
  UtilitiesCS/Threading/ProgressTrackerAsync.cs remains outside the write-target list, and the
  restore step writes only into gitignored `packages/`.
- **P4-T7's new redaction rule was checked against P4-T7's own acceptance clauses.** Clauses (a) and
  (b) read the intersected line numbers and their `hits` values, which the rule explicitly preserves;
  clauses (c) and (d) read root-element attributes, which carry no path. The rule therefore removes
  the account-name disclosure without removing anything the gate reads. It was also checked against
  P5-T7, which cites the artifact and quotes coverage figures rather than paths.
- **The new "Worktree state assumed by Phase 0" section was checked against every Phase 0 task.** It
  states that the authoring worktree's prebuilt state must not be treated as licence to skip a task,
  which is consistent with P0-T5's unconditional restore steps and with its conditional SDK bootstrap.
  No task's acceptance was weakened to accommodate the prebuilt state.

### Sibling regions re-checked in the revision round 5 pass

- Every one of the eight `vstest.console.exe` invocations in the plan was re-read after the C2
  rewrite. Six of them (P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, P4-T6) lead with
  `PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe`, and two (P0-T10, P4-T5) carry
  `-- "<resolved-vstest-dir-native>\vstest.console.exe"` as an argument to `dotnet-coverage`. The
  pre-C2 single placeholder that stood for a full executable path was removed everywhere; every
  vstest invocation in the plan now substitutes one of the two directory placeholders defined in
  P0-T5 step 2, and no command in the plan names a full `vstest.console.exe` path in the
  command-NAME position.
- P0-T10 and P4-T5 were compared operand by operand after the rewrite. They remain identical apart
  from `--output` and `/ResultsDirectory`, which is the precondition P4-T7 clause (c) rests on. Both
  use the native directory spelling; neither carries a `PATH=` prefix; neither carries
  `/EnableCodeCoverage`.
- P0-T11 and P4-T6 were compared the same way and remain identical apart from `/ResultsDirectory`.
  Both now carry the `PATH=` prefix, so P4-T6's "flag set identical to P0-T11's" clause still holds.
- Sibling invalidation caught in this pass: P0-T10's forward-slash rationale asserted that the P0-T5
  `-find` pattern was "the only backslashes left anywhere in this plan's commands". The C2 rewrite
  added two more, inside the double-quoted `dotnet-coverage` operands. The sentence was corrected to
  name all three sites and to state that double quoting is what preserves them in each.
- Every `msbuild` invocation was re-read after the C3 rewrite. All six command blocks (P0-T8, P0-T9,
  P1-T3, P3-T1, P4-T3, P4-T4) and the P0-T5 step 3 probe now spell `msbuild.exe`. The switch sets are
  byte-identical to their pre-C3 form: `/t:Rebuild` in all six, `/p:EnableNETAnalyzers=true
  /p:EnforceCodeStyleInBuild=true` in the four analyzer builds, `/p:TreatWarningsAsErrors=true` in
  the two type-check builds, and no `/p:Nullable=enable` anywhere. The quoted-command-line acceptance
  clauses in P0-T9 and P4-T4 were updated to expect the `msbuild.exe` spelling; both retain their two
  substantive clauses unchanged, and both now state that the spelling clause is not the substantive
  one.
- P0-T5's step 3 justification previously rested on a prior round's review note that bare `msbuild`
  resolved. That claim is now known to be wrong in this worktree and was replaced with the measured
  exit-127 and exit-0 readings.
- P0-T5's acceptance clause previously required "the resolved vstest path is a non-empty existing
  file path". After C2 the recorded artefact value is a directory, so the clause was rewritten to
  require both recorded directory values and the existence of `vstest.console.exe` inside them.
- P3-T5's rationale named a PowerShell-hosted filter as the superseded alternative and cited the
  round-2 refusal of a `-Command` payload specifically. Because the refusal is now known to apply to
  every argument shape, the paragraph was rewritten so it does not imply that some other PowerShell
  invocation shape would have been available.
- Phase 0's new introductory paragraph (finding N1) was checked against every porcelain gate in the
  plan. P4-T1's two porcelain spans are deliberately unscoped but are compared before-against-after,
  so pre-existing .claude/agent-memory/** entries appear in both and cancel. P5-T10, P5-T11,
  P5-T12, and P5-T13 are pathspec-scoped to `UtilitiesCS`, `UtilitiesCS.Test`, and the feature
  folder. P3-T4's porcelain span is scoped to a single file. No gate is affected by the dirty
  .claude/ state, which is what the paragraph asserts.
- P5-T11, P5-T12, and P5-T13 (finding N2) were rewritten to carry the feature-folder pathspec on
  `git commit` as well as on `git add`, matching P5-T9. P5-T12 previously described its commit only
  in prose; it now has an explicit command block, so all four commit tasks in Phase 5 are stated in
  the same form and none of them can sweep residue left staged by P3-T4's `git add -A -- UtilitiesCS
  UtilitiesCS.Test`.
- The five write targets were re-read against the "Scope" section after all of this round's edits.
  The set is unchanged: this round touched only command spellings, commit pathspecs, and prose.
  UtilitiesCS/Threading/ProgressTrackerAsync.cs remains outside the write-target list.

### Sibling regions re-checked in the preflight round 3 pass and found consistent

- Every command block in the plan was re-read for a surviving PowerShell-hosted payload. The three
  line-count blocks are `wc -l` and P3-T5's filter is a `grep -E` pipeline. One conditional
  PowerShell-script bootstrap remained in P0-T5 after this round and was removed in the round-5 pass
  recorded below.
- Every occurrence of the string `Measure-Object` was re-read. The one remaining occurrence is
  P0-T13's explicit prohibition on using it, which is prose about the idiom rather than a command.
- P0-T10's forward-slash paragraph was corrected in this pass to say every command block runs
  through a POSIX shell, naming the places where backslashes are deliberately retained and protected
  by double quoting.
- P3-T5's two closing rationale paragraphs named `Select-String` as the consumer of the diff file.
  Both were corrected to name the `grep` pipeline that replaced it, so the stated vacuous-pass
  mechanisms still describe the command the task actually runs.
- P5-T10's rationale asserted that P4-T1 "restored every unowned path the formatter rewrote". After
  the NB1 change P4-T1 restores nothing, because it rewrites nothing outside the owned five. The
  clause was corrected to state the new mechanism. This is the sibling-invalidation case: the NB1
  edit was in Phase 4 and the invalidated sentence was in Phase 5.
- The acceptance-criteria mapping table was re-read row by row against the `AC-MAPPING:` block. NB3
  named AC3's Implementation cell. Two further disagreements of the same class were found in this
  pass and corrected: AC4's cell used a semicolon where `AC-MAPPING:` uses a comma, and AC5's cell
  listed `P2-T1, P1-T2, P1-T5` where `AC-MAPPING:` lists `P1-T2, P1-T5, P2-T1`. All seven rows now
  match their `AC-MAPPING:` line character for character in the Implementation column.
- P1-T2's acceptance already forbade `Thread.Sleep`, `Task.Delay`, `Thread.CurrentThread.Join`,
  `SpinWait`, and `Dispatcher.PushFrame` in the new CODE. The NB2 change extends the prohibition to
  the new DOC COMMENT and adds the two tokens the code clause did not carry, `Retry`/`retries` and
  `Timeout(`, so the P1-T2 constraint is now a superset of what P3-T5 searches for. The verbatim
  test code quoted in P1-T2 was re-read against all seven tokens and contains none of them.
- P0-T7's note that "P4-T2 and P5-T10 are both written to be satisfiable against a non-empty drift
  set" was re-checked against the rewritten P4-T2 acceptance and still holds: P4-T2 now requires the
  reported set to be a subset of `BASELINE_FORMAT_DRIFT_SET`, which a non-empty baseline satisfies.
- P5-T9's `git add` was retained ahead of its new pathspec commit deliberately. A pathspec commit
  accepts only paths already known to git, and this plan creates new untracked evidence files under
  the feature folder, so removing the `git add` would make the commit fail on exactly those files.

### Sibling regions re-checked in the preflight round 2 pass and found consistent

- `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` lines 134-188 — the `DispatcherField` /
  `ForceDispatcherNull` / `RestoreDispatcher` helpers sit far from line 28, so P1-T5's attribute
  insertion cannot disturb them and the new test's mirrored idiom stays valid.
- `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` lines 1-16 — the file's `using` block
  already imports `Microsoft.VisualStudio.TestTools.UnitTesting`, so `DoNotParallelize` resolves
  without a new `using`, and the attribute-only edit adds no directive to a 514-line file.
- The acceptance-criteria mapping table and the `AC-MAPPING:` block below were re-derived from the
  task list in this pass rather than edited independently, which is what removes the AC2 and AC7
  disagreements the prior round reported.
- Every remaining `git diff` span in the plan was re-read in this pass. The two-dot form now appears
  only in P5-T10, which runs after the P5-T9 commit and is therefore not vacuous; every span that
  runs before that commit uses either the single-ref working-tree form or `--cached`.

---

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: UtilitiesCS/Threading/UiThread.cs | lines 135-140, Dispatcher property and `null!` backing field
CITATION: UtilitiesCS/Threading/UiThread.cs | line 1, nullable-enable directive
CITATION: UtilitiesCS/Threading/UiThread.cs | line 61, `Dispatcher = _syncContextForm.UiDispatcher;`
CITATION: UtilitiesCS/Threading/UiThread.cs | lines 113-125 and 147-158, lazy-init sibling properties
CITATION: UtilitiesCS/Threading/ProgressTrackerAsync.cs | line 33, `UiDispatcher = UiThread.Dispatcher;`
CITATION: UtilitiesCS/Threading/ProgressTrackerAsync.cs | line 35, first dereference `await UiDispatcher.InvokeAsync(`
CITATION: UtilitiesCS/Threading/SyncContextForm.cs | line 30, `public Dispatcher UiDispatcher { get; private set; } = null!;`
CITATION: UtilitiesCS/Threading/WpfUiDispatcher.cs | lines 24-25 and 37, parameterless ctor provider over UiThread.Dispatcher
CITATION: UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs | lines 57-67, existing InvalidOperationException precedent
CITATION: UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs | lines 45-46, default fallback provider
CITATION: UtilitiesCS.Test/Threading/UiThread_Tests.cs | 104 lines, namespace UtilitiesCS.Test.Threading, single TestClass
CITATION: UtilitiesCS.Test/UtilitiesCS.Test.csproj | line 494, Compile Include for Threading\UiThread_Tests.cs
CITATION: UtilitiesCS.Test/UtilitiesCS.Test.csproj | lines 477, 479, 490, Compile Include for ProgressTracker_Tests.cs, ProgressTrackerAsync_Tests.cs, IdleAsyncQueue_Tests.cs
CITATION: UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs | lines 141-186, DispatcherField/ForceDispatcherNull/RestoreDispatcher
CITATION: UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs | lines 248-289, AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException
CITATION: UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs | line 28, sole [TestClass] and P1-T5 insertion point; line 144, "_dispatcher" reflection write; 347 total lines
CITATION: UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs | lines 126-190, InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker
CITATION: UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs | line 13, sole [TestClass] and P1-T5 insertion point; lines 138, 150, 152, 162, reflection write then InitializeAsync then Dispatcher.PushFrame; 205 total lines
CITATION: UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs | line 12, namespace UtilitiesCS.Test; line 14, sole [TestClass] and P1-T5 edit site; line 422, "_dispatcher" reflection write; lines 411-432, Initialize_WithCurrentDispatcherAndScreen_InitializesViewerAndUpdatesUi; 514 total lines, pre-existing file-size overage
CITATION: UtilitiesCS.Test/Threading/CurrentStoreContextTests.cs | lines 15-16, prevailing [TestClass] then [DoNotParallelize] two-line idiom
CITATION: UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs | lines 117-142, YieldAsync_WithoutDispatcher_RemainsStrict
CITATION: UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs | line 55, new WpfDispatcherYield()
CITATION: UtilitiesCS.Test/Properties/AssemblyInfo.cs | line 18, assembly Parallelize attribute
CITATION: QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs | lines 26 and 64, new WpfUiDispatcher()
CITATION: QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs | lines 23-24 and 48-50, the file's only two [TestMethod] declarations, Construction_YieldsAnIUiDispatcher and Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread — the basis for P3-T6's Total tests of 2 and its two by-name clauses
CITATION: .csharpierignore | lines 4-14, evidence and project-file exclusions, including line 8 `*.trx`, which keeps the TRX files P0-T10, P0-T11, P3-T2, P3-T3, P3-T6, P4-T5, and P4-T6 read out of P4-T2's repo-wide format check
CITATION: .gitignore | line 39 `[Tt]est[Rr]esult*/` and lines 144-145 `coverage/*`, both directory-scoped, which is why backslash-stripped root-level paths would escape them and why every `TestResults/<task>/` TRX this plan reads is untracked
CITATION: coverage.config | lines 12-22, third-party-only module excludes; no first-party assembly excluded, so added production and test lines legitimately move lines-valid
CITATION: docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md | lines 259-332, AC1-AC7, with the ## Acceptance Criteria heading at line 257 and ## Risks & Mitigations at line 334; the AC bullet openings are AC1 at 259, AC2 at 269, AC3 at 277, AC4 at 286, AC5 at 314, AC6 at 329, AC7 at 331 (re-derived in revision round 16 after that round's AC5 amendment and its two-line Status edit; supersedes the round-15 "lines 257-319" reading, the round-9 "lines 249-272" reading, and citation 18's round-1 "lines 234-257" reading)
CITATION: docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md | ## Write Set at line 84 with six path bullets at lines 88-93, the sixth being QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs — re-derived in revision round 16 and agreeing with this plan's own Scope list
CITATION: docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md | line 314, AC5 now reads `- [ ]` and carries a revision round 16 amendment note at lines 318-328 recording why it was unchecked and that P5-T5 re-checks it once evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md exists; the criterion text on line 314 is unchanged
CITATION: docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md | lines 286-313, AC4's five-file no-regression set and its SIX-artifact evidence list (p1-t5-donotparallelize.md, p3-t3-at-risk-tests.md, p3-t6-quickfiler-wpfuidispatcher.md, p2-t4-emailmovemonitor-reflection-target.md, p4-t6-first-pass-failure.md, p4-t6-quickfiler-tests.md) — the derivation behind P5-T4's corrected artifact numeral
CITATION: docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p3-t5-no-timing-tokens.md | recorded Command line `git diff 87cb4df338322844abfa580abea14df77e738e5c -- UtilitiesCS UtilitiesCS.Test`, 5626-byte diff, "The diff covers exactly this plan's five owned files" — the pathspec limitation NB6's AC5 amendment records
CITATION: docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/issue.md | line 8, promotion-time merge base SHA 5ebaaf105d8241f309f704d1ff90af2e32e5a6c1, superseded by BASE 87cb4df338322844abfa580abea14df77e738e5c after the reconciliation merge; issue.md is outside this plan's write set and is not edited
CITATION: .git worktree log for agent-a18cc3bc53f9c1d8a | final entry `merge origin/main: Merge made by the 'ort' strategy` producing a2ef517b, and recorded origin/main tip 87cb4df338322844abfa580abea14df77e738e5c — the derivation of the re-anchored BASE
CITATION: global.json | lines 6-10, sdk paths [".dotnet-sdk", "$host$"] and the missing-repo-local-SDK error message; `.dotnet-sdk/` absent from this worktree
CITATION: scripts/vscode/Install-RepoDotNetSdk.ps1 | line 3 default version 8.0.205; line 26 download URL https://builds.dotnet.microsoft.com/dotnet/Sdk/$Version/dotnet-sdk-$Version-win-$Architecture.zip; line 36 install dir Join-Path $PSScriptRoot '..\..\.dotnet-sdk' — the three values P0-T5's POSIX bootstrap reproduces without running the script
CITATION: .gitignore | line 350 `.dotnet*/`, which already ignores the `.dotnet-sdk/` directory P0-T5's bootstrap creates
CITATION: .gitignore | line 191 `**/[Pp]ackages/*` and line 193 `!**/[Pp]ackages/build/`, which ignore the NuGet restore output P0-T5 step 4 creates, with the single named exception
CITATION: .github/workflows/_build-analyzers.yml | line 17 `SOLUTION_PATH: TaskMaster.sln`, line 45 `nuget restore $env:SOLUTION_PATH`, line 50 the analyzer msbuild that follows it — the CI-parity basis for P0-T5 step 4
CITATION: .github/workflows/_build-nullable.yml | line 45, `nuget restore $env:SOLUTION_PATH` preceding the nullable build
CITATION: .github/workflows/_mstest-coverage.yml | line 45, `nuget restore $env:SOLUTION_PATH` preceding the coverage run
CITATION: UtilitiesCS/packages.config | one of 18 packages.config files in this solution, which is why a missing NuGet restore fails the first build with CS0246 and missing-.targets errors
CITATION: UtilitiesCS.Test/packages.config | the test-assembly half of the same packages.config-based restore requirement
CITATION: dotnet-tools.json | repository-root local tool manifest pinning CSharpier 1.2.6; no `.config/` directory exists, so `dotnet tool restore` reads this file and performs no NuGet package restore
CITATION: .github/workflows/ci.yml | lines 21-23, format-check delegated to _format-check.yml
CITATION: .github/workflows/_format-check.yml | line 37 `dotnet tool restore`, line 41 `dotnet csharpier check .` repo-wide, the CI-parity command P4-T2 runs
CITATION: UtilitiesCS.Test/Threading/UiThread_Tests.cs | 17 blank lines of 104 total, and UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs 92 blank of 514, the measurement that disqualifies Measure-Object -Line as the counting idiom
CITATION: QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs | 314 total lines; namespace QuickFiler.Helper_Classes.Tests; lines 21-22 [TestClass] then [DoNotParallelize]; line 32 private object _capturedDispatcher; lines 33-37 the PropertyInfo DispatcherProperty declaration over GetProperty("Dispatcher", Public | Static); line 49 and line 58 the two DispatcherProperty?.GetValue(null) call sites; line 59 current.Should().BeSameAs(_capturedDispatcher); eight [TestMethod] declarations at lines 87, 107, 134, 147, 176, 200, 234, 266; no using System.Reflection directive — the sixth owned file and P2-T4's edit sites
CITATION: QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs | none of Thread.Sleep, Task.Delay, SpinWait, Retry, retries, Timeout(, PushFrame occurs anywhere in the file, while Thread.CurrentThread does occur at line 273 — the derivation that makes P2-T4's whole-diff token filter satisfiable
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs | line 135, FieldInfo field = typeof(UiThread).GetField( — the same-assembly precedent for the field route P2-T4 adopts
CITATION: QuickFiler.Test/QuickFiler.Test.csproj | line 206, Compile Include for Helper Classes\EmailMoveMonitorTests.cs — the sixth owned file is already wired and already tracked, so P2-T4 needs no project-file edit
CITATION: UtilitiesCS/Threading/UiThread.cs | post-P2-T1 state re-derived in round 15: property lines 135-148, null guard at 139, throw at 141, return at 145, private setter at 147, and private static Dispatcher? _dispatcher; at line 149 — the mechanism a reflective property read now propagates
CITATION: UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs | lines 142-148 DispatcherField() over GetField("_dispatcher", NonPublic | Static), lines 165-187 ForceDispatcherNull/RestoreDispatcher — the repository idiom P2-T4 mirrors
CITATION: UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs | line 469 typeof(UiThread) and line 471 the field name "_uiSyncContext" — a reflective UiThread consumer examined and excluded, unaffected by P2-T1
CITATION: UtilitiesCS/Threading/WpfUiDispatcher.cs | line 14, and UtilitiesCS/Threading/ThreadMonitor.cs line 25, and UtilitiesCS/Threading/IUiDispatcher.cs line 13, and QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs line 14 — the four <see cref="Dispatcher"/> documentation references that make up the non-reflection remainder of the repository-wide "Dispatcher" literal census
CITATION: docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t11-quickfiler-tests.md | Total tests 1312, Passed 1312, Failed 0, Skipped 0, BASELINE_FAILURE_SET empty — the figure P4-T6's restated acceptance names
CITATION: docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t6-quickfiler-tests.md | EXIT_CODE 1, Total tests 1312, Passed 1304, Failed 8, the eight failing EmailMoveMonitorTests names, and the executed BASE-versus-fixed counterfactual — the fail-before record for P2-T4
CITATION: QuickFiler.Test/QuickFiler.Test.csproj, SVGControl.Test/SVGControl.Test.csproj, Tags.Test/Tags.Test.csproj, TaskMaster.Test/TaskMaster.Test.csproj, TaskTree.Test/TaskTree.Test.csproj, TaskVisualization.Test/TaskVisualization.Test.csproj, ToDoModel.Test/ToDoModel.Test.csproj, UtilitiesCS.Test/UtilitiesCS.Test.csproj, VBFunctions.Test/VBFunctions.Test.csproj | the nine test assemblies of this repository, the pathspec list P0-T14's FIRST command carries (its second and third commands carry the repository-wide `'*.cs'` pathspec instead, the second having been widened to it in revision round 16)
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7
AC-MAPPING: AC1 | IMPLEMENTATION: P2-T1 | TESTS: P1-T2, P1-T4, P3-T2 | EVIDENCE: evidence/regression-testing/p1-t4-expect-fail.md, evidence/regression-testing/p3-t2-regression-green.md
AC-MAPPING: AC2 | IMPLEMENTATION: P2-T1 | TESTS: P2-T2, P4-T4 | EVIDENCE: evidence/qa-gates/p2-t2-nullforgiving-removed.md, evidence/qa-gates/p4-t4-nullable-build.md
AC-MAPPING: AC3 | IMPLEMENTATION: P0-T3 (verification, no edit) | TESTS: P3-T4 | EVIDENCE: evidence/other/p3-t4-progresstrackerasync-unmodified.md
AC-MAPPING: AC4 | IMPLEMENTATION: P1-T5 (attribute-only, no assertion changed), P2-T4 (reflection-target-only, no assertion changed) | TESTS: P3-T3, P3-T6, P4-T6 | EVIDENCE: evidence/qa-gates/p1-t5-donotparallelize.md, evidence/regression-testing/p3-t3-at-risk-tests.md, evidence/regression-testing/p3-t6-quickfiler-wpfuidispatcher.md, evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md, evidence/regression-testing/p4-t6-first-pass-failure.md, evidence/qa-gates/p4-t6-quickfiler-tests.md
AC-MAPPING: AC5 | IMPLEMENTATION: P1-T2, P1-T5, P2-T1, P2-T4 | TESTS: P3-T5 | EVIDENCE: evidence/qa-gates/p3-t5-no-timing-tokens.md, evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md
AC-MAPPING: AC6 | IMPLEMENTATION: P4-T1, P4-T3, P4-T4 | TESTS: P4-T5, P4-T6 | EVIDENCE: evidence/qa-gates/p4-t1-format.md, evidence/qa-gates/p4-t2-format-check.md, evidence/qa-gates/p4-t3-analyzer-build.md, evidence/qa-gates/p4-t4-nullable-build.md, evidence/qa-gates/p4-t5-utilitiescs-tests.md, evidence/qa-gates/p4-t6-quickfiler-tests.md, evidence/qa-gates/p4-t8-loop-closure.md
AC-MAPPING: AC7 | IMPLEMENTATION: P2-T1 | TESTS: P4-T5, P4-T7 | EVIDENCE: evidence/baseline/p0-t10-utilitiescs-tests-coverage.md, evidence/qa-gates/p4-t7-coverage-delta.md
UNRESOLVED-GAPS: NONE

DIRECTIVE: PREFLIGHT VALIDATION ONLY
