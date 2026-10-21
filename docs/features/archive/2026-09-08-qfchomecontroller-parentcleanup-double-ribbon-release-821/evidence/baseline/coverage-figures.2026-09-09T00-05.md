# Phase 0 — Baseline numeric coverage figures

Timestamp: 2026-09-09T12-45
Task: [P0-T11]

`coverage/coverage.cobertura.xml` is gitignored (`.gitignore` line 144 ignores `coverage/*`), so the
raw file is never committed and its numbers must be copied into the feature evidence tree.

Command:

```text
pwsh -NoProfile -Command '[xml]$c = Get-Content -LiteralPath "coverage/coverage.cobertura.xml" -Raw; "line-rate={0} lines-valid={1} lines-covered={2}" -f $c.coverage.GetAttribute("line-rate"), $c.coverage.GetAttribute("lines-valid"), $c.coverage.GetAttribute("lines-covered")'
```

EXIT_CODE: 0

Verbatim output:

```text
line-rate=0.856094 lines-valid=65376 lines-covered=55968
```

## Baseline values

| Attribute | Value |
|---|---|
| `line-rate` | **0.856094** |
| `lines-valid` | **65376** |
| `lines-covered` | **55968** |

None of the three is a placeholder. Expressed as a percentage the baseline repository line coverage
is 85.6094%, above the 80% floor CLAUDE.md sets and above the value at which
`Assert-CoberturaLineCoverageThreshold` would have thrown.

Output Summary: three explicit numeric baseline coverage values recorded. The same three values were
appended as a `Coverage Headline:` line to
`docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/mstest-coverage.2026-09-09T00-05.md`,
so the baseline test-step artifact itself carries numeric coverage as the plan contract requires.
