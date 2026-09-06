# [P0-T11] Anchor record — `pre-782-base` and the remediation base

Timestamp: 2026-09-06T01-35

Command:

```powershell
git rev-parse pre-782-base
git rev-parse HEAD
```

EXIT_CODE: 0

Output Summary: `pre-782-base` resolves to a SHA beginning `736c2cf2`, which is the value the
remediation plan's scope boundary states it must keep, and `HEAD` resolves to the commit this
remediation starts from.

```text
pre-782-base  736c2cf234cdd71b604c908f348b6aa89b256b53
HEAD          e01cf434197d34e0fff1ba408616dc175dfa5fd6
```

REMEDIATION-BASE-SHA: e01cf434197d34e0fff1ba408616dc175dfa5fd6

## Consumers

[P5-T5] reads the base SHA from the `REMEDIATION-BASE-SHA:` line above rather than from any value
tabled in the plan, and uses it as the left side of the post-commit C# diff. It also re-runs
`git rev-parse pre-782-base` and requires the value to be unchanged from the one recorded here.

No task in this plan creates, moves, deletes, or re-points `pre-782-base`.
