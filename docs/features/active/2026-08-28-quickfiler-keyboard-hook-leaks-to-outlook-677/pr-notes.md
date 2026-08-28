# PR Notes — QuickFiler keyboard input leaks to native Outlook windows (#677)

- **Issue:** #677 (https://github.com/drmoisan/TaskMaster/issues/677)
- **Branch:** `bug/quickfiler-keyboard-hook-leaks-to-outlook-677`
- **Baseline commit:** `361a49b884a4e3fe192bf04bae05151c598398fa`
- **Work Mode:** full-bug
- **Spec:** `spec.md` v0.3 (acceptance-criteria source)

## Summary — a two-part focus fix

While QuickFiler is open, clicking into a native Outlook window leaves Outlook unresponsive to
keyboard input. The mouse still works, other applications are unaffected, and closing QuickFiler
restores typing. This change fixes it in two coordinated parts. `KeyboardHandler` is **not**
modified.

### Part 1 — execution-time focus-permission predicate on `BreadcrumbDropDownHost`

`BreadcrumbDropDownHost` gains an additive internal settable property
`internal Func<bool> MayTakeFocus { get; set; } = () => true;`. It is consulted at exactly three
execution-time sites:

- `FinishClose` now hands `CompleteAll` a private `FocusAnchorIfPermitted()` wrapper instead of the
  raw `_focusAnchor` field. Only the focus step is gated; the `_cancelSelection` step is not.
- `internal void FocusPending()` now reads `if (MayTakeFocus()) _focusPending();`, which guards the
  fresh-open focus completion without editing `BreadcrumbDropDownOpenLifetime.Focus.cs`.
- `BreadcrumbDropDownHost.Open.cs` schedules `FocusPending` instead of the raw `_focusPending`
  delegate, so the already-open refocus is guarded at execution time too.

Because the predicate is *invoked inside each guard body when the scheduled action runs*, a caller
that reassigns it after scheduling is honored. That is the whole point: the defect lives in the gap
between scheduling a focus action and executing it.

`ItemViewer.Breadcrumb.cs` supplies the production predicate as
`host.MayTakeFocus = MayRestoreBreadcrumbFocus;`, implemented as
`Form form = FindForm(); return form != null && ReferenceEquals(Form.ActiveForm, form);`.

**No constructor signature changed.** The six-constructor chain keeps its baseline arities, which is
what lets five existing reflection-bound test harnesses continue to bind unmodified.

### Part 2 — deactivate-time focus parking and selector cancellation

`IQfcFormViewer` gains three additive Seam-B members — `event EventHandler FormDeactivated` (forwarded
to `Form.Deactivate`), `bool IsWebView2Focused` (walks the `ActiveControl` chain to its leaf), and
`void ParkFocusOffWebView2()` (parks focus on the OK button). A new partial,
`QuickFiler/Controllers/QfcFormController.Deactivate.cs`, handles the event: park focus off any
focused WebView2, then cancel every item's breadcrumb selector inside a per-item try/catch that logs
via the class's log4net logger. The cancel chain is
`IQfcItemController.CancelBreadcrumbSelector()` → `QfcItemController` →
`IItemViewer.CancelBreadcrumbSelector()` → `ItemViewer` → `BreadcrumbCoordinator?.CancelSelector()`.

## Root-cause synopsis

Full findings: `research/2026-08-28T09-15-quickfiler-outlook-keyboard-suppression-677-research.md`.

The reporter's hypothesis — that QuickFiler's keyboard hooking is scoped to the whole Outlook
process — is **refuted in its literal form and confirmed in spirit**. `KeyboardHandler` is ordinary
WinForms `PreviewKeyDown`/`KeyDown` wiring strictly confined to QuickFiler's own control tree,
instantiated per launch and never static, so it cannot receive events from native Outlook windows.
No global hook, message filter, `Form.KeyPreview`, or window disabling exists anywhere in the repo.

The mechanism is focus routing. Every visible `ItemViewer` row hosts two WebView2 instances on
Outlook's shared UI thread, plus a lazily created popup WebView2. `FinishClose` unconditionally
re-focused the anchor WebView2 on **every** close, including the `OnDropDownClosed` event WinForms
raises when the user clicks *out* of QuickFiler — and it did so asynchronously, so the re-focus
landed after the click into Outlook. Once a WebView2 holds thread keyboard focus in a VSTO WinForms
host, the runtime does not reliably release it (MicrosoftEdge/WebView2Feedback issue #951, open).
Since Outlook and QuickFiler share one UI thread and one input queue, every keystroke typed "into"
Outlook was delivered to that browser surface instead.

A secondary contributor, active only while the popup is open, is WinForms modal menu mode: an
`AutoClose = true` `ToolStripDropDown` in a hosted (no `Application.MessageLoop`) process makes the
framework install a thread-scoped message hook that redirects keyboard messages for the whole
Outlook UI thread. Part 2 ensures no popup can outlive form deactivation.

## Risks and mitigations (from `spec.md`)

| Risk | Mitigation |
|---|---|
| **Predicate evaluated at the wrong time.** The steal arises precisely from the scheduling/execution gap; a predicate captured at scheduling time would hold a stale `true` and reintroduce it. | The predicate is invoked inside each guard body at execution time. Pinned by `FinishClose_PredicateFlipsFalseAfterScheduling_DoesNotFocusAnchor`, which asserts `context.PendingCount > 0` to prove the work was queued, flips the predicate, then drains. |
| **Over-broad focus parking** would break the Escape/commit-returns-to-breadcrumb behavior delivered under #438/#400. | `Form.ActiveForm` (not `ContainsFocus`) is the predicate, so it is true on in-form paths and false only once activation has left. Parking is scoped strictly to `Form.Deactivate`. Control tests `FinishClose_PredicateTrue_FocusAnchorInvoked`, `AlreadyOpenRefocus_PredicateTrue_FocusPendingInvoked` and `UnsetPredicate_DefaultsTrue_FocusAnchorStillInvoked` assert the preserved behavior, and all 21 pre-existing `BreadcrumbDropDownHostTests` methods pass unmodified. |
| **An exception escaping the deactivate handler** would surface as an unhandled Outlook UI-thread failure. | Per-item try/catch with log4net error logging, pinned by `FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues`, which also asserts the remaining items are still cancelled. |
| **Additive interface members** could break unlisted implementers. | Interface-implementer sweep (decision D8) verified each interface's implementer set. The only manual test double needing a member is `FakeQfcItemController`; every other double is a loose Moq mock. |

## Validation performed

### Phase 4 — regression verification

| Artifact | Result |
|---|---|
| `evidence/regression-testing/p1-t5-expectfail-build.md` | `[expect-fail]` compile-red gate. EXIT_CODE 1 (ExpectedExitCode 1), 22 diagnostics, all in the three new test files, naming all five absent guard members. |
| `evidence/regression-testing/p1-t5-expectfail-build.msbuild.txt` | Full teed msbuild output for the above. |
| `evidence/regression-testing/fail-before-exception.2026-08-28T15-55.md` | Fail-before exception dossier with `WhyFailingRunImpossible:`. |
| `evidence/regression-testing/p3-t10-greenflip-build.md` | Green flip. EXIT_CODE 0, 0 errors. Diff-scope proof that the flip came from production code. |
| `evidence/regression-testing/p3-t10-greenflip-build.msbuild.txt` | Full teed msbuild output for the above. |
| `evidence/regression-testing/p4-t1-summary.md` + `p4-t1/p4-t1-breadcrumbdropdownhosttests.trx` | 29/29 passed (8 new + 21 pre-existing). |
| `evidence/regression-testing/p4-t2-summary.md` + `p4-t2/p4-t2-controller-deactivate-and-cancel.trx` | 9/9 passed. |
| `evidence/regression-testing/p4-t3-summary.md` + `p4-t3/p4-t3-quickfiler-test-whole-assembly.trx` | 1218/1218 passed = baseline 1201 + 17 new. |
| `evidence/qa-gates/keyboardhandler-unchanged.md` | AC-6 invariant: both git commands empty. |

### Phase 5 — final QA loop (one clean pass; the loop never restarted)

| Artifact | Result |
|---|---|
| `evidence/qa-gates/final-format.md` | `csharpier format .` EXIT_CODE 0, repo-wide branch, content no-op. |
| `evidence/qa-gates/final-format-check.md` | `csharpier check .` EXIT_CODE 0, 1558 files, zero violations. |
| `evidence/qa-gates/final-analyzer-build.md` | Analyzer rebuild EXIT_CODE 0, 0 errors, 5 warnings (zero delta versus baseline). |
| `evidence/qa-gates/final-nullable-build.md` | Nullable rebuild EXIT_CODE 0, 0 errors, zero `CS86xx`. |
| `evidence/qa-gates/coverage-final.md` + `coverage-final.cobertura.xml` | Full suite 6838/6838 passed, line-rate 0.852804. |
| `evidence/qa-gates/coverage-delta.md` | 100% changed-line coverage on all five files; repo line-rate **+0.000083** versus baseline. |
| `evidence/qa-gates/file-size-audit.md` | Max file 498 lines; no stray untracked `.cs`. |

Baseline evidence for comparison lives under `evidence/baseline/` (policy reads, repo baseline, SDK
bootstrap, tool restore, formatting baseline, NuGet restore, analyzer baseline, nullable baseline,
coverage baseline, scope lock, and the `QFT_BASELINE_TOTAL` run).

## Rollback

Single-commit revert, no data or configuration cleanup. The predicate's `() => true` property
initializer **is** the pre-fix behavior, so reverting Part 1 restores the old semantics exactly, and
Part 2 is entirely additive surface plus one new partial file and two subscription lines.

## Outstanding

Acceptance criteria **AC-1**, **AC-2**, and the manual half of **AC-3** remain unchecked pending
manual verification in a live Outlook session. The checklist is in
`evidence/other/manual-verification-pending.md`. Live WebView2-runtime focus retention and actual
Outlook keystroke delivery are not unit-testable without violating the repository's determinism and
no-external-process test policies.

## PR creation

PR creation goes through the **`pr-author` skill** and is **orchestrator-gated**. This plan does not
run `gh pr create` or `gh pr edit`, and no GitHub write of any kind was performed during execution
(see `evidence/issue-updates/issue-677.md`, recorded as `PostedAs: unknown` with a
`POSTING BLOCKED` header).
