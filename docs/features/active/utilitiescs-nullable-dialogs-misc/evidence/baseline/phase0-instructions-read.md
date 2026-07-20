# Phase 0 — Policy Instructions Read Receipt

- Timestamp: 2026-07-19T10-53
- Task: [P0-T1]
- Issue: #374 (utilitiescs-nullable-dialogs-misc)

## Policy Order

Policies were read in the required order defined by `policy-compliance-order`:

1. CLAUDE.md (standing instructions, C# toolchain section)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. `.claude/rules/csharp.md` (C#-specific toolchain and coding standards)

## Files Read

- `CLAUDE.md` — read (loaded as project instructions; C# toolchain order, coverage floor, MSTest/Moq/FluentAssertions requirements).
- `.claude/rules/general-code-change.md` — read (design principles, mandatory toolchain loop, 500-line file limit, I/O boundaries).
- `.claude/rules/general-unit-test.md` — read (five core principles, coverage requirements, determinism infrastructure).
- `.claude/rules/csharp.md` — read (CSharpier formatting, analyzer stack, nullable analysis type-check command, coverage thresholds).

## Feature-Specific Deviation Acknowledged

Per the approved plan Scope Invariants and spec Constraints item 1, the nullable type-check
gate for this child uses the per-file pragma command
`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
WITHOUT `/p:Nullable=enable`. This is a deliberate, documented deviation from the stock
`.claude/rules/csharp.md` step 3 (`/p:Nullable=enable`) for this child only, and it must NOT be
resolved by editing any `.claude/rules/*` file. The rules-vs-convention conflict is flagged for
the maintainer and deferred to the Wave-2 CI capstone child.
