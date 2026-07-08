# Phase 0 — Instructions Read (Remediation Cycle 1, Issue #183 R1)

Timestamp: 2026-06-10T09-43

Policy Order: CLAUDE.md → .claude/rules/general-code-change.md → .claude/rules/general-unit-test.md → .claude/rules/csharp.md

## Files Read (in order)

1. `CLAUDE.md` — standing project instructions, policy compliance order, C# toolchain order.
2. `.claude/rules/general-code-change.md` — cross-language code change policy, including the 500-line File Size Limit (test code is not an excepted file type).
3. `.claude/rules/general-unit-test.md` — cross-language unit test policy (independence, isolation, determinism, coverage gates).
4. `.claude/rules/csharp.md` — C#-specific standards, MSTest/Moq/FluentAssertions, toolchain commands, analyzer stack.

## Relevant Constraints Acknowledged

- 500-line file-size limit applies to test code; `Triage_OlLogicTests.cs` (553 lines) violates it.
- Test-organization change only; no production file may be modified.
- Preserve all 21 test methods verbatim; no weakening, renaming, or removal.
- Full C# toolchain order: csharpier → analyzer build → nullable/TWAE build → vstest with coverage; restart on any change/failure.
- Repository-wide first-party line coverage must remain >= 80%.
