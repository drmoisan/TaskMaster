# Preflight Round 1 Delta — Issue #742

Timestamp: 2026-09-12T17-05
Reviewer signal: PREFLIGHT: REVISIONS REQUIRED
Convergence signal: CONVERGENCE: FURTHER ROUNDS LIKELY
Plan under review: docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/plan.2026-09-12T16-09.md

## How to apply this delta

Apply each numbered item to the plan file named above, in place. Do not create a timestamped sibling
plan file.

For every item, report one of these dispositions:

- `applied-verbatim`
- `applied-with-mechanical-reassembly` (used only when a command or path was wrapped for width in this
  artifact and had to be rejoined onto one line)
- `not-applied-with-reason`

Do not silently substitute your own wording for a supplied item. Substituted text is unreviewed until
the following round, so each paraphrase converts a closed defect into a new unreviewed region. If you
judge a supplied item to be wrong, leave it unapplied and state the disagreement so the orchestrator can
adjudicate it.

## Orchestrator verification of the round-1 findings

The orchestrator independently re-ran the central finding (D1) against this worktree on 2026-09-12
before relaying this delta:

- `git grep -c '[.]ToString[(]@?"[^"]*[:/.\-][^"]*"[)]' -- <the five production files>` printed no line
  and exited 1.
- `git grep -c '[.]ToString[(]@\?"[^"]*[:/.\-][^"]*"[)]' -- <the same five production files>` printed the
  per-file distribution 4, 4, 1, 3, 2 and exited 0.

The finding is confirmed. The earlier figure of 14 recorded in the planning prompt was measured with the
Grep tool, which uses an extended-regular-expression engine in which a bare question mark is a
quantifier. The plan's acceptance conditions run through basic-regular-expression mode, in which a bare
question mark is a literal character. The two engines disagree, and the plan's own engine is the one
that decides whether its acceptance conditions can fail.

Findings D3, D4, D5 and D6 were also confirmed by the orchestrator before delegation. Item O5 was
reviewed by the reviewer and found not to be a defect; no change is required for it, and the reviewer's
reasoning is accepted.

## Already applied by the orchestrator — do not duplicate

The same unescaped-question-mark defect was present in the residual-sweep acceptance criterion in
spec.md. The orchestrator has already corrected spec.md to use the escaped form and has added the
two-spelling measurement to that criterion's text. Do not edit spec.md. The plan must now match the
corrected spec.

## Delta 1 — [P0-T9], replace item 1 of the eleven-command list with

```
  1. `git grep -c '[.]ToString[(]@\?"[^"]*[:/.\-][^"]*"[)]' -- QuickFiler/Controllers/QfcHomeController.Metrics.cs QuickFiler/Controllers/EfcHomeController.Metrics.cs QuickFiler/Controllers/QfcItemController.ViewerSetup.cs QuickFiler/Controllers/QfcCollectionController.cs QuickFiler/Controllers/EfcItemController.cs` — expected per-file counts 4, 4, 1, 3, 2 (14 total). The `@` is escaped as `@\?` rather than `@?` because `git grep`'s default basic-regex mode treats a bare `?` as a literal character, not a quantifier; `\?` is the documented GNU basic-regex extension for "optional preceding atom." An unescaped `@?` never matches anything in this repository (no site uses a verbatim string literal), so every acceptance condition built on this pattern would read as a permanent zero-match regardless of whether the fix was applied.
```

## Delta 2 — [P2-T3], replace the Acceptance sentence with

```
Acceptance: `git grep -c '[.]ToString[(]@\?"[^"]*[:/.\-][^"]*"[)]' -- QuickFiler/Controllers/EfcHomeController.Metrics.cs` prints no line for that path and exits 1 (baseline was 4, per P0-T9 control 1).
```

## Delta 3 — [P2-T4], replace the Run sentence with

```
Run `git grep -c '[.]ToString[(]@\?"[^"]*[:/.\-][^"]*"[)]' -- QuickFiler/Controllers/QfcHomeController.Metrics.cs QuickFiler/Controllers/EfcHomeController.Metrics.cs`.
```

## Delta 4 — [P3-T4], replace the Run sentence with

```
Run `git grep -c '[.]ToString[(]@\?"[^"]*[:/.\-][^"]*"[)]' -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs QuickFiler/Controllers/QfcCollectionController.cs QuickFiler/Controllers/EfcItemController.cs`.
```

## Delta 5 — [P4-T5], replace the Run sentence with

```
Run `git grep -c '[.]ToString[(]@\?"[^"]*[:/.\-][^"]*"[)]' -- QuickFiler/Controllers/QfcHomeController.Metrics.cs QuickFiler/Controllers/EfcHomeController.Metrics.cs QuickFiler/Controllers/QfcItemController.ViewerSetup.cs QuickFiler/Controllers/QfcCollectionController.cs QuickFiler/Controllers/EfcItemController.cs`.
```

## Delta 6 — [P0-T3], replace the whole task with

```
- [ ] [P0-T3] Bootstrap the toolchain. Run `pwsh -NoProfile -Command 'nuget restore TaskMaster.sln'` followed by `pwsh -NoProfile -Command 'dotnet tool restore'`, followed by `pwsh -NoProfile -Command 'if (Get-Command dotnet-coverage -ErrorAction SilentlyContinue) { Write-Output "DOTNET_COVERAGE_PRESENT" } else { dotnet tool install --global dotnet-coverage; Write-Output "DOTNET_COVERAGE_INSTALLED" }'` (P0-T8 and P5-T4 both require `dotnet-coverage` on PATH; `dotnet-tools.json` pins only `csharpier`, so `dotnet tool restore` alone does not provide it). Acceptance: all three `EXIT_CODE` values are 0, and the third command prints exactly one of `DOTNET_COVERAGE_PRESENT` or `DOTNET_COVERAGE_INSTALLED`. Record `Timestamp:`, `Command:` (all three), `EXIT_CODE:` (all three), `Output Summary:` in `docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/evidence/baseline/toolchain-bootstrap.2026-09-12T16-09.md`.
```

## Delta 7 — [P0-T8], replace the whole task with

```
- [ ] [P0-T8] Baseline `QuickFiler.Test` pass/fail state and coverage figure on the unfixed tree, using the repository's coverage runner, scoped to the `QuickFiler.Test` assembly only, with the raw Cobertura output written under the repository's gitignored `coverage\` directory and deleted by this task before completion (the script's `-CoverageOutput` parameter is passed through `Join-Path $repoRoot $CoverageOutput`, which does not special-case an already-rooted path, so an absolute value produces a malformed path; a relative value avoids this). Run `pwsh -NoProfile -Command './scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput "coverage\issue-742-coverage\baseline.cobertura.xml"; Write-Output "EXITCODE=$LASTEXITCODE"'`. This inner invocation already carries `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook` (see `scripts/vscode/Invoke-MSTestWithCoverage.ps1`'s own argument construction), satisfying the plan's vstest-invocation requirements; scoping `-SearchRoot` to `QuickFiler.Test` restricts assembly discovery to `QuickFiler.Test.dll` and avoids running the rest of the repository's test suite, including the UtilitiesCS.Test shell-icon classes known to stall vstest on this machine. Acceptance: none (baseline only). Open the produced Cobertura XML at `coverage\issue-742-coverage\baseline.cobertura.xml` and transcribe the `line-rate` figure for the classes covering `QfcHomeController.Metrics.cs`, `EfcHomeController.Metrics.cs`, and the `QfcItemController` `ViewerSetup` partial, together with the overall pass/fail/skip counts vstest reports; explicitly note that `QfcCollectionController` and `EfcItemController` carry a type-level `[ExcludeFromCodeCoverage]` attribute and are absent from the report, so their baseline quality signal is the named-test pass/fail state captured here, not a coverage percentage. Delete the `coverage\issue-742-coverage\` directory after transcribing. Record `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail/skip counts plus the three transcribed line-rate figures) in `docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/evidence/baseline/vstest-coverage-baseline.2026-09-12T16-09.md`.
```

## Delta 8 — [P5-T4], replace the whole task with

```
- [ ] [P5-T4] Run the `QuickFiler.Test` suite with coverage enabled via the repository's coverage runner, scoped to the `QuickFiler.Test` assembly only, again writing the raw Cobertura output under the repository's gitignored `coverage\` directory and deleting it before completion (same `Join-Path`/rooted-path reasoning as P0-T8). Run `pwsh -NoProfile -Command './scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput "coverage\issue-742-coverage\final.cobertura.xml"; Write-Output "EXITCODE=$LASTEXITCODE"'`. Acceptance: `EXITCODE` is 0 (zero failures for `QuickFiler.Test`, including the two rewritten oracle tests and the five new tests; this inner invocation already carries `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook`). If this fails or a prior step in this phase rewrote a tracked file since P5-T1, restart from P5-T1. Open the produced Cobertura XML at `coverage\issue-742-coverage\final.cobertura.xml`, transcribe the post-change `line-rate` figures for the same three non-excluded classes captured in P0-T8, state the baseline-vs-final comparison for each, and confirm no regression on the lines this change touched; also transcribe the overall pass/fail/skip counts. Delete the `coverage\issue-742-coverage\` directory after transcribing. Record `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in `docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/evidence/qa-gates/vstest-coverage-final.2026-09-12T16-09.md`.
```

## Delta 9 — [P5-T6], replace the whole task with

```
- [ ] [P5-T6] Confirm no `.trx` or Cobertura coverage XML file has been introduced as a tracked evidence artifact. Run `pwsh -NoProfile -Command '$m = git status --porcelain --untracked-files=all -- docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/evidence | Select-String -Pattern "\.trx$|\.cobertura\.xml$"; $m; if ($m) { exit 0 } else { exit 1 }'`. Acceptance: the command prints no line and exits 1, matching this plan's zero-match reading convention. Record `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in `docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/evidence/qa-gates/no-trx-cobertura-evidence.2026-09-12T16-09.md`.
```

## Delta 10 — SELF-REVIEW block, replace the InternalsVisibleTo bullet with

```
- The QuickFiler production project file (not backticked here; it is out of scope and needs no edit), `QuickFiler/Controllers/QfcHomeController.cs`, and `Properties/AssemblyInfo.cs` — re-read (grep) in this pass; confirmed both files carry `[assembly: InternalsVisibleTo("QuickFiler.Test")]`, corroborating that `QuickFiler.Test` can call the `internal` `GetItemSummary()`, `TryGetMoveReadiness`, and `ToggleExpansionStyle` members exercised by Phase 1 Test 3 and Test 4. QfcHighConfidencePreFilter.cs and the Legacy folder's IAcceleratorCallbacks file were also read; both carry only `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]`, which targets Moq's dynamic-proxy assembly and does not grant `QuickFiler.Test` access, so they are named here in plain prose without backticks rather than cited as supporting evidence; the Legacy file is additionally out of this issue's scope per the Scope and Non-Goals section.
```

## Delta 11 — orchestrator addition, preamble

Add a sentence to the plan's existing zero-match reading convention paragraph recording the
regular-expression-dialect constraint, so a later revision does not reintroduce the defect:

```
The residual-sweep pattern is written with `@\?` rather than `@?` because these searches run in basic-regular-expression mode, in which a bare question mark is a literal character rather than an optional-atom quantifier. Both spellings were run against the unfixed tree on 2026-09-12: the escaped form printed the 4, 4, 1, 3, 2 per-file distribution and exited 0, and the unescaped form printed no line and exited 1. Any future edit to this pattern must keep the escape, or every acceptance condition built on it becomes unfalsifiable.
```

## Not in scope for this delta

Item O5 (a `git commit` with no pathspec operand) was examined and dismissed by the reviewer, on the
grounds that the preceding staging task already scopes the index precisely and no hook in this
repository requires an explicit pathspec on a commit. No change is required.
