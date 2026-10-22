# Code Review: Issue #439 EfcViewer lineage and segment navigation

**Review Date:** 2026-08-24
**Reviewer:** Codex feature reviewer
**Feature Folder:** `docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439`
**Feature Folder Selection Rule:** branch suffix and PR-context scoping document both identify Issue #439.
**Base Branch:** `main` at `988e819b3bf3d31d6bbe523a2ce6c66189ce718d`
**Head Branch:** `bug/efcviewer-missing-lineage-and-segment-navigation-439` at `f1b8e504d9d84f2327c919cb27bdb7b076424a6b`
**Review Type:** initial feature-branch review

## Executive Summary

The reviewed range introduces archive-root-aware hierarchy lookup while retaining original filing targets, typed segment and rendered-child messages, explicit active-segment state, and generated-document activation handling. The boundaries remain appropriately separated: the router receives host/provider seams, the row owns state transitions, the codec validates inbound messages, and the renderer/document assets generate the browser-facing content. Final C# QA evidence is current for the implementation commit `c39db103`; there is no subsequent C# or test drift through `HEAD`.

The code has no functional blocker in the inspected behavior, but it is not ready for PR flow because the branch violates the repository 500-line limit in a new test file and a modified production file, and the modified Efc controller is below the mandatory 80% per-file coverage floor. The separate policy audit records those remediation requirements.

**What changed:** archive-relative root expansion, root-first row lineage and preserved score keys, typed segment/child activation, stable ancestor-key expansion, generated-document event propagation control, renderer arrow/child output, and focused headless coverage.

**Top 3 risks:**

1. `BreadcrumbBridgeRouter.cs` is 596 lines after growing from 450; further state/routing changes will be hard to isolate.
2. `BreadcrumbBridgeRouterIssue439Tests.cs` is a 531-line new test file, beyond the repository file-size policy.
3. `EfcFormController.cs` is 81/721 = 11.234397% covered after removal of its exclusion, below the mandatory 80% modified-file floor.

**PR readiness recommendation:** **Needs Revision** — functional and QA evidence pass, but the two file-size policy violations require an approved remediation before PR readiness.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | whole file, 596 lines | The branch expands a 450-line router to 596 lines, violating the 500-line repository limit. | Extract a cohesive hierarchy-path or inbound-activation collaborator while retaining the existing public router contract and tests. | The routing, hierarchy transformation, and activation flows now exceed the mandated maintainability limit. | `git show 988e819b:...` = 450 lines; `HEAD` = 596 lines. |
| Major | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | whole file, 531 lines | Newly added Issue #439 test file exceeds the 500-line test-file limit. | Split by behavior into cohesive headless test classes/files without weakening coverage. | The repository policy applies the limit to test files as well as production files. | `HEAD` physical line count = 531. |
| Major | `QuickFiler/Controllers/EfcFormController.cs` | whole file coverage, 81/721 | Modified controller coverage is 11.234397%, below the feature-review workflow's 80% mandatory floor. | Add or extract headless-testable seams sufficient to meet the per-file policy without constructing real UI/COM resources; if unavailable, remediate as a separate approved scope before PR readiness. | The global repository metric and changed-line metric pass, but modified-file coverage must also be at least 80%. | `evidence/qa-gates/csharp-coverage-final.md`; feature-review workflow coverage rule. |

No functional Blocker findings were identified.

## Implementation Audit

### C# implementation audit

#### What changed well

- `EfcFormController.BindBreadcrumbRowsAsync` supplies `ArchiveRootPath` only to the internal router overload; the public legacy overload remains unchanged.
- `BreadcrumbBridgeRouter.ToHierarchyPath` avoids duplicate root prefixes with ordinal-ignore-case comparisons, and `ToArchiveRelativePath` returns archive-relative selection values after ancestor or child activation.
- `BreadcrumbRow` retains `FilingTarget` independently from full hierarchy segment paths and requires a provider-bound key before non-leaf activation.
- The codec rejects missing required indices before state change, and the embedded document script stops propagation after segment and child activation messages.

#### Type safety and API notes

Final analyzer and nullable evidence reports zero new diagnostics. `[ExcludeFromCodeCoverage]` is absent from `QuickFiler/Controllers/EfcFormController.cs`; the base had the attribute at line 27 and the branch removes it. The archive-root overload is internal, so the review found no new public API surface. Its resulting whole-file coverage is only 11.234397%, which is a separate policy failure.

#### Error handling and logging

Provider cancellation and exceptions in chain resolution produce a logged, selectable fallback. Invalid row, segment, and child input is rejected without selection/expansion mutation. Provider exceptions during child expansion are logged and leave state unchanged.

## Test Quality Audit

The review audited all 17 added or modified Issue #439 tests: seven router scenarios, two modified queue scenarios, one Efc binding test, two renderer tests, two codec tests, one row-builder test, and two row-state tests. The static call audit found no executable use of WinForms/WebView2 construction, window handles, `Show`, `ShowDialog`, `Application.Run`, Outlook COM, filesystem, network, or external process APIs. The only token matches were explanatory comments. The focused final regression evidence reports 97 passing tests; the normalized coverage wrapper reports 6,474 passing tests.

### Reviewed test and QA artifacts

- `evidence/regression-testing/issue-439-post-fix-regression.md` — focused headless regression, 97 passed and 0 failed.
- `evidence/qa-gates/issue-439-qa-loop.md` — final ordered C# QA loop, 6,474/6,474 tests passed.
- `evidence/qa-gates/issue-439-coverage-comparison.md` — normalized coverage threshold and no-regression proof.

- **Determinism:** fixed strings, pure models, and mock provider/host responses.
- **Isolation:** each test targets a router, row, codec, renderer, or controller-binding boundary.
- **Speed:** focused regression passes in the recorded deterministic runner; no live Outlook or GUI is involved.
- **Diagnostics:** FluentAssertions and strict mock verification identify the affected contract.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Reviewed C# diff contains no credential/configuration additions. |
| No unsafe subprocess or command construction | PASS | Changed implementation and test call audit found none. |
| Input validation at boundaries | PASS | `BreadcrumbMessageCodec` requires known types, row IDs, and applicable indices; router/row reject invalid targets. |
| Error handling remains explicit | PASS | Provider failure paths log and preserve deterministic fallback/state behavior. |
| Configuration / path handling is safe | PASS | Archive-root additions/removals use ordinal-ignore-case comparisons and separator-boundary checks. |

## Research Log

No external research was required. The review used the repository PR-context artifacts, implementation diff, feature specification, plan, and committed evidence.

## Verdict

The implementation meets the reviewed functional contracts and the stated headless-test constraint. Final C# toolchain evidence is PASS and no post-QA C# drift exists, but the modified Efc controller's 11.234397% coverage fails the required per-file floor. PR readiness is **Needs Revision** until the router and test file are split under 500 lines and EfcForm coverage is remediated under an approved headless-compatible scope.
