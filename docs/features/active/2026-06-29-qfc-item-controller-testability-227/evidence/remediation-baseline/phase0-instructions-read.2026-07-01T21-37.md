# Phase 0 — Instructions and Authoritative Inputs Read (Cycle-2 Remediation)

Timestamp: 2026-07-01T21-37

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific toolchain and standards)

Files read (policy):
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-29-09-38\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-29-09-38\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-29-09-38\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-29-09-38\.claude\rules\csharp.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-29-09-38\.claude\rules\tonality.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-29-09-38\.claude\rules\ci-workflows.md

Files read (authoritative inputs per plan §Authoritative Inputs):
- docs/features/active/2026-06-29-qfc-item-controller-testability-227/remediation-plan.2026-07-01T00-30.md (plan of record)
- docs/features/active/2026-06-29-qfc-item-controller-testability-227/spec.md
- docs/features/active/2026-06-29-qfc-item-controller-testability-227/issue.md
- docs/features/active/2026-06-29-qfc-item-controller-testability-227/maintainer-decision.2026-07-01.md
- docs/features/active/2026-06-29-qfc-item-controller-testability-227/remediation-inputs.2026-07-01T00-30.md
- artifacts/research/2026-07-01T00-00-qfc-item-controller-seam-redesign-research.md
- docs/features/active/2026-06-29-qfc-item-controller-testability-227/plan.2026-06-29T10-15.md
- docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-06-29T12-40.md

Source read (blast radius): all 10 QfcItemController*.cs partials, IItemViewer.cs, UiThread.cs,
Theme.cs, and the 7 existing QfcItemController*Tests.cs files, plus QuickFiler.csproj /
QuickFiler.Test.csproj / UtilitiesCS.csproj compile-include structure.

Acceptance: artifact exists with Timestamp, Policy Order, and explicit list of files read.
