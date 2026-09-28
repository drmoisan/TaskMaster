# Post-Merge Revalidation Against origin/main 03d2ece20

Timestamp: 2026-09-14T07-30

Verdict: PASS — all four toolchain gates green and all twelve acceptance criteria still hold.

## Why This Artifact Exists

The delivery recorded in this feature folder was verified against merge base `a5622ab9`. The branch was
then 89 commits behind and 16 commits ahead of `origin/main`. Every inherited gate result was therefore
evidence about a superseded tree. This artifact records the re-run of every gate and the re-derivation
of every numeric baseline against the merged tree.

Merge commit: `292782050`. Second parent: `origin/main` at `03d2ece20`.

## Merge Resolution

One conflict occurred, in `.claude/agent-memory/orchestrator/MEMORY.md`. Both sides had independently
recompacted the same index. It was resolved by keeping this branch's committed version per the standing
agent-memory rule. The consequence is stated rather than hidden: index entries main added for its own
new memory files are not carried into the retained index, while those files are present on disk.

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` auto-merged without conflict and the merged file is correct in
both directions: this branch's removal of the `Threading\ProgressTrackerAsync_Tests.cs` Compile item
survived, and main's addition of the `Threading\UiThreadApartmentMeasurement_Tests.cs` Compile item
survived. No other file in the Write Set was touched by main, so the two deletions produced no
modify/delete conflict.

## Toolchain Gates, In CLAUDE.md Order

| Stage | Command | EXIT_CODE | Result |
|---|---|---|---|
| Format | `dotnet tool run csharpier check .` | 0 | Checked 1633 files, no unformatted file |
| Analyze | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | Build succeeded, 0 Warning(s), 0 Error(s) |
| Type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | Build succeeded, 0 Warning(s), 0 Error(s) |
| Test | `vstest.console.exe` per assembly, `/InIsolation`, pinned `/TestCaseFilter:` | 0 | Both runs `Test Run Successful.` |

No stage failed and no stage rewrote a file, so the loop completed in a single pass.

### Non-Vacuity Observations

Both rebuilds were checked against the detailed file log rather than being accepted on exit code alone.
Each recorded 18 `CoreCompile` target runs and zero occurrences of the up-to-date skip message, so
compilation genuinely ran in both. The analyzer build additionally recorded 36 `Csc` task lines. The
nullable build recorded zero occurrences of `Nullable=enable`, confirming the property was not supplied
on the command line and that the gate matches the CI workflow.

## Re-Derived Numeric Baselines

Every inherited figure was re-measured. Differences are reported with their attribution rather than
silently adopted.

| Figure | Inherited | Re-measured | Difference | Attribution |
|---|---|---|---|---|
| UtilitiesCS.Test executed tests | 4897 | 4901 | +4 | main added 4 tests to this assembly (item 816 threading tests) |
| QuickFiler.Test executed tests | 1397 | 1436 | +39 | main added 39 tests (items 743, 871, 742 suites) |
| Coverage-runner population | 7218 | 7261 | +43 | the same 43 tests main added, in the runner's wider population |
| Repository first-party line coverage | 85.72% | 85.84% | +0.12pt | main's added covered code |
| Repository first-party branch coverage | 79.89% | 80.01% | +0.12pt | main's added covered code |
| Lines covered / valid | 56040 / 65378 | 56155 / 65415 | +115 / +37 | main's added covered code |
| `UtilitiesCS.csproj` Compile items, base | 492 | 492 | 0 | main did not touch this project file |
| `UtilitiesCS.Test.csproj` Compile items, base | 478 | 479 | +1 | main added one Compile item |
| `ProgressPackage.cs` covered / total lines | 61 / 61 | 61 / 61 | 0 | main did not touch this file |

Both coverage rates remain above the floors CLAUDE.md governs, which are 80 percent for line coverage
and 75 percent for branch coverage. The 85 and 75 figures under `.claude/rules/` are push-down-owned and
are not authoritative for this repository.

## Acceptance Criteria, Re-Verified

All twelve remain satisfied. The two criteria that are stated as deltas against a base commit were
re-derived against the new base rather than carried forward.

- **AC7** requires each of the two Compile-item counts to fall by exactly one. Against the new base the
  production project falls 492 to 491 and the test project falls 479 to 478. Both deltas are exactly
  minus one. The absolute test-project figures both rose by one relative to the inherited record because
  main added a Compile item; the delta the criterion constrains is unchanged.
- **AC11** requires the UtilitiesCS executed-test count to change by exactly minus six and the QuickFiler
  count by exactly plus two. Measured against the new base by counting `[TestMethod]` in exactly the
  three test files this delivery touches: `ProgressPackage_Tests.cs` 4 to 7 is plus three,
  `ProgressTrackerAsync_Tests.cs` 9 to 0 is minus nine, giving minus six; and
  `QfcStreamingDequeueConfidenceGateTests.Part4.cs` 7 to 9 is plus two. No file carries a `DataRow`
  attribute, so the executed count equals the method count and the arithmetic is sound.
- **AC12** requires no per-file coverage regression on `ProgressPackage.cs`. The post-merge document
  reports 61 of 61 lines covered, a ratio of 1.00 against a baseline ratio of 1.00, with zero
  uncovered lines. Every line of the mandatory floor measures hits 1: lines 26 and 42, the two ownership
  assignments, and lines 179 through 185, the whole Dispose body. The document resolves the file to
  exactly one class element, matching the baseline derivation and confirming a post-processed document.
- **AC1, AC2, AC4** are carried by five named tests, each confirmed `Passed` in the post-merge TRX:
  `DequeueAsync_ZeroAcceptedAndCapReached_LogsScanCapBoundAndStopDecision`,
  `DequeueAsync_ZeroAcceptedAndCeilingReached_LogsCeilingBoundNotScanCapBound`,
  `Dispose_WhenPackageConstructedTheSource_ReleasesIt`,
  `Dispose_WhenCallerSuppliedTheSource_LeavesItUsable` and
  `Dispose_OnSpawnedChild_DoesNotReleaseTheParentsSource`.
- **AC3** is confirmed structurally: `ProgressPackage` declares `IDisposable`, assigns
  `_ownsCancelSource` at the construction site in both `InitializeAsync` overloads, and its
  `CancelSource` setter assigns only the backing source field, so the `SpawnChild` assignment through
  that setter cannot make a child claim its parent's source.
- **AC5** is confirmed structurally: `RebuildAsync` holds its source through a `using` declaration, which
  releases on the completing and the faulting path alike.
- **AC6** is confirmed: neither deleted file is present in the merged working tree.
- **AC8, AC9, AC10** are the three gates recorded in the table above.

## Citation Relocation

Citations were re-resolved by text rather than by line number.

- `UtilitiesCS/UtilitiesCS.csproj`: the production Compile item resolves to line 971 against the new
  base, unchanged.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: the test Compile item resolves to line 499 against the new
  base. The `issue.md` prose still cites line 496. That citation went stale when item 877 merged, which
  is before this branch's merge base, and the existing `evidence/baseline/pinned-source-facts.md` already
  records the corrected value of 499 with the same attribution. The merge performed here did not move it
  further: main's added Compile item sits below line 499.

## A Runner Behaviour Change This Merge Introduced

Item 873 rewrote the coverage runner and added a conditional discard of the raw collector document. The
document is now retained only when its parent directory is exactly the repository `coverage` directory;
any other destination is deleted once the projection is written. The inherited Phase 2 command wrote to
a path under `TestResults`, which the post-873 runner discards, so the inherited AC12 measurement recipe
no longer leaves a document to read. This revalidation therefore wrote to the retained directory
instead. Both candidate paths are git-ignored, so no raw document can be staged either way.

## Evidence Hygiene

Projections only are committed. This artifact transcribes the acceptance-bearing values. No `.trx` file,
no `.cobertura.xml` file and no MSBuild log is added to git. Every vstest span set an explicit
`/ResultsDirectory:` and an explicit `LogFileName=` inside its `/Logger:trx` value, so the default
account-and-host TRX name was never produced. The path `artifacts/csharp/coverage.xml` was not created,
which is deliberate: a hook's 85 percent floor activates only when that file exists.

## Environment

Outlook was confirmed not running before every rebuild and test gate. The shared build lock was acquired
before each toolchain command and released immediately after that command returned. Both
`Meziantou.Analyzer` package versions, 3.0.203 and 3.0.235, are present in the untracked packages
directory, so the pre-existing HintPath skew across 15 project files resolved without any change to
tracked source. The .NET SDK resolved to 8.0.205. No environment defect required remediation.

## Issue 891 Note

`Assert-CoberturaLineCoverageThreshold` throws unless the document-level Cobertura line-rate clears a
hard-coded 80 percent measured across every instrumented assembly, so a `-SearchRoot`-scoped invocation
cannot exit 0 however well its tests do. That defect was not encountered here because the run was
repository-wide and its document-level rate is above the threshold. It is named for completeness and is
tracked as issue 891; the runner script was not modified.
