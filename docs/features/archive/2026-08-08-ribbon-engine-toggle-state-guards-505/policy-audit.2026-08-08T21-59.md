# Policy Compliance Audit — ribbon-engine-toggle-state-guards (#505, #506, #518)

- **Artifact:** `policy-audit.2026-08-08T21-59.md`
- **Reviewer:** feature-review agent
- **Date:** 2026-08-08
- **Branch:** `bug/ribbon-engine-toggle-state-guards-505` at `96650d6e`
- **Base:** `origin/main` at `f910ff2f` (merge base)
- **Audit scope:** full branch diff `f910ff2f..96650d6e` — 79 files, +6856/-55
- **Work mode:** `full-bug` (marker verified in `issue.md`); AC source: `spec.md` (AC-1..AC-23)
- **Template note:** the MCP template-resolution tool is not available in this session; this artifact reproduces the canonical major-section structure used by the accepted prior audit `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/policy-audit.2026-08-07T23-00.md`.

## Executive Summary

**Verdict: PASS — 0 blocking findings.**

Three defects in the ribbon engine-command surface are fixed as one unit of work: the two `getPressed` callbacks are rewritten to the synchronous Office contract answered from a cache (#505), the two toggle clicks are awaited with fault observation at a testable boundary (#506), and all 10 previously-unguarded `Controller.Engines.<member>` dereferences are guarded through two deliberately different shapes (#518). Every mandated gate passed in a single fingerprint-proven uninterrupted pass. Repo-wide coverage improved slightly; both new non-exempt types exceed the 90% new-code floor. One Major (non-blocking) concurrency finding and three Minor findings are recorded in `code-review.2026-08-08T21-59.md`; none blocks merge.

Verification model: evidence inspection of the committed artifacts under `evidence/`, corroborated by independent read-only checks in this worktree (diff greps, line counts, source inspection of every rewritten call site, `#507` zero-diff confirmation, `gh issue view 524`). No coverage generation or build was rerun; the orchestrator's independent re-verification of the toolchain results against the committed tree is accepted as given.

## 1. General Unit Test Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Independence / isolation / determinism | PASS | All async control flows through `TaskCompletionSource` or the coordinator's own `GetPrimeTask` handle; strict Moq mocks; no shared mutable state between tests (per-test `Harness`). Verified by reading all three new test files. |
| Fast execution, no flakiness sources | PASS | Grep of added lines: zero `Thread.Sleep`, `Task.Delay`, wall-clock reads, `Form`, `BackgroundWorker`, or message pump. The single `MessageBox` match is a doc comment. |
| No temporary files / external dependencies | PASS | Grep of added lines: zero filesystem, network, or COM usage in tests. The shape tests read the embedded ribbon resource from the production assembly (in-memory stream), not the filesystem. |
| AAA structure, documented intent | PASS | Every new test carries Arrange/Act/Assert sections, a doc comment, and reasoned FluentAssertions `because` strings. |
| Scenario completeness (UT2) | PASS with one Minor gap | Positive, negative, edge, error, and concurrency (in-flight prime) scenarios present. Gap: the `InvalidOperationException` guard in `ExecuteToggleAsync` (lines 219-220) is trivially reachable with the existing harness but has no test (code-review CR-3, Minor). |
| Coverage floors | PASS | See § 5. |

## 2. General Code Change Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Bugfix workflow (red-first regression tests) | PASS | `evidence/regression-testing/fail-before-505.2026-08-08T20-52.md` records 11 attributed pre-fix failures for R1/R2/R3/R5 (exit 1); R4's structurally-later red at `fail-before-r4-xml.2026-08-08T21-04.md`; green at `pass-after-505.2026-08-08T21-06.md`. Each failure is attributed to its pre-fix cause, not merely counted. |
| Minimal, targeted fix; scope discipline | PASS | Production diff confined to `TaskMaster/Ribbon/**` plus the two `.csproj` registrations and the ribbon XML. `TestSpam_Click` is byte-identical to the merge base (independently compared). Out-of-scope defects promoted, not fixed (§ 8). |
| Design principles (simplicity, separation of concerns) | PASS | Decision logic extracted into the host-neutral `EngineToggleStateCoordinator` + `EngineToggleCatalog`; COM-affine work stays behind injected delegates in the existing exempt glue. Follows the #503 seam precedent. |
| Error handling — fail fast, no swallow-all | PASS with one Minor note | Exactly one `catch` in the new type, at the click boundary; `ExecuteToggleAsync` propagates; prime faults observed via continuation and logged. Minor: a canceled prime task is silently ignored and permanently blocks re-priming (code-review CR-2). |
| Logging pattern | PASS | Faults route to the injected `logError`, production-wired to `logger.Error(message, exception)` (per-type log4net pattern). No console output. |
| File size <= 500 lines | PASS with documented exceptions | Every `.cs` file in the diff measured at head: largest are `EngineToggleStateCoordinatorTests.cs` 459, `EngineToggleStateCoordinator.cs` 389, `RibbonViewerEngineCallbackShapeTests.cs` 365, `RibbonViewer.EngineCommands.cs` 328. Exceptions: `RibbonExplorer.xml` 539 -> 545 (pre-existing overage recorded at #503; declarative embedded UI resource) and `TaskMaster.csproj` 581 -> 583 (MSBuild project file). Both were over the cap at the merge base; neither is production, test, or reusable-script code. |
| Public API compatibility | PASS | The two `*_GetPressed` signature changes have zero compile-time callers (reached only via XML strings). All new members are `internal`. |
| Dependencies | PASS | No new package; no project-reference change. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Check | Verdict | Evidence |
|---|---|---|
| Formatting — CSharpier | PASS | `evidence/qa-gates/csharpier-check.2026-08-08T21-33.md`: exit 0, 1517 files, 0 unformatted; independently re-verified by the orchestrator. |
| Analyzers — `EnableNETAnalyzers` + `EnforceCodeStyleInBuild`, `/t:Rebuild` | PASS | `evidence/qa-gates/msbuild-analyzers.2026-08-08T21-35.md`: exit 0, 0 errors, 6 warnings byte-identical to merge base (2x pre-existing CS2002 in `UtilitiesCS.Test`, 4 untagged System.Reactive advisories). 18 `csc.exe` invocations confirm the rebuild was not vacuous. |
| Type check | PASS (documented deviation, ratified) | The type-check gate used CI's command (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`), exit 0, 0 errors. The `CLAUDE.md` variant's `/p:Nullable=enable` is known-defective and tracked as issue #522; nullable is per-file opt-in in this solution and `.github/workflows/ci.yml` deliberately omits the flag. The deviation is documented in `spec.md` § Verification and in the issue delivery note. **This is not raised as a policy violation.** |
| Nullable handling | PASS | New files follow the sibling `TaskMaster/Ribbon` seam convention (no `#nullable` pragma anywhere under `TaskMaster/Ribbon/`); null contracts documented in XML doc comments; the two null-forgiving operators each carry an in-code rationale comment mirroring the #503 files. |
| Naming, XML docs | PASS | PascalCase/camelCase throughout; every new public/internal member carries an XML doc comment; comments explain why (STA constraint, readiness-vs-configuration split), not what. |
| Analyzer suppressions | PASS | No new suppression in the diff. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Check | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]`/`[TestMethod]`/`[DataTestMethod]` throughout; no xUnit/NUnit introduced. |
| Moq for mocking | PASS | `Mock<IAppItemEngines>(MockBehavior.Strict)` in the coordinator harness. |
| FluentAssertions | PASS | All new assertions use FluentAssertions with `because` strings. |
| Tests exercise behavior, not mock-call tautologies | PASS | Adversarially inspected. The ordering test records a real call sequence and probes the cache from inside the invalidation sink — the exact moment Office would re-query — so update-before-invalidate is asserted as observable behavior. The shape tests resolve callback names from the embedded ribbon XML (not hard-coded) and pin the compiler-emitted `AsyncStateMachineAttribute`, which exists only when the body actually awaits. The pre-`SetGlobals` test invokes the real viewer/controller pair by reflection. No tautological assertion was found. |

## 5. Test Coverage Detail

Coverage evidence: `evidence/baseline/tests-with-coverage.2026-08-08T20-44.md` (merge-base baseline, captured before implementation), `evidence/qa-gates/tests-with-coverage.2026-08-08T21-37.md`, `evidence/qa-gates/new-type-coverage.2026-08-08T21-38.md`, `evidence/qa-gates/coverage-comparison.2026-08-08T21-39.md`. Raw Cobertura documents live under the gitignored `coverage/` directory by design; the canonical hook artifact `artifacts/csharp/coverage.xml` was deliberately not created for this delivery and is absent from the worktree.

### Per-language coverage verdicts (full branch diff)

- **C# (.NET): repo-wide line coverage 85.92% (baseline 85.89%, +0.03), branch coverage 79.36% (baseline 79.34%); new-code line coverage `EngineToggleStateCoordinator.cs` 99.15% (133/135) and `EngineToggleCatalog.cs` 100.00% (18/18) against the 90% new-code floor; zero changed-line regression; 80% floor on the `CLAUDE.md` § UT2 testable denominator cleared — verdict PASS.**
  - Baseline: line-rate 0.858904, branch-rate 0.793353 (95706/111428 lines).
  - Post-change: line-rate 0.859190, branch-rate 0.793602 (95993/111725 lines). Change: +0.000286 line, +0.000249 branch.
  - New/changed-code coverage: 99.15% (coordinator), 100.00% (toggle catalog), 100.00% (`EngineCommandCatalog.cs`, 37/37 after its 13-line growth).
  - Disposition: no regression; the measured figures also clear the 85% line / 75% branch readings in `.claude/rules/general-unit-test.md`, so the recorded 80-vs-85 policy-document conflict (§ 8) has no effect on this delivery.
  - The 2 uncovered coordinator lines are a defensive `InvalidOperationException` guard (`ExecuteToggleAsync` 219-220); a trivially-addable direct test is recommended (code-review CR-3, Minor).
- **TypeScript:** zero changed `.ts`/`.tsx` files on the branch; no coverage verdict is required for this diff.
- **Python:** zero changed `.py` files on the branch; no coverage verdict is required for this diff.
- **PowerShell:** zero changed `.ps1`/`.psm1` files on the branch; no coverage verdict is required for this diff.

### Exemption integrity

`RibbonViewer` and `RibbonController` carry type-level `[ExcludeFromCodeCoverage]` under the ratified VSTO/COM ribbon-handler exemption (`CLAUDE.md` § UT2). Independently confirmed in the evidence: both types (all partials) are absent from the final Cobertura document, so the attribute is honored despite the custom `coverage.config` block; the exemption was neither removed nor widened, and no `[ExcludeFromCodeCoverage]` attribute appears on either new type (verified by grep). The nearly-flat repo-wide figure is the expected outcome and is not a regression.

## 6. Test Execution Metrics

- Final gate: 6435 passed, 0 failed across all 9 test assemblies (orchestrator's independent per-assembly re-verification additionally recorded 1 skipped); +36 executed cases over the 6399 baseline.
- The fingerprint-proven single pass (`evidence/qa-gates/toolchain-clean-pass.2026-08-08T21-40.md`): SHA-256 tree fingerprint identical before and after the five back-to-back steps, `csharpier` rewrote 0 files, 18 `csc.exe` invocations, all exits 0.
- One earlier Phase 5 attempt aborted at the aggregate test gate on `QuickFiler.Test` `WinFormsPumpHost` message-pump failures under machine load. This is the pre-existing environmental instability tracked as **#511**; `QuickFiler.csproj` does not reference `TaskMaster`, so this change cannot reach it. Disclosed in `evidence/other/phase5-attempt1-aborted.2026-08-08T21-30.md`; the recorded final pass contains no restart. **Not a regression of this change.**

## 7. Code Quality Checks

| Check | Verdict | Evidence |
|---|---|---|
| Format gate (CSharpier) — Code Quality | PASS | Exit 0, 0 unformatted, final pass. |
| Analyzer gate — Code Quality | PASS | 0 errors; 6 warnings byte-identical to the merge base; zero new diagnostics. |
| #507 fix intact | PASS | `RibbonController.Intelligence.cs:204` reads exactly `internal IAppItemEngines Engines => Globals?.Engines;` at head; the file has a zero-line diff on the branch. No revert. |
| Guarded-site accounting | PASS | Independently re-derived: the merge-base file held 11 `Engines.` references, 1 pre-gated (`TestSpam_Click`), 10 unguarded. At head, all 7 remaining `Controller.Engines.` textual references sit inside `RunEngineCommandAsync` lambdas; the 4 toggle/getPressed sites route through the coordinator. 0 unguarded production dereferences remain. |
| Tone of committed prose | PASS | Spec, issue, plan, and evidence artifacts are factual and neutral; no humor, hyperbole, or metaphor found. |

## 8. Gaps and Exceptions

1. **#522 type-check deviation (accepted).** Documented in `spec.md` § Verification with the #522 citation; CI's command used instead. Not a violation; #522 is deliberately not evaluated here.
2. **AC-22 MANUAL-ONLY pending (intended disposition).** Live-Outlook verification (callback binding, toggle persistence across menu reopen, pre-`SetGlobals` safety of all ten callbacks) is deliberately unchecked in `spec.md` with a maintainer checklist at `evidence/manual-verification/ac22-checklist.2026-08-08T21-44.md`. Automated evidence pins the required signatures; only the live session can prove VSTO actually binds them. This is the designed disposition, not a gap in the delivery.
3. **Recorded policy-document conflict (pre-existing, unresolved).** `CLAUDE.md` § UT2 states 80% repo-wide / 90% new-code; `.claude/rules/general-unit-test.md` and `quality-tiers.md` state 85% line / 75% branch uniform. The executor recorded the conflict rather than silently resolving it (`coverage-comparison.2026-08-08T21-39.md` § 5). The measured figures clear every reading, so the conflict is moot for this delivery; reconciling the documents is outside this scope.
4. **Pre-existing file-size overages.** `RibbonExplorer.xml` (539 -> 545) and `TaskMaster.csproj` (581 -> 583) exceed 500 lines at both base and head; both are declarative resource/MSBuild files, recorded and accepted at #503, not remediated here.
5. **Out-of-scope defects promoted, not fixed (AC-17).** Research § 10 item 1 was already tracked as #504 (tracker-verified); item 3 resolved during authoring; item 2 (unguarded `Globals` dereferences in `RibbonController.Intelligence.cs`) was promoted through the MCP lifecycle as **#524** — independently re-verified in this review via `gh issue view 524` (OPEN, `Bug: ribbon-controller-intelligence-unguarded-globals-deref`). Receipts: `evidence/issue-updates/research-defect-promotions.2026-08-08T21-43.md`. One Minor documentation staleness: `issue.md` Delivery Note point 3 still describes the item-2 promotion as deferred (code-review CR-4).
6. **Major non-blocking concurrency finding.** A prime/toggle last-writer interleaving can persist a stale cached pressed state; detailed as CR-1 in `code-review.2026-08-08T21-59.md` with a recommended fix and a promotion recommendation. It does not violate any acceptance criterion or policy gate and strictly improves on the merge-base behavior, so it is dispositioned as a follow-up, not remediation.

## 9. Summary of Changes

- New: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (389 lines), `TaskMaster/Ribbon/EngineToggleCatalog.cs` (92 lines) — host-neutral, non-exempt, fully unit-tested.
- Rewritten callbacks in `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`: two synchronous `getPressed`, two awaited `async void` toggle clicks, six command handlers routed through `RunEngineCommandAsync` with deferred dereferences, plus `InvalidateEngineToggle` (dispatcher-marshalled).
- `TaskMaster/Ribbon/RibbonController.EngineCommands.cs`: lazy `EngineToggles` coordinator wiring plus `IsEngineToggleActive` / `HandleEngineToggleClickAsync` forwarders.
- `EngineCommandCatalog.cs` +6 entries; `RibbonExplorer.xml` +6 `getEnabled` attributes (landed atomically with the catalog, as the set-equality tests force).
- New tests: `EngineToggleStateCoordinatorTests.cs` (16 methods), `EngineToggleCatalogTests.cs` (7 methods), `RibbonViewerEngineCallbackShapeTests.cs` (5 methods); `EngineCommandCatalogTests.cs` and `RibbonExplorerXmlTests.cs` extended. +36 executed cases.
- Feature-folder documentation, planning artifacts, evidence tree, and agent-memory updates.

## 10. Compliance Verdict

**PASS. 0 blocking findings.** All applicable gates pass on evidence; the single Major finding (CR-1) and three Minor findings are dispositioned non-blocking with recommended follow-ups. No remediation-inputs artifact is produced.

## Evidence Location Compliance

The branch diff was scanned for evidence files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`: **zero occurrences**. All delivery evidence lives under the canonical `docs/features/active/2026-08-08-ribbon-engine-toggle-state-guards-505/evidence/<kind>/` tree. No stale `artifacts/csharp/coverage.xml` or `artifacts/pester/` leftover exists in the worktree.

## Appendix A: Test Inventory

| File | Kind | Members | Notes |
|---|---|---|---|
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | new | 16 test methods | Constructor contracts (4); `GetPressed` cached-read semantics incl. null/whitespace/unmapped key, null accessor, single in-flight prime, prime completion, prime fault (6); `ExecuteToggleAsync` ordering, fault propagation, unmapped key (3); `HandleToggleClickAsync` boundary observation, blocked click, ready path (3). |
| `TaskMaster.Test/Ribbon/EngineToggleCatalogTests.cs` | new | 7 test methods | Mapping rows, unknown/null/empty key, set equality, no duplicates. |
| `TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs` | new | 5 test methods | R1 `getPressed` signature pins; toggle `onAction` pins; R2 pre-`SetGlobals` no-throw/false; R5 `AsyncStateMachineAttribute` pins for toggle clicks and the two `ShowSaveInfo` handlers. Callback names resolved from the embedded ribbon XML. |
| `TaskMaster.Test/Ribbon/EngineCommandCatalogTests.cs` | extended | +6 data rows; 8->14 set equality | R3. |
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | comment update | existing set-equality tests | R4 red forced mid-change by the catalog extension; stale comment corrected. |

## Appendix B: Toolchain Commands Reference

1. Format: `dotnet tool run csharpier .` / `csharpier check .`
2. Analyze: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. Type-check: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (CI's command; `/p:Nullable=enable` omitted per issue #522, documented in `spec.md` § Verification)
4. Test: `vstest.console.exe <9 test assemblies> /EnableCodeCoverage`, excluding stale `\.claude\worktrees\` builds from the assembly glob; per-assembly isolation for the final verification run.
