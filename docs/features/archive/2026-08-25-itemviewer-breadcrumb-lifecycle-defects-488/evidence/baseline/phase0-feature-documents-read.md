# Phase 0 — Feature Documents Read ([P0-T2])

Timestamp: 2026-08-28T05-09

Command: `grep -c '^- \[[ x]\] ' spec.md` and `grep -n 'Work Mode' issue.md`
EXIT_CODE: 0

## Resolved work mode

`full-bug`, taken from the persisted marker `- Work Mode: full-bug` at `issue.md:6`.

## Sole acceptance-criteria source

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/spec.md` is the sole
acceptance-criteria source for this feature. Under `full-bug`, `spec.md` is required and
`user-story.md` is intentionally absent; its presence would be an integrity failure.
`issue.md`'s `## Acceptance Criteria` section is a pointer only and carries no criteria of its own.

## Total acceptance-criterion count

**54** checkbox criteria, all currently unchecked. Measured by counting lines matching
`^- \[[ x]\] ` in `spec.md`, which is the file's only checkbox form. The count agrees with the
distribution the spec states in its own `## Acceptance Criteria` preamble: Process 2, D1 6, D2 5,
D3 6, D4 6, D5 4, #475 7, scope and ownership 6, file-size/toolchain/coverage/document-integrity 12
(2+6+5+6+6+4+7+6+12 = 54).

## Six defect units

| Label | Title | Owned surface |
| --- | --- | --- |
| D1 | Host replacement on WebView2 environment change disposes the outgoing host too late | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` |
| D2 | `SetBreadcrumbTheme` lost when the host post is deferred | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` |
| D3 | A second, different `IFolderHierarchyProvider` is silently discarded | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` |
| D4 | Non-atomic read-then-write on pipeline initialization; UI-thread affinity undeclared | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` |
| D5 | A `Container` created during teardown is never disposed | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` |
| #475 | `CaptureCurrentOrTests()` inverts the fail-fast guard | `BreadcrumbPopupUiOperations.cs`, `BreadcrumbDropDownHost.cs`, `ItemViewer.Breadcrumb.cs` |

## Documents read

- `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/issue.md` (3056 bytes) — READ.
  Records the work mode, the two issues closed (#488 primary, #475 also-closes), the epic context
  (`quickfiler-bug-family`, integration branch `epic/quickfiler-bug-family-integration`, wave 3), the
  upstream dependency on feature `itemviewer-surface-defects-489`, and the provenance note that both
  issues and their potential entries pre-date this feature folder.
- `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/spec.md` (83111 bytes) — READ.
  Authoritative inputs and precedence, line-citation anchor, context, repro and evidence per defect,
  scope and non-goals with the explicitly excluded file list, root-cause analysis, the proposed fix
  per unit with rationale and rejected alternatives, boundaries and invariants, the cross-fix
  interaction section, files and members to change, the test strategy including the reusable-harness
  table and the constraining-test table, the coverage-exemption section, the toolchain commands, the
  54 acceptance criteria, the four recorded divergences from the research, and the risk register.
- `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/research/2026-08-25T10-00-itemviewer-breadcrumb-lifecycle-defects-research.md`
  (70347 bytes) — READ. Method and the one unmet input, the current-state map with line-citation
  corrections and the `ItemViewer` coverage-exemption finding, the confirmed root cause per defect,
  the minimal-fix design per defect (sections 3.1 through 3.7 including the D5 faulted-task open item
  and the D1c follow-up recommendation), regression-test feasibility, the existing-test inventory and
  blast radius, file-size headroom, file ownership across concurrently prepared siblings, the 484
  upstream-contract citation, the `## Dependencies on 489` section, and the summary of
  recommendations.
- `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/research/2026-08-25T10-20-orchestrator-comment-crosscheck.md`
  (7506 bytes) — READ. Verifies the three claims of the #488 correction comment against source, records
  that no research conclusion is overturned, and records the two out-of-scope decisions (the
  `SetBridgeCoordinator` replace-without-dispose coupling, contingent on D3 staying fail-fast; and the
  `Reset()` synchrony mismatch, out of scope on ownership grounds because half of it lives in
  501-owned `BreadcrumbDropDownOpenCoordinator.cs`). It names the three follow-up candidates this
  plan's [P7-T5] must record as potential entries.

## Precedence recorded

`spec.md` governs over the research. The spec's `## Divergences from the research` section names four
divergences the executor must honour: the `QuickFiler.Test.csproj` item group is not alphabetically
ordered; five of the six research-named harnesses are `private` and unreachable from a new file; D1
requires a fresh pattern variable because the one bound in the same-environment guard is not
definitely assigned on the fall-through path; and the D1/D2 `ObjectDisposedException` interaction is
an accepted documented residual.

Output Summary: All four feature documents exist and were read. Work mode resolves to `full-bug`;
`spec.md` is the sole acceptance-criteria source and carries exactly 54 checkbox criteria, all
unchecked at this point. The six defect units are D1, D2, D3, D4, D5, and #475. `user-story.md` is
absent, which is correct for `full-bug` and is separately verified by [P7-T8].
