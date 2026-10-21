# P5-T3 S and M Format, Using-Block Check, Production Rebuild and Diff Observations

Timestamp: 2026-10-03T12-20
Command: CMD-SCOPED-FORMAT (PATHS-S, PATHS-M; TASKID p5-t3); CMD-USINGS; CMD-BUILD-PROD (PROJ UtilitiesCS\UtilitiesCS.csproj, DLL UtilitiesCS.dll, TASKID p5-t3): msbuild UtilitiesCS\UtilitiesCS.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true, resolved through vswhere, plus /nodeReuse:false, plus a normal-verbosity file logger; git diff --numstat MERGE-BASE -- UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs; git diff --name-status MERGE-BASE HEAD -- UtilitiesCS/EmailIntelligence/EmailParsingSorting; git status --porcelain -- UtilitiesCS/EmailIntelligence/EmailParsingSorting (MERGE-BASE 94287369908cc920b21b0e3256314f988ad7d2f5, recorded by P0-T3)
EXIT_CODE: 0 (scoped to the CMD-BUILD-PROD invocation, the printed MSBUILD_EXIT_CODE)
Output Summary: The scoped format made no change to S or M (BEFORE and AFTER hashes equal; FORMAT_EXIT_CODE 0, CHECK_EXIT_CODE 0). All five surviving partials carry exactly their specified using blocks (USINGS-EXACT-FILES: 5). The UtilitiesCS rebuild with warnings as errors is green, with no CS0246, CS0103, CS0104 or CS1061 lines, so no removed directive was needed. The numstat shows nine deletions and no addition in each of S and M (usings only). The committed name-status lists M for the five surviving partials and D for the legacy partial. The porcelain for the directory is empty, because the formatter changed nothing.

## CMD-SCOPED-FORMAT output

```
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = D08A06D2921F987BB6D0FCFDDA6CED5DAF88801C56A30E7D780234A8B71A2161
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs = 61FED1959AFE0C8D322ADDC88405E347E21CE44F038018E578D1FEBB7CB388AB
Formatted 2 files in 1971ms.
FORMAT_EXIT_CODE: 0
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = D08A06D2921F987BB6D0FCFDDA6CED5DAF88801C56A30E7D780234A8B71A2161
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs = 61FED1959AFE0C8D322ADDC88405E347E21CE44F038018E578D1FEBB7CB388AB
Checked 2 files in 774ms.
CHECK_EXIT_CODE: 0
```

## CMD-USINGS output

```
USINGS A count=8 exact=True firstline=True blankafter=True
USINGS T count=5 exact=True firstline=True blankafter=True
USINGS U count=10 exact=True firstline=True blankafter=True
USINGS S count=9 exact=True firstline=True blankafter=True
USINGS M count=9 exact=True firstline=True blankafter=True
USINGS-EXACT-FILES: 5
```

## CMD-BUILD-PROD labelled output

```
    0 Warning(s)
    0 Error(s)
MSBUILD_EXIT_CODE: 0
PROD_CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 1
ERRORS: 0
CS0246_LINES: 0
CS0103_LINES: 0
CS0104_LINES: 0
CS1061_LINES: 0
```

## git diff --numstat MERGE-BASE (working-tree form)

```
0	9	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs
0	9	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs
```

## git diff --name-status MERGE-BASE HEAD (committed form)

```
M	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs
D	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs
M	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs
M	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs
M	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs
M	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs
```

## git status --porcelain (before this task's commit)

```
(empty)
```

## Acceptance (P5-T3, all five required)

1. `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`: met.
2. `USINGS-EXACT-FILES: 5` with every `USINGS` row `exact=True firstline=True blankafter=True`: met.
3. `MSBUILD_EXIT_CODE: 0`, `PROD_CSC_OUT_LINES:` 2 (at least 1), `ERRORS: 0`, and `CS0246_LINES`, `CS0103_LINES`, `CS0104_LINES`, `CS1061_LINES` each 0: met.
4. The two numstat lines read `0`, `9` and the respective path: met.
5. The name-status output is exactly six rows (`M` for the five surviving partials, `D` for SortEmail.LegacyAttachmentSaving.cs), and the porcelain lists nothing under the directory (the formatter did not change either file's hash): met.
