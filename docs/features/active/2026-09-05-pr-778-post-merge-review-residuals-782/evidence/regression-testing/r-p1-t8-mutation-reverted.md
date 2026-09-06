# [P1-T8] The falsification mutation is reverted

Timestamp: 2026-09-06T01-40

Command:

```powershell
git checkout -- UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs
git status --porcelain -- UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs
git diff --name-only HEAD -- UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs
Select-String -SimpleMatch 'before yielding folder tree work' -Path 'UtilitiesCS\OutlookObjects\Folder\WpfDispatcherYield.cs'
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

EXIT_CODE: 0

That is the msbuild exit code. All four required observations are recorded below.

Output Summary: the mutated path is clean by every one of the three git and search checks, and the
analyzer build passes with the same figures as the [P0-T8] baseline.

```text
PORCELAIN_LINES=0
DIFF_NAME_ONLY_LINES=0
TAIL_MATCHES=0
MSBUILD_EXIT_CODE=0
```

### 1. `git status --porcelain -- UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`

No output. Zero lines. The path is neither modified nor staged.

### 2. `git diff --name-only HEAD -- UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`

No output. Zero lines. The path is byte-identical to its content at `HEAD`.

### 3. `Select-String -SimpleMatch 'before yielding folder tree work'` over that file

Zero matching lines. The appended tail is gone from the source.

### 4. The analyzer build

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Why three git and search checks rather than one

They fail in different states and no one of them is sufficient. The porcelain status reports a
modified path but goes empty once a change is committed. The anchored `git diff --name-only HEAD`
compares content against the committed tree and is the check that would catch a revert that restored
the path's modification time without restoring its bytes. The literal search is independent of git
entirely and would catch a revert that git considered complete while the tail survived somewhere else
in the file. All three report clean.

## Consequence for the delivered result

No production `.cs` file is changed by this remediation. The only two files it changes are
`UtilitiesCS.Test/Threading/UiThread_Tests.cs` and
`UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`, both test files. [P4-T6] and
[P5-T5] re-verify that enumeration before and after the commit respectively.
