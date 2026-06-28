# Phase 2 — QA Gate: Format (CSharpier) (Issue #219)

Timestamp: 2026-06-28T19-58

Command:
dotnet tool run csharpier format UtilitiesCS.Test/HelperClasses/QfcTipsDetails_Tests.cs
followed by:
dotnet tool run csharpier check UtilitiesCS.Test/HelperClasses/QfcTipsDetails_Tests.cs

(CSharpier 1.x uses the `format`/`check` subcommands; bare `csharpier .` is rejected by this
version.)

EXIT_CODE: 0

Output Summary:
- First `format` pass: "Formatted 1 files in 424ms." The new single-line assertion
  `details.Should().NotBeNull("...")` was wrapped by CSharpier onto three lines (cosmetic).
  Because formatting changed a file, the QA loop was restarted from P2-T1.
- Re-run `check`: "Checked 1 files in 362ms." EXIT_CODE 0 with no further changes, confirming
  the file is now CSharpier-stable. The subsequent analyzer/nullable/test steps were run
  against this stable formatted file.
