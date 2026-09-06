# Acceptance Criteria Status Summary — Issue #782

Timestamp: 2026-09-05T23-15

This is the single acceptance-criteria status summary for this delivery. P8-T8 creates it, P8-T13
and P8-T18 append to it, and no second file matching `evidence/other/ac-status-summary.*.md` is
created.

Command:

```powershell
# P8-T8
Get-ChildItem -LiteralPath 'docs/features/potential/promoted' -Filter '*.md' |
    Where-Object { $_.Name -ne '2026-09-05-pr-778-post-merge-review-residuals.md' -and $_.Name -ne '2026-08-07-webview2breadcrumbhost-unmarshalled-sdk-call-and-unsynchronized-state.md' } |
    Select-String -Pattern 'uithread-init|non-STA|apartment state'
```

EXIT_CODE: 0

Output Summary:

## P8-T8 — AC8 resolution

### The two mandatory exclusions, with the line at which each matches the unfiltered pattern

Both exclusions are mandatory and are not an optimisation. `Select-String` matches
case-insensitively, and both files match the pattern **today, before any promotion has occurred**:

| Excluded file | Matches at | Matched text | `- Issue:` |
|---|---|---|---|
| `2026-08-07-webview2breadcrumbhost-unmarshalled-sdk-call-and-unsynchronized-state.md` | line 86 | a sentence about corrupting COM apartment state | `#476` |
| `2026-09-05-pr-778-post-merge-review-residuals.md` | lines 63 and 107 | the clauses carving the C09 behavioural half out of scope | `#782` |

Both observed line numbers match the plan's stated ones exactly.

Without the exclusions the unfiltered search returns 3 hits across those 2 files, both of which
satisfy the issue-number conjunct, Branch A would fire against an issue that is not the C09
follow-up, and AC8 would be checked off although nothing was promoted. The second exclusion is this
delivery's own promoted entry, which is why the search must exclude it: a delivery cannot satisfy a
promotion criterion with its own record.

### Unfiltered search output, recorded in full

```text
2026-08-07-webview2breadcrumbhost-unmarshalled-sdk-call-and-unsynchronized-state.md:86: Defect 1 can throw `InvalidCastException`/`COMException` or corrupt COM apartment ...
2026-09-05-pr-778-post-merge-review-residuals.md:63: follow-up (make `Init()` reject non-STA callers) is out of scope; see below.
2026-09-05-pr-778-post-merge-review-residuals.md:107: - C09 behavioral follow-up (make `Init()` reject non-STA callers): a production ...
UNFILTERED_HIT_COUNT=3
```

### Filtered search output, recorded in full

```text
FILTERED_HIT_COUNT=0
FILTERED_DISTINCT_FILES=0
QUALIFYING_COUNT=0
```

The filtered search returns zero files.

### Branch taken

**Branch B.** The filtered search returned zero files, so no file contains a line matching
`^- Issue: #[0-9]+` whose number is neither 782 nor 476.

**Branch B is the state the plan measured at authoring time.** The observed state matches it.

AC8 is therefore left unchecked in `spec.md`, and this line is recorded verbatim as the branch
requires:

AC8 DEFERRED: the C09 behavioural follow-up has not yet been promoted; owner is the orchestrator, which performs promotion outside this plan.

### Both-branch preconditions

| Precondition | Result |
|---|---|
| Exactly one file matches `evidence/other/upstream-followups-drm-copilot.*.md` | **Holds.** One match: `upstream-followups-drm-copilot.2026-09-05T23-02.md`. |
| `evidence/qa-gates/p6-t3-dotclaude-untouched.md` records zero output from both of its commands | **Does not hold for the second command.** |

The second precondition is recorded rather than worked around. `p6-t3-dotclaude-untouched.md`
records zero output from `git diff --stat pre-782-base..HEAD -- .claude`, but two lines from
`git status --porcelain --untracked-files=all -- .claude`. Both lines are under
`.claude/agent-memory/atomic-planner/`, were written by the atomic-planner agent at 2026-09-05
22:17, fifteen minutes before this executor's first commit `d5e192b3` at 22:32:36, and are outside
this delivery's scope. P6-T3 is left unchecked for the same reason.

This unmet precondition does not change AC8's outcome. Branch B was selected by the filtered search
returning zero files, which is independent of the `.claude/` state, and Branch B leaves AC8
unchecked either way.

---

## P8-T13 — AC-U1 resolution

Timestamp: 2026-09-05T23-16

Command:

```powershell
git rev-list --count pre-782-base..HEAD
git branch --show-current
Get-ChildItem -Recurse -Filter 'pr_body_782.md' -ErrorAction SilentlyContinue
```

### Output, recorded in full

```text
REVLIST=11
BRANCH=refactor/pr-778-post-merge-review-residuals-782
PR_BODY_MATCHES=0
```

| Command | Result | Condition | Verdict |
|---|---|---|---|
| `git rev-list --count pre-782-base..HEAD` | 11 | integer of at least 6 | **Holds.** |
| `git branch --show-current` | `refactor/pr-778-post-merge-review-residuals-782` | exactly one branch name | **Holds.** |
| `Get-ChildItem -Recurse -Filter 'pr_body_782.md'` | zero paths | — | selects Branch B |

The commit-count condition is a lower bound rather than an equality because the range also contains
the implementation commit the external actor created under SD23, plus the pass-1 formatter commit
`47448924` that the Phase 7 loop restart produced. The count therefore exceeds the number of commits
this plan's own phases contribute by design.

### Branch taken

**Branch B.** The search returned zero paths, so no candidate file exists and none can contain all
four of the tokens `C01`, `C26`, `S2-1`, and `S3-9`.

AC-U1 is therefore left unchecked in `user-story.md`, and this line is recorded verbatim as the
branch requires:

AC-U1 DEFERRED: the pull request body has not yet been authored; owner is the orchestrator, which authors it outside this plan.

No second `ac-status-summary` file was created; this record is appended to the file P8-T8 created.

---

## P8-T18 — Acceptance Criteria Status Summary

Timestamp: 2026-09-05T23-18

Command:

```powershell
Select-String -Path 'docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/spec.md' -Pattern '^- \[[ x]\] AC'
Select-String -Path 'docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/user-story.md' -Pattern '^- \[[ x]\] AC'
```

EXIT_CODE: 0

Output Summary:

Every row's recorded state below was verified by re-running the two searches above and comparing
line by line against the state actually present in each document. Every artifact path cited exists
on disk.

### Source: `spec.md` (twelve criteria)

| ID | State | Evidence |
|---|---|---|
| AC1 | `[x]` | `evidence/qa-gates/p2-t4-file-size.md`, `evidence/qa-gates/p2-t5-split-test-names.md`, `evidence/qa-gates/p5-t14-584-corrections.md`, `evidence/qa-gates/p7-t5-tests-coverage.md`; the branch diff lists all eleven paths AC1's clauses name |
| AC2 | `[x]` | `evidence/other/code-review.2026-09-05T23-00.md` carries a disposition row for each of the fourteen nits; thirteen implemented plus one recorded omission (C03), with the bisect figures 5179/5180 and 5180/5180; `UtilitiesCS/Threading/UiThread.cs` carries exactly one `new ThreadSafeSingleShotGuard()`, the field initializer, proving no re-arm shipped |
| AC3 | `[x]` | `evidence/qa-gates/p5-t14-584-corrections.md`: 37 conforming `EXIT_CODE:` lines, zero evaluative-token hits, exactly the 23 expected #584 paths |
| AC4 | `[x]` | `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` carries zero `dispatcher != null`; both `ProgressTracker` files carry exactly one `UiThread.Dispatcher`; the anchored diff touches no XML-documentation line |
| AC5 | `[x]` | `evidence/qa-gates/p3-t10-reflection-sites.md` records exactly two `"_dispatcher"` hits, down from six; `EmailMoveMonitorTests.cs` carries zero `FieldInfo`; `evidence/qa-gates/p7-t5-tests-coverage.md` records the round-trip restore test `Passed` |
| AC6 | `[x]` | `evidence/qa-gates/p4-t10-file-size.md` records both files under 500 and under 350; the csproj carries one `<Compile Include>` each; `evidence/qa-gates/p2-t5-split-test-names.md` records 24 names, all `Passed` |
| AC7 | `[x]` | `evidence/regression-testing/p4-t7-fail-before.md` (`Failed: 3`, `ExpectedExitCode: 1`) and `evidence/regression-testing/p4-t8-pass-after.md` (`Passed: 3`, `EXIT_CODE: 0`) over the same three names |
| AC8 | `[ ]` | **Deferred.** See the P8-T8 record above. Branch B: the filtered promoted-entry search returned zero files. Owner is the orchestrator. |
| AC9 | `[x]` | The five Phase 7 step artifacts each record `EXIT_CODE: 0`; `evidence/qa-gates/coverage-summary.2026-09-05T23-11.md`; `evidence/qa-gates/p7-t7-changed-line-coverage.md`; `artifacts/csharp/coverage.xml` does not exist, per SD1 |
| AC10 | `[x]` | `UtilitiesCS/Threading/UiThread.cs` declares exactly one `internal const string DispatcherNotInitializedMessage` and references it on two lines, one the declaration and one the throw; `WpfDispatcherYield.cs` references it once; the `UtilitiesCS` tree carries zero `before yielding folder tree work` and zero `UiThread.Initialize()`; `YieldAsync_WithoutDispatcher_RemainsStrict` recorded `Passed` |
| AC11 | `[x]` | The test method retains its exact name and asserts `WithMessage("*UiThread.Init()*")`; `evidence/other/code-review.2026-09-05T23-00.md` records the SD4 residual naming inaccuracy and the reason the name is retained |
| AC12 | `[x]` | `evidence/baseline/p0-t9-584-spec-rederivation.md` and `evidence/baseline/p0-t10-584-plan-rederivation.md` both exist and quote the cited locations verbatim |

### Source: `user-story.md` (five criteria)

| ID | State | Evidence |
|---|---|---|
| AC-U1 | `[ ]` | **Deferred.** See the P8-T13 record above. Branch B: no `pr_body_782.md` exists. Owner is the orchestrator. |
| AC-U2 | `[x]` | `git diff --name-only pre-782-base..HEAD` over the nine production project directories lists exactly the five Write Set production paths and no other; `evidence/other/code-review.2026-09-05T23-00.md` records that only the message-text change is delivered, that SD18 withdraws the second permitted change, and that AC-U2 bounds the permitted set from above rather than requiring both |
| AC-U3 | `[x]` | `evidence/other/code-review.2026-09-05T23-00.md` carries 26 `C` rows plus rows for S2-1, S3-1 through S3-9, S4-1, and S4-2, each recording resolution, promotion, an upstream follow-up, or no action required |
| AC-U4 | `[x]` | `#584/policy-audit` records `All 38 evidence artifacts` and exactly one `csharpier format .` (the labelled Appendix B reference); `#584/feature-audit` carries zero; `evidence/qa-gates/p5-t14-584-corrections.md` records 37 conforming `EXIT_CODE:` lines |
| AC-U5 | `[x]` | `evidence/qa-gates/p7-t8-loop-closure.md` records pass 2 closing clean with all five steps green and no tracked-file rewrite after step 1; `evidence/qa-gates/p7-t7-changed-line-coverage.md` records an empty uncovered-changed-line enumeration |

### Totals

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/spec.md and docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/user-story.md
- Total AC items: 17
- Checked off (delivered): 15
- Remaining (unchecked): 2
- Items remaining: AC8 (the C09 behavioural follow-up promotion), AC-U1 (the pull request body)
```

Both remaining items are owned by the orchestrator and are performed outside this plan. Neither is
a delivery gap: each was resolved through its task's explicitly gated Branch B, and each carries its
verbatim deferral line above.

---

## Orchestrator resolution of AC8 (appended 2026-09-06T07-45)

The deferral above was accurate when the executor recorded it. It is now discharged. The record
above is retained unedited, because it is the true state at the moment P8-T8 ran; this section
supersedes it rather than rewriting it.

AC8 has two clauses and both are now satisfied.

**Clause 1, the C09 behavioural follow-up.** Promoted through the MCP promotion lifecycle by the
orchestrator:

- Potential entry: `docs/features/potential/promoted/2026-09-05-uithread-init-accepts-non-sta-callers.md`
- Issue: https://github.com/drmoisan/TaskMaster/issues/787
- Promotion type `bug`, work mode `full-bug`, matching the research recommendation. The defect is a
  missing precondition check on an existing contract rather than a new capability, and the sibling
  entry `2026-08-27-wpfuidispatchertests-ungated-static-swap.md` is the same shape.

**Clause 2, the upstream follow-ups for drm-copilot.** Recorded by P6-T2 at
`evidence/other/upstream-followups-drm-copilot.2026-09-05T23-02.md`, covering both the S4-1 stale
agent-memory notes and the S3-1 request to define `Timestamp:` semantics. Neither is fixed in this
repository; `git diff --stat pre-782-base..HEAD -- .claude` returns zero lines, verified by P6-T3.

A second promotion was made in the same pass, beyond AC8's requirement:

- Potential entry: `docs/features/potential/promoted/2026-09-05-uithread-init-latch-not-rearmed-after-failed-initialize.md`
- Issue: https://github.com/drmoisan/TaskMaster/issues/788

That entry carries finding C03 forward. C03 was withdrawn from this delivery under SD18 after the
executor measured a reproducible regression and bisected it to the single re-arm line. The entry
records the measurement, the mechanism, and three candidate approaches, so a future attempt does not
repeat the naive form. Recording the withdrawal only as prose inside a feature folder would have
lost it when the folder is archived.

`spec.md` AC8 is changed from `- [ ]` to `- [x]` by the orchestrator, which is the party that
performed the work the criterion names.

### Acceptance Criteria Status, revised

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/spec.md and docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/user-story.md
- Total AC items: 17
- Checked off (delivered): 16
- Remaining (unchecked): 1
- Items remaining: AC-U1 (the pull request body)
```

AC-U1 remains open by design until the pull request exists, and is checked off after it is opened.
