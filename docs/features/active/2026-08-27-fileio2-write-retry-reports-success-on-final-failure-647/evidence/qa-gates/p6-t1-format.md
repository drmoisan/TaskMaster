# P6-T1 — Final Repository-Wide Format

Timestamp: 2026-08-31T20-17
Command: dotnet tool run csharpier format .
EXIT_CODE: 0
Iteration: 1

The `format` command exits 0 whether or not it rewrote a file, so its exit code observes nothing and is not the gate. The gate for the format step is the read-only check in P6-T2.

## Supporting evidence: ten SHA-256 hashes over the five footprint files

| Path | Before | After | Rewritten |
|---|---|---|---|
| `UtilitiesCS/To Depricate/FileIO2.cs` | CC16BEA463D2E545A113F30FCCDB763AF58CBB82BC3935602F0EBB618A54F0BA | CC16BEA463D2E545A113F30FCCDB763AF58CBB82BC3935602F0EBB618A54F0BA | False |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 4512823A565979DC980FF8FC02FC41C887870B237EA29B641B79B4B91596A05A | 4512823A565979DC980FF8FC02FC41C887870B237EA29B641B79B4B91596A05A | False |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 71B6A20028D3E1FAAA6502A141E4FC67CCCC3957400EC95AE7422CBF7ED607B8 | 71B6A20028D3E1FAAA6502A141E4FC67CCCC3957400EC95AE7422CBF7ED607B8 | False |
| `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` | CF136E1C3A9D5375C3D4D2BC02E11E6582682A5A9EC6AEDDC50D0B0F5DE229E8 | CF136E1C3A9D5375C3D4D2BC02E11E6582682A5A9EC6AEDDC50D0B0F5DE229E8 | False |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | 4B645C3E86A5F8BB01D7CCA0C9968B1E7B556CA2D3465CD2D0F49ECE3B337461 | 4B645C3E86A5F8BB01D7CCA0C9968B1E7B556CA2D3465CD2D0F49ECE3B337461 | False |

REWRITTEN_FILE_COUNT: 0

This is the number of files whose `Get-FileHash -Algorithm SHA256` value differs between the capture taken immediately before the invocation and the capture taken immediately after. It is supporting evidence only. All five footprint hashes are unchanged, because the per-file formats in P4-T7 and P5-T8 already left them in CSharpier's canonical form.

The console line printed by this invocation reads `Formatted 1565 files in 4726ms.` That is the count of files **processed** across the whole repository, not the count rewritten, and it must not be recorded as the rewrite count. The two figures diverge sharply here: 1565 processed against a measured 0 rewritten among the footprint files.

## Footprint consequence

`evidence/baseline/p0-t12-csharpier-check.md` records `PRE_EXISTING_FORMAT_DRIFT: none`, so this repository-wide format had no pre-existing drift to repair and therefore could not widen the change footprint beyond the five files. Nothing is carried into P7-T19 as an authorized formatter-drift exception.

## Post-format line counts of the five footprint files

| Path | Lines | Limit | Within |
|---|---|---|---|
| `UtilitiesCS/To Depricate/FileIO2.cs` | 293 | 500 | Yes |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 227 | 500 | Yes |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 494 | 500 | Yes |
| `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` | 335 | 500 | Yes |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | 454 | 500 | Yes |

Every one of the five counts is at most 500. This re-audit after the final repository-wide format is what keeps the P4-T11 and P5-T9 audits from being stale.

Output Summary: Ten hashes, a rewritten-file count of 0, iteration 1, and five post-format line counts all within the 500-line limit.
