# dependabot-repair-deferred-credential-criteria-and-residuals (Issue #914)

- Date captured: 2026-09-20
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/dependabot-repair-deferred-credential-criteria-and-residuals/ (Issue #914)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #914
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/914
- Last Updated: 2026-09-20
## Summary

Follow-up to issue #911. Three acceptance criteria of that change depend on a GitHub App installation
token that does not exist yet, two PowerShell files carry formatting #911 deliberately left alone,
and Phase 7 surfaced two further residuals. This entry carries all of them so none is lost when #911
merges.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (.NET Framework 4.8.1 VSTO solution)
- Command/flags used: measured in the #911 execution worktree on 2026-09-20
- Data source or fixture: repository secrets query and open-pull-request query, both exiting 0

## Steps to Reproduce

1. Query repository secrets and open Dependabot pull requests; both return empty.
2. Observe that AC18, AC19 and AC20 of #911 cannot be exercised without a credential and a fixture.
3. Run the repository formatter over `scripts/vscode/`; observe two files it would rewrite.
4. Inspect the six `app.config` files named below against their restored packages.

## Expected Behavior

Every acceptance criterion of #911 is exercised against a live fixture, the repository formatter
leaves no file it would rewrite, and every binding redirect names the assembly version the restored
package actually ships.

## Actual Behavior

Three criteria are deferred, two files remain unformatted by deliberate scope decision, ten binding
redirects are stale, and one shipped module carries an unreached defect in its own entry point.

## Logs / Screenshots

- [x] Attached minimal logs or snippet

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


## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Nothing here blocks #911. The deferred criteria are unverifiable rather than failing, the formatting
and redirect items are pre-existing, and the module defect is unreachable through the shipped
composition root.

## Suspected Cause / Notes

See the measurements above. The entry-point defect is the one worth acting on soonest: it is live
code in a module #911 ships, and nothing asserts its unreachability.

## Proposed Fix / Validation Ideas

- [ ] Provision the GitHub App credential per the #911 runbook, then exercise AC18, AC19 and AC20
      against a live Dependabot pull request.
- [ ] Decide whether to format the two `scripts/vscode/` files or record them as permanently excluded.
- [ ] Reconcile the ten stale binding redirects, or state why they are correct as written.
- [ ] Fix `Invoke-ProjectConsistencyRepair` so it cannot rewrite assembly versions to package
      versions, and add a test covering that path.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
