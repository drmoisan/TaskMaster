# configurehost-generation-guard-drops-incoming-host (Potential Bug)

- Date captured: 2026-08-28
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

`BreadcrumbItemViewerLifecycleCoordinator.ConfigureHost` posts its adoption work through the UI
dispatcher and opens that posted lambda with a generation guard. When `_generation` advanced between the
schedule and the run, the lambda returns early and the **incoming** host — already constructed by
`ItemViewer.ConfigureBreadcrumbDropDown` — is dropped without being disposed. This is defect D1c from
the #488 research, out of scope for #488 and recorded here so the follow-up does not have to re-derive
it.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (.NET Framework 4.8.1, VSTO / WinForms)
- Command/flags used: n/a — identified by source reading during issue #488 research and execution
- Data source or fixture: `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`

## Steps to Reproduce

1. Drive `BreadcrumbItemViewerLifecycleCoordinator.ConfigureHost` with a queued, non-inline
   synchronization context so the posted lambda is observable before it runs.
2. Advance the coordinator's generation before draining — through `Reset()` or `Dispose()`, per the two
   triggers below.
3. Drain the queue and observe that the generation guard returns early, leaving the host constructed for
   this call unreferenced and undisposed.

## Expected Behavior

A host the coordinator declined to adopt should be disposed on the early-return path, since nothing
else holds a reference to it.

## Actual Behavior

The posted lambda returns without disposing the incoming host. The host owns a `ToolStripDropDown` and,
once opened, a WebView2-backed surface, so the drop is a real resource leak rather than a bookkeeping
gap.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no captured log; identified by code reading, recorded in the #488 research §3.6 and in
  `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/spec.md` under
  "Out of scope / non-goals" item 1.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The window requires a generation advance between the schedule and the run, which on the production UI
thread does not arise because every post runs inline. The severity comes from what leaks — a native
popup and potentially a WebView2 surface — rather than from the likelihood.

## Suspected Cause / Notes

**Mechanism.** The generation guard exists to discard stale adoption work after a reset or disposal. It
correctly declines to adopt, but the host it declines was constructed by the caller and handed over on
the assumption that the coordinator would take ownership. Nothing else disposes it.

**Triggers.** Two paths advance the generation:
- `Reset()`, reached from `QfcItemController.Cleanup()` through the viewer-setup teardown path.
- `Dispose()`.

**Why it was out of scope for #488.** It leaks the **incoming** host, which is a different defect from
the one #488 defect D1 filed — D1 concerns the **outgoing** host's disposal ordering. Adding a
`host.Dispose()` to a branch no current test exercises would have been an unpinned behaviour change
inside a bugfix change-set.

Files to inspect: `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a test that queues `ConfigureHost`, advances the generation before draining, and asserts the incoming host was disposed
- [ ] Integration scenario to retest: reset-then-reconfigure and dispose-then-reconfigure sequences
- [ ] Manual verification notes: confirm the fix does not double-dispose a host that was adopted normally

## Next Step

Promote to a GitHub issue against `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`.
