# QA Gate — CI Re-Run Requirement and Remediation Closure Note

Timestamp: 2026-06-13T01-05

## CI Re-Run Expectation

(a) After pushing the `.csharpierignore` edit to the branch head, the required CI check
    "Format, build, analyze, and test" MUST re-run green on PR #190
    (`bug/vscode-test-runner-parity-188`). The failing step was `Verify formatting`
    running `dotnet csharpier check .`; this edit makes that step pass locally
    (verified: before EXIT_CODE 1 with 8 `.csproj` failures, after EXIT_CODE 0).

(b) The `modified-workflow-needs-green-run` rule does NOT apply: no workflow YAML
    (`.github/workflows/*.yml`/`*.yaml`) was changed. Only `.csharpierignore` was modified.

(c) The C# analyzer, nullable/type-check, and test/coverage gates are N/A for this
    ignore-file change. An ignore-file edit changes no compiled source, so:
    - Analyzer/build gate (`msbuild ... /p:EnableNETAnalyzers=true`): N/A — no `.cs`/build inputs changed.
    - Nullable/type-check gate (`msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`): N/A — no `.cs` changed.
    - Test/coverage gate (`vstest.console.exe ... /EnableCodeCoverage`): N/A — no production or test code changed; coverage cannot regress.

## Closure

The local empirical gate (`dotnet csharpier check .`) passes (exit 0). The commit/push
is handled by the orchestrator, after which CI must re-run green on the branch head.
