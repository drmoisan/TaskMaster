# Scope Boundary Against the Merge Base

Recorded by `[P6-T5]`.

Timestamp: 2026-09-14T13-00

Command, the three spans in this order:

```
git rev-parse --verify origin/main
git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs/features"
git diff --name-only origin/main...HEAD
```

EXIT_CODE: 0

ANCHOR: origin/main, substituted for the stale local main ref

AC14 at `spec.md` line 536 names "the merge base with `main`". This task anchors on
`origin/main` instead, and the substitution is recorded here rather than left implicit. In a
worktree-per-item run the local `main` ref is stale: it stands at `03d2ece20` while `origin/main`
stands at `a49c9729e`, and `a49c9729e` is already merged into this branch at `f02cee3fe`, so
`origin/main...HEAD` is the merge-base diff AC14 describes. Anchoring on the stale local ref
would additionally list every change `origin/main` gained since `03d2ece20`, none of which this
work authored. The AC text is not amended: `spec.md` line 536 is the first line of a seven-line
criterion spanning lines 536-542, every `[P6-T#]` check-off addresses `spec.md` by line number,
and a re-wrap at line 536 would move the five criteria that follow it.

Output Summary:

`git rev-parse --verify origin/main`:

```
a49c9729e27689c6a3963d7bf6037a0517fa7716
```

`git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs/features"`:

```
NONE
```

The porcelain span returned no output. Its pathspec is repository-wide rather than scoped to the
four source directories, because every path this task's acceptance condition prohibits sits
outside those four directories and a span scoped to them could not report an uncommitted change
to any of them. The two exclusions are the two path classes the inherited-path rule places
outside every scope assertion in this plan.

The porcelain span is the companion the name-listing diff needs. An anchored
`git diff --name-only` enumerates tracked committed changes only, so a path this plan created is
invisible to it until it is committed, and `[P6-T1]` has committed it.

`git diff --name-only origin/main...HEAD`, 58 paths:

```
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs
TaskMaster.Test/TaskMaster.Test.csproj
TaskMaster/ThisAddIn.cs
TaskMaster/app.config
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs
UtilitiesCS.Test/UtilitiesCS.Test.csproj
UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs
UtilitiesCS/UtilitiesCS.csproj
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/analyzer-baseline-console.2026-09-13T18-22.projection.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/analyzer-baseline.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/appdomain-resolve-field-probe.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/build-output-premises.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/format-baseline.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/nullable-baseline-console.2026-09-13T18-22.projection.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/nullable-baseline.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/outlook-closed-gate.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/phase0-instructions-read.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/test-coverage-baseline.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/toolchain-bootstrap.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/write-set-decision.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/ac-inventory.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/console-log-projections.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/deedle-member-surface.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/file-size-audit.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/hardening-not-the-fix.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/netstandard-2-0-0-0-child-domain-observation.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/preflight-clearance.2026-09-14T01-55.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/scope-and-determinism-checks.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/static-shape-checks.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/analyzer-final-console.2026-09-13T18-22.projection.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/analyzer-final.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/coverage-assemblybindingfallback.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/coverage-delta.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/format-final.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/nullable-final-console.2026-09-13T18-22.projection.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/nullable-final.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/outlook-closed-gate.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/r7-build.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/test-final.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-build-console.2026-09-13T18-22.projection.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-build.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-run-superseded-probe-surface.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-run.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/isolation-field-decisive-check.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/pass-after-build-console.2026-09-13T18-22.projection.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/pass-after-build.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/pass-after-harness.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/pass-after-ladder-r7.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/pass-after-ladder.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/pass-after-shape.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/issue.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/plan.2026-09-13T18-22.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/research/2026-09-13T19-05-deedle-netstandard-bind-research.md
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md
docs/features/potential/promoted/2026-09-14-fsharp-core-hintpath-netstandard21-skew.md
```

Acceptance Condition: MET.

- `git rev-parse --verify origin/main` exited 0 and printed `a49c9729e27689c6a3963d7bf6037a0517fa7716`.
- This artifact records the sentence `ANCHOR: origin/main, substituted for the stale local main ref`.
- The porcelain span returned no output.
- The diff list contains none of the eleven prohibited exact paths, no path beginning `.github/`,
  no path beginning `.claude/hooks/`, no path beginning `.claude/rules/`, and no path ending
  `packages.config`. This was checked mechanically over the 58-path list rather than by reading,
  and the check reported `PROHIBITED_TOTAL=0`.

The eleven source and project paths above are exactly the authorised write set. The remaining 47
paths are this plan's own feature folder plus one promoted follow-up entry,
`docs/features/potential/promoted/2026-09-14-fsharp-core-hintpath-netstandard21-skew.md`, which
records the `FSharp.Core` HintPath skew as out of scope for this plan and tracked separately.
