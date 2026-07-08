# P1-T1 — Constrained Implementation Handoff Record (Issue #269)

- Timestamp: 2026-07-08T09-50
- Task: [P1-T1]

## References

- Issue: #269 (`docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md`)
- Feature folder: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/`
- Requirements source: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md` (`## Acceptance Criteria`, AC1-AC5)
- Policy rule: `.claude/rules/csharp.md`
- Plan "Chosen Fix Shape" section (`plan.md` lines 33-40): two complementary changes — probe null-guard at `QfcThemeHelper.cs:89`, and a second narrow `catch (System.NullReferenceException)` at `Theme.Rendering.cs:42-50`.

## Constraint

Production changes are limited to exactly two files:
- `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs`
- `QuickFiler/Helper Classes/QfcThemeHelper.cs`

No opportunistic refactor. No other production file is to be touched.

## Disposition

Proceeding to P1-T2/P1-T3 (fail-before regression tests) and P1-T4/P1-T5 (the fix itself) under this constraint.
