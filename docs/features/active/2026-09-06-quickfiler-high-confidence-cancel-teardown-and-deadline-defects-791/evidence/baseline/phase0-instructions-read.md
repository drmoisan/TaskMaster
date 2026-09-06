# [P0-T1] Phase 0 — Policy instructions read

Timestamp: 2026-09-06T14-21

Policy Order: The `policy-compliance-order` sequence was followed exactly: (1) `CLAUDE.md` — all
sections; (2) General Code Change Policy; (3) General Unit Test Policy; (4) the C#-specific rules;
(5) the tonality rules. Language-specific standards layer on top of the general policy. Where a
conflict is found, the session halts and notifies the user; no conflict was found in this pass.

## Files read (in policy order), with line counts

| # | Path | Lines |
|---|---|---|
| 1 | `CLAUDE.md` | 447 |
| 2 | `.claude/rules/general-code-change.md` | 80 |
| 3 | `.claude/rules/general-unit-test.md` | 105 |
| 4 | `.claude/rules/csharp.md` | 96 |
| 5 | `.claude/rules/tonality.md` | 80 |

Command: `pwsh -NoProfile -Command '... foreach ($f in @(<the five paths>)) { (Get-Content -LiteralPath $f).Count } ...'`

EXIT_CODE: 0

Output Summary: All five policy files exist and were read in full in the order listed above. Line
counts were measured with `Get-Content ... | .Count` rather than estimated. The five counts are
447, 80, 105, 96 and 80 respectively.

## Constraints carried forward into execution

- C# toolchain order is format, lint, type-check, test; a failure or a file-changing step restarts
  the loop from formatting.
- CSharpier is invoked through `dotnet tool run` so the manifest-pinned 1.2.6 is used;
  `dotnet format` is prohibited.
- Both gate builds use `/t:Rebuild`; `/t:Build` and `/p:Nullable=enable` are prohibited for the
  gate builds.
- MSTest + Moq + FluentAssertions are the required test stack; no temporary files, no wall-clock
  waits, no external services in tests.
- No `.cs` production, test, or reusable-script file may exceed 500 lines.
- Tone is professional, factual, and neutral in all artifacts.
