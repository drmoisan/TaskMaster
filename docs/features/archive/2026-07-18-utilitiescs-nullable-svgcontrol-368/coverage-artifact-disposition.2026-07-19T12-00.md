# Coverage/Process Finding Disposition — utilitiescs-nullable-svgcontrol (Issue #368)

- Timestamp: 2026-07-19T12-00
- Author: orchestrator (child feature orchestrator, epic utilitiescs-nullable-remediation)
- Source review: `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/remediation-inputs.2026-07-19T11-15.md`

## Overall Feature-Review Verdict (unchanged)

Feature-review's independent re-verification found no Blocking code-correctness or
behavior-preservation defect in any of the 12 remediated `SVGControl/` files. All 6 acceptance
criteria (AC1-AC6) were independently re-verified PASS. Zero CS86xx solution-wide under the
per-file pragma gate; zero `<Nullable>` element introduced; 37/37 tests passed; no coverage
regression on `RelativePath.cs`. The two findings below are procedural/systemic, not code
defects in this feature's scope.

## Finding 1 — Coverage artifact absence (C# and PowerShell): ACCEPTED, NOT REMEDIATED

**Independently re-verified by the orchestrator** (not just the reviewer's claim): a plain
`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` in this worktree
fails with `CS0006` ("Metadata file ... could not be found") for `UtilitiesCS.csproj` and
`VBFunctions.csproj`, referencing specific pinned analyzer-package versions
(`Meziantou.Analyzer.3.0.101`, `SonarAnalyzer.CSharp.10.27.0.140913`,
`Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4`) that are not present under `packages/` in this
worktree's package restore. This reproduces independent of `/p:EnableNETAnalyzers=true` and
independent of the previously-tracked Moq/binding-redirect issue (issue #354, fixed by merged PR
#359, already an ancestor of this branch — confirmed via
`git merge-base --is-ancestor <359-merge-commit> HEAD`). It is a distinct, pre-existing,
environment-level packages.config/analyzer-version-pin gap, unrelated to `SVGControl/` and not
introduced or worsened by this feature.

Because `UtilitiesCS.Test`/`VBFunctions.Test` cannot build in this environment, a genuine
repo-wide C# coverage artifact (`artifacts/csharp/coverage.xml`, covering all first-party test
assemblies per the canonical Koverage procedure) cannot be produced here. Generating a
coverage.xml scoped to `SVGControl.Test` alone would not be a repo-wide figure and would misstate
the coverage-verdict row if presented as canonical.

**Disposition: ACCEPTED as a known, tracked, out-of-feature-scope tooling/CI gap.** Not
remediated by this feature. This is not a step requiring human interaction in the
autonomous-execution sense (the fix — restoring/repinning the analyzer NuGet packages for
`UtilitiesCS`/`VBFunctions` — is itself automatable, just out of scope for a `SVGControl/`-only
per-file nullable remediation) and not merge-blocking for this feature.

**Precedent:** Issue #309 (epic swordfish-removal, PR #311) received an identical disposition —
`artifacts/csharp/coverage.xml` absent, forcing a FAIL verdict on the coverage-artifact-presence
row despite strong substitute per-module evidence clearing the floor — and PR #311 was merged on
2026-07-11 with that FAIL row unresolved, on the basis that it was a pre-existing tooling gap, not
a code defect (`.claude/agent-memory/feature-review/project_deletion-only-pr-absent-coverage-artifact-309.md`).
This feature's disposition follows the same precedent.

## Finding 2 — Missing regression test for the `Invoke-MSTestWithCoverage.ps1` bugfix: ACCEPTED, NOT REMEDIATED (follow-up recommended)

The one-line `@(...)` array-wrap fix at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:133` was
applied without a preceding failing regression test, which is a gap against the General Code
Change Policy's Bugfix Workflow. Feature-review independently re-verified the fix's correctness
(rebuilt `SVGControl.Test.csproj`, ran the coverage-wrapped test-discovery step, confirmed no
`Set-StrictMode` throw with exactly one matching `*.Test.dll`). The fix is a defensive,
behavior-preserving array-coercion correction with no plausible regression path for the N>1 case
(the existing `@(...)`-free code already worked for N>1; the wrap only changes N=1 behavior from
throwing to succeeding).

**Disposition: ACCEPTED, not remediated in this cycle.** The fix is incidental shared-tooling
glue discovered while capturing this feature's own coverage evidence, not a change to
`SVGControl/` production code, and its correctness has been independently confirmed by two
separate parties (the executor and the reviewer). A follow-up is recommended (tracked below) but
is not merge-blocking for this feature.

**Recommended follow-up (out of this feature's scope, tracked for a future small-audit fix):**
extract the `Get-ChildItem | Where-Object | Select-Object` test-assembly-discovery pipeline into a
named, testable function in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (following the
existing `Get-DotnetCoverageArgumentList`/`ConvertTo-KoverageCoberturaXml` pattern in that file),
then add a Pester test in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`
asserting the function returns an array (not a scalar) for a single-match input under
`Set-StrictMode -Version Latest`.

## Not Remediation-Required (carried forward from feature-audit)

- The two flagged maintainer-judgment deviations (`ISvgResource` interface nullability;
  additional-member annotations beyond the plan's literal task text) and the
  `SvgImageSelector.ImagePath` judgment call were evaluated in `feature-audit` and judged
  legitimate, in-scope, correctly resolved. No remediation action.
- The CLAUDE.md-vs-`.claude/rules/general-unit-test.md` coverage-threshold conflict (80%/90% vs.
  85%/75% uniform) is a pre-existing, repository-wide condition, out of scope for this feature.

## Conclusion

No remediation cycle is opened for this feature. Both findings are dispositioned as known,
accepted, out-of-feature-scope procedural/tooling gaps, consistent with documented repository
precedent (PR #311 / issue #309). The feature proceeds to PR creation against the epic
integration branch.
