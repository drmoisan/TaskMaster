# P5-T1 — Repository-wide CSharpier format, final QC loop

Timestamp: 2026-09-13T16-48
Command: dotnet tool run csharpier format .
EXIT_CODE: 0
LoopPass: 1

The command was run while this item held the shared cross-item build lock, which was acquired
immediately before it and released immediately after it returned.

## Porcelain status immediately before the command, verbatim

```
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/coverage-baseline.2026-09-12T10-25.cobertura.xml
```

## CMD-FORMAT output, verbatim

```
Formatted 1632 files in 5007ms.
FORMAT-EXIT: 0
```

## Porcelain status immediately after the command, verbatim

```
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/coverage-baseline.2026-09-12T10-25.cobertura.xml
```

## Whether this pass rewrote anything

The two captures are byte-identical. The single line present in both names the raw Cobertura baseline
document produced by P0-T12, which is not a C# source file and which CSharpier neither inspects nor
rewrites; it was already untracked before the command ran and is untracked after it. No tracked file
moved from clean to modified across the command, so this repository-wide pass rewrote no file. That is
what makes this pass eligible to be the completed loop pass P5-T8 records: a pass in which the formatter
rewrote a file would not be.

The distinction the acceptance condition asks for is therefore recorded as a measured difference of the
two captures rather than as an inference from the exit code. CSharpier exits 0 both when it rewrites a
file and when it does not, so the exit code alone cannot carry this observation.

## Set difference between the two captures

PathsInAfterCaptureNotInBeforeCapture: NONE

The after-capture contains no path that the before-capture does not. The scope-lock clause of this
task's acceptance condition is therefore satisfied vacuously: there is no newly appearing path to test
against the Scope-lock rule, so no path outside the Write Set was rewritten by this repository-wide pass
and no path falls into the category the condition designates a scope-lock failure.

## FormatterRepairedPreExistingDrift

FormatterRepairedPreExistingDrift: NONE

This list is derived from the P0-T8 record and not from what this particular pass rewrote, exactly as
the acceptance condition directs. P0-T8 recorded `PreExistingDriftFiles: NONE` and
`DriftInsideWriteSet: NONE` against the post-merge tree at the re-derived anchor
8213826f695439e86e3ed34faa575de493a11ec7. Because P0-T8 recorded `NONE`, the derivation rule yields the
single token `NONE` here, and it yields it regardless of which pass of the loop finally survives.

This value is carried forward into P6-T6, which reproduces it and checks it against the P0-T8
`PreExistingDriftFiles:` line, and into P7-T22, where the `NONE` reading is the condition under which
AC22 may be checked off rather than escalated.

## Inspected-file count reconciliation

P0-T8 reported that CMD-CHECK inspected 1627 files at the anchor. This command reports 1632 formatted
files. The difference of five is exactly the five C# files this item created and added to the two
project manifests: `QuickFiler/Controllers/QfcQueue.Tlp.cs`,
`QuickFiler/Controllers/QfcQueue.UiIdle.cs`, `QuickFiler/Interfaces/IUiIdleDispatcher.cs`,
`QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` and
`QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs`. CSharpier is file-based and discovers
sources by walking the directory tree rather than by reading a project manifest, so the count rises with
the files on disk. No file outside the Write Set was added or removed.

Output Summary: The repository-wide CSharpier format exited 0 after processing 1632 files. The porcelain
captures taken immediately before and immediately after the command are byte-identical, so this pass
rewrote no file and is eligible as the completed loop pass. No path appears in the after-capture that is
absent from the before-capture, so the scope lock holds with no path to adjudicate.
`FormatterRepairedPreExistingDrift: NONE`, derived from the P0-T8 record whose
`PreExistingDriftFiles:` line reads `NONE`. The inspected-file count rose from 1627 to 1632, accounted
for exactly by the five C# files this item created. Acceptance met.
