# Baseline — CSharpier (#211 Phase 3.2)

Timestamp: 2026-06-23T22-30
Command (plan-stated): `dotnet tool run csharpier . --check`
Command (executed, version-adapted): `dotnet tool run csharpier check .`
EXIT_CODE: 0

Output Summary:
- `Checked 1093 files in 2548ms.` Formatter reports clean; no unformatted files at baseline.
- Adaptation note: the locally pinned CSharpier (1.x) uses the `check <directoryOrFile>` subcommand; the legacy `. --check` flag form is rejected by this version. The executed `csharpier check .` is the version-correct equivalent and performs no file writes.
