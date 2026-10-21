# Issue 995 p14-acceptance-criteria-checkoff

Timestamp: 2026-10-17T23-34
Command: Verify eight spec.md criteria against canonical regression, final toolchain, coverage, protocol and hygiene evidence; compare unchanged criterion text
EXIT_CODE: 0
Output Summary: Work Mode: full-bug, read from issue.md. Sole authoritative source: spec.md, ## Acceptance Criteria. Intake issue.md checkboxes are historical traceability and are not a second AC source. No user-story.md is applicable.

| # | Exact criterion | Verification and evidence |
|---|---|---|
| 1 | One plain, nonrepeated Enter press with a valid highlighted current destination requests the existing guarded OK action exactly once, and a row-selection message alone does not request OK. | PASS: the renderer red/green proof and ValidSelectedRow_ConfirmsExactlyOnce, RowSelected_DoesNotConfirm, and controller boundary cases establish one request through existing OK. Full C# final: 7,471/7,471. |
| 2 | Missing, unknown, banner, stale, or invalid destinations never file an email through confirmation; the existing OK validation remains effective. | PASS: codec required-row-ID negatives and router missing/unknown/banner/mismatched/invalid/rebind cases reject invalid confirmations; controller existing OK validation/error boundary remains in place. Targeted green and final C# coverage evidence. |
| 3 | Confirmation after ancestor-segment or rendered-child activation retains that activated destination, and list rebinding prevents stale selection confirmation. | PASS: ConfirmAfterSegmentActivation_PreservesTarget and ConfirmAfterChildActivation_PreservesTarget verify retained targets without SelectRow; RebindThenConfirm_DoesNotUseStaleSelection verifies invalidation. Targeted green and final C# evidence. |
| 4 | Modified Enter and repeated keydown events do not request confirmation; existing arrow navigation, focus transfer, row selection, breadcrumb activation, and cancellation behavior remain unchanged. | PASS: Issue995BridgeJsPostsConfirmationForPlainEnterAndScrollsSelectedRowIntoView asserts modifier/repeat exclusion and event consumption in generated text; existing navigation, focus, activation and controller cases remain passing. Static JavaScript assertions are not a browser execution claim. |
| 5 | A deterministic Enter regression fails before the production fix and passes afterward; codec, router, and controller boundary tests cover the positive, invalid, stale-selection, retained-target, and exception scenarios with no live Outlook/WebView or temporary-file dependencies. | PASS: issue-995-enter-red declares ExpectedExitCode:1 before the product edit; issue-995-csharp-green passes, followed by all 7,471 final tests. New tests use in-memory collaborators, Moq and FluentAssertions without live Outlook/WebView or temporary fixtures. |
| 6 | The complete inherited model-workflow repair has consistent profile/configuration/resolver values, preserved attestation rejection behavior, and compatible MCP package version `1.1.21`; deterministic routing tests and actual MCP protocol validation establish those results without bypassing required gates. | PASS: configuration-contracts and routing parity prove 62 changed profiles/configuration/resolver values; 50 new routing/attestation cases are included in 513/513 final Pester passes. Published MCP 1.1.21 and normal-startup protocol evidence pass. Cached 1.1.20 model failure is retained truthfully; helper hash/path tests are explicitly scoped to the existing helper. |
| 7 | The final C# and PowerShell formatting, analysis, type/compatibility checks, tests, and required coverage comparisons pass with canonical evidence for the entire changed-language scope. | PASS: final ordered C# formatter/analyzer/nullable/tests and PowerShell MCP format/analyze/test plus direct scoped analysis/compatibility/coverage pass. Coverage-comparison proves C# 86.0212% -> 86.0331%, expanded PowerShell 84.0257% -> 94.9465%, unchanged default CI 95.1042%, all new executable units >=90%. Scoped analysis has zero findings; broad analyzer EXIT1 preserves 13 preexisting findings (10 warnings, 3 informational), zero errors/parse errors, delta0. This is the approved zero-regression criterion, not a globally clean analyzer claim. |
| 8 | Issue, spec, promoted-record links, and supporting workflow references match the final behavior and `full-bug` work mode; all required evidence is stored under this feature's canonical evidence directories. | PASS: issue/spec and promoted-record links consistently identify issue995/full-bug and the final confirmation route; all evidence is canonical under this feature. Redacted MCP replacement references exist; exact raw originals remain under SCRATCH. Final local results below supersede historical targeted counts. Independent review/PR/CI remain coordinator-owned delivery gates. |

Inspected canonical evidence:
- evidence/regression-testing/issue-995-enter-red.2026-10-17T23-03.md
- evidence/regression-testing/issue-995-csharp-green.2026-10-17T23-09.md
- evidence/qa-gates/csharp-format-check.2026-10-17T23-13.md
- evidence/qa-gates/csharp-analyzers.2026-10-17T23-14.md
- evidence/qa-gates/csharp-nullable.2026-10-17T23-14.md
- evidence/qa-gates/csharp-tests-coverage.2026-10-17T23-15.md
- evidence/qa-gates/powershell-format-check.2026-10-17T23-23.md
- evidence/qa-gates/powershell-analyze.2026-10-17T23-23.md
- evidence/qa-gates/powershell-analyze-repository-comparison.2026-10-17T23-23.md
- evidence/qa-gates/powershell-compatibility.2026-10-17T23-23.md
- evidence/qa-gates/powershell-tests-coverage.2026-10-17T23-26.md
- evidence/qa-gates/coverage-comparison.2026-10-17T23-27.md
- evidence/qa-gates/repository-hygiene-after-proof-redaction.2026-10-17T23-32.md
- evidence/qa-gates/configuration-contracts.2026-10-17T23-30.md
- evidence/other/issue-995-published-mcp-validation.2026-10-17T23-31.md
- evidence/other/issue-995-cached-mcp-validation.2026-10-17T23-31.md
- evidence/other/mcp-normal-startup-validation.2026-10-17T23-31.md

AC Status Summary: source spec.md; total8; checked8; remaining0. Only the seventh checkbox changed during this task; each earlier checked item was independently reconciled to its passing evidence. Criterion wording was mechanically compared before/after and is identical. No criterion is CI-dependent; required local execution gates are complete, while coordinator delivery gates remain pending.
