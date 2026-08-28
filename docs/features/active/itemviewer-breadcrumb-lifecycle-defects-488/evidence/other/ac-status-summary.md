# Acceptance-Criteria Status Summary ([P9-T16])

Timestamp: 2026-08-28T06-36

Command: checkbox counts over the acceptance-criteria source file.
EXIT_CODE: 0

### Acceptance Criteria Status

- **Source:** `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/spec.md`
- **Total AC items:** 54
- **Checked off (delivered):** 53
- **Remaining (unchecked):** 1
- **Items remaining:**

```
The research §3.5 open item is discharged with recorded evidence: it is confirmed whether a faulted
`QfcItemController.InitializeWebViewAsync` task is observed by its caller. If it is not observed, a
new issue is opened against `QfcItemController.ViewerSetup.cs` (484-owned) and referenced here —
**the guard is not weakened in response.**
```

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
| D5 — `Container` created during teardown | 4 | **3** |
| #475 — `CaptureCurrentOrTests()` | 7 | 7 |
| Scope, ownership, and the 489 dependency | 6 | 6 |
| File size, toolchain, coverage, document integrity | 12 | 12 |
| **Total** | **54** | **53** |

The single gap is in the D5 section.

## Status of the remaining item

Two of its three clauses are delivered and one is not:

- **Delivered** — the open item is discharged with recorded evidence. `[P5-T6]` enumerated every in-repo
  caller of `QfcItemController.InitializeWebViewAsync` and concluded the faulted task is **not**
  observed: three of its four production call sites discard it (`Initialization.cs:192`, `:288`,
  `:324`) and only `:256` awaits it.
- **Delivered** — the guard is **not** weakened in response. D5's `ObjectDisposedException` throw is
  unchanged, and `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` was read but not edited,
  confirmed by `[P7-T3]`'s empty forbidden-file diff.
- **Not delivered** — the GitHub issue. The follow-up is prepared as
  `docs/features/potential/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved.md`, but promotion
  is blocked: `.claude/hooks/enforce-promotion-mcp-only.ps1` forbids `gh issue create` and requires MCP
  promotion tools that are not in this executor's tool set. The forbidden path was not used.

Because this criterion names an **issue** specifically, a potential entry alone does not satisfy it, and
it is left `- [ ]` rather than checked without evidence.

## Integrity of the check-offs

No criterion text was modified. `git diff --numstat` on `spec.md` reports `53 53`, every changed line is
a checkbox line, and stripping the checkbox prefix from both sides of the diff yields **identical**
text sets — only the single character inside the brackets changed on each line. No criterion was added,
removed, reworded, or reordered.

Output Summary: **53 of 54** acceptance criteria are checked off in
`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/spec.md`, with **1 remaining**, quoted
verbatim above. The remaining item is the research §3.5 criterion, whose discharge and
guard-not-weakened clauses are delivered but whose required GitHub issue could not be opened because the
MCP-only promotion tools are unavailable to this executor. No criterion text was modified.
