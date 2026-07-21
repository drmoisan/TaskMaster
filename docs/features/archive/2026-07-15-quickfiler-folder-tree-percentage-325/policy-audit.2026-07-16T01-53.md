# Policy Compliance Audit — quickfiler-folder-tree-percentage (#325)

- Timestamp: 2026-07-16T01-53
- Reviewer: feature-review
- Feature branch: `feature/quickfiler-folder-tree-percentage-325` @ `ae104f84`
- Base (merge-base, verified): `epic/folder-tree-percentage-ui-integration` @ `34ed0422` (epic-child PR base is the integration branch, not `main`)
- Work mode: `full-feature` (from `issue.md`)
- Diff command: `git diff epic/folder-tree-percentage-ui-integration...HEAD`
- Overall verdict: PASS
- blocking_count (this artifact): 0

## Scope and Baseline

The audit scope is the full branch diff against the resolved base branch. `git merge-base HEAD origin/epic/folder-tree-percentage-ui-integration` and the local/remote epic ref all resolve to `34ed0422`, so the caller-supplied base is current (no stale-merge-base condition). The changed set (source only; docs/evidence omitted here) is:

- New C# production seams: `UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs`, `FolderNodeViewModel.cs`, `FolderHierarchyBuilder.cs`, `FolderTreeStateModel.cs`
- New C# tests: `UtilitiesCS.Test/OutlookObjects/Folder/{PercentageFormatter,FolderNodeViewModel,FolderHierarchyBuilder,FolderTreeStateModel}Tests.cs`, `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs`
- Modified C#: `QuickFiler/Viewers/IItemViewer.cs` (+additive member), `ItemViewer.FolderSearch.cs` (owner-draw/hit-test glue), `ItemViewer.Designer.cs` (owner-draw config), `QuickFiler/Controllers/KeyboardHandler.cs` (arrow routing), `QfcItemController.FolderHandling.cs` (injection), `UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs` (+additive member)
- csproj Compile-Include wiring: `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`, `QuickFiler.Test.csproj`

C# is the only changed source language. No `.ts/.tsx`, `.py`, or `.ps1/.psm1` source files changed on the branch.

## Rejected Scope Narrowing

None. The caller prompt directed a full feature-vs-base audit and did not attempt to narrow scope to a plan/task/phase, subset of files, or mark any language out of scope. The plan document contains no injection directive aimed at review. No narrowing to reject.

## Evidence Location Compliance

PASS. All feature evidence is written to the canonical `<FEATURE>/evidence/<kind>/` locations (`evidence/baseline/`, `evidence/qa-gates/`, `evidence/regression-testing/`). The branch diff contains no files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. No evidence-location violation found.

## Coverage Verdicts (per changed language)

Coverage-threshold basis: `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` (uniform line >= 85%, branch >= 75%, no regression on changed lines) plus CLAUDE.md new-module target line >= 90%. The feature targets the stricter union of both documents.

- CSharp (C#) coverage verdict: PASS. The four new host-neutral seams exceed thresholds per the feature evidence Cobertura (`evidence/qa-gates/final-tests` and `coverage-delta`): PercentageFormatter line 100.00% / branch 100.00%; FolderNodeViewModel line 100.00% / branch 100.00%; FolderHierarchyBuilder line 96.55% / branch 94.44%; FolderTreeStateModel line 100.00% / branch 91.18%. All meet line >= 90% (new-module) and branch >= 75%.
- CSharp (C#) no-regression coverage verdict: PASS. Repository-wide line coverage moved 64.19% -> 64.31% (+0.12 pt) and branch 33.18% -> 33.34% (+0.16 pt); the change did not reduce coverage on any touched line. The absolute repo-wide figure is a pre-existing whole-solution value dominated by vendored/host-bound modules (Swordfish, SVGControl, WinForms designer/viewer) outside the #325 seam denominator; per the orchestrator direction it is not treated as this feature's regression.
- CSharp (C#) canonical-artifact note: the canonical `artifacts/csharp/coverage.xml` is absent in the local worktree (known TaskMaster local-run limitation; per-assembly full-suite Cobertura is trimmed/blocked locally, and repo-wide enforcement is the PR CI run). The substantive C# coverage verdict is verified PASS from the feature-evidence Cobertura figures above; the canonical-artifact absence is recorded as a non-blocking procedural item, not a code defect.
- TypeScript, Python, PowerShell: no changed source files on the branch; no coverage row required.

## Policy Checklist

### 1. General Code Change Policy (`.claude/rules/general-code-change.md`, CLAUDE.md General)

| Item | Verdict | Evidence |
|---|---|---|
| Simplicity / separation of concerns | PASS | Pure host-neutral seams (builder, state model, formatter, view model) are separated from WinForms glue (`ItemViewer.FolderSearch.cs`, Designer, `KeyboardHandler`). The ComboBox is a dumb renderer; correctness lives in tested seams. |
| Reusability / extensibility | PASS | Seams reuse existing `TreeNode<T>`; the new `IItemViewer` and `IFolderSearchHandler` members are additive. |
| Error handling / fail-fast | PASS | `FolderHierarchyBuilder.Build` guards null input; `FolderTreeStateModel` ctor coalesces null roots; no broad catch introduced. |
| File size <= 500 lines | PARTIAL (non-blocking) | New/other changed files are within limit (largest: `ItemViewer.FolderSearch.cs` 283, `FolderTreeStateModelTests.cs` 261). `QuickFiler/Controllers/KeyboardHandler.cs` is 631 lines at head, but it was already 604 lines at the base (pre-existing >500 violation); this feature added 27 lines of arrow-routing glue rather than crossing the threshold. See code-review finding CR-2. Not introduced by #325. |
| Mandatory toolchain loop | PASS | csharpier EXIT 0; analyzers EXIT 0 (0 errors); nullable/TWAE `/t:Build` EXIT 0 (0/0); tests 4760/4760 pass (uninstrumented). Evidence under `evidence/qa-gates/`. |
| Dependencies | PASS | No new dependencies; `evidence/baseline/dependency-verification` confirms reuse of `TreeNode<T>`. |
| I/O boundaries | PASS | Seams touch no disk/network/COM; the only I/O is WinForms paint in the exempt glue. |

### 2. General Unit Test Policy (`.claude/rules/general-unit-test.md`, CLAUDE.md General UT)

| Item | Verdict | Evidence |
|---|---|---|
| Independence / isolation / determinism | PASS | Tests build in-memory forests / `FolderRow[]`; no shared mutable state; INV8 test asserts repeat-projection determinism. |
| Fast / no external deps | PASS | No DB/network/COM/process; Moq isolates `IItemViewer`; controller test uses reflection to set private fields and a `FolderScorer()` default. |
| No temp files | PASS | No filesystem writes in any test. |
| Arrange-Act-Assert + intent docs | PASS | All new tests use FluentAssertions with reason strings and class-level XML doc describing scenarios. |
| Test file location mirrors source | PASS | Tests live under `UtilitiesCS.Test/OutlookObjects/Folder/` and `QuickFiler.Test/Controllers/`, mirroring source; no colocation. |
| Coverage thresholds | PASS | See Coverage Verdicts. |
| Coverage exclusion policy | PASS | `coverage.config` excludes only third-party/mixed-mode; the four new seams are in the coverage denominator; no production `src` path excluded. |

### 3. C# Code Change Policy (CLAUDE.md C#1-C#7)

| Item | Verdict | Evidence |
|---|---|---|
| C#1 csharpier formatting | PASS | `csharpier check .` — Checked 1352 files, EXIT 0. |
| C#1 .NET analyzers | PASS | `msbuild /t:Build /p:EnableNETAnalyzers /p:EnforceCodeStyleInBuild` EXIT 0, 0 errors; new files produce zero analyzer diagnostics. |
| C#1 nullable / TreatWarningsAsErrors | PASS (with documented posture) | The policy-specified gate `msbuild /t:Build /p:Nullable=enable /p:TreatWarningsAsErrors=true` passes 0/0 (EXIT 0). A supplementary `/t:Rebuild` (not the specified gate) surfaces nullable-enable-only diagnostics in new files (CS8618 `_highlighted`, CS8600/CS8625 `cumulative`) and pre-existing repo-wide debt (SVGControl). These are consistent with the repo's nullable-disabled convention and match the sibling #324 Folder types; adding `?` would emit CS8632 in the default build. Non-blocking; see code-review CR-1. |
| C#2 strong contracts / null-safety by convention | PASS | Public members have explicit types and XML docs; optional values modeled with value-type nullables (`double?`, `char?`, `FolderScore?`). |
| C#3 focused classes/methods | PASS | Each seam has a single responsibility; methods are small and shallow. |
| C#4 exceptions / logging | PASS | No broad catch; no ad-hoc console logging introduced. |
| C#5 module structure / internal surface | PASS | Glue helpers are `internal`; seams are assembly-public as required by tests. |
| C#7 additive interface members | PASS | See Interface-Change Assessment. |

### 4. C# Unit Test Policy (CLAUDE.md CUT1-CUT3)

| Item | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]`/`[TestMethod]` throughout. |
| Moq for mocking | PASS | `Mock<IItemViewer>` in controller-injection tests. |
| FluentAssertions | PASS | `.Should()...` assertions with reasons in all new tests. |
| Toolchain command order | PASS | Evidence follows csharpier -> analyzers -> nullable -> vstest order. |

## Interface-Change Assessment (`FolderRow[] FolderRowArray { get; }` on `IFolderSearchHandler`)

Verdict: acceptable additive change; non-blocking.

- The member was not in the plan's named file set; it was added as necessary integration wiring so the controller can hand the row model to `SetFolderSuggestions`. The addition is minimal and consistent with the interface's stated purpose (a narrow seam over the `FolderPredictor` members `QfcItemController` consumes).
- Contract preserved: existing members (`FolderArray`, `Suggestions`, `FindFolder`) are unchanged; the new member is additive only.
- All implementers satisfied: the sole production implementer is `FolderPredictor` (`FolderPredictor.IFolderSearchHandler.cs`: `public partial class FolderPredictor : IFolderSearchHandler { }`), which already implements `FolderRowArray` (pre-existing, `FolderPredictor.cs:237`). The test double `FakeFolderHandler` implements it. The analyzers/type-check build is green, confirming no unimplemented-member break.
- Breaking-change concern: adding a member to a public interface is source-breaking for any out-of-repo implementer. None exists in-repo, and `IFolderSearchHandler` is a newly introduced narrow seam in this epic lineage, so practical breakage risk is nil. Acceptable.

## Nullable-Posture Ruling (blocking vs non-blocking)

Verdict: non-blocking; convention-consistent posture.

The policy-mandated type-check gate in CLAUDE.md C#1.3 / CUT3 is `msbuild ... /t:Build /p:Nullable=enable /p:TreatWarningsAsErrors=true`. That exact command passes 0 warnings / 0 errors (`evidence/qa-gates/final-nullable`), identical to the P0 baseline. The `/t:Rebuild` diagnostics are a supplementary observation, not the specified gate. The surfaced items are not real defects: `_highlighted` is intentionally null until `Highlight()` is called (documented INV3, `Highlighted` returns null when nothing is highlighted); `cumulative` is initialized to null then always assigned before use because `string.Split` returns at least one segment. The whole repository is nullable-disabled by convention and every sibling #324 Folder type follows the same pattern. Remediation (adding `?` annotations) would emit CS8632 in the default build and break the analyzer/incremental gates; a proper fix requires a repo-wide `#nullable enable` migration that is out of this feature's scope. Recommended follow-up: a separate maintainer-owned nullable-context migration issue. No remediation required for #325.

## Toolchain Evidence Summary

| Gate | Command | Result | Evidence |
|---|---|---|---|
| Format | `csharpier check .` | EXIT 0, 1352 files clean | `evidence/qa-gates/final-csharpier` |
| Analyzers | `msbuild /t:Build /p:EnableNETAnalyzers /p:EnforceCodeStyleInBuild` | EXIT 0, 0 errors, 74 pre-existing warnings | `evidence/qa-gates/final-analyzers` |
| Nullable/TWAE | `msbuild /t:Build /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT 0, 0/0 | `evidence/qa-gates/final-nullable` |
| Tests | `vstest.console.exe ... /InIsolation` | 4760/4760 pass (uninstrumented) | `evidence/qa-gates/final-tests` |
| Coverage delta | Cobertura baseline vs post | No-regression PASS; per-seam PASS | `evidence/qa-gates/coverage-delta` |
| Non-interference (9004) | `git diff --name-only` scan | PASS, disjoint from 9004 + dead variants | `evidence/qa-gates/non-interference-9004` |

Note on instrumented run: 20 timing-sensitive `UtilitiesCS.Test` tests flake under coverage instrumentation + parallelism; the identical pre-existing tests flake at the P0 baseline and pass on the uninstrumented re-run. Not caused by #325 (same-commit instrumented-vs-uninstrumented divergence). Non-blocking.

## Findings

- No blocking findings.
- Non-blocking: CR-1 (nullable `/t:Rebuild` posture), CR-2 (`KeyboardHandler.cs` pre-existing >500 lines), CR-3 (INV8 equal-score ordinal tie-break not independently unit-tested), and the canonical `artifacts/csharp/coverage.xml` absence (procedural; substantive coverage verified from feature evidence).

## Summary

All mandatory policy gates pass as specified. Coverage for the new host-neutral seams is verified from the feature-evidence Cobertura and exceeds thresholds with no repo-wide regression. The additive `IFolderSearchHandler.FolderRowArray` member and the nullable posture are ruled non-blocking with cited rationale. Policy audit verdict: PASS. blocking_count: 0.
