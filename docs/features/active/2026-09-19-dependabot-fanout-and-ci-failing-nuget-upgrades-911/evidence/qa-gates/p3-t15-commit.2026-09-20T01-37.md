# Phase 3 Commit — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-04-40
- Task: [P3-T15]
- Findings: R3, R6, R7, R8
- EXIT_CODE: 0

## Commit

Head SHA after the commit: **`07b4872eae664e9e5242c79e2ed546a1ee9fe797`**

| Comparison | Value | Differs |
|---|---|---|
| [P2-T11] head | `4a858005862593199541dbafe3450195d4e680fd` | **yes** |
| [P1-T15] head | `7cda4543995f52b8f2f41165de086c6b2eefb036` | yes |
| [P0-T2] anchor | `4043b913468f913649be3e6aa189b1be8310df00` | yes |

This SHA is the `<P3-T15-head-sha>` that [P4-T3] and [P4-T5] anchor their diffs to.

## Pathspec

```
git add -- .github/workflows/dependabot-repair.yml \
           tests/scripts/dependencies/DependabotConfig.Tests.ps1 \
           tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1 \
           docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/
```

## Commit Message

A single `-m` argument containing no `<`, `>`, `$` or backtick character. Its subject and four
finding paragraphs are reproduced in the commit itself; the `Co-Authored-By:` trailer is omitted
because its address requires angle brackets, and the omission is recorded rather than left
silent.

## `git status --porcelain --untracked-files=all` After the Commit, Verbatim

```
(empty)
```

No entry at all. The Phase 3 checkboxes were ticked before the commit, so the plan file was part
of the committed set.

## `git show --name-only --format= HEAD`

**19 paths.**

| Check | Required | Measured | Result |
|---|---|---|---|
| Lists `.github/workflows/dependabot-repair.yml` | yes | **yes** | PASS |
| Lists `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | yes | **yes** | PASS |
| Lists `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | yes | **yes** | PASS |
| Paths under `scripts/` | none | **0** | PASS |
| Head SHA differs from the [P2-T11] value | yes | **yes** | PASS |

Zero paths under `scripts/` is the check that Phase 3 kept off the production PowerShell it does
not own; its only production change is the workflow.

The other 16 paths are the modified plan, the [P2-T11] commit artifact written after the previous
commit, and the 14 Phase 3 evidence artifacts.

## One Acceptance Clause of This Phase Was Not Met, and It Is Carried Forward

[P3-T10]'s anchored numstat clause required **at least 6 deletions** on
`.github/workflows/dependabot-repair.yml`. The measured figure is **5**, against 55 additions
where at least 12 were required.

The shortfall is recorded in full in
`evidence/qa-gates/p3-t10-workflow-footprint.2026-09-20T01-37.md`, with the per-edit deletion
accounting and the reason: the delivered disclosure rewrite replaces one line rather than two,
because the `$updated = Join-Path ...` line was preserved verbatim rather than rewritten. Raising
the count to 6 would mean deleting a line that needs no deletion.

The clause's stated purpose — "fails if one of the four edits was not in fact applied" — is
satisfied by four independent exact-count measurements ([P3-T2], [P3-T4], [P3-T6], [P3-T7]) and
by [P3-T8]'s four red-to-green pairs. No plan text, acceptance clause or fixture was adjusted.
The discrepancy is reported to the coordinator at cycle completion.

## Output Summary

Phase 3 committed at `07b4872e`. All three source paths present, zero paths under `scripts/`,
working tree clean after the commit. One numstat expectation in [P3-T10] reads 5 against a floor
of 6 and is carried forward as a reported discrepancy rather than resolved by editing the change.
