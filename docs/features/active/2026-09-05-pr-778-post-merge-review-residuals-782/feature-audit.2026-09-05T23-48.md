# Feature Audit — Issue #782 (pr-778-post-merge-review-residuals)

- **Date:** 2026-09-05
- **Reviewer:** feature-review agent
- **Work Mode:** `full-feature` (marker at `issue.md:10`)

## Scope and Baseline

| Item | Value |
|---|---|
| Base branch (resolved) | `main` -> `origin/main` @ `77c6d31404e2bc2291aec7eb9561e393c20cdcae` |
| Merge base (recomputed by this reviewer) | `77c6d31404e2bc2291aec7eb9561e393c20cdcae` |
| Merge base is an ancestor of HEAD | Yes — two-dot and three-dot diffs agree |
| Head | `refactor/pr-778-post-merge-review-residuals-782` @ `4ed2f790e96d8c22abd36514db3848b71e073912` |
| Diff range audited | `77c6d314...4ed2f790` (full branch diff, no subset) |
| Commits on branch | 22 |
| Files changed | 87 (+8,691 / -448) |
| C# and csproj files changed | 16 (+742 / -402) |
| Languages with changed files | C# only |
| PR context artifacts | `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`, both fresh — the summary's recorded `Head SHA: 4ed2f790e96d8c22abd36514db3848b71e073912` matches `git rev-parse HEAD` exactly, so no refresh was required |
| Worktree state | Clean at HEAD before and after this audit |

**Acceptance-criteria sources.** The work-mode marker resolves to `full-feature`, so the authoritative
sources are `spec.md` (AC1-AC12) and `user-story.md` (AC-U1 to AC-U5) — 17 criteria in total.
`issue.md` also carries an `## Acceptance Criteria` section with nine items; under `full-feature` that
section is not an AC source and is treated as the requirements record it is. The spec's twelve
criteria are a superset of it, and the spec documents each place it supersedes `issue.md` in its
"Corrections to issue.md Encoded Here" table.

**Note on the PR context summary.** Its `Changed files overview` reports `Core logic changes: 0 files`
and lists ten Markdown paths. That is a top-N-by-churn truncation, not the changed-file set; the real
figure of 16 C# files was taken from `git diff --stat` and used throughout. Its `Close candidates`
section lists seven author-asserted auto-close issues, all of which are prose scrapes from this
delivery's own artifacts. **Only #782 is closed by this branch.**

## Acceptance Criteria Inventory

| # | Source | Criterion (abbreviated) | State in source file |
|---|---|---|---|
| AC1 | `spec.md` | The seven Should-fix findings resolved as specified | `[x]` |
| AC2 | `spec.md` | The fourteen code/test nits resolved, or omission recorded with a reason | `[x]` |
| AC3 | `spec.md` | The eight documentation/evidence nits resolved in the #584 folder | `[x]` |
| AC4 | `spec.md` | The two optional refuted-item cleanups applied | `[x]` |
| AC5 | `spec.md` | Exactly one `FieldInfo` acquisition in `UtilitiesCS.Test`; none in `EmailMoveMonitorTests` | `[x]` |
| AC6 | `spec.md` | Both split parts under 500 lines, registered, same partial class, all names preserved | `[x]` |
| AC7 | `spec.md` | Three new tests, each failing with its guard removed and passing on current code | `[x]` |
| AC8 | `spec.md` | C09 behavioural half promoted; S4-1 and the `Timestamp:` request recorded upstream | `[x]` |
| AC9 | `spec.md` | Full C# toolchain passes in one final pass; changed-line coverage does not decrease | `[x]` |
| AC10 | `spec.md` | One shared message constant; both throw sites reference it; no literal remains | `[x]` |
| AC11 | `spec.md` | Test method name retained while its assertion changes to `*UiThread.Init()*` | `[x]` |
| AC12 | `spec.md` | Neither re-derivation item asserted without a fresh derivation in evidence | `[x]` |
| AC-U1 | `user-story.md` | One branch and one pull request deliver all in-scope findings | `[ ]` |
| AC-U2 | `user-story.md` | No production behaviour change beyond those named in the Behavioral Contract | `[x]` |
| AC-U3 | `user-story.md` | The #584 folder can be archived with no unrecorded residual | `[x]` |
| AC-U4 | `user-story.md` | A reader can verify every #584 command, count, and ordering claim | `[x]` |
| AC-U5 | `user-story.md` | Full C# toolchain passes in one final pass; changed-line coverage does not decrease | `[x]` |

Total: 17. Checked in source: 16. Unchecked: 1 (AC-U1).

## Acceptance Criteria Evaluation

Every verdict below rests on a check this reviewer performed against the tree or the coverage
documents. Where a delivery artifact made a claim, the claim was re-derived rather than accepted.

| # | Verdict | Independent verification |
|---|---|---|
| AC1 | **PASS** | **C10:** `UiThread_Tests.cs:186-206` — a `StaDispatcherHost` nested class starts a dedicated STA thread, captures `Dispatcher.CurrentDispatcher` there, and its `Dispose` calls `BeginInvokeShutdown(DispatcherPriority.Send)` then `_thread.Join()`; it is constructed inside a `using`, so shutdown runs on every exit path. The populated-branch test is retained under its original name. **C02:** the getter reads `_dispatcher` once into `captured`, tests the local, returns the local. **C18:** `EmailMoveMonitorTests.cs` now reads `UiThreadDispatcherFixture.Current`; a search of that file for `FieldInfo` returns zero hits. **C19:** the P27-T2 docstring, Act comment, and `NotThrow` reason all now read `InvalidOperationException` / `synchronous`; the diff removes every `NullReferenceException` mention from those three passages. **C20:** the false comment clause is replaced, `WpfDispatcherYield.cs:65` throws `UiThread.DispatcherNotInitializedMessage`, and a `WithMessage` assertion was added at `WpfDispatcherYieldTests.cs:136`. **C16:** split verified under AC6. **S3-2:** the #584 policy-audit formatter cell now records the six-path invocation that actually ran, row 3.1 is amended to disclose the deviation, Appendix B is relabelled, and a section 8 gap entry `B0` was added. |
| AC2 | **PASS** | Thirteen of fourteen nits are present in the diff and were spot-checked: C05 (non-lazy comment), C06 (message names only `Init()`), C08 (`<summary>`, `<remarks>`, `<exception>` on a file that previously carried zero `///`), C09-message (the "on the UI (STA) thread during host startup" clause), C11 (`Action act = () => _ = UiThread.Dispatcher;` expression-bodied), C12/C13 (four sites migrated), C14 (`[TestCleanup]` added), C15 (attributes on separate lines), C21 and C26 (new tests), C25 (both "avoid WindowsBase" clauses deleted), S2-1 (the false clause corrected). The fourteenth, **C03, is an omission**, and AC2's omission branch is discharged in full: `evidence/other/code-review.2026-09-05T23-00.md` section (a) opens with the required verbatim token `C03 OMITTED: latch re-arm not implemented` and records the discharge route, the measured regression, the bisect to the single line `_loaded = new ThreadSafeSingleShotGuard();` (5179/5180 with it, 5180/5180 without), the mechanism via the two lazy accessors, an explicit refusal to claim coverage for a branch that does not exist, and the promotion to a separate follow-up. This reviewer confirmed `Init()` carries no `try` or `catch` in the delivered tree. |
| AC3 | **PASS** | A search of the entire #584 evidence subtree for `EXIT_CODE:` lines that are not a bare signed integer returns **zero matches** in any evidence artifact (the only hits are prose occurrences inside that feature's plan file, which is not an evidence artifact and not in the S3-5 member set). S3-3 verified: `34` -> `38`. S3-1 verified: the ordering assertion in row 2.15 is softened to state that the recorded `Timestamp:` values do not establish relative execution order. S3-8 verified: the evaluative span "This is a model instance of the rule" is replaced with neutral wording. S3-9 verified: the disposition now cites C12/C13 as the discharging item and records that the follow-up was never promoted. S3-4, S3-5, S3-6, S3-7 present in the diff for the named files. |
| AC4 | **PASS** | `RibbonViewer.EngineCommands.cs` — both `dispatcher != null &&` comparisons removed at lines 72 and 115; the two XML-doc mentions of `UiThread.Dispatcher` are untouched. `ProgressTracker.cs:39` and `ProgressTrackerAsync.cs:39` now read `UiDispatcher = UiDispatcher`. This reviewer verified the fix is not a no-op: `ProgressTracker.cs:83-88` declares `internal Dispatcher UiDispatcher` over the private field `_uiDispatcher`, so the lambda closes over captured instance state and no longer re-reads the static. |
| AC5 | **PASS** | A search for the token `"_dispatcher"` across every `*.cs` in the repository returns **exactly two hits**: `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs:117` and `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136`. `EmailMoveMonitorTests.cs` contains no `FieldInfo`. The round-trip restore assertion AC5 requires is present at `UiThread_Tests.cs`: an outer `InstallNull()` establishes a null prior, an inner `Install(expected)` is asserted inside its scope, and after the inner scope disposes the test asserts `UiThreadDispatcherScope.Current.Should().BeNull()`. |
| AC6 | **PASS** | Line counts by `awk END{print NR}`: `ProgressTracker_Tests.cs` **271**, `ProgressTracker_ReportAndViewerTests.cs` **288** — both strictly under 500. `UtilitiesCS.Test.csproj:76` and `:479` carry exactly one `<Compile Include>` each. `ProgressTracker_Tests.cs:14-16` declares `[TestClass]` and `[DoNotParallelize]` on separate lines over `public partial class ProgressTracker_Tests`; `ProgressTracker_ReportAndViewerTests.cs:14` declares the same partial class with no attributes. Reviewer-run regex comparison of test method names across the pre-split file versus both post-split parts: **24 before, 25 after, zero missing, one added**. |
| AC7 | **PASS** | The three tests exist. `evidence/regression-testing/p4-t7-fail-before.md` records `Total tests: 3`, `Failed: 3` with both guards temporarily removed, and quotes each verbatim failure message; two carry production stack frames at `ProgressTrackerAsync.cs:35` and `ProgressTracker.cs:35`, the exact lines the edits exposed. The dossier removes **both** guards and states why removing only one would have made the C21 demonstration vacuous — this reviewer regards that as the difference between a real RED-first record and a decorative one. Pass-after is recorded in the final nine-assembly run, in which all five named tests report `Passed`. |
| AC8 | **PASS** | `docs/features/potential/promoted/2026-09-05-uithread-init-accepts-non-sta-callers.md:9` carries `- Issue: #787` (the C09 behavioural half) and `...latch-not-rearmed-after-failed-initialize.md:9` carries `- Issue: #788` (the withdrawn C03). `evidence/other/upstream-followups-drm-copilot.2026-09-05T23-02.md` records S4-1 and the `Timestamp:`-semantics request as upstream items. `git diff --stat <base>...HEAD -- ".claude/"` returns **empty**, confirming neither was fixed in this repository. |
| AC9 | **PASS** | All four gates were **re-run by this reviewer**, not accepted: CSharpier check exit 0 `Checked 1583 files in 4139ms.`; analyzer `msbuild /t:Rebuild` exit 0 across 18 projects; nullable `msbuild /t:Rebuild /p:TreatWarningsAsErrors=true` exit 0 with `0 Warning(s)` and `0 Error(s)`. `evidence/qa-gates/p7-t8-loop-closure.md` records the loop restarting once (pass 1 rewrote a file) and closing clean on pass 2, which is the correct handling. **Changed-line coverage was independently re-derived: 7 of 7 changed executable production lines covered, 100%, zero uncovered.** It did not decrease. |
| AC10 | **PASS**, with a recorded qualification | `UiThread.cs:135-136` declares exactly one `internal const string DispatcherNotInitializedMessage` whose value is character-identical to the text in the spec's Behavioral Contract. A search for `DispatcherNotInitializedMessage` returns three hits: the declaration and the two throw sites, one in each file. A search for `UiThread.Initialize()` across all `*.cs` returns **zero**. A search for `before yielding folder tree work` returns **zero**. The `WithMessage` assertion required by the criterion exists at `WpfDispatcherYieldTests.cs:136`. **Qualification:** the criterion's closing clause asserts the tail's removal "is pinned by" that assertion. It is not — `WithMessage("*UiThread.Init()*")` is a wildcard that would still match a message with the tail restored. Every verifiable requirement of AC10 is met; the defect is in the criterion's own reasoning about what the assertion proves, and is recorded as finding CR-1 in the code review. |
| AC11 | **PASS** | `UiThread_Tests.cs:133` retains `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` verbatim; `:142` asserts `WithMessage("*UiThread.Init()*")`. `evidence/other/code-review.2026-09-05T23-00.md` section (c) records the residual naming inaccuracy and the SD4 reason — the fully-qualified name is quoted inside a committed `TestCaseFilter` expression and renaming would make that recorded command resolve to zero tests. |
| AC12 | **PASS** | Both re-derivation artifacts exist and quote current text at each location: `evidence/baseline/p0-t9-584-spec-rederivation.md` re-reads the #584 spec's status line and AC checkbox block for S3-6, and `evidence/baseline/p0-t10-584-plan-rederivation.md` re-reads the two #584 plan line references (936-946 and 1064-1086) before either is quoted. Neither SD11 item is asserted anywhere without a fresh derivation. |
| AC-U1 | **FAIL — correctly open** | No pull request exists for this branch; `gh` is reported unavailable in the PR context artifacts and no PR metadata was retrievable. This criterion cannot be satisfied before the pull request is opened, and it is correctly left `[ ]` in `user-story.md`. It is not a defect in the delivery. The second half of the criterion — that the PR body map every finding identifier to a file or a recorded reason — is fully prepared for: `evidence/other/code-review.2026-09-05T23-00.md` carries a disposition row for all 26 `C` identifiers plus all 12 `S` identifiers. |
| AC-U2 | **PASS**, with a recorded qualification | The production behaviour changes in the diff are: the `InvalidOperationException` message text, and nothing else. The `RibbonViewer` comparison removals are behaviour-preserving because the accessor can no longer return null. The `ProgressTracker` and `ProgressTrackerAsync` edits substitute a captured field read for a static read of the same value assigned six lines earlier. `Init()` is byte-identical to its `pre-782-base` form. **Qualification:** AC-U2's text names the `Init()` retry behaviour as a permitted change; C03 was withdrawn so that change does not exist. The criterion bounds the permitted set from above rather than requiring both members, so a delivery that ships one and not the other still satisfies it — the reading the delivery's own artifact gives, which this reviewer accepts. The text is nonetheless stale; recorded as CR-8. |
| AC-U3 | **PASS** | Every one of the 38 finding identifiers has a disposition: 22 resolved in the diff, 4 refuted with no action (C04, C07, C22, C24), C17 and S4-2 no-action, C03 omitted with a full recorded reason and promoted as #788, C09's behavioural half promoted as #787, and S4-1 plus the `Timestamp:`-semantics request recorded as upstream follow-ups. This reviewer spot-verified the disposition table's row for each of the four refuted items against `pr-778-review-source.md`. Nothing is left unrecorded, so the #584 folder can be archived. |
| AC-U4 | **PASS** | The four claims S3-2, S3-3, S3-1, and S3-8 targeted were each re-verified as now reader-checkable: the formatter command cell records the six-path invocation that actually ran rather than the whole-tree form; the evidence count reads 38, matching the tree; the ordering sentence is softened to state that timestamps do not establish execution order; and the evaluative spans are replaced with neutral wording. A reader can now reconcile each recorded command, count, and ordering claim against committed evidence without re-deriving it. **Adjacent note, outside this criterion's scope:** this delivery's *own* baseline coverage artifact does not meet the same standard — see CR-2 / EV-1 in the code review. AC-U4 is scoped to the #584 artifacts and is satisfied. |
| AC-U5 | **PASS** | Same evidence as AC9. Toolchain re-run by this reviewer, single clean pass; changed-line coverage 100% (7/7) and therefore not decreased. |

## Summary

**16 of 17 acceptance criteria PASS. One (AC-U1) is correctly open pending creation of the pull
request. Zero criteria FAIL for a reason attributable to the delivery.**

Two criteria, AC10 and AC-U2, pass with recorded qualifications. In both cases every verifiable
requirement is met and the qualification concerns the criterion's own wording rather than the
delivered tree:

- **AC10** claims a wildcard `WithMessage` assertion pins the removal of a message tail. It does not.
  The requirement it states — that the assertion exist — is met; the inference it draws is wrong.
- **AC-U2** names a production behaviour change (the `Init()` latch re-arm) that was withdrawn
  mid-delivery and therefore does not exist. The criterion is an upper bound, so it is still
  satisfied, but the text is stale.

Both are recorded in `code-review.2026-09-05T23-48.md` as CR-1 and CR-8. Neither blocks.

Beyond the criteria, this reviewer established one fact the delivery's artifacts do not state, and it
strengthens rather than weakens the delivery's position. The per-file line coverage of
`UtilitiesCS/Threading/UiThread.cs` moves from 77.11% to 76.83%, which reads as a regression. It is
not one. The **uncovered line set is byte-identical on both sides** — the same 19 line numbers,
`28,29,30,32,33,34,67-76,118,119,120` — so no line transitioned from covered to uncovered. The
percentage moved only because a covered three-line wrapped `throw` collapsed to one line when routed
through the shared constant, shrinking a numerator and denominator that share a fixed uncovered
residue. The residue itself sits entirely in members the diff never touched: the `Init`
parameter-handling block, the `ThreadMonitor` construction that requires a live UI thread, and the
lazy `UiSyncContext` accessor.

**Recommendation: GO for pull request.** The pull request must close **only #782**; the seven
auto-close candidates in the PR context summary are prose scrapes and must not be carried into the
PR body.

## Acceptance Criteria Check-off

No source file was modified by this review. All 16 criteria this reviewer evaluated as PASS were
already checked `[x]` in their source files by the executor, and each was independently verified
before this audit confirmed the check-off. No criterion required a check-off correction, and no
criterion was found checked without supporting evidence.

AC-U1 remains `[ ]` in `user-story.md`. It is correctly unchecked: the pull request it requires does
not yet exist. This reviewer did not check it off, and it should be checked off by the agent that
opens the pull request, once the PR body carries the finding-to-file mapping the criterion specifies.

### Acceptance Criteria Status

```
- Source: docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/spec.md
          docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/user-story.md
- Total AC items: 17
- Checked off (delivered): 16
- Remaining (unchecked): 1
- Items remaining: AC-U1 — "One branch and one pull request deliver all in-scope findings; the
  pull request body maps every finding identifier to the file that changed or to the recorded
  reason it did not." Blocked only on the pull request not yet existing.
```
