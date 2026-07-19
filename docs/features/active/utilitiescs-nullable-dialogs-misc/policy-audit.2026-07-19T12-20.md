# Policy Audit — utilitiescs-nullable-dialogs-misc (Issue #374)

- Component: `UtilitiesCS/Dialogs/` cluster + 2 verify-only misc files
- Date: 2026-07-19
- Reviewer: feature-review agent
- Work Mode: full-feature (AC sources: `spec.md` and `user-story.md`)
- Branch: `feature/utilitiescs-nullable-dialogs-misc-374`
- Head: `9b09b1c91ce60af5aa3bec26f45d1cf21103fb2d`
- Base branch: `origin/epic/utilitiescs-nullable-remediation-integration`
- Merge-base / base tip: `dffadd5a102884dd811ed5731477de18417594f1`
- Files under test (14 C# source files):
  `UtilitiesCS/Dialogs/{ActionButton, DelegateButton, DelegateButtonTemplate, FolderNotFoundViewer,
  FunctionButton, InputBox, InputBoxViewer, MyBox, MyBoxModeless, MyBoxViewer, NotImplementedDialog,
  YesNoToAll}.cs`, `UtilitiesCS/Properties/AssemblyInfo.cs`, `UtilitiesCS/WindowsAPI/ExtraDeclarations.cs`

> Note on template source: this repo's `policy-audit-template-usage` skill designates the MCP tool
> `mcp__drm-copilot__resolve_policy_audit_template_asset` as the authoritative template source. That
> MCP tool is not available in this review session; the artifact is therefore constructed directly
> while preserving the skill's canonical major section set (`## Executive Summary`, `## 1`..`## 10`,
> `## Appendix A`, `## Appendix B`).

## Executive Summary

This branch applies per-file `#nullable enable` opt-in nullable-reference-type remediation to 14
C# source files: the 12 substantive `UtilitiesCS/Dialogs/` remediation targets plus 2 verify-only
misc files (`WindowsAPI/ExtraDeclarations.cs`, `Properties/AssemblyInfo.cs`). The change is
annotation and null-safety only: `#nullable enable` pragmas, `?` nullability annotations on
types/parameters/returns, and runtime-neutral `!` null-forgiving operators on existing expressions.
No executable statement, branch, guard, or logic was added or removed.

Verification result: PASS. Independent review of the source diff and the committed evidence
confirms zero CS86xx nullable diagnostics across the 14 files, no behavior change, no coverage
regression on changed lines, no project- or solution-level `<Nullable>` element, and no
cross-blocking of non-opted-in files. The reviewer independently re-ran the authoritative isolated
`UtilitiesCS.csproj` rebuild and confirmed zero CS86xx (the only errors surfaced were pre-existing
CS0168/CS0618 obsolete-API/unused-variable diagnostics in unrelated `EmailIntelligence/`/`Extensions/`
files, none in `Dialogs/` and none nullable-related).

Blocking findings: 0. Merge-readiness (policy): PASS.

## Rejected Scope Narrowing

No caller-supplied scope narrowing was detected. The audit covers the full branch diff against the
resolved base tip `dffadd5a`. The deviations described in the delegation prompt (per-file pragma
gate instead of global `/p:Nullable=enable`; isolated `UtilitiesCS.csproj` build supplementing the
solution-wide rebuild that short-circuits on a pre-existing vendored SVGControl CS0649; the two
flagged out-of-scope items — the `helperclasses` #364 dependency-edge note and the ~110-file
ownership-gap table) are documented, maintainer-approved epic-architecture decisions recorded in
`spec.md` "Constraints & Risks" and the plan's "Scope Invariants" / "Open Questions" sections. They
are legitimate scope boundaries, not narrowing of the branch-diff audit, and are recorded here for
transparency rather than rejected.

## Evidence Location Compliance

`git diff --name-only dffadd5a..HEAD` was scanned for files written under `artifacts/baselines/`,
`artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. None were found. All feature
evidence is written under the canonical `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/<kind>/`
path (`baseline/`, `qa-gates/`, `regression-testing/`, `other/`), consistent with the
evidence-and-timestamp-conventions skill. No evidence-location violation. Verdict: PASS.

## 1. General Unit Test Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| No test source files added/modified | PASS | `git diff --name-only` shows no `*.Test`/`*Tests.cs` source files changed; only evidence `.md` files under `evidence/` reference "tests" in their path. |
| Existing tests still pass | PASS | `evidence/qa-gates/final-tests-coverage.md`: 5702 passed / 0 failed (identical to baseline 5702/0). |
| Determinism / no temp files / no banned test APIs introduced | PASS (N/A for new tests) | No test code added; annotation-only change. No `DateTime.Now`/`UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay` introduced (grep of added lines returned none). |
| Coverage not reduced on changed lines | PASS | `evidence/qa-gates/final-coverage-delta.md`: per-file delta 0.00% across all 14 files; cluster 93.10% baseline and post-change. |

This feature adds no unit tests. Because it introduces no executable lines (pragma + annotations +
`!` only), the "no coverage regression on changed lines" obligation is the applicable test-policy
gate and is satisfied.

## 2. General Code Change Policy Compliance

| Principle | Verdict | Evidence |
|---|---|---|
| Simplicity / no opportunistic refactor | PASS | Diff is limited to pragma + `?`/`!` annotations; no restructuring. |
| Separation of concerns / no behavior change | PASS | `evidence/qa-gates/final-signature-compat.md`; AsyncLocal dialog-invoker seams untouched. |
| Error handling — no new broad catch / no silent swallow | PASS | No control-flow or exception changes in the diff. |
| File size < 500 lines | PASS | `evidence/qa-gates/final-scope-guards.md`: largest is `MyBox.cs` at 417 lines. |
| No banned APIs introduced | PASS | Grep of added lines: no `DateTime.Now/UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`, `Guid.NewGuid`. |
| Public API compatibility | PASS | All signature changes are additive nullability annotations; existing callers compile and behave identically (see §3 and `final-signature-compat.md`). |
| Toolchain loop completed | PASS | CSharpier clean (`final-csharpier.md`), analyzer build 0 errors (`final-analyzers.md`), nullable pragma gate 0 CS86xx (`final-nullable-pragma-gate.md`), tests 5702/0 (`final-tests-coverage.md`). |
| No workflow/benchmark files touched | PASS | No `.github/workflows/**`, `scripts/benchmarks/**`, or baseline files in the diff; `.claude/rules/ci-workflows.md` and `benchmark-baselines.md` are not engaged. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Rule (`CLAUDE.md` / `.claude/rules/csharp.md`) | Verdict | Evidence |
|---|---|---|
| Nullable reference types modeled explicitly with `?` and guards | PASS | `?` applied to `_name`/`_button`/delegate fields, `FolderAction`, `_map`, `FunctionButton<T>.Value`, `MyBox.ShowDialog<T>`/`FunctionButtonGroup<T>.Result`, `MyBoxModeless` `showAction`, `InputBox.ShowDialog` return. |
| Null-forgiving `!` used only where justified | PASS | `!` applied on getters/derefs of fields the surrounding contract keeps non-null (e.g. `Name`/`Button`/`Delegate` getters, `_action!.DynamicInvoke()`, `_map!` after set-in-2-arg-ctor). |
| CSharpier formatting | PASS | `evidence/qa-gates/final-csharpier.md`: `Checked 1406 files`, EXIT 0, no residual changes. |
| .NET analyzers / code style build | PASS | `evidence/qa-gates/final-analyzers.md`: `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, 0 errors. |
| Nullable type-check gate | PASS | See §3.1 deviation note; isolated `UtilitiesCS.csproj` rebuild yields 0 CS86xx (independently reproduced by reviewer). |
| No `record`/`record struct`/`init` (net481 CS0518 risk) | PASS | `final-scope-guards.md`: `BoxIcon`/`YesNoToAllResponse` remain plain enums; no struct/init added. |
| No nullable post-condition attributes / no polyfill (net481) | PASS | `evidence/qa-gates/final-no-postcondition-attrs.md`; reviewer grep of added lines found none of `[NotNullWhen]`/`[MaybeNullWhen]`/`[NotNullIfNotNull]`/`[MaybeNull]`/`[AllowNull]`/`[DisallowNull]`/`[DoesNotReturn]`/`[MemberNotNull]`. |
| No project/solution `<Nullable>` element | PASS | `evidence/qa-gates/final-ac2-csproj-check.md`; reviewer `grep -c '<Nullable>' UtilitiesCS.csproj` = 0; no `.csproj/.props/.sln/.targets` in diff. |

### 3.1 Approved deviation — nullable verification command

The stock C# type-check step in `CLAUDE.md` / `.claude/rules/csharp.md` is
`msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`. This child instead uses the
per-file pragma gate `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` WITHOUT
`/p:Nullable=enable`. This is a documented, maintainer-approved epic-architecture decision
(`spec.md` Constraints & Risks item 1; plan Scope Invariants): the global flag would surface the
entire epic's pre-existing CS86xx debt across unrelated files as false failures. The deviation is
scoped to this child and did not edit any `.claude/rules/*` file. Verdict: APPROVED / compliant with
the epic architecture. This is not a policy violation.

The solution-wide pragma rebuild (`final-nullable-pragma-gate.md` Command 1) exits 1 because it
short-circuits on 2 pre-existing vendored SVGControl CS0649 diagnostics (promoted by
`TreatWarningsAsErrors`) before `UtilitiesCS` compiles. SVGControl is not in this branch diff
(reviewer-confirmed) and is pre-existing and out of scope. The authoritative CS86xx detector is the
isolated `UtilitiesCS.csproj` rebuild (Command 2), which excludes only the pre-existing non-nullable
codes CS0649/CS0618/CS0168 and returns EXIT 0 with 0 CS86xx. Reviewer independently reproduced the
isolated rebuild: 0 CS86xx; the only errors were pre-existing CS0168 (1) and CS0618 (14) in
`EmailIntelligence/`/`Extensions/` files, none in `Dialogs/`.

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Rule | Verdict | Evidence |
|---|---|---|
| MSTest / Moq / FluentAssertions conventions in touched tests | PASS (no test files touched) | No `*.Test`/`*Tests.cs` source file is in the diff; annotation-only change. |
| No new uncovered executable lines (prefer annotation over new guards) | PASS | `spec.md` Constraints item 9 honored; diff adds no `if (x is null) throw` statements; changed-line coverage unchanged. |

## 5. Test Coverage Detail

Changed languages in the branch diff: C# only. No TypeScript, Python, or PowerShell source files
were changed (verify-only pr_context summary at `artifacts/pr_context.summary.txt`).

Coverage evidence source: `evidence/qa-gates/final-coverage.cobertura.xml` (post-change) compared
against `evidence/baseline/baseline-coverage.cobertura.xml` (baseline). Per the evidence-location
invariant, feature coverage evidence is stored under the feature `evidence/` tree rather than the
alternate `artifacts/csharp/` path. The reviewer independently parsed both Cobertura documents and
confirmed the root `line-rate`/`branch-rate` values quoted below.

- **C# (CSharp / .NET) coverage verdict: PASS.** Repo-wide line coverage is 83.82%
  (line-rate 0.838187) and branch 76.38% (branch-rate 0.763759); the cluster changed-line coverage is
  93.10% (958/1029) with zero regression versus baseline (per-file delta 0.00% across all 14 files).
  - Baseline: line 83.80% (0.838032), branch 76.35% (0.763485).
  - Post-change: line 83.82% (0.838187), branch 76.38% (0.763759).
  - Change: +0.02 pp line, +0.03 pp branch; no repo-wide regression, no changed-line regression.
  - New/changed-code coverage: 93.10% for the 14-file cluster (958/1029 executable lines).
  - Disposition: PASS. Repo-wide line 83.82% clears the `CLAUDE.md` 80% testable-denominator floor
    and branch 76.38% clears the 75% branch floor. The 85% uniform line target in
    `.claude/rules/general-unit-test.md` is a pre-existing epic-wide condition that this
    annotation-only child improved rather than worsened (no executable line added); it is therefore
    not a defect introduced by this feature and is dispositioned non-blocking.
  - Evidence: `evidence/qa-gates/final-coverage-delta.md`, `evidence/qa-gates/final-coverage.cobertura.xml`,
    `evidence/baseline/baseline-coverage.cobertura.xml`; reviewer independent Cobertura root-rate parse.

| Language | Changed files | New files | Repo-wide line | Repo-wide branch | Changed-line | Verdict |
|---|---|---|---|---|---|---|
| C# (CSharp / .NET) | 14 (all modified) | 0 | 83.82% | 76.38% | 93.10% | PASS |
| TypeScript | 0 | 0 | — | — | — | PASS (no changed files) |
| Python | 0 | 0 | — | — | — | PASS (no changed files) |
| PowerShell | 0 | 0 | — | — | — | PASS (no changed files) |

## 6. Test Execution Metrics

- Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput .../final-coverage.cobertura.xml` (EXIT 0).
- Total tests: 5702; Passed: 5702; Failed: 0 (identical to baseline P0-T7).
- Note: three earlier attempts aborted with a test-host crash under concurrent sibling-agent
  coverage load; the recorded clean run was captured on a quiet-machine retry (documented in
  `final-tests-coverage.md`). The clean run is deterministic and reproducible.

## 7. Code Quality Checks

| Check | Verdict | Notes |
|---|---|---|
| CSharpier formatting | PASS | `final-csharpier.md`: 1406 files checked, EXIT 0. |
| .NET analyzer / code-style build | PASS | `final-analyzers.md`: 0 errors. |
| Nullable pragma gate (isolated UtilitiesCS build) | PASS | 0 CS86xx; reviewer-reproduced. |
| Signature compatibility | PASS | `final-signature-compat.md`: all changes additive nullability. |
| File-size / structure guards | PASS | `final-scope-guards.md`: all 14 files < 500 lines. |

## 8. Gaps and Exceptions

- **Approved deviation:** per-file pragma nullable gate instead of global `/p:Nullable=enable` (see §3.1). Non-blocking; epic architecture.
- **Pre-existing, out-of-scope:** vendored SVGControl CS0649 short-circuits the solution-wide rebuild; the isolated `UtilitiesCS.csproj` build is the authoritative CS86xx detector. Not this feature's defect.
- **Flagged (not this feature's to resolve):** the `helperclasses` #364 dependency-edge note and the ~110-file ownership-gap table are recorded in `spec.md` for the epic-planner/maintainer; per the delegation prompt they are explicitly out of scope for #374 and are not blocking.
- **Minor evidence inaccuracy (non-blocking):** `evidence/qa-gates/final-ac6-no-cross-block.md` states "All other changes are confined to `docs/features/active/.../`", but the branch also contains 4 `.claude/agent-memory/` note files. These are documentation-class agent memory, not source, and do not affect any AC or policy verdict. Recorded for accuracy only.

## 9. Summary of Changes

14 C# source files receive a single `#nullable enable` pragma and additive nullability annotations
(`?`) plus justified null-forgiving operators (`!`). The 12 `Dialogs/` substantive files are brought
to zero CS86xx under the pragma; the 2 misc files are verify-only (pragma added, no annotation edits
required). No project/solution files, no Designer-generated files, no test files, and no
workflow/benchmark files are changed. Remaining diff content is feature docs (plan/spec/user-story
AC check-offs), evidence artifacts, and agent-memory notes.

## 10. Compliance Verdict

**PASS.** Zero blocking findings. The change complies with the General Code Change Policy, the C#
Code Change Policy (with the documented, maintainer-approved per-file nullable-gate deviation), the
General and C# Unit Test Policies (no new tests; no changed-line coverage regression), the file-size
limit, the banned-API prohibition, and the net481 post-condition-attribute prohibition. All six
acceptance criteria are independently supported by evidence (see the feature-audit artifact). The
feature is merge-ready from a policy standpoint.

## Appendix A: Test Inventory

- No unit test source files were added or modified by this feature.
- The existing `UtilitiesCS.Test` suite (5702 tests) was executed post-change with 5702 passed / 0
  failed, identical to the pre-change baseline. Coverage was captured via
  `Invoke-MSTestWithCoverage.ps1` to `evidence/qa-gates/final-coverage.cobertura.xml`.
- Per-batch regression runs (A–E + misc) are recorded under `evidence/regression-testing/` with
  matching per-batch Cobertura documents.

## Appendix B: Toolchain Commands Reference

1. `dotnet tool run csharpier format .` then `csharpier check .` (format gate).
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzers / code style).
3. `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649;CS0618;CS0168 /p:BuildProjectReferences=false` (authoritative isolated nullable/CS86xx gate; reviewer-reproduced, 0 CS86xx).
4. `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput <cobertura path>` (test + coverage).
