# P0-T2 — Full-Bug Mode Preconditions and Spec Amendment Verification

Timestamp: 2026-09-17T02-08

Command: read-only verification with the Grep and Glob tools over
`docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/issue.md`
and
`docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/spec.md`.
No file was written by this task.

EXIT_CODE: 0

CHANNEL: NONE

## Output Summary

All six required conditions hold. `MODE PRECONDITION FAILED` was not reached.

### Condition 1 — work-mode marker

Pattern `^- Work Mode: full-bug$` over `issue.md`, one match:

    4:- Work Mode: full-bug

RESULT: the exact line `- Work Mode: full-bug` is present. PASS.

### Condition 2 — acceptance-criteria heading

Pattern `^## Acceptance Criteria$` over `spec.md`, one match at line 272. The match is anchored at
both ends, so the heading text is exactly `## Acceptance Criteria`. PASS.

### Condition 3 — acceptance-criteria inventory, box-state independent

Pattern `^- \[[ x]\] AC[1-8]\. ` over `spec.md`, 8 matching lines:

    273:- [ ] AC1. `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` and
    278:- [ ] AC2. Each rewritten test explicitly establishes the distinct-thread precondition
    281:- [ ] AC3. Each rewritten test asserts the captured exception is exactly `InvalidOperationException`
    285:- [ ] AC4. A `fail-before-exception.<timestamp>.md` dossier is recorded under
    288:- [ ] AC5. A deterministic guard-disabled failing run of the two *replacement* tests (via a temporary,
    292:- [ ] AC6. Both rewritten tests pass under `scripts/vscode/TaskMaster.cli.runsettings` (`Workers=0`,
    295:- [ ] AC7. No sibling test in `ItemViewerBreadcrumbThreadAffinityTests.cs` regresses (all 7
    297:- [ ] AC8. Full C# toolchain pass completed in order (CSharpier format → .NET analyzers/EnforceCodeStyleInBuild

COUNT: 8. Every one of the eight begins `- [ ] `; none begins `- [x] `. The inventory regex admits
both box states, so the count is independent of check-off, and the observed uniform `- [ ] ` prefix
establishes that no acceptance criterion was checked before execution started. PASS.

### Condition 4 — user-story.md absent

Glob `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/*.md`
returned exactly three paths: `issue.md`, `spec.md`, `plan.2026-09-16T23-27.md`. `user-story.md` is
not among them and does not exist. This matches the full-bug expectation that `spec.md` is the sole
acceptance-criteria source. PASS.

### Condition 5 — planner amendment markers and the corrected mechanism citation

Pattern `amended 2026-09-17 during planning` over `spec.md`, occurrence-mode output, 5 occurrences,
one per line at lines 6, 40, 64, 231, 316.

- Line 6 is the header sentence that names the marker.
- Lines 40, 64, 231 and 316 are the four in-place amendment markers.

COUNT: 5, exactly as required.

Pattern `ItemViewer\.Breadcrumb\.cs:80` over `spec.md`, 1 occurrence at line 241, which is at least
one as required. That citation is the corrected throw site for the two-argument
`InitializeBreadcrumbPipeline` overload: `BreadcrumbUiDispatcher.CaptureCurrent()` evaluated as a
`BreadcrumbBridgeCoordinator` constructor argument. PASS.

### Condition 6 — superseded-wording parenthetical

Pattern `immediately after constructing each` over `spec.md`, 1 occurrence at line 232, inside the
parenthetical that records the superseded mutation placement. COUNT: 1, exactly as required. PASS.

### Scope note

This task is read-only. `spec.md` was not edited here and is edited later only to flip an acceptance
criterion checkbox from `- [ ]` to `- [x]`, per the plan's Write Set.
