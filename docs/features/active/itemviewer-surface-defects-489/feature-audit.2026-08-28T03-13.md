# Feature Audit — itemviewer-surface-defects (Issue #489)

- Timestamp: 2026-08-28T03-13 (UTC)
- Branch: `bug/itemviewer-surface-defects-489` at `74d02ad2` vs merge base `69e83171` (`epic/quickfiler-bug-family-integration`)
- Work Mode: `full-bug` — `spec.md` § Acceptance Criteria is the sole AC source; `user-story.md` correctly does not exist (verified on disk); `issue.md` § Acceptance Criteria is a pointer only.
- AC population: 62 checkbox criteria, all 62 pre-checked by the executor. This audit re-verifies each; none required un-checking, and no criterion was newly checked by this review.

## Verification method

Evidence-verification model: committed evidence artifacts were read and their load-bearing claims independently re-measured where cheap (git diff/numstat, solution-wide greps, line counts, direct re-parse of the on-disk Cobertura document, reflection targets read in source). Toolchain commands were not rerun.

## The mid-flight amendment (spec.md:751-775) — audited and accepted

The approved plan routed five new tests into `EventWiringTests.cs` and `MailActionsTests.cs` believing them at 374 and 184 lines; merged siblings 484/444/493 had grown them to 499 and 498 (re-measured at HEAD: 499 and 498; both equal their Phase 0 baselines, so neither grew on this branch). The amendment reroutes the five tests to `.Part2.cs` partial-class continuation files — an established repo convention (the `InitializationTests.Part2/.Part3` precedent exists on disk, and ten prior `PartN` entries appear in test csproj files) — and amends exactly one criterion (csproj entries: "exactly two" -> "exactly four", AC45) plus file-path tokens in four criteria and one prose line. Checked against the quoted original wording in the dated amendment note: the criterion count stayed 62, every test name / node ID / assertion is unchanged, and the no-reordering guarantee is intact. **No criterion was weakened; the amendment is honest and locationally exact.** The stale narrative rows it superseded are self-recorded as finding E4.

## Acceptance criteria evaluation

### Phase 0 baseline (AC1–AC3)

| AC | Verdict | Evidence |
|---|---|---|
| AC1 seven baseline quantities | PASS | `evidence/baseline/` holds all seven with usable values after the amendment section of `phase0-baseline-index.2026-08-27T23-36.md` (2026-08-28T00-18) superseded the four measurements blocked by the inherited E1 analyzer skew; superseded artifacts retained as audit record. Criterion is stated over the directory as a whole and is satisfied. |
| AC2 U1 answer, pre-edit | PASS | `phase0-u1-designer-format-gate.2026-08-27T23-22.md`: Branch A (CSharpier 1.2.6 skips `*.Designer.cs` via generated-file detection), recorded with a name-only-diff proof that no Designer edit preceded it. |
| AC3 upstream landing + anchors | PASS | `phase0-upstream-landing-check` records `Upstream484Landed: true`, `Upstream444Landed: true`; anchor re-derivation artifact exists with member=file:line rows. |

### Issue #486 (AC4–AC11)

| AC | Verdict | Evidence |
|---|---|---|
| AC4 `ItemViewerExpanded_DeclaresNoMenuItemCheckedChangedHandler` passes | PASS | Test present in `ToolStripMenuItemCbTests.cs` (read); final runs 1121/0 and 6741/0; both overloads absent from `ItemViewerExpanded.cs` (diff). |
| AC5 four constructor calls + four designer wirings deleted | PASS | Diff shows the four `MenuItem_CheckedChanged(this.*MenuItem)` constructor calls and the four `CheckedChanged +=` designer lines deleted; analyzer build exit 0 (CS0103 argument). |
| AC6 three setter pins + `IsNotDerivedFromControl` pass | PASS | All four tests read in source; green in final TRX-backed runs. |
| AC7 `ToolStripMenuItemCb.cs` untouched | PASS | Absent from `git diff --name-only` (re-verified). |
| AC8 `ItemViewer` dead members deleted, contract tests pass | PASS | Diff deletes `:166-169`, `:171-175`, `:177-187`, `:205`; both metadata tests read in the grown contract file. |
| AC9 `WireIntentEvents_SubscribesToPicturesChanged` in Part2 passes | PASS | Test read in `EventWiringTests.Part2.cs` with the exact `VerifyAdd` shape; wire line present in production diff. |
| AC10 `PicturesChanged_WhenRaised_RefreshesOptionsPictures` passes | PASS | Test read; handler added in `EventHandlers.cs`; member coverage 3/3 re-parsed from the Cobertura document. |
| AC11 16->17 handoff record in `evidence/other/` | PASS as written, with a Blocking consequence | `wireintentevents-16-to-17-handoff.2026-08-28T01-55.md` exists, states the 16->17 change, names 484 as owner, records `Upstream484Landed: true`. The criterion bound only the record, and the record is accurate. The undischarged obligation itself is Blocking finding RC-1 (policy audit § 8) because 484 is merged and can never absorb it. |

### Issue #487 (AC12–AC16)

| AC | Verdict | Evidence |
|---|---|---|
| AC12 both `DeclaresNoParentChangedHandler` tests pass | PASS | Tests read; handlers deleted in diff. |
| AC13 `git grep "Parent Changed"` zero in `QuickFiler/Viewers/` | PASS | Re-run by this review: 0 matches. |
| AC14 two designer wirings deleted | PASS | Diff: `ItemViewer.Designer.cs` one deletion (the `ParentChanged +=`), `ItemViewerExpanded.Designer.cs` includes the `:274` deletion; analyzer exit 0. |
| AC15 no replacement diagnostic introduced | PASS | Both files' diffs are deletion-only (numstat 0/32 and 0/27); no logger/`Debug.WriteLine` added. |
| AC16 no wholesale Designer reformat | PASS on purpose, wording defective | Measured: 0 additions in both Designer files; deletions 1 and 5. The literal "single deleted wiring line" contradicts AC5's mandated four extra deletions in `ItemViewerExpanded.Designer.cs` — a spec wording defect. The criterion's protective purpose (no reformat; consistent with the U1 Branch A record) is fully satisfied. This audit concurs with the executor's adjudication and recommends a one-line spec erratum at fan-in. The check-off stands. |

### Issue #489 (AC17–AC25)

| AC | Verdict | Evidence |
|---|---|---|
| AC17 `ThemeMarshallingTests.cs` exists, marshal test passes | PASS | File read; `Invoke(It.IsAny<Delegate>())` verification exact. |
| AC18 other two theme tests pass | PASS | Both read; green runs. |
| AC19 no D2 test in `FocusAndThemeTests.cs`, count unchanged (497) | PASS | File absent from diff. |
| AC20 D3 dossier exists with required content | PASS | `fail-before-exception-489-d3-set-then-sort.2026-08-28T01-40.md` names both must-stay-green tests (`ConversationTests.cs:249`, `:266`) and the rejected F2 alternative. |
| AC21 XML docs on set/sort pair, signatures unchanged | PASS | Diff shows doc-only additions around unchanged declarations. |
| AC22 `IItemViewer_DeclaresNoUiSchedulerMember` passes | PASS | Test read; `UiScheduler` grep on `IItemViewer.cs`: 0. |
| AC23 `UiDispatcher`/`UiSyncContext` pins pass | PASS | Tests read; members present in interface source. |
| AC24 six unrelated `UiScheduler` members untouched | PASS | None of the six named files appears in the diff (re-verified). |
| AC25 `MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher` unchanged and green | PASS | `SeamDispatcherTests.cs` diff is the single rename hunk at :193; the :99 test region byte-identical; green in final runs. |

### Issue #490 (AC26–AC37)

| AC | Verdict | Evidence |
|---|---|---|
| AC26 `DeclaresAddFolderItemsAndNotSetFolderItems` passes | PASS | Test read with the exact `GetMethod` pair. |
| AC27 every call site renamed; build + zero failures | PASS | Solution grep for residual `.SetFolderItems(`: 0; analyzer exit 0; 6741/0. |
| AC28 two protected test names survive and pass | PASS | Both greps return exactly one declaration each; green runs. |
| AC29 `BreadcrumbBridgeCoordinator.Search.cs` not edited | PASS | Absent from diff. |
| AC30 D2 dossier exists with contract + residual | PASS | Dossier names the adopted contract and `Navigation.cs:54` residual, promoted as reframed O3. |
| AC31 `FocusSearch` bare forward; one documented contract | PASS | `TxtboxSearch.Invoke` grep: 0 (was 1); XML doc on both focus members in `IItemViewer.cs`. |
| AC32 NavigationTests verify still green; `Navigation.cs` absent from diff | PASS | File absent from diff; green runs; dossier records the test/assertion location at current head (:185/:199). |
| AC33 `IItemViewer_FocusSubjectReturnsBool` passes | PASS | Test read; `bool FocusSubject();` in interface diff. |
| AC34 `Expand_WhenFocusSubjectReturnsFalse_StillEnumeratesConversation` in `MailActionsTests.Part2.cs` passes | PASS | Test read; the `Setup(...).Returns(false)` compile-time RED argument holds against a `void` member. |
| AC35 `LblSubject` untouched beyond return type; Designer diff single line | PASS | `DisplayState.cs` diff is the one-line signature change; `ItemViewer.Designer.cs` diff is the one deleted wiring. |
| AC36 both `FlagAsTask*_DoesNotReadBackFlagTaskDialogResult` pass | PASS | Tests read with `VerifyGet ... Times.Never()`; production local-hold form in diff. |
| AC37 `FlagTaskDialogResult` still declared; ViewerSetup setter assertions green | PASS | Property present on interface and `Commands.cs` (not in diff); `ViewerSetupTests.cs` absent from diff; green runs. |

### Scope discipline (AC38–AC46)

| AC | Verdict | Evidence |
|---|---|---|
| AC38 `ItemViewer.Breadcrumb.cs` absent | PASS | Re-verified against full diff. |
| AC39 `BreadcrumbBridgeCoordinator.cs` absent | PASS | Re-verified. |
| AC40 sibling-owned production files confined to named members | PASS | Per-file diffs read: `EventWiring.cs` = one wire line; `FocusAndTheme.cs` = `HtmlDarkConverter` only; `MailActions.cs` = discard + two local-holds + two renames; `FolderHandling.cs` = one token. |
| AC41 `FolderHandling.cs` diff is the one-token rename, no `ClearFolderItems()` insertion | PASS | Diff is exactly one changed line. |
| AC42 `QfcCollectionControllerTests.cs` absent | PASS | Re-verified. |
| AC43 `TestSupport.cs` absent; `BuildSyncDispatcher` consumed | PASS | Absent from diff; consumed in `ThemeMarshallingTests` and `MailActionsTests.Part2`. |
| AC44 no `UtilitiesCS` file in diff | PASS | Re-verified. |
| AC45 `QuickFiler.csproj` absent; test csproj gains exactly the four amended entries, appended, no reorder | PASS | Diff read: 4 additions / 0 deletions at the recorded block tails, order as amended. |
| AC46 § Out-of-Scope Findings complete | PASS | All mandated rows present plus execution-discovered E1–E4, each with an evidence pointer. |

### File size, toolchain, coverage, evidence (AC47–AC62)

| AC | Verdict | Evidence |
|---|---|---|
| AC47 500-line ceiling + baseline growth discipline | PASS | Head line counts re-measured for all 25 files: all <= 500 except the two pre-existing Designer files, which shrank; growth confined to the plan's intentional-growth list (`p11-t11`). |
| AC48 csharpier check clean | PASS | Exit 0, 1547 files, empty unformatted set. |
| AC49 analyzer build exit 0, warnings <= baseline | PASS | 5 = 5, all pre-existing System.Reactive advisories. |
| AC50 analyzer non-vacuity (zero CoreCompile skips) | PASS | 0 occurrences in the committed 11.9k-line log. |
| AC51 nullable build exit 0, correct command | PASS | Recorded command matches CLAUDE.md exactly; no `/p:Nullable=enable`, `/t:Rebuild`. |
| AC52 net48 guard (no init/record) | PASS | Nullable build green; no such construct in the diff. |
| AC53 vstest counts vs baseline (per-class discipline) | PASS | 1121/0/0 vs 1099/0/0; zero failures in the two absolute-gated new classes; per-class table in `p11-t7`. |
| AC54 repo-wide line coverage not lower than baseline | PASS | Shape-matched 0.851185 -> 0.851567 (+0.038 pp); method audited and accepted (policy audit § 7.3); final document re-parsed by this review. |
| AC55 no coverage-delta claim over `ItemViewer*.cs`; attribute unchanged | PASS | `ItemViewer.cs:20` attribute not in diff; no such claim in the spec. |
| AC56 exclusion-attribute count <= baseline (both spellings) | PASS | Re-run by this review: 261 = 261. |
| AC57 `CbxPictures_CheckedChanged` >= 90 percent | PASS | 3/3 = 100 percent, independently re-parsed. |
| AC58 no banned timing API in the two new files | PASS | Re-run over all four new files: 0 matches. |
| AC59 no temp files / no live Form; structural guard green | PASS | `p11-t12` + TRX; no file I/O in the new tests (read). |
| AC60 MSTest + Moq + FluentAssertions | PASS | All 22 new/edited tests conform. `ToolStripMenuItemCbTests.cs` imports no Moq because its five tests mock nothing (concrete `Component`-derived type constructed directly); the criterion binds the library choice where mocking occurs, and no competing framework appears. Concur with the executor's reading. |
| AC61 evidence location canonicality | PASS | All artifacts under `FEATURE/evidence/<kind>/`, canonical kinds only; diff scan for `artifacts/{baselines,qa,coverage,evidence}/`: 0. |
| AC62 `user-story.md` absent | PASS | Verified on disk. |

## Verdicts on the five disclosed judgment calls

1. AC16 purpose-over-wording: **agree** (spec wording defect; measured 0/1 and 0/5 with zero additions; AC5 mandates four of the five deletions).
2. Subscription leak deferral: **disagree — Blocking** (RC-1; the AC passes as written, the shipped behavior does not).
3. Coverage shape-boundary comparison: **agree — method sound** (repo's own converter, identical denominators across final runs, margin 12x the observed noise; reproducibility residual noted as NB-4).
4. AC1 baseline-index staleness: **agree — cured by the dated amendment section**; fail-closed handling of the blocked first attempt was correct.
5. AC60 Moq-free pin file: **agree**.

## Deviations and caller-brief corrections

- The caller briefing described this feature's owned test region as `Controllers\QfcItemController*` and `Viewers\ToolStrip*`; the spec's § Scope additionally enumerates the rename-only edits to three `Viewers/Breadcrumb*` test files and the contract-test additions to `ItemViewerBreadcrumbDropDownContractTests.cs`. The spec governs; all four files' diffs match the spec's enumerated edit shapes exactly (line-neutral renames; test-only additions). No scope violation.
- `QfcItemController.SeamDispatcherTests.cs` (once listed as untouchable in an early briefing) carries exactly the compiler-forced one-token rename at line 193; without it the assembly fails CS1061. The protected :99 test is untouched.

### Acceptance Criteria Status
- Source: `docs/features/active/itemviewer-surface-defects-489/spec.md` § Acceptance Criteria
- Total AC items: 62
- Checked off (delivered): 62
- Remaining (unchecked): 0
- Items remaining: none
- Newly checked by this review: none (all were pre-checked; every check-off was re-verified and stands)

## Overall

62/62 acceptance criteria PASS. One Blocking finding outside the AC set (RC-1, subscription-leak regression) prevents a merge-ready verdict; see `remediation-inputs.2026-08-28T03-13.md`. After RC-1 remediation the feature is ready to merge.
