# P0-T9 — Git Baseline (Issue #751)

Timestamp: 2026-09-03T14-24

Branch: `bug/terminal-notification-hook-test-lacks-sync-barrier-751`
Branch-point ref (BASE convention): `f8414ee9`

## Command 1

Command: `git status --porcelain`
EXIT_CODE: 0

Verbatim output:

```
?? docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/
```

This is the **unscoped** porcelain output and is the reference set that P5-T14 part 2 compares against.
It consists of exactly one line, naming the untracked `evidence/` directory created earlier in Phase 0 by
P0-T7 and P0-T8.

The repo-local .NET SDK directory `.dotnet-sdk/` and the NuGet `packages/` directory, both created by
P0-T8 immediately before this capture, do **not** appear: they are ignored by `.gitignore`.

## Command 2

Command: `git diff --stat f8414ee9..HEAD`
EXIT_CODE: 0

Verbatim output:

```
 .../issue.md                                       |  92 ++++
 .../plan.2026-09-03T11-48.md                       | 303 +++++++++++++
 ...inal-notification-hook-sync-barrier-research.md | 495 +++++++++++++++++++++
 .../spec.md                                        | 410 +++++++++++++++++
 4 files changed, 1300 insertions(+)
```

The default `--stat` width abbreviates the paths, which would make the P4-T8 cross-check unreadable. The
same diff was therefore additionally captured at full path width. This is a presentation of the identical
diff, not a second or different measurement.

Command: `git diff --stat=200 f8414ee9..HEAD`
EXIT_CODE: 0

Verbatim output:

```
 docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/issue.md                                          |  92 +++++++++++++
 docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/plan.2026-09-03T11-48.md                          | 303 ++++++++++++++++++++++++++++++++++++++++
 .../research/2026-09-03T09-45-terminal-notification-hook-sync-barrier-research.md                                             | 495 ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
 docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/spec.md                                           | 410 ++++++++++++++++++++++++++++++++++++++++++++++++++++++
 4 files changed, 1300 insertions(+)
```

Command: `git diff --name-only f8414ee9..HEAD`
EXIT_CODE: 0

Verbatim output:

```
docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/issue.md
docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/plan.2026-09-03T11-48.md
docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/research/2026-09-03T09-45-terminal-notification-hook-sync-barrier-research.md
docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/spec.md
```

## What the branch already carries relative to the branch point

Four files, all of them Markdown documents under
`docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/`, totalling 1300 inserted
lines and zero deletions. No source file, no production file, and no path outside the feature folder is
present in the branch diff before any edit made by this plan.

## Acceptance

| Required | Observed | Result |
|---|---|---|
| `git status --porcelain` printed no line naming any path under `TaskMaster/` or `TaskMaster.Test/` | The single printed line names `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/`. No `TaskMaster/` or `TaskMaster.Test/` path appears. | PASS |
| `git diff --stat f8414ee9..HEAD` output recorded | Recorded verbatim above, in both default and full-width form. | PASS |

## Bearing on the P4-T8 cross-check

P4-T8 requires this baseline diff to be examined for any path outside the two in-scope test files and
outside the feature folder. Every one of the four paths listed above begins with
`docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/`. No such pre-existing
branch condition exists, so the P4-T8 cross-check is not triggered by this baseline.
