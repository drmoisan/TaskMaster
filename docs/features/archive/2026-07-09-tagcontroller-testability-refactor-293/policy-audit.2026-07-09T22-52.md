# Policy Compliance Audit — Issue #293 (tagcontroller-testability-refactor)

- Feature folder: `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/`
- Work mode: `full-feature` (AC source per issue.md exception: `spec.md` + `issue.md`; `user-story.md` waived by epic #295)
- Parent epic: winforms-testability-refactor (#295), wave 0
- Base branch (resolved): `epic/winforms-testability-refactor-integration`
- Merge-base SHA: `3f04d50f6544f084323e5d7a9a563facb9d579df`
- Head SHA: `55a4835659f977a0dce9e1f5f872b121b659167d`
- Head branch: `feature/tagcontroller-testability-refactor-293`
- Audit timestamp: 2026-07-09T22-52
- Scope: full branch diff against the merge-base (feature-vs-base). Not narrowed to any plan/task/phase subset.

## Executive Summary

Overall policy verdict: **FAIL (one Blocking finding; remediation required).**

The refactor is structurally sound and meets its testability intent: `ITagViewer : IForm` is introduced, `TagController` depends only on the seam, host-neutral logic is extracted into `TagSelectionModel`, dialog and focus-draw COM dependencies are isolated behind `IUserPrompt`/`_drawFocus`, and the maintainer-ratified STA refinement is honored precisely (STA control construction confined to the two dedicated `*.StaTests.cs` files, seams-first, no `Show()`/`ShowDialog()`, no message pump/timer/sleep, no `Form`-derived types, controls disposed). The Coverage Exemption Register is applied as planned, with no `[ExcludeFromCodeCoverage]` on any testable seam. Committed coverage evidence reports Tags.dll line coverage 92.63%, clearing the CLAUDE.md 80% floor and the 90% new-module target.

One Blocking policy finding prevents a GO: the new test file `Tags.Test/TagControllerSeamTests.cs` is **579 lines**, exceeding the repository 500-line file-size limit, which applies to test code per `.claude/rules/general-code-change.md` (File Size Limit) and CLAUDE.md §4.1 / §C#5.1. The executor's `file-size-compliance.md` evidence measured production files only and did not catch this. Remediation: split the file into two or more `<= 500`-line test files. See `remediation-inputs.2026-07-09T22-52.md`.

## 1. Scope and Baseline

Branch diff vs merge-base `3f04d50f` (verified with `git diff --numstat` / `--name-status`):

C# production changes (Tags project):
- `Tags/ITagViewer.cs` (NEW, 59) — viewer seam interface `: IForm`.
- `Tags/IUserPrompt.cs` (NEW, 21) — dialog seam interface.
- `Tags/WinFormsUserPrompt.cs` (NEW, 25) — production dialog adapter, `[ExcludeFromCodeCoverage]` (register E1).
- `Tags/TagSelectionModel.cs` (NEW, 224) — host-neutral selection/search/filter/prefix logic.
- `Tags/TagController.cs` (MODIFIED, 435) — controller orchestration; constructor now takes `ITagViewer` + optional `IUserPrompt`/`Action<CheckBox>`.
- `Tags/TagController.Rendering.cs` (NEW partial, 327) — rendering + keyboard navigation + `DrawFocus` seam.
- `Tags/LauncherAutoAssign.cs` (NEW, extracted, 112) — pure delegate wiring, NOT exempt.
- `Tags/TagLauncher.cs` (MODIFIED, 169) — live-form launcher, `[ExcludeFromCodeCoverage]` (register E5).
- `Tags/TagViewer.cs` (MODIFIED, 167) — `: Form, ITagViewer`, `[ExcludeFromCodeCoverage]` (register E3).
- `Tags/Helper Classes/CheckBoxController.cs` (MODIFIED, 257) — decision logic extracted/covered; exemption narrowed to 4 focus/key handlers (register E6).
- `Tags/Tags.csproj`, `Tags/properties/AssemblyInfo.cs` (MODIFIED) — compile entries / metadata.

C# test changes (Tags.Test project):
- NEW: `CheckBoxControllerDecisionTests.cs` (74), `CheckBoxControllerWiring.StaTests.cs` (104), `Fakes/FakeTagViewer.cs` (137), `LauncherAutoAssignTests.cs` (99), `TagControllerRendering.StaTests.cs` (63), `TagControllerSeamTests.cs` (**579**), `TagSelectionModelTests.cs` (228).
- MODIFIED: `TagControllerCoverageExpansionTests.cs` (466), `TagControllerTests.cs` (105), `Tags.Test.csproj`.

Non-code changes: feature docs/evidence under the feature folder (issue.md, spec.md, plan, baseline/qa-gate evidence, runsettings).

Languages with changed files in the branch diff: **C# only** (plus Markdown, no coverage obligation). No `.ts/.tsx`, `.py`, or `.ps1/.psm1` files changed.

## 2. General Code Change Policy (`.claude/rules/general-code-change.md`, CLAUDE.md)

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity / smallest structural design | PASS | Controller decomposed along logical boundaries; state moved into a single cohesive `TagSelectionModel`; seams are minimal and intent-named. |
| Separation of concerns (I/O isolated) | PASS | `TagSelectionModel` has zero `System.Windows.Forms` references; dialogs isolated behind `IUserPrompt`; focus-draw isolated behind `Action<CheckBox> _drawFocus`. |
| Fail-fast error handling | PASS (one Low nit) | Guard clauses throw `ArgumentException`/`ArgumentOutOfRangeException`. `LauncherAutoAssign.AutoFindAsync` has a `try { } catch (Exception) { throw; }` that adds no context (see code-review Low finding); not a policy FAIL. |
| **File size <= 500 lines (production, test, and reusable script)** | **FAIL** | Production files all `<= 435`. Test file `Tags.Test/TagControllerSeamTests.cs` = **579 lines** (`awk`/`wc -l` both 579), exceeding the 500 limit. Test code is explicitly in scope for this limit. |
| Public API / naming | PASS | PascalCase types/members, camelCase locals/fields; XML docs on new public surfaces. |
| Dependencies | PASS | No new production or test dependencies (spec invariant honored). |

## 3. C# Code Change Policy + Toolchain (CLAUDE.md C#1–C#7)

Toolchain gates (executor evidence, `evidence/qa-gates/final-qa-summary.md`; independent re-run not performed — no msbuild/vstest on PATH in this worktree; evidence-verification model per feature-review skill):

| Stage | Command | Result | Evidence |
|---|---|---|---|
| Format (CSharpier) | `csharpier check .` | PASS (1331 files, 0 changes) | `final-csharpier.md`, `final-qa-summary.md` |
| Lint (.NET analyzers) | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | PASS (0 errors; 0 Tags/Tags.Test warnings) | `final-analyzer.md` |
| Type-check (nullable/TWAE) | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | PASS (0 warnings, 0 errors) | `final-nullable.md` |
| Test (MSTest + coverage) | `vstest.console.exe Tags.Test.dll /EnableCodeCoverage /Settings:tags-coverage.runsettings` | PASS (64/64) | `final-coverage.md` |

MSTest + Moq + FluentAssertions used as required (CUT1/CUT2). Nullable enabled and clean. Note: the toolchain gates report clean, but the file-size policy check in Section 2 is a static policy rule independent of the compiler/analyzer gates and is not enforced by the toolchain; hence the toolchain being green does not clear the Section 2 FAIL.

## 4. Unit Test Policy (General + C# Unit Test Policy)

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence / isolation / determinism | PASS | Tests target single behaviors; no shared mutable state; no wall-clock/RNG. Determinism scan (`determinism-scan.md`) clean. |
| No external dependencies / no temp files | PASS | No live `Form`, no popup, no temp-file API. Determinism scan: zero matches for `Task.Delay`/`Thread.Sleep`/`DoEvents`/`GetTempPath`/`MessageBox`/`InputBox`/`new TagViewer(`. Independently re-verified: `grep` in `Tags.Test` returned NONE. |
| Banned APIs in test code | PASS | No `Task.Delay`/`Thread.Sleep`/`Date.Now`/`DateTime.Now` in touched test files (independently grepped). Prior `[STAThread]` + `Task.Delay(50)` anti-patterns removed. |
| STA confinement (maintainer-ratified refinement) | PASS | `[STATestClass]`/`[STATestMethod]` occur only in `TagControllerRendering.StaTests.cs` and `CheckBoxControllerWiring.StaTests.cs`. Both construct only unshown `CheckBox` controls (never `Form`), never call `Show()`/`ShowDialog()`, use no pump/timer/sleep, and dispose via `using`/explicit `Dispose()`. Clicks raised via reflection `Control.InvokeOnClick`. |
| Scenario completeness | PASS | Positive/negative/edge/error scenarios per spec Test Strategy (search parse, filter, prefix-missing, toggle, navigation bounds, dialog decline, empty auto-assign). |
| Test file location | PASS | Tests live in `Tags.Test/` mirroring `Tags/`. |
| **Test file size <= 500 lines** | **FAIL** | `TagControllerSeamTests.cs` = 579 lines. See Section 2. |

## 5. Coverage Verification (mandatory for every changed language)

Coverage artifact note: the generic canonical path `artifacts/csharp/coverage.xml` is absent in this worktree because `artifacts/` is `.gitignore`d (line 57) working-tree output and is not committed to the branch. The authoritative, committed C# coverage evidence for this feature is the executor-produced markdown under `evidence/qa-gates/` (`final-coverage.md`, `coverage-delta.md`) plus the scoped `tags-coverage.runsettings`, inspected directly (not re-run). Internal consistency verified: 704/760 = 92.63%. Independent coverage regeneration was not performed (no msbuild/vstest on PATH; the feature-review skill's evidence-verification model applies, and coverage is not re-run).

| Language | C# / .NET coverage row | Baseline | Post-change | Change | New/changed-code coverage | Disposition | Verdict |
|---|---|---|---|---|---|---|---|
| C# | `Tags.dll` module line coverage (project floor) | 67.28% (516/767) | 92.63% (704/760) | +25.35% | 89.71% (lowest non-exempt changed partial; new modules 93.33–97.50%) | project >= 80% floor met (>= 85% uniform-tier line met) | PASS |
| C# | `Tags.TagSelectionModel` (new module) | not present | 97.50% | +97.50% | 97.50% line | new module >= 90% met | PASS |
| C# | `Tags.LauncherAutoAssign` (extracted, exemption removed) | exempt (0 counted) | 93.33% | +93.33% | 93.33% line | new module >= 90% met | PASS |
| C# | `Tags.TagController` (+ `.Rendering` partial) | partial | 95.10% / 89.71% | increase | 95.10% / 89.71% line | controller >= 80% met | PASS |
| TypeScript | coverage (no changed files) | N/A | N/A | N/A | N/A | no `.ts/.tsx` files in diff | N/A |
| Python | coverage (no changed files) | N/A | N/A | N/A | N/A | no `.py` files in diff | N/A |
| PowerShell | coverage (no changed files) | N/A | N/A | N/A | N/A | no `.ps1/.psm1` files in diff | N/A |

C# coverage verdict: **PASS**. Tags.dll line coverage 92.63% clears the CLAUDE.md 80% floor and the 85% uniform-tier line floor; both new modules exceed the 90% new-module target; the changed controller partials exceed the 80% controller target; `coverage-delta.md` records no regression on changed lines. Branch coverage is not separately reported in the committed evidence; the operative CLAUDE.md UT2 gate for this repo's C# is line coverage, which passes. This is noted as an evidence limitation, not a coverage FAIL.

## 6. Coverage Exemption Register Conformance

Independently verified `[ExcludeFromCodeCoverage]` placements (grep across `Tags/`):
- `Tags/WinFormsUserPrompt.cs` L14 — class-level (register E1, live dialog UI). Conformant.
- `Tags/TagViewer.cs` L18 — class-level (register E3, `Form`-derived). Conformant.
- `Tags/TagLauncher.cs` L15 — class-level (register E5, live-form/globals wiring). Conformant.
- `Tags/Helper Classes/CheckBoxController.cs` L172/221/230/239 — on exactly the four sanctioned members `ctrlCB_KeyDown`, `ctrlCB_GotFocus`, `ctrlCB_LostFocus`, `ctrlCB_PreviewKeyDown` (register E6, narrowed). Conformant.
- No `[ExcludeFromCodeCoverage]` on `TagSelectionModel`, `LauncherAutoAssign`, `TagController`, or `TagController.Rendering` (including `DrawFocusDefault`, register E2 removed — covered by STA test). Conformant: no testable seam is exempt.

Note on policy layering: CLAUDE.md UT2 (authoritative, ordered first) permits `[ExcludeFromCodeCoverage]` for maintainer-ratified COM/VSTO/WinForms wiring. This governs over the general-unit-test.md "no production file excluded" glob-exclude prose, which addresses coverage-config path excludes, not per-member attributes. The attribute exemptions here are individually justified in-source and ratified by the epic's STA refinement. Verdict: PASS.

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| File-size scan (production) | `awk NR` on `Tags/*.cs` | PASS (max 435) |
| File-size scan (test) | `awk NR` / `wc -l` on `Tags.Test/*.cs` | FAIL (`TagControllerSeamTests.cs` 579 > 500) |
| Determinism / banned-API scan | `grep -rnE` on `Tags.Test` | PASS (0 matches) |
| STA-confinement scan | `grep -rn STATest` on `Tags.Test` | PASS (2 sanctioned files only) |
| Evidence-location scan | `git diff --name-only | grep artifacts/(baselines|qa|evidence|coverage)` | PASS (0 matches) |

## Evidence Location Compliance

All evidence artifacts are written under the canonical `<FEATURE>/evidence/<kind>/` location (`baseline/`, `qa-gates/`). Branch-diff scan for prohibited paths (`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`) returned zero matches. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` conditions. Verdict: PASS.

## Policy Rule: modified-workflow-needs-green-run

The branch diff modifies no path matching `.github/workflows/**`, `scripts/benchmarks/**`, or `.github/actions/**` (verified via `git diff --name-only`). The rule does not fire. Verdict: not applicable.

## Rejected Scope Narrowing

None. The caller prompt requested a full feature-vs-base audit against `epic/winforms-testability-refactor-integration` and did not attempt to narrow scope to a plan/task/phase subset, to a subset of changed files, or to mark any changed language's coverage as out of scope. The reminder that the child->integration PR runs zero required CI checks is a factual statement about branch-protection triggers, not a directive to skip the local coverage/file-size gate; the full local audit was performed. The plan's trailing `DIRECTIVE: PREFLIGHT VALIDATION ONLY` line is standard planner/executor handoff text, not a narrowing directive aimed at feature-review.

## Verdict

**FAIL — remediation required before merge.** One Blocking finding: `Tags.Test/TagControllerSeamTests.cs` (579 lines) violates the 500-line file-size limit for test code (`.claude/rules/general-code-change.md` File Size Limit; CLAUDE.md §4.1 / §C#5.1). All other policy dimensions PASS. See `remediation-inputs.2026-07-09T22-52.md`.

## Appendix A — Independent verification commands

- `git merge-base HEAD origin/epic/winforms-testability-refactor-integration` -> `3f04d50f...` (base confirmed; head `55a4835...`).
- `git diff --numstat 3f04d50f HEAD` -> C#-only change set (Tags + Tags.Test) plus feature docs.
- `awk 'END{print NR}'` / `wc -l` on each changed `.cs` -> production max 435; `TagControllerSeamTests.cs` 579.
- `grep -rn "ExcludeFromCodeCoverage" Tags/` -> 4 CheckBoxController members + 3 class-level (WinFormsUserPrompt/TagViewer/TagLauncher); none on testable seams.
- `grep -rn "STATest" Tags.Test` -> only the two sanctioned `*.StaTests.cs` files.
- `grep -rnE "Task\.Delay|Thread\.Sleep|DoEvents|MessageBox|InputBox|new TagViewer\(|new Form\(" Tags.Test` -> NONE.
- `grep -n "class TagViewer" Tags/TagViewer.cs` -> `public partial class TagViewer : Form, ITagViewer`.

## Appendix B — Executor toolchain command reference (from feature evidence)

1. `csharpier check .` (final-csharpier.md, EXIT 0)
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (final-analyzer.md, EXIT 0)
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` (final-nullable.md, EXIT 0)
4. `vstest.console.exe Tags.Test/bin/Debug/Tags.Test.dll /EnableCodeCoverage /Settings:tags-coverage.runsettings` (final-coverage.md, EXIT 0, 64/64)
