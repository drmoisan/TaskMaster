# P5-T8 — Format Gate for the Two Changed Test Files

Timestamp: 2026-08-31T20-10
Command: dotnet tool run csharpier format "UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs"
Command: dotnet tool run csharpier format "QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs"
Command: dotnet tool run csharpier check .
EXIT_CODE: 0
ExpectedExitCode: 0

P5-T8 runs two independently non-zero-capable gates and `ExpectedExitCode:` is a per-file field, so its evidence is split across two artifacts. **This artifact records the format gate only.** The test gate is recorded in `evidence/qa-gates/p5-t8-scoped-tests.md`, which is the artifact every later task in this plan reads when it refers to "the P5-T8 artifact"; no task reads this one.

The recorded `EXIT_CODE:` is that of the read-only `check` command, which is the governing observation for the format step.

## Expectation selection

`ExpectedExitCode:` is 0. The rule this task states selects 1 only when the run reports at least one unformatted path and every such path is enumerated on the P0-T12 `PRE_EXISTING_FORMAT_DRIFT:` list. This run reported no unformatted path, and `evidence/baseline/p0-t12-csharpier-check.md` records `PRE_EXISTING_FORMAT_DRIFT: none` in any case, so the expectation is 0 by the rule's "and of 0 otherwise" clause.

## Result

Rewritten-file count: 1 of the 2 paths, measured as the number whose `Get-FileHash -Algorithm SHA256` value differs between a capture taken immediately before the invocation and one taken immediately after.

- `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` — rewritten. The formatter collapsed the hand-written two-line `expectedContent` concatenation onto a single line. Post-format line count 335.
- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` — not rewritten. Post-format line count 454.

`dotnet tool run csharpier check .` transcribed final summary line:

```
Checked 1565 files in 4619ms.
```

CHECK_EXIT_CODE: 0. The repository is formatter-clean.

CARRIED_BASELINE_FORMAT_DRIFT: not applicable. P0-T12 recorded no drift, so no carried-drift branch is available and a `check` exit code of 0 is the only outcome that satisfies this gate. It is the observed outcome.

Token survival after the reflow was re-verified: the assertion-ordering invariant still holds, with `midWriteFactoryCalls.Should().Be(1);` on line 62 and `midWriteDelayCalls.Should().Be(0);` on line 63, and `transientContent.Should().Be(expectedContent);` remains present as a single-line token.

Output Summary: Both changed test files are formatter-clean and the read-only repository-wide check exited 0.
