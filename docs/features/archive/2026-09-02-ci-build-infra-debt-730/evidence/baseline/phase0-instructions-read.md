# Phase 0 — Policy Instructions Read (Task [P0-T1])

Timestamp: 2026-09-02T22-11

Policy Order: the five policy files below were read in the order defined by the
`policy-compliance-order` skill (CLAUDE.md first, then the cross-language
code-change rule, then the cross-language unit-test rule, then the
language/domain-specific rules applicable to the files this plan touches).

## Files read (itemized, in the order read)

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/ci-workflows.md`

## Scope justification for items 4 and 5

- `.claude/rules/csharp.md` is in scope because CLAUDE.md's C# Code Change Policy
  explicitly extends to `*.props` files, and Phase 1 of this plan creates a new
  repository-root `Directory.Build.props`.
- `.claude/rules/ci-workflows.md` is the rule scoped to the `.github/workflows`
  directory tree; it is in scope because Phase 1 edits three workflow files
  (`_build-analyzers.yml`, `_build-nullable.yml`, `_mstest-coverage.yml`).

## Key constraints carried forward into execution

- C# toolchain order is format -> lint -> type-check -> test; restart from step 1
  if any step fails or changes files.
- `msbuild ... /t:Rebuild` (not `/t:Build`) is required locally; do not add
  `/p:Nullable=enable`.
- CSharpier is invoked only through `dotnet tool run`.
- No edit to any policy document under `.claude/rules/`.
