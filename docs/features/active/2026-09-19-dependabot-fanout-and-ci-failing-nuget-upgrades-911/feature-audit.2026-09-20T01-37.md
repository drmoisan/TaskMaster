# Feature Audit — Issue #911 (dependabot fan-out and CI-failing NuGet upgrades)

- Date: 2026-09-20
- Reviewer: feature-review
- Work mode: `full-bug`
- Acceptance-criteria source: `spec.md` only (no `user-story.md` exists, and none may exist for this mode)

## Scope and Baseline

- Base branch: `origin/main`
- Merge base: `734112ed25bba293cb074e71fee2286bc3b72fae`
- Feature branch: `bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`
- Head: `794d34f02647214030fc3c2b076112dee731ff62`
- Branch diff: 214 files, +23,626 / -6,272, across 21 commits
- PR context: `artifacts/pr_context.summary.txt` generated 2026-09-20 at head `794d34f0`, current and not stale

The audit is feature-versus-base across the entire branch diff. No scope narrowing was accepted.

### Independent baseline comparison

The central invariant the bug report describes is that a Dependabot upgrade rewrites the package
import guards of packages outside the upgraded group to a version no `packages.config` declares. This
review re-derived every package restore-path reference in all 18 project and manifest pairs at both
ends of the range, rather than relying on the executor reports.

| Measurement | Merge base `734112ed2` | Head `794d34f02` |
|---|---|---|
| Restore-path references in `.csproj` | 1,498 | 1,498 |
| By element: `Import` / `Error` / `HintPath` / `Analyzer` | 234 / 230 / 872 / 162 | 234 / 230 / 872 / 162 |
| References naming a version the sibling manifest does not declare | 15 | 0 |
| References to a package absent from the sibling manifest | 13 | 11 |
| `packages.config` files | 18 | 18 |
| Projects carrying analyzer items | 17 | 17 |
| `Meziantou.Analyzer` plus `Roslynator.Analyzers` items | 80 | 80 |

All 15 disagreements at the merge base were `Meziantou.Analyzer.3.0.203` against a manifest declaring
`3.0.235`. The two eliminated orphan references were `Deedle.3.0.0` and `FSharp.Core.11.0.100`, which
is the AC8 repair. The 11 remaining orphans (`altcover`, `Microsoft.Web.WebView2`,
`ObjectListView.Official`) are pre-existing and are reported non-fatally by the verifier.

## Acceptance Criteria Inventory

- Source: `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`
- Total AC items: 26
- Checked off in source at review time: 23
- Unchecked in source at review time: 3 (AC18, AC19, AC20)
- Format: markdown checkboxes under `## Acceptance Criteria`

## Acceptance Criteria Evaluation

| AC | Subject | Verdict | Basis |
|---|---|---|---|
| AC1 | Dependabot configuration consolidated | PASS | Verified by direct diff of `.github/dependabot.yml` against the merge base: exactly one group `all-nuget-updates` with `applies-to: version-updates` and pattern `*`; `open-pull-requests-limit` 1; all eight baseline `ignore` entries preserved verbatim; Deedle added with neither a `versions` nor an `update-types` qualifier; `group-by` removed from every group. |
| AC2 | Config manifests outside the formatting gate, proven positively | PASS | `.csharpierignore` gains `**/packages.config` and `**/app.config`. The AC2 control perturbed a named C# file alongside the two config files and the captured formatter output reported the C# file only, which establishes the run was live. |
| AC3 | 18 manifests normalised, normalisation idempotent | PASS | 18 `packages.config` files counted independently at head; the idempotence capture records an empty diff on the second pass. |
| AC4 | NuGet CLI version pinned everywhere it is selected | PASS | All four `nuget/setup-nuget@v2` steps declare `nuget-version: '7.9.0'`, verified by inspection of `_build-analyzers.yml`, `_build-nullable.yml`, `_mstest-coverage.yml` and `dependabot-repair.yml`; actionlint exits 0. |
| AC5 | Every analyzer item agrees with its manifest | PASS | Independently re-derived: 162 analyzer items across 17 projects, zero version disagreements at head against 15 at the merge base. The examined-count non-vacuity guard is satisfied by the same measurement. |
| AC6 | Cold-cache failure observed before and absent after | PASS | Red log under `evidence/baseline/p0-t11`, green log under `evidence/qa-gates/p1-t14`, plus a second red run under `evidence/regression-testing/898-cold-restore-red-run`. Executor-attested; not reproduced by this review, which cannot delete and re-restore the shared package cache. |
| AC7 | Incompatible framework excluded rather than ranked | PASS | `tests/scripts/dependencies/PackageCompatibility.Tests.ps1` asserts the four positive cases and the three no-selection cases. `scripts/vscode/Sync-PackageReferences.ps1` contains no framework literal at all, confirmed by grep, so the deleted preference array cannot be reintroduced silently. |
| AC8 | Orphaned hint paths eliminated and detectable | PASS | Independently measured: orphan references fell from 13 to 11 and the eliminated pair is exactly `Deedle` and `FSharp.Core` in `ToDoModel.Test`. The detector direction is asserted in `ProjectConsistency.Tests.ps1`. |
| AC9 | Compatibility gate is asset-level | PASS | Asserted in `PackageCompatibility.Tests.ps1` against asset folder sets rather than a declared framework attribute, in both the rejection and acceptance directions. |
| AC10 | Incompatible package skipped, remaining upgrades proceed | PASS | `Repair-PackageManifestConsistency.Tests.ps1` drives the entry point over a two-candidate in-memory fixture and asserts all three clauses, including the skip record and its non-empty reason. |
| AC11 | Reconciliation covers all four dependent element kinds | PASS | `ProjectConsistency.Tests.ps1` asserts per kind. `Invoke-VersionReconciliation` reconciles `Import`, `Error`, `Reference` and `HintPath` and deliberately excludes `Analyzer`, which is the documented division with the preserve-rule module. |
| AC12 | Analyzer items repaired by preserving the folder segment | PASS | Verified two ways. Structurally, `Get-RewrittenPackageFolderLine` is the only writer and its substitution is anchored on the separators either side of the version segment, reusing both separators and the identifier casing, so no other character can move; `Invoke-AnalyzerItemRepair` uses the offered-segment list only through `.Contains()`. Empirically, the restored packages ship `roslyn4.14`, `roslyn4.8`, `roslyn5.0`, `roslyn5.6`, `roslyn5.9` (Meziantou) and `roslyn3.8`, `roslyn4.7`, `roslyn5.0` (Roslynator), while every delivered item still names `roslyn5.0` and `roslyn4.7` respectively. The measured 80-versus-15 justification is confirmed. |
| AC13 | Sibling elements survive regeneration | PASS | `AnalyzerItemRepair.Tests.ps1` asserts the `AdditionalFiles` element and preceding comment survive, and that a project with no analyzer item group is returned byte-identical. The mechanism supports this trivially: the repair is a same-line substitution, so no sibling line is visited. |
| AC14 | Binding redirects reconciled to the resolved assembly version | PASS with a recorded gap | The unit assertions in `ProjectConsistency.Tests.ps1` hold in both directions. The criterion as written is satisfied. The gap, recorded as a Major code-review finding rather than an AC failure, is that the production trigger path cannot reach this pass: the workflow supplies no `-CandidateUpgrade`, so `$upgrade.Applied` is always empty and the reconciliation block never executes. |
| AC15 | Repair pass leaves a formatting-stable tree | PASS | Second-pass empty diff and a clean `csharpier check` are captured under `evidence/qa-gates/p7-t5` and `p9-t4`. Executor-attested; the delivered tree is formatter-clean. |
| AC16 | Verifier repairs freely and fails only on residual inconsistency | PASS | Both directions asserted in `ProjectConsistency.Tests.ps1`. The failing direction exists, which is what proves the verifier is not a pass-through. |
| AC17 | Repair workflow exists and is statically valid | PASS | `dependabot-repair.yml` is present, actionlint exits 0 with zero output over 9 enumerated workflow files, the job declares `contents: write` and `pull-requests: write`, the `if:` restricts to `startsWith(..., 'dependabot/')`, and `pull_request_target` appears nowhere in the file. |
| AC18 | Repair commit pushed under the GitHub App identity | UNVERIFIED | No App credential and no open Dependabot pull request exist; `evidence/other/p8-t1` records an empty secrets list and a pull-request count of zero. Deferred to #914. This review additionally judges the implementation unlikely to satisfy the criterion once exercised, because the commit email is a hand-written literal that matches no account, so `.author.login` would resolve to null. Recorded as a Major code-review finding. |
| AC19 | Required checks re-run and pass on the post-repair head SHA | UNVERIFIED | Same missing preconditions. Deferred to #914. The mechanism is sound in principle: the push uses an App installation token, which is what causes required checks to re-run. |
| AC20 | Disclosure present and conditional | UNVERIFIED | Same missing preconditions. Deferred to #914. This review notes the disclosure step is unguarded and appends rather than replaces, so the criterion would pass while the body accumulates duplicate blocks. Recorded as a Major code-review finding. |
| AC21 | The #908 three-way divergence reproduced and resolved | PASS | The in-memory fixture is present in `ProjectConsistency.Tests.ps1` with the exact 3.0.235 / 3.0.259 / 3.0.203 shape, and asserts disagreement before and agreement after. |
| AC22 | The AC21 regression test observed failing before the fix | PASS | Failing capture under `evidence/baseline/p5-t5-ac22-fail-before`, passing capture under `evidence/regression-testing/p5-t21-ac22-fail-before-pass-after`. Both artifacts exist and are internally consistent with the commit ordering in `git log`. |
| AC23 | Reference completeness asserted and detectable | PASS | `Test-ReferenceCompleteness` is exercised in both directions, including a fixture with one element removed. The detector can be made to fail, which is the criterion's own non-vacuity condition. |
| AC24 | PowerShell toolchain and coverage | PASS as written, with a policy failure recorded separately | The criterion requires the toolchain to pass in order, each new module under `scripts/dependencies/` to reach at least 90 percent, and no coverage regression on changed lines in `Sync-PackageReferences.ps1`. All three hold: formatter and analyzer clean at the recorded baseline, 302 tests passing, the lowest new module at 94.12 percent, and the rewritten file moving from 0 covered to 95 covered. The criterion's regression clause is weaker than repository policy, and the file's 74.80 percent breaches both coverage floors; that is recorded as a Blocking coverage finding rather than as an AC failure, because the AC does not state a per-file floor for it. |
| AC25 | C# toolchain passes on the delivered tree | PASS | Four commands captured in order at phase 9 with a single clean pass; the analyzer and nullable builds use `/t:Rebuild` and their logs carry no skipped compile target. Executor-attested; this review did not re-run msbuild. The coverage figures were independently re-derived from the committed JaCoCo projection and match: 85.91 percent line, 80.07 percent branch. |
| AC26 | Documentation matches delivered behaviour | PASS | `.github/workflows/README.md` gains 47 lines documenting the repair workflow, its trigger, its credential requirement and the pin; a Pester assertion binds the README pin literal to the workflow literal. |

## Summary

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`
- Total AC items: 26
- Checked off (delivered): 23
- Remaining (unchecked): 3
- Items remaining:
  - AC18 — The repair commit is pushed under the GitHub App identity.
  - AC19 — The required checks re-run and pass on the post-repair head SHA.
  - AC20 — Disclosure is present and conditional.

### Verdict

The bug is fixed in the tree and the fix is independently verified. The 15 stale package import
guards that caused the reported CI failures are gone, the count of examined items proves the
measurement is not vacuous, and the analyzer repair preserved every Roslyn folder segment exactly as
the specification requires.

The consolidation half of issue #911 is complete and verified: four groups become one, the
pull-request limit falls from 10 to 1, and every ignore entry survives.

The prevention half is not yet demonstrated. The repair workflow has never run. Three code-review
findings sit inside it, and three acceptance criteria that would have exercised it are deferred.

Recommendation: **remediate before opening the pull request.** The four blocking findings are
tractable and none requires redesign. The three deferred criteria are a legitimate carry to #914
provided the change is not represented as closing #911 until they are discharged.

## Acceptance Criteria Check-off

No source-file mutation was required. All 23 criteria this review evaluates as PASS were already
marked `[x]` in `spec.md`, and the three evaluated as UNVERIFIED were already `[ ]`. The source file
state and this audit agree item for item.

| Action | Count | Items |
|---|---|---|
| Already checked off and confirmed PASS by this review | 23 | AC1 through AC17, AC21 through AC26 |
| Newly checked off by this review | 0 | none |
| Left unchecked, UNVERIFIED | 3 | AC18, AC19, AC20 |
| Downgraded from checked to unchecked | 0 | none |
