# Plan — bayesian-staging-asynclazy-null-guard (Issue #131)

DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED

- **Issue:** #131
- **Owner:** drmoisan
- **Last Updated:** 2026-04-14T10-30
- **Status:** Completed
- **Version:** 2.0
- **Work Mode:** `minor-audit`
- **Requirements Source:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-04-14-bayesian-staging-asynclazy-null-guard-131\issue.md`
- **Plan Path:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-04-14-bayesian-staging-asynclazy-null-guard-131\plan.2026-04-14T07-16.md`
- **Plan Path Continuity:** Updated in place after the earlier async-lazy diagnosis was disproven for this crash.
- **Scope Guardrails:** Keep the fix on the small path. Limit production edits to `UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs` and test edits to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs`.
- **Small-Path Budget:** `Production File Count: 1`; `Test File Count: 1`.
- **Execution Note:** The host for this run did not expose the delegated agent surface required by the orchestration skill, so the documented small-path fallback was executed locally while preserving the active feature folder and canonical plan path.
- **Confirmed Diagnosis:** `BuildCategoryClassifierAsync` is intentionally separate from `ContinueMiningAsync` and `ScrapeAndMineAsync`. When `%LocalAppData%\TaskMaster\Bayesian` is missing or empty, `EmailDataMiner.Load<MinedMailInfo[]>(folderPath)` returns null or an empty array and `CategoryClassifierGroup.LoadStagingData` previously converted that missing prerequisite into an unhandled `ArgumentNullException`.

## Overview

Preserve the existing two-step workflow. Do not auto-run mining from the category-classifier build path. Instead, stop the build cleanly, tell the user which mining actions must run first, remove the unused miner local, and cover the missing-staging-data path with MSTest.

## Acceptance Criteria Source Snapshot

Use only the checkbox items under `## Acceptance Criteria` in `issue.md`:

- `Build Category Classifier no longer crashes when staged Bayesian data is missing.`
- `The user sees an actionable warning that tells them to run Continue Mining or Scrape and Mine before building category classifiers.`
- `The dead EmailDataMiner local in CategoryClassifierGroup.BuildClassifiersAsync is removed or otherwise accounted for.`
- `MSTest regression coverage verifies the missing-staging-data path in UtilitiesCS.Test.`

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read the required repository policy files, the active `issue.md`, and `change-plan.md`, then confirm that this fix fits the C# small path.
- [x] [P0-T2] Reconfirm the diagnosis in source by comparing `CategoryClassifierGroup`, `EmailDataMiner`, and `RibbonController`, and preserve the explicit mine-then-build workflow.

### Phase 1 — Constrained Small-Path Implementation

- [x] [P1-T1] Add a failing MSTest regression in `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` that exercises `BuildClassifiersAsync` when Bayesian staging data is missing.
- [x] [P1-T2] Update `UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs` so missing staging data is treated as a build prerequisite failure with an actionable warning dialog instead of an unhandled exception.
- [x] [P1-T3] Remove the dead `EmailDataMiner` local from `CategoryClassifierGroup.BuildClassifiersAsync`.
- [x] [P1-T4] Update the active feature `issue.md` to reflect the confirmed diagnosis and delivered acceptance criteria.

### Phase 2 — Final QC Loop

- [x] [P2-T1] Run `dotnet tool run csharpier format UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs`.
- [x] [P2-T2] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`.
- [x] [P2-T3] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`.
- [x] [P2-T4] Run `dotnet-coverage collect --output coverage\utilitiescs-category-classifier-staging-prereq-fix.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest.console.exe> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation`.
- [x] [P2-T5] Record that the repo-wide coverage wrapper over all test assemblies aborted after 2,349 passing tests because a separate host process crashed, then complete the clean coverage-backed verification on `UtilitiesCS.Test`, which contains the changed regression.
