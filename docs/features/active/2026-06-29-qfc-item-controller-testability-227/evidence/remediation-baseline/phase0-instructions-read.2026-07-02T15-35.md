# Phase 0 — Instructions Read (Cycle 4, Issue #227)

Timestamp: 2026-07-02T15-35

Policy Order:
1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/skills/atomic-plan-contract/SKILL.md`
6. `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
7. `docs/features/active/2026-06-29-qfc-item-controller-testability-227/2026-07-02T15-35-remediation/remediation-inputs.2026-07-02T15-35.md`
8. `docs/features/active/2026-06-29-qfc-item-controller-testability-227/2026-07-02T15-26-audit/code-review.2026-07-02T15-26.md` (the "ToggleFocus" finding)
9. `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:27-123`
10. `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:81-151`
11. `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:405-437`

Files actually read (this session):
- `CLAUDE.md` (delivered inline via system reminder at session start; content confirmed current)
- `.claude/rules/general-code-change.md` (delivered inline via system reminder at session start)
- `.claude/rules/general-unit-test.md` (delivered inline via system reminder at session start)
- `.claude/rules/csharp.md` (delivered inline via tool-result metadata at session start)
- `.claude/skills/atomic-plan-contract/SKILL.md` (delivered inline via system reminder at session start)
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` (delivered inline via system reminder at session start)
- `docs/features/active/2026-06-29-qfc-item-controller-testability-227/2026-07-02T15-35-remediation/remediation-plan.2026-07-02T15-35.md` (full plan, read directly)
- `docs/features/active/2026-06-29-qfc-item-controller-testability-227/2026-07-02T15-35-remediation/remediation-inputs.2026-07-02T15-35.md` (full file, read directly)
- `docs/features/active/2026-06-29-qfc-item-controller-testability-227/2026-07-02T15-26-audit/code-review.2026-07-02T15-26.md` (ToggleFocus finding sections, grepped with context)
- `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` (full file, read directly — confirms lines 27-123 cover both `ToggleFocus` overloads and the private `ToggleFocusOnAsync`/`OffAsync`)
- `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` (full file, read directly)
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (lines 1-260, read directly — confirms `SetField`/`GetField`/`BuildColorTheme` shapes)
- `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs` (field declarations grepped; lines 405-437 read directly — confirms `SetQfcTheme(bool async)` branching)
- `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.DispatcherTests.cs` (lines 1-140, read directly — confirms the proven handle-less double construction shape cited by the plan)

Acceptance: artifact exists with `Timestamp:`, `Policy Order:`, and the explicit file list actually read, all populated.
