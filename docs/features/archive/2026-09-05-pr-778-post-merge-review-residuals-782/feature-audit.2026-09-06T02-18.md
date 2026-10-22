# Feature Audit — Issue #782 (pr-778-post-merge-review-residuals)

- **Date:** 2026-09-06
- **Reviewer:** feature-review agent (re-audit, cycle 2)
- **Companion artifacts:** `policy-audit.2026-09-06T02-18.md`, `code-review.2026-09-06T02-18.md`, `remediation-inputs.2026-09-06T02-18.md`

## Scope and Baseline

| Item | Value |
|---|---|
| Base branch | `main` |
| Base commit (recomputed with `git merge-base HEAD origin/main`) | `77c6d31404e2bc2291aec7eb9561e393c20cdcae` |
| Head | `refactor/pr-778-post-merge-review-residuals-782` @ `e053a4f2305502adb09afe6bcc9a26351804f6fe` |
| Diff form agreement | two-dot and three-dot `--name-only` outputs are byte-identical, 126 paths each |
| Commits on branch | 25 |
| Work mode marker | `issue.md:10` -> `- Work Mode: full-feature` |
| Resolved AC sources | `spec.md` and `user-story.md` |
| PR context freshness | `Head SHA: e053a4f2305502adb09afe6bcc9a26351804f6fe` equals `git rev-parse HEAD`; not stale |
| Working tree | clean before and after this review |

The baseline for acceptance-criteria verification is the tree at `77c6d314`. Every "before" figure in
this audit was obtained from that commit with `git show <base>:<path>` or from
`coverage/782-p0-baseline.cobertura.xml`, and every "after" figure from the working tree at head or
from `coverage/782-r1-final.cobertura.xml`.

### Delta since cycle 1

Cycle 1 audited head `4ed2f790`. Three further commits are under audit for the first time:

| Commit | Subject |
|---|---|
| `b91dd859` | `fix(782): correct the message-pinning claim and the baseline coverage input record` |
| `7d67a7ab` | `docs(782): record remediation closure evidence` |
| `e053a4f2` | `docs(782): record remediation plan completion state` |

Their combined `.cs` footprint is 6 lines across 2 test files. No acceptance criterion transitioned in
either direction: `spec.md` was 12 of 12 before the remediation and is 12 of 12 after, and
`user-story.md` was 4 of 5 before and is 4 of 5 after. AC10 and AC11 had their supporting prose
corrected, not their state, which this reviewer confirmed by diffing the checkbox lines across
`e01cf434..HEAD` — both were `- [x]` on both sides.

## Acceptance Criteria Inventory

Counted from the `## Acceptance Criteria` section of each source file, terminating at the next
equal-or-shallower heading.

| Source | Total | Checked `[x]` | Unchecked `[ ]` |
|---|---|---|---|
| `spec.md` | 12 | 12 | 0 |
| `user-story.md` | 5 | 4 | 1 |
| **Combined** | **17** | **16** | **1** |

The single unchecked item is AC-U1, which requires a pull request that does not yet exist. Leaving it
unchecked is correct.

## Acceptance Criteria Evaluation

### Source: `spec.md`

| AC | Verdict | Verification performed by this reviewer |
|---|---|---|
| AC1 | PASS | All seven Should-fix findings verified individually. C10: `StaDispatcherHost` in `UiThread_Tests.cs:186` sets `IsBackground`, calls `SetApartmentState(ApartmentState.STA)`, and shuts down via `Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send)` on the disposal path, with the populated-branch test retained at line 158. C02: the getter reads `_dispatcher` once into `Dispatcher? captured` and returns the local. C18: `EmailMoveMonitorTests` reads `UiThreadDispatcherFixture.Current` and no longer holds a local `FieldInfo`. C19: the three P27-T2 passages in `IdleAsyncQueue_Tests.cs` are rewritten. C20: both throw sites route through the shared constant; grep for `before yielding folder tree work` over `UtilitiesCS` returns zero. C16: split verified below under AC6. S3-2: both formatter command cells in the #584 folder are corrected to the scoped six-path form, policy-audit row 3.1 is amended, and a section 8 gap entry is added — read directly from the #584 diff. |
| AC2 | PASS | Fourteen identifiers accounted for. C03's omission is discharged through the omission branch and is documented in `evidence/other/code-review.2026-09-05T23-00.md` section (a), which records the omission, the measured regression, the bisect to the single `_loaded = new ThreadSafeSingleShotGuard();` line, and the promotion to a follow-up. The delivery code-review carries nine such disposition sections, (a) through (i). |
| AC3 | PASS | Eight documentation and evidence nits verified in the #584 folder. 23 files changed there: 4 documentation, 19 evidence, matching the Write Set counts exactly. Falsifiable sub-claim checked: every `EXIT_CODE:` line in the #584 evidence tree now matches `^EXIT_CODE: [0-9]+$` — a grep for lines failing that pattern returns zero. The S3-1 softenings are visible in the #584 policy-audit diff, for example row 2.15 losing the evaluative span "This is a provable assertion-level RED-first" in favour of a statement that the two artifacts' `Timestamp:` values do not establish execution order. |
| AC4 | PASS | `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` contains no `dispatcher != null` comparison; both guards removed at the lines the Write Set names. `ProgressTracker.cs:39` and `ProgressTrackerAsync.cs:39` each pass the captured `UiDispatcher` property rather than re-reading `UiThread.Dispatcher`; the value is identical because line 33 assigned it from that static one statement earlier. |
| AC5 | PASS | A grep over every `*.cs` file in the repository for the single-line token `"_dispatcher"` returns **exactly two** hits: `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs:117` and the unchanged `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136`. `UtilitiesCS.Test` therefore holds exactly one acquisition. `EmailMoveMonitorTests.cs` holds no `FieldInfo`. The AC's own note on why `GetField("_dispatcher"` is not a usable search conjunction — CSharpier wraps the call so the two tokens never share a line — was confirmed correct. The restore-to-null assertion is present at `UiThread_Tests.cs:157-162`, which installs `expected` over a null prior inside an outer `InstallNull` scope. |
| AC6 | PASS | `ProgressTracker_Tests.cs` is 271 lines (was 514) and `ProgressTracker_ReportAndViewerTests.cs` is 288; both strictly under 500. Each has exactly one `<Compile Include>` entry, at csproj lines 478 and 479. Both declare `public partial class ProgressTracker_Tests` in namespace `UtilitiesCS.Test`; `[TestClass]` and `[DoNotParallelize]` appear on separate lines in exactly one part (lines 14 and 15). Name preservation verified set-wise: extracting `[TestMethod]`-adjacent method names from the base file yields 21 names, from the union of the two head files yields 22, and the set difference of base minus head is **empty**, with the single addition being `Initialize_WhenDispatcherNotCaptured_ThrowsInvalidOperationException`. Every fully-qualified name is therefore preserved. |
| AC7 | PASS | All three tests exist and are named in the evidence. RED-first is recorded twice over: `evidence/regression-testing/p4-t7-fail-before.md` records `EXIT_CODE: 1` with both throws removed together — removing only one would leave the sibling guard throwing the same type with the same constant and make the demonstration vacuous, which the artifact states — and `p4-t8-pass-after.md` records `EXIT_CODE: 0` after `git checkout HEAD --` restore. The R3 falsification adds a third: TRX `TestResults/782-r1-p1t7` read directly by this reviewer records `outcome="Failed"`, 2 total, 1 passed, 1 failed. |
| AC8 | PASS | Two promoted entries exist with real issue numbers: `docs/features/potential/promoted/2026-09-05-uithread-init-accepts-non-sta-callers.md` (Issue #787, URL present) and `...-uithread-init-latch-not-rearmed-after-failed-initialize.md` (Issue #788, URL present). The upstream follow-up record is at `evidence/other/upstream-followups-drm-copilot.2026-09-05T23-02.md`. `git diff --name-only 77c6d314..HEAD -- .claude` returns zero paths, satisfying the AC's own evidence clause. |
| AC9 | PASS | Three of the four toolchain steps were re-executed by this reviewer at the current head and all exited 0: `csharpier check` (`Checked 1583 files`), the analyzer `msbuild /t:Rebuild` (`0 Warning(s) 0 Error(s)`), and the nullable `msbuild /t:Rebuild` (19 projects recompiled, no diagnostic). The fourth, the coverage-bearing test run, was verified from its committed TRX: 7000/7000/0. `evidence/qa-gates/r-p4-t7-loop-closure.md` records `PASS NUMBER: 1`. The package-level summary is committed at `evidence/qa-gates/coverage-summary.2026-09-05T23-11.md`. Changed-line coverage does not decrease: all seven changed executable production lines are covered at head, and no measurable file lost a covered line. `artifacts/csharp/coverage.xml` is not produced, as the AC itself states under SD1. |
| AC10 | PASS | `UiThread.cs:135-136` declares exactly one `internal const string DispatcherNotInitializedMessage`, referenced on two lines in that file (declaration and throw) and once in `WpfDispatcherYield.cs`. Zero occurrences of `before yielding folder tree work` and zero of `UiThread.Initialize()` remain in `UtilitiesCS`. The corrected claim is the one this reviewer required and it is now backed by observation: `evidence/regression-testing/r-p1-t7-fail-before.md` records the FluentAssertions failure verbatim, showing the expected value as the constant's whole text and the actual as that text plus the mutation's tail. The artifact also correctly labels its one derived leg — that the old wildcard would not have failed — as derived rather than observed. |
| AC11 | PASS | The method name `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` is unchanged; the assertion is `WithMessage(UiThread.DispatcherNotInitializedMessage)` at `UiThread_Tests.cs:144`. `evidence/qa-gates/r-p1-t10-assertion-token-gate.md` records the inversion 0 -> 2 for the constant form and 2 -> 0 for the wildcard form, scoped to the two files, using `-SimpleMatch` so the asterisks and parentheses are literal. The SD4 retention reason is recorded in the delivery code-review section (c). |
| AC12 | PASS | Both re-derivation artifacts exist and are non-empty: `evidence/baseline/p0-t9-584-spec-rederivation.md` (2,492 bytes) and `evidence/baseline/p0-t10-584-plan-rederivation.md` (3,197 bytes). |

### Source: `user-story.md`

| AC | Verdict | Verification performed by this reviewer |
|---|---|---|
| AC-U1 | NOT YET MET, correctly unchecked | No pull request exists. This is the one criterion whose satisfaction is external to the branch. It must be satisfied by a PR body that maps every finding identifier to its file or to its recorded omission reason, and that closes **#782 only**. |
| AC-U2 | PASS with a noted staleness | The proposition holds: the only production behavior deltas on the branch are the exception message text and the single-read capture in `UiThread.Dispatcher`, which closes a torn-read window. `RibbonViewer`'s guard removal is behavior-preserving because the getter already threw on the base commit; `ProgressTracker` and `ProgressTrackerAsync` read the same value from a local instead of re-reading the static. The AC also names "the retry-after-failed-initialization behavior of `UiThread.Init()`", which was withdrawn under SD18 and is not delivered; `Init()` is byte-identical to `pre-782-base`. Because the AC is phrased as an upper bound, delivering strictly less than it permits does not falsify it, and `spec.md`'s Behavioral Contract records the withdrawal in full. Recorded as non-blocking finding N3. |
| AC-U3 | PASS | Every #584 review finding is resolved, promoted, recorded as an upstream follow-up, or recorded as needing no action. The delivery code-review carries nine explicit disposition sections; the C-identifier and S-identifier disposition tables account for all twenty-six. Two promotions carry live issue numbers (#787, #788) and two push-down-owned items are recorded for drm-copilot. |
| AC-U4 | PASS | Spot-checked by re-deriving rather than by reading. The #584 formatter command cells now match the command actually run. The `EXIT_CODE:` normalization is verified by grep across the whole #584 evidence tree. The claim that this delivery's own figures are verifiable was tested end to end: this reviewer reproduced the coverage counters, the baseline document's figures, the TRX counters, and the toolchain exit codes from primary sources without relying on any prose summary. |
| AC-U5 | PASS | The toolchain passes in a single pass, independently confirmed for three of four gates. Changed-line coverage does not decrease. The `UiThread.cs` percentage moved from 77.11% to 76.83%, but the uncovered line set is identical in membership and line number on both sides — 19 lines, `28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120` — so no changed line regressed. The movement is arithmetic from a covered three-line wrapped `throw` collapsing to one line. |

## Summary

**Feature verdict: PASS.**

| Metric | Value |
|---|---|
| Acceptance criteria evaluated | 17 |
| PASS | 16 |
| NOT YET MET (external dependency) | 1 (AC-U1, pending the pull request) |
| PARTIAL | 0 |
| FAIL | 0 |
| UNVERIFIED | 0 |
| Failing for a reason attributable to the delivery | 0 |

No acceptance criterion is UNVERIFIED. Every criterion was checked against a primary source: the
working tree, the base commit, raw Cobertura XML, committed TRX documents, or a command this reviewer
executed.

### What the remediation changed, assessed

R3 and R4 were both accuracy defects in the delivery's own artifacts, and both are now fixed correctly.
The R3 fix is notable for what it declines to claim. A constant-reference assertion pins the throw
site's use of the constant, not the constant's text, and the delivery says so in three places rather
than letting the stronger reading stand. It then locates the one assertion that does hold the literal
and cites it by file and line. The preflight rounds that caught two successive false framings of this
same claim did real work; the second correction, refuted by `WpfDispatcherYieldTests.cs:196`, is
exactly the kind of near-miss that ships silently in most deliveries.

The R4 fix records both baseline collections with their own inputs and figures instead of choosing one
and discarding the other, states which is authoritative and why, and marks the authoritative
collection's output document as not retained. It further declines to assert a mechanism for that
document's absence, on the stated grounds that no record supports one. That restraint is correct and
is the harder choice.

### Residuals carried forward

None blocks. Three non-blocking accuracy nits are raised in
`remediation-inputs.2026-09-06T02-18.md`: N1 (absolute host paths in two committed artifacts, which
also corrects this reviewer's cycle-1 row 2.11), N2 (a disposition record titled as a maintainer
disposition with no maintainer ratification on record), and N3 (AC-U2's stale reference to the
withdrawn C03 behavior). R1 and R2 recur unchanged from cycle 1 because both are properties of scope
decisions rather than of the remediation; both carry a written disposition.

## Acceptance Criteria Check-off

This reviewer checked off **no** acceptance criteria this cycle. All 12 `spec.md` criteria were already
`[x]` and all were evaluated PASS, so no state change was warranted. The single unchecked criterion,
AC-U1, evaluates to NOT YET MET and must stay `- [ ]` until a pull request exists. Per the
acceptance-criteria-tracking protocol, an item is checked only after the work satisfying it is
delivered and verified; no pull request has been created, so checking it would be a phantom check-off.

No AC text was modified. No AC item was added.

### Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/spec.md
- Total AC items: 12
- Checked off (delivered): 12
- Remaining (unchecked): 0
- Items remaining: none

- Source: docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/user-story.md
- Total AC items: 5
- Checked off (delivered): 4
- Remaining (unchecked): 1
- Items remaining: AC-U1 — One branch and one pull request deliver all in-scope findings; the pull
  request body maps every finding identifier to the file that changed or to the recorded reason it
  did not.
```
