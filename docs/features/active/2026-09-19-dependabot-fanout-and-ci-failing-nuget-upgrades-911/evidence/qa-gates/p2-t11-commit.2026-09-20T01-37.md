# Phase 2 Commit — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-55-30
- Task: [P2-T11]
- Findings: R5, R9b, R9c
- EXIT_CODE: 0

## Commit

Head SHA after the commit: **`4a858005862593199541dbafe3450195d4e680fd`**

| Comparison | Value | Differs |
|---|---|---|
| [P1-T15] head | `7cda4543995f52b8f2f41165de086c6b2eefb036` | **yes** |
| [P0-T2] anchor | `4043b913468f913649be3e6aa189b1be8310df00` | yes |

## Pathspec

Explicit, covering the five source paths and the feature folder:

```
git add -- scripts/dependencies/ProjectConsistency.psm1 \
           scripts/dependencies/ConsistencyVerifier.psm1 \
           scripts/dependencies/Repair-PackageManifestConsistency.ps1 \
           tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1 \
           tests/scripts/dependencies/DependabotConfig.Tests.ps1 \
           docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/
```

## Commit Message

A single `-m` argument containing no `<`, `>`, `$` or backtick character:

```
fix(deps): make the consistency repair entry point impossible to call incorrectly

Discharges remediation findings R5, R9b and R9c for issue 911.

R5: Resolve-ReferenceAssemblyVersion moves from the composition root into
ProjectConsistency.psm1 and Invoke-ProjectConsistencyRepair now resolves the assembly
version per package and passes it, so the entry point preserves a declared Reference
assembly version instead of rewriting it to the package version for every caller.

R9b: the unfiltered Get-AnalyzerAssemblyPath call site is commented as a verification
membership set that must never be written to a project file.

R9c: the default manifest lister emits the enumerated directory and returned file counts,
making a one-level-deep discovery shortfall observable in the run log.

Claude-Session: https://claude.ai/code/session_01QaUVgY37zfbsTvSTPd7wsr
```

The `Co-Authored-By:` trailer is omitted because its address requires angle brackets, which this
plan and the repository's pre-implementation gate both forbid in the commit argument. The omission
is recorded rather than left silent.

## `git status --porcelain --untracked-files=all` After the Commit, Verbatim

```
(empty)
```

No entry at all, so no entry outside `coverage/`. The Phase 2 checkboxes were ticked **before**
the commit so the plan file was part of the committed set.

## `git show --name-only --format= HEAD`

**17 paths.**

| Check | Required | Measured | Result |
|---|---|---|---|
| Lists `scripts/dependencies/ProjectConsistency.psm1` | yes | **yes** | PASS |
| Lists `scripts/dependencies/ConsistencyVerifier.psm1` | yes | **yes** | PASS |
| Lists `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | yes | **yes** | PASS |
| Lists `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | yes | **yes** | PASS |
| Lists `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | yes | **yes** | PASS |
| Paths under `.github/` | none | **0** | PASS |
| Head SHA differs from the [P1-T15] value | yes | **yes** | PASS |

The other 12 paths are the modified plan, the [P1-T15] commit artifact written after the previous
commit, and the 10 Phase 2 evidence artifacts.

Zero paths under `.github/` is the check that Phase 2 kept off the workflow, which is Phase 3's
territory.

## Output Summary

Phase 2 committed at `4a858005`. All five source paths present, zero paths under `.github/`,
working tree clean after the commit.
