# Code Review — Issue #911 (dependabot fan-out and CI-failing NuGet upgrades)

- Date: 2026-09-20
- Reviewer: feature-review
- Branch: `bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`
- Head: `794d34f02647214030fc3c2b076112dee731ff62`
- Base: `origin/main`, merge base `734112ed25bba293cb074e71fee2286bc3b72fae`
- Diff reviewed: 214 files, +23,626 / -6,272, 21 commits

## Executive Summary

The engineering quality of the five new modules is high. They are pure over text, reach the disk only
through injected delegates, carry rationale comments for every non-obvious decision, and sit well
under the file-size cap. The preserve rule in `AnalyzerItemRepair.psm1` is correct and provably
cannot select a Roslyn folder; the claim was verified structurally and then confirmed empirically
against the delivered diff and the restored package directories.

The defects are concentrated in the one component nothing exercises: `dependabot-repair.yml` and the
part of the composition root it consumes. Three of the four blocking findings and three of the five
major findings would have been caught by a single live run. That is the practical cost of deferring
AC18, AC19 and AC20.

One finding sits outside the workflow: `scripts/vscode/Sync-PackageReferences.ps1` was rewritten and
its negative and error paths are untested, which shows up both as a 74.80 percent coverage figure and
as a scenario-completeness failure.

Counts: 4 Blocking, 5 Major, 4 Minor.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocking | `.github/workflows/dependabot-repair.yml` | 6 workflow files changed | Six paths under `.github/workflows/**` changed, including a new job holding `contents: write` and `pull-requests: write`, and no green workflow run exists against head `794d34f02`. | Dispatch the `CI` workflow against the branch head and record the run id and conclusion, or open the pull request and record the PR-context run, before merge. | The `modified-workflow-needs-green-run` rule in `feature-review-workflow` fires on this path set and demands green-run evidence at head as a line of defence independent of the orchestrator S9 gate. | `git diff --name-only 734112ed2..794d34f02` lists `_build-analyzers.yml`, `_build-nullable.yml`, `_mstest-coverage.yml`, `_pester.yml`, `dependabot-repair.yml`, `README.md`; `pr_context.summary.txt` CI status section reads `(not available)`. |
| Blocking | `scripts/vscode/Sync-PackageReferences.ps1` | whole file, 423 lines, +407/-143 | Line coverage is 74.80 percent (95 of 127 instrumented lines) on a file this change effectively rewrote. Nine uncovered lines are pure logic, not the I/O seam: lines 151, 180, 248, 290, 293, 330, 336, 337, 345. Line 248 is the issue #902 rejection handler. | Add tests for the six uncovered behaviours: unresolvable identifier, absent library directory, unconsumable asset set reaching line 248, both `Set-ReferenceAssemblyVersion` early returns, empty project list, merge-conflict skip, and empty repair set. | Both coverage floors in this repository (80 in CLAUDE.md, 85 in the rules file) are breached, and `.claude/rules/general-unit-test.md` requires negative, edge and error-handling scenarios independently of the percentage. | Parsed directly from `coverage/p9-t3-pester-coverage.iter1.xml`: `sourcefile name="vscode/Sync-PackageReferences.ps1"` LINE counter covered 95, missed 32. `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` contains six `It` blocks, all AC7-scoped positive paths. |
| Blocking | `.github/workflows/dependabot-repair.yml` | commit step `if:` condition | The push gate `if: steps.repair.outputs.repair-count != '0'` measures only project-file repairs. `RepairCount` is `@($Repair).Count` over the per-project verification reports; manifest normalisation writes and binding-redirect writes never increment it. A run whose only writes are those classes reports success, skips the commit, discards the repair, and the pull request keeps failing CI. | Gate the push on the working-tree state instead, for example on `git status --porcelain` being non-empty after the repair, or expose a `WrittenPath` count as a step output and gate on that. | `Repair-PackageManifestConsistency.ps1` already tracks every write in `$written` and publishes it as `WrittenPath`; the workflow reads the wrong aggregate. The failure is silent: the job is green and the report says no repairs were applied. | `ConsistencyVerifier.psm1:345` defines `RepairCount = @($Repair).Count`; `Repair-PackageManifestConsistency.ps1:464-466` adds normalisation paths to `$written` without touching any repair record; the workflow condition reads `repair-count`. |
| Blocking | 27 evidence and planning documents | `docs/features/active/2026-09-19-.../` | 74 occurrences of the absolute host path `<user-home>` are committed across 27 markdown files, leaking the account name. Sanitisation was applied inconsistently: `p4-t5-actionlint` uses `<execution-worktree-root>` while `p0-t5-sdk-bootstrap` and `p0-t8-dotnet-coverage` do not. | Replace every occurrence with `<repo-root>` or `<execution-worktree-root>` in the working tree, then squash-merge. Sanitising after commit leaves the original blobs reachable in history. | This leak class has recurred on issues #645, #680, #730 and #752. The remedy is a squash merge, not a follow-up commit. | `git diff 734112ed2..794d34f02` added lines matching the literal path; the 27 files and their per-file counts are enumerable with a case-sensitive fixed-string grep over the feature folder. |
| Major | `scripts/dependencies/ConsistencyVerifier.psm1` | `Invoke-ProjectConsistencyRepair`, lines 383-479 | The function calls `Invoke-VersionReconciliation` at line 440 without `-AssemblyVersion`. That parameter defaults to the empty string, so `$resolvedAssemblyVersion` falls back to `$ManifestVersion` and `Get-RewrittenReferenceVersionLine` rewrites every matching `<Reference Include="...Version=X..." />` assembly version to the package version. The function exposes no `-AssemblyVersion` parameter, so no consumer can avoid this. | Either remove it from `Export-ModuleMember` and mark it internal to the test surface, or add an `-AssemblyVersion` or `-AssetProvider` parameter and thread a resolved value through, mirroring `Resolve-ReferenceAssemblyVersion` in the composition root. | An exported function whose own documentation calls it "the entry point" and which cannot be called correctly should not ship. It is unreachable today only because the composition root wires the module functions directly; unreachability is a property of the wiring, not an asserted invariant, and nothing tests it. | `ConsistencyVerifier.psm1:440` omits the parameter; `ProjectConsistency.psm1:182-187` shows the fallback; `ProjectConsistency.psm1:202-203` shows the Reference branch. The composition root does it correctly at `Repair-PackageManifestConsistency.ps1:265-268`. The caller-supplied note attributed this to `ProjectConsistency.psm1`; the defect is in `ConsistencyVerifier.psm1`. |
| Major | `.github/workflows/dependabot-repair.yml` | disclosure step, no `if:` guard | The "Disclose the repairs on the pull request" step carries no condition, and `$result.Body` always contains a `## Repairs applied` heading (with the literal text "No repairs were applied." when empty). It appends to the existing body on every completed CI run on a `dependabot/` branch. The repair push itself triggers a new CI run whose completion re-fires this workflow, so one repair produces at least two appended blocks, and the body grows without bound over the life of the pull request. | Guard the step on `steps.repair.outputs.repair-count != '0'`, and make the edit replace a delimited block rather than append, for example by stripping any prior block between HTML comment markers before writing. | AC20 asserts only that the body contains a "Repairs applied" block, so the criterion would pass while the behaviour is wrong. | `$script:ReportBody` at `Repair-PackageManifestConsistency.ps1:364-388` unconditionally emits the heading; the workflow step has no `if:`; `$updated = $existing + "`n`n" + $report`. |
| Major | `.github/workflows/dependabot-repair.yml` | repair step invocation | The workflow invokes `Repair-PackageManifestConsistency.ps1` with no arguments, so `-CandidateUpgrade` takes its `@{}` default, `$upgrade.Applied` is always empty, and the `app.config` binding-redirect block at lines 450-461 never executes. The workflow's `$_ -ne 'BindingRedirect'` filter is therefore dead: no repair record with that Kind can reach it, because line 456 discards the record and keeps only `.Text`. | Decide explicitly whether the redirect class is in scope for the Dependabot flow. If it is, derive the applied upgrade set from the Dependabot commit and pass it; if it is not, remove the dead filter branch and say so in the workflow comment. Separately, add the `Kind = 'BindingRedirect'` records that `Invoke-BindingRedirectReconciliation` already returns to the repair report so they appear in the disclosure. | AC14 is satisfied by unit assertions against a function the production entry point can never reach in its configured trigger path. That is a real gap between what the specification advertises and what ships. | `Repair-PackageManifestConsistency.ps1:57` sets the default; line 450 gates on `@($upgrade.Applied).Count -eq 0`; line 456 takes only `.Text`; `ProjectConsistency.psm1:312` shows the discarded record. |
| Major | `.github/workflows/dependabot-repair.yml` | commit step, git identity | `git config user.email 'dependabot-repair[bot]@users.noreply.github.com'` is a hand-written address. GitHub resolves `commits/<sha>.author.login` by matching the author email to an account; a GitHub App bot account uses `<app-id>+<app-slug>[bot]@users.noreply.github.com`. The written address matches no account, so `.author.login` will be null and AC18's stated acceptance, that the login ends with `[bot]` and is not `github-actions[bot]`, will not hold. | Read the App slug and id from the token step outputs and construct the address, or assert on the push actor rather than the commit author. | AC19 is unaffected: the required checks re-run because the push carries an App installation token, not because of the commit author. But AC18 as written appears unsatisfiable by this implementation, and its deferral is what prevented that from surfacing. | Workflow commit step sets `user.name` and `user.email` to literals; `evidence/qa-gates/p8-t2-ac18-repair-identity.2026-09-19T09-44.md` records the intended assertion on `.author.login`. |
| Major | `spec.md`, AC18 / AC19 / AC20 | acceptance criteria | Three criteria are unverified. The deferral is correctly evidenced (an empty repository secrets list and zero open Dependabot pull requests), but the consequence is that the entire forward-prevention half of the change ships with no runtime execution at all. | Land the change on the strength of the retroactive repair, but do not represent issue #911 as closed until #914 discharges AC18 through AC20 against a live fixture. Treat the three workflow findings above as things a single live run would have caught. | The deferral is legitimate as a fact but not cost-free: three of the defects in this table live in exactly the code path those criteria would exercise. | `evidence/other/p8-t1-credential-availability.2026-09-19T09-44.md` records `QUERY1-OUTPUT` as `[]` and `QUERY2-LENGTH: 0`. |
| Minor | `scripts/dependencies/AnalyzerItemRepair.psm1` | `Get-AnalyzerAssemblyPath`, lines 210-270 | Called without `-PreservedSegment`, the function derives an item path for every consumable analyzer assembly in every Roslyn folder the package ships. The composition root calls it in that mode at line 320, which is safe because the result is used only as a membership set for a `-contains` test and nothing writes from it. The contract nevertheless permits producing a full derived path set, which is the shape a future caller could mistake for a selection. | Rename the unfiltered mode or add a comment at the call site stating that the result is a verification set and must never be written. | The preserve rule is the load-bearing invariant of this change; the one function capable of producing a non-preserved path should be hard to misuse. | `Repair-PackageManifestConsistency.ps1:320-323` calls without `-PreservedSegment`; line 324 uses the result only in `-contains`. |
| Minor | `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | `$script:DefaultFileLister`, lines 81-94 | Manifest discovery enumerates only the repository root and its immediate subdirectories. A project nested two levels deep would be silently skipped, with no report and no failure. | Either walk recursively with the existing prune list, or emit a verbose record of the enumerated directory count so a shortfall is visible. | All 18 current manifests sit at depth one, so this is latent rather than active, but silent omission is the wrong failure mode for a consistency tool. | `Get-ChildItem -LiteralPath $script:Root -Directory` followed by a single non-recursive file enumeration per directory. |
| Minor | `artifacts/pr_context.summary.txt` | close-candidates section | The author-asserted autoclose list contains `#MEZIANTOU-898` and `#SHA-256`, which are false positives from the issue-number detector reading `Meziantou.Analyzer` and `SHA-256` out of prose. | Strip both before any pull-request body is authored. | A pull request body carrying `#SHA-256` would reference an unrelated issue number. | `pr_context.summary.txt` close-candidates section. |
| Minor | `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, `ConsistencyVerifier.psm1` | 498 and 493 lines | Both files sit within two and seven lines of the 500-line cap. | No action now. Treat either file as at capacity: the next addition should extract rather than append. | The cap is a hard policy limit, and both files are the ones most likely to grow when the workflow findings above are remediated. | `wc -l` on the delivered tree. |

## Design and Structure Observations

**The preserve rule is correct and was verified two ways.** Structurally, the only function that
writes analyzer item text is `Get-RewrittenPackageFolderLine`, whose regular expression captures the
separators either side of the `<Id>.<Version>` segment and reuses them and the existing identifier
casing verbatim, so every character outside that span survives byte-identically.
`Invoke-AnalyzerItemRepair` derives the preserved segment from the item's own remainder and uses the
offered-segment list only through `.Contains()`; it never indexes, orders or maximises over it.
Empirically, the delivered diff changes exactly one substring per item: at the merge base the tree
carried two distinct Meziantou item paths (`3.0.203` and `3.0.235`, both at `roslyn5.0/cs/`); at head
it carries one. The restored packages confirm the measured justification:
`Meziantou.Analyzer.3.0.235` ships `roslyn4.14`, `roslyn4.8`, `roslyn5.0`, `roslyn5.6` and
`roslyn5.9` while every item names `roslyn5.0`, and `Roslynator.Analyzers.5.0.0` ships `roslyn3.8`,
`roslyn4.7` and `roslyn5.0` while every item names `roslyn4.7`. Meziantou and Roslynator items number
exactly 80, matching the specification's claim that a selection rule would rewrite 80 items rather
than the 15 that were stale.

**Separation of concerns is well executed.** The five modules hold no filesystem call. Every disk
interaction is a script-scope delegate in the composition root, which is what makes the whole
pipeline drivable over an in-memory fixture with no temporary file. That is the right shape for this
repository's prohibition on temporary files in tests, and it is why five of the six new files reach
100 percent or near it.

**The failure mode chosen for an unresolvable analyzer segment is right.** Emitting a
missing-segment record and leaving the item unmodified, rather than guessing a path, is the correct
choice, and the non-fatal class is aggregated in exactly one module.

**The `.csharpierignore` scope change was proven live rather than assumed.** The AC2 control
perturbed a C# file alongside the two config files and confirmed the formatter reported the C# file
and not the configs. Without that control a silent no-op run would have read as a pass.

## Non-Findings Checked and Cleared

- File-size limit: all 16 changed script files measure under 500 lines.
- Final toolchain loop: the phase-9 restart from the formatter happened after the analyzer failed at
  18 findings and before any later stage ran, so the final pass is a single clean pass. This is not a
  loop violation.
- The two `scripts/vscode` files left unformatted under PSScriptAnalyzer defaults are unformatted on
  `main`, are clean under the PoshQC ruleset that CI runs, and were not touched here.
- Ten stale binding redirects across six `app.config` files were verified pre-existing at the merge
  base.
- Evidence locations: zero files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`
  or `artifacts/coverage/`.
- The `app.config` and `packages.config` diffs are layout-only; no redirect version or package
  version changed in them.
- `workflow_run` with `workflows: [CI]` resolves: `ci.yml` declares `name: CI` and triggers on
  `pull_request` against `main`, which is what Dependabot pull requests target.
- The fork-based `workflow_run` escalation vector is blocked in practice: `actions/checkout` with
  `ref: <head_branch>` resolves against the base repository, so a fork branch named `dependabot/x`
  would not check out.
