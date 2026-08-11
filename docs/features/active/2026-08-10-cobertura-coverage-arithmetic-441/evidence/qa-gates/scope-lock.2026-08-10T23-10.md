# Scope-Lock Diff Gate (P4-T8)

Timestamp: 2026-08-10T23-10

Enforces `spec.md` AC-18: the diff touches exactly two source files.

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
git diff --name-only edf3d34c -- scripts tests
```

EXIT_CODE: 0

Output Summary:

```
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
```

## Verdict

| Expected file | Present | Notes |
| --- | --- | --- |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | yes | the one production file changed |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | yes | the one test file changed |
| anything else | **none** | — |

The gate lists **exactly** the two in-scope source files and nothing else. **PASS.**

Notably absent, as required:

- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — unchanged, including its missing `\.claude\`
  discovery exclusion (separately verified by P2-T6 and P6-T6, and filed as follow-up candidate 3).
- `scripts/temp-extract-coverage.ps1` — unchanged.
- No third test file was created. The test-file line budget was met by compacting the fixture
  here-strings per the plan's § Test-File Line Budget, because adding a third file would have broken
  AC-18.

The gate is scoped to `scripts` and `tests` deliberately: `docs/` (feature documents and evidence)
and `.claude/agent-memory/` are tracked and legitimately change during this work, and are covered by
the separate threshold gate (P4-T9) and the evidence-location audit (P5-T5, P7-T20).
