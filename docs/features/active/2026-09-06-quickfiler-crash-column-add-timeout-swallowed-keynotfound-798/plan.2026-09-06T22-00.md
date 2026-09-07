# 2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound (Plan)

- **Issue:** #798
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-07T05-30
- **Status:** Ready for preflight (revision round 2 applied)
- **Version:** 0.4
- **Work Mode:** full-bug
- **Base commit:** c431dc32

**Fail-closed evidence rule:** Every evidence-producing task names its artifact path. If any required
baseline artifact, QA artifact, or coverage-comparison artifact is missing or incomplete, the verdict
is BLOCKED or INCOMPLETE, never PASS. An unchecked task may not be checked off without its artifact.

**Evidence accounting rule:** Each command-step artifact carries `Timestamp:`, `Command:`,
`EXIT_CODE:`, and `Output Summary:`. Artifacts for gates whose expected exit code is non-zero also
carry `ExpectedExitCode:`.

**Blast-radius extraction convention — do not "fix" this.** The section `## Write Set Under Change`
is the authoritative list of files this change creates or modifies. Backticks elsewhere in this
document mark code identifiers, read-only toolchain inputs, gitignored build outputs, and
item-scoped feature-folder artifacts. A command span is backticked only when every repository path
inside it belongs to the write set or to this item's feature folder; a command span naming any other
repository path, including a pathspec exclusion, is written as plain prose without backticks,
because the extractor tokenizes inside a backtick span and cannot tell a command operand from a
write claim.
The category "read-only toolchain inputs" in the preceding sentence covers identifiers and tool
names, not repository file paths: the solution file, the coverage settings file, the runsettings
file, the repository-root instruction file and the dot-claude policy documents are all named as
plain prose throughout this document, matching the explicitly-excluded-systems list in spec.md.
The same rule governs a non-command literal that quotes a repository path: an existing
`<Compile Include>` anchor line naming a neighbouring file outside the write set has that file name
written as plain prose, while the entry the task inserts, which names a write-set path, stays
backticked.
Files this change deliberately does **not** touch are
written as plain prose without backticks, matching the convention established in spec.md: the
downstream extractor cannot read negation, so backticking an untouched path produces a false
scheduling conflict against a concurrently-prepared sibling item.

**Requirements authority.** spec.md is the sole authoritative acceptance-criteria source for this
`full-bug` item and carries 14 criteria, AC1 through AC14. The research record under the feature
folder's research directory is the technical record; where it corrects an issue.md citation, the
research record wins.

---

## Write Set Under Change

### Production (6)

- `UtilitiesCS/Extensions/DfDeedle.cs`
- `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`
- `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs`
- `QuickFiler/Controllers/QfcDatamodel.cs`
- `TaskMaster/Ribbon/RibbonCommandBoundary.cs`
- `TaskMaster/Ribbon/RibbonViewer.cs`

### Test (5)

- `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs`
- `UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs`
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`
- `TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs`
- `QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs`

### Project compile-entry files (5)

- `UtilitiesCS/UtilitiesCS.csproj`
- `TaskMaster/TaskMaster.csproj`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- `TaskMaster.Test/TaskMaster.Test.csproj`
- `QuickFiler.Test/QuickFiler.Test.csproj`

Total: 16 paths. Feature-folder artifacts under
`docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/`
are item-scoped and are additionally created or updated by this plan.

---

## Toolchain conventions

The four toolchain steps run in this order and restart from step 1 whenever any step fails or
rewrites a file:

1. `dotnet tool run csharpier format .`, verified with `dotnet tool run csharpier check .`
2. msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
3. msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
4. `vstest.console.exe` over the built test assemblies with coverage

Non-negotiable facts that make a gate vacuous or unsatisfiable if ignored:

- `/t:Rebuild` is mandatory. MSBuild's up-to-date check does not invalidate on a command-line `/p:`
  change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and runs no
  analyzers.
- `/p:Nullable=enable` must not be added. No project carries a `<Nullable>` element and there is no
  repository-root build property file, so the property conscripts every file that never adopted the
  pragma. CI omits it deliberately.
- A successful msbuild run still prints the substring `error` many times in switch names and summary
  text. Acceptance conditions assert the exit code plus the summary line `0 Error(s)`, never the
  absence of the substring `error`.
- `csharpier format` is a write-mode command that exits 0 whether or not it rewrote files, and its
  own summary line reports the number of files **scanned**, not the number rewritten, so that line
  cannot distinguish a clean run from a repairing one. Every task that invokes it therefore records a
  before-and-after git status --porcelain --untracked-files=all -- . ":(exclude).claude"
  observation stating whether the format pass rewrote any file, and is paired with
  `dotnet tool run csharpier check .`, whose success-case summary line begins with the literal
  `Checked ` and ends with the literal `ms.` and is recorded verbatim in the artifact. No task in
  this plan asserts the literal `Formatted 0 files in`: csharpier 1.2.6 prints it on no tree, in
  neither command.
- Every `git diff` in an acceptance condition is anchored to the base commit `c431dc32`. Every
  name-listing diff is paired with a `git add --intent-to-add` span or a
  `git status --porcelain --untracked-files=all` span in the same task, because a name-listing diff
  cannot report a newly created untracked file.
- The dot-claude agent-memory directory is tracked in this repository. Every diff, status, and
  name-listing gate in this plan is scoped with a pathspec that excludes it, written in plain prose
  as ":(exclude).claude".

### Local test invocation

The vswhere-resolved `vstest.console.exe` path is not preserved between tasks; each command-bearing
task resolves it inline. The canonical local flag set is CI's
`/EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` with two
local additions established by Phase 0:

- Assembly discovery excludes any path containing a dot-claude directory segment, matched in plain
  prose as "\.claude\", because a recursive `*.Test.dll` search
  under `bin\Debug\` also matches stale build outputs in leftover agent worktrees and produces mass
  bogus failures. CI is unaffected because it starts from a fresh checkout.
- `/InIsolation` is required. Without it vstest runs in-process and never loads each assembly's
  configuration file, so binding redirects are ignored and assemblies fail to load. The signature is
  an empty error message with a sub-millisecond duration across many tests; that is an assembly-load
  failure, not a regression.
- `/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None` names any new hang.
- `P0-T8` determines whether the four `UtilitiesCS.Test` shell-icon classes still stall the local
  testhost through `SHGetFileInfo`. If they do, every later run extends the filter with
  `&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser`
  and the exclusion is recorded as an environmental exclusion covered by CI. If P0-T8 shows they now
  pass, the filter is not extended.
- `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` is a known sporadic failure
  under high-worker coverage runs, tracked as issue #780. It is recorded if seen and is not treated
  as a regression caused by this change.

### Coverage contract

Numeric coverage comes from `dotnet-coverage collect --output-format cobertura`, which writes the
Cobertura document even when the inner test run exits non-zero. Per-file figures are computed by
aggregating every Cobertura `class` element whose `filename` attribute matches the target file, and
counting only the `line` elements that are direct children of that class element's `lines` element;
`line` elements nested under a `method` element duplicate the same source lines and must not be
counted. The document-level `line-rate` attribute is a fraction between 0 and 1, not a percentage.

Blocking coverage obligations for this item, all change-scoped:

- Each of the two new production files `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` and
  `TaskMaster/Ribbon/RibbonCommandBoundary.cs` reaches a per-file line rate at or above 0.90. These
  two files carry all three new modules named in spec.md: the column partial, the validator that
  lives inside it, and the ribbon boundary type.
- No production write-set file loses covered lines relative to the Phase 0 baseline, measured with
  the relocation adjustment defined in P8-T6.
- The ribbon boundary decision logic lives in a type that is not marked `[ExcludeFromCodeCoverage]`.

The repository-wide figure is a record-and-report obligation, not a blocking gate for this item: no
merge-base repository baseline exists in this feature folder, the repository floor applies to the
testable denominator after the COM, VSTO and WinForms exemptions, and the repository-wide Cobertura
line rate produced by this pipeline is not reproducible run-to-run on an identical tree. The final QC
evidence records the baseline figure and the post-change figure and states the direction of movement.

---

### Phase 0 — Baseline capture and environment verification

- [ ] [P0-T1] Read the repository policy documents in the mandated order and record the read list in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/phase0-instructions-read.md`.
  - Read, in order, written as plain prose because spec.md's excluded-systems section names the repository-root instruction file and the dot-claude tree as paths this change does not touch: CLAUDE.md, then .claude/rules/general-code-change.md, then .claude/rules/general-unit-test.md, then .claude/rules/quality-tiers.md, then .claude/rules/tonality.md, then .claude/rules/csharp.md.
  - Acceptance: the artifact exists and contains the fields `Timestamp:` and `Policy Order:` plus one line per file read, with all six paths present.

- [ ] [P0-T2] Record worktree identity and base commit in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/worktree-identity.md`.
  - Run `git rev-parse --show-toplevel`, `git rev-parse HEAD`, `git rev-parse --abbrev-ref HEAD`, and git status --porcelain --untracked-files=all -- . ":(exclude).claude" written in plain prose.
  - Acceptance: the artifact records a toplevel path ending in `agent-afce202e93dec23a9`, records the resolved `HEAD` value, and records `EXIT_CODE: 0` for each of the four commands. The recorded merge-base against the base commit is the forty-character object name whose abbreviated form is `c431dc32`; verify with `git merge-base HEAD c431dc32` and record the full value the command prints.

- [ ] [P0-T3] Bootstrap the .NET SDK and the CSharpier tool manifest for this worktree and record the outcome in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/toolchain-bootstrap.md`.
  - Run pwsh -File scripts/vscode/Install-RepoDotNetSdk.ps1, written in plain prose because that script is outside the write set, if `dotnet --version` fails, then `dotnet tool restore`.
  - Resolve and record the MSBuild and vstest paths: `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find 'MSBuild\**\Bin\MSBuild.exe'` and the same vswhere call with `-find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`.
  - Record whether `dotnet-coverage` resolves; if it does not, run `dotnet tool install --global dotnet-coverage` and record the install.
  - Acceptance: the artifact records `EXIT_CODE: 0` for `dotnet tool restore`, a non-empty resolved MSBuild path, a non-empty resolved vstest path, and a non-empty `dotnet-coverage --version` value. Host-specific absolute paths are redacted to a `<repo-root>` or `<user>` token before the artifact is written.

- [ ] [P0-T4] Restore NuGet packages for the non-SDK-style projects and record the outcome in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/nuget-restore.md`.
  - Run nuget restore TaskMaster.sln, written in plain prose because the solution file is outside the write set. If `nuget` does not resolve on this host, run pwsh -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" instead, which performs the same packages.config restore through MSBuild, and record in the artifact which of the two was used.
  - Acceptance: the artifact records `EXIT_CODE: 0` and its `Output Summary:` states the number of packages installed or, when the nuget path was used, the tool's already-installed summary line, quoted verbatim in the artifact as plain prose because it names a repository configuration file outside the write set. When the Invoke-Restore.ps1 fallback was used, the `Output Summary:` states the package count the MSBuild restore reported. CS0006 missing-assembly errors in a later build indicate this task did not complete and require re-running it.

- [ ] [P0-T5] Capture the CSharpier baseline for `UtilitiesCS/Extensions/DfDeedle.cs` and the rest of the tree in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/csharpier-check-baseline.md`.
  - Run `dotnet tool run csharpier check .`.
  - Acceptance: the artifact records the command, the exit code, and the full summary line printed by csharpier 1.2.6. If the exit code is non-zero, the artifact enumerates every file csharpier reports as unformatted; those files are pre-existing drift and are recorded, not repaired, at this point.

- [ ] [P0-T6] Capture the analyzer build baseline over the solution file in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/msbuild-analyzers-baseline.md`.
  - Run msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true, written as plain prose because the solution file is outside the write set.
  - Acceptance: the artifact records `EXIT_CODE: 0` and its `Output Summary:` quotes the summary line `0 Error(s)` together with the reported warning count.

- [ ] [P0-T7] Capture the nullable build baseline over the solution file in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/msbuild-nullable-baseline.md`.
  - Run msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true, written as plain prose because the solution file is outside the write set.
  - Acceptance: the artifact records `EXIT_CODE: 0` and its `Output Summary:` quotes the summary line `0 Error(s)`. The artifact states explicitly that `/p:Nullable=enable` was not supplied and why.

- [ ] [P0-T8] Probe whether the four shell-icon test classes still stall the local testhost, and record the decision in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/shell-icon-stall-probe.md`.
  - Resolve vstest via vswhere, then run it against `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` with `/InIsolation /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /Logger:trx /ResultsDirectory:coverage\trx\p0-shellicon` and `/TestCaseFilter:"FullyQualifiedName~ShellUtilities|FullyQualifiedName~SysImageListHelper|FullyQualifiedName~OSBrowser"`.
  - Acceptance: the artifact records `ExpectedExitCode:` reflecting the observed outcome, the total and passed test counts read from the produced `.trx`, and one of exactly two verdicts written verbatim: `SHELL_ICON_EXCLUSION: REQUIRED` or `SHELL_ICON_EXCLUSION: NOT REQUIRED`. Every later vstest task in this plan applies the filter extension if and only if the verdict is `SHELL_ICON_EXCLUSION: REQUIRED`.

- [ ] [P0-T9] Capture the full-suite baseline test run with coverage and write the Cobertura document to `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/coverage-baseline.cobertura.xml`.
  - Discover assemblies with `Get-ChildItem -Path . -Recurse -Filter '*.Test.dll'` filtered to paths matching `\bin\Debug\` and not matching `\obj\`, `\ref\`, or a dot-claude directory segment written in plain prose as "\.claude\".
  - Run dotnet-coverage collect --output coverage\baseline.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest> <assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p0-baseline /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook", written as plain prose because the settings file and the runsettings file are repository paths outside the write set, extending the filter per the P0-T8 verdict. The vstest path and the assembly list are resolved inline as described under Local test invocation.
  - Copy `coverage\baseline.cobertura.xml` to the evidence path above. The `coverage` directory is gitignored, so the evidence copy is the only retained artifact and must be added to the index.
  - Acceptance: the evidence Cobertura file exists, `git ls-files --error-unmatch` succeeds for it after `git add --intent-to-add`, and a companion artifact `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/vstest-coverage-baseline.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode:`, and an `Output Summary:` containing the total, passed, failed and skipped test counts and the document-level `line-rate` value as a decimal fraction. The artifact additionally designates the observed failing tests as `PREEXISTING_FAILURE_SET`. If that set contains any test other than the issue #780 sporadic `TryAddValuesAsync_UpdatesExistingValue` and any test excluded by the P0-T8 verdict, the artifact records each such test by fully-qualified name and states verbatim `BASELINE NOT GREEN: P8-T4 cannot reach a failed count of 0 without a separate remediation`, because P8-T4's acceptance and AC14 both require a clean final run. That condition is reported to the maintainer at the end of Phase 0 rather than discovered in Phase 8.

- [ ] [P0-T10] Extract baseline per-file coverage for the six production write-set paths into `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/per-file-coverage-baseline.md`.
  - Load the evidence Cobertura document, select every `class` element whose `filename` attribute ends with `Extensions\DfDeedle.cs`, `Controllers\QfcDatamodel.FrameBuilding.cs`, `Controllers\QfcDatamodel.cs`, or `Ribbon\RibbonViewer.cs`, and for each target file sum the count of direct-child `lines/line` elements and the count of those whose `hits` attribute is not `0`.
  - Acceptance: the artifact records, for each of those four existing files, a `covered=` integer and a `valid=` integer, both derived from the counts above, and records `covered=0` and `valid=0` with the note `NOT INSTRUMENTED` for any file that produces no matching class element. `TaskMaster/Ribbon/RibbonCommandBoundary.cs` and `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` do not exist at baseline and are recorded as `ABSENT AT BASELINE`.

- [ ] [P0-T11] Record the baseline line counts of the eleven write-set `.cs` files in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/line-count-baseline.md`.
  - Acceptance: the artifact records one `path=count` line per existing file, five in total, and states `ABSENT` for each of the six `.cs` files this change creates. The recorded counts must be `UtilitiesCS/Extensions/DfDeedle.cs=410`, `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs=154`, `QuickFiler/Controllers/QfcDatamodel.cs=483`, `TaskMaster/Ribbon/RibbonViewer.cs=388`, `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs=882`. A different value for any of these means the worktree is not at the base commit and the plan must be re-verified before proceeding.

- [ ] [P0-T12] Record the pre-existing 500-line-cap state in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/line-cap-preexisting.md`.
  - Acceptance: the artifact states that `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` is at 882 lines at the base commit and is therefore already over the repository's 500-line cap before this change; that bringing it under the cap would require splitting it into an additional file outside the 16-path write set fixed by spec.md, which the bugfix workflow in CLAUDE.md prohibits as an opportunistic refactor; and that AC13 as written therefore holds this one file to a strictly decreasing line count relative to 882 rather than to the absolute cap. The artifact records this as satisfying AC13, not as a deviation from it, and states that every other file created or modified by this change is held to the absolute 500-line cap. The artifact also records that AC13's final clause requires the pre-existing violation to be promoted as a follow-up, tracked by P9-T16.

- [ ] [P0-T13] Probe cross-assembly log4net capture by temporarily appending a probe test to `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` and record the outcome in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/log4net-capture-probe.md`.
  - The probe test is named `Probe798_MemoryAppenderCapturesDfDeedleLogger`. It reflects the private static `logger` field of `UtilitiesCS.DfDeedle`, attaches a `log4net.Appender.MemoryAppender` to the logger named by `typeof(DfDeedle).FullName` through `(Hierarchy)LogManager.GetRepository()` with `Level.Debug`, invokes `Debug` on the reflected `ILog` instance with the literal message `probe-798`, and asserts the appender's events contain that message.
  - Build with the analyzer command from P0-T6, then run vstest against `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` with `/InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p0-log4net /TestCaseFilter:"FullyQualifiedName~Probe798_MemoryAppenderCapturesDfDeedleLogger"`.
  - Acceptance: the artifact records exactly one of two verdicts written verbatim: `LOG4NET_CROSS_ASSEMBLY_CAPTURE: CONFIRMED` or `LOG4NET_CROSS_ASSEMBLY_CAPTURE: NOT AVAILABLE`, together with the trx-reported passed and failed counts for that single test. If the verdict is `NOT AVAILABLE`, the artifact additionally records the fallback strategy defined in P2-T5: the `MemoryAppender` is attached to the repository reached through the production assembly's own logger instance rather than through the test assembly's default repository. The artifact states verbatim that the fallback suggested in spec.md's assumptions section — asserting AC2 indirectly through the injected column-adder — is not used, because injecting an adder replaces the whole `AddQfcColumns` body so the AC2 instrumentation never executes and the assertion could not fail. AC14 requires only that the fallback actually used be documented in the plan, and P2-T5 documents it.

- [ ] [P0-T14] Remove the probe test from `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` and confirm the file is byte-identical to the base commit.
  - Acceptance: `git diff --stat c431dc32 -- UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` produces no output lines, and a repository-wide search for the literal `Probe798_MemoryAppenderCapturesDfDeedleLogger` over `*.cs` returns zero matches. Record both observations in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/log4net-capture-probe.md` under a `Probe Removal:` heading.

- [ ] [P0-T15] Probe the plan validator MCP tool and record the outcome in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/mcp-validator-probe.md`.
  - Attempt `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"` and `artifact_path` set to this plan file.
  - Acceptance: the artifact records either the validator's exit status and error output, or the literal line `VALIDATOR NOT RUN: tool absent from this agent tool surface`. This task never halts the plan; an absent validator is recorded and execution continues.

---

### Phase 1 — Defect-preserving seams, compile entries, and reflection-test repair

The seams in this phase deliberately preserve today's defective behaviour so that the Phase 2
regression tests compile and fail at runtime rather than reddening whole assemblies at compile time.
No behavioural fix is applied in this phase.

- [ ] [P1-T1] Create `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` as a new partial of `public static partial class DfDeedle` in namespace `UtilitiesCS`, holding the four column methods moved verbatim out of `UtilitiesCS/Extensions/DfDeedle.cs`.
  - Move `AddQfcColumns`, `AddQfcColumnsAsync`, `EnsureTriageColumnExists` and `HasUserDefinedProperty` without altering their bodies. The file carries `#nullable enable` to match the source partial.
  - Acceptance: the file exists, contains the literal `public static partial class DfDeedle`, and contains the four identifiers `AddQfcColumns`, `AddQfcColumnsAsync`, `EnsureTriageColumnExists`, `HasUserDefinedProperty`.

- [ ] [P1-T2] Remove the four moved methods from `UtilitiesCS/Extensions/DfDeedle.cs` so no member is declared twice.
  - Acceptance: a search of `UtilitiesCS/Extensions/DfDeedle.cs` for the literal `private static void AddQfcColumns(` returns zero matches, and the file's line count is strictly less than 410.

- [ ] [P1-T3] Widen `AddQfcColumnsAsync` in `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` to `internal` and add the two optional parameters, keeping the existing defective timeout loop unchanged.
  - The declaration becomes `internal static async Task AddQfcColumnsAsync(Table table, MAPIFolder folder, CancellationToken token, int counter, Action<object, object>? columnAdder = null, TimeProvider? timeProvider = null)`.
  - Three mechanical substitutions are made and nothing else. First, the body of the existing `Task.Run` becomes `adder(table, folder)` where `adder` is `columnAdder ?? ((t, f) => AddQfcColumns((Table)t, (MAPIFolder)f))`, so an injected adder is actually invoked; without this the Phase 2 non-overlap tests observe an invocation count of zero and their fail-before evidence cannot be produced. Second, `timeProvider` is threaded into the existing `TimeoutAfter` call as its third argument, which keeps the same bound overload. Third, both recursive calls forward the two new arguments, so a `FakeTimeProvider` supplied by a test drives the second and third deadlines as well as the first; without this only the first attempt is deterministic.
  - The defect is preserved exactly: the recursion itself, the `counter < 2` guard, the empty `catch` body at exhaustion, and the 3000 ms deadline are unchanged, so the method still starts a new `Task.Run` per attempt and still completes successfully after the third timeout.
  - The generic argument type is `object` rather than the interop types because embedded interop types cannot be used as generic type arguments across an assembly boundary, which produces CS1769; the same constraint is already recorded in source on the `TableEtlInvoker` and `StoreTableEtlInvoker` seams.
  - Acceptance: the file contains the literal `internal static async Task AddQfcColumnsAsync(`, the literal `Action<object, object>? columnAdder = null`, the literal `adder(table, folder)`, and exactly two occurrences of the literal `counter + 1`; and the analyzer build in P1-T13 reports `0 Error(s)`.

- [ ] [P1-T4] Add the not-yet-implemented validator `ValidateRequiredEmailColumns` to `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` with a body that performs no validation.
  - The declaration is `internal static void ValidateRequiredEmailColumns(Dictionary<string, int> columnInfo, string folderName)`. The body contains only a comment recording that the AC3 implementation lands in P4-T1, so today's absence of validation is preserved and the Phase 2 test fails at runtime rather than at compile time.
  - Acceptance: the file contains the literal `internal static void ValidateRequiredEmailColumns(`, and no call to that identifier exists anywhere under the UtilitiesCS extensions directory, written as plain prose because that directory also holds files outside the write set.

- [ ] [P1-T5] Create `TaskMaster/Ribbon/RibbonCommandBoundary.cs` as an `internal sealed class` in namespace `TaskMaster` that is not marked `[ExcludeFromCodeCoverage]`, with a pass-through `RunAsync` that does not catch.
  - The constructor is `internal RibbonCommandBoundary(Action<string, System.Exception> logFailure, Action<string> presentFailure)` and rejects a null argument with `ArgumentNullException`. `internal Task RunAsync(string commandName, Func<Task> action)` awaits `action` and does not catch, preserving today's unguarded behaviour.
  - Acceptance: the file exists, contains the literal `internal sealed class RibbonCommandBoundary`, and a search of that file for the literal `ExcludeFromCodeCoverage` returns zero matches.

- [ ] [P1-T6] Add the `<Compile Include>` entry for the new partial to `UtilitiesCS/UtilitiesCS.csproj` adjacent to the existing entry for the sibling partial.
  - Insert `    <Compile Include="Extensions\DfDeedle.QfcColumns.cs" />` immediately after the existing `<Compile Include>` line at line 993, whose Include attribute is Extensions\DfDeedle.FrameUtilities.cs, written as plain prose because that neighbouring file is outside the write set. Do not append at the end of the `ItemGroup`, so a concurrently-prepared sibling item's insertion lands in a different place.
  - Acceptance: the project file contains the literal `<Compile Include="Extensions\DfDeedle.QfcColumns.cs" />` exactly once.

- [ ] [P1-T7] Add the `<Compile Include>` entry for the ribbon boundary to `TaskMaster/TaskMaster.csproj` adjacent to the existing ribbon-runner entry.
  - Insert `    <Compile Include="Ribbon\RibbonCommandBoundary.cs" />` immediately after the existing `<Compile Include>` line at line 461, whose Include attribute is Ribbon\EngineGatedCommandRunner.cs, written as plain prose because that neighbouring file is outside the write set.
  - Acceptance: the project file contains the literal `<Compile Include="Ribbon\RibbonCommandBoundary.cs" />` exactly once.

- [ ] [P1-T8] Create the three new test files as compiling stubs: `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs`, `UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs`, and `TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs`.
  - `DfDeedleQfcColumnTimeoutTests` and `DfDeedleRequiredColumnValidationTests` sit in namespace `UtilitiesCS.Test.Extensions`; `RibbonCommandBoundaryTests` sits in namespace `TaskMaster.Test.Ribbon`. Each is a `[TestClass]` with no test methods yet. `DfDeedleQfcColumnTimeoutTests` additionally carries `[DoNotParallelize]`, because the UtilitiesCS test assembly parallelizes at class level and this class drives process-wide log4net state.
  - Acceptance: all three files exist and each contains the literal `[TestClass]`.

- [ ] [P1-T9] Create `QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs` as a compiling stub in namespace `QuickFiler.Controllers.Tests`.
  - Acceptance: the file exists and contains the literal `[TestClass]`.

- [ ] [P1-T10] Add the four `<Compile Include>` entries for the new test files to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, `TaskMaster.Test/TaskMaster.Test.csproj`, and `QuickFiler.Test/QuickFiler.Test.csproj`.
  - In `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, insert both `    <Compile Include="Extensions\DfDeedleQfcColumnTimeoutTests.cs" />` and `    <Compile Include="Extensions\DfDeedleRequiredColumnValidationTests.cs" />` immediately after the existing line `    <Compile Include="Extensions\DfDeedle_COM_Tests.cs" />` at line 189.
  - In `TaskMaster.Test/TaskMaster.Test.csproj`, insert `    <Compile Include="Ribbon\RibbonCommandBoundaryTests.cs" />` immediately after the existing `<Compile Include>` line at line 323, whose Include attribute is Ribbon\EngineGatedCommandRunnerTests.cs, written as plain prose because that neighbouring file is outside the write set.
  - In `QuickFiler.Test/QuickFiler.Test.csproj`, insert `    <Compile Include="Controllers\QfcDatamodelRethrowTests.cs" />` immediately after the existing `<Compile Include>` line at line 146, whose Include attribute is Controllers\QfcDatamodelTests.cs, written as plain prose because that neighbouring file is outside the write set.
  - Acceptance: each of the four literals above appears exactly once in its project file.

- [ ] [P1-T11] Repair the two reflection invocations in `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` that break against the widened signature.
  - Delete the reflection helper `GetAddQfcColumnsAsyncMethod`, declared at lines 495 to 499, together with the two local bindings that call it at lines 505 and 527; the identifier occurs exactly three times in the file, at 495, 505 and 527. Replace both reflective invocations, at lines 511 to 515 and 534 to 538, with direct calls to the now-`internal` `DfDeedle.AddQfcColumnsAsync`. The assembly already has access through the `InternalsVisibleTo("UtilitiesCS.Test")` declaration at line 19 of the UtilitiesCS assembly-info file.
  - Reflection does not apply C# default parameter values absent `Type.Missing` plus `BindingFlags.OptionalParamBinding`, so a four-element argument array against a six-parameter method throws `TargetParameterCountException`; the direct call is the repair, not a workaround.
  - Acceptance: a search of the file for the literal `GetAddQfcColumnsAsyncMethod` returns zero matches, the file's line count is strictly less than 882, and the two tests `AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing` and `AddQfcColumnsAsync_PreCancelledToken_CompletesGracefully` both pass in P1-T13.

- [ ] [P1-T12] Format the tree with `dotnet tool run csharpier format .` and verify with `dotnet tool run csharpier check .`, recording both in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p1-csharpier.md`.
  - Run `dotnet tool run csharpier format .` over the whole tree. Where P0-T5 recorded `EXIT_CODE: 0` the whole-tree pass is a no-op outside this change's paths. Where P0-T5 recorded a non-zero exit and enumerated pre-existing unformatted files, this task additionally asserts that the set of files rewritten by the format pass is a subset of the union of this change's paths and that enumerated P0-T5 set, observed with git status --porcelain --untracked-files=all -- . ":(exclude).claude" before and after the format pass, and records both observations in the artifact.
  - Acceptance: the artifact records `EXIT_CODE: 0` for the check command and quotes its success-case summary line, which begins with the literal `Checked ` and ends with the literal `ms.`.

- [ ] [P1-T13] Build and run the three affected test assemblies to confirm the seams compile and no existing test regressed, recording the result in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p1-seam-build-and-scoped-tests.md`.
  - Run the analyzer build command, then vstest against `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`, `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` and `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` with `/InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p1-seams /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook"`, extending the filter per the P0-T8 verdict.
  - Acceptance: the artifact records `EXIT_CODE: 0` for the analyzer build with the summary line `0 Error(s)`, and `EXIT_CODE: 0` for the test run with a failed count of 0. A run whose only failure is the issue #780 sporadic `TryAddValuesAsync_UpdatesExistingValue` is rerun rather than accepted, matching the rule P8-T4 applies to the same test; the artifact records each rerun and the count of reruns. Any other failure blocks this task.

---

### Phase 2 — Failing regression tests

Every task in this phase is expected to fail. Each carries an evidence artifact recording the
observed failure so the fail-before condition is auditable.

- [ ] [P2-T1] [expect-fail] Add the AC1 non-overlap test `AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce` to `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs`.
  - Arrange a `FakeTimeProvider`, a never-completing injected `Action<object, object>` adder that increments a counter and blocks on a `ManualResetEventSlim` released in the assert phase, and a fresh `CancellationTokenSource`. Act by invoking `DfDeedle.AddQfcColumnsAsync` with `counter` 0 and driving the three deadlines through the arming barrier described in the next bullet. Assert the counter equals 1.
  - The three deadlines are driven through a deterministic arming barrier, not by three consecutive `Advance` calls. The test class declares a private `TimeProvider` wrapper that holds an inner `FakeTimeProvider`, forwards `GetUtcNow`, `GetTimestamp`, `LocalTimeZone`, `TimestampFrequency` and `CreateTimer` to it unchanged, and, after forwarding `CreateTimer`, completes a `TaskCompletionSource` signalling that the next deadline has been armed. The test awaits that signal, re-arms it, then calls `Advance(TimeSpan.FromMilliseconds(3000))` on the inner provider, and repeats once per deadline. Forwarding rather than intercepting is required so the inner `FakeTimeProvider` still owns and fires the timer. No wall-clock wait, no `Thread.Sleep` and no `Task.Delay` is used, so the determinism rules in the general unit test policy are satisfied. P2-T2, P2-T3 and P2-T4 use this same wrapper and this same barrier; a bare sequence of `Advance` calls is prohibited in all four tasks, because it advances the clock past deadlines the production loop has not yet created and leaves the returned task permanently incomplete.
  - The barrier's gate is released in a `finally`, not in the assert phase. The expected fail-before outcome for this task is an assertion failure, and releasing only on the success path would leave thread-pool threads blocked for the life of the test process; `[DoNotParallelize]` bounds that within the class but not across the assembly.
  - The barrier's completion source is signalled with `TrySetResult`, never `SetResult`. The production loop can arm one more timer than the surrounding prose predicts, and a second `SetResult` on the same instance throws where an unobserved `TrySetResult` is inert.
  - Acceptance: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac1-nonoverlap-fail-before.md` records `ExpectedExitCode: 1`, names the fully-qualified test `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce`, and records that test as failed with an observed adder invocation count of 3.

- [ ] [P2-T2] [expect-fail] Add the AC1 loud-failure test `AddQfcColumnsAsync_ThirdDeadlineExpires_ThrowsNamingFolderAndStep` to `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs`.
  - Arrange as in P2-T1 with a folder mock whose `Name` returns the literal `T&E`. Act by driving the three deadlines through the P2-T1 arming barrier and then awaiting the returned task. Assert the awaited call throws, and that the thrown exception's flattened message contains both the literal `T&E` and the literal `column add`.
  - Acceptance: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac1-loud-failure-fail-before.md` records `ExpectedExitCode: 1`, names the fully-qualified test `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_ThirdDeadlineExpires_ThrowsNamingFolderAndStep`, and records it as failed because the call returned normally instead of throwing.

- [ ] [P2-T3] [expect-fail] Add the three AC1 positive-path tests to `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs`, one per deadline on which the adder completes.
  - The tests are `AddQfcColumnsAsync_AdderCompletesBeforeFirstDeadline_ReturnsWithoutThrowingAndStartsOneTask`, `AddQfcColumnsAsync_AdderCompletesBeforeSecondDeadline_ReturnsWithoutThrowingAndStartsOneTask`, and `AddQfcColumnsAsync_AdderCompletesBeforeThirdDeadline_ReturnsWithoutThrowingAndStartsOneTask`. Each asserts the adder invocation count is 1 and no exception is thrown. Each drives the deadlines through the P2-T1 arming barrier, firing exactly N-1 deadlines before releasing the adder gate for the test whose adder completes before deadline N; for N of 1, no deadline is fired, and the barrier is not awaited, because a task already complete when `TimeoutAfter` is called short-circuits and arms no timer.
  - Acceptance: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac1-positive-paths.md` records `ExpectedExitCode: 1`, names all three fully-qualified tests, and records the observed pass or fail status of each together with the observed adder invocation count for each. The first of the three is expected to pass against the unfixed code because the adder completes before any recursion occurs; the second and third are expected to fail with observed counts of 2 and 3 respectively, because the unfixed loop starts a new `Task.Run` on each retry. The artifact records the observed statuses and counts without prejudging them, and all three tests are required to pass in P3-T6.

- [ ] [P2-T4] Add the AC1 cancellation guard test `AddQfcColumnsAsync_CancellationRequestedMidLoop_DoesNotThrowTimeoutExhausted` to `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs`.
  - Arrange a never-completing adder, drive the first deadline through the P2-T1 arming barrier, and cancel the token after that first advance. Assert the awaited call does not throw a `TimeoutException` whose message contains the literal `column add`, so user cancellation continues to be silent and is not converted into a user-facing dialog.
  - This is a guard test, not a fail-before test: today's code returns normally on cancellation, so it passes before the fix and must still pass after it. Tagging it `[expect-fail]` would be false.
  - Acceptance: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac1-cancellation.md` names the fully-qualified test `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_CancellationRequestedMidLoop_DoesNotThrowTimeoutExhausted` and records it as passed. The same test is required to be passing again in P3-T6.

- [ ] [P2-T5] [expect-fail] Add the AC2 timing tests to `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs`, using the assertion strategy selected by the P0-T13 verdict.
  - When P0-T13 recorded `LOG4NET_CROSS_ASSEMBLY_CAPTURE: CONFIRMED`, add `AddQfcColumns_EmitsDfTimingLineForEachColumnOperation` and `HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration`, each attaching a `MemoryAppender` to the logger named by `typeof(DfDeedle).FullName` and asserting existence of the expected `[Df timing]` lines. Assert existence, never a count: log4net binds one logger per type for the whole process, so a concurrently running test class can add events but can never remove them, which makes the existence claim deterministic and a count assertion order-dependent.
  - Both AC2 tests reach the relocated methods the way the existing COM test class already reaches `AddQfcColumnsAsync`: through `typeof(DfDeedle).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)`, invoked against a Moq-built `Table` and a Moq-built `MAPIFolder` whose `UserDefinedProperties` collection contains a `Triage` entry, so that `EnsureTriageColumnExists` returns true without reaching `MessageBoxInvoker` and all six `Columns` operations execute. Neither `AddQfcColumns` nor `HasUserDefinedProperty` is widened from `private`: spec.md's Boundaries section widens only `AddQfcColumnsAsync`, and widening a second and third member is a design change this item is not scoped for. The mock builders are declared privately in the new test class rather than shared with the existing COM test class, whose own builders are private.
  - When P0-T13 recorded `LOG4NET_CROSS_ASSEMBLY_CAPTURE: NOT AVAILABLE`, do not assert through the injected adder: injecting an adder replaces the whole `AddQfcColumns` body, so the AC2 instrumentation never executes, the adder's own entry-to-exit interval reads identically before and after the fix, and a single delegate cannot observe six per-column intervals. Instead add the same two test names but attach the `MemoryAppender` to the same log4net repository from inside the production assembly's own logger instance, obtained by reflecting the private static `logger` field of `UtilitiesCS.DfDeedle` and calling `Logger.Repository` on it, which removes the cross-assembly default-repository assumption that P0-T13 found unmet. If that also fails, P0-T13 is re-run and its artifact records `LOG4NET_CROSS_ASSEMBLY_CAPTURE: NOT AVAILABLE` together with the second failure, and P2-T5 is recorded as blocked rather than satisfied by a vacuous assertion.
  - Acceptance: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac2-timing-fail-before.md` records `ExpectedExitCode: 1`, names both fully-qualified tests, records both as failed, and states verbatim which of the two P0-T13 verdicts selected the strategy actually implemented.

- [ ] [P2-T6] [expect-fail] Add the AC3 negative tests to `UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs`, one per required key removed in turn.
  - The five tests are `ValidateRequiredEmailColumns_MissingEntryID_Throws`, `ValidateRequiredEmailColumns_MissingMessageClass_Throws`, `ValidateRequiredEmailColumns_MissingSentOn_Throws`, `ValidateRequiredEmailColumns_MissingConversationId_Throws`, and `ValidateRequiredEmailColumns_MissingTriage_Throws`. Each builds a `Dictionary<string, int>` containing the other four keys and asserts the call throws.
  - Acceptance: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac3-negative-fail-before.md` records `ExpectedExitCode: 1`, names all five fully-qualified tests under `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests`, and records all five as failed.

- [ ] [P2-T7] [expect-fail] Add the AC3 message-content, multi-missing, case-variant and positive tests to `UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs`.
  - The four tests are `ValidateRequiredEmailColumns_MissingTwoKeys_MessageNamesBothAndFolder`, `ValidateRequiredEmailColumns_CaseVariantEntryid_ReportsEntryIDMissing`, `ValidateRequiredEmailColumns_AllKeysPresent_DoesNotThrow`, and `ValidateRequiredEmailColumns_MissingSentOn_MessageNamesFolder`. The message-content assertions use the folder-name literal `T&E`. The case-variant test supplies the key `Entryid` and asserts the required key `EntryID` is still reported missing, pinning ordinal comparison; the dictionary produced by the table utility uses the default ordinal comparer and the required names carry an intentional casing asymmetry, capital D in `EntryID` and lowercase d in `ConversationId`.
  - Acceptance: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac3-message-fail-before.md` records `ExpectedExitCode: 1`, names all four fully-qualified tests, and records the three throwing tests as failed and `ValidateRequiredEmailColumns_AllKeysPresent_DoesNotThrow` as passed.

- [ ] [P2-T8] [expect-fail] Add the AC4 stack-preservation test `GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack` to `QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs`.
  - Construct the datamodel with `FormatterServices.GetUninitializedObject`, set the globals field so that the MAPI namespace reports offline and the offline toggle short-circuits without touching the command bars, set `Token` and `TokenSource` through their public setters, then reflection-invoke `GetEmailsInViewDfAsync` with an explorer whose table acquisition throws a sentinel exception. The wrapping described in Correction 2 occurs at the dataframe-transform `TimeoutAfter` call, which is downstream of table acquisition, so a sentinel thrown at acquisition reaches the boundary unwrapped while a sentinel thrown during the dataframe transform reaches it wrapped in an `AggregateException`. The assertion must therefore be shape-agnostic: it walks the thrown exception and every inner exception in turn and asserts that the originating frame appears in at least one of their `StackTrace` values. It must neither assume unwrapping nor assume wrapping.
  - Acceptance: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac4-fail-before.md` records `ExpectedExitCode: 1`, names the fully-qualified test `QuickFiler.Controllers.Tests.QfcDatamodelRethrowTests.GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack`, and records it as failed because the originating frame is absent from the observed stack.

- [ ] [P2-T9] [expect-fail] Add the AC5 boundary behaviour tests to `TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs`.
  - The five tests are `RunAsync_ActionSucceeds_InvokesNeitherSink`, `RunAsync_ActionThrows_InvokesLogSinkOnce`, `RunAsync_ActionThrows_InvokesPresentationSinkOnce`, `RunAsync_ActionThrows_DoesNotPropagateToCaller`, and `RunAsync_PresentationSinkThrows_IsContainedAndDoesNotPropagate`. A sixth test `RunAsync_AggregateException_PresentedMessageIncludesInnerExceptionDetail` asserts the message passed to the presentation sink contains the inner exception's message and not only the literal `One or more errors occurred.`
  - The TaskMaster test assembly does not reference the QuickFiler assembly, so no test in this file may depend on any QuickFiler type.
  - Acceptance: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac5-boundary-fail-before.md` records `ExpectedExitCode: 1`, names all six fully-qualified tests under `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests`, records `RunAsync_ActionSucceeds_InvokesNeitherSink` as passed, and records the other five as failed.

- [ ] [P2-T10] [expect-fail] Add the AC5 handler shape pin `NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary` to `TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs`.
  - Mirror the existing `AssertAwaitedAsyncVoidShape` helper in the sibling ribbon shape-test file: assert each of `QuickFiler_Click`, `QuickFilerHighConfidence_Click` and `SortEmail_Click` returns `void` and carries the compiler-emitted `AsyncStateMachineAttribute`. Additionally assert that the `RibbonViewer` type declares a field whose type is `RibbonCommandBoundary`.
  - Acceptance: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac5-shape-fail-before.md` records `ExpectedExitCode: 1`, names the fully-qualified test `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary`, and records it as failed because no `RibbonCommandBoundary`-typed field exists on `RibbonViewer` yet.

- [ ] [P2-T11] Run the three affected assemblies and record the consolidated fail-before set in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-consolidated-fail-before.md`.
  - Run the analyzer build, then vstest over `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`, `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` and `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` with `/InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p2-failbefore /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook"`, extending the filter per the P0-T8 verdict.
  - Acceptance: the artifact records `ExpectedExitCode: 1`, records `0 Error(s)` from the analyzer build, and enumerates the failing tests by fully-qualified name. The enumerated failing set is designated `BASELINE_FAILURE_SET` and is the only set later phases must turn green; no other test in these three assemblies may be failing at this point apart from the issue #780 sporadic failure, which is recorded separately if seen.

---

### Phase 3 — AC1 timeout hardening and AC2 timing instrumentation

AC1 and AC2 land together in this phase. AC1 converts a silent degradation into a hard, reproducible
launch failure on a slow folder, and only the AC2 timing lines make that difference diagnosable, so
this plan does not permit a state in which AC1 has landed and AC2 has not.

- [ ] [P3-T1] Replace the recursive timeout loop in `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` with a single held task re-deadlined up to three times.
  - Start the work exactly once with `var work = Task.Run(() => adder(table, folder), token);` where `adder` is the injected `columnAdder` or its default, then loop up to three times awaiting `work.TimeoutAfter(3000, timeProvider)` on that same instance. No second `Task.Run` is created on any iteration. The total budget stays at 9000 milliseconds, three deadlines of 3000 milliseconds each, so no folder that succeeds today begins to fail on timing alone.
  - A blocking synchronous COM call cannot be cancelled on .NET Framework: `Task.Run` with a token suppresses only scheduling and cannot interrupt a call already inside the interop marshaller. AC1's clause "the underlying task is cancelled or the retry waits for it" is satisfied by the second alternative.
  - The recursion is removed entirely, so neither of the two recursive call expressions that P1-T3 left in the file survives. The `counter` parameter is retained in the signature and becomes the starting attempt index of the loop, so it does not become an unused parameter and the three-deadline budget is preserved for the existing call site, which passes 0.
  - Acceptance: the file contains the literal `work.TimeoutAfter(3000, timeProvider)`, a search of the file for the literal `counter + 1` returns zero matches where P1-T3 left exactly two, and the test `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce` passes in P3-T6. The recursion is asserted by the `counter + 1` token rather than by the whole call expression, because CSharpier wraps that call past its 100-column print width once the two new arguments are forwarded, so a whole-expression literal matches on no tree.

- [ ] [P3-T2] Add the descriptive timeout-exhausted throw to `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` so the method fails loudly after its final deadline.
  - After the third `TimeoutException`, and only when cancellation has not been requested, throw a `TimeoutException` whose message names the folder and the step. The message is composed from the folder's `Name` and the literal step token `column add`.
  - Acceptance: the file contains the literal `column add`, and the tests `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_ThirdDeadlineExpires_ThrowsNamingFolderAndStep` and `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_CancellationRequestedMidLoop_DoesNotThrowTimeoutExhausted` both pass in P3-T6.

- [ ] [P3-T3] Add the AC2 timing instrumentation around the property enumeration in `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`.
  - Wrap the `folder.UserDefinedProperties` enumeration inside `HasUserDefinedProperty` in a `Stopwatch` and emit through the existing `LogDfTiming(string phase, string? details = null)` helper, which prefixes `[Df timing] `, appends the `threadId=...; syncContext=...` context and emits at `logger.Debug`. Time the enumeration rather than the cheap null guard.
  - Acceptance: the file contains the literal `LogDfTiming(`, and the test `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration` passes in P3-T6.

- [ ] [P3-T4] Add the AC2 timing instrumentation around each of the six column operations in `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`.
  - Time each of the three `Columns.Add` calls and each of the three `Columns.Remove` calls individually, so a single slow column is attributable, and emit each through `LogDfTiming`. The log format, prefix and level are unchanged from the existing helper.
  - Acceptance: the test `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumns_EmitsDfTimingLineForEachColumnOperation` passes in P3-T6, and the file's added timing call sites number exactly six for the column operations, verified by the artifact produced in P3-T6.

- [ ] [P3-T5] Confirm no banned time-abstraction symbol was introduced into `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` and that no `TimeoutAfter` overload was added or edited.
  - A `Task.WhenAny` plus delay implementation of AC1 would fail the analyzer gate: the repository's banned-symbols list bans `Thread.Sleep` and `Task.Delay` in both int and TimeSpan arities with the message directing callers to inject a time abstraction. Re-applying the existing three-argument `TimeoutAfter` overload to a single held task is the only analyzer-clean, deterministically testable shape. The threading helper file that declares the four overloads is 1011 lines, already over the 500-line cap, and is not edited by this change.
  - Acceptance: a search of `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` for the literal `Task.Delay` returns zero matches and a search for the literal `Thread.Sleep` returns zero matches. Running git diff --name-only c431dc32 -- UtilitiesCS/Threading/ produces no output lines, and running git status --porcelain --untracked-files=all -- UtilitiesCS/Threading/ also produces no output lines. The status companion is required and is the load-bearing half at this point in the plan: this task runs before the P7-T1 commit, so a newly created file under that directory would be untracked and a name-listing diff could not report it. Record all four observations in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p3-banned-symbol-and-overload-scope.md`.

- [ ] [P3-T6] Run the analyzer build and the three affected assemblies, recording the AC1 and AC2 green transition in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p3-ac1-ac2-pass-after.md`.
  - Run `dotnet tool run csharpier format .` over the whole tree, then `dotnet tool run csharpier check .`, then the analyzer build, then vstest over the three assemblies with `/InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p3-ac1ac2 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook"`, extending the filter per the P0-T8 verdict.
  - Acceptance: the artifact records the csharpier check success-case summary line, which begins with the literal `Checked ` and ends with the literal `ms.`, `0 Error(s)` from the analyzer build, a pass status for every test in the class `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests`, including the members that were already passing at P2-T3 and P2-T4 and are therefore outside `BASELINE_FAILURE_SET`, so P9-T7's evidence condition is satisfiable from this artifact, and a counted search of `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` for the literal `LogDfTiming(` reporting the observed total together with the breakdown P3-T3 and P3-T4 require: one call site for the property enumeration and exactly six for the three `Columns.Add` and three `Columns.Remove` operations. The AC3, AC4 and AC5 members of `BASELINE_FAILURE_SET` are still expected to fail and are recorded as such; the artifact carries `ExpectedExitCode: 1` for that reason.

---

### Phase 4 — AC3 required-column validation

- [ ] [P4-T1] Implement `ValidateRequiredEmailColumns` in `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` as a pure ordinal validator.
  - The required key set is exactly `EntryID`, `MessageClass`, `SentOn`, `ConversationId`, `Triage`. Compare ordinally. On any missing key throw an exception whose message names every missing key and the folder name.
  - Acceptance: the nine tests in `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests` added by P2-T6 and P2-T7 all pass in P4-T4.

- [ ] [P4-T2] Call the validator from the asynchronous entry point in `UtilitiesCS/Extensions/DfDeedle.cs`.
  - Capture the folder name as a `string` on the STA at the existing acquisition-complete log site near line 158, then call `ValidateRequiredEmailColumns(tableSnapshot.Item2, folderName)` between the table-snapshot log at line 177 and the dataframe-transform stopwatch at line 181. Pass the captured string, never the COM folder object, to the validator, so the folder name is read once on the STA rather than from a thread-pool continuation. The `Task.Run` lambda at lines 186 to 189 is not changed: its argument list stays `(storeID, tableSnapshot.Item1, tableSnapshot.Item2)`, because adding the folder name to it would change the arity of `Email2dArrayToDf`, which AC9 forbids and P4-T3 pins. The capture must yield a non-nullable `string`, because `currentFolder?.Name` at line 158 has maybe-null flow state under the file's `#nullable enable` while the validator parameter is `string`.
  - Acceptance: `UtilitiesCS/Extensions/DfDeedle.cs` contains the literal `ValidateRequiredEmailColumns(tableSnapshot.Item2` exactly once, and the nullable build in P4-T4 reports `0 Error(s)`.

- [ ] [P4-T3] Call the validator from the synchronous entry point in `UtilitiesCS/Extensions/DfDeedle.cs`, closing the duplicate unchecked indexing on that path.
  - Insert the call between the table extraction at line 94 and the frame construction at line 96, with the folder in scope from line 89. This closes the second set of five unchecked dictionary reads that `GetEmailDataFromTable` performs, which the asynchronous path never reaches.
  - The parameter lists of `Email2dToRecords`, `Email2dArrayToDf` and `GetEmailDataFromTable` are not changed: all three are pinned by fixed-arity reflection and direct-call tests, and none has the folder name in scope. AC3's "or its caller" wording is satisfied at the caller.
  - Acceptance: `UtilitiesCS/Extensions/DfDeedle.cs` contains the literal `ValidateRequiredEmailColumns(` exactly twice, and `git diff c431dc32 -- UtilitiesCS/Extensions/DfDeedle.cs` contains no removed line (no line beginning with a single `-`) carrying any of the three identifiers `Email2dToRecords`, `Email2dArrayToDf` or `GetEmailDataFromTable`, which pins all three arities without depending on a declaration fitting on one line. The three declarations are CSharpier-wrapped across a header line and one line per parameter, so a single-line search for a name-plus-first-parameter literal returns zero matches on any tree and gates nothing.

- [ ] [P4-T4] Run the toolchain steps and the three affected assemblies, recording the AC3 green transition in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p4-ac3-pass-after.md`.
  - Run csharpier format then check, the analyzer build, the nullable build, then vstest over the three assemblies with `/InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p4-ac3 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook"`, extending the filter per the P0-T8 verdict.
  - Acceptance: the artifact records the csharpier check summary line, which begins with the literal `Checked ` and ends with the literal `ms.`, `0 Error(s)` for both msbuild commands, a pass status for all nine `DfDeedleRequiredColumnValidationTests` tests, and a pass status for the pre-existing fixed-arity tests `UtilitiesCS.Test.Extensions.DfDeedle_Tests` and `UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests`. The AC4 and AC5 members of `BASELINE_FAILURE_SET` are still expected to fail; the artifact carries `ExpectedExitCode: 1`.

---

### Phase 5 — AC4 stack-preserving rethrow

- [ ] [P5-T1] Change the rethrow at line 108 of `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` from `throw e;` to `throw;`.
  - Acceptance: a search of that file for the literal `throw e;` returns zero matches, and the file still contains the literal `catch (System.Exception e)` so the logging call that uses `e` is preserved.

- [ ] [P5-T2] Change the two identical sibling rethrows at lines 359 and 400 of `QuickFiler/Controllers/QfcDatamodel.cs` from `throw e;` to `throw;`.
  - Line 359 is in `LoadRemainingEmailsToQueueAsync(CancellationToken)` and line 400 is in `LoadRemainingEmailsToQueue(BackgroundWorker, CancellationToken)`; both are in the same `QfcDatamodel` partial family as the AC4 target.
  - Acceptance: a search of that file for the literal `throw e;` returns zero matches, and the file still contains the literal `catch (System.Exception e)` exactly three times, at the two handlers this task edits and at the third, out-of-scope handler later in the file, so the logging calls that consume `e` are all preserved.

- [ ] [P5-T3] Confirm the two out-of-scope `throw e;` occurrences outside the `QfcDatamodel` partial family are unchanged, recording the check in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p5-ac10-out-of-scope-throws.md`.
  - The occurrence in the QuickFiler queue type is in a different type and is out of AC4 scope. The occurrence in the QuickFiler mail helper class is commented out. Neither is touched.
  - Acceptance: a repository-wide search of the QuickFiler project directory for the literal `throw e;` returns exactly two matches, one in QuickFiler/Controllers/QfcQueue.cs and one in QuickFiler/Helper Classes/cInfoMail.cs, the second of which is a commented-out line. Running git diff --name-only c431dc32 -- QuickFiler/ ":(exclude)QuickFiler/Controllers/QfcDatamodel.cs" ":(exclude)QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs" produces no output lines, and running git status --porcelain --untracked-files=all with that same pathspec also produces no output lines. The status companion is the load-bearing half at this point in the plan, which runs before the P7-T1 commit. Record all three observations in the artifact.

- [ ] [P5-T4] Run the toolchain steps and the three affected assemblies, recording the AC4 green transition in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p5-ac4-pass-after.md`.
  - Run csharpier format then check, the analyzer build, the nullable build, then vstest over the three assemblies with `/InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p5-ac4 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook"`, extending the filter per the P0-T8 verdict.
  - Acceptance: the artifact records the csharpier check summary line, which begins with the literal `Checked ` and ends with the literal `ms.`, `0 Error(s)` for both msbuild commands, and a pass status for `QuickFiler.Controllers.Tests.QfcDatamodelRethrowTests.GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack`. The AC5 members of `BASELINE_FAILURE_SET` are still expected to fail; the artifact carries `ExpectedExitCode: 1`.

---

### Phase 6 — AC5 ribbon command boundary

- [ ] [P6-T1] Implement the catching boundary in `TaskMaster/Ribbon/RibbonCommandBoundary.cs`.
  - `RunAsync` awaits the action, catches `System.Exception`, forwards to both injected sinks and never rethrows. A throwing presentation sink is contained by an inner catch that logs through the log sink and does not propagate; a throwing log sink is likewise contained. The type stays free of `[ExcludeFromCodeCoverage]`, following the ratified pattern of the sibling ribbon runner class whose own documentation records that it is deliberately not coverage-exempt because it is host-neutral decision logic while presentation belongs to the coverage-exempt ribbon shim.
  - Acceptance: the five tests `RunAsync_ActionSucceeds_InvokesNeitherSink`, `RunAsync_ActionThrows_InvokesLogSinkOnce`, `RunAsync_ActionThrows_InvokesPresentationSinkOnce`, `RunAsync_ActionThrows_DoesNotPropagateToCaller` and `RunAsync_PresentationSinkThrows_IsContainedAndDoesNotPropagate` under `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests` all pass in P6-T5.

- [ ] [P6-T2] Render inner exception detail in the message that `TaskMaster/Ribbon/RibbonCommandBoundary.cs` passes to the presentation sink.
  - The timeout helper's result marshalling wraps upstream of the AC4 rethrow, so the boundary observes an `AggregateException`. `throw;` restores the original stack but does not unwrap. Without inner-exception rendering the dialog reads only "One or more errors occurred." and carries no actionable content.
  - Acceptance: the test `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_AggregateException_PresentedMessageIncludesInnerExceptionDetail` passes in P6-T5.

- [ ] [P6-T3] Add the boundary field and the failure sink to `TaskMaster/Ribbon/RibbonViewer.cs`, modelled on the existing static failure reporter at lines 311 to 315.
  - Add one `private readonly RibbonCommandBoundary` field initialised in both constructors, and one `private static void ReportRibbonCommandFailure(string commandName, System.Exception exception)` that logs through the established `logger.Error(string, Exception)` shape and presents through `MessageBox.Show`. The repository has no non-modal notice surface; the established mechanism is a logger call plus a message box, and there is no message-box seam in the TaskMaster assembly.
  - Acceptance: the file contains the literal `ReportRibbonCommandFailure` and the literal `RibbonCommandBoundary`, and the type retains both its `[ExcludeFromCodeCoverage]` attribute and its `[System.Runtime.InteropServices.ComVisible(true)]` attribute, verified by the file still containing both literals.

- [ ] [P6-T4] Route the three named handlers in `TaskMaster/Ribbon/RibbonViewer.cs` through the boundary, leaving every other member untouched.
  - `QuickFiler_Click` at line 148, `QuickFilerHighConfidence_Click` at line 153 and `SortEmail_Click` at line 158 each become a one-line `await` of the boundary's `RunAsync`. No handler is renamed, reordered or reformatted. The already-guarded `RunFolderFilterCallback` at line 289 and the remaining 20 out-of-scope `async void` members in this file are unchanged, as are all `async void` members in the sibling ribbon partial, whose awaited tasks are contractually non-faulting.
  - The narrow `catch (OperationCanceledException)` in the QuickFiler home controller is neither widened nor removed, and no existing catch anywhere is broadened to `System.Exception` as an alternative to delivering AC1 or AC3.
  - Acceptance: the file still contains exactly 24 occurrences of the literal `async void`, verified by a counted search recorded in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p6-ac11-handler-inventory.md`, and the test `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary` passes in P6-T5.

- [ ] [P6-T5] Run the toolchain steps and the three affected assemblies, recording the AC5 green transition in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p6-ac5-pass-after.md`.
  - Run csharpier format then check, the analyzer build, the nullable build, then vstest over the three assemblies with `/InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p6-ac5 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook"`, extending the filter per the P0-T8 verdict.
  - Acceptance: the artifact records the csharpier check summary line, which begins with the literal `Checked ` and ends with the literal `ms.`, `0 Error(s)` for both msbuild commands, `EXIT_CODE: 0` for the test run, a pass status for every member of `BASELINE_FAILURE_SET`, and a pass status for every test in the class `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests`, including `RunAsync_ActionSucceeds_InvokesNeitherSink`, which passed at P2-T9 and is therefore outside `BASELINE_FAILURE_SET`, so P9-T5's evidence condition covering all seven AC5 tests is satisfiable from this artifact.

---

### Phase 7 — Scope, size, and inverse-constraint audits

- [ ] [P7-T1] Stage and commit the sixteen write-set paths, including `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` and `TaskMaster/Ribbon/RibbonCommandBoundary.cs`, so the anchored diff gates in this phase have a commit to compare against.
  - Run git add -A -- . ":(exclude).claude" then `git commit`, with the commit message naming issue #798. Both command spans are written in plain prose because the pathspec names a repository path outside the write set.
  - Acceptance: running git status --porcelain --untracked-files=all -- . ":(exclude).claude" produces no output line whose path lies outside `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/`, and `git rev-parse HEAD` differs from `c431dc32`.

- [ ] [P7-T2] Verify the code diff touches exactly the sixteen write-set paths, recording the result in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-write-set-diff.md`.
  - Run git add --intent-to-add -A -- . ":(exclude).claude", then git diff --name-only c431dc32 -- . ":(exclude).claude" ":(exclude)docs", then git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs". All three command spans are written in plain prose because their pathspecs name repository paths outside the write set.
  - The whole-tree csharpier passes mandated by P1-T12, P3-T6, P4-T4, P5-T4, P6-T5, P7-T8 and P8-T1 can add a path to this set only if P0-T5 recorded a non-zero exit and enumerated pre-existing unformatted files. If that happened, the extra paths must be exactly the set P0-T5 enumerated, which is a mechanically derived set and not an executor choice; the artifact then records the observed extra paths against the P0-T5 enumeration and this task is recorded as BLOCKED pending maintainer adjudication, because AC13 fixes the write set at sixteen and this plan may not widen it. If P0-T5 recorded `EXIT_CODE: 0`, no extra path is possible and any extra path is a defect in this change.
  - Acceptance: the diff output set is exactly the sixteen paths enumerated under `## Write Set Under Change`, with no extra path and no missing path, and the porcelain output contains no path outside that set. The artifact lists the observed set verbatim and states the observed count as `16`.

- [ ] [P7-T3] Verify each of the six new `.cs` files — two production and four test — has a `<Compile Include>` entry in its project, recording the result in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-compile-entries.md`.
  - Acceptance: the artifact records one match for each of the six literals `<Compile Include="Extensions\DfDeedle.QfcColumns.cs" />`, `<Compile Include="Ribbon\RibbonCommandBoundary.cs" />`, `<Compile Include="Extensions\DfDeedleQfcColumnTimeoutTests.cs" />`, `<Compile Include="Extensions\DfDeedleRequiredColumnValidationTests.cs" />`, `<Compile Include="Ribbon\RibbonCommandBoundaryTests.cs" />`, `<Compile Include="Controllers\QfcDatamodelRethrowTests.cs" />`, each in its own project file.

- [ ] [P7-T4] Audit the 500-line cap over the eleven write-set `.cs` files, recording the result in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-line-cap.md`.
  - Formatting changes line counts, so P7-T8 re-runs this audit whenever its format pass rewrites a file, and P8-T9 re-verifies it after the final formatting pass. The counts recorded here are those observed at this position in the sequence; this task is not deferred.
  - Acceptance: the artifact records one `path=count` line per file. Every file other than `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` has a count at or below 500. `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` has a count strictly below its base-commit value of 882, and the artifact restates the AC13 condition recorded in P0-T12: the file was already over the cap at the base commit, AC13 requires only that its count strictly decrease, and bringing it under the cap would require an additional file outside the sixteen-path write set. The artifact states that this satisfies AC13 and records no deviation.

- [ ] [P7-T5] Verify the AC12 inverse constraints, recording the result in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac12-inverse-constraints.md`.
  - Acceptance: running git diff --name-only c431dc32 -- QuickFiler/Controllers/QfcHomeController.cs produces no output lines, and running git status --porcelain --untracked-files=all -- QuickFiler/Controllers/QfcHomeController.cs also produces no output lines, so the narrow cancellation catch in the home controller is provably untouched in both the committed and the working state. A search of the four modified production files `UtilitiesCS/Extensions/DfDeedle.cs`, `QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` and `TaskMaster/Ribbon/RibbonViewer.cs` shows the count of `catch (System.Exception` occurrences is not greater than the base-commit count for each file, verified by running git diff c431dc32 -- against each of those four paths in turn, one command per path, written as plain prose, and recorded per file in the artifact.

- [ ] [P7-T6] Verify the AC8 overload constraint, recording the result in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac8-timeoutafter-unchanged.md`.
  - Acceptance: running git diff --name-only c431dc32 -- UtilitiesCS/Threading/ produces no output lines, and running git status --porcelain --untracked-files=all -- UtilitiesCS/Threading/ also produces no output lines; a repository-wide declaration-anchored search over `*.cs` for lines matching `public static .*TimeoutAfter` returns exactly four matches, all in the threading helper file, two of which are the generic `Task<TResult> TimeoutAfter<TResult>(` declarations whose type-parameter list makes a trailing-parenthesis anchor miss them; and a search of the four modified production files and the new production partial for the literals `Task.Delay` and `Thread.Sleep` returns zero matches for each.

- [ ] [P7-T7] Verify the AC9 fixed-arity constraint, recording the result in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac9-fixed-arity.md`.
  - Acceptance: running git diff c431dc32 -- UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs produces no output lines, running git status --porcelain --untracked-files=all against that same path also produces no output lines, and the artifact records a pass status for `UtilitiesCS.Test.Extensions.DfDeedle_Tests.Email2dArrayToDf_ViaReflection_ValidData_ReturnsFrame`, which invokes `Email2dArrayToDf` reflectively with a three-element argument array, and for `UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataFromTable_OneRow_ReturnsFrameWithExpectedFields`, which calls `GetEmailDataFromTable` directly, both read from the P6-T5 test run, which is the last run of the three affected assemblies before this phase. No Phase 8 task may be the evidence source for a Phase 7 gate, and P8-T3 in particular is the nullable build and produces no test result.

- [ ] [P7-T8] Run a final csharpier format-then-check pass over this change's paths and record it in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-csharpier-final.md`.
  - Run `dotnet tool run csharpier format .` over the whole tree, then `dotnet tool run csharpier check .`, and record a git status --porcelain --untracked-files=all -- . ":(exclude).claude" observation taken before and after the format pass.
  - Acceptance: the artifact records `EXIT_CODE: 0` for the check command and quotes its success-case summary line, which begins with the literal `Checked ` and ends with the literal `ms.`. If the format command rewrote any file, P7-T4 is re-run afterwards.

---

### Phase 8 — Final QC toolchain loop and coverage delta

The four toolchain steps run in order in this phase. If any step fails or rewrites a file, the loop
restarts at P8-T1.

- [ ] [P8-T1] Run the formatting step and record it in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/final-csharpier.md`.
  - Run `dotnet tool run csharpier format .` over the whole tree, then `dotnet tool run csharpier check .`, and record a git status --porcelain --untracked-files=all -- . ":(exclude).claude" observation taken before and after the format pass.
  - Acceptance: the artifact records `EXIT_CODE: 0` for the check command and quotes its success-case summary line, which begins with the literal `Checked ` and ends with the literal `ms.`. The artifact additionally states whether the format command rewrote any file, as a before-and-after observation of the tree.

- [ ] [P8-T2] Run the analyzer step over the solution file and record it in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/final-msbuild-analyzers.md`.
  - Run msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true, written as plain prose because the solution file is outside the write set.
  - Acceptance: the artifact records `EXIT_CODE: 0` and quotes the summary line `0 Error(s)`. The warning count is recorded and compared against the P0-T6 baseline warning count; an increase is enumerated by diagnostic id in the artifact.

- [ ] [P8-T3] Run the type-check step over the solution file and record it in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/final-msbuild-nullable.md`.
  - Run msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true, written as plain prose because the solution file is outside the write set.
  - Acceptance: the artifact records `EXIT_CODE: 0` and quotes the summary line `0 Error(s)`. The artifact states explicitly that `/p:Nullable=enable` was not supplied, matching CI.

- [ ] [P8-T4] Run the full-suite test step with coverage and write the Cobertura document to `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml`.
  - Discover assemblies exactly as in P0-T9, then run dotnet-coverage collect --output coverage\final.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest> <assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p8-final /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook", written as plain prose because the settings file and the runsettings file are repository paths outside the write set, extending the filter per the P0-T8 verdict, then copy the result to the evidence path above.
  - Acceptance: the evidence Cobertura file exists and is tracked after `git add --intent-to-add`, and `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/final-vstest-coverage.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` containing the total, passed, failed and skipped counts with a failed count of 0, and the document-level `line-rate` value as a decimal fraction. A failure of the issue #780 sporadic `TryAddValuesAsync_UpdatesExistingValue` test requires a rerun of this task rather than acceptance.

- [ ] [P8-T5] Extract post-change per-file coverage for the six production write-set paths into `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/per-file-coverage-final.md`.
  - Use the same aggregation rule as P0-T10: match `class` elements by `filename` suffix and count only direct-child `lines/line` elements, never the copies nested under a `method` element. The `filename` attribute is repository-relative and uses backslash separators, so the six suffixes matched are `Extensions\DfDeedle.cs`, `Extensions\DfDeedle.QfcColumns.cs`, `Controllers\QfcDatamodel.FrameBuilding.cs`, `Controllers\QfcDatamodel.cs`, `Ribbon\RibbonCommandBoundary.cs` and `Ribbon\RibbonViewer.cs`. A forward-slash suffix matches no element in this document, records every path as `NOT INSTRUMENTED`, and makes the first two clauses of P8-T6 fail against a correct change.
  - Acceptance: the artifact records a `covered=` integer, a `valid=` integer and a `rate=` decimal fraction for each of the six production paths, and records `NOT INSTRUMENTED` for any path producing no matching class element.

- [ ] [P8-T6] Verify the change-scoped coverage obligations against the baseline, recording the comparison in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-delta.md`.
  - The artifact reports, in this order: the P0-T9 baseline document-level line rate, the P8-T4 post-change document-level line rate, the per-file rate for each new module, and the covered-line comparison for each existing production file.
  - Acceptance, all four clauses required:
    - `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` has `rate=` at or above 0.90.
    - `TaskMaster/Ribbon/RibbonCommandBoundary.cs` has `rate=` at or above 0.90.
    - For each of `QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` and `TaskMaster/Ribbon/RibbonViewer.cs`, the post-change `covered=` value is at or above the P0-T10 baseline `covered=` value for the same path. All three are `[ExcludeFromCodeCoverage]` at the type level — `RibbonViewer` on its own declaration, and both `QfcDatamodel` partials through the single class-level attribute on the `QfcDatamodel` declaration — so all three are expected to be recorded as `NOT INSTRUMENTED` on both sides. When both sides are `NOT INSTRUMENTED` the artifact records `NOT APPLICABLE` for that path rather than a numeric comparison. A `NOT INSTRUMENTED` reading for any of the three is the expected outcome and is not a defect.
    - The sum of the post-change `covered=` values for `UtilitiesCS/Extensions/DfDeedle.cs` and `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` is at or above the P0-T10 baseline `covered=` value for `UtilitiesCS/Extensions/DfDeedle.cs`. This relocation adjustment is required because the source file loses lines to the new partial, so a per-file comparison on it alone would compare different denominators.
  - The artifact additionally states, as a record-and-report obligation and not a blocking gate, the direction of movement of the repository-wide figure, together with the reason no repository-wide floor is asserted for this item: no merge-base repository baseline exists in this feature folder, the repository floor applies to the testable denominator after the COM, VSTO and WinForms exemptions, and this pipeline's repository-wide line rate is not reproducible run-to-run on an identical tree.

- [ ] [P8-T7] Close any coverage gap identified by P8-T6 by extending the owning test files, then re-run P8-T4, P8-T5 and P8-T6.
  - Gaps in `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` are closed in `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` or `UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs`. Gaps in `TaskMaster/Ribbon/RibbonCommandBoundary.cs` are closed in `TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs`. The QuickFiler datamodel files are `[ExcludeFromCodeCoverage]`, so no per-file coverage gap can arise in them and none is closed; `QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs` is extended only if a behavioural gap is identified. No file outside the write set is added.
  - Acceptance: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-delta.md` in its final state satisfies all four clauses of P8-T6, and the artifact records whether any gap-closure test was added and, if so, names each added test by fully-qualified name. If P8-T6 already satisfied all four clauses, the artifact records `GAP CLOSURE: NOT REQUIRED`.

- [ ] [P8-T8] Confirm the loop completed clean in a single pass and record the confirmation in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/final-toolchain-clean-pass.md`.
  - Acceptance: the artifact names the four commands in order, records `EXIT_CODE: 0` for each, records that no step rewrote a file during the final pass, and records the pass number of the final pass. If P8-T7 added tests, the four steps are re-run in order after that task and only the final clean pass is recorded here.

- [ ] [P8-T9] Re-verify the write-set diff and the line cap after the final formatting pass, updating `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-write-set-diff.md` and `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-line-cap.md`.
  - Acceptance: both artifacts carry a second `Timestamp:` block recording the post-final-pass observation, the write-set count is still `16`, and every file other than `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` is at or below 500 lines.

---

### Phase 9 — Acceptance-criteria check-off, documentation, and handoff

Each acceptance criterion is checked off in its own task against named evidence. A task may not be
checked off before its named evidence artifact exists and satisfies the stated condition.

- [ ] [P9-T1] Check off AC1 in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`.
  - Evidence: `evidence/regression-testing/p2-ac1-nonoverlap-fail-before.md`, `evidence/regression-testing/p2-ac1-loud-failure-fail-before.md`, `evidence/regression-testing/p3-ac1-ac2-pass-after.md`.
  - Acceptance: the AC1 checkbox in spec.md is `[x]`, and the three artifacts show the two named AC1 tests failing before P3-T1 and P3-T2 and passing after.

- [ ] [P9-T2] Check off AC2 in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`.
  - Evidence: `evidence/regression-testing/p2-ac2-timing-fail-before.md`, `evidence/regression-testing/p3-ac1-ac2-pass-after.md`.
  - Acceptance: the AC2 checkbox in spec.md is `[x]`, and the artifacts show both named AC2 tests failing before P3-T3 and P3-T4 and passing after.

- [ ] [P9-T3] Check off AC3 in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`.
  - Evidence: `evidence/regression-testing/p2-ac3-negative-fail-before.md`, `evidence/regression-testing/p2-ac3-message-fail-before.md`, `evidence/regression-testing/p4-ac3-pass-after.md`.
  - Acceptance: the AC3 checkbox in spec.md is `[x]`, and the artifacts show the nine named AC3 tests reaching a pass status in P4-T4.

- [ ] [P9-T4] Check off AC4 in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`.
  - Evidence: `evidence/regression-testing/p2-ac4-fail-before.md`, `evidence/regression-testing/p5-ac4-pass-after.md`.
  - Acceptance: the AC4 checkbox in spec.md is `[x]`, and the artifacts show the named AC4 test failing before P5-T1 and passing after.

- [ ] [P9-T5] Check off AC5 in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`.
  - Evidence: `evidence/regression-testing/p2-ac5-boundary-fail-before.md`, `evidence/regression-testing/p2-ac5-shape-fail-before.md`, `evidence/regression-testing/p6-ac5-pass-after.md`.
  - Acceptance: the AC5 checkbox in spec.md is `[x]`, and the artifacts show the seven named AC5 tests reaching a pass status in P6-T5.

- [ ] [P9-T6] Record the AC6 manual verification handoff in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/other/ac6-manual-verification-handoff.md` and leave the AC6 checkbox in spec.md unchecked.
  - The artifact states the manual steps: load the debug build, launch QuickFiler on the reproduction folder, confirm either a successful launch or an error dialog naming the folder and the failing step with no unhandled Outlook exception; launch on Inbox as a regression check; and in both cases confirm the debug log contains the per-step column-add timing lines.
  - Acceptance: the artifact exists and the AC6 line in spec.md is byte-identical to its base-commit text, still `[ ]`, with no note added beside it. The pending status is recorded in this task's artifact and in the P9-T15 status summary instead, because spec.md's Authority blockquote forbids rewording AC1 through AC6 and the acceptance-criteria-tracking skill permits changing only `- [ ]` to `- [x]`. AC6 is not checked off by this plan.

- [ ] [P9-T7] Check off AC7 in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`.
  - Evidence: `evidence/regression-testing/p3-ac1-ac2-pass-after.md`.
  - Acceptance: the AC7 checkbox in spec.md is `[x]`, and the artifact shows `AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce` and the three positive-path tests all passing.

- [ ] [P9-T8] Check off AC8 in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`.
  - Evidence: `evidence/qa-gates/p3-banned-symbol-and-overload-scope.md`, `evidence/qa-gates/p7-ac8-timeoutafter-unchanged.md`, `evidence/qa-gates/final-msbuild-analyzers.md`.
  - Acceptance: the AC8 checkbox in spec.md is `[x]`, and the artifacts show exactly four `TimeoutAfter` declarations, no diff under the threading directory, no banned symbol in the changed production files, and `0 Error(s)` from the analyzer build.

- [ ] [P9-T9] Check off AC9 in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`.
  - Evidence: `evidence/qa-gates/p7-ac9-fixed-arity.md`, `evidence/regression-testing/p4-ac3-pass-after.md`.
  - Acceptance: the AC9 checkbox in spec.md is `[x]`, and the artifacts show two validator call sites in the source file, unchanged arities for the three pinned methods, and a pass status for the fixed-arity tests.

- [ ] [P9-T10] Check off AC10 in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`.
  - Evidence: `evidence/qa-gates/p5-ac10-out-of-scope-throws.md`.
  - Acceptance: the AC10 checkbox in spec.md is `[x]`, and the artifact shows zero `throw e;` occurrences in the two `QfcDatamodel` partial files and exactly two remaining occurrences elsewhere in the QuickFiler project, both untouched by the diff.

- [ ] [P9-T11] Check off AC11 in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`.
  - Evidence: `evidence/qa-gates/p6-ac11-handler-inventory.md`, `evidence/regression-testing/p6-ac5-pass-after.md`.
  - Acceptance: the AC11 checkbox in spec.md is `[x]`, and the artifacts show 24 `async void` members still present in the ribbon viewer file, a diff confined to the three named handlers plus one field and one sink, no `[ExcludeFromCodeCoverage]` on the boundary type, and a pass status for the inner-exception rendering test.

- [ ] [P9-T12] Check off AC12 in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`.
  - Evidence: `evidence/qa-gates/p7-ac12-inverse-constraints.md`.
  - Acceptance: the AC12 checkbox in spec.md is `[x]`, and the artifact shows an empty diff for the QuickFiler home controller file and no increase in `catch (System.Exception` occurrences in any modified production file.

- [ ] [P9-T13] Check off AC13 in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`.
  - Evidence: `evidence/qa-gates/p7-ac13-write-set-diff.md`, `evidence/qa-gates/p7-ac13-compile-entries.md`, `evidence/qa-gates/p7-ac13-line-cap.md`, `evidence/baseline/line-cap-preexisting.md`.
  - Acceptance: the AC13 checkbox in spec.md is `[x]`, the diff artifact records exactly 16 paths, the compile-entry artifact records all six entries, the line-cap artifact records every file at or below 500 lines except `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`, that file's count is strictly below 882 as AC13 requires for a file already over the cap at base, and P9-T16 has recorded the pre-existing violation as the third follow-up promotion that AC13's final clause requires. No deviation note is added beside the AC13 checkbox: the change satisfies the criterion as written.

- [ ] [P9-T14] Check off AC14 in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`.
  - Evidence: `evidence/qa-gates/final-toolchain-clean-pass.md`, `evidence/baseline/log4net-capture-probe.md`, `evidence/qa-gates/coverage-delta.md`.
  - Acceptance: the AC14 checkbox in spec.md is `[x]`, the clean-pass artifact records `EXIT_CODE: 0` for all four commands in order in a single pass, and the log-capture artifact records one of the two verdict lines together with the AC2 strategy actually implemented.

- [ ] [P9-T15] Write the acceptance-criteria status summary to `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/issue-updates/ac-status-summary.md`.
  - Acceptance: the artifact lists all 14 criteria with a status of `MET` or `PENDING MANUAL`, names the evidence artifact for each, and records AC6 as `PENDING MANUAL`.

- [ ] [P9-T16] Record the three follow-up promotions in `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/other/followup-promotions.md`, matching the three enumerated under Rollout & Follow-up in spec.md.
  - The three findings are: the unreachable `catch (TimeoutException)` in the two `repeatAttempts` timeout overloads, whose documented retry never executes because the wrapped call returns a proxy that faults later and never throws synchronously; the unguarded shared-static message-box seam mutation in the existing COM test class under class-level parallelization; and the pre-existing 500-line-cap violation in that same COM test class, which stands at 882 lines at the base commit and is reduced but not brought under the cap by this change.
  - Acceptance: the artifact records all three findings with their locations, records the observed post-change line count of the COM test class for the third, and states that each is to be promoted through the potential-to-issue lifecycle so none is lost when this feature folder is archived. If the promotion route is unavailable in this session, the artifact records that fact explicitly rather than omitting the item. The third entry is required by the final clause of AC13 and P9-T13 may not be checked off until it is present.

- [ ] [P9-T17] Update `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md` status metadata and record the observed final line count of the COM test file.
  - Acceptance: the spec Status field reads `Implemented`, the Last Updated field carries the execution timestamp, and the Risks section item covering the reflection-test repair is annotated with the observed post-change line count of the COM test file.

- [ ] [P9-T18] Commit every artifact under `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/` together with the updated `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md` and confirm the worktree is clean.
  - Run git add -A -- . ":(exclude).claude" then `git commit`, with the commit message naming issue #798. Both command spans are written in plain prose because the pathspec names a repository path outside the write set.
  - Acceptance: running git status --porcelain --untracked-files=all -- . ":(exclude).claude" produces no output lines, and `git ls-files --error-unmatch` succeeds for `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/coverage-baseline.cobertura.xml` and for `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml`. Both files are excluded from CSharpier by the repository's formatter ignore rules for evidence directories and Cobertura documents, so their presence does not affect the format gate.
