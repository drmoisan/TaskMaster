# Feature Audit — Issue #911 (dependabot fan-out and CI-failing NuGet upgrades)

- Date: 2026-09-20
- Reviewer: feature-review
- Cycle: re-audit after remediation cycle 1
- Work mode: `full-bug` (marker read from `issue.md`); acceptance-criteria source is `spec.md` only

## Scope and Baseline

- Branch: `bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`
- Head: `db53ca1407592108af5d6792bb05803574b2b768`
- Base: `origin/main`
- Merge base: `b5621910c5b97d2471e368e87e80dc294207111b`, which is the tip of `origin/main`
- Diff: 288 files, +32,789 / -6,257, 32 commits
- Prior cycle artifacts: `feature-audit.2026-09-20T01-37.md` — 23 of 26 criteria PASS

The branch merged `origin/main` at `b76cb8c39`, so the two-dot and three-dot ranges coincide and
the merge base is the base tip. This review used the full branch diff. No narrowing was applied or
accepted.

### The baseline moved between cycles

The prior audit ran against merge base `734112ed2`. Pull request #913 has since landed the
`Meziantou.Analyzer` analyzer-item realignment on `origin/main`, and the merge absorbed it. The
branch diff now contains **zero `.csproj` and zero `.cs` files**, where it previously contained 15
`.csproj`. This changes attribution, not outcome, and it affects AC5, AC12 and AC13. It is called
out under each.

### Independent baseline comparison

Re-derived from the tree at head rather than read from an executor artifact. Across all 18 project
and manifest pairs there are **1,498** package restore-path references:

| Element kind | Count |
|---|---|
| `<Import>` | 234 |
| `<Error>` | 230 |
| `<HintPath>` | 872 |
| `<Analyzer Include>` | 162 |

**Zero** disagree with a sibling manifest at head. Eleven are orphaned, naming a package identifier
absent from the sibling manifest: `altcover`, `Microsoft.Web.WebView2` and `ObjectListView.Official`
across three test projects. That count and that composition match the prior cycle exactly, which is
the expected result for a cycle that touched no project file.

All **80** `Meziantou.Analyzer` analyzer items name `3.0.235`, and all **16** manifests that declare
the package declare `3.0.235`.

PowerShell coverage moved from **83.93 percent** on `origin/main` (731 of 871 lines) to **94.43
percent** at head (1,611 of 1,706). C# coverage is **85.93 percent** line and **80.10 percent**
branch on a denominator identical to the remediation baseline.

## Acceptance Criteria Inventory

- Source: `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`
- Section: `## Acceptance Criteria`
- Total AC items: **26**
- Checked off in the source file at review time: **23**
- Unchecked in the source file at review time: **3** (AC18, AC19, AC20)

## Acceptance Criteria Evaluation

| AC | Verdict | Basis |
|---|---|---|
| AC1 — Dependabot configuration is consolidated | PASS | `.github/dependabot.yml` at head carries exactly one group, `all-nuget-updates`, with `applies-to: version-updates` and pattern `*`; `open-pull-requests-limit: 1`; an unqualified Deedle ignore; the eight baseline semver-major ignore entries intact; no `group-by` key anywhere. Read from the file, not from the test. |
| AC2 — Config manifests are outside the formatting gate, proven positively | PASS | `.csharpierignore` gained `**/packages.config` and `**/app.config`; the control perturbation proving the check was live is recorded in the prior cycle and the file is unchanged since. |
| AC3 — All 18 manifests normalised and normalisation idempotent | PASS | 34 `app.config` and `packages.config` files in the diff are layout-only changes to the inline NuGet CLI form; the terminal tree is clean, so a re-run produced no diff. |
| AC4 — NuGet CLI version pinned everywhere it is selected | PASS | Four workflows moved from `nuget-version: latest` to `'7.9.0'`; `dependabot-repair.yml` declares the same literal; actionlint exits 0. |
| AC5 — Every analyzer item agrees with its manifest (#898) | PASS, attribution moved | Zero disagreements over 162 analyzer items across 17 projects, re-derived by this review. The `.csproj` edits that produce this state now arrive from `origin/main` via #913, not from this branch's diff. The criterion is a statement about the tree at head and it holds. |
| AC6 — Cold-cache failure observed before and absent after | PASS | Both logs captured in the prior cycle under `evidence/baseline` and `evidence/qa-gates`; no C# or project file changed since. |
| AC7 — Incompatible framework excluded, not ranked (#902) | PASS | `PackageCompatibility.Tests.ps1` asserts the six selector cases including "offered only netstandard2.1 returns no selection"; `Sync-PackageReferences.Tests.ps1` asserts parity through the shared module. The module measures 100 percent line coverage. |
| AC8 — Orphaned hint paths eliminated and detectable (#903) | PASS | `ToDoModel.Test/ToDoModel.Test.csproj` contributes zero orphans in this review's own census; the detector's positive direction is asserted in `ProjectConsistency.Tests.ps1`. |
| AC9 — Compatibility gate is asset-level | PASS | Rejection carries a reason string; acceptance names the selected asset folder. Both asserted. |
| AC10 — Incompatible package skipped, remaining upgrades proceed | PASS | Three separate assertions in `Repair-PackageManifestConsistency.Tests.ps1` over a two-package fixture. |
| AC11 — Version reconciliation covers all four dependent element kinds | PASS | One assertion per kind in `ProjectConsistency.Tests.ps1`; module at 100 percent line coverage. |
| AC12 — Analyzer items repaired by preserving the folder segment | PASS, attribution moved | `AnalyzerItemRepair.Tests.ps1` asserts the preserve rule across four path shapes with fixtures constructed so the highest available folder is not the one the item names. The module is at 100 percent. The tree-level consequence now arrives from `origin/main`. |
| AC13 — Sibling elements survive regeneration | PASS, attribution moved | Same suite; the `<AdditionalFiles>` element, the preceding comment, and the byte-identical no-item-group shape are all asserted. |
| AC14 — Binding redirects reconciled to the resolved assembly version | PASS with a disclosed limitation | The unit assertions hold and the module is at 100 percent. The spec text was amended this cycle to record that the class is exercised by unit assertion only and is not reachable from the `workflow_run` trigger, which this review verified in code from both ends. `.github/workflows/README.md` was not amended to match and still advertises the class; recorded as a Major code-review finding. |
| AC15 — Repair pass leaves a formatting-stable tree | PASS | Terminal tree clean; `dotnet tool run csharpier check .` reports no findings over 1,623 files. |
| AC16 — Verifier repairs freely and fails only on residual inconsistency | PASS | Both directions asserted in `ProjectConsistency.Tests.ps1`; the failing direction is the one that proves the verifier is not a pass-through. |
| AC17 — Repair workflow exists and is statically valid | PASS | Present at 173 lines; actionlint exits 0 with empty output; `contents: write` and `pull-requests: write` declared; the `startsWith(..., 'dependabot/')` restriction present; `pull_request_target` appears nowhere in the file. |
| AC18 — Repair commit pushed under the GitHub App identity | UNVERIFIED | Requires a GitHub App credential and an open Dependabot pull request; both measured absent. The implementation defect the prior cycle found is fixed — the address is now derived at run time in the form GitHub resolves — but the outcome cannot be observed. Carried to #914. |
| AC19 — Required checks re-run and pass on the post-repair head SHA | UNVERIFIED | Same two missing preconditions. The mechanism is sound in principle because the push carries an App installation token. Carried to #914. |
| AC20 — Disclosure present and conditional | UNVERIFIED | Same two missing preconditions. The conditional guard and the idempotent block replacement were added this cycle and verified statically, but neither label state has been exercised against a real pull-request body. Carried to #914. |
| AC21 — The #908 three-way divergence reproduced and resolved | PASS | In-memory fixture with 3.0.235 / 3.0.259 / 3.0.203 across manifest, guard elements and analyzer item; pre-repair disagreements and post-repair agreement both asserted. |
| AC22 — The AC21 regression test observed failing before the fix | PASS | Failing run captured under `evidence/baseline`, passing run under `evidence/regression-testing`. |
| AC23 — Reference completeness asserted and demonstrably detectable | PASS | The detector's failing direction is asserted against a fixture with one element removed. |
| AC24 — PowerShell toolchain and coverage | PASS | Format 0 rewrites of 46 files; analyze 13 findings equal to the recorded baseline; 318 tests, 0 failed. Every new module under `scripts/dependencies/` is at or above 93.81 percent against the criterion's 90 percent clause. `Sync-PackageReferences.ps1` went from 0.00 percent on `origin/main` to 81.89 percent, so the no-regression clause holds by a wide margin. Note that 81.89 is below the 85 percent floor in `.claude/rules/general-unit-test.md`; the criterion as written does not impose that floor and the policy audit records the FAIL separately. |
| AC25 — C# toolchain passes on the delivered tree | PASS | CSharpier check clean; both msbuild gates exit 0 with 0 warnings and 0 errors under `/t:Rebuild`; the analyzer log carries 36 compiler invocations across 18 output assemblies, counted independently by this review, so neither build was vacuous; 7,343 MSTest tests pass. |
| AC26 — Documentation matches the delivered behaviour | PASS as written | `.github/workflows/README.md` documents the repair workflow, its trigger, its credential requirement and the pinned NuGet CLI version, and the Pester pin-equality assertion holds. The criterion's four named subjects are all satisfied. The README's separate claim about binding-redirect repair became inaccurate when decision D2 landed; that is outside this criterion's text and is recorded as a Major code-review finding. |

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

No criterion was newly checked off or newly unchecked by this review. The three that remain are the
three the prior cycle left, their deferral is evidenced by an authorised query returning zero
repository Actions secrets and zero open pull requests, and they are carried by issue #914.

### Verdict

**PARTIAL — 23 of 26 criteria PASS, 3 UNVERIFIED with an evidenced deferral.**

Relative to the prior cycle the criteria tally is unchanged, which is the expected result: the
remediation cycle targeted review findings, not unmet criteria. What changed is the quality of the
evidence behind the criteria that were already passing. AC7's rejection path, AC14's reachability,
AC24's coverage floor and AC26's documentation claims were each re-tested rather than accepted, and
three of the four came back clean.

The retroactive repair — the half of this change that fixes the tree as it stands — is verified
correct and complete at head by an independent re-derivation. The forward-prevention half is now
free of the four defects the prior cycle found in it, but it has still never executed, and three
criteria will stay unverifiable until it does.

Two obligations attach at merge time and neither is a criterion:

1. **Squash-merge.** The sanitisation was applied as a follow-up commit, so the pre-sanitisation
   blobs remain reachable in branch history. A merge commit would preserve them on the default
   branch. This leak class has recurred on #645, #680, #730 and #752 for exactly this reason.
2. **Strip the two false autoclose candidates.** The regenerated `artifacts/pr_context.summary.txt`
   again lists `#MEZIANTOU-898` and `#SHA-256`, as the merge-time instructions predicted. The
   correct list is `#181`, `#563`, `#668`, `#895`, `#898`, `#902`, `#903`, `#907`, `#908`, `#909`,
   `#911`.

Issue #911 must not be represented as closed until #914 discharges AC18, AC19 and AC20.

## Acceptance Criteria Check-off

No check-off change was made by this review. The source file's state is already correct: the 23
criteria this audit evaluates PASS are marked `[x]`, and the 3 evaluated UNVERIFIED are marked
`[ ]`. No criterion was found checked that this audit does not evaluate PASS, and no criterion was
found unchecked that this audit evaluates PASS.

| Criterion | Source state | Audit verdict | Action |
|---|---|---|---|
| AC1–AC17, AC21–AC26 (23 items) | `[x]` | PASS | none required |
| AC18 | `[ ]` | UNVERIFIED | leave unchecked; carried to #914 |
| AC19 | `[ ]` | UNVERIFIED | leave unchecked; carried to #914 |
| AC20 | `[ ]` | UNVERIFIED | leave unchecked; carried to #914 |
