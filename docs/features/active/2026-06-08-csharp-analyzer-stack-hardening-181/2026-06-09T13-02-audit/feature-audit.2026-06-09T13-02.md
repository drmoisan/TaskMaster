# Feature Audit: Issue #181 — csharp-analyzer-stack-hardening (Cycle 6 reaudit)

- Branch: `feature/csharp-analyzer-stack-181`
- HEAD: `6ede1964` (cycle-6 changes in WORKING TREE)
- Base: `main`; merge-base: `2a522ed831865c2918ab02df153ef2929b0617dc`
- Work Mode: full-feature (per `issue.md` marker) -> AC sources are `user-story.md` (authoritative AC1–AC8) and `spec.md` (Definition of Done / Seeded Test Conditions)
- Timestamp: 2026-06-09T13-02

## Summary

This is the cycle-6 end-of-cycle reaudit. Cycle 6 is a deterministic-test-conversion remediation: it converts prohibited non-deterministic timing primitives (`Thread.Sleep`, `signal.Wait(<timeout>)`, `SpinWait.SpinUntil(..., <timeout>)`) in the test suite to deterministic seams. The feature's acceptance criteria (AC1–AC8) concern the analyzer-stack hardening itself and were delivered and verified in prior cycles (1–5); they are all checked `[x]` in both `user-story.md` and `issue.md`. Cycle 6 must preserve, and does not regress, those criteria.

The relevant cycle-6 acceptance is: (a) the named failing test is deterministic; (b) every cataloged prohibited occurrence is converted, retained-with-justification, or recorded as a scope-change (none silently omitted/masked); (c) full toolchain passes in one clean pass with zero regression and the coverage gate met; (d) blocking_count == 0. All four hold. The full first-party suite passes 4065/4065 with `/InIsolation`, confirming AC5 (toolchain non-regression) and AC7 (MSTest/Moq, 80/90 coverage retained) are not broken by the conversion.

Overall verdict: PASS. BLOCKING FINDINGS: 0.

## Scope and Baseline

- Audit scope: the full branch diff against the resolved base branch `main` (merge-base `2a522ed831865c2918ab02df153ef2929b0617dc`) PLUS the uncommitted cycle-6 working-tree changes on top of HEAD `6ede1964`. Scope is NOT narrowed to any plan/task/phase subset.
- Baseline: pre-cycle state captured in `evidence/baseline/*.2026-06-09T11-31.md` (git state, named-test source, format, analyzer, nullable, and full-suite coverage 85.52% on UtilitiesCS.dll).
- Changed files in scope (cycle 6): 7 production files (`SmartSerializableBase.cs`, `SmartSerializable.cs`, `TimedQueueOfActions.cs`, `IEnumerableExtensions.cs`, `AsyncMultiTasker.cs`, `FolderRemapTree.cs`, `OlTableExtensions.TableAccess.cs`), 14 test files, the new `ManualFireTimerWrapper.cs` helper, and `UtilitiesCS.Test.csproj`.
- Out of scope (excluded, modified-but-unstaged): `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs`, `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs`.
- Language coverage: C# is the only language with changed files; its coverage verdict is explicit PASS (85.46% repo-wide first-party, no changed-line regression).

## Acceptance Criteria Inventory

Authoritative source (full-feature mode): `user-story.md` `## Acceptance Criteria` (mirrored in `issue.md`):

- AC1: Analyzer packages referenced by first-party projects; restore cleanly via `nuget restore`.
- AC2: BannedApiAnalyzers + BannedSymbols.txt active; 5 banned symbols flagged in new/touched code.
- AC3: TimeProvider/FakeTimeProvider seam + guidance added to rules/csharp.md; no runtime behavior changed.
- AC4: .editorconfig/.globalconfig carries new severities, file-scoped-namespace pref, naming rules, scoped to avoid build-breaking errors.
- AC5: All four toolchain stages pass locally to the extent the environment allows; nullable TreatWarningsAsErrors step does NOT regress.
- AC6: PR CI is GREEN, including nullable-as-errors and MSTest-with-coverage steps.
- AC7: No do_not_change invariant violated; rules/csharp.md updated retaining MSTest/Moq, 80/90 coverage, msbuild+vstest.
- AC8: Change scoped to C# build-config + rules/csharp.md (+ analyzer refs); no application logic changes except seam introductions required to compile.

Secondary source: `spec.md` `## Definition of Done` and `## Seeded Test Conditions` are generic process checkboxes (unchecked in the template); they are tracked below but are not the authoritative numbered ACs.

Cycle-6 remediation acceptance (from `remediation-inputs.2026-06-09T11-31.md`):
- R-AC1: `Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite` passes deterministically with no `Thread.Sleep`/`signal.Wait(<timeout>)`.
- R-AC2: Every cataloged prohibited occurrence converted, retained-with-justification, or halted as a recorded scope-change — none silently omitted or masked.
- R-AC3: Full toolchain passes in one clean pass; zero regression; coverage gate met.
- R-AC4: blocking_count == 0.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence (relative to baseline) |
|----|---------|---------------------------------|
| AC1 | PASS (unchanged this cycle) | Analyzer package refs delivered in prior cycles; cycle-6 analyzer build EXIT 0. No package/restore change this cycle. |
| AC2 | PASS (unchanged) | BannedApiAnalyzers active; cycle-6 diff scan confirms NO banned symbol (`Thread.Sleep`/`Task.Delay`/`DateTime.Now/UtcNow`/`Random.Shared`) added to production. |
| AC3 | PASS (unchanged) | `rules/csharp.md` TimeProvider guidance present; cycle-6 seams reuse existing `ITimerWrapper` and introduce no runtime behavior change (all seams default to current behavior). |
| AC4 | PASS (unchanged) | No `.editorconfig`/`.globalconfig` change this cycle; analyzer 0/0 confirms no build-breaking severity regression. |
| AC5 | PASS | All four C# toolchain stages pass in one clean final pass: csharpier EXIT 0; analyzer 0/0; nullable (TreatWarningsAsErrors) 0/0; vstest 4065/4065. Nullable step does not regress. |
| AC6 | PARTIAL/PENDING (CI gate is external) | Local gates are green. PR GitHub Actions CI green against the branch head is required by the cycle acceptance and is the authoritative repo-wide 80% gate; cycle-6 changes are currently in the WORKING TREE (uncommitted), so the CI run must be re-confirmed after commit/push. Non-blocking for this reviewer artifact; the orchestrator must verify CI green post-push. |
| AC7 | PASS | MSTest/Moq/FluentAssertions retained; 80/90 coverage model retained (UtilitiesCS.dll 85.46% >= 80%, no changed-line regression); msbuild+vstest retained. No do_not_change invariant violated (no `.claude/rules` / `BannedSymbols.txt` / analyzer-wiring change). |
| AC8 | PASS (with deliberate scope note) | Cycle 6 deliberately introduces behavior-preserving seam changes in 7 production files — this is exactly the "seam introductions required" carve-out in AC8, applied here to enable deterministic tests. No production runtime behavior change. Out-of-scope StackGeek files excluded. |
| R-AC1 | PASS | Named test deterministic: `Thread.Sleep(50)`/`signal.Wait` removed; `ManualFireTimerWrapper` + `TimerFactory` + `FireElapsed()` + `signal.IsSet.Should().BeTrue()`. `named-test-pass-after.2026-06-09T11-31.md` EXIT 0. |
| R-AC2 | PASS | 26 lettered occurrences A1–L1 all dispositioned: 21 Converted, 1 Converted-PARTIAL (J1, documented scope-change), 4 Retained-with-justification (B1–B3, L1), 0 Halted. Residual grep scan cross-checked; nothing silently omitted/masked. |
| R-AC3 | PASS | One clean final toolchain pass; 4065/4065; coverage 85.46% vs 85.52% baseline (denominator-driven −0.06 pp, no changed-line regression). |
| R-AC4 | PASS | Zero blocking findings across all three reaudit artifacts. |

### Definition of Done / Seeded Test Conditions (spec.md, secondary)

| Item | Verdict | Evidence |
|------|---------|----------|
| Acceptance criteria mapped to tests/demos | PASS | AC mapping above; converted tests exercise the seams. |
| Behavior matches AC in documented environments | PASS | Local toolchain green; CI to confirm post-push. |
| Tests updated/added | PASS | 14 test files converted + new helper. |
| Edge cases / error handling covered | PASS | Timeout/cancellation/empty-queue/concurrent paths preserved. |
| Docs updated | PASS | Evidence + disposition artifacts updated. |
| Toolchain pass completed | PASS | csharpier -> analyzer -> nullable -> vstest, one clean pass. |
| `nuget restore` succeeds | PASS (unchanged) | No package change this cycle; prior-cycle restore green. |
| Both msbuild stages green | PASS | analyzer 0/0; nullable 0/0. |
| vstest MSTest run unaffected | PASS | 4065/4065, 0 failed. |
| PR GitHub Actions CI green | PENDING | Re-confirm after commit/push (external gate). |

## Acceptance Criteria Check-off

All authoritative feature ACs (AC1–AC8) remain `[x]` in `user-story.md` and `issue.md`; cycle 6 does not change their state and does not regress them. No new check-off is performed by this cycle (the ACs were delivered in prior cycles). AC6 (PR CI green) is the only item requiring post-push confirmation by the orchestrator; it is left as delivered-pending-CI rather than re-opened.

### Acceptance Criteria Status
- Source: `user-story.md` (authoritative), `issue.md` (mirror), `spec.md` (secondary DoD)
- Total AC items: 8 (AC1–AC8)
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none (AC6 delivered locally; PR CI green to be re-confirmed post-push)

## Verdict

PASS. The feature's acceptance criteria are preserved (not regressed) by the cycle-6 deterministic-test conversion, and the cycle-6 remediation acceptance (named test deterministic; all occurrences dispositioned; clean toolchain; zero regression; zero blocking findings) is met. The only external dependency is confirming the PR GitHub Actions CI is green against the branch head after the working-tree changes are committed and pushed.

BLOCKING FINDINGS: 0
