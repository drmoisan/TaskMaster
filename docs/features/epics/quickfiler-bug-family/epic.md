---
epic: quickfiler-bug-family
integration_branch: epic/quickfiler-bug-family-integration
created_at: 2026-08-24T00-45
intent:
  epic_type: enabler
  business_outcome_hypothesis: Drain the QuickFiler open-bug backlog from 48 issues to zero by fixing defects in owning-class groups, so the QuickFiler surface stops generating follow-on defects during unrelated feature work.
  leading_indicators:
    - Open bug issues whose blast radius touches QuickFiler or QuickFiler.Test falls from 48 to 0
    - No new QuickFiler defect is promoted as a side effect of an epic child
    - QuickFiler and QuickFiler.Test line coverage does not regress against the pre-epic baseline
  nfrs:
    - Every child adds a failing regression test before its fix, per the Bugfix Workflow in CLAUDE.md
    - No child edits QuickFiler.csproj or QuickFiler.Test.csproj outside the alphabetical region it owns
    - Determinism rules in general-unit-test.md apply; no real wall-clock waits in added tests
features:
  - issue_num: 468
    feature_folder: qfc-collection-controller-defects-468
    depends_on: []
  - issue_num: 484
    feature_folder: qfc-item-controller-defects-484
    depends_on: []
  - issue_num: 501
    feature_folder: breadcrumb-coordinator-hub-defects-501
    depends_on: []
  - issue_num: 476
    feature_folder: webview2-host-initializer-defects-476
    depends_on: []
  - issue_num: 498
    feature_folder: breadcrumb-router-navigation-defects-498
    depends_on: []
  - issue_num: 442
    feature_folder: quickfiler-home-controller-metrics-442
    depends_on: []
  - issue_num: 446
    feature_folder: quickfiler-bug-family-446
    depends_on: []
  - issue_num: 493
    feature_folder: quickfiler-test-uithread-dispatcher-493
    depends_on: []
  - issue_num: 444
    feature_folder: quickfiler-keyboard-action-defects-444
    depends_on: [468]
  - issue_num: 464
    feature_folder: efc-controller-surface-defects-464
    depends_on: [484, 444]
  - issue_num: 489
    feature_folder: itemviewer-surface-defects-489
    depends_on: [484, 444]
  - issue_num: 488
    feature_folder: itemviewer-breadcrumb-lifecycle-defects-488
    depends_on: [489]
---

# Epic: QuickFiler Bug Family

## Goal

Close the 48 open `bug`-labelled GitHub issues whose blast radius touches `QuickFiler/` or
`QuickFiler.Test/`, grouped into 12 child features by **owning production class** rather than one
feature per issue.

## Scope

Intake was the 78 open `bug` issues on `main` at 988e819b, cross-checked against the disposition
record produced by the earlier parallel-planning run. Two exclusion sets were applied, then the
remainder was classified by the file paths named in each issue's promoted potential document.

**Excluded, not fixable in this checkout (6):** #589, #555, #554, #546, #536, #513. These target
`drm-copilot` MCP tools or `scripts/dev_tools/`, neither of which exists in TaskMaster.

**Excluded, CI and coverage ordering chain (6):** #565, #564, #563, #562, #561, #569. This cluster
carries real internal ordering (the coverage-threshold contradiction must be reconciled before any
gate is built against it) and belongs to its own epic.

**Excluded, other families (18):** the UtilitiesCS, UtilitiesCS.Test, TaskMaster Ribbon, and
coverage-tooling issues, whose fixes write no file under `QuickFiler/` or `QuickFiler.Test/`.

**In scope (48):** #286, #426, #427, #439, #440, #442, #443, #444, #446, #448, #451, #458, #459,
#460, #461, #462, #463, #464, #465, #466, #467, #468, #469, #470, #471, #472, #473, #474, #475,
#476, #477, #480, #481, #482, #483, #484, #485, #486, #487, #488, #489, #490, #493, #498, #499,
#500, #501, #502.

## Non-Goals

- No refactoring beyond the minimal targeted fix each issue requires. Deeper design problems found
  during a child run are promoted as new issues, not absorbed into scope.
- No coverage-uplift work. Coverage must not regress, but raising it is a separate effort.
- No change to `QuickFiler.csproj` or `QuickFiler.Test.csproj` beyond adding a `Compile Include`
  entry for a genuinely new file.

## Decomposition Rationale

One feature per issue is the wrong unit here. Many of these issues edit adjacent lines of the same
partial class, so 48 separate branches would collide at fan-in on files that only one feature can
sensibly own. The decomposition therefore assigns each **production source file exactly one owning
feature**, derived from a WRITE-versus-CITE reading of all 48 promoted potential documents: a path
counts as owned only when the document's suggested-fix section would modify it, not when the
document merely cites it as a caller or as evidence.

Where a file is genuinely written by issues that landed in different features, the relationship is
expressed as a `depends_on` edge so the later feature branches from an integration branch that
already carries the earlier change. Five such relationships exist, producing four edges, and no
edge was added for stylistic reasons.

| Shared file | Owning feature | Dependent feature | Edge |
| --- | --- | --- | --- |
| `QfcCollectionController.cs` | 468 | 444 via #444 and #472 | 444 depends_on 468 |
| `QfcItemController.ViewerSetup.cs` | 484 | 464 via #463 | 464 depends_on 484 |
| `QfcItemController.EventWiring.cs` and `.FocusAndTheme.cs` | 484 | 489 via #486 and #489 | 489 depends_on 484 |
| `KbdActions.cs` | 444 | 464 via #459, 489 via #482 | 464 and 489 depend_on 444 |
| `ItemViewer.Designer.cs` and `ItemViewer.Breadcrumb.cs` | 489 | 488 | 488 depends_on 489 |

## Wave Layering

Computed by longest-path layering: `wave(f) = 0` when `depends_on` is empty, else
`1 + max(wave(d))`. The graph is cycle-free and every `depends_on` entry resolves to another entry
in `features[]`.

| Wave | Features |
| --- | --- |
| 0 | 468, 484, 501, 476, 498, 442, 446, 493 |
| 1 | 444 |
| 2 | 464, 489 |
| 3 | 488 |

## Complexity Assessment

Bands follow the `model_policy` scale in `config/orchestration-routing.json`. Nearly every child
modifies a concurrency, ordering, or state-transition invariant, or a contract consumed across
module boundaries. Both are floor-forcing signals, so C3 dominates.

| Feature | Band | Rationale |
| --- | --- | --- |
| 468 qfc-collection-controller-defects | C3 | About 25 enumerated defects in one controller plus `IQfcCollectionController` and `IFilerFormController` contract changes |
| 484 qfc-item-controller-defects | C3 | Event-unwiring and cleanup-timer lifecycle invariants across four `QfcItemController` partials |
| 501 breadcrumb-coordinator-hub-defects | C3 | Upgrade-lifetime lock, superseded lease, and cache-before-broadcast ordering defects |
| 476 webview2-host-initializer-defects | C3 | Unmarshalled SDK call and unsynchronized state, plus the `IWebViewCoreInitializer` contract |
| 498 breadcrumb-router-navigation-defects | C3 | Spans `QuickFiler`, the `UtilitiesCS` folder model, and the `FolderBreadcrumb.html` asset |
| 442 quickfiler-home-controller-metrics | C3 | Metric flush is a state-transition invariant across both Qfc and Efc home controllers |
| 446 quickfiler-queue-datamodel-defects | C3 | Queue deadline and dequeue-confidence ordering; silent data loss is the failure mode |
| 493 quickfiler-test-uithread-dispatcher | C2 | Localized test-harness dispatcher restore; re-band to C3 if the fix reaches `UtilitiesCS/Threading/UiThread.cs` |
| 444 quickfiler-keyboard-action-defects | C3 | `KbdActions` is consumed by both the collection and item controllers |
| 464 efc-controller-defects | C3 | Async-void boundary, null guards, and timer leaks across the EFC controller set |
| 489 item-viewer-defects | C3 | UI-thread marshalling divergence between viewer and controller |
| 488 item-viewer-breadcrumb-pipeline | C3 | Breadcrumb pipeline lifecycle spanning viewer, host, and coordinator |

## Shared-Surface Coordination

**Project files.** Both project files are legacy non-SDK with explicit `<Compile Include>` items:
`QuickFiler.csproj` carries 125 entries in the item group spanning lines 287 to 464, and
`QuickFiler.Test.csproj` carries 117 entries spanning lines 57 to 175. Both item groups are
**alphabetically ordered**, and each feature owns a distinct class-name prefix (`Breadcrumb`,
`Efc`, `ItemViewer`, `Qfc`, `WebView2`), so insertions land in disjoint line ranges and no
dependency edge is needed for the project files.

`QuickFiler.Test/` already carries 117 test files covering essentially every affected area, so the
common case is adding a test method to an existing file and touching no project file at all. A
child must add a `Compile Include` entry **only** when it creates a genuinely new test file, and
then only within its own alphabetical neighbourhood.

**In-flight branches.** Four branches were reported as overlapping this surface. All four were
checked and none is a hazard:

- `bug/quickfiler-test-form1-live-form-491-exec` and
  `bug/winformspumphost-suite-determinism-511-exec` are already merged into `main`, 0 commits ahead.
- `feature/quickfiler-per-file-coverage-capstone-r2` at 56 commits ahead and
  `feature/quickfiler-breadcrumb-bridge-coverage-r2` at 58 commits ahead contain **zero** `.cs` and
  `.csproj` changes. Their diffs are additions under `.claude/agent-memory/` and `docs/features/`,
  plus 14 modifications all under `.claude/agent-memory/`.

No epic child is sequencing-blocked behind any of them, including #446, #448 and #469.

## Requirements Source

Every in-scope issue has a promoted potential document on `origin/main` under
`docs/features/potential/promoted/<date>-<slug>.md`, where the slug derives from the issue title
with the `Bug: ` prefix stripped, lowercased, with non-alphanumeric runs replaced by hyphens. Those
documents carry the `file:line`, the offending code block, the root cause, a suggested fix, and a
severity. They are the authoritative requirements source and are richer than the GitHub issue
bodies. Child runs read the promoted document, not only `gh issue view`.
