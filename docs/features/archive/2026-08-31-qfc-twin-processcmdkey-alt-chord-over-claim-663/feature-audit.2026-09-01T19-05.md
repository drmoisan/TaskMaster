# Feature Audit — qfc-twin-processcmdkey-alt-chord-over-claim-663

- **Issue:** #663
- **Branch:** `bug/qfc-twin-processcmdkey-alt-chord-over-claim-663` @ `20f1b201`
- **Baseline:** `origin/main` @ `9ca9e99a` (recomputed merge base; identical)
- **Work Mode:** `full-bug` — `spec.md` is the sole acceptance-criteria source, carrying AC-1 through AC-15. No `user-story.md` exists and none is required.
- **Audit timestamp:** 2026-09-01T19-05

The worktree root is rendered as `<repo-root>` throughout.

## Method

Every acceptance criterion was evaluated against the tree and the evidence on disk, not against the executor's checkbox. Each criterion's own stated verification was executed by this reviewer where it was re-runnable — this covers AC-3, AC-7, AC-8, AC-9, AC-12, AC-13, and AC-14 in full, and the structural half of AC-1, AC-2, AC-4, AC-5, and AC-6. Where the verification depended on an artifact the plan required to be deleted after transcription — the msbuild logs, the vstest console captures, and the Cobertura document — the transcribed evidence was read and cross-checked for internal consistency against other artifacts that record the same quantities.

All fifteen criteria were checked off by the executor. All fifteen are **confirmed**. None is contradicted.

## Per-Criterion Evaluation

| AC | Verdict | Basis |
|---|---|---|
| AC-1 | **PASS** | Read at head: `internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)` is declared in `QuickFiler/Controllers/QfcFormKeyHandler.cs`. Its body returns `false` when `handler is null \|\| !keyData.HasFlag(Keys.Alt)`, then computes `keyData & Keys.KeyCode` and returns `keyCode == Keys.Menu \|\| keyCode == Keys.None` — the exact if-and-only-if condition the criterion states. All seven `ClaimsAltChord_*` methods are present and pass in the final run (6934 of 6934, empty failing list). Every row of the specification's behavior table is covered by a named test. |
| AC-2 | **PASS** | Both shapes are pinned by distinct methods read in the test file: `ClaimsAltChord_WithBareAltFlagAndHandler_ReturnsTrue` supplies `Keys.Alt` (key-code half `Keys.None`), and `ClaimsAltChord_WithMenuKeyCodeAndAltFlag_ReturnsTrue` supplies `Keys.Menu \| Keys.Alt` (key-code half `Keys.Menu`). Both assert `BeTrue`. Both pass; `evidence/regression-testing/red-run.md` separately confirms neither appeared in the red run's failing list, so both were green across both phases. |
| AC-3 | **PASS** | `ClaimsAltChord_WithAltM_ReturnsFalse` is declared inside `namespace QuickFiler.Controllers.Tests` on class `QfcFormKeyHandlerTests`, satisfying the declaring-type qualification the criterion requires. Its because-string reads "Alt+M is the Move Options mnemonic on the hosted item viewers and must reach the base implementation". Reviewer-executed: `Move Options` returns 2 matches (>= 1 required); `Filters menu` returns 0 matches (0 required). `evidence/regression-testing/red-run.md` records this test failing in the red phase with its declaring type read from the stack trace, distinguishing it from the identically named Email Filer method. |
| AC-4 | **PASS** | `ClaimsAltChord_WithAltF4_ReturnsFalse` (input `Keys.Alt \| Keys.F4`) and `ClaimsAltChord_WithAltLeft_ReturnsFalse` (input `Keys.Alt \| Keys.Left`) are both present and both assert `BeFalse`. Both are among the three genuine reds in the red run and both pass in the green and final runs. |
| AC-5 | **PASS** | `ClaimsAltChord_WithoutAltFlag_ReturnsFalse` asserts two inputs in one body, exactly as the criterion prescribes: `Keys.M` with because-string "a bare letter key carries no Alt flag and is not the dialog gesture", and `Keys.Control` with a separate because-string "Keys.Control carries no Alt flag even though its key-code half is Keys.None". The eighth behavior-table row is therefore exercised while the seven-method enumeration stays intact. |
| AC-6 | **PASS** | `ClaimsAltChord_WithNullHandler_ReturnsFalse` on `QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests` declares `IQfcKeyboardHandler handler = null;` and calls the predicate with `Keys.Alt`, asserting `BeFalse`. The declaring type is confirmed by reading the file's namespace and class, which is required because the Email Filer fixture declares a method of the same name. |
| AC-7 | **PASS** | Reviewer-executed, all three clauses: `ClaimsAltChord` returns exactly 1 match, at line 58; `Keys\.Alt` returns 0 matches; `IsAltKeyCommand` returns 0 matches. Line 58 lies inside `ProcessCmdKey`, whose declaration is at line 56 and whose fall-through `return base.ProcessCmdKey(...)` is at line 69 — confirmed by reading the file rather than assumed. Command output is recorded at `evidence/qa-gates/663-predicate-structure.md`, which additionally establishes the before/after direction of each reading against the Phase 0 baseline. |
| AC-8 | **PASS** | The `IsAltKeyCommand` body at head is unchanged: `internal static bool IsAltKeyCommand(Keys keyData) => keyData.HasFlag(Keys.Alt);`. Reviewer-executed: `git diff -U0 origin/main...HEAD` over the two named files contains 0 removed lines matching `IsAltKeyCommand`. All four named test methods are present and unmodified in the diff, which adds only new methods after line 65. Independent corroboration: the test-count arithmetic 6927 + 7 = 6934 holds across the baseline, red, green, and final runs, which is inconsistent with any existing test having been removed or renamed. |
| AC-9 | **PASS** | Reviewer-executed: `git diff --name-only origin/main...HEAD` lists neither `QuickFiler/QuickFiler.csproj` nor `QuickFiler.Test/QuickFiler.Test.csproj`. The only diff entry containing the string "csproj" is the evidence artifact `evidence/qa-gates/csproj-untouched.md`. |
| AC-10 | **PASS** | All four stages recorded in order with exit code 0. Format: `dotnet tool run csharpier check .`, exit 0, 1566 files checked — the same count as the Phase 0 baseline, consistent with no file added or removed — and no unformatted path reported. Analyzers: exit 0, one `^\s*0 Error\(s\)$` line, zero `: error [A-Z]+[0-9]+:` lines, 36 occurrences of `Task "Csc"` proving `CoreCompile` ran, and a warning pair set equal to the empty `BASELINE_WARNINGS`. Type-check: exit 0, one `0 Error(s)` line, zero diagnostic-form error lines, 36 `Task "Csc"` occurrences, and the transcribed command line verified to contain no `Nullable=enable`. Tests: `-SearchRoot .` used on every wrapper invocation, 6934 of 6934 passed, empty failing list, standard error 0 bytes. The instrumented coverage run's gate is failure-set membership rather than exit status, and `BASELINE_COVERAGE_FAILURE_SET` is empty with no failure observed outside it. **Basis note:** the msbuild logs and console captures were deleted after transcription, so the build stages rest on attested transcripts rather than reviewer re-execution; the file counts, warning counts, and `Task "Csc"` counts are mutually consistent across the Phase 0 and Phase 4 artifacts. |
| AC-11 | **PASS** | `evidence/qa-gates/coverage-final.md` carries the required `AC-11 evidence of record:` line stating that the raw document was transcribed and then deleted. The transcribed `<method>` element for `ClaimsAltChord` has `line-rate="1"`, which is at least 0.90 as required, and `branch-rate="1"`; all seven of its instrumented lines carry `hits="1"` and both branch lines report 100% condition coverage. The transcribed `<class>` element for `QuickFiler.Controllers.QfcFormKeyHandler` has `line-rate="1"`, not lower than the `BASELINE_CLASS_LINE_RATE` of 1 recorded at `evidence/baseline/coverage.md`. Both figures were read from post-processed documents, satisfying the same-document-kind clause; the baseline artifact states this requirement explicitly and the post-change artifact confirms compliance. Repository-wide root line rate moved 0.853866 to 0.853726 and branch rate 0.794064 to 0.794078, both above their floors. |
| AC-12 | **PASS** | `ExecutingAssembly_ContainsNoFormDerivedType` is reported `Passed [1 ms]` in the final run transcript. Reviewer-executed: pattern VC-1 (`new Form\|: Form\|Thread\.Sleep\|Task\.Delay\|GetTempFileName\|GetTempPath`) returns 0 matches over `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`. The new tests construct only `Mock<IQfcKeyboardHandler>` instances and `Keys` constants. |
| AC-13 | **PASS** | Reviewer-executed: `git diff -U0 origin/main...HEAD -- '*.cs'` contains 0 added lines matching `ExcludeFromCodeCoverage`. The `.cs` scoping is correct and is justified in the criterion itself: the documentation commits quote the attribute name in prose, so an unscoped diff would report matches before any source edit. |
| AC-14 | **PASS** | Reviewer-executed: `git diff --name-only origin/main...HEAD -- '*.cs'` lists exactly the three authorised paths and no other. `git status --porcelain` reports a completely clean working tree, so no untracked `.cs` path exists. Pattern VC-2 (`FromHandle\|new KeyEventArgs`) returns exactly 2 matches over `QuickFiler/Viewers/QfcFormViewer.cs`, at lines 61 and 62, both inside `ProcessCmdKey` (lines 56 to 70) — the retained unused locals survive as required. Call sites 2 through 5 are untouched: `QfcFormViewerDark.cs`, `QfcFormViewerExpanded.cs`, `QuickFiler/Legacy/QfcFormLegacyViewer.cs`, and `TaskVisualization/TaskViewer.cs` appear nowhere in the branch diff. |
| AC-15 | **PASS** | `evidence/other/manual-validation.md` exists and records all three gestures — bare Alt, Alt+M, Alt+F4 — each with the status `MANUAL_CHECK_DEFERRED`. Both required probes are present with measured values: `Get-Process -Name OUTLOOK` returned count 0 and `[Environment]::UserInteractive` returned `True`. The record carries explicit, separate lists of what the automated tests do establish and what they do not, and states that no Outlook build is named because no Outlook process was running to read one from. No gesture is omitted and none is recorded as a pass on assertion. The criterion explicitly accepts a deferral of this form; the residual risk it leaves open is stated below. |

**Totals: 15 PASS, 0 PARTIAL, 0 FAIL, 0 UNVERIFIED.**

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md
- Total AC items: 15
- Checked off (delivered): 15
- Remaining (unchecked): 0
- Items remaining: none
```

All fifteen checkboxes in the `spec.md` acceptance-criteria checklist were already marked `[x]` by the executor, and this audit confirms all fifteen on the evidence. No checkbox required a change, and none was modified by this review.

## Defect Resolution Against the Reported Symptom

The issue reported that Alt-bearing chords are swallowed by `QfcFormViewer.ProcessCmdKey`. The delivered change resolves the claim decision as follows, measured against the expected-versus-actual table in `spec.md`:

| Chord | Pre-change claim | Post-change claim | Resolves symptom |
|---|---|---|---|
| Alt (bare), synthetic `Keys.Alt` | claimed | claimed | preserved, as required |
| Alt (bare), physical `Keys.Menu \| Keys.Alt` | claimed | claimed | preserved, and now pinned by a test |
| Alt+M | claimed | released | yes |
| Alt+F4 | claimed | released | yes |
| Alt+arrow | claimed | released | yes |
| Non-Alt chords | not claimed | not claimed | unchanged |

`spec.md` corrects the issue body's assertion that Alt+F is also swallowed. That correction was verified: the QuickFiler form's `ButtonFilters.Text` is the plain string `"Filters"` with no ampersand, so there is no Alt+F mnemonic on this surface to restore, and no acceptance criterion asserts one. The correction is accurate and is properly documented rather than silently dropped.

## Residual Risk

**The user-facing outcome is not confirmed on a live host.** What the delivered tests establish is the claim decision — that `ClaimsAltChord` returns `false` for Alt+M and Alt+F4 and `true` for both shapes of bare Alt, and that `ProcessCmdKey` routes its entire decision through that predicate. What they do not establish is the downstream consequence: that releasing Alt+M actually causes WinForms mnemonic resolution to open a `&Move Options` drop-down, that releasing Alt+F4 actually closes the window, and which of the several `&Move Options` owners WinForms selects when multiple rows are loaded.

This is a correctly disclosed limitation rather than a gap in delivery. AC-15 was written to accept a deferral of exactly this kind, the deferral is justified by measured probes rather than by assertion, and `evidence/other/manual-validation.md` states the limitation in its own words. The mnemonic-ownership question is separately identified in the `spec.md` risk table with the disposition that, if the wrong row's menu opens, that is a distinct defect belonging to a follow-up issue.

**Recommended closing action for the maintainer:** open the QuickFiler form in a live Outlook session with at least one loaded row, press Alt, then Alt+M with a row focused, then Alt+F4, and append the observed outcome and the Outlook build to `evidence/other/manual-validation.md`.

## Baseline Comparison Summary

| Dimension | Baseline (`origin/main` @ `9ca9e99a`) | Head (`20f1b201`) | Delta |
|---|---|---|---|
| `.cs` files changed | — | 3 | +3, exactly the authorised set |
| Test methods in `QfcFormKeyHandlerTests` | 4 | 11 | +7, exactly the specified set |
| Repository test total | 6927 | 6934 | +7, matching |
| Repository test failures | 0 | 0 | no regression |
| `QfcFormKeyHandler` class line rate | 1 | 1 | no regression |
| Repository root line rate | 0.853866 | 0.853726 | −0.000140, above floor |
| Repository root branch rate | 0.794064 | 0.794078 | +0.000014, above floor |
| CSharpier files checked | 1566 | 1566 | unchanged, consistent with no file added |
| Analyzer warnings naming changed files | 0 | 0 | no regression |
| `ExcludeFromCodeCoverage` attributes in `.cs` | — | 0 added | none introduced |

## Verdict

**PASS. Blocking findings: 0. All 15 acceptance criteria confirmed on the evidence.**

No remediation inputs artifact is produced, because no finding requires remediation before merge.
