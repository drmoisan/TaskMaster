# Research: Workflow Citation Mapping for Issue #564

**Issue:** #564 — CLAUDE.md cites `.github/workflows/ci.yml` as the source of three C# toolchain commands, but PR #556 (issue #553) moved those steps into five reusable `workflow_call` files. `ci.yml` is now a pure dispatcher.

**Scope:** Documentation-only repoint of three citations in `CLAUDE.md`. No command text or workflow behavior changes. This artifact is research only; no files were edited.

## 1. `.github/workflows/ci.yml` current content

Full file (33 lines), read in full:

```yaml
name: CI
...
jobs:
  actionlint:
    name: actionlint
    uses: ./.github/workflows/_actionlint.yml
  format-check:
    name: format-check
    uses: ./.github/workflows/_format-check.yml
  build-analyzers:
    name: build-analyzers
    uses: ./.github/workflows/_build-analyzers.yml
  build-nullable:
    name: build-nullable
    uses: ./.github/workflows/_build-nullable.yml
  mstest-coverage:
    name: mstest-coverage
    uses: ./.github/workflows/_mstest-coverage.yml
```

Confirmed: `ci.yml` has exactly 5 jobs (`actionlint`, `format-check`, `build-analyzers`, `build-nullable`, `mstest-coverage`), and every job body is a single `uses: ./.github/workflows/_*.yml` dispatch line. There are no `run:` steps, no `steps:` blocks, and no direct `msbuild` or `csharpier` invocations anywhere in `ci.yml`. The claim in the issue — that `ci.yml` is now a pure dispatcher — is confirmed.

Sibling reusable-workflow file present but out of scope: `.github/workflows/_actionlint.yml` and `.github/workflows/_mstest-coverage.yml` (not cited by any of the three CLAUDE.md sites; also `.github/workflows/codex-web-setup-test.yml`, unrelated).

## 2. CSharpier check step — CLAUDE.md line 194

`CLAUDE.md:194`:
> "Always invoke through `dotnet tool run` so the manifest-pinned version is used. Do not invoke a globally installed `csharpier`: a different global version produces diffs that disagree with `.github/workflows/ci.yml`, which runs the pinned version after `dotnet tool restore`."

**Correct target file:** `.github/workflows/_format-check.yml`

Relevant steps (`.github/workflows/_format-check.yml:35-41`):

```yaml
      - name: Setup CSharpier
        shell: pwsh
        run: dotnet tool restore

      - name: Verify formatting
        shell: pwsh
        run: dotnet csharpier check .
```

Note for the fix author: the CI step invokes `dotnet csharpier check .` (bare, after `dotnet tool restore` puts the pinned local tool on the manifest-resolved path), not `dotnet tool run csharpier check .`. Both resolve to the same pinned 1.2.6 binary declared in `dotnet-tools.json`, so the parity claim ("runs the pinned version after `dotnet tool restore`") is still accurate, but the exact command string differs from what CLAUDE.md's own recommended local command is. This is a pre-existing wording nuance, not something introduced by #553/#556; it is out of scope for #564 (documentation-only citation repoint), but is noted here as evidence context.

## 3. Analyzer step — CLAUDE.md line 202

`CLAUDE.md:202`:
> "Use `/t:Rebuild`, not `/t:Build`. Analyzer diagnostics are produced during compilation, and MSBuild's incremental up-to-date check compares timestamps without invalidating on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and runs no analyzers. `.github/workflows/ci.yml` uses `/t:Build /m` for its analyzer step because a runner checkout is always cold; a local working tree is not."

**Correct target file:** `.github/workflows/_build-analyzers.yml`

Relevant step (`.github/workflows/_build-analyzers.yml:47-53`):

```yaml
      - name: Build with analyzers and code style enforcement
        shell: pwsh
        run: |
          & msbuild $env:SOLUTION_PATH /t:Build /m /p:Configuration=Debug `
              "/p:Platform=Any CPU" `
              /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
          if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
```

`$env:SOLUTION_PATH` is set to `TaskMaster.sln` at `.github/workflows/_build-analyzers.yml:17`. Modulo that variable substitution, the command matches CLAUDE.md's description of the CI analyzer step (`/t:Build /m`, `/p:Configuration=Debug`, `"/p:Platform=Any CPU"`, `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`).

## 4. Nullable step — CLAUDE.md lines 210-211

`CLAUDE.md:210`:
> "This is character-for-character the command in `.github/workflows/ci.yml` (step "Build with nullable warnings treated as errors"). Two properties of it are load-bearing and must not be "restored": ..."

**Correct target file:** `.github/workflows/_build-nullable.yml`

Job/step name confirmed literal match. Relevant step (`.github/workflows/_build-nullable.yml:11-60`):

```yaml
  build-nullable:
    name: Build with nullable warnings treated as errors
    ...
      - name: Build with nullable warnings treated as errors
        shell: pwsh
        run: |
          # Use /t:Rebuild (not /t:Build) so this step always performs a genuine full
          # recompile. ...
          & msbuild $env:SOLUTION_PATH /t:Rebuild /m /p:Configuration=Debug `
              "/p:Platform=Any CPU" `
              /p:TreatWarningsAsErrors=true
          if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
```

`$env:SOLUTION_PATH` is set to `TaskMaster.sln` at `.github/workflows/_build-nullable.yml:17`.

**Character-for-character comparison against CLAUDE.md's stated command:**
`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

Substituting `$env:SOLUTION_PATH` -> `TaskMaster.sln`, the workflow's line reads:
`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

This is a character-for-character match (modulo the `$env:SOLUTION_PATH` -> `TaskMaster.sln` variable substitution and PowerShell line-continuation backticks/whitespace, which are formatting artifacts of the multi-line `run:` block, not command-text differences). Both the job's `name:` field (`.github/workflows/_build-nullable.yml:12`) and the step's `name:` field (`.github/workflows/_build-nullable.yml:47`) are literally "Build with nullable warnings treated as errors", matching CLAUDE.md's quoted step name exactly.

## 5. `.claude/rules/csharp.md` — confirm no citation to ci.yml or any workflow file

Search patterns run against `.claude/rules/csharp.md` (full path: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a2e056cf0bbd1ce8b\.claude\rules\csharp.md`):

| Pattern | Result |
|---|---|
| `ci\.yml` | No matches found |
| `\.github/workflows` | No matches found |
| `workflows` | No matches found |

All three searches returned zero matches. `.claude/rules/csharp.md` does not cite `ci.yml` or any `.github/workflows/*.yml` file anywhere in the file. **Confirmed: this file requires no change for issue #564.**

## 6. CLAUDE.md — confirm exactly the three known citation sites (no others)

Search pattern: `ci\.yml` against `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a2e056cf0bbd1ce8b\CLAUDE.md`.

Result (content mode, with line numbers):
- Line 194: `.github/workflows/ci.yml` (CSharpier parity claim)
- Line 202: `.github/workflows/ci.yml` (analyzer `/t:Build /m` claim)
- Line 210: `.github/workflows/ci.yml` (nullable `TreatWarningsAsErrors` claim)

Count-mode confirmation: `CLAUDE.md:3` — 3 total occurrences across 1 file, matching the 3 content-mode hits above. No additional citation sites exist. The fix's scope (repoint exactly these three lines) is complete; no other CLAUDE.md location references `.github/workflows/ci.yml`.

## Summary — Correct Citation Targets

| CLAUDE.md line | Claim | Current (incorrect) target | Correct target |
|---|---|---|---|
| 194 | CSharpier check-parity | `.github/workflows/ci.yml` | `.github/workflows/_format-check.yml` |
| 202 | Analyzer `/t:Build /m` step | `.github/workflows/ci.yml` | `.github/workflows/_build-analyzers.yml` |
| 210-211 | Nullable `TreatWarningsAsErrors` step, named "Build with nullable warnings treated as errors" | `.github/workflows/ci.yml` | `.github/workflows/_build-nullable.yml` |

## Numeric Derivation Evidence

Not applicable. This research does not propose any numeric count, enumeration, or population for a `spec.md` acceptance criterion. The three citation sites were located by direct grep against the full CLAUDE.md file content (content-mode, line-numbered) and cross-checked by an independent count-mode grep against the same pattern, both yielding 3 — this is a citation-location confirmation, not a numeric assertion requiring the Numeric Derivation Evidence protocol.

## Recommended Fix (for the implementing agent — not applied here)

Repoint the three citations, updating only the file-name references and, where present, the phrase "for its analyzer step" / "runs the pinned version after" to name the correct reusable workflow file instead of `ci.yml`. No command text, step names, or workflow YAML should change. `.claude/rules/csharp.md` requires no edit (per section 5).

## Testing Implications

This is a documentation-only change with no executable code path. No unit, integration, or coverage tests apply. Verification is limited to: (a) grep-confirming the three CLAUDE.md citations now name the correct reusable workflow files, (b) grep-confirming no other CLAUDE.md location still cites `ci.yml` for these claims, and (c) confirming `.claude/rules/csharp.md` remains unchanged (git diff shows no modification to that file).
