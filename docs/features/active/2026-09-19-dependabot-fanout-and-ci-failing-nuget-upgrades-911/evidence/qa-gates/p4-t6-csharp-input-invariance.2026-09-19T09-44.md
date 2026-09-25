# P4-T6 — Batch B changed no C# compilation input

Timestamp: 2026-09-20T01-18

Commands:

```
git -C <W> diff --name-only 48f0c710a -- .
git -C <W> status --porcelain --untracked-files=all
```

`48f0c710a` is the Batch A head SHA P2-T8 recorded, which is also the current `HEAD`
(`48f0c710a9a970587ab8b17956be224513c1f7fd`) because Batch B has not yet been committed.

EXIT_CODE: 0

## Capture 1 — anchored diff, 4 tracked paths

```
.github/dependabot.yml
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/plan.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md
scripts/vscode/Sync-PackageReferences.ps1
```

## Capture 2 — porcelain, 25 paths

```
 M .github/dependabot.yml
 M docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/plan.2026-09-19T09-44.md
 M docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md
 M scripts/vscode/Sync-PackageReferences.ps1
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/other/p2-t9-batch-a-boundary.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p2-t8-commit.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p3-t1-packagecompatibility-module.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p3-t10-ac4-nuget-pin.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p3-t2-packagecompatibility-tests-authored.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p3-t3-ac9-asset-level-gate.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p3-t4-sync-package-references.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p3-t5-sync-tests-authored.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p3-t6-ac7-framework-exclusion.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p3-t7-dependabot-consolidation.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p3-t8-dependabotconfig-tests-authored.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p3-t9-ac1-dependabot-consolidated.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p4-t1-poshqc-format.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p4-t2-poshqc-analyze.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p4-t3-pester.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p4-t4-csharpier-check.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p4-t5-actionlint.2026-09-19T09-44.md
?? scripts/dependencies/PackageCompatibility.psm1
?? tests/scripts/dependencies/DependabotConfig.Tests.ps1
?? tests/scripts/dependencies/PackageCompatibility.Tests.ps1
?? tests/scripts/vscode/Sync-PackageReferences.Tests.ps1
```

`coverage/` does not appear because `.gitignore:144` ignores it.

## Union and the type condition

```
DIFF_COUNT=4
PORCELAIN_COUNT=25
UNION_COUNT=25
CSHARP_INPUT_PATHS=0
```

The union is 25 distinct paths: the four `.github/dependabot.yml`, the plan, the spec and
`scripts/vscode/Sync-PackageReferences.ps1` tracked modifications, plus the four untracked
PowerShell files Batch B creates and the 17 untracked evidence documents this run has written.

**Zero** of the 25 matches `*.cs`, `*.csproj`, `*.sln`, `packages.config` or `app.config`.

## Why both captures are needed

Per gate rule 8 the two are complementary and each alone is wrong in one state. The anchored diff
enumerates tracked changes only and can never report the four PowerShell files Batch B creates,
which are untracked at this point; porcelain reports them but goes empty once the change is
committed. The union is the complete footprint at this instant, and it is what the type condition
is evaluated over.

The at-least-4 clause is the non-vacuity guard: an empty union would satisfy the zero trivially.
The measured union is 25, six times that floor, and both underlying captures are individually
non-empty at 4 and 25.

## What this establishes

Batch B changed no C# compilation input, so the green results from the Batch A solution-wide
gates still hold for the tree as it stands: P2-T5's analyzer `/t:Rebuild` at exit 0 with 0
`CS0006` lines and at least 18 `/out:obj\Debug\` compile lines, and P2-T6's nullable `/t:Rebuild`
at exit 0 with the same compile-line evidence. No solution-wide rebuild is re-run at this
boundary, and CMD-OUTLOOK therefore does not bind this task: its scope is the two solution-wide
`/t:Rebuild` commands only, which are the seven tasks P0-T11, P0-T12, P1-T14, P2-T5, P2-T6, P9-T5
and P9-T6.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| Union of the two captures | at least 4 paths | 25 | PASS |
| Paths matching `*.cs`, `*.csproj`, `*.sln`, `packages.config`, `app.config` | exactly 0 | 0 | PASS |
| Both captures recorded | verbatim | both recorded in full above | PASS |

Output Summary: anchored at the Batch A head `48f0c710a`, `git diff --name-only` lists **4**
tracked paths and `git status --porcelain --untracked-files=all` lists **25**, for a union of
**25** distinct paths against a non-vacuity floor of 4. **Zero** of them matches `*.cs`,
`*.csproj`, `*.sln`, `packages.config` or `app.config`, so Batch B changed no C# compilation
input and the green analyzer and nullable rebuilds from P2-T5 and P2-T6 still describe this tree.
The union is the two `.github` and docs modifications, the rewritten
`scripts/vscode/Sync-PackageReferences.ps1`, the four new PowerShell files, and 17 evidence
documents.
