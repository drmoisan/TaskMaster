# Head Reconciliation, `H1` to `H2` — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-30-00
- Task: [P6-T5]
- Finding: R1
- EXIT_CODE: 0

## `git diff --name-only <H1>..<H2>`

```
git diff --name-only de9a00106c951a073c1ac33a4cf5223e24563cd8..2d4374edc3c38d597d75ac86ad8ea20a7602261f
```

```
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/other/p6-t3-merge-time-instructions.2026-09-20T01-37.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p5-t14-commit.2026-09-20T01-37.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p6-t1-push.2026-09-20T01-37.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p6-t2-ci-run.2026-09-20T01-37.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/remediation-plan.2026-09-20T01-37.md
```

| Clause | Required | Measured | Result |
|---|---|---|---|
| Every path under the feature folder | yes | **5 of 5** | PASS |
| Count of such paths | at least 4 | **5** | PASS |

Zero paths outside
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`. The five
are four markdown evidence artifacts and the remediation plan.

**This clause fails if `H2` changed any file the CI toolchain builds or tests**, and therefore
fails if the `H1` run no longer describes the code at head. It does not fail: no `.cs`,
`.csproj`, `.ps1`, `.psm1`, `.yml`, `.sln`, `packages.config` or `app.config` path appears.

## Porcelain Companion

```
git status --porcelain --untracked-files=all
```

```
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p6-t4-commit.2026-09-20T01-37.md
```

**One entry, outside `coverage/` and `artifacts/`,** and it is recorded rather than glossed. It
is the [P6-T4] artifact, which records `H2` and therefore cannot be inside the commit that
produced `H2`. It is markdown under the feature folder and no CI gate reads it. [P6-T6] records
the same class and names the evidence-sweep commit that clears it.

The two captures are paired per **gate rule 8**, each being blind in the state the other covers:
the name-listing diff enumerates tracked changes only and cannot report an untracked file, and
porcelain goes empty once a change is committed. Here the diff reports the five committed paths
and porcelain reports the one untracked path, and neither alone would have reported both.

## `git push origin HEAD`, Verbatim

```
To https://github.com/drmoisan/TaskMaster.git
   de9a00106..2d4374edc  HEAD -> bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911
```

Exit code **0**. The remote branch advances from `H1` to `H2`, a fast-forward.

## Why the `H1` Run Is Evidence About the Code at `H2`

Stated in terms, as the task requires.

The CI run [P6-T2] recorded — run **35513025198**, `headSha` `de9a0010`, conclusion **success**,
six of six jobs green — executed against `H1`. Head is now `H2`.

**That run is evidence about the code at `H2` only because the intervening commit touched
documentation alone.** The five changed paths are four evidence artifacts and the plan file; not
one is an input to any of the six CI jobs. `_pester.yml` scans `tests/scripts/dependencies` and
`tests/scripts/vscode`; the build and coverage jobs read `TaskMaster.sln` and the project tree;
`format-check` reads the CSharpier scan set; `actionlint` reads `.github/workflows`. None reads
the feature folder.

Had any path outside the feature folder appeared in the diff above, this reasoning would not
hold and the run would have had to be re-dispatched at `H2`.

**The authoritative discharge of R1 remains the PR-context `CI` run at whatever SHA is
merged.** The run recorded here carries `event` `workflow_dispatch`, and the
`modified-workflow-needs-green-run` rule demands a green run of the modified gate at the exact
commit being merged. [P6-T3] records the same and names it as the closing evidence for R1.

## Output Summary

The `H1`-to-`H2` diff lists 5 paths, all under the feature folder and none an input to any CI
job, which is what licenses the `H1` run as evidence about the code at `H2`. Porcelain carries
one untracked entry, the [P6-T4] artifact. The push succeeded at exit 0, fast-forwarding the
remote branch from `de9a00106` to `2d4374edc`.
