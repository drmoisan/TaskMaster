# Code Review: EFC Enter confirmation and native workflow repair (#995)

Review Date: 2026-10-17T23-41 UTC
Reviewer: feature-reviewer-c3
Feature Folder: docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995
Feature Folder Selection Rule: changed spec and branch issue suffix identify #995.
Base Branch: main, merge-base 438436893d2e7500057b8a4f871e23a702ab9b34 (2026-10-17T11:45:54-05:00).
Head Branch: bug/efc-viewer-enter-confirmation-995, c4a92b986c2951ad933cfe231643d599e2605626.
Review Type: independent full branch review; all169files, including inherited changes.
Template: MCP code-review-template resolved/copied before authoring.
FEATURE denotes the feature folder; QA denotes FEATURE/evidence/qa-gates.

## Executive Summary

No Blocker or Major finding. The typed confirmation message validates the current retained destination and invokes the existing OK validation/error boundary. Selection alone remains selection-only. Named subscription ownership detaches old handlers on reconfiguration/cleanup.

The inherited native-model repair aligns62profiles, routing JSON, resolver, forced epics, MCP pin and six workflow references. Authority stream defaults and rejection behavior remain intact. Current independent read-only checks and full final QA evidence support local review PASS.

Top3limits: renderer tests inspect JavaScript text; broad PoshQC remains EXIT1 on13unchanged baseline findings; remote CI awaits draft PR creation. Live WebView/Outlook was not exercised and is not required by ACs.

PR readiness recommendation: Go for draft PR creation and exact-head CI. Local review PASS does not claim merge readiness.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Minor | FEATURE/evidence/baseline/csharp-package-restore.2026-10-17T22-55.md; FEATURE/evidence/regression-testing/issue-995-red-build.2026-10-17T23-02.md | Restore8,13,18; red-build16 | Four copied-output lines have trailing whitespace. | Trim spaces during evidence cleanup; preserve substantive output. | Cosmetic documentation issue, no behavior or required-gate failure. | git diff --check main...HEAD, EXIT2. |
| Minor | FEATURE/spec.md | Status metadata, line7 | Says final QA in progress despite completed QA narrative. | Update metadata when recording delivery state. | Editorial inconsistency;8ACs have passing evidence. | Spec Status and Acceptance Criteria sections. |
| Info | QA/powershell-analyze-mcp.2026-10-17T23-23.md | Output Summary | Broad scan EXIT1 has10warnings/3info, unchanged. | Retain raw result and zero-delta comparison under root disposition. | Existing unchanged-script debt is not a new regression. | Comparison.2026-10-17T23-23.md: delta0; current changed-scope findings0. |

No Blockers or Major findings. Minor documentation findings do not trigger remediation.

## Implementation Audit

### C# implementation audit

- Bridge Enter consumes the event, rejects Alt/Ctrl/Shift/Meta/repeat, and posts one rowConfirmed only with a selected ID.
- Typed codec rejects empty/nonstring/malformed IDs. Router resolves the row and ConfirmRow (Selection.cs:153) checks nonbanner/current selected identity/valid retained path.
- Confirmation does not reselect, preserving activated ancestor/child destinations. Existing rebinding clears selected identity/path and rejects stale confirmation.
- ConfigureBreadcrumbConfirmation (Breadcrumb.cs:30) defaults to ButtonOK_Click and detaches before replacing the router. Cleanup detaches before releasing collaborators. ButtonOkClickAsync/ActionOkAsync preserve UI context, guarded filing and error reporting.
- Additive discriminator/event documentation is present. No new filing pipeline, direct bridge ActionOkAsync call or new runtime dependency.

### PowerShell implementation audit

- Resolver model changes agree with routing JSON: Luna/low, Sol/medium, Sol/high, elevated Astra/high, Astra/max; forced epics Astra/ultra.
- Attestation wrapper exposes narrow console/checkpoint/HEAD/clock seams. Default exclusive CreateNew/Open access, durable Flush(true), disposal and single-use root consumption remain.
- Provenance/routing markers and exit2 errors retain existing entry behavior. Tests use fixed payloads/MemoryStream collaborators and matching mock signatures.
- Hash/path rejection tests are scoped to the profile helper; the hook does not newly supply its optional hash arguments. No claim of new hook hash enforcement.

### Configuration and workflow audit

Independent62/62profile model/effort parity passed; TOML/JSON parse evidence is present. Genuine protocol validation confirms server1.1.21 and native checkpoint support. Reviewer launch attestation routing_valid/launch_authority_valid are true and profile SHA256 matches. Historical receipts remain historical.

All six changed skills and inherited changes were reviewed; gates remain enforced. Copied environment CI text was not used as TaskMaster command authority; actual wrappers and _pester.yml govern verification. Separate FolderBreadcrumb.html is unchanged.

## Test Quality Audit

Red renderer regression failed before production edits;232targeted and7,471full C# cases then passed. PowerShell513passed, zero failed/skipped; seeded runs995/1995/2995 each pass50new cases. Tests isolate invalid/stale selections, retained targets, lifecycle/error boundaries and authority rejection without live services or temporary files.

C#coverage86.0212% ->86.0331%; expanded PowerShell84.0257% ->94.9465%; every new executable unit>=90%, no file/changed-line regression. Constant asset0/0,N/A is justified by instrumentation/source proof and renderer red/green.

Independent checks: CSharpier1,652files EXIT0; changed four PowerShell files drift0/findings0 EXIT0; profile parity62/62 and executable size<=500 PASS; genuine MCP1.1.21 checkpoint ok=true. Full current QA evidence was reused; no duplicate full toolchain run.

Evidence: QA final C#format23-13/analyzers23-14/nullable23-14/tests23-15; PowerShellformat23-22/scoped analyze+compatibility23-23/MCP tests23-25/coverage23-26; coverage-comparison23-27; regression red23-03/green23-09 and isolation23-28. Exact commands: companion policy-audit Appendix B.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets introduced | PASS | Full diff and hygiene Findings0 |
| Safe command construction | PASS | Argument-separated default HEAD invocation and injected test boundary |
| Input validation | PASS | Codec/current row/retained destination guards |
| Explicit errors | PASS | Existing OK fault sink and hook stderr/exit2 |
| Configuration/path safety | PASS | Profile parity, exclusive I/O and durable consumption tests |
| Lifecycle/resource ownership | PASS | Reconfigure/cleanup/disposal cases |
| Remote required CI | Pending downstream | No PR exists; coordinator owns draft PR and exact-head CI |

## Research Log

Canonical PR summary was read first; appendix supplies baseline/commit/file scope. Local source/diff supplied exact anchors; current QA artifacts supplied execution evidence. No external research was needed.

## Verdict

CODE_REVIEW_STATUS: PASS
REVIEW_STATUS: PASS
REMEDIATION_INPUTS: NONE
REMEDIATION_PLAN: NONE

Go for draft PR creation. No blocking new defect or unmet required AC found. Preserve baseline analyzer disclosure and static-test limitation; verify all remote required checks at the eventual PR head before final delivery.
