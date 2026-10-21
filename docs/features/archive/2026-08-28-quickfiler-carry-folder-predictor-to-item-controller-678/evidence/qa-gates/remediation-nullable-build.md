# P2-T4 — Nullable / type-check build, remediation cycle 1

Timestamp: 2026-09-02T01-33

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

EXIT_CODE: 0

## Output Summary

**No `CS86` diagnostic was introduced relative to the P0-T7 enumeration.** The P0-T7 baseline
enumeration was empty (zero `CS86` diagnostics), and this run also reports **0**: the literal
`CS86` occurs zero times in the 11957-line build log. The set of introduced diagnostics is
therefore the empty set.

MSBuild summary lines:

```
    5 Warning(s)
    0 Error(s)
```

The five warnings are the same pre-existing System.Reactive `packages.config` migration
notice recorded at P0-T6 and P2-T3. They are emitted by an MSBuild target rather than by the
C# compiler, so `/p:TreatWarningsAsErrors=true` does not promote them and the build exits 0.

`CoreCompile:` occurrences: **72**.

## Acceptance clauses

| # | Clause | Result |
|---|---|---|
| 1 | `EXIT_CODE: 0` | PASS |
| 2 | no `CS86` diagnostic introduced relative to the P0-T7 enumeration | PASS — 0 at baseline, 0 now |
| 3 | `CoreCompile:` occurrences recorded and greater than zero | PASS — **72** |

`/p:Nullable=enable` is deliberately absent: no project carries a `<Nullable>` element and
there is no `Directory.Build.props`, so adding it would conscript every file that never
adopted the per-file `#nullable enable` pragma. `/t:Rebuild` rather than `/t:Build` is what
makes clause 3 meaningful.

The two production files this cycle edited that carry nullable-relevant changes are
`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`, whose new `ResolveCarrier` returns
`QfcPreScoredItem?`, and `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, whose
guard changed from `string.IsNullOrEmpty(archiveRootPath)` to `archiveRootPath is null`.
Neither introduced a `CS86xx` diagnostic.
