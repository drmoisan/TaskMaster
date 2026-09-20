# Phase 5 Commit — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-15-50
- Task: [P5-T14]
- EXIT_CODE: 0

## Commit

**`H1` = `de9a00106c951a073c1ac33a4cf5223e24563cd8`**

| Comparison | Value | Differs |
|---|---|---|
| [P4-T6] head | `597bb2fcb14970e7222f6adad596405773753fc7` | **yes** |
| [P3-T15] head | `07b4872eae664e9e5242c79e2ed546a1ee9fe797` | yes |
| [P0-T2] anchor | `4043b913468f913649be3e6aa189b1be8310df00` | yes |

`H1` is the SHA [P6-T1] pushes and [P6-T2] expects the dispatched CI run to report as its
`headSha`.

## Pathspec

```
git add -- docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/
```

Phase 5 changed no source file, so no source path is in the pathspec.

## Commit Message

A single `-m` argument containing no `<`, `>`, `$` or backtick character, recording the
single-pass toolchain result, the coverage figures and the footprint. The `Co-Authored-By:`
trailer is omitted because its address requires angle brackets.

## `git status --porcelain --untracked-files=all` After the Commit, Verbatim

```
(empty)
```

No entry at all, so no entry outside `coverage/` and `artifacts/`. The Phase 5 checkboxes were
ticked before the commit, so the plan file was part of the committed set.

The gitignored working state that remains on disk and correctly does not appear: the four
`coverage/*.xml` and `coverage/*.log` collector documents, the hash-set and census `.xml`
records, the three throwaway helpers under `coverage/helpers/`, and the `artifacts/` directory
[P4-T4] edited.

## The Permitted Evidence Form Is Committed

| Clause | Required | Measured | Result |
|---|---|---|---|
| Committed set lists `evidence/qa-gates/p5-t7-coverage-projection.2026-09-20T01-37.jacoco.xml` | yes | **yes**, 1 occurrence | PASS |
| `H1` differs from the [P4-T6] value | yes | **yes** | PASS |
| Porcelain entries outside `coverage/` and `artifacts/` | none | **none** | PASS |

The projection is the permitted evidence form **gate rule 12** requires be committed in place of
the prohibited collector document. Its [P0-T12] counterpart was committed at [P1-T15].

## `git show --name-only --format= HEAD`

**17 paths**: the modified plan, the [P4-T6] commit artifact written after the previous commit,
14 Phase 5 evidence artifacts, and the copied `p5-t7` projection and test-result summary.

## Plan State at This Commit

| Measurement | Value |
|---|---|
| Tasks ticked | **74** |
| Tasks unticked | **6** |

The six unticked are [P6-T1] through [P6-T6], which this commit precedes. [P6-T4] ticks all six
and commits the plan in its terminal state.

## Output Summary

Phase 5 committed at `H1` = `de9a0010`. 17 paths, all under the feature folder. The mandatory
`p5-t7` coverage projection is in the committed set. Working tree clean after the commit, with
only gitignored `coverage/` and `artifacts/` state remaining on disk.
