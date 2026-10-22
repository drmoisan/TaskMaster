# Upstream Follow-Ups for the drm-copilot Repository — Issue #782

Timestamp: 2026-09-05T23-02

Command:

```powershell
git status --porcelain --untracked-files=all -- .claude
git diff --stat pre-782-base..HEAD -- .claude
```

EXIT_CODE: 0

Output Summary:

Two items surfaced by the #782 review belong to the drm-copilot repository rather than to this one.
Neither is fixed here, and this record exists so that neither is lost when this feature folder is
archived.

## Item 1 — finding S4-1: stale agent-memory notes

Notes under `.claude/agent-memory/task-researcher/` describe `UiThread.Dispatcher` as permanently
null in tests and as producing `NullReferenceException`. Both statements were true before PR #778
and are false after it: the accessor now throws `InvalidOperationException` synchronously when the
backing field has not been captured, and it never returns null.

The risk is that a future agent reading those notes reproduces the superseded mechanism in a plan or
an artifact. This delivery has already had to correct exactly that class of statement in three
`#584` passages under finding C19.

**Where it must be fixed:** the drm-copilot repository, under
`.claude/agent-memory/task-researcher/`.

## Item 2 — the S3-1 request to define `Timestamp:` semantics

The `evidence-and-timestamp-conventions` skill specifies the field as `Timestamp: <ISO-8601>` and
defines no semantics for which instant the value denotes. It does not say whether the value is the
instant the command ran, the instant the artifact was written, or the instant the work it records
completed.

That gap is what allowed the four #584 ordering passages corrected under finding S3-1 to assert an
execution order the recorded values could not establish. Those four passages are now restated
without the ordering claim, but the underlying ambiguity is unchanged and will recur in the next
delivery that compares two artifacts' timestamps.

**Where it must be fixed:** the drm-copilot repository, in the
`evidence-and-timestamp-conventions` skill.

## Why neither is fixed in this repository

Both items live under `.claude/`, which is overwritten by push-down from drm-copilot with zero
templating. An edit made in this repository is silently lost at the next push-down, and the loss is
invisible: the file simply reverts, with no conflict and no diff to review.

This delivery therefore modifies nothing under `.claude/`. That is verified rather than asserted:
P6-T3 runs `git diff --stat pre-782-base..HEAD -- .claude` and
`git status --porcelain --untracked-files=all -- .claude` and requires both to produce zero lines of
output. The porcelain span is required alongside the diff because `.claude/agent-memory/` is a
tracked directory in this repository, so an untracked addition there would be invisible to the diff
alone.

## Recommended action

Open one follow-up in drm-copilot covering both items. They share a cause — a record that outlived
the state it described — and they share a fix location.
