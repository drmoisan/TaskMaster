# Remediation Inputs: remaining-stale-binding-redirect-pairs (Issue #973)

- Timestamp: 2026-10-06T19-30 (assigned label; see the timestamp derivation in policy-audit.2026-10-06T19-30.md)
- Branch: bug/remaining-stale-binding-redirect-pairs-973, head 9b0d5421707aa9a192afc6ad3700a4e9667f0744
- Source artifacts: policy-audit.2026-10-06T19-30.md, code-review.2026-10-06T19-30.md, feature-audit.2026-10-06T19-30.md (all in docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/)

Review-Verdict: REMEDIATION_REQUIRED

## Derivation

- Counted findings: 2 (B-1 and B-2 below).
- Findings of class autonomous: 1 (B-1), so the verdict is REMEDIATION_REQUIRED.
- Findings of class human_decision_required: 1 (B-2); it does not change the verdict while an autonomous finding exists. Once B-1 is closed and no autonomous finding remains, the verdict becomes HALT_NON_REMEDIABLE until the maintainer records the AC18 evidence.
- Findings of class external_dependency, policy_hold or awaiting_ci: 0.
- No change to any code, configuration, manifest or project file is required. B-1 is a specification-text amendment plus a re-verification and check-off; B-2 is a manual observation by the maintainer.

## Counted findings

### B-1: AC17's Azure.Core clause asserts a bind the add-in process never performs

Severity: Blocking
Remediability: autonomous
Remediability-Evidence: the remedy is a specification amendment and a check-off, both agent-executable in the item worktree; this item already carries two orchestrator-ruled amendments of acceptance-criteria text (spec.md Planner Amendments 4 and 5, applied under orchestrator rulings during preflight), so the route is established; the delivered values are already correct and no code, config or build change is needed; the facts the amended criterion must state are recorded in evidence and re-verified in this review.
File: docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/spec.md, line 373 (`- [ ] AC17`); the same premise at spec.md Proposed Fix trace step 1 ("Microsoft.Kiota.Authentication.Azure 2.1.2, installed in UtilitiesCS and deployed beside TaskMaster.dll") and step 4; origin research 1 section 5.2
Rule: spec.md acceptance criterion AC17; acceptance-criteria-tracking skill (evidence before check-off; criterion text amended only by planning or scoping agents, recorded in the Scope Amendment Log)
Evidence: evidence/qa-gates/p5-t17-ac17-checkoff.2026-10-06T18-36.md (TaskMaster/app.config lines 90-91 and 218-219 at the corrected values; Test-Path probes: Azure.Core.dll False and Microsoft.Kiota.Authentication.Azure.dll False in TaskMaster\bin\Debug, both True only in UtilitiesCS\bin\Debug; TaskMaster/TaskMaster.csproj carries no Azure.Core Reference); the caller-reported System.Reflection.Metadata read of UtilitiesCS/bin/Debug/UtilitiesCS.dll listing no Microsoft.Graph, Microsoft.Kiota or Azure assembly reference; this review's Grep over UtilitiesCS/**/*.cs and TaskMaster/**/*.{cs,csproj} for Microsoft.Graph, Azure., Kiota, GraphServiceClient and TokenCredential returning 0 occurrences (before Part F the only occurrences were the six deleted `using` directives, which bind nothing and emit no assembly reference); UtilitiesCS/packages.config line 47 (Microsoft.Kiota.Authentication.Azure 2.1.2 installed in UtilitiesCS). Mechanism: csc emits an assembly reference only for assemblies whose types are used, and MSBuild copies a ProjectReference's dependencies from the referenced assembly's metadata, so the Graph, Kiota and Azure family is never copied beside TaskMaster.dll and no Azure.Core request arises in the add-in process. The System.Linq.AsyncEnumerable half of AC17 is observed and holds.
Required action (orchestrator, planner or prd-feature agent): amend AC17 so that its Azure.Core clause states the observable facts and nothing more, for example: the Azure.Core block in TaskMaster/app.config and in every test-host config reads oldVersion 0.0.0.0-1.63.0.0 / newVersion 1.63.0.0, the one version every csproj Reference declares; Azure.Core 1.63.0 and its requester Microsoft.Kiota.Authentication.Azure 2.1.2 are deployed to UtilitiesCS\bin\Debug and the nine test outputs, where the corrected configs now redirect the 1.50.0.0 request to the deployed file; no assembly deployed beside TaskMaster.dll references Azure.Core, so the add-in process issues no such request and the TaskMaster/app.config block is correct and inert. Correct the Proposed Fix trace steps 1 and 4 to the same facts. Record the amendment in the Scope Amendment Log with the version 1.1 wording it replaces and the evidence paths above; then re-verify the amended criterion against TaskMaster/app.config, one test-host config, the AC12 artifact and the P5-T17 probes, write the check-off artifact, change `- [ ] AC17` to `- [x] AC17`, check off plan task P5-T17, and commit from the item worktree. If the orchestrator prefers maintainer ratification of the amendment, reclassify this finding as human_decision_required; the delivered code needs no change either way.

### B-2: AC18 manual designer-load and add-in-start verification has not been performed

Severity: Blocking
Remediability: human_decision_required
Remediability-Evidence: the criterion requires a human-operated Visual Studio session (the WinForms designer runs in-process in devenv.exe and applies no project app.config) and a live Outlook start under the debugger with inspection of the add-in's session log; no agent route exists in this repository, and the caller designated this class.
File: docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/spec.md, line 374 (`- [ ] AC18`); runbook docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/runbooks/verify-designer-and-addin-load.runbook.md
Rule: spec.md acceptance criterion AC18; issue.md Proposed Fix item "Re-test the #418 designer path for PictureBoxSVG after the sweep"; human-exception-runbook skill contract
Evidence: evidence/other/p5-t18-ac18-pending-manual.2026-10-06T18-43.md (AC18: PENDING-MANUAL; no designer or add-in observation made; no designer-load-*.md file under evidence/regression-testing, confirmed by Glob in this review); the runbook is present and contract-conformant (Cue, Prerequisites, Step-by-step Instructions, Verification, Source and Citation; Part A designer load, Part B add-in start with the log search and the System.Linq.AsyncEnumerable.dll presence check).
Required action (maintainer): on branch head 9b0d5421707aa9a192afc6ad3700a4e9667f0744 (which contains commit B ab801d2bd392cf2c9b6bc577431dbebc0606dd2d), with Outlook and Visual Studio closed, restore and /t:Rebuild in Debug Any CPU, then run runbook Parts A and B and write docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md with the fields the runbook lists, the Part A insensitivity statement and EXIT_CODE 0 on Pass. Note for the log search: the Microsoft.Graph, Kiota and Azure family is not deployed to TaskMaster\bin\Debug (B-1), so a FileNotFoundException naming one of those assemblies would indicate a pre-existing deployment condition rather than a redirect value. On Pass, change `- [ ] AC18` to `- [x] AC18` in the item's own worktree and commit from there.

## Optional items for the same cycle (not counted)

- CR-1 (Minor, plan text; autonomous): plan.2026-10-02T22-16.md line 424, P0-T20 expects Grep `ConditionalEngine` over UtilitiesCS/UtilitiesCS.csproj to count 0; the pattern also matches the pre-existing `Interfaces\IGlobals\IConditionalEngine.cs` Compile item, so the count is 1. Amend to the narrower pattern `CategoryClassifierGroup\.ConditionalEngine` and add a 1.7 revision-log entry. No gate consumed the literal count.
- Caller item 4 (unquoted pathspecs) is corrected in code-review observation O-1: the plan quotes every wildcard pathspec; the unquoted forms were executor-issued and each result was corroborated by a quoted re-capture. No plan-text remedy is owed.

## Non-blocking findings carried for reference (no remediation owed in this item)

- CR-2: tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 line 315 asserts a literal defined two lines earlier (informational; keep as a guard or convert to a comment).
- CR-3: pre-existing sub-floor per-file coverage in UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs (66.91% lines after the split, 72.01% before; the type aggregate 229/318 lines and 45/58 branches is identical at both stages), Triage/Triage_OlLogic.cs (68.97% / 54.17%, unchanged) and OutlookObjects/Store/StoreWrapper.cs branches (70%, unchanged); no changed line carries a sequence point; the spec excludes refactor or test additions for the moved members. Follow-up candidate.
- O-2: canonical artifacts/csharp/coverage.xml absent in the worktree; committed projections and the local raw documents used under the standing ruling (recurring).
- O-3: canonical artifacts/pester/powershell-coverage.xml reads 0 covered of 9294 and instruments only .claude/ and .codex/; the policy audit carries a FAIL row on that artifact beside a vacuous PASS on changed lines; the scripts/dependencies figure comes from the CI Pester job and the module is unchanged (recurring; #928 promotion candidate P-4).

## Unrelated defects to file

- Coverage uplift for the classifier build path (CategoryClassifierGroup.BuildClassifiersAsync, LoadClassifierGroup, LoadStagingData) and Triage_OlLogic, if not already tracked (CR-3).
- The bundled PoshQC test route's coverage document never instruments scripts/ (O-3), if not already tracked.
