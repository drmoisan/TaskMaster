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
