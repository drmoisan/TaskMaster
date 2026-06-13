# com-vsto-coverage-exemption (Issue #197)

- Date captured: 2026-06-13
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/com-vsto-coverage-exemption/ (Issue #197)

- Issue: #197
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/197
- Last Updated: 2026-06-13
- Work Mode: full-feature

## Problem / Why

Repository-wide C# coverage cannot meaningfully reach the 80% policy floor because a large fraction of first-party production code is bound to the live Outlook COM object model and the WinForms form lifecycle, and cannot be unit-tested without a running Outlook process (prohibited by the unit-test policy). Re-measurement (post-#189, test assemblies excluded) shows production-only coverage of 58.95%, with the gap concentrated in COM/VSTO/WinForms code: TaskVisualization 0.37%, ToDoModel 10.43%, QuickFiler 25.15%, TaskMaster 25.68%. Without a formal exemption, the 80% target is unreachable regardless of test effort, and the metric does not distinguish genuinely-testable code from architecturally-untestable interop code.

Maintainer ratified pursuing this exemption (2026-06-13). Design basis: `artifacts/research/2026-06-12-com-vsto-coverage-exemption-design.md`.

## Proposed Behavior

Formally exempt Outlook-COM / VSTO / WinForms-bound code from the 80% coverage floor, so the remaining genuinely-testable first-party code can be held to a meaningful target, using a two-layer hybrid mechanism:

1. **Assembly-level exclude** for `TaskVisualization` (≈0.37% covered, 90–95% COM/WinForms) via `coverage.config` `ModulePaths/Exclude`.
2. **Class-level `[ExcludeFromCodeCoverage]`** on COM/VSTO/WinForms-bound classes in `QuickFiler`, `TaskMaster`, `ToDoModel`, and `Tags`, leaving genuinely-testable seams (e.g. `ToDoLoader`, `IDList.GetNextToDoID`, `KbdActions<>`, settings/path helpers, `TagController` pure-logic methods) NOT exempt.
3. **Policy documentation** updates in `CLAUDE.md` and `.claude/rules/general-unit-test.md` recording the COM/VSTO exemption rationale and the testable-denominator definition.

## Acceptance Criteria (early draft)

- [ ] `coverage.config` excludes the `TaskVisualization` module from instrumentation.
- [ ] `[ExcludeFromCodeCoverage]` applied to the enumerated COM/VSTO/WinForms classes in QuickFiler, TaskMaster, ToDoModel, Tags (per the design memo scope table); no exemption applied to the enumerated testable seams.
- [ ] `CLAUDE.md` and `.claude/rules/general-unit-test.md` record the exemption policy, rationale, and the testable-denominator target.
- [ ] Post-exemption coverage is re-measured and recorded; the C# toolchain (csharpier, msbuild analyzers, nullable, MSTest) passes.
- [ ] No production behavior change; attributes and config only.

## Constraints & Risks

- Must not exempt testable seams (would mask real gaps). The exempt/non-exempt boundary is enumerated in the design memo and must be reviewed.
- `[ExcludeFromCodeCoverage]` touches many production `.cs` files across four assemblies — large change budget; should be phased into reviewable batches.
- Policy-doc edits are authority-level; maintainer has ratified.
- Roadmap increment tests (raising covered code toward target) are a SEPARATE follow-up, not part of this exemption.

## Test Conditions to Consider

- [ ] Coverage re-measurement confirms exempt assemblies/classes are removed from the denominator and testable seams remain.
- [ ] C# build + analyzers + nullable + MSTest pass after annotation passes.
- [ ] No unintended behavioral or API change from attribute additions.

## Next Step

- [ ] Promote to GitHub issue (refactor template, full mode)
- [ ] Create active feature folder from the template