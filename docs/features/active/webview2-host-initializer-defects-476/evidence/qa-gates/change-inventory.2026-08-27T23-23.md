# Change Inventory ([P5-T1])

Timestamp: 2026-08-27T23-23

Command:

```
git diff --name-only 4f238289090e4c97ca505511a5a73e8092dce0f9
git status --porcelain --untracked-files=all
git diff --name-only origin/epic/quickfiler-bug-family-integration..HEAD
```

`4f238289090e4c97ca505511a5a73e8092dce0f9` is the `BASELINE_SHA` recorded in
`evidence/baseline/baseline-repo-state.2026-08-27T19-56.md`.
`origin/epic/quickfiler-bug-family-integration` is at `69e8317152c0a9ee6ee6e65db0ef81f6906189b1`,
the integration tip merged into this branch at `9cb2c4f6`.

EXIT_CODE: 0

## Output Summary

- `git status --porcelain --untracked-files=all` printed **nothing**. The working tree is clean and
  there is no untracked path, so the union of changed and untracked paths equals the committed diff.
- `git diff --name-only <BASELINE_SHA>` lists **250** paths. That count is not this feature's change
  set. `BASELINE_SHA` was recorded before the integration base was merged in at `9cb2c4f6`, so the
  diff against it is the union of this feature's own work and the twenty-eight base commits the merge
  brought in (features 444 and 493 among them).
- `git diff --name-only origin/epic/quickfiler-bug-family-integration..HEAD` lists **78** paths and
  is this feature's own change set: everything on this branch that the merged base does not already
  contain.
- **The production classification of this feature's own change set contains exactly the three
  in-scope files** and nothing else.

---

## This feature's own change set (78 paths, `base..HEAD`) — the authoritative classification

### Production (3)

| Path |
| --- |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs` |
| `QuickFiler/Viewers/IWebViewCoreInitializer.cs` |

Exactly three, exactly the three the acceptance names. No other production path appears.

### Test (3)

| Path |
| --- |
| `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` |
| `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` |
| `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs` |

### Project file (1)

| Path |
| --- |
| `QuickFiler.Test/QuickFiler.Test.csproj` |

### Documentation and evidence (71)

All 71 are under `docs/features/active/webview2-host-initializer-defects-476/`: this feature's
`plan`, `spec`, `issue`, `research`, and its `evidence/` tree. No path under any other feature folder
and no path under `docs/features/potential/` belongs to this feature's own change set.

### Repository metadata (0)

This feature's own change set contains no `.claude/**` path. No agent-memory write is attributable to
this branch beyond the merged base.

---

## Diff against the recorded `BASELINE_SHA` (250 paths) — for completeness

This is the literal output of the command the task names. It is recorded in full classification below
so the merge-induced difference between the two readings is auditable rather than hidden.

| Class | Count | Attribution |
| --- | --- | --- |
| Production | 6 | 3 this feature, 3 from the merged base |
| Test | 11 | 3 this feature, 8 from the merged base |
| Project file | 1 | `QuickFiler.Test/QuickFiler.Test.csproj`, edited by this feature and by the merged base |
| Documentation and evidence | 225 | 71 this feature, 154 from the merged base |
| Repository metadata | 7 | 0 this feature, 7 from the merged base |

### Production (6)

| Path | Attribution |
| --- | --- |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | **this feature** |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs` | **this feature** |
| `QuickFiler/Viewers/IWebViewCoreInitializer.cs` | **this feature** |
| `QuickFiler/Controllers/KbdActions.cs` | merged base (feature 444) |
| `QuickFiler/Controllers/QfcCollectionController.cs` | merged base (feature 444) |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` | merged base (feature 444) |

The three merged-base production paths are absent from `base..HEAD`, which is the mechanical proof
that this feature did not touch them: if this branch had modified any of them, the two-dot diff
against the base would list it.

### Test (11)

| Path | Attribution |
| --- | --- |
| `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` | **this feature** |
| `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` | **this feature** |
| `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs` | **this feature** |
| `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` | merged base (444) |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs` | merged base (444) |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` | merged base (444) |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | merged base (444) |
| `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` | merged base (444) |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | merged base (444) |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | merged base (493) |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | merged base (493) |

`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` warrants a note because
`[P5-T18]` names it as one of the three in-repo callers that must not be modified by this feature. It
is modified in the `BASELINE_SHA` diff but **not** in `base..HEAD`: the modification arrived with the
merged base under feature 444's ownership, not from this branch.

### Project file (1)

`QuickFiler.Test/QuickFiler.Test.csproj`. This feature adds exactly two `Compile Include` lines,
`Viewers\WebView2BreadcrumbHostContractTests.cs` and `Viewers\WebView2BreadcrumbHostTests.cs`, both
inside the owned `Viewers\WebView2*` alphabetical prefix. The merged base's own additions to this
file (feature 493's two `Controllers\QfcItemController.UiThreadDispatcherFixture*` entries and
feature 444's eight `Controllers\QfcCollectionController*` entries) arrived intact and were not
reordered, replaced, or dropped; that was verified at merge time and recorded in
`evidence/qa-gates/base-merge-reconciliation.2026-08-27T23-09.md`.

### Documentation and evidence (225)

Distributed across four locations:

| Location | Paths | Attribution |
| --- | --- | --- |
| `docs/features/active/webview2-host-initializer-defects-476/` | 71 | **this feature** |
| `docs/features/active/quickfiler-keyboard-action-defects-444/` | 91 | merged base |
| `docs/features/active/quickfiler-test-uithread-dispatcher-493/` | 61 | merged base |
| `docs/features/potential/promoted/` | 2 | merged base |

### Repository metadata (7)

| Path |
| --- |
| `.claude/agent-memory/atomic-executor/MEMORY.md` |
| `.claude/agent-memory/atomic-executor/project_epic_integration_base_invalidates_research_line_counts.md` |
| `.claude/agent-memory/atomic-executor/project_msbuild_log_token_search_matches_csc_command_line.md` |
| `.claude/agent-memory/feature-review/MEMORY.md` |
| `.claude/agent-memory/feature-review/project_493-review-residuals-and-msbuild-log-gate-adjudication.md` |
| `.claude/agent-memory/orchestrator/MEMORY.md` |
| `.claude/agent-memory/orchestrator/potential-to-issue-keeps-only-summary-section.md` |

`.claude/agent-memory/**` is tracked in this repository, so an agent-memory write appears in a
repository-wide diff. Such a path is repository metadata: it is neither production nor test and does
not by itself fail any gate in this plan. All seven arrived with the merged base; none is attributable
to this branch.

---

## Files that must NOT appear, and do not

Checked against this feature's own change set (`base..HEAD`). None of the following is present:

- `QuickFiler/Controllers/EfcFormController.cs`
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
- `QuickFiler/Controllers/EfcItemController.cs`
- `QuickFiler/Viewers/WebView2Messenger.cs`
- `QuickFiler/Viewers/BreadcrumbMessengerHub.cs`
- `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`
- `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`
- `QuickFiler/QuickFiler.csproj`
- `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs`
- `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs`
- `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs`
- `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs`
- `QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs`
- `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`

The last of these is present in the `BASELINE_SHA` diff for the merged-base reason stated above and
absent from this feature's own change set.
