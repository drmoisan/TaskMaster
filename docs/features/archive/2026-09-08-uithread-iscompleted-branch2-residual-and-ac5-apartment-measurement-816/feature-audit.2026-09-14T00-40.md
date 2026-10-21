# Feature Audit — Issue #816, UiThread IsCompleted branch-2 residual and AC5 apartment measurement

- Date: 2026-09-14
- Reviewer: feature-review agent
- Work mode: `full-bug` (persisted marker `- Work Mode: full-bug` at `issue.md` line 12)
- Acceptance-criteria sources resolved under that mode: `spec.md` only, plus the one residual criterion in the sibling feature folder that this delivery's AC11 governs

## Scope and Baseline

- Branch: `bug/uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816`
- Head audited: `b4941e25229497b18316023794fbc4a2dfe83159`, working tree clean (`git status --porcelain` empty)
- Base branch: `main`. `git merge-base origin/main HEAD` recomputed in this review returns
  `b63eaa4630d13da46f7ece130bedade53ac39e22`, which is `origin/main` itself, so the two-dot and
  three-dot diffs coincide and the branch is fully up to date with its base.
- Plan anchor: `refs/issue816/base` = `92cf2723451087550cdf019af8c138a4fee9b555`, the merge commit that
  brought `origin/main` onto the branch. The plan-anchored diff is a subset of the branch diff; the
  audit uses the branch diff and reports both.
- Branch diff against the base: 71 paths. Five are code or project files
  (`UtilitiesCS/Threading/UiThread.cs` +15/-2, `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs`
  +164/-0, `UtilitiesCS.Test/Threading/UiThread_Tests.cs` +35/-0,
  `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` +5/-1,
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj` +1/-0). The rest are the two feature folders' documents,
  research, plan and 34 evidence artifacts, plus nine `.claude/agent-memory/` records. No file under
  `.github/`, `artifacts/` or `scripts/` changed.
- Plan of record: `plan.2026-09-12T13-23.md`. I counted the checkboxes directly on disk: 74 items
  matching `^- \[x\]` and 0 matching `^- \[ \]`. The executor's 74/74 claim is correct.
- Coverage baseline: `coverage/p0-t16-baseline.cobertura.xml`, repository-wide 152,996/187,704 lines,
  the changed production file at 121/126. Post-change: `coverage/p4-t11-postchange.cobertura.xml`,
  repository-wide 153,116/187,821, the changed production file at 130/133. Both documents are present
  in the review worktree and were parsed in this review rather than transcribed.
- Test baseline: 6332 passed across the two named assemblies. Post-change: 6336 passed, three times.

## Acceptance Criteria Inventory

Source file 1: `docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/spec.md`,
section `## Acceptance Criteria` (lines 396-540). Fourteen checkbox items, AC1 through AC14. All
fourteen were already marked `- [x]` on arrival at this review.

Source file 2: `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/spec.md`,
section `## Acceptance Criteria` (lines 458-465). Six checkbox items, of which one — AC5 — is governed
by this delivery's AC11. It was marked `- [ ]` at the plan anchor and is marked `- [x]` at head; the
one-line diff changes only the checkbox marker and leaves the criterion text byte-identical.

Total acceptance criteria in scope for this audit: 15.

`user-story.md` is present and was modified by this delivery, but under `full-bug` it is not an
acceptance-criteria source, and the document states of itself that it is "Non-normative ... it carries
no acceptance criteria." No criterion was harvested from it.

## Acceptance Criteria Evaluation

| ID | Verdict | Basis |
|---|---|---|
| AC1 — the 176-179 exit is hardened and nothing else in the accessor changes | PASS | Re-derived, not cited. `git diff refs/issue816/base HEAD -- UtilitiesCS/Threading/UiThread.cs` produces exactly two removed lines (`// The persistent UI context captured at Init() time.` and `if (ReferenceEquals(_context, _uiSyncContext))`) and fifteen added lines, all inside the `_uiSyncContext` exit and its comment. I read the post-change accessor and counted five exits in the same source order: the ambient-identity `true` at 162, the null-ambient `false` at 169, the thread-id `false` at 173, the hardened `true` at 191, and the dispatcher expression at 197-201. The research of record's `## Numeric Derivation Evidence` section (lines 736-757) carries the two independent derivations AC1 requires and both report 5. Neither the ambient-identity exit, the null-ambient exit, the thread-id guard, the dispatcher exit nor `UiThread.Init` is touched. |
| AC2 — recycled-id negative regression test red before, green after | PASS | The test exists at `UiThreadApartmentMeasurement_Tests.cs:31` and installs exactly the four preconditions the criterion names: captured context, captured dispatcher owned by a different (STA host) thread, captured thread id set to the executing MTA thread's own id, and a distinct non-null ambient context. `evidence/regression-testing/p1-t9-ac02-ac03-fail-before.md` records it Failed at exit code 1 with `Expected observed to be False, but found True.`; `evidence/regression-testing/p2-t6-pass-after-projection.md` records it Passed at exit code 0. Both artifacts carry the verbatim command. The commit order corroborates the ordering: `3f228e320 test(816): add failing regression tests` precedes `3936b5428 fix(threading): harden the captured-UI-context exit`. |
| AC3 — null-dispatcher fail-closed negative test passes | PASS | The test exists at `UiThreadApartmentMeasurement_Tests.cs:68` and installs a null captured dispatcher on a dedicated MTA thread that owns none of its own. Recorded Passed in the pass-after projection. I confirmed by reading the post-change predicate that this case is discriminating: with `_dispatcher` null, the `_dispatcher is not null` term short-circuits before the `ReferenceEquals`, so a null-to-null match cannot satisfy the conjunct. The criterion's FAIL condition — an implementation satisfied by a null-to-null reference match — is detected by this test and is not present. |
| AC4 — positive twin passes both before and after | PASS | `IsCompleted_OnTheThreadThatOwnsTheCapturedDispatcherWithTheCapturedUiContext_ReturnsTrue` added at `UiThread_Tests.cs:277` inside `SynchronizationContextAwaiter_Tests`. Recorded Passed in the P1-T8 fail-before run and Passed again in the P2-T5 pass-after run, both stated in `evidence/regression-testing/p2-t6-pass-after-projection.md`. Structurally corroborated: the added terms are conjuncts on a leg where both evaluate true, so the exit cannot change value on this leg. |
| AC5 — the weak retry assertion is tightened and discriminates | PASS | The diff of `UiThreadInitContract_Tests.cs` is exactly the two edits the criterion names: `Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.STA);` added as the first Arrange step, and `.WithMessage(FakeUiCaptureSource.CaptureFailureMessage)` added to the previously unconstrained throw assertion. Both `Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields` and `Init_WhenInitializeThrows_LeavesAllFourCaptureFieldsUnset` are recorded Passed. The projection records the message comparison, and I verified it independently: `NonStaInitMessagePrefix` begins "UiThread.Init() must be called on the UI (STA) thread..." while `CaptureFailureMessage` is "FakeUiCaptureSource was configured to fail during CaptureUiVariables()." The two share no leading substring, so the constraint distinguishes the two exception sources. |
| AC6 — runtime apartment measurement, clause (i) of #809's AC5 | PASS | `SyncContextFormShow_OnAThreadMeasuredAsMta_RecordsTheOutcome` at `UiThreadApartmentMeasurement_Tests.cs:123` runs on a thread created with `SetApartmentState(ApartmentState.MTA)`, reads `Thread.CurrentThread.GetApartmentState()` as the first statement inside the delegate, constructs `SyncContextForm`, calls `Show()`, and disposes in a `finally`. The artifact records `MTA_GUARD_APARTMENT: MTA` and `MTA_INITIALIZE_OUTCOME: COMPLETED`, plus timestamp, command and exit code 0. The guard value is MTA, so the run is not void; the settling line is present in the token form #809's plan fixed. The apartment is read on the executing thread and the artifact states explicitly that it was not derived from a settings file, from the assembly `Parallelize` attribute, or from documented `[STATestClass]` behaviour — which is the precise defect of the earlier probe. The test asserts the guard value, so a non-MTA run would have failed rather than silently recorded. |
| AC7 — measurement leaves nothing behind, needs no host | PASS | I read the method: the form is disposed in a `finally`; `ShowInTaskbar = false` and `WindowState = Minimized` precede `Show()`; the method contains zero occurrences of the token `Dispatcher`, so it creates none and starts no message loop; thread creation and joining are delegated to `ApartmentThreadRunner.RunOnThread`, which sets `IsBackground = true`, starts, and joins unconditionally before returning. `UtilitiesCS.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` is recorded Passed in all three repetitions — the guard is not violated because `SyncContextForm` is compiled into the production `UtilitiesCS` assembly, not the test assembly. `evidence/qa-gates/p4-t1-outlook-precondition.md` records no live Outlook process. No `Sequence_*.xml` blame document exists in any of the three results directories, which is the falsifiable observation that no run stalled. |
| AC8 — both #782 findings recorded, neither changes production | PASS | `evidence/other/p3-t3-issue782-findings.md` records finding 4A (retry-after-failed-initialize already present in production at lines 47-59 of the pre-change file, `_initialized` assigned after `Initialize()` returns inside `lock (InitLock)`) and finding 4B (the retry test's apartment premise unmeasured and its first assertion unable to distinguish the two exception sources), each with supporting line ranges, and states that this delivery acts on 4B only. I verified 4A against the post-change source at `UiThread.cs:51-59` and confirmed the ordering claim holds. The AC1 diff shows `UiThread.Init` unchanged: the removed-line set contains neither `lock (InitLock)` nor `_initialized`. The artifact does not present #782 as a single finding and does not assert a production residual the research contradicts. |
| AC9 — clause (ii) of #809's AC5 discharged or left outstanding | PASS | `evidence/regression-testing/p4-t9-ac09-repetitions.md` records three repetitions over the two explicitly named assemblies, each with the verbatim command, exit code 0, and the per-repetition outcome of `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` as Passed. It states that no `/Settings:` argument was passed by any of the three, and separately that the P4-T11 coverage collection does pass one and is not counted as a repetition. The shell-icon exclusion is stated with the accurate reason (two probe runs each producing one non-deterministic Win32 icon-handle failure) and the artifact explicitly declines to attribute it to the older stall. Every recorded outcome is Passed, so the first branch of the PASS condition is satisfied and no failure attribution is owed. |
| AC10 — coverage of the hardened exit and the repository floor | PASS | `evidence/qa-gates/p4-t12-ac10-coverage.md` records 130/133 = 97.74% for `UtilitiesCS/Threading/UiThread.cs`, above the 80% floor in CLAUDE.md, identifies the hardened exit's lines by content rather than by pre-change line number, and records that no changed line lost coverage. I re-derived all of it from the two Cobertura documents. The three anchor lines carry `hits="1"`; the accessor's `line-rate` rose from 0.90 to 1 and its `branch-rate` from 0.9167 to 1; the new compound condition at line 182 carries `condition-coverage="100% (6/6)"`. The falsifiability of the claim is confirmed at the baseline end: pre-change line 178 (`return true;`) carries `hits="0"` and line 176 carries `condition-coverage="50% (1/2)"`. The post-change uncovered set is exactly {38, 39, 40}; the baseline set was {38, 39, 40, 177, 178}; no line moved from covered to uncovered. The projection states that the delivery adds no new production member, which I confirmed — no type, method, property, field or constructor is added — so the 90% new-member floor has an empty denominator on the production side. No FAIL condition of the criterion is met. |
| AC11 — #809's AC5 checked off only on a complete discharge | PASS | AC6 is PASS and AC9 is PASS, so the criterion requires the checkbox to be checked. The diff of the sibling `spec.md` is a single line in which `- [ ] AC5:` becomes `- [x] AC5:` with the criterion text byte-identical — no other character on the line changed. The artifact the criterion names is committed to the folder the criterion names, at `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/other/ac05-mta-initialize-measurement.md`. The two copies carry identical measured values; `evidence/other/p3-t4-mirror-parity.md` records a `git diff --no-index` producing zero output lines plus equal SHA-256 hashes, and I read both files and confirm they agree. No partial discharge was committed, so no outstanding-clause note was required. |
| AC12 — new file registered and its tests actually ran | PASS | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` gains `<Compile Include="Threading\UiThreadApartmentMeasurement_Tests.cs" />` inserted between the `UiThreadInitContract_Tests.cs` item and the `WpfUiDispatcherTests.cs` item, both named in project-relative form — verified directly in the diff at line 516. The pass-after projection lists all three tests from the new file by fully qualified name among the executed tests, which is the discovery proof the criterion demands. The arithmetic corroborates it independently: 6332 baseline plus four added methods equals the 6336 recorded in all three repetitions and in the coverage run. The P4-T11 artifact explicitly disclaims its own Passed-by-absence inference and defers the discovery proof to the TRX-based list, which is the correct epistemic move. |
| AC13 — file-size limit respected | PASS | I counted all four files directly rather than citing the projection: `UiThread.cs` 306, `UiThread_Tests.cs` 493, `UiThreadInitContract_Tests.cs` 464, `UiThreadApartmentMeasurement_Tests.cs` 164. All four are at or below 500 and all four match `evidence/qa-gates/p4-t13-ac13-file-sizes.md` exactly. The pre-change counts of 293, 458 and 460 stated in the criterion reconcile with the diff line counts (+13, +35, +4). No recorded count exceeds 500. The 7-line headroom on `UiThread_Tests.cs` is recorded as a Low code-review finding for the next change to that file, not as a failure here. |
| AC14 — full four-step toolchain passes in one final pass | PASS, with the evidence basis stated | `evidence/qa-gates/p4-t10-ac14-toolchain.md` records all four commands verbatim in CLAUDE.md order with exit code 0 each, the format check reporting zero `Was not formatted` lines over 1634 files, and four SHA-256 pairs equal before and after the format command, establishing that the last formatting step modified no file and therefore required no restart from step one. For both msbuild commands the artifact records a zero count of the skipped-`CoreCompile` message and, more usefully, a compile-task count of 36, and it states plainly that under `/t:Rebuild` the skipped-compile count is an invariant rather than a condition that can fail. That is the honest framing. I could not re-run any of the four commands: this agent's Bash surface is restricted to `git` by binding caller directive, and the MSBuild and CSharpier logs live under the gitignored `coverage/` directory. What I did verify independently is that the gate is non-vacuous for the changed file — `UtilitiesCS/Threading/UiThread.cs` carries `#nullable enable` at line 1, so the nullable rebuild promotes its `CS86xx` diagnostics to errors — and that the four changed C# files contain zero `#pragma warning disable`, `[ExcludeFromCodeCoverage]` or `SuppressMessage` occurrences, so no gate was satisfied by suppression. |
| Sibling AC5 (2026-09-07 folder, line 464) — MTA measurement artifact and three-repetition record | PASS | Both clauses are discharged. Clause (i): the artifact exists in the folder the criterion names, records the measurement of `new SyncContextForm(); Show();` on a thread measured as MTA on this host, and settles it as `COMPLETED`. Its sub-clause — that the #809 AC2 regression test is justified against the measured result rather than against the #782 narrative — is discharged, though it is discharged across three artifacts rather than stated in the one the criterion names. The measurement establishes that `Show()` on MTA does not throw on this host, which refutes the #782 narrative as a means of inducing the failure; `evidence/other/p3-t3-issue782-findings.md` finding 4B records that the AC2 test instead induces the failure through the `SyncContextFormFactory` seam and that its apartment premise was unmeasured; and the two edits this delivery makes to that test (the STA apartment assertion and the message constraint) convert the premise into a measured assertion and prove the exception comes from the capture failure and not from the apartment rejection. Taken together, the AC2 test now rests on measured facts. Clause (ii): three repetitions with the named test's per-repetition outcome recorded Passed each time, with commands, exit codes, settings-file status and the accurately-stated exclusion reason. |

## Acceptance Criteria Check-off

Per the acceptance-criteria-tracking protocol, a criterion is checked only where the evidence supports
PASS. All fifteen criteria in scope evaluate PASS, and all fifteen were already checked off in their
source files when this review began.

**No criterion was un-checked by this review, and no criterion required checking.** Specifically:

- `spec.md` AC1 through AC14: all fourteen already `- [x]`; all fourteen evaluate PASS; all fourteen
  remain `- [x]`. No file was modified.
- The sibling `2026-09-07` folder's AC5: already `- [x]`; evaluates PASS; remains `- [x]`. No file was
  modified.

I checked specifically for the failure mode this protocol exists to catch — a criterion checked ahead
of its evidence. The highest-risk candidates were AC11 (which is conditional on AC6 and AC9 both being
PASS) and the sibling AC5 (which was checked by this delivery rather than by its own). Both survive:
AC6's guard value is asserted by the test, not merely recorded, so a void run would have failed rather
than passed; AC9's three repetitions each carry a command, an exit code and the named test's outcome;
and the sibling AC5's checkbox diff is a single-character change with the criterion text preserved
byte-for-byte, which is what AC11 requires.

The only criterion where I considered a PARTIAL was the sibling AC5, because its clause (i) sub-clause
about justifying the #809 AC2 regression test is not stated in the measurement artifact the criterion
names. I resolved it to PASS: the substance is present and traceable across the measurement artifact,
the #782 findings artifact and the two test edits, and this feature's AC6 and AC11 — which are the
governing scoping instruments and were authored before the work — define the discharge condition as the
guard plus settling lines. A reader wanting the justification in one place would have to follow three
citations, which is an evidence-presentation weakness rather than a missing outcome. It is recorded in
the policy audit as an informational observation, not as a gap.

### Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/spec.md
- Total AC items: 14
- Checked off (delivered): 14
- Remaining (unchecked): 0
- Items remaining: none

- Source: docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/spec.md
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0
- Items remaining: none
  (Of these six, only AC5 is governed by this delivery; AC1-AC4 and AC6 were discharged by the
  2026-09-07 delivery and were already checked at the plan anchor.)
```

## Summary

**PASS. No blocking findings.**

Fifteen acceptance criteria were in scope: AC1 through AC14 in this feature's `spec.md` under the
`full-bug` work mode, and the residual AC5 in the sibling 2026-09-07 feature folder that this
delivery's AC11 governs. All fifteen evaluate PASS on the evidence, and all fifteen are correctly
checked off. None should have been left unchecked and none was checked ahead of its evidence.

The delivery does what it says. The production change is a strict narrowing of one predicate, confined
to two removed lines and fifteen added lines in one file, proved red-then-green by two negative tests
and held invariant on the out-of-scope leg by a positive twin. The apartment measurement that issue
#809 left unmeasured was taken on a thread whose apartment was read at runtime rather than inferred,
which is the precise defect of the earlier probe, and it settled as `COMPLETED`. The three-repetition
record closes the second clause. Coverage of the changed file rose from 96.03% to 97.74%, the changed
accessor reached line-rate 1 and branch-rate 1, and the previously dead `return true;` is now live —
all of which I re-derived from the two Cobertura documents rather than accepting on report.

Two non-blocking items are carried forward, neither of which is a defect of this delivery:

1. A latent null-to-null residual in the sibling dispatcher exit at `UiThread.cs:197-201`, pre-existing
   and explicitly fenced off by AC1, recommended for promotion to its own follow-up issue. It is the
   same defect shape this item just closed one exit above it.
2. The evidence-strength limitation that the TRX documents, MSBuild logs and CSharpier logs are
   gitignored, so the four toolchain results and the per-test outcomes are executor-attested. That is
   the ratified convention for this repository. The two Cobertura documents happened to survive in the
   review worktree and were parsed directly, so the coverage half of the evidence base is independently
   confirmed.

Remediation inputs are not produced. No finding requires a code or test change before merge.
