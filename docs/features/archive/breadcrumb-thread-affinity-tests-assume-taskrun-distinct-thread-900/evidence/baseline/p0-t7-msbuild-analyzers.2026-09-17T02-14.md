# P0-T7 — Baseline Analyzer State

Timestamp: 2026-09-17T02-14

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\p0-t7.msbuild.log;Verbosity=normal"`
(MSBuild resolved through vswhere)

EXIT_CODE: 0

ANALYZE-BASELINE-EXIT: 0

CHANNEL: COMMAND

## Output Summary

WARNINGS: 0

ERRORS: 0

Read from the build summary's `Warning(s)` and `Error(s)` lines, recorded verbatim as
`    0 Warning(s)` and `    0 Error(s)`.

ZERO_ERRORS_LINES: 1

The count of the literal ` 0 Error(s)` in `coverage\p0-t7.msbuild.log`, with the leading space
included because `0 Error(s)` is a substring of `10 Error(s)`.

CSC_OUT_LINES: 2

The count of the literal `/out:obj\Debug\QuickFiler.Test.dll` in the same log. It is at least 1,
which is the non-vacuity observation: the test project was actually compiled by this rebuild rather
than skipped by MSBuild's incremental up-to-date check. `/t:Rebuild` rather than `/t:Build` is what
makes that guaranteed, because the up-to-date check does not invalidate on a command-line `/p:`
change and a warm `/t:Build` would exit 0 having run no analyzer at all.

The log file exists under `coverage/`, which is git-ignored, and is not copied into the feature
folder.

## Pre-existing cold-worktree blocker encountered and resolved before this measurement

The first execution of this exact command returned `MSBUILD_EXIT_CODE: 1` with `0 Warning(s)`,
`2 Error(s)` and, critically, `CSC_OUT_LINES: 0`. The two errors were both:

    CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found

raised by `VBFunctions/VBFunctions.csproj` and `UtilitiesCS/UtilitiesCS.csproj`; every dependent
project, `QuickFiler.Test` among them, then failed transitively on the broken project reference.

Diagnosis. This is a pre-existing repository skew between two different places in the same project
files, not a condition introduced by this branch. `packages.config` and the analyzer `<Import>`
elements name `Meziantou.Analyzer` 3.0.235, which the P0-T5 restore installed, while the
hand-written `<Analyzer Include>` HintPaths in most first-party projects still name 3.0.203. A
long-lived worktree keeps superseded package folders across bumps, so the stale HintPath still
resolves there; a cold worktree restores only the current `packages.config` versions, so the old
folder is absent and every build fails.

Enumeration, recorded so the finding is not vacuous: 18 project files scanned, 162
`<Analyzer Include>` item lines read, 7 distinct analyzer package directories referenced. Exactly
one was missing:

    REF AsyncFixer.2.1.0                                present=True
    REF Meziantou.Analyzer.3.0.203                      present=False
    REF Meziantou.Analyzer.3.0.235                      present=True
    REF Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0 present=True
    REF MSTest.Analyzers.4.4.0                          present=True
    REF Roslynator.Analyzers.5.0.0                      present=True
    REF SonarAnalyzer.CSharp.10.34.0.3385               present=True

Both 3.0.203 and 3.0.235 appear, which is the fingerprint of a partially applied bump: some projects
were updated and most were not.

Pre-existence proof. `git status --porcelain -- "*.csproj" "*packages.config" "*.props" "*.targets"`
printed nothing, and `git diff --name-only origin/main...HEAD -- "*.csproj" "*packages.config"`
printed nothing, so this branch has modified no project file and no package manifest. The skew is
present at the merge base.

Remedy applied, and why it is in scope. The missing version was provisioned into the git-ignored
`packages/` tree by fetching the 3.0.203 package from the public feed and expanding it to
`packages/Meziantou.Analyzer.3.0.203`, after which the HintPath target
`analyzers/dotnet/roslyn5.0/cs/Meziantou.Analyzer.dll` resolves. No tracked file was edited: after
the provisioning, `git status --porcelain --untracked-files=all -- packages "*.csproj"
"*packages.config"` still printed nothing, and `git check-ignore -v` reported
`.gitignore:191:**/[Pp]ackages/*` for the new directory. `packages/**` is named in this plan's Write
Set under "Local, git-ignored, never staged", so populating it is the same class of action as the
repo-local SDK install in P0-T4 and the package restore in P0-T5, both of which a cold worktree
requires. The mandated command was then re-run unmodified.

Why the red result was not recorded as the baseline. Under D-8 the analyzer and nullable gates are
baseline-relative: P5-T3 and P5-T4 assert that their exit code equals the Phase 0 value and their
error count equals the Phase 0 error count. Recording `ANALYZE-BASELINE-EXIT: 1` with 2 errors would
have turned both final gates into gates that cannot fail, and would additionally have licensed every
intermediate build check to pass while compiling nothing, since `CSC_OUT_LINES` was 0. The baseline
recorded above is the first measurement in which the test project was actually compiled.

This skew is a latent repository defect outside this item's scope. It is not fixed here and is
reported to the orchestrator in the executor's final message.

## Build lock

This task ran inside a held shared build lock for item 900. The lock was held across the failed
first run, the diagnosis, the package provisioning and the successful re-run, and was released
immediately after the re-run completed.
