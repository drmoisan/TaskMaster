# P0-T8 — Type-Check Build Baseline (CI's command)

Timestamp: 2026-08-08T20-46

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; & '<MSBUILD>' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform='Any CPU' /p:TreatWarningsAsErrors=true"
```

EXIT_CODE: 0

Output Summary:

- **Errors: 0**
- **Warnings: 6** — the same set recorded in the P0-T7 analyzer baseline: 2 x `CS2002`
  (pre-existing duplicate `<Compile Include>` for
  `UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs`) and 4 untagged
  `System.Reactive.PackagesConfigCheck.targets(31,5)` advisories. Neither is promoted to an error
  by `/p:TreatWarningsAsErrors=true`: `CS2002` is emitted by MSBuild's item resolution rather than
  as a csc warning subject to the switch, and the System.Reactive advisory carries no diagnostic
  ID.
- Elapsed: 00:00:14.40
- Target is `/t:Rebuild`, so `CoreCompile` ran for every project and the pass is not vacuous
  (see the P0-T7 artifact for the corroborating 18-invocation `csc.exe` count from the identical
  `/t:Rebuild` target).

## Deliberate deviation from the `CLAUDE.md` type-check command (issue #522)

This command is **CI's actual type-check command** as defined in
`.github/workflows/ci.yml`. The `CLAUDE.md` variant additionally passes `/p:Nullable=enable`.
That variant is known-defective and tracked as **issue #522**: nullable reference types are
per-file opt-in in this solution (via `#nullable enable` pragmas), and forcing the flag
solution-wide reports 200-414 errors that are red on `main` regardless of any change. CI
deliberately omits the flag.

`/p:Nullable=enable` is therefore **deliberately omitted** here and at P5-T5, per plan rule 7 and
the `## Verification` section of `<FEATURE>\spec.md`, which is the in-folder authority for this
deviation. Issue #522 is not fixed by this delivery.

Binary outcome: PASS — expected merge-base value EXIT 0, observed EXIT 0.
