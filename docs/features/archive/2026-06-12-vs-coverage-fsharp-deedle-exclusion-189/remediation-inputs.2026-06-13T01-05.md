# Remediation Inputs — PR #190 CI failure (cycle 1)

- Entry timestamp: 2026-06-13T01-05
- Branch: bug/vscode-test-runner-parity-188 (PR #190)
- Trigger: Required CI check "Format, build, analyze, and test" failed after the PR was opened.
- Failing step: `Verify formatting` running `dotnet csharpier check .`
- Run: https://github.com/drmoisan/TaskMaster/actions/runs/27450707889

## Failure detail (verbatim cause)

`dotnet csharpier check .` reports 8 project files "Was not formatted. The file did not end with a single newline":
`QuickFiler.Test.csproj`, `Tags.Test.csproj`, `TaskMaster.Test.csproj`, `TaskMaster.csproj`, `TaskVisualization.Test.csproj`, `ToDoModel.Test.csproj`, `UtilitiesCS.Test.csproj`, `VBFunctions.Test.csproj`. Checked 1060 files; exit code 1. (These 8 are the complete failing set; preflight empirically confirmed adding the ignore globs returns exit 0.)

## Root cause (verified)

- Pre-existing and repo-wide, NOT introduced by #188/#189/#191. No `.csproj` was modified on this branch.
- CSharpier v1 began inspecting `.csproj` files; the repository's existing project files do not satisfy its trailing-newline rule. `origin/main`'s own `TaskMaster.csproj` ends with `</Project>` and no trailing newline, so `main` fails this gate today.
- This contradicts the repository's documented intent: CLAUDE.md states "csharpier is file-based and formats only `*.cs` without touching project files," and "Do not use `dotnet format` — it ... rewriting `.csproj` files." CSharpier v1 regressed that file-type scope.

## Approved fix (user-approved option 1)

Add project-file globs to `.csharpierignore` so `csharpier check .` no longer inspects them, restoring the documented "C# source only" scope:
- `*.csproj`
- `*.props`
- `*.targets`

Place them under the existing ignore entries with a brief comment explaining the rationale (CSharpier formats C# source only; project files are owned by Visual Studio per CLAUDE.md).

## Constraints

- Edit ONLY `.csharpierignore`. Do not modify any `.csproj`/`.props`/`.targets`, any `.cs`, the workflow YAML, or the #188/#189 change set already on this branch.
- This is not a workflow-file change, so the `modified-workflow-needs-green-run` rule does not apply; however CI must re-run green on the branch head after the change.
- Acceptance: `dotnet csharpier check .` (or the repo-scoped equivalent) passes locally for the in-scope file types, and the `.csharpierignore` additions are present and correctly globbed.

## Out of scope

- Pinning/downgrading the CSharpier version (alternative not selected).
- Adding trailing newlines to project files (alternative not selected; would touch `.csproj`).
- PR #192 (separate branch) — it will need the same ignore entries to pass the same gate; tracked separately.
