# Policy Audit — folderconverter-folderpredictor-dead-code-and-bugs (#732)

- Timestamp: 2026-09-03T13-00
- Branch: bug/folderconverter-folderpredictor-dead-code-and-bugs-732
- HEAD: b1e78c4a78d1298a3704d293770a3dbe96e19718
- Anchor (resolved base): origin/main tip 87233f867ad60c0a5c0d19b09cc121ae536d7ba1 (re-fetched; `git merge-base origin/main HEAD` == origin/main tip, confirming origin/main is a direct ancestor of HEAD — the two-dot diff `origin/main..HEAD` is the correct, non-degraded scope)
- Work Mode: `full-bug` (declared in issue.md) — AC source is `spec.md` only, per policy-compliance-order and acceptance-criteria-tracking
- Reviewer worktree: C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-aa274c17b2c682ab3 (all reads/writes performed here per binding instructions)

## Rejected Scope Narrowing

None detected. The delegation prompt's "BASH DISCIPLINE" and worktree-addressing instructions are tooling/mechanics directives, not scope narrowing, and were followed as procedural constraints only. No instruction attempted to exclude a changed-file language, narrow the diff to a plan/task subset, or mark any coverage/toolchain check as out of scope. The full `origin/main..HEAD` diff (43 files) was audited in its entirety.

## Full Branch Footprint (origin/main..HEAD)

```
D  UtilitiesCS.Test/OutlookExtensions/FolderConverter_Tests.cs
M  UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs
D  UtilitiesCS/EmailIntelligence/FolderConverter.cs
M  UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs
A  docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732/** (issue.md, spec.md, plan.2026-09-02T12-01.md, research/, evidence/**)
```

4 source files (2 production, 2 test), all inside the spec.md Write Set. Every other changed path is documentation/evidence under the feature folder. No file under `UtilitiesCS/Threading/**` or `UtilitiesCS/To Depricate/FileIO2.cs` appears in the diff (independently confirmed via `git diff --name-status origin/main..HEAD`, cross-checked against the executor's own P5-T6 scope-boundary evidence).

Changed language: C# only (0 Python/TypeScript/PowerShell files changed — N/A verdicts for those languages are correct per the zero-changed-files exception).

## Policy Compliance Order Applied

1. CLAUDE.md — all sections
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md` (via CLAUDE.md's C# Code/Unit Test Policy sections, which are the operative text in this repo)
5. `.claude/rules/quality-tiers.md` (uniform 85%/75% coverage floors)

No conflicting instructions were found requiring a halt.

## General Code Change Policy — PASS

| Check | Verdict | Evidence |
|---|---|---|
| Bugfix Workflow (RED test first, minimal fix, toolchain verified) | PASS | `evidence/regression-testing/p1-t4-*.md` (RED, IndexOutOfRangeException, EXIT_CODE 1 expected) -> `evidence/regression-testing/p2-t4-*.md` (GREEN, Passed:1). Fix is a single conditional-expression edit at FolderPredictor.cs:691; no opportunistic refactor. |
| Design principles (simplicity, separation of concerns) | PASS | Fix is a minimal boolean-logic correction; no new abstraction introduced. |
| Error handling / fail fast | PASS | Fix closes an unintended crash path without adding new suppression or broad catch. |
| File size limit (500 lines) | **PRE-EXISTING VIOLATION, not introduced by this PR** | See "File Size Limit" section below. |
| Naming / public API stability | PASS | `CreateFolder`'s public signature is unchanged; no breaking change. |
| Dependencies | PASS | No new NuGet package or library introduced. |

### File Size Limit

Both touched production/test files already exceeded the repository's 500-line cap at the resolved base commit, before this PR touched them:

- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`: 1000 lines at origin/main -> 1003 lines at HEAD (independently counted via `git show <sha>:<path> | wc -l`). Delta +3.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`: 1043 lines at origin/main -> 1066 lines at HEAD. Delta +23 (one new `[TestMethod]`).

This is a pre-existing policy violation neither introduced nor meaningfully worsened by this change; the Bugfix Workflow directive ("change only what is needed... avoid opportunistic refactors") correctly precludes splitting either file as part of a minimal defect fix. Not scored as blocking for this PR. See the code-review artifact for the one evidence-accuracy defect found in the executor's own line-budget self-check for the production file.

## General Unit Test Policy — PASS

| Check | Verdict | Evidence |
|---|---|---|
| Framework/mocking/assertions (MSTest, Moq, FluentAssertions) | PASS | New test uses `[TestMethod]`, `Action act = () => ...`, `act.Should().NotThrow()` (FluentAssertions), consistent with the file's existing convention. |
| Independence / isolation / determinism | PASS | New test constructs its own mocks (`CreateFolder`, `CreateApplication`, `CreateGlobals`, `TestableFolderPredictor`); no shared mutable state; no wall-clock/temp-file use. |
| AAA structure, documented intent | PASS | Arrange/Act/Assert sections present; a comment states the regression rationale and citation to issue #732. |
| RED-then-GREEN regression discipline | PASS | Independently verified: `p1-t4` shows EXIT_CODE 1, Failed:1, message containing `IndexOutOfRangeException` at `FolderPredictor.cs:line 691`, run against the pre-fix code; `p2-t4` shows the identical filter, EXIT_CODE 0, Passed:1, run against the post-fix code. |
| Pre-existing tests unmodified and still passing | PASS | `p2-t5`: the three pre-existing `CreateFolder` tests (`...StartsWithSeparator...`, `InjectedDirectory_CreateFolder...`, `...AncestorIsNull...`) all pass, 3/3, after the fix. Confirmed unmodified via the diff (only one new test method inserted; no existing test body touched). |
| No temp files / external dependencies | PASS | Test uses in-memory mocks only. |
| Test file location | PASS | Test lives in `UtilitiesCS.Test/OutlookObjects/Folder/`, mirroring `UtilitiesCS/OutlookObjects/Folder/`, consistent with existing convention (not the `tests/` tree convention, but this matches the repo's established parallel-project MSTest layout, which pre-dates this change and is out of scope to alter). |

## Coverage Verification

### C# — Cobertura (`evidence/baseline/coverage-baseline.cobertura.xml`, `evidence/qa-gates/coverage-final.cobertura.xml`)

Independently re-parsed both committed XML files' root `<coverage>` elements (not trusted from the .md summaries):

| Metric | Baseline (origin/main-equivalent BASELINE_SHA) | Final (HEAD) | Verdict |
|---|---|---|---|
| Root `line-rate` (whole-instrumented-process, includes vendored assemblies) | 0.7072916597091885 (70.73%) | 0.7073783191750814 (70.74%) | Matches the executor's claimed figures exactly; no discrepancy found. |
| Root `lines-covered`/`lines-valid` | 105895 / 149719 | 105920 / 149736 | Matches claimed figures exactly. |
| Root `branches-covered`/`branches-valid` | 13216 / 28246 (46.79%) | 13223 / 28250 (46.81%) | Not separately claimed by executor; independently derived, consistent with a marginal improvement. |

**Repo-scoped (whole-instrumented-process) line coverage is 70.74%, below both CLAUDE.md's 80% floor and the uniform 85% floor in `.claude/rules/general-unit-test.md` / `quality-tiers.md`.** Per the mandatory Coverage Verification procedure, this is recorded as **FAIL** at the repo-wide tier. This is flagged, not silently normalized, but its disposition is **non-blocking** for this PR:
- The gap is a long-standing, repo-wide, vendor-inflated artifact of measuring a whole `UtilitiesCS.Test`-scoped in-process run rather than a first-party-only figure (a pattern independently corroborated across many prior reviews of this repository — the same raw-vs-first-party gap recurs whenever the full instrumented process, including third-party/vendored assemblies loaded at runtime, is measured).
- This PR **does not regress** the figure: 70.7292% -> 70.7378% (+0.0086 points).
- No new source file was added by this PR (only a deletion and a small in-place edit), so the "new code >= 90%" tier does not apply to any file in this diff.

**Modified-file tier (the only tier this PR's substance actually touches):** `UtilitiesCS.FolderPredictor` class-level Cobertura figures:

| | Baseline | Final | Verdict |
|---|---|---|---|
| Class line-rate | 89.26% (0.8926014319809069) | 89.81% (0.8981042654028436) | **PASS** — exceeds 85% floor, improved, no regression |
| Class branch-rate | 88.81% (0.8880597014925373) | 89.13% (0.8913043478260869) | **PASS** — exceeds 75% floor, improved, no regression |
| Changed line 691 condition-coverage | 100% (2/2) — single bitwise-`|` jump | 83.33% (5/6) — three short-circuit jump points from the new `||`/`&&` decomposition | See code-review artifact; one of the three decomposed jump conditions (`olAncestor.EndsWith("\\")` == true) is not exercised by any test. Pre-existing logic path, not newly introduced by the fix's guard clause; non-blocking. |

The deleted file `UtilitiesCS/EmailIntelligence/FolderConverter.cs` had no `<Compile Include>` entry in any `.csproj` (independently confirmed: zero matches in `UtilitiesCS/UtilitiesCS.csproj`), so it was never in the coverage denominator; its removal has zero coverage impact, consistent with the spec's claim.

- Python: N/A (0 changed files)
- PowerShell: N/A (0 changed files)
- TypeScript: N/A (0 changed files)

## Evidence Location Compliance

No occurrence of `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` in the branch diff (`git diff --name-only` against `artifacts/baselines`, `artifacts/qa`, `artifacts/coverage`, `artifacts/evidence` path filters returned empty). All evidence is written under the canonical `docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732/evidence/{baseline,regression-testing,other,qa-gates}/` tree, consistent with `evidence-and-timestamp-conventions`. `validate_evidence_locations.py` does not exist in this repository (consistent with prior review findings that several named validator scripts referenced in shared skill docs are not present in TaskMaster); compliance was instead verified by direct diff-path inspection. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` cases arose — the delegation prompt did not supply a non-canonical evidence path.

## Toolchain (independently re-verified from evidence, not trusted from the executor's summary)

| Stage | Command | Evidence | Verdict |
|---|---|---|---|
| Format (apply, scoped) | `dotnet tool run csharpier format <2 files>` | `qa-gates/p5-t1-*.md`: EXIT_CODE 0, "Formatted 2 files" | PASS |
| Format (verify, repo-wide, CI parity) | `dotnet tool run csharpier check .` | `qa-gates/p5-t2-*.md`: "Checked 1574 files", EXIT_CODE 0, empty reported list | PASS |
| Analyzer rebuild | `msbuild ... /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | `qa-gates/p5-t3-*.md`: EXIT_CODE 0, 0 occurrences of `Skipping target "CoreCompile"` (proves a genuine forced rebuild, not an incremental no-op), 40 occurrences of `(Rebuild target(s))` | PASS |
| Nullable/TreatWarningsAsErrors rebuild | `msbuild ... /t:Rebuild ... /p:TreatWarningsAsErrors=true` | `qa-gates/p5-t4-*.md`: EXIT_CODE 0, "Build succeeded. 0 Warning(s) 0 Error(s)." | PASS |
| Test (full scoped suite + coverage) | `vstest.console.exe` via `dotnet-coverage collect` (`Invoke-MSTestWithCoverage.ps1` substituted due to a documented pre-existing `.claude`-path-exclusion defect in that script, out of this PR's scope) | `qa-gates/p5-t5-*.md`: EXIT_CODE 0, "Total tests: 4786 Passed: 4786" | PASS — Passed count = baseline 4785 + 1 (the new regression test), Failed 0 |

All four toolchain stages pass in the final QC pass with no restart required per the recorded evidence.

## Overall Verdict

**PASS.** 0 blocking findings. Two non-blocking findings are reported (see code-review artifact): (1) a stale line-count claim in `evidence/baseline/p2-t2-predictor-file-line-budget.md` not re-verified after the CSharpier reformat pass, and (2) the repo-scoped (whole-instrumented-process) coverage figure sitting below the 80%/85% floors, a pre-existing, non-regressing, non-PR-attributable condition.
