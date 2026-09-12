# kastringasync-keyequals-contains-offset (Plan)

- **Issue:** #583
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12T10-25
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-bug (per issue.md). Acceptance-criteria source is spec.md's Acceptance Criteria
  section only, per the acceptance-criteria-tracking skill's full-bug rule. user-story.md is
  intentionally absent; its absence is not a defect for this work mode.

## Task-count ledger (mechanically counted, per phase)

| Phase | Tasks |
|---|---|
| 0 | 9 (P0-T1..P0-T9) |
| 1 | 2 (P1-T1..P1-T2) |
| 2 | 1 (P2-T1) |
| 3 | 2 (P3-T1..P3-T2) |
| 4 | 1 (P4-T1) |
| 5 | 15 (P5-T1..P5-T15) |
| **Total** | **30** |

## Write-set discipline

An automated blast-radius extractor harvests every whitespace-free backticked token from this
plan and treats it as a claim that the diff writes that path, with no notion of polarity. To keep
this plan's backticked write claims from exceeding spec.md's Write Set, this plan reserves
backtick spans exclusively for the following repository-relative paths, all of which are entries
in that Write Set: issue.md, spec.md, this plan file, the research artifact, the two
production/test source files QuickFiler/Controllers/KaStringAsync.cs and
QuickFiler.Test/Controllers/KaStringAsyncTests.cs (written with their full directory-qualified
paths, never as a bare filename), and the four evidence subfolders under this feature's evidence
directory.

Every other command, flag, tool name, build output, settings file, identifier, field label, and
context path in this plan is written as plain text with no backticks, including command-line
switches (for example the vstest TestCaseFilter and InIsolation switches), MSBuild target and
property names, the git ref used to anchor diffs, bare source-file names with no directory
qualifier, and every path cited only for context. This applies in particular to: coverage.config,
dotnet-tools.json, the .dotnet-sdk per-worktree SDK junction, the packages directory, TaskMaster.sln,
scripts/vscode/Install-RepoDotNetSdk.ps1, scripts/vscode/TaskMaster.cli.runsettings, the
QuickFiler.Test build output assembly, the collection controller QfcCollectionController.cs, the
keyboard dispatch handler KeyboardHandler.cs, the QuickFiler.Test Controllers KbdActions test
file, and every path cited under the archived feature folder for issue #445
(docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445) as the source
of recorded gate output. None of these is created, modified, or deleted by this change; the
diff's only writes are the backtick-marked Write Set paths.

## Pinned file (read-only, never edited by this plan)

The QuickFiler.Test Controllers KbdActions test file — the file housing the test method named
for preserving keyboard-matching semantics across distinct stored keys — is pinned per the
maintainer decision in issue.md. No task in this plan edits it. Its unaffected status is asserted
only through test-run outcomes (P0-T9 baseline, P5-T6 post-change), never through a diff of that
file.

## Command Reference (applies to every task below that names one of these commands)

- **pwsh invocation.** Every command in this plan that is not a bare git invocation is run through
  pwsh -NoProfile -Command, with the outer quoting as single quotes around the whole payload,
  because the calling shell expands a dollar sign inside double quotes and would corrupt any
  PowerShell-side variable reference in the payload.
- **dotnet.exe resolution.** Resolve the pinned .NET SDK via the per-worktree .dotnet-sdk junction
  (running its dotnet.exe with the version switch) first; if that path does not resolve, run
  scripts/vscode/Install-RepoDotNetSdk.ps1 as the fallback provisioner (P0-T2). Every later dotnet
  invocation in this plan uses the resolved absolute path, referred to below as the resolved
  dotnet executable.
- **CSharpier invocation.** CSharpier is invoked only as the resolved dotnet executable's "tool run
  csharpier format" or "tool run csharpier check" subcommand, so the dotnet-tools.json
  manifest-pinned version 1.2.6 is used, never a global install. CSharpier 1.2.6 requires an
  explicit subcommand; a bare csharpier invocation with no subcommand does not run.
- **MSBuild / vstest resolution.** Neither msbuild nor vstest.console.exe is on PATH in this
  environment. Resolve each via vswhere.exe at the standard Visual Studio Installer path, using the
  latest-version, all-products, find-by-relative-tool-path form, and invoke the resolved absolute
  path, referred to below as the resolved vstest executable.
- **Rebuild, never a plain build, for the analyzer and nullable gates.** MSBuild's incremental
  up-to-date check does not invalidate on a command-line property change, so a warm plain-build
  invocation would return exit 0 with compilation skipped on every project and run no analyzers,
  making the gate incapable of failing. Every analyzer-rebuild and nullable-rebuild task in this
  plan therefore issues the solution's Rebuild target and records a non-vacuity proof (a zero count
  of compile-skip log lines in a detailed file log). A plain incremental build target is used only
  in P1-T2 and P4-T1: at P1-T2 its purpose is solely to compile the P1-T1 test-file edit into the
  QuickFiler.Test build output assembly; at P4-T1 its purpose is to compile the P2-T1 production
  fix and the P3-T1/P3-T2 test-file rewords into both the QuickFiler and QuickFiler.Test build
  output assemblies. Neither invocation gates on analyzer or nullable behavior.
- **No solution-wide nullable opt-in property anywhere.** No project in this repository carries a
  Nullable element; that MSBuild property is a solution-wide opt-in that CI deliberately omits. No
  task in this plan adds it.
- **git diff anchoring.** Every git diff in this plan is anchored to the origin main ref, this
  worktree's base (identical to the worktree HEAD at plan-authoring time), never left unanchored
  and never pinned to a literal commit SHA.

### Phase 0 — Context, Policy Reads, Toolchain Bootstrap, and Baselines

- [ ] [P0-T1] Read, in order, CLAUDE.md, .claude/rules/general-code-change.md,
  .claude/rules/general-unit-test.md, .claude/rules/quality-tiers.md, .claude/rules/tonality.md,
  and .claude/rules/csharp.md, plus
  docs/features/active/kastringasync-keyequals-contains-offset-583/issue.md,
  docs/features/active/kastringasync-keyequals-contains-offset-583/spec.md, and
  docs/features/active/kastringasync-keyequals-contains-offset-583/research/2026-09-12T10-35-kastringasync-keyequals-contains-offset-research.md
  end to end, then author
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/phase0-instructions-read.md`
  with a Timestamp field, a Policy Order field (the six-file order above), and the explicit list
  of every file read, including the two source files QuickFiler/Controllers/KaStringAsync.cs and
  QuickFiler.Test/Controllers/KaStringAsyncTests.cs (cited here as plain context, not as write
  targets of this task).
  Acceptance: the artifact exists and contains all three required fields with the ordered file
  list; no policy file is modified.

- [ ] [P0-T2] Resolve the pinned .NET SDK for this worktree (attempt the .dotnet-sdk junction's
  dotnet.exe with the version switch; if that path does not resolve, run
  scripts/vscode/Install-RepoDotNetSdk.ps1 as the fallback provisioner), then run the resolved
  dotnet executable's tool-restore command from the repository root to restore the CSharpier 1.2.6
  tool pinned by dotnet-tools.json, recording both steps in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/dotnet-bootstrap.md`.
  Acceptance: an observed exit code of 0 for the tool-restore invocation, with output naming
  csharpier version 1.2.6, matching the recorded success shape ("Tool 'csharpier' (version
  '1.2.6') was restored... Restore was successful.") at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/dotnet-tool-restore.2026-08-22T09-18.md.

- [ ] [P0-T3] Run a solution-wide NuGet restore of TaskMaster.sln from the repository root to
  populate this worktree's packages directory, then verify the restored package-directory count
  is at least 100, recording both steps in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/nuget-restore.md`.
  Acceptance: an observed exit code of 0 for the restore command and a recorded package-directory
  count of at least 100, consistent in kind with the recorded precedent (265 directories, floor
  150) at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/nuget-restore.2026-08-22T09-18.md.

- [ ] [P0-T4] Probe global-tool availability of the dotnet-coverage tool (a Get-Command lookup
  followed by its version switch); if absent, install it as a global dotnet tool via the resolved
  dotnet executable. Record the result in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/coverage-tool-probe.md`.
  Acceptance: the artifact records the tool as present, with a resolved path, a version string, and
  an observed exit code of 0, matching the recorded shape at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/coverage-tool-probe.2026-08-22T09-32.md.

- [ ] [P0-T5] Run the resolved dotnet executable's CSharpier check subcommand against the whole
  repository root (read-only) and record a Timestamp field, a Command field, an EXIT_CODE field,
  and an Output Summary field stating the observed files-checked count and files-needing-formatting
  count in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/csharpier-check.md`.
  Acceptance: an observed exit code of 0 and a summary line of the form "Checked N files in T ms."
  with 0 files needing formatting, matching the recorded clean-tree shape ("Checked 1517 files in
  6621ms.", exit code 0) at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/csharpier-check.2026-08-22T09-19.md.

- [ ] [P0-T6] Run the solution's analyzer Rebuild (Configuration Debug, Platform Any CPU, the
  EnableNETAnalyzers and EnforceCodeStyleInBuild properties both true) with a detailed file logger,
  then count the "Skipping target CoreCompile" and "CoreCompile:" log-line occurrences, and count
  the total warning lines. Record a Timestamp field, a Command field, an EXIT_CODE field, and an
  Output Summary field stating the observed warning count in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/msbuild-analyzers.md`.
  Acceptance: an observed exit code of 0, a verdict line reading "Build succeeded.", a
  "Skipping target CoreCompile" count of exactly 0, a "CoreCompile:" count of at least 9
  (proving the rebuild was not vacuous), and a recorded numeric warning count, which is the
  ceiling P5-T3 compares against, matching the recorded shape (0 skip / 96 CoreCompile / 5
  pre-existing System.Reactive packages.config warnings / 0 errors) at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/msbuild-analyzers.2026-08-22T09-21.md.

- [ ] [P0-T7] Run the solution's nullable Rebuild (Configuration Debug, Platform Any CPU, the
  TreatWarningsAsErrors property true, no nullable opt-in property) with a detailed file logger,
  then count the "Skipping target CoreCompile" log-line occurrences. Record a Timestamp field, a
  Command field, an EXIT_CODE field, and an Output Summary field in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/msbuild-nullable.md`.
  Acceptance: an observed exit code of 0, a verdict line reading "Build succeeded.", and a
  "Skipping target CoreCompile" count of exactly 0, matching the recorded shape (0 skip / 130
  CoreCompile / 5 pre-existing warnings / 0 errors) at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/msbuild-nullable.2026-08-22T09-23.md.

- [ ] [P0-T8] Run a coverage-instrumented test capture of the whole QuickFiler.Test.dll assembly,
  using dotnet-coverage collect wrapping the resolved vstest executable against that assembly with
  the TaskMaster CLI runsettings file, InIsolation, and the LiveOutlook-category exclusion filter,
  writing Cobertura-format output to
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/coverage-baseline.cobertura.xml`.
  Read the emitted Cobertura XML's root line-rate and branch-rate attributes, and aggregate every
  class element whose filename ends with the KaStringAsync source file name (deduplicated by line
  number, maximum hits) into a covered/total count for that file. Record both in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/coverage-baseline.md`.
  Acceptance: an observed exit code of 0; the inner test run reports "Test Run Successful." with 0
  Failed; the Output Summary field carries the numeric line-rate, branch-rate, and the
  KaStringAsync file's covered/total counts (not a placeholder), using the same per-file
  aggregation method recorded at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/coverage-baseline.2026-08-22T09-34.md.

- [ ] [P0-T9] Run a scoped, non-instrumented vstest filter selecting every test whose fully
  qualified name contains the pinned KbdActions test class name, against the QuickFiler.Test build
  output assembly, with InIsolation and the TaskMaster CLI runsettings file, to record the pinned
  KbdActions test file's baseline Passed/Failed count without ever diffing that file. Record a
  Timestamp field, a Command field, an EXIT_CODE field, and an Output Summary field in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/kbdactions-baseline.md`.
  Acceptance: an observed exit code of 0, a verdict line reading "Test Run Successful.", and a
  recorded Passed count with 0 Failed for that class, in the same per-class reporting shape (class
  name, with total, passed, failed and exit columns) recorded at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/vstest-baseline.2026-08-22T09-30.md;
  that archived record's own count, three of three on 2026-08-22, predates one test method since
  added to the pinned KbdActions test file, so this task's recorded count is expected to differ
  from that archived figure and is not compared against it.

### Phase 1 — Regression Test First (Red Before Fix)

- [ ] [P1-T1] Add a new test method named
  KeyEquals_ContainsMatchAtNonPrefixIndex_InvokesUpdateWithLastMatchedCharacter to
  `QuickFiler.Test/Controllers/KaStringAsyncTests.cs`, using the file's existing NewKa factory
  helper, MSTest attributes, a FluentAssertions assertion with a because-style string, and
  Arrange/Act/Assert section comments preceded by an Intent comment block naming issue #583 and
  both the pre-fix ("0") and post-fix ("1") results: construct with Key equal to "01", a non-null
  Update callback capturing its argument, Activated set to true, call KeyEquals with the argument
  "1", and assert the captured argument equals "1".
  Acceptance: the method exists in the file with exactly this Key/other/Activated arrangement and
  an assertion of the literal "1"; the file's pre-existing test methods are otherwise unchanged.

- [ ] [P1-T2] [expect-fail] Build TaskMaster.sln with a plain incremental build (Configuration Debug, Platform
  Any CPU) to compile the P1-T1 test into the QuickFiler.Test build output assembly, then run the
  resolved vstest executable against that assembly with InIsolation and a test-case filter
  selecting only the new test method by its exact fully qualified name, against unmodified
  production code (Phase 2 has not yet run). Record the failing run, with an ExpectedExitCode
  field declared as 1 so the evidence collector normalizes the row rather than reading a genuine
  failure, in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/regression-testing/red-before-fix.md`.
  Acceptance: the observed EXIT_CODE is 1, matching the declared ExpectedExitCode of 1; the verdict
  line reads "Test Run Failed."; exactly 1 test ran and it Failed, with the captured assertion
  showing an observed value of "0" against an expected "1" — the same failing-run shape (exit code
  1, ExpectedExitCode 1, "Test Run Failed.") recorded at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/regression-testing/red-before-fix.2026-08-22T09-38.md.

### Phase 2 — Minimal Production Fix

- [ ] [P2-T1] In `QuickFiler/Controllers/KaStringAsync.cs`, replace the branch-1 Update argument
  expression, currently reading Key.Substring(other.Length - 1, 1) at line 128, with
  Key.Substring(Key.IndexOf(other, StringComparison.Ordinal) + other.Length - 1, 1), leaving the
  Key.Contains(other) guard at line 125 and every other line of the method unchanged, and
  introducing no StartsWith call anywhere in the file.
  Acceptance: the file contains the new expression at that call site; the literal text
  "Substring(other.Length - 1, 1)" no longer appears anywhere in the file; the guard text
  "Key.Contains(other)" is present and unchanged; the file contains no occurrence of the text
  "StartsWith(".

### Phase 3 — Prose Rewords

- [ ] [P3-T1] In `QuickFiler.Test/Controllers/KaStringAsyncTests.cs`, reword the because-string in
  the test named for a contains-match while activated that invokes Update and returns true, from
  "Update receives Key.Substring(other.Length - 1, 1) => index 1 => \"b\"" to "Update receives the
  last character of the matched span (Key.IndexOf(\"ab\", StringComparison.Ordinal) + other.Length
  - 1 = 1) => \"b\"", without changing the asserted value "b".
  Acceptance: the old because-string no longer appears in the file; the new because-string appears
  verbatim; the test's assertion of the value "b" is unchanged.

- [ ] [P3-T2] In `QuickFiler/Controllers/KaStringAsync.cs`, reword the doc-comment sentence in the
  "Argument contract" paragraph, currently at lines 80-82, from "The guard clause at the top of
  this method rejects both fail-fast, so branch 1's substring offset expression is never evaluated
  with a negative start index." to "The guard clause at the top of this method rejects both
  fail-fast, so branch 1's derived offset (Key.IndexOf(other) plus the matched length) is never
  evaluated with a negative start index: IndexOf is non-negative because branch 1 only runs when
  Contains already matched, and other.Length is at least 1 because of the guard above."
  Acceptance: the old sentence no longer appears in the file; the new sentence appears verbatim
  inside the XML doc comment; no other sentence in the doc comment is changed.

### Phase 4 — Green-After-Fix Verification

- [ ] [P4-T1] Build TaskMaster.sln with a plain incremental build (Configuration Debug, Platform
  Any CPU) to compile the P2-T1 fix and the P3-T1/P3-T2 rewords into the QuickFiler.Test build
  output assembly and QuickFiler's own output assembly, then run the resolved vstest executable
  against the QuickFiler.Test build output assembly with InIsolation and a test-case filter
  selecting every test whose fully qualified name contains the KaStringAsyncTests class name.
  Record the passing run in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/regression-testing/green-after-fix.md`.
  Acceptance: an observed exit code of 0; the verdict line reads "Test Run Successful."; 0 Failed
  among every test in that class; the new method
  KeyEquals_ContainsMatchAtNonPrefixIndex_InvokesUpdateWithLastMatchedCharacter and the test named
  for a contains-match while activated that invokes Update and returns true are both individually
  reported Passed — the same passing-run shape ("Test Run Successful.", 0 Failed, exit code 0)
  recorded at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/vstest-baseline.2026-08-22T09-30.md.

### Phase 5 — Final Quality-Assurance Loop

- [ ] [P5-T1] Run the resolved dotnet executable's CSharpier format subcommand from the repository
  root, scoped only to the two files this change edits (QuickFiler/Controllers/KaStringAsync.cs
  and `QuickFiler.Test/Controllers/KaStringAsyncTests.cs`), then take a SHA-256 hash of each file
  immediately before and immediately after the invocation to measure the actual rewrite count
  (distinct from CSharpier's own processed-file count). Record a Timestamp field, a Command field,
  an EXIT_CODE field, and an Output Summary field (stating both the processed count and the
  measured rewrite count) in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/csharpier-format.md`.
  Acceptance: an observed exit code of 0; the summary states a processed count of 2 files, distinct
  from the SHA-256-measured rewrite count, per the same processed-versus-rewritten distinction
  recorded at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/qa-gates/csharpier-format.2026-08-22T09-52.md;
  if the measured rewrite count is greater than 0, P5-T15 restarts this phase from P5-T1.

- [ ] [P5-T2] Run the resolved dotnet executable's CSharpier check subcommand from the repository
  root (read-only, repository-wide) and record a Timestamp field, a Command field, an EXIT_CODE
  field, and an Output Summary field in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/csharpier-check.md`.
  Acceptance: an observed exit code of 0 and a summary reporting 0 files needing formatting (only
  the "Checked N files in T ms." summary line appears, with no per-file line ahead of it), matching
  the recorded repo-wide clean shape ("Checked 1517 files in 6574ms.", 0 needing formatting) at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/qa-gates/csharpier-check.2026-08-22T09-53.md.

- [ ] [P5-T3] Run the solution's analyzer Rebuild (Configuration Debug, Platform Any CPU, the
  EnableNETAnalyzers and EnforceCodeStyleInBuild properties both true) with a detailed file logger,
  count the "Skipping target CoreCompile" and "CoreCompile:" log-line occurrences, and record a
  Timestamp field, a Command field, an EXIT_CODE field, and an Output Summary field in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/msbuild-analyzers.md`.
  Acceptance: an observed exit code of 0, a verdict of "Build succeeded.", a "Skipping target
  CoreCompile" count of exactly 0, a "CoreCompile:" count of at least 9, and a warning count no
  greater than the P0-T6 baseline ceiling, matching the recorded shape at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/msbuild-analyzers.2026-08-22T09-21.md.

- [ ] [P5-T4] Run the solution's nullable Rebuild (Configuration Debug, Platform Any CPU, the
  TreatWarningsAsErrors property true, no nullable opt-in property) with a detailed file logger,
  count the "Skipping target CoreCompile" log-line occurrences, and record a Timestamp field, a
  Command field, an EXIT_CODE field, and an Output Summary field in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/msbuild-nullable.md`.
  Acceptance: an observed exit code of 0, a verdict of "Build succeeded.", and a "Skipping target
  CoreCompile" count of exactly 0, matching the recorded shape at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/msbuild-nullable.2026-08-22T09-23.md.

- [ ] [P5-T5] Run a coverage-instrumented test capture of the whole QuickFiler.Test.dll assembly,
  identical in form to P0-T8, writing Cobertura-format output to
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/coverage-postchange.cobertura.xml`.
  Read the emitted Cobertura XML's root line-rate and branch-rate attributes, and aggregate every
  class element whose filename ends with the KaStringAsync source file name into a post-change
  covered/total count. Record both in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/coverage-postchange.md`.
  Acceptance: an observed exit code of 0; the inner test run reports "Test Run Successful." with 0
  Failed; the Output Summary field carries the numeric line-rate, branch-rate, and the
  KaStringAsync file's covered/total counts (not a placeholder), using the same per-file
  aggregation method recorded at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/qa-gates/coverage-postchange.2026-08-22T10-38.md.

- [ ] [P5-T6] Run the same scoped, non-instrumented vstest filter used at P0-T9 (selecting every
  test whose fully qualified name contains the pinned KbdActions test class name, against the
  QuickFiler.Test build output assembly, with InIsolation and the TaskMaster CLI runsettings file)
  after the fix, and compare its Passed count against the P0-T9 baseline, without ever diffing the
  pinned file. Record a Timestamp field, a Command field, an EXIT_CODE field, and an Output Summary
  field (stating both counts and their delta) in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/kbdactions-postchange.md`.
  Acceptance: an observed exit code of 0, a verdict of "Test Run Successful.", and a Passed count
  identical to the P0-T9 baseline with 0 Failed and a delta of 0 — this is the AC5 evidence,
  obtained solely from the test-run outcome and never from a diff of the pinned file.

- [ ] [P5-T7] Compute the coverage delta between
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/coverage-postchange.cobertura.xml`
  and
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/coverage-baseline.cobertura.xml`:
  (a) the instrumented-run line-rate/branch-rate delta, (b) the KaStringAsync file's per-file
  covered/total before and after, and (c) changed-line coverage, by intersecting the added line
  numbers from a git diff of QuickFiler/Controllers/KaStringAsync.cs anchored to the origin main
  ref, using a zero-context unified diff, against the post-change per-line hit map. Record all
  three, plus a companion git-status porcelain listing (to confirm no untracked file escapes the
  anchored diff's tracked-file scope), in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/coverage-delta.md`.
  Acceptance: the artifact records numeric baseline coverage, numeric post-change coverage, and a
  numeric changed-line coverage percentage; every changed line in
  QuickFiler/Controllers/KaStringAsync.cs shows hits of at least 1 post-change and no line covered
  at baseline is uncovered after, following the methodology recorded at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/qa-gates/coverage-delta.2026-08-22T10-40.md.

- [ ] [P5-T8] Check off AC1 in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/spec.md`'s Acceptance Criteria
  section ("The recorded maintainer decision ... is reflected in the implementation ...") by
  changing its unchecked box to a checked box, citing P2-T1's diff (guard text unchanged, no
  StartsWith introduced) as the verifying evidence.
  Acceptance: only that one checkbox line changes in the file; no criterion text is altered.

- [ ] [P5-T9] Check off AC2 in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/spec.md`'s Acceptance Criteria
  section ("KaStringAsync's KeyEquals branch-one derives its Update argument from the match
  position via Key.IndexOf(other) ...") by changing its unchecked box to a checked box, citing
  P2-T1's diff and P4-T1's green run as the verifying evidence.
  Acceptance: only that one checkbox line changes in the file; no criterion text is altered.

- [ ] [P5-T10] Check off AC3 in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/spec.md`'s Acceptance Criteria
  section ("A regression test in QuickFiler.Test/Controllers/KaStringAsyncTests.cs covers the
  two-digit-width non-prefix case ...") by changing its unchecked box to a checked box, citing
  P1-T1 (test added) and P4-T1 (passes post-fix) as the verifying evidence.
  Acceptance: only that one checkbox line changes in the file; no criterion text is altered.

- [ ] [P5-T11] Check off AC4 in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/spec.md`'s Acceptance Criteria
  section ("The pre-existing prefix-case behavior is preserved ...") by changing its unchecked box
  to a checked box, citing P3-T1's reword (asserted value unchanged) and P4-T1's green run as the
  verifying evidence.
  Acceptance: only that one checkbox line changes in the file; no criterion text is altered.

- [ ] [P5-T12] Check off AC5 in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/spec.md`'s Acceptance Criteria
  section ("The pinned keyboard-matching test in the QuickFiler.Test KbdActionsTests file passes
  unchanged, and that file is not modified ...") by changing its unchecked box to a checked box,
  citing P0-T9 and P5-T6's matching Passed counts (0 delta) as the verifying evidence.
  Acceptance: only that one checkbox line changes in the file; no criterion text is altered.

- [ ] [P5-T13] Check off AC6 in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/spec.md`'s Acceptance Criteria
  section ("The full C# toolchain passes in order ...") by changing its unchecked box to a checked
  box, citing P5-T1 through P5-T7's four gate artifacts as the verifying evidence.
  Acceptance: only that one checkbox line changes in the file; no criterion text is altered.

- [ ] [P5-T14] Verify all six acceptance criteria in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/spec.md` are checked, and
  report the AC Status Summary (Source, Total AC items, Checked off, Remaining, Items remaining)
  per the acceptance-criteria-tracking skill in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/ac-status-summary.md`.
  Acceptance: the artifact reports Total AC items 6, Checked off 6, Remaining 0.

- [ ] [P5-T15] Attest, in
  `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/final-qc-pass-attestation.md`,
  that P5-T1 through P5-T7 each recorded an exit code of 0 (or, for P5-T1, a measured rewrite count
  of 0), by running an anchored name-status git diff against the origin main ref together with a
  companion git-status porcelain listing (per the paired-diff convention: the anchored diff
  enumerates committed tracked changes, the porcelain listing catches anything uncommitted or
  untracked) and confirming every path reported by the anchored name-status diff, which enumerates
  tracked changes only, falls only within: the two production/test files
  QuickFiler/Controllers/KaStringAsync.cs and `QuickFiler.Test/Controllers/KaStringAsyncTests.cs`,
  this plan file's own checklist, spec.md's Acceptance Criteria checkboxes, and the evidence
  subtree under this feature folder. The companion git-status porcelain listing is expected to
  report the evidence artifacts this phase creates, which git reports as one collapsed entry per
  wholly-untracked directory rather than one line per file; that entry lies within the evidence
  subtree named above, and the porcelain listing exists to catch a change the anchored diff
  cannot see rather than to be scope-checked as a tracked change. If any of P5-T1 through P5-T7
  recorded a non-zero exit code (excluding a declared ExpectedExitCode normalization), or
  P5-T1's measured rewrite count was greater than 0, or the anchored diff shows a tracked file
  outside that scope, or the porcelain listing reports an untracked path outside that scope, this
  task instructs restarting Phase 5 from P5-T1, per the general-code-change.md restart rule.
  Acceptance: the artifact states, for each of P5-T1 through P5-T7, its recorded exit code and,
  for P5-T1, its measured rewrite count; lists the paired diff/status output; confirms every
  path reported by the anchored diff falls within the stated scope and that the porcelain
  listing reports no untracked path outside it; and confirms the
  format-then-lint-then-type-check-then-test loop completed as one uninterrupted pass, in the
  same attestation shape recorded at
  docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/qa-gates/final-qc-pass-attestation.2026-08-22T10-42.md.
