# [P6-T7] Phase 6 commit — issue #469 defects 1 and 2, plus a scoped sanitisation remediation

Timestamp: 2026-08-26T10-29

Command:

```
git add -- QuickFiler/Controllers/QfcCollectionController.cs \
           QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs \
           docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md \
           docs/features/active/qfc-collection-controller-defects-468/spec.md \
           docs/features/active/qfc-collection-controller-defects-468/research/test-harness-feasibility.md \
           docs/features/active/qfc-collection-controller-defects-468/evidence/
git commit -m "fix(469): correct the diagnostics array length and guard before dereference"
git show --name-only HEAD
```

EXIT_CODE: 0

## Output Summary

Commit `137ee3076ecae066c8a53306149b100dee29fb7e`
`fix(469): correct the diagnostics array length and guard before dereference`

37 files changed, 20,251 insertions, 5,704 deletions. The large line counts are TRX evidence, not
source: 5,668 of the deletions and an equal share of the insertions are the host-identifier
substitutions applied to sixteen previously committed TRX files by the scoped remediation described
below.

## Acceptance verification — no path outside the owned file set

`git show --name-only HEAD` filtered to `\.(cs|csproj|sln)$` returns exactly two paths:

| Path | Owned because |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | `<CTRL>`, the feature's single production file |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | D12 test file 3 of 5, already registered in the csproj by P4-T2 |

No `.csproj` and no `.sln` changed. Every other path in the commit is under
`docs/features/active/qfc-collection-controller-defects-468/`. No path outside the owned set
appears.

`.claude/agent-memory/**` and `.claude/state/**` remain unstaged; every `git add` used an explicit
pathspec. After the commit, `git status --porcelain` reports only those two `.claude` paths, which
this feature does not own.

Per D15 the commit also carries the plan checklist, `spec.md` (for the AC check-offs), and this
phase's evidence artifacts, including `p5-t7-commit.2026-08-26T10-43.md`, which could only be
written after the Phase 5 commit existed.

## Production change (P6-T4)

One edit pass over `GetMoveDiagnostics`:

1. **Issue #469 defect 1.** `new string[_itemGroupsToMove.Count + 1]` became
   `new string[_itemGroupsToMove.Count]`. The loop bound was already `Count`, so the surplus element
   was allocated and never assigned, and `QfcHomeController.Metrics` wrote it out as a blank row.
2. **Issue #469 defect 2.** The item-controller null test moved to the top of the loop body as an
   early-out that emits the `Unknown` diagnostics line and `continue`s. It now dominates both
   dereferences that previously preceded it — the `qf.ItemHelper` read and the
   `xComma(qf.ItemHelper.Subject)` interpolation.
3. The trailing `if (qf is not null) / else` collapsed to its non-null arm, because after the
   early-out `qf` is provably non-null and retaining the test would have reintroduced an
   unreachable branch in a new position.

## Toolchain state at commit

| Step | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | `EXIT_CODE 0`, 1,523 files checked |
| Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build` | `EXIT_CODE 0`, 0 errors, 5 pre-existing warnings |
| Test | full `QuickFiler.Test` suite, P6-T6 | `EXIT_CODE 0`, 949 passed, 0 failed |

Line-ending and BOM state was verified on both `.cs` files before staging:
`QfcCollectionController.cs` retains its UTF-8 BOM and is 100% CRLF (2,167 of 2,167 lines);
`QfcCollectionControllerDefects468MoveTests.cs` retains no BOM and is 100% CRLF. This check exists
because an earlier chunk in this series hit an editing round-trip that silently converted `.cs`
files to LF and injected a BOM.

## Acceptance criteria checked off in this commit

**AC-4 (#469 defect 1 as numbered in `spec.md`)** — marked `[x]`.

| Clause | Evidence |
|---|---|
| `GetMoveDiagnostics` returns without throwing when a group's `ItemController` is `null` | `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing`, `act.Should().NotThrow()` |
| The returned line for that group contains `To Unknown,Sender Unknown,Email,Folder Unknown` | the same test asserts that exact literal, which only the guard's else branch produces |
| Verified by a named MSTest test that throws `NullReferenceException` before the fix | `p6-t3-fail-before.2026-08-26T10-17.md`, `ExpectedExitCode: 1`, failed count 1, `NullReferenceException` at `QfcCollectionController.cs:2097` |

The assertion literal in that test was widened from `Folder Unknown` to the full AC-4 text after the
fix landed. The amendment is disclosed in the P6-T3 artifact together with the reason it cannot
affect the recorded red state: the pre-fix run throws inside the `Act` delegate and never reaches
the string assertion.

**AC-5 (#469 defect 2 as numbered in `spec.md`)** — marked `[x]`.

| Clause | Evidence |
|---|---|
| Returned array `Length` equals `_itemGroupsToMove.Count` | `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine` asserts against the cached count read back by reflection, not a hard-coded `1` |
| Contains no `null` element | `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls` asserts `Should().NotContainNulls()` |
| Verified by named MSTest tests for a one-group and a three-group arrangement | `p6-t1-fail-before.2026-08-26T10-17.md` (observed 2, expected 1) and `p6-t2-fail-before.2026-08-26T10-17.md` (observed 4, expected 3, surplus element `<null>`) |

Note that `spec.md` transposes the two defect numbers relative to the plan's phase heading: AC-4 is
labelled "defect 1" but states the guard-before-dereference criterion, and AC-5 is labelled
"defect 2" but states the array-length criterion. Both criteria are satisfied; the labels are left
as authored, since an executor does not rewrite acceptance-criterion text.

## Scoped sanitisation remediation carried in this commit

This commit also carries a scoped artifact-hygiene remediation that is not a plan task. Two
committed files were reported as leaking host identifiers; the sweep that verified the repair found
sixteen more. Full record, including per-pattern hit counts before and after, is at
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`. Summary:

| File(s) | Problem | Repair |
|---|---|---|
| `evidence/qa-gates/p1-t8-suite.2026-08-26T08-45.md` | documented its own sanitisation by quoting the raw identifiers | substitution and residual-scan tables now name each token by class; `BEFORE:` lines removed, `AFTER:` lines kept |
| 16 committed TRX files (`p2-t6` through `p5-t6`) | sanitised case-sensitively, so the all-lower-case `storage=` attribute survived — 946 occurrences in `p5-t6.trx` alone | rewritten in binary mode with case-insensitive substitutions; 5,668 in total |
| `evidence/baseline/p0-t14-tests-coverage.2026-08-26T08-25.md` | quoted the absolute-path prefix in order to report zero hits for it | pattern named by class; count unchanged |
| `research/test-harness-feasibility.md` | absolute worktree path including the account name | replaced with the `<repo-root>` placeholder form |

The research file is pre-existing debt rather than a regression from this branch: it arrived at
commit `0ac4b11b`, an ancestor of the epic integration base `61edc19b`.

Post-remediation sweep over all 71 files in the feature folder plus every branch-touched path: zero
hits for all four patterns.

Result: PASS.
