# Issue #439 Review Remediation Inputs

Timestamp: 2026-08-24T22-25

## Authoritative Requirement Source

This document is the primary requirements source for remediation planning. It supplements, but does not replace, `spec.md` and `plan.2026-08-24T17-30.md`.

## Required Fixes

1. Split `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` into cohesive C# files so every modified or added production file is at most 500 physical lines. Preserve the existing public `BreadcrumbBridgeRouter` contract, archive-root behavior, inbound typed-message validation, fallback behavior, event propagation, and provider-bound active-segment key behavior.
   - Verification: physical line counts for every changed C# production file are `<=500`; all existing Issue #439, queue, and binding-boundary tests pass; full C# QA and normalized coverage comparison pass without regression.
2. Split `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` into cohesive headless MSTest files/classes so every added or modified test file is at most 500 physical lines. Retain all seven Issue #439 scenarios and their assertions.
   - Verification: physical line counts for every changed C# test file are `<=500`; the 17 audited Issue #439-related tests pass; no test creates a real WinForms/WebView2 window/control/handle, calls `Show`, `ShowDialog`, or `Application.Run`, starts a UI message pump, uses Outlook COM, creates temporary files, accesses filesystem/network resources, or starts an external process.
3. Raise modified `QuickFiler/Controllers/EfcFormController.cs` coverage from 81/721 = 11.234397% to at least 80% while preserving the strict headless constraint. The plan must first determine whether this can be achieved through cohesive extraction/injected seams without real WinForms/WebView2 controls or handles, UI pump, Outlook COM, filesystem, network, or external process use. If the required threshold cannot be achieved within Issue #439 without prohibited or unrelated scope, the plan must fail closed with an explicit automated remediation-required result rather than weaken the requirement.
   - Verification: the normalized coverage comparison records numeric `EfcFormController` coverage `>=80%`, relevant headless tests pass, and no prohibited test behavior is introduced.
4. Preserve the verified removal of `[ExcludeFromCodeCoverage]` from `QuickFiler/Controllers/EfcFormController.cs`.
   - Verification: `rg -n -i 'ExcludeFromCodeCoverage|System.Diagnostics.CodeAnalysis' QuickFiler/Controllers/EfcFormController.cs` returns no match.
5. Re-run the final C# sequence after implementation: CSharpier, analyzer build, nullable build, headless focused regression, coverage wrapper, normalized comparison, and a fresh feature review.
   - Verification: all commands exit 0; final C# coverage remains `>=80%` repository-wide; changed/new instrumentable coverage remains `>=90%`; no required comparison file regresses.

## Do Not Do

- Do not change policy documents, coverage thresholds, or acceptance-criteria wording.
- Do not weaken, remove, or convert headless tests into live WinForms, WebView2, Outlook COM, filesystem, network, or process tests.
- Do not alter `ItemViewer` resources, Issue #400 behavior, score-model calculation, public configuration, or external APIs.
- Do not restore `[ExcludeFromCodeCoverage]` on `EfcFormController`.
- Do not weaken the 80% modified-file coverage requirement or substitute a live GUI/COM test for a headless seam.
- Do not silently skip formatter, analyzer, nullable, test, coverage, or review gates.
- Do not stage, commit, push, publish, or create/edit a pull request as part of planning.

## Review Evidence

- Policy finding: `BreadcrumbBridgeRouter.cs` is 596 lines, increased from 450 at `main`; `BreadcrumbBridgeRouterIssue439Tests.cs` is newly added at 531 lines.
- Functional acceptance: all 14 specification criteria are PASS in `feature-audit.2026-08-24T22-20.md`.
- Headless audit: 17 added/modified Issue #439-related tests contain no prohibited executable API use; final focused evidence is 97/97 passing.
- C# QA: final normalized evidence records 6,474/6,474 tests, 84.7835% repository coverage, and 200/203 = 98.522167% changed/new coverage.
- Additional coverage failure: modified `EfcFormController.cs` is 81/721 = 11.234397%, below the mandatory 80% per-file floor.
