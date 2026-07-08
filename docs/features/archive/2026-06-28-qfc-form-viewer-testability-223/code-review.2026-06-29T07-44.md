# Code Review: qfc-form-viewer-testability (#223)

**Review Date:** 2026-06-29
**Reviewer:** feature-review agent
**Feature Folder:** `docs/features/active/2026-06-28-qfc-form-viewer-testability-223`
**Feature Folder Selection Rule:** Supplied active folder; suffix `-223` matches the issue number in the branch range.
**Base Branch:** `main` (merge-base `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`)
**Head Branch:** `TaskMaster-wt-2026-06-28-18-50` (`f4b455e6a3ca536b3fc47fa7026b076efbacf453`)
**Review Type:** Cycle-1 remediation closing reaudit

---

## Executive Summary

This branch is a C# WinForms testability refactor for the QuickFiler form. It narrows the `IQfcFormViewer` interface from a UI-coupled surface (four `Button` properties, one `NumericUpDown`, two template UserControls) to 23 intent-level members across four seams: pure Alt-key routing (`QfcFormKeyHandler.IsAltKeyCommand`), command events plus skip/spinner state (Seam B), a `SwapItemTableLayout` TLP-swap method with a get-only `L1v0L2L3v_TableLayout` (Seam C), and plain-C# snapshot intents `CaptureTlpCellStates`/`GetKeyEventExclusionControls`/`ItemViewerTemplateMargin` (Seam D). To stay within the 500-line file cap before adding code, the 1142-line `QfcFormController.cs` was split into four partial-class files (195 / 311 / 399 / 232 lines). The diff is 74 files (+3751 / -992); 15 `.cs` + 2 `.csproj` are code, the remainder are feature/evidence docs.

This reaudit closes feature-review remediation cycle 1. The prior cycle's single blocking coverage-evidence gap is resolved: the canonical Cobertura artifact `artifacts/csharp/coverage.xml` now exists (well-formed; root `line-rate="0.741108"`), and a repo-wide first-party testable-denominator coverage figure (73.35%–74.11%) is recorded. That figure is below the bare `>= 80%` floor, but the shortfall is pre-existing (the change adds tests and exempts Form-bound code; it cannot lower first-party coverage) and is accepted under a maintainer-ratified authority-scoped exception scoped to #223, with residual uplift tracked under #197.

**What changed:**
`IQfcFormViewer.cs` removed the raw control properties and added typed intent members; `QfcFormViewer.cs` implements them and routes through the new static key handler; `QfcFormViewerDark/Expanded.cs` adopt `IsAltKeyCommand` and gain `[ExcludeFromCodeCoverage]`; `QfcCollectionController.ActivateQueuedTlp` delegates the swap to `SwapItemTableLayout` (net -3 lines); `QfcHomeController` switches to `ItemsPerLoadEnabled`/`SkipButtonEnabled`. New MSTest coverage (`QfcFormKeyHandlerTests`, `QfcFormControllerSeamTests`, plus migrations in `QfcFormControllerTests`/`QfcHomeControllerRunAsyncTests`) exercises routing, skip-flow state, and capture null/populated paths via `Mock<IQfcFormViewer>`. The four toolchain gates each recorded EXIT_CODE 0; the first-party suite is 4566/4566 passing. No `.cs`/`.csproj` changed after the cycle-close gate run (the two intervening commits are docs-only).

**Top 3 risks:**
1. Repo-wide first-party coverage (73.35%/74.11%) remains below the 80% floor. This is pre-existing, not introduced by this change, and accepted under a maintainer-ratified authority-scoped exception (`maintainer-decision.2026-06-29.md`); residual uplift is owned by #197. Non-blocking for #223.
2. Two pre-existing 500-line-cap files remain over cap (`QfcCollectionController.cs` 2296, `QfcFormControllerTests.cs` 821), accepted as net-negative pre-existing debt but still policy debt carried forward.
3. The interface narrowing is a breaking change to `IQfcFormViewer`; correctness depends on all in-repo consumers being migrated (verified for the three named controllers).

**PR readiness recommendation:** **Go** — implementation quality is sound, all four toolchain gates pass, the prior blocking coverage-evidence gap is resolved, and the only remaining coverage shortfall is a pre-existing, maintainer-ratified, authority-scoped exception that is out of scope for #223.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `artifacts/csharp/coverage.xml` | root element | Canonical Cobertura coverage artifact is now present and well-formed (root `line-rate="0.741108"`, `lines-covered="71654"`, `lines-valid="96685"`); repo-wide first-party testable-denominator figure recorded (73.35%/74.11%). Resolves the prior-cycle Blocker. | None. | Coverage verification is mandatory for every language with changed files; the artifact and a repo-wide figure now exist. | `head artifacts/csharp/coverage.xml`; `evidence/regression-testing/repo-wide-coverage-testable-denominator.2026-06-28T21-30.md` |
| Minor | repo-wide C# (testable denominator) | n/a | Repo-wide first-party coverage 73.35%/74.11% is below the 80% floor. Pre-existing, not introduced by this change; accepted under maintainer-ratified authority-scoped exception scoped to #223. | Accept for #223; complete repo-wide uplift under #197. | Repository policy expressly permits maintainer-ratified exemptions for COM-host-bound code; new code 100% and changed type +12.62pp confirm the change does not lower coverage. | `maintainer-decision.2026-06-29.md`; `evidence/other/repo-wide-floor-escalation-finding.2026-06-28T21-30.md` |
| Minor | `QuickFiler/Controllers/QfcCollectionController.cs` | whole file (2296 lines) | Pre-existing 500-line-cap violation remains; touched with only a net-negative Seam C edit (2299→2296). | Accept as pre-existing-debt this cycle; open a follow-up to split this `[ExcludeFromCodeCoverage]` class. | File cap is a policy invariant; the edit reduces rather than worsens it, and splitting is out of scope. | `awk END{print NR}` = 2296; baseline 2299 |
| Minor | `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | whole file (821 lines) | Pre-existing test-code 500-line-cap violation remains; held net-negative (823→821) with new seam tests routed to a separate 326-line file. | Accept as pre-existing-debt; consider future split of the legacy test file. | Test files count toward the 500-line cap; the change does not grow the violation. | `awk` = 821; baseline 823; new `QfcFormControllerSeamTests.cs` = 326 |
| Info | `QuickFiler/Controllers/QfcFormKeyHandler.cs` | lines 10-19 | Pure `internal static` predicate `IsAltKeyCommand(Keys) => keyData.HasFlag(Keys.Alt)`, XML-documented, called by all three viewers. | None. | Clean extraction of previously untestable Form-bound routing logic. | `git grep IsAltKeyCommand` shows 3 viewer call sites + definition |
| Info | `QuickFiler/Interfaces/IQfcFormViewer.cs` | lines 12-50 | Interface narrowed to 23 intent members; no raw `Button`/`NumericUpDown` property type; `L1v0L2L3v_TableLayout` get-only; templates removed. | None. | Achieves the Passive-View testability objective. | Inspected file (51 lines) |

No Blocker or Major findings remain.

---

## Implementation Audit

### C# implementation audit

#### What changed well

- The Alt-key predicate was extracted into a one-line pure static (`QfcFormKeyHandler.IsAltKeyCommand`) and reused by all three `ProcessCmdKey` overrides (verified call sites at `QfcFormViewer.cs:60`, `QfcFormViewerDark.cs:43`, `QfcFormViewerExpanded.cs:43`), removing the only piece of pure routing logic from Form-bound code and making it directly unit-testable.
- The interface narrowing is consistent: every removed control property has a corresponding intent member (command events, `decimal ItemsPerLoadValue`, `Padding ItemViewerTemplateMargin`, `IReadOnlyList<Control>`), and the get-only `L1v0L2L3v_TableLayout` plus `SwapItemTableLayout` correctly encapsulate the one setter write that previously lived in `QfcCollectionController.ActivateQueuedTlp` (verified delegation at `QfcCollectionController.cs:843`).
- The Phase 0 partial-class split is a clean responsibility partition (SetupDisposal / EventHandlers / Actions), each file well under 500 lines, with explicit `<Compile Include>` entries added to the csproj.

#### Type safety and API notes

- Nullable build passes under `TreatWarningsAsErrors`; no new nullable warnings introduced.
- `QfcFormKeyHandler` and the controller partials are `internal`, keeping the public surface intentional; `IQfcFormViewer` stays `public` because it is consumed cross-assembly.
- Form-derived and Designer code remains `[ExcludeFromCodeCoverage]` (verified on `QfcFormViewer:17`, `QfcFormViewerDark:16`, `QfcFormViewerExpanded:16`), consistent with the repo COM/VSTO/WinForms exemption. The coverage collector honors the attribute (the exempt Form classes are absent from the instrumented denominator), so the measured repo-wide figure already reflects the testable denominator.

#### Error handling and logging

- No new broad `catch` blocks; runtime behavior is preserved (structural refactor). Skip-flow and capture-null paths degrade to intended fallbacks rather than throwing, and are covered by the new seam tests.

---

## Test Quality Audit

The new tests use MSTest + Moq + FluentAssertions exclusively (no xUnit/NUnit) and isolate the Form boundary through `Mock<IQfcFormViewer>`. Routing is exercised via Moq `Raise`, skip-flow state via `VerifySet`, and `CaptureItemSettings` is tested on both populated and null `CaptureTlpCellStates()` results. The first-party suite is 4566/4566 passing with 0 failures; no test was removed or weakened.

### Reviewed test and QA artifacts

- `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` — verifies `IsAltKeyCommand` for Alt, Alt+Left, Control, None; deterministic, no I/O.
- `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` — 11 `[TestMethod]` cases covering command-event routing, skip-flow state, capture populated/null, and exclusion-control usage.
- `evidence/qa-gates/final-tests-coverage.2026-06-28T21-30.md` — 4566/4566 pass; repo-wide first-party 73.35%/74.11%.
- `evidence/regression-testing/coverage-delta.2026-06-28T20-52.md` — changed-type no-regression (+12.62pp) with denominator-shift explanation.
- `artifacts/csharp/coverage.xml` — canonical Cobertura; QfcFormKeyHandler 100%, QfcFormController 363/700 = 51.86% (independently re-derived this reaudit).

### Quality assessment prompts

- **Determinism:** No `DateTime.Now`/`Random`/network/temp-file usage in changed tests.
- **Isolation:** Each test targets one routing or state behavior with a fresh mock.
- **Speed:** Single coverage-enabled run; no external dependencies.
- **Diagnostics:** FluentAssertions yields descriptive failure messages.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | No credentials/keys in the diff; refactor of UI seams only. |
| No unsafe subprocess or command construction | N/A | No process or shell invocation introduced. |
| Input validation at boundaries | ✅ PASS | Capture null/early-return paths handled and tested. |
| Error handling remains explicit | ✅ PASS | No new broad catches; behavior preserved. |
| Configuration / path handling is safe | N/A | No path or config handling changed. |

---

## Research Log

No external research was required. All findings are grounded in direct diff inspection, head-state line counts (`awk`), `git grep` of call sites and markers, independent parsing of `artifacts/csharp/coverage.xml` (per-class line-rate derivation), an independent CSharpier check on three changed files (exit 0), and the executor evidence artifacts under the feature `evidence/` tree.

---

## Verdict

The implementation is well-structured and achieves its testability objective: the interface is narrowed to intent-level members, pure routing logic is extracted and tested, and the controller is split to respect the file cap. All four C# toolchain gates pass and the first-party suite is green at 4566/4566. The prior cycle's single blocking coverage-evidence gap is resolved — the canonical Cobertura artifact exists and a repo-wide first-party figure is recorded. The remaining repo-wide shortfall (73.35%/74.11% < 80%) is pre-existing, not introduced by this change, and accepted under a maintainer-ratified authority-scoped exception scoped to #223, with residual uplift owned by #197. No blocking finding remains. The two pre-existing 500-line-cap dispositions are accepted as net-negative debt and are not blockers. Recommendation: **Go**. This conclusion is consistent with the Findings Table (no Blocker/Major) and the Go readiness recommendation above.
