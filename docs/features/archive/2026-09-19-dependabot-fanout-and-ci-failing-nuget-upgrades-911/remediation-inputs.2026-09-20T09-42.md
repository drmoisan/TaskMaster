# Remediation Inputs — Issue #911, Cycle 2

- Date: 2026-09-20
- Source review artifacts:
  - `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/policy-audit.2026-09-20T09-42.md`
  - `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/code-review.2026-09-20T09-42.md`
  - `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/feature-audit.2026-09-20T09-42.md`
- Branch: `bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911` at `db53ca140`
- Base: `origin/main`, merge base `b5621910c5b97d2471e368e87e80dc294207111b`

## This Is an Itemised Fix List, Not a Cycle Trigger

**Zero remediable blocking findings remain.** All eleven findings remediation cycle 1 claimed to
discharge are discharged, tested against the delivered code. The items below are one Major and four
Minor findings, plus two merge-time obligations and one pull-request-time gate.

None of the five findings requires a planned remediation cycle. R-C2-1 is a one-sentence
documentation edit. R-C2-2 through R-C2-5 are each a few lines. If the orchestrator prefers to fold
them into the pull request as in-place edits followed by a re-run of the PowerShell toolchain loop,
that is a sufficient discharge; only R-C2-2 and R-C2-4 touch executable code and neither changes
behaviour that any test currently pins.

## Remediation Triggers Fired

| Trigger | State |
|---|---|
| Policy audit contains FAIL results | yes — per-file coverage floor, workflow green-run, absent canonical coverage artifacts |
| Toolchain checks fail | no |
| Code review contains blockers | no remediable blockers; one pull-request-time gate |
| Required acceptance criteria FAIL or PARTIAL | no — 23 PASS, 3 UNVERIFIED with an evidenced deferral |
| Coverage below policy threshold | yes — one rewritten production file at 81.89 percent against the 85 percent rules floor, structurally capped at 82.68 |
| Coverage artifact absent for a language with changed files | yes — both canonical paths absent, figures substituted from parsed JaCoCo documents |

## G1 — Pull-request-time gate. Record a green CI run at the merge head

**Not remediable in a cycle. Do not attempt another dispatched run.**

Six paths under `.github/workflows/**` changed. No run of any event type exists at head
`db53ca140`. The recorded green run 35513025198 sits at `de9a00106`, two commits back; this review
confirmed the intervening diff is documentation only, so the run is good evidence that the six
gates pass at head today. It is not the run the rule asks for, because the rule asks for the commit
being merged and that commit does not exist yet.

Discharge by opening the pull request and recording the PR-context `CI` run once it concludes
success, with its run id, head SHA and conclusion, under
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/`.

The caller's stated reason for non-discharge — that this repository's ruleset does not evaluate
checks from a `workflow_dispatch` event — was not verifiable here and is not needed. The executor's
own artifact gives the sufficient reason and states it correctly.

## R-C2-1 — Major. Correct the README's binding-redirect claim

**Invariant to restore:** an operator-facing document does not advertise a capability the deployed
configuration cannot exercise.

`.github/workflows/README.md` line 87 says the workflow repairs "a version reconciled in
`packages.config` but not in the `<Import>`, `<Error>`, `<Reference>`, `<HintPath>` and
`<Analyzer Include>` elements that depend on it, **or in an `app.config` binding redirect**".

Decision D2 made the binding-redirect class unreachable from the `workflow_run` trigger, and this
review verified the unreachability in code from both ends. `spec.md`'s AC14 note was amended to say
so and `dependabot-repair.yml` carries the same statement as a comment. The README was not.

Fix: amend the sentence to state that binding-redirect reconciliation exists in
`scripts/dependencies/Repair-PackageManifestConsistency.ps1` but is not exercised by the
`workflow_run` invocation, because that invocation supplies no `-CandidateUpgrade` and the
reconciliation pass runs only over applied upgrades. Name the condition under which it would become
reachable, so the statement stays true if a later change supplies the parameter.

Do not discharge by deleting the clause without explanation; a reader comparing the README against
the script would then think the README is merely incomplete.

## R-C2-2 — Minor. Make the two Reference-version guards accept the same set

**Invariant to restore:** the predicate that decides whether to resolve and the predicate that
decides whether to rewrite must agree.

`scripts/dependencies/ProjectConsistency.psm1`:

- `Resolve-ReferenceAssemblyVersion` line 137 matches
  `'Include="' + [regex]::Escape($PackageId) + ',\s*Version=(?<value>[^,"]+)'` with a default .NET
  regex, which is case-sensitive, and allows no whitespace after the opening quote.
- `Get-RewrittenReferenceVersionLine` line 116 captures `(?<name>[^",]+)` and compares it with
  `$match.Groups['name'].Value.Trim() -ne $targetName`, which in PowerShell is case-insensitive.

Where a manifest `id` and an `Include` name differ only in case, the resolver returns the empty
string, `Invoke-VersionReconciliation` falls back to `$ManifestVersion`, and the R5 behaviour — a
Reference assembly version rewritten to the package version with no assembly evidence — reappears
for that package. No parameter lets a consumer avoid it.

Fix: construct the resolver's pattern with `[System.Text.RegularExpressions.RegexOptions]::IgnoreCase`
and allow optional leading whitespace after the quote. Add one assertion in
`tests/scripts/dependencies/ProjectConsistency.Tests.ps1` driving a fixture whose `Include` name
differs in case from the manifest `id`, asserting the declared version is preserved.

Measured exposure today: **zero** of 912 `<Reference Include="...">` elements in this repository
diverge in case from their sibling manifest identifier. The hazard is latent.

## R-C2-3 — Minor. Make the R9c discovery record actually visible

**Invariant to restore:** a remedy whose stated purpose is observability is observable from the
deployed invocation.

`scripts/dependencies/Repair-PackageManifestConsistency.ps1` line 99 emits the enumerated-directory
and returned-file counts with `Write-Verbose`, and its comment says this "makes the shortfall
observable in the run log". `.github/workflows/dependabot-repair.yml` line 79 invokes the script
with no `-Verbose` and sets no `$VerbosePreference`, so the record is suppressed and the run log
contains nothing.

Fix by either route:

- Add `-Verbose` to the workflow invocation, which also surfaces the three other `Write-Verbose`
  records in the script; or
- Change the line to `Write-Information ... -InformationAction Continue`, which is the pattern
  `scripts/vscode/Sync-PackageReferences.ps1` already uses for its own summary and which needs no
  caller cooperation.

Either way, correct the comment so it describes what the code does.

Note the scope: all 18 `.csproj` in this repository sit at depth 1, so no project is skipped today.
The record exists for the future nested project, which is exactly the case in which nobody will be
watching for it.

## R-C2-4 — Minor. Cover the non-zero-fix summary branch

**Invariant to restore:** the positive outcome of a script is exercised, not only its negative ones.

`scripts/vscode/Sync-PackageReferences.ps1` line 410, the `$totalFixed -gt 0` branch of
`Invoke-PackageReferenceSync`, is uncovered. It is the only one of the four remaining non-seam
uncovered lines that is drivable through the existing injected seam; 387, 390 and 422 are the
default-resolution branches and the dot-source guard.

Fix: add one test to `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` that supplies a seam
producing at least one hint-path repair, and asserts the returned `FixedCount` and the text handed
to the `WriteText` delegate.

This takes the file from 104 of 127 to 105 of 127, which is 82.68 percent — the true ceiling under
the seam, and a figure that can be defended as maximal rather than merely better.

## R-C2-5 — Minor. Correct the P3-T10 deletion table arithmetic

**Invariant to restore:** an evidence artifact's columns sum to its totals.

`evidence/qa-gates/p3-t10-workflow-footprint.2026-09-20T01-37.md` attributes deletions as 1 + 2 + 1
+ 2 = 6 while the measured total is 5, and the total row reconciles to the measured number without
noting the discrepancy. The `[P3-T4]` row is the wrong one: it claims "the old `$updated`
composition and its `WriteAllText`", but `$updated = Join-Path $env:RUNNER_TEMP 'pr-body.md'` is
unchanged context in the diff. Only the `WriteAllText` line was deleted.

Fix: set the `[P3-T4]` row to 1 and change the prose from "the disclosure change replaces two lines
rather than three" to "replaces one line rather than two".

The artifact's conclusion is correct and its decision to report the clause unmet rather than adjust
the change to satisfy a counting expectation was the right call. Only the arithmetic is wrong.

## Merge-Time Obligations, Unchanged From Cycle 1

Both are reproduced from
`evidence/other/p6-t3-merge-time-instructions.2026-09-20T01-37.md` and both are still live.

1. **Squash-merge.** Sanitisation was applied as commit `597bb2fcb`, a follow-up, so the
   pre-sanitisation blobs remain reachable in branch history. This review confirmed 6 occurrences of
   the account name survive in one sample file at `794d34f02`. A merge commit preserves them on the
   default branch. Verified at head: **zero** occurrences of the account name and **zero** of a
   drive-qualified user path across all 288 changed files.
2. **Strip the two false autoclose candidates.** The regenerated
   `artifacts/pr_context.summary.txt` again lists `#MEZIANTOU-898` and `#SHA-256`, exactly as the
   instruction predicted. The correct list is `#181`, `#563`, `#668`, `#895`, `#898`, `#902`,
   `#903`, `#907`, `#908`, `#909`, `#911`. Note that `#898` is already resolved on `origin/main` by
   pull request #913, so listing it is harmless but redundant.

## Carried to #914, Not Remediated Here

- AC18, AC19 and AC20 remain unverified pending a GitHub App credential and an open Dependabot pull
  request. The implementation defects behind AC18 and AC20 are fixed and were verified statically;
  the outcomes cannot be observed. Do not represent issue #911 as closed until they are discharged.
- The four residuals reproduced at `[P5-T12]` are unchanged: that
  `actions/create-github-app-token@v3` publishes an `app-slug` output; that the resolved bot user id
  produces a commit whose `author.login` ends `[bot]`; that the push causes the required checks to
  re-run; and that the disclosure edit produces exactly one block on a real pull-request body.
- The 80-versus-85 coverage floor conflict between `CLAUDE.md` and
  `.claude/rules/general-unit-test.md` is tracked as open issue #668. Only
  `scripts/vscode/Sync-PackageReferences.ps1` sits between the two readings.
- Ten stale binding redirects across six `app.config` files were verified pre-existing at the merge
  base.

## Go / No-Go

**Go for opening the pull request.** Zero remediable blocking findings. The one Blocking gate, G1,
can only be closed by the pull request itself.

Recommended order: apply R-C2-1 in place, optionally fold in R-C2-2 through R-C2-5, re-run the
PowerShell toolchain loop, squash-merge, and record the PR-context CI run against G1.
