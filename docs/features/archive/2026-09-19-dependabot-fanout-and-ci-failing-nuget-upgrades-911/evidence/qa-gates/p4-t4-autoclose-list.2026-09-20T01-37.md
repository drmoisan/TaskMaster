# R9a — The Two Autoclose False Positives Are Stripped

- Timestamp: 2026-09-20T09-06-28
- Task: [P4-T4]
- Finding: R9a, Minor
- EXIT_CODE: 0

## Files Examined

| File | Exists | `#MEZIANTOU-898` before | `#SHA-256` before |
|---|---|---|---|
| `artifacts/pr_context.summary.txt` | yes | **2** | **2** |
| `artifacts/pr_context.appendix.txt` | yes | **0** | **0** |

Only the summary carried them, in **both** of its close-candidate sections. The appendix carried
neither, so it was not edited; the plan's conditional clause for it did not fire.

## Section 1 — `===== Close candidates =====`, `Auto-close issues (author asserted)`

**Before**, verbatim:

```
Auto-close issues (author asserted):
- #181
- #563
- #668
- #895
- #898
- #902
- #903
- #907
- #908
- #909
- #911
- #MEZIANTOU-898
- #SHA-256
```

**After**, verbatim:

```
Auto-close issues (author asserted):
- #181
- #563
- #668
- #895
- #898
- #902
- #903
- #907
- #908
- #909
- #911
```

## Section 2 — The Second Close-Candidate Listing

**Before**, verbatim, tail:

```
- #898
- #902
- #903
- #907
- #908
- #909
- #911
- #MEZIANTOU-898
- #SHA-256
NOTE: Unverified (GitHub unavailable)
```

**After**, verbatim, tail:

```
- #898
- #902
- #903
- #907
- #908
- #909
- #911
NOTE: Unverified (GitHub unavailable)
```

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| Section 1 after-list | exactly the 11 members, in order, no twelfth | **11**, in order | PASS |
| Section 2 after-list | exactly the 11 members, in order, no twelfth | **11**, in order | PASS |
| `#MEZIANTOU-898` occurrences in either file | 0 | **0** | PASS |
| `#SHA-256` occurrences in either file | 0 | **0** | PASS |

The 11 genuine members, in file order in both sections: `#181`, `#563`, `#668`, `#895`, `#898`,
`#902`, `#903`, `#907`, `#908`, `#909`, `#911`.

The file's line terminator was detected as **LF** and each token was removed together with its
own terminator, so no blank line was left behind and no terminator style was changed.

## The Edit Is to an Untracked Working File

| Measurement | Value |
|---|---|
| `.gitignore` line 57 | `artifacts/` |
| `git ls-files -- artifacts/pr_context.summary.txt` | **0 paths**, untracked |
| `git status --porcelain --untracked-files=all -- artifacts` | **empty** |

`artifacts/` is gitignored, so the file is untracked and **no commit lists it**. Porcelain shows
nothing for the directory, which is why [P4-T6] and every later commit gate tolerates
`artifacts/` without expecting an entry there.

## Standing Instruction for Whoever Authors the Pull-Request Body

**If `pr_context` is regenerated before the body is authored, the same two tokens must be
stripped again.**

The detector re-derives them from the words `Meziantou.Analyzer` and `SHA-256` in prose, both of
which appear throughout this feature's documents and neither of which is going away. A
regenerated `pr_context.summary.txt` will reintroduce both, in both sections. The current file
was generated at `2026-09-20 05:30:49 UTC` against head `794d34f02`, which is four commits behind
the current head, so a regeneration before the pull request is likely.

A pull-request body carrying `#SHA-256` would reference an unrelated issue number and could close
it.

This instruction is repeated in the [P6-T3] merge-time dossier.

## Output Summary

Both false positives removed from both close-candidate sections of
`artifacts/pr_context.summary.txt`, leaving exactly the 11 genuine members in order. The appendix
carried neither. The file is untracked under a gitignored directory, so no commit lists it, and
the standing re-strip instruction is recorded here and carried to [P6-T3].
