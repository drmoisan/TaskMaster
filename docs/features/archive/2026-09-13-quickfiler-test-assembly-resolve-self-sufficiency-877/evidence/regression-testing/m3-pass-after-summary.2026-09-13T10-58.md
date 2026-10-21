# M3 repetition summary — issue #877

Timestamp: 2026-09-13T10-58
Command: read-only aggregation of the three committed M3 pass-after artifacts and their `.trx` files, no command executed
EXIT_CODE: 0
Output Summary: Repeat count 3. All three post-fix M3 runs passed 3 of 3 with exit code 0. The fail-before half of the pair is satisfied by citation and was not re-run.

## Repeat count

3

## Per-run results

| Run | `/ResultsDirectory:` leaf | `LogFileName=` | Total | Passed | Failed | Exit code |
|---|---|---|---|---|---|---|
| 1 | `m3-post-1` | `m3-post-1.trx` | 3 | 3 | 0 | 0 |
| 2 | `m3-post-2` | `m3-post-2.trx` | 3 | 3 | 0 | 0 |
| 3 | `m3-post-3` | `m3-post-3.trx` | 3 | 3 | 0 | 0 |

Each run used a distinct `/ResultsDirectory:` and a distinct `LogFileName=`, so no run read or overwrote another run's results file. All three runs used the identical command shape: `QuickFiler.Test.dll`, `/TestCaseFilter:FullyQualifiedName~QfcInitEmailQueueZeroBatchTests`, `/InIsolation`, and NO `/Settings:` file.

## Fail-before half of the pair

Cited, not re-run: `evidence/regression-testing/m3-fail-before.2026-09-13T09-14.md`, which records `EXIT_CODE: 1` with `ExpectedExitCode: 1`, Total 3 and Failed 3, at head `c9590a8b7` before any fix, with all three failures raising `FileNotFoundException: netstandard, Version=2.1.0.0` at Deedle static-initializer time.

## Interpretation

M3 is the stable discriminator. It failed 3 of 3 before the change and passes 3 of 3 after it, across three separate runs, with no change to the runsettings, the parallelization configuration, the test bodies, or their ordering. The mechanism recorded in `issue.md` `## Verified Mechanism` is therefore CONFIRMED: `QuickFiler.Test` now installs its own `AssemblyResolve` fallback from its own `[AssemblyInitialize]`, and no longer depends on an unrelated class touching `SVGControl.SvgRenderer` first.

The three post-fix artifacts aggregated here are `evidence/regression-testing/m3-pass-after-run1.2026-09-13T10-57.md`, `evidence/regression-testing/m3-pass-after-run2.2026-09-13T10-57.md` and `evidence/regression-testing/m3-pass-after-run3.2026-09-13T10-58.md`.
