# Remediation Inputs: csharp-analyzer-stack-hardening (Issue #181) — Cycle 3

- Cycle entry timestamp: 2026-06-08T19-44
- Feature folder: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181`
- Base branch: `main` @ `2a522ed831865c2918ab02df153ef2929b0617dc`
- Head branch: `feature/csharp-analyzer-stack-181` @ `0883d0f7367844f16ede7d48972a91886aaff5be`
- Work mode: `full-feature`
- PR: https://github.com/drmoisan/TaskMaster/pull/182
- Trigger: required CI check RED after PR open (post-PR CI monitoring; CI run display #215)

## Failing Check (origin of this cycle)

The required GitHub Actions check **"Format, build, analyze, and test"** FAILED at its FIRST step, **"Verify formatting" (`dotnet csharpier check .`)**, on:

- `ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs` — "Was not formatted." (around line 111: the commented-out `//[TestCategory("ProductionBugSuspected")]` attribute is indented with 7 spaces where CSharpier expects 8, misaligned with the adjacent `[TestMethod]` and `//[Ignore(...)]` lines).

Run: https://github.com/drmoisan/TaskMaster/actions/runs/27173988735/job/80218940980 (conclusion: failure, 1m45s).

Whole-repo `dotnet csharpier check .` confirms **exactly one** unformatted file (1057 files checked); no other formatting violations exist.

## Root-Cause Classification

This is a **feature-branch-introduced** formatting defect (not a pre-existing `main` regression). It originates from branch commit `0883d0f7` ("fix(file-path-helper): subscribe before initializing path state"), which re-enabled the `Constructor_WithOutlookItem_ShouldInitializeProperties` regression test by commenting out its `[TestCategory("ProductionBugSuspected")]` and `[Ignore("ProductionBugSuspected")]` markers. The comment-out edit left the first commented line at 7-space indentation instead of 8.

`git diff main..HEAD -- ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs` shows the 2-line change:

```
-        [TestCategory("ProductionBugSuspected")]
-        [Ignore("ProductionBugSuspected")]
+       //[TestCategory("ProductionBugSuspected")]
+        //[Ignore("ProductionBugSuspected")]
```

## Downstream Risk (must verify locally before pushing)

Commit `0883d0f7` re-enabled two previously-ignored regression tests:
- `ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs` — `Constructor_WithOutlookItem_ShouldInitializeProperties`
- `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs` — (un-ignored regression test)

The CI failed at the formatting step (step 1), so the build, analyze, and test steps **never ran**. After the formatting fix unblocks those steps, the two newly-enabled tests will execute for the first time on CI. The local toolchain run (MSTest-with-coverage) MUST confirm both tests pass before push; if either fails, that is a new finding that triggers a follow-up cycle per the scope-change rule. Do not weaken, re-ignore, or skip those tests to force green.

## Fix List (file paths, expected behavior, verification)

- Action: Apply CSharpier 1.2.6 formatting to `ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs` (formatter output is authoritative; no hand-formatting). This is a pure formatting change — correct the line-111 comment indentation to 8 spaces. NO logic, behavior, attribute-state, or public-API change.
  - File touched: `ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs` (1 test file, formatting only).
  - Verification (local): `dotnet tool restore` then `dotnet csharpier check .` exits 0; then the full toolchain loop (analyzer build, nullable `TreatWarningsAsErrors` build at the established vendored-only baseline with no first-party regression, MSTest-with-coverage including the two re-enabled tests) per CLAUDE.md.
  - Verification (authoritative): after commit + push, the PR #182 "Format, build, analyze, and test" check completes GREEN (all steps). Record the run URL and per-check status.

## Do Not Do (scope guard)

- Do NOT modify the logic, attributes, or comment content of `ToDoItemTests.cs`; apply only CSharpier whitespace formatting output.
- Do NOT re-add `[Ignore]`/`[TestCategory]` or otherwise re-disable the re-enabled regression tests to force green.
- Do NOT touch any other `.cs` source file; only this one file is unformatted per the whole-repo CSharpier check.
- Do NOT alter the analyzer-stack build-config delivered earlier in this feature.
- Do NOT introduce any CS8032 suppression or re-add SecurityCodeScan.
- Do NOT touch the four vendored projects (SVGControl, SVGControl.Test, UtilitiesSwordfish.NET.General, UtilitiesSwordfish.Test).
- Do NOT promote RS0030 or any analyzer rule from suggestion to warning/error.
- Do NOT modify `.claude/rules/` policy documents.
- Do NOT weaken or skip any CI gate to force green.

## Cycle Artifacts (this remediation cycle)

1. `remediation-inputs.2026-06-08T19-44.md` (this file) — authored at cycle entry by the orchestrator.
2. `remediation-plan.2026-06-08T19-44.md` — `atomic-planner` authors at cycle entry.
3. `code-review.<exit-ts>.md`, `feature-audit.<exit-ts>.md`, `policy-audit.<exit-ts>.md` — `feature-review` authors at cycle exit after CI is green.

## Handoff

Per `remediation-handoff-atomic-planner`: hand off to `atomic-planner` to author `remediation-plan.2026-06-08T19-44.md`, then `atomic-executor` preflight (`DIRECTIVE: PREFLIGHT VALIDATION ONLY`) and execution, then `feature-review` reaudit at the exit timestamp. Exit gate: `blocking_count == 0` (PR #182 CI GREEN; AC6 PASS).
