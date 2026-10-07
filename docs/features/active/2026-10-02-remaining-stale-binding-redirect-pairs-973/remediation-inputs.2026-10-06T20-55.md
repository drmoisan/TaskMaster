# Remediation Inputs: remaining-stale-binding-redirect-pairs (Issue #973), re-audit at the exit of remediation cycle 1

- Timestamp: 2026-10-06T20-55 (assigned label; see the timestamp derivation in policy-audit.2026-10-06T20-55.md)
- Branch: bug/remaining-stale-binding-redirect-pairs-973, head 0e0d7122b23a8d149c6165c88ccea62badaca69a
- Source artifacts: policy-audit.2026-10-06T20-55.md, code-review.2026-10-06T20-55.md, feature-audit.2026-10-06T20-55.md (all in docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/)
- Prior inputs: remediation-inputs.2026-10-06T19-30.md (B-1 autonomous, closed in cycle 1; B-2 human_decision_required, carried below)

Review-Verdict: HALT_NON_REMEDIABLE

## Derivation

- Counted findings: 1 (B-2 below).
- Findings of class autonomous: 0. B-1 of the prior review (AC17 false premise) is closed: spec.md Planner Amendment 6 replaced the false clause with four fail-capable observations, each was run and recorded (evidence/qa-gates/r1-p1-t1 to r1-p1-t4), the check-off artifact evidence/qa-gates/p5-t17-ac17-checkoff.2026-10-06T20-34.md records AC17 MET as amended, and this review re-verified observation (i) and the presence half of (iv) directly from the worktree. The optional CR-1 and the ruled-in CR-2 are also closed.
- Findings of class human_decision_required: 1 (B-2). With no autonomous finding and one human_decision_required finding the verdict is HALT_NON_REMEDIABLE.
- Findings of class external_dependency, policy_hold or awaiting_ci: 0.
- No change to any code, configuration, manifest or project file is required. B-2 is a manual observation by the maintainer followed by a check-off in the item worktree.

## Counted findings

### B-2: AC18 manual designer-load and add-in-start verification has not been performed

Severity: Blocking
Remediability: human_decision_required
Remediability-Evidence: the criterion requires a human-operated Visual Studio session (the WinForms designer runs in-process in devenv.exe and applies no project app.config) and a live Outlook start under the debugger with inspection of the add-in's session log; no agent route exists in this repository; the caller designated this class and the remediation plan excluded the criterion from cycle 1 by the orchestrator's ruling.
File: docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/spec.md, line 376 (`- [ ] AC18`); runbook docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/runbooks/verify-designer-and-addin-load.runbook.md
Rule: spec.md acceptance criterion AC18; issue.md Proposed Fix item "Re-test the #418 designer path for PictureBoxSVG after the sweep"; human-exception-runbook skill contract
Evidence: evidence/other/p5-t18-ac18-pending-manual.2026-10-06T18-43.md (AC18: PENDING-MANUAL; no designer or add-in observation made); evidence/other/r1-p3-t6-ac-status.2026-10-06T20-42.md (AC18 the one remaining item); no designer-load-*.md file under evidence/regression-testing (Glob in this review: 13 Markdown files, none so named); the runbook is present and contract-conformant (Cue, Prerequisites, Step-by-step Instructions, Verification, Source and Citation; Part A designer load, Part B add-in start with the log search and the System.Linq.AsyncEnumerable.dll presence check).
Required action (maintainer): on branch head 0e0d7122b23a8d149c6165c88ccea62badaca69a (which contains commit B ab801d2bd392cf2c9b6bc577431dbebc0606dd2d; the only code change after the base review is the one-line test-file fold), with Outlook and Visual Studio closed, restore and /t:Rebuild in Debug Any CPU, then run runbook Parts A and B and write docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md with the fields the runbook lists, the Part A insensitivity statement and EXIT_CODE 0 on Pass. Note for the log search: the Microsoft.Graph, Kiota and Azure family is not deployed to TaskMaster\bin\Debug (AC17 observation ii), so a FileNotFoundException naming one of those assemblies would indicate a pre-existing deployment condition rather than a redirect value. On Pass, change `- [ ] AC18` to `- [x] AC18` in the item's own worktree and commit from there; the same commit may restate the two stale plan Status lines (code-review N-1).

## Non-blocking findings carried for reference (no remediation owed in this item)

- N-1: remediation-plan.2026-10-06T19-30.md line 7 and plan.2026-10-02T22-16.md line 8 Status lines describe a state the cycle superseded (all 20 and all 108 tasks are checked); text only, optional, suggested alongside the AC18 check-off commit.
- CR-3: pre-existing sub-floor per-file coverage in UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs (66.91% lines after the split; the type aggregate 229/318 lines and 45/58 branches identical at both stages), Triage/Triage_OlLogic.cs (68.97% / 54.17%, unchanged) and OutlookObjects/Store/StoreWrapper.cs branches (70%, unchanged); no changed line carries a sequence point; unchanged by the cycle. Follow-up candidate.
- O-1: the folded assertion compares counts; if `$expectedDebt` were ever re-populated the comparison would not check the recorded pairs' identity. The rule in its `-Because` forbids re-recording; no action owed.
- O-2: canonical artifacts/csharp/coverage.xml absent in the worktree; committed projections and the local raw documents used under the standing ruling (recurring).
- O-3: canonical artifacts/pester/powershell-coverage.xml, rewritten by the cycle's P3-T3 run, reads 0 covered of 9294 and instruments only .claude/ and .codex/; the policy audit carries a FAIL row on that artifact beside a vacuous PASS on changed lines; the scripts/dependencies figure comes from the CI Pester job and the module is unchanged (recurring; #928 promotion candidate P-4).

## Unrelated defects to file

- Coverage uplift for the classifier build path (CategoryClassifierGroup.BuildClassifiersAsync, LoadClassifierGroup, LoadStagingData) and Triage_OlLogic, if not already tracked (CR-3).
- The bundled PoshQC test route's coverage document never instruments scripts/ (O-3), if not already tracked.
