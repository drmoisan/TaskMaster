# Code Review — Issue #911 (dependabot fan-out and CI-failing NuGet upgrades)

- Date: 2026-09-20
- Reviewer: feature-review
- Cycle: re-audit after remediation cycle 1
- Branch: `bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`
- Head: `db53ca1407592108af5d6792bb05803574b2b768`
- Base: `origin/main`, merge base `b5621910c5b97d2471e368e87e80dc294207111b`
- Diff: 288 files, +32,789 / -6,257, 32 commits
- Prior cycle: `code-review.2026-09-20T01-37.md` — 4 Blocking, 5 Major, 4 Minor

## Executive Summary

Eleven findings were claimed discharged. **All eleven are discharged.** Each was re-tested against
the delivered code rather than against the executor's statement, and in three cases the discharge is
stronger than the plan required.

The highest-value confirmations:

- **R2 is discharged on substance, not on the number.** Eight new `It` blocks cover all nine
  previously uncovered pure-logic lines, every one of them asserting an outcome rather than merely
  executing the line. Two carry an explicit non-vacuity guard on the warning set before asserting
  its text. The #902 rejection handler at line 248 is genuinely reached: the seam returns absent for
  every relative probe and present for every absolute one, so the candidate hint path fails, the
  library directory and the required file are both found, and the empty selection is the
  compatibility gate's decision rather than a missing file.
- **R3's write-set gate is sound end to end.** `$written` accumulates all four write classes —
  candidate-upgrade manifest writes, project-file writes, binding-redirect writes, and normalisation
  changed paths. Every add is guarded by a content-inequality test and `ShouldProcess`, so a
  non-zero `written-count` implies a real on-disk change and the `git add --update` that follows
  cannot find an empty index. All written kinds match the step's pathspecs; there is no root-level
  `packages.config` that `*/packages.config` would miss.
- **R8's identity derivation is correct, not merely different.** The address built is
  `<bot-user-id>+<slug>[bot]@users.noreply.github.com`, which is the form GitHub resolves to an
  account, and the numeric part is read from the users API rather than assumed to be the app id.
  Both reads are guarded so a wrong assumption produces a named step failure.
- **R7's out-of-scope label is honest.** Binding-redirect records cannot reach the removed filter
  for two independent reasons, and both were traced in code: the workflow supplies no
  `-CandidateUpgrade`, so `$upgrade.Applied` is empty and the `app.config` block short-circuits at
  the `continue`; and the call site keeps only `.Text`, discarding the `Kind` record. The spec AC14
  note was amended as the discharge required.
- **R4's working-tree-only label is honest.** Zero occurrences of the account name and zero of a
  drive-qualified user path across all 288 changed files. The pre-sanitisation blobs remain
  reachable, so the squash-merge obligation is real and is documented.

Six findings are recorded, none of them Blocking. One is Major and concerns a document that
decision D2 made inaccurate. Four are Minor. One is a gate that only the pull request can close.

The prior cycle's highest-yield heuristic held again: **five of the six findings below are in, or
about, the component nothing exercises.**

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocking (pull-request gate, not remediable in a cycle) | `.github/workflows/dependabot-repair.yml` and five siblings | whole-file | Six workflow paths changed and no CI run of any event type exists at head `db53ca140`. The recorded green run 35513025198 is at `de9a00106`, two commits back, and its event is `workflow_dispatch`. | Record the pull-request-context `CI` run at the merge head. Do not attempt to close this with another dispatched run: the merge head is not knowable before the pull request exists. | The repository rule requires a green run of a modified gate at the commit being merged. A dispatched run at an ancestor is evidence the gates pass today, which is useful, but it is not that run. | `evidence/qa-gates/p6-t2-ci-run.2026-09-20T01-37.md`; `git diff de9a00106..HEAD` is documentation only, verified by this review |
| Major | `.github/workflows/README.md` | "Dependabot repair workflow", lines 84-90 | The README still advertises that the workflow repairs "an `app.config` binding redirect". Decision D2 made that class unreachable from the `workflow_run` trigger, and the workflow now carries a comment saying so. The operator-facing document was not updated when the spec AC14 note was. | Amend the sentence to state that binding-redirect reconciliation is available in the repair script but is not exercised by the `workflow_run` invocation, and name the condition (`-CandidateUpgrade` supplied) under which it would be. | An operator reading the README will expect a class of repair the deployed workflow cannot perform, and will not investigate a redirect that stayed stale. The workflow comment records the decision where only a maintainer reading YAML will find it. | `.github/workflows/README.md` line 87 against `.github/workflows/dependabot-repair.yml` lines 86-94 and `spec.md` AC14 note |
| Minor | `scripts/dependencies/ProjectConsistency.psm1` | `Resolve-ReferenceAssemblyVersion` line 137 against `Get-RewrittenReferenceVersionLine` line 116 | The two guards that must agree do not. The resolver matches `Include="<Id>,\s*Version=` with a case-sensitive .NET regex and no allowance for leading whitespace; the rewriter compares the captured name with PowerShell `-ne`, which is case-insensitive, after `.Trim()`. Where a manifest `id` and an `Include` name differ only in case, the resolver returns empty, `Invoke-VersionReconciliation` falls back to `$ManifestVersion`, and the R5 behaviour reappears. | Build the resolver's pattern with `RegexOptions.IgnoreCase` and allow optional leading whitespace after the opening quote, so the two predicates accept the same set. | R5's invariant is that no exported production function has a failure mode its callers cannot avoid. This is a narrow surviving instance of exactly that failure mode, and no parameter lets a consumer avoid it. | Measured: 912 `<Reference Include="...">` elements in the repository, **zero** case divergences against their sibling manifest identifiers, so the hazard is latent rather than live |
| Minor | `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | `$script:DefaultFileLister`, line 99 | The R9c remedy is inert under the production invocation. `Write-Verbose` emits nothing unless `$VerbosePreference` is raised, and the workflow invokes the script at line 79 with no `-Verbose` and sets no preference. The comment states the record "makes the shortfall observable in the run log"; it does not. | Either add `-Verbose` to the workflow invocation, or emit the count with `Write-Information ... -InformationAction Continue`, which is the pattern `Sync-PackageReferences.ps1` already uses for its own summary line. | This is the same defect shape that produced three of the prior cycle's four blockers: a mechanism verified from the test and never traced from the deployed invocation. | Workflow line 79 `$result = & "$env:GITHUB_WORKSPACE\scripts\dependencies\Repair-PackageManifestConsistency.ps1"`; all 18 `.csproj` sit at depth 1 today, so no project is actually skipped |
| Minor | `scripts/vscode/Sync-PackageReferences.ps1` | `Invoke-PackageReferenceSync` line 410 | The non-zero-fix summary branch is untested, and it is the positive outcome of the whole script. It is drivable through the existing injected seam, unlike lines 387, 390 and 422. | Add one test that supplies a seam producing at least one hint-path repair and asserts the returned `FixedCount` and the written text. | Covering it takes the file to 105 of 127 lines, 82.68 percent, which is the true ceiling under the seam and a more defensible figure to record than 81.89. | `coverage/p5-t3-pester-coverage.iter1.xml`: uncovered set is 60-91, 387, 390, 410, 422 |
| Minor | `docs/.../evidence/qa-gates/p3-t10-workflow-footprint.2026-09-20T01-37.md` | per-edit deletion table | The table's deletion column sums to 6 while the measured total is 5, and the total row silently reconciles to the measured number. The `[P3-T4]` row claims two deletions, "the old `$updated` composition and its `WriteAllText`", but `$updated = Join-Path $env:RUNNER_TEMP 'pr-body.md'` is unchanged context in the diff; only the `WriteAllText` line was removed. | Correct the row to 1 and the prose from "replaces two lines rather than three" to "replaces one line rather than two". | The artifact's conclusion is right and its decision to report the clause unmet rather than adjust it was right. The arithmetic inside it is not, and an evidence artifact whose columns do not sum is harder to trust on the claims that cannot be re-measured. | `git diff --numstat 794d34f02..HEAD -- .github/workflows/dependabot-repair.yml` reports `55 5` |

## Design and Structure Observations

**The R5 discharge took a third route and it is the better one.** The remediation inputs offered two
options: thread an assembly-version parameter through, or unexport the function. The executor did
neither. It extracted `Resolve-ReferenceAssemblyVersion` into `ProjectConsistency.psm1`, exported
it, and called it from `Invoke-ProjectConsistencyRepair` without an identity provider, so the
resolver returns the version the project already declares and the Reference line is written back
unchanged. The destructive default is gone rather than made avoidable, which is what the invariant
asked for. The extraction also discharged half of R9d: the composition root fell from 498 lines to
470.

**The disclosure de-duplication derives both markers from one pattern.** `$marker =
$blockPattern.Substring(4) -split '\.\*\?'` splits the strip pattern on its own `.*?` and yields the
two literals the emitted block uses. The block this step writes and the block it strips therefore
cannot drift apart. That is a better answer than writing the markers twice, and the comment explains
why `Substring(4)` is there. Verified by evaluation: the split produces exactly the two comment
markers, and the guard on `written-count || skip-count` means the replace path is rarely reached in
the first place.

**The `skip-count` clause in the disclosure guard is load-bearing and correctly reasoned.** A run
that skipped an incompatible package without writing anything must still disclose, because AC20
requires the skipped block whenever a skip was recorded. Guarding on `written-count` alone would
have silently dropped that case. The comment says so.

**Compaction versus extraction.** `ConsistencyVerifier.psm1` is at 499 of 500 and was brought there
by compressing a 510-line intermediate. That intermediate was never committed, so the claim that
only whitespace and re-wrapped prose were removed cannot be tested by comparison. It is corroborated
instead by a whole-cycle census: no test file lost an `It`, a `Should`, a `-Because` or an AAA
marker, and `DependabotConfig.Tests.ps1` went from 11/27/11-7-10 to 17/55/17-11-16. The standing
instruction in the size audit — the next addition to `ConsistencyVerifier.psm1` must extract rather
than append — is the right control and should be honoured.

**Three gates were corrected after passing or failing for the wrong reason, and all three
corrections are real.** The `[P3-T1]` reds first arrived as `Cannot bind argument to parameter
'Line' because it is an empty string`, a binding error rather than an assertion failure; the fix was
`[AllowEmptyString()]` on the helper, and the recorded reds are now substantive messages naming the
absent `written-count` output, the absent disclosure guard, the two-clause filter and the absent
`app-slug` reference. The msbuild non-vacuity needle is built from `[char]92` because a PowerShell
double-quoted `"\\"` carries both backslashes and matched zero against a log holding the token 36
times; this review counted 36 compiler invocations in the analyzer log independently. The Phase 1
restart after a nine-finding PSScriptAnalyzer batch is recorded with the restart preceding every
later stage, so the final pass is a single clean pass. Each correction is documented in the artifact
that would otherwise have carried the false result, which is the behaviour to keep.

**The plan clause that was reported unmet rather than accommodated.** P3-T10 required at least six
deletions and five were measured. Reporting it was right: the alternative is an edit made to satisfy
a counting expectation. The clause's purpose — proving the four remediation edits removed the
defective constructs rather than adding around them — is satisfied independently, and this review
measured it directly: `repair-count != '0'` appears **0** times, the hand-written bot email appears
**0** times, `-ne 'BindingRedirect'` appears **0** times, and the appending `$existing + ...`
composition appears **0** times, while `written-count != '0'` appears twice as expected (push gate
and disclosure guard) and `app-slug` and the block marker appear as expected.

## Non-Findings Checked and Cleared

- **The retroactive repair still holds at the new head.** 1,498 restore-path references across 18
  project and manifest pairs, zero disagreeing with a sibling manifest, 11 orphans. Re-derived from
  the tree, not read from an artifact. All 80 `Meziantou.Analyzer` analyzer items name `3.0.235` and
  all 16 manifests declare `3.0.235`.
- **The write-set pathspec covers every path the script can write.** Checked because a
  `written-count` that the subsequent `git add --update` cannot stage would fail the commit. There
  is no root-level `packages.config`, and every `.csproj`, `packages.config` and `app.config` in the
  repository sits at depth 1.
- **`Invoke-ProjectConsistencyRepair` is exported but has no production caller.** Only the three
  test files call it; the composition root uses its own `Invoke-ProjectFileRepair`, which does pass
  a real identity provider. This bounds the impact of the case-divergence finding above.
- **Scope control is exact.** All 57 changed non-documentation paths appear in the `spec.md` write
  set; none is outside it.
- **`_pester.yml` genuinely widened.** Both `Run.Path` and `CodeCoverage.Path` became two-member
  arrays, which is what makes the new suite execute in CI rather than report green while measuring
  nothing.
- **No temporary file is created by any test.** Every fixture is an in-memory string or hashtable
  behind an injected delegate, including the new R2 suite.
- **Terminal tree is clean.** `git status --porcelain --untracked-files=all` returns empty.
- **No suppression attribute or pragma was added on any line of the diff.**
- **Two `scripts/vscode` files remain unformatted under PSScriptAnalyzer defaults.** Unchanged from
  the prior cycle: they are unformatted on `main`, clean under the ruleset CI runs, and untouched by
  this change. Not a finding against this branch.
- **Ten stale binding redirects across six `app.config` files.** Verified pre-existing at the merge
  base and outside this change's remit, as recorded in the prior cycle.
