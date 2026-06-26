# Baseline — CSharpier (#211 Phase 3.1)

Timestamp: 2026-06-23T18-40
Command (plan-stated): `dotnet tool run csharpier . --check`
Command (executed, version-adapted): `dotnet tool run csharpier check .`
EXIT_CODE: 0

Output Summary:
- `Checked 1091 files in 3903ms.` Formatter reports clean; no unformatted files at baseline.
- Adaptation note: the locally pinned CSharpier (1.x) uses the `check <directoryOrFile>` subcommand; the legacy `. --check` flag form is rejected by this version (`'--check' was not matched. Did you mean one of the following? check`). The executed command `csharpier check .` is the version-correct equivalent of the plan-stated check-mode invocation and performs no file writes. No banned-token or behavior change; this is a tool-invocation adaptation only.
