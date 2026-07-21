# Phase 0 — Policy Read Receipt

Timestamp: 2026-07-19T00-00

Policy Order: CLAUDE.md → .claude/rules/general-code-change.md → .claude/rules/general-unit-test.md → .claude/rules/csharp.md

Files read (in required order):
1. CLAUDE.md (standing instructions, C# toolchain section) — read (loaded in session context).
2. .claude/rules/general-code-change.md (cross-language code change policy) — read (loaded in session context).
3. .claude/rules/general-unit-test.md (cross-language unit test policy) — read (loaded in session context).
4. .claude/rules/csharp.md (C#-specific toolchain and coding standards) — read via Read tool.

Additional references consulted for execution correctness:
- docs/features/active/2026-07-18-utilitiescs-nullable-email-classifier-372/plan.2026-07-18T22-06.md (plan of record).
- docs/features/active/2026-07-18-utilitiescs-nullable-email-classifier-372/spec.md (AC1–AC5 source).
- docs/features/active/2026-07-18-utilitiescs-nullable-email-classifier-372/user-story.md (AC1–AC5 source).

Key operative constraints acknowledged:
- Verification uses the per-file `#nullable enable` pragma gate (project-scoped per batch, solution-wide at final QC) with `/t:Rebuild` and `/p:TreatWarningsAsErrors=true`, WITHOUT `/p:Nullable=enable` globally.
- Target net481 / C# 12: no `System.Diagnostics.CodeAnalysis` post-condition attributes and no polyfill; no `init`/positional `record`/`record struct` introduced.
- No `<Nullable>` element added to UtilitiesCS/UtilitiesCS.csproj (AC2).
- Annotation and null-safety only; no behavior/scoring/model/corpus math changes (AC3/AC5).
- #363 `ThrowIfNull<T> where T : notnull` (no `[NotNull]`) does not narrow null-state; remediate by return-capture, justified `!` with `// why`, or invariant member annotation — not new `if (x is null) throw` guards (AC4 pressure).
