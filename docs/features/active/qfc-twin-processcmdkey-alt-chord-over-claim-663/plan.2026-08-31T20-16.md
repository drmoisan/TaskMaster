# qfc-twin-processcmdkey-alt-chord-over-claim (Plan)

- **Issue:** #663
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T20-16
- **Status:** Ready for preflight
- **Version:** 1.0
- **Work Mode:** `full-bug`. `spec.md` is the sole acceptance-criteria source and carries AC-1 through
  AC-15. No `user-story.md` exists for this feature and none is required.

**Fail-closed evidence rule:** every command-bearing task below emits an evidence artifact carrying
`Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`, with the single exception of the
acceptance check-off tasks `[P6-T2]` through `[P6-T16]`, whose product is the flipped checkbox in
`spec.md` and whose verification search is recorded in the task's progress output rather than in a
separate artifact. If any required baseline artifact, QA
artifact, or coverage artifact is missing or incomplete, the outcome is BLOCKED or INCOMPLETE, never
PASS, and the corresponding plan checkbox stays unchecked.

**Evidence location rule (non-overridable):** every evidence path in this plan resolves under
`docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/<kind>/` where `<kind>`
is one of `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`,
`remediation-baseline`. `coverage` is not a member of that set, so coverage artifacts are written under
`qa-gates`. No `artifacts/` sub-path is used for evidence. Helper scripts are not placed under
`evidence/`.

---

## Reading guide for the executor

#### Scope, fixed by the spec

Three files change. No file is created, no file is deleted, and neither
`QuickFiler/QuickFiler.csproj` nor `QuickFiler.Test/QuickFiler.Test.csproj` is edited.

1. `QuickFiler/Controllers/QfcFormKeyHandler.cs` (20 lines at branch head) — one new
   `internal static bool` member is added. `IsAltKeyCommand`, declared on line 18, is not modified.
2. `QuickFiler/Viewers/QfcFormViewer.cs` (296 lines at branch head) — the four-line guard on lines
   58 through 61 is replaced by a single predicate call. The dispatch on line 68 stays the
   parameterless `ToggleKeyboardDialogAsync()`. The pre-existing unused locals on lines 64 through 67
   are retained deliberately; removing them is an explicit non-goal of the spec.
3. `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` (67 lines at branch head, compiled through
   `QuickFiler.Test/QuickFiler.Test.csproj` line 151) — seven test methods are added. The four existing
   `IsAltKeyCommand_*` methods on lines 16, 29, 42 and 55 are not touched.

#### The new member, quoted verbatim so the executor writes exactly this

The member name this plan creates is `ClaimsAltChord`. Its final source form, which mirrors the
delivered Email Filer predicate at QuickFiler/Viewers/EfcViewer.cs lines 96 through 104:

```csharp
internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)
{
    if (handler is null || !keyData.HasFlag(Keys.Alt))
    {
        return false;
    }

    Keys keyCode = keyData & Keys.KeyCode;
    return keyCode == Keys.Menu || keyCode == Keys.None;
}
```

`QuickFiler/Controllers/QfcFormKeyHandler.cs` gains `using QuickFiler.Interfaces;` for the
`IQfcKeyboardHandler` parameter type, which is declared at `QuickFiler/Interfaces/IQfcKeyboardHandler.cs`
line 9 in namespace `QuickFiler.Interfaces`.

The rewritten guard in `QuickFiler/Viewers/QfcFormViewer.cs`, quoted verbatim:

```csharp
if (Controllers.QfcFormKeyHandler.ClaimsAltChord(_keyboardHandler, keyData))
```

The existing qualification form `Controllers.QfcFormKeyHandler` is retained because the file's
namespace is `QuickFiler`, so the relative qualification resolves. The literal `ClaimsAltChord` must
appear exactly once in that file, so no comment may repeat it.

#### The seven test methods, named verbatim

These names are created by this plan and are quoted here so that a later search for them is an
instruction rather than an unfalsifiable assertion:

| Test method | Input | Expected |
|---|---|---|
| `ClaimsAltChord_WithBareAltFlagAndHandler_ReturnsTrue` | `Keys.Alt` | `true` |
| `ClaimsAltChord_WithMenuKeyCodeAndAltFlag_ReturnsTrue` | `Keys.Menu` OR-ed with `Keys.Alt` | `true` |
| `ClaimsAltChord_WithAltM_ReturnsFalse` | `Keys.Alt` OR-ed with `Keys.M` | `false` |
| `ClaimsAltChord_WithAltF4_ReturnsFalse` | `Keys.Alt` OR-ed with `Keys.F4` | `false` |
| `ClaimsAltChord_WithAltLeft_ReturnsFalse` | `Keys.Alt` OR-ed with `Keys.Left` | `false` |
| `ClaimsAltChord_WithoutAltFlag_ReturnsFalse` | `Keys.M` | `false` |
| `ClaimsAltChord_WithNullHandler_ReturnsFalse` | `null`, `Keys.Alt` | `false` |

`ClaimsAltChord_WithoutAltFlag_ReturnsFalse` must assert two inputs in a single Arrange-Act-Assert
body, `Keys.M` first and `Keys.Control` second, each with its own because-string. The spec's behavior
table carries eight rows and the seven method names cover seven of them; the `Keys.Control` row, which
the spec's Edge cases subsection names explicitly, exercises the same predicate arm as the `Keys.M`
row, so the second assertion closes AC-1's "every row" claim without changing the eleven-`[TestMethod]`
count that `[P2-T1]` pins or the seven-name enumeration the plan uses elsewhere.

The because-string of `ClaimsAltChord_WithAltM_ReturnsFalse` must name `Move Options`. It must not
name a Filters menu: `ButtonFilters.Text` on the QuickFiler surface is the plain string `"Filters"`
with no ampersand, so a Filters-menu justification would be false for this surface.

#### Why a two-step edit, and where the red run sits

The seven new tests reference `QfcFormKeyHandler.ClaimsAltChord`, which does not exist at branch head.
Adding them first would fail the whole `QuickFiler.Test` assembly at compile time, which is a build
break rather than a behavioural red and yields no per-test failure evidence. Phase 1 therefore lands a
**behaviour-preserving seam**: `ClaimsAltChord` is introduced carrying the current defect, and the
viewer guard is routed through it. That intermediate state is exactly equivalent to the branch-head
guard, because `handler is not null && keyData.HasFlag(Keys.Alt)` is the same condition as
`(_keyboardHandler is not null) && Controllers.QfcFormKeyHandler.IsAltKeyCommand(keyData)`. Phase 2
then adds the tests, which compile and produce a genuine runtime red on exactly three of the seven.
Phase 3 adds the key-code mask and turns them green. No fail-before exception dossier is needed,
because a real failing run exists.

The three tests that must fail in Phase 2 and pass in Phase 3 are
`ClaimsAltChord_WithAltM_ReturnsFalse`, `ClaimsAltChord_WithAltF4_ReturnsFalse` and
`ClaimsAltChord_WithAltLeft_ReturnsFalse`. The other four pass in both phases.

#### Verification patterns VC-1 and VC-2, reused verbatim from the spec

`Select-String -Pattern` takes a .NET regular expression, in which a backslash before a pipe makes the
pipe a literal character rather than an alternation operator. The escaped spelling therefore matches
nothing and any "returns zero matches" assertion over it would pass whatever the executor wrote. The
alternation pipes below are deliberately unescaped and must not be "corrected". The backslash before
each dot in VC-1 is correct and must be retained: there the backslash escapes a literal dot.

VC-1, run against `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`:

```
new Form|: Form|Thread\.Sleep|Task\.Delay|GetTempFileName|GetTempPath
```

VC-2, run against `QuickFiler/Viewers/QfcFormViewer.cs`:

```
FromHandle|new KeyEventArgs
```

#### Toolchain command forms

MSBuild is resolved through vswhere inside each command rather than assumed on `PATH`, and every
MSBuild command runs under `pwsh -NoProfile -Command` with outer single quotes and inner double
quotes, because passing `/m` through a POSIX shell can mangle it into a drive-qualified path and
produce MSB1008.

Analyzer gate, quoted verbatim:

```
pwsh -NoProfile -Command '$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\663-analyzers.msbuild.log;Verbosity=detailed"'
```

Type-check gate, quoted verbatim:

```
pwsh -NoProfile -Command '$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\663-nullable.msbuild.log;Verbosity=detailed"'
```

`/p:Nullable=enable` must never be added to the type-check command. No project in this repository
carries a `<Nullable>` element and there is no `Directory.Build.props`, so the property is a
solution-wide opt-in that conscripts every file that has never adopted the `#nullable enable` pragma.
The command above is the one `.github/workflows/_build-nullable.yml` lines 57 through 59 run. That
step is a backtick-continued pwsh block: line 57 ends at `/p:Configuration=Debug`, line 58 carries
`"/p:Platform=Any CPU"`, and line 59 carries `/p:TreatWarningsAsErrors=true`, which is the switch
this paragraph is about. It differs from the form above only in resolving the executable and the
solution path through `msbuild` on `PATH` and `$env:SOLUTION_PATH` rather than through vswhere and a
literal solution name.

`/t:Rebuild` is load-bearing. MSBuild's up-to-date check does not invalidate on a command-line `/p:`
change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and runs no
analyzers.

**Non-vacuity observation.** The literal `Task "Csc"` is emitted by MSBuild only at detailed
verbosity, so each gate above attaches a detailed file logger writing to the repository's gitignored
`coverage\` directory (`.gitignore` line 144 is `coverage/*`). Each gate task greps that log for
`Task "Csc"`, records the occurrence count, and then deletes the log so no multi-megabyte machine
artifact is committed. A run whose log contains zero occurrences of that literal has skipped
compilation and fails the gate regardless of its exit code.

The `LogFile=` segment is the only part of the quoted commands a task may vary. `[P0-T9]` substitutes
`coverage\663-analyzers-baseline.msbuild.log`, `[P1-T3]` substitutes
`coverage\663-analyzers-seam.msbuild.log`, `[P0-T10]` substitutes
`coverage\663-nullable-baseline.msbuild.log`, and `[P4-T3]` and `[P4-T4]` use the quoted names
unchanged. Every other character of the quoted commands is fixed.

**Formatting.** The mutating CSharpier pass is scoped to the three changed paths, one invocation per
path, so a repository-wide rewrite cannot widen the change set and break AC-14. The read-only
repository-wide `dotnet tool run csharpier check .` is then the gate. It is equivalent to, but not
character-for-character identical with, the CI command: `.github/workflows/_format-check.yml` line 41
is `run: dotnet csharpier check .`, which omits the `tool run` segment. Both resolve to the same
manifest-pinned executable, because that job restores the tool manifest first on line 37, and that
job runs a CSharpier check on every pull request and therefore keeps `origin/main` format-clean.
CSharpier is always invoked here through `dotnet tool run` so the manifest-pinned 1.2.6 is used.

**Tests.** Tests run only through the repository wrappers `scripts/vscode/Invoke-MSTest.ps1` and
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`. Both append `/Settings:`, `/InIsolation` and
`/TestCaseFilter:TestCategory!=LiveOutlook` to the vstest argument list
(`Invoke-MSTest.ps1` line 54, `Invoke-MSTestWithCoverage.ps1` line 76). A bare `vstest.console.exe`
call drops the LiveOutlook exclusion and would attempt to run tests that need a live Outlook process.

Every wrapper test run in this plan uses `-SearchRoot .`, which is the form the spec's Toolchain
commands to run subsection names for both wrappers. The spec names `-SearchRoot QuickFiler.Test` only
in its Known tooling defect subsection, and names it there as a root that cannot be used, so this
plan agrees with the spec on this point rather than deviating from it. The mechanism is recorded
by `[P0-T11]`: `Invoke-MSTest.ps1` lines 107 through 113 pipe discovery through
`Select-Object -ExpandProperty FullName`, which yields a scalar rather than an array when exactly one
assembly matches, and lines 115 and 120 then read `.Count` on that scalar under the
`Set-StrictMode -Version Latest` set on line 77. A repository-scoped search root discovers nine
assemblies and is therefore array-valued and unambiguous. `[P0-T11]` records the scoped form's actual
behaviour so the choice rests on a measurement rather than on an assumption, and so the finding can
be promoted to its own issue.

**Reading a named test's outcome.** `vstest.console.exe` prints the names of failing tests but not the
names of passing ones, so "test X passed" is derived, once, as follows and this derivation is reused
by every acceptance below that names a test: X is declared as a `[TestMethod]` in a compiled test file,
no entry in the run's failed-test list carries both the name X and the declaring type
`QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests`, the run's `Total tests:` figure equals the
Phase 0 baseline total plus seven, and the run's not-run figure — its skipped or inconclusive count,
read under the rule `[P0-T12]` fixes and recorded by both run tasks that invoke this derivation,
`[P3-T3]` and `[P4-T5]` — is unchanged from the `[P0-T12]` baseline. Under those four
observations every named method both ran and passed. The fourth observation is load-bearing rather
than decorative: a skipped or inconclusive test is absent from the failing list and is still counted
in `Total tests:`, so the first three observations alone are satisfied by a method that never
executed. The declaring type is part of the derivation rather than the bare name because
`QuickFiler.Test` already declares `ClaimsAltChord_WithAltM_ReturnsFalse` and
`ClaimsAltChord_WithNullHandler_ReturnsFalse` in QuickFiler.Test/Controllers/EfcViewerTests.cs, so two
of the seven new method names are not unique within the assembly.

**Gating rule for the repository-wide runs.** `Invoke-MSTestWithCoverage.ps1` throws at line 236 when
the inner run reports a non-zero exit, and `Invoke-MSTest.ps1` throws at line 130 for the same reason.
This repository carries pre-existing load-driven failures concentrated in the `QfcItemController` test
files that appear under the concurrent instrumented run. Neither repository-wide run is therefore gated
on exit code 0. Both are gated on failure-set membership, against two separate baselines. `[P0-T12]`
captures BASELINE_FAILURE_SET as a verbatim list of failing test names from the uninstrumented run, and
every later uninstrumented run — `[P2-T3]`, `[P3-T3]` and `[P4-T5]` — must report no failing test
outside that set apart from the three named expected failures in Phase 2. `[P0-T13]` captures
BASELINE_COVERAGE_FAILURE_SET from the instrumented run, and `[P4-T6]`, which is the only later
instrumented run, is compared against that set instead. The two sets are not interchangeable, because
instrumentation adds load-driven failures that the uninstrumented baseline does not contain.

**Coverage post-processing.** Because `Invoke-MSTestWithCoverage.ps1` throws before its
Koverage post-processing block on lines 333 through 344, a run with any failing test leaves the raw
dotnet-coverage document on disk with absolute paths and unpruned third-party packages. Every coverage
task in this plan therefore applies `ConvertTo-KoverageCoberturaXml` out of band from
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` before reading any figure. That function is
idempotent with respect to the `<sources>` injection it performs, because line 430 guards it, so
applying it to an already-post-processed document is safe.

---

### Phase 0 — Baseline capture and worktree bootstrap

This worktree is not bootstrapped: it contains no `.dotnet-sdk` directory and no `packages` directory.
Both are gitignored (`.gitignore` line 350 is `.dotnet*/`, line 191 is `**/[Pp]ackages/*`), so
provisioning them does not dirty the tree. Until both exist, every `dotnet` and `msbuild` command is
unreachable and every downstream exit-code acceptance in this plan is unsatisfiable.

- [ ] [P0-T1] Read, in this order, `CLAUDE.md`, `.claude/rules/general-code-change.md`,
      `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, and then the supplementary
      `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md` and
      `.claude/rules/plan-acceptance-gates.md`. Write
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/phase0-instructions-read.md`
      carrying `Timestamp:`, `Policy Order:` naming the four core policies in the order above, and an
      explicit list of all seven file paths read. Acceptance: the artifact exists and contains all
      three required fields and all seven paths. Serves AC-10.
- [ ] [P0-T2] Read the requirement sources
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md`,
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/issue.md`,
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/research/2026-09-01T01-05-qfc-alt-chord-over-claim-research.md`,
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/other/call-site-compile-inclusion.md`
      and
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/other/qfc-mnemonic-inventory.md`.
      Write
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/phase0-requirements-read.md`
      carrying `Timestamp:`, the five paths, the transcribed identifiers AC-1 through AC-15 with no
      identifier repeated and none omitted, and the three in-scope file paths from the spec's In scope
      subsection. Acceptance: the artifact lists exactly fifteen acceptance identifiers and exactly
      three in-scope file paths. Serves AC-14.
- [ ] [P0-T3] Provision the repository-local .NET SDK by running
      `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1` from the worktree root.
      `global.json` pins `sdk.version` 8.0.205 with `paths` of `.dotnet-sdk` and `$host$`, so a host
      SDK alone cannot satisfy it. Acceptance: `dotnet --version` prints `8.0.205`, and
      `Test-Path .dotnet-sdk/sdk/8.0.205` returns `True` — that is the same marker path
      `Install-RepoDotNetSdk.ps1` builds at line 56 and validates at line 102, under the install
      directory its line 36 resolves to `<repo>/.dotnet-sdk` and into which its line 93 extracts the
      SDK zip. The `dotnet --version` clause discriminates on its own: the host muxer root carries no
      8.0.x SDK, measured in this worktree on 2026-09-01 where `dotnet --list-sdks` printed the single
      line `10.0.400 [C:\Program Files\dotnet\sdk]`, so `8.0.205` can only be resolved through the
      `.dotnet-sdk` entry in `global.json`'s `sdk.paths`. Do not assert over `dotnet --list-sdks`:
      that command enumerates the muxer's own root and does not consult `global.json`, so it prints
      the host line whether or not the repo-local SDK was installed. Record the `dotnet --version`
      output and the `Test-Path` result in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/sdk-bootstrap.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P0-T4] Restore NuGet packages by running
      `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"`.
      Every project declares an `EnsureNuGetPackageBuildImports` target whose `Error` fires at
      `BeforeTargets="PrepareForBuild"`, so no build can run before this completes. Acceptance: exit
      code 0 and the file
      `packages\Meziantou.Analyzer.3.0.194\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll`
      exists on disk. If that file is absent after an exit-0 restore, run `nuget restore TaskMaster.sln`
      and re-check; feature #469 recorded that shape on 2026-08-31, where the MSBuild restore reported
      success and the analyzer rebuild then failed on missing packages until a `nuget restore` was
      run. Record both invocations and both exit codes. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/nuget-restore.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P0-T5] Re-derive analyzer version concordance with two independently formulated searches and
      record both member sets. Search one enumerates the project-file side, keyed on the analyzer
      reference path:
      `pwsh -NoProfile -Command 'Get-ChildItem -Path . -Recurse -Filter *.csproj | Select-String -Pattern "packages\x5C(Meziantou\.Analyzer|Roslynator\.Analyzers)\.[0-9][0-9.]*\x5C" -AllMatches'`.
      Search two enumerates the pins independently, keyed on the package identifier rather than on a
      path and reading a different file type:
      `pwsh -NoProfile -Command 'Get-ChildItem -Path . -Recurse -Filter packages.config | Select-String -Pattern "id=\x22(Meziantou\.Analyzer|Roslynator\.Analyzers)\x22" -Context 0,2'`.
      Both patterns spell the backslash as `\x5C` and the double quote as `\x22` rather than as `\\`
      and `\"`. Those hex escapes are interpreted by the .NET regular-expression engine and are inert
      to every intervening quoting layer. The literal spellings are not usable here: `\"` is not an
      escape sequence in a PowerShell double-quoted string, so the string terminates at that quote and
      the alternation group is then parsed as a command; and a doubled backslash de-doubles when the
      command text is handed to the native `pwsh` executable, leaving an unbalanced group. Both
      spellings were executed against this worktree and both raised terminating errors.
      Acceptance: both sets are recorded verbatim in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/analyzer-version-concordance.md`
      and the artifact states, per package identifier, whether the two version sets are equal. A
      version present on one side and absent from the other is `error CS0006` at compile time rather
      than a warning, so a recorded disagreement is a blocking finding that must be resolved before
      `[P0-T9]`. Include `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P0-T6] Restore the manifest-pinned dotnet tools by running `dotnet tool restore` once from the
      worktree root. Acceptance: exit code 0 and the output of `dotnet tool run csharpier --version`
      begins with `1.2.6`, the version `dotnet-tools.json` pins. Equality over the whole version
      string is not asserted, because CSharpier 1.x can print an informational version carrying build
      metadata after the three-part number. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/dotnet-tool-restore.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P0-T7] Ensure the `dotnet-coverage` global tool is present, because
      `Invoke-MSTestWithCoverage.ps1` lines 292 and 293 throw before running anything when it is
      absent, and it is a global tool that `dotnet tool restore` does not supply. Run
      `pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }'`.
      Acceptance: `dotnet-coverage --version` prints a version string and exits 0. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/dotnet-coverage-tool.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P0-T8] Capture the baseline formatting state with the read-only command
      `dotnet tool run csharpier check .` from the worktree root. This is a read-only invocation, so
      its exit code is a sufficient observation and no tree comparison is needed. Acceptance: the exit
      code and the tool's final summary line are recorded verbatim as BASELINE_CSHARPIER_EXIT in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/csharpier-check.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`, together with the verbatim
      list of any file the tool reports as unformatted. If BASELINE_CSHARPIER_EXIT is non-zero, this
      is a blocking finding for `[P4-T2]`: record every path the tool reports as unformatted and, in
      the same artifact, state whether any of them is one of the three files this plan changes. If
      none is, `[P4-T2]` compares against exit code 0 for those three files only and records the
      pre-existing set as carried forward; if one is, resolve the drift before Phase 1.
      Serves AC-10.
- [ ] [P0-T9] Capture the baseline analyzer build by running the analyzer gate command quoted in the
      Toolchain command forms section above, writing its detailed log to
      `coverage\663-analyzers-baseline.msbuild.log`. Acceptance: the exit code is recorded; the log
      contains at least one occurrence of the literal `Task "Csc"`, whose occurrence count is
      recorded. This is the first task in the plan to assert that literal, and no detailed-verbosity
      MSBuild log exists in this repository's recorded evidence, so the literal is reasoned rather
      than measured. If the count is zero, this task fails: record the log's
      `Logging verbosity is set to:` header line and the verbatim task-started line the log emits for
      the compiler task, then substitute that literal into `[P0-T10]`, `[P1-T3]`, `[P4-T3]` and
      `[P4-T4]` as a micro-action before continuing. The artifact also records, verbatim, every
      console line matching `: warning [A-Z]+[0-9]+:` that names
      `QfcFormKeyHandler.cs`, `QfcFormViewer.cs` or `QfcFormKeyHandlerTests.cs`, as BASELINE_WARNINGS,
      together with the `(source file name, diagnostic identifier)` pair derived from each such line.
      No value is predicted for that set. It is measured here and recorded, and both `[P1-T3]` and
      `[P4-T3]` compare their own such set against it; an empty set and a non-empty set are equally
      admissible baselines, because `.github/workflows/_build-analyzers.yml` lines 50 through 52 run
      this command without `/p:TreatWarningsAsErrors=true`, so an analyzer warning naming one of the
      three files can already exist on `origin/main`.
      Record the exit code as BASELINE_ANALYZER_EXIT. If it is non-zero, this is a blocking finding
      for `[P1-T3]` and `[P4-T3]`, which both require exit code 0: record verbatim every console line
      matching `: error [A-Z]+[0-9]+:`, record the `(source file name, diagnostic identifier)` pairs
      derived from them as BASELINE_ANALYZER_ERRORS, and state in the same artifact whether any pair
      names `QfcFormKeyHandler.cs`, `QfcFormViewer.cs` or `QfcFormKeyHandlerTests.cs`. If one does,
      resolve the drift before Phase 1; the plan cannot distinguish a diagnostic it introduced from
      one it inherited in a file it edits. If none does, the carry-forward disposition is that
      `[P1-T3]` and `[P4-T3]` replace their exit-code-0 clause with the clause that their own error
      pair set equals BASELINE_ANALYZER_ERRORS, which is the same baseline-relative shape the warning
      comparison already uses. A scoped per-file re-reading is not available for this gate the way it
      is for `[P0-T8]`: msbuild builds the solution, not a file list.
      Delete the detailed log after recording its byte size.
      Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/msbuild-analyzers.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P0-T10] Capture the baseline type-check build by running the type-check gate command quoted in
      the Toolchain command forms section above, writing its detailed log to
      `coverage\663-nullable-baseline.msbuild.log`. Confirm in the artifact that the command line
      recorded under `Command:` contains no occurrence of `Nullable=enable`. Acceptance: the exit code
      is recorded as BASELINE_TYPECHECK_EXIT, the log contains at least one occurrence of the literal
      `Task "Csc"` and the count is recorded, and the detailed log is deleted after its byte size is
      recorded. If BASELINE_TYPECHECK_EXIT is non-zero, this is a blocking finding for `[P4-T4]`,
      which requires exit code 0: record verbatim every console line matching
      `: error [A-Z]+[0-9]+:`, record the `(source file name, diagnostic identifier)` pairs derived
      from them as BASELINE_TYPECHECK_ERRORS, and state whether any pair names one of the three files
      this plan changes. If one does, resolve the drift before Phase 1. If none does, the
      carry-forward disposition is that `[P4-T4]` replaces its exit-code-0 and `0 Error(s)` clauses
      with the clause that its own error pair set equals BASELINE_TYPECHECK_ERRORS. The same
      no-scoped-re-reading constraint recorded in `[P0-T9]` applies here. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/msbuild-nullable.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P0-T11] Probe the scoped test-runner form by running
      `pwsh -NoProfile -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -NoExecute`.
      The `-NoExecute` switch returns at line 125 after the discovery and count logic on lines 107
      through 120 has run, so the probe exercises exactly the scalar-versus-array question without
      launching vstest. Acceptance: the artifact records the exit code and the complete stdout and
      stderr verbatim, and states which of the two outcomes occurred: the line
      `Discovered 1 test assemblies.` was printed, or a terminating error was raised. No later task in
      this plan depends on the outcome; the record exists to justify this plan's use of
      `-SearchRoot .` and to supply evidence for issue #713, which was opened for this defect during
      preparation. Do not fix that defect here; it is out of scope for issue #663. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/scoped-runner-probe.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P0-T12] Capture the baseline repository-wide test state by running
      `pwsh -NoProfile -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug`.
      Acceptance: the artifact records the printed `Discovered N test assemblies.` line, the
      `Total tests:`, `Passed:` and `Failed:` figures, the run's not-run figure recorded as
      BASELINE_NOT_RUN, the runner's summary block transcribed verbatim, and the verbatim list of
      every failing test name as BASELINE_FAILURE_SET. BASELINE_NOT_RUN is the skipped or
      inconclusive count the runner actually prints, read under whatever label it actually uses — a
      `Skipped:` line of its own in the multi-line vstest summary, or the `Skipped:` field of the
      single-line `Failed! - Failed: N, Passed: N, Skipped: N, Total: N` summary. The label is fixed
      by the transcribed summary block rather than assumed, because this plan has not observed a
      successful run of this wrapper. If the runner prints no such figure under any label,
      BASELINE_NOT_RUN is instead derived arithmetically as `Total tests:` minus `Passed:` minus
      `Failed:`, and the artifact states which of the two routes was used, so every later run reads
      the figure the same way. The exit code is recorded but is not the gate, because the wrapper
      throws at line 130 on any failure. A run in which the discovered-assembly count is zero is a
      discovery defect and fails this task rather than being read as an empty suite. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/tests.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-8 and AC-10.
- [ ] [P0-T13] Capture baseline coverage by running
      `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/663-baseline.cobertura.xml`,
      then applying the post-processing out of band with
      `pwsh -NoProfile -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; $raw = Get-Content -LiteralPath "coverage/663-baseline.cobertura.xml" -Raw -Encoding UTF8; $p = ConvertTo-KoverageCoberturaXml -XmlContent $raw -RepoRoot (Get-Location).Path; Set-Content -LiteralPath "coverage/663-baseline.processed.cobertura.xml" -Value $p -Encoding UTF8 -NoNewline'`.
      Acceptance: the artifact records, read from the post-processed document, the root `line-rate`,
      `branch-rate`, `lines-covered`, `lines-valid`, `branches-covered` and `branches-valid`
      attributes as numeric values; the `line-rate` attribute of the `class` element whose `filename`
      attribute ends with `QfcFormKeyHandler.cs`, recorded as BASELINE_CLASS_LINE_RATE together with
      that element's `name` attribute verbatim. If the post-processed document contains no `class`
      element whose `filename` ends with `QfcFormKeyHandler.cs`, record that fact verbatim, take the
      same reading from the raw pre-processing document `coverage/663-baseline.cobertura.xml`, and
      record which of the two documents BASELINE_CLASS_LINE_RATE was taken from. `[P4-T7]` must then
      read its comparison figure from the same document kind, so the two figures are commensurable.
      That branch is required because the element's survival is a property of the post-processing
      rather than of the run: `ConvertTo-KoverageCoberturaXml` removes every `<package>` element
      outside the project allowlist at `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` lines
      417 through 421 and then calls `Remove-CoberturaExemptClosureCoverage` on line 427 and
      `Merge-CoberturaClassesByFilename` on line 428. Also record the observation that the document
      BASELINE_CLASS_LINE_RATE was taken from contains no `method` element named `ClaimsAltChord`
      under the `class` element whose `filename`
      attribute ends with `QfcFormKeyHandler.cs`. Record separately, and as a measured reading rather
      than as a prediction, whether a `method` element of that name exists under the `class` element
      whose `filename` ends with `EfcViewer.cs`. A present reading additionally corroborates that the
      `QuickFiler` `<package>` survived the pruning; an absent reading is uninformative on that
      question, because the class-level exclusion attribute produces the same result. The pruning
      question is settled independently by the clause above: if the `<class>` element whose `filename`
      ends with `QfcFormKeyHandler.cs` is absent from the post-processed document, this task already
      falls back to the raw document. Either answer is admissible and neither fails this task, because
      two mechanisms pull in opposite directions and only the reading settles which prevails. Toward
      presence: QuickFiler/Viewers/EfcViewer.cs line
      96 declares the delivered Email Filer predicate of the same name, that file is a compile item
      of `QuickFiler/QuickFiler.csproj` at line 389, `Get-KoverageProjectAllowlist` at
      `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` lines 4 through 48 builds the allowlist
      from every non-`.Test` project assembly name, with the `.Test` suffix skip at lines 40 through
      42 and the add at line 44, so the `QuickFiler` `<package>` survives the
      pruning loop on lines 417 through 421, `Merge-CoberturaClassesByFilename` at lines 262 through
      391 deep-clones the primary node at line 295 and installs that clone in its place at line 382,
      rebuilding only the `<lines>` subtree at lines 303 through 310 and 360 through 363, so the
      cloned `<methods>` subtree survives the merge, and
      QuickFiler.Test/Controllers/EfcViewerTests.cs lines 111 through 162 exercise the member. Toward
      absence: the declaring type carries, at QuickFiler/Viewers/EfcViewer.cs line 20, the
      coverage-exclusion attribute AC-13 forbids adding, and coverage.config overrides only
      `ModulePaths`, leaving the collector's default attribute-based exclusion in force; that is the
      mechanism the spec's placement rationale relies on when it states that only the
      `QfcFormKeyHandler` placement produces a `<method>` element in the Cobertura output. Record
      also the verbatim list of every failing test name this
      instrumented run reports, recorded as BASELINE_COVERAGE_FAILURE_SET. That set is captured
      separately from the `[P0-T12]` BASELINE_FAILURE_SET because instrumentation adds load-driven
      failures, so the two sets are not interchangeable. The exit code is recorded but is not the
      gate, for the reason stated in the reading guide. Both coverage documents stay under the gitignored `coverage`
      directory and are not committed. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/coverage.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`, and render any absolute
      worktree path as `<repo-root>`. Serves AC-11.
- [ ] [P0-T14] Record the pre-change structural state so that every later structural gate has a
      false-before reading to be compared against. Run and record, each with its match count and
      matched lines: `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'IsAltKeyCommand'`
      (expected one match, on line 60); `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'ClaimsAltChord'`
      (expected zero matches); `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'Keys\.Alt'`
      (expected zero matches); VC-2 against `QuickFiler/Viewers/QfcFormViewer.cs` (expected two
      matches, on lines 64 and 65); VC-1 against `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`
      (expected zero matches); `Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern 'IsAltKeyCommand_'`
      (expected four declaration matches, one for each existing test method). Acceptance: all six
      counts are recorded in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/pre-change-structure.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-7, AC-8, AC-12 and
      AC-14.

### Phase 1 — Behaviour-preserving predicate seam

- [ ] [P1-T1] Add the member `ClaimsAltChord` to the existing type `QfcFormKeyHandler` in
      `QuickFiler/Controllers/QfcFormKeyHandler.cs`, in its behaviour-preserving intermediate form,
      together with the `using QuickFiler.Interfaces;` directive the parameter type requires. The
      intermediate body is exactly
      `internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData) =>` followed by
      `handler is not null && keyData.HasFlag(Keys.Alt);`. Do not modify `IsAltKeyCommand` on line 18
      and do not add any `[ExcludeFromCodeCoverage]` attribute. An XML documentation comment on the new
      member is permitted. Acceptance:
      `Select-String -Path QuickFiler/Controllers/QfcFormKeyHandler.cs -Pattern 'ClaimsAltChord'`
      returns at least one match and one of the matched lines is the member declaration, which is a
      change from the zero matches this file returns at branch head;
      `Select-String -Path QuickFiler/Controllers/QfcFormKeyHandler.cs -Pattern 'Keys\.KeyCode'`
      returns zero matches, which is what distinguishes the intermediate seam from the final form; and
      `Select-String -Path QuickFiler/Controllers/QfcFormKeyHandler.cs -Pattern 'IsAltKeyCommand'`
      returns exactly one match. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/seam-predicate.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-1 and AC-8.
- [ ] [P1-T2] Replace the guard on lines 58 through 61 of `QuickFiler/Viewers/QfcFormViewer.cs` with
      the single-line condition quoted verbatim in the reading guide. Retain the body of the branch
      unchanged, including the pre-existing locals `object sender = FromHandle(msg.HWnd)` and
      `var e = new KeyEventArgs(keyData)` and the assignment `e.Handled = true`, and retain the
      parameterless `ToggleKeyboardDialogAsync()` dispatch. Acceptance:
      `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'ClaimsAltChord'` returns
      exactly one match and that match is inside `ProcessCmdKey`;
      `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'IsAltKeyCommand'` returns
      zero matches; and VC-2 against the same file still returns two matches. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/seam-viewer-guard.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-7 and AC-14.
- [ ] [P1-T3] Prove the seam compiles and introduces no new analyzer diagnostic by running the
      analyzer gate command quoted in the Toolchain command forms section, writing its detailed log to
      `coverage\663-analyzers-seam.msbuild.log`. This gate exists because routing the viewer through
      the new predicate removes the last compiled consumer of `IsAltKeyCommand`, which could trip an
      unused-member diagnostic. Acceptance: exit code 0, or, if BASELINE_ANALYZER_EXIT is non-zero,
      the carry-forward disposition `[P0-T9]` recorded in its place; the log contains at least one
      occurrence of
      the literal `Task "Csc"`; and the set of `(source file name, diagnostic identifier)` pairs taken
      from console lines matching `: warning [A-Z]+[0-9]+:` that name `QfcFormKeyHandler.cs`,
      `QfcFormViewer.cs` or `QfcFormKeyHandlerTests.cs` is equal to the same set derived from
      BASELINE_WARNINGS in `[P0-T9]`. The comparison is on pairs rather than on whole lines because
      `[P1-T2]` shifts the line and column numbers a verbatim warning line carries.
      If a new diagnostic appears, fix the root diagnostic; deleting `IsAltKeyCommand` is prohibited
      because AC-8 requires it to survive unchanged. Delete the detailed log after recording its byte
      size. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/seam-build.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-8 and AC-10.

### Phase 2 — Regression tests captured red

- [ ] [P2-T1] Add the seven test methods named in the reading guide to the existing file
      `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`, inside the existing
      `QfcFormKeyHandlerTests` class in namespace `QuickFiler.Controllers.Tests`. Add
      `using Moq;` and `using QuickFiler.Interfaces;` to that file. Use MSTest `[TestMethod]`,
      Arrange-Act-Assert, a `Mock<IQfcKeyboardHandler>` for the handler argument, and a
      FluentAssertions because-string on every assertion, following the shape of the delivered Email
      Filer fixture at QuickFiler.Test/Controllers/EfcViewerTests.cs lines 112 through 162. Do not
      modify the four existing `IsAltKeyCommand_*` methods. Do not modify the class-level XML summary
      on lines 8 through 11 of that file: line 9 names `IsAltKeyCommand`, so rewriting the summary to
      mention the new methods produces a removed line containing that identifier and fails `[P5-T3]`,
      which is an AC-8 gate about something else entirely. Document the new methods with their own
      per-method comments instead. No test may construct, show, or derive
      from a `System.Windows.Forms.Form`, and no test may use a temporary file, `Thread.Sleep` or
      `Task.Delay`. Acceptance: the file declares exactly eleven `[TestMethod]` attributes, each of the
      seven method names appears exactly once, VC-1 against the file returns zero matches, and
      `Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern 'Keys\.Control'`
      returns at least two matches with at least one matched line inside the body of
      `ClaimsAltChord_WithoutAltFlag_ReturnsFalse`. That last clause is a change detector: at branch
      head the pattern returns exactly one match, on line 45 inside
      `IsAltKeyCommand_WithControlKey_ReturnsFalse`, so without it no acceptance condition in this plan
      would change value if the second assertion AC-5 requires were omitted. Record
      in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/regression-testing/tests-added.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-2, AC-3, AC-4,
      AC-5, AC-6 and AC-12.
- [ ] [P2-T2] Compile the solution so the red run is a runtime observation rather than a build break,
      by running
      `pwsh -NoProfile -Command '$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"'`.
      Acceptance: exit code 0 and the `LastWriteTimeUtc` of `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
      is strictly later than the value captured immediately before the command ran, which proves the
      test assembly was recompiled rather than skipped. Record both timestamps in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/regression-testing/red-build.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P2-T3] [expect-fail] Capture the red run by running
      `pwsh -NoProfile -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug`.
      Acceptance: the run's failing-test name list contains exactly the three names
      `ClaimsAltChord_WithAltM_ReturnsFalse`, `ClaimsAltChord_WithAltF4_ReturnsFalse` and
      `ClaimsAltChord_WithAltLeft_ReturnsFalse` in addition to a subset of BASELINE_FAILURE_SET from
      `[P0-T12]`, and contains no other name; and the `Total tests:` figure equals the `[P0-T12]`
      baseline total plus seven. Record the artifact at
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/regression-testing/red-run.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 1` and `Output Summary:`,
      including the verbatim failing-test list, with each failing name recorded alongside the
      declaring type read from its stack trace. The declaring type is required because
      `QuickFiler.Test` already declares `ClaimsAltChord_WithAltM_ReturnsFalse` at
      QuickFiler.Test/Controllers/EfcViewerTests.cs:134 and
      `ClaimsAltChord_WithNullHandler_ReturnsFalse` at
      QuickFiler.Test/Controllers/EfcViewerTests.cs:156, so two of the seven new method names are not
      unique within the assembly. Serves AC-3 and AC-4.

### Phase 3 — Minimal fix and green run

- [ ] [P3-T1] Replace the intermediate body of `ClaimsAltChord` in
      `QuickFiler/Controllers/QfcFormKeyHandler.cs` with the final source form quoted verbatim in the
      reading guide, which adds the null and Alt-flag guard, the `keyData & Keys.KeyCode` mask, and
      the acceptance of `Keys.Menu` or `Keys.None` only. Change nothing else in the file; in
      particular do not modify `IsAltKeyCommand` and do not add an `[ExcludeFromCodeCoverage]`
      attribute. Acceptance:
      `Select-String -Path QuickFiler/Controllers/QfcFormKeyHandler.cs -Pattern 'Keys\.KeyCode'`
      returns at least one match, which is a change from the zero matches `[P1-T1]` recorded;
      `Select-String -Path QuickFiler/Controllers/QfcFormKeyHandler.cs -Pattern 'Keys\.Menu'`
      returns at least one match; and
      `Select-String -Path QuickFiler/Controllers/QfcFormKeyHandler.cs -Pattern 'IsAltKeyCommand'`
      still returns exactly one match. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/fix-predicate.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-1 and AC-8.
- [ ] [P3-T2] Recompile with the same `/t:Build` command as `[P2-T2]`. Acceptance: exit code 0 and the
      `LastWriteTimeUtc` of `QuickFiler\bin\Debug\QuickFiler.dll` is strictly later than the value
      captured immediately before the command ran. Record both timestamps in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/fix-build.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P3-T3] Capture the green run by running
      `pwsh -NoProfile -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug`.
      Acceptance: none of the three names listed in `[P2-T3]` appears in the failing-test list; every
      remaining failing name, if any, is a member of BASELINE_FAILURE_SET; no failing name belongs to
      `QfcFormKeyHandlerTests`; no failing name is `ExecutingAssembly_ContainsNoFormDerivedType`;
      the `Total tests:` figure equals the `[P0-T12]` baseline total plus
      seven; and the run's not-run figure, read under the rule `[P0-T12]` fixes and recorded here
      alongside the transcribed summary block, equals BASELINE_NOT_RUN. By the derivation stated in
      the reading guide, those observations establish that all
      seven new methods, the four existing `IsAltKeyCommand_*` methods and
      `ExecutingAssembly_ContainsNoFormDerivedType` all passed. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/regression-testing/green-run.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`, including the verbatim
      failing-test list and the total-count arithmetic. Serves AC-1, AC-2, AC-3, AC-4, AC-5, AC-6,
      AC-8 and AC-12.

### Phase 4 — Final C# QA loop

Run stages in order: format, then analyzers, then type-check, then tests, then coverage. If any stage
fails or rewrites a tracked file, restart this phase from `[P4-T1]`.

- [ ] [P4-T1] Apply formatting with three scoped invocations, one per changed path:
      `dotnet tool run csharpier format QuickFiler/Controllers/QfcFormKeyHandler.cs`, then
      `dotnet tool run csharpier format QuickFiler/Viewers/QfcFormViewer.cs`, then
      `dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`. The
      pass is scoped rather than repository-wide so a rewrite of an unrelated file cannot widen the
      change set that AC-14 pins. Because a formatter rewrites tracked source and still exits 0 after
      rewriting, the exit code alone cannot distinguish a clean run from a repairing one. A
      `git status --porcelain` span cannot distinguish them either at this point in the plan: the
      three files are already modified relative to `HEAD` and remain so whether or not CSharpier
      rewrites them, because `[P5-T1]` is the first task that commits them. The required observation
      is therefore the SHA-256 of each of the three files captured immediately before the three
      invocations and again immediately after. Acceptance: all three invocations exit 0, all six
      hashes are recorded verbatim, and the artifact states for each file whether its hash changed.
      Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/csharpier-format.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P4-T2] Verify formatting repository-wide and read-only with `dotnet tool run csharpier check .`.
      Acceptance: exit code 0 and no reported unformatted path, which is the same reading `[P0-T8]`
      recorded as BASELINE_CSHARPIER_EXIT if that baseline was 0; if it was not, apply the
      carry-forward disposition `[P0-T8]` recorded. Under that disposition the scoped reading is
      taken with three additional read-only invocations,
      `dotnet tool run csharpier check QuickFiler/Controllers/QfcFormKeyHandler.cs`,
      `dotnet tool run csharpier check QuickFiler/Viewers/QfcFormViewer.cs` and
      `dotnet tool run csharpier check QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`, each of
      which must exit 0; the repository-wide exit code and the carried-forward pre-existing set are
      both recorded alongside them. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/csharpier-check.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P4-T3] Run the analyzer gate command quoted in the Toolchain command forms section, writing its
      detailed log to `coverage\663-analyzers.msbuild.log`. Acceptance: exit code 0 and the console
      output carries a summary line matching `^\s*0 Error\(s\)$` and no line matching the MSBuild
      diagnostic form `: error [A-Z]+[0-9]+:`, or, if BASELINE_ANALYZER_EXIT is non-zero, the
      carry-forward disposition `[P0-T9]` recorded in place of those three clauses; the log contains
      at least one occurrence of the literal
      `Task "Csc"` and the count is recorded; and the set of `(source file name, diagnostic
      identifier)` pairs taken from console lines matching `: warning [A-Z]+[0-9]+:` that name
      `QfcFormKeyHandler.cs`, `QfcFormViewer.cs` or `QfcFormKeyHandlerTests.cs` is equal to the same
      set derived from BASELINE_WARNINGS in `[P0-T9]`, using the same pair comparison `[P1-T3]` uses.
      The comparison is baseline-relative rather than absolute because
      `.github/workflows/_build-analyzers.yml` lines 50 through 52 run this command with no
      `/p:TreatWarningsAsErrors=true`, so an analyzer warning naming one of these files can already
      exist on `origin/main`; an absolute-zero clause would then be unsatisfiable without an edit the
      spec's non-goals forbid. If BASELINE_WARNINGS is empty, the two formulations coincide. A bare
      search for the
      word `error` is not used because a successful MSBuild run prints the `/errorreport:prompt` token
      on every Csc command line and prints the `0 Error(s)` summary, so that search matches on a clean
      run and the gate could never pass. Delete the detailed
      log after recording its byte size. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/msbuild-analyzers.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P4-T4] Run the type-check gate command quoted in the Toolchain command forms section, writing
      its detailed log to `coverage\663-nullable.msbuild.log`. Acceptance: exit code 0 and the console
      output carries a summary line matching `^\s*0 Error\(s\)$` and no line matching the MSBuild
      diagnostic form `: error [A-Z]+[0-9]+:`, for the reason given in `[P4-T3]`, or, if
      BASELINE_TYPECHECK_EXIT is non-zero, the carry-forward disposition `[P0-T10]` recorded in place
      of those three clauses; the recorded
      `Command:` value contains no occurrence of `Nullable=enable`;
      and the log contains at least one occurrence of the literal `Task "Csc"` and the count is
      recorded. No warning clause is asserted on this gate. `/p:TreatWarningsAsErrors=true` promotes
      every compiler and analyzer warning to an error, so an exit-0 run has by construction emitted no
      such warning anywhere in the solution; the exit-code, `^\s*0 Error\(s\)$` and
      `: error [A-Z]+[0-9]+:` clauses already carry the whole assertion, and a warning clause here
      would return the same value whatever the executor wrote.
      Delete the
      detailed log after recording its byte size. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/msbuild-nullable.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-10.
- [ ] [P4-T5] Run the final repository-wide test gate with
      `pwsh -NoProfile -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug`.
      Acceptance: the `Total tests:` figure equals the `[P0-T12]` baseline total plus seven; every
      failing name, if any, is a member of BASELINE_FAILURE_SET; no failing name belongs to
      `QfcFormKeyHandlerTests` or is `ExecutingAssembly_ContainsNoFormDerivedType`; and the run's
      not-run figure, read under the rule `[P0-T12]` fixes and recorded here alongside the
      transcribed summary block, equals BASELINE_NOT_RUN. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/tests-final.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-8, AC-10 and AC-12.
- [ ] [P4-T6] Run the post-change coverage collection with
      `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/coverage.cobertura.xml`,
      which is the output path AC-11 names. Acceptance: the Cobertura document exists at that path and
      its byte size is recorded; the run's failing-test list is recorded verbatim and contains no name
      outside BASELINE_COVERAGE_FAILURE_SET from `[P0-T13]`; and the exit code is recorded but is not
      the gate, for the reason stated in the reading guide. The comparison uses the instrumented
      baseline rather than the `[P0-T12]` uninstrumented one because instrumentation adds load-driven
      failures, so the two sets are not interchangeable. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/coverage-run.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-11.
- [ ] [P4-T7] Post-process the coverage document in place and extract the required figures, by running
      `pwsh -NoProfile -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; $f = "docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/coverage.cobertura.xml"; $raw = Get-Content -LiteralPath $f -Raw -Encoding UTF8; $p = ConvertTo-KoverageCoberturaXml -XmlContent $raw -RepoRoot (Get-Location).Path; Set-Content -LiteralPath $f -Value $p -Encoding UTF8 -NoNewline'`.
      Acceptance, all read from the post-processed document and recorded verbatim in the artifact: the
      document contains a `method` element whose `name` attribute is `ClaimsAltChord`, under the class
      whose `filename` attribute ends with `QfcFormKeyHandler.cs`, and that element's `line-rate`
      attribute parses to a value of at least 0.90; that class element's own `line-rate` attribute is
      not lower than BASELINE_CLASS_LINE_RATE from `[P0-T13]`, read from the same document kind
      `[P0-T13]` recorded that figure from, so the two figures are commensurable; and the root
      `line-rate`,
      `lines-covered` and `lines-valid` attributes are recorded alongside the `[P0-T13]` baseline
      values with the difference stated. If `[P0-T13]` recorded BASELINE_CLASS_LINE_RATE from the raw
      pre-processing document rather than the post-processed one, take every class-scoped and
      method-scoped reading here from the raw document as well, and take it before running the
      post-processing command quoted above: that command writes the post-processed text back to the
      same path, so the raw document does not survive it. The artifact must carry an explicit
      `AC-11 evidence of record:` line stating that `coverage.cobertura.xml` was transcribed into this
      file and then deleted, and that the two verbatim XML fragments recorded here — the `<method>`
      element whose `name` is `ClaimsAltChord` under the `<class>` element whose `filename` ends with
      `QfcFormKeyHandler.cs`, and that `<class>` element itself — are the evidence AC-11's
      verification names. The class qualifier is part of the identification rather than decoration,
      for the reason `[P0-T13]` records: `ClaimsAltChord` is not a unique method name in the
      instrumented tree.
      After the figures and the two XML fragments are transcribed,
      delete `coverage.cobertura.xml` and record its byte size and the deletion, mirroring the
      disposition feature #464 recorded for the same class of artifact; raw Cobertura is
      machine-generated measurement data of order ten megabytes and is not committed in this
      repository. Render any absolute worktree path as `<repo-root>`. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/coverage-final.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-11.

### Phase 5 — Commit and scope verification gates

Every gate in this phase runs against committed state, because a three-dot diff compares the merge base
to the `HEAD` commit and cannot see uncommitted work. `[P5-T1]` therefore commits first. Each
name-listing diff below is paired with a `git status --porcelain` span in the same task: the two
mechanisms are complementary and each alone is wrong in one state, since the anchored diff is blind to
an untracked file while porcelain status goes empty once the change is committed.

- [ ] [P5-T1] Stage and commit the three changed source files with a conventional-commit message
      naming issue #663. Acceptance: `git status --porcelain -- '*.cs'` prints nothing, and
      `git diff --name-only origin/main...HEAD -- '*.cs'` prints exactly three lines. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/code-commit.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-14.
- [ ] [P5-T2] Verify the predicate structure of the viewer. Run
      `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'ClaimsAltChord'`,
      `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'Keys\.Alt'` and
      `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'IsAltKeyCommand'`.
      Acceptance: the first returns exactly one match and the matched line lies inside the
      `ProcessCmdKey` method body, the second returns zero matches, and the third returns zero
      matches, which is a change from the single match `[P0-T14]` recorded before the fix. The
      `ClaimsAltChord` and `IsAltKeyCommand` clauses are change detectors, reading zero before and one
      after and one before and zero after respectively. The `Keys.Alt` clause is an invariant guard
      rather than a change detector: `[P0-T14]` records zero matches at branch head and AC-7 requires
      that the rewritten guard introduce none, so it fails only if the executor inlines a modifier test
      into the viewer. Record all three commands and their output in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/663-predicate-structure.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-7.
- [ ] [P5-T3] Verify that `IsAltKeyCommand` survives unchanged. Run
      `git diff -U0 origin/main...HEAD -- QuickFiler/Controllers/QfcFormKeyHandler.cs QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`
      and, in the same task, `git status --porcelain -- QuickFiler/Controllers/QfcFormKeyHandler.cs QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`.
      Acceptance: no line of the diff that begins with a single `-` character contains
      `IsAltKeyCommand`, and the porcelain span prints nothing. Record the full diff and the porcelain
      output in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/isaltkeycommand-unchanged.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-8.
- [ ] [P5-T4] Verify that neither project file changed. Run
      `git diff --name-only origin/main...HEAD` and, in the same task,
      `git status --porcelain`. Acceptance: the diff output contains no line equal to
      `QuickFiler/QuickFiler.csproj` and no line equal to `QuickFiler.Test/QuickFiler.Test.csproj`,
      and the porcelain output contains neither path. Record both outputs in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/csproj-untouched.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-9.
- [ ] [P5-T5] Verify the test-shape prohibitions. Run VC-1 against
      `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`. Acceptance: zero matches, matching the
      `[P0-T14]` pre-change reading, and the `[P4-T5]` artifact records that
      `ExecutingAssembly_ContainsNoFormDerivedType` is not in the failing list. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/no-live-form.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-12.
- [ ] [P5-T6] Verify that no coverage exemption was introduced. Run
      `git diff -U0 origin/main...HEAD -- '*.cs'` and, in the same task, `git status --porcelain`.
      The diff is scoped to `.cs` paths because the documentation commits already on this branch add
      twenty lines that quote the attribute name in prose, so an unscoped diff reports twenty `+`
      matches before any source edit is made; scoping preserves the gate's discrimination because
      AC-13 is about the change set's C# content. Acceptance:
      no line of the diff that begins with a single `+` character contains `ExcludeFromCodeCoverage`,
      and the porcelain output reports no path ending in `.cs`. The porcelain span is scoped to `.cs`
      paths rather than required to be empty, because evidence artifacts written by Phases 0 through 4
      are still untracked at this point and `.claude/agent-memory` is a tracked directory that
      unrelated agent activity can leave modified. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/no-new-exemption.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-13.
- [ ] [P5-T7] Verify the change set and the retained locals. Run
      `git diff --name-only origin/main...HEAD -- '*.cs'`, then in the same task
      `git status --porcelain`, then VC-2 against `QuickFiler/Viewers/QfcFormViewer.cs`. Acceptance:
      the diff lists exactly the three paths `QuickFiler/Controllers/QfcFormKeyHandler.cs`,
      `QuickFiler/Viewers/QfcFormViewer.cs` and
      `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` and no other; the porcelain output
      reports no path ending in `.cs`; and VC-2 returns exactly two matches, one for each of the two
      literals, both inside `ProcessCmdKey`, which confirms the pre-existing unused locals were
      retained. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/change-set.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-14.
- [ ] [P5-T8] Verify the justification wording of the mnemonic test. Run
      `Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern 'Move Options'`
      and `Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern 'Filters menu'`.
      Acceptance: the first returns at least one match and at least one matched line lies inside the
      body of `ClaimsAltChord_WithAltM_ReturnsFalse`; the second returns zero matches. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/because-string.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-3.

### Phase 6 — Manual validation, acceptance check-off and closure

Each check-off task below flips exactly one checkbox in the `### Acceptance-criteria checklist` section
of `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md`. Identifiers in that
list share a prefix, so every verification search must include the space that follows the identifier:
searching for `- [x] AC-1` alone would also match the AC-10 through AC-15 lines, whereas
`- [x] AC-1 ` with the trailing space matches only the AC-1 line.

- [ ] [P6-T1] Record the live-host manual validation at
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/other/manual-validation.md`.
      For each of the three gestures bare Alt, Alt+M and Alt+F4, the record must carry either an
      observed outcome naming the Outlook build, or the status `MANUAL_CHECK_DEFERRED` accompanied by
      the two measured probes `Get-Process -Name OUTLOOK` with its returned count and
      `[Environment]::UserInteractive` with its returned value, plus a statement of what the automated
      tests do and do not establish. A deferral is an acceptable outcome; recording a pass on
      assertion, or omitting a gesture, is not. Acceptance: the artifact carries `Timestamp:`,
      `Command:`, `EXIT_CODE:` and `Output Summary:`, names all three gestures, and for each names
      either an observed outcome or the deferral status with both probe values. Serves AC-15.
- [ ] [P6-T2] Check off AC-1 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-1 '`
      returns exactly one match, and the `[P3-T3]` and `[P5-T2]` artifacts both exist. Serves AC-1.
- [ ] [P6-T3] Check off AC-2 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-2 '`
      returns exactly one match, and the `[P3-T3]` artifact records that neither
      `ClaimsAltChord_WithBareAltFlagAndHandler_ReturnsTrue` nor
      `ClaimsAltChord_WithMenuKeyCodeAndAltFlag_ReturnsTrue` appears in its failing list. Serves AC-2.
- [ ] [P6-T4] Check off AC-3 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-3 '`
      returns exactly one match, and the `[P5-T8]` artifact exists. Serves AC-3.
- [ ] [P6-T5] Check off AC-4 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-4 '`
      returns exactly one match, and the `[P2-T3]` artifact records
      `ClaimsAltChord_WithAltF4_ReturnsFalse` and `ClaimsAltChord_WithAltLeft_ReturnsFalse` as failing
      before the fix while the `[P3-T3]` artifact records neither as failing after it. Serves AC-4.
- [ ] [P6-T6] Check off AC-5 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-5 '`
      returns exactly one match, and the `[P3-T3]` artifact records that
      `ClaimsAltChord_WithoutAltFlag_ReturnsFalse` is absent from its failing list. Serves AC-5.
- [ ] [P6-T7] Check off AC-6 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-6 '`
      returns exactly one match, and the `[P3-T3]` artifact records that
      `ClaimsAltChord_WithNullHandler_ReturnsFalse` is absent from its failing list. Serves AC-6.
- [ ] [P6-T8] Check off AC-7 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-7 '`
      returns exactly one match, and the `[P5-T2]` artifact exists. Serves AC-7.
- [ ] [P6-T9] Check off AC-8 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-8 '`
      returns exactly one match, and the `[P5-T3]` artifact exists. Serves AC-8.
- [ ] [P6-T10] Check off AC-9 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-9 '`
      returns exactly one match, and the `[P5-T4]` artifact exists. Serves AC-9.
- [ ] [P6-T11] Check off AC-10 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-10 '`
      returns exactly one match, and all five artifacts from `[P4-T1]` through `[P4-T5]` exist and each
      carries the four required fields. Serves AC-10.
- [ ] [P6-T12] Check off AC-11 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-11 '`
      returns exactly one match, and the `[P4-T7]` artifact records the line-rate of the
      `ClaimsAltChord` `method` element under the `class` element whose `filename` ends with
      `QfcFormKeyHandler.cs`, and the class line-rate comparison against BASELINE_CLASS_LINE_RATE.
      Serves AC-11.
- [ ] [P6-T13] Check off AC-12 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-12 '`
      returns exactly one match, and the `[P5-T5]` artifact exists. Serves AC-12.
- [ ] [P6-T14] Check off AC-13 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-13 '`
      returns exactly one match, and the `[P5-T6]` artifact exists. Serves AC-13.
- [ ] [P6-T15] Check off AC-14 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-14 '`
      returns exactly one match, and the `[P5-T7]` artifact exists. Serves AC-14.
- [ ] [P6-T16] Check off AC-15 in the spec checklist. Acceptance:
      `Select-String -Path docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md -Pattern '- \[x\] AC-15 '`
      returns exactly one match, and the `[P6-T1]` artifact exists. Serves AC-15.
- [ ] [P6-T17] Mirror the issue update locally at
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/issue-updates/issue-663.2026-08-31T20-16.md`,
      carrying `Timestamp:`, the exact text of the update, and one of `PostedAs: body`,
      `PostedAs: comment` or `PostedAs: unknown`; if posting is not performed, the artifact carries a
      `POSTING BLOCKED` header and the reason. The text must state the corrected symptom: Alt+M is the
      swallowed mnemonic on this surface and Alt+F is not, because `ButtonFilters.Text` is the plain
      string `"Filters"` with no ampersand. Acceptance: the artifact exists and carries `Timestamp:`
      and a `PostedAs:` or `POSTING BLOCKED` line. Serves AC-3.
- [ ] [P6-T18] Commit every remaining change, including this plan file, the spec checklist edits and
      all evidence artifacts. Stage paths explicitly rather than with an all-paths stage: a blanket
      stage sweeps an unrelated queued promotion file from `docs/features/potential/` onto this branch.
      Before committing, run `git status --porcelain` and confirm that every listed path lies under
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/` or under
      `.claude/agent-memory/`; any other path must be dispositioned in the artifact rather than
      silently committed. Acceptance: `git status --porcelain`, run immediately after the commit and
      before either this task's artifact is written or this task's own checkbox is flipped, prints
      nothing. The artifact written next, and this task's own check-off in the plan file, are
      expected residues that `[P6-T19]` folds into this commit by amendment; the artifact cannot be
      inside the commit it describes, because it records that commit's `EXIT_CODE:`. If that reading
      shows a path under `.claude/agent-memory/` that unrelated agent activity produced between the
      stage and the commit, stage it, amend, and re-run the reading; the gate remains "prints
      nothing" rather than being relaxed.
      Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/final-commit.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-14.
- [ ] [P6-T19] Confirm the end state after the documentation commits. Run
      `git diff --name-only origin/main...HEAD -- '*.cs'` and, in the same task,
      `git status --porcelain`. Acceptance: the diff still lists exactly the same three `.cs` paths
      recorded by `[P5-T7]` and no other, and the porcelain output lists at most
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/plan.2026-08-31T20-16.md`,
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/final-commit.md`
      and
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/end-state.md`,
      with no `.cs` path among them. Those three residues are structural: this task writes
      `end-state.md`; `[P6-T18]` writes `final-commit.md` after its own commit, because that artifact
      records the commit's `EXIT_CODE:` and so cannot be inside the commit it describes; and the plan
      file carries the `[P6-T18]` check-off that could only be made after
      `[P6-T18]` committed. The list is an at-most rather than an exactly, because `end-state.md`
      does not yet exist when this task takes the porcelain reading above. A path under
      `.claude/agent-memory/` is the one admitted addition to that list: the directory is tracked and
      unrelated agent activity can dirty it inside the short window between the `[P6-T18]` commit and
      this reading, which is the same reason `[P5-T6]` scopes its own porcelain span. Such a path is
      recorded in the artifact and folded into the amend below alongside the three structural
      residues; it does not fail this gate, and no other unlisted path is admitted. After recording the
      artifact, flip this task's own checkbox to `[x]` in the
      plan file, which its acceptance permits at that point because the artifact is already written,
      then stage exactly those three paths — the plan file, `final-commit.md` and `end-state.md` —
      with an explicit `git add` naming each, and fold them into the `[P6-T18]` commit with
      `git commit --amend --no-edit`, then run `git status --porcelain` once more and record that it
      prints nothing. The post-amend `git status --porcelain` result is reported in this task's
      progress output and is deliberately not appended to `end-state.md`: appending it would modify a
      file the amend has just committed and would reopen the residue this task closes. `end-state.md`
      is written exactly once, before the amend, and carries the `git diff --name-only` output, the
      pre-amend porcelain output, and an explicit statement that the post-amend porcelain run is
      recorded in the progress output rather than in this file.
      The check-off precedes the amend because performing it afterwards would leave the
      plan file modified and uncommitted, which is the state this task exists to close. Record in
      `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/end-state.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Serves AC-14.

---

## Acceptance-criteria traceability

| AC | Implementation tasks | Test tasks | Evidence tasks |
|---|---|---|---|
| AC-1 | [P1-T1], [P3-T1] | [P2-T1], [P3-T3] | [P3-T1], [P6-T2] |
| AC-2 | [P3-T1] | [P2-T1], [P3-T3] | [P3-T3], [P6-T3] |
| AC-3 | [P3-T1] | [P2-T1], [P2-T3], [P3-T3] | [P5-T8], [P6-T4], [P6-T17] |
| AC-4 | [P3-T1] | [P2-T1], [P2-T3], [P3-T3] | [P2-T3], [P6-T5] |
| AC-5 | [P3-T1] | [P2-T1], [P3-T3] | [P3-T3], [P6-T6] |
| AC-6 | [P3-T1] | [P2-T1], [P3-T3] | [P3-T3], [P6-T7] |
| AC-7 | [P1-T2] | [P3-T3] | [P0-T14], [P5-T2], [P6-T8] |
| AC-8 | [P1-T1], [P3-T1] | [P0-T12], [P3-T3], [P4-T5] | [P0-T14], [P1-T3], [P5-T3], [P6-T9] |
| AC-9 | [P1-T1], [P1-T2], [P2-T1] | [P4-T5] | [P5-T4], [P6-T10] |
| AC-10 | [P0-T3], [P0-T4], [P0-T5], [P0-T6], [P0-T7], [P2-T2], [P3-T2] | [P0-T12], [P4-T5] | [P0-T1], [P0-T8], [P0-T9], [P0-T10], [P0-T11], [P1-T3], [P4-T1], [P4-T2], [P4-T3], [P4-T4], [P6-T11] |
| AC-11 | [P3-T1] | [P4-T6] | [P0-T13], [P4-T7], [P6-T12] |
| AC-12 | [P2-T1] | [P3-T3], [P4-T5] | [P0-T14], [P5-T5], [P6-T13] |
| AC-13 | [P1-T1], [P3-T1] | [P4-T5] | [P5-T6], [P6-T14] |
| AC-14 | [P1-T2] | [P4-T5] | [P0-T2], [P0-T14], [P5-T1], [P5-T7], [P6-T15], [P6-T18], [P6-T19] |
| AC-15 | none, manual gesture check | none, live host required | [P6-T1], [P6-T16] |

## Out of scope, restated so a reviewer does not read an omission as an oversight

1. `IsAltKeyCommand` is neither narrowed nor renamed, and its four existing tests are unmodified.
2. QuickFiler/Viewers/QfcFormViewerDark.cs, QuickFiler/Viewers/QfcFormViewerExpanded.cs and
   QuickFiler/Legacy/QfcFormLegacyViewer.cs are unchanged. None is a build input.
3. TaskVisualization/TaskViewer.cs is unchanged. It implements a different accelerator contract that is
   pinned by existing tests, and its Designer declares no menu strip, so the symptom cannot arise there.
4. The unused locals on lines 64 through 67 of `QuickFiler/Viewers/QfcFormViewer.cs` are retained. AC-14
   pins their survival through VC-2.
5. No source or test file is added, so neither csproj is edited.
6. No `[ExcludeFromCodeCoverage]` attribute is added.
7. The drop-down mnemonics C, A, M and P are unchanged; they are reached only after the drop-down is
   open, at which point the form-level `ProcessCmdKey` is not the gate.
8. The `Invoke-MSTest.ps1` single-assembly discovery behaviour that `[P0-T11]` measures is recorded for
   a follow-up issue and is not fixed here.
