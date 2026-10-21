# P0-T23 — CI Pester-Scope Census

Timestamp: 2026-09-19T23-15

Commands:

```
git grep -n -E "Run\.Path|CodeCoverage\.Path" -- ".github/workflows/_pester.yml"
```

together with a direct read of `.github/workflows/ci.yml`.

EXIT_CODE: 0

## 1. `.github/workflows/_pester.yml` scope assignments, verbatim with line numbers

```
.github/workflows/_pester.yml:41:          $configuration.Run.Path = 'tests/scripts/vscode'
.github/workflows/_pester.yml:45:          $configuration.CodeCoverage.Path = 'scripts/vscode'
```

| Assignment | Value | Line |
|---|---|---|
| `Run.Path` | `'tests/scripts/vscode'` | **41** |
| `CodeCoverage.Path` | `'scripts/vscode'` | **45** |

Both are single-valued string assignments, not arrays.

The 80 percent line gate the plan directs P1-T13 to leave unchanged is at line 71:

```
          if ($linePercent -lt 80) { exit 1 }
```

## 2. `.github/workflows/ci.yml` job list, verbatim

```
jobs:
  actionlint:
    name: actionlint
    uses: ./.github/workflows/_actionlint.yml
  format-check:
    name: format-check
    uses: ./.github/workflows/_format-check.yml
  build-analyzers:
    name: build-analyzers
    uses: ./.github/workflows/_build-analyzers.yml
  build-nullable:
    name: build-nullable
    uses: ./.github/workflows/_build-nullable.yml
  mstest-coverage:
    name: mstest-coverage
    uses: ./.github/workflows/_mstest-coverage.yml
  pester:
    name: pester
    uses: ./.github/workflows/_pester.yml
```

**Exactly 6 jobs**, including `pester`:

| # | Job | Line | Reusable workflow |
|---|---|---|---|
| 1 | `actionlint` | 18 | `_actionlint.yml` |
| 2 | `format-check` | 21 | `_format-check.yml` |
| 3 | `build-analyzers` | 24 | `_build-analyzers.yml` |
| 4 | `build-nullable` | 27 | `_build-nullable.yml` |
| 5 | `mstest-coverage` | 30 | `_mstest-coverage.yml` |
| 6 | `pester` | 33 | `_pester.yml` |

`ci.yml` triggers on `push`, `pull_request` and `workflow_dispatch`, so the `pester` job runs on
every pull request.

## Evidence for the Scope Decision 1 amendment

**This artifact is the evidence P1-T1 cites when it adds `.github/workflows/_pester.yml` to the spec
`## Write Set`.**

The reasoning the measurement supports: the `pester` job runs on every pull request, and it is
hard-scoped to `tests/scripts/vscode` for discovery and `scripts/vscode` for coverage. Every test
file this change creates lives under `tests/scripts/dependencies/` and every production module under
`scripts/dependencies/`. Without the P1-T13 edit widening both assignments to two-member arrays, the
new suite would never execute in CI and the `pester` check would report green while measuring
nothing of what this change adds — and the line gate at line 71 would keep passing on the old
population. `spec.md` does not list `_pester.yml`, so the amendment is required for the change
footprint to be complete.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| `Run.Path` value and line | `'tests/scripts/vscode'` at line 41 | `'tests/scripts/vscode'` at line 41 | PASS |
| `CodeCoverage.Path` value and line | `'scripts/vscode'` at line 45 | `'scripts/vscode'` at line 45 | PASS |
| `ci.yml` job count | exactly 6, including `pester` | 6, `pester` present at line 33 | PASS |

Output Summary: `.github/workflows/_pester.yml` assigns `Run.Path = 'tests/scripts/vscode'` at line
**41** and `CodeCoverage.Path = 'scripts/vscode'` at line **45**, both single-valued, with the 80
percent line gate at line 71. `.github/workflows/ci.yml` declares exactly **6** jobs — `actionlint`,
`format-check`, `build-analyzers`, `build-nullable`, `mstest-coverage` and `pester` — and runs on
every pull request. This is the evidence for the Scope Decision 1 amendment P1-T1 makes to the spec
`## Write Set`: without widening both assignments at P1-T13, the suite this change creates under
`tests/scripts/dependencies/` never executes in CI.
