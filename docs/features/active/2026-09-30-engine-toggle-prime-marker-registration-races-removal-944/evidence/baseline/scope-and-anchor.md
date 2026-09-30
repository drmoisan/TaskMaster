# Scope and Anchor (P0-T2)

Timestamp: 2026-09-30T13-16
Command: pwsh -NoProfile -Command (count lines of spec.md beginning "- [ ] AC" and "- [x] AC"; print issue.md line 12)
EXIT_CODE: 0
Output Summary: spec.md, issue.md and the research record were read in full. spec.md acceptance section: 18 lines beginning "- [ ] AC", 0 lines beginning "- [x] AC". issue.md line 12 reads "- Work Mode: full-bug".

## Documents read in full

- docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/spec.md (298 lines)
- docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/issue.md (65 lines)
- docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/research/2026-09-30T08-00-engine-toggle-prime-marker-registration-research.md (208 lines)

## Write Set (verbatim from the plan)

Code files:

- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (modify)
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` (create)
- `TaskMaster.Test/TaskMaster.Test.csproj` (modify: one compile entry)

Inherited promotion record:

- `docs/features/potential/promoted/2026-09-30-engine-toggle-prime-marker-registration-races-removal.md`

Feature folder: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/ (spec check-off edits, plan check-off edits, evidence files).

## Prohibited files and trees (from the plan's Write Set section)

- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs (main fixture, including the private Harness and its issue 942 OnLogError hook)
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs
- TaskMaster/Ribbon/RibbonController.EngineCommands.cs
- TaskMaster/TaskMaster.csproj
- TaskMaster.runsettings
- scripts/vscode/TaskMaster.cli.runsettings
- every file under scripts/
- every file under .claude/ (including .claude/agent-memory/, never staged)
- every file under config/
- every file under artifacts/
- every file under docs/features/potential/ other than the promotion record
- inside the production file: the CompletePrime method (summary, remarks, body, including its TryRemove statement), the GetPrimeTask method (documentation and body) and the ApplyPrimeAsync method
- no potential entry, no orchestration state file, no raw trx, cobertura, coverage, coveragexml document or msbuild log copied into the feature folder

## Recorded facts

- issue.md line 12: `- Work Mode: full-bug`
- spec.md `## Acceptance Criteria`: 18 lines beginning `- [ ] AC`; 0 lines beginning `- [x] AC` (counted from the file).

## PHASE0-ARTIFACTS:

Appended by P0-T19 at 2026-09-30T13-28. Listing of docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/ (19 files), with a field check (Timestamp, Command, EXIT_CODE, Output Summary, ExpectedExitCode where EXIT_CODE is non-zero):

| Artifact (task) | Timestamp | Command | EXIT_CODE | Output Summary | ExpectedExitCode |
|---|---|---|---|---|---|
| phase0-instructions-read.md (P0-T1) | yes | yes | 0 | yes | n/a |
| scope-and-anchor.md (P0-T2, P0-T19) | yes | yes | 0 | yes | n/a |
| upstream-942-check.md (P0-T3) | yes | yes | 0 | yes | n/a |
| pre-merge-docs-commit.md (P0-T4) | yes | yes | 0 | yes | n/a |
| anchor-merge.md (P0-T5) | yes | yes | 0 | yes | n/a |
| anchor-production-shape.md (P0-T6) | yes | yes | 0 | yes | n/a |
| anchor-edit-regions.md (P0-T7) | yes | yes | 0 | yes | n/a |
| anchor-test-side.md (P0-T8) | yes | yes | 0 | yes | n/a |
| bootstrap-sdk.md (P0-T9) | yes | yes | 0 | yes | n/a |
| bootstrap-tool-restore.md (P0-T10) | yes | yes | 0 | yes | n/a |
| bootstrap-nuget-restore.md (P0-T11) | yes | yes | 0 | yes | n/a |
| bootstrap-dotnet-coverage.md (P0-T12) | yes | yes | 0 | yes | n/a |
| csharpier-check-baseline.md (P0-T13) | yes | yes | 0 | yes | n/a |
| msbuild-analyzer-baseline.md (P0-T14) | yes | yes | 0 | yes | n/a |
| msbuild-nullable-baseline.md (P0-T15) | yes | yes | 0 | yes | n/a |
| stall-probe.md (P0-T16) | yes | yes | 1 | yes | 1 |
| coordinator-tests-baseline.md (P0-T17) | yes | yes | 0 | yes | n/a |
| coverage-baseline.md (P0-T18) | yes | yes | 0 | yes | n/a |
| file-line-counts-baseline.md (P0-T19) | yes | yes | 0 | yes | n/a |

Every artifact named by P0-T1 through P0-T19 exists at its exact path; the one non-zero EXIT_CODE (stall-probe.md, 1) carries ExpectedExitCode: 1.
