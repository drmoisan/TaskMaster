# P0-T2 — Worktree identity and anchor commit

Timestamp: 2026-09-08T09-17
Task: [P0-T2]
Command: pwsh -NoProfile -File coverage/plan811-helper.ps1 (C1 preamble, then the six git observations below)
EXIT_CODE: 0

BASE-SHA: bb1c7d4b60f7b782227956f36859314d5c47bb03

## Observations

| Observation | Value |
|---|---|
| `git rev-parse HEAD` | `bb1c7d4b60f7b782227956f36859314d5c47bb03` |
| `git rev-parse --abbrev-ref HEAD` | `bug/utilitiescs-test-determinism-780-803-594-811` |
| `git cat-file -t bb1c7d4b60f7b782227956f36859314d5c47bb03` | `commit` |
| `git merge-base HEAD bb1c7d4b60f7b782227956f36859314d5c47bb03` | `bb1c7d4b60f7b782227956f36859314d5c47bb03` |
| `git rev-parse origin/main` | `0e9c95a5dd45104d82f46fd801973a6bc068f25f` |
| `git merge-base --is-ancestor origin/main HEAD` exit code | `0` |

## Scoped porcelain

`git status --porcelain --untracked-files=all -- . ":(exclude).claude"`

```
 M docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/plan.2026-09-07T22-03.md
?? docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/evidence/baseline/phase0-instructions-read.md
```

Both entries are under the feature folder. No path outside `<FEATURE>/` is listed. The modified
plan file carries the orchestrator's re-anchor edit (decision D16) plus this executor's P0-T1
check-off. The untracked file is the P0-T1 evidence artifact. The C1 helper script
`coverage/plan811-helper.ps1` does not appear because `coverage/*` is gitignored (`.gitignore:144`).

## Acceptance evaluation

- Branch name equals `bug/utilitiescs-test-determinism-780-803-594-811`. PASS
- `cat-file -t` prints `commit`. PASS
- Merge-base output equals the anchor, so the anchor is an ancestor of HEAD; here it is HEAD
  itself, as expected until P7-T12 commits. PASS
- Scoped porcelain lists only paths under `<FEATURE>/`. PASS
- `BASE-SHA:` present on its own line; no host path anywhere in this artifact. PASS
- `git merge-base --is-ancestor origin/main HEAD` exited 0, so the merged `origin/main`
  (`0e9c95a5dd45104d82f46fd801973a6bc068f25f`) is still contained in this branch. Main has not
  advanced since the re-anchor. No reconciliation is required before source edits. PASS

## Output Summary

Worktree identity confirmed. HEAD equals the D16 anchor `bb1c7d4b`. `origin/main` at `0e9c95a5`
is an ancestor of HEAD (exit 0), so the re-anchored base is still valid. Working tree carries only
two feature-folder paths and nothing outside them.
