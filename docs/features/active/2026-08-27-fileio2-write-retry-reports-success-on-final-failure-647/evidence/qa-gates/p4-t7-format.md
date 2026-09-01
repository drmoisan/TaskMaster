# P4-T7 — Format the Five Footprint Files

Timestamp: 2026-08-31T19-50
Command: dotnet tool run csharpier format <path>, invoked once per footprint path
Command: dotnet tool run csharpier check .
EXIT_CODE: 0
ExpectedExitCode: 0

The recorded `EXIT_CODE:` is that of the read-only `check` command. Each of the five `format` invocations also exited 0, but a `format` exit code observes nothing: it is 0 whether or not the file was rewritten.

## Supporting evidence: ten SHA-256 hashes

| Path | Before | After | Rewritten |
|---|---|---|---|
| `UtilitiesCS/To Depricate/FileIO2.cs` | CC16BEA463D2E545A113F30FCCDB763AF58CBB82BC3935602F0EBB618A54F0BA | CC16BEA463D2E545A113F30FCCDB763AF58CBB82BC3935602F0EBB618A54F0BA | False |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 4512823A565979DC980FF8FC02FC41C887870B237EA29B641B79B4B91596A05A | 4512823A565979DC980FF8FC02FC41C887870B237EA29B641B79B4B91596A05A | False |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | D1D844B1B75F9926BC27642599D30CFEC35441EF9A8FB4B81425CEF43B126BEA | 71B6A20028D3E1FAAA6502A141E4FC67CCCC3957400EC95AE7422CBF7ED607B8 | True |
| `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` | D4C29F2B07E10EFA247EF7B81D13A2B8CD90C9FF20561E340AB4AA4DC838DCE5 | D4C29F2B07E10EFA247EF7B81D13A2B8CD90C9FF20561E340AB4AA4DC838DCE5 | False |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | 4B645C3E86A5F8BB01D7CCA0C9968B1E7B556CA2D3465CD2D0F49ECE3B337461 | 4B645C3E86A5F8BB01D7CCA0C9968B1E7B556CA2D3465CD2D0F49ECE3B337461 | False |

REWRITTEN_FILE_COUNT: 1

The rewritten-file count is the number of files whose `Get-FileHash -Algorithm SHA256` value differs between the capture taken immediately before the invocation and the capture taken immediately after. Exactly one of the five differs.

Each `format` invocation printed a console line of the form `Formatted 1 files in <n>ms.` That figure is the count of files **processed** by that invocation, not rewritten, and is deliberately not recorded as the rewritten count. The two figures diverge here: the console lines sum to 5 processed while the measured rewritten count is 1. This is the case the plan's execution rule anticipates, and it is why the hash comparison rather than the console line is the recorded evidence.

## Loop restart recorded

This task ran twice. Its first invocation reported a rewritten-file count of 0, with all five hashes unchanged, and `check` exit 0. The P4-T8 analyzer build that followed then failed with `EXIT_CODE: 1` and one error:

```
TaskMaster\AppGlobals\AppOlObjects.cs(324,28): error CS0104: 'Exception' is an ambiguous reference between 'Microsoft.Office.Interop.Outlook.Exception' and 'System.Exception'
```

`TaskMaster/AppGlobals/AppOlObjects.cs` carries `using Microsoft.Office.Interop.Outlook;`, and that namespace declares its own type named `Exception`, so the `catch (Exception ex)` clause P4-T5 added was ambiguous against `System.Exception`. The remediation was a file-scoped using alias, `using Exception = System.Exception;`, which resolves the ambiguity while leaving the single-line token `catch (Exception ex)` intact. Qualifying the clause as `catch (System.Exception ex)` was rejected because it would destroy that token and make the P4-T5 and P7-T15 acceptance conditions unsatisfiable. The alias follows the existing repository precedent at `UtilitiesCS.Test/OutlookObjects/Table/OlToDoTable_Tests.cs` line 7, which is the only other file in the tree that resolves this ambiguity.

Line 324 was the only unqualified `Exception` in the file, verified before the alias was added; every other occurrence is either `COMException`, `InvalidOperationException`, or text inside a comment or XML documentation, so the alias changes the meaning of no other construct.

Because that remediation edited a tracked source file, the toolchain loop was restarted from the formatting step, as the General Code Change Policy requires. The hashes and counts recorded in the table above are those of the **second, accepted** invocation, in which the formatter rewrote `TaskMaster/AppGlobals/AppOlObjects.cs` to normalize the newly added using directive. The P4-T8 analyzer build was then re-run against that formatted tree and exited 0.

## Read-only verification

`dotnet tool run csharpier check .` transcribed final summary line:

```
Checked 1565 files in 4406ms.
```

CHECK_EXIT_CODE: 0.

CARRIED_BASELINE_FORMAT_DRIFT: not applicable. `evidence/baseline/p0-t12-csharpier-check.md` records `PRE_EXISTING_FORMAT_DRIFT: none`, so no carried-drift branch is available to this task and a `check` exit code of 0 is the only outcome that satisfies its acceptance. It is the observed outcome.

## Post-format line counts

- `UtilitiesCS/To Depricate/FileIO2.cs` = 293
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs` = 227
- `TaskMaster/AppGlobals/AppOlObjects.cs` = 494
- `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` = 203
- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` = 454

Output Summary: Ten hashes recorded, rewritten-file count 0, and the read-only repository-wide check exited 0.
