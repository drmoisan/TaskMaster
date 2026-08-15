# Bug: `[ExcludeFromCodeCoverage]` does not suppress nested lambdas

- Issue: #457
- Work Mode: full-bug
- Type: bug
- Owner: drmoisan
- Epic: build-ci-coverage-gate-fidelity (wave 1)
- Depends On: #441 (feature folder `cobertura-coverage-arithmetic-441`, wave 0)
- Branch: bug/excludefromcodecoverage-nested-lambdas-457
- Integration Branch: epic/build-ci-coverage-gate-fidelity-integration
- Last Updated: 2026-08-10T14-08

## Promotion Provenance

The potential entry for this issue was promoted before this feature folder was created. GitHub
issue #457 was already open at folder-creation time, and the promoted potential document
(`docs/features/potential/promoted/2026-08-07-excludefromcodecoverage-does-not-suppress-nested-lambdas.md`)
is not present on the epic integration branch. This folder was therefore created with
`new_active_feature_folder` only; `new_potential_bug_entry` and `potential_to_issue` were
deliberately not invoked, because `potential_to_issue` always opens a new GitHub issue and would
have duplicated #457. The issue content below is transcribed from the live GitHub issue body.

## Summary

A method-level `[ExcludeFromCodeCoverage]` attribute does not suppress instrumentation of lambdas
declared inside the attributed member. The C# compiler hoists those lambdas into a separate
compiler-generated closure type (`<>c`, `<>c__DisplayClass*`) whose members do not inherit the
attribute, so the lambda bodies remain in the coverage denominator. When the attributed member is
exempt precisely because it cannot execute in a unit-test host, its nested lambda bodies are
permanently uncovered and permanently counted against the file.

This is a silent measurement defect. It does not crash; it quietly and irreducibly depresses the
line-coverage figure of any file that uses the "thin exempt production forwarder" seam pattern.

## Environment

- OS: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1
- Coverage pipeline: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` producing Cobertura output
- Instrumentation settings: `coverage.config` (dotnet-coverage settings file)

## Steps to Reproduce

1. Take a method carrying `[ExcludeFromCodeCoverage]` that declares one or more lambdas in its body.
2. Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.
3. Inspect the Cobertura report for the file's `<line>` entries.
4. Observe that the source line numbers of the lambda bodies are present with `hits="0"`, while the
   attributed member's own lines are correctly absent.

## Expected Behavior

A lambda declared inside a member carrying `[ExcludeFromCodeCoverage]` is excluded from the
coverage denominator, exactly as the attributed member's own lines are.

## Actual Behavior

The lambda bodies' source lines are emitted under the compiler-generated closure type and counted
in the denominator with `hits="0"`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

`BreadcrumbPopupUiOperations.cs` cannot exceed roughly 91.5% line coverage ((258 - 22) / 258)
regardless of how many tests are written; it currently measures 90.7%. Any gate, audit, or
acceptance criterion that assumes the remaining 9.3% is closable by testing is working from a
false premise. Epic #136 requires every testable file to reach the repository line-coverage floor,
and several of its children plan to adopt exactly this seam pattern; each would inherit an
unannounced ceiling.

## Dependency on Issue #441

Issue #441 (wave 0 of this epic) corrects two arithmetic defects in
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`: the `.//lines/line` descendant-axis double
count in `Get-CoberturaCoverageSummary` and `Merge-CoberturaClassesByFilename`, and the blended
union/primary-methods denominator in `Merge-CoberturaClassesByFilename`. After #441 lands, each
source line appears exactly once in the denominator and per-file `line-rate` equals the rate
computed from the merged class-level `<lines>` set alone (distinct line numbers, max hits per
number).

This feature is specified and planned against the post-#441 contract, not against the current
double-counted behavior. All plan locators are expressed as function/symbol anchors rather than
absolute line numbers, because #441 will have shifted them.

## Scope

In scope:

- Excluding compiler-generated closure-type lines that originate in an `[ExcludeFromCodeCoverage]`
  member from the coverage denominator.
- Regression coverage proving both directions: a lambda inside an exempt member is excluded, and a
  lambda inside a non-exempt member is still counted.
- A re-captured repository coverage baseline measured against the post-#441 arithmetic.

Out of scope:

- Re-tuning any coverage threshold. Threshold reconciliation is owned by issue #494, which runs
  after this feature. A corrected figure that would fail an existing threshold is recorded in
  evidence and handed off to #494.
- Editing `CLAUDE.md` or anything under `.claude/rules/`. Those edits belong to sibling features
  #512 and #494.
- The `/p:Nullable=enable` type-check command documented in `CLAUDE.md` is a known defect
  (issue #522) producing roughly 200-414 spurious errors on a clean `main`. It is not a blocking
  gate for this feature.

## Acceptance Criteria

- [ ] A lambda declared inside a member carrying `[ExcludeFromCodeCoverage]` does not appear in the
      coverage denominator of the post-processed Cobertura report.
- [ ] A lambda declared inside a member that does not carry `[ExcludeFromCodeCoverage]` still
      appears in the coverage denominator.
- [ ] The selected fix surface is recorded in `spec.md` with an explicit justification against the
      candidate alternatives evaluated in research.
- [ ] Deterministic Pester regression tests cover both directions and create no temporary files.
- [ ] A repository coverage baseline is re-captured against the post-#441 arithmetic and recorded
      numerically under `evidence/baseline/` and `evidence/qa-gates/`.
- [ ] No coverage threshold is changed by this feature; any figure that would fail an existing
      threshold is recorded in evidence and handed to issue #494.
- [ ] Full PowerShell toolchain pass completed (format -> lint -> test) with recorded exit codes.

## Source

GitHub issue #457. Potential entry:
`docs/features/potential/promoted/2026-08-07-excludefromcodecoverage-does-not-suppress-nested-lambdas.md`
(promoted prior to this branch; not present on the epic integration branch).
