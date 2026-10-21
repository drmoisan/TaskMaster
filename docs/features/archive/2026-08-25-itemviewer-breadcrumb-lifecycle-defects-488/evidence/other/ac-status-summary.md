# Acceptance-Criteria Status Summary ([P9-T16])

Timestamp: 2026-08-28T06-42

Command: checkbox counts over the acceptance-criteria source file.
EXIT_CODE: 0

> Revision note. This artifact was first written at 2026-08-28T06-36 recording **53 of 54**, with the
> research §3.5 criterion outstanding because the executor's tool set did not include the MCP promotion
> tools. The orchestrator subsequently ran the approved promotion path and opened issue **#670**, so the
> counts below are revised to **54 of 54**. The superseded record and the reason for the original gap
> are preserved in `evidence/other/ac-reconciliation.md` and
> `evidence/qa-gates/d5-faulted-task-observation.md`.

### Acceptance Criteria Status

- **Source:** `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/spec.md`
- **Total AC items:** 54
- **Checked off (delivered):** 54
- **Remaining (unchecked):** 0
- **Items remaining:** none

## Work mode and source resolution

`issue.md` carries `- Work Mode: full-bug`, so `spec.md` is the **sole** acceptance-criteria source and
`user-story.md` is intentionally absent. `[P7-T8]` confirmed that absence with a recorded negative-
evidence search, and confirmed that `issue.md` carries **0** checkboxes of its own, so no second
document competes as a source.

## Distribution of the 54 criteria

| Section | Count | Checked |
| --- | --- | --- |
| Process | 2 | 2 |
| D1 — host replacement on environment change | 6 | 6 |
| D2 — `SetBreadcrumbTheme` lost when the post is deferred | 5 | 5 |
| D3 — a second, different `IFolderHierarchyProvider` | 6 | 6 |
| D4 — UI-thread affinity | 6 | 6 |
| D5 — `Container` created during teardown | 4 | 4 |
| #475 — `CaptureCurrentOrTests()` | 7 | 7 |
| Scope, ownership, and the 489 dependency | 6 | 6 |
| File size, toolchain, coverage, document integrity | 12 | 12 |
| **Total** | **54** | **54** |

There is no remaining gap.

## How the last criterion was closed

The research §3.5 criterion is a conjunction of three clauses. All three are delivered:

- **The open item is discharged with recorded evidence.** `[P5-T6]` enumerated every in-repo caller of
  `QfcItemController.InitializeWebViewAsync` and concluded the faulted task is **not** observed: three
  of its four production call sites discard it (`Initialization.cs:192`, `:288`, `:324`) and only
  `:256` awaits it. The two `EfcItemController` sites discard theirs as well.
- **A new issue is opened against `QfcItemController.ViewerSetup.cs` and referenced here.** GitHub issue
  **[#670](https://github.com/drmoisan/TaskMaster/issues/670)** — "Bug:
  qfc-initializewebviewasync-fault-is-unobserved" — is OPEN, verified with
  `gh issue view 670 --json number,title,state,url`. It is referenced by number and URL in the D5
  section of `spec.md` and in `evidence/qa-gates/d5-faulted-task-observation.md`. The promoted record is
  `docs/features/potential/promoted/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved.md`.
- **The guard is not weakened in response.** D5's `ObjectDisposedException` throw is unchanged, and
  `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` was read but not edited, confirmed by
  `[P7-T3]`'s empty forbidden-file diff.

The issue was opened through the approved path — `mcp__drm-copilot__potential_to_issue` — by the
orchestrator, which holds the promotion tool set the executor lacked. `gh issue create` was not used and
nothing was reworded to evade `enforce-promotion-mcp-only.ps1`.

## Integrity of the check-offs

No criterion text was modified. Every check-off changed only the single character inside the brackets on
its own line: the executor's fifty-three (`git diff --numstat` on `spec.md` reported `53 53`, every
changed line a checkbox line, and stripping the checkbox prefix from both sides of the diff yielded
identical text sets), plus the orchestrator's one. The total remains exactly **54**. No criterion was
added, removed, reworded, or reordered.

One non-criterion paragraph was added to the spec's D5 design section recording the discharge and the
#670 reference, because the criterion requires the issue to be "referenced here". That paragraph is
prose, not a checkbox, and does not change the criterion count.

Output Summary: **54 of 54** acceptance criteria are checked off in
`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/spec.md`, with **0 remaining**. The
final gap — the GitHub issue required by the research §3.5 criterion — was closed by promoting the
executor's prepared potential entry through the approved MCP path to issue **#670**, which is OPEN and
referenced from both `spec.md` and the D5 evidence artifact. No criterion text was modified.
