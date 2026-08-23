# P5-T5 — Type-Check Gate (CI's command)

Timestamp: 2026-08-08T21-36

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; & '<MSBUILD>' TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug /p:Platform='Any CPU' /p:TreatWarningsAsErrors=true"
```

`/nodeReuse:false` is the only addition to the plan's stated command; it suppresses MSBuild's
persistent worker processes, the cause diagnosed and removed after the aborted attempt 1. All
gate-bearing switches are exactly as the plan specifies and the measured result is identical to
the run without it.

EXIT_CODE: **0**

Output Summary:

- **Errors: 0**
- **Warnings: 6** — identical to the P0-T8 merge-base baseline and to the P5-T4 analyzer run:
  2 x `CS2002` (the pre-existing `UtilitiesCS.Test` duplicate `<Compile Include>`) and 4 untagged
  `System.Reactive.PackagesConfigCheck.targets(31,5)` `packages.config` advisories. Neither is
  promoted to an error by `/p:TreatWarningsAsErrors=true`, matching merge-base behavior exactly.
- Elapsed: 00:00:16.53.
- Target is `/t:Rebuild`, so `CoreCompile` ran for every project — the pass is not vacuous. The
  identical `/t:Rebuild` target at P5-T4 recorded 18 `csc.exe` invocations and zero
  `Skipping target "CoreCompile"` occurrences.
- Delta versus the P0-T8 merge-base baseline: **zero new errors, zero new warnings**. No new type
  or nullable-flow diagnostic is introduced by this change.

## Deliberate deviation from the `CLAUDE.md` type-check command (issue #522)

This is **CI's actual type-check command** as defined in `.github/workflows/ci.yml`. The
`CLAUDE.md` variant additionally passes `/p:Nullable=enable`. That variant is known-defective and
tracked as **issue #522**: nullable reference types are per-file opt-in in this solution (via
`#nullable enable` pragmas), and forcing the flag solution-wide reports 200-414 errors that are
red on `main` regardless of any change. CI deliberately omits the flag.

`/p:Nullable=enable` is therefore **deliberately omitted**, per plan rule 7 and the
`## Verification` section of `<FEATURE>\spec.md`, which is the in-folder authority for this
deviation. A reviewer encountering the missing flag should read this note and the #522 citation as
the authority, not as non-compliance. Issue #522 is not fixed by this delivery.

Neither new file carries a `#nullable enable` pragma, consistent with every other file under
`TaskMaster\Ribbon\`; null contracts are documented in XML doc comments, and null-forgiving
operators appear only where the #503 seam files already model them.

Binary outcome: **PASS**.
