# Phase 0 — Policy Instructions Read (issue #440)

Timestamp: 2026-08-29T06-20

Policy Order: the order mandated by the `policy-compliance-order` skill and by the
repository-root `CLAUDE.md` section "Policy Compliance Order": repository-root
`CLAUDE.md` first, then the cross-language code-change policy, then the
cross-language unit-test policy, then the language-specific rules for the files in
scope (C#, because this change touches `*.cs` files only).

## Read list (in the order read)

1. `CLAUDE.md` (repository root) — read in full, 448 lines. [P0-T1]
2. `.claude/rules/general-code-change.md` — read in full, 81 lines. [P0-T2]
3. `.claude/rules/general-unit-test.md` — read in full, 106 lines. [P0-T3]
4. `.claude/rules/csharp.md` — read in full, 97 lines. [P0-T4]

The four entries appear in exactly the order P0-T1 through P0-T4.

## Constraints carried forward into execution

- Toolchain order is format, lint (analyzers), type-check (nullable), test. Restart
  from the first step if any step fails or rewrites a file.
- Both msbuild gates use `/t:Rebuild`, never `/t:Build`, and neither adds
  `/p:Nullable=enable`.
- CSharpier is invoked only through `dotnet tool run`, never a global install.
- MSTest is the test framework, Moq the mocking library, FluentAssertions the
  assertion library.
- No production, test, or reusable script file may exceed 500 lines.
- No temporary file may be created or used by a test.
- Tone policy: professional, factual, neutral; no humor, hyperbole, or decorative
  metaphor in any artifact this plan writes.
