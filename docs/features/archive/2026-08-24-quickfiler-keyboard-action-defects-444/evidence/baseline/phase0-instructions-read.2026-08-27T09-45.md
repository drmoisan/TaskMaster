# Phase 0 — Repository policy documents read

Timestamp: 2026-08-27T09-45

Feature: `quickfiler-keyboard-action-defects-444` (issues #444, #472, #482)
Work Mode: `full-bug`
Reading order authority: `.claude/skills/policy-compliance-order/SKILL.md`

Policy Order:

| # | Path | Lines | SHA-256 |
| --- | --- | --- | --- |
| 1 | `CLAUDE.md` | 447 | 0deb5c764d385d4190e23e37b9e91d7fddca7e79f49d1ffeef3017baf4ee316d |
| 2 | `.claude/rules/general-code-change.md` | 80 | 91a89164532368f02b617ae9ff2b4e5247ba155c4d6f34acefd767b74ae46f53 |
| 3 | `.claude/rules/general-unit-test.md` | 105 | c0b3f9b1bd2e55c29484611d64655e2f71a1db97e05ba0680e289754713b63bf |
| 4 | `.claude/rules/csharp.md` | 96 | 05e69e4a114dafb1a337e0428909f522a3f57a82544b07b52af95108f271172c |

All four documents were read in full, in the order shown, before any source file was inspected for
edit.

Supporting documents read:

- `docs/features/active/quickfiler-keyboard-action-defects-444/issue.md`
- `docs/features/active/quickfiler-keyboard-action-defects-444/spec.md`
- `docs/features/active/quickfiler-keyboard-action-defects-444/research/2026-08-24T20-45-quickfiler-keyboard-action-defects.md`
- `.claude/rules/plan-acceptance-gates.md`
- `.claude/skills/atomic-plan-contract/SKILL.md`
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `.claude/skills/acceptance-criteria-tracking/SKILL.md`

Notes recorded at read time:

- The coverage-floor conflict is real and pre-existing: `CLAUDE.md` §UT2 states `>= 80%` repo-wide
  and `>= 90%` for new members, while `.claude/rules/general-unit-test.md` and
  `.claude/rules/quality-tiers.md` state `>= 85%` line and `>= 75%` branch. This artifact records the
  conflict; it is not resolved here.
- `.claude/rules/csharp.md` and `CLAUDE.md` agree that both MSBuild gates use `/t:Rebuild` and that
  `/p:Nullable=enable` must not be added.
