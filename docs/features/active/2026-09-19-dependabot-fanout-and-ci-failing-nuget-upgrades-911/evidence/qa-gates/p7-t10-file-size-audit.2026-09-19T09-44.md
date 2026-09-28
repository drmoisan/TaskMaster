# P7-T10 — Batch D file-size audit

Timestamp: 2026-09-20T02-20

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; foreach ($f in @("scripts/dependencies/Repair-PackageManifestConsistency.ps1","tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1","tests/scripts/dependencies/DependabotConfig.Tests.ps1")) { "$f => " + (Get-Content -LiteralPath $f).Count }'
```

EXIT_CODE: 0

## Output Summary

```
scripts/dependencies/Repair-PackageManifestConsistency.ps1 => 498
tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1 => 374
tests/scripts/dependencies/DependabotConfig.Tests.ps1 => 335
```

## Acceptance conditions

| File | Lines | At most 500 |
|---|---|---|
| `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | 498 | PASS |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | 374 | PASS |
| `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | 335 | PASS |

Exactly 3 files are listed, each with an integer count, and every count is at most 500.

`.github/workflows/dependabot-repair.yml` at 123 lines and `.github/workflows/README.md` at 281
lines are also Batch D members. The workflow is audited at P9-T10 with the other workflow files;
Markdown documentation is exempt from the 500-line cap under `.claude/rules/general-code-change.md`.

The composition root has **2 lines of margin**, which is narrower than the 7 lines
`ConsistencyVerifier.psm1` carries and is the narrowest in the change. It reached 500 exactly while
the two defects P7-T2 surfaced were being fixed, and was brought to 498 by compressing
comment-based help rather than by removing behaviour. Any further addition to this file should
plan an extraction first; the same note applies to it as to `ConsistencyVerifier.psm1`.
