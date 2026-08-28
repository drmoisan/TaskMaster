# P10-T12 — `FEATURE/spec.md` § Out-of-Scope Findings completeness

Timestamp: 2026-08-28T01-57
Command: (read docs/features/active/itemviewer-surface-defects-489/spec.md section "Out-of-Scope Findings" and enumerate its rows and evidence pointers)
EXIT_CODE: 0

## Acceptance — all eleven required entries are present with an evidence pointer

`FEATURE/spec.md` § Out-of-Scope Findings begins at `spec.md:719` and states its purpose: "Required
by `issue.md` § Scope Restrictions. Each entry carries an evidence pointer so it can be promoted to a
follow-up issue through the feature-promotion lifecycle after this feature merges."

| # | Entry | Spec line | Evidence pointer | Present |
|---|---|---|---|---|
| 1 | **#489 D4 residual** — consolidating the remaining two `ItemViewer` marshalling seams | 726 | research §4, §9.1 | Yes |
| 2 | **#490 D5** — grouping the ten display projections into a transactional construct | 727 | research §9.2 | Yes |
| 3 | **#490 D1 second half** — the deferred `ClearFolderItems()` clear-insertion at `QfcItemController.FolderHandling.cs:182` | 728 | research §5.5.1, §8.2 | Yes |
| 4 | **O1** — dead `FlagTaskDialogResult` writes at `QfcItemController.ViewerSetup.cs:375` and `:379` | 729 | research §5.5.4 | Yes |
| 5 | **O2** — `ButtonSVG : Button` implements `IButtonControl`, giving a non-`None` `DialogResult` form-closing semantics | 730 | research §5.5.4, U2 | Yes |
| 6 | **O3 (reframed)** — the 444-owned `QfcItemController.Navigation.cs:54` caller-side guard | 731 | research §5.5.2, §9.3 O3 | Yes |
| 7 | **O4** — `ThemeControlGroup.cs:212-229` marshals only when `_controls is not null` | 732 | research §5.4.2, U6 | Yes |
| 8 | **O5** — `ItemViewer.Designer.cs` (6224 lines) and `ItemViewerExpanded.Designer.cs` (821 lines) exceed the 500-line ceiling | 733 | research §1 | Yes |
| 9 | **O6** — `BreadcrumbBridgeRouterIssue439Tests.cs` is 531 lines, over the ceiling | 734 | research §7.1 | Yes |
| 10 | **O7** — `GetSelectedFolder()` nullable erasure | 735 | research §5.5.6, U3 | Yes |
| 11 | **O8** — raw `await Task.Delay(newDelay);` in production at `QfcItemController.EventWiring.cs:135` | 736 | research §9.3 O8 | Yes |

All **eleven** entries the task enumerates — the #489 D4 residual, #490 D5, the deferred #490 D1
clear-insertion, and O1 through O8 — are present, each with an evidence pointer. `issue.md` § Scope
Restrictions is satisfied.

The deferred #490 D1 clear-insertion is cross-checked independently by P10-T6, which confirms the
`FolderHandling.cs` diff inserts no `ClearFolderItems` call. O3's reframing is cross-checked by the
P9-T6 dossier, which records the residual and states that the original viewer-side O3 is resolved in
scope and must not be promoted as written.

## Four additional findings discovered during execution, appended to the same section

P10-T12 instructs: "Do not widen scope: any deeper design problem found during execution is added to
that section, never to this plan." Four items surfaced during execution of Phases 0 through 10 that
are genuinely out of scope for this feature. They were appended as rows **E1** through **E4** to the
same § Out-of-Scope Findings table, each labelled `(discovered during execution)` and each carrying
an evidence pointer. **No new file was created for them and no plan task was added.**

| # | Spec line | Finding | Verification performed here | Evidence pointer |
|---|---|---|---|---|
| **E1** | 737 | **Repo-wide stale analyzer HintPaths.** All sixteen tracked `.csproj` files name `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0` in `<Analyzer Include>` while `packages.config` declares `3.0.174` and `4.16.1`. | `git grep -c` over `*.csproj` for the two stale version strings returns **5 matches in each of 16 files**. Within `QuickFiler/QuickFiler.csproj` the skew is visible in one file: `3.0.174` at `:3` and `:579`, `3.0.156` at `:585`, `4.16.0` at `:586-588`; `packages.config` declares `3.0.174` and `4.16.1`. `git show origin/main:QuickFiler/QuickFiler.csproj` contains the stale string, so the state is committed on `origin/main` and a cold checkout fails `CS0006`. | `FEATURE/evidence/baseline/phase0-analyzer-build.2026-08-27T23-26.md` |
| **E2** | 738 | **Dangling `cref` to a removed member.** `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs:16` carries `<see cref="IItemViewer.SetFolderItems"/>`. | The cref is present at `:16`. `SetFolderItems` no longer exists as a member: a repo-wide `git grep` over `QuickFiler/` finds it only in three comment lines (`IItemViewer.cs:100`, `QfcItemController.EventHandlers.cs:165`, `BreadcrumbBridgeCoordinator.Search.cs:26`). No `CS1574` is emitted because `QuickFiler.Test.csproj` contains **zero** `DocumentationFile` elements, so XML documentation generation is off and crefs are never resolved — confirmed by the P9-T7 build, which reports zero `CS` diagnostics. P8-T7 restricts this feature's edits to that file to invocation renames only. | this artifact |
| **E3** | 739 | **The plan's `$LASTEXITCODE` convention is wrong for a zero-match `git grep`.** The `(… \| Measure-Object).Count` wrapper does not reset `$LASTEXITCODE`. | Measured directly at P9-T8: `Count=0`, `$? = True`, `$Error.Count = 0`, `$LASTEXITCODE = 1`. `$Error` was cleared and `$LASTEXITCODE` reset to `0` immediately before the command, so the `1` is attributable to that command alone. P9-T8 and P10-T15 are the exposed gates; both judge success from `$?` and `$Error.Count` under `$ErrorActionPreference = 'Stop'` and document the residual explicitly rather than writing a bare `0`. | `FEATURE/evidence/regression-testing/p9-t8-txtboxsearch-invoke-after.2026-08-28T01-44.md` |
| **E4** | 740 | **Stale narrative rows in the spec test matrix.** The § test-matrix rows for #486 D3 (`spec.md:655`) and for #490 D3 and #490 D4 (`spec.md:664`) name pre-growth measurements. | `spec.md:655` states `QfcItemController.EventWiringTests.cs` is "374 lines, 10 `[TestMethod]`, 126 spare"; `spec.md:664` states `QfcItemController.MailActionsTests.cs` is "184 lines, 7 `[TestMethod]`, 316 spare". The dated amendment note at `spec.md:761-762` records the current figures, 499 and 498 lines, and routes the new tests to `.Part2.cs` continuation files. These are narrative rows, not criteria; the amendment supersedes them. | `FEATURE/spec.md:761-762` (amendment note); `FEATURE/evidence/qa-gates/p10-t10-csproj-discipline.2026-08-28T01-54.md` |

## Consequence: `spec.md` acceptance-criterion line numbers moved

Appending four rows at `spec.md:737-740` — before § Acceptance Criteria — shifted every criterion line
down by **4**. This is recorded here because Phase 12 prints a line number for each of the 62
check-offs.

**Those printed numbers were already stale before this edit.** The plan's § Acceptance-criterion index
gives AC1 at `spec.md:766` and AC62 at `:845`. Measured immediately before the E1–E4 append, AC1 stood
at `:773` and AC62 at `:852` — already **+7**, from a spec edit made after the plan's index was last
reconciled. After the append the offset is **+11** uniformly.

| Marker | Plan-printed | Before this edit | After this edit | Total offset |
|---|---|---|---|---|
| AC1 | 766 | 773 | **777** | +11 |
| AC62 | 845 | 852 | **856** | +11 |

The full current list of the 62 unchecked criterion lines is:

```
777 778 779 783 784 785 786 787 788 789 790 794 795 796 797 798 802 803 804 805
806 807 808 809 810 814 815 816 817 818 819 820 821 822 823 824 825 829 830 831
832 833 834 835 836 837 841 842 843 844 845 846 847 848 849 850 851 852 853 854
855 856
```

62 criteria, all `- [ ]`, none checked. This batch checks off no acceptance criterion; Phase 12 owns
that. Phase 12 tasks locate each criterion by its printed line number **and** by a description, and
the `acceptance-criteria-tracking` skill directs check-off by matching criterion text, so the offset
degrades a locator rather than falsifying an acceptance condition. It is reported so the plan's
Phase 12 numbers can be reconciled before that phase runs.

## Scope discipline

No plan task was added, no plan text was edited, and no new file was created for these four findings.
They were appended to the record the plan already creates, which is exactly what P10-T12's closing
sentence directs.

Output Summary: `FEATURE/spec.md` § Out-of-Scope Findings contains **all eleven** required entries —
the #489 D4 residual, #490 D5, the deferred #490 D1 clear-insertion, and O1 through O8 — each with an
evidence pointer, satisfying `issue.md` § Scope Restrictions. Four further items discovered during
execution were appended to the same table as rows **E1** through **E4**, each labelled "discovered
during execution" and each with an evidence pointer: the repo-wide stale analyzer HintPaths committed
on `origin/main`, the dangling `IItemViewer.SetFolderItems` cref at
`QfcItemController.FolderSuggestionsTests.cs:16`, the incorrect `$LASTEXITCODE` claim in the plan's
execution conventions, and the stale test-matrix narrative rows at `spec.md:655` and `:664`. Each was
independently verified before being recorded. The append shifted every acceptance-criterion line down
by 4; combined with a pre-existing +7 drift the total offset from the plan's printed Phase 12 numbers
is now **+11**, with AC1 at `spec.md:777` and AC62 at `:856`. All 62 criteria remain unchecked.
