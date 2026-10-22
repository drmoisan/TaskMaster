# P5-T7 — Toolchain Single-Clean-Pass Declaration (remediation cycle 1, issue #614)

Timestamp: 2026-08-26T22-40

Command: this task records no new command; it declares the observed sequencing of P5-T1 through
P5-T4 and re-states the four exact commands executed there.

EXIT_CODE: 0

Output Summary: the four toolchain steps completed in one uninterrupted clean sequence with no file
rewritten between them. Restart count: 1, triggered at step 1 before any later step had run.

## The four commands, verbatim

1. `dotnet tool run csharpier format .`
   then `dotnet tool run csharpier check .`
2. `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .`

## Exit codes

| Step | Command | EXIT_CODE | Expectation | Verdict |
| ---: | --- | ---: | --- | --- |
| 1 | `csharpier check .` | 0 | 0 | PASS |
| 2 | analyzer `/t:Rebuild` | 0 | 0 | PASS |
| 3 | nullable `/t:Rebuild` | 0 | 0 | PASS |
| 4 | `Invoke-MSTestWithCoverage.ps1` | 0 | 0 (declared by omission; run fully green) | PASS |

## Restart count and sequencing

**Restart count: 1.**

- Attempt 1 executed step 1 only. `dotnet tool run csharpier format .` rewrote two files —
  `QuickFiler/Controllers/EfcSelectionGuard.cs` and
  `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`, determined by SHA-256 comparison before
  and after, not by reading CSharpier's processed-file count. Under the plan's restart rule the loop
  restarted from P5-T1 at that point. Steps 2, 3 and 4 had not yet been executed on attempt 1.
- Attempt 2 executed steps 1, 2, 3 and 4 in order. The step-1 `format` invocation rewrote **zero**
  files (all three touched-file hashes identical to the end of attempt 1), and
  `dotnet tool run csharpier check .` exited 0.

**No file was rewritten between steps 2, 3 and 4 of attempt 2.** The only writes between the four
steps of the clean pass were to the gitignored `coverage/` tree (the Cobertura artifact and its
copies) and to evidence Markdown under `<FEATURE>/evidence/`, neither of which is compiled or
formatted. The P5-T6 audit, run after step 4, confirms the tracked modified-path set is unchanged
from the P4-T2 record.

## Command-form invariants

- `/t:Rebuild` was used for BOTH MSBuild gates (steps 2 and 3). `/t:Build` was not substituted in
  either. Compilation was verified to have genuinely occurred in both: 36 `csc.exe` invocations and
  18 assembly outputs in each log, so neither gate was a vacuous incremental up-to-date skip.
- `/p:Nullable=enable` was NOT added to step 3, nor to any command in this cycle. Verified: no
  invocation anywhere in this cycle contains that property.
- CSharpier was invoked through `dotnet tool run` in both attempts, so the `dotnet-tools.json`
  manifest pin (1.2.6) was used rather than any global install.
- Step 4 ran in coverage mode and produced numeric line and branch figures, recorded in
  `final-test-coverage.2026-08-26T22-30.md` and compared against the baseline in
  `coverage-delta.2026-08-26T22-34.md`.
- No step in this plan recorded `EXIT_CODE: SKIPPED`.
