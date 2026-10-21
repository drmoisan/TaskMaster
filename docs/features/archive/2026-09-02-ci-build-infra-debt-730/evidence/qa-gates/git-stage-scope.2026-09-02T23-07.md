# QA Gate — Staged Change Scope

- Task: [P2-T1]
- Phase: Phase 2 — Verification & Final QC

Timestamp: 2026-09-02T23-07

Command:
1. `git add Directory.Build.props .github/workflows/_build-analyzers.yml .github/workflows/_build-nullable.yml .github/workflows/_mstest-coverage.yml`
2. `git status --porcelain -- Directory.Build.props .github/workflows/_build-analyzers.yml .github/workflows/_build-nullable.yml .github/workflows/_mstest-coverage.yml`

EXIT_CODE: 0

## Output Summary

Full porcelain output of command 2 (scoped to the four Phase-1 pathspecs, so this plan file's own checkbox-edit modification and the evidence directory recorded in [P0-T5] fall outside this command's view):

```
M  .github/workflows/_build-analyzers.yml
M  .github/workflows/_build-nullable.yml
M  .github/workflows/_mstest-coverage.yml
A  Directory.Build.props
```

- Output line count: exactly 4.
- Staged path set: `A  Directory.Build.props`, `M  .github/workflows/_build-analyzers.yml`, `M  .github/workflows/_build-nullable.yml`, `M  .github/workflows/_mstest-coverage.yml`. This is the exact four-path set required by the acceptance condition; git emits them in its own sorted order, which differs from the order the acceptance condition lists them in but is the same set.
- No other entries appear.

Note on command 1's stderr: `git add` emitted `warning: in the working copy of 'Directory.Build.props', LF will be replaced by CRLF the next time Git touches it`. This is the repository's configured end-of-line normalization applying to the newly added file; it is informational, not an error, and command 1 exited 0.

## Acceptance

- Scoped porcelain output lists exactly the four staged paths and no other entries: PASS.
