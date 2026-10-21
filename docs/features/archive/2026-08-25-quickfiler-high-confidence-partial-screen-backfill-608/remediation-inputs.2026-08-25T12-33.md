# Remediation Inputs — cycle 1 (R1), Issue #608

Timestamp: 2026-08-25T12-33
Canonical issue number for this feature is 608.
Feature folder: `docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608`
Original plan: `docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/plan.2026-08-25T11-53.md`
Original plan state: executed through P4-T2; blocked at P4-T3. The original plan and its chronology are immutable.

## Trigger and verified evidence

The hard-locked original plan required P4-T3 to run:

`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`

That command exited 1 with 195 diagnostics because global `Nullable=enable` applies nullable analysis to legacy projects outside Issue #608's one-production-file and one-test-file scope. The failure is recorded at:

`docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/qa-gates/csharp-nullable.2026-08-25T12-33.md`

This is not a passing result, a hook failure, or authorization to repair unrelated diagnostics.

## Policy reconciliation

The generated `AGENTS.md` source prescribed `/p:Nullable=enable`. The executable/current C# QA contract conflicts with that instruction:

- `.agents/skills/csharp/SKILL.md` requires a local nullable/type gate of `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` and states that `/p:Nullable=enable` must not be passed for this legacy solution.
- `.agents/skills/csharp-qa-gate/SKILL.md` repeats the same type/nullable command and prohibition.
- `scripts/vscode/Invoke-VSBuild.ps1` is the current executable repository helper and follows the per-file nullable model.

The remediation cycle must use the current C# QA contract without weakening the gate or suppressing diagnostics.

## Required remediation work

1. Preserve the original plan, completed task checkboxes, execution-sequence-deviation receipt, and failed global-nullable evidence without modification.
2. Reconcile the policy conflict in remediation evidence. Do not alter generated `AGENTS.md`, repository policies, source code, test code, project files, or configuration as part of that reconciliation.
3. Starting from the current implementation, run the full C# QA loop in this exact order and record each result under the Issue #608 feature evidence directories:
   1. CSharpier format and read-only check.
   2. Analyzer rebuild with `EnableNETAnalyzers=true` and `EnforceCodeStyleInBuild=true`.
   3. Type/nullable rebuild with `TreatWarningsAsErrors=true` and without `/p:Nullable=enable`.
   4. Coverage-enabled MSTest execution.
4. Compare final analyzer, compiler/nullable, test, and coverage results against the original baseline. Do not accept a regression.
5. Preserve and verify the Issue #608 seven-item and eight-item deterministic regressions, source exhaustion, zero-accepted deadline, cancellation, order, discard, and inclusive-cutoff evidence.
6. Complete remaining Issue #608 acceptance-criteria tracking only after evidence is verified, then prepare the branch for feature review, PR context, PR authoring, and CI.

## Do not do

- Do not repair or suppress any of the 195 unrelated legacy nullable diagnostics.
- Do not use `/p:Nullable=enable` in this remediation cycle.
- Do not revise or reopen the original executed plan.
- Do not weaken `TreatWarningsAsErrors`, coverage, analyzer, test, ordering, cancellation, source-exhaustion, #233, #424, or #446 requirements.
- Do not modify controllers, datamodel APIs, result types, settings, configuration, or the separate #446 epic worktree.
- Do not add manual validation, external dependencies, or out-of-scope production/test files.

## Exit criteria

The remediation plan can exit only after its preflight is clear, the revised full C# QA loop passes with zero regression from baseline, all remaining applicable Issue #608 acceptance criteria are verified, and subsequent feature review has zero blocking findings.
