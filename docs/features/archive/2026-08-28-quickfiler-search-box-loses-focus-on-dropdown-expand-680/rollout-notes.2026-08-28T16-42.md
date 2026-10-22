# Rollout and Follow-up Notes — Issue #680

Timestamp: 2026-08-28T16-42

## Rollout

Single-PR delivery. No phased rollout, no feature flag, no configuration key, and no migration. The
change is confined to the QuickFiler search-box open/dismiss lifecycle; `AutoClose` retains its
current `true` behaviour everywhere outside the `takeFocus: false` branch, so a revert is a
single-commit revert.

## Post-merge action — owned by the repository owner

Perform the human-verification runbook at
`evidence/other/hv-runbook-680.2026-08-28T16-12.md` in a live Outlook session with the add-in loaded,
and record the completed checklist as a new artifact under
`docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/other/`.

Spec AC-1 and AC-2 remain unchecked until that outcome is recorded. They are not dischargeable by any
automated test: WinForms menu-mode engagement and live keyboard-message retargeting require a real
message pump, a real popup window, and a live WebView2 surface.

The runbook's nine items cover continuous multi-character typing (AC-1), the unchanged gesture paths —
Down-arrow handoff, mouse toggle, row click, outside-click dismissal, Escape (AC-2, per #400/#438) —
and the two composition risks recorded as DR-8 in the plan:

- **Risk 1 (HV-7)** — an outside click after a Down-arrow handoff, where `AutoClose` is restored to
  `true` on a popup that is already visible. Menu mode is entered inside `SetVisibleCore(true)`, so it
  is not retroactively engaged for that showing and outside-click dismissal on this specific state may
  differ from a fresh gesture open.
- **Risk 2 (HV-9)** — a mouse click directly on a result row of a search-driven, non-capturing popup.
  The click moves Win32 focus off the textbox and raises `Leave`; the controller's suppression latch is
  armed only by the `Keys.Down` branch, so an unsuppressed `Leave` would cancel the very selection the
  click is making.

## Conditional fallback — owned by the orchestrator

If the human-verification step surfaces `AutoClose`-toggling fragility — including either DR-8 case —
do not patch this fix in place. Promote the **borderless-`Form` popup rewrite** (replacing
`ToolStripDropDown` entirely, so no menu filter is involved) through the MCP promotion lifecycle as its
own issue. That rewrite was evaluated during #680 research and rejected as non-minimal for this fix,
but it remains the viable long-term option. Promoting it through the promotion lifecycle rather than
leaving it as prose in a feature folder is required, because feature-folder prose disappears at merge.

## Cross-issue record: issue #677

This fix discharges the "WinForms modal-menu-mode contributor" follow-up item from issue #677's spec
Rollout & Follow-up section, which recorded that contributor as asserted but not verified. The #680
research verified it at `dotnet/winforms` framework-source level and this change fixes it.

No `docs/features/**/*677*` tracking folder exists in this worktree, so the discharge record is carried
by `spec.md`'s Rollout & Follow-up section, by this file, and by the pull-request body — not by an
in-repo #677 folder.

Issue #677's own `MayTakeFocus` / `Deactivate` machinery had **not** merged into this branch's base at
implementation time (verified in `evidence/other/base-state-677.2026-08-28T15-20.md`: zero occurrences
of `MayTakeFocus` under `QuickFiler/` or `QuickFiler.Test/`, and no `Deactivate` handler). This change
was therefore authored against the pre-#677 shape. When #677 (PR #684) merges, its `MayTakeFocus`
guard gates *managed focus-taking calls* while this change gates *framework menu-mode entry*; the two
are orthogonal and compose. Whichever change merges second should confirm that composition rather than
assume it.

## Traceability

- Issue #680: https://github.com/drmoisan/TaskMaster/issues/680
- Research: `research/2026-08-28T11-00-quickfiler-search-box-focus-loss-680-research.md`
- Archived issue #438 (`quickfiler-search-keystroke-focus-steal`), whose deferred HV-1 negative
  outcome this issue is: `docs/features/archive/2026-08-07-quickfiler-search-keystroke-focus-steal-438/`
- Related: issue #677 / PR #684 — same host file region, distinct mechanism, composition dependency.
- Delivery report: `delivery-report.2026-08-28T16-40.md`
