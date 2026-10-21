# Base anchor (issue #826, [P0-T2])

Timestamp: 2026-09-09T19-00

Command: `git rev-parse HEAD` / `git rev-parse --abbrev-ref HEAD` / two scoped
`git status --porcelain --untracked-files=all` invocations, run as one `pwsh -NoProfile -Command` block
with the plan's C2 preamble (branch guard active).

BaseCommit: dea7b49dae31a9bda8d35ecb73b8c8d646b1a460
Branch: bug/console-out-aggressors-and-banned-symbol-promotion-826-exec

EXIT_CODE: 0

## Porcelain span 1 — whole tree, excluding `.claude` and this feature folder

```
(empty)
```

Empty, which proves no file outside this feature is dirty at the base anchor.

## Porcelain span 2 — this feature folder only

```
 M docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/plan.2026-09-08T23-52.md
?? docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/evidence/baseline/phase0-instructions-read.md
```

Both entries are accounted for by Phase 0 work performed so far: the plan file carries the [P0-T1]
check-off, and the evidence path is the [P0-T1] artifact. No other path appears.

Output Summary: base anchor recorded as `dea7b49dae31a9bda8d35ecb73b8c8d646b1a460` on the expected
branch. Span 1 empty; span 2 lists only this plan file and one path under `<FEATURE>/evidence/`.

---

Timestamp correction, recorded rather than absorbed: the `Timestamp:` value in this artifact was initially written as an extrapolated value rather than read from the clock. At 2026-09-09T19-10 the executor read the real clock, observed the discrepancy, and replaced the value with this artifact's own observed filesystem write time. The corrected value is an observation; the superseded value was not.
