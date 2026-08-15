# potential-to-issue-promoted-copy-not-written (Issue #554)

- Date captured: 2026-08-14
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/potential-to-issue-promoted-copy-not-written/ (Issue #554)
- Severity: Medium
- Discovered during: orchestration of issue #553 (CI parallel job split)

- Issue: #554
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/554
- Last Updated: 2026-08-14
## Summary

`mcp__drm-copilot__potential_to_issue` returns a success receipt naming a
`destination_path` under `docs/features/potential/promoted/`, but no file is written
to that path. The source potential entry is removed from
`docs/features/potential/`, so the on-disk potential document is destroyed while the
receipt asserts it was relocated.

## Observed Behavior

Promoting `docs/features/potential/2026-08-14-ci-parallel-job-split.md` returned:

```json
{
  "ok": true,
  "tool": "potential_to_issue",
  "summary": "Promoted '.../docs/features/potential/2026-08-14-ci-parallel-job-split.md' as a feature workflow in full-feature mode.",
  "artifacts": ["https://github.com/drmoisan/TaskMaster/issues/553"],
  "destination_path": "C:/Users/DanMoisan/repos/TaskMaster-wt/2026-08-14T09-01/docs/features/potential/promoted/2026-08-14-ci-parallel-job-split.md"
}
```

After the call:

- `docs/features/potential/2026-08-14-ci-parallel-job-split.md` — absent.
- `docs/features/potential/promoted/2026-08-14-ci-parallel-job-split.md` — absent.
- `find docs -name "*ci-parallel-job-split*"` returns only the active feature folder
  and its research artifact.
- `git status --porcelain` shows no deletion, because the potential entry had not yet
  been committed. A committed entry would presumably show as a deletion instead.

The `promoted/` directory itself exists and holds many older entries (for example
`2026-08-08-ribbon-async-getpressed-signature.md`), so the directory is not missing
and the convention is otherwise in use.

## Expected Behavior

Either:

1. The file is actually written to the reported `destination_path`, matching the
   receipt; or
2. The receipt does not report a `destination_path` that was not written.

A success receipt must not assert a filesystem outcome that did not occur.

## Impact

Content loss is possible. In the observed case the content survived only because it
had independently been copied into the GitHub issue body and into the active feature
folder's `issue.md`. A promotion that failed earlier in its sequence, or one whose
issue creation did not carry the full body, would lose the authored analysis with no
warning and a green receipt.

The receipt is also consumed as promotion evidence and persisted verbatim into
`artifacts/orchestration/orchestrator-state.json` under
`delegation_receipts.promotion.issue`. A checkpoint therefore records a path that does
not exist, which weakens the audit trail.

## Reproduction

1. Create a potential entry via `mcp__drm-copilot__new_potential_entry`.
2. Add content to it.
3. Promote it via `mcp__drm-copilot__potential_to_issue`.
4. Check both the source path and the reported `destination_path`. Neither holds the
   file.

Observed with the bundled drm-copilot MCP server in the TaskMaster worktree
`C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-14T09-01` on 2026-08-14.

## Acceptance Criteria

- [ ] Promotion writes the potential document to `docs/features/potential/promoted/`
      at the exact path reported as `destination_path`, or the receipt stops reporting
      a `destination_path` it did not write.
- [ ] The source document is not removed unless the destination write has succeeded.
- [ ] A regression test asserts that after a successful promotion, a file exists at the
      receipt's `destination_path`.
- [ ] The failure mode is surfaced as a non-`ok` receipt rather than a silent
      inconsistency.

## Next Step

- [ ] Promote to GitHub issue (bug template)
