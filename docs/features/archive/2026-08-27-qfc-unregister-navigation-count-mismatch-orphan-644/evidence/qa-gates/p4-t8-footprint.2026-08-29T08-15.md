# QA gate — Footprint containment ([P4-T8])

- Issue: #644
- Task: `[P4-T8]`
- Timestamp: 2026-08-29T08-15

## Diff anchor substitution (recorded local_execution_override: `diff_anchor_substitution`)

- Plan's literal anchor: `ecdb1c84ba8541ab67042985919cfed4df768c01`
- Substituted anchor actually run: `e968a1a8804b7641380d4489c496662824d45767`

Rationale, as authorized by the parent orchestrator: this run merged the current `origin/main`
tip into the feature branch before execution, and `e968a1a8804b7641380d4489c496662824d45767` is
that merge commit, i.e. the true pre-change state of this run. The plan's literal anchor predates
the merged fix for issue #638, so anchoring there would list every path that fix brought in, which
this task's acceptance clauses were not written to admit. The substitution narrows the listing to
this change; it is not a widening of any acceptance clause.

## Commands and results

Command: `git add QuickFiler QuickFiler.Test`
EXIT_CODE: 0

Command: `git diff --name-only e968a1a8804b7641380d4489c496662824d45767 -- QuickFiler QuickFiler.Test`
EXIT_CODE: 0

```
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/Controllers/QfcCollectionController.cs
```

Command: `git status --porcelain -- QuickFiler QuickFiler.Test`
EXIT_CODE: 0

```
(no output)
```

The porcelain listing is empty because the code change was committed by an earlier segment of this
resumed run, so the `git add` span above was a no-op. An empty listing satisfies the acceptance
clause "shows no path under `QuickFiler` or `QuickFiler.Test` outside those six".

Command: `git diff --stat e968a1a8804b7641380d4489c496662824d45767 -- QuickFiler/Controllers/QfcCollectionController.cs`
EXIT_CODE: 0

```
 QuickFiler/Controllers/QfcCollectionController.cs | 27 +++++++++++++++--------
 1 file changed, 18 insertions(+), 9 deletions(-)
```

Command: `git diff --name-only e968a1a8804b7641380d4489c496662824d45767 -- . ':!.claude/agent-memory'`
EXIT_CODE: 0

```
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/Controllers/QfcCollectionController.cs
<FEATURE>/evidence/baseline/p0-t10-nullable-build.2026-08-29T08-15.md
<FEATURE>/evidence/baseline/p0-t11-vstest-baseline.2026-08-29T08-15.md
<FEATURE>/evidence/baseline/p0-t12-coverage-baseline.2026-08-29T08-15.md
<FEATURE>/evidence/baseline/p0-t2-dotnet-sdk.2026-08-29T08-15.md
<FEATURE>/evidence/baseline/p0-t3-tool-restore.2026-08-29T08-15.md
<FEATURE>/evidence/baseline/p0-t4-nuget-restore.2026-08-29T08-15.md
<FEATURE>/evidence/baseline/p0-t5-analyzer-backfill.2026-08-29T08-15.md
<FEATURE>/evidence/baseline/p0-t6-dotnet-coverage.2026-08-29T08-15.md
<FEATURE>/evidence/baseline/p0-t7-counts.2026-08-29T08-15.md
<FEATURE>/evidence/baseline/p0-t8-csharpier-check.2026-08-29T08-15.md
<FEATURE>/evidence/baseline/p0-t9-analyzer-build.2026-08-29T08-15.md
<FEATURE>/evidence/baseline/phase0-instructions-read.2026-08-29T08-15.md
<FEATURE>/evidence/other/p2-t1-ledger-field.2026-08-29T08-15.md
<FEATURE>/evidence/other/p2-t2-record-after-add.2026-08-29T08-15.md
<FEATURE>/evidence/other/p3-t1-reported-repro.2026-08-29T08-15.md
<FEATURE>/evidence/other/p3-t2-swaps-page.2026-08-29T08-15.md
<FEATURE>/evidence/other/p3-t3-swap-guarded.2026-08-29T08-15.md
<FEATURE>/evidence/other/p3-t4-digits-flip.2026-08-29T08-15.md
<FEATURE>/evidence/other/p3-t5-comment-sync.2026-08-29T08-15.md
<FEATURE>/evidence/qa-gates/p2-t3-registereddigits-removed.2026-08-29T08-15.md
<FEATURE>/evidence/qa-gates/p2-t4-nullable-build.2026-08-29T08-15.md
<FEATURE>/evidence/qa-gates/p3-t6-frozen-file-interim.2026-08-29T08-15.md
<FEATURE>/evidence/qa-gates/p4-t1-csharpier-format.2026-08-29T08-15.md
<FEATURE>/evidence/qa-gates/p4-t2-csharpier-check.2026-08-29T08-15.md
<FEATURE>/evidence/qa-gates/p4-t3-analyzer-build.2026-08-29T08-15.md
<FEATURE>/evidence/qa-gates/p4-t4-nullable-build.2026-08-29T08-15.md
<FEATURE>/evidence/qa-gates/p4-t5-vstest-final.2026-08-29T08-15.md
<FEATURE>/evidence/qa-gates/p4-t6-coverage-final.2026-08-29T08-15.md
<FEATURE>/evidence/qa-gates/p4-t7-file-size-audit.2026-08-29T08-15.md
<FEATURE>/evidence/regression-testing/p1-t1-new-test-file.2026-08-29T08-15.md
<FEATURE>/evidence/regression-testing/p1-t2-csproj-registration.2026-08-29T08-15.md
<FEATURE>/evidence/regression-testing/p1-t3-prefix-build.2026-08-29T08-15.md
<FEATURE>/evidence/regression-testing/p1-t4-expect-fail.2026-08-29T08-15.md
<FEATURE>/evidence/regression-testing/p2-t5-ledger-green.2026-08-29T08-15.md
<FEATURE>/evidence/regression-testing/p3-t7-reconciled-green.2026-08-29T08-15.md
<FEATURE>/plan.2026-08-29T07-42.md
```

`<FEATURE>` above abbreviates `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644`.

## Acceptance evaluation

1. **Pathspec-scoped name-only listing is exactly the six code paths** — PASS. Six lines, matching
   the change footprint exactly.
2. **Contains neither the QuickFiler project file nor any path under the QuickFiler interfaces
   directory** — PASS. Neither appears in any span.
3. **Porcelain listing shows no path under `QuickFiler` or `QuickFiler.Test` outside those six** —
   PASS, vacuously: the listing is empty.
4. **`--stat` net insertions for the controller no greater than 10** — PASS. 18 insertions minus 9
   deletions is a net of 9, which is at or below 10.
5. **Repository-wide listing contains all six code paths** — PASS.
6. **Repository-wide listing carries no path outside the enumerated set** — see the deviation
   section below. No path outside the six code paths and the feature folder appears.
7. **Conditional `PRE-EXISTING FORMAT DRIFT SET` clause** — vacuously satisfied. `[P0-T8]` recorded
   that set as empty, so no member of it lies under `QuickFiler` or `QuickFiler.Test`.

## LITERAL-CLAUSE DEVIATION (recorded and reported, not widened)

The plan's sixth clause enumerates the admissible non-code paths as exactly four: issue.md,
spec.md, research/research.2026-08-29T07-55.md, and plan.2026-08-29T07-42.md, relative to the
feature folder. The observed repository-wide listing differs from that enumeration in two ways:

- **Absent from the listing:** issue.md, spec.md, and research/research.2026-08-29T07-55.md. They
  are unchanged relative to the substituted anchor, so `git diff` correctly omits them. The clause
  is a membership test rather than an equality, so their absence is not a failure.
- **Present but outside the enumeration:** 35 evidence artifacts under the feature folder's
  `evidence/` tree.

Category counts, measured rather than hand-counted, over the recorded span output: 42 total paths,
of which 6 are the code paths, 36 are under the feature folder (35 evidence artifacts plus
plan.2026-08-29T07-42.md), and 0 are anywhere else in the repository.

Cause of the delta. The clause's supporting prose states "The evidence artifacts this plan writes
under the feature folder are untracked and unstaged at this point and are correctly absent from
the listing." That presumption held for a single-segment execution. This run was resumed after an
earlier segment had already committed Phases 0 through `[P4-T7]` together with their evidence, so
those 34 artifacts are tracked at execution time and an anchored diff necessarily lists them. The
delta is anchor-independent: the same 34 artifacts are committed on this branch and would appear
against the plan's literal anchor `ecdb1c84ba8541ab67042985919cfed4df768c01` as well.

Why this is not treated as a widening of the acceptance. The hazard this clause exists to detect is
named in the plan's own supporting prose for this task: a rewrite made anywhere else in the
repository by `[P4-T1]`'s repository-wide `dotnet tool run csharpier format .`, which the three
pathspec-scoped spans cannot see. That hazard is absent. Every path in the listing is either one of
the six code paths or lies under the feature folder, and every one of the 34 extra paths is an
artifact this plan itself specifies by exact path under its "Evidence accounting rule". No file
outside the feature folder and outside the two project directories appears. The plan's own change
footprint section records that evidence artifacts under the feature folder "are orchestration
process outputs and are not part of the fix's code diff."

Disposition. The deviation is recorded here verbatim and is reported to the orchestrator alongside
this task's completion. The acceptance text is not edited and the enumerated set is not enlarged.

Output Summary: Footprint containment verified against the substituted anchor
`e968a1a8804b7641380d4489c496662824d45767`. The pathspec-scoped anchored name-only listing is
exactly the six code paths of the change footprint; the porcelain companion is empty; the
controller's `--stat` shows 18 insertions and 9 deletions for a net of +9, at or below the limit of
10; and the repository-wide anchored listing carries 42 paths in total — the six code paths plus 36
feature-folder paths — and nothing else. Neither the QuickFiler project file nor any path under the
QuickFiler interfaces directory appears. The `[P0-T8]` pre-existing format drift set is empty, so
the conditional remediation clause is vacuous. One literal-clause deviation is recorded above and
reported: 35 committed evidence artifacts under the feature folder fall outside the clause's
four-path enumeration because this resumed run committed them in an earlier segment, whereas the
clause presumed them untracked. No path outside the six code paths and the feature folder appears,
so the formatter-rewrite hazard the clause guards against is absent.
