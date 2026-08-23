# Policy Compliance Audit — winforms-message-pump-test-seam (Issue #230)

- Reviewer: feature-review agent
- Date: 2026-08-07T23-00
- Branch: `feature/winforms-message-pump-test-seam-230` @ `8f98264c7c31a0afcf18848b28a021a2ba9012e0`
- Base branch: `main`; merge-base `74be19646f0412c6f0eab22999624b9acad91d22` (recomputed via `git merge-base HEAD origin/main`; matches the caller-supplied value)
- Work mode: `full-feature` (persisted marker in `issue.md`)
- Template note: the MCP tool `resolve_policy_audit_template_asset` is not available in this session (no MCP tools exposed). The canonical major headings were reproduced from `.claude/skills/policy-audit-template-usage/SKILL.md`, which enumerates them authoritatively. No template instruction block is present because no template body was copied.

## Executive Summary

The branch delivers the `WinFormsPumpHost` test seam, 21 new MSTest tests, an additive optional-parameter extension to two static factories, and the removal of 8 `[ExcludeFromCodeCoverage]` attributes (boundary 19 -> 11), plus complete evidence under the feature folder. Independent re-verification confirms: the exemption census (19 baseline sites -> 11 head sites, exactly the 8 target members removed), the repo-wide C# coverage figures (line 85.83%, branch 79.22%, both above the uniform floors), changed-line coverage (100%), determinism (zero banned timing/temp-file APIs; all 21 new tests carry `[Timeout]`), file sizes (largest changed file 489 lines), and formatting (`csharpier check` clean on all 9 changed `.cs` files). One coverage sub-check is recorded as FAIL with a non-blocking disposition: the modified file `QfcItemController.ViewerSetup.cs` sits below the uniform per-file floor for pre-existing reasons and improved on both metrics in this feature. Overall verdict: **PASS** — no remediation trigger fires.

- Scope-narrowing attempts by the caller: none detected. The caller prompt explicitly requested the full feature-vs-base audit; scope was independently derived from `git diff 74be1964..8f98264c`.
- PR-context correction: the summary overview at `artifacts/pr_context.summary.txt` originally read "Core logic changes: 0 files", misclassifying this C#-bearing branch as docs-only (recurring defect; see Section 8). The overview was corrected in place to enumerate the 10 changed C# code files so downstream language enumeration operates on accurate data.

## 1. General Unit Test Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Independence / isolation (UT1) | PASS | One pump host per test, released in `finally`/`using`; the iteration-1 cross-class dispatcher race was diagnosed as a genuine UT1 defect and fixed with a `SemaphoreSlim(1,1)` ownership gate (`evidence/qa-gates/final-test-coverage.2026-08-08T00-05.md`); iteration-2 full suite 6293/6293. |
| Determinism | PASS | Independent scan of all 9 changed `.cs` files: zero hits for `Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`, `SpinWait`, `while (` polling. All waits are `ManualResetEventSlim`, `TaskCompletionSource` (`RunContinuationsAsynchronously`), or awaited tasks. All 21 new tests carry `[Timeout]` (13+5+2+1 across the four new-test files, independently counted). |
| No temporary files (UT4) | PASS | Independent scan: zero `GetTempFileName`/`GetTempPath`/`GetRandomFileName` hits in changed files; matches `evidence/qa-gates/determinism-audit.2026-08-07T23-35.md`. |
| No external dependencies (UT4) | PASS | All Outlook types are Moq'd COM interfaces; `IWebViewCoreInitializer` is always mocked and faults at the seam, so the WebView2 runtime never initializes; no network/DB access in any new test. |
| AAA structure, documented intent (UT3) | PASS | All 21 new tests use explicit Arrange/Act/Assert comments, XML doc summaries, and FluentAssertions `because:` messages (verified by reading `WinFormsPumpHostTests.cs` and the controller test diffs). |
| Scenario completeness (UT2) | PASS | Positive flows (all four posting members, both marshal routes), negative flows (sync throw, async fault, post-after-stop), edge cases (double-`Dispose`, null-task delegate guard), error channels (`Application.ThreadException` recorder rethrow at `StopAsync`), concurrency (cross-class dispatcher gate), state transitions (started -> stopped). |
| Coverage requirements | PASS with one dispositioned sub-check | See Section 1.2. |

### 1.2 Coverage Verification (mandatory for every language with changed files)

Changed-language enumeration from `git diff --name-only 74be1964..HEAD`: C# only (`.cs`, `.csproj`); remaining changes are Markdown documentation, agent-memory Markdown, and committed Cobertura XML evidence.

- TypeScript: zero changed files on the branch (`git diff` shows no `.ts`/`.tsx`); no verdict required for a language with zero changed files.
- Python: zero changed files on the branch (no `.py`); no verdict required for a language with zero changed files.
- PowerShell: zero changed files on the branch (no `.ps1`/`.psm1`); no verdict required for a language with zero changed files.

#### 1.2.1 C# coverage (changed language — explicit verdicts)

Artifact: `artifacts/csharp/coverage.xml` (canonical path, Cobertura, produced by `dotnet-coverage collect` wrapping `vstest.console.exe /InIsolation` over all 9 first-party test assemblies). Verified byte-identical (`cmp`) to the committed `evidence/qa-gates/coverage-final.cobertura.xml`, and its head SHA-era timestamp matches the executor's final P8-T5 run. Test assemblies are excluded from the denominator (no test-project packages appear in the XML), per UT2.

- **C# repo-wide line coverage: 85.83% (95,293/111,021) — PASS** (>= 85% uniform floor; also >= the CLAUDE.md 80% repo floor). Independently parsed from the Cobertura root element of `artifacts/csharp/coverage.xml`.
  - Baseline: 85.6453% (94,937/110,849) — `evidence/baseline/coverage-baseline.cobertura.xml`
  - Post-change: 85.8333% (95,293/111,021)
  - Change: +0.1880 pts raw; +0.1769 pts denominator-adjusted (172-line denominator growth from de-exemption accounted for in `evidence/qa-gates/coverage-delta.2026-08-08T00-15.md`)
  - Disposition: improvement in both raw and denominator-adjusted comparisons; no regression.
- **C# repo-wide branch coverage: 79.22% (22,073/27,862) — PASS** (>= 75% uniform floor). Baseline 79.0039%; change +0.2187 pts.
- **C# new/changed-code coverage: 100.00% (6/6 changed executable lines covered) — PASS.** The only production edits introducing executable statements are the six seam-assignment lines in `CreateAsync`/`CreateSequentialAsync`; every other production edit is a comment rewrite or attribute removal. Per-line hits verified in `coverage-delta.2026-08-08T00-15.md` Gate (b); the newly de-exempted member surface aggregates to 92.98% line coverage (159/171), above the 90% new-code bar in CLAUDE.md UT2.
- **C# modified file `QuickFiler/Controllers/QfcItemController.Initialization.cs`: line coverage 95.28% (404/424), branch 94.83% (55/58) — PASS** (independently computed from the Cobertura XML; baseline 90.11%/96.15%; the branch-percentage movement reflects newly instrumented de-exempted members entering the file denominator, with zero changed-line regression).
- **C# modified file `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`: line coverage 81.88% (244/298), branch 62.96% (68/108) — FAIL against the strict 85%/75% per-file uniform floor, dispositioned non-blocking.** Disposition rationale: (a) baseline for the same file was 74.37% line / 56.00% branch, so this feature *improved* the file by +7.51 line pts and +6.96 branch pts; (b) every line changed by this feature in the file is either non-executable (comment/attribute edits) or fully covered (`ResolveControlGroupsAsync` 38/38 = 100%), so there is no regression on changed lines; (c) the residual uncovered lines sit entirely in members this feature did not touch — pre-existing QuickFiler coverage debt tracked by the repo-wide uplift issue #197; (d) the workflow remediation trigger for modified files (< 80% or regression) does not fire (81.88% >= 80%, no regression). New production files: none (the two new `.cs` files are test-project files, outside the coverage denominator per UT2), so no new-file floor applies to production coverage.
- Package context: the QuickFiler package reads 80.82% line / 74.65% branch at head — also a pre-existing sub-floor figure that this feature improved; same #197 disposition as above.

Coverage-tool note: the SubagentStop hook parses `artifacts/csharp/coverage.xml` with a JaCoCo `//counter` query; this file is Cobertura, so the hook computes null and does not itself gate the percentages. The figures above are therefore reported from direct Cobertura parsing (root attributes plus per-class `<line>` aggregation), which is the authoritative measurement here.

## 2. General Code Change Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Design principles (simplicity, reusability, separation) | PASS | The host is a single cohesive seam class mirroring the accepted WPF `StaDispatcherHost` precedent; pure completion logic is in small static helpers; no new frameworks or indirection layers. |
| File size <= 500 lines (all changed non-markdown files) | PASS | Independently measured with `awk NR` (not `Measure-Object -Line`): Initialization.cs 489, WinFormsPumpHost.cs 482, QuickFiler.Test.csproj 476, ViewerSetupTests.cs 467, WinFormsPumpHostTests.cs 443, SeamFactoryTests.cs 436, ViewerSetup.cs 430, Part2 409, Part3 290, InitializationTests.cs 209. The 500-line pressure was handled by splitting the test partials (Part2/Part3) — the correct mechanism. |
| Error handling — fail fast, no silent catches | PASS | Every `catch` in the host immediately routes the exception to a `TaskCompletionSource`, the initialization-error rethrow, or the pump-exception recorder rethrown at `StopAsync`; nothing is swallowed. |
| Public API compatibility | PASS | Factory extension is additive with defaulted parameters; `evidence/other/factory-seam-verification.2026-08-07T23-00.md` enumerates zero in-repo callers; full suite passes unchanged. |
| Dependencies | PASS | `QuickFiler.Test.csproj` diff adds only four `<Compile Include>` items; no new packages. |
| Bugfix workflow | Not applicable to this branch | Feature work (test infrastructure), not a defect fix. |
| Supporting documents updated | PASS | spec.md, user-story.md, plan, research doc, and 24 evidence artifacts committed in the feature folder. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Check | Verdict | Evidence |
|---|---|---|
| Formatting — CSharpier | PASS | Independently re-run this session: `dotnet tool run csharpier check` on all 9 changed `.cs` files -> "Checked 9 files", EXIT 0. Executor final pass: `evidence/qa-gates/final-format.2026-08-07T23-45.md`. |
| Analyzers — msbuild `EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | PASS (evidence-verified) | `evidence/qa-gates/final-analyzers.2026-08-07T23-48.md` iteration 2: EXIT 0, 0 errors, 5 pre-existing warning lines identical to the P0-T4 baseline set. Not re-run this session (full-solution build); the artifact records command, exit code, and baseline comparison. |
| Nullable gate — msbuild `Nullable=enable /p:TreatWarningsAsErrors=true` | PASS (evidence-verified) | `evidence/qa-gates/final-nullable.2026-08-07T23-50.md` iteration 2: EXIT 0, 0 errors, unchanged from the P0-T5 baseline. |
| net481 constraints | PASS | No `init`/`record`/`record struct` anywhere in the new files (host and harness are plain sealed classes with constructor-assigned fields), avoiding CS0518. |
| Naming, XML docs | PASS | PascalCase/camelCase respected; every internal member of the host and the harness carries an XML doc comment explaining contract and rationale. |
| Design seams | PASS | Factory seam parameters mirror the primary constructor's existing optional-seam pattern (interface seams `IUiDispatcher`, `IWebViewCoreInitializer`, delegate seam `Func<MailItem, ConversationResolver>`); smallest seam that unlocks the tests. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Check | Verdict | Evidence |
|---|---|---|
| MSTest framework (CUT1) | PASS | `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting` throughout; no xUnit/NUnit introduced. |
| Moq for mocking (CUT2) | PASS | `Mock<IApplicationGlobals>`, `Mock<MailItem>`, `Mock<IWebViewCoreInitializer>` etc. in the harness partial. |
| FluentAssertions (CUT2) | PASS | All new assertions use FluentAssertions with `because:` diagnostics. |
| CUT3 toolchain order | PASS (evidence-verified) | Phase 8 iteration 2 ran format -> analyzers -> nullable -> `vstest.console.exe` with coverage, all EXIT 0 in a single clean pass, with the restart rule honored after the iteration-1 test failure. |

## 5. Test Coverage Detail

See Section 1.2.1 for the full C# figures. Per-member line coverage of the 8 de-exempted members (from `coverage-delta.2026-08-08T00-15.md`, spot-consistent with the canonical XML): `Initialize` 9-arg 100%, `Initialize(bool)` 100%, `InitializeAsync` 83.33%, `InitializeGraphicsAsync` 86.36%, `InitializeSequentialAsync` 86.36%, `CreateAsync` 89.47%, `CreateSequentialAsync` 100%, `ResolveControlGroupsAsync` 100%; aggregate 92.98%. The uncovered residue is the documented dark/light theme branch split plus the structurally unreachable post-`InitializeWebViewAsync` tails (reaching them requires the real CoreWebView2 runtime, an external process barred by UT4).

## 6. Test Execution Metrics

- Full suite (iteration 2, unattended): **6293/6293 passed, 0 failed, 0 skipped, EXIT 0**, 9 test assemblies (`evidence/qa-gates/final-test-coverage.2026-08-08T00-05.md`).
- Delta vs baseline run: +21 tests, exactly matching the 21 feature-added test methods (independently counted from `[Timeout]`/`[TestMethod]` attribution across the four new-test files).
- Iteration 1 recorded 2 failures from a genuine cross-class static-dispatcher race; the fix (semaphore ownership gate) is deterministic and the loop was restarted from the formatting stage per the restart rule.

## 7. Code Quality Checks

| Check | Result |
|---|---|
| Formatting (csharpier check, re-run this session) | PASS — 9/9 changed files clean |
| Analyzer build (executor evidence) | PASS — 0 errors, warnings identical to baseline |
| Nullable build (executor evidence) | PASS — 0 errors |
| Exemption census | PASS — independently recounted: baseline 19 sites (Initialization.cs 7 + ViewerSetup.cs 3 + others 9), head 11 sites; exactly the 8 target members de-exempted; `InitializeWebViewAsync` retains its attribute with the required updated justification |
| Evidence integrity | PASS — canonical `artifacts/csharp/coverage.xml` byte-identical to committed final Cobertura; baseline/final root figures match every number quoted in `coverage-delta.2026-08-08T00-15.md` |

## 8. Gaps and Exceptions

1. **`QfcItemController.ViewerSetup.cs` per-file floor** — FAIL sub-check dispositioned non-blocking (Section 1.2.1). Pre-existing debt in untouched members; improved by this feature; tracked by #197. No remediation trigger fires (>= 80%, no regression, changed lines 100% covered).
2. **PR-context summary misclassification** — the generated overview reported "Core logic changes: 0 files" for a branch with 10 changed C# code files. Recurring tooling defect in the summary generator (observed across many prior C#-touching reviews). Corrected in place this session; the correction is annotated inside the artifact. The underlying generator defect remains open tooling debt.
3. **Analyzer/nullable/test stages not re-run this session** — full-solution msbuild/vstest runs were not repeated during review; verdicts for those stages are evidence-verified from the committed Phase 8 iteration-2 artifacts (command lines, exit codes, and baseline comparisons recorded). Formatting, determinism, census, file-size, and all coverage figures were independently re-verified this session.
4. **`InitializeAsync`/`CreateAsync` partial member coverage** — 83.33%/89.47% with the uncovered tails structurally unreachable without the CoreWebView2 runtime; accepted per the spec's documented carve-out and UT4's external-process prohibition.

## 9. Summary of Changes

- New: `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` (482 lines) — STA background-thread WinForms message-pump seam; `WinFormsPumpHostTests.cs` (13 self-tests).
- New: `QfcItemController.InitializationTests.Part2.cs` (pump harness, no tests) and `Part3.cs` (5 pump-hosted tests); 2 new factory tests in `SeamFactoryTests.cs`; 1 new pump-hosted test in `ViewerSetupTests.cs`.
- Production: additive optional seam parameters on `CreateAsync`/`CreateSequentialAsync`; 8 `[ExcludeFromCodeCoverage]` removals with covering tests landed in the same change; `InitializeWebViewAsync` justification comment rewritten, attribute retained.
- Docs/evidence: full feature folder (issue/spec/user-story/plan/research + 24 evidence artifacts including baseline and final Cobertura XMLs).
- Agent memory: atomic-executor/atomic-planner/task-researcher memory updates (Markdown, non-code).

## 10. Compliance Verdict

**PASS.** All policy gates pass on independent verification or committed final-pass evidence. The single FAIL-labelled sub-check (ViewerSetup.cs per-file floor) is pre-existing debt that this feature measurably reduced, with zero changed-line regression, and does not meet any remediation trigger. Recommendation: GO for PR, with maintainer re-ratification of the reduced 19 -> 11 exemption boundary as the required PR-lifecycle approval step (per the #227 precedent and spec.md's governance note).

## Evidence Location Compliance

- Scan of the full branch diff for files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`: **zero occurrences.** All 24 evidence artifacts live under the canonical `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/<kind>/` tree (baseline/, qa-gates/, other/).
- `validate_evidence_locations.py` does not exist in this repository (`scripts/dev_tools/` absent; confirmed by listing `scripts/`); the equivalent check was performed manually against the diff file list.
- The gitignored canonical coverage path `artifacts/csharp/coverage.xml` is an allowed canonical artifact location, not a diff-committed evidence file.

## Appendix A: Test Inventory

Feature-added tests (21), all carrying `[Timeout]`:

1-13. `WinFormsPumpHostTests`: Constructor_WhenHostStarts_CapturesWinFormsContextOnADistinctThread; InvokeAsyncAction_WhenPosted_RunsOnThePumpThread; InvokeAsyncFactory_WhenPosted_RunsOnThePumpThreadAndReturnsTheValue; RunAsyncVoid_WhenPosted_StartsAndResumesOnThePumpThread; RunAsyncResult_WhenPosted_RunsOnThePumpThreadAndReturnsTheValue; AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread; BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread; InvokeAsync_WhenWorkThrows_FaultsTheAwaitedTaskWithTheOriginalException; RunAsyncVoid_WhenWorkFaults_SurfacesTheOriginalUnwrappedException; RunAsyncResult_WhenWorkFaults_SurfacesTheOriginalUnwrappedException; PostingMembers_AfterStop_FaultWithObjectDisposedException; Dispose_CalledTwice_IsANoOp; StopAsync_WhenThePumpLoopRecordedAnException_RethrowsIt.

14-18. `QfcItemController_InitializationTests` (Part3, 5 pump-hosted tests): Initialize(bool)/9-arg overload/InitializeAsync/InitializeGraphicsAsync/InitializeSequentialAsync through the pump host.

19-20. `QfcItemController_SeamFactoryTests` (2): CreateAsync_WithFaultingWebViewSeam_*; CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController.

21. `QfcItemController_ViewerSetupTests` (1): ResolveControlGroupsAsync_ThroughThePumpHost_*.

Full-suite context: 6293 tests across 9 assemblies, all passing in the final unattended run.

## Appendix B: Toolchain Commands Reference

Commands run this session (check-only):

- `git merge-base HEAD origin/main` -> `74be19646f0412c6f0eab22999624b9acad91d22`
- `git diff --name-status 74be1964..HEAD` (scope derivation); `git diff --numstat` (language enumeration)
- `dotnet tool run csharpier check <9 changed .cs files>` -> EXIT 0
- `grep` scans for banned timing APIs, temp-file APIs, `[Timeout]`/`[TestMethod]` counts, `ExcludeFromCodeCoverage` census (head and `git show 74be1964:` baseline)
- `awk 'END{print NR}'` line counts for the 500-line audit
- Python Cobertura parsing of `artifacts/csharp/coverage.xml`, `evidence/baseline/coverage-baseline.cobertura.xml`, `evidence/qa-gates/coverage-final.cobertura.xml` (repo-wide root attributes; per-file `<line>`/condition-coverage aggregation)
- `cmp` identity check: canonical coverage artifact vs committed final Cobertura

Executor final-pass commands (evidence-verified, `evidence/qa-gates/`):

1. `dotnet tool run csharpier format .` / `check .` (P8-T1)
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (P8-T3, EXIT 0)
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` (P8-T4, EXIT 0)
4. `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput .../coverage-final.cobertura.xml` (`dotnet-coverage collect` wrapping `vstest.console.exe /InIsolation`, P8-T5, EXIT 0, 6293/6293)
