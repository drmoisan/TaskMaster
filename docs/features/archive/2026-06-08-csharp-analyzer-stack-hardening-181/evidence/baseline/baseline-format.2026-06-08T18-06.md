# Baseline Formatting Check — Fail-Before (Issue #181, Cycle 2)

Timestamp: 2026-06-08T18-06

Command: dotnet tool restore && dotnet tool run csharpier check .
EXIT_CODE: 1

Output Summary:
- CSharpier version: 1.2.6 (restored via `dotnet tool restore`).
- Checked 1057 files in 3586ms.
- Exactly ONE file reported as unformatted:
  `.\UtilitiesCS\Extensions\IEnumerableExtensions.cs - Was not formatted.`
- Location: Around Line 132 — a `System.Threading.Timer` lambda argument.
  - Expected (CSharpier output): `_ => progress.Report(completed, $"Consuming ...")`
    on a single line within the Timer constructor call.
  - Actual (current file): the lambda body wraps `progress.Report(` across lines.
- This is the pre-fix failing-gate proof for the formatting acceptance criterion.
  Pairs with the pass-after evidence in P1-T2 / P2-T1.
