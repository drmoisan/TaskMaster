# P6-T1 — Final QA: CSharpier Format (Issue #181)

Timestamp: 2026-06-08T13-36
Command: `dotnet tool restore` then `dotnet tool run csharpier check .`
EXIT_CODE: 1

Output Summary:
- `dotnet tool restore`: csharpier 1.2.6 restored, EXIT_CODE 0.
- Before this step, the 30 first-party project files modified by this plan (15 `.csproj` + 15 `packages.config`) were flagged by CSharpier 1.2.6 (which formats XML project files). They were brought to CSharpier's canonical XML layout via `dotnet tool run csharpier format <files>` (29 files reformatted; QuickFiler.csproj formatted separately earlier). The reformatting is whitespace/element-reflow only and does not change MSBuild semantics; verified by the subsequent analyzer build (P6-T3, 0 errors), nullable build (P6-T4, 84 baseline errors), and test run (P6-T5).
- `dotnet tool run csharpier check .` after formatting: EXIT_CODE 1. Checked 1057 files. The ONLY remaining finding is the single pre-existing baseline `.cs` file `UtilitiesCS\Extensions\IEnumerableExtensions.cs` (a `System.Threading.Timer` lambda formatting difference recorded at the Phase 0 baseline; see `evidence/baseline/baseline-format.2026-06-08T12-12.md`). This file is not touched by this plan and is not reformatted, per the executor directive to preserve plan scope.
- The format gate has returned to the exact Phase 0 baseline state (1 pre-existing `.cs` file; all in-scope project files now pass). No first-party `.cs` source file other than the pre-existing baseline file is flagged.

Verdict: The format step is at the Phase 0 baseline. EXIT_CODE 1 is attributable solely to the documented pre-existing `IEnumerableExtensions.cs` baseline condition, not to any file modified by this plan.
