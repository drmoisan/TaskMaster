# Remediation Plan — PR #190 CI csharpier `.csproj` failure

- Plan timestamp: 2026-06-13T01-05
- Feature folder: `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/`
- Branch: `bug/vscode-test-runner-parity-188` (PR #190)
- Source of fix: `remediation-inputs.2026-06-13T01-05.md` (user-approved option 1)
- Mode: CI-failure remediation, atomic and minimal (single-file edit)

## Scope Lock (non-negotiable)

- Edit ONLY `.csharpierignore` (repo root). Add project-file globs `*.csproj`, `*.props`, `*.targets` with a rationale comment, placed alongside the existing ignore globs.
- Do NOT modify any `.csproj`/`.props`/`.targets`, any `.cs`, the workflow YAML, or the #188/#189 change set on this branch.
- Root cause is settled and must not be re-litigated: CSharpier v1 inspects `.csproj` files; the repo's existing project files lack a trailing newline, so `csharpier check .` fails repo-wide (it fails on `main` too). The fix restores the documented "C# source only" CSharpier scope.

## Toolchain Applicability (explicit)

Only the CSharpier format/verify gate applies to this change. An ignore-file edit changes no compiled source, so the remaining C# toolchain stages are N/A:
- Analyzer/build gate (`msbuild ... /p:EnableNETAnalyzers=true`): N/A — no `.cs`/build inputs changed.
- Nullable/type-check gate (`msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`): N/A — no `.cs` changed.
- Test/coverage gate (`vstest.console.exe ... /EnableCodeCoverage`): N/A — no production or test code changed; coverage cannot regress.

The executor MUST NOT run these N/A gates for this remediation. The only required empirical gate is `dotnet csharpier check .` (or `dotnet tool run csharpier check .`).

## Evidence Locations (canonical, non-overridable)

All evidence artifacts MUST be written under:
`docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/evidence/<kind>/`

- Phase 0 baseline: `evidence/baseline/`
- Phase 2 verification: `evidence/qa-gates/`

Note: `.csharpierignore` already ignores `**/evidence/**`, so evidence artifacts are not themselves subject to the csharpier gate.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read the policy files in the mandatory order from `policy-compliance-order` (CLAUDE.md → `.claude/rules/general-code-change.md` → `.claude/rules/general-unit-test.md` → `.claude/rules/csharp.md`) and the CSharpier scope statements in CLAUDE.md (C#1: "csharpier is file-based and formats only `*.cs` without touching project files"). Write `evidence/baseline/phase0-instructions-read.2026-06-13T01-05.md` containing `Timestamp:`, `Policy Order:`, and the explicit list of files read. Acceptance: the artifact exists and lists every file read in order.

- [x] [P0-T2] Capture the current `.csharpierignore` contents verbatim into `evidence/baseline/csharpierignore-preedit.2026-06-13T01-05.md` with fields `Timestamp:`, `Command:` (the read/`Get-Content` used), `EXIT_CODE:`, and `Output Summary:` (the full pre-edit file body and note that no project-file globs are present). Acceptance: artifact shows the pre-edit file with existing globs `**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx` and no `*.csproj`/`*.props`/`*.targets` entries.

- [x] [P0-T3] Run the before-state CSharpier check from the repo root and record the failure. Command: `dotnet csharpier check .` (or `dotnet tool run csharpier check .` if the global tool is not on PATH). Write `evidence/baseline/csharpier-check-before.2026-06-13T01-05.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` capturing the non-zero exit and the list of `.csproj` files reported as "Was not formatted. The file did not end with a single newline" (expected: 8 `.csproj` files; 1060 files checked; exit code 1). Acceptance: artifact records `EXIT_CODE: 1` and the enumerated failing `.csproj` files (`QuickFiler.Test.csproj`, `Tags.Test.csproj`, `TaskMaster.csproj`, `TaskMaster.Test.csproj`, `TaskVisualization.Test.csproj`, `ToDoModel.Test.csproj`, `UtilitiesCS.Test.csproj`, `VBFunctions.Test.csproj`). This is the fail-before evidence for the remediation.

---

### Phase 1 — Apply the `.csharpierignore` Edit

- [x] [P1-T1] Edit ONLY `.csharpierignore` (repo root) to append the three project-file globs alongside the existing ignore entries, preceded by a brief rationale comment. Add exactly:
  ```
  # Project files (*.csproj/*.props/*.targets) are owned by Visual Studio and are
  # not C# source. CSharpier formats C# source only (per CLAUDE.md C#1), so exclude
  # project files from the formatting check.
  *.csproj
  *.props
  *.targets
  ```
  Constraints: do not remove or reorder existing globs; do not modify any other file. Acceptance: `.csharpierignore` contains the three new globs and the rationale comment; `git status` shows `.csharpierignore` as the only modified tracked file (evidence artifacts excluded).

---

### Phase 2 — Verify and Confirm

- [x] [P2-T1] Re-run the CSharpier check from the repo root after the edit. Command: `dotnet csharpier check .` (or `dotnet tool run csharpier check .`). Write `evidence/qa-gates/csharpier-check-after.2026-06-13T01-05.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (expected: exit code 0; none of the previously-failing 8 `.csproj` files reported). Acceptance: artifact records `EXIT_CODE: 0` and confirms zero project-file failures. This is the pass-after evidence.

- [x] [P2-T2] Confirm no `.cs` formatting regressed and the edit is correctly scoped. Verify the Phase 2 csharpier run reports no `.cs` formatting failures, and run `git diff --stat` to confirm `.csharpierignore` is the only modified tracked file. Write `evidence/qa-gates/scope-and-cs-noregress.2026-06-13T01-05.md` with `Timestamp:`, `Command:` (`git diff --stat`), `EXIT_CODE:`, and `Output Summary:` listing the single changed file and confirming no `.cs` formatting diagnostics. Acceptance: only `.csharpierignore` is changed among tracked source files and no `.cs` file is reported unformatted.

- [x] [P2-T3] Record the CI re-run requirement and remediation closure note. Write `evidence/qa-gates/ci-rerun-required.2026-06-13T01-05.md` with `Timestamp:` and a note that: (a) after pushing the `.csharpierignore` edit, the required CI check "Format, build, analyze, and test" MUST re-run green on the branch head (PR #190); (b) the `modified-workflow-needs-green-run` rule does NOT apply because no workflow YAML was changed; (c) the C# analyzer/nullable/test gates are N/A for this ignore-file change. Acceptance: artifact exists and states the CI re-run expectation and the N/A justification.

---

## Acceptance Criteria (remediation)

1. `.csharpierignore` contains `*.csproj`, `*.props`, `*.targets` with a rationale comment, alongside existing globs. (P1-T1)
2. `dotnet csharpier check .` exits 0 after the edit; the 8 previously-failing `.csproj` files are no longer reported. (P2-T1)
3. No `.cs` formatting regressed and `.csharpierignore` is the only modified tracked source file. (P2-T2)
4. Before/after csharpier evidence captured under canonical evidence paths. (P0-T3, P2-T1)
5. CI re-run requirement recorded; N/A toolchain gates explicitly justified. (P2-T3)
