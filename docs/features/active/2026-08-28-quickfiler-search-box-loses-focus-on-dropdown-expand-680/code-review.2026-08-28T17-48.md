# Code Review — Issue #680 (re-audit cycle after remediation)

- Date: 2026-08-28T17-48
- Reviewer: feature-review agent
- Branch: `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680` @ `c4e96b72b38fc122a8658ecbeff245814eef09bd`
- Base: merge-base `b0c7fa18a3beb073e7b051f49e28f48159f0f179`
- Scope: full branch diff (13 C#/build files re-read this cycle; production diff read in full), with emphasis on the delta since `code-review.2026-08-28T16-27.md` (commit `c4e96b72`)

## Prior-Finding Closure Verification

| Prior finding | Status | Verification |
|---|---|---|
| **CR-0 (Blocking)** — `BreadcrumbDropDownHost.cs` 514 > 500 lines | **CLOSED** | Re-measured at head this session: 498 lines (`awk 'END{print NR}'`; PowerShell `Get-Content .Count` corroborated by the executor's P4-T6 artifact). `BreadcrumbDropDownHost.Open.cs`: 107 lines. Both <= 500. |
| **CR-1** — two stale delivery-report statements post-rebase | **CLOSED** | `## Post-Rebase Addendum — 2026-08-28T19-30` appended to `delivery-report.2026-08-28T16-40.md`. Verified append-only in the `c4e96b72` diff (+26/-0 on that file); both correction lines present verbatim; the execution-time record is preserved unedited, as required. |
| **CR-2** — missing composed predicate-false restore test | **CLOSED** | `OpenAsync_TakeFocusReopenAfterNonCapturingOpenWithPredicateFalse_RestoresAutoCloseButSuppressesFocus` added to `BreadcrumbDropDownHostTests.Part3.cs` in the specified `PredicateHarness` style: non-capturing open, `AllowFocus = false`, `takeFocus: true` reopen, asserts `DropDown.AutoClose == true` AND `FocusPendingCount == 0`. Because `FocusPendingCount` counts the raw `_focusPending` delegate, the test pins both halves of the composition (unconditional #680 restore; effective #677 suppression). Green in `p2-t3.trx` (36/36), `p4-t4.trx`, and the reviewer's 64/64 scoped rerun at head. |

## Relocation Review (commit `c4e96b72`, production delta)

The relocation is a verbatim move and is behavior-preserving:

- **Textual identity.** The 7-line issue-#680 comment block, `internal void ShowPopup(Point location, bool takeFocus)` body (`DropDown.AutoClose = takeFocus; _showPopup(DropDown, Anchor, location);`), and `internal void PublishPopupMessengerReady() => PopupMessengerReady?.Invoke(this, EventArgs.Empty);` are byte-identical between the deletion hunk in `BreadcrumbDropDownHost.cs` and the insertion hunk in `BreadcrumbDropDownHost.Open.cs`.
- **Same type, same accessibility.** Both files declare `public sealed partial class BreadcrumbDropDownHost` in `namespace QuickFiler.Viewers`; both members remain `internal`. Partial-class relocation has no overload-resolution, accessibility, or ordering semantics to disturb.
- **Call sites untouched.** Repo-wide grep finds exactly two call sites — `_host.ShowPopup(...)` (line 276) and `_host.PublishPopupMessengerReady()` (line 355), both in `BreadcrumbDropDownOpenLifetime.cs` — and neither line is modified by `c4e96b72`.
- **Using directive.** `using System;` added (alphabetically first) for `EventArgs.Empty` — the single necessary supporting edit. The file's `#nullable enable` directive is retained and the members introduce no nullable warnings (nullable rebuild exit 0).
- **Thematic placement.** `.Open.cs` is the type's established ceiling-relief partial for open-path members; `ShowPopup` and the messenger-ready publisher are open-path members. #677-owned members (`MayTakeFocus`, `FocusPending`, `FocusAnchorIfPermitted`) were deliberately left in place, keeping ownership-aligned placement per the remediation inputs.

## Full-Diff Production Review (re-read this cycle)

The seven production files were re-read against merge-base. No new findings beyond cycle 16-27; the design remains as previously assessed:

- `BreadcrumbDropDownHost.Open.cs` — non-capturing open via `AutoClose = takeFocus` before the show delegate; already-open `takeFocus: true` branch schedules `DropDown.AutoClose = true; FocusPending();` (restore precedes the guarded focus call, making the restore unconditional with respect to #677's predicate — now pinned by the CR-2 test).
- `BreadcrumbDropDownHost.cs` — `FinishClose` restores `AutoClose = true` as the first `CompleteAll` operation, covering the programmatic, native-close, and open-failure paths at the single completion point.
- `BreadcrumbDropDownOpenLifetime.cs` — `takeFocus` threaded through `ShowCurrentSurface` to the host's `ShowPopup`.
- `QfcItemController.EventHandlers.cs` — Escape routes through the existing cancel path only when the drop-down is open; `TextBoxSearch_Leave` dismissal with the one-shot `_searchLeaveHandoffPending` latch (single producer / single consumer, read-and-clear, documented).
- `QfcItemController.EventWiring.cs` — symmetric subscribe/detach for `SearchLeave`.
- `IItemViewer.cs` / `ItemViewer.FolderSearch.cs` — additive `SearchLeave` + `IsFolderDropDownOpen` members with XML docs; thin forwarding in the coverage-exempt WinForms partial.

## Findings (this cycle)

| ID | Severity | Finding |
|---|---|---|
| RC-1 | **Blocking (policy, not code)** | Host-identity leak in the five remediation-cycle TRX files (`runUser="Megalodon4\DanMoisan"` in all five; 1240 raw `c:\users\danmoisan\...` paths in `p4-t4.trx`). Detailed in `policy-audit.2026-08-28T17-48.md` § 7 and routed via `remediation-inputs.2026-08-28T17-48.md` R2. The production and test code themselves are clean. |
| RC-2 | Major (non-blocking) | Remediation task-ID collision: the remediation plan's P2-T3 wrote its green-run TRX into `evidence/regression-testing/p2-t3/`, overwriting the feature plan's fail-before red-run TRX (27 total / 2 predicted failures) that backs AC-3's evidence chain. The red-run markdown transcription survives and the red TRX is recoverable at `8e82a2e0`; restore recommended alongside RC-1's sanitization pass. Root cause: two plans sharing one `P#-T#` namespace and one results-directory convention — future remediation plans should prefix remediation task directories (e.g., `r-p2-t3/`). |
| RC-3 | Minor (non-blocking) | Remediation evidence artifacts are future-dated: self-stamps 18-16 through 20-12 versus a 17:40 commit and a 17:48 wall clock at review time. Timestamps must be taken at task execution time per the evidence conventions. Content is corroborated; recording accuracy only. |
| RC-4 | Informational | Near-ceiling watch: `BreadcrumbDropDownHost.cs` 498/500, `BreadcrumbDropDownHostTests.cs` 499/500 (untouched by this branch), `QfcItemController.EventWiring.cs` 486/500. |

## Test Quality Assessment (new test)

The CR-2 composition test is well-constructed: descriptive name encoding the scenario; XML doc naming both composed issues; AAA structure with reason strings on every assertion; deterministic drain-based scheduling (no timers, no sleeps); and assertion power derived from counting the raw delegate rather than the wrapper, so a regression that bypasses the guard or skips the restore fails the test rather than passing vacuously. File remains at 427/500 lines.

## Verdict

Production and test code: **no blocking code findings**; the remediation delta is a clean mechanical relocation plus a well-aimed regression test. The single Blocking item this cycle (RC-1) is evidence hygiene, not code, and is mechanically fixable using the branch's own `72b4b7ed` sanitization treatment.
