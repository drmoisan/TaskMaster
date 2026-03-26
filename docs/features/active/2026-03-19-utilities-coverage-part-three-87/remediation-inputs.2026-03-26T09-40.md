# Remediation Inputs — utilities-coverage-part-three-87 (2026-03-26T09-40)

## Required fixes

1. **Isolate the `#87` feature branch relative to `development`**
   - **Files / locations:** branch diff as captured in `artifacts/pr_context.appendix.txt`
   - **Expected behavior:** the branch diff relative to `origin/development` contains only the files required to deliver `docs/features/active/2026-03-19-utilities-coverage-part-three-87/`.
   - **Acceptance criteria:**
     - `git diff --name-status $(git merge-base HEAD origin/development) HEAD` no longer lists `.codex/*`, `.github/*`, `QuickFiler/*`, `QuickFiler.Test/*`, other active feature folders (`#96`, `#97`), or unrelated `TaskMaster/*` changes.
   - **Verification commands / tasks:**
     - Refresh `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` against `development` after branch cleanup.
     - Confirm the refreshed appendix shows a scope limited to `UtilitiesCS`, `UtilitiesCS.Test`, and the `#87` feature folder docs/evidence.

2. **Finish the remaining `UtilitiesCS` coverage uplift so AC1 actually passes**
   - **Files / locations:** remaining below-threshold non-skip files called out in:
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qc-test-coverage.md`
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/qa-gates/final-coverage-verification.md`
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/plan.2026-03-22T21-00.md` (`P90-T6` follow-up note)
   - **Expected behavior:** the next coverage-enabled test run records `UtilitiesCS line coverage >= 80%`, and no non-skip compiled `UtilitiesCS` file remains below `80%`.
   - **Acceptance criteria:**
     - `final-qc-test-coverage.md` replacement artifact records `UtilitiesCS line coverage >= 80%`.
     - Coverage verification artifact records `EXIT_CODE: 0`, `Per-file >=80% result (excluding documented skip candidates): PASS`, and zero remaining non-skip below-threshold files.
   - **Minimum change guidance:**
     - Prioritize the worst remaining non-skip files first, including the low-coverage set already named in the plan follow-up note (`ProgressMultiStepViewer.cs`, `MetricChartViewer.cs`, `ThreadMonitor.cs`, `ConfusionViewer.cs`, `InputBox.cs`, `SortEmail.cs`, `ScreenHelper.cs`, `Theme.cs`, `EmailDataMiner.cs`, `FileIO2.cs`, `DfDeedle.cs`, `EmailFiler.cs`, `FileInfoWrapper.cs`, `PeopleScoDictionaryNew.cs`, `DirectoryInfoWrapper.cs`, `ClassifierGroupUtilities.cs`, `SubjectMapSco.cs`) and the broader remaining table in `final-coverage-verification.md`.
     - If any file must remain below threshold, update skip-candidate documentation with policy-compliant rationale before final verification.
   - **Verification commands / tasks:**
     - `dotnet tool run csharpier format .`
     - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`
     - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`
     - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

3. **Synchronize completion-oriented docs only after the coverage gate passes**
   - **Files / locations:**
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/spec.md`
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/user-story.md`
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/plan.2026-03-22T21-00.md`
   - **Expected behavior:** feature docs reflect the actual end state with no checked item implying full completion before AC1 is satisfied.
   - **Acceptance criteria:**
     - `spec.md` status changes to complete only after AC1 passes.
     - Remaining unchecked AC / DoD items are resolved or explicitly left open with documented rationale.
   - **Verification commands / tasks:**
     - Static audit of feature docs after the successful coverage rerun.

4. **Re-run the feature review after branch cleanup and coverage completion**
   - **Files / locations:**
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/policy-audit.*.md`
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/code-review.*.md`
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/feature-audit.*.md`
   - **Expected behavior:** the refreshed review reaches a PASS-style conclusion with no scope blocker and no unmet acceptance criteria.
   - **Acceptance criteria:**
     - Refreshed review artifacts no longer report branch-scope contamination relative to `development`.
     - Refreshed review artifacts no longer report the `UtilitiesCS >= 80%` coverage criterion as failing.
   - **Verification commands / tasks:**
     - Re-run the feature review workflow against refreshed `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`.

## Unmet acceptance criteria

The following acceptance criterion remains unmet:

- Every `.cs` file compiled by `UtilitiesCS.csproj` has `>= 80%` line coverage as reported by Cobertura.

Minimum work required to meet it:
- Raise measured `UtilitiesCS` coverage from `69.81%` to at least `80%`.
- Eliminate the remaining non-skip below-threshold `UtilitiesCS` files or formally reclassify them with documented skip rationale.

## Do not do

- Do **not** merge or review this branch against `development` while unrelated `.codex`, `.github`, `QuickFiler`, `TaskMaster`, or other feature-folder changes remain in the diff.
- Do **not** weaken the `>= 80%` requirement in the feature docs to make the current run appear complete.
- Do **not** silently skip the refreshed PR-context step after branch cleanup.
- Do **not** mark the feature `Complete` while AC1 is still unchecked.
- Do **not** add broad policy suppressions or coverage-config exclusions without explicit scoping justification in the feature docs.