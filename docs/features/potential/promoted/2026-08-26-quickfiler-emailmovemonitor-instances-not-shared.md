# quickfiler-emailmovemonitor-instances-not-shared (Issue #620)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-emailmovemonitor-instances-not-shared/ (Issue #620)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #620
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/620
- Last Updated: 2026-08-26
## Summary


`UnhookItem` can never release a hook that `QfcDatamodel` registered, because three collaborators each construct their own independent `EmailMoveMonitor` rather than sharing one. `QuickFiler/Controllers/QfcDatamodel.cs:103`, `QuickFiler/Controllers/QfcQueue.cs:40` and `QuickFiler/Controllers/QfcCollectionController.cs:78` each build a separate instance, so the `UnhookItem` call at `QuickFiler/Controllers/QfcQueue.cs:76` consults a hook list that structurally cannot contain the datamodel's registrations. Any unhook routed through `QfcQueue` is therefore a silent no-op against datamodel-registered items, and the monitor retains those hooks for the life of the object.

## Environment

- OS/version:
- Python version:
- Command/flags used:
- Data source or fixture:

## Steps to Reproduce


1. Register a mail item hook through `QfcDatamodel`, which uses the monitor constructed at `QuickFiler/Controllers/QfcDatamodel.cs:103`.
2. Route an unhook for that same item through `QfcQueue`, which calls `UnhookItem` at `QuickFiler/Controllers/QfcQueue.cs:76` against the distinct monitor constructed at `QuickFiler/Controllers/QfcQueue.cs:40`.
3. Inspect the datamodel monitor's hook list and observe the hook is still present.

## Expected Behavior


The unhook releases the item, whichever collaborator registered it.

## Actual Behavior


The unhook is a no-op. The two monitors are separate objects with separate hook lists, so the lookup cannot match.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet:

## Impact / Severity


- [x] Medium

Correctness and lifetime issue rather than a crash. Hooks accumulate and are never released through this path.

## Suspected Cause / Notes


Discovered during issue #446 (`docs/features/active/quickfiler-bug-family-446`). Out of scope there: #446 fixed the datamodel's own unhook path by calling its own monitor directly, which is correct but does not consolidate the instances. Consolidating ownership is a separate cross-cutting change to three files that were not in #446's owned set.

## Proposed Fix / Validation Ideas


- [ ] Establish a single shared `EmailMoveMonitor` (constructor injection or a shared owner) consumed by all three collaborators
- [ ] Unit coverage asserting a hook registered via one collaborator is released by an unhook routed through another
- [ ] Verify no double-unhook or double-dispose is introduced by the consolidation

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
