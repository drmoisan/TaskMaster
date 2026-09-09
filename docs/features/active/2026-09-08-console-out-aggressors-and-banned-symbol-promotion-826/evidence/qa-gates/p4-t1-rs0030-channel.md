# RS0030 observation channel certification (issue #826, [P4-T1])

Timestamp: 2026-09-09T19-21

CHANNEL: SARIF

Exactly one `CHANNEL:` value is recorded. Channel 1 in the plan's D7 order was tried first and its
control fired, so channels 2 and 3 were not attempted. No severity value was mutated anywhere by this
task.

## Exact commands

Run as `pwsh -NoProfile -Command` blocks carrying the plan's C2 preamble branch guard. A warm solution
build ran first so that `/p:BuildProjectReferences=false` is valid, then each relevant project was built
separately with its own SARIF path, because a command-line `/p:ErrorLog=` is a global property and a
`/m` solution build would have projects overwrite one another's log.

```
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
msbuild UtilitiesCS\UtilitiesCS.csproj      /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/p:ErrorLog=<repo-root>\coverage\826-raw\utilitiescs.sarif"
msbuild QuickFiler\QuickFiler.csproj        /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/p:ErrorLog=<repo-root>\coverage\826-raw\quickfiler.sarif"
msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/p:ErrorLog=<repo-root>\coverage\826-raw\quickfilertest.sarif"
```

EXIT_CODE: 0 for all four invocations (warm solution build 0, `UtilitiesCS` 0, `QuickFiler` 0,
`QuickFiler.Test` 0).

## SARIF version and location shape

Every one of the three documents reports `version` = **1.0.0**. The v1 location shape therefore applies:
a result's own file is read from `locations[0].resultFile.uri` and its line from
`locations[0].resultFile.region.startLine`. The v2 shape `locations[0].physicalLocation.artifactLocation.uri`
is absent from these documents; the extraction probes for both and selects by which is non-null, so it
would work unchanged if a future toolchain emitted v2.

## Control counts — RS0030-scoped, not file-name-scoped

Each count below is the number of **RS0030 results whose own result location is that file**, obtained by
filtering `runs[0].results` on `ruleId -eq "RS0030"` and then matching the extracted URI. It is not a
count of textual occurrences of the file name. That distinction is load-bearing: with
`EnableNETAnalyzers` and `EnforceCodeStyleInBuild` set, each control file also carries unrelated IDE
diagnostics whose result locations name it, so a file-name search would report at least 1 whether or not
the channel carries RS0030 and could certify a void channel.

| Control site (from D7) | SARIF document | RS0030-scoped count | Required |
|---|---|---|---|
| `UtilitiesCS/Threading/ApplicationIdleTimer.cs` | `utilitiescs.sarif` | 9 | at least 1 |
| `QuickFiler/Controllers/EfcHomeControllerDependencies.cs` | `quickfiler.sarif` | 1 | at least 1 |
| `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs` | `quickfilertest.sarif` | 1 | at least 1 |

All three are at least 1, so the channel is certified: it demonstrably carries info-severity RS0030
diagnostics for already-banned symbols with known live usages.

The `ApplicationIdleTimer.cs` count of 9 exceeds the three `DateTime.Now` reads D7 names (lines 60, 140
and 236). The extracted result lines for that file are 60, 140, 141, 209, 236, 240, 292, 309 and 330.
The surplus is other already-banned symbols in the same file and is recorded rather than asserted; the
gate only requires the count to be non-zero.

## Textual `RS0030` counts, recorded and not diagnostic counts

| SARIF document | `-SimpleMatch "RS0030"` count | RS0030 result count |
|---|---|---|
| `utilitiescs.sarif` | 39 | 37 |
| `quickfiler.sarif` | 7 | 5 |
| `quickfilertest.sarif` | 5 | 3 |

Each textual count exceeds its result count by exactly 2, which is the rule-metadata block a Roslyn error
log carries for every rule that could report. This is why the textual figures are recorded only: a
Roslyn error log can carry rule metadata for rules that produced no result at all, so a non-zero textual
count is not evidence of a diagnostic.

## Per-project scoping check

Every RS0030 result location in each document lies under that document's own project directory:

- `utilitiescs.sarif` — all 37 result locations are under `UtilitiesCS/`.
- `quickfiler.sarif` — all 5 result locations are under `QuickFiler/`.
- `quickfilertest.sarif` — all 3 result locations are under `QuickFiler.Test/`.

No document was overwritten by a referenced project, so no per-project build had to be re-run.

Output Summary: `CHANNEL: SARIF` certified at D7's first channel. All three RS0030-scoped control counts
are at least 1 (9, 1 and 1), each SARIF is correctly scoped to its own project, and every document
reports SARIF version 1.0.0 with the `resultFile.uri` location shape. The raw SARIF documents stay under
the gitignored `coverage/826-raw/` directory and are not committed, because they embed absolute host
paths.
