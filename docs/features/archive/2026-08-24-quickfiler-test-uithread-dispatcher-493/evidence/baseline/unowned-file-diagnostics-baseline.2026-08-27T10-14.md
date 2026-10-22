# Unowned-File Diagnostics Baseline (P0-T10)

Timestamp: 2026-08-27T10-14
Task: [P0-T10]
Command: For each of `TestResults/plan-logs/p0-t8/msbuild-analyzers.log` and `TestResults/plan-logs/p0-t9/msbuild-nullable.log`, extract every line containing the simple string `QfcItemController.FocusAndThemeTests.cs` and every line containing the simple string `UiThread.cs`, then apply the § Conventions redaction filter to each matched line.
EXIT_CODE: 0
Output Summary: Four matched lines per log, two per token. **Every matched line is a compiler
invocation line, not a diagnostic**: the `csc.exe` command line emitted by `CoreCompile` and the
companion `BuildResponseFile = '...'` line. The diagnostic-bearing subset of the match set is
**empty in both logs for both tokens**. A zero diagnostic count is a legitimate recorded value and is
the value recorded here.

AnalyzerStepMatchCount: 4
NullableStepMatchCount: 4

Those two integers are the total match counts for the two tokens combined in each log. The
per-token breakdown is given below because `P4-T2` compares per token and per log.

| Log | Token | Match count | Diagnostic-bearing matches |
| --- | --- | --- | --- |
| `p0-t8/msbuild-analyzers.log` | `QfcItemController.FocusAndThemeTests.cs` | 2 | 0 |
| `p0-t8/msbuild-analyzers.log` | `UiThread.cs` | 2 | 0 |
| `p0-t9/msbuild-nullable.log` | `QfcItemController.FocusAndThemeTests.cs` | 2 | 0 |
| `p0-t9/msbuild-nullable.log` | `UiThread.cs` | 2 | 0 |

**A zero count is a legitimate recorded value.** It is stated explicitly here as the task requires:
the diagnostic-bearing match count is zero in every one of the four token-and-log combinations, and
that zero is the recorded baseline rather than a missing measurement.

## Matched lines — analyzer step (`TestResults/plan-logs/p0-t8/msbuild-analyzers.log`)

### Token `QfcItemController.FocusAndThemeTests.cs`

| Log line | Redacted length (chars) | SHA-256 of redacted line | Diagnostic |
| --- | --- | --- | --- |
| 3322 | 33240 | `5e9bcfaf9a2dbe939b5de86d59b2e818c61abf98e6ffab2a120735e041794923` | no |
| 3325 | 33163 | `feae55559f707ab32c10b006a641986383febb91bc633e4bb5d172a4171df901` | no |

Redacted head of line 3322 (first 200 characters):

```
         C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Roslyn\csc.exe /noconfig /nowarn:1701,1702 /fullpaths /nostdlib+ /errorreport:prompt /warn:5 /define:DEBUG;TRACE /hig
```

Redacted head of line 3325 (first 200 characters):

```
         BuildResponseFile = '/nowarn:1701,1702 /fullpaths /nostdlib+ /errorreport:prompt /warn:5 /define:DEBUG;TRACE /highentropyva+ /reference:<repo-root>\packages\Azure.Core.1.62.0\lib\net472\Azure
```

Both lines belong to the `QuickFiler.Test` `CoreCompile` invocation (`8>CoreCompile:` at log line
3320). The token appears inside each line as one source-file argument among the project's full
`<Compile Include>` set.

### Token `UiThread.cs`

| Log line | Redacted length (chars) | SHA-256 of redacted line | Diagnostic |
| --- | --- | --- | --- |
| 430 | 56052 | `897a69626ed94b1f9a4f48dcecaa35ebece77e508b404cd013d2223d8f598cd4` | no |
| 433 | 55975 | `5177d946258328a9fb3ae8d2b1a236e99e86066a135753f7fae90209fc350b5f` | no |

Redacted head of line 430 (first 200 characters):

```
         C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Roslyn\csc.exe /noconfig /unsafe- /nowarn:1701,1702 /fullpaths /nostdlib+ /errorreport:prompt /warn:4 /define:DEBUG;T
```

Redacted head of line 433 (first 200 characters):

```
         BuildResponseFile = '/unsafe- /nowarn:1701,1702 /fullpaths /nostdlib+ /errorreport:prompt /warn:4 /define:DEBUG;TRACE /highentropyva+ /reference:<repo-root>\packages\AngleSharp.1.7.1\lib\net4
```

Both lines belong to the `UtilitiesCS` `CoreCompile` invocation. `UtilitiesCS/Threading/UiThread.cs`
appears as one source-file argument among that project's full source set.

## Matched lines — type-check step (`TestResults/plan-logs/p0-t9/msbuild-nullable.log`)

### Token `QfcItemController.FocusAndThemeTests.cs`

| Log line | Redacted length (chars) | SHA-256 of redacted line | Diagnostic |
| --- | --- | --- | --- |
| 10533 | 33254 | `b2501bc1592c1717c3206bd9743b047fdeed8d5245c26e8ae68ca1c54cd45a58` | no |
| 10536 | 33177 | `8cdc01b82cc4d95d6bddf312703114e23428878030db13972a3a19deb8e9a217` | no |

### Token `UiThread.cs`

| Log line | Redacted length (chars) | SHA-256 of redacted line | Diagnostic |
| --- | --- | --- | --- |
| 7338 | 56066 | `166e4ace653d4a19d6638723485c631ec863dfcc4a46a65e9b2ea6a6a712cc8b` | no |
| 7341 | 55989 | `b1173f7203e898f9d51b53d8f3390f7a093fc1c28c4754333b504a676380bd52` | no |

The heads of these four lines are byte-identical to the analyzer-step heads quoted above for the
first 200 characters. The full lines differ from the analyzer-step lines by 14 characters each,
which is the `/p:TreatWarningsAsErrors=true` step contributing `/warnaserror+` in place of the
analyzer step's `/analyzerconfig`-related difference; the two steps are compared only against their
own counterparts, never across steps.

## Why the "verbatim" listing is delegated to a git-ignored extract file

The task requires every matched line to be listed verbatim in redacted form. The four matched lines
in each log total roughly 178 KB of `csc.exe` command line per log, because each line enumerates the
project's entire `/reference:` set and entire source-file set. Embedding roughly 356 KB of compiler
command line into a committed Markdown artifact would contradict the plan's own § Conventions clause
that "Evidence artifacts quote only redacted excerpts", and it would add no reviewable signal: the
whole content is a reference list.

The full redacted lines are therefore written byte-for-byte to the git-ignored extract files below,
and this artifact records, for each matched line, its log line number, its redacted character
length, and the SHA-256 of its redacted text. The SHA-256 values make the `P4-T2` set comparison
exact and reproducible without the artifact carrying the bytes.

| Extract file (git-ignored) | Contents |
| --- | --- |
| `TestResults/plan-logs/p0-t10/analyzer-step.QfcItemController_FocusAndThemeTests_cs.extract.txt` | the 2 redacted analyzer-step lines for that token |
| `TestResults/plan-logs/p0-t10/analyzer-step.UiThread_cs.extract.txt` | the 2 redacted analyzer-step lines for that token |
| `TestResults/plan-logs/p0-t10/nullable-step.QfcItemController_FocusAndThemeTests_cs.extract.txt` | the 2 redacted type-check-step lines for that token |
| `TestResults/plan-logs/p0-t10/nullable-step.UiThread_cs.extract.txt` | the 2 redacted type-check-step lines for that token |

## Recorded hazard for `P4-T2` — the raw line set is expected to change

This is recorded now, at the point the baseline is established, so that `P4-T2` is read against a
disclosed expectation rather than treated as an anomaly.

Every matched line is a `csc.exe` command line or its response-file echo, and each such line
enumerates the compiling project's complete source-file set. `P1-T2` adds two `<Compile Include>`
entries to `QuickFiler.Test/QuickFiler.Test.csproj`. The post-change `QuickFiler.Test` `csc.exe`
command line therefore necessarily differs from the baseline line by the two added source-file
arguments, and its SHA-256 will differ.

Consequently:

- The **match count** condition in `P4-T2` is expected to hold: the count stays 2 per token per log,
  because adding source files to an existing command line does not add a line.
- The **byte-exact set equality** condition in `P4-T2` is expected to fail for the
  `QfcItemController.FocusAndThemeTests.cs` token in both logs, for a reason that is not a
  diagnostic regression: the token's containing line is the compiler invocation, and the invocation
  legitimately grows by the two files this feature adds.
- The `UiThread.cs` token's lines belong to `UtilitiesCS`, which this feature does not change, so
  their set equality is expected to hold byte-for-byte and is a real gate on AC-7.
- The **diagnostic-bearing subset** of the match set is the quantity AC-6's final sentence ("No
  analyzer diagnostic is raised at either call site under toolchain steps 2 and 3") is actually
  about. That subset is empty at baseline, so a post-change diagnostic-bearing count of zero
  discharges AC-6's diagnostic clause **absolutely**, which is the stronger of the two cases the
  plan's § Notes rule 2 anticipates.

`P4-T2` will record all three results — count equality, byte-exact set equality with its symmetric
difference, and diagnostic-subset equality — and will state plainly which held.
