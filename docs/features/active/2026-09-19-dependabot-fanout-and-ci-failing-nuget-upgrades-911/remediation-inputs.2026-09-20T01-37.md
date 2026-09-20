# Remediation Inputs — Issue #911

- Date: 2026-09-20
- Source review artifacts:
  - `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/policy-audit.2026-09-20T01-37.md`
  - `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/code-review.2026-09-20T01-37.md`
  - `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/feature-audit.2026-09-20T01-37.md`
- Branch: `bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911` at `794d34f02`
- Base: `origin/main`, merge base `734112ed25bba293cb074e71fee2286bc3b72fae`

## Remediation Triggers Fired

| Trigger | State |
|---|---|
| Policy audit contains FAIL results | yes — coverage, workflow green-run, artifact hygiene |
| Toolchain checks fail | no |
| Code review contains blockers | yes — 4 |
| Required acceptance criteria FAIL or PARTIAL | no — 23 PASS, 3 UNVERIFIED with an evidenced deferral |
| Coverage below policy threshold | yes — one modified production file at 74.80 percent |
| Coverage artifact absent for a language with changed files | yes — both canonical paths absent, figures substituted |

## R1 — Blocking. Record a green workflow run at the branch head

**Invariant to restore:** every change to a CI gate is demonstrated green by that gate at the exact
commit being merged, not at an ancestor.

Six paths under `.github/workflows/**` changed, one of them a new job holding `contents: write` and
`pull-requests: write`, and `_pester.yml` changed its run and coverage scope. No workflow run exists
against `794d34f02`.

Discharge by either route:

- `gh workflow run CI --ref bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`, wait for
  completion, then record the run id, head SHA and conclusion. The head SHA in the recorded run must
  equal the branch head at merge time.
- Open the pull request and record the PR-context `CI` run once it concludes success.

Write the evidence to
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/`.

## R2 — Blocking. Raise `scripts/vscode/Sync-PackageReferences.ps1` above the coverage floor

**Invariant to restore:** a rewritten production file's negative, edge and error paths are exercised,
not merely its happy path.

Current state: 95 covered of 127 instrumented lines, 74.80 percent. Nineteen uncovered lines are the
`Get-PackageSyncSeam` delegate table and four are the top-level invocation; those legitimately remain
in the denominator under the Coverage Exclusion Policy and must not be excluded. The nine that matter
are pure logic.

Add one test per uncovered behaviour, all drivable through the existing injected seam with no
temporary file:

| Line | Behaviour to assert |
|---|---|
| 151 | `Get-PackageIdentifier` returns empty for a folder no manifest identifier matches |
| 180 | `Resolve-PackageAssetFolder` returns empty when `TestPath` reports the library directory absent |
| 248 | the repair emits the warning and continues when the shared module selects no asset folder |
| 290 | `Set-ReferenceAssemblyVersion` returns the text unchanged when the Include attribute does not match |
| 293 | `Set-ReferenceAssemblyVersion` returns the text unchanged when the version already agrees |
| 330 | the project result is returned unchanged when the directory holds no `.csproj` |
| 336, 337 | the project is skipped with a warning when conflict markers are present |
| 345 | the project result is returned when the repair set is empty |

Line 248 is the priority: it is the handler for the exact condition issue #902 introduced, and AC7
asserts only what the selector returns, never what the script does with that answer.

Target: at least 85 percent line coverage on that file, which the nine logic lines plus the four
entry lines would reach.

## R3 — Blocking. Gate the repair push on the write set, not on the repair count

**Invariant to restore:** if the repair pass wrote a file, the workflow commits and pushes it.

`.github/workflows/dependabot-repair.yml` conditions the commit step on
`steps.repair.outputs.repair-count != '0'`. `RepairCount` is defined in `ConsistencyVerifier.psm1` as
`@($Repair).Count` over the per-project verification reports, which counts project-file repairs only.
Two write classes never increment it:

- manifest normalisation, whose changed paths are appended to `$written` in
  `Repair-PackageManifestConsistency.ps1` without producing any repair record;
- binding-redirect reconciliation, whose `Kind = 'BindingRedirect'` record is produced by
  `ProjectConsistency.psm1` and then discarded at the call site, which keeps only `.Text`.

A run whose only writes fall in those classes exits green, skips the push, discards the repair, and
reports "No repairs were applied." on the pull request.

Fix: emit a write-set count as a step output and gate on it. `$result.WrittenPath` already carries
exactly the right set. Add `"written-count=$(@($result.WrittenPath).Count)"` to `GITHUB_OUTPUT` and
condition the commit step on it. Add a unit assertion that a run whose only change is a normalisation
reports a non-zero write count.

## R4 — Blocking. Remove absolute host paths from the committed artifacts, then squash-merge

**Invariant to restore:** no committed artifact discloses a filesystem path containing the account
name.

74 occurrences of `C:\Users\DanMoisan` across 27 markdown files under the feature folder. Sanitisation
was applied inconsistently: `evidence/qa-gates/p4-t5-actionlint` already uses
`<execution-worktree-root>`, while `evidence/baseline/p0-t5-sdk-bootstrap`,
`evidence/baseline/p0-t8-dotnet-coverage` and 25 others do not.

Replace every occurrence with `<repo-root>` or `<execution-worktree-root>`, then **squash-merge the
pull request.** A follow-up sanitisation commit leaves the original blobs reachable in history, which
is why this class recurred on issues #645, #680, #730 and #752 despite being fixed each time.

## R5 — Major. Make `Invoke-ProjectConsistencyRepair` impossible to call incorrectly

**Invariant to restore:** no exported production function has a failure mode its callers cannot avoid.

`ConsistencyVerifier.psm1` line 440 calls `Invoke-VersionReconciliation` without `-AssemblyVersion`.
That parameter defaults to the empty string, `$resolvedAssemblyVersion` falls back to
`$ManifestVersion`, and `Get-RewrittenReferenceVersionLine` then rewrites every matching
`<Reference Include="...Version=X..." />` assembly version to the package version. The function
exposes no parameter through which a consumer could supply a correct value.

Note for the planner: the caller-supplied hand-off attributed this defect to
`scripts/dependencies/ProjectConsistency.psm1`. It is in `scripts/dependencies/ConsistencyVerifier.psm1`.

Two acceptable discharges:

1. Add an `-AssemblyVersion` or `-AssetProvider` parameter and thread a resolved value through,
   mirroring `Resolve-ReferenceAssemblyVersion` in the composition root. Add a test asserting that a
   `<Reference>` whose assembly version the package carries is preserved.
2. Remove the function from `Export-ModuleMember`, retarget its tests at the composition root, and
   state in the module header that the repair entry point is the composition root.

Do not discharge by documenting the hazard. The composition root already avoids it at lines 265-268;
the module-level API should make that the only reachable behaviour.

## R6 — Major. Guard and de-duplicate the pull-request disclosure

**Invariant to restore:** a run that applied no repair posts nothing, and a repeated run does not
duplicate the block.

The "Disclose the repairs on the pull request" step has no `if:` condition, and `$result.Body` always
emits a `## Repairs applied` heading. Because the repair push itself triggers a new `CI` run whose
completion re-fires `dependabot-repair`, one repair yields at least two appended blocks, and every
subsequent completed CI run on the branch adds another.

Fix: guard the step on `steps.repair.outputs.repair-count != '0'` (or the write count from R3), and
make the body edit replace a block delimited by HTML comment markers rather than append.

## R7 — Major. Resolve the binding-redirect reachability gap

**Invariant to restore:** every repair class the specification advertises is reachable from the
production entry point, or is documented as out of the production path.

The workflow invokes `Repair-PackageManifestConsistency.ps1` with no arguments, so `-CandidateUpgrade`
takes its `@{}` default, `$upgrade.Applied` is always empty, and the `app.config` reconciliation block
never executes. The workflow's `$_ -ne 'BindingRedirect'` filter is consequently dead, because the
call site discards the `Kind = 'BindingRedirect'` record and keeps only `.Text`.

Decide and record one of:

- In scope: derive the applied upgrade set from the Dependabot commit and pass it, and add the
  binding-redirect repair records to the report so they appear in the disclosure and in the
  beyond-known-weak count.
- Out of scope: remove the dead filter branch, and amend the AC14 note in `spec.md` to say the class
  is exercised by unit assertion only and is not reachable from the `workflow_run` trigger.

## R8 — Major. Correct the repair commit identity, or restate AC18

**Invariant to restore:** AC18 is satisfiable by the shipped implementation.

`git config user.email 'dependabot-repair[bot]@users.noreply.github.com'` is a hand-written literal.
GitHub resolves `commits/<sha>.author.login` by matching the author email to an account; a GitHub App
bot account uses `<app-id>+<app-slug>[bot]@users.noreply.github.com`. The written address matches no
account, so the login will resolve to null and AC18's stated acceptance will not hold.

Either construct the address from the `actions/create-github-app-token` step outputs, or restate AC18
to assert on the pushing actor rather than the commit author. AC19 is unaffected: required checks
re-run because the push carries an App installation token.

## R9 — Minor. Housekeeping

- Strip `#MEZIANTOU-898` and `#SHA-256` from the author-asserted autoclose list before any
  pull-request body is authored. They are false positives from the issue-number detector.
- Add a call-site comment at `Repair-PackageManifestConsistency.ps1` line 320 stating that the
  unfiltered `Get-AnalyzerAssemblyPath` result is a verification membership set and must never be
  written.
- Either make `$script:DefaultFileLister` walk recursively with the existing prune list, or emit a
  verbose enumerated-directory count, so a nested project cannot be silently skipped.
- Treat `Repair-PackageManifestConsistency.ps1` (498 lines) and `ConsistencyVerifier.psm1` (493) as at
  capacity; the R3, R6 and R7 edits should extract rather than append.

## Carried to #914, not remediated here

- AC18, AC19 and AC20 remain unverified pending a GitHub App credential and an open Dependabot pull
  request. The deferral is evidenced and legitimate. Do not represent issue #911 as closed until they
  are discharged. R6, R7 and R8 should be verified by the same live run.
- The 80-versus-85 coverage floor conflict between `CLAUDE.md` and `.claude/rules/general-unit-test.md`
  is tracked as open issue #668 and is not resolved by this change. Every figure here except
  `Sync-PackageReferences.ps1` clears both readings.
- The two `scripts/vscode` files unformatted under PSScriptAnalyzer defaults are a recorded scope
  decision, unformatted on `main`, and clean under the ruleset CI runs.
- Ten stale binding redirects across six `app.config` files were verified pre-existing at the merge
  base.

## Go / No-Go

**No-go for opening the pull request until R1 through R4 are discharged.** R5 through R8 should be
resolved in the same cycle because they live in the same file and none is large. The retroactive
repair itself is sound and does not need rework.
