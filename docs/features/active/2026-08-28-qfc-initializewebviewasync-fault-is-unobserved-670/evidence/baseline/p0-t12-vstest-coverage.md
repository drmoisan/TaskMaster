# P0-T12 — Test-and-coverage baseline (toolchain stage 4)

Timestamp: 2026-09-01T19-48
Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\baseline.cobertura.xml`, followed by a copy of `coverage/baseline.cobertura.xml` to `evidence/baseline/baseline.cobertura.xml`
EXIT_CODE: 0

POSTPROCESSED: yes

## Output Summary

The script's discovery line, reproduced verbatim:

    Discovered 9 test assemblies.

Test result summary, reproduced verbatim:

    Test Run Successful.
    Total tests: 6934
         Passed: 6934
     Total time: 47.2656 Seconds

**Total 6934, passed 6934, failed 0, skipped 0.** The pre-existing suite is fully green on this tree.

Repository-wide line rate, read from the `line-rate` attribute of the Cobertura document root of the copied artifact:

    line-rate     = 0.853866  →  85.3866%
    lines-covered = 54983
    lines-valid   = 64393
    package nodes = 9

The runner prints no coverage percentage on a successful run. `Assert-CoberturaLineCoverageThreshold` emits a percentage only inside the exception it raises when the rate falls below 80%, so the headline value above is read from the artifact rather than from the console. This figure is the document's own aggregate and is recorded for orientation only; the pass/fail arithmetic in P0-T13, P4-T6 and P4-T8 is derived from the `line` nodes, per the plan's section 4.

## Which of the three admissible outcomes occurred

Outcome **(i)**: `EXIT_CODE: 0`. The runner completed normally, reached `Assert-CoberturaLineCoverageThreshold` without that helper throwing (85.3866% is above its 80% floor), and then wrote the post-processed document. Neither outcome (ii) — a non-zero exit carrying `Cobertura line coverage` — nor outcome (iii) — a non-zero exit carrying `MSTest with coverage failed with exit code` — occurred.

`POSTPROCESSED: yes` follows from the exit code being 0, and is corroborated directly by the console tail:

    Post-processing coverage XML for Koverage compatibility...
    Done. Coverage artifact: <repo-root>\.claude\worktrees\agent-<id>\coverage\baseline.cobertura.xml

This flag is load-bearing for P4-T8: a post-processed document carries repository-relative filenames and has the third-party `<package>` nodes removed, so its denominator differs from that of a raw `dotnet-coverage` document, and the two are not comparable until both sides are in the same state.

## Artifact location

The contents of `coverage/` are git-ignored (`.gitignore:144` reads `coverage/*` with `!coverage/.gitkeep` at `:145`), so the committed copy under `evidence/baseline/baseline.cobertura.xml` is the evidence. That file exists at the evidence path and measures 10,792,204 bytes. A case-insensitive fixed-string sweep of it for the drive-qualified user-profile root and for the drive-qualified Program Files root, in each of the two separator spellings, returns zero, which is consistent with the post-processing step having rewritten filenames to repository-relative form.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
