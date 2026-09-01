# AC9 Verification (P2-T20)

Timestamp: 2026-09-01T16-57

Sources: the six final-QC artifacts under
`docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/qa-gates/`.

EXIT_CODE: 0

Output Summary:

## The six records

| Artifact | Timestamp | EXIT_CODE | Timestamp field | Command field | Output Summary field |
|---|---|---|---|---|---|
| `csharpier-format.md` | 2026-09-01T15-59 | 0 | present | present | present |
| `csharpier-check.md` | 2026-09-01T16-00 | 0 | present | present | present |
| `msbuild-analyzers.md` | 2026-09-01T16-01 | 0 | present | present | present |
| `msbuild-nullable.md` | 2026-09-01T16-02 | 0 | present | present | present |
| `vstest-quickfiler-postchange.md` | 2026-09-01T16-24 | 0 | present | present | present |
| `vstest-utilitiescs-postchange.md` | 2026-09-01T16-35 | 0 | present | present | present |

All six exist, all carry `Timestamp:`, `Command:`, `EXIT_CODE:` and
`Output Summary:`, and all record `EXIT_CODE: 0`.

## All six carry timestamps from the same final loop pass

The Phase 2 loop ran two passes. Pass 1 ended when `dotnet tool run csharpier
format .` rewrote `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs`,
which is the CSharpier wrap P1-T6 predicted and which the loop rule treats as a
restart rather than a failure. Pass 2 began at 15-59 with a format run whose
before-and-after tree observation showed an empty set difference.

Every one of the six timestamps above falls at or after 15-59 and in the stated
toolchain order, so all six belong to the final pass. The loop is bounded at
three passes; two were used and the second was clean, so the
`loop-termination.md` BLOCKED path was not reached and no
`REMEDIATION-REQUIRED` arises.

## The order AC9 requires

AC9 requires the toolchain to pass in one clean pass in the order format,
analyze, type-check, test, using the exact commands in CLAUDE.md. The six
records above run in that order:

1. **Format** — `dotnet tool run csharpier format .`, verified read-only by
   `dotnet tool run csharpier check .`.
2. **Analyze** — `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug
   "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
3. **Type-check** — `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug
   "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`.
4. **Test** — `vstest.console.exe <assembly> /EnableCodeCoverage /InIsolation`,
   once per owned assembly.

`/p:Nullable=enable` was not added and `/t:Build` was not substituted for
`/t:Rebuild`; CLAUDE.md records why each is load-bearing. `/InIsolation` was
supplied on every `vstest.console.exe` invocation, per Decisions Record D10.

The two test artifacts are required because AC9 names four toolchain steps
ending in test and requires an evidence artifact for each; both owned assemblies
therefore carry one.

## The format artifact records more than an exit code

AC9's final sentence requires the format step's artifact to record the CSharpier
summary line printed on a no-change run, not the exit code alone.
`csharpier-format.md` records the summary line `Formatted 1566 files in 2071ms.`
verbatim, and additionally the before-and-after `git status --porcelain --
QuickFiler UtilitiesCS QuickFiler.Test` listings with their set difference stated
explicitly as EMPTY. That tree observation is what distinguishes a rewriting run
from a non-rewriting one, because the subcommand rewrites in place and still
exits 0 after rewriting, so the exit code is identical in both cases.

## Verdict

AC9 is satisfied: the full C# toolchain passed in one clean pass, in the required
order, with an evidence artifact per step carrying every required field.

**AC9 checked off in `issue.md`.**
