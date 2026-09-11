# storewrappercontroller-absent-from-coverage-report (Potential)

- Date captured: 2026-09-11
- Author: Dan Moisan
- Status: Draft

## Problem / Why

`StoreWrapperController` is entirely absent from the Cobertura coverage report, in both the pre- and post-#287 documents, even though only 2 of its members carry `[ExcludeFromCodeCoverage]`. A class that is absent is neither counted as covered nor as uncovered, so the repository figure silently excludes it. It is unknown whether this is an instrumentation gap (assembly not instrumented, class stripped by post-processing, or excluded by the Koverage project allowlist) or a genuine zero that the report drops. Deferred out of the 2026-09-11 consolidated bug run as investigation work (issue #727 sub-finding 2). Source: item #287 review, PR #716.

## Proposed Behavior

Determine why the class is absent and make it appear in the report with a true figure. Candidate causes to check in order: `coverage.config` module excludes; `Get-KoverageProjectAllowlist` and the assembly-discovery filter corrected under #752; the third-party stripping pass in `Invoke-MSTestWithCoverage.ps1`; and whether the class is compiled into an assembly that the test run never loads. If the cause is a tooling defect, fix it in `scripts/vscode/` with a Pester regression. If the class is simply never loaded by any test, add the test that loads it.

## Acceptance Criteria (early draft)

- [ ] The root cause is recorded with evidence, not inferred.
- [ ] `StoreWrapperController` appears as a `<class>` element in the committed coverage projection with a non-null line count.
- [ ] Any other class dropped by the same cause is listed in the delivery.
- [ ] No threshold, exclusion, or allowlist is widened to produce the result.

## Constraints & Risks

- Shares `scripts/vscode/Invoke-MSTestWithCoverage*.ps1` with the CI-gates and evidence-projection items of the 2026-09-11 run; schedule after they merge.
- If the class is COM-bound and its true figure is low, the repository line figure may drop when it appears; that is the correct outcome and must not be avoided.

## Test Conditions to Consider

- [ ] Unit coverage areas: Pester regression for whichever filter or strip step dropped the class.
- [ ] Integration scenarios: one full coverage run showing the class present.
- [ ] CLI/API examples: not applicable.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/storewrappercontroller-absent-from-coverage-report/` folder from the template
