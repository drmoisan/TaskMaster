# Policy Audit — Issue #743: QuickFiler `ItemViewer` UI-marshalling seam

- **Feature folder:** `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743`
- **Branch:** `bug/quickfiler-itemviewer-ui-marshalling-seam-743`, head `06773349ad8861d18bd0dd1265aa8732cc19037a`
- **Base:** `origin/main` at `39ce2892b90ce9e8d7a4311c12195f1a06392f5b`; the branch contains a merge of that tip at `c358b2d809ca58db0197eb10229f872f2e9a924e` (sibling item #583, no Write Set file touched). Base SHAs are as supplied by the orchestrator; they were not recomputed in this review (see Reviewer constraints).
- **Work mode:** `full-bug` (`issue.md` line 12). AC source: `spec.md` `## Acceptance Criteria` (lines 409-488), five criteria.
- **Audit timestamp:** 2026-09-13T15-30 (the orchestrator receipt records the review dispatch at 2026-09-13T15-14; no clock is available to this review without a shell, so the timestamp is taken from that receipt plus elapsed reading time).
- **Reviewer constraints (recorded assumptions):** the launching agent's directive forbids the Bash tool for this run. Every observation below was made with Read, Grep and Glob against the item worktree. Consequences: (1) no `git` command was run, so the branch diff was taken from the orchestrator's verbatim name-status list and cross-checked against the executor's anchored diff audit (`evidence/qa-gates/final-write-set-audit.2026-09-12T19-30.md`) and the current file contents; (2) `validate_evidence_locations.py` and the coverage-hook simulation were not run; (3) no test, build or coverage command was executed — every toolchain and coverage figure is a transcription from the committed projection artifacts, read directly. Where a claim could not be checked with the read-only tools, the section says so and names the check the orchestrator should run.
- **Policy documents applied (read from the item worktree):** `CLAUDE.md`; `.claude/rules/general-code-change.md`; `.claude/rules/general-unit-test.md`; `.claude/rules/quality-tiers.md`; `.claude/rules/csharp.md`; `.claude/rules/tonality.md`; skills `policy-compliance-order`, `acceptance-criteria-tracking`, `evidence-and-timestamp-conventions`, `policy-audit-template-usage`, `feature-promotion-lifecycle` (user-story rule, line 111).
- **Template provenance deviation:** the MCP template asset (`mcp__drm-copilot__resolve_policy_audit_template_asset`) is not in this agent's tool surface for this run. The document is hand-authored preserving the twelve canonical major headings the `policy-audit-template-usage` skill enumerates, plus the `## Rejected Scope Narrowing` and `## Evidence Location Compliance` sections the feature-review contract requires. The document is not marked BLOCKED because every section is populated from evidence.

## Executive Summary

- **Verdict: REMEDIATION REQUIRED.** Two findings block acceptance; neither is a production-code defect.
  1. **Host-path leak (Blocking).** `evidence/other/orchestrator-citation-verification.2026-09-12T13-50.md` line 6 contains an absolute user-profile path with the operator's account name. The file is added by this branch (the whole feature folder is new versus `origin/main`). Rule: `.claude/agent-memory/_shared_no_absolute_host_paths.md` (applies to every committed artifact) and the plan's own gate-literal rule 4. Remedy: substitute `<repo-root>/.claude/worktrees/agent-a190dd2fffe21a25d` and squash-merge so the pre-sanitisation blob is not reachable from `main`.
  2. **AC1 evaluated PARTIAL (Blocking for acceptance, documentation-level remedy).** The AC1 verdict artifact reports `H-LEAK REJECTED by direct observation`, but the instrumented runs produced zero expiries, and H-LEAK as defined in spec section 4.2 is a cascade that follows an initial expiry. In a run with no expiry the counter observable cannot take any value other than the one recorded, so it does not discriminate. Spec AC1's final sentence states that a no-expiry run "is a recorded negative result, not a pass", and the spec risk table says "Escalate rather than infer". Details in `feature-audit.2026-09-13T15-30.md`, AC1.
- **Production change quality: PASS.** The seven Write Set files are exactly the seven files changed under `QuickFiler/` and `QuickFiler.Test/`. The interface widening is additive (`UiDispatcher` and `UiSyncContext` retained; contract tests pass), the parameter widening is source-compatible with both existing callers, and the single converted marshal forwards to the same `Dispatcher.InvokeAsync(Action)` primitive on the non-null path (`UtilitiesCS/Threading/WpfUiDispatcher.cs` lines 24-25 and 43, read directly).
- **Toolchain: PASS** on one clean pass (format rewrote nothing; check exit 0; analyzer Rebuild `0 Warning(s)` / `0 Error(s)`; nullable Rebuild `0 Warning(s)` / `0 Error(s)`; serial `QuickFiler.Test` run 1400/1400, baseline 1394/1394). All transcribed from `evidence/qa-gates/final-*.2026-09-12T19-30.md`.
- **C# coverage verdict: FAIL** (canonical `artifacts/csharp/coverage.xml` absent under the item's projections-only convention; the transcribed root line-rate of the single-assembly run is 24.15%, below the 85% floor; branch rate not transcribed). Disposition: non-blocking and procedural. The change-scope gates hold from the projections: modified file `QfcItemController.ViewerSetup.cs` 90.61% line (baseline 90.48%, no regression, +3 measurable lines all hit); `QfcItemController.Initialization.cs` 95.04% unchanged; the two production members added to `ItemViewer.cs` are unmeasurable because of the pre-existing type-level `[ExcludeFromCodeCoverage]` at line 20, and the two interface members have no bodies.
- **Non-blocking findings** are enumerated in section 8 and in `code-review.2026-09-13T15-30.md`.

## Rejected Scope Narrowing

The audit scope is the full branch diff against the resolved base. The launching prompt contains one instruction that narrows the file set, recorded verbatim:

> `M .claude/agent-memory/** (6 modified index/memory files, 5 added memory files — tracked agent memory written by earlier sessions; out of review scope except for host-path hygiene)`

Justification for rejecting it: the eleven agent-memory files are part of the branch diff and merge to `main` with the rest of the branch, so they are inside the audit scope. What this review could do about them without a shell: Grep over the item worktree's `.claude/agent-memory` tree for the user-profile prefix (drive letter plus the `Users` segment, both separator forms) and the account name returns 15 files (one occurrence each), listed in section 8. Whether any of those occurrences is introduced by this branch cannot be determined without the diff; the orchestrator should run the branch-scoped sweep named in section 8 before merge. No other narrowing instruction was found. The `DIRECTIVE: PREFLIGHT VALIDATION ONLY` line at the foot of the plan is planner-to-executor handoff text, not a scope instruction to this review.

One further directive is recorded as a deviation from this agent's output contract rather than as narrowing: the launching prompt states "Write no other file", so `remediation-inputs.<timestamp>.md` was not written. The remediation-required findings are enumerated in the feature audit under `## Remediation-Required Findings` so that nothing is lost; the orchestrator should either author the remediation-inputs artifact from that section or re-dispatch this review with permission to write it.

## Evidence Location Compliance

- Canonical scheme: `<FEATURE>/evidence/<kind>/`. Glob over the feature folder shows every evidence artifact under `evidence/baseline/`, `evidence/regression-testing/`, `evidence/qa-gates/`, `evidence/issue-updates/` or `evidence/other/` (54 files enumerated). PASS.
- Forbidden paths: Glob `artifacts/**/*` over the item worktree returns only `artifacts/orchestration/orchestrator-state.json` (gitignored orchestration state) and six pre-existing `artifacts/pr_body_*` files that are not in this branch's diff. No file exists under `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/` or `artifacts/post-change/`. PASS.
- `validate_evidence_locations.py --root .` was not run (no shell); the Glob enumeration above is the substitute check. The orchestrator can run the script to confirm; a non-zero exit would add findings here.
- Projections-only convention (spec section 8, plan D1): `evidence/qa-gates/final-projections-only-audit.2026-09-12T19-30.md` records 570 tracked `*.trx`/`*.cobertura.xml` paths both at the self-anchor and at the final tree, and zero untracked or modified raw artifacts. The 570 pre-existing raw files are outside this item's scope. PASS for this item's additions.
- Issue-update mirror: `evidence/issue-updates/issue-511-and-571-reconciliation.2026-09-12T19-00.md` carries `Timestamp:`, the exact posted text, `PostedAs: comment` and both comment URLs. The filename does not follow the skill's `issue-<N>.<timestamp>.md` pattern (one combined file for two issues). Observation, non-blocking.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none; no caller instruction supplied a non-canonical evidence path.

## 1. General Unit Test Policy Compliance

### 1.1 Test principles (independence, isolation, determinism, readability, AAA, external dependencies)

| Check | Verdict | Evidence |
|---|---|---|
| New tests are MSTest + Moq + FluentAssertions only | PASS | `QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs` lines 1-14 (usings), `[TestClass]`/`[TestMethod]` throughout; `QfcItemController.UiThreadDispatcherFixtureTests.cs` lines 363-394 |
| No `Thread.Sleep`, `Task.Delay`, `Stopwatch`, wall-clock read, retry or polling loop in new or modified test code | PASS | Read of the three test files; `evidence/regression-testing/ac2-determinism-audit.2026-09-12T18-00.md` records an empty match list for the seam file; the fixture edits add only `Interlocked.Increment`/`Volatile.Read` |
| Only time-valued construct is `[Timeout(...)]` as a deadlock bound | PASS | `SeamTimeoutMs = 60000` (seam file line 44) on all five tests; `GateTimeoutMs` on the new balance test (line 364) |
| No temporary files | PASS | No file I/O in any changed test file |
| Independence / order-independence | PASS with one observation | The seam tests install and restore the ambient `SynchronizationContext` in `try`/`finally` (lines 150-171, 196-219, 294-308). The balance test's assertion is order-independent by construction (acquisitions minus releases while holding the sole permit). Observation: the three fixture counters are process-wide statics that are never reset; that is intentional (monotonic) and the assertion tolerates it |
| Isolation (one unit per test) | PASS | Each seam test targets one member; the balance test targets the gate |
| Arrange-Act-Assert with intent comments | PASS | Every new test carries `// Arrange`, `// Act`, `// Assert` markers and an XML summary |
| Tests live under the test project mirroring production structure | PASS | `QuickFiler.Test/Controllers/` mirrors `QuickFiler/Controllers/` |
| Existing tests treated as part of the spec | PASS | No existing test deleted or weakened; the retained pump-hosted test `ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups` is unchanged (`QfcItemController.ViewerSetupTests.cs` is not in the diff; `final-file-sizes` records it at 498 lines, identical to baseline) |
| Scenario completeness for the changed members | PASS with observation | Positive (tests 1, 2, 5), negative/cancellation (test 4), structural contract (test 3). The null-tolerance branch of `AssignControlsAsync` is exercised by the pre-existing `AssignControlsAsync_DispatchesAssignThroughViewerDispatcher` (ViewerSetupTests lines 309-344), whose name now describes a path production no longer takes (see code review N-2) |
| Test-code determinism infrastructure (no banned APIs) | PASS | See rows above |

### 1.2 Coverage

Languages with changed files in the branch diff: **C# only** (`*.cs`, `*.csproj`). Zero TypeScript, Python or PowerShell files are changed (the seven Write Set paths plus Markdown under `docs/` and `.claude/agent-memory/`).

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/phase0-coverage-prechange.2026-09-12T16-30.md` (projection of `coverage/743-prechange.cobertura.xml`, raw file discarded per plan D1)
- C# post-change coverage artifact: `evidence/qa-gates/ac4-coverage-comparison.2026-09-12T19-30.md` (projection of `coverage/743-postchange.cobertura.xml`, raw file discarded per plan D1; canonical `artifacts/csharp/coverage.xml` absent, recorded FAIL in section 5)
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 7 (3 production, 3 test, 1 test csproj) | 1400 in QuickFiler.Test (6 added) | 1400 passed, 0 failed, 0 timeout (serial) | 24.17% lines (single-assembly root; ViewerSetup.cs 90.48%, Initialization.cs 95.04%) | 24.15% lines (single-assembly root; ViewerSetup.cs 90.61%, Initialization.cs 95.04%) | 100.00% of measurable new lines (3 of 3 in ViewerSetup.cs hit; ItemViewer.cs members unmeasurable by pre-existing type-level exclusion; IItemViewer.cs members have no bodies) |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |
| PowerShell | 0 | 0 | N/A | N/A | N/A | N/A |

### 1.2.1 Per-language coverage comparison

- C#: Baseline coverage: 24.17% line (root line-rate 0.241706, lines-valid 61852, single-assembly QuickFiler.Test run; ViewerSetup.cs 190/210 = 90.48%, Initialization.cs 249/262 = 95.04%); Post-change coverage: 24.15% line (root line-rate 0.241516, lines-valid 61855; ViewerSetup.cs 193/213 = 90.61%, Initialization.cs 249/262 = 95.04%); Disposition: FAIL (non-blocking, procedural: canonical artifact absent by the projections-only convention, single-assembly root figure is below the 85% floor and is not a repository-wide measurement, branch rate not transcribed; changed-file gate PASS at 90.61% with no regression, new-line gate PASS at 3 of 3 measurable lines hit).
- TypeScript: Baseline: 0 changed files; Post-change: 0 changed files; Disposition: PASS (no TypeScript file on the branch).
- Python: Baseline: 0 changed files; Post-change: 0 changed files; Disposition: PASS (no Python file on the branch).
- PowerShell: Baseline: 0 changed files; Post-change: 0 changed files; Disposition: PASS (no PowerShell file on the branch).

### 1.2.2 Coverage Artifact State

| Language | Canonical artifact | State |
|---|---|---|
| C# | `artifacts/csharp/coverage.xml` | Absent in the item worktree (Glob `artifacts/csharp/*` returns nothing). The item follows the maintainer decision on issue #671 (spec section 8, plan D1): raw Cobertura output written under the gitignored `coverage/` directory, figures transcribed, raw output discarded at P6-T18. |
| TypeScript | `coverage/lcov.info` | Not required (no changed files) |
| Python | `artifacts/python/lcov.info` | Not required (no changed files) |
| PowerShell | `artifacts/pester/powershell-coverage.xml` | Not required (no changed files) |

C# coverage verdict: FAIL (canonical artifact absent under the projections-only convention; single-assembly root line-rate 24.15% is below the 85% floor; branch rate not transcribed). Disposition: non-blocking, procedural; changed-file gate PASS (90.61% line, no regression) and new-line gate PASS (3 of 3 measurable new lines hit).

Observations on the C# figures (details in section 5):

1. The root figure is a single-assembly run (`-SearchRoot QuickFiler.Test`), so its denominator includes every loaded first-party module while only QuickFiler tests executed; it is not comparable with the repository-wide figure the CI gate computes and it is expected to sit far below the floor. The last repository-wide figures known to this reviewer from prior reviews were approximately 84.8-85.3% line and approximately 79% branch; they were not re-measured on this branch.
2. Root `lines-covered` fell by 11 (14950 to 14939) between the two runs while ViewerSetup.cs gained 3 covered lines. The post-change run recorded three failed tests in `QfcInitEmailQueueZeroBatchTests` (Deedle `TypeInitializationException`, environmental, see section 8), which is the plausible cause of the movement in unrelated classes. It is not a changed-line regression: the AC4 artifact accounts every added ViewerSetup.cs line as hit.
3. Branch coverage is not transcribed in either projection. The only new branch in production code is `if (dispatcher is null)` in `AssignControlsAsync`; both arms are exercised (null arm by `AssignControlsAsync_DispatchesAssignThroughViewerDispatcher`, seam arm by `AssignControlsAsync_WithSyncDispatcherDouble_AssignsThroughTheInjectedSeam`), which is evidence of exercise, not a measured branch rate.

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Plan read and followed; Decisions Record present | PASS | `plan.2026-09-12T13-23.md` D1-D11; 64/64 tasks checked; status `Executed` |
| Bugfix workflow: failing regression test first | PASS with recorded deviation | Fail-before 3 of 3 on the defect-preserving intermediate (`ac2-fail-before-three-runs`), pass-after 3 of 3 (`ac2-pass-after-three-runs`). Deviation: the seam test's arrangement order was corrected between the two run sets; see feature audit AC2 for why the fail-before outcome is unaffected |
| Minimal, targeted fix; no opportunistic refactor | PASS | Diff confined to two additive interface members, two one-line implementations, one parameter widening, two identifier substitutions, one marshal conversion with null tolerance, one comment update, fixture counters, one balance test, one csproj entry |
| Design principles (simplicity, reusability, extensibility, separation of concerns) | PASS | The seam reuses the existing injected `IUiDispatcher`; no new abstraction; the interface widening is additive |
| Error handling: fail fast | PASS | `Token.ThrowIfCancellationRequested()` retained as the first statement; no broad catch introduced |
| Logging pattern | PASS (no logging added or required) | |
| File size <= 500 lines for every changed code file | PASS | `final-file-sizes`: IItemViewer.cs 212, ItemViewer.cs 406, ViewerSetup.cs 478, UiThreadDispatcherFixture.cs 304, UiThreadDispatcherFixtureTests.cs 396, SeamMarshallingTests.cs 312. `QuickFiler.Test.csproj` is 533 lines (532 at baseline); it is a project file, not code or a reusable script, and is outside the rule's stated classes. Observation only |
| Naming, docs, comments (why not what) | PASS with one wording finding | Interface members carry XML docs; the `AssignControlsAsync` comment's second sentence ("so this is no no-op") is unclear (code review N-1) |
| Public API compatibility | PASS | Additive interface members only; `internal` member's parameter widened from concrete to interface (source-compatible for both callers, verified by the P3-T5 analyzer Rebuild of the whole solution recorded in `p3-postfix-build`) |
| Dependencies | PASS | No package added; `QuickFiler.Test.csproj` diff is one `<Compile Include>` line |
| I/O boundaries | PASS | No I/O introduced |
| Supporting documents updated | PASS | Plan status and counts updated; `#230` de-exemption comment updated to name both covering tests (ViewerSetup.cs lines 276-279) |
| Toolchain loop reported with commands and final clean pass | PASS | `final-format`, `final-csharpier-check`, `final-analyzer-rebuild`, `final-nullable-rebuild`, `final-serial-test-run` (one pass, all clean) |
| Write Set boundary honoured | PASS | `final-write-set-audit`: anchored name-only diff lists exactly the seven paths; porcelain empty |

## 3. Language-Specific Code Change Policy Compliance

C# (`.claude/rules/csharp.md`, CLAUDE.md C# Code Change Policy):

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier via `dotnet tool run`, pinned 1.2.6 | PASS | `phase0-toolchain-bootstrap` block 2 restored csharpier 1.2.6; `final-format` and `final-csharpier-check` used `dotnet tool run csharpier format .` / `check .`, exit 0 |
| Analyzer Rebuild with `/t:Rebuild`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild` | PASS | `final-analyzer-rebuild`: exact approved command, `0 Warning(s)` / `0 Error(s)`, identical to the P0-T6 baseline |
| Nullable Rebuild with `/t:Rebuild`, `TreatWarningsAsErrors`, no `/p:Nullable=enable` | PASS | `final-nullable-rebuild`: exact approved command, `0 Warning(s)` / `0 Error(s)`, identical to the P0-T7 baseline |
| Strong contracts, explicit types at public boundaries | PASS | `IEnumerable<Control> DescendantControls()`, `Label ItemNumberLabel { get; }` |
| Null safety (guard clauses) | PASS | `if (dispatcher is null)` guard mirrors the existing `NotifyMoveFailure` shape (`QfcItemController.MailActions.cs` lines 35-46, read directly) |
| Composition, minimal public surface | PASS with observation | Two public members added to the concrete `ItemViewer`; `ItemNumberLabel` aliases the existing public `LblItemNumber` (spec 6.3 mandates the interface accessor; the alias is the cost of the additive rule) |
| XML docs on non-obvious public members | PASS | Interface members documented; concrete implementations carry a `//` comment (observation N-9) |
| No suppression added | PASS | The existing `#pragma warning disable CS0618` region is untouched |
| Analyzer stack intact | PASS | No `.csproj` analyzer item changed; the pre-existing Meziantou HintPath skew is a follow-up (section 8) |

## 4. Language-Specific Unit Test Policy Compliance

C# (`.claude/rules/csharp.md` Testing Standards, CLAUDE.md C# Unit Test Policy):

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]`, `[TestMethod]`, `[Timeout]`, `TestContext` |
| Moq for mocks | PASS | `Mock<IItemViewer>`, `Mock<IUiDispatcher>`, `Mock<IApplicationGlobals>`, `Mock<IAppQuickFilerSettings>` |
| FluentAssertions preferred | PASS | All assertions use `.Should()`; Moq `Verify`/`VerifySet` for interaction checks |
| Test command `vstest.console.exe <assembly> /EnableCodeCoverage` | PASS with note | The serial gate ran `vstest.console.exe` with `/InIsolation` and the LiveOutlook filter (CI-equivalent regime); coverage was collected by the repository runner `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (parallel regime), which is the repo-standard wrapper |
| New module/class/method coverage >= 90% (CLAUDE.md) / >= 85% (rules) | PASS for measurable code | ViewerSetup.cs new lines 3 of 3 hit; `ItemViewer.cs` additions unmeasurable (pre-existing type-level exclusion, ratified under CLAUDE.md UT2(b); not a Blocking finding per the standing ruling that a source attribute is not a coverage-config `exclude` entry) |
| No coverage regression on changed lines | PASS | ViewerSetup.cs 90.48% to 90.61%, uncovered count unchanged at 20 |
| Deterministic test rules (no PATH, profile, network, external process) | PASS | The seam tests construct only WinForms `Panel`/`Label`/`TableLayoutPanel`/`Button` instances without handles and a plain `SynchronizationContext` |

## 5. Test Coverage Detail

Source projections: `evidence/baseline/phase0-coverage-prechange.2026-09-12T16-30.md` (P0-T9, 2026-09-13T02-26) and `evidence/qa-gates/ac4-coverage-comparison.2026-09-12T19-30.md` (P6-T6, 2026-09-13T03-49), same session, same command `pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput <path>`, same per-file extraction (group on line number, maximum hit, `lines/line` axis, no descendant double-count).

| File | Baseline (valid/covered, rate) | Post-change (valid/covered, rate) | Delta and accounting |
|---|---|---|---|
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (modified) | 210 / 190, 0.904762 | 213 / 193, 0.906103 | +3 valid, +3 covered; the `AssignControlsAsync` marshal region grew from four measured lines to seven, all hit; uncovered count unchanged at 20 |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` (measured, not edited) | 262 / 249, 0.950382 | 262 / 249, 0.950382 | 0 |
| `QuickFiler/Viewers/ItemViewer.cs` (modified) | no Cobertura element | no Cobertura element | Type-level `[ExcludeFromCodeCoverage]` at line 20, pre-existing (spec C4, plan D9). The two added one-line members are executed by the retained pump-hosted test but produce no coverage observation |
| `QuickFiler/Viewers/IItemViewer.cs` (modified) | no executable lines | no executable lines | Interface members without bodies |
| Test files (3 modified/added) | excluded from measurement by policy | excluded | `.Test.dll` assemblies are stripped by the runner's post-processing |

Repository-wide: root `line-rate` 0.241706 (baseline) and 0.241516 (post-change), `lines-valid` 61852 and 61855, from a single-assembly run; below the 85% floor; not a repository-wide measurement; branch-rate attribute not transcribed. The repository's ratified COM/VSTO/WinForms exemption (CLAUDE.md UT2) applies to the denominator's largest components. Recorded FAIL, non-blocking, procedural (section 1.2).

Threshold note: CLAUDE.md states 80% repository-wide and 90% new code; `.claude/rules/general-unit-test.md` and `quality-tiers.md` state a uniform 85% line / 75% branch. The divergence is pre-existing and unreconciled; this audit reports against the stricter figure where the two differ, and the change-scope outcome is the same under either.

## 6. Test Execution Metrics

| Run | Regime | Total | Passed | Failed / Timeout | Source |
|---|---|---|---|---|---|
| P0-T10 baseline, whole assembly | SERIAL (no `/Settings:`) | 1394 | 1394 | 0 / 0 | `evidence/baseline/phase0-serial-test-baseline.2026-09-12T16-30.md` |
| P1-T9 AC1 serial (R4 excluded) | SERIAL | 1394 | 1394 | 0 / 0 | `evidence/baseline/ac1-serial-measurement.2026-09-12T17-00.md` |
| P1-T10 AC1 parallel | PARALLEL (`/Settings:TaskMaster.runsettings`) | 1395 | 1395 | 0 / 0 | `evidence/baseline/ac1-parallel-measurement.2026-09-12T17-00.md` |
| P2-T9 fail-before (3 runs) | SERIAL, seam class filter | 5 | 2 | 3 / 0 | `evidence/regression-testing/ac2-fail-before-three-runs.2026-09-12T17-30.md` |
| P3-T6 pass-after (3 runs) | SERIAL, seam class filter | 5 | 5 | 0 / 0 | `evidence/regression-testing/ac2-pass-after-three-runs.2026-09-12T18-00.md` |
| P4-T3 serial confirmation | SERIAL (R4 excluded) | 1399 | 1399 | 0 / 0 | `evidence/regression-testing/p4-branch-confirmation.2026-09-12T18-30.md` |
| P4-T3 parallel, first execution | PARALLEL | 1400 | 1397 | 3 / 0 | same (Deedle `TypeInitializationException` in `QfcInitEmailQueueZeroBatchTests`) |
| P4-T3 parallel, re-run | PARALLEL | 1400 | 1400 | 0 / 0 | same |
| P5-T1 AC3B streak | SERIAL, seam class filter, 62 runs | 62 runs | 62 | 0 / 0 | `evidence/regression-testing/ac3b-consecutive-runs.2026-09-12T19-00.md` |
| P6-T5 final gate | SERIAL | 1400 | 1400 | 0 / 0 | `evidence/qa-gates/final-serial-test-run.2026-09-12T19-30.md` |
| P6-T6 post-change coverage run | PARALLEL (runner) | 1400 | 1397 | 3 / 0 | `evidence/qa-gates/ac4-coverage-comparison.2026-09-12T19-30.md` (same Deedle failure; environmental) |

Six tests were added: five in `QfcItemController_SeamMarshallingTests`, one in `QfcItemController_UiThreadDispatcherFixtureTests` (1394 + 6 = 1400).

## 7. Code Quality Checks

| Check | Verdict | Evidence |
|---|---|---|
| Formatting (CSharpier check, repository-wide) | PASS | `final-csharpier-check`: `Checked 1625 files`, exit 0; `final-format`: porcelain empty after `format .` |
| Analyzers (Rebuild) | PASS | `0 Warning(s)` / `0 Error(s)` |
| Nullable / compiler (Rebuild, warnings as errors) | PASS | `0 Warning(s)` / `0 Error(s)` |
| Formatting drift baseline | PASS | `phase0-csharpier-check`: `PRE-EXISTING DRIFT FILES: none` |
| Absolute host paths in changed source files | PASS | Grep over the seven Write Set files for the user-profile prefix (both separator forms) and the account name: 0 hits |
| Absolute host paths in the feature folder | FAIL (Blocking) | 1 hit: `evidence/other/orchestrator-citation-verification.2026-09-12T13-50.md:6` (user-profile path with the account name, inside backticks). `evidence/baseline/phase0-toolchain-bootstrap.2026-09-12T16-30.md:7` carries a `C:\Program Files\...\MSBuild.exe` path that names no account or host; observation only |
| Absolute host paths in `.claude/agent-memory` on the branch | Not determinable without a diff | 15 files in the current tree contain a match; attribution to this branch requires `git diff origin/main...HEAD -- .claude/agent-memory` (section 8) |
| Tonality of committed artifacts | PASS | Spot-read of the plan, spec, user story and eleven evidence artifacts: factual, no humour or hyperbole |

## 8. Gaps and Exceptions

1. **Blocking — host-path leak.** `evidence/other/orchestrator-citation-verification.2026-09-12T13-50.md` line 6 carries a backticked absolute path of the form `<user-profile>/repos/TaskMaster/.claude/worktrees/agent-a190dd2fffe21a25d`, with the real user-profile prefix (drive letter, `Users` segment and account name) written out in the file; the raw value is deliberately not reproduced here. Rule: `.claude/agent-memory/_shared_no_absolute_host_paths.md` ("No file committed to this repository may contain an absolute host path or a host identifier"). Authored in the preparation session, added to `main` by this branch. Remedy: replace with `<repo-root>/.claude/worktrees/agent-a190dd2fffe21a25d`; squash-merge the branch so the original blob is unreachable from `main`. The executor disclosed this in its run-B receipt and declined repair on Write Set grounds; that scope lock binds the executor, not the branch.
2. **Blocking for acceptance — AC1 PARTIAL.** See the feature audit. Remedy options, in order of cost: (a) amend section (iii) of `evidence/baseline/ac1-mechanism-verdict.2026-09-12T17-00.md` to state that H-COST is the only available originating mechanism (H-LEAK is by definition a cascade that requires a prior expiry, and the spec's U2 remains open and untested), supported by the measured elongation figures, and obtain the maintainer's explicit ratification that the recorded negative result is accepted for AC1, transcribed into `issue.md`; or (b) design and run an instrumented reproduction that produces an expiry (the spec's risk table anticipates this may be infeasible and says to escalate).
3. **Non-blocking — C# coverage procedural FAIL** (section 1.2): canonical artifact absent by convention; single-assembly root figure below floor; branch rate not transcribed. No code change required. If the orchestrator needs the canonical artifact for the hook, the maintainer's projections-only decision (#671) conflicts with the artifact-path rule and should be reconciled at policy level rather than by committing raw XML.
4. **Non-blocking — AC2 arrangement-order deviation** between the fail-before and pass-after run sets (feature audit AC2). A literal re-run of the final seam-test text against the P2 intermediate (commit `bce810495` production files) would remove the residual reliance on reasoning; optional.
5. **Non-blocking — evidence timestamp convention.** Executor artifacts carry `Timestamp:` values 02-11 through 03-53 on 2026-09-13 while the orchestrator receipts for the same runs record 13-12 through 15-10; the offset is a consistent 12 hours. `phase0-diff-base` records a re-anchor addendum at `2026-09-13T13-05` that precedes P0-T4 (`02-11`) in the executor's convention. The artifacts remain internally ordered; the two clocks should not be mixed in one folder. Observation.
6. **Non-blocking — `user-story.md` present in a `full-bug` folder.** `feature-promotion-lifecycle` line 111: "For `full-bug`, `spec.md` is expected alongside `issue.md`; `user-story.md` should be absent unless the requirements explicitly justify it." The file self-justifies with "the feature-document contract requires both artifacts to exist", which is a contract citation rather than a requirements justification. It is explicitly marked non-authoritative and carries no checkboxes, so no AC-source ambiguity results. Severity: observation; the orchestrator may delete it at merge or leave it.
7. **Non-blocking — `.claude/agent-memory` host-path attribution.** Current-tree matches (one occurrence each): `_shared_no_absolute_host_paths.md`; `epic-planner/reference_isolated_worktrees_cut_from_main_not_session_head.md`; `feature-review/project_464-review-residuals.md`; `feature-review/project_565-review-residuals.md`; `feature-review/project_730-review-residuals.md`; `atomic-executor/project_bash_heredoc_collapses_doubled_backslashes.md`; `atomic-executor/project_koverage_reporoot_needs_native_separators.md`; `atomic-executor/project_selftest_probe_literal_trips_the_next_sweep_pass.md`; `atomic-planner/worktree-root-breaks-dotclaude-exclusion.md`; `orchestrator/angle-bracket-redaction-breaks-trx-xml.md`; `orchestrator/bash-tool-collapses-double-backslash-in-sed.md`; `orchestrator/collect-pr-context-lands-in-main-checkout.md`; `orchestrator/preparation-child-cwd-is-session-root-not-item-worktree.md`; `epic-orchestrator/feedback_measure_whole_volume_before_blaming_worktrees.md`; `epic-orchestrator/feedback_region_ownership_is_a_prefix_claim.md`. Several of these are the hygiene rule's own examples. Required check before merge: `git diff origin/main...HEAD -- .claude/agent-memory | grep -i -E "^\+.*(<account>|C:[\\/]Users)"`; any hit is a Blocking finding of the same class as item 1.
8. **Out-of-scope defects reported by the executors (follow-ups, not findings against this item):** (a) Meziantou.Analyzer HintPath skew — `UtilitiesCS/UtilitiesCS.csproj` line 1308 and `VBFunctions/VBFunctions.csproj` line 58 name `Meziantou.Analyzer.3.0.203` while lines 3/1300 and 3/73 of the same files and both `packages.config` files name `3.0.235` (confirmed by Grep in this review); a cold restore fails with CS0006 until `3.0.203` is installed manually; pre-dates the branch; promote to an issue. (b) `QfcInitEmailQueueZeroBatchTests` fails intermittently under class-level parallelism with `TypeInitializationException` (`Deedle.Reflection`, `netstandard 2.1.0.0` binding) and passes serially and on re-run (two occurrences recorded: P4-T3 run B, P6-T6); promote to an issue.
9. **Reviewer tooling gaps (recorded, not findings):** no `git` recomputation of the merge-base; no coverage-hook simulation; `validate_evidence_locations.py` not run; the live GitHub comments for AC5 not fetched. The orchestrator can close each with one command.

## 9. Summary of Changes

| Path | Change | Purpose |
|---|---|---|
| `QuickFiler/Viewers/IItemViewer.cs` | +12 lines: `IEnumerable<Control> DescendantControls()` and `Label ItemNumberLabel { get; }` with XML docs | Additive intent members so `ResolveControlGroupsAsync` can be driven through the interface (spec 6.3) |
| `QuickFiler/Viewers/ItemViewer.cs` | +6 lines: one-line implementations of both members | Same |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 467 to 478 lines: parameter widened to `IItemViewer`; `LblItemNumber` to `ItemNumberLabel`; `GetAllChildren()` to `DescendantControls()`; `AssignControlsAsync` marshal routed through the injected `_uiDispatcher` with a null-tolerance branch; `#230` comment extended to name both covering tests | Part B (spec 6.3) and the single Part A site (plan D4) |
| `QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs` | new, 312 lines, five tests | AC2/AC3 regression and structural tests, no pump host |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | 278 to 304 lines: three monotonic counters and accessors | AC1 observable instrumentation, retained under Branch COST |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | 353 to 396 lines: `TestContext` property and the balance test | AC1 balance assertion and `GATECOUNTERS` output |
| `QuickFiler.Test/QuickFiler.Test.csproj` | +1 `<Compile Include>` | Legacy project requires the explicit entry |
| `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/**` | new | issue, spec, user story, research, plan, 54 evidence projections |
| `.claude/agent-memory/**` | 6 modified, 5 added | Agent memory written during the run; content not audited beyond host-path hygiene (see section 8 item 7) |

## 10. Compliance Verdict

**REMEDIATION REQUIRED — 2 Blocking findings (one hygiene, one acceptance-evidence); 0 production-code defects.**

- General Unit Test Policy: PASS (coverage row FAIL, non-blocking, procedural).
- General Code Change Policy: PASS.
- C# Code Change Policy: PASS.
- C# Unit Test Policy: PASS.
- Toolchain: PASS (one clean pass).
- Evidence location: PASS.
- Evidence hygiene: FAIL (Blocking, one occurrence).
- Acceptance criteria: 4 PASS, 1 PARTIAL (AC1) — see `feature-audit.2026-09-13T15-30.md`.

Merge readiness after the two remedies: the code changes need no further work; the AC1 remedy is a documentation amendment plus a maintainer ratification, and the hygiene remedy is a one-token substitution plus a squash-merge.

## Appendix A: Test Inventory

New tests (6):

| Class | Test | Purpose | Time-valued construct |
|---|---|---|---|
| `QfcItemController_SeamMarshallingTests` | `ResolveControlGroupsAsync_WithMockViewerAndSyncDispatcher_CompletesWithoutAConcreteViewer` | AC3A structural: completes with a viewer mock and the synchronous dispatcher double; item-number tip built | `[Timeout(60000)]` |
| same | `ResolveControlGroupsAsync_WithMockViewer_PopulatesTipsAndControlGroups` | AC2 named regression test: tip collections sized to the label lists; control groups classified; `DescendantControls()` called once | `[Timeout(60000)]` |
| same | `ResolveControlGroupsAsync_FirstParameterType_IsTheViewerInterface` | Reflection contract on the parameter type | `[Timeout(60000)]` |
| same | `ResolveControlGroupsAsync_WithCancelledToken_ThrowsOperationCanceled` | Negative flow | `[Timeout(60000)]` |
| same | `AssignControlsAsync_WithSyncDispatcherDouble_AssignsThroughTheInjectedSeam` | Seam path: exactly one `InvokeAsync(Action)` on the injected double; `ItemNumberText = "2"` once | `[Timeout(60000)]` |
| `QfcItemController_UiThreadDispatcherFixtureTests` | `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition` | AC1 balance: acquisitions minus releases equals 1 while holding the permit; emits `GATECOUNTERS` | `[Timeout(60000)]` |

Retained and unchanged tests relied on as evidence: `ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups` (ViewerSetupTests, pump-hosted, named in the `#230` comment); `AssignControlsAsync_DispatchesAssignThroughViewerDispatcher` (ViewerSetupTests lines 309-344, now exercises the null-tolerance branch); `IItemViewer_StillDeclaresUiDispatcher` and `IItemViewer_StillDeclaresUiSyncContext` (contract guards for the additive rule); the six pre-existing gate tests in the fixture test class; the five `ThroughThePumpHost` initialization tests; the eight breadcrumb-host tests. All passed in the P6-T5 serial run (per-test rows in `evidence/qa-gates/final-serial-test-run.2026-09-12T19-30.md`).

## Appendix B: Toolchain Commands Reference

Commands as recorded in the evidence artifacts (tool paths resolved through `vswhere` per the plan's Command Reference; run from the item worktree root):

1. `dotnet tool restore` (csharpier 1.2.6 restored)
2. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
4. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
5. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=<task>.trx" /ResultsDirectory:coverage\trx\<task> "/TestCaseFilter:TestCategory!=LiveOutlook"` (serial regime; `/Settings:TaskMaster.runsettings` added for the parallel regime)
6. `pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput coverage\743-<pre|post>change.cobertura.xml` followed by the plan's per-file Cobertura extraction
7. `gh issue comment <N> --repo drmoisan/TaskMaster --body-file coverage\issue-<N>-comment.md` (AC5)

Checks the orchestrator should run to close this review's tooling gaps: `git merge-base origin/main HEAD`; `git diff origin/main...HEAD --name-status`; `git diff origin/main...HEAD -- .claude/agent-memory | grep -i -E "^\+.*(<account>|C:[\\/]Users)"`; `python scripts/... validate_evidence_locations.py --root .`; the coverage-hook simulation described in the feature-review agent memory.
