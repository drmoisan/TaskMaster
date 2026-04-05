# Remediation Inputs — quickfiler-navigation-key-collision-111 (2026-03-27T13-11)

## Required fixes

1. **Align the branch diff to the intended QuickFiler duplicate-key scope relative to `main`**
   - **Files / locations:**
     - `QuickFiler/Controllers/KbdActions.cs`
     - `QuickFiler/Controllers/QfcCollectionController.cs` (only if truly required by the fix)
     - `QuickFiler.Test/Controllers/KbdActionsTests.cs`
     - `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/*`
   - **Expected behavior:** `git diff --name-status main...HEAD` must show the requested issue `#111` files, and must not be dominated by unrelated `QfcQueue` or archived-doc work.
   - **Acceptance criteria:**
     - `git diff --name-status main...HEAD` lists the intended QuickFiler production/test files and the matching active feature-folder artifacts.
     - `git diff --name-status main...HEAD -- 'QuickFiler/Controllers/KbdActions.cs' 'QuickFiler/Controllers/QfcCollectionController.cs' 'QuickFiler.Test/Controllers/KbdActionsTests.cs'` is non-empty.
     - `git log --date=short --pretty=format:'%h %ad %an %s' main..HEAD` no longer represents unrelated issue `#106` work as the effective branch payload.
   - **Verification commands / tasks:**
     - `git log --date=short --pretty=format:'%h %ad %an %s' main..HEAD`
     - `git diff --name-status main...HEAD`
     - `git diff --name-status main...HEAD -- 'QuickFiler/Controllers/KbdActions.cs' 'QuickFiler/Controllers/QfcCollectionController.cs' 'QuickFiler.Test/Controllers/KbdActionsTests.cs'`

2. **Populate `issue.md` as the authoritative `minor-audit` requirements source**
   - **Files / locations:**
     - `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/issue.md`
   - **Expected behavior:** `issue.md` must replace placeholders with the real duplicate-key bug description, reproducible scenario, expected/actual behavior, and explicit feature acceptance-criteria checkbox items.
   - **Acceptance criteria:**
     - `issue.md` no longer contains placeholder lines such as `One or two sentences on what is broken.`, `1. ...`, or empty expected/actual behavior sections.
     - `issue.md` contains explicit checkbox items describing the duplicate-key fix expectations and verification expectations for issue `#111`.
     - No `spec.md` or `user-story.md` is introduced for this `minor-audit` feature.
   - **Verification commands / tasks:**
     - Direct file inspection of `issue.md`
     - Confirm `spec.md` and `user-story.md` remain absent from the feature folder

3. **Repair the plan checklist so every checked item is evidence-backed**
   - **Files / locations:**
     - `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/plan.2026-03-27T12-45.md`
     - `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/baseline/p0-t3-format.2026-03-27T12-52.md`
     - `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/regression-testing/p1-t2-kbdactions-distinct-keys.2026-03-27T13-01.md`
   - **Expected behavior:** The plan may only mark tasks complete when the linked artifact satisfies the exact task acceptance. Checked items with failing or non-literal evidence must be corrected.
   - **Acceptance criteria:**
     - `P0-T3` is either backed by a passing artifact for the actual supported formatter command or marked incomplete until corrected.
     - `P1-T2` is backed by deterministic fail-before evidence that satisfies the intended focused-test acceptance without relying on an unplanned manual fallback.
     - The plan remains the single canonical plan file in the feature folder.
   - **Verification commands / tasks:**
     - Re-run the supported formatter command and store schema-valid evidence
     - Re-run a deterministic focused regression invocation for the duplicate-key test and store schema-valid fail-before evidence
     - Reconcile the checklist state in `plan.2026-03-27T12-45.md`

4. **Re-run the QA loop and refresh the review artifacts after scope and evidence corrections**
   - **Files / locations:**
     - `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/policy-audit.*.md`
     - `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/code-review.*.md`
     - `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/feature-audit.*.md`
     - `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/qa-gates/*`
   - **Expected behavior:** After the branch, issue, and plan/evidence chain are corrected, the feature should be re-audited against `main` with an internally consistent evidence set.
   - **Acceptance criteria:**
     - The refreshed audit set no longer reports branch-scope mismatch, placeholder-only `issue.md`, or non-evidence-backed checked plan items.
     - The C# QA loop still passes on the corrected branch state.
   - **Verification commands / tasks:**
     - `dotnet tool run csharpier check .`
     - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
     - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
     - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

## Unmet acceptance criteria

The authoritative `minor-audit` source currently contains **no explicit feature-specific acceptance criteria** for the duplicate-key fix.

Minimum changes required before the feature can be accepted:

- Put the intended QuickFiler duplicate-key change set on the branch relative to `main`.
- Replace placeholder issue text with real issue `#111` requirements and acceptance-criteria checkboxes.
- Synchronize the plan so no checked item depends on failing or non-literal evidence.

## Do not do

- Do not keep unrelated `QfcQueue` or archive-only branch content as the effective payload for issue `#111`.
- Do not introduce `spec.md` or `user-story.md` for this `minor-audit` feature.
- Do not weaken repo policy requirements to excuse missing requirements or broken evidence.
- Do not mark plan tasks complete without schema-valid supporting evidence on disk.
- Do not silently change requirement text after adding explicit acceptance criteria; preserve requirement intent and auditability.