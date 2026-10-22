# 2026-09-11-ci-coverage-threshold-and-pester-gates (User Story)

- **Issue:** #869 (closes #561 and #562 on merge)
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12
- **Status:** Ready for planning
- **Work Mode:** full-bug

> **Acceptance-criteria banner.** This feature is `full-bug`, so `docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/spec.md` is the **sole** acceptance-criteria source. This document contains no checkboxes and no acceptance criteria; nothing here is tracked, checked off, or audited as a criterion. It exists only because the preparation directive for this parallel run names it explicitly, so that the maintainer who performs the out-of-band branch-protection edit and the downstream consumers of the two new gates have a narrative statement of intent that does not require reading the full specification.

## Who this is for

Three audiences, in the order they encounter the change.

**The maintainer performing the branch-protection edit.** After the pull request merges, one repository-settings change remains, and it is deliberately not part of the delivery. The `main` branch ruleset must gain the new required check-run context for the Pester gate. The specification's manual follow-up section carries the operational detail: the ruleset id, the predicted context string, the requirement to capture the real string from a live run rather than hand-writing it, the prohibition on a two-step remove-then-add, and the fact that the existing required contexts stay required and unchanged. The single most important point for this audience is that the C# threshold work adds **no** new context, because it goes inside the existing MSTest coverage job and changes no job name. The issue text anticipated two new contexts; that expectation is superseded, and acting on it would add a context that never reports and would block every future pull request.

**Contributors whose pull requests the new gates will judge.** Two failure modes become possible that were not possible before. A change that lowers first-party C# line coverage below 80 percent, or first-party C# branch coverage below 75 percent, fails the MSTest coverage job. A change that lowers the PowerShell line coverage of the developer tooling below 80 percent, or that breaks any Pester test, fails the new Pester job. Both gates print the measured figure before they fail, so a red run identifies the number that moved rather than only the fact that something moved. Both gates also fail when their input is missing: an absent coverage document fails the upload step rather than passing quietly, which closes the path by which a coverage gate can be defeated by withholding what it measures.

**Reviewers reading the diff.** Two points in the change will look like scope creep and are not. First, two production PowerShell scripts gain an extracted main function and an invocation guard. This is not an opportunistic refactor: the existing test for the build script dot-sources an unguarded script body, which launches a process locator and executes the package-reference sync script against the real repository during a test run. That is both the source of the reported non-determinism and the source of a material share of the directory's measured coverage, so a test-only fix would remove coverage the item cannot afford to lose. Second, the PowerShell floor asserted here is 80, while three rules files state 85. That gap is a maintainer decision recorded under #563 and is documented in the specification as a ratified exception with citations. The rules files are pushed down from an upstream repository and are not edited here.

## What changes, in plain terms

Today the pipeline measures C# coverage and then discards the result: nothing compares it to a number, so a coverage regression merges green. The PowerShell that performs that measurement is itself never tested in CI, so a defect in the coverage arithmetic would also merge green. This item closes both gaps in one delivery, using the thresholds the maintainer settled under #563.

On the C# side, the existing coverage job stops running the test command inline and instead runs the same script the local tooling runs, so the pipeline produces the same post-processed, first-party coverage document that developers see locally. The existing line assertion then reaches execution for the first time in CI, and a new branch assertion runs beside it at the same point, reading the same document. The branch assertion fails closed: a missing counter, an unparseable value, an impossible value, or a document reporting no branches at all is treated as a failure, not as a pass, because a projection with nothing to measure must never read as success.

On the PowerShell side, a new reusable workflow runs the existing Pester suite over the developer tooling with coverage, asserts the line floor, and fails on any test failure. Reaching that floor requires new tests, because the current measurement sits below it; the specification carries the arithmetic and the ranked targets. If the floor cannot be reached with the planned work, the item halts and reports rather than lowering the floor or excluding a file from measurement.

## What deliberately does not change

No product code and no C# source are touched. No file is excluded from coverage measurement. No PowerShell branch figure is asserted or reported, because the tooling does not measure one and a reviewer would treat its appearance as a policy violation. The repository coverage settings file is left alone, and the coverage helpers script, which is close to its line ceiling, does not receive the new assertion.

## How success is recognised

The pull request's own run shows the new Pester check alongside the existing checks, and both coverage gates pass on their measured figures rather than vacuously. A deliberately introduced regression on each side turns the corresponding job red, captured as evidence in the feature folder. The measured PowerShell figure is identical across two consecutive clean runs, which is the observable proof that the seam defect is fixed. The maintainer then applies the ruleset edit using the context name captured from the live run.

## The two gates as delivered, from the maintainer's point of view

Two gates are now enforced on every pull request, and between them they enforce **three figures**.

**Gate one, the MSTest coverage job.** It reports under the unchanged context `mstest-coverage / Run MSTest suite with coverage`. It runs the same route as the local tooling, produces the post-processed first-party Cobertura projection, and then asserts two floors against that projection's document root:

- **80** for C# line coverage, enforced by `Assert-CoberturaLineCoverageThreshold`. Measured at 85.87 percent on the delivered tree.
- **75** for C# branch coverage, enforced by the new `Assert-CoberturaBranchCoverageThreshold`. Measured at 80.04 percent on the delivered tree.

The branch assertion additionally refuses a document that reports no valid branches, so a projection with nothing to measure fails rather than passing. A non-zero exit from the script propagates to the job through the step's exit-code guard, and the coverage document is uploaded with `if-no-files-found: error`, so an absent document fails the job too.

**Gate two, the Pester job.** It reports under the new context predicted as `pester / Run Pester suite with coverage`, which is the one context the maintainer must add to the `main` ruleset. It runs the Pester suite over the developer tooling with coverage scoped to `scripts/vscode`, prints the measured figures, and then asserts one floor:

- **80** for PowerShell line coverage, read from the JaCoCo `LINE` counter. Measured at 83.93 percent on the delivered tree, which is 34 covered lines above the floor.

It exits non-zero on any test failure as well, through an explicit exit placed after the figures print, so a red run always records the numbers. No PowerShell branch figure is asserted, printed or computed anywhere, because Pester measures none.

The three enforced figures are therefore **80 for C# line, 75 for C# branch, and 80 for PowerShell line**. All three are fixed by the #563 maintainer decision and none was lowered by this delivery. New code added by the delivery reaches 98.36 percent line coverage against the separate 90 percent new-code floor.
