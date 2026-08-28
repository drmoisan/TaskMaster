# P0-T2 — Repository state at the remediation base (cycle 1)

Timestamp: 2026-08-28T03-40
Task: [P0-T2]
Command: git status --porcelain && git rev-parse HEAD && git diff --name-only cecd7813..HEAD
EXIT_CODE: 0

## Working-tree cleanliness

`git status --porcelain` printed **nothing** — zero lines of output. The tree is clean at the
remediation base.

## RemediationBaseCommit:

RemediationBaseCommit: d77ac2126ec62a37e18a9e20ef220571dc2e4ec2

### Why this differs from the planning-time expectation of `7ad2bd17`

The plan records `7ad2bd17` as HEAD at planning time and directs that the actual SHA be recorded if
HEAD differs. It differs, by exactly two commits, both of which are this cycle's own opening
bookkeeping and neither of which touches any source or project file:

| # | SHA | Contents |
|---|---|---|
| 1 | `899000d379f1f54b71aaf88cb3b5173a42509d37` | the approved remediation plan file, which arrived untracked and was committed as the first action so it could not be lost |
| 2 | `d77ac2126ec62a37e18a9e20ef220571dc2e4ec2` | the P0-T1 read artifact and the P0-T1 plan check-off |

Both are confined to `docs/features/active/itemviewer-surface-defects-489/`. A diff of
`7ad2bd17..d77ac212` restricted to everything outside `docs/` is empty, so the code tree at
REM_BASE is byte-identical to the code tree the plan was authored against, and every adopted
baseline in P0-T5 remains valid by tree identity.

**REM_BASE** in every later task of this plan means `d77ac2126ec62a37e18a9e20ef220571dc2e4ec2`.

## ScopeLockPathCount:

ScopeLockPathCount: 25

Measured as `git diff --name-only cecd7813..HEAD` filtered to paths ending in `.cs`, `.csproj`,
`.props`, `.targets`, `.config`. `cecd7813` is the feature's Phase 0 base commit.

## ScopeLockPaths:

```
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs
QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs
QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs
QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs
QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs
QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs
QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs
QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs
QuickFiler/Controllers/QfcItemController.EventHandlers.cs
QuickFiler/Controllers/QfcItemController.EventWiring.cs
QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs
QuickFiler/Controllers/QfcItemController.FolderHandling.cs
QuickFiler/Controllers/QfcItemController.MailActions.cs
QuickFiler/Viewers/IItemViewer.cs
QuickFiler/Viewers/ItemViewer.Designer.cs
QuickFiler/Viewers/ItemViewer.DisplayState.cs
QuickFiler/Viewers/ItemViewer.FolderSearch.cs
QuickFiler/Viewers/ItemViewer.cs
QuickFiler/Viewers/ItemViewerExpanded.Designer.cs
QuickFiler/Viewers/ItemViewerExpanded.cs
```

Both files this remediation edits are already present in the set — `QfcItemController.EventWiring.cs`
at row 15 and `QfcItemController.EventWiringTests.Part2.cs` at row 1 — so this cycle adds **no new
source path**, which is the condition P4-T9 re-verifies at the end.

None of the four explicitly prohibited paths appears in the set:
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`, `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`,
`QuickFiler/Controllers/QfcItemController.Navigation.cs`,
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`. No `UtilitiesCS/` path appears
either, so the P4-T9 "no `UtilitiesCS/` path beyond those already in the set" condition has an empty
allowance and reduces to "no `UtilitiesCS/` path at all".

## Acceptance

| P0-T2 condition | Result |
|---|---|
| `git status --porcelain` output is empty | **Yes** — zero lines |
| `RemediationBaseCommit:` is recorded | **Yes** — `d77ac2126ec62a37e18a9e20ef220571dc2e4ec2` |
| `ScopeLockPathCount:` and the full path list are recorded | **Yes** — 25 paths, listed verbatim above |

Output Summary: The working tree is clean — `git status --porcelain` printed zero lines.
`RemediationBaseCommit: d77ac2126ec62a37e18a9e20ef220571dc2e4ec2`, two doc-only commits ahead of the
planning-time `7ad2bd17`; the code tree is byte-identical between the two, so the P0-T5 adopted
baselines hold by tree identity. `ScopeLockPathCount: 25`, matching the expected figure exactly, with
both files this remediation will edit already inside the set and none of the four prohibited paths
present. `EXIT_CODE: 0`.
