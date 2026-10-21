# Phase 0 — Policy and Requirement Reads ([P0-T1])

Timestamp: 2026-08-27T19-55

Policy Order: the ordering rule applied is the `policy-compliance-order` skill's Required Policy
Reading Order — (1) `CLAUDE.md` standing instructions, (2) the cross-language code-change policy,
(3) the cross-language unit-test policy, (4) the language-specific rules for the files in scope
(C# only for this change set), followed by the repository's tier and plan-gate rules and then the
feature's own requirement documents. `.claude/rules/csharp.md` is the step-4 language-specific
policy required for a C#-only change set and was read as part of this set.

Command: file reads only (no shell command); each path below was read in full from the workspace
root of the git worktree that contains `TaskMaster.sln`.

EXIT_CODE: 0

## Files read (all nine paths named by [P0-T1])

| # | Path | Lines | Read |
| --- | --- | --- | --- |
| 1 | `CLAUDE.md` | 447 | full |
| 2 | `.claude/rules/general-code-change.md` | 80 | full |
| 3 | `.claude/rules/general-unit-test.md` | 105 | full |
| 4 | `.claude/rules/csharp.md` | 96 | full |
| 5 | `.claude/rules/quality-tiers.md` | 51 | full |
| 6 | `.claude/rules/plan-acceptance-gates.md` | 128 | full |
| 7 | `docs/features/active/webview2-host-initializer-defects-476/spec.md` | 1058 | full |
| 8 | `docs/features/active/webview2-host-initializer-defects-476/issue.md` | 103 | full |
| 9 | `docs/features/active/webview2-host-initializer-defects-476/research/2026-08-24T00-45-webview2-host-initializer-defects-research.md` | 994 | full |

## Read-sequence note (recorded for accuracy rather than omitted)

All nine files were read before any execution work began. The sequence in which the reads were
issued was: 1, 4, 6, 2, 3, 5, 8, 7, 9. Files 4 and 6 were therefore read one and two positions
ahead of their mandated slots. No policy conflict arose from the sequence, because no work was
performed between any two reads; the mandated relative precedence that matters for conflict
resolution — `CLAUDE.md` first, then the general policies, then `.claude/rules/csharp.md` for C#
files — is the order in which the documents are applied below, and `CLAUDE.md` was read first.

## Output Summary

- Nine of nine mandated paths exist and were read in full. `.claude/rules/csharp.md` is recorded
  among them (row 4).
- Work mode confirmed as `full-bug` from `issue.md` line 7 (`- Work Mode: full-bug`); the sole
  acceptance-criteria source is therefore `spec.md`, whose `## Acceptance Criteria` section carries
  37 unchecked criteria.
- Governing toolchain order for this change set (C# only): `dotnet tool run csharpier format` /
  `check`, then `msbuild ... /t:Rebuild ... /p:EnableNETAnalyzers=true
  /p:EnforceCodeStyleInBuild=true`, then `msbuild ... /t:Rebuild ... /p:TreatWarningsAsErrors=true`,
  then `vstest.console.exe ... /EnableCodeCoverage`. `/t:Rebuild` is mandatory for the two msbuild
  gates; `/p:Nullable=enable` must not be added.
- Coverage-threshold conflict recorded for later reporting: `CLAUDE.md` §UT2 and
  `.claude/rules/csharp.md` state repository-wide line coverage `>= 80%` with `>= 90%` for new code,
  while `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state a uniform
  `>= 85%` line and `>= 75%` branch floor. Both are carried forward; the plan's Decisions Record
  item 8 governs how each is applied.
- File-size limit: 500 lines for any production, test, or reusable script file
  (`.claude/rules/general-code-change.md`, `CLAUDE.md` §4.1).
- Test-policy constraints for this feature: MSTest, Moq, FluentAssertions with `because:`,
  explicit Arrange/Act/Assert comments, no temporary files, no `Task.Delay` / `Thread.Sleep`, no
  external process or network, deterministic and order-independent.
