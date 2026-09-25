# P0-T12 — Nullable Build Baseline

Timestamp: 2026-09-19T22-54

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\nullable.msbuild.log;Verbosity=normal"
```

EXIT_CODE: 1

ExpectedExitCode: 1

OUTLOOK-CLOSED: true

## CMD-OUTLOOK precondition

`Get-Process outlook -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count`
returned `0`, measured in the same shell invocation that ran the build and guarded so the build
could not proceed on a non-zero count. No process was terminated.

## First error text, verbatim

Absolute worktree prefixes are replaced by `<repo-root>` per the repository's
no-absolute-host-paths rule; no other character is altered.

```
11>CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [<repo-root>\VBFunctions\VBFunctions.csproj]
```

## Recorded figures

| Measurement | Value |
|---|---|
| Log line count, `coverage/nullable.msbuild.log` | 745 |
| Lines matching `error ` | 4 |
| Distinct failing projects | 2 — `VBFunctions`, `UtilitiesCS` |
| MSBuild summary | `0 Warning(s)`, `2 Error(s)` |
| Lines containing `/out:obj\Debug\` | 8 |

The file logger records each diagnostic twice, once on the live `/m`-prefixed event and once in the
end-of-log recapitulation, so 4 matching lines correspond to the 2 errors MSBuild's own summary
reports.

## Acceptance evaluation

- The artifact exists and carries every schema field: `Timestamp:`, `Command:`, `EXIT_CODE:`,
  `ExpectedExitCode:`, `OUTLOOK-CLOSED:`, `Output Summary:`. PASS.
- The `Output Summary:` names the first error text verbatim. PASS.

No exit-0 demand is placed on this task. The baseline is red for the same cause as P0-T11 — defect
#898 leaves 15 `*.csproj` files naming an analyzer assembly no manifest declares and no restore
produces — and the nullable gate never reaches a `CS86xx` diagnostic because compilation aborts at
analyzer reference resolution. The `/out:` count of 8 is recorded as the same red-state observation
noted at P0-T11: most projects never reach `CoreCompile`. The first green nullable run is P2-T6,
which asserts exit 0 and at least 18 `/out:obj\Debug\` lines.

Output Summary: CMD-MSBUILD-NULLABLE returned EXIT_CODE 1 against the expected 1, with Outlook
confirmed closed at 0 processes. The first error text is
`11>CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [<repo-root>\VBFunctions\VBFunctions.csproj]`.
MSBuild reported 0 warnings and 2 errors across `VBFunctions` and `UtilitiesCS`; no nullable
`CS86xx` diagnostic was reached because compilation aborts at analyzer reference resolution.
