# Remediation Inputs: Issue #1002 (review 2026-10-22T13-37, re-audit after cycle 1)

Review-Verdict: REMEDIATION_REQUIRED

Remediation-Mode: proportionate

- Feature folder: `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002`
- Branch head reviewed: `b72ac1dbbfdb6b45d01a14dde9e91428a6bf41ac`; merge base `8e4ad1ecbf50aae00fe2c092c22ae457f4feb7a7`
- Source audit artifacts:
  - `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/policy-audit.2026-10-22T13-37.md`
  - `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/code-review.2026-10-22T13-37.md`
  - `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/feature-audit.2026-10-22T13-37.md`
- Prior inputs: `remediation-inputs.2026-10-22T10-39.md`; its B-1, B-2, B-3 and B-4 are resolved (policy audit section 8).
- Next link: `atomic-planner` authors the cycle-2 remediation plan per `remediation-handoff-atomic-planner`; this review does not author the plan.
- Mode rationale: the single blocking finding is a text amendment to one acceptance criterion in `spec.md`; no source, test or evidence regeneration is required.

## Remediation-Required Findings

### R-1 AC23 text contradicts the ratified coverage exemption

- Severity: Blocking
Remediability: autonomous
Remediability-Evidence: the maintainer already ratified the five member-level attributes in the recorded B-3 decision of remediation-inputs.2026-10-22T10-39.md; aligning AC23 with that recorded decision is a clerical spec amendment with no code change. If the orchestrator rules that acceptance-criterion wording is maintainer-owned, reclassify this finding as human_decision_required.
- File: `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/spec.md`, `## Acceptance Criteria`, AC23 (line 298).
- Problem: AC23 reads "... changed lines lose no coverage, no coverage exclusion is added, and the repository line and branch figures are recorded ..." and is checked `[x]`. The branch adds five `[ExcludeFromCodeCoverage]` attributes (`EfcFormController.ActionOkAsync`, `ActionCancelAsync`, `CreateFolderAsync`, `RefreshSuggestionsAsync`, `ConfigureBreadcrumbControl`). The B-3 decision lifted the plan's prohibition for these members, but the spec was not amended, so the criterion is false as written.
- Expected behaviour after the fix: AC23 states the ratified exception explicitly, `spec.md` carries a dated amendment note citing the decision, and every clause of AC23 is true of the head tree.
- Fix:
  - Replace the clause "no coverage exclusion is added" in AC23 with "no coverage exclusion is added other than the five member-level `[ExcludeFromCodeCoverage]` attributes on `ActionOkAsync`, `ActionCancelAsync`, `CreateFolderAsync`, `RefreshSuggestionsAsync` and `ConfigureBreadcrumbControl` ratified by the maintainer under CLAUDE.md UT2 (remediation-inputs.2026-10-22T10-39.md, B-3 Maintainer decision); no `coverage.config` entry and no class-level exclusion is added". Change no other words of AC23 and no other criterion.
  - Add a dated amendment entry to `spec.md` (for example under a `## Planner Amendments` heading) recording the AC23 change, its authority (maintainer decision 2026-10-22T10:50Z) and its reason (cycle-0 finding B-3).
  - Keep AC23 checked; it is re-evaluated by the next review.
- Verification commands:
  - `git diff -- docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/spec.md` shows only the AC23 clause change and the amendment entry.
  - `git diff 8e4ad1ecb...HEAD -- '*.cs' | grep '^+' | grep -c ExcludeFromCodeCoverage` returns 5, matching the five members named in the amended text.
  - `git diff --stat 8e4ad1ecb...HEAD -- coverage.config` is empty.

## Recommended In-Scope Items (non-blocking)

- N-1: reword the exemption comment above `ActionCancelAsync` in `QuickFiler/Controllers/EfcFormController.Actions.cs` to name the concrete `EfcViewer` dependency (`UiSyncContext` awaiter and `Close`); the method calls no `MessageBox`. If taken, rerun the full C# toolchain in CLAUDE.md order.
- N-3: in `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs`, give the three `delivered.Task` waits a failure exit that does not use wall-clock time (for example, complete the `TaskCompletionSource` from the fault-report path), so a fault before delivery fails the test instead of leaving it waiting.
- N-5: optionally wrap bare `.child` text in `<span class="name">` in the bridge `subfolderResult` handler and the renderer's key-less branch so the truncation rule applies; the first path is transient and the second has no production caller.

## Non-Blocking Items

- O-1: the PR-context summary classifies every changed C# file as docs/tooling; language scope was derived from `git diff`.
- O-2: `ExcludeFromCodeCoverage` has no `Justification` property on net48; adjacent comments are used (CS0246 probe evidence).
- O-3: the manual Outlook check stays pending human interaction by design (AC26).
- O-4: skill layout conflicts recorded in cycle 0 are unchanged.

## Do Not Do

- Do not change any acceptance criterion other than the AC23 clause named in R-1; do not uncheck or add criteria.
- Do not add, remove or move any `ExcludeFromCodeCoverage` attribute or `coverage.config` entry.
- Do not edit files in the spec's excluded list (QFC router, selection session, coordinator, render projection, wiring, `BreadcrumbDescendantNode.cs`, `FolderBreadcrumb.html`, `Resources.resx`, `EfcDataModel.cs`).
- Do not extend the near-limit test files listed in the spec Constraints; keep every file at or under 500 lines.
- Do not add `ConfigureAwait(false)` or locks to `QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs`.
- Do not edit policy documents under `.claude/rules/` or `.github/instructions/`.
- Do not commit raw `.trx`, `.coverage` or Cobertura documents; evidence stays as Markdown projections under `evidence/`.
