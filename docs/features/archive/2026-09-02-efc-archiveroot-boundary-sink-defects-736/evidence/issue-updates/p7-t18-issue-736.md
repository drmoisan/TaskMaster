# P7-T18 — Issue #736 update mirror

Timestamp: 2026-09-04T02-45

PostedAs: body

The exact text written into the `## Proposed Fix / Validation Ideas` section of this feature
folder's issue.md:

```
## Proposed Fix / Validation Ideas

- [x] Wrap the two COM reads in `ArchiveRootPath`'s getter in a try/catch that routes through the existing guard/sink pattern instead of throwing uncaught
- [x] Add try/catch to both `KbdExecuteAsync` overloads, routing caught exceptions through `BoundaryErrorSink`
- [ ] Reorder `ActionOkAsync` so disposal happens before (or is guaranteed via `finally` regardless of) the form-hide step
- [x] Give `BoundaryErrorSink`'s default implementation a user-facing surface (e.g. a non-blocking notification), not just a log call
- [x] Route the five `_globals.Ol.ArchiveRootPath` reads through a guarded accessor once finding 1 is fixed
- [x] Update `EfcDataModelArchiveRootTests.cs:182` to assert the new guarded/handled behavior instead of asserting a crash, once findings 1 and 5 land

The `ActionOkAsync` disposal reordering is out of scope for this item and is owned by a sibling item.
```

## Section delimitation and counts

The section is delimited mechanically as the lines between the heading line
`## Proposed Fix / Validation Ideas` and the next line beginning with two hash characters and a
space, which is `## Next Step`.

| Observation | Value | Required |
|---|---|---|
| Lines within that span matching the fixed string `- [x] ` | **5** | exactly 5 |
| Lines within that span matching the fixed string `- [ ] ` | **1** | exactly 1 |
| The single unticked line contains the token `ActionOkAsync` | **yes** | yes |

The five ticked boxes are the five in-scope findings: the archive-root guard (finding 1), the
`KbdExecuteAsync` handling (finding 2), the user-facing sink default (finding 4), the five
archive-root reads (finding 5), and the data-model test rewrite (finding 6). The one unticked box is
finding 3, the `ActionOkAsync` disposal reordering, which is out of scope for this item and owned by
a sibling item; the appended sentence states that.

## Mirroring

This artifact is the local mirror of a `PostedAs: body` update. The update was written into the local
feature folder's issue.md, which is the authoritative copy inside this worktree. Posting to GitHub is
the orchestrator's step; this executor does not create or edit the GitHub issue, and no GitHub URL is
recorded here because none was obtained.
