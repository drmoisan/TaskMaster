# 2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams (Plan)

- **Issue:** #871
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12T10-25
- **Status:** Ready for preflight
- **Version:** 1.0
- **Work Mode:** full-bug (resolved from the metadata marker in `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/issue.md`)
- **Acceptance-criteria source:** the `## Acceptance Criteria` section of `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md`, AC1 through AC22. No other document contributes acceptance criteria.

**Task counts (mechanical, one count per phase, derived by counting lines matching the task-id pattern):**
Phase 0 = 15, Phase 1 = 10, Phase 2 = 11, Phase 3 = 13, Phase 4 = 25, Phase 5 = 9, Phase 6 = 9, Phase 7 = 25. Total = 117. Counted by matching the task-id pattern at the start of a line; the
line count and the unique-task-id count agree at 117, so no id is duplicated.

---

## Formatting contract for this document

This plan is an input to a change-footprint harvester that treats every whitespace-free
backticked token as a claim that the change writes to that path, and that has no notion of
negation. Therefore:

- Inline backticks are used for exactly two kinds of token: (a) the seventeen repository-relative
  paths of the Write Set reproduced below, and (b) single-segment C# identifiers and literals that
  contain neither a dot nor a slash.
- Every other path — commands, tool locations, evidence artifact file names, policy documents,
  out-of-scope source files — appears either in a fenced block or in plain prose without backticks.
  Files this change does not touch are named in plain prose, deliberately without backticks and
  without slashes.
- Do not "fix" this formatting.

---

## Write Set (authoritative footprint, reproduced verbatim from the spec)

- `QuickFiler/Controllers/QfcQueue.cs`
- `QuickFiler/Controllers/QfcQueue.Enqueue.cs`
- `QuickFiler/Controllers/QfcQueue.Tlp.cs`
- `QuickFiler/Controllers/QfcQueue.UiIdle.cs`
- `QuickFiler/Interfaces/IUiIdleDispatcher.cs`
- `QuickFiler/QuickFiler.csproj`
- `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs`
- `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs`
- `QuickFiler.Test/QuickFiler.Test.csproj`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/issue.md`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/user-story.md`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/research/2026-09-12T10-35-qfcqueue-enqueue-seams-research.md`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/`

No task in this plan writes to any path outside this set. Build output, the test-results directory
and the coverage directory are matched by ignore patterns in the repository ignore file, so a
porcelain status that excludes ignored paths never reports them and the Scope-lock rule needs no
clause for them. The two exceptions the rule must carry are the tracked agent-memory directory of
this worktree and the set of paths the worktree already carried at the anchor, which P0-T2 records.

---

## Scope-lock rule (used by every diff and status gate in this plan)

A verification task never compares against a hand-written list of expected paths. It applies this
rule to every path the anchored diff or the porcelain status reports:

> A reported path passes the scope lock when it is one of the Write Set paths, or when it
> lies underneath one of the three Write Set evidence directories, or when it lies underneath the
> tracked agent-memory directory of this worktree, or when it appears verbatim in the
> `PreExistingWorktreePaths:` block that P0-T2 recorded before this plan wrote anything. Any other
> reported path is a scope-lock failure and must be reported, not silently accepted.

The agent-memory carve-out exists because that directory is tracked and the executing agent writes
to it during the run. The anchor carve-out exists because this worktree may not be clean at the
anchor: the promotion lifecycle that produced this feature folder left promotion-lifecycle
residuals in the index and the working tree — a staged rename of a potential entry into the
promoted subdirectory, a staged addition under the potential features directory, and modified and
untracked files under the tracked agent-memory directory — none of which any task in this plan
writes. Their exact composition is not asserted here; P0-T2 records it. Both carve-outs
are stated as rules rather than as file lists precisely because the file names are not knowable
when this plan is written; P0-T2 makes the second one a recorded fact rather than an assumption.

---

## Evidence conventions

Every artifact this plan produces is written under one of the three canonical evidence directories
of the feature folder. The permitted kinds are baseline, qa-gates and regression-testing. An
evidence path rooted at the repository-level artifacts directory is a policy violation and is never
used anywhere in this plan.

Artifact path shape and the mandatory schema:

```
docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/<kind>/<task-id-lowercase>-<slug>.2026-09-12T10-25.md
```

Every command-step artifact carries, at minimum, these four lines, each on its own line:

```
Timestamp: <ISO-8601 yyyy-MM-ddTHH-mm of the run>
Command: <the exact command string that was executed>
EXIT_CODE: <observed integer>
Output Summary: <1-20 lines carrying the essential result signal>
```

Where a task's expected exit code is not zero, the artifact additionally carries an
`ExpectedExitCode:` line with that integer. The Cobertura XML artifacts are written with their
native `.cobertura.xml` extension alongside the Markdown artifact that interprets them.

Where a task calls for a package-level or file-level covered and valid line count, that figure is
obtained by dot-sourcing the coverage helpers file under the vscode scripts directory and calling
`Get-CoberturaPackageLineSummary` on the package element or `Get-CoberturaClassLineSummary` on the
class element whose filename attribute names the file, then reading `LinesCovered` and `LinesValid`
from the returned object. The package and class elements carry a line-rate attribute and carry no
lines-covered and no lines-valid attribute, so those two figures have no attribute to read and no
task may record them as though they did. Only the coverage root element carries them directly.

---

## Command catalogue

All commands are run from the worktree root in PowerShell. When a command is routed through a
`pwsh -Command` wrapper, the payload must be wrapped in outer single quotes with inner double
quotes; an outer-double form is consumed by the calling shell and produces a spurious failure.

```
CMD-SDK
  pwsh -NoProfile -File .\scripts\vscode\Install-RepoDotNetSdk.ps1

CMD-SDK-VERIFY
  Test-Path .\.dotnet-sdk\sdk\8.0.205 ; dotnet --version

CMD-TOOLRESTORE
  dotnet tool restore

CMD-TOOLVERIFY
  dotnet tool run csharpier --version

CMD-RESTORE
  pwsh -NoProfile -File .\scripts\vscode\Invoke-Restore.ps1

CMD-ANALYZERPATHS
  Get-ChildItem -Recurse -Filter *.csproj |
    Where-Object { $_.FullName -notmatch '\\packages\\' -and $_.FullName -notmatch '\\bin\\' } |
    ForEach-Object {
      $projDir = $_.DirectoryName
      ([xml](Get-Content -LiteralPath $_.FullName -Raw)).GetElementsByTagName('Analyzer') |
        ForEach-Object {
          $inc = $_.GetAttribute('Include')
          if ($inc) {
            $resolved = [IO.Path]::GetFullPath((Join-Path $projDir $inc))
            if (-not (Test-Path -LiteralPath $resolved)) { "MISSING: $resolved" }
          }
        }
    }

CMD-COVERAGETOOL
  if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }

CMD-FORMAT
  dotnet tool run csharpier format .

CMD-FORMAT-SCOPED
  dotnet tool run csharpier format <one or more explicit Write Set code paths>

CMD-CHECK
  dotnet tool run csharpier check .

CMD-BUILD  (compiles the tree so that a following test run observes the current sources; this is a
build step for the runner, not an analyzer or nullable gate, and no diagnostic count is read from it)
  msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

CMD-CHECK-SCOPED
  dotnet tool run csharpier check <the same explicit Write Set code paths passed to CMD-FORMAT-SCOPED>

CMD-ANALYZE
  msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

CMD-NULLABLE
  msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true

CMD-VSTEST  (scoped to the one test assembly; <RD> is a fresh subdirectory under the gitignored TestResults directory, named for the task id)
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=vstest-run.trx" /ResultsDirectory:TestResults\<RD>
  $exit = $LASTEXITCODE

CMD-VSTEST-CLASS  (the same, with the class filter appended instead)
  & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\<RD>

CMD-TRXCOUNTERS  (read the counters element of the newest trx under <RD>, sorted by LastWriteTime)
  $trx = Get-ChildItem TestResults\<RD> -Recurse -Filter *.trx | Sort-Object LastWriteTime | Select-Object -Last 1
  $c = ([xml](Get-Content -LiteralPath $trx.FullName -Raw)).TestRun.ResultSummary.Counters
  "total=$($c.total) executed=$($c.executed) passed=$($c.passed) failed=$($c.failed)"

CMD-COVERAGE  (<OUT> is the repository-relative Cobertura output path for the task)
  if (Test-Path -LiteralPath <OUT>) { Remove-Item -LiteralPath <OUT> -Force }
  $runnerOutput = & pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput <OUT> 2>&1
  $exit = $LASTEXITCODE
  $runnerOutput
  "RUNNER-EXIT: $exit"
  if (Test-Path -LiteralPath <OUT>) { "COVERAGE-ARTIFACT-WRITTEN" }
  if ($runnerOutput -match 'is below the required 80% threshold') { "THRESHOLD-ASSERTION: THREW" } else { "THRESHOLD-ASSERTION: PASSED" }

CMD-LINECOUNT  (<P> is a repository-relative code path)
  (Get-Content -LiteralPath <P>).Count

CMD-ANCHOR
  git rev-parse HEAD

CMD-DIFF  (<BASE> is the sha recorded by P0-T2)
  git diff --name-status <BASE>..HEAD
  git status --porcelain --untracked-files=all
```

Five properties of this catalogue are load-bearing and must not be "simplified":

- The rebuild target is required in CMD-ANALYZE and CMD-NULLABLE, exactly as both commands are
  written in the catalogue above. MSBuild's incremental up-to-date check does not invalidate on a
  command-line property change, so substituting the plain build target returns exit 0 with the
  compile skipped on every project and the gate cannot fail.
- No solution-wide nullable property is added to CMD-NULLABLE. No project in this repository
  carries a nullable element and there is no directory-level build props file, so forcing it
  conscripts every file that never adopted the per-file pragma.
- CMD-VSTEST names exactly one test assembly. A whole-solution local run pulls in four shell-icon
  test classes in another assembly that stall the runner on this machine; that is an environmental
  fact about this host, not a regression, and CI covers those classes.
- CMD-BUILD deliberately uses the plain build target. It is not a gate, so the incremental
  up-to-date check that disqualifies that target for CMD-ANALYZE and CMD-NULLABLE is exactly the
  behaviour wanted here: it recompiles the projects whose sources changed and leaves the rest alone.
  Its success-case output carries a summary line reading `0 Error(s)`, which is what the tasks
  record, read by an anchored regular expression over the whole summary line under the rule P0-T9
  states, because the zero-error text is also a substring of a ten-error line. That line is printed
  on an up-to-date skip as well as on a compile and so does not by itself prove a compile occurred;
  what makes the compile occur is that the executor has just edited a source file, which invalidates
  MSBuild's timestamp comparison for the affected project. The incremental behaviour that
  disqualifies this target for CMD-ANALYZE and CMD-NULLABLE follows from a command-line property
  change, not from a source edit.
- CMD-DIFF carries no staging span. The name-status span is a commit-to-commit comparison, which an
  intent-to-add in the index does not affect, and the porcelain span is what makes files this change
  creates visible. An intent-to-add over the whole tree would additionally stage the residual paths
  P0-T2 records, so a later commit that stages everything would sweep them onto this branch. Every
  commit task in this plan therefore stages explicit pathspecs and never stages the whole tree.

The coverage runner has two behaviours the acceptance conditions below depend on, both re-derived
against the current tree this pass. First, it post-processes and writes the Cobertura document at
line 342 of the runner script and evaluates its own document-level 80 percent assertion at line 344,
so the artifact exists on disk even when that assertion then throws. Second, it discovers test
assemblies through an array-wrapped enumeration, so a search root resolving to a single assembly is
safe. Two further facts shape CMD-COVERAGE. A PowerShell try and catch does not catch the failure of
an external process, so the runner's terminating message reaches the caller on the child's error
stream rather than as an exception object; CMD-COVERAGE therefore merges that stream into the
pipeline, captures it, and reads the child's exit code rather than inferring one. And the
document-level rate of a run scoped to one test assembly has never been measured on this host: the
only committed evidence of this runner in the tree was produced with the repository root as its
search root and read 0.856686, so its assertion passed there. This plan therefore does not assert in
advance which branch a single-assembly run takes; it records the branch as an observation. Either
way the gate is on the content of the artifact.

---

### Phase 0 — Policy reading, toolchain bootstrap and baseline capture

This worktree is unbootstrapped: it carries no repo-local SDK directory, no packages directory and
no build output. Every later `EXIT_CODE: 0` acceptance is unreachable until the five bootstrap
tasks below complete.

- [ ] [P0-T1] Read, in this exact order, the standing instructions file at the repository root, the
      general code change rule file, the general unit test rule file, the C# rule file and the
      tonality rule file, all four rule files being under the repository rules directory. Write the
      evidence artifact phase0-instructions-read.2026-09-12T10-25.md into
      `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/`.
      **Acceptance:** the artifact exists and contains a `Timestamp:` line, a `Policy Order:` line
      naming the five documents in the order above, and one line per document giving its
      repository-relative path.

- [ ] [P0-T2] Run CMD-ANCHOR and record the resulting commit sha as the diff anchor for every
      anchored diff in this plan, in artifact p0-t2-diff-anchor.2026-09-12T10-25.md under the
      baseline evidence directory. In the same artifact, and before any file in this plan is created
      or edited, record the full verbatim output of the porcelain status span of CMD-DIFF under a
      line whose first token is `PreExistingWorktreePaths:`. **Acceptance:** the artifact contains a
      line whose first token is `BASE_SHA:` followed by a 40-character hexadecimal sha, a
      `PreExistingWorktreePaths:` block reproducing that output verbatim, and the four schema fields.
      Every later task that says "the recorded anchor" means the sha; every later task that applies
      the Scope-lock rule means this path set. No sha and no path list is written into this plan.

- [ ] [P0-T3] Run CMD-SDK to provision the repo-local .NET SDK, then CMD-SDK-VERIFY. Record in
      artifact p0-t3-sdk.2026-09-12T10-25.md under the baseline evidence directory.
      **Acceptance:** the first command of CMD-SDK-VERIFY prints `True`, which is the installer's
      own filesystem marker for the pinned version, and the second prints `8.0.205`.

- [ ] [P0-T4] Run CMD-TOOLRESTORE, then CMD-TOOLVERIFY. Record in artifact
      p0-t4-tool-restore.2026-09-12T10-25.md under the baseline evidence directory.
      **Acceptance:** CMD-TOOLRESTORE exits 0 and CMD-TOOLVERIFY prints `1.2.6`, the version pinned
      by the tool manifest.

- [ ] [P0-T5] Run CMD-RESTORE to restore the packages-config NuGet graph for the solution. Record
      in artifact p0-t5-restore.2026-09-12T10-25.md under the baseline evidence directory.
      **Acceptance:** EXIT_CODE 0 and a packages directory now exists at the worktree root
      containing at least one directory whose name begins with the token `Meziantou`.

- [ ] [P0-T6] Run CMD-ANALYZERPATHS to verify that every analyzer include item in every first-party
      project resolves against that project's own directory. Record in artifact
      p0-t6-analyzer-paths.2026-09-12T10-25.md under the baseline evidence directory.
      **Acceptance:** the command emits zero lines beginning with the token `MISSING:`. A missing
      analyzer path is compiler error CS0006, not a warning, so a non-empty result blocks every
      later build gate and must be remedied by installing the named package version into the
      packages directory before this task is checked off. No version number is written into this
      acceptance condition; the enumeration derives the versions from the project files themselves.

- [ ] [P0-T7] Run CMD-COVERAGETOOL to ensure the dotnet-coverage global tool is present. It is a
      global tool and is not supplied by the tool manifest restore in P0-T4; the coverage runner
      throws before it runs anything when the tool is absent. Record in artifact
      p0-t7-coverage-tool.2026-09-12T10-25.md under the baseline evidence directory.
      **Acceptance:** a subsequent `Get-Command dotnet-coverage` resolves to a command and prints
      its source path.

- [ ] [P0-T8] Run CMD-CHECK to capture the pre-existing formatting state of the whole tree, and
      record the names of every file the tool reports, in artifact
      p0-t8-csharpier-baseline.2026-09-12T10-25.md under the baseline evidence directory.
      **Acceptance:** the artifact carries the four schema fields plus a line whose first token is
      `PreExistingDriftFiles:` listing every file name the command reported, or the single token
      `NONE` when it reported none, plus a line whose first token is `DriftInsideWriteSet:` naming
      any of the nine Write Set code paths that appear in that list, or `NONE`. This figure is
      consumed by P5-T1, and carried from there into P6-T6 and P7-T22: a repo-wide format in the
      final QC loop can only rewrite files outside the Write Set if this baseline already showed
      drift in them. This task additionally records the
      consequence for P5-T1. When both `PreExistingDriftFiles:` and `DriftInsideWriteSet:` read
      `NONE`, the repo-wide format in P5-T1 rewrites nothing outside the Write Set and its scope lock
      is satisfiable as written. When `PreExistingDriftFiles:` is not `NONE`, this task does not
      repair the drift; it records that P5-T1 will repair those files as a consequence of the
      repository-wide format command the standing instructions file mandates rather than as an edit
      this item chose, that those paths are admitted by the Scope-lock rule for the remainder of this
      plan on the strength of this record alone, and that the repair puts acceptance criterion AC22
      at risk.

- [ ] [P0-T9] Run CMD-ANALYZE. Record in artifact p0-t9-analyzer-baseline.2026-09-12T10-25.md under
      the baseline evidence directory. **Acceptance:** EXIT_CODE 0, and the artifact records the
      integer captured from the build summary line matching the anchored pattern for the error
      count and the integer captured from the anchored pattern for the warning count. The counts
      must be read by an anchored regular expression over the whole summary line and not by a
      substring search, because a zero-error substring also occurs inside a ten-error line.

- [ ] [P0-T10] Run CMD-NULLABLE. Record in artifact p0-t10-nullable-baseline.2026-09-12T10-25.md
      under the baseline evidence directory. **Acceptance:** EXIT_CODE 0, with the error and
      warning counts captured by the same anchored-pattern rule as P0-T9.

- [ ] [P0-T11] Run CMD-VSTEST followed by CMD-TRXCOUNTERS, with the results directory named for
      this task. Record in artifact p0-t11-test-baseline.2026-09-12T10-25.md under the baseline
      evidence directory. **Acceptance:** EXIT_CODE 0 and the counters line reports `failed=0`. The
      artifact records the `total`, `executed`, `passed` and `failed` integers read from the trx
      counters element. The counters element is the assertion target rather than a console phrase,
      because a green run of this runner prints no failed or skipped line at all. Record the
      `total` integer as BASELINE_TEST_TOTAL; later phases compare against it.

- [ ] [P0-T12] Run CMD-COVERAGE with the Cobertura output written as
      coverage-baseline.2026-09-12T10-25.cobertura.xml under
      `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/`,
      and interpret it in the Markdown artifact p0-t12-coverage-baseline.2026-09-12T10-25.md in the
      same directory. **Acceptance:** the command printed the token `COVERAGE-ARTIFACT-WRITTEN`;
      the Cobertura document contains a filename attribute for each of
      `QuickFiler/Controllers/QfcQueue.cs` and `QuickFiler/Controllers/QfcQueue.Enqueue.cs` in the
      backslash-separated form the post-processor writes; and the Markdown artifact records, as
      labelled numeric lines, the document-level line-rate, lines-covered and lines-valid; the
      QuickFiler package `LinesCovered`, `LinesValid` and `LineRate` from
      `Get-CoberturaPackageLineSummary`; and the `LineRate`, `LinesCovered` and `LinesValid` from
      `Get-CoberturaClassLineSummary` for the class element of each of those two files. The artifact
      records `EXIT_CODE:` as the integer CMD-COVERAGE printed on its `RUNNER-EXIT:` line, and a line
      whose first token is `ThresholdAssertion:` carrying the value CMD-COVERAGE printed on its
      `THRESHOLD-ASSERTION:` line. When that value is `THREW`, the artifact additionally carries
      `ExpectedExitCode: 1` and reproduces the runner's terminating message verbatim on a
      `RUNNER-TERMINATED:` line; when it is `PASSED`, the artifact carries no `ExpectedExitCode:`
      line and the recorded exit code must be 0. A `ThresholdAssertion:` of `THREW` paired with an
      exit code of 0, or of `PASSED` paired with a non-zero exit code, is a failure of this task and
      is reported. That assertion is a repository-wide gate and is not this task's gate either way;
      this task's gate is the artifact content stated above.

- [ ] [P0-T13] Measure the physical line count of `QuickFiler/Controllers/QfcQueue.cs`,
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs` and the three existing QfcQueue test files in the
      QuickFiler test project's Controllers folder with CMD-LINECOUNT. Record in artifact
      p0-t13-line-counts-baseline.2026-09-12T10-25.md under the baseline evidence directory.
      **Acceptance:** the recorded count for `QuickFiler/Controllers/QfcQueue.cs` is exactly 507 and
      the recorded count for `QuickFiler/Controllers/QfcQueue.Enqueue.cs` is exactly 200. Both
      figures were re-derived against the current tree while this plan was authored; a divergence
      means the tree moved and the split ranges in Phase 1 must be re-derived before proceeding.

- [ ] [P0-T14] Write the fail-before exception dossier
      fail-before-exception.2026-09-12T10-25.md into
      `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/`.
      A failing pre-change run is structurally impossible here: the defect is untestability, and any
      test that substitutes a seam cannot compile before the seam exists, while any test that does
      not substitute one fails both before and after the change because the production default still
      reads the process-wide dispatcher. **Acceptance:** the dossier carries a
      `WhyFailingRunImpossible:` paragraph stating exactly that, plus an absence-of-test proof with
      `SearchScope:` naming, by file name, the three existing QfcQueue test files in the QuickFiler
      test project's Controllers folder, `SearchPatterns:` naming the literal token `EnqueueAsync`,
      and `SearchResult:` recording the match count across exactly those three files, which must be
      zero. The scope is those three files and not the whole Controllers folder: a folder-wide search
      matches the two QfcHomeController iteration test files, which assert on the queue interface's
      enqueue member through a Moq expression and are not tests of the queue's own enqueue path. The
      artifact records that folder-wide count alongside the scoped one so the distinction is
      auditable rather than implied.

- [ ] [P0-T15] Commit the Phase 0 evidence with a single-line message. **Acceptance:** CMD-DIFF is
      run afterwards and every path it reports satisfies the Scope-lock rule; the commit contains at
      least the fourteen artifacts produced by P0-T1 through P0-T14.

---

### Phase 1 — Mandatory split of the base part under the 500-line ceiling

`QuickFiler/Controllers/QfcQueue.cs` stands at 507 lines, already seven lines over the repository's
500-line hard ceiling before any seam is added, so this split is a precondition rather than an
option. Both relocated blocks are complete region and endregion pairs at the recorded anchor and
move intact. No relocated member references a primary-constructor parameter: the three parameters
are referenced only by field initializers on the base part, which do not move.

- [ ] [P1-T1] Create `QuickFiler/Controllers/QfcQueue.Tlp.cs` holding the whole Tlp Manipulation
      region, which occupies lines 230 through 453 inclusive of `QuickFiler/Controllers/QfcQueue.cs`
      at the recorded anchor, moved verbatim, and delete exactly those lines from
      `QuickFiler/Controllers/QfcQueue.cs`, leaving in their place a one-line breadcrumb comment in
      the style of the two breadcrumb comments the base file already carries. The new file declares
      the same namespace and `public partial class QfcQueue`, carries a file-header doc comment
      explaining the split, and carries the using directives the relocated members require. Derive
      that set rather than assuming it: start from the full directive set of
      `QuickFiler/Controllers/QfcQueue.cs` at the recorded anchor and remove only those directives
      the analyzer gate in P1-T5 reports as unnecessary. Two resolutions are already settled and need
      no derivation: the static viewer queue helper is declared in the parent namespace of this file
      and needs no directive, and the control clone is an extension in the UtilitiesCS namespace, so
      that directive is required. Do not add a nullable pragma. **Acceptance:**
      `QuickFiler/Controllers/QfcQueue.cs` contains zero occurrences of the literal
      `#region Tlp Manipulation` and `QuickFiler/Controllers/QfcQueue.Tlp.cs` contains exactly one;
      the measured line count of `QuickFiler/Controllers/QfcQueue.cs` is strictly less than the
      value P0-T13 recorded for it; and both files are recorded with their measured counts in
      artifact p1-t1-split-tlp.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P1-T2] Create `QuickFiler/Controllers/QfcQueue.UiIdle.cs` holding the whole Helper Methods
      region, which occupies lines 472 through 505 inclusive of
      `QuickFiler/Controllers/QfcQueue.cs` at the recorded anchor, moved verbatim, and delete
      exactly those lines from `QuickFiler/Controllers/QfcQueue.cs`, leaving a one-line breadcrumb
      comment in their place. The new file declares the same namespace and
      `public partial class QfcQueue` and carries the using directives the relocated members require,
      derived by the same rule as P1-T1. Do not add the QuickFiler interfaces directive at this task:
      nothing in this file references that namespace until P2-T5 adds the adapter, and adding it here
      produces an unnecessary-using diagnostic against a file this task just created. Do not add a
      nullable pragma. **Acceptance:**
      `QuickFiler/Controllers/QfcQueue.cs` contains zero occurrences of the literal
      `#region Helper Methods` and `QuickFiler/Controllers/QfcQueue.UiIdle.cs` contains exactly one;
      the three members `UiIdleCallAsync`, its generic overload and `UiIdleAsyncCallAsync` appear in
      the new file with their bodies byte-identical to the anchor apart from indentation; recorded
      in artifact p1-t2-split-uiidle.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P1-T3] Add one `<Compile Include>` item to `QuickFiler/QuickFiler.csproj` for each of
      `QuickFiler/Controllers/QfcQueue.Tlp.cs` and `QuickFiler/Controllers/QfcQueue.UiIdle.cs`,
      placed adjacent to the two existing QfcQueue items, which sit at lines 348 and 349 at the
      recorded anchor. This project is a legacy non-SDK project with no implicit source glob, so a
      missing item does not present as a missing-file error; it presents as the seam member not
      existing. **Acceptance:** a search of `QuickFiler/QuickFiler.csproj` finds exactly one item
      naming each of the two new files.

- [ ] [P1-T4] Run CMD-FORMAT-SCOPED over `QuickFiler/Controllers/QfcQueue.cs`,
      `QuickFiler/Controllers/QfcQueue.Tlp.cs` and `QuickFiler/Controllers/QfcQueue.UiIdle.cs`,
      capturing the porcelain status immediately before and immediately after the command, then run
      CMD-CHECK-SCOPED over the same paths. **Acceptance:** the artifact
      p1-t4-format.2026-09-12T10-25.md under the qa-gates evidence directory records both porcelain
      captures verbatim, so that a run which rewrote nothing is distinguishable from one that did,
      and CMD-CHECK-SCOPED exits 0. The scoped form is used here rather than the repo-wide form
      because the repo-wide pass belongs to the final QC loop; an interim repo-wide format would
      rewrite files outside the Write Set and break the Phase 6 scope lock.

- [ ] [P1-T5] Run CMD-ANALYZE. **Acceptance:** EXIT_CODE 0, with the error and warning counts
      captured by the anchored-pattern rule of P0-T9 and recorded in artifact
      p1-t5-analyze.2026-09-12T10-25.md under the qa-gates evidence directory. If the build reports
      an unnecessary-using diagnostic against any of `QuickFiler/Controllers/QfcQueue.cs`,
      `QuickFiler/Controllers/QfcQueue.Tlp.cs` or `QuickFiler/Controllers/QfcQueue.UiIdle.cs`, remove
      that directive from the file the diagnostic names and re-run this task; removing an unused
      using from the base part is not a relocated
      member and does not affect the verbatim-move property that AC18 gates.

- [ ] [P1-T6] Run CMD-NULLABLE. **Acceptance:** EXIT_CODE 0, recorded in artifact
      p1-t6-nullable.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P1-T7] Run CMD-VSTEST followed by CMD-TRXCOUNTERS with a results directory named for this
      task. **Acceptance:** EXIT_CODE 0, the counters report `failed=0`, and the reported `total`
      equals BASELINE_TEST_TOTAL from P0-T11. Recorded in artifact
      p1-t7-tests.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P1-T8] Measure with CMD-LINECOUNT the three files touched in this phase. **Acceptance:**
      each measured count is strictly less than 500 and each is recorded as a labelled numeric line
      in artifact p1-t8-line-counts.2026-09-12T10-25.md under the qa-gates evidence directory. No
      predicted figure from the spec or the research is acceptable in place of a measurement.

- [ ] [P1-T9] Verify the split preserved the two invariants the spec names. **Acceptance:** a search
      of `QuickFiler/Controllers/QfcQueue.Tlp.cs` and `QuickFiler/Controllers/QfcQueue.UiIdle.cs`
      for the literal token `#nullable` returns zero matches in each, and in each of
      `QuickFiler/Controllers/QfcQueue.cs`, `QuickFiler/Controllers/QfcQueue.Tlp.cs` and
      `QuickFiler/Controllers/QfcQueue.UiIdle.cs` the count of `#region` occurrences equals the
      count of `#endregion` occurrences. Recorded in artifact
      p1-t9-invariants.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P1-T10] Commit Phase 1 with a single-line message. **Acceptance:** CMD-DIFF is run afterwards
      and every path it reports satisfies the Scope-lock rule.

---

### Phase 2 — Move-monitor seam and UI-idle dispatcher seam

A constructor-parameter seam for the move monitor is illegal here: the move-monitor interface is
`internal` and the queue class is `public`, so an interface-typed primary-constructor parameter is
an inconsistent-accessibility error and a public property of that type is likewise. The field is
retained rather than converted to an auto-property because six existing test methods in the
QuickFiler test project — three in the QfcQueue coverage-expansion test file and three in the
QfcQueue pure-paths test file — resolve the backing field by reflection, assert through
FluentAssertions that the resulting field descriptor is non-null, and then set the field; an
auto-property would rename the backing field to a compiler-generated name and fail all six.

- [ ] [P2-T1] Add seam S1 to `QuickFiler/Controllers/QfcQueue.cs`: an `internal` property named
      `MoveMonitor` of the move-monitor interface type, whose getter returns the existing field and
      whose setter assigns the value or throws `ArgumentNullException` when it is null, with an XML
      doc comment recording that the default remains the per-owner instance the field initializer
      creates. **Acceptance:** the file contains exactly one declaration of `MoveMonitor`; it still
      contains exactly one occurrence of the field declaration line for the move monitor; and the
      load-bearing per-owner comment that precedes that field at line 41 of the anchor is present
      byte-for-byte. Recorded in artifact p2-t1-s1.2026-09-12T10-25.md under the qa-gates evidence
      directory.

- [ ] [P2-T2] In `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, replace the single field read in the
      hook loop at line 91 of the anchor so that the hook call is made through `MoveMonitor`.
      **Acceptance:** `QuickFiler/Controllers/QfcQueue.Enqueue.cs` contains zero occurrences of the
      move-monitor field name and exactly one occurrence of the literal `MoveMonitor.HookItem`; the
      surrounding `Task.Run` wrapper, the lambda and the `items.ForEach` call are unchanged.

- [ ] [P2-T3] Create `QuickFiler/Interfaces/IUiIdleDispatcher.cs` declaring an `internal` interface
      named `IUiIdleDispatcher` with exactly three members: one taking an action and returning a
      task, one generic member taking a function of the type parameter and returning a task of it,
      and one generic member taking a function returning a task of the type parameter and returning
      a task of it. All three are named `InvokeIdleAsync`. This file is brand-new code and may carry
      a nullable pragma, following the existing threading interface files in the utilities assembly.
      **Acceptance:** the file contains exactly one occurrence of the literal
      `internal interface IUiIdleDispatcher` and exactly three occurrences of the literal
      `InvokeIdleAsync` on member declaration lines, with the count taken over declaration lines
      only so that XML documentation comments naming the member do not change it. A new narrow
      interface is introduced rather than reusing the existing
      dispatcher abstraction because that abstraction expresses no priority for two of the three
      shapes and its adapter forwards them at the framework default, which would silently promote
      two call sites and change when background page construction runs.

- [ ] [P2-T4] Add a `<Compile Include>` item to `QuickFiler/QuickFiler.csproj` for
      `QuickFiler/Interfaces/IUiIdleDispatcher.cs`, placed among the existing interface items, which
      begin at line 363 at the recorded anchor. **Acceptance:** a search of
      `QuickFiler/QuickFiler.csproj` finds exactly one item naming that file.

- [ ] [P2-T5] In `QuickFiler/Controllers/QfcQueue.UiIdle.cs`, declare the production adapter class
      named `UiThreadIdleDispatcher`, `internal` and sealed, implementing the new interface and
      holding the three relocated bodies verbatim, and declare seam S2 as an `internal` property
      named `UiIdleDispatcher` over a private backing field, with a lazy null-coalescing-assignment
      getter that constructs the adapter on first read and a setter that throws
      `ArgumentNullException` on null. Convert the three existing marshalling members in that file
      into one-line forwards to the seam. The adapter is declared in this file rather than in a file
      of its own because the only other suitable folder has a space in its name and a path
      containing a space is dropped by the downstream change-footprint tooling. **Acceptance:**
      `QuickFiler/Controllers/QfcQueue.UiIdle.cs` contains exactly four occurrences of the literal
      token `ContextIdle`: three are the priority arguments of the three relocated adapter bodies,
      and the fourth is the commented-out alternative implementation that line 502 of
      `QuickFiler/Controllers/QfcQueue.cs` carries at the recorded anchor, which travels with the
      third body and is not deleted, because deleting it would break the verbatim-move property that
      AC18 and P6-T5 gate. The artifact records the line number of each of the four so the three
      executable ones are distinguishable from the commented one; zero occurrences of the dotted
      literal for the framework-default
      dispatcher priority, searched as the whole dotted token and not as the bare word; exactly one
      occurrence of the literal `await Task.Yield();`; and each of the three marshalling members has
      a body that is a single expression routed through `UiIdleDispatcher`; and
      `QuickFiler/Controllers/QfcQueue.UiIdle.cs` contains zero occurrences of the literal token
      `#nullable`, re-checked here because P1-T9 measured that file before this task added the
      adapter class. All of the above is recorded in artifact
      p2-t5-s2.2026-09-12T10-25.md under the qa-gates evidence directory. Because the getter is
      lazy, constructing a queue still performs no read of the process-wide dispatcher.

- [ ] [P2-T6] Run CMD-FORMAT-SCOPED over the four code paths touched in this phase, capturing the
      porcelain status immediately before and after, then CMD-CHECK-SCOPED over the same paths.
      **Acceptance:** both porcelain captures are recorded verbatim in artifact
      p2-t6-format.2026-09-12T10-25.md under the qa-gates evidence directory and CMD-CHECK-SCOPED
      exits 0.

- [ ] [P2-T7] Run CMD-ANALYZE. **Acceptance:** EXIT_CODE 0, counts captured by the anchored-pattern
      rule, recorded in artifact p2-t7-analyze.2026-09-12T10-25.md under the qa-gates evidence
      directory.

- [ ] [P2-T8] Run CMD-NULLABLE. **Acceptance:** EXIT_CODE 0, recorded in artifact
      p2-t8-nullable.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P2-T9] Run CMD-VSTEST followed by CMD-TRXCOUNTERS with a results directory named for this
      task. **Acceptance:** EXIT_CODE 0, `failed=0`, and `total` equal to BASELINE_TEST_TOTAL.
      Recorded in artifact p2-t9-tests.2026-09-12T10-25.md under the qa-gates evidence directory.
      This is the gate that proves the three existing QfcQueue test files still pass unmodified.

- [ ] [P2-T10] Measure with CMD-LINECOUNT the four code files touched in this phase.
      **Acceptance:** each measured count is strictly less than 500 and each is recorded as a
      labelled numeric line in artifact p2-t10-line-counts.2026-09-12T10-25.md under the qa-gates
      evidence directory.

- [ ] [P2-T11] Commit Phase 2 with a single-line message. **Acceptance:** CMD-DIFF is run afterwards
      and every path it reports satisfies the Scope-lock rule.

---

### Phase 3 — Viewer, row-placer, item-group and background-template seams

Two of the four seams in this phase have a production default that is an instance method of the
queue class. A C# instance field or auto-property initializer cannot reference the instance, so
those two use a lazy null-coalescing-assignment property getter, which does have the instance in
scope and still yields a non-null default on first read. The other two have instance-free defaults
and use the plain initializer form that the existing item-controller seam uses.

- [ ] [P3-T1] Add seam S3 to `QuickFiler/Controllers/QfcQueue.Tlp.cs`: an `internal` auto-property
      named `ItemViewerFactory` of delegate type taking a cancellation token and returning the
      viewer type, initialized to the static dequeue method group of the viewer queue helper class,
      with a setter guard that throws `ArgumentNullException` on null; and substitute the direct
      dequeue call inside `AddAsync` with a call through the seam, passing the same token field.
      **Acceptance:** `QuickFiler/Controllers/QfcQueue.Tlp.cs` contains exactly one declaration of
      `ItemViewerFactory` and exactly one call of the form `ItemViewerFactory(_token)`, and zero
      remaining direct calls to the viewer queue helper's dequeue member.

- [ ] [P3-T2] Add seam S4 to `QuickFiler/Controllers/QfcQueue.Tlp.cs`: an `internal` property named
      `ViewerRowPlacer` of delegate type taking the panel, the viewer and an integer, over a private
      backing field, with a lazy null-coalescing-assignment getter defaulting to the
      `AddViewerToTlp` method group and a setter that throws `ArgumentNullException` on null; and
      substitute the direct call inside `AddAsync` with a call through the seam, leaving it inside
      the same marshalling wrapper as before. **Acceptance:** the file contains exactly one
      declaration of `ViewerRowPlacer`, `AddViewerToTlp` is still declared in the file with its body
      unchanged, and the call inside `AddAsync` is of the form
      `ViewerRowPlacer(tlp, viewer, indexNumber)` wrapped in the unchanged marshalling call.

- [ ] [P3-T3] Add seam S5 to `QuickFiler/Controllers/QfcQueue.Tlp.cs`: an `internal` property named
      `ItemGroupFactory` of delegate type taking the panel, a mail item and an integer and returning
      a task of the item-group type, over a private backing field, with a lazy
      null-coalescing-assignment getter defaulting to the `AddAsync` method group and a setter that
      throws `ArgumentNullException` on null. **Acceptance:** the file contains exactly one
      declaration of `ItemGroupFactory` and `AddAsync` remains declared with its signature
      unchanged. S4 and S5 are both present deliberately: a single coarse seam in place of S4 would
      make the loader coverable while leaving the production default it displaces permanently
      uncovered, which relocates the untestable region rather than closing it.

- [ ] [P3-T4] Add seam S6 to `QuickFiler/Controllers/QfcQueue.Tlp.cs`: an `internal` auto-property
      named `BackgroundTlpFactory` of delegate type taking a panel and returning a panel,
      initialized to a lambda that performs the same clone call with the same named argument as the
      expression it replaces, with a setter guard that throws `ArgumentNullException` on null.
      **Acceptance:** the file contains exactly one declaration of `BackgroundTlpFactory` and its
      initializer contains the literal string `"BackgroundTableLayout"` passed as the same named
      argument used at line 98 of `QuickFiler/Controllers/QfcQueue.Enqueue.cs` at the recorded
      anchor.

- [ ] [P3-T5] In `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, route the per-row construction at
      line 177 of the anchor through `ItemGroupFactory`, preserving the index expression exactly.
      **Acceptance:** `QuickFiler/Controllers/QfcQueue.Enqueue.cs` contains exactly one occurrence of
      the single-line token `ItemGroupFactory(` and zero occurrences of the token `AddAsync(`, and
      the index expression passed to it is unchanged; and artifact
      p3-t5-itemgroup-callsite.2026-09-12T10-25.md under the qa-gates evidence directory records the
      substituted statement as it stands at this task under a line whose first token is
      `PreFormatStatement:`. The statement measures 94 columns at the recorded anchor and the
      substitution adds eight, taking it past CSharpier's 100-column default, so it is certain to be
      re-wrapped by P3-T8 and this pre-format capture is not the evidence any acceptance criterion
      cites. The post-format form is captured by P3-T8.

- [ ] [P3-T6] In `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, route the background template clone
      at lines 97 through 99 of the anchor through `BackgroundTlpFactory`, leaving the enclosing
      marshalling call unchanged. **Acceptance:** the file contains exactly one occurrence of the
      literal `BackgroundTlpFactory(_tlpTemplate)`, that call sits inside the same marshalling
      wrapper member as before, and the file contains zero occurrences of the clone call it
      replaced.

- [ ] [P3-T7] Verify the separately promoted out-of-scope defect was not fixed and not disturbed.
      The running-jobs increment sits outside the try block whose finally decrements it, at lines 94
      and 103 of the anchor respectively. **Acceptance:** in
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs` the line number of the sole increment of the
      running-jobs counter is strictly less than the line number of the `try` keyword that opens the
      block whose `finally` holds the sole decrement; and an anchored zero-context content diff of
      that single path against the recorded anchor produces no hunk containing the increment line,
      the `try` line, or the decrement line, recorded alongside the porcelain capture from CMD-DIFF.
      The name-status span of CMD-DIFF reports one status letter per file and carries no line
      content, so it cannot carry this observation; the porcelain capture is recorded because a
      change this task has not yet committed is invisible to any commit-to-commit comparison.
      Recorded in
      artifact p3-t7-out-of-scope-untouched.2026-09-12T10-25.md under the qa-gates evidence
      directory.

- [ ] [P3-T8] Run CMD-FORMAT-SCOPED over the two code paths touched in this phase, capturing the
      porcelain status immediately before and after, then CMD-CHECK-SCOPED over the same paths.
      **Acceptance:** both porcelain captures are recorded verbatim in artifact
      p3-t8-format.2026-09-12T10-25.md under the qa-gates evidence directory and CMD-CHECK-SCOPED
      exits 0; and the same artifact reproduces, under a line whose first token is
      `PostFormatStatement:`, the substituted statement in
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs` as it stands after CMD-FORMAT-SCOPED has run,
      together with its measured column width. This is the form that AC5 evidence cites; the
      pre-format capture in the P3-T5 artifact is retained only to make the re-wrap auditable.

- [ ] [P3-T9] Run CMD-ANALYZE. **Acceptance:** EXIT_CODE 0, recorded in artifact
      p3-t9-analyze.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P3-T10] Run CMD-NULLABLE. **Acceptance:** EXIT_CODE 0, recorded in artifact
      p3-t10-nullable.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P3-T11] Run CMD-VSTEST followed by CMD-TRXCOUNTERS with a results directory named for this
      task. **Acceptance:** EXIT_CODE 0, `failed=0`, and `total` equal to BASELINE_TEST_TOTAL.
      Recorded in artifact p3-t11-tests.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P3-T12] Measure with CMD-LINECOUNT the two code files touched in this phase and also
      `QuickFiler/Controllers/QfcQueue.cs`, `QuickFiler/Controllers/QfcQueue.UiIdle.cs` and
      `QuickFiler/Interfaces/IUiIdleDispatcher.cs`. **Acceptance:** each measured count is strictly
      less than 500 and each is recorded as a labelled numeric line in artifact
      p3-t12-line-counts.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P3-T13] Commit Phase 3 with a single-line message. **Acceptance:** CMD-DIFF is run afterwards
      and every path it reports satisfies the Scope-lock rule.

---

### Phase 4 — Regression suite

All new tests land in the new partial test class, whose default allocation puts the shared harness
in the harness part and every `[TestMethod]` in the test-class part; P4-T20 and the remediation
branch of P5-T6 are the only two tasks permitted to depart from that allocation, and both depart
the same way: by moving whole test methods or arrangement from the test-class part into the harness
part. No existing test file is edited. The suite is one partial
test class split across two files: the harness in
`QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs` and every `[TestMethod]` that neither
P4-T20 nor the remediation branch of P5-T6 has moved in
`QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs`. Both must stay strictly under the 500-line
ceiling. Every task below uses the shared harness rather than repeating arrangement. P4-T20
measures both files, and P5-T6 re-measures them after the final format, because the formatter can
add physical lines.

Framework and libraries are MSTest, Moq and FluentAssertions only. No temporary file, no
filesystem, no network, no Outlook process, no sleep and no real wall-clock wait appears anywhere in
either file.

- [ ] [P4-T1] Create `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` containing the partial
      test class declaration and nothing else, and
      `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs` containing the same partial test
      class carrying the shared harness only: a factory building a real queue through the real primary
      constructor with a token drawn from a cancellation token source held by the harness, a literal
      null concrete home controller and a loose mock of the application globals, mirroring the
      construction pattern the existing coverage-expansion test file uses; a private sealed
      hand-written synchronous fake implementing `IUiIdleDispatcher` that invokes inline and returns
      a completed task for each of the three shapes; a recording item-group factory capturing the
      panel, mail item and index per call and returning a new item group carrying the supplied mail
      item with its viewer left null; a recording item-controller factory capturing all nine
      arguments and returning a mock item controller whose initialize member returns a completed
      task; a background-template factory returning a sentinel panel identifiable by reference; and
      a generic helper that, given a getter delegate and a setter delegate, asserts the getter is
      non-null and the setter throws `ArgumentNullException` on null. The fake is hand-written
      rather than built with Moq because two of the three interface members are generic methods
      whose return type depends on the type parameter, and the repository already hand-writes such
      fakes for the other dispatcher interface. **Acceptance:** both files exist and declare the same
      partial class; the test-class file declares exactly one `[TestClass]` and the harness file
      declares none; the two files together declare zero `[TestMethod]` members; each file's measured
      line count under CMD-LINECOUNT is strictly less than 500; and each file contains zero
      occurrences of each of the literals `Thread.Sleep`, `Task.Delay` and `DateTime.Now`.

- [ ] [P4-T2] Add a `<Compile Include>` item to `QuickFiler.Test/QuickFiler.Test.csproj` for each of
      `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` and
      `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs`, placed adjacent to the existing
      QfcQueue test items, which sit at lines 119, 120 and 215 at the recorded anchor.
      **Acceptance:** a search of `QuickFiler.Test/QuickFiler.Test.csproj` finds exactly one item
      naming each of the two files.

- [ ] [P4-T3] Run CMD-ANALYZE. **Acceptance:** EXIT_CODE 0, recorded in artifact
      p4-t3-analyze.2026-09-12T10-25.md under the qa-gates evidence directory. This is the positive
      verification that the three production manifest entries took effect, because the harness
      references a type declared in `QuickFiler/Interfaces/IUiIdleDispatcher.cs`. It does not verify
      the two test-project entries: at this task both test files declare zero `[TestMethod]` members
      and nothing references them, so an omitted entry leaves the file uncompiled and this build
      still exits 0. The detector for those two entries is P4-T4, whose CMD-BUILD step fails to
      compile the test-class part if the harness part is not in the manifest, and whose class-scoped
      run reports no cases if the test-class part is not.

- [ ] [P4-T4] Add six named seam-contract tests, one per seam, each a `[TestMethod]` whose body is a
      single call to the harness helper from P4-T1 with that seam's getter and setter. The six seams
      are `MoveMonitor`, `UiIdleDispatcher`, `ItemViewerFactory`, `ViewerRowPlacer`,
      `ItemGroupFactory` and `BackgroundTlpFactory`. Run CMD-BUILD, then CMD-VSTEST-CLASS followed by
      CMD-TRXCOUNTERS. **Acceptance:** CMD-BUILD exits 0 and prints the summary line `0 Error(s)`;
      then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0` and a `passed` count of at
      least 6, recorded in artifact p4-t4-seam-contracts.2026-09-12T10-25.md under the
      regression-testing evidence directory.

- [ ] [P4-T5] Add one named test constructing a queue in the headless test host and asserting that
      no exception is thrown, and additionally asserting that the value returned by the
      `UiIdleDispatcher` getter is of the adapter type declared in
      `QuickFiler/Controllers/QfcQueue.UiIdle.cs`. Run CMD-BUILD, then CMD-VSTEST-CLASS followed by
      CMD-TRXCOUNTERS. **Acceptance:** CMD-BUILD exits 0 and prints the summary line `0 Error(s)`;
      then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0`, recorded in artifact
      p4-t5-headless-construction.2026-09-12T10-25.md under the regression-testing evidence
      directory. The second assertion is also the positive reference that ties a test to a type
      declared in that new production file.

- [ ] [P4-T6] Add one named test asserting, without invoking the delegate, that the default value of
      `ItemViewerFactory` has a method whose name is the literal `Dequeue` and whose declaring type
      is the viewer queue helper class. The delegate must not be invoked, because invoking it would
      read the process-wide dispatcher. Run CMD-BUILD, then CMD-VSTEST-CLASS followed by
      CMD-TRXCOUNTERS. **Acceptance:** CMD-BUILD exits 0 and prints the summary line `0 Error(s)`;
      then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0`, recorded in artifact
      p4-t6-viewer-factory-identity.2026-09-12T10-25.md under the regression-testing evidence
      directory.

- [ ] [P4-T7] Add two named guard tests proving the enqueue member throws `ArgumentNullException`
      for a null item list and `ArgumentException` for an empty one, using the FluentAssertions
      asynchronous throw assertions. Run CMD-BUILD, then CMD-VSTEST-CLASS followed by
      CMD-TRXCOUNTERS. **Acceptance:** CMD-BUILD exits 0 and prints the summary line `0 Error(s)`;
      then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0`, recorded in artifact
      p4-t7-guards.2026-09-12T10-25.md under the regression-testing evidence directory.

- [ ] [P4-T8] Add one named success-path test that substitutes the dispatcher fake, the
      background-template factory and the item-group factory, enqueues a single page of mail-item
      mocks, and asserts the queue count becomes 1, that the dequeued tuple carries the exact panel
      reference the substituted background-template factory returned, and that the item groups
      appear in input order. Run CMD-BUILD, then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS.
      **Acceptance:** CMD-BUILD exits 0 and prints the summary line `0 Error(s)`; then
      CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0`, recorded in artifact
      p4-t8-success-path.2026-09-12T10-25.md under the regression-testing evidence directory.

- [ ] [P4-T9] Add one named test that captures the running-jobs count from inside a seam callback,
      proving the increment took effect mid-flight, and asserts the count is 0 after the call
      returns, proving the finally decrement. Run CMD-BUILD, then CMD-VSTEST-CLASS followed by
      CMD-TRXCOUNTERS. **Acceptance:** CMD-BUILD exits 0 and prints the summary line `0 Error(s)`;
      then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0`, recorded in artifact
      p4-t9-counter-bookkeeping.2026-09-12T10-25.md under the regression-testing evidence directory.

- [ ] [P4-T10] Add two named catch-path tests making the substituted item-group factory throw an
      `OperationCanceledException` and an `InvalidOperationException` respectively; in both cases the
      enqueue member must not propagate, the queue count must stay 0 and the running-jobs count must
      return to 0. The throw is raised from inside the try block, so the counter decrements normally;
      no test raises from the background-template factory or the hook loop, because a throw from
      there would require the separately promoted counter-leak behaviour to be treated as expected,
      which this item forbids. Run CMD-BUILD, then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS.
      **Acceptance:** CMD-BUILD exits 0 and prints the summary line `0 Error(s)`; then
      CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0`, recorded in artifact
      p4-t10-catch-paths.2026-09-12T10-25.md under the regression-testing evidence directory.

- [ ] [P4-T11] Add two named tests covering both arms of the collection-changed notification: one
      subscribing and asserting exactly one event whose action is the add action, and one running
      the same flow with no subscriber attached and asserting no exception. Run CMD-BUILD, then
      CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS. **Acceptance:** CMD-BUILD exits 0 and prints the
      summary line `0 Error(s)`; then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports
      `failed=0`, recorded in artifact p4-t11-collection-changed.2026-09-12T10-25.md under the
      regression-testing evidence directory.

- [ ] [P4-T12] Add one named test assigning a strict-behaviour mock of the move-monitor interface
      through `MoveMonitor` and verifying the hook member is called exactly once per item with that
      item and a non-null action delegate. The captured delegate is not invoked, because it is an
      async-void lambda. Run CMD-BUILD, then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS.
      **Acceptance:** CMD-BUILD exits 0 and prints the summary line `0 Error(s)`; then
      CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0`, recorded in artifact
      p4-t12-move-monitor.2026-09-12T10-25.md under the regression-testing evidence directory.

- [ ] [P4-T13] Add one named data-driven test with three rows covering item totals of 9, 10 and 11
      and asserting the digits argument captured by the recording item-controller factory is 1, 2
      and 2 respectively. Run CMD-BUILD, then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS.
      **Acceptance:** CMD-BUILD exits 0 and prints the summary line `0 Error(s)`; then
      CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0` and the artifact records the
      `total` this run reported, the `total` the preceding task recorded, and the difference between
      them, and states whether the trx counters element counted the three data rows as three results
      or as four with an aggregate parent. The gate is that the difference is 3 or 4 and that all
      three data rows appear as passed results in the trx; the artifact records which of the two the
      runner produced, because the same counting rule applies to the class-scoped run P4-T19 records
      and to the whole-assembly run P4-T24 records, which is what makes P4-T24's arithmetic identity
      hold whichever rule the runner applies. Recorded in
      artifact p4-t13-digits.2026-09-12T10-25.md under the regression-testing evidence directory.

- [ ] [P4-T14] Add two named tests covering the carrier-found and carrier-absent outcomes of the
      carried-handler resolution: one supplying a pre-scored carrier list containing an entry built
      for the enqueued mail item and asserting the captured carried-handler argument is that
      entry's handler, and one supplying a null carrier list and asserting the captured argument is
      null. Run CMD-BUILD, then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS. **Acceptance:**
      CMD-BUILD exits 0 and prints the summary line `0 Error(s)`; then CMD-VSTEST-CLASS followed by
      CMD-TRXCOUNTERS reports `failed=0`, recorded in artifact
      p4-t14-carrier.2026-09-12T10-25.md under the regression-testing evidence directory.

- [ ] [P4-T15] Add two named tests covering the item-controller pass-through: one asserting every
      one of the nine captured arguments, namely the globals instance the queue was constructed
      with, the home controller value the queue holds, the collection controller passed to the
      enqueue call, the viewer value which is null because the recording item-group factory leaves
      it null, the one-based position, the digits value, the mail item, the value of the panel-cell
      states property, and the carried handler; and one verifying the item controller's initialize
      member is awaited exactly once per row. Run CMD-BUILD, then CMD-VSTEST-CLASS followed by
      CMD-TRXCOUNTERS. **Acceptance:** CMD-BUILD exits 0 and prints the summary line `0 Error(s)`;
      then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0`, recorded in artifact
      p4-t15-controller-passthrough.2026-09-12T10-25.md under the regression-testing evidence
      directory.

- [ ] [P4-T16] Add one named test pinning the index mapping for a non-zero start. The loader member
      is private, so the test obtains it by reflection on the queue type, invokes it with a start
      value of 9 and a single-item list, awaits the returned value-task, and asserts the recording
      item-group factory received that single item at index 9 while the item-controller factory
      received a digits value of 2. Run CMD-BUILD, then CMD-VSTEST-CLASS followed by
      CMD-TRXCOUNTERS. **Acceptance:** CMD-BUILD exits 0 and prints the summary line `0 Error(s)`;
      then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0`, recorded in artifact
      p4-t16-index-mapping.2026-09-12T10-25.md under the regression-testing evidence directory.

- [ ] [P4-T17] Add one named test that leaves `ItemGroupFactory` at its default, substitutes only
      `ItemViewerFactory` and `ViewerRowPlacer` plus the dispatcher fake, calls `AddAsync` directly,
      and asserts the returned item group carries the supplied mail item, that the viewer factory
      received the token the queue was constructed with, and that the row placer received the panel,
      the viewer the factory returned and the index. This exercises the production body of
      `AddAsync` rather than displacing it. Run CMD-BUILD, then CMD-VSTEST-CLASS followed by
      CMD-TRXCOUNTERS. **Acceptance:** CMD-BUILD exits 0 and prints the summary line `0 Error(s)`;
      then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0`, recorded in artifact
      p4-t17-addasync-body.2026-09-12T10-25.md under the regression-testing evidence directory.

- [ ] [P4-T18] Add one named test that substitutes only the dispatcher, leaving the item-group,
      viewer and row-placer seams at their defaults except for the viewer factory and row placer
      needed to keep the flow headless, drives one enqueue call, and asserts the substituted
      dispatcher recorded one invocation of each of the three shapes, and that the queue count
      became 1 so that the observable behaviour is unchanged. Run CMD-BUILD, then CMD-VSTEST-CLASS
      followed by CMD-TRXCOUNTERS. **Acceptance:** CMD-BUILD exits 0 and prints the summary line
      `0 Error(s)`; then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0`, recorded in
      artifact p4-t18-dispatcher-shapes.2026-09-12T10-25.md under the regression-testing evidence
      directory.

- [ ] [P4-T19] Add one named test asserting that the panel reference the substituted
      background-template factory returns is the same reference that reaches the dequeued queue
      entry, proving the value flows through unmodified. Run CMD-BUILD, then CMD-VSTEST-CLASS
      followed by CMD-TRXCOUNTERS. **Acceptance:** CMD-BUILD exits 0 and prints the summary line
      `0 Error(s)`; then CMD-VSTEST-CLASS followed by CMD-TRXCOUNTERS reports `failed=0` and the
      artifact records the `total`, `executed`, `passed` and `failed` integers from the counters
      line, because P4-T24's arithmetic identity reads the `total` from this artifact. Recorded in
      artifact p4-t19-background-template.2026-09-12T10-25.md under the regression-testing evidence
      directory.

- [ ] [P4-T20] Measure `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` and
      `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs` with CMD-LINECOUNT.
      **Acceptance:** each measured count is strictly less than 470 and each is recorded as a
      labelled numeric line in artifact p4-t20-test-file-size.2026-09-12T10-25.md under the qa-gates
      evidence directory, together with the remaining headroom to 500. The trigger is 470 rather
      than 500 because the repo-wide format in P5-T1 can add physical lines to a file — it
      chain-wraps a fluent assertion past the 100-column default and inserts a blank line before a
      comment that follows a statement — so a file measured just under the ceiling here can cross it
      there. A count of 470 or greater is resolved by moving whole test methods from the test-class
      file into the harness file, or arrangement out of a test method into the harness file, until
      both counts are under 470; no third file is created, because the Write Set permits exactly
      these two. The authoritative measurement for the acceptance criterion is taken again after the
      final format in P5-T6.

- [ ] [P4-T21] Run CMD-FORMAT-SCOPED over `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` and
      `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs`, capturing the porcelain status
      immediately before and after, then CMD-CHECK-SCOPED over the same paths. **Acceptance:** both
      porcelain captures are recorded verbatim in artifact
      p4-t21-format.2026-09-12T10-25.md under the qa-gates evidence directory and CMD-CHECK-SCOPED
      exits 0.

- [ ] [P4-T22] Run CMD-ANALYZE. **Acceptance:** EXIT_CODE 0, recorded in artifact
      p4-t22-analyze.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P4-T23] Run CMD-NULLABLE. **Acceptance:** EXIT_CODE 0, recorded in artifact
      p4-t23-nullable.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P4-T24] Run CMD-VSTEST across the whole test assembly followed by CMD-TRXCOUNTERS with a
      results directory named for this task. **Acceptance:** EXIT_CODE 0, `failed=0`, and `total`
      strictly greater than BASELINE_TEST_TOTAL by exactly the `total` that the P4-T19 artifact
      recorded from CMD-TRXCOUNTERS. That figure is the class-scoped count of every case the new
      suite contributes, because the class filter matches a name fragment that no existing test
      class carries and P4-T19 is the last task that adds a case; P4-T20 and P4-T21 move and
      reformat cases but add none. The artifact states BASELINE_TEST_TOTAL, that figure, their sum,
      and the observed whole-assembly `total` on an explicit arithmetic line, and the observed total
      must equal the sum. No count predicted from this plan is used. Recorded in artifact
      p4-t24-tests.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P4-T25] Commit Phase 4 with a single-line message. **Acceptance:** CMD-DIFF is run afterwards
      and every path it reports satisfies the Scope-lock rule.

---

### Phase 5 — Final QC loop

This phase runs the full four-step C# toolchain unconditionally and in order, in coverage mode. If
any step fails or rewrites a file, restart this phase from P5-T1. No step in this phase may be
recorded as skipped.

- [ ] [P5-T1] Run CMD-FORMAT over the whole tree, capturing the porcelain status immediately before
      and immediately after the command. **Acceptance:** both porcelain captures are recorded
      verbatim in artifact p5-t1-format.2026-09-12T10-25.md under the qa-gates evidence directory,
      so that a run which rewrote nothing is distinguishable from one that repaired drift; and every
      path that appears in the after-capture but not in the before-capture satisfies the Scope-lock
      rule. A path outside the Write Set that this repo-wide pass rewrites is admitted only when it
      appears on the `PreExistingDriftFiles:` line recorded by P0-T8. A path in neither category is a
      scope-lock failure. This artifact carries a line whose first token is
      `FormatterRepairedPreExistingDrift:` listing every path on that P0-T8 line that lies outside
      the Write Set, or the single token `NONE` when P0-T8 recorded `NONE` or recorded only paths
      inside the Write Set. The list is derived from the P0-T8 record rather than from what this
      particular pass rewrote: this phase restarts whenever a step rewrites a file, so the pass
      P5-T8 finally records is by construction one in which the formatter rewrote nothing, and a
      list keyed to the rewriting pass would always read `NONE` in the artifact that survives while
      the repairs an earlier pass made are still present in the tree. This list is not a waiver: it
      is carried into P6-T6 and into P7-T22, because a formatter repair of a file outside the Write
      Set is still a change to that file.

- [ ] [P5-T2] Run CMD-CHECK over the whole tree. **Acceptance:** EXIT_CODE 0, recorded in artifact
      p5-t2-check.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P5-T3] Run CMD-ANALYZE. **Acceptance:** EXIT_CODE 0, with the error and warning counts
      captured by the anchored-pattern rule of P0-T9, recorded in artifact
      p5-t3-analyze.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P5-T4] Run CMD-NULLABLE. **Acceptance:** EXIT_CODE 0, recorded in artifact
      p5-t4-nullable.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P5-T5] Run CMD-COVERAGE with the Cobertura output written as
      coverage-postchange.2026-09-12T10-25.cobertura.xml under
      `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/`,
      and interpret it in artifact p5-t5-coverage-postchange.2026-09-12T10-25.md in the same
      directory. The command string is identical to P0-T12 apart from the output path, so the two
      measurements are directly comparable. **Acceptance:** the command printed the token
      `COVERAGE-ARTIFACT-WRITTEN`; the Cobertura document contains a filename attribute for each of
      `QuickFiler/Controllers/QfcQueue.cs`, `QuickFiler/Controllers/QfcQueue.Enqueue.cs`,
      `QuickFiler/Controllers/QfcQueue.Tlp.cs` and `QuickFiler/Controllers/QfcQueue.UiIdle.cs` in the
      backslash-separated form the post-processor writes; and the Markdown artifact records, as
      labelled numeric lines, the document-level line-rate, lines-covered and lines-valid; the
      QuickFiler package `LinesCovered`, `LinesValid` and `LineRate` from
      `Get-CoberturaPackageLineSummary`; and the `LineRate`, `LinesCovered` and `LinesValid` from
      `Get-CoberturaClassLineSummary` for the class element of each of those four files. As in
      P0-T12, the `EXIT_CODE:`, `ThresholdAssertion:` and conditional `ExpectedExitCode:` lines are
      recorded by observation under the same rule, and the runner's own repository-wide
      document-level assertion is not this task's gate either way.

- [ ] [P5-T6] Measure with CMD-LINECOUNT, after the final format, every production file in the Write
      Set plus the two new test files, namely `QuickFiler/Controllers/QfcQueue.cs`,
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, `QuickFiler/Controllers/QfcQueue.Tlp.cs`,
      `QuickFiler/Controllers/QfcQueue.UiIdle.cs`, `QuickFiler/Interfaces/IUiIdleDispatcher.cs`,
      `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` and
      `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs`. **Acceptance:** all seven
      measured counts are strictly less than 500 and each is recorded as a labelled numeric line in
      artifact p5-t6-line-counts-final.2026-09-12T10-25.md under the qa-gates evidence directory.
      This measurement is taken after the formatter has run, because the formatter can change a
      file's physical line count; a count taken before it is not load-bearing. This artifact is the
      evidence for the file-size acceptance criterion. If any count is 500 or greater, this task is
      not complete: move whole test methods or arrangement between the two test-file parts until
      every count is under 500, then re-run P5-T1 through P5-T6 as a fresh pass of the loop. A count
      at or over the ceiling is never recorded as a pass with an explanation.

- [ ] [P5-T7] Run CMD-VSTEST across the whole test assembly followed by CMD-TRXCOUNTERS with a
      results directory named for this task, as the fourth step of the loop. **Acceptance:**
      EXIT_CODE 0 and `failed=0`, with `total`, `executed`, `passed` and `failed` recorded in
      artifact p5-t7-tests-final.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P5-T8] Record the loop outcome. **Acceptance:** artifact
      p5-t8-loop-result.2026-09-12T10-25.md under the qa-gates evidence directory states the number
      of the pass through P5-T1 through P5-T7 that completed with every step exiting as required and
      with P5-T1 rewriting no file, and names the artifact produced by each of those seven tasks in
      that pass. A recorded pass in which the formatter rewrote a file is not a completed loop.

- [ ] [P5-T9] Commit Phase 5 with a single-line message. **Acceptance:** CMD-DIFF is run afterwards
      and every path it reports satisfies the Scope-lock rule.

---

### Phase 6 — Coverage analysis, residual record and diff review

Every task in this phase writes Markdown only. No source, test or project file is modified here, so
the clean toolchain pass recorded in Phase 5 remains valid through the end of the plan.

- [ ] [P6-T1] Compare the file-level line rates. **Acceptance:** artifact
      p6-t1-coverage-file-rates.2026-09-12T10-25.md under the qa-gates evidence directory records,
      as labelled numeric lines: the spec's recorded pre-change rate of 0.152941 for
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs`; the rate P0-T12 measured for that file; the rate
      P5-T5 measured for it; the spec's recorded pre-change rate of 0.503205 for
      `QuickFiler/Controllers/QfcQueue.cs`; the rate P0-T12 measured for it; and the post-change
      combined rate for `QuickFiler/Controllers/QfcQueue.cs`,
      `QuickFiler/Controllers/QfcQueue.Tlp.cs` and `QuickFiler/Controllers/QfcQueue.UiIdle.cs`,
      computed as the sum of the `LinesCovered` values divided by the sum of the `LinesValid` values
      that `Get-CoberturaClassLineSummary` returns for the three class elements of the P5-T5 document
      whose filename attributes name those three files. The gate is that the post-change enqueue-part
      rate is strictly greater than both 0.152941 and the P0-T12 measurement, and that the
      post-change combined rate is not below either 0.503205 or the P0-T12 measurement for the
      pre-split file. A failure of either clause is reported, not waived.

- [ ] [P6-T2] Derive new-code coverage. **Acceptance:** artifact
      p6-t2-new-code-coverage.2026-09-12T10-25.md under the qa-gates evidence directory carries a
      line-by-line table derived by this procedure, which a third party re-running it obtains the
      same table from. Run an anchored, rename-disabled, zero-context diff of the five production
      Write Set code paths against the sha P0-T2 recorded. Take the added lines, meaning every output
      line whose first character is a plus sign and which is not a file header line, and the removed
      lines the same way from the minus side. A line is relocated when its text, with leading and
      trailing whitespace stripped, is identical to the stripped text of at least one removed line;
      it is genuinely new otherwise. No other classification is permitted and no line is classified
      by judgment. A line that carries no line element in the P5-T5 document carries no executable
      statement and is excluded from both the numerator and the denominator of both rates; the
      artifact records how many lines that removed. Each remaining line's hit count is read from the
      P5-T5 document by matching the file's filename attribute in the backslash-separated form the
      post-processor writes and the line element's number attribute.
      The artifact then reports two rates: the genuinely-new line
      rate and the relocated line rate, each with its covered and valid counts. The gate is that the
      genuinely-new line rate is at least 0.90, which is the new-code floor stated in the standing
      instructions file. The relocated rate is reported alongside the rate the same statements
      carried at the anchor, and any relocated statement that was uncovered at the anchor and is
      still uncovered is carried into the residual record written by P6-T3 rather than being
      excluded from measurement; no coverage-exclusion attribute and no assembly-level exclusion is
      introduced anywhere in this change.

- [ ] [P6-T3] Write the residual-region record
      residual-uncovered-regions.2026-09-12T10-25.md into
      `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/`.
      **Acceptance:** the record names every region on the enqueue path still reported at zero hits
      by the P5-T5 document, states why each remains uncovered, and cites the P5-T5 Cobertura
      artifact by file name and element for each. It must address at minimum: the reflection-driven
      control clone in the utilities extensions namespace and the template setter that calls it,
      which remain uncovered because the background-template seam deliberately bypasses them; the
      dead, entirely commented-out template-activation member relocated into
      `QuickFiler/Controllers/QfcQueue.Tlp.cs`, whose deletion is out of scope for this item; the
      three relocated marshalling bodies inside the adapter class in
      `QuickFiler/Controllers/QfcQueue.UiIdle.cs`, which cannot be reached without a live
      process-wide dispatcher; the default lambda of `BackgroundTlpFactory`, which is deliberately
      never invoked by any test; and every remaining statement in
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs` or in `AddAsync` still reported at zero hits.
      Leaving a zero-hit region out of this record is a failure of this task.

- [ ] [P6-T4] Project repository-wide line coverage. A repository-wide measurement cannot be taken
      on this host, because a whole-solution run stalls on four shell-icon test classes in another
      assembly; the projection below is therefore the instrument. **Acceptance:** artifact
      p6-t4-repo-wide-projection.2026-09-12T10-25.md under the qa-gates evidence directory records:
      the lines-covered and lines-valid of the most recent committed repository-wide Cobertura
      document in the tree, which is the post-change coverage artifact under the evidence qa-gates
      directory of the 2026-09-08 etl-deadline-mechanics follow-ups feature folder for item 825, a
      file whose name begins with the words coverage postchange, namely 56029 and 65402,
      both re-derived from that document this pass; the QuickFiler package `LinesCovered` and
      `LinesValid` that P0-T12 and P5-T5 each recorded from `Get-CoberturaPackageLineSummary`; the
      two deltas; and the projected repository-wide rate
      computed as the reference covered plus the covered delta, divided by the reference valid plus
      the valid delta. The gate has two clauses. The first, which is the discriminating one, is the
      QuickFiler package delta rate: the covered delta divided by the valid delta must be at least
      0.80. When the valid delta is zero or negative, this clause is satisfied only if the covered
      delta is zero or greater, and the artifact states that fact explicitly instead of computing a
      rate. The second clause is that the projected repository-wide rate is at least 0.80, which is
      the repository-wide floor stated in the standing instructions file. The artifact states plainly
      that the second clause is dominated by the reference document and cannot on its own
      discriminate the quality of this change — 56029 divided by 65402 is already 0.856686, and no
      delta this change can produce moves that quotient below 0.80 — so the second clause records the
      floor while the first clause is what can fail. The artifact additionally
      records, as a factual note and not as a gate, that a competing 85 percent line and 75 percent
      branch figure appears in the general unit-test rule file and the quality-tiers rule file, that
      the standing instructions file's stated policy compliance order places itself first and does
      not name those rule files, and that no tier-classification criterion is written anywhere in
      this plan because the tier manifest those rule files refer to does not exist at the repository
      root.

- [ ] [P6-T5] Write the no-behaviour-change diff review
      p6-t5-diff-review.2026-09-12T10-25.md into the qa-gates evidence directory. **Acceptance:**
      the artifact records the output of an anchored diff of
      `QuickFiler/Controllers/QfcQueue.cs`, `QuickFiler/Controllers/QfcQueue.Enqueue.cs`,
      `QuickFiler/Controllers/QfcQueue.Tlp.cs` and `QuickFiler/Controllers/QfcQueue.UiIdle.cs`
      against the recorded anchor, paired with the porcelain capture from CMD-DIFF so that files
      created by this change are visible, and states an explicit verdict line for each of these
      eight properties: every relocated member moved verbatim apart from the named seam
      substitutions; no nullable pragma was added to any relocated code; the obsolete-API pragma
      pair around the async-enumerable projection in
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs` is intact, the disable and the restore both
      present exactly once; every region opens and closes on the same side of the split; the two
      catch blocks, the single error-log call and its message string are unchanged; no public member
      of the queue class was added, removed, retyped or resigned; and no occurrence of the dotted
      framework-default dispatcher priority token was introduced anywhere in the Write Set; and the
      marshalling wrapper at line 275 of `QuickFiler/Controllers/QfcQueue.cs` at the anchor relocated
      into `QuickFiler/Controllers/QfcQueue.Tlp.cs` with its `UiIdleCallAsync` wrapper unchanged and
      only its inner argument substituted, while the call sites at line 197 of
      `QuickFiler/Controllers/QfcQueue.cs` and line 105 of
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs` are byte-identical to the anchor.

- [ ] [P6-T6] Write the untouched-files record p6-t6-scope-lock.2026-09-12T10-25.md into the
      qa-gates evidence directory. **Acceptance:** the artifact records the full output of CMD-DIFF
      and states, for every path it reports, which clause of the Scope-lock rule admits it; it
      records explicitly that the three existing QfcQueue test files in the QuickFiler test
      project's Controllers folder, the threading types in the utilities assembly and the utilities
      extension that performs the control clone appear in neither the diff nor the porcelain output;
      and it cites the P5-T7 result as the proof that the full test assembly passes with those files
      unchanged. It also states, for each path admitted by the anchor clause of the Scope-lock rule,
      that it was present at the anchor and is reproduced from the `PreExistingWorktreePaths:` block
      of the P0-T2 artifact rather than produced by this item. It also reproduces the
      `FormatterRepairedPreExistingDrift:` line from the P5-T1 artifact and states that it agrees
      with the `PreExistingDriftFiles:` line P0-T8 recorded. When P0-T8 recorded a path outside the
      Write Set that the P5-T1 line does not name, that disagreement is a failure of this task and
      is reported rather than reconciled to `NONE`.

- [ ] [P6-T7] Add the link to the separately promoted out-of-scope defect into the Rollout and
      Follow-up section of
      `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md`.
      The target is the potential-bug entry captured on 2026-09-12 for the enqueue running-jobs
      counter leak, which lives under the potential features directory and whose file name begins
      with that date followed by the words qfcqueue enqueueasync jobsrunning counter leak. Add it as
      a relative Markdown link in the existing Links bullet without altering any other sentence in
      that section. **Acceptance:** the spec's Rollout and Follow-up section contains exactly one
      link whose target resolves to an existing file, verified by a path-existence check recorded in
      artifact p6-t7-followup-link.2026-09-12T10-25.md under the qa-gates evidence directory.

- [ ] [P6-T8] Record the two carried-forward obligations that this item's completion enables but
      does not itself discharge, in artifact p6-t8-carried-obligations.2026-09-12T10-25.md under the
      qa-gates evidence directory. **Acceptance:** the artifact names the item 678 acceptance
      criterion AC20 and the issue 727 sub-finding 4, states that both are recorded in an archived
      feature folder outside this Write Set and are therefore closed out by the owning orchestrator
      rather than by any task in this plan, and cites the P5-T5 coverage artifact and the P6-T1
      comparison as the evidence that the coverage movement they wait on has occurred. It also
      records that the out-of-scope counter-leak defect already holds a potential-bug entry in the
      potential features directory, so no new entry is filed by this plan.

- [ ] [P6-T9] Update
      `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/issue.md`
      with an outcome note naming the six seams, the two new production partial files, the new
      interface file and the two new test files, and pointing at the P5-T5 coverage artifact and the
      P6-T3 residual record by file name. **Acceptance:** the issue document contains a dated
      outcome note carrying those references and no other section of it is modified.

---

### Phase 7 — Acceptance-criteria check-off and close-out

Each task in this phase flips exactly one checkbox in the `## Acceptance Criteria` section of
`docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md` and
records that AC's evidence pointer beside the check-off in the completion record. A task may be
checked off only when the named evidence exists on disk and its acceptance condition is met.
Markdown only; no code file is touched.

- [ ] [P7-T1] Check off AC1 in the spec. **Acceptance:** the AC1 checkbox reads checked and the
      completion record cites the P2-T1 artifact, the P4-T4 artifact and the P2-T9 artifact.

- [ ] [P7-T2] Check off AC2 in the spec. **Acceptance:** the AC2 checkbox reads checked and the
      completion record cites the P2-T3 acceptance, the P2-T5 acceptance, the P4-T18 artifact and
      the P6-T5 diff review.

- [ ] [P7-T3] Check off AC3 in the spec. **Acceptance:** the AC3 checkbox reads checked and the
      completion record cites the P3-T1 acceptance, the P4-T6 artifact and the P4-T17 artifact.

- [ ] [P7-T4] Check off AC4 in the spec. **Acceptance:** the AC4 checkbox reads checked and the
      completion record cites the P3-T2 acceptance, the P4-T4 artifact and the P4-T17 artifact.

- [ ] [P7-T5] Check off AC5 in the spec. **Acceptance:** the AC5 checkbox reads checked and the
      completion record cites the P3-T3 acceptance, the P3-T5 acceptance, the `PostFormatStatement:`
      line of the P3-T8 artifact, the P4-T4 artifact and the P4-T16 artifact.

- [ ] [P7-T6] Check off AC6 in the spec. **Acceptance:** the AC6 checkbox reads checked and the
      completion record cites the P3-T4 acceptance, the P3-T6 acceptance and the P4-T19 artifact.

- [ ] [P7-T7] Check off AC7 in the spec. **Acceptance:** the AC7 checkbox reads checked and the
      completion record cites the P4-T4 artifact and the P4-T5 artifact.

- [ ] [P7-T8] Check off AC8 in the spec. **Acceptance:** the AC8 checkbox reads checked and the
      completion record cites the P5-T6 artifact, which carries the seven post-format measured line
      counts; no predicted figure is cited.

- [ ] [P7-T9] Check off AC9 in the spec. **Acceptance:** the AC9 checkbox reads checked and the
      completion record cites the P1-T3 acceptance, the P2-T4 acceptance, the P4-T2 acceptance, the
      P4-T3 analyzer-build artifact as the positive verification of the three production entries,
      the P4-T4 artifact as the positive verification of the two test-project entries, and the
      P4-T5 artifact as the positive reference from a test to a type declared in each new
      production file.

- [ ] [P7-T10] Check off AC10 in the spec. **Acceptance:** the AC10 checkbox reads checked and the
      completion record cites the P4-T7 artifact.

- [ ] [P7-T11] Check off AC11 in the spec. **Acceptance:** the AC11 checkbox reads checked and the
      completion record cites the P4-T8 artifact.

- [ ] [P7-T12] Check off AC12 in the spec. **Acceptance:** the AC12 checkbox reads checked and the
      completion record cites the P4-T10 artifact.

- [ ] [P7-T13] Check off AC13 in the spec. **Acceptance:** the AC13 checkbox reads checked and the
      completion record cites the P4-T9 artifact, the P4-T8 artifact and the P4-T10 artifact.

- [ ] [P7-T14] Check off AC14 in the spec. **Acceptance:** the AC14 checkbox reads checked and the
      completion record cites the P4-T11 artifact.

- [ ] [P7-T15] Check off AC15 in the spec. **Acceptance:** the AC15 checkbox reads checked and the
      completion record cites the P4-T12 artifact.

- [ ] [P7-T16] Check off AC16 in the spec. **Acceptance:** the AC16 checkbox reads checked and the
      completion record cites the P4-T13 artifact, the P4-T14 artifact and the P4-T15 artifact.

- [ ] [P7-T17] Check off AC17 in the spec. **Acceptance:** the AC17 checkbox reads checked and the
      completion record cites the P4-T17 artifact.

- [ ] [P7-T18] Check off AC18 in the spec. **Acceptance:** the AC18 checkbox reads checked and the
      completion record cites the P6-T5 diff review and the P5-T8 loop result.

- [ ] [P7-T19] Check off AC19 in the spec. **Acceptance:** the AC19 checkbox reads checked and the
      completion record cites the P0-T12 baseline Cobertura artifact, the P5-T5 post-change
      Cobertura artifact, the P6-T1 file-rate comparison, the P6-T2 new-code derivation and the
      P6-T4 repository-wide projection, and names both Cobertura artifacts by file name.

- [ ] [P7-T20] Check off AC20 in the spec. **Acceptance:** the AC20 checkbox reads checked and the
      completion record cites the P6-T3 residual record.

- [ ] [P7-T21] Check off AC21 in the spec. **Acceptance:** the AC21 checkbox reads checked and the
      completion record cites the P3-T7 artifact, the P6-T7 follow-up link and an explicit statement
      that no test in either `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` or
      `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs` asserts the counter-leak
      behaviour as correct, supported by the P4-T10 task text which confines both throws to the
      substituted item-group factory inside the try block.

- [ ] [P7-T22] Check off AC22 in the spec. **Acceptance:** the
      `FormatterRepairedPreExistingDrift:` line in the P6-T6 record reads `NONE`, and the AC22
      checkbox reads checked, and the completion record cites the P6-T6 scope-lock record and the
      P5-T7 full-assembly test result. When that line is not `NONE`, AC22 is not checked off: the
      completion record states the list and reports that the repository-wide format command the
      standing instructions file mandates repaired pre-existing drift in files outside the Write Set,
      which is a conflict between two repository rules that this item cannot resolve and must
      escalate rather than waive.

- [ ] [P7-T23] Write the completion record
      p7-t23-completion-record.2026-09-12T10-25.md into the qa-gates evidence directory, carrying
      one row per acceptance criterion with its evidence pointers as recorded by P7-T1 through
      P7-T22. **Acceptance:** the record carries exactly 22 rows, one per criterion, with no
      criterion absent and none duplicated.

- [ ] [P7-T24] Reconcile the spec against the evidence on disk. **Acceptance:** every one of the 22
      checkboxes in the spec's `## Acceptance Criteria` section reads checked, with one admitted
      exception: AC22 reads unchecked when, and only when, P7-T22 recorded the escalation branch
      because the `FormatterRepairedPreExistingDrift:` line in the P6-T6 record is not `NONE`. In
      that case this artifact reproduces that line and the escalation statement verbatim, records
      the reconciliation outcome as complete with one escalated criterion, and the AC status summary
      reports 21 of 22. Any other unchecked criterion is a reconciliation failure; for every checkbox,
      each evidence artifact the completion record names for it exists on disk under one of the
      three canonical evidence directories; and the reconciliation result is recorded in artifact
      p7-t24-reconciliation.2026-09-12T10-25.md under the qa-gates evidence directory. A checkbox
      whose named evidence is absent is a reconciliation failure and must be un-checked, not
      explained.

- [ ] [P7-T25] Commit the Phase 6 and Phase 7 documents and evidence with a single-line message.
      **Acceptance:** CMD-DIFF is run afterwards, every path it reports satisfies the Scope-lock
      rule, and the porcelain output contains no line naming a path under the three canonical
      evidence directories, proving that every artifact written after the Phase 5 commit has itself
      been committed.

---

## Traceability table

| AC | Implementation tasks | Verification tasks | Evidence |
|---|---|---|---|
| AC1 | P2-T1 | P4-T4, P2-T9, P7-T1 | p2-t1-s1, p4-t4-seam-contracts, p2-t9-tests |
| AC2 | P1-T1, P2-T3, P2-T5, P3-T2 | P4-T18, P6-T5, P7-T2 | p4-t18-dispatcher-shapes, p6-t5-diff-review |
| AC3 | P3-T1 | P4-T6, P4-T17, P7-T3 | p4-t6-viewer-factory-identity, p4-t17-addasync-body |
| AC4 | P3-T2 | P4-T4, P4-T17, P7-T4 | p4-t4-seam-contracts, p4-t17-addasync-body |
| AC5 | P3-T3, P3-T5 | P3-T8, P4-T4, P4-T16, P7-T5 | p3-t8-format, p4-t4-seam-contracts, p4-t16-index-mapping |
| AC6 | P3-T4, P3-T6 | P4-T19, P7-T6 | p4-t19-background-template |
| AC7 | P2-T1, P2-T5, P3-T1, P3-T2, P3-T3, P3-T4 | P4-T4, P4-T5, P7-T7 | p4-t4-seam-contracts, p4-t5-headless-construction |
| AC8 | P1-T1, P1-T2, P4-T1, P4-T20 | P5-T6, P7-T8 | p5-t6-line-counts-final |
| AC9 | P1-T3, P2-T4, P4-T2 | P4-T3, P4-T4, P4-T5, P7-T9 | p4-t3-analyze, p4-t4-seam-contracts, p4-t5-headless-construction |
| AC10 | P4-T7 | P4-T24, P7-T10 | p4-t7-guards |
| AC11 | P4-T8 | P4-T24, P7-T11 | p4-t8-success-path |
| AC12 | P4-T10 | P4-T24, P7-T12 | p4-t10-catch-paths |
| AC13 | P4-T9 | P4-T24, P7-T13 | p4-t9-counter-bookkeeping |
| AC14 | P4-T11 | P4-T24, P7-T14 | p4-t11-collection-changed |
| AC15 | P4-T12 | P4-T24, P7-T15 | p4-t12-move-monitor |
| AC16 | P4-T13, P4-T14, P4-T15 | P4-T24, P7-T16 | p4-t13-digits, p4-t14-carrier, p4-t15-controller-passthrough |
| AC17 | P4-T17 | P4-T24, P7-T17 | p4-t17-addasync-body |
| AC18 | P1-T1, P1-T2, P2-T5 | P6-T5, P5-T8, P7-T18 | p6-t5-diff-review, p5-t8-loop-result |
| AC19 | P4-T4 through P4-T19 | P6-T1, P6-T2, P6-T4, P7-T19 | coverage-baseline and coverage-postchange Cobertura, p6-t1, p6-t2, p6-t4 |
| AC20 | P6-T3 | P7-T20 | residual-uncovered-regions |
| AC21 | P6-T7 | P3-T7, P7-T21 | p3-t7-out-of-scope-untouched, p6-t7-followup-link |
| AC22 | none; the Scope-lock rule constrains every phase | P6-T6, P5-T7, P7-T22 | p6-t6-scope-lock, p5-t7-tests-final |

---

## Known divergences recorded factually

- Coverage floors used by this plan are 80 percent repository-wide and 90 percent for new code, as
  stated in the standing instructions file whose own policy compliance order lists itself first. A
  competing 85 percent line and 75 percent branch figure appears in the general unit-test rule file
  and the quality-tiers rule file. This plan does not adopt those figures and records the
  divergence once, here, without resolving it.
- No task in this plan depends on a tier classification. The tier manifest the quality-tiers rule
  file names does not exist at the repository root and no pipeline stage validates one, so any step
  asking to confirm a project's tier would be unsatisfiable.
- There is no Python toolchain in this repository. No task invokes one.
- The coverage runner's own document-level 80 percent assertion is a repository-wide gate whose
  outcome under a single-assembly search root has never been measured on this host. P0-T12 and
  P5-T5 record the branch it actually took as an observation, together with the observed exit code,
  and place their gates on artifact content instead; the repository-wide figure is handled by the
  projection in P6-T4.
