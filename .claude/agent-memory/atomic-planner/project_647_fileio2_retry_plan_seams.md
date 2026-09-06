---
name: project-647-fileio2-retry-plan-seams
description: Preflight R1/R2 seams from issue #647 (FileIO2 write-retry) — vstest needs explicit /Settings:TaskMaster.runsettings, staged diffs are blind by construction, ExpectedExitCode must key on the RUN not the baseline, permitted-set cardinality traps, and one-artifact-per-non-zero-capable-gate
metadata:
  type: project
---

Four reusable seams surfaced by preflight round 1 on the #647 plan, plus three from round 2.

**1. `vstest.console.exe` does not auto-detect `TaskMaster.runsettings`.** The repo-root `TaskMaster.runsettings` carries the Code Coverage `ModulePaths/Exclude` list (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest) at lines 14-24. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` resolves and passes it; a hand-rolled `vstest.console.exe /EnableCodeCoverage` run does not. Without `/Settings:` the collector instruments those modules and produces instrumentation-induced failures, so its failure set is NOT comparable to a `BASELINE_FAILURE_SET:` captured through the script.

**Why:** a subset-comparison gate against a baseline failure set silently becomes a different measurement when the two runs use different exclusion configuration.

**How to apply:** any plan task that invokes `vstest.console.exe` with `/EnableCodeCoverage` and compares its failures to a script-captured baseline must pass `/Settings:TaskMaster.runsettings` and record the path in evidence. Extends [[reference_vstest_scoped_run_command]].

**2. Order the `ExpectedExitCode:` branches failure-set first, coverage-floor second.** `Invoke-MSTestWithCoverage.ps1` line 236 (`throw "MSTest with coverage failed with exit code $coverageExitCode"`, inside `Invoke-DotnetCoverageCollection`, invoked at 326-331) fires before the post-processing at 340/341/343. A suite with ANY test failure therefore never reaches `Assert-CoberturaLineCoverageThreshold`, the floor is never evaluated, and the on-disk Cobertura is never rewritten so it carries no `<sources>` element.

**Why:** a plan that declares `ExpectedExitCode: 1` only for a carried coverage-floor blocker mis-predicts the exit code whenever the carried blocker is a test failure instead — and also mis-predicts which branch of a `<sources>`-keyed derivation applies. Sharpens [[project_494_threshold_reconciliation_plan_seams]].

**How to apply:** when both a carried failure set and a carried coverage shortfall are possible, state three ordered rules (failure-set, then floor, then 0) and say explicitly which derivation branch the first rule forces.

**3. `git diff --cached --name-only` after an enumerated-pathspec `git add` is blind by construction.** The index contains only what the pathspec staged, so an out-of-footprint rewrite (e.g. from a repo-wide `csharpier format .`) can never appear, and any "no path ending `.csproj`" clause over that list is unreachable.

**Why:** the footprint claim is exactly the claim the staged list cannot falsify.

**How to apply:** pair the staged observation with `git diff --name-only <BASE_SHA> -- ":(exclude).claude"` and put the extension/exclusion clauses on the worktree list, not the staged list. After a pathspec-scoped commit, the same blindness recurs — add an `UNCOMMITTED_PATHS:` observation and gate on the union. See [[agent-memory-is-tracked-scope-git-gates]] for why `.claude` is excluded.

**4. Count the permitted zero-hit lines against the plan's own deletions.** The plan permitted exactly two uncovered lines (the production-default delegates) while separately deleting the only test that called the public forwarding overload and requiring all remaining calls to bind the seam — guaranteeing a third uncovered line and making the gate unsatisfiable.

**How to apply:** after writing a "the only uncovered lines are N and M" clause, re-walk every test-deletion and every call-shape mandate elsewhere in the plan and re-derive N. Related: [[wiring-gates-must-be-wiring-sensitive]].

**5. An `ExpectedExitCode:` selection rule must key on THIS RUN's observations, not on the recorded baseline.** R2 rejected the R1 branch ordering because both directions break: a rule reading "when `BASELINE_FAILURE_SET:` is a name list, declare 1" is satisfied by ANY non-zero exit including one caused by a regression the change just introduced (vacuous), and when the baseline failures are non-deterministic and do not recur the run exits 0 against a declared 1, failing a gate whose restart rule then re-runs a repo-wide format, two `/t:Rebuild` builds, a full vstest run and a 20-minute coverage run forever (non-terminating).

**Why:** the recorded baseline is a property of a past run; the exit code being predicted is a property of the current one, so keying the prediction on the baseline predicts the wrong quantity in both directions.

**How to apply:** phrase each branch as "when this run reports X and X is a subset of the recorded baseline", and give the coverage-floor branch its own current-run test (`this run reports no Failed test` AND the run's own derived `POST_LINE_RATE:` is below the floor). Verify the run task itself derives every field the rule references. Sharpens seam 2 above.

**6. A downstream clause must not presuppose the cardinality of an upstream permitted-set enumeration.** The upstream task enumerated "every line whose hit count is 0" against a permitted set of three — an upper bound satisfied at cardinality 0, 1, 2 or 3. The downstream AC task said "all three of which are enumerated ... and match the three lines" and so became false whenever CSharpier wrapped a default-delegate declaration onto a line shared with executed code.

**How to apply:** state the downstream clause as a set-identity against the upstream's observed set plus a membership test against the permitted set, never as a count. Exclude the permitted lines from BOTH numerator and denominator when a rate is asserted. Related: [[enumerate-condition-outcomes-before-case-list]].

**7. One task, two independently non-zero-capable gates, two artifacts.** `ExpectedExitCode:` is a per-FILE field, so a task recording a format check and a test run in one artifact cannot express the case where one gate exits 1 and the other 0. Split the evidence into one artifact per non-zero-capable gate, then re-grep every reader of "the <task> artifact" and confirm which of the two each resolves to.
