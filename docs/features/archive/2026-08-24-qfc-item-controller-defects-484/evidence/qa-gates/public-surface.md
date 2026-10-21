# Public-surface stability of the four owned production files

Timestamp: 2026-08-26T11-13
Task: [P6-T5]

Commands (run from the worktree root; `<BASE_SHA>` is the `[P0-T3]` value
`61edc19befcf6c4e95b5acd32542f2dcdab41b78`):

```
git diff 61edc19befcf6c4e95b5acd32542f2dcdab41b78 -- <the four owned production files> | grep -E "^-" | grep -E "(public|internal|private|protected)\s"
git diff 61edc19befcf6c4e95b5acd32542f2dcdab41b78 -- <the four owned production files> | grep -E "^\+\s+public "
grep -n "<member name>" <the four owned production files>
```

EXIT_CODE: 0

## Members added (nine, all production members from the plan's literals list)

| # | Member | Declared accessibility | Declaration site |
|---|---|---|---|
| 1 | `UnwireEvents` | `internal` | `QuickFiler/Controllers/QfcItemController.EventWiring.cs:392` |
| 2 | `UnwireControlTreeEvents` | `internal` | `QuickFiler/Controllers/QfcItemController.EventWiring.cs:399` |
| 3 | `UnwireIntentEvents` | `internal` | `QuickFiler/Controllers/QfcItemController.EventWiring.cs:445` |
| 4 | `MoveFailureNotifier` | `internal` | `QuickFiler/Controllers/QfcItemController.MailActions.cs:30` |
| 5 | `NotifyMoveFailure` | `private` | `QuickFiler/Controllers/QfcItemController.MailActions.cs:35` |
| 6 | `TryResolveCidResource` | `internal` (`static`) | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:215` |
| 7 | `_webResourceRequestedHandler` | `private` | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:33` |
| 8 | `_coreWebView2` | `private` | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:34` |
| 9 | `DetachWebResourceRequestedHandler` | `private` | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:486` |

These are exactly the nine production members named in the plan's "Literals this plan instructs the
executor to create" section. The remaining literals in that section are test method names, which are
not production members.

## Members removed

The diff filter for removed declaration lines produced **zero** output lines across all four files. No
member was removed from any of the four owned production partials.

## Public members added

The diff filter for added `public` declaration lines produced **zero** output lines. Every added member
is `internal` or `private`. Six are `private` or `internal` methods, one is an `internal` auto-property
with an initializer, and two are `private` fields.

Output Summary: Nine members added, all `internal` or `private`; zero members removed; zero `public`
members added. The public surface of the four owned production partials is unchanged.
