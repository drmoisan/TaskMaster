# Issue 995 single-pass preflight

Timestamp: 2026-10-17T22-51
Command: validate_orchestration_artifacts artifact_type=plan artifact_path=docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/plan.2026-10-17T13-44.md
EXIT_CODE: 0
Output Summary: Native MCP plan schema validation returned ok=true. Source/tooling review requires one planner revision for the substantive defects below. No plan task, formatter, test or production edit was executed.

## Attestation

Actual and expected model gpt-6.1-sol; effort high; profile .codex/agents/atomic-executor-c3.toml; SHA256 2460c2bdf20fa8b88136c16492ee3a810441f60be697432b1975882e8832d46a. Runtime attestation agent 01a14c0e-66f5-7150-bcbd-ff5e36e57f65 reports routing_valid, launch_authority_valid and provenance_valid true. Current profile hash matches persisted issue-995-preflight-1 receipt.

## Trivial fix applied

PREFLIGHT-FIX: P4-T2 | stale document path | Removed user-story.md from the update inventory because full-bug requirements use existing spec.md and the optional user-story file is absent.

## Substantive defects and precise plan deltas

PREFLIGHT-DEFECT: P2-T4 | wrong file set | Add QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs Cleanup to the task file inventory. Detach the named confirmation handler before controller collaborators are cleared and detach old router wiring before replacement. Preserve existing cleanup assertions and P1-T5 must prove old router callbacks are inactive after Cleanup and reconfiguration.

PREFLIGHT-DEFECT: P0-T6, P5-T4 and Scope and execution contract | report-retention design | Invoke-MSTestWithCoverage.ps1 deletes raw Cobertura outside the exact repository coverage directory via Test-RawCoverageDocumentRetained in Invoke-MSTestWithCoverage.Projection.ps1. Its surviving JaCoCo projection contains package counters only. Authorize transient ignored coverage output, run the canonical wrapper there, copy the complete raw report, projections and available results to issue-prefixed SCRATCH destinations immediately, and remove only verified task-created transient outputs after preservation. Measure per-file, changed-line and method coverage from the preserved complete report.

PREFLIGHT-DEFECT: P3-T2 | wrong asserting seam | Test explicit incorrect-hash rejection directly on the existing real Test-CodexAgentProfileBinding helper with ExpectedProfileSha256. Get-CodexSubagentAttestation does not pass expected receipt path/hash into that helper; distinguish this existing helper contract from the hook's existing model/effort checks and do not add unplanned hook hash enforcement. Retain actual hook wrapper, stream and provenance tests.

PREFLIGHT-DEFECT: P0-T6, P5-T4 and P5-T8 | impossible numeric coverage acceptance | BreadcrumbDocumentAssets.cs contains compile-time const strings only and has no instrumentable executable sequence points. Require covered/valid counts 0/0 and justified N/A percentage only for proven compiler-elided constant-only files, supported by source/report inspection and renderer regression evidence. Keep numeric percentage and all coverage floors for every executable production file/new unit; do not fabricate 0 percent or 100 percent or change instrumentation exclusions.

## Checks completed

- Reviewed all 37 tasks in order against spec.md eight sole-source criteria, actual EFC source, legacy Compile inventory and test families.
- Confirmed root dotnet-tools.json pins CSharpier 1.2.6; actual solution wrappers expose requested analyzer/nullable Rebuild options.
- Confirmed actual Pester 5.6.1 CI discovery and coverage populations match the plan's three directories; copied upstream-only generators/scripts are absent and not executable prerequisites.
- Confirmed existing router selection retention/rebinding, codec required-string contract, controller existing OK error boundary, actual Cleanup ownership and test seams.
- Confirmed affected existing files have room under 500 physical lines; new files remain bounded by final contract task.
- Confirmed raw Cobertura retention and package-only projection behavior by source inspection.
- No implementation tasks or acceptance criteria were checked off; next task remains P0-T1 after revised-plan clearance.

PREFLIGHT: REVISIONS REQUIRED
BLOCKED at preflight (before [P0-T1]). Return the exact plan to atomic-planner once with all four defects, then perform one confirming preflight.
