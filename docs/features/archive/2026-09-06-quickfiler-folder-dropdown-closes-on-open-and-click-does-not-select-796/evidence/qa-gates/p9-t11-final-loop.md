# P9-T11 — Closing the QA loop for the execution worktree

Timestamp: 2026-09-07T16-12
Task: [P9-T11]
Issue: #796
Channel used: A

## Branch taken

RESTART BRANCH. The loop was restarted at P9-T1 and this artifact records the SECOND
PASS.

The trigger is stated plainly rather than reasoned away: task **P9-T3 attempt 1 FAILED**.
Its acceptance clause compares the build-summary warning total against the P0-T8 baseline
of 0, and attempt 1 reported 3. The three warnings were `MSB3061` file-lock warnings
raised by the `CoreClean` target of TaskMaster/TaskMaster.csproj because a running
Microsoft Outlook process held three native DLLs under TaskMaster/bin/Debug open. That
cause was environmental and outside this item's diff, and it was cleared by closing
Outlook, after which P9-T3 attempt 2 met all four clauses. Both attempts are retained in
evidence/qa-gates/p9-t3-analyzer-rebuild.md.

The environmental character of the failure does not change the branch. This task's
condition is "if any of P9-T1 through P9-T8 failed", and P9-T3 attempt 1 failed, so the
restart branch is the correct one and the single-clean-pass branch is not available. The
second pass below was executed in full, in toolchain order, after the cause was cleared,
so that the recorded clean pass contains no failing step anywhere inside it.

No task in P9-T1 through P9-T8 changed a tracked file; the restart is triggered by the
failure alone.

## The four commands of the final clean pass, in order

### 1. Format

```
pwsh -NoProfile -Command 'dotnet tool run csharpier format .'
```

EXIT_CODE: 0

```
Formatted 1608 files in 3370ms.
```

The repo-wide branch was used, matching the branch P9-T1 recorded, which is correct
because evidence/baseline/p0-t7-csharpier-check-baseline.md carries
`CSHARPIER-BASELINE: CLEAN`.

A write-mode formatter exits 0 whether or not it rewrote anything, so the exit code is
not the evidence. The evidence is the before-and-after tree observation:

Before:

```
 M docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/plan.2026-09-06T21-59.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t10-scope-boundary.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t9-final-commit.md
```

After: identical, path for path.

No path appears only in the after capture, and no `.cs` file carries an `M` status in
either capture. That second point removes the ambiguity P9-T1 had to resolve separately
by diff shape: at P9-T1 one `.cs` file was already `M` before the pass, so porcelain could
not distinguish "unchanged" from "rewritten" for it. Here every tracked `.cs` file was
clean before the pass and every one is clean after it, so an unchanged status code is
conclusive on its own.

Verification, read-only:

```
pwsh -NoProfile -Command 'dotnet tool run csharpier check .'
```

EXIT_CODE: 0

```
Checked 1608 files in 6457ms.
```

The check reported no unformatted file.

### 2. Lint — .NET analyzers

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults\796\p9-t11\analyzer-rebuild.log;Verbosity=detailed"; "EXIT_CODE=$LASTEXITCODE"'
```

RunStartedUtc: 2026-09-07T20:11:13.6702864Z
EXIT_CODE: 0

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:18.82
```

CscTaskCount=36, CscToolCount=36. The gate is not vacuous: `/t:Rebuild` was used, not
`/t:Build`, and thirty-six compiler invocations were recorded in the detailed log. Zero
warnings, so the three `MSB3061` file-lock warnings of P9-T3 attempt 1 are gone and did
not recur.

### 3. Type check — nullable analysis

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=TestResults\796\p9-t11\nullable-rebuild.log;Verbosity=detailed"; "EXIT_CODE=$LASTEXITCODE"'
```

RunStartedUtc: 2026-09-07T20:11:38.4358523Z
EXIT_CODE: 0

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:16.68
```

CscTaskCount=36, CscToolCount=36. The command line carries no `/p:Nullable=enable` token.

### 4. Test

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" /ResultsDirectory:TestResults\796\p9-t11 "/Logger:trx;LogFileName=p9-t11.trx"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

```
Test Run Successful.
Total tests: 1380
     Passed: 1380
 Total time: 14.9534 Seconds
```

1380 of 1380 passed, reproducing the P9-T5 result exactly. No `Failed:` line was printed,
so the Failed set is empty.

The assemblies under test are the ones step 3 produced:
QuickFiler/bin/Debug/QuickFiler.dll at 2026-09-07T20:11:46.4983058Z and
QuickFiler.Test/bin/Debug/QuickFiler.Test.dll at 2026-09-07T20:11:50.2586690Z, both after
that step's RunStartedUtc and both before this run.

## Second-pass result

All four steps passed in a single pass, in order, and none of them changed a tracked
file. The loop therefore terminates here and no third pass is required.

Raw logs and the TRX are at the gitignored paths TestResults/796/p9-t11/ and are not
committed; a TRX embeds the host account name and machine name in its `runUser` and
`computerName` attributes.

## Terminal porcelain reading

Taken immediately before the amend, after this artifact and this task's check-off in the
plan file had both been written. Recorded below after observation, not predicted.

Command, on the recorded command channel A:

```
pwsh -NoProfile -Command 'git status --porcelain --untracked-files=all'
```

Observed output:

```
 M docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/plan.2026-09-06T21-59.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t10-scope-boundary.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t11-final-loop.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t9-final-commit.md
```

Four paths, every one inside the feature folder
docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796:

| Path | Why it is present at this reading |
|---|---|
| plan.2026-09-06T21-59.md | carries the P9-T10 and P9-T11 check-offs, written after the P9-T9 commit |
| evidence/qa-gates/p9-t9-final-commit.md | records a SHA that did not exist until the P9-T9 commit had been made |
| evidence/qa-gates/p9-t10-scope-boundary.md | written after that commit, because it measures it |
| evidence/qa-gates/p9-t11-final-loop.md | this artifact |

The `PRE-EXISTING-DIRTY-SET:` recorded in evidence/baseline/p0-t14-scope-baseline.md is
EMPTY, so this gate admits no exception outside the feature folder and is strict. It
passes because no path lies outside the feature folder: no path under QuickFiler, none
under QuickFiler.Test, and none anywhere else in the tree is modified or untracked.

A terminal gate demanding zero porcelain lines is not used here, and neither is one
permitting only the plan file and this task's own artifact, because all four paths above
are necessarily untracked or modified at this reading. The amend below is what folds them
into the final commit.

## Amend

```
pwsh -NoProfile -Command 'git add QuickFiler QuickFiler.Test docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796'
pwsh -NoProfile -Command 'git commit --amend --no-edit'
```

The staging form is the P9-T9 pathspec form. `--no-edit` keeps the P9-T9 commit message
unchanged, which is why no acceptance clause asserts over the amended message body.

The resulting SHA is deliberately NOT recorded in this artifact. An artifact that is
itself staged by the amend cannot carry the hash of the commit that contains it: writing
the hash changes the file, which changes the tree, which changes the hash. That is a
fixpoint with no solution, not an omission. The amended SHA is instead observable
directly from `git rev-parse HEAD` after the amend, and the P9-T9 artifact records the
pre-amend SHA 676966ef2d36e20f231cfb3949591d44a9adb92d for the audit trail.

For the same reason the amend was applied twice. The first application folded in the
P9-T9, P9-T10 and P9-T11 artifacts and the plan check-offs, exactly as the terminal
porcelain reading above enumerates. It then became apparent that this section of this
artifact still held an unresolved placeholder, so the placeholder was replaced with the
paragraph above and a second `git commit --amend --no-edit` was applied to fold that
correction in. Both applications used `--no-edit`, so the commit message is unchanged by
either, and the branch still carries exactly one commit for this work rather than two.

The terminal porcelain reading recorded above is the one this task's acceptance clause
names: it was taken immediately before the amend, after this artifact and this task's
check-off in the plan file had both been written, and it enumerates the four paths that
amend folds in. The second application carries only the correction to this paragraph.

## Acceptance clause by clause

| Clause | Observed | Met |
|---|---|---|
| the artifact names which of the two branches was taken | RESTART BRANCH, with the P9-T3 attempt 1 failure named as the trigger | yes |
| it records the four commands of the final clean pass in order | format, lint, type-check, test, all recorded above in that order, all EXIT_CODE 0 | yes |
| the porcelain reading is taken immediately before the amend, after this task's own artifact and check-off have been written | artifact written, then plan check-off written, then the reading taken | yes |
| it lists no path other than members of the `PRE-EXISTING-DIRTY-SET:` and paths inside the feature folder | 4 paths, all inside the feature folder; the pre-existing set is empty and needed no exception | yes |

Output Summary: the RESTART branch was taken because P9-T3 attempt 1 failed on three
environmental `MSB3061` file-lock warnings. The second pass ran format, lint, type-check
and test in order and all four returned EXIT_CODE 0 with no tracked file changed: 1608
files formatted and checked clean, two `/t:Rebuild` builds at 0 warnings and 0 errors with
36 Csc invocations each, and 1380 of 1380 tests passing. The terminal porcelain reading
lists four paths, all inside the feature folder, and the amend folds them into the final
commit.

