# Policy Audit — winformspumphost-suite-determinism-511

- Timestamp: 2026-08-24T00-01 (UTC)
- Feature folder: `docs/features/active/winformspumphost-suite-determinism-511`
- Branch: `bug/winformspumphost-suite-determinism-511-exec`
- Base branch (resolved): `main`
- Merge base: `f85a36faebaaec29fe5233c9d9f69d223d80e4c5`
- Head: `b4d47adc369d021f6fb4eff092f419dc49e9a5e5` (verified against `git rev-parse HEAD`; tree clean)
- Work mode: `full-bug` (persisted marker in `issue.md`; AC source is `spec.md` only)
- Review cycle: re-audit after remediation cycle 1 (inputs: `remediation-inputs.2026-08-23T20-57.md`)

> Template provenance. The MCP tool `resolve_policy_audit_template_asset` is not reachable from
> this review subagent's environment; the artifact was authored directly against the canonical
> heading set required by `.claude/skills/policy-audit-template-usage/SKILL.md`, with the template
> instruction block omitted. All 13 canonical major headings are present.

## Executive Summary

The branch diff against `main` at merge base `f85a36fa` contains 3 C# test files (+124 lines, all
in `QuickFiler.Test/`), 68 documentation/evidence files under the feature folder, and 26
agent-memory bookkeeping files under `.claude/agent-memory/`. No production code, project file,
workflow, or script file changed. The change corrects two falsified comment blocks, adds two
regression tests pinning the measured WebView2 handle-inheritance state, and re-scopes the
feature's claims per the maintainer decision recorded in `decision-record.2026-08-23T20-40.md`.

All policy sections below are PASS. The remediation cycle 1 exit criteria (remediation-inputs
Part 5, criteria 1-7) were each independently re-verified against the working tree and are
satisfied. **Blocking findings in this audit: 0.**

## Rejected Scope Narrowing

None detected. The caller prompt supplied repository facts and explicitly declined to assert
scope. The audit scope is the full branch diff `f85a36fa..b4d47adc` against `main`. One
generator defect was found and corrected rather than accepted: `artifacts/pr_context.summary.txt`
classified the branch as "Core logic changes: 0 files" and omitted the three changed `.cs` files
from its changed-files bullets. The summary was corrected in place (reviewer correction block,
dated 2026-08-24 UTC) and the C# language gates below were applied in full.

## 1. General Unit Test Policy Compliance

| Check | Verdict | Evidence |
| --- | --- | --- |
| Independence / isolation | PASS | Both new tests build a private `WinFormsPumpHost` + `PumpHarness`, restore state in `finally` (`harness.Restore()`, `host.StopAsync()`); the process-wide `UiThreadDispatcherGate` serialization is preserved (`Part2.cs:51`, `SwapUiThreadDispatcher` now at `:148`). |
| Determinism | PASS | Both new tests recorded `Passed` in 10/10 full nine-assembly runs under induced CPU load (`evidence/regression-testing/regression-tests-ten-runs.2026-08-21T18-10.md`) and in the final 6,459/6,459 suite run (`evidence/qa-gates/remediation-suite-run.2026-08-23T20-57.md`). |
| Banned timing APIs | PASS | Added-line scan of the branch diff: zero `Thread.Sleep`, `Task.Delay`, `SpinWait`, retry loops, or raised timeout constants. Only `[Timeout(PumpTimeoutMs)]` attributes referencing the existing 60,000 ms constant, unchanged (`ViewerSetupTests.cs:34` et al.). |
| No temporary files in tests | PASS | Diff adds no filesystem access. |
| No external dependencies | PASS | New tests exercise in-process WinForms fixtures only; WebView2 core initialization remains behind the sentinel mock. |
| AAA structure and documented intent | PASS | Both new tests carry Arrange/Act/Assert comments, XML doc summaries, and FluentAssertions `because:` messages. |
| Test files excluded from coverage denominator | PASS | Cobertura packages enumerate production assemblies only; `QuickFiler.Test` is not a package in `artifacts/csharp/coverage.xml`. |

## 2. General Code Change Policy Compliance

| Check | Verdict | Evidence |
| --- | --- | --- |
| 500-line file cap | PASS | Independently re-measured (`awk`, full line count): `Part2.cs` 418, `ViewerSetupTests.cs` 474, `Part3.cs` 398 — all under 500. Matches `evidence/qa-gates/remediation-file-size-audit.2026-08-23T20-57.md`. |
| Toolchain loop, full pass | PASS | `evidence/qa-gates/remediation-clean-pass.2026-08-23T20-57.md`: P3-T1..P3-T10 green in a single consecutive pass, 0 loop restarts, `SKIPPED` unused. |
| Comments state the truth (why, synchronized) | PASS | Both formerly-false comment blocks (`Part2.cs:87-93`, `ViewerSetupTests.cs:436-442`) now state the measured inherited-handle truth and the deliberate redundancy of the retained read, discharging remediation Finding D verbatim, including the required redundancy statement. |
| Simplicity / minimal diff | PASS | Code diff is +124 lines across 3 test files; no production edits (`git diff -- QuickFiler/` is empty). |
| No policy-document edits | PASS | Diff touches no `.claude/rules/**` and no `.github/instructions/**`. `.claude/` paths in the diff are exclusively `.claude/agent-memory/**`, the subtree spec AC 9 and epic hard constraint 1 explicitly permit. |
| Fail fast / logging / contracts | PASS | Not exercised: no production logic changed. |

## 3. Language-Specific Code Change Policy Compliance

C# is the only code language with changed files on the branch.

| Check | Verdict | Evidence |
| --- | --- | --- |
| CSharpier formatting | PASS | `remediation-csharpier-format` (hash-derived rewritten count 0) and `remediation-csharpier-check` (0 unformatted of 1,519 files), both exit 0, pinned tool via `dotnet tool run`. |
| .NET analyzers (`/t:Rebuild`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`) | PASS | `remediation-analyzer-gate.2026-08-23T20-57.md`: exit 0, error count 0, `Skipping target "CoreCompile"` count 0 (cold rebuild proven). |
| Nullable / type gate (`/t:Rebuild`, `TreatWarningsAsErrors`, no `/p:Nullable=enable`) | PASS | `remediation-nullable-gate.2026-08-23T20-57.md`: exit 0, error count 0, `CoreCompile` skip count 0, command carried no `/p:Nullable=enable`. |
| No `dotnet format` use | PASS | All evidence records CSharpier via `dotnet tool run`. |
| Legacy project-file protection | PASS | Zero `*.csproj` / `*.props` / `*.targets` files in the diff. |

## 4. Language-Specific Unit Test Policy Compliance

| Check | Verdict | Evidence |
| --- | --- | --- |
| MSTest framework | PASS | New tests use `[TestMethod]`, `[Timeout]` (`Part3.cs:299-300, 354-355`). |
| Moq for mocks | PASS | Harness continues to inject `Mock<IQfcKeyboardHandler>`, `Mock<IQfcExplorerController>`, sentinel `Mock<IWebViewCoreInitializer>`; no new mocking library. |
| FluentAssertions | PASS | All new assertions use `.Should().BeTrue()/.BeFalse()` with `because:` messages. |
| No xUnit/NUnit introduction | PASS | Diff adds no new test-framework references. |

## 5. Test Coverage Detail

Changed-language enumeration from `git diff --name-only f85a36fa..b4d47adc`: C# only (3 `.cs`
files, all pre-existing test files; 0 new files, 0 production files). TypeScript, Python, and
PowerShell each have zero changed files on this branch, so no per-language verdict attaches to
them and no artifact is required for them.

| Language | Artifact | Repo-wide line | Repo-wide branch | Verdict |
| --- | --- | --- | --- | --- |
| C# | `artifacts/csharp/coverage.xml` (Cobertura, produced 2026-08-23 19:24, root `line-rate=0.855916`, `branch-rate=0.790598`) | 85.59% (>= 85%) | 79.06% (>= 75%) | C# repo-wide coverage PASS |

- C# new-code coverage: PASS — the diff adds no new production files or production lines; the two
  added tests raise executed-line counts (`QuickFiler` package +0.15 pp) and add nothing to the
  denominator.
- C# modified-file / changed-line coverage: PASS — zero production lines changed
  (`git diff -- QuickFiler/` empty); no changed-line regression is possible, and the measured
  per-class deltas for all 10 `QfcItemController*` classes are exactly +0.00 pp with package and
  repo-wide deltas positive (`evidence/qa-gates/remediation-coverage-delta.2026-08-23T20-57.md`,
  baseline `evidence/baseline/coverage.2026-08-21T18-10.md`, same-session methodology, identical
  deduplicated counting method, matched class count 10 = 10).
- Artifact-vs-record integrity: PASS — the on-disk `artifacts/csharp/coverage.xml` root attributes
  were read directly by this review and match the committed evidence figures byte-for-byte.
- TypeScript: zero changed files on the branch; no verdict attaches.
- Python: zero changed files on the branch; no verdict attaches (constraint 7 of `issue.md`: no
  Python toolchain exists in this repository).
- PowerShell: zero changed files on the branch; no verdict attaches.

## 6. Test Execution Metrics

| Run | Result |
| --- | --- |
| Final nine-assembly suite (P3-T6, `/InIsolation`, `TestCategory!=LiveOutlook`) | 6,459 total / 6,459 passed / 0 failed, exit 0 (`remediation-suite-run.2026-08-23T20-57.md`) |
| Coverage run (P3-T7, `Invoke-MSTestWithCoverage.ps1`) | 6,459/6,459, 0 failed, exit 0, single attempt |
| Ten-run determinism record under induced load (pre-remediation source; comment-only delta since, proven) | `QuickFiler.Test` failed count 0 in 10/10; suite-wide 9/10 green; run 5: one sibling `UtilitiesCS.Test` failure attributed to issue #594 (`determinism-ten-runs.2026-08-21T18-10.md`; comment-only-delta proof in `p4-t2-narrowing-rationale.2026-08-23T20-57.md`) |
| Both new regression tests + both named end-to-end tests | `Passed` in 10/10 runs (`regression-tests-ten-runs`, `named-tests-ten-runs`) |

## 7. Code Quality Checks

| Gate | Verdict | Detail |
| --- | --- | --- |
| Formatting | PASS | 0 unformatted files repo-wide (CSharpier 1.2.6 pinned) |
| Linting / analyzers | PASS | 0 errors; 5 pre-existing warnings, not gated |
| Type checking | PASS | 0 errors under `TreatWarningsAsErrors` |
| Tests | PASS | 0 failures, final run |
| C# coverage gates | PASS | 85.59% line / 79.06% branch repo-wide; no regression |
| File-size budget | PASS | 418 / 474 / 398 lines |
| Host-identifier hygiene | PASS | Added-line scan of the full branch diff for real `C:\Users\...` paths, the account name, and the machine name: zero real identifiers added (the only matches are the placeholder examples inside the sanitization rule document itself). |
| Closing-keyword scan | PASS | `git log --format=%B f85a36fa..HEAD`: zero matches of the case-insensitive regex for closing keywords before `#511` or `#571`. File-content matches exist only in `remediation-inputs.2026-08-23T20-57.md` (three negations, exempt by the recorded carve-out) and `plan.2026-08-21T18-10.md:26` (pre-existing; file content cannot auto-close an issue). |
| `modified-workflow-needs-green-run` | PASS | The diff touches no `.github/workflows/**`, `scripts/benchmarks/**`, or `.github/actions/**` path, so the rule does not fire. |

## Evidence Location Compliance

- Diff scan: zero files in the branch diff under `artifacts/baselines/`, `artifacts/qa/`,
  `artifacts/evidence/`, `artifacts/coverage/`, or any other forbidden `artifacts/` sub-path.
  Every evidence artifact in the diff is under
  `docs/features/active/winformspumphost-suite-determinism-511/evidence/<kind>/` with canonical
  kinds (`baseline`, `remediation-baseline`, `regression-testing`, `qa-gates`, `other`).
- `validate_evidence_locations.py` does not exist in this repository (there is no
  `scripts/dev_tools/`); the scan above was performed directly against `git diff --name-only`
  output.
- No evidence-path override was supplied by any caller; no `EVIDENCE_LOCATION_OVERRIDE_REJECTED`
  entry is required.
- Raw-artifact hygiene: `evidence/.gitignore` excludes `*.trx`, `*.coverage`, `*.coveragexml`, and
  the vstest scratch directories; zero raw TRX or binary coverage files are tracked or present on
  disk under the evidence tree (verified with `git ls-files` and `find`).

## 8. Gaps and Exceptions

1. **Spec `## Root Cause Analysis` retains pre-measurement assertions** ("`IsHandleCreated` is
   `false` for the whole test"; "The `ItemViewer`'s two WebView2 children never obtain a handle")
   that the committed measurement and the revised AC 6 / Scope sections contradict. Non-blocking:
   the remediation exit criteria deliberately scoped spec revision to the AC and Scope sections,
   and the revised sections carry explicit "(Revised 2026-08-23...)" markers. Recorded as
   code-review finding CR-1 with a follow-up recommendation.
2. **AC 1 / AC 3 literal wording** still says the ten TRX results are "stored under
   `evidence/regression-testing/`", but the raw TRX were deleted at explicit maintainer
   instruction after fidelity verification
   (`evidence/other/raw-vstest-artifact-disposition.2026-08-23T21-40.md`). The distilled markdown
   is the evidence of record. Non-blocking; recorded as CR-2.
3. **Original plan Phases 5-6 checkboxes remain unchecked** in `plan.2026-08-21T18-10.md`; that
   work was carried and completed by `remediation-plan.2026-08-23T20-57.md` (42/42 tasks checked),
   as its header records. Non-blocking bookkeeping observation (CR-4).
4. **Template resolution**: the MCP template asset tool is unreachable from this environment; this
   artifact reproduces the canonical heading set directly (provenance note above).

## 9. Summary of Changes

- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` (+9): corrected
  comment block above the retained defensive `viewer.Handle` read.
- `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` (+7): same correction at the
  standalone arrange block.
- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` (+108): two new
  regression tests — `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` (handle exists;
  `InvokeRequired` false on the pump thread) and
  `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` (pins the measured inherited
  handle-created state of both WebView2 children).
- `docs/features/active/winformspumphost-suite-determinism-511/**`: spec re-scope, decision
  record, remediation inputs/plan, and the full evidence tree (68 files).
- `.claude/agent-memory/**`: agent bookkeeping, including the host-identifier sanitization rule
  and its index entries (26 files).

## 10. Compliance Verdict

**PASS.** Zero blocking findings. All seven remediation cycle 1 exit criteria are satisfied on
independently re-verified evidence. The branch is ready for a pull request against `main` that
does not claim to close #511 or #571, per the recorded decision.

## Appendix A: Test Inventory

New tests introduced by this branch (both in
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`):

| Test | Location | Purpose |
| --- | --- | --- |
| `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` | `:301` | Asserts the harness viewer's `IsHandleCreated` is true and `InvokeRequired` is false on the pump thread, so `Control.Invoke` cannot throw for want of a handle. |
| `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` | `:356` | Pins the measured inherited state: both WebView2 children are handle-created by `ItemViewer` construction (`ISupportInitialize.EndInit`), not by the harness. |

Modified tests: none (the Part2/ViewerSetup edits are comment blocks plus the previously-added
defensive read inside existing harness/arrange code).

## Appendix B: Toolchain Commands Reference

Commands recorded in the evidence of record (executor-run; this review verified the committed
artifacts and re-ran read-only checks rather than regenerating coverage):

1. `dotnet tool restore`
2. `dotnet tool run csharpier format <three touched files>`; verify with `dotnet tool run csharpier check .`
3. `& '<VS18>\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
4. `& '<VS18>\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
5. `& '<VS18>\...\Extensions\TestPlatform\vstest.console.exe' <nine assemblies> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
6. `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput coverage\remediation.cobertura.xml` (numeric C# coverage source; copied to `artifacts/csharp/coverage.xml`)

Reviewer-run verification commands (read-only): `git diff --numstat f85a36fa..b4d47adc`;
`git diff f85a36fa..HEAD -- QuickFiler/` (empty); full line counts of the three changed files;
added-line scans for timing APIs, host identifiers, and closing keywords; direct reads of the
Cobertura root attributes in `artifacts/csharp/coverage.xml`; `git ls-files` / `find` raw-artifact
scans of the evidence tree.
