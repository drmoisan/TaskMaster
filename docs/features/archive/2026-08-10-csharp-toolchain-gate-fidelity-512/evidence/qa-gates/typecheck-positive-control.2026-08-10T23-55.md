# TYPECHECK positive control ([P5-T4], AC3)

Timestamp: 2026-08-10T23-55
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo /v:m /fl "/flp:logfile=coverage/qa-typecheck-positive.log;verbosity=normal"`
EXIT_CODE: 0

`MSBUILD` = `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe`,
invoked via `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-typecheck-rebuild.ps1 -LogName qa-typecheck-positive`.

Run on the **unperturbed** tree, before the [P5-T5] negative control. This is step 1 of the three-run
negative-path proof design in `spec.md` § "Negative-path proof design (AC4)".

## Measurements

| Metric | Value | Acceptance |
|---|---|---|
| `EXIT_CODE` | **0** | required 0 — PASS |
| MSBuild summary | **`0 Error(s)`** | required `0 Error(s)` — PASS |
| `Skipping target "CoreCompile"` count | **0** | required 0 — PASS |
| Elapsed | **15.0 s** | recorded |
| Node-prefixed `error CS` count | 0 | corroborates `0 Error(s)` |
| MSBuild summary | `6 Warning(s)` | not gated |
| `CoreCompile:` header-line count | 61 | informational only, not the assertion |

**The gate is passable.** A non-zero exit here would have invalidated the central design assumption
and required a stop-and-report; it did not occur, and no workaround was applied.

## AC2 counting-mechanism deviation (restated)

The non-vacuity assertion is a **zero** count of `Skipping target "CoreCompile"` in the `/fl` log,
substituted for AC2's `csc.exe` parenthetical, which measures zero at `verbosity=normal` even for
genuine compiles. `CoreCompile:` header lines are not counted; they print even when the target is
skipped. Recorded in `spec.md` § "The non-vacuity assertion mechanism".

## Corroboration

- Pre-change positive control on the same command:
  `FEATURE/evidence/baseline/baseline-typecheck-rebuild.2026-08-10T22-59.md` (`EXIT_CODE: 0`,
  0 skips, `0 Error(s)`).
- Independent corroboration on `main`'s tip: the CI step "Build with nullable warnings treated as
  errors" **succeeded**
  (`FEATURE/evidence/baseline/baseline-ci-parity-on-main.2026-08-10T15-05.md`).

## Output Summary

The corrected type-check command returns `EXIT_CODE: 0` with MSBuild reporting `0 Error(s)` and
**zero** `Skipping target "CoreCompile"` occurrences against the unperturbed post-edit tree, in
15.0 s. AC3 is satisfied: the documented type-check gate is passable, and it passes by compiling, not
by skipping.
