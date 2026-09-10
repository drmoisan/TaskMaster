# Phase 6 — Post-change numeric coverage figures

Timestamp: 2026-09-09T13-58
Task: [P6-T9]

`coverage/coverage.cobertura.xml` is gitignored (`.gitignore` line 144 ignores `coverage/*`), so the
raw file is never committed and its numbers must be copied into the feature evidence tree.

Command: the same command as `[P0-T11]`.

```text
pwsh -NoProfile -Command '[xml]$c = Get-Content -LiteralPath "coverage/coverage.cobertura.xml" -Raw; "line-rate={0} lines-valid={1} lines-covered={2}" -f $c.coverage.GetAttribute("line-rate"), $c.coverage.GetAttribute("lines-valid"), $c.coverage.GetAttribute("lines-covered")'
```

EXIT_CODE: 0

Verbatim output:

```text
line-rate=0.856132 lines-valid=65435 lines-covered=56021
```

## Post-change values

| Attribute | Value |
|---|---|
| `line-rate` | **0.856132** |
| `lines-valid` | **65435** |
| `lines-covered` | **56021** |

None of the three is a placeholder. Expressed as a percentage the post-change repository line
coverage is 85.6132%, above the 80% floor CLAUDE.md sets.

## Comparison with the baseline

| Attribute | Baseline `[P0-T11]` | Post-change | Delta |
|---|---|---|---|
| `line-rate` | 0.856094 | **0.856132** | +0.000038 |
| `lines-valid` | 65376 | **65435** | +59 |
| `lines-covered` | 55968 | **56021** | +53 |

The denominator moved: `lines-valid` rose by 59, which is the production code this change added — the
three-statement idiom at each cleanup site, the two `RequestCancel` members, the two rewritten
`SetCancellationTokenSource` methods, the two rewritten `CancelButton_Click` handlers and the two
logger field initializers. Because the denominator differs between the two runs, the two `line-rate`
values are not measuring the same quantity and a bare comparison of them is not sound. `[P6-T11]`
states that explicitly.

Output Summary: three explicit numeric post-change coverage values recorded, none a placeholder. The
same three values were appended as a `Coverage Headline:` line to
`docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/mstest-coverage.2026-09-09T00-05.md`,
so the final-QC test-step artifact itself carries numeric coverage as the plan contract requires.
