# Scope and Anchor (P0-T2)

Timestamp: 2026-10-01T22-51
Command: Read of spec.md, issue.md and the research record in full; acceptance-line counts measured with pwsh Get-Content over spec.md (lines starting `- [ ] AC-` and `- [x] AC-`); issue.md line 12 read by index
EXIT_CODE: 0
Output Summary: spec version 1.3; issue.md line 12 reads `- Work Mode: full-bug`; acceptance section holds 16 unchecked and 0 checked AC lines; Write Set and prohibited paths recorded below.

## Requirement documents read

- docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/spec.md (read in full)
- docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/issue.md (read in full)
- docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/research/2026-10-01T07-20-engine-toggle-permanent-config-fault-logs-every-poll-research.md (read in full)

## Observations

- ISSUE-WORK-MODE: issue.md line 12 reads `- Work Mode: full-bug`
- SPEC-VERSION: spec header line 8 reads version 1.3
- SPEC-AC-UNCHECKED: 16 (lines beginning `- [ ] AC-`)
- SPEC-AC-CHECKED: 0 (lines beginning `- [x] AC-`)

## Write Set (verbatim from the plan)

Code files:

- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (modify: edits E1 to E4)
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs` (create)
- `TaskMaster.Test/TaskMaster.Test.csproj` (modify: one compile entry)

Inherited promotion record:

- `docs/features/potential/promoted/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll.md`

Feature documents: spec.md (check-off edits only), the plan file (task check-off edits only), and the evidence files under evidence/baseline, evidence/regression-testing, evidence/qa-gates and evidence/other named in the plan's Write Set section.

## Prohibited files and trees (from the plan's Write Set section)

- Every existing partial of the coordinator fixture present at MERGE-BASE: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs, EngineToggleStateCoordinatorTests.Race.cs, EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs, EngineToggleStateCoordinatorTests.PrimeRegistration.cs and, under shape S, EngineToggleStateCoordinatorTests.ThrowingSink.cs
- TaskMaster/Ribbon/RibbonController.EngineCommands.cs
- TaskMaster/Ribbon/EngineTogglePressedStateCache.cs
- TaskMaster/TaskMaster.csproj
- every packages.config
- TaskMaster.runsettings
- scripts/vscode/TaskMaster.cli.runsettings
- every file under scripts/
- .claude/ (including .claude/agent-memory/, never staged by this plan)
- config/
- artifacts/
- every file under docs/features/potential/ other than the promotion record
- Inside the production file: GetPressed, HandleToggleClickAsync, ExecuteToggleAsync, StartPrimeIfNeeded, StartObservedPrime, ApplyPrimeAsync, the constructor, the _primeTasks declaration, the GetPrimeTask body, RenderEngineName, BuildUnavailableMessage, BuildToggleFailedMessage and BuildUnmappedKeyMessage are not edited.
- No raw trx, cobertura, coverage, coveragexml document or msbuild log is copied into the feature folder.
- No orchestration state file is written or named.
