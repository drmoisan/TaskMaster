# swordfish-scosorteddictionary-removal — Atomic Implementation Plan (Issue #309)

- **Issue:** #309
- **Parent epic:** swordfish-removal (child F3, integration branch `epic/swordfish-removal-integration`)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-10T20-17
- **Status:** Ready for Preflight
- **Version:** 1.0
- **Work Mode:** full-feature

## Sources of Truth

- Requirements/AC: `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/issue.md`.
- Authoritative spec: `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/spec.md`.
- Acceptance criteria (canonical, full-feature mode): `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/user-story.md` `## Acceptance Criteria`.
- Research: `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/research/research.2026-07-10T21-10.md` (GO recommendation; zero production consumers confirmed).
- Epic manifest: `docs/features/epics/swordfish-removal/epic.md`.

## Evidence Location Determination

- Canonical evidence scheme is `<FEATURE>/evidence/<kind>/` per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` (non-overridable).
- `<FEATURE>` = `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309`.
- No non-canonical evidence path was supplied by the delegation prompt for this plan; all evidence tasks below already target `<FEATURE>/evidence/baseline/`, `<FEATURE>/evidence/other/`, and `<FEATURE>/evidence/qa-gates/`. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` note is required.
- Evidence folders used by this plan:
  - Baseline: `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/baseline/`
  - Other (re-verification, companion-file check): `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/other/`
  - QA gates: `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/qa-gates/`

## Scope Lock (files this plan may create, modify, or delete)

DELETE:
- `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoSortedDictionary.cs`
- `UtilitiesCS.Test/ReusableTypeClasses/ScoSortedDictionary_Tests.cs`

MODIFY:
- `UtilitiesCS/UtilitiesCS.csproj` (remove the single `<Compile Include>` line at line 1047 referencing `ScoSortedDictionary.cs`)
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (remove the single `<Compile Include>` line at line 414 referencing `ScoSortedDictionary_Tests.cs`)

CREATE (evidence only, non-production):
- Files under `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/**` as named in the task list below.

Out of scope (MUST NOT edit, per issue.md/spec.md/user-story.md scope boundary): `UtilitiesSwordfish/**`, `UtilitiesSwordfish.Test/**`, any `ProjectReference` entry in any `.csproj`, `TaskMaster.sln`, `IScoCollection`/`IScoCollection2` or any other Swordfish-dependent interface, `scripts/temp-extract-coverage.ps1`, and any F1 (`ScoDictionary`/`ScoDictionaryNew`), F2 (`ScoCollection`/`ScoStack`), or F4 (`KbdActions` raw-usage) type or file.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Compliance & Baseline

- [ ] [P0-T1] Read repository policy files in the required order — `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md` — and write `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:` (the four files in the order listed above), and the explicit list of files read.
  - Acceptance: The evidence file exists and contains all three required fields, with the four policy files listed in the exact order read.

- [ ] [P0-T2] Capture the C# formatting baseline by running `dotnet tool run csharpier .` (or `csharpier .` if installed globally) from the repository root and write `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/baseline/baseline-format.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (files reformatted count or "no changes needed").
  - Acceptance: The evidence file exists with all four required fields and a numeric or explicit `EXIT_CODE`.

- [ ] [P0-T3] Capture the C# analyzer baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` from the repository root and write `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/baseline/baseline-analyzers.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (warning/error count and build result).
  - Acceptance: The evidence file exists with all four required fields and a numeric `EXIT_CODE`.

- [ ] [P0-T4] Capture the C# nullable/type-check baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` from the repository root and write `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/baseline/baseline-nullable.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (warning/error count and build result).
  - Acceptance: The evidence file exists with all four required fields and a numeric `EXIT_CODE`.

- [ ] [P0-T5] Capture the C# test-and-coverage baseline by running `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage` from the repository root and write `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/baseline/baseline-tests-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the pass/fail test counts and the numeric line-coverage headline values: the `UtilitiesCS.dll` module line-coverage percent (touched-module baseline) and the overall line-coverage percent reported for the run (recorded as the repo-wide baseline figure for the modules exercised by this run, since `UtilitiesCS.Test.dll` is the sole assembly under test for this deletion-only change).
  - Acceptance: The evidence file exists with all four required fields and both numeric coverage percentages (module and overall-for-run) recorded, not placeholders.

### Phase 1 — Deletion

- [ ] [P1-T1] Re-run the repo-wide consumer search for `ScoSortedDictionary` and `ConcurrentObservableSortedDictionary` (`grep -rn "ScoSortedDictionary" --glob "*.cs"`, `grep -rn "ScoSortedDictionary" --glob "*.csproj"`, `grep -rn "ConcurrentObservableSortedDictionary" --glob "*.cs"`) immediately before deleting, and write `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/other/reverify-no-consumer.md` with `Timestamp:`, the three commands run, and their results, confirming the only hits are the class's own definition, its own test, the two `<Compile Include>` build entries, and the unrelated `UtilitiesSwordfish/Collections/ConcurrentObservableSortedDictionary.cs` base-type file.
  - Acceptance: The evidence file exists and states explicitly that zero genuine production consumers were found; if any unexpected consumer is found, this task is marked BLOCKED and Phase 1 deletion tasks are not started.

- [ ] [P1-T2] Confirm no companion files exist for either target file by running `Glob` (or equivalent directory listing) over `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/*` and confirming `ScoSortedDictionary_Tests.cs` has no `.Designer.cs`/`.resx` pairing, and write `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/other/companion-file-check.md` with `Timestamp:` and the directory listing result.
  - Acceptance: The evidence file exists and lists exactly the four files present in the SCO directory (`ScoCollection.cs`, `SCODictionary.cs`, `ScoSortedDictionary.cs`, `ScoStack.cs`), confirming no companion file for `ScoSortedDictionary.cs`, and confirms `ScoSortedDictionary_Tests.cs` has no companion file.

- [ ] [P1-T3] Delete the file `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoSortedDictionary.cs`.
  - Acceptance: The file no longer exists at that path.

- [ ] [P1-T4] Delete the file `UtilitiesCS.Test/ReusableTypeClasses/ScoSortedDictionary_Tests.cs`.
  - Acceptance: The file no longer exists at that path.

- [ ] [P1-T5] Remove the line `<Compile Include="ReusableTypeClasses\Serializable\Concurrent\SCO\ScoSortedDictionary.cs" />` from `UtilitiesCS/UtilitiesCS.csproj`.
  - Acceptance: `grep -n "ScoSortedDictionary.cs" UtilitiesCS/UtilitiesCS.csproj` returns no match.

- [ ] [P1-T6] Remove the line `<Compile Include="ReusableTypeClasses\ScoSortedDictionary_Tests.cs" />` from `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
  - Acceptance: `grep -n "ScoSortedDictionary_Tests.cs" UtilitiesCS.Test/UtilitiesCS.Test.csproj` returns no match.

### Phase 2 — Final QC

Run the full C# toolchain loop in order (format → analyzers → nullable → test+coverage). If any step fails or changes files, restart the loop from the format step (P2-T1) until a clean pass completes in a single sequential run; no `EXIT_CODE: SKIPPED` outcome is valid for any command task below.

- [ ] [P2-T1] Run the final-QC formatting pass `dotnet tool run csharpier .` (or `csharpier .`) from the repository root and write `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/qa-gates/qc-format.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
  - Acceptance: The evidence file exists with all four required fields and `EXIT_CODE: 0`.

- [ ] [P2-T2] Run the final-QC analyzer build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` from the repository root and write `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/qa-gates/qc-analyzers.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
  - Acceptance: The evidence file exists with all four required fields and `EXIT_CODE: 0`, with no new analyzer diagnostics relative to the P0-T3 baseline.

- [ ] [P2-T3] Run the final-QC nullable/type-check build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` from the repository root and write `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/qa-gates/qc-nullable.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
  - Acceptance: The evidence file exists with all four required fields and `EXIT_CODE: 0`, with no new nullable/type-check diagnostics relative to the P0-T4 baseline.

- [ ] [P2-T4] Run the final-QC test-and-coverage pass `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage` from the repository root and write `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/qa-gates/qc-tests-coverage.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the pass/fail test counts (confirming the `ScoSortedDictionary_Tests` test class no longer appears in the run) and the numeric post-change line-coverage headline values: the `UtilitiesCS.dll` module line-coverage percent and the overall line-coverage percent for the run.
  - Acceptance: The evidence file exists with all four required fields, `EXIT_CODE: 0`, and both numeric post-change coverage percentages recorded, not placeholders.

- [ ] [P2-T5] Compare baseline vs. post-change vs. changed-code coverage and write `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/qa-gates/coverage-delta.<timestamp>.md` reporting: (a) the P0-T5 baseline `UtilitiesCS.dll` and overall-for-run percentages, (b) the P2-T4 post-change `UtilitiesCS.dll` and overall-for-run percentages, (c) the changed-code assessment (this change deletes `ScoSortedDictionary.cs` production lines and its 100%-dedicated `ScoSortedDictionary_Tests.cs` test file together, so no remaining line anywhere in the repository lost test coverage as a side effect; explicitly confirm no other file's covered-line count decreased between P0-T5 and P2-T4).
  - Acceptance: The evidence file exists, states all three required comparison figures numerically, and concludes explicitly either "no coverage regression on remaining lines" or identifies a specific regression as a BLOCKING finding.

- [ ] [P2-T6] Verify each of the eight acceptance criteria in `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/user-story.md` `## Acceptance Criteria` against the evidence produced by Phases 0–2 (repo-wide search evidence from P1-T1, file-deletion evidence from P1-T3/P1-T4, `<Compile Include>` removal evidence from P1-T5/P1-T6, toolchain-green evidence from P2-T1 through P2-T4, and the no-other-type-touched/no-`ProjectReference`/no-`.sln`-change confirmation from the Scope Lock diff), and check off each satisfied criterion in `user-story.md`.
  - Acceptance: All eight `user-story.md` acceptance-criteria checkboxes are checked, each backed by a named evidence artifact from this plan; any criterion that cannot be verified remains unchecked and is reported as a blocking gap rather than checked speculatively.

## Test Plan

- Unit: No new unit tests are introduced (deletion-only change per spec.md/user-story.md). Regression coverage is the full `UtilitiesCS.Test` suite run in P0-T5 (baseline) and P2-T4 (post-change), confirming `ScoSortedDictionary_Tests` no longer appears and no other test regresses.
- Integration: Not applicable; no integration surface is touched.
- Manual/CLI: Not applicable.
- Coverage evidence:
  - Baseline: `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/baseline/baseline-tests-coverage.md`
  - Post-change: `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/qa-gates/qc-tests-coverage.<timestamp>.md`
  - Comparison: `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/qa-gates/coverage-delta.<timestamp>.md`

## Open Questions / Notes

- None. Research (`research/research.2026-07-10T21-10.md`) resolved the sole open design question (Q2, Swordfish-free sorted dictionary) as explicitly out of scope for F3.
