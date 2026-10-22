# Feature Audit — efc-archiveroot-boundary-sink-defects (Issue #736)

- Date: 2026-09-04
- Work Mode: `full-bug` → **AC source is `spec.md` only** (`user-story.md` is correctly absent; `issue.md` checkboxes are not AC for this mode)
- Branch: `bug/efc-archiveroot-boundary-sink-defects-736`, HEAD `54da9e4d`
- Baseline: `origin/main` `66749143`, which is also the merge base
- AC count: 13 (AC1–AC13), all currently `[x]` in `spec.md`

## Verdict

**11 PASS, 2 PARTIAL, 0 FAIL, 0 UNVERIFIED. No blocking defect.**

Two acceptance criteria are marked `[x]` in `spec.md` that this review does not evaluate as fully
met: **AC11** and **AC12**. Both are recorded as PARTIAL below with the specific unmet conjunct
named. Neither is a functional defect; both are accounting gaps between the AC's literal text and
the delivered state.

Per the `acceptance-criteria-tracking` skill, an AC evaluated PARTIAL should not carry a check. Both
boxes were checked by the delivering agent before this review. This review **does not endorse those
two check-offs** and has deliberately left `spec.md` unmodified rather than flipping another agent's
recorded state; the non-endorsement is stated here and in the summary block so the orchestrator can
decide whether to uncheck them.

## AC Evaluation Table

| AC | Subject | Verdict | Basis |
|---|---|---|---|
| AC1 | Finding 1 — guarded seam exists, getter delegates, size ceiling, project registration | **PASS** | All four conjuncts verified against source, not against the evidence artifact. `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` exists (95 lines) declaring both `internal string ResolveValidatedArchiveRootPath()` and `internal static string ResolveValidatedArchiveRootPath(Func<string>, Func<string>, Action<string>)`. `AppOlObjects.cs:266` is exactly `_archiveRootPath = ResolveValidatedArchiveRootPath();`. `AppOlObjects.cs` measures **493** lines, under 500. `TaskMaster.csproj` gained exactly one `<Compile Include="AppGlobals\AppOlObjects.ArchiveRoot.cs" />`. |
| AC2 | Finding 1 — COM normalization contract | **PASS** | Verified by reading the implementation. Both reads are inside one `try`; `catch (COMException comFailure)` throws `new InvalidOperationException(ArchiveRootPathGuard.UnresolvableRule, comFailure)`, preserving the instance. The message constant was read in full and contains no path and no address. The XML-doc conjunct is present verbatim at `AppOlObjects.cs:254`. The no-cache conjunct holds structurally — `_archiveRootPath` is assigned only on success — and is pinned by `..._WhenComReadFailsTwice_ReReadsTheComposedPathOnTheSecondCall` asserting `composedReads == 2`. Red/green pair recorded: `p1-t7` 6/2/4 → `p1-t9` 6/6/0. See code review CR-3 for a non-blocking note on the constant's wording. |
| AC3 | Invariant stated and traced; implementation matches the trace | **PASS** | The spec's Boundaries section states the invariant and the four-step trace. Step 4's named boundary is delivered: both `KbdExecuteAsync` overloads route through `RunKbdGuardedAsync`, whose general catch calls `TryReportBoundaryFault`, so a fault at `EfcFormController.cs:863`/`:873` is caught there rather than travelling to the `[ExcludeFromCodeCoverage]` `KeyboardHandler.KeyboardHandler_KeyDownAsync`. Red/green: `p2-t8` 6/0/6 → `p2-t10` 6/6/0. |
| AC4 | Finding 2 — keyboard boundary, exactly 2 overloads, both covered, cancellation not a fault | **PASS** | Both overloads verified in source as delegating to `RunKbdGuardedAsync`; the count is still 2. `catch (OperationCanceledException)` logs at debug and does not invoke the sink; `catch (System.Exception ex)` calls `TryReportBoundaryFault` once. Both classification arms are pinned by dedicated tests (`..._WhenBodyThrowsOperationCanceled_DoesNotReportAsFault` asserting `sinkCallCount == 0`, `..._WhenBodyThrowsInvalidOperation_ReportsExactlyOnce` asserting `== 1`). Both overloads carry a fault-path **and** a success-path test. |
| AC5 | Finding 4 — user-facing sink default, non-blocking, no modal, null/throwing sink branches covered | **PASS** | `DefaultBoundaryErrorSink` logs and then calls `UserFaultNotifier?.Invoke(message)`. The default notifier is `ShowModelessFaultNotice`, which calls `Show()` — modeless — and never `ShowDialog()`. Non-blocking is pinned by a `Stopwatch` bound rather than a sleep. The null-sink and throwing-sink branches of `TryReportBoundaryFault` are covered by `KbdExecuteAsync_WhenBoundaryErrorSinkIsNull_DoesNotThrow` and `KbdExecuteAsync_WhenBoundaryErrorSinkThrows_DoesNotThrow`. Red/green: `p4-t4` 3/2/1 → `p4-t6` 3/3/0. See code review CR-1 and CR-2 for two non-blocking notes on this surface. |
| AC6 | Finding 5 — all five `ArchiveRootPath` reads covered by a reporting boundary | **PASS** | The three mappings were checked: 556/566 remain under the pre-existing `ButtonCreateClickAsync` catch, which this change does not touch; 863/873 are now under `RunKbdGuardedAsync`; 1014 is under `BindBreadcrumbRowsAsync`'s general catch, which was rerouted from `logger.Error(...)` to `TryReportBoundaryFault(...)` — the single-line change at `EfcFormController.cs:1126` in the diff. The `catch (OperationCanceledException)` arm above it is byte-identical (no diff hunk touches it). Red/green: `p3-t2` → `p3-t4` 2/2/0. |
| AC7 | Finding 6 — filer seam, override, deliberate stop, comment rewritten | **PASS** | `protected internal virtual Task<bool> InvokeFilerAsync(EmailFilerConfig, IList<MailItemHelper>)` exists at `EfcDataModel.cs:355`; `TestableEfcDataModel` overrides it returning `Task.FromResult(true)`. The `await act.Should().ThrowAsync<NullReferenceException>()` line is gone, replaced by a plain `await MoveAsync(dataModel);` retaining only `olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Once())`. The summary comment no longer describes the crash as the barrier. Red/green: `p5-t2` 1/0/1 (failure naming `NullReferenceException`) → `p5-t5` 11/11/0. |
| AC8 | Finding 6 — coverage preservation at `EfcDataModel.cs:339`, all 11 methods pass | **PASS** | Independently re-verified against the post-change Cobertura rather than accepted: `QuickFiler\Controllers\EfcDataModel.cs` line **339** carries `hits=1`. All 11 methods in the class pass (`p5-t5` 11/11/0), and this review counted 11 `[TestMethod]` attributes in the file. A stronger result than the AC requires also holds: the seam newly covered `SortEmail.Cleanup_Files();` and `return result;`, which were `hits=0` at baseline. |
| AC9 | Frozen contracts hold | **PASS** | Verified directly, not via the evidence artifact. `ArchiveRootPathGuard.cs`, `AppOlObjectsArchiveRootValidationTests.cs`, and `IOlObjects.cs` appear in **no** hunk of the 98-path branch diff. `EfcDataModel.cs` contains exactly one `catch (InvalidOperationException ex)` (line 287) and **zero** `catch (COMException` — the prohibited widening did not occur. `AppOlObjectsArchiveRootValidationTests` 6/6/0 and the COM-propagation test passing are recorded in `p6-t11`. |
| AC10 | Regression-first evidence for findings 1, 2, 4, 5, 6; tests listed in the delivery report | **PASS** | Each of the five findings has a recorded failing run against the defect-preserving state and a recorded passing run after. The three P6-T13 success-path tests have green-only records; that is structurally correct, not an omission — P6-T13 changed no production code, so a fail-before run is impossible, the artifact records `WhyFailingRunImpossible:`, and no AC is discharged by a fail-before observation on those three. |
| AC11 | Scope containment | **PARTIAL** | Conjuncts 1–3 **PASS**, conjunct 4 not met on the full branch diff. See § AC11 below. |
| AC12 | Coverage targets | **PARTIAL** | The literal ">= 90%" conjunct is not met at 88.14%. See § AC12 below — adjudicated independently, not deferred to the discharge note. |
| AC13 | Full toolchain pass, both msbuild steps non-vacuous, logs retained | **PASS** | Four of five conjuncts independently re-derived by this review from the primary logs. The retention conjunct is met literally but weakly — see § AC13. |

## § AC11 — why PARTIAL

AC11 has four conjuncts. Three are met and were verified directly:

1. **No change to `ActionOkAsync` or any disposal ordering.** PASS. `ActionOkAsync` begins at
   `EfcFormController.cs:838`. The four hunks in that file are at 129, 170, 994–1034, and 1126; none
   intersects it. The file's single added `.Dispose()` is
   `notice.FormClosed += (sender, args) => notice.Dispose();` inside `ShowModelessFaultNotice` — a
   self-disposing notification form, not a disposal-ordering change. Finding 3 is untouched.
2. **No change to any binding-scope-excluded file.** PASS.
3. **The EFC controller file is not split.** PASS — it remains one file of 1320 lines.

Conjunct 4 — *"the delivered diff touches only the files enumerated in the Write Set section"* — is
not met on the full branch diff. The 98 changed paths decompose as:

| Group | Count | Covered by the Write Set section? |
|---|---|---|
| Ratified eleven-path Write Set | 11 | Yes, enumerated |
| Feature-folder documents and evidence | 71 | Yes — the Write Set section explicitly excludes "this planning document and the research and evidence artifacts under this feature folder" |
| `.claude/agent-memory/**` | 16 | **No** — covered by neither the enumeration nor the exclusion clause |

The sixteen agent-memory paths are real entries in `origin/main...HEAD`. They become invisible only
under the plan's D11 pathspec, which carries `":(exclude).claude/**"`. The audit scope for this
review is the full branch diff, not the D11 pathspec, so the narrowing was not adopted.

**Assessment of severity: low, and the disclosure is good.** All sixteen are agent-memory markdown,
written by this run's task-researcher, prd-feature, atomic-planner, and orchestrator, and committed
to the branch *before* Phase 0 began. None is production code, none is a test, none carries an
unredacted host token. `evidence/other/p7-t2-commit.md` enumerates all sixteen by name and states
plainly that each is outside the ratified Write Set and invisible to the AC11 gate — including the
`Difference: none` derivation proving the delivery commit's own `git add -A` contributed no
agent-memory path of its own. That is exactly the disclosure this review would want.

**What would close it:** amend the spec's Write Set exclusion clause to name `.claude/agent-memory/**`
alongside the feature-folder carve-out it already contains, so AC11's fourth conjunct is literally
true of the branch. This is a one-sentence documentation change, not a code change.

## § AC12 — independent adjudication

The caller asked that this be adjudicated independently rather than by deferring to the executor's
discharge note. It was: the changed-line set was rebuilt from `git diff -U0 origin/main...HEAD` and
joined to the per-line `hits` in the post-change Cobertura, whose SHA-256 this review computed and
matched to the value recorded in `p6-t6-coverage.md`, and which `coverage/p6-t6-run.log` names as
the output of the 7013/7013 run.

### What was verified

| Quantity | Executor | This review | Agreement |
|---|---|---|---|
| Changed coverable lines | 59 | **59** | Yes |
| Covered | 52 | **52** | Yes |
| Strict aggregate | 88.14% | **88.1356%** | Yes |
| Uncovered changed coverable lines **outside** `U` | 0 | **0** | Yes |
| New file, strict | 18/21 = 85.71% | **18/21 = 85.71%** | Yes |
| New file, after removing `L` = {89, 90, 91} | 18/18 = 100.00% | **18/18 = 100.00%** | Yes |
| Repo line / branch | 85.46% / 79.52% | **85.459% / 79.5242%** | Yes |

The uncovered set is exactly `{ AppOlObjects.cs:266; EfcDataModel.cs:359, 360, 361;
AppOlObjects.ArchiveRoot.cs:89, 90, 91 }` — seven members, identical to the `U` declared in advance
by D2, with **zero** members outside it.

**The escape's stated precondition is therefore satisfied, and this review confirms it. The escape
was not used to launder an uncovered reachable line.** That is the failure mode this review exists
to catch, and it is not present.

The calibration note about the attribute is also confirmed as measured behaviour. In the Cobertura,
the wrapper's own lines 87, 88, 92, and 93 carry **no `<line>` node at all** — `[ExcludeFromCodeCoverage]`
removed them — while 89, 90, and 91 do carry nodes and read `hits="0"`. The three lambdas capture
`this` and are lifted into instance members of `AppOlObjects`, so the attribute on the enclosing
method does not reach them.

### Why the verdict is nonetheless PARTIAL

**1. The literal conjunct is not met.** AC12's first clause reads *"New and changed code reaches at
least 90% line coverage."* 88.14% is not at least 90%. The escape explains *why* it cannot be
reached — `10U` = 70 exceeds the strict denominator of 59, so no amount of testing moves the
quotient to 90% — but explaining an unreachable threshold is not the same as meeting it. The AC's
text was not amended, so the criterion is unmet as written.

**2. Under the AC's own qualifier, two new-code items still fall short.** The conjunct is qualified
"per CLAUDE.md General Unit Test Policy UT2," and that qualifier is load-bearing — UT2 defines no
"changed-line aggregate" metric at all. UT2's two applicable rules are:

- *"Any new modules, classes, or methods added must target >= 90% coverage."* The new module
  `AppOlObjects.ArchiveRoot.cs` is **85.71%** strict. The new method
  `EfcDataModel.InvokeFilerAsync` is **0/3 = 0.00%**.
- *"Code changes or refactors must not reduce coverage for the lines that were changed."* This holds:
  no changed line carrying `hits > 0` at baseline carries `hits = 0` now.

So the qualified reading does not rescue the AC either; it relocates the shortfall from an aggregate
to two specific new-code items.

**3. One characterization in the discharge note is overstated, and correcting it matters.** `U` is
described as an "unreachable set." Three of its seven members are not host-unreachable:
`EfcDataModel.cs:359–361` is the body of the new `InvokeFilerAsync`, i.e. `{`,
`return new EmailFiler(config).SortAsync(mailHelpers);`, `}`. This review checked the baseline
document: the pre-change equivalents `EfcDataModel.cs:343` (`var sorter = new EmailFiler(config);`)
and `:344` (`var result = await sorter.SortAsync(mailHelpers);`) both carry **`hits="1"`** in
`coverage/p0-t6-baseline.cobertura.xml`. Production code that a test *did* execute is now executed by
no test. That is a chosen non-execution, not an environmental impossibility, and calling it
"unreachable" conflates the two. The executor's own AC12 note states "every reachable changed line
is covered," which is not accurate for those three lines.

### Why it is not Blocking

The overstatement does not conceal a real gap, and the substance is defensible:

- The three lines are a **zero-branch, zero-logic delegation** to a collaborator plus its braces.
  There is no decision, no state, and no invariant in them to test.
- Their prior "coverage" was an artifact of the incidental `NullReferenceException` that finding 6
  exists to eliminate. The call never completed, and no test asserted anything about it. Trading a
  hit-via-crash for a clean seam is the improvement the item was commissioned to make.
- The **same edit newly covered two lines** the crash had prevented reaching:
  `SortEmail.Cleanup_Files();` and `return result;` moved from `hits=0` to `hits=1`. The file's
  covered-line count rose 188 → 189.
- The shape is what `.claude/rules/general-unit-test.md` prescribes for host-bound code: *"extract
  all logic into host-neutral, testable modules and leave only the thinnest possible wiring in the
  host-bound entry point."* `InvokeFilerAsync` is that wiring.
- The new file's 3-line shortfall is genuinely irreducible: all three are Outlook COM crossings or a
  logger delegate literal, inside a member that cannot execute without a live Outlook process.
- Repo-wide line coverage **rose**, 85.4332% → 85.459%, clearing both the 85% floor in
  `.claude/rules/general-unit-test.md` and the 80% floor in CLAUDE.md. Branch coverage sits at
  79.5242% against a 75% floor.
- AC12's remaining three conjuncts — no regression on changed lines, repo figure recorded, and an
  explicit statement of whether the change lowers it — are all met.

**Verdict: PARTIAL. The escape is properly invoked on its stated precondition, which this review
independently confirmed; the AC's literal 90% conjunct is not met and one new method sits at 0%;
this review does not endorse the `[x]`.** No remediation cycle is recommended, because both
shortfalls are irreducible within this item's ratified scope.

## § AC13 — note on the retention conjunct

AC13 is PASS. Four of its five conjuncts were independently re-derived by this review from the
primary logs rather than read from the summary artifacts: `Build succeeded` with 0 Warning(s) and
0 Error(s) on both msbuild steps, `Skipping target "CoreCompile"` = 0 and `Task "Csc"` = 18 on both,
and `Test Run Successful.` with 7013/7013. Log mtimes corroborate the mandated ordering
(analyzer 05:35:01Z → nullable 05:36:22Z → coverage 05:38:33Z → delivery commit 05:48:31Z), and
`p6-t6-coverage.md` correctly identifies itself as the **second** execution superseding a document
made stale by the P6-T13 restart.

The fifth conjunct, *"with the logs retained as evidence,"* is met literally — two `.min.log.txt`
files are tracked and entered the delivery commit — but weakly. This review read both: each is 19
lines of project→DLL mappings, and **neither contains a single occurrence of
`Skipping target "CoreCompile"` or `Task "Csc"`**. They cannot corroborate the counts AC13 turns on.
The files that can are the two 10.6 MB detailed logs under the gitignored `coverage/` directory, and
they will not survive the merge. Recorded as non-blocking finding F-5 in the policy audit; not
grounds for downgrading AC13, since the conjunct as written asks for retention and retention exists.

## Baseline comparison

| Dimension | Baseline (`66749143`) | HEAD (`54da9e4d`) | Direction |
|---|---|---|---|
| Tests | 6995 | 7013 | +18, exactly the methods this item adds |
| Failures | 0 | 0 | unchanged |
| Repo line coverage | 85.4332% | 85.459% | improved |
| Repo branch coverage | 79.5348% | 79.5242% | −0.0106 pt, 4.5 pt clear of the floor |
| `EfcFormController.cs` coverage | 25.69% | 30.57% | improved |
| `AppOlObjects.cs` coverage | 29.58% | 30.08% | improved |
| `EfcDataModel.cs` coverage | 66.20% | 66.08% | −0.12 pt; +1 covered line, +2 coverable |
| `EfcFormController.cs` size | 1216 | 1320 | +104, within the D7 budget of 1330, still over the 500 ceiling |
| Analyzer warnings | 0 | 0 | unchanged |
| Nullable warnings | 0 | 0 | unchanged |

## Newly checked-off items

None. All 13 boxes were already `[x]` when this review began. This review checked off nothing and
unchecked nothing; `spec.md` is unmodified by this review. AC11 and AC12 carry checks that this
review does not endorse, per the reasoning above.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/spec.md`
- Total AC items: 13
- Checked off (delivered): 13
- Remaining (unchecked): 0
- Items remaining: none
- **Reviewer evaluation differs from the checkbox state on 2 items.** Evaluated PASS: 11 (AC1–AC10, AC13). Evaluated PARTIAL: 2 — **AC11** (scope containment: 16 `.claude/agent-memory/**` paths in the branch diff are outside both the ratified Write Set and the Write Set section's own exclusion clause; disclosed in full in `evidence/other/p7-t2-commit.md`) and **AC12** (coverage targets: strict changed-line aggregate is 88.14% against a literal ">= 90%", and the new method `EfcDataModel.InvokeFilerAsync` is at 0.00%; the D2 escape's precondition was independently verified as satisfied, with 0 uncovered changed lines outside `U`). Evaluated FAIL: 0. Evaluated UNVERIFIED: 0.
