# Remediation Plan — outlook-startup-ui-thread-deblock (Issue #141)

- **Issue:** #141
- **Branch:** `bug/outlook-startup-ui-thread-deblock-141`
- **Base Branch:** `development`
- **Last Updated:** 2026-05-06
- **Status:** Draft remediation plan generated from review findings
- **Work Mode:** `full-bug`
- **Requirements Sources:** `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/spec.md`, `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/remediation-inputs.2026-05-06T20-33.md`
- **Supporting Context:** `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`, `artifacts/orchestration/orchestrator-state.json`, `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/policy-audit.2026-05-06T20-33.md`, `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/code-review.2026-05-06T20-33.md`, `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/feature-audit.2026-05-06T20-33.md`

## Objective

Close the review blockers for issue `#141` by restoring policy-compliant coverage, reconciling any out-of-scope branch changes, validating retained PowerShell tooling changes if they remain in scope, and completing the deferred manual Outlook validation on a coverage-pass build.

### Phase 0 — Inputs and Scope Reconciliation

- [ ] [P0-T1] Read the current policy inputs, `remediation-inputs.2026-05-06T20-33.md`, the latest review artifacts, the active plan, and the latest blocked-path QA artifacts, then write a remediation baseline evidence note under `evidence/remediation-baseline/`.
	- Acceptance: The evidence note records `Timestamp:`, the policy-read order, the review artifacts read, the latest blocking coverage artifact path, and the current work-mode resolution.
- [ ] [P0-T2] Compare the current branch diff against `implementation-scope.2026-05-05T09-23-00.md` and either remove/split extra files from the branch or update scope-control evidence to promote any truly required additions.
	- Acceptance: A remediation scope artifact records the exact retained out-of-scope files or confirms they were removed; no unresolved scope drift remains undocumented.

### Phase 1 — Coverage and Contract Remediation

- [ ] [P1-T1] Raise changed-line coverage for `TaskMaster/AppGlobals/ApplicationGlobals.cs` and `TaskMaster/AppGlobals/AppOlObjects.cs` to the policy threshold using deterministic MSTest additions and only the minimal production changes required by the approved bug-fix scope.
	- Acceptance: The latest coverage summary records changed/new-code coverage `>=90%` and no remaining uncovered hotspot in those two files that is attributable to the branch delta.
- [ ] [P1-T2] Reconcile the `StoresWrapper` completion contract by either removing the retained `[OnDeserialized] async void RewireOlObjects(...)` path or proving with tests and documentation why the callback must remain alongside `RewireAfterDeserializeAsync()`.
	- Acceptance: The final branch has one explicitly supported completion story, and store-rewire tests cover the retained behavior.
- [ ] [P1-T3] If the `scripts/vscode/*.ps1` changes remain in scope after [P0-T2], update them as needed to satisfy repo PowerShell policy without introducing unrelated tooling work.
	- Acceptance: The retained scripts are ready for the PowerShell QA loop in Phase 2, and no unmanaged script scope drift remains.

### Phase 2 — Multi-Language QA Loop

- [ ] [P2-T1] Run `dotnet tool run csharpier format .` and record a clean remediation QA artifact.
	- Acceptance: The formatter exits `0` and produces no remaining tracked changes for the final pass.
- [ ] [P2-T2] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild` and record the result.
	- Acceptance: The analyzer-enabled build exits `0`.
- [ ] [P2-T3] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors` and record the result.
	- Acceptance: The nullable/type-check build exits `0`.
- [ ] [P2-T4] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` and record the result.
	- Acceptance: The MSTest coverage run exits `0` with all tests passing.
- [ ] [P2-T5] Write an updated C# coverage summary comparing baseline vs remediation end-state coverage.
	- Acceptance: The summary records `Coverage Conclusion: PASS`, repo coverage is non-regressing relative to the branch baseline, and changed/new-code coverage meets the required threshold.
- [ ] [P2-T6] If PowerShell scripts remain in scope, run the repo-required PowerShell formatter step using the approved PoshQC entry point and capture evidence.
	- Acceptance: The PowerShell format step is recorded with success or any required restart behavior.
- [ ] [P2-T7] If PowerShell scripts remain in scope, run the repo-required PSScriptAnalyzer step using the approved PoshQC entry point and capture evidence.
	- Acceptance: The PowerShell analyze step is recorded with success and no unresolved findings.
- [ ] [P2-T8] If PowerShell scripts remain in scope, run the repo-required Pester step using the approved PoshQC entry point and capture evidence.
	- Acceptance: The PowerShell test step is recorded with success.

### Phase 3 — Manual Validation and Review Closeout

- [ ] [P3-T1] Run the PASS-path manual Outlook startup validation on a representative multi-store profile and write the required `outlook-manual-validation.*.md` artifact.
	- Acceptance: The artifact records responsiveness, timing evidence, and explicit checks for the prior COM-safety fixes from issues `#124`, `#126`, and `#128`.
- [ ] [P3-T2] Update the full-bug end-state artifact to the PASS path using the completed remediation QA evidence and manual validation artifact.
	- Acceptance: `Ready For Validator: true` is justified by present artifacts and a passing coverage summary.
- [ ] [P3-T3] Refresh the feature review artifacts after remediation and confirm whether the branch is ready to merge.
	- Acceptance: Updated review artifacts exist, remediation-trigger findings are closed or explicitly reduced to non-blocking items, and merge readiness is stated unambiguously.
