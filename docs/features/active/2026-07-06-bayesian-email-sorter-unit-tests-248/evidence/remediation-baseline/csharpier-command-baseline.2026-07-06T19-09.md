Timestamp: 2026-07-06T19:16:32-04:00
Command: formatter invocation baseline from recorded evidence
EXIT_CODE: 0

Input Evidence:
- dotnet-tools.json
- docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharpier-final.2026-07-06T18-07.md
- docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharpier-final.2026-07-06T18-07.attempt1.md

Manifest Finding:
- Planned manifest path `.config/dotnet-tools.json` is absent in this checkout.
- Actual manifest path: `dotnet-tools.json`.
- Pinned CSharpier version: 1.2.6.
- Manifest command entry: `csharpier`.

Recorded Formatter Commands:
- Command: `dotnet tool run csharpier .`
- EXIT_CODE: 1
- Diagnostic: CSharpier 1.2.6 rejected the direct directory argument because the local CLI requires an explicit subcommand.
- Command: `dotnet tool run csharpier format .`
- EXIT_CODE: 0
- Output: Formatted 1275 files in the recorded final QA pass.

Output Summary:
- Accepted formatter invocation for this checkout: `dotnet tool run csharpier format .`.
- The accepted invocation preserves formatting enforcement and completed with exit code 0 in existing final QA evidence.
- The policy-listed command `dotnet tool run csharpier .` remains incompatible with the pinned local CSharpier CLI and is a policy-command mismatch requiring policy-owner follow-up.
