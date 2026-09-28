# Phase 0 — Analyzer Baseline

Timestamp: 2026-09-13T23-12

Build lock: ACQUIRED 879 at 2026-09-13T23:11:29, RELEASED by 879 at 2026-09-13T23:12:06.

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

resolved through `vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\amd64\MSBuild.exe"`,
with all streams redirected to
`evidence/baseline/analyzer-baseline-console.2026-09-13T18-22.txt`.

EXIT_CODE: 0

ExpectedExitCode: 0

Output Summary:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:23.65
```

Baseline error count: 0. Baseline warning count: 0.

Occurrences of `Skipping target "CoreCompile"` in the console log: 0. The gate was therefore
not vacuous; every project compiled.

Console log length after sanitisation: 5030 lines.

Path sanitisation: the raw MSBuild console output carries the absolute worktree path and the
absolute user-profile path on most lines. Both were replaced with the placeholders
`<repo-root>` and `<user-profile>` before the log was retained, matching the convention
already used by committed console logs in this repository. Residual occurrences of the
account name in the retained log: 0. Occurrences of `<repo-root>`: 4153.

## Environment Bootstrap Required Before This Baseline Could Be Taken

The first attempt at this command, taken immediately after `[P0-T4]`'s restore, exited 1 with
`Build FAILED` and this summary:

```
    0 Warning(s)
    2 Error(s)
```

Both errors were the same diagnostic, raised against `VBFunctions/VBFunctions.csproj` and
`UtilitiesCS/UtilitiesCS.csproj`:

```
CSC : error CS0006: Metadata file
'..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll'
could not be found
```

Cause, established by reading tracked source rather than inferred:

- All sixteen `packages.config` files declare `Meziantou.Analyzer` at version `3.0.235`.
- All fifteen first-party `.csproj` files still carry an `<Analyzer Include>` item whose
  HintPath names version `3.0.203`.
- `msbuild /t:Restore /p:RestorePackagesConfig=true` materialises exactly the versions
  `packages.config` declares, so it produced `packages/Meziantou.Analyzer.3.0.235` and no
  `3.0.203` folder. The HintPath then resolved to nothing.

The skew is present at `origin/main` itself (`b63eaa4630d13da46f7ece130bedade53ac39e22`),
verified with `git grep -c` against that commit for both the `3.0.203` HintPath and the
`3.0.235` `packages.config` entry. It is not introduced by this branch and not introduced by
the merge of `origin/main` into it.

Why it is not visible elsewhere:

- CI is green on that exact commit. `.github/workflows/_build-analyzers.yml` restores
  `packages` through `actions/cache` with a bare-prefix fallback key
  (`restore-keys: nuget-${{ runner.os }}-`), so a cache entry populated under an earlier
  `packages.config` hash supplies an orphaned `Meziantou.Analyzer.3.0.203` folder, and
  `nuget restore` then adds `3.0.235` alongside it. Both folders exist on the runner, so the
  stale HintPath resolves. The workflow's inline comment argues that a fallback cache hit can
  only contribute inert orphaned version folders; that argument does not hold in this
  direction, because here the orphaned folder is the one the build actually consumes.
- The primary checkout `repos/TaskMaster` still carries
  `packages/Meziantou.Analyzer.3.0.203` from a pre-bump restore, so warm local checkouts
  build. A worktree restored from scratch does not.

Bootstrap action taken, and its boundary:

```
nuget install Meziantou.Analyzer -Version 3.0.203 -OutputDirectory packages -NonInteractive
```

This writes only into `packages/`, which `.gitignore` line 191 excludes via
`**/[Pp]ackages/*` (confirmed with `git check-ignore -v`). No tracked file was modified. The
action reproduces the package set CI actually builds against rather than altering the
repository, and it is the same class of action as `[P0-T4]`'s SDK install and package
restore. After it, a sweep of all twelve distinct `<Analyzer Include>` HintPaths across the
first-party projects reported `MISSING_COUNT=0`, and the command above was re-run to produce
the baseline recorded at the top of this artifact.

Standing defect, reported and not repaired here: fifteen tracked `.csproj` files carry an
`<Analyzer Include>` HintPath for `Meziantou.Analyzer.3.0.203` that no `packages.config`
declares. Thirteen of those files are outside this plan's `## Authorised Write Set`, and the
two that are inside it are admitted for one `Compile Include` addition only, so the repair is
out of scope for this item and is reported to the caller instead.
