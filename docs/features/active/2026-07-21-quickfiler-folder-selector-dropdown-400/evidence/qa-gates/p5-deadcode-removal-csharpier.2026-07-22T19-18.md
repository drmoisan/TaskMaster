# P5-T204 — CSharpier format + scoped check (dead-code removal batch)

Timestamp: 2026-07-22T19-18Z

Command: `$tool='C:\Users\DanMoisan\.dotnet\tools\csharpier.exe'; $f1=(Resolve-Path 'QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs').Path; $f2=(Resolve-Path 'QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs').Path; & $tool format $f1 $f2 --log-level Information; & $tool format $f1 $f2 --log-level Information; & $tool check $f1 $f2 --log-level Information; $code=$LASTEXITCODE`

EXIT_CODE: 0

## Files actually edited by P5-T203

- `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` (production: inner `try`/`catch` removed)
- `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` (comment-only doc update)

## Result

- Mutating `csharpier format` was run on disk against exactly the two edited files. `csharpier pipe-files`
  was NOT used as a formatting or verification gate.
- Repeated `format` produced no further change (both file hashes stable across two consecutive runs),
  and the authoritative scoped `csharpier check` reported `Checked 2 files` and exited **0**.
- Final SHA-256:
  - `BreadcrumbDropDownOpenLifetime.cs` = `b8f4d6e27dc0fee69f2a659c7830f212bb0410f3283c1db8800bd005a9756cb0`
  - `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` = `88b42147682e935a01cf24bd7cb842410295234b26b3c0c6d0d09e247cdb0c23`

## Post-format physical line counts

- `BreadcrumbDropDownOpenLifetime.cs`: **425** lines (at most 500 — satisfied; down from 437 because
  the four removed dead-code lines plus the surrounding inner `try`/`catch` braces were removed).
- `BreadcrumbPopupBoundaryCoverageTests.Part2.cs`: **480** lines (at most 480 — satisfied). The
  authorized comment-only doc update was kept to two physical lines so the file remains at its prior
  480-line bound.

## Output Summary

`csharpier format` (mutating, on-disk) followed by scoped `csharpier check` exited 0 for both edited
files. Repeated format produced no change. `BreadcrumbDropDownOpenLifetime.cs` is 425 lines (<=500) and
`BreadcrumbPopupBoundaryCoverageTests.Part2.cs` is 480 lines (<=480). No `csharpier pipe-files` gate was
used.
