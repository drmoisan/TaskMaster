# Block L insertion into spec.md (P7-T7)

Timestamp: 2026-09-03T00-18

EXIT_CODE: 0

AC16 requires the four verified reasons no test-only fix exists for Finding 4 to be recorded in
the spec itself. Before this insertion the Finding 4 out-of-scope bullet cited "four independent
reasons, §4.2" without enumerating them, so this insertion is what makes AC16 true.

## Line-count delta

```
(Get-Content 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md').Count
```

- Before the insertion: `332`
- After the insertion: `337`
- Delta: `+5`

Five is the number of lines in Block L: its heading line plus its four numbered lines. No other
line of `spec.md` was changed by this task.

## Placement

The five lines were inserted immediately after the final sentence of the Finding 4 out-of-scope
bullet (the single line beginning `- **Finding 4 — pump-hosted` in the
`### Out of scope / non-goals` section) and before the next top-level bullet,
`- **All QuickFiler/ production sources.**`. Each of the five lines is indented by exactly two
spaces so it renders as a continuation of that bullet. The heading line now sits at line 88 and
the four numbered lines at lines 89 through 92.

## The six fixed-string searches

| Search | Matches | Expected |
|---|---|---|
| `Finding 4 — reasons no test-only fix exists:` | 1 | exactly 1 |
| `The production code reads the context off the control, not from an injected seam.` | 1 | exactly 1 |
| `The fixture's cost is the real WinForms control tree, not the pump.` | 1 | exactly 1 |
| `` `[DoNotParallelize]` would be a no-op. `` | 1 | exactly 1 |
| `` Removing `[Timeout]` trades a bounded failure for an unbounded hang. `` | 1 | exactly 1 |
| `- **Finding 4 — pump-hosted` | 1 | still exactly 1 |
| `four independent reasons, §4.2` | 1 | still exactly 1 |

The last two confirm that the pre-existing bullet line was not rewritten: its opening text and its
`§4.2` citation both remain, each exactly once.

Note on measurement method: three of these searches contain a non-ASCII character (the em dash, or
the section sign). Those three were evaluated with a UTF-8-correct fixed-string search rather than
through a PowerShell standard-input harness, because that harness decodes non-ASCII input
incorrectly and reported zero matches for all three while reporting the correct count for every
ASCII-only search in the same run. The four ASCII-only reason sentences were confirmed by the
PowerShell harness, and the three non-ASCII searches were confirmed by the UTF-8-correct search;
all seven return exactly one match.

## No ref-anchored diff

A ref-anchored diff is deliberately not used here. `spec.md` is tracked, and the `$base`-anchored
name-status diff that P7-T2 and P7-T5 record reports it as an `A` entry, so that diff presents the
whole file as added and cannot isolate this task's five inserted lines. The line-count delta and
the fixed-string searches above are what make this insertion verifiable.

Output Summary: Block L was inserted under the Finding 4 out-of-scope bullet in `spec.md`. The
file grew from 332 to 337 lines, a delta of exactly 5, which is the size of Block L. All five new
lines are indented by two spaces. Every one of the required fixed-string searches returns exactly
one match, including the two witnesses confirming the pre-existing bullet line was not rewritten.
