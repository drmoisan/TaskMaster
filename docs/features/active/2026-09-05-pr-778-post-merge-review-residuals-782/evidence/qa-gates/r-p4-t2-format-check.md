# [P4-T2] Final QC step 2 — CSharpier check

Timestamp: 2026-09-06T01-49

Command:

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet tool run csharpier check .
```

EXIT_CODE: 0

Output Summary: the read-only check passed with no reformatting required. The printed line, verbatim:

```text
Checked 1583 files in 4444ms.
```

FINAL-CSHARPIER-CHECKED-FILES: 1583

## Comparison against the [P0-T7] baseline

| Run | `Checked` numeral |
|---|---|
| [P0-T7] baseline | 1583 |
| [P4-T2] this run | 1583 |

The two numerals are equal, so the tracked file set CSharpier processes is unchanged by this
remediation. That is the expected result: the remediation edits two existing `.cs` files and creates
no new one. No explanation is required, and none is offered.

The elapsed-milliseconds figure differs between the two runs and is not asserted against; it carries
no acceptance meaning.

## Why the check is run after the format

`check` is read-only and returns a non-zero exit code on drift, so unlike `format` its exit code
alone distinguishes a passing run from a failing one. It is the CI-parity form: the repository's
format-check workflow runs the manifest-pinned CSharpier in exactly this mode.
