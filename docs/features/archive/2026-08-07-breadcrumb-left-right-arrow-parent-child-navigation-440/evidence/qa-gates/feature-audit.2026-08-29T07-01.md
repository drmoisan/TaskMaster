# Feature Audit — issue #440 (breadcrumb Left walk-to-root, Qfc)

- Timestamp: 2026-08-29T07-01
- Reviewer: feature-review agent
- Branch: `bug/breadcrumb-left-right-arrow-parent-child-navigation-440`
- Base ref: `b56400ab663a85b6039139d4548f408821e957ce`
- Head ref: `99767554243a7b99a71d2084823d29afcc7127ce`
- Work mode: `full-bug`
- AC source: `spec.md` only (15 criteria, AC-1 through AC-15)
- Verdict: **PASS** — 15 of 15 criteria PASS, 0 blocking findings

## AC source resolution

Work mode `full-bug` was read from the persisted marker `- Work Mode: full-bug` at `issue.md` line
12. Under that mode `spec.md` is the sole authoritative acceptance-criteria source and no
`user-story.md` is expected. Its absence is not a finding.

`spec.md` also carries a four-item severity checklist at lines 49-52 (Blocker / High / Medium /
Low). Those are not acceptance criteria and were excluded. The `## Acceptance Criteria` section at
line 492 contains exactly 15 checkbox items.

## Independent verification performed

The reviewer did not accept the executor's evidence at face value. The following were reproduced or
re-derived in this session:

| Check | Command / method | Result |
|---|---|---|
| Base is the true merge base | `git merge-base HEAD <BASE>` | returns `<BASE>` |
| Formatter | `dotnet tool run csharpier check .` | exit 0, `Checked 1560 files in 4472ms.` |
| Analyzer build non-vacuity | re-read of `coverage/logs/p4-t3-analyzer.msbuild.txt` | 0 CoreCompile skips, 40 rebuild markers, 36 `csc.exe`, 0 errors, 5 warnings |
| Nullable build non-vacuity | re-read of `coverage/logs/p4-t4-nullable.msbuild.txt` | same counts |
| Arrow transition tests | vstest over the 31 arrow-related methods in `UtilitiesCS.Test` | 31/31 passed |
| Efc router suite | vstest over `QuickFiler.Test` breadcrumb router classes | 68/68 passed |
| Full suite | repository wrapper, full run with coverage | 6859/6859 passed, exit 0 |
| Coverage arithmetic | hand re-sum of the JaCoCo package counters; direct read of both Cobertura roots | reconciles exactly |
| Per-file coverage | direct read of the `class` element for `BreadcrumbStateModel.cs` in both documents | baseline 119/121 and 41/44; final 118/120 and 39/42 |
| Root boundary claim | read of `BreadcrumbStateModel.Row.cs:195-211` | `ActivateSegment` refuses index < 0 and index >= `Chain.Count - 1` |

## Acceptance criteria evaluation

| AC | Verdict | Evidence relied on |
|---|---|---|
| AC-1 | PASS | Reviewer re-ran `LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled`: passed. The test asserts the starting index is 2, index 1 after press 1, index 0 after press 2, and `false` with index still 0 on press 3, so it cannot pass on the boolean alone. Fail-before at `evidence/regression-testing/p1-t4-fail-before.2026-08-29T06-30.md` (`EXIT_CODE: 1`, both press-2 assertions failing); pass-after at `evidence/regression-testing/p3-t1-pass-after.2026-08-29T06-32.md`. |
| AC-2 | PASS | Reviewer grep: `activeIndex.Value == row.Chain.Count - 1` returns **0** matches in the production file; `row.ActivateSegment(activeIndex.Value - 1)` returns **exactly 1**. Anchored hunk diff shows numstat `+5/-5`, of which 4 removed lines are the superseded comment block and exactly 1 is the conjunct. `_selectedSubfolderIndex >= 0` reset block, `return true;`, and `return row.TryCollapseLeaf();` all appear as unchanged context. No other conditional changed. |
| AC-3 | PASS | Reviewer grep: `_selectedSubfolderIndex < 0` present at production file line 234, rendered as unchanged context in the anchored diff. Reviewer re-ran `LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses`: passed, and the test is absent from the diff so it passed unmodified. |
| AC-4 | PASS | Reviewer re-ran `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` and `LeftArrow_AtRootSegment_IsNoOp`: both passed. `QuickFiler/Controllers/KeyboardHandler.cs` is absent from the reviewer's own repository-wide anchored diff (`git diff --name-only <BASE> HEAD -- . ":(exclude)docs" ":(exclude).claude"`), which lists exactly three source paths. Legacy fall-through is therefore retained unchanged. |
| AC-5 | PASS | Reviewer re-ran `Route_RightArrow_NothingToExpand_ReportsUnhandledRight`: passed, unmodified (absent from the diff). The production hunk lies wholly inside `LeftArrow()` (lines 220-246); `TryRightTreeTransition` at lines 193-214, including its `activeIndex.Value >= row.Chain.Count - 1` condition, is untouched. |
| AC-6 | PASS | Reviewer read the updated test at `FolderBreadcrumbBridgeRouterTests.cs:368-385`. Two `ArrowAsync(router, "left")` presses in Arrange drive the three-segment fixture to the root before the asserted third press. The Arrange comment now reads "Left walks the ancestor chain"; the superseded literal "the one available #440 parent-select transition" returns 0 matches. File is 491 lines, under its 495-line baseline. See code-review CR-1 for a non-blocking weakness in this test's regression value; the criterion as written is met. |
| AC-7 | PASS | Reviewer read the updated test at `BreadcrumbStateModelSequenceTests.cs:60-84`. It now asserts press 1 returns true with `LeafExpanded` false and index 1, press 2 returns true with index 0, and only then press 3 returns false. The comment records the decision D1 rationale verbatim. Reviewer re-ran it: passed. |
| AC-8 | PASS | Both new tests exist in the named file, carry `[TestMethod]` and XML doc comments, use FluentAssertions for every assertion, follow labelled Arrange-Act-Assert, and create no temporary file and no Outlook, WebView2, or timer dependency. Reviewer re-ran both: passed. The conditional-Moq reading is correct: the walk-to-root path has no collaborator, and the `IFolderHierarchyProvider` seam is mocked with `MockBehavior.Strict` via `ProviderMock()` at the router level. See OB-2 for a governance observation about how this criterion was amended. |
| AC-9 | PASS | Reviewer's own repository-wide anchored diff contains none of `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`, `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`, or `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`. Reviewer re-ran the Efc breadcrumb router suite: 68/68 passed, including `HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior`. Executor's broader run recorded 119/119. |
| AC-10 | PASS | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` is absent from the reviewer's anchored diff. `git status --porcelain -- UtilitiesCS UtilitiesCS.Test` is empty (all work committed), and the anchored diff lists exactly three source paths, none of them new. No new test file was added. |
| AC-11 | PASS | Reviewer measured with `awk END{print NR}`, not `Measure-Object -Line`: `FolderBreadcrumbBridgeRouterTests.cs` = **491**, `BreadcrumbStateModelSequenceTests.cs` = **292**. Both at or under 500. The production file is 248, unchanged from its baseline. |
| AC-12 | PASS | Reviewer ran `git diff --name-only <BASE> HEAD -- . ":(exclude)docs" ":(exclude).claude"` and obtained exactly the three declared source paths and no others. `git status --porcelain -- QuickFiler QuickFiler.Test` is empty, which catches a file created rather than modified under either QuickFiler root. The feature folder's own documentation and evidence are the criterion's own explicit carve-out. |
| AC-13 | PASS | Reviewer read the "Boundary decisions — DECIDED, do not reopen" subsection (`spec.md` lines 208-228): both boundaries are recorded as ratified under #498 decision D2 and locked by its AC-23 and AC-24, with provenance. The "Issue #400 reconciliation" subsection cites the existing record and states "Cite the existing record; do not duplicate it." Reviewer independently ran `grep -rlniE "supersession\|supersede\|retract"` over the whole 36-file feature folder: 5 files match, and every match is a citation of the #498 record, an instruction to cite rather than re-author, or an unrelated use of the word. No file declares a new supersession record. |
| AC-14 | PASS | Reviewer reproduced step 1 (`csharpier check .`, exit 0, `Checked 1560 files`) and step 4 (full suite, 6859/6859 passed, exit 0). Steps 2 and 3 were verified from the raw msbuild file-logger output rather than the summary artifacts: both logs show 0 occurrences of `Skipping target "CoreCompile"`, 40 occurrences of `(Rebuild target(s))`, 36 `csc.exe` invocations, `CoreClean` execution, 0 errors and 5 warnings at the baseline count. `evidence/qa-gates/p4-t7-consecutive-pass.2026-08-29T06-40.md` records the five artifacts in ascending timestamp order with 0 restarts and a formatter rewritten-file count of 0. The wrapper substitution for the fourth command is correctly declared and is the right call: the wrapper supplies the `TestCategory!=LiveOutlook` exclusion that a bare invocation omits. |
| AC-15 | PASS | Reviewer re-derived every figure. Repo-wide rose: 85.2935% to 85.3026% line, 79.2523% to 79.2558% branch. Per-file uncovered counts held at 2 lines and 3 branches (baseline 119/121 and 41/44; final 118/120 and 39/42), read directly from the `class` element `line-rate` and `branch-rate` attributes in both Cobertura documents. All 19 instrumented `line` elements in the post-change `LeftArrow()` span carry `hits > 0`, so every changed line is covered; the transition `if` reads `condition-coverage 100% (6/6)` after against `100% (8/8)` before. The `EVIDENCE_LOCATION_OVERRIDE_REJECTED` record correctly redirects the criterion's non-canonical `evidence/coverage/` location. See the rate-versus-count nuance below. |

**Criteria evaluated PASS: 15 of 15. PARTIAL: 0. FAIL: 0. UNVERIFIED: 0.**

### AC-15 nuance, stated so it is not discovered later as a concealed dip

Read as a **rate**, per-file line coverage moves from 98.3471% to 98.3333% and per-file branch
coverage from 93.1818% to 92.8571%. Read as **uncovered counts**, both are invariant: 2 uncovered
lines and 3 uncovered branches before and after. The denominators shrank because the change deletes
one already-covered line and one already-covered `&&` conjunct. No previously covered line or branch
became uncovered, and the changed region is fully covered. The reviewer judges AC-15 PASS on the
substantive reading and records the rate figures here so the maintainer can see both.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440/spec.md
- Total AC items: 15
- Checked off (delivered): 15
- Remaining (unchecked): 0
- Items remaining: none
```

No checkbox was modified by this review. All 15 were already `- [x]` and all 15 were evaluated PASS,
so the check-off protocol required no change. Had any been evaluated below PASS its box would have
been left alone and the gap recorded prominently.

## Scope boundary confirmation

The narrowed scope is legitimate and was verified rather than assumed. `spec.md` lines 15-22 record
that most of issue #440 landed on `main` as a secondary payload of feature #498, whose `spec.md`
line 4 reads "Also closes: #440, #499". The residual defect is Qfc-only. The two recorded
divergences — the Right-descent commit asymmetry between the surfaces, and the single-level Right
descent limit on both — are declared non-goals at `spec.md` lines 146-152 and are not introduced by
this diff; `TryRightTreeTransition` is untouched. They are not reported as gaps.

## Follow-up observations (non-blocking, not defects of this change)

| ID | Observation |
|---|---|
| OB-1 | Repository-wide analyzer version skew persists on `main`: the packages-config files pin Meziantou.Analyzer 3.0.174 and Roslynator.Analyzers 4.16.1 while the hand-written Analyzer items name 3.0.156 and 4.16.0. A fresh worktree therefore fails every msbuild invocation with CS0006 until the referenced directories are provisioned. This change correctly worked around it in the gitignored packages directory only, editing no `.csproj` and no `packages.config`; the reviewer confirmed neither file type appears in the branch diff. The durable repair is a solution-wide realignment and warrants its own issue. |
| OB-2 | AC-8's Moq clause was amended from unconditional to conditional. The amendment is recorded transparently in `spec.md` lines 611-616 with its rationale, and the reviewer confirmed by diffing `spec.md` between the preparation commit `f6a56926` and head that the amendment predates execution and that the executor changed only the 15 `- [ ]` markers, altering no criterion text. The amendment was made by an orchestrator; no maintainer ratification record exists in the tree. Worth a one-line maintainer confirmation at merge. |
| OB-3 | The Right-descent commit asymmetry and the single-level Right descent limit remain. `spec.md` line 823 already recommends filing them if surface parity beyond the Left contract is wanted. Recommend filing both as their own issues so they do not disappear when this feature folder is archived. |
| OB-4 | The working tree carries uncommitted changes under `.claude/agent-memory/atomic-executor/` (one modified `MEMORY.md`, one untracked memory file). Plan P5-T19 deliberately excluded `.claude` from staging to avoid sweeping another agent's work onto this branch, which is the correct call. Flagged only so the maintainer is not surprised by a non-empty `git status` at merge time. |
| OB-5 | `feature/quickfiler-breadcrumb-bridge-coverage-r2` (#495) is cut from a base older than #439, #498, #499 and #614. `spec.md` lines 792-798 records that merging it as-is would silently revert the landed #440 Efc implementation with no merge conflict. It requires a rebuild on `main`, not a conflict resolution. Not this change's problem, but it is a live hazard against this change's payload. |
| OB-6 | Router-level regression protection for the second consecutive Left press is absent. See code-review CR-1 for the analysis and the recommended repair. |

## Verdict

**PASS.** All 15 acceptance criteria are met and independently verified. The change is minimal,
correct, fully covered on its changed lines, and clean against every applicable repository policy.
0 blocking findings. No remediation cycle is required.
