# P8-T4 — AC20: disclosure is present and conditional

DEFERRED: credential or fixture absent

Timestamp: 2026-09-20T02-40

Command:

```
gh api repos/drmoisan/TaskMaster/actions/secrets --jq '[.secrets[].name] | sort'
gh pr list --repo drmoisan/TaskMaster --state open --json number,headRefName,author --jq '[.[] | select(.author.login == "app/dependabot")] | length'
```

EXIT_CODE: 0

## Output Summary

The deferred branch is taken. AC20 remains unchecked and is carried into the P8-T5 follow-up issue.

## The measurement that selected this branch

Quoted from `evidence/other/p8-t1-credential-availability.2026-09-19T09-44.md`:

```
QUERY1-EXIT: 0
QUERY1-OUTPUT-BEGIN
[]
QUERY1-OUTPUT-END
QUERY2-LENGTH: 0
```

`CREDENTIAL-PRESENT: false` and `DEPENDABOT-PR-COUNT: 0`. The live branch requires both
`CREDENTIAL-PRESENT: true` and a count greater than 0, so the deferred branch is selected. That
branch is explicitly authorised by the plan for this task.

## What the live branch would have captured

The pull-request body and label state for two runs: one that applied a repair outside the
analyzer-item and binding-redirect classes, and one that applied only those two classes. Acceptance
would be that both bodies carry a "Repairs applied" block enumerating repairs by project; that a
"Packages skipped" block is present on exactly those runs that recorded a skip; that
`deps:autofixed` is present on the first run and absent on the second; and that both label states
are captured. The absent-label case is what prevents an implementation that always labels from
passing.

## What is already verifiable without the credential, and where

The two halves of the disclosure the workflow itself controls are covered by static and unit
evidence, so the deferred half is the live behaviour on a real pull request rather than the logic:

| Element | Evidence available now |
|---|---|
| The `## Repairs applied` block is always emitted | `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`, case "carries a skipped block in the body, naming the package and its reason", and the agreeing-tree case asserting `No repairs were applied.` |
| The `## Packages skipped` block is emitted only when a skip was recorded | the same two cases, asserting presence in the skip run and absence in the run with no skip |
| `deps:autofixed` is applied only for a repair outside the two known-weak classes | `.github/workflows/dependabot-repair.yml`, the label step guarded on the `beyond-known-weak` output computed by excluding the `Analyzer` and `BindingRedirect` kinds |

What cannot be evaluated here is the end-to-end result on a pull request: that the body edit and the
label call actually land, under the App identity, against a real Dependabot branch.

## What has to happen before it can run

The credential provisioning in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md`,
an open Dependabot pull request, and two repair runs chosen so that one applies a repair outside the
analyzer-item and binding-redirect classes and the other applies only those.

AC20 remains **unchecked** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
