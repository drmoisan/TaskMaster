Follow-up to issue #911. Three acceptance criteria of that change depend on a GitHub App
installation token that does not exist yet, and two PowerShell files in `scripts/vscode/` carry
formatting the repository's formatter would rewrite but which #911 deliberately left alone. This
issue carries both, so neither is lost when #911 merges.

## 1. Three criteria deferred for want of a credential and a fixture

Measured in the execution worktree on 2026-09-20, with both queries exiting 0:

```
gh api repos/drmoisan/TaskMaster/actions/secrets --jq '[.secrets[].name] | sort'
QUERY1-EXIT: 0
[]

gh pr list --repo drmoisan/TaskMaster --state open --json number,headRefName,author --jq '[.[] | select(.author.login == "app/dependabot")] | length'
QUERY2-EXIT: 0
0
```

`CREDENTIAL-PRESENT: false`, from a successful secrets query returning an empty name list rather
than from a forbidden one. `DEPENDABOT-PR-COUNT: 0`; the repository currently has no open pull
request of any author. Both conditions for the live branch therefore fail independently.

The credential is provisioned by hand following the runbook at
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md`:
create the App, grant it contents and pull-requests write, install it on this repository, and store
`DEPENDABOT_REPAIR_APP_ID` and `DEPENDABOT_REPAIR_APP_PRIVATE_KEY` as repository secrets. An open
Dependabot pull request is then needed as the fixture.

### AC18 — the repair commit is pushed under the GitHub App identity

After a repair run on the fixture pull request:

```
gh api repos/drmoisan/TaskMaster/pulls/<PR> --jq '.head.sha'
gh api repos/drmoisan/TaskMaster/commits/<HEAD_SHA> --jq '.author.login'
```

Pass when the head SHA differs from the pre-repair SHA and the login ends with `[bot]` and is not
`github-actions[bot]`.

### AC19 — the required checks re-run and pass on the post-repair head SHA

```
gh api repos/drmoisan/TaskMaster/rulesets/18572843 --jq '[.rules[] | select(.type == "required_status_checks") | .parameters.required_status_checks[].context] | sort'
gh api repos/drmoisan/TaskMaster/commits/<HEAD_SHA>/check-runs --jq '.check_runs[] | {name, status, conclusion, details_url}'
gh api repos/drmoisan/TaskMaster/actions/runs/<run_id> --jq '.event'
```

Pass when the run-time-derived required-check list is non-empty, every member has a check run on the
post-repair head SHA, every originating event resolves to `pull_request`, every conclusion is
`success`, and no run carries `action_required` as a conclusion or `waiting` as a status.

### AC20 — disclosure is present and conditional

Capture the pull-request body and label state for two runs: one applying a repair outside the
analyzer-item and binding-redirect classes, one applying only those two classes. Pass when both
bodies carry a "Repairs applied" block enumerating repairs by project, a "Packages skipped" block
appears on exactly those runs that recorded a skip, and `deps:autofixed` is present on the first run
and absent on the second.

## 2. Two files carrying unformatted PowerShell on main

`scripts/vscode/Invoke-MSTest.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1`. Issue #911
reverts any formatter rewrite outside its own write set at every format step, so these two are left
as they are on `main` rather than being reformatted inside an already large pull request. Formatting
them is a small standalone change.

## 3. Related observation from the same work: stale binding redirects

Ten `bindingRedirect` entries across six `app.config` files name an older assembly version than the
restored package and the sibling project reference declare. Measured against the merge base
`734112ed25bba293cb074e71fee2286bc3b72fae`, so the drift predates the #911 branch:

| Application configuration | Assembly | Redirect declares | Reference and restored package declare |
|---|---|---|---|
| `QuickFiler/app.config` | `Microsoft.Bcl.Memory` | 10.0.0.11 | 10.0.0.12 |
| `SVGControl/app.config` | `Fizzler` | lower than 1.3.1.0 | 1.3.1.0 |
| `SVGControl/app.config` | `System.Runtime.CompilerServices.Unsafe` | lower than 6.0.3.0 | 6.0.3.0 |
| `SVGControl.Test/app.config` | `MSTest.TestFramework` | lower than 4.4.0.0 | 4.4.0.0 |
| `ToDoModel/app.config` | `Microsoft.Bcl.Memory` | 10.0.0.11 | 10.0.0.12 |
| `UtilitiesCS/app.config` | `AngleSharp` | 1.7.1.0 | 1.8.1.0 |
| `UtilitiesCS/app.config` | `Microsoft.Bcl.Memory` | 10.0.0.11 | 10.0.0.12 |
| `UtilitiesCS/app.config` | `Microsoft.Bcl.Numerics` | 10.0.0.11 | 10.0.0.12 |
| `UtilitiesCS/app.config` | `Microsoft.Extensions.Diagnostics.Abstractions` | 10.0.0.11 | 10.0.0.12 |
| `UtilitiesCS.Test/app.config` | `Microsoft.Bcl.Memory` | 10.0.0.11 | 10.0.0.12 |

A redirect that maps a version range onto an assembly version the tree does not contain resolves to
a missing assembly at runtime for any request inside that range. The #911 repair pass reconciles a
binding redirect only for a package the run upgraded, so it leaves this pre-existing drift in place
deliberately; correcting it is a behaviour change unrelated to the upgrade that pass repairs. The
tooling #911 delivers can perform the correction: run
`scripts/dependencies/Repair-PackageManifestConsistency.ps1` with the affected packages supplied as
candidate upgrades at their current versions, or extend the pass to reconcile every redirect and
accept the resulting six-file diff.

Evidence:
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p7-t5-ac15-repair-idempotence.2026-09-19T09-44.md`.

## 4. Related observation from the same work: a latent defect in `Invoke-ProjectConsistencyRepair`

`Invoke-ProjectConsistencyRepair` calls `Invoke-VersionReconciliation` without `-AssemblyVersion`.
That parameter's documented fallback is the package manifest version, so the call rewrites a
`<Reference>` assembly version to the package version wherever the Include's simple name equals the
package identifier. Measured over the working tree, that fallback produces 51 such rewrites in
`QuickFiler.csproj` alone, for example `Apache.Arrow, Version=23.0.0.0` to `Version=23.0.0` and
`Microsoft.Data.Analysis, Version=1.0.0.0` to `Version=0.23.0`. An assembly version is not required
to track its package version, and the module's own help states that the caller supplies the resolved
value.

The defect ships unreached. The composition root delivered by #911 wires the module functions
directly and supplies the resolved assembly version itself, so no shipped code path reaches the
fallback. Correcting it means either resolving the assembly version inside
`Invoke-ProjectConsistencyRepair` or removing that entry point in favour of the composition root.

Whether this warrants an issue of its own is pending the coordinator's decision; it is recorded here
so that the observation is not lost when #911 merges.

Evidence:
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p7-t1-composition-root.2026-09-19T09-44.md`.
