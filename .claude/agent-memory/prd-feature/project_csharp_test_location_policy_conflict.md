---
name: csharp-test-location-policy-conflict
description: .claude/rules/general-unit-test.md demands a tests/ mirror tree and bans colocation, but every C# test in TaskMaster lives in a sibling <Project>.Test project — CLAUDE.md wins; record the resolution in the spec so it is not re-litigated
metadata:
  type: project
---

New C# tests belong in the sibling `<Production>.Test` project (`QuickFiler.Test`,
`TaskMaster.Test`, `UtilitiesCS.Test`, `SVGControl.Test`), mirroring the production namespace folder
— e.g. `QuickFiler/Controllers/EfcDataModel.cs` → `QuickFiler.Test/Controllers/…Tests.cs`. Not a
`tests/` mirror tree.

**Why:** `.claude/rules/general-unit-test.md` § "Test File Location" states test files must live in a
`tests/` tree and that colocation is "not permitted". Verified 2026-08-29: `tests/` in TaskMaster
holds exactly five PowerShell Pester files under `tests/scripts/vscode/` and zero C# files. The
General and C# Unit Test Policies embedded in `CLAUDE.md` impose no `tests/` requirement, and
`policy-compliance-order` ranks `CLAUDE.md` above the `.claude/rules/*` files. The General Code
Change Policy § 7.1 ("match existing style") settles the rest. `.claude/**` is push-down-owned from
drm-copilot, so the rules file cannot be corrected in this repo — see
[[claude-files-are-pushdown-owned]] in the shared index.

**How to apply:** State the decision *and* the three-part rationale in the spec's Test Strategy
section, so a reviewer does not re-open it. Two mechanical follow-ons that are easy to miss:

- `QuickFiler.csproj` and `QuickFiler.Test.csproj` are legacy non-SDK projects that enumerate every
  source file. A new test file does not compile until an explicit
  `<Compile Include="Controllers\NewTests.cs" />` entry is added (existing `EfcDataModel` test files
  sit at `QuickFiler.Test/QuickFiler.Test.csproj:114-115`). Make it its own acceptance criterion,
  phrased as "the new tests appear in the executed test list", not "the file exists".
- Namespace precedent inside `QuickFiler.Test/Controllers/` is inconsistent
  (`QuickFiler.Controllers.Tests` vs `QuickFiler.Test.Controllers`). Prefer the latter, matching the
  newer files.

Related: [[ac-gates-verify-satisfiability]].
