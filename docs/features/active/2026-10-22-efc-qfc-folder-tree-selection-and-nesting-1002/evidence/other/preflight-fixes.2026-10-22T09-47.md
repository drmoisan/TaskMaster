# Preflight Fixes — Issue #1002

Timestamp: 2026-10-22T09-47
Plan: docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/plan.2026-10-22T05-08.md
Directive: PREFLIGHT VALIDATION ONLY (single pass; no task checked; no task added, removed, or reordered)

## Fixes applied in task text

1. [P0-T6] | unsatisfiable-by-ordering evidence (validator-blind) | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` writes its post-processed Cobertura document to the fixed default `coverage\coverage.cobertura.xml`, which the P9-T4 run overwrites; P9-T5 needs the baseline per-line data to compute changed-line coverage against "the baseline coverage of the lines it replaced". Added an instruction to copy the baseline document to `SCRATCH` as `issue-1002-baseline-cobertura.xml` (never committed). The P9-T5 assertion is unchanged.
2. [P3-T4] | count typo | Acceptance named "the eight `RenderRowFragment_` tests"; the spec Test Strategy, P2-T3, and P9-T15 name six. Changed to "six".
3. [P3-T9] | nullable compile defect (validator-blind) | `BreadcrumbInboundMessage.NodeId` is `string?` in a `#nullable enable` file; passing it to a `string` parameter raises CS8604, which TYPECHECK promotes to an error. Task text now specifies `message.NodeId!`, matching the existing `message.Key!` arm in `ProcessInboundAsync`. Behavior unchanged.
4. [P4-T1] | nullable compile defect (validator-blind) | `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` carries no `#nullable enable`; the literal `string? preferredRowText = null` raises CS8632, which TYPECHECK promotes to an error. Literal changed to `string preferredRowText = null`. Behavior unchanged.
5. [P9-T10] | ambiguous test reference | "the four `DescendantToggle_` router tests" matched five spec names (`DescendantToggle_LeafNode_IsRejectedWithoutProviderCall` belongs to AC6). Replaced with the four explicit test names.
6. [P9-T33] | unsatisfiable anchor (validator-blind) | "every criterion text unchanged against the merge base": `spec.md` was added on this branch and does not exist at the merge base of `origin/main` and `HEAD`. Changed the comparison to each AC line, with the checkbox marker normalized, against the same line of `spec.md` at `HEAD`. The asserted property (criterion text unchanged) is unchanged.

## Rule check on the fixes

- No fix cites a line number or commit identifier (G11, G12).
- No fix records a host path; the scratch copy is named by the `SCRATCH` token.
- No fix adds, removes, or reorders a task; every assertion keeps its meaning.

## Re-validation

- `mcp__drm-copilot__validate_orchestration_artifacts` (artifact_type `plan`) run after the fixes.
