# Phase 0 — Policy Read Evidence (Remediation Plan: issue-96 2026-03-26T15-25)

Timestamp: 2026-03-26T15:30:00Z

Policy Order:
1. `.github/copilot-instructions.md`
2. `.github/instructions/general-code-change.instructions.md`
3. `.github/instructions/general-unit-test.instructions.md`
4. `.github/instructions/csharp-code-change.instructions.md`
5. `.github/instructions/csharp-unit-test.instructions.md`

## Files Read (in order)

### Policy Files

1. `.github/copilot-instructions.md`
   - Covers: project guidelines, MSTest + Moq + FluentAssertions requirements, tone policy.

2. `.github/instructions/general-code-change.instructions.md`
   - Covers: bugfix workflow, design principles, classes/functions/APIs, error handling, module structure, naming, toolchain loop (format → lint → type-check → test).

3. `.github/instructions/general-unit-test.instructions.md`
   - Covers: independence, isolation, determinism, coverage thresholds (≥80% repo-wide, ≥90% new code), AAA pattern, no external dependencies, no temp files.

4. `.github/instructions/csharp-code-change.instructions.md`
   - Covers: CSharpier formatting (not dotnet format), .NET analyzer linting via MSBuild, nullable type-check via MSBuild, null-safety by default, composition.

5. `.github/instructions/csharp-unit-test.instructions.md`
   - Covers: MSTest framework, Moq for mocking, FluentAssertions for assertions, toolchain commands (csharpier → msbuild analyzers → msbuild nullable → vstest).

### Feature Context Files

6. `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/issue.md`
   - Issue #96, Work Mode: minor-audit. Root cause: Keys.Right handler missing from RegisterFocusAsyncActions(). AC-1 through AC-3 checked off; AC-4, AC-5 are manual verification.

7. `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/plan.2026-03-25T09-03.md`
   - Original plan, Status: Completed. All P0-P2 tasks checked off. Bugfix delivered in commits bd8fc03 and 3b472b2.

8. `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/policy-audit.2026-03-25T14-00.md`
   - Policy audit: READY FOR MERGE. All toolchain gates pass. Minor deviations documented (ToggleExpansionAsync signature, test method names).

9. `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/feature-audit.2026-03-25T14-00.md`
   - Feature audit: PASS (automated scope). AC-1 through AC-3 met with evidence; AC-4, AC-5 require manual Outlook verification.

10. `artifacts/research/20260326-issue87-unstacking-sequence-research.md`
    - Unstacking research: serial unstacking order is #97 → #96 → residual excluded work → #87. Cherry-pick as replay primitive; issue #96 commits are bd8fc03 and 3b472b2.

## Key Constraints for This Remediation Pass

- Treat issue.md as sole AC source (minor-audit mode).
- Use origin/development as comparison base.
- Keep main workspace on feature/utilities-coverage-part-three-87.
- Run clean-branch operations in sibling worktree c:\Users\DanMoisan\repos\TaskMaster-issue96-clean.
- Replay only commits bd8fc03 and 3b472b2.
- Limit clean-branch diff to QuickFiler/**, QuickFiler.Test/**, docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/**.
