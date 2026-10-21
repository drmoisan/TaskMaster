# P4-T5 — Toolchain step 4 of 4: testing with coverage

Timestamp: 2026-09-01T20-16
Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\postchange.cobertura.xml`, then a copy of `coverage/postchange.cobertura.xml` to `evidence/qa-gates/postchange.cobertura.xml`
EXIT_CODE: 0

POSTPROCESSED: yes

## Which of the three admissible outcomes occurred

Outcome **(i)**: `EXIT_CODE: 0`. The runner completed normally, reached `Assert-CoberturaLineCoverageThreshold` without that helper throwing, and then wrote the post-processed document.

Neither of the other two admissible outcomes occurred:

- Outcome (ii) — a non-zero exit whose output contains `Cobertura line coverage`, identifying the runner's repository-wide 80% floor assertion rather than a test failure — did not occur.
- Outcome (iii) — a non-zero exit whose output contains `MSTest with coverage failed with exit code`, the throw `Invoke-DotnetCoverageCollection` raises on any non-zero vstest exit — did not occur.

Because the exit is 0, the runner reached line 343 and wrote the **post-processed** document, so `POSTPROCESSED: yes`. This is corroborated directly by the console tail:

    Post-processing coverage XML for Koverage compatibility...
    Done. Coverage artifact: <repo-root>\.claude\worktrees\agent-<id>\coverage\postchange.cobertura.xml

This flag matters for P4-T8: the P0-T12 baseline also recorded `POSTPROCESSED: yes`, so both sides of the comparison are in the same post-processing state and no normalization step is required.

## Output Summary

The script's discovery line, reproduced verbatim:

    Discovered 9 test assemblies.

Test result summary, reproduced verbatim:

    Test Run Successful.
    Total tests: 6938
         Passed: 6938

**Total 6938, passed 6938, failed 0, skipped 0.**

An independent per-test-line count confirms the summary: lines beginning `Passed ` number **6938**, and lines beginning `Failed `, `Skipped ` or `NotRunnable ` number **0**. The two counts come from different parts of the output and agree.

The total rose from the baseline's 6934 to 6938 — an increase of exactly **4**, matching the four tests this change adds and no more. The same nine test assemblies were discovered as at baseline, so no assembly was gained or lost.

Repository-wide line rate, read from the `line-rate` attribute of the Cobertura document root of the copied artifact:

    line-rate     = 0.853771  →  85.3771%
    lines-covered = 54988
    lines-valid   = 64406
    package nodes = 9

The runner prints no coverage percentage on a successful run: `Assert-CoberturaLineCoverageThreshold` emits a percentage only inside the exception it raises when the rate is below 80%, so this headline value is read from the artifact rather than from the console. It is the document's own aggregate and is recorded for orientation only; the pass/fail arithmetic in P4-T6, P4-T7 and P4-T8 is derived from the `line` nodes, per the plan's section 4.

## Substitution of the stage-4 command

CLAUDE.md and `spec.md` AC10 state stage 4 as `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. This task substitutes `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which invokes `dotnet-coverage collect` around that same `vstest.console.exe`. The substitution is required rather than convenient: `/EnableCodeCoverage` emits a binary `.coverage` file, while AC13 needs a per-file line figure and AC14 a comparable repository-wide counter set, both of which are read from the Cobertura document only this runner produces. The assemblies under test are the same set the literal command would name, enumerated by the runner itself. P4-T22 checks AC10 off against this substituted stage-4 command.

## Artifact location

The contents of `coverage/` are git-ignored, so the committed copy at `evidence/qa-gates/postchange.cobertura.xml` is the evidence.

## Position in the Phase 4 pass

This is stage 4 of the single uninterrupted toolchain pass P4-T1 through P4-T5. All five tasks ran in order — format, check, analyzers, nullable, test — with no stage failing and no stage rewriting a tracked file under `QuickFiler/` or `QuickFiler.Test/`, so the pass completed without a restart. AC10 is satisfied by this pass.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
