# P5-T21 — AC22 red-before and green-after pair for the AC21 regression test

Timestamp: 2026-09-19T09-44

## Cited artifacts

| Role | Path | Exists |
|---|---|---|
| Failing run | `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p5-t5-ac22-fail-before.2026-09-19T09-44.md` | yes |
| Passing run | `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p5-t20-ac21-908-divergence-resolved.2026-09-19T09-44.md` | yes |

## Side-by-side

| | Failing run (P5-T5) | Passing run (P5-T20) |
|---|---|---|
| `EXIT_CODE` | 1 | 0 |
| `ExpectedExitCode` | 1 | not declared, so 0 |
| `Passed` | 0 | 1 |
| `Failed` | 1 | 0 |
| `Skipped` | 0 | 0 |
| `Total` (executed population) | 1 | 1 |
| `Total` (discovered, invariant under the filter) | 13 | 13 |
| `NotRun` | 12 | 12 |
| Filter | `*AC21-*` | `*AC21-*` |
| Run path | `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` | same |

Both `Total` values are exactly 1 on the executed reading, and identical on the discovered
reading, so the same single-case population ran in both directions.

## The `It` name, in both runs

```
Project consistency reconciliation and verification.Three-way divergence from pull request 908.AC21- reports separate guard and analyzer disagreements before repair and reconciles all three locations after
```

The fully expanded path recorded as failing at P5-T5 and as passing at P5-T20 is
character-for-character the same string. The red and the green are therefore the same
test, not two tests with similar names.

## Failure message recorded in the failing artifact, verbatim

```
Expected the actual value to be greater than 0, because the Import and Error guards disagree with the manifest, but got 0.
```

This references **the assertion**, not a missing module and not a missing command. It is
the first assertion of the case: against the pass-through tree,
`Find-VersionDisagreement` returned an empty finding set, so the guard-disagreement count
was 0 where the case requires it to be greater than 0.

Both structural failure modes were positively excluded before the red run, and P5-T5
records the exclusions: `Import-Module` of both modules succeeded with `-ErrorAction Stop`
at P5-T1 and P5-T2, and `Get-Command -Module` listed every function the case invokes. An
import failure would have produced a container-level error rather than one failed test, and
an unresolved command would have produced a `CommandNotFoundException` message rather than
an assertion message.

## What changed between the two runs

The three declared pass-throughs were implemented: version reconciliation at P5-T6, binding
redirect reconciliation at P5-T7, the verifier at P5-T8 and the analyzer item repair at
P5-T12. No fixture, no assertion and no `It` name in
`tests/scripts/dependencies/ProjectConsistency.Tests.ps1` was changed between the failing
run and the passing run. The suite was authored once, at P5-T4, before the red run.

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| Both cited artifacts exist | yes | both present at the paths above |
| Failing artifact records `EXIT_CODE: 1` | yes | 1 |
| Failing artifact records `Failed` at least 1 | >= 1 | 1 |
| Passing artifact records `EXIT_CODE: 0` | yes | 0 |
| Passing artifact records `Failed=0` | yes | 0 |
| Both `Total` values exactly 1 | yes | 1 and 1, executed population |
| Failure message references the assertion, not a missing module or command | yes | quoted above |

A test that cannot be shown failing is not admitted. This one was shown failing on the
pass-through tree and passing on the delivered tree, with no intervening edit to the test.

**This task checks off AC22** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
