# [P6-T12] No test-results file and no coverage document entered the tree

Timestamp: 2026-09-08T03-14

Command: `git status --porcelain --untracked-files=all`

Command: `git ls-files -- TestResults coverage`

Command: `git diff --name-only pre-809-base -- '*.trx' '*.cobertura.xml' '*.coverage'`

EXIT_CODE: 0

## `git ls-files -- TestResults coverage`, verbatim

```
coverage/.gitkeep
```

That output is exactly the single line `coverage/.gitkeep` and nothing else. `TestResults/` matches the `[Tt]est[Rr]esult*/` entry at `.gitignore:39`, and the contents of `coverage/` match `.gitignore:144` `coverage/*`, while `.gitignore:145` exempts the tracked `coverage/.gitkeep`. Both the derived settings file `coverage\809-effective-coverage.config` and the two Cobertura documents `coverage\809-p0-baseline.cobertura.xml` and `coverage\809-p5-final.cobertura.xml` are therefore invisible to git, as are the twelve results directories under `TestResults\`.

## `git diff --name-only pre-809-base -- '*.trx' '*.cobertura.xml' '*.coverage'`

The span returned no lines.

DELIVERY_ADDED_RESULTS_FILE_COUNT: 0

A repository-wide `git ls-files -- '*.trx' '*.cobertura.xml' '*.coverage'` is deliberately not used here: earlier feature folders under `docs/features/active/` committed their own `.trx` and `.cobertura.xml` evidence, so that spelling would enumerate hundreds of pre-existing tracked paths and a count asserted over it to be `0` could never pass. Anchoring the diff to `pre-809-base` restricts the count to paths this delivery added, which is the quantity the gate is about.

## `git status --porcelain --untracked-files=all`, verbatim

```
 M docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/issue.md
 M docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/spec.md
?? docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/other/p6-t4-ac2-regression-reconciliation.md
?? docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/qa-gates/p6-t1-uithread-file-coverage.md
?? docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/qa-gates/p6-t2-changed-line-coverage.md
?? docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/qa-gates/p6-t3-aggregate-coverage.md
```

No line of that output names a path under `TestResults/` or under `coverage/`. The two modified paths are the acceptance-criteria check-offs [P6-T5] through [P6-T11] wrote, and the four untracked paths are Phase 6 evidence artifacts that [P6-T15] commits.
