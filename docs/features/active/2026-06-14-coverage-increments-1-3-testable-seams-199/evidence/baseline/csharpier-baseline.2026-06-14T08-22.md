# Csharpier Baseline

Timestamp: 2026-06-14T08-22

Command: csharpier check .
Note: The plan-prescribed `dotnet tool run csharpier .` cannot run in this environment because
the repo-local .NET SDK (pinned by global.json to `.dotnet-sdk`) is not installed. The globally
installed CSharpier 1.3.0 (`csharpier check .`) is the file-based functional equivalent and is
used throughout this feature's QA loop. This is a tooling-invocation substitution only; the same
formatter and version are applied.

EXIT_CODE: 0

Output Summary: Checked 1040 files in ~1.6s. No files would be reformatted (exit 0). Repository
is clean against CSharpier 1.3.0 before any test additions.
