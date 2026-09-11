# breadcrumb-partial-class-line-count-comments-stale (Issue #862)

- Date captured: 2026-09-11
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-partial-class-line-count-comments-stale/ (Issue #862)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #862
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/862
- Last Updated: 2026-09-11
## Summary

Two partial-class XML doc comments state the line count of their sibling base file, and both counts are wrong. One understates a file that is now 3 lines from the repository's 500-line ceiling, so a reader consulting the comment is told the file has comfortable headroom when it does not.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# source comments)
- Command/flags used: `git grep -c "" main -- <path>` (the allowlisted line-count oracle)
- Data source or fixture: `main` at 3cb9744228f93f9d909511ad9a2cc9aeed8a9340

## Steps to Reproduce

1. Read the class-level XML doc comment in `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs`, which states that `BreadcrumbBridgeCoordinator.cs` is 487 lines.
2. Read the class-level XML doc comment in `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs`, which states that `BreadcrumbItemViewerLifecycleCoordinator.cs` is 481 lines.
3. Measure both base files on `main` with `git grep -c "" main -- QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` and the same for `BreadcrumbItemViewerLifecycleCoordinator.cs`.

## Expected Behavior

A comment that cites a line count as the justification for a partial-class split states the current count, or does not state a number at all.

## Actual Behavior

Both counts are stale, and they drift in opposite directions:

| Base file | Comment claims | Measured on `main` | Delta | Headroom to 500 |
|---|---|---|---|---|
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 487 | 437 | -50 | 63 |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 481 | 497 | +16 | **3** |

The second is the harmful one. The comment says 481 and "stays clear of the repository's 500-line ceiling", while the file is actually 497 lines, leaving 3 lines of headroom. Any contributor who trusts the comment will believe there is roughly 19 lines of room and can breach the ceiling in a single small edit.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet:

```
$ git grep -c "" main -- QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs
437
$ git grep -c "" main -- QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs
497
```

Comment text on `main`:

```
BreadcrumbBridgeCoordinator.Search.cs:11
    /// Held on a second partial-class part so <c>BreadcrumbBridgeCoordinator.cs</c> (487 lines)
    /// stays clear of the repository's 500-line ceiling.

BreadcrumbItemViewerLifecycleCoordinator.Search.cs:10
    /// <c>BreadcrumbItemViewerLifecycleCoordinator.cs</c> (481 lines) stays clear of the
    /// repository's 500-line ceiling.
```

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: no runtime effect. The impact is that a documented safety margin against a policy limit in the General Code Change Policy is misreported, on a file with 3 lines of real headroom.

## Suspected Cause / Notes

A hard-coded count in a comment cannot survive edits to the file it describes. Both files have changed since the comments were written, under issue #438 and the later QuickFiler work.

This was found during the 2026-09-11 worktree-cleanup triage. On `main` it is recorded only as prose inside a closed feature folder at `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p4-t3-r4-sibling-fence.md`, which explicitly fences the two files as out of scope for issue #823. No potential entry or issue named them before this one.

Note that the stale counts sit in the `.Search.cs` partial parts but describe the base files; the base files themselves are not the ones to edit.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: none applicable; this is comment text with no executable behavior.
- [ ] Integration scenario to retest: n/a.
- [x] Manual verification notes: prefer removing the parenthetical count from both comments rather than refreshing it, since a refreshed number goes stale again on the next edit. The justification ("held on a second partial-class part to stay clear of the 500-line ceiling") stands without a number. If a number is kept, re-derive it with `git grep -c ""` and add a gate that re-checks it.

Edit `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs` and `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs`.

Consider separately whether `BreadcrumbItemViewerLifecycleCoordinator.cs` at 497 lines warrants its own split, independent of the comment fix.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
