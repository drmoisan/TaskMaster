# Code Review: QfcItemController Testability — Cycle-3 Targeted Residual Reduction (#227)

**Review Date:** 2026-07-02
**Reviewer:** feature-reviewer (Claude)
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Feature Folder Selection Rule:** Selected version is the feature root (no `vN/` subfolder present); per-cycle audit artifacts are grouped under `<exit-ts>-audit/` subfolders per the repository convention established in commit `0a212191`.
**Base Branch:** `main` (merge-base `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head Branch:** `TaskMaster-wt-2026-06-29-09-38` — committed HEAD `0a212191` (cycle-1 + cycle-2) plus the **uncommitted working tree** carrying all cycle-3 (Phases 9-11) production, seam, test, csproj, and evidence changes.
**Review Type:** Post-remediation re-review (cycle 3)

---

## Executive Summary

Cycle 3 executes a maintainer-authorized targeted residual reduction following an independent re-audit
of cycle-2's 41-member `[ExcludeFromCodeCoverage]` boundary against the original seam-redesign
research's ~6-8 irreducible estimate. The re-audit found 17 of the 41 residuals actionable (9 test-only,
8 via two new/extended seams), and cycle-3 delivers that reduction: 41 → 24, verified by an exact grep
match against the itemized boundary artifact.

**What changed:**
- `FolderPredictor` factory-delegate seam (mirroring the already-built `EmailFiler`/`FlagTasks`/
  `ConversationResolver` pattern): a new `IFolderSearchHandler` interface, an empty
  `FolderPredictor.IFolderSearchHandler.cs` partial-declaration file, and two new factory-delegate
  fields/constructor parameters on `QfcItemController` with production defaults. Unblocks 5 members.
- `Theme` + `IUiDispatcher` retrofit: `Theme.cs` gains an optional `IUiDispatcher` constructor
  parameter (default `WpfUiDispatcher`), replacing the direct `UiThread.Dispatcher`/`_lblSender.BeginInvoke`
  calls in `SetQfcThemeAsync`/`SetQfcTheme(async:true)`/`SetMailRead(async:true)`; the render body is
  extracted verbatim to a new `Theme.Rendering.cs` (which also resolves a pre-existing 544-line
  over-cap condition in `Theme.cs`, now 451 lines). Unblocks 3 members.
- 9 Tier-1 test-only de-exemptions requiring zero new production seams (`RegisterExpandedActions`,
  `JumpToAsync(Control)`, `PopulateControls(MailItem,int)`/`PopulateControlsAsync`,
  `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)`, `WpfUiDispatcher`'s forwarding body,
  `MailItemActionsAdapter`, `BtnFlagTask_Click`).
- Tests: +19 QuickFiler.Test (328→347), +4 UtilitiesCS.Test (4089→4093); 0 removed, 0 failed.

**Top 3 risks:**
1. **Two of the 17 claimed de-exemptions are not behaviorally verified.**
   `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)` are tested only for the fact that they call
   `_itemViewer.Invoke(...)`; the delegate carrying all substantive logic is never executed by either
   test, so no assertion exists on the actual outcome of calling these methods. See Findings Table.
2. Cycle-3's delivery is **uncommitted** in the working tree (recurring process/merge gate from cycle-2).
3. The affected non-exempt denominator (77.40%) remains below the spec's 80% target, though improved
   +3.81pp with no regression — an open item, not newly introduced this cycle.

**PR readiness recommendation:** **Needs Revision** — one Major code-quality finding (item 1 above) plus
the recurring uncommitted-delivery process gate must be resolved before this cycle is merge-ready.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major | `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | `27-67` (`ToggleFocus(Enums.ToggleState)`), `83-123` (`ToggleFocus()`) | Both members' entire bodies are wrapped in a single `_itemViewer.Invoke(...)` call. The only tests for either method (`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:124-151`) assert solely that `Invoke` was called once; the delegate is never executed (confirmed by the tests' own inline comments: "its delegate body is never executed"). No test verifies the `_activeUI`/`_activeTheme` state transition, `RegisterFocusAsyncActions`/`UnregisterFocusAsyncActions` calls, or the `_themes[...].SetQfcTheme(async:false)` call that constitute the method's actual behavior. | Either (a) restructure the two methods to perform state-mutation directly (outside the `Invoke` wrapper) — mirroring how `ToggleFocusOnAsync`/`ToggleFocusOffAsync` (same file, lines 138-166) already decouple state-mutation from the Theme-render call — then assert on the resulting `_activeUI`/`_activeTheme` fields the same way `ToggleFocusOnAsync_ActivatesUiAndSwitchesToActiveTheme` (line 156) does; or (b) restore `[ExcludeFromCodeCoverage]` on both members with the same per-member justification cycle-2 used, since the underlying barrier (`Theme.SetQfcTheme(async:false)` faulting on a handle-less `Theme` double) is unresolved, not removed. | This is exactly the reduction-honesty concern the cycle-3 review scope calls out: removing `[ExcludeFromCodeCoverage]` without genuinely resolving the barrier converts an honestly-labeled gap into a silently-uncovered "instrumented but behaviorally unverified" member, which the General Unit Test Policy explicitly warns against ("untested critical behavior is not acceptable even if the overall percentage looks good"). Cycle-2's own accepted code review (`2026-07-02T10-47-audit/code-review.2026-07-02T10-47.md:124-128`) already documented this exact barrier as genuine. | Source read of both files; `evidence/other/exemption-boundary.2026-07-02T15-05.md` lines 27-31 (claims "non-executing Mock<IItemViewer>.Invoke marshal-verification" as the covering technique, self-disclosing the gap). |
| Major (process) | working tree | n/a | All cycle-3 production/seam/test/csproj/evidence files are uncommitted; committed HEAD `0a212191` has no cycle-3 diff. | Commit the full cycle-3 change set; confirm `git status` clean before merge. | Recurring from cycle-2 (same finding type); the branch cannot merge the reviewed work while it is uncommitted. | `git status --short`; `git rev-parse HEAD` = `0a212191` |
| Minor | `docs/features/active/2026-06-29-qfc-item-controller-testability-227/spec.md` | AC5 Coverage Target section | The affected non-exempt denominator (77.40%) has not crossed the spec's stated ≥80% target, though it improved and did not regress. `spec.md`'s narrative for AC5/AC8/AC10 does not explicitly flag this residual gap for cycle-3. | State explicitly in `spec.md` (or a follow-up note) whether the sub-80% affected-denominator reading is accepted as a documented exception (mirroring the repo-wide-floor exception already granted under #223) or is scheduled for further uplift. | Keeps the acceptance narrative honest about what remains open versus what is fully resolved. | `evidence/qa-gates/final-tests-coverage.2026-07-02T15-12.md` (77.40%); `spec.md` Coverage Target section. |
| Minor | `artifacts/csharp/coverage.xml` | n/a | Canonical C# coverage artifact remains cycle-1 (2026-06-29); cycle-3 coverage lives only in the evidence markdown files. | Regenerate the canonical `artifacts/csharp/coverage.xml` from the cycle-3 final run. | Keeps the standard gate artifact current; numeric evidence already exists elsewhere so non-blocking. | `ls -la artifacts/csharp/coverage.xml` (dated 2026-06-29 12:36). |
| Nit | `QuickFiler/Controllers/QfcItemController.*.cs` | using blocks | Suggestion-level analyzer diagnostics persist unchanged from cycle-2 (unnecessary usings, make-field-readonly, simplify-null-check). No new field introduced this cycle triggers a new diagnostic beyond the pre-existing set. | No action required this cycle; continue tracking as follow-up cleanup. | Suggestion severity does not break the `TreatWarningsAsErrors` build (verified `EXIT_CODE 0`), non-blocking per the repo severity-first analyzer invariant. | `evidence/qa-gates/final-analyzers.2026-07-02T15-09.md` (EXIT_CODE 0). |
| Info | `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs` | n/a | The `Theme.Rendering.cs` split incidentally resolves a pre-existing 544-line over-cap condition in `Theme.cs` (now 451 lines) that predates this cycle. | None required; positive side effect worth noting. | Demonstrates the split was structurally beneficial beyond its seam purpose. | `evidence/qa-gates/final-file-sizes.2026-07-02T15-15.md` line 23. |

No Blocker findings.

---

## Implementation Audit

### C# implementation audit

#### What changed well

- **`FolderPredictor` factory-delegate seam correctly mirrors the established pattern.** The
  `Func<IApplicationGlobals, object, FolderPredictor.InitOptions, FolderPredictor>` and
  `Func<IApplicationGlobals, FolderPredictor>` fields follow the exact shape and defaulting convention
  (`??=` in `SaveParameters`) already used for `_conversationResolverFactory`/`_flagTasksFactory`/
  `_emailFilerFactory`. No new abstraction tier was introduced beyond what the pattern requires.
- **`Theme` + `IUiDispatcher` retrofit is a genuine, narrow extension of an existing seam type**, not a
  new seam design. The three retrofitted call sites (`SetQfcThemeAsync`, `SetQfcTheme(async:true)`,
  `SetMailRead(async:true)`) are minimal, surgical diffs (verified via `git diff`); the render body
  (`SetQfcTheme()`, the private method) is moved verbatim to `Theme.Rendering.cs`, not rewritten.
- **Production-default application is universal across construction paths.** `SaveParameters`'s
  `_uiDispatcher ??= new WpfUiDispatcher();` (and the analogous `_folderPredictorFactory ??=`/
  `_folderPredictorEmptyFactory ??=`) is the single choke point every constructor and the
  `CreateAsync`/`CreateSequentialAsync` static factories funnel through — independently verified by
  reading `QfcItemController.Initialization.cs:346-398` and confirming both factories call
  `SaveParameters` directly (lines 419, 452) rather than duplicating default-application logic.
- **`WpfUiDispatcher`'s forwarding body is genuinely exercised**, not just constructed. The new test
  (`WpfUiDispatcherTests.cs:39-86`) swaps the private static `UiThread._dispatcher` field via
  reflection for a real, running WPF `Dispatcher` hosted on a dedicated STA thread, then asserts that
  `Invoke`/`InvokeAsync`/`BeginInvoke` each execute the supplied delegate on the dispatcher's own
  thread — a real behavioral test, not a construction smoke test dressed up as coverage.
- **`MailItemActionsAdapter`'s de-exemption is a clean attribute-removal.** The class carried no
  substantive logic beyond 1:1 forwarding, and `MailItemActionsAdapterTests.cs` (unchanged this cycle)
  already provided full coverage before the attribute was removed — verified by reading both files.
- **`BtnFlagTask_Click`'s de-exemption genuinely exercises the delegator.** The test injects a
  factory that throws a sentinel exception and asserts the exception propagates through the click
  handler, proving the handler actually delegates into `FlagAsTask()` rather than merely compiling.

#### Type safety and API notes

- `Theme`'s new constructor parameter (`IUiDispatcher uiDispatcher = null`) is nullable and optional;
  the nullable build remains clean (`EXIT_CODE 0`). No breaking change to any existing `Theme`
  construction call site (all four `QfcThemeHelper.SetupThemes` dictionary entries pass the new
  argument explicitly, but the parameter itself is optional for any other caller).
- `IFolderSearchHandler` is a narrow, single-method interface (`FindFolder(...)`), consistent with the
  DI-seam-ordering preference for minimal purpose-specific interfaces.

#### Error handling and logging

- No new error-handling paths were introduced this cycle beyond what the seam extension required; no
  new broad `catch` blocks were added.

---

## Test Quality Audit

Cycle-3 tests are, with the one flagged exception, well-designed and genuinely exercise production
behavior: `FolderPredictor`-cluster tests use `Mock<IFolderSearchHandler>` and assert on the resulting
folder-combobox population; `Theme`-dispatcher tests use `Mock<IUiDispatcher>` and assert the dispatcher
method was called with the correct routing; `WpfUiDispatcherTests` uses a real, running dispatcher on a
dedicated STA thread; `BtnFlagTask_Click`'s test uses a sentinel-exception factory to prove real
delegation. The `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)` tests are the sole exception — see
Findings Table.

### Reviewed test and QA artifacts

- `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` — genuine coverage for
  `ToggleFocusOnAsync`/`ToggleFocusOffAsync` (asserts `_activeUI`/`_activeTheme`); plumbing-only for
  `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)` (see Findings Table).
- `QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs` — genuine coverage for
  `ToggleFocusAsync`×2 and `ApplyReadEmailFormat` via `Mock<IUiDispatcher>` + field/mock assertions.
- `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` — genuine live-dispatcher execution test.
- `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.DispatcherTests.cs` — genuine coverage of the four
  changed `Theme` lines with per-function line-coverage confirmation in
  `evidence/qa-gates/final-tests-coverage.2026-07-02T15-12.md`.
- `evidence/regression-testing/coverage-delta.2026-07-02T15-14.md` — 73.59% → 77.40% affected-denominator,
  no changed-line regression, 4440/4440 tests pass.

### Quality assessment prompts

- **Determinism:** No network/clock/temp-file dependence; `WpfUiDispatcherTests` uses a deterministic
  `ManualResetEventSlim` signal (not polling) to observe the fire-and-forget `BeginInvoke` completion.
- **Isolation:** Each test targets one member/behavior via reflection field injection or direct
  construction.
- **Speed:** 4440-test MSTest suite; no sleeps or retries observed in the new tests.
- **Diagnostics:** FluentAssertions with `because` reasons used throughout the new tests.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | Diff inspected; none present. |
| No unsafe subprocess or command construction | N/A | No process/shell construction in scope. |
| Input validation at boundaries | ✅ PASS | `Theme`'s new constructor parameter uses a null-coalescing default; `SaveParameters` applies all seam defaults at a single choke point. |
| Error handling remains explicit | ✅ PASS | No new broad catches; existing error handling in unchanged surrounding code preserved. |
| Configuration / path handling is safe | N/A | No new config/path handling introduced. |

---

## Research Log

No external research was required for this review beyond reading the cited feature-folder artifacts,
`spec.md` v0.4, `artifacts/research/2026-07-02T11-00-qfc-item-controller-residual-reaudit-research.md`
§1 (the per-member disposition table used to cross-check the 17 de-exemptions), the cited cycle-2
audit/code-review artifacts (for the `ToggleFocus` barrier precedent), and direct source inspection of
every changed production and test file.

---

## Verdict

Cycle 3 is a mostly clean, well-evidenced targeted reduction: 16 of the 17 claimed de-exemptions are
genuinely behavior-verified, the toolchain is green in order, 4440/4440 tests pass with zero
regressions, and file sizes remain compliant. However, this review's independent cross-check (the
reduction-honesty check explicitly requested for this cycle) found that `ToggleFocus()` and
`ToggleFocus(Enums.ToggleState)` were de-exempted using a test that verifies only that the method's
`Invoke` wrapper was called — not that the method does what it is supposed to do. Combined with the
recurring uncommitted-delivery process gate, this cycle is **Needs Revision**, not a clean Go: it is not
merge-ready until (1) the `ToggleFocus` finding is resolved by either genuine verification or an honest
re-exemption, and (2) the delivered working tree is committed.

**Code-review blocking-finding count: 2** (1 Major code-quality finding — `ToggleFocus`/
`ToggleFocus(Enums.ToggleState)` reduction honesty; 1 Major process/merge-readiness gate — uncommitted
delivery).
