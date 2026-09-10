# Commit of the change and its evidence (issue #826, [P8-T19])

Timestamp: 2026-09-09T19-50

Command, run as one `pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard:

```
git add -- . ":(exclude).claude"
git status --porcelain --untracked-files=all -- . ":(exclude).claude"
git commit -m "fix(826): remove unrestored console-writer installs, route timeout diagnostics through log4net, extend BannedSymbols"
git status --porcelain --untracked-files=all -- . ":(exclude).claude"
```

`git add -A` was deliberately not used: it would sweep a queued sibling promotion's untracked file onto
this branch. The `:(exclude).claude` pathspec is applied per plan decision D17, because
`.claude/agent-memory` is tracked and may be written mid-run by this executor or by sibling agents.

EXIT_CODE: 0 for `git add`, 0 for `git commit`.

## Commit

Subject:

```
fix(826): remove unrestored console-writer installs, route timeout diagnostics through log4net, extend BannedSymbols
```

Result reported by git: **85 files changed, 3678 insertions(+), 192 deletions(-)**.

The 85 files are the 38 write-set paths, this feature's `spec.md` and `plan.2026-09-08T23-52.md`, and 45
evidence artifacts under `<FEATURE>/evidence/`.

## Pre-commit porcelain span

81 entries: 38 tracked write-set paths, `spec.md`, this plan file, and 42 untracked evidence artifacts
(the count rose to 45 in the commit because [P8-T18]'s summary, the incident record and this task's own
staging happened between the two observations). Every entry was inside the [P8-T1] allow-list; the span
is reproduced in full in `<FEATURE>/evidence/qa-gates/p8-t1-ac16-write-set.md`, which measured the same
scoped pathspec.

## Post-commit porcelain span

```
(empty)
```

Empty for the scoped pathspec, as observed in the shell at the moment the block ran.

## Raw artifacts correctly excluded from the commit

No raw msbuild log, SARIF document, TRX file, `.coverage` file or raw Cobertura document is in the
commit. They live under `coverage/826-raw/`, which `.gitignore` excludes via its `coverage/*` entry. That
matters because those documents embed absolute host paths, the account name in `runUser=` and the machine
name in `computerName=`, and one `.coverage` attachment filename embeds both.

The committed evidence carries only sanitized extracts: counts, rule IDs, repository-relative paths and
line numbers. Before committing, the whole evidence tree was scanned for the tokens `C:\Users`,
`C:/Users`, the account name, the machine name and both worktree directory names; every count was 0. One
artifact, the executor incident record, initially carried a worktree directory name in a measured output
block and was sanitized to `<exec-worktree>` and `<session-worktree>` before the commit. No absolute host
path, account name or machine name appears in the commit message or in any committed artifact.

The `warning: LF will be replaced by CRLF` lines git printed for the 45 new evidence files are the
repository's `text=auto` normalization applying to newly added Markdown; they are not a content change
and no tracked file's existing line endings were altered.

## This is not the terminal clean-tree claim

Writing this artifact and checking [P8-T19] off in the plan both happen after the post-commit observation
above, so they reappear in the [P8-T20] span and are committed by [P8-T21], which carries the terminal
clean-tree gate.

Output Summary: all feature changes and all evidence artifacts are committed in a single commit, 85 files
changed. The post-commit porcelain span for the scoped pathspec is empty, and the gitignored raw
measurement documents are absent from the commit.
