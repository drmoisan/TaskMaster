# Pull-Request Closing-Keyword Constraint (Issue 446 Family)

Timestamp: 2026-08-26T10-41

Task: [P4-T17]
Feature: docs/features/active/quickfiler-bug-family-446
Audience: the pull-request author for the branch `bug/quickfiler-bug-family-446`
PostedAs: unknown — this is a local constraint note for the pull-request author, not an issue
update. It has not been posted to GitHub.

## Constraint

The pull-request body for this change set carries GitHub closing keywords for **exactly three**
issue numbers:

- **#446**
- **#448**
- **#426**

No other issue number may be preceded by a closing keyword anywhere in the pull-request body.

## Issue #427 must remain open

`docs/features/active/quickfiler-bug-family-446/issue.md:5` reads:

```
- Also closes: #426, #427, #448
```

That line is **superseded by decision D1** and **must not be transcribed** into the pull-request
body. This change set delivers only the #427-A producer side — the scorer returns
`(long Score, string TopFolder)` and the accepted candidate's folder reaches the datamodel
boundary as `QfcDequeueBatch.PreScored`. The consumer side of #427 is not delivered here, so #427
must remain open after this pull request merges.

#427 may be referenced in the pull-request body as context (for example "advances #427" or
"partially addresses #427"), but never with `close`, `closes`, `closed`, `fix`, `fixes`, `fixed`,
`resolve`, `resolves` or `resolved` — with or without a trailing colon — immediately preceding it.

## Verification already performed

`[P4-T17]` scanned every path returned by
`git diff --name-only 61edc19befcf6c4e95b5acd32542f2dcdab41b78...HEAD` (105 paths) and the output
of `git log 61edc19befcf6c4e95b5acd32542f2dcdab41b78..HEAD --format=%B` (6 commits) for the
case-insensitive pattern `(close[sd]?|fix(es|ed)?|resolve[sd]?):? +#427`. Both scans returned
**zero matches**. See `evidence/qa-gates/p4-t17-ac17.*.md`.

The pull-request body is authored after this plan completes and is therefore outside the scope of
that scan. This note is the record of the constraint it must satisfy.

## Output Summary

Closing keywords in the pull-request body are limited to three issue numbers: #446, #448 and #426.
The `Also closes: #426, #427, #448` line in `issue.md` is superseded by decision D1 and is not
transcribed. Issue #427 remains open.
