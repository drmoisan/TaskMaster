---
epic: review-residuals-2026-09-08
integration_branch: epic/review-residuals-2026-09-08-integration
created_at: 2026-09-08T23-55
intent:
  epic_type: enabler
  business_outcome_hypothesis: >-
    Clearing the eight residual defects that the bugs-2026-09-06 run left behind removes the
    latent QuickFiler teardown and UtilitiesCS concurrency failures that would otherwise
    resurface, and restores a reproducible first-party coverage figure so every subsequent
    quality gate measures the quantity it claims to measure.
  leading_indicators:
    - A second Cleanup call on QfcHomeController invokes the ribbon-release callback zero
      additional times, proven by a test whose assertion follows both calls.
    - Clicking Cancel on ProgressViewer raises no exception when the shared token source is
      null or already disposed.
    - The reported first-party branch coverage for a Cobertura report equals an independent
      de-duplicated recomputation from that same report.
    - No test class leaves Console.Out pointing at a writer it installed and never restored.
    - AssignFolderComboBox degrades to no preselection, rather than throwing on the UI
      dispatcher, when the archive root is unset.
  nfrs:
    - No coverage threshold, analyzer severity, or policy requirement may be lowered,
      weakened, or deleted in order to make a gate pass.
    - Promoting RS0030 must not break the build on the roughly 143 pre-existing
      banned-symbol usages tracked by issue #181.
    - The intermittent test recorded as R5 in issue #823 must not be stabilized with a
      sleep, a retry, or a timing tolerance.
    - No production file may be added to a coverage exclusion list to protect a threshold.
features:
  - issue_num: 813
    feature_folder: assignfoldercombobox-unguarded-archiverootpath-read
    depends_on: []
  - issue_num: 815
    feature_folder: coverage-aggregation-double-counts-method-rows
    depends_on: []
  - issue_num: 817
    feature_folder: utilitiescs-test-hygiene-residuals
    depends_on: []
  - issue_num: 821
    feature_folder: qfchomecontroller-parentcleanup-double-ribbon-release
    depends_on: []
  - issue_num: 823
    feature_folder: quickfiler-teardown-review-residuals
    depends_on: []
  - issue_num: 824
    feature_folder: ilglobals-loadopcodes-unsynchronised-static-race
    depends_on: []
  - issue_num: 825
    feature_folder: etl-deadline-mechanics-follow-ups
    depends_on: []
  - issue_num: 826
    feature_folder: console-out-aggressors-and-banned-symbol-promotion
    depends_on: [825]
---

# Epic: Review Residuals 2026-09-08

## Goal

Close the eight open bug issues that the reviews of the `bugs-2026-09-06` parallel run filed on
2026-09-08 and that no subsequent item picked up. Every one of them was deliberately excluded
from the item that surfaced it, in order to hold that item's blast radius to a bugfix. They are
therefore known, located, and unowned, and they will be rediscovered by every future review of
these subsystems until they are delivered.

## Source

The eight issues were promoted from potential records captured on 2026-09-08 during the reviews
and executions of items 809, 810, 811 and 812:

| Issue | Origin item | Promoted record |
| --- | --- | --- |
| 813 | 812 review | `docs/features/potential/promoted/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read.md` |
| 815 | 809 review | `docs/features/potential/promoted/2026-09-08-coverage-aggregation-double-counts-method-rows.md` |
| 817 | 809 `[P4-T5]` gate | `docs/features/potential/promoted/2026-09-08-utilitiescs-test-hygiene-residuals.md` |
| 821 | 810 review | `docs/features/potential/promoted/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release.md` and `.../2026-09-08-progressviewer-cancel-suppressed-null-check-fourth-sharer.md` |
| 823 | 810 and 812 reviews | `docs/features/potential/promoted/2026-09-08-quickfiler-teardown-review-residuals.md` |
| 824 | 811 AC4 | `docs/features/potential/promoted/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race.md` |
| 825 | 811 | `docs/features/potential/promoted/2026-09-08-etl-deadline-mechanics-follow-ups.md` |
| 826 | 811 | `docs/features/potential/promoted/2026-09-08-console-out-aggressors-and-banned-symbol-promotion.md` |

All nine promoted records existed only on the unpushed local session branch
`TaskMaster-wt-2026-09-06T17-16` and were absent from `origin/main`. Commit `a2d766c7` on this
integration branch restores them, so every preparation child branched from here can resolve a
promotion source. Issues 824, 825 and 826 additionally still carried an unpromoted duplicate
under `docs/features/potential/` on `main`; the same commit removes each duplicate in favour of
its promoted copy, which supersedes it byte-for-byte apart from the promotion metadata block.

## Scope

Eight open bug issues, one independently mergeable child feature each:

| Feature | Issue | Primary surface | Wave | Complexity |
| --- | --- | --- | --- | --- |
| `assignfoldercombobox-unguarded-archiverootpath-read` | 813 | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 0 | C2 |
| `coverage-aggregation-double-counts-method-rows` | 815 | `scripts/vscode/Invoke-MSTestWithCoverage.*.ps1` | 0 | C3 |
| `utilitiescs-test-hygiene-residuals` | 817 | `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` | 0 | C2 |
| `qfchomecontroller-parentcleanup-double-ribbon-release` | 821 | `QuickFiler/Controllers/QfcHomeController.cs`, `UtilitiesCS/Threading/ProgressViewer.cs` | 0 | C3 |
| `quickfiler-teardown-review-residuals` | 823 | `UtilitiesCS/OutlookObjects/Store/StoreWrapperController*.cs`, `QuickFiler/Viewers/Breadcrumb*` | 0 | C3 |
| `ilglobals-loadopcodes-unsynchronised-static-race` | 824 | `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/` | 0 | C3 |
| `etl-deadline-mechanics-follow-ups` | 825 | `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.*`, `UtilitiesCS/Threading/TimeOutTask.cs` | 0 | C3 |
| `console-out-aggressors-and-banned-symbol-promotion` | 826 | 33 test classes across six test projects, `.editorconfig`, `BannedSymbols.txt` | 1 | C3 |

## Non-Goals

1. **Issue 816** (`uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement`) is open
   and was promoted from the same 2026-09-08 batch, but it is not in this epic's scope. It was
   not named in the epic objective and no child depends on it.
2. **R6 of issue 823** — the plan-authoring guidance amendment in the `atomic-plan-contract`
   skill — is out of scope. Everything under `.claude/` in this repository is pushed down from
   the separate `drm-copilot` governance repository with no templating, so a fix applied here is
   overwritten by the next push-down. The correct fix is an upstream change in `drm-copilot`.
   Issue 823's other five entries are in scope.
3. **R5 of issue 823** is a flake watch, not a fix. The feature records observations; it does not
   stabilize the test.
4. **R2 of issue 823** is a decision, not a code change. The feature records the decision on
   whether the relocated throw from item 818 is the intended end state.
5. **Reducing any threshold to accommodate a corrected coverage denominator.** Issue 815 corrects
   the arithmetic; if the corrected figure falls below a threshold, that is a finding to record,
   not a threshold to lower.

## Shared Design

Three constraints apply to every child.

**One file is shared and must be partitioned.**
`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` is edited by two features.
Feature 825 owns the 2000 ms `GetTableInViewAsync` deadline window and the method's timeout
mechanics. Feature 826 owns the two `Console.WriteLine($"Task timed out on try {counter}")`
diagnostics at lines 79 and 97. Both sites sit inside the same method, `GetTableInViewAsync`
(lines 32 to 119), which is why 826 carries the epic's single `depends_on` edge and executes in
wave 1 against a `TableAccess.cs` that already contains 825's changes. No other file is shared
between any two features.

**Project files are contended but are not a dependency.**
Every project in scope is legacy non-SDK MSBuild with explicit `<Compile Include=` entries:
`QuickFiler.csproj` (138), `UtilitiesCS.csproj` (491), `UtilitiesCS.Test.csproj` (470) and
`QuickFiler.Test.csproj` (172). Any feature that adds a new test file must add a `Compile
Include` entry, so several features may touch the same `.csproj`. This is contention, not a
contract: it does not gate a launch and it must never be expressed as a `depends_on` edge.
Resolve it by unioning the item lists at merge. Feature 817 is the largest such contributor
because splitting `FolderPredictorTests.cs` creates several new files in `UtilitiesCS.Test`.

**Every issue carries `- Work Mode: full-bug`.** Each child therefore produces `spec.md`;
`user-story.md` is absent by default unless the requirements explicitly justify one.

## Decomposition Rationale

The decomposition is one feature per issue. Two decomposition rules were applied and both
resolved the same way.

*Merge rather than serialize when two issues edit the same expressions.* No two issues in this
set do. Issue 821 already consolidates what was filed as 821 and 822 for exactly this reason —
its two sites are one enumeration pass over the same defect class — and issue 822 is closed as a
result. Issues 823, 825 and 826 are each already a batched residuals record, so further merging
would only widen blast radius without removing a conflict.

*Add a `depends_on` edge only for a real upstream contract or an unavoidable same-region
overlap.* Exactly one edge qualifies: 826 on 825, for the shared `GetTableInViewAsync` method
described above. Every other pairing is file-disjoint. A repo-wide search for `Console.SetOut(`
returns 33 test files across `QuickFiler.Test`, `UtilitiesCS.Test`, `TaskMaster.Test`,
`ToDoModel.Test` and `VBFunctions.Test`, and none of those 33 files is touched by any other
feature in this epic, so feature 826's large test-side blast radius creates no additional edge.

## Waves

```
wave(f) = 0                                       if depends_on(f) is empty
wave(f) = 1 + max(wave(d) for d in depends_on(f))  otherwise
```

- **Wave 0** — 813, 815, 817, 821, 823, 824, 825 (seven features, no dependencies).
- **Wave 1** — 826 (depends on 825).

The graph is cycle-free: only one edge exists, and it points from 826 to 825, which has no
outgoing edges. Every `depends_on` entry resolves to an `issue_num` present in `features[]`.

## In-Flight Work Check

Performed before decomposition, per the requirement that candidate target files be diffed
against unmerged work rather than only against `origin/main`:

- Eight `bug/*` branches from the `bugs-2026-09-06` run hold live worktrees. Seven are ancestors
  of `origin/main`. The eighth, `bug/utilitiescs-archive-root-and-user-email-retry-812`, is not
  an ancestor only because it received documentation commits after its pull request #818 merged;
  `git diff --stat origin/main...origin/bug/utilitiescs-archive-root-and-user-email-retry-812 --
  '*.cs' '*.csproj'` is empty, so it holds no unmerged source change.
- No unmerged branch relocates, renames or rewrites any file named by any of the eight issues.
- The stalled `epic/quickfiler-per-file-coverage-integration` family is between 65 and 126
  commits behind its own integration branch and is not scheduled. It is recorded here as a
  future merge-order consideration for `QuickFiler.Test`, not as a blocker on this epic.

Conclusion: this integration branch, cut from `origin/main` at `6f08302a`, is a valid base for
all eight features.

## Execution Authorization

No child in this epic edits a policy document under `.claude/rules/` or `.github/instructions/`,
so the `policy-compliance-order` hard constraint is not suspended for any feature.

One child edits build configuration. Feature 826 changes `dotnet_diagnostic.RS0030.severity` at
`.editorconfig` line 548 and extends the seven-entry `BannedSymbols.txt` at the repository root
to cover `CancelAfter`, `TimeoutAfter`, `WaitOne` and `new CancellationTokenSource(int)`. The
severity is currently held at `suggestion` because roughly 143 pre-existing usages would
otherwise break the build (issue #181). Running `/epic-run review-residuals-2026-09-08`
authorizes those two configuration edits. It does not authorize breaking the build to obtain
them: if the promotion cannot be staged without failing the analyzer gate, feature 826 records
the constraint and delivers the reachable subset rather than lowering any other gate to
compensate.
