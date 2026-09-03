# Repoint stale ci.yml citations in CLAUDE.md to reusable workflow files

## Summary

Fix three stale file citations in CLAUDE.md that incorrectly reference .github/workflows/ci.yml for C# toolchain commands that were moved to separate reusable workflow files in PR #556. The citations are:

- Line 194: CSharpier pinned-version parity reference (should cite .github/workflows/_format-check.yml, not ci.yml)
- Line 202: .NET analyzer `/t:Build /m` step reference (should cite .github/workflows/_build-analyzers.yml, not ci.yml)  
- Line 210: Nullable `TreatWarningsAsErrors` step reference (should cite .github/workflows/_build-nullable.yml, not ci.yml)

Documentation-only change: no command text altered, no workflow behavior changed, no test coverage impact.

## Why

PR #556 (issue #553) split the CI workflow into separate reusable `workflow_call` files to parallelize build tasks, moving the CSharpier, analyzer, and nullable-check steps out of ci.yml. CLAUDE.md's policy documentation was not updated to reflect these moves, so agents following citations to verify command compliance find stale references. This misleads developers about the authoritative location of the documented commands and is a documentation defect.

## What Changed

CLAUDE.md:
- Line 194: replaced `.github/workflows/ci.yml` citation with `.github/workflows/_format-check.yml` in the CSharpier tooling section
- Line 202: replaced `.github/workflows/ci.yml` citation with `.github/workflows/_build-analyzers.yml` in the .NET analyzers section  
- Line 210: replaced `.github/workflows/ci.yml` citation with `.github/workflows/_build-nullable.yml` in the Type Checking section (step parenthetical "Build with nullable warnings treated as errors" retained)

No other files modified.

## Verification

All 7 acceptance criteria verified and documented:

- AC1: `Select-String -Path CLAUDE.md -Pattern '_format-check\.yml'` confirms exactly one match at line 194
- AC2: `Select-String -Path CLAUDE.md -Pattern '_build-analyzers\.yml'` confirms exactly one match at line 202
- AC3: `Select-String -Path CLAUDE.md -Pattern '_build-nullable\.yml'` confirms exactly one match at line 210; step name parenthetical retained
- AC4: `Select-String -Path CLAUDE.md -Pattern 'ci\.yml'` returns zero matches (all stale citations removed)
- AC5: `git diff origin/main...HEAD -- .claude/rules/csharp.md` confirms .claude/rules/csharp.md is unchanged (already correct)
- AC6: `git diff origin/main...HEAD --name-only` confirms only CLAUDE.md changed; no edits under .claude/**, .codex/**, .agents/**, config/blast-radius.json, or config/orchestration-routing.json
- AC7: `git diff origin/main...HEAD -- CLAUDE.md` shows exactly 3 removed lines and 3 added lines (only citation tokens changed; all surrounding command text preserved)

Verification evidence: 11 artifacts in `docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/evidence/` documenting each check.

## Backward Compatibility

No breaking changes, API changes, or migration impact. This is a documentation citation correction with no behavioral effect.

## Risks & Mitigations

Risk: incorrect reusable workflow file citation(s)  
Mitigation: Each citation was verified directly against the reusable workflow files to confirm the cited step exists and matches the documented command text. Low risk: single-file text revert is sufficient if correction is later found incorrect.

## Follow-ups

None. Issue #564 is fully resolved by this change.

- Closes #564
