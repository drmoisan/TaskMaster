# Terminal Gate — Remediation R-1, Issue #752

- Timestamp: 2026-09-03T23-52
- Task: `[P2-T5]`, steps 3 and 4

This artifact records the **measured** values for AC-R3 and AC-R4. The step-3 `Index`-mode sweep
compares the merge base against the staged index — that is, against the tree the step-7 commit
creates, minus only this artifact, which step 6 proves token-free before it is staged. The step-8
`Diff`-mode sweep over `<MERGE_BASE>..HEAD` is a confirming re-run and is reported in the execution
summary rather than here, because step 7 prohibits writing a further file inside the feature folder.

Command:

1. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode Index -BaseSha 87233f867ad60c0a5c0d19b09cc121ae536d7ba1`
2. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/policy-audit.2026-09-03T12-23.md`
3. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/code-review.2026-09-03T12-23.md`
4. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/feature-audit.2026-09-03T12-23.md`
5. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-inputs.2026-09-03T12-23.md`
6. `git -C <repo-root> status --porcelain -uall`

EXIT_CODE:

1. `0`
2. `0`
3. `0`
4. `0`
5. `0`
6. `0`

Output Summary:

## Step 3 — `Index`-mode sweep (the measured AC-R3 and AC-R4 values)

Full stdout, verbatim:

```
TOKENCOUNT: account | COUNT: 0
TOKENCOUNT: parentdir | COUNT: 0
TOKENCOUNT: winprofile | COUNT: 0
TOKENCOUNT: winprofilefs | COUNT: 0
TOKENCOUNT: posixprofile | COUNT: 0
TOTAL: 0
```

- `TOTAL: 0`
- `TOKENCOUNT: account | COUNT: 0`
- `TOKENCOUNT: parentdir | COUNT: 0`
- `TOKENCOUNT: winprofile | COUNT: 0`
- `TOKENCOUNT: winprofilefs | COUNT: 0`
- `TOKENCOUNT: posixprofile | COUNT: 0`
- No `MATCHFILE:` line was printed.

AC-R3 is satisfied: the account-name token reports `COUNT: 0` and the worktree-parent directory-name
token reports `COUNT: 0`. AC-R4 is satisfied: `TOTAL: 0` with no `MATCHFILE:` line, so there is no
added-line match for any of the five tokens in any markdown file under `docs/features/`, and no
matching path of any other kind.

## Step 4 — untracked-artifact disclosure (four invocations, one path each)

This is a **disclosure, not a failure gate**. These four files are outside this plan's write set;
the `[P0-T4]` enumeration selected none of them, because all four are untracked in this worktree and
a Diff-mode enumeration cannot observe uncommitted content. Whatever counts they report, they do not
block this task.

The four `FILECOUNT:` lines, verbatim:

```
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/policy-audit.2026-09-03T12-23.md | COUNT: 2
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/code-review.2026-09-03T12-23.md | COUNT: 0
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/feature-audit.2026-09-03T12-23.md | COUNT: 0
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-inputs.2026-09-03T12-23.md | COUNT: 3
```

Token classes on the reported `FILEMATCH:` lines, stated by class only and never by quoting the
matched value:

| File | Line | Token classes present |
|---|---|---|
| `policy-audit.2026-09-03T12-23.md` | 182 | Windows user-profile path prefix |
| `policy-audit.2026-09-03T12-23.md` | 185 | account-name token, worktree-parent directory-name token, POSIX user-profile path segment |
| `remediation-inputs.2026-09-03T12-23.md` | 27 | Windows user-profile path prefix |
| `remediation-inputs.2026-09-03T12-23.md` | 35 | Windows user-profile path prefix, POSIX user-profile path segment |
| `remediation-inputs.2026-09-03T12-23.md` | 56 | Windows user-profile path prefix, POSIX user-profile path segment |

`code-review.2026-09-03T12-23.md` and `feature-audit.2026-09-03T12-23.md` report `COUNT: 0` and
printed no `FILEMATCH:` line. This distribution matches the disclosure expectation recorded in the
plan's "Why this plan is larger than the single mandated line edit" section.

**Required follow-up, recorded here so it cannot be lost:** each of the two files reporting a
non-zero count must be sanitised under the same hygiene rules before any later commit stages it.
They remain untracked at the end of this plan, so nothing this remediation commits carries their
content.

## Step 4 — porcelain snapshot

Verbatim output of `git -C <repo-root> status --porcelain -uall`:

```
M  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-plan.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/code-review.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/feature-audit.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/policy-audit.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-inputs.2026-09-03T12-23.md
```

This is a recorded snapshot only. The single `M ` entry is the plan file staged at step 2, which step
7 commits. The tracked-clean assertion is made at step 9, not here. The four `??` entries are this
loop's audit artifacts, which this plan does not stage.

`AGENT_MEMORY_WRITES:` `<none>`. No path under `.claude/agent-memory/` appears in this porcelain
output. Had any appeared, it would be the executing agent's standing, plan-independent
memory-persistence write: it would be listed here by path, not staged, and not treated as a
scope-lock violation.

This artifact records only repo-relative paths, token class names, line numbers, counts, and a commit
SHA. It reproduces no matched text, so it does not quote a removed value.
