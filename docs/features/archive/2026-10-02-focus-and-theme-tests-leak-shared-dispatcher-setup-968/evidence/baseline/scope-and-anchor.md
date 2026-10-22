# Scope and anchor (issue #968, tasks P0-T2 and P0-T3)

## P0-T2 Scope read

Timestamp: 2026-10-03T02-41
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); $src = Get-Content -LiteralPath "docs\features\active\2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968\spec.md" -Encoding UTF8; foreach ($t in @("- [ ] AC", "- [x] AC", "- [ ] AC32:", "Amendment 1.2", "Amendment 1.1", "acquired and released inside a held", "the removal of its baseline pin", "inherited committed set")) { Write-Output ("TOKEN [" + $t + "] = " + @($src | Where-Object { $_.Contains($t) }).Count) }'
Canonical command: CMD-TOKEN-COUNT on FEATURE/spec.md
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- TOKEN [- [ ] AC] = 32
- TOKEN [- [x] AC] = 0
- TOKEN [- [ ] AC32:] = 1
- TOKEN [Amendment 1.2] = 1
- TOKEN [Amendment 1.1] = 1
- TOKEN [acquired and released inside a held] = 4
- TOKEN [the removal of its baseline pin] = 1
- TOKEN [inherited committed set] = 2
- All five amendment literals are at least 1 (fact 22 values 1, 1, 4, 1, 2 reproduced); no SPEC AMENDMENT MISSING.

Documents read in full: FEATURE/spec.md (amendment 1.2), FEATURE/issue.md (including the Coordinator Scope Amendment), FEATURE/research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md, FEATURE/research/2026-10-02T22-20-qfc-datamodel-972-fold-research.md.

Issue metadata: issue.md line 12 reads `- Work Mode: full-bug`; the heading `## Coordinator Scope Amendment (2026-10-02T22-15, binding)` exists (line 65).

Write Set (fourteen code paths, verbatim):

- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
- QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs
- QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs
- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs
- QuickFiler.Test/QuickFiler.Test.csproj
- QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs
- QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs
- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs
- QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs
- QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs
- QuickFiler.Test/Controllers/QfcDatamodelTests.cs
- QuickFiler/Controllers/QfcDatamodel.cs
- QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs

Prohibited paths (from the plan Write Set section): every file under QuickFiler/ other than the two production Write Set paths; every file under UtilitiesCS/, UtilitiesCS.Test/ and every other project; QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs; every other file under QuickFiler.Test/ not listed above (including QfcDatamodelRethrowTests.cs, QfcQueuePurePathsTests.cs and QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs); QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs; QuickFiler/Interfaces/IQfcDatamodel.cs; TaskMaster.runsettings; scripts/vscode/TaskMaster.cli.runsettings; every file under scripts/; every file under .github/; every file under .claude/; every file under docs/features/potential/; the content of both research documents.

Acceptance-criteria inventory: FEATURE/spec.md section `## Acceptance Criteria`, thirty-two checkbox lines AC1 to AC32, all unchecked.

Promoted records present on the tree (read with the Read tool):

- docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md (heading `# focus-and-theme-tests-leak-shared-dispatcher-setup (Issue #968)`)
- docs/features/potential/promoted/2026-10-02-qfc-datamodel-950-review-residuals.md (heading `# qfc-datamodel-950-review-residuals (Issue #972)`)

## P0-T3 Anchor and pre-change tree state

Timestamp: 2026-10-03T02-41
Commands (separate Bash calls, in order):
1. git -C WORKTREE rev-parse HEAD -> exit 0
2. git -C WORKTREE rev-parse --abbrev-ref HEAD -> exit 0
3. git -C WORKTREE rev-parse origin/main -> exit 0
4. git -C WORKTREE merge-base --is-ancestor 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD -> exit 0
5. git -C WORKTREE merge-base origin/main HEAD -> exit 0
6. git -C WORKTREE diff --name-status 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD -> exit 0
7. git -C WORKTREE diff --exit-code 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD -- QuickFiler QuickFiler.Test UtilitiesCS/Threading/UiThread.cs UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs scripts/vscode TaskMaster.runsettings coverage.config .csharpierignore .gitignore global.json dotnet-tools.json -> exit 0
8. git -C WORKTREE status --porcelain --untracked-files=all -> exit 0

- HEAD-SHA: 911558bac7dd59aca5f6255cff45e33842511308 (observation)
- BRANCH: bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968
- ORIGIN-MAIN-SHA: 993fdd01566dee82e5f37acb761a600feaaa1454 (observation; the P8-T9 comparison basis)
- Ancestor check: exit 0 (BASE is an ancestor of HEAD)
- Merge-base output: 94287369908cc920b21b0e3256314f988ad7d2f5 (equals BASE)
- CODE-TREE-AT-BASE: UNCHANGED (command 7 exit 0)

INHERITED-COMMITTED:
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/planner-review.2026-10-02T22-44.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-clearance.2026-10-03T02-21.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round1-report.2026-10-02T08-40.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round1.2026-10-02T08-40.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round2-report.2026-10-02T23-56.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round2.2026-10-03T00-12.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round3-report.2026-10-03T01-01.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round3.2026-10-03T01-15.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round4-report.2026-10-03T01-25.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round4.2026-10-03T01-43.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round5-report.2026-10-03T01-53.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round6-report.2026-10-03T02-21.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/issue.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/plan.2026-10-02T05-42.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/research/2026-10-02T22-20-qfc-datamodel-972-fold-research.md
- A	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md
- A	docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md
- A	docs/features/potential/promoted/2026-10-02-qfc-datamodel-950-review-residuals.md

Every inherited path is under FEATURE or is one of the two promoted records (no INHERITED SET OUT OF SCOPE).

PRE-EXISTING-WORKTREE-PATHS:
- ` M docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/plan.2026-10-02T05-42.md`
- `?? docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/baseline/phase0-instructions-read.md`
- `?? docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/baseline/scope-and-anchor.md`

No porcelain line names a path under QuickFiler/ or QuickFiler.Test/ (no CODE TREE DIRTY AT ANCHOR).
