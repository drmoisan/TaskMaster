# Phase 0 Instructions Read (issue #292, remediation cycle 1)

- Timestamp: 2026-07-09T16-05
- Task: [P0-T1]

## Policy Order

Policies were read in the mandatory order defined by `policy-compliance-order`:

1. `CLAUDE.md` (standing instructions, always loaded)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. `.claude/rules/csharp.md` (C#-specific toolchain and coding standards)

Plus the remediation authority input.

## Files Read

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/remediation-inputs.2026-07-09T16-05.md`

## Key Constraints Carried Into Execution

- Fix is test-attribute-only: add `[DoNotParallelize]` to `UtilitiesCS.Test` classes that open a `CurrentStoreContext` scope. No production `*.cs`/`*.csproj` change.
- Do NOT modify `CurrentStoreContext.cs`, `StoresWrapper.cs`, `StoreWrapper.cs`, the enumeration-phase scope, or any reader assertion.
- No sleeps/retries/timing hacks; no temp files; no coverage regression.
- C# toolchain order: csharpier (v1 `check` subcommand) -> analyzer msbuild -> nullable msbuild -> vstest with coverage. Restart on any change/failure.
- CI-equivalent test invocation is the FULL `*.Test.dll` set with `/EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`.
- Coverage floor >= 80% on the testable denominator; new/changed-code >= 90% (no new production code here).
- Evidence paths resolve only to `<FEATURE>/evidence/<kind>/`.
