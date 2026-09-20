# R4 Residual and Its Positive Counterpart — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-05-51
- Task: [P4-T3]
- Finding: R4, **Blocking**
- EXIT_CODE: 0

## Scope

The **same scope [P0-T3] defined**: the frozen 206-path union of the branch diff and the
porcelain capture, re-read from the census record rather than re-derived. Re-deriving would
widen the scope to include the artifacts this cycle has written since, and the arithmetic below
is defined against the frozen set.

```
SCOPE = 206 paths
```

## The Residual

The census was recomputed with the **same run-time-derived pattern**, built from `$HOME` and
never typed:

```powershell
$acct    = Split-Path $HOME -Leaf
$pattern = [regex]::Escape($acct) + '|' + [regex]::Escape($acct.Substring(0, 6) + '~')
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| Host-path **occurrence** total | exactly **0** | **0** | PASS |
| Matching **file** count | exactly **0** | **0** | PASS |
| Matching **line** count | — | **0** | — |

## The Positive Counterpart

A zero residual alone is **not acceptance**. Per **gate rule 2** it is also what an empty scope
reports, what a broken pattern reports, and what a pass that deleted the text rather than
replacing it reports. The placeholder equality is what distinguishes those cases.

Placeholder occurrences of the four tokens `<execution-worktree-root>`,
`<session-worktree-root>`, `<repo-root>` and `<user-home>`, counted over the same 206 paths:

| Term | Value |
|---|---|
| `PLACEHOLDERS-BEFORE`, recorded by [P0-T3] | **121** |
| Host-path occurrences, recorded by [P0-T3] | **103** |
| Expected placeholders after | 121 + 103 = **224** |
| **Measured placeholders after** | **224** |
| Difference | **0** |

Exact. Every one of the 103 removed occurrences became exactly one placeholder token, and none
of the 121 pre-existing tokens was disturbed. A pass that deleted text instead of replacing it
would have landed at 121; a pass that replaced only some spellings would have landed between the
two.

## Anchored Numstat for the Rewritten Files

```
git diff --numstat 07b4872eae664e9e5242c79e2ed546a1ee9fe797
```

anchored to the `<P3-T15-head-sha>`.

| Measurement | Required | Measured | Result |
|---|---|---|---|
| Paths in the diff | — | **33** | matches the [P4-T2] rewrite set exactly |
| Files whose additions do **not** equal their deletions | **0** | **0** | PASS |
| Total additions | — | **94** | — |
| Total deletions | — | **94** | — |
| Paths not ending `.md` | — | **0** | — |

Per-file equality of additions and deletions is checked **per file**, not only in the total. A
pure substitution rewrites a line in place, so git reports one addition and one deletion for it;
any line-count change a substitution cannot produce would show as an inequality. Zero files
differ.

The 94 changed lines are the 94 matching lines [P0-T3] counted, which is the second metric that
census recorded and the one that maps to a git line diff. The 103 occurrences sit on those 94
lines, some lines carrying more than one.

## Output Summary

Host-path residual is **0 occurrences across 0 files** over the frozen 206-path scope. The
positive counterpart holds exactly: placeholder occurrences rose from 121 to **224**, which is
121 plus the 103 replaced occurrences. The anchored numstat shows 33 markdown paths with
additions equal to deletions on every one.
