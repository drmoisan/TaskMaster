# Phase 0 — Instructions Read (remediation cycle 1, issue #614)

Timestamp: 2026-08-26T21-05

Timestamp convention note: this feature folder's existing artifact series (`...T18-40`,
`...T19-50`, `...T20-05`, plan `...T21-00`) runs on the agent-session clock, which is offset
approximately +3h30m from the host wall clock. This cycle continues that series so the audit
trail stays monotonic. Host wall-clock at the time of this artifact: 2026-08-26 17:30 local.

Policy Order: `CLAUDE.md` -> `.claude/rules/general-code-change.md` ->
`.claude/rules/general-unit-test.md` -> `.claude/rules/csharp.md` -> cycle requirements
(`remediation-inputs.2026-08-26T21-00.md`, `code-review.2026-08-26T16-55.md` CR-1/CR-2,
`spec.md` AC16, `plan.2026-08-26T09-59.md` P3-T2).

## Files read (8 documents)

| # | Path | Scope read | Task |
| --- | --- | --- | --- |
| 1 | `CLAUDE.md` (repo root, 447 lines) | full | P0-T1 |
| 2 | `.claude/rules/general-code-change.md` (80 lines) | full | P0-T2 |
| 3 | `.claude/rules/general-unit-test.md` (105 lines) | full | P0-T3 |
| 4 | `.claude/rules/csharp.md` (96 lines) | full | P0-T4 |
| 5 | `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/remediation-inputs.2026-08-26T21-00.md` | full (111 lines) | P0-T5 |
| 6 | `.../code-review.2026-08-26T16-55.md` | CR-1 and CR-2 findings rows plus executive summary, toolchain table, severity rollup and recommendation (lines 1-110) | P0-T5 |
| 7 | `.../spec.md` | AC16 (line 1066-1068) plus surrounding AC15/AC17 context | P0-T5 |
| 8 | `.../plan.2026-08-26T09-59.md` | P3-T2 task text (line 119) plus the E1 scope-pinning delta record (line 52) | P0-T5 |

Supporting policy files additionally loaded by the session as project instructions and applied:
`.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`,
`.claude/rules/plan-acceptance-gates.md`.

## Controlling constraints extracted

1. Toolchain order format -> lint -> type-check -> test; restart from step 1 on any failure or
   formatter rewrite (`CLAUDE.md`, `.claude/rules/general-code-change.md`).
2. MSBuild gates use `/t:Rebuild`; `/p:Nullable=enable` is never added (`.claude/rules/csharp.md`
   items 2 and 3).
3. MSTest + Moq + FluentAssertions; Arrange-Act-Assert; no temp files; no `Thread.Sleep`,
   `Task.Delay`, `DateTime.Now`, `Random.Shared` (`.claude/rules/general-unit-test.md`,
   `.claude/rules/csharp.md`).
4. 500-line file ceiling for production and test code (`general-code-change.md`).
5. DI seam preference: interface, then injectable delegate, then adapter. This cycle uses
   preference 2 (`Func<string>` / `Action<string>`) per `.claude/rules/csharp.md` DI Seams.
6. Cycle scope is CR-1 and CR-2 only. CR-3, CR-4, all Minor findings, the pre-existing repo-wide
   coverage shortfall, AC26 manual validation, and `spec.md` edits are out of scope
   (`remediation-inputs.2026-08-26T21-00.md`).
7. D1/D4/D9 must not be weakened: store-root, cross-store, and above-archive values must still be
   rejected. The fix narrows over-rejection only.
