# Phase 0 — Instructions Read (remediation cycle 2, issue #614)

Timestamp: 2026-08-26T22-13

Policy Order: `CLAUDE.md` -> `.claude/rules/general-code-change.md` ->
`.claude/rules/general-unit-test.md` -> `.claude/rules/csharp.md` -> cycle requirements.

Codex policy mapping: the repository-local shared skills under `.agents/skills/` and the
current `.github/instructions/` policy sources are authoritative for this Codex execution.
The equivalent Claude-era files named by the inherited remediation plan are also read as
historical plan inputs.

## Files read

| # | Path | Scope read | Task |
| --- | --- | --- | --- |
| 1 | `CLAUDE.md` (repo root, 447 lines) | full | P0-T1 |
| 2 | `.claude/rules/general-code-change.md` (80 lines) | full | P0-T2 |
| 3 | `.claude/rules/general-unit-test.md` (105 lines) | full | P0-T3 |
| 4 | `.claude/rules/csharp.md` (96 lines) | full | P0-T4 |
| 5 | `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/remediation-inputs.2026-08-26T22-12.md` (264 lines) | full; SUPERSEDING DECISION governs | P0-T5 |
| 6 | `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/code-review.2026-08-26T22-12.md` | RC-1 through RC-4 rows and surrounding findings context | P0-T5 |
| 7 | `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/spec.md` | AC16 and adjacent AC15/AC17 context | P0-T5 |
| 8 | `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/remediation-plan.2026-08-26T21-00.md` | Phase 1 through Phase 3 task texts | P0-T5 |

## Controlling constraints extracted

1. The cycle-2 superseding decision requires a partial revert: restore strict single-argument
   filing validation, retain the creation predicate and CR-1 short-stem behavior, and defer
   producer-side router normalization to issue #637.
2. The composition regression must provide genuine fail-before/pass-after proof without COM.
3. RC-2 amends AC16, RC-3 removes the now-unused resolver only after a consumer search, and RC-4
   adds an out-of-ancestor `GetStem` behavior-pinning test.
4. C# verification runs format, analyzer rebuild, nullable/type-check rebuild, then coverage tests;
   both rebuild gates omit `/p:Nullable=enable`.
5. Evidence remains under the feature's canonical `evidence/<kind>/` folders and must be redacted
   per issue #602.
