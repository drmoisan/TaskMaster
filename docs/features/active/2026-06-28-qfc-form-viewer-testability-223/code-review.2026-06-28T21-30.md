# Code Review: qfc-form-viewer-testability (#223)

**Review Date:** 2026-06-28
**Reviewer:** feature-review agent
**Feature Folder:** `docs/features/active/2026-06-28-qfc-form-viewer-testability-223`
**Feature Folder Selection Rule:** Supplied active folder; suffix `-223` matches the issue number in the branch range.
**Base Branch:** `main` (merge-base `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`)
**Head Branch:** `TaskMaster-wt-2026-06-28-18-50` (`e91927105abde2ceadd10a7011bc17d714108afd`)
**Review Type:** Initial review

---

## Executive Summary

This branch is a C# WinForms testability refactor for the QuickFiler form. It narrows the `IQfcFormViewer` interface from a UI-coupled surface (four `Button` properties, one `NumericUpDown`, two template UserControls) to 23 intent-level members across four seams: pure Alt-key routing (`QfcFormKeyHandler.IsAltKeyCommand`), command events plus skip/spinner state (Seam B), a `SwapItemTableLayout` TLP-swap method with a get-only `L1v0L2L3v_TableLayout` (Seam C), and plain-C# snapshot intents `CaptureTlpCellStates`/`GetKeyEventExclusionControls`/`ItemViewerTemplateMargin` (Seam D). To stay within the 500-line file cap before adding code, the 1142-line `QfcFormController.cs` was split into four partial-class files (195 / 311 / 399 / 232 lines). The diff is 46 files (+2278 / -992); 15 `.cs` + 2 `.csproj` are code, the remainder are feature/evidence docs.

**What changed:**
`IQfcFormViewer.cs` removed the raw control properties and added typed intent members; `QfcFormViewer.cs` implements them and routes through the new static key handler; `QfcFormViewerDark/Expanded.cs` adopt `IsAltKeyCommand` and gain `[ExcludeFromCodeCoverage]`; `QfcCollectionController.ActivateQueuedTlp` delegates the swap to `SwapItemTableLayout` (net -3 lines); `QfcHomeController` switches to `ItemsPerLoadEnabled`/`SkipButtonEnabled`. New MSTest coverage (`QfcFormKeyHandlerTests`, `QfcFormControllerSeamTests`, plus migrations in `QfcFormControllerTests`/`QfcHomeControllerRunAsyncTests`) exercises routing, skip-flow state, and capture null/populated paths via `Mock<IQfcFormViewer>`. The four toolchain gates each recorded EXIT_CODE 0; the suite is 196/196 passing.

**Top 3 risks:**
1. The canonical C# coverage artifact (`artifacts/csharp/coverage.xml`) is absent and the repo-wide first-party >= 80% floor is not measured — coverage of the floor is unverified.
2. Two pre-existing 500-line-cap files remain over cap (`QfcCollectionController.cs` 2296, `QfcFormControllerTests.cs` 821), accepted as net-negative pre-existing debt but still policy debt carried forward.
3. The interface narrowing is a breaking change to `IQfcFormViewer`; correctness depends on all in-repo consumers being migrated (verified for the three named controllers).

**PR readiness recommendation:** **Needs Revision** — implementation quality is sound and all four toolchain gates pass, but the absent canonical C# coverage artifact / unverified repo-wide floor is a blocking coverage-evidence gap that must be closed before merge.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocker | `artifacts/csharp/coverage.xml` | n/a (absent) | Canonical machine-readable C# coverage artifact is missing; repo-wide first-party (testable-denominator) >= 80% coverage is not measured (only disclaimed single-assembly process-wide 12.86%). | Generate `artifacts/csharp/coverage.xml` (Cobertura) and a repo-wide first-party measurement confirming the >= 80% floor. | Coverage verification is mandatory for every language with changed files; without it the repo-wide floor cannot be confirmed. | `ls artifacts/csharp` → no such dir; `evidence/regression-testing/coverage-delta.2026-06-28T20-52.md` |
| Minor | `QuickFiler/Controllers/QfcCollectionController.cs` | whole file (2296 lines) | Pre-existing 500-line-cap violation remains; touched with only a net-negative Seam C edit (2299→2296). | Accept as pre-existing-debt this cycle; open a follow-up to split this `[ExcludeFromCodeCoverage]` class. | File cap is a policy invariant; the edit reduces rather than worsens it, and splitting is out of scope. | `awk END{print NR}` = 2296; baseline 2299; `[ExcludeFromCodeCoverage]` at line 20 |
| Minor | `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | whole file (821 lines) | Pre-existing test-code 500-line-cap violation remains; held net-neutral (823→821) with new seam tests routed to a separate 326-line file. | Accept as pre-existing-debt; consider future split of the legacy test file. | Test files count toward the 500-line cap; the change does not grow the violation. | `awk` = 821; baseline 823; new `QfcFormControllerSeamTests.cs` = 326 |
| Info | `QuickFiler/Controllers/QfcFormKeyHandler.cs` | lines 10-19 | Pure `internal static` predicate `IsAltKeyCommand(Keys) => keyData.HasFlag(Keys.Alt)`, XML-documented, called by all three viewers. | None. | Clean extraction of previously untestable Form-bound routing logic. | `git grep IsAltKeyCommand` shows 3 viewer call sites + definition |
| Info | `QuickFiler/Interfaces/IQfcFormViewer.cs` | lines 12-50 | Interface narrowed to 23 intent members; no raw `Button`/`NumericUpDown`; `L1v0L2L3v_TableLayout` get-only; templates removed. | None. | Achieves the Passive-View testability objective. | Inspected file (51 lines) |

No additional Blocker or Major findings beyond the coverage artifact gap.

---

## Implementation Audit

### C# implementation audit

#### What changed well

- The Alt-key predicate was extracted into a one-line pure static (`QfcFormKeyHandler.IsAltKeyCommand`) and reused by all three `ProcessCmdKey` overrides, removing the only piece of pure routing logic from Form-bound code and making it directly unit-testable.
- The interface narrowing is consistent: every removed control property has a corresponding intent member (command events, `decimal ItemsPerLoadValue`, `Padding ItemViewerTemplateMargin`, `IReadOnlyList<Control>`), and the get-only `L1v0L2L3v_TableLayout` plus `SwapItemTableLayout` correctly encapsulate the one setter write that previously lived in `QfcCollectionController.ActivateQueuedTlp`.
- The Phase 0 partial-class split is a clean responsibility partition (SetupDisposal / EventHandlers / Actions), each file well under 500 lines, with explicit `<Compile Include>` entries added to the csproj.

#### Type safety and API notes

- Nullable build passes under `TreatWarningsAsErrors`; no new nullable warnings introduced.
- `QfcFormKeyHandler` and the controller partials are `internal`, keeping the public surface intentional; `IQfcFormViewer` stays `public` because it is consumed cross-assembly.
- Form-derived and Designer code remains `[ExcludeFromCodeCoverage]` (verified on `QfcFormViewer`, `QfcFormViewerDark`, `QfcFormViewerExpanded`, and `QfcCollectionController`), consistent with the repo COM/VSTO/WinForms exemption.

#### Error handling and logging

- No new broad `catch` blocks; runtime behavior is preserved (structural refactor). Skip-flow and capture-null paths degrade to intended fallbacks rather than throwing, and are covered by the new seam tests.

---

## Test Quality Audit

The new tests use MSTest + Moq + FluentAssertions exclusively (no xUnit/NUnit; grep clean) and isolate the Form boundary through `Mock<IQfcFormViewer>`. Routing is exercised via Moq `Raise`, skip-flow state via `VerifySet`, and `CaptureItemSettings` is tested on both populated and null `CaptureTlpCellStates()` results. The baseline 181-test suite was preserved and grew to 196 passing, with 0 failures.

### Reviewed test and QA artifacts

- `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` — verifies `IsAltKeyCommand` for Alt, Alt+Left, Control, None; deterministic, no I/O.
- `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` — 11 `[TestMethod]` cases covering command-event routing, skip-flow state, capture populated/null, and exclusion-control usage.
- `evidence/qa-gates/final-tests-coverage.2026-06-28T20-52.md` — 196/196 pass; QfcFormKeyHandler 100%; QfcFormController 51.86%.
- `evidence/regression-testing/coverage-delta.2026-06-28T20-52.md` — changed-type no-regression (+12.62pp) with denominator-shift explanation.

### Quality assessment prompts

- **Determinism:** No `DateTime.Now`/`Random`/network/temp-file usage in changed tests (grep clean).
- **Isolation:** Each test targets one routing or state behavior with a fresh mock.
- **Speed:** Single vstest `/InIsolation` run; no external dependencies.
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

No external research was required. All findings are grounded in direct diff inspection, head-state line counts (`awk`), `git grep` of call sites and markers, an independent CSharpier check on the four most-changed files (exit 0), and the executor evidence artifacts under the feature `evidence/` tree.

---

## Verdict

The implementation is well-structured and achieves its testability objective: the interface is narrowed to intent-level members, pure routing logic is extracted and tested, and the controller is split to respect the file cap. All four C# toolchain gates pass and the suite is green at 196/196. The change is not yet ready for normal PR flow because of one blocking coverage-evidence gap: the canonical C# coverage artifact (`artifacts/csharp/coverage.xml`) is absent and the repo-wide first-party >= 80% floor is unverified. Once that artifact and a repo-wide first-party measurement are produced (and assuming they confirm the floor), the change should be Go. The two pre-existing 500-line-cap dispositions are accepted as net-negative debt and are not blockers. This conclusion is consistent with the Findings Table and the Needs Revision readiness recommendation above.
