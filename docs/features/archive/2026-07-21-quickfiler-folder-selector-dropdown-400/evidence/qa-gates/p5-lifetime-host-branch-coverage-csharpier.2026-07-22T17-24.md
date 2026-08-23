# P5-T196 — CSharpier format + scoped check (batch N2)

Timestamp: 2026-07-22T17-24Z

Command: `$file=(Resolve-Path 'QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs').Path; $tool='C:\Users\DanMoisan\.dotnet\tools\csharpier.exe'; & $tool format $file --log-level Information; $first=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; & $tool format $file --log-level Information; $second=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; & $tool check $file --log-level Information; $code=$LASTEXITCODE`

EXIT_CODE: 0

## Output Summary

Mutating `csharpier format` was run on disk against exactly one file,
`QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs`. `csharpier pipe-files` was not used as a
formatting or verification gate. Repeated `format` produced no further change and the authoritative scoped
`csharpier check` reported `Checked 1 files` and exited **0**. Final file SHA-256:
`594d96f2a8f34e6e987d2ad7efeda6fce999152027924d83a15fc22b7f3e63db`.

Post-format physical line count: **480** lines, which satisfies the "at most 480" bound.

Disclosure of in-batch line-budget iterations (no replanning trigger was reached, and no new file was created):
the first two drafts of this batch formatted to 587 and then 526 lines. Both exceeded the 480-line bound, so the
in-scope harness was reworked twice inside the same single approved file — first by delegating queueing to the
existing `BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext` instead of reimplementing a
queue, then by hoisting reflection lookups and removing three assertions that were strictly redundant with a
retained assertion (`opening.IsFaulted`/`opening.IsCanceled`, both implied by the retained
`opening.Status.Should().Be(TaskStatus.RanToCompletion)`, and a `probe.AddedItem.Should().NotBeNull()` implied by
the retained dereference of the same value). No pre-existing case, assertion, or file was touched by that
reduction, and the final artifact is the only state gated here.
