# Atomic Plan — Folder Settings never persist; User Email shows "Error Loading" (Issue #797)

- Issue: #797
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/797
- Branch: bug/folder-settings-never-persist-797
- Base commit: c431dc3297e864041d829e8d79b348960b8d8019
- Work Mode: full-bug
- Owner: drmoisan
- Last Updated: 2026-09-06T22-00
- Status: Draft — pending atomic-executor preflight
- Version: 1.3 (preflight revision rounds 1, 2 and 3 applied; same file, no sibling plan created)
- Requirements source (authoritative): spec.md in this feature folder, section `## Acceptance Criteria`
- Supporting sources: issue.md and research/research-folder-settings-persistence.md in this feature folder

Work mode is full-bug, so spec.md is the single authoritative acceptance-criteria source. This plan
does not introduce, renumber, reword, merge or split any criterion. This is not a minimal-audit plan;
the three-phase minimal-audit contract does not apply.

---

## Formatting convention inherited from spec.md — do not "fix" it

A downstream scheduler harvests backtick-delimited path tokens to derive the change footprint for a
parallel run against three concurrent sibling work items. This plan therefore reproduces spec.md's
discipline exactly: every file this change creates or modifies is backticked exactly once, inside the
`## Write Set` section below, and nowhere else. Every other file citation in this document is written
as plain prose or inside a fenced command block, without inline backticks, on purpose. Adding an
inline backtick to a citation outside the Write Set would inject a false write claim and needlessly
serialize the run; removing one inside the Write Set would drop a real file from the footprint.

Two consequences follow, and both are deliberate:

1. Task lines cite paths with forward slashes and no backticks. Forward slashes are accepted by
   PowerShell, by MSBuild and by git, and a path carrying no backslash cannot be silently corrupted
   by a doubled-backslash collapse in any tool that transports it.
2. Non-path inline code spans (identifiers, test names, literal strings, quoted runtime values) are
   permitted, because they are not path tokens. spec.md's own verbatim acceptance-criteria block
   contains such spans for the same reason.

---

## Write Set

Every file this change creates or modifies appears below as a concrete repository-relative path
inside backticks. This is the only section of this document containing backticked paths. It
reproduces spec.md's `## Write Set` section without addition or removal.

### Production — modify

- `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs`
- `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs`
- `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`

### Production — create

- `UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs`

### Tests — modify

- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs`
- `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs`

### Tests — create

- `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs`

### Project compile-entry carriers — modify

Every project in this solution is non-SDK-style, verified against this worktree: each project file
opens with a ToolsVersion attribute and the 2003 MSBuild namespace and closes with an import of the
C# targets, and no project file carries an Sdk attribute. A newly created C# file is therefore not
picked up by a wildcard and must be registered by a hand-added compile entry.

- `UtilitiesCS/UtilitiesCS.csproj`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- `TaskMaster.Test/TaskMaster.Test.csproj`

The third entry is retained as a write claim to keep the parallel run schedule-safe. It is required
only if the AC1 tests are placed in a new file under the TaskMaster test project's AppGlobals
directory. This plan directs the executor to append the AC1 tests to the already-registered
AppOlObjectsCoverageTests.cs file, which is 347 lines and has ample headroom, so that project file is
expected to end the change unmodified. Claiming it unconditionally is the conservative choice, and
every scope gate in Phase 5 is written as a subset test rather than an equality test so that an
unmodified claimed file does not fail the gate.

### Requirements document — modify

- `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md`

This file is modified for acceptance-criteria check-off only, mirroring the check-offs made in
spec.md. No criterion text is altered.

### Files written but excluded from the Write Set by spec.md's stated convention

Stated in plain prose without backticks so that no false write claim is injected. This change also
writes, inside this feature folder only: the specification document spec.md, for acceptance-criteria
check-off only; this plan file, for task check-off only; and the timestamp-named evidence artifacts
under the feature folder's evidence directory. spec.md excludes itself and the timestamp-named
research and evidence artifacts from the Write Set by convention, and this plan reproduces that
convention. These paths carry no scheduling risk, because each concurrent sibling work item writes
into its own feature folder.

### Files not written, stated in plain prose

The TaskMaster production project file, TaskMaster dot c-s-p-r-o-j, needs no compile-entry change,
because the two files that receive the AC1 and AC5 production edits are already registered in it. The
Visual Studio solution file, TaskMaster dot s-l-n at the repository root, is not modified, because
every project that receives a new source file already exists in the solution and only its own project
file changes. No repository-root build property file is modified.

One correction to spec.md is recorded here, because spec.md states that no Directory dot Build dot
props file and no Directory dot Build dot targets file exists anywhere in this repository. Both files
do exist at the repository root as of the base commit; Directory dot Build dot props sets a single
property, RxUseUnsupportedPackagesConfig, under issue #730. The scope constraint is unaffected and
strengthened: neither file is created, modified, or otherwise involved by this change. No file at the
repository root is written by this change.

No file with an extension of r-e-s-x, config, props or targets is created or modified. The resource
entry that defines the settings file name and its AppData special folder already carries the correct
values, per D7.

### Explicit scope constraints

- Nothing under the dot-claude, dot-codex or dot-agents trees is edited.
- Neither published JSON file under the config directory is edited.
- No GitHub workflow file is edited.
- The footprint stays inside the Write Set above, plus the feature-folder documents named in the
  preceding subsection.

---

## Acceptance Criteria (reproduced verbatim from spec.md)

The eight criteria below are reproduced verbatim from spec.md. They are the same criteria, not
additional ones. Check-off is performed in spec.md and mirrored into issue.md, one criterion per
task, in Phase 6. The acceptance-criteria-tracking skill's one-at-a-time and evidence-before-check-off
rules are satisfied by Phase 6's structure: one task per criterion, each gated on a named evidence
artifact, each asserting that no other checkbox changes state. The skill's timing guidance is met by
that gating rather than by interleaving, because every criterion's verifying artifact exists before
its check-off task runs and no check-off is batched.

AC1: When `StoresWrapper.json` is absent, the fresh-build path adopts the resource-defined disk configuration so `Config.Disk.FilePath` resolves to `%LocalAppData%\TaskMaster\StoresWrapper.json`, and the first Save creates the file.

AC2: `SmartSerializable<T>.Serialize()` logs an error (not a silent return) when invoked with an empty or null `Config.Disk.FilePath`.

AC3: A value saved in Folder Settings is present after an Outlook restart (manual verification).

AC4: An explicit Save is not lost if Outlook closes within the 3-second deferred-write window (flush on save or on shutdown).

AC5: The junk-folder double-persistence path is either removed or made to fail loudly; the reflection lookup is replaced by a typed seam.

AC6: User Email shows the SMTP address; on lookup failure it shows a specific message including the reason, falls back to an alternative source (the account SMTP address or the store display name when it is an SMTP address), and the lookup is retried when the dialog opens.

AC7: Inbox and Root Folder are displayed without the leading `\\` (cosmetic).

AC8: A null `Current` store selection renders the placeholder text instead of throwing.

### Root-cause traceability

Root cause 1 is the bootstrap gap between the loader and the serializer. It carries AC1, AC2, AC3,
AC4 and AC5. Its automatable implementation is Phase 2 (AC1, AC2, AC4) and the AC5 portion in Phase 4.
Root cause 2 is the unretried COM failure in the Exchange SMTP lookup. It carries AC6 alone and its
implementation is Phase 3. The two root causes are kept in separate phases and separate evidence
artifacts so that a failure is attributable to exactly one of them. AC7 and AC8 are adjacent defects
in the same rendering method and land in Phase 4.

### AC3 is manual and is not automated

AC3 requires an Outlook restart and is not automatable in this environment. No automated gate in this
plan claims to prove it. AC3 is verified by the written manual procedure in Phase 6, its result is
recorded in a manual-verification evidence artifact, and its fail-before requirement is discharged by
a fail-before exception dossier authored in Phase 1, per the evidence-and-timestamp-conventions
skill. Every other criterion has a real automated test.

---

## Settled design decisions (D1 through D7, not reopened)

These are recorded in spec.md as the chosen approach and are not re-opened by this plan.

D1. AC1 is fixed only in the TaskMaster store-loading globals partial, by applying the already-in-scope
loader configuration to the freshly built wrapper. The shared deserialize overload is not changed; its
null return is a load-bearing fail-soft contract for the folder-predictor load path.

D2. AC5 introduces a new dedicated interface for the junk-folder sink in the UtilitiesCS interfaces
tree, implemented explicitly by the TaskMaster junk-folders globals partial, replacing the reflection
lookup with a typed cast that logs an error when the cast fails. The member is deliberately not added
to the existing IOlObjects interface.

D3. AC4 is satisfied by a synchronous flush on the explicit Save path. The deferred three-second
behaviour for all other callers is unchanged. The VSTO add-in lifecycle file is not modified.

D4. The store wrapper controller is 478 lines against a 500-line cap and four criteria land in it, so
it is split into a new display partial and the class declaration gains the partial keyword.

D5. The serializer file is already 613 lines, over the same cap, before any change. It is not split.
This is a pre-existing condition this change does not resolve. No task in this plan splits it.

D6. The existing test named `PopulateWithCurrent_NullCurrent_SetsErrorLoadingText` asserts that a null
current store throws a `NullReferenceException`, contradicting its own name. AC8 changes that
behaviour, so this plan includes an explicit task to invert that test, declared as a deliberate
test-expectation change.

D7. No resource file change is required.

### One recorded reading of D4, not a re-opening

D4 describes the AC7 trim helper as a small pure private static method on the display partial. The
criterion-to-evidence map in spec.md requires direct pure-function tests over that helper across five
cases. A `private` member is not reachable from the test assembly, so this plan specifies `internal
static`. UtilitiesCS already grants `InternalsVisibleTo("UtilitiesCS.Test")` in its assembly
information file, and `internal` does not widen the public surface of the type, so both stated intents
are satisfied. This is a mechanical accessibility reading, not a change of approach.

---

## Plan-wide execution rules

These rules apply to every task and are restated here so the executor does not have to infer them.

### R1 — no shell variable survives between tasks

Every fenced block runs in its own shell. Re-bind any value the block needs at the top of that block.
In particular, re-derive the base SHA from the Phase 0 artifact rather than pasting a literal:

```powershell
$BaseSha = (Select-String -Path 'docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-base-sha.2026-09-06T22-00.md' -Pattern '^BASE-SHA: ([0-9a-f]{40})$').Matches[0].Groups[1].Value
```

An unbound base SHA silently degrades an anchored `git diff` into the ref-less form, which passes
vacuously once the change is committed. Every `git diff` in this plan carries an explicit ref operand.

### R2 — one fixed helper script path

Snippets that need a backslash-bearing literal, XML parsing, or vswhere resolution are authored into
one fixed helper at coverage/plan797-helpers.ps1 and invoked with pwsh and the File switch. That path
is inside a git-ignored directory, so it never appears in a porcelain or diff scope gate. The helper
builds its own dotnet-coverage argument list rather than calling the repository coverage runner script
end to end. Three properties of that script make it unusable here, each verified against this worktree
at the base commit: it discovers every test assembly whose name ends in the test dll suffix under its
search root and offers no way to restrict the run to two assemblies; the argument list it builds pins
the vstest test-case filter to the live-Outlook category exclusion alone, so the four shell-icon
exclusions rule R6 requires cannot be added; and it asserts an 80 percent document-level line rate and
throws below it, which would convert a recording step into a gate. The helper therefore reuses that
script's shape — dotnet-coverage collect, cobertura output format, the off-root CLI runsettings, the
isolation switch — while supplying its own assembly list, its own combined test-case filter and its own
output path. It does not pass the repository coverage settings file unmodified. That file excludes
third-party modules only; the repository runner derives a settings document in memory that adds one
further module exclusion matching any module name ending in dot Test dot dll. The helper performs the
same derivation and passes the derived document, so both test assemblies are excluded from
instrumentation and the denominator holds production code only. The helper performs no other
post-processing, so both the Phase 0 and the Phase 5 documents retain any third-party module the
settings did not exclude; they are produced identically and are therefore comparable with each other,
and are not comparable with a document produced by the repository runner. No repository script under
the vscode scripts directory is modified.

The helper is a session-scoped throwaway: it is created in Phase 0, rewritten in place as later tasks
require, and deleted in P6-T11 before the commit, so it satisfies the general code change policy's
exemption for a script created and deleted within an agent session. Because it is never committed and
is not a repository deliverable, it is not a production PowerShell file for the purposes of the
PowerShell change budget, the PowerShell testing standards or the coverage denominator, and no Pester
test is authored for it. It consumes one of the three per-session production PowerShell slots the
repository's batch-budget hook enforces, and it is the only PowerShell file this plan creates.

### R3 — toolchain order and the two mandatory constraints

The C# toolchain runs in exactly this order, and the loop restarts from step 1 if any step fails or
changes files:

```text
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /EnableCodeCoverage
```

Always the Rebuild target, never Build. MSBuild's up-to-date check does not invalidate on a
command-line property change, so a warm Build returns exit 0 with the compile target skipped and the
analyzer gate becomes vacuous. Never add a solution-wide nullable enable property to the third
command: it is deliberately absent from CI, no project in this repository carries a nullable element,
and forcing it conscripts every file that has never adopted the per-file pragma. A `dotnet tool
restore` is required once per worktree before the first csharpier invocation.

Neither msbuild nor vstest.console.exe is on PATH in a plain shell in this worktree. Both are resolved
through vswhere before use, by the same mechanism the repository's own build and test scripts under
the vscode scripts directory already use: query the Visual Studio installer's vswhere executable for
the latest installation and take the MSBuild and Test Platform executables it reports. Every msbuild
line written in this plan is shorthand for an invocation of the resolved absolute MSBuild path with
the arguments exactly as written.

### R4 — how a build gate discriminates pass from fail

A successful msbuild run prints the substring "error" many times in ordinary output, so a grep for
"error" cannot discriminate pass from fail. Every msbuild acceptance condition in this plan asserts
two things: the process exit code, and the presence of the MSBuild summary count line whose text is
`    0 Error(s)`. When a baseline run is not clean, the acceptance condition instead asserts that the
recorded set of compiler diagnostic identifiers (the `CS` and `CA` codes) is a subset of the Phase 0
baseline set and contains no diagnostic attributed to a Write Set file. Both branches are stated on
every build task so that neither is left unmeasured.

### R5 — how a write-mode formatter gate discriminates pass from fail

The csharpier format subcommand rewrites files and still exits 0, and its summary line reports the
number of files processed rather than the number rewritten, so neither the exit code nor that line
distinguishes a clean run from a repairing one. Every csharpier format task in this plan is paired
with a before-and-after tree observation using `git status --porcelain --untracked-files=all` scoped
to C# sources, and with the read-only check subcommand whose exit code is a real signal.

### R6 — local vstest invocation

Test runs use explicit assembly paths, never directory discovery, so no assembly from a sibling
worktree can be discovered. Every run additionally passes the /InIsolation switch that CI uses.
Four shell-icon test classes in the UtilitiesCS test project stall vstest on this workstation for
environmental reasons unrelated to this change: `HelperClasses.ShellUtilities_Tests`,
`HelperClasses.ShellUtilitiesStatic_Tests`, `HelperClasses.SysImageListHelperTests` and
`EmailIntelligence.OSBrowser_Tests`. Every run in this plan excludes them by
`FullyQualifiedName!~` clauses and repeats the `TestCategory!=LiveOutlook` clause on every disjunct
where a disjunction is used, because `&` binds tighter than `|` in a vstest filter expression. The
four shell-icon exclusion clauses are themselves conjunctive, so in a disjunctive filter they bind to
the first disjunct alone and do not constrain the remaining disjuncts. That is inert here, because no
shell-icon test name matches any selector used in the scoped runs, so no disjunct other than the first
could select one; the exclusions are load-bearing only for the unfiltered whole-assembly runs, whose
filter expression carries no disjunction. The filter text recorded verbatim in each evidence artifact
should be read with that binding in mind. CI covers the four excluded classes; their exclusion is
recorded in every test evidence artifact.

### R7 — evidence artifacts

Every evidence artifact path resolves under
docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/
in one of the canonical kind subdirectories: baseline, regression-testing, qa-gates, issue-updates,
other. No evidence is written under an artifacts directory. Every command-step artifact carries the
fields `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. An artifact for a step whose acceptance
admits a non-zero outcome additionally carries `ExpectedExitCode:` with a concrete integer value, one
gate per artifact file. The steps that admit either 0 or 1 depending on the recorded baseline are
P0-T9, P0-T10, P1-T3, P5-T5 and P5-T6, and each carries the field unconditionally with the value its
own run produced; an explicit `ExpectedExitCode: 0` is equivalent to omitting the field and keeps the
row a pass. P5-T2 is a further step that admits a non-zero outcome, on the branch where the P0-T6
artifact recorded pre-existing format drift; that outcome is the formatter check's own unformatted-file
code rather than a test-failure code, so it is stated separately from the enumeration above and is
carried under the general rule in the preceding sentence. Raw run output that carries host tokens (a test results file carries `runUser` and
`computerName` attributes) is written to the git-ignored coverage directory and never committed; only
sanitized summary fields are copied into an evidence artifact.

The fixed label 2026-09-06T22-00 in every artifact file name is a plan-wide naming label, not an
observation. Each artifact's `Timestamp:` field carries the actual ISO-8601 time at which that
artifact was written, read from the clock at write time, and will therefore differ from the label.

### R8 — coverage thresholds and their authority

The repository-wide line-coverage floor named by CLAUDE.md, which is rank 1 in the policy compliance
order, is 80 percent. Every coverage run in this plan is scoped to the two test assemblies this change
touches, which is a narrower denominator than the full-suite denominator that floor is written
against, so the two binding gates for this change are the no-regression comparison between the
same-scope Phase 0 baseline and the Phase 5 post-change figure, and the 90 percent changed-line
figure CLAUDE.md requires of new and changed code. The 80 percent floor is recorded against both
percentages in the Phase 5 delta artifact rather than asserted, and a baseline already below it under
this scope is recorded as a pre-existing condition this change neither creates nor resolves. The 85
percent line and 75 percent branch figures in .claude/rules/general-unit-test.md are recorded in the
same artifact as observations and are not the gate. No coverage-exclusion attribute is introduced by
this change.

### R9 — repo-wide Cobertura comparability

A repository-wide Cobertura line rate is not stable across runs when the instrumented denominator
moves. Every coverage comparison in this plan therefore branches explicitly: when the post-change
`lines-valid` differs from the baseline `lines-valid` by at most 5 percent of the baseline value, the
document-level line rates are compared directly and the comparison is recorded as comparable;
otherwise the comparison is recorded as non-comparable, the covered and valid counters are reported
for both runs, and the binding gate becomes the changed-line coverage figure. Both branches are
recorded, so neither is left unmeasured.

### R10 — changed-line coverage admits a non-executable outcome

Cobertura emits a line element only for a line carrying IL. Blank lines, brace-only lines, `using`
directives, XML documentation comments and interface member declarations therefore appear in a
`git diff --unified=0` changed-line set and in no coverage map. The changed-line report marks such a
line `hits=non-executable` and the percentage is computed over executable changed lines only. A file
whose type carries a class-level coverage-exclusion attribute produces no class element at all; the
Phase 0 measurability determination records which Write Set production files are measurable, and a
file determined not measurable is reported as NOT APPLICABLE rather than as a zero.

### R11 — no line in this document may begin with a left square bracket after a hyphen and a space,
except a task line

The plan validator treats every line beginning with a hyphen, a space and a left square bracket as a
task line. Acceptance criteria, checklists and bullet lists in this document therefore use other
prefixes.

---

## Test policy for this change

MSTest with `[TestClass]` and `[TestMethod]`, Moq for mocking, FluentAssertions for assertions,
Arrange-Act-Assert structure, descriptive names. Creating temporary files in tests is prohibited, and
`Thread.Sleep`, `Task.Delay` and real wall-clock waits are banned. Tests must not require a live
Outlook process and must not trigger any user interface. Test files live in the mirroring test project
tree and never beside production source.

The serializer already exposes five injectable protected seams that the new tests drive instead of
touching disk: the read-all-text seam, the disk-exists seam, the stream-writer seam, the dialog seam
and the timer factory. A deterministic manual-fire timer double already exists in the UtilitiesCS test
project's test-helpers directory and raises its elapsed event synchronously. The established harness
that exposes those seams is a private nested class inside the existing serializer test file and is
therefore not reachable from a new file, so the new serializer guard test file declares its own
equivalent harness and its own minimal test item type. No new production seam is introduced for AC1,
AC3, AC5, AC6, AC7 or AC8.

AC2 is the one genuine gap: the serializer's logger is a private static log4net logger and is not
injectable. AC2 is asserted through an in-memory log4net appender, following the same shape the
TaskMaster test project's startup-timing and app-events helper files already use: activate the
appender, take a logger from the default repository hierarchy, set its level to Debug, mark the
repository configured, add the appender, and remove it again on teardown. The
attachment point differs for AC2 and only for AC2: the serializer's logger name is derived from the
declaring type reported by reflection over a member of a generic type, so no closed constructed type
name is a reliable attachment point and P1-T11 attaches to the root logger of that hierarchy instead,
selecting the events it asserts on by level and by a message fragment unique to that test file. The
AC5 loud-failure appender in P1-T13 attaches to a named logger in the ordinary way, because the
controller type is not generic. The UtilitiesCS test project already carries a direct log4net
reference, so the same helper shape compiles there. Every appender is detached in a finally block so
tests remain independent. The same finally block restores the logger's previous level and the
repository's previous configured flag, because attaching to the root logger and marking the repository
configured are process-wide mutations that would otherwise outlive the test and change the behaviour of
concurrently running classes in the same assembly. This adds no production surface to an already
over-cap shared file.

---

### Phase 0 — Policy reads, toolchain bootstrap and baseline capture

- [ ] [P0-T1] Read, in the required order, CLAUDE.md, then .claude/rules/general-code-change.md, then .claude/rules/general-unit-test.md, then .claude/rules/quality-tiers.md, then .claude/rules/csharp.md, then .claude/rules/tonality.md, and record the read in the artifact named below.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-instructions-read.2026-09-06T22-00.md
      Acceptance: the artifact exists and carries `Timestamp:`, `Policy Order:` and an explicit list naming all six files above, each with its line count as read.

- [ ] [P0-T2] Read the three requirements sources in this feature folder — spec.md, issue.md and research/research-folder-settings-persistence.md — and record their SHA-256 digests.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-requirements-read.2026-09-06T22-00.md
      Acceptance: the artifact records one SHA-256 digest per source file and confirms that spec.md contains a `## Acceptance Criteria` section holding exactly eight criteria identified AC1 through AC8.

- [ ] [P0-T3] Record the base SHA into docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-base-sha.2026-09-06T22-00.md as a single line whose first ten characters are `BASE-SHA: ` followed by the forty lowercase hexadecimal characters of the merge base, derived with the command below.

```powershell
git merge-base HEAD origin/main
```

      Acceptance: the artifact contains exactly one line matching the pattern `^BASE-SHA: [0-9a-f]{40}$`, and that value equals the merge base printed by the command. The artifact also carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`.

- [ ] [P0-T4] Author the fixed helper script at coverage/plan797-helpers.ps1 providing the functions the later tasks invoke: resolve vstest through vswhere, resolve msbuild through vswhere, run a scoped vstest invocation, run a coverage collection by building its own dotnet-coverage argument list as rule R2 describes, read the four Cobertura root counters, and compute changed-line coverage. The three backslash-bearing literals the helper needs are given below and must be authored verbatim into the helper file.

```text
${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe
Common7\IDE\Extensions\TestPlatform\vstest.console.exe
MSBuild\**\Bin\MSBuild.exe
```

      Acceptance: running the helper in its SelfCheck mode through pwsh with the NoProfile and File switches exits 0 and prints one line beginning `VSTEST-RESOLVED=` followed by an existing file path, one line beginning `MSBUILD-RESOLVED=` followed by an existing file path, and one line beginning `HELPER-FUNCTIONS=` listing at least the six function names. Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-helper-selfcheck.2026-09-06T22-00.md

- [ ] [P0-T5] Bootstrap this worktree in four steps, run with the working directory set to the repository root of this worktree, because a script that derives the repository root from the current directory would otherwise act on the session's root checkout. Step 1 installs the repository-local .NET SDK: the repository-root global.json pins SDK 8.0.205 and lists a repository-local .dotnet-sdk directory first in its paths list, and that directory does not exist here at the base commit. Step 2 restores the pinned tool manifest, the file named dotnet-tools.json at the repository root, which pins csharpier to 1.2.6 and nothing else. Step 3 confirms the global dotnet-coverage tool, which the repository coverage runner requires and which the local manifest does not carry. Step 4 restores the packages.config package set, because no packages directory exists here at the base commit. Commands, in the fenced text block below: pwsh with the NoProfile and File switches over scripts/vscode/Install-RepoDotNetSdk.ps1; then dotnet tool restore against the root manifest dotnet-tools.json; then dotnet-coverage with the version switch; then msbuild TaskMaster.sln with the Restore target, the parallel switch, the Debug configuration, the Any CPU platform and the RestorePackagesConfig property.

```text
pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1
dotnet tool restore --tool-manifest dotnet-tools.json
dotnet-coverage --version
msbuild TaskMaster.sln /t:Restore /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:RestorePackagesConfig=true
```

      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-bootstrap.2026-09-06T22-00.md
      Acceptance: all four commands exit 0; the artifact records four EXIT_CODE values and, in Output Summary, states for each of the repository-local SDK directory, the csharpier tool, the global dotnet-coverage tool and the packages directory whether it was already present or was created by this step, and records the SDK version the first command reports and the version the third command prints. The third command's exit-0 requirement may be satisfied only after the recorded recovery install described next, in which case the artifact records the initial non-zero exit, the recovery install and the exit-0 re-run, and the requirement is read against the re-run rather than against the first attempt. If the third command exits non-zero, the executor runs dotnet tool install with the global switch for dotnet-coverage once, records it as a fifth command with its own EXIT_CODE, and re-runs the version command. A non-zero exit on any other command halts the plan and is reported, not worked around.

- [ ] [P0-T6] Capture the csharpier formatting baseline for the whole tree by running the read-only check subcommand from the repository root of this worktree, recording the result into docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-csharpier-check.2026-09-06T22-00.md.

```text
dotnet tool run csharpier check .
```

      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-csharpier-check.2026-09-06T22-00.md
      Acceptance: the artifact records `EXIT_CODE:` and, in `Output Summary:`, the exact count printed on the `Checked` summary line plus the complete list of any file paths the run reported as unformatted. If the exit code is 0 the artifact states `PRE-EXISTING-FORMAT-DRIFT: NONE`; otherwise it states `PRE-EXISTING-FORMAT-DRIFT:` followed by the enumerated paths. Phase 5 consumes this determination and neither branch is left unrecorded.

- [ ] [P0-T7] Capture the analyzer baseline by running the analyzer rebuild against TaskMaster.sln and writing the full log to coverage/plan797-baseline-analyzers.log.

```text
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:Verbosity=detailed;LogFile=coverage/plan797-baseline-analyzers.log"
```

      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-analyzer-build.2026-09-06T22-00.md
      Acceptance: the artifact records `EXIT_CODE:`, states whether the MSBuild summary line `    0 Error(s)` is present, records the warning count from the summary, and enumerates under a heading `BASELINE-DIAGNOSTIC-IDS:` the distinct diagnostic identifiers reported as errors (empty when the build is clean). Both the clean and the non-clean branch are recorded.

- [ ] [P0-T8] Capture the nullable and warnings-as-errors baseline by running the type-check rebuild against TaskMaster.sln and writing the full log to coverage/plan797-baseline-nullable.log.

```text
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:Verbosity=detailed;LogFile=coverage/plan797-baseline-nullable.log"
```

      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-nullable-build.2026-09-06T22-00.md
      Acceptance: the artifact records `EXIT_CODE:`, states whether `    0 Error(s)` is present, and enumerates under `BASELINE-DIAGNOSTIC-IDS:` the distinct diagnostic identifiers reported as errors. No solution-wide nullable enable property is added to this command; the artifact states that explicitly.

- [ ] [P0-T9] Capture the baseline test result for the two assemblies this change touches, UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll and TaskMaster.Test/bin/Debug/TaskMaster.Test.dll, using the helper at coverage/plan797-helpers.ps1 with the filter and isolation switch from rule R6, writing the results file under coverage/plan797-trx/baseline. The narrower controller-scope counts the acceptance requires are derived from this same results file by selecting the four named test classes, so no second run is performed.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-vstest.2026-09-06T22-00.md
      Acceptance: the artifact records `EXIT_CODE:` and, in `Output Summary:`, the total, passed, failed and skipped counts read from the run, the exact filter expression used, and the sentence naming the four excluded shell-icon classes and stating that CI covers them. The results file itself is not committed; only these sanitized counts are recorded. The artifact additionally enumerates, under a heading `BASELINE-FAILING-TESTS:`, the fully qualified name of every test that failed in this run, and states `BASELINE-FAILING-TESTS: NONE` when the run is green. Phase 1 and Phase 5 subtract this set, so a pre-existing failure is neither reported as a fail-before signal nor demanded as a pass-after. The artifact additionally records, under a heading `BASELINE-CONTROLLER-SCOPE:`, obtained with the narrower filter P1-T3 uses — the one selecting the store wrapper controller, store wrapper controller tests, store wrapper and store wrapper viewer test classes — the total, passed and failed counts, the fully qualified name of every test that failed within that narrower scope, and the exact text of that filter expression. The skipped count is derived from the results file counters as the total minus the executed count, not from console text: a green run prints no `Skipped` line and the results file writes its not-executed counter as zero, so a console-derived figure would be unreadable on exactly the run this baseline expects. The artifact carries `ExpectedExitCode:` with the integer this run produced.

- [ ] [P0-T10] Capture the baseline coverage document at coverage/plan797-baseline/coverage.cobertura.xml using the helper at coverage/plan797-helpers.ps1, and record the numeric coverage values.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-coverage.2026-09-06T22-00.md
      Acceptance: `EXIT_CODE: 0`, or a non-zero exit code recorded together with `ExpectedExitCode:` carrying that same integer when the collected run reproduced only failures enumerated under `BASELINE-FAILING-TESTS:` in the P0-T9 artifact, because the coverage collection propagates the inner test run's exit code; the artifact states which branch applies. In `Output Summary:` the artifact carries one single line with the four space-separated assignments `LINES_COVERED=`, `LINES_VALID=`, `BRANCHES_COVERED=` and `BRANCHES_VALID=` in that order, each immediately followed by a concrete integer, plus `BASELINE_LINE_PERCENT=` carrying the document-level line rate multiplied by 100 and rendered to two decimal places, plus `BASELINE_ASSEMBLY_SCOPE=` naming the two test assemblies the run covered, and states that both test assemblies were excluded from instrumentation by the derived coverage settings. No field carries the text UNVERIFIED or any placeholder. This step records values and asserts no threshold: the repository's own 80 percent assertion is written against a full-suite denominator and this run's denominator is narrower, so the comparison belongs in P5-T7.

- [ ] [P0-T11] Determine coverage measurability for each of the seven Write Set production C# files by searching the baseline document coverage/plan797-baseline/coverage.cobertura.xml for a class element whose filename attribute ends with a path separator followed by that file name.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-coverage-measurability.2026-09-06T22-00.md
      Acceptance: the artifact lists each of the seven production paths with a verdict of MEASURABLE or NOT MEASURABLE and, for each MEASURABLE entry, the baseline covered and valid line counts. The trailing filename match is anchored on a path separator so that a file name cannot also select a differently named sibling that ends with the same characters. The two files that do not yet exist at this point are recorded as NOT YET CREATED.

- [ ] [P0-T12] Record the pre-change line count of every Write Set C# file, using a line count over each of the following paths: TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs, TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs, UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs, UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs, UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs, UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs, UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs, UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs, TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-file-sizes.2026-09-06T22-00.md
      Acceptance: the artifact lists all nine paths with an integer line count each, and records under a heading `PRE-EXISTING-OVER-CAP:` the single entry for the serializer file, whose count is expected to be 613 and which D5 declares out of scope for splitting. The project files are not enumerated in this census, because the 500-line cap applies to production code, test code and reusable script files and not to project files.

- [ ] [P0-T13] Record the anchored change-set baseline for the Phase 5 scope gate by listing the tracked paths already differing from the base SHA on this branch, using the anchored diff and the porcelain companion below, into docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-scope-baseline.2026-09-06T22-00.md.

```powershell
$BaseSha = (Select-String -Path 'docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-base-sha.2026-09-06T22-00.md' -Pattern '^BASE-SHA: ([0-9a-f]{40})$').Matches[0].Groups[1].Value
git diff --name-status $BaseSha HEAD
git status --porcelain --untracked-files=all
```

      Acceptance: the artifact records both listings under the headings `SCOPE-BASELINE-COMMITTED:` and `SCOPE-BASELINE-WORKTREE:`, states that the first listing covers work already committed on this branch and the second covers uncommitted work, that a path may appear in both, and that Phase 5 subtracts the union, and records `EXIT_CODE:` for the diff command. Phase 5 subtracts the union of these two sets, so both are captured here rather than inferred later. The artifact additionally records, under a heading `PREPARATION-TRACKED:`, the result of `git ls-files --error-unmatch` over the five preparation documents — the issue document, the specification, the research artifact, this plan file and the promoted feature entry — confirming each is tracked in HEAD. Any path reported as untracked is recorded and reported to the caller before Phase 1 begins, because P6-T11's terminal porcelain gate admits no residual other than this plan file and the agent-memory tree.

---

### Phase 1 — Enabling seams and regression tests written first

Phase 1 establishes the file layout and the declaration-only production seams needed so that the new
tests compile, then writes every regression test, then runs them and records a failing result. The
seams are declaration-only and defect-preserving: they add members and a file, and they change no
behaviour. Without them the new tests would fail to compile, which reddens the entire test assembly
and produces no attributable test result at all.

Tests that are red before the Phase 2 to Phase 4 fixes: the AC1 loader-adoption test, both AC2
error-log tests, the AC4 synchronous-flush test, the AC5 loud-failure test, the three AC6 fallback
cases that exercise a failing primary lookup and the two AC6 retry cases, the two AC7 trim cases whose
input carries a leading backslash pair together with the AC7 populate test, and the three AC8 cases
including the inverted D6 test. Tests that are additive coverage and green from the moment they are
written: the AC1 key-absent negative case, the AC4 deferred-path-unchanged case, the AC5
argument-order case, the AC6 fallback case in which the primary SMTP address is present, and the four
AC7 trim cases whose input carries no leading backslash pair — no leading backslash, a single leading
backslash, the empty string and null — because the P1-T8 placeholder returns its argument unchanged
and each of those four expects its argument unchanged. The `[expect-fail]` tag is carried by the run
task, which is the task with a binary observable outcome.

- [ ] [P1-T1] Create UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs as a pure move: relocate `PopulateWithCurrent`, `BindExcludeStoreCheckbox` and `GetRelativeFsPath` verbatim out of UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs, and add the `partial` keyword to the class declaration in both files. The new file's first line is the nullable enable pragma, matching the first line of the file the members are moved out of. Without it the moved annotations lose their nullable context, the compiler reports CS8632 on each of them, and the Phase 5 type-check gate, which treats warnings as errors, fails. No behavioural edit in this task.

```powershell
$BaseSha = (Select-String -Path 'docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-base-sha.2026-09-06T22-00.md' -Pattern '^BASE-SHA: ([0-9a-f]{40})$').Matches[0].Groups[1].Value
git add --intent-to-add UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
git diff $BaseSha -- UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
```

      Acceptance: the three member bodies are byte-identical to their pre-move text apart from indentation; the class declaration in each file reads `public partial class StoreWrapperController`; and the anchored diff above, which compares the base commit to the working tree and therefore reports uncommitted work, shows in the controller file only deletions of the moved members plus the single declaration-line change, and in the new file only additions.

- [ ] [P1-T2] Register the new file by adding one compile entry for OutlookObjects\Store\StoreWrapperController.Display.cs to UtilitiesCS/UtilitiesCS.csproj, beside the existing entry for the controller.
      Acceptance: the project file contains exactly one compile entry naming that file, and the anchored, staged listing below reports the project file as modified.

```powershell
$BaseSha = (Select-String -Path 'docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-base-sha.2026-09-06T22-00.md' -Pattern '^BASE-SHA: ([0-9a-f]{40})$').Matches[0].Groups[1].Value
git add --intent-to-add UtilitiesCS/UtilitiesCS.csproj UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
git diff --name-status $BaseSha -- UtilitiesCS/UtilitiesCS.csproj UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
```

- [ ] [P1-T3] Prove the relocation is behaviour-preserving by building the solution and running the existing controller and store test classes through the helper at coverage/plan797-helpers.ps1, before any new test exists.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/regression-testing/p1-t3-pure-move-green.2026-09-06T22-00.md
      Acceptance: the artifact carries an `ExpectedExitCode:` equal to the exit code this run actually produced; that exit code is 0, or 1 with every failing test a member of the failing set recorded under `BASELINE-CONTROLLER-SCOPE:` in the P0-T9 artifact. An exit code of 1 carrying any failing test outside that set is a gate failure and is attributable to the relocation. The artifact records a passed count at or above the passed count recorded under `BASELINE-CONTROLLER-SCOPE:`, obtained with the identical filter expression recorded there. The filter selects `StoreWrapperController_Tests`, `StoreWrapperControllerTests`, `StoreWrapperTests` and `StoreWrapperViewerTests`, with the `TestCategory!=LiveOutlook` clause repeated on every disjunct.

- [ ] [P1-T4] Create UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs declaring a public interface in namespace `UtilitiesCS` with the single member `void ApplyJunkFolderSelections(string junkCertainRelativePath, string junkPotentialRelativePath);`, with XML documentation naming the parameter order and stating that the certain path is first.
      Acceptance: the file exists, declares exactly one interface and exactly one member with that signature, takes no dependency on the TaskMaster project, and the declaration is syntactically well formed; compilation of every Phase 1 seam is proven once, in P1-T9.

- [ ] [P1-T5] Register the new interface by adding one compile entry for Interfaces\IGlobals\IJunkFolderSelectionSink.cs to UtilitiesCS/UtilitiesCS.csproj, beside the existing entries for the store disable and store rehook service interfaces.
      Acceptance: the project file contains exactly one compile entry naming that file, inside an ItemGroup that already carries the compile entries for the store disable and store rehook service interfaces. Compilation is proven once, in P1-T9.

- [ ] [P1-T6] Add the declaration-only, defect-preserving guarded flush entry point to UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs: a new public method named `SerializeNow` whose body in this task calls the existing deferred `Serialize()` and nothing else, with an in-code comment stating that Phase 2 replaces the body with the guarded synchronous flush.
      Acceptance: the method exists with signature `public void SerializeNow()`, the existing `Serialize()`, `Serialize(string)`, `SerializeThreadSafe(string)` and `RequestSerialization(string)` members are unchanged, and the declaration is syntactically well formed; compilation of every Phase 1 seam is proven once, in P1-T9.

- [ ] [P1-T7] Add the declaration-only, defect-preserving retry entry point and failure-reason property to UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs: an `internal string? LastSmtpLookupError { get; private set; }` carrying a JsonIgnore attribute, and an `internal string? RefreshUserEmailAddress()` whose body in this task assigns `UserEmailAddress = GetSmtpAddressFromStore();` and returns it, with an in-code comment stating that Phase 3 adds the fallback chain and the captured reason.
      Acceptance: both members exist with those signatures, `GetSmtpAddressFromStore` is otherwise unchanged, and the declaration is syntactically well formed; compilation of every Phase 1 seam is proven once, in P1-T9.

- [ ] [P1-T8] Add the declaration-only, defect-preserving trim helper to UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs: `internal static string? TrimStorePrefix(string? folderPath)` whose body in this task returns its argument unchanged, with an in-code comment stating that Phase 4 supplies the real trim.
      Acceptance: the method exists with that exact signature and accessibility, no call site yet references it, and the declaration is syntactically well formed; compilation of every Phase 1 seam is proven once, in P1-T9.

- [ ] [P1-T9] Build the solution with the analyzer rebuild to confirm every Phase 1 seam compiles, writing the log to coverage/plan797-p1-analyzers.log.

```text
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:Verbosity=detailed;LogFile=coverage/plan797-p1-analyzers.log"
```

      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/regression-testing/p1-t9-seam-build.2026-09-06T22-00.md
      Acceptance: `EXIT_CODE: 0` and the summary line `    0 Error(s)` is present. If the Phase 0 analyzer baseline was not clean, the acceptance is instead that the recorded diagnostic identifier set is a subset of `BASELINE-DIAGNOSTIC-IDS:` from the Phase 0 artifact and contains no diagnostic attributed to a Write Set file.

- [ ] [P1-T10] Append the AC1 tests to TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs, driving the existing `TestableAppOlObjects` harness. Positive case: a loader whose `Config.Disk.FilePath` is a fixed fake AppData path is added to the configuration dictionary under the key `StoresWrapper`, the deserialize stub returns null, and after `LoadStoresAsync()` the fresh wrapper's `Config.Disk.FilePath` equals the loader's path. Negative case: the configuration key is absent, the fresh build occurs, and the path remains the empty string. No filesystem access in either case.
      Acceptance: two new `[TestMethod]` members exist with descriptive names, the file compiles, and the positive case fails at this point in the plan because the fresh-build branch does not yet adopt the loader configuration.

- [ ] [P1-T11] Create UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs holding the AC2 and AC4 tests, with its own harness subclass exposing the stream-writer and timer-factory seams and its own minimal test item type, because the existing harness is a private nested class in another file. AC2: with an in-memory log4net appender attached to the serializer's declaring type, calling `Serialize()` with an empty `Config.Disk.FilePath` and, in a separate test, with a null `Config.Disk.FilePath`, each produces at least one error-level event whose rendered message names this file's own test item type, and arms no timer. AC4: with a manual-fire timer double injected and a memory-stream-backed writer, the explicit-save entry point writes without the timer firing, and a separate test asserts the pre-existing deferred path still requires a timer fire. The appender is detached in a finally block. The serializer initialises its logger from the declaring type reported by MethodBase.GetCurrentMethod, which for a member of a generic type resolves to the generic type definition rather than to any closed constructed type, so one logger serves every instantiation and its name carries no type argument; an appender attached to the full name of a closed constructed serializer type is a different logger and captures nothing. To be correct under either resolution, the tests attach the memory appender to the root logger of the default log4net repository, set that logger's level to Debug, mark the repository configured, and select captured events by an error level together with a rendered message naming this file's own test item type; the appender is removed from the root logger in a finally block that also restores the root logger's previous level and the repository's previous configured flag. The minimal test item type this file declares carries a name occurring nowhere else in the UtilitiesCS test project, so a concurrently running class cannot contribute a matching event to the existence assertion. The assertion is existence, not an exact count, because the run settings this plan uses impose a class-level parallel scope on every assembly and concurrent classes can only add events. The MSTest attribute opting the class out of parallel execution is still applied, but it is not the isolation mechanism: the unique message fragment and the paired behavioural assertions, that no write reached the stream-writer seam and no timer was armed, are.
      Acceptance: the file exists in the mirroring test directory, declares four or more `[TestMethod]` members covering the four scenarios named above, creates no temporary file, uses no `Thread.Sleep` or `Task.Delay`, and compiles.

- [ ] [P1-T12] Register the new serializer test file by adding one compile entry for ReusableTypeClasses\SmartSerializableSerializeGuardTests.cs to UtilitiesCS.Test/UtilitiesCS.Test.csproj, beside the existing serializer test entries.
      Acceptance: the project file contains exactly one compile entry naming that file, beside the existing serializer test entries. Whether the entry pulls the file into the assembly is proven by the NEW-TEST-FILES-DISCOVERED: enumeration required of P1-T18.

- [ ] [P1-T13] Add the AC5 tests to UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs. Retarget the existing `RecordingOlObjects` double to implement the new sink interface in addition to its stub base, and add a new double implementing only the globals interface while still declaring a public method named `ApplyJunkFolderSelections` with the same two string parameters. Add an argument-order test asserting the certain path arrives first and the potential path second, and a loud-failure test asserting that with the non-sink double the controller records at least one error-level event whose rendered message names the new sink interface, through an in-memory appender attached to the controller's declaring type, and does not invoke the method. The assertion is existence, not an exact count: the controller's logger is a static field shared with every other controller test class in this assembly, the run settings this plan uses impose a class-level parallel scope, and the opt-out attribute on one class does not exclude writers in sibling classes. That attribute is still applied, and the paired assertion that the non-sink double records no invocation is what attributes the event to this test.
      Acceptance: both new `[TestMethod]` members exist, the existing test named `PersistJunkFolderSelections_WhenApplyMethodIsMissing_DoesNotThrow` is retargeted to the typed seam rather than deleted, the two existing tests named `SaveChanges_PersistsBothSettingsAndRefreshesActiveJunkFolders` and `ButtonCancel_Click_LeavesStoredSettingsAndActiveFoldersUnchanged` still compile, and the loud-failure test fails at this point because the reflection lookup still succeeds against the non-sink double.

- [ ] [P1-T14] Add the AC6 fallback-ordering tests to UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs, table-driven over the mocked Outlook folder chain already built by the private helper `CreateRootFolderWithPrimarySmtpAddress` in that file. Four cases: the primary SMTP address is present; the primary SMTP read throws and the address entry address contains an at-sign; both fail and the display name contains an at-sign; all fail, producing a null result and a non-empty captured failure reason.
      Acceptance: four new `[TestMethod]` members exist, none requires a live Outlook process, and cases two, three and four fail at this point because the single outer catch converts every failure to null with no fallback.

- [ ] [P1-T15] Create UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs as a new partial of the existing `StoreWrapperController_Tests` class, holding the AC6 retry tests, the AC7 trim tests and the AC8 guard tests. AC6 retry: the retry runs on a populate when the address is null and does not run when the address is already populated, and the user-email label carries the specific unavailability message containing the captured reason. AC7: six pure-function cases over the trim helper covering a leading double backslash, no leading backslash, a single leading backslash, the empty string, null, and a path consisting only of the double backslash; plus one populate test asserting the rendered Inbox and Root Folder label text. AC8: a null current store renders the existing placeholder literals and does not throw, and the relative-path helper returns its placeholder rather than throwing.
      Acceptance: the file declares `public partial class StoreWrapperController_Tests` in the same namespace so the existing private helpers are reachable, holds nine or more `[TestMethod]` members covering the scenarios above, and compiles.

- [ ] [P1-T16] Register the new controller display test file by adding one compile entry for OutlookObjects\Store\StoreWrapperController_Tests.Display.cs to UtilitiesCS.Test/UtilitiesCS.Test.csproj, beside the existing controller test partial entries.
      Acceptance: the project file contains exactly one compile entry naming that file, beside the existing controller test partial entries. Whether the entry pulls the file into the assembly is proven by the NEW-TEST-FILES-DISCOVERED: enumeration required of P1-T18.

- [ ] [P1-T17] Invert the deliberate D6 test expectation in UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs: the test named `PopulateWithCurrent_NullCurrent_SetsErrorLoadingText` currently asserts that the act throws a `NullReferenceException`; replace that assertion with one asserting that the act does not throw and that the archive and junk labels carry their existing placeholder literals. Replace the two misleading in-body comments with a comment naming this as the declared AC8 expectation inversion under D6.
      Acceptance: the test name is unchanged, the assertion no longer references `NullReferenceException`, the new assertion pins specific rendered values rather than an exception type, and the test fails at this point because the four unguarded dereferences still throw.

- [ ] [P1-T18] [expect-fail] Build the solution and run the scoped regression set through the helper at coverage/plan797-helpers.ps1 against UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll and TaskMaster.Test/bin/Debug/TaskMaster.Test.dll, writing the results file under coverage/plan797-trx/p1.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/regression-testing/p1-t18-fail-before.2026-09-06T22-00.md
      Acceptance: the artifact carries `ExpectedExitCode: 1` and an `EXIT_CODE:` equal to 1, and its `Output Summary:` enumerates one `FAIL-BEFORE:` line per failing test whose fully qualified name is absent from the `BASELINE-FAILING-TESTS:` set in the P0-T9 artifact, enumerates separately under `PRE-EXISTING-FAILURES:` every failing test whose name is present in that set, and records the passed and failed counts. The `FAIL-BEFORE:` enumeration must contain at least one failing test attributable to each of AC1, AC2, AC4, AC5, AC6, AC7 and AC8. The `Output Summary:` also names, under `NEW-TEST-FILES-DISCOVERED:`, at least one test method from each of the two new test files registered in P1-T12 and P1-T16, so a missing compile entry is caught here rather than silently dropping a file. The build itself must succeed: a compile error is not an acceptable fail-before signal and halts the plan.

- [ ] [P1-T19] Author the AC3 fail-before exception dossier at docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/regression-testing/fail-before-exception.2026-09-06T22-00.md.
      Acceptance: the dossier carries `Timestamp:`, a `WhyFailingRunImpossible:` statement of one to three sentences explaining that persistence across an Outlook restart requires a live VSTO host and cannot be reproduced by any automated test in this environment, an alternative-proof section citing the runtime log evidence that the settings file has never been created on the reporting machine, and the negative-evidence fields `SearchScope:`, `SearchPatterns:` and `SearchResult:` recording that no automated fail-before run exists for AC3.

---

### Phase 2 — Root cause 1: the bootstrap gap, the serializer guard and the flush (AC1, AC2, AC4)

- [ ] [P2-T1] Implement AC1 in TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs alone: in `LoadStoresAsync`, on the branch where the configuration key was found and the deserialize returned null, apply the already-in-scope loader configuration to the freshly built wrapper by calling the configuration copy with a deep copy, after the fresh build assignment. The branch where the configuration key is not found must remain a fresh build with an empty path.
      Acceptance: the shared deserialize overload in UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs is unchanged, UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableBase.cs is unchanged, and the AC1 positive test added in P1-T10 passes while the AC1 negative test continues to pass.

- [ ] [P2-T2] Implement AC2 in UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs: change the guard in `Serialize()` from a comparison against the empty string to a null-or-empty check, and log at error level, naming the serialized item type reported by typeof over the type parameter and the empty or null path, when the guard rejects. No timer is armed on the rejecting path.
      Acceptance: the two AC2 tests added in P1-T11 pass, and the existing test callers of the serializer that the research enumerated across the serializer, non-typed serializer, linked-list and stack test files continue to pass unchanged.

- [ ] [P2-T3] Implement AC4 in UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs: replace the placeholder body of `SerializeNow` with the guarded synchronous flush. The AC2 empty-or-null-path error must be evaluated first, so that the fix does not substitute one silent failure for another; only when the path is non-empty and non-null does the method call the existing thread-safe write method directly, which takes the write lock, writes through the injectable stream-writer seam and re-arms the single-shot guard in its finally block.
      Acceptance: the AC4 synchronous-flush test passes, the AC4 deferred-path-unchanged test still passes, `Serialize()` and `RequestSerialization(string)` retain their existing three-second single-shot deferred behaviour with the first caller's path captured, and the serializer file is not split.

- [ ] [P2-T4] Wire the AC4 flush at the explicit Save path in UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs by replacing the deferred serialize call at the end of `SaveChanges` with the guarded synchronous flush entry point. The VSTO add-in lifecycle file is not modified.
      Acceptance: `SaveChanges` calls the flush entry point exactly once, the existing tests named `SaveChanges_SetsCurrentProperties` and `SaveChanges_PersistsBothSettingsAndRefreshesActiveJunkFolders` pass unchanged, and neither raises because the guard rejects an empty path with an error log rather than an exception.

- [ ] [P2-T5] Run the root-cause-1 automated criteria through the helper at coverage/plan797-helpers.ps1, scoped to the AC1, AC2 and AC4 test names, writing the results file under coverage/plan797-trx/p2.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/regression-testing/p2-t5-root-cause-1-green.2026-09-06T22-00.md
      Acceptance: `EXIT_CODE: 0`, a failed count of 0, and one `PASS-AFTER:` line per test name that appeared as a `FAIL-BEFORE:` entry for AC1, AC2 or AC4 in the P1-T18 artifact. The run's own totals are recorded separately as non-asserted observations.

- [ ] [P2-T6] [expect-fail] Run the full scoped suite over UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll and TaskMaster.Test/bin/Debug/TaskMaster.Test.dll to confirm that no test outside the AC1, AC2 and AC4 set regressed, writing the results file under coverage/plan797-trx/p2-full.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/regression-testing/p2-t6-no-regression.2026-09-06T22-00.md
      Acceptance: the artifact carries `ExpectedExitCode: 1` and an `EXIT_CODE:` equal to 1, because the AC5, AC6, AC7 and AC8 tests are still red at this point by design, and its `Output Summary:` states that the set of failing tests, after subtracting every name recorded under `PRE-EXISTING-FAILURES:` in the P1-T18 artifact, is a proper subset of the P1-T18 `FAIL-BEFORE:` set and contains no test outside it, and lists the subtracted names separately under `PRE-EXISTING-FAILURES:`.

---

### Phase 3 — Root cause 2: the SMTP lookup fallback and retry (AC6)

- [ ] [P3-T1] Implement the AC6 fallback chain inside `GetSmtpAddressFromStore` in UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs, replacing the single outer catch with per-step handling in the order the specification fixes: the Exchange primary SMTP address; then the address entry's address when it contains an at-sign; then the store display name when it contains an at-sign; then null. Each step carries its own handling for a COM failure, mirroring the existing in-repo helper in the application globals that already implements exactly this ordering.
      Acceptance: the four AC6 fallback tests added in P1-T14 pass, and the two pre-existing tests in that file named `GetSmtpAddressFromStore_WhenExchangeUserIsUnavailable_ReturnsNull` and `GetSmtpAddressFromStore_WhenExchangeLookupThrowsComException_ReturnsNull` are re-derived against the new ordering: each supplies neither an at-sign-bearing address entry address nor an at-sign-bearing display name, so each still returns null and must pass unchanged. If either would now return a non-null value, the test arrangement is corrected in this task and the correction is recorded as a declared expectation change with its reason.

- [ ] [P3-T2] Implement the AC6 captured reason and the retry entry point in UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs: the failure-reason property added in P1-T7 records the caught exception's message when every fallback step fails and is cleared when a lookup succeeds; the retry entry point re-runs the chain and assigns the result, and is safe to call when the root folder is null.
      Acceptance: the AC6 test asserting a non-empty captured reason on total failure passes, and calling the retry entry point on a store wrapper with a null root folder returns null without throwing.

- [ ] [P3-T3] Implement the AC6 retry and the specific unavailability message in UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs: inside `PopulateWithCurrent`, when the current store's user email address is null, invoke the retry entry point at most once per dialog open, then render either the address or a specific unavailability message that includes the captured reason, replacing the generic placeholder for the user-email label only. The Inbox and Root Folder placeholders are unchanged in this task.
      Acceptance: the two AC6 retry tests pass, the retry does not run when the address is already populated, the message rendered on total failure contains the captured reason, and the existing test named `PopulateWithCurrent_ShowsCurrentJunkSelectionsInViewer`, which constructs the controller with a null globals reference, still passes because every new dereference on this path is null-conditional.

- [ ] [P3-T4] Run the root-cause-2 automated criterion through the helper at coverage/plan797-helpers.ps1, scoped to the AC6 test names, writing the results file under coverage/plan797-trx/p3.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/regression-testing/p3-t4-root-cause-2-green.2026-09-06T22-00.md
      Acceptance: `EXIT_CODE: 0`, a failed count of 0, and one `PASS-AFTER:` line per test name that appeared as a `FAIL-BEFORE:` entry for AC6 in the P1-T18 artifact.

- [ ] [P3-T5] Re-run the root-cause-1 scoped set through the helper at coverage/plan797-helpers.ps1 to confirm the two root causes remain separately traceable and that the Phase 3 edits did not disturb Phase 2, writing the results file under coverage/plan797-trx/p3-rc1.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/regression-testing/p3-t5-root-cause-1-still-green.2026-09-06T22-00.md
      Acceptance: `EXIT_CODE: 0` and a failed count of 0 over exactly the AC1, AC2 and AC4 test set recorded in the P2-T5 artifact.

---

### Phase 4 — Remaining folded defects (AC5, AC7, AC8) and the readability correction

- [ ] [P4-T1] Implement the AC5 typed sink in TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs: declare the partial class as implementing the new interface, and add an explicit interface implementation that forwards to the existing internal method. Explicit implementation is required so the public surface of the globals type does not widen; the existing internal method keeps its accessibility and its body.
      Acceptance: the file declares the interface on the partial, the explicit implementation forwards both arguments in the certain-then-potential order, the existing internal method is otherwise unchanged, and the project reference direction remains one-way from TaskMaster to UtilitiesCS.

- [ ] [P4-T2] Replace the reflection lookup with the typed cast in UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs: `PersistJunkFolderSelections` casts the globals Outlook objects reference to the new interface and calls the member directly; when the cast fails it logs at error level, not warning, and returns. The rendered message must name the new sink interface, because the P1-T13 loud-failure test selects the event by that name fragment on a logger shared with every other controller test class. Remove the `using System.Reflection;` directive from that file if no other member in it uses reflection.
      Acceptance: the AC5 loud-failure test and the AC5 argument-order test both pass, the retargeted test named `PersistJunkFolderSelections_WhenApplyMethodIsMissing_DoesNotThrow` passes, the file contains no `GetMethod` call. Analyzer cleanliness after the Phase 4 edits is proven in P5-T3.

- [ ] [P4-T3] Implement AC7 in UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs: replace the placeholder body of the trim helper with a pure trim that removes a leading pair of backslash characters and returns every other input unchanged, including null and the empty string; then apply it to the Inbox and Root Folder label assignments in `PopulateWithCurrent`.
      Acceptance: the six AC7 pure-function cases pass, the AC7 populate test asserts the rendered label text with no leading backslash pair, the helper performs no allocation-free assumption about a null input, and the shared archive stem contract type is not modified.

- [ ] [P4-T4] Implement the AC8 guards at the top of `PopulateWithCurrent` in UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs: make the four dereferences of the current store null-conditional so that they match the null-conditional form the very next block already uses, and confirm the existing placeholder literals render for the Inbox, Root Folder, the two archive fields and the two junk fields.
      Acceptance: the inverted D6 test passes, the AC8 null-current test in the new display partial passes, and the six placeholder literals are unchanged apart from the user-email literal that AC6 replaced in P3-T3.

- [ ] [P4-T5] Implement the AC8 guard in `GetRelativeFsPath` in UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs so that a null current store returns the existing archive placeholder rather than throwing.
      Acceptance: the AC8 relative-path test passes, and the three pre-existing tests in UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs named `GetRelativeFsPath_ArchiveFsWithEmptyPath_ReturnsPlaceholder`, `GetRelativeFsPath_ArchiveFsWithPath_ConverterReturnsEmpty_ReturnsPlaceholder` and `GetRelativeFsPath_ArchiveFsWithPath_ConverterReturnsValues_ReturnsFormatted`, plus the test in UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs named `GetRelativeFsPath_NullArchiveFsRoot_ReturnsPlaceholder`, all pass unchanged.

- [ ] [P4-T6] Apply the readability correction in UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs: change the single-ampersand operator in the relative-path condition to the short-circuit form. This is behaviourally inert, because both operands call a null-tolerant string extension and neither has a side effect; the change description must not claim it repairs a fault.
      Acceptance: the four relative-path tests named in P4-T5 still pass, and the only operator changed is the one in the live condition. The commented-out block relocated with `PopulateWithCurrent` also contains a single-ampersand occurrence; it is dead commented text and is left untouched, so an occurrence count over the file is not used as the gate.

- [ ] [P4-T7] Run the remaining automated criteria through the helper at coverage/plan797-helpers.ps1, scoped to the AC5, AC7 and AC8 test names, writing the results file under coverage/plan797-trx/p4.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/regression-testing/p4-t7-remaining-criteria-green.2026-09-06T22-00.md
      Acceptance: `EXIT_CODE: 0`, a failed count of 0, and one `PASS-AFTER:` line per test name that appeared as a `FAIL-BEFORE:` entry for AC5, AC7 or AC8 in the P1-T18 artifact.

- [ ] [P4-T8] Record the pre-format line count of every Write Set C# file and the two created files into docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/p4-t8-file-sizes-preformat.2026-09-06T22-00.md, using the same nine paths as P0-T12 plus UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs, UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs, UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs and UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs.
      Acceptance: every listed C# path is at or below 500 lines, except the serializer file, whose pre-existing overage D5 declares out of scope and which is reported with both its Phase 0 count and its current count. This is a pre-format census; the binding audit is P5-T8, after the formatter runs.

---

### Phase 5 — Final QA loop with coverage comparison

The loop below runs in order. If any step fails against its declared expectation or changes files, the
loop restarts at P5-T1. A step whose artifact declares a non-zero `ExpectedExitCode:` and whose
observed exit code matches that declaration has not failed and does not restart the loop. The plan is
not complete until P5-T1 through P5-T6 complete in a single uninterrupted pass.

- [ ] [P5-T1] Run the formatter over the tree from the repository root of this worktree, capturing the C#-scoped worktree state into coverage/plan797-format-before.txt immediately before and into coverage/plan797-format-after.txt immediately after, so the write-mode run is observable beyond its exit code.

```powershell
git status --porcelain --untracked-files=all -- '*.cs' | Out-File -Encoding utf8 coverage/plan797-format-before.txt
dotnet tool run csharpier format .
git status --porcelain --untracked-files=all -- '*.cs' | Out-File -Encoding utf8 coverage/plan797-format-after.txt
```

      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/p5-t1-csharpier-format.2026-09-06T22-00.md
      Acceptance: `EXIT_CODE: 0`; the artifact records the summary count the formatter printed, and the set difference between the after and before listings. If the Phase 0 artifact recorded `PRE-EXISTING-FORMAT-DRIFT: NONE`, every path in that difference must be a Write Set path. If it recorded pre-existing drift, the difference may additionally contain exactly the paths that artifact enumerated, and those paths are reverted in this task so the change does not carry unrelated reformatting.

- [ ] [P5-T2] Verify formatting read-only from the repository root of this worktree so that a real signal, rather than a write-mode exit code, decides the gate, recording the result into docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/p5-t2-csharpier-check.2026-09-06T22-00.md.

```text
dotnet tool run csharpier check .
```

      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/p5-t2-csharpier-check.2026-09-06T22-00.md
      Acceptance: `EXIT_CODE: 0` and the `Output Summary:` records the count printed on the `Checked` summary line and states that the run reported no unformatted file; or, when the P0-T6 artifact recorded pre-existing drift and P5-T1 reverted those paths, a non-zero `EXIT_CODE:` whose reported unformatted paths are a subset of the `PRE-EXISTING-FORMAT-DRIFT:` set enumerated in the P0-T6 artifact and contain no Write Set path. In that branch the artifact carries `ExpectedExitCode:` with the observed integer, records the drift as a pre-existing condition this change neither creates nor resolves, and the Phase 5 loop does not restart on it.

- [ ] [P5-T3] Run the analyzer rebuild against TaskMaster.sln, writing the log to coverage/plan797-final-analyzers.log.

```text
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:Verbosity=detailed;LogFile=coverage/plan797-final-analyzers.log"
```

      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/p5-t3-analyzer-build.2026-09-06T22-00.md
      Acceptance: `EXIT_CODE: 0` and the summary line `    0 Error(s)` is present. If the Phase 0 analyzer baseline was not clean, the acceptance is instead that the recorded diagnostic identifier set is a subset of `BASELINE-DIAGNOSTIC-IDS:` from the Phase 0 artifact and that no diagnostic in it is attributed to a Write Set file. The warning count is recorded and compared to the Phase 0 warning count.

- [ ] [P5-T4] Run the type-check rebuild against TaskMaster.sln, writing the log to coverage/plan797-final-nullable.log.

```text
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:Verbosity=detailed;LogFile=coverage/plan797-final-nullable.log"
```

      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/p5-t4-nullable-build.2026-09-06T22-00.md
      Acceptance: `EXIT_CODE: 0` and `    0 Error(s)` is present; or, when the Phase 0 nullable baseline was not clean, the recorded diagnostic identifier set is a subset of the Phase 0 set and contains no diagnostic attributed to a Write Set file. The artifact states explicitly that no solution-wide nullable enable property was supplied.

- [ ] [P5-T5] Run the full scoped test suite over UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll and TaskMaster.Test/bin/Debug/TaskMaster.Test.dll through the helper at coverage/plan797-helpers.ps1, writing the results file under coverage/plan797-trx/p5.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/p5-t5-vstest.2026-09-06T22-00.md
      Acceptance: the artifact carries an `ExpectedExitCode:` equal to the exit code this run actually produced, and that exit code is either 0, or 1 with every failing test a member of the `BASELINE-FAILING-TESTS:` set in the P0-T9 artifact and no failing test residing in any Write Set test file. An exit code of 1 when the P0-T9 artifact recorded `BASELINE-FAILING-TESTS: NONE` is a gate failure. When the exit code is 0 the failed count is 0, and when the P0-T9 artifact recorded failing names the artifact states which of them did not reproduce. The `Output Summary:` records total, passed, failed and skipped counts, the exact filter expression used, and the sentence naming the four excluded shell-icon classes and stating that CI covers them. As in P0-T9, the skipped count is derived from the results file counters as the total minus the executed count, not from console text, because a green run prints no `Skipped` line and the results file writes its not-executed counter as zero. It additionally records one `PASS-AFTER:` line for every fully qualified test name that appeared as a `FAIL-BEFORE:` entry in the P1-T18 artifact, so the fail-before to pass-after correspondence is complete rather than sampled; names carried under `PRE-EXISTING-FAILURES:` there are excluded from that correspondence and are listed separately. A failing test that is a member of `BASELINE-FAILING-TESTS:` and also resides in a Write Set test file does not restart the Phase 5 loop. It is recorded under `PRE-EXISTING-IN-WRITE-SET:` with the reason it was not repaired, and the plan outcome is remediation-required rather than complete.

- [ ] [P5-T6] Collect post-change coverage into coverage/plan797-final/coverage.cobertura.xml through the helper at coverage/plan797-helpers.ps1 and record the numeric values.
      Artifact: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/p5-t6-coverage.2026-09-06T22-00.md
      Acceptance: `EXIT_CODE: 0`; or a non-zero exit code carried as `ExpectedExitCode:` with that same integer when the collected run reproduced only failures already recorded under `BASELINE-FAILING-TESTS:` in the P0-T9 artifact, in which case the artifact names those tests and states that the Cobertura document was produced despite them. The coverage collection propagates the exit code of the inner test run, so a reproduced pre-existing failure does not fail this step and does not restart the Phase 5 loop. The `Output Summary:` carries one single line with the four space-separated assignments `LINES_COVERED=`, `LINES_VALID=`, `BRANCHES_COVERED=` and `BRANCHES_VALID=` in that order, each immediately followed by a concrete integer, plus `POSTCHANGE_LINE_PERCENT=` carrying the document-level line rate multiplied by 100 to two decimal places, plus `POSTCHANGE_ASSEMBLY_SCOPE=` naming the same two test assemblies P0-T10 recorded, and states that both test assemblies were excluded from instrumentation by the derived coverage settings. No field carries the text UNVERIFIED or any placeholder. The binding comparison is made in P5-T7 against the same-scope Phase 0 baseline, not here.

- [ ] [P5-T7] Produce the coverage delta report at docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/p5-t7-coverage-delta.2026-09-06T22-00.md, comparing the Phase 0 baseline document coverage/plan797-baseline/coverage.cobertura.xml with the Phase 5 document coverage/plan797-final/coverage.cobertura.xml, and computing changed-line coverage over the seven Write Set production C# files from the anchored diff below.

```powershell
$BaseSha = (Select-String -Path 'docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-base-sha.2026-09-06T22-00.md' -Pattern '^BASE-SHA: ([0-9a-f]{40})$').Matches[0].Groups[1].Value
git add --intent-to-add UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
git diff --unified=0 $BaseSha -- TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
```

      Acceptance: the artifact reports three figures explicitly — `BASELINE_LINE_PERCENT=`, `POSTCHANGE_LINE_PERCENT=` and `CHANGED_LINE_PERCENT=` — each a concrete number, none carrying the text UNVERIFIED. It records the comparability branch chosen under rule R9 and the two `lines-valid` values that decided it. It records the changed-line table per file with a `hits` value per changed executable line and a `hits=non-executable` marker for each changed line that emits no IL, computes the percentage over executable changed lines only, and reports NOT APPLICABLE for any file the Phase 0 measurability artifact recorded as not measurable. The AC5 explicit interface implementation lands in TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs, whose partial class carries no class-level coverage-exclusion attribute, so that file is measurable and produces a real per-file changed-line row; no automated test in this plan drives the real settings-writing implementation that member forwards to, so that row is expected to sit at or near zero. The Phase 1 relocation moves PopulateWithCurrent, BindExcludeStoreCheckbox and GetRelativeFsPath verbatim into the display partial, so the anchored diff reports every line of those three members as added although none of them changed. Those lines are enumerated in the artifact under `RELOCATED-UNMODIFIED:` with their post-change hit counts recorded as observations, and they are excluded from the `CHANGED_LINE_PERCENT=` denominator. Lines inside those members that the Phase 3 and Phase 4 edits altered are not relocated lines and remain in the denominator. Without this exclusion the denominator would carry pre-existing partially covered code that this change does not modify. The gate is the aggregate `CHANGED_LINE_PERCENT=` figure computed over the executable changed lines of every measurable file in the table, and a low per-file row on that one file is expected and is not itself a failure. `CHANGED_LINE_PERCENT=` must be at or above 90, and, when rule R9 selected the comparable branch, `POSTCHANGE_LINE_PERCENT=` must not be below `BASELINE_LINE_PERCENT=`, which is then the no-regression rule and the binding repository-wide gate for this change. When rule R9 selected the non-comparable branch, the artifact records both percentages and both covered-and-valid counter pairs as observations, states that the denominator moved by more than 5 percent, and the sole binding gate is `CHANGED_LINE_PERCENT=`. The artifact records whether each of the two percentages is at or above the 80 percent floor in CLAUDE.md, which is rank 1 in the policy compliance order, and when the baseline is already below that floor it states plainly that the condition is pre-existing under the two-assembly scope, that this change neither creates nor resolves it, and that the binding gates are therefore the no-regression comparison and the changed-line percentage. The artifact additionally records, as non-asserted observations, the 85 percent line and 75 percent branch figures from .claude/rules/general-unit-test.md.

- [ ] [P5-T8] Perform the binding post-format file-size audit over the thirteen C# paths enumerated in P4-T8, writing docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/p5-t8-file-sizes-postformat.2026-09-06T22-00.md.
      Acceptance: every listed C# path is at or below 500 lines except UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs, which is reported with its Phase 0 count of 613 and its post-change count, and is declared a pre-existing condition this change does not resolve under D5. In particular UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs and UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs are each at or below 500. No project file is enumerated in this audit, because the cap does not reach project files.

- [ ] [P5-T9] Produce the scope gate at docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/p5-t9-scope.2026-09-06T22-00.md, listing every source path the change touches and confirming it is a subset of the Write Set.

```powershell
$BaseSha = (Select-String -Path 'docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-base-sha.2026-09-06T22-00.md' -Pattern '^BASE-SHA: ([0-9a-f]{40})$').Matches[0].Groups[1].Value
git add --all -- '*.cs' '*.csproj'
git diff --name-status $BaseSha -- '*.cs' '*.csproj'
git status --porcelain --untracked-files=all -- '*.cs' '*.csproj'
```

      Acceptance: the artifact records the diff listing and the porcelain listing under separate headings, states that the porcelain listing is taken after the staging command and therefore overlaps the diff listing rather than complementing it, and records the union of the two listings as the working set, subtracts the union of the two Phase 0 scope-baseline sets, and confirms that every remaining path is a member of the Write Set. The test is a subset test, not an equality test, so a claimed-but-unmodified path such as TaskMaster.Test/TaskMaster.Test.csproj does not fail the gate. The artifact separately confirms zero paths under the dot-claude, dot-codex or dot-agents trees, zero paths under the config directory, zero GitHub workflow files, and no repository-root file. The pathspec restricts the gate to source and project files, because the change also writes feature-folder documents and evidence artifacts by design; that reading and its reason are stated in the artifact.

- [ ] [P5-T10] Record the clean-pass confirmation at docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/p5-t10-clean-pass.2026-09-06T22-00.md, naming the commands run in P5-T1 through P5-T6 and stating that they completed in a single uninterrupted pass with no step failing against its declared expectation and no step changing files.
      Acceptance: the artifact names all six commands verbatim, records each `EXIT_CODE:`, states the number of loop restarts performed and the reason for each, and confirms that the final pass required no restart.

---

### Phase 6 — Manual verification, acceptance check-off and handoff

- [ ] [P6-T1] Perform the AC3 manual verification and record it at docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/other/p6-t1-ac3-manual-verification.2026-09-06T22-00.md. The written procedure is: (1) confirm the settings file StoresWrapper.json does not exist under the local AppData TaskMaster directory, recording the check without recording the absolute path of the user profile; (2) build and load the add-in and start Outlook; (3) open Settings, then Folder Settings, and record the rendered Archive Root Outlook, Archive Root File System, Junk Potential, Junk Email, User Email, Inbox and Root Folder values; (4) select an Archive Root Outlook value and click Save; (5) confirm the settings file now exists; (6) close Outlook fully and reopen it; (7) reopen Folder Settings and confirm the saved value is present; (8) confirm the session log contains no serializer error and no line reporting an empty or null settings path; (9) check the junk-folder rollout consideration recorded as risk 4 in spec.md by confirming whether the junk selections shown agree with the .NET user settings.
      Acceptance: the artifact carries `Timestamp:`, the nine numbered steps with an observed result for each, an explicit `AC3-RESULT:` line reading PASS or FAIL, and a statement that this criterion is verified manually because it requires a live Outlook process. No absolute host path, user account name or machine name appears in the artifact.

- [ ] [P6-T2] Check off AC1 in the `## Acceptance Criteria` section of docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md and mirror the same check-off in the Proposed Fix / Validation Ideas section, at heading level two, of docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md.
      Acceptance: exactly the AC1 checkbox is marked in both files, the criterion text is byte-identical to its pre-change text in both files, and no other checkbox changes state in this task.

- [ ] [P6-T3] Check off AC2 in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md and mirror it in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md.
      Acceptance: exactly the AC2 checkbox is marked in both files, its text is unchanged, and no other checkbox changes state in this task.

- [ ] [P6-T4] Check off AC3 in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md and mirror it in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md, only if the P6-T1 artifact records `AC3-RESULT: PASS`.
      Acceptance: exactly the AC3 checkbox is marked in both files when and only when the P6-T1 artifact records PASS; when it records FAIL, this task leaves the checkbox unmarked and the plan outcome is remediation-required rather than complete. Both branches are recorded in the P6-T11 summary.

- [ ] [P6-T5] Check off AC4 in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md and mirror it in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md.
      Acceptance: exactly the AC4 checkbox is marked in both files, its text is unchanged, and no other checkbox changes state in this task.

- [ ] [P6-T6] Check off AC5 in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md and mirror it in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md.
      Acceptance: exactly the AC5 checkbox is marked in both files, its text is unchanged, and no other checkbox changes state in this task.

- [ ] [P6-T7] Check off AC6 in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md and mirror it in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md.
      Acceptance: exactly the AC6 checkbox is marked in both files, its text is unchanged, and no other checkbox changes state in this task.

- [ ] [P6-T8] Check off AC7 in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md and mirror it in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md.
      Acceptance: exactly the AC7 checkbox is marked in both files, its text is unchanged, and no other checkbox changes state in this task.

- [ ] [P6-T9] Check off AC8 in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md and mirror it in docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md.
      Acceptance: exactly the AC8 checkbox is marked in both files, its text is unchanged, and no other checkbox changes state in this task.

- [ ] [P6-T10] Mirror the issue update at docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/issue-updates/issue-797.2026-09-06T22-00.md.
      Acceptance: the artifact carries `Timestamp:`, the exact text intended for the issue, and a `PostedAs:` field with the value body, comment or unknown; when not posted it carries a POSTING BLOCKED header and the reason. When `PostedAs: body`, the same update is mirrored into the feature folder issue.md.

- [ ] [P6-T11] Produce the acceptance status summary at docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/p6-t11-ac-status.2026-09-06T22-00.md, then delete the session helper described in rule R2, then commit every source change, every evidence artifact, and the acceptance-criteria check-off edits to the specification and the issue document, so the worktree is clean. The feature folder documents and the promoted feature entry were committed to this branch during preparation, before execution began, so they are already tracked and are not new files here.
      Acceptance: the summary lists AC1 through AC8 with a status of PASS or NOT MET, names for each the implementing task, the test name or manual procedure, and the evidence artifact path; it restates the final coverage figures from the P5-T7 artifact including the changed-line percentage; it names the four excluded shell-icon test classes and states that CI covers them; and it records the two declared expectation changes, namely the D6 inversion and any P3-T1 arrangement correction. After the commit, `git status --porcelain --untracked-files=all` reports at most two residual classes: this plan file, whose final task check-off is written after the commit, and any file under the repository's agent-memory tree, tracked or untracked. The artifact records the verbatim porcelain output and identifies which residual class each line belongs to. Any other path present is a gate failure.

---

## Known limitations recorded rather than resolved

1. AC3 cannot be gated automatically. The automated tests establish that the disk path is populated
   and that the write occurs through the injectable seam, but they do not prove the file appears on
   disk in a live VSTO host.
2. The serializer file remains over the 500-line cap after this change. D5 records this as
   pre-existing and deliberately not resolved here; no task in this plan splits it.
3. The AC6 retry reintroduces a synchronous Outlook COM property read on the UI thread at dialog-open
   time. The risk is bounded to one lookup per dialog open, attempted only when the address is null.
   A genuinely non-blocking read is out of scope and is recorded in spec.md as a follow-up.
4. The QuickFiler recipient-resolution blocking hazard is a different caller of the same Outlook
   getter and is not fixed under this issue.
5. The junk-folder rollout divergence described as risk 4 in spec.md is a known, accepted consequence
   of the chosen AC5 reading and is checked during the P6-T1 manual verification.
6. Four shell-icon test classes are excluded from every local run for environmental reasons unrelated
   to this change. CI covers them.
