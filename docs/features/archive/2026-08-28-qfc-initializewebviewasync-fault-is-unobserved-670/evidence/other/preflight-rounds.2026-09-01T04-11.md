# Preflight validation rounds — issue #670 atomic plan

Timestamp: 2026-09-01T04-11
Command: Agent(atomic-executor) with `DIRECTIVE: PREFLIGHT VALIDATION ONLY` against `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/plan.2026-08-31T20-20.md`, repeated until clearance; `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: plan` after every revision
EXIT_CODE: 0
Output Summary: Final signal `PREFLIGHT: ALL CLEAR` with `CONVERGENCE: NO FURTHER ROUNDS EXPECTED` on round 5. Five preflight rounds ran in total across two orchestrator children (round 1 by the prior child, rounds 2 through 5 by the resumed child). 27 defects were raised and closed in total: 11 in round 1, 9 in round 2, 4 in round 3, 3 in round 4, and 0 in round 5. The plan held at 5 phases and 71 tasks from round 2 onward. The MCP plan validator returned `ok: true` with no warnings after every revision, including the final state.

## Round ledger

| Round | Reviewer | Defects | Blocking | Signal | Convergence |
|---|---|---|---|---|---|
| 1 | prior child | 11 | 3 | REVISIONS REQUIRED | not recorded as an artifact |
| 2 | this child | 9 | 3 | REVISIONS REQUIRED | FURTHER ROUNDS LIKELY |
| 3 | this child | 4 | 3 | REVISIONS REQUIRED | NO FURTHER ROUNDS EXPECTED |
| 4 | this child | 3 | 1 | REVISIONS REQUIRED | NO FURTHER ROUNDS EXPECTED |
| 5 | this child | 0 | 0 | ALL CLEAR | NO FURTHER ROUNDS EXPECTED |

Round 1's outcome was not recorded as an evidence artifact at the time. It is reconstructed here from the commit message of `8cca5aab` and is marked as such; rounds 2 through 5 are recorded from the reviewer returns themselves.

## Round 2 — 9 defects

Reviewed all 71 tasks in one pass. Three blocking:

1. **P1-T4** required `Select-String -SimpleMatch 'throw'` to return zero matches in the new partial, but `Select-String` is case-insensitive by default and the guard body the same task dictates contains `Token.ThrowIfCancellationRequested()`. The condition was unsatisfiable against the file the task writes. Closed with `-CaseSensitive`.
2. **P4-T5** carried a two-outcome stage-4 rule that contradicted P0-T14 and P4-T9. `Invoke-DotnetCoverageCollection` throws at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:236` on any non-zero vstest exit, and that message does not contain the floor-assertion literal, so a single pre-existing test failure routed the task into a restart branch that could not clear a failure predating the change. Closed with a three-outcome taxonomy.
3. **Cobertura post-processing asymmetry.** Section 4 asserted the same arithmetic applied whether or not post-processing completed. Post-processing also rewrites filenames and removes third-party `package` nodes, so a baseline and a post-change document captured in different states have different denominators and the P4-T8 comparison fails spuriously. Closed with a `POSTPROCESSED:` flag on both sides plus a normalization procedure.

Six non-blocking: an incomplete host-path placeholder set for `vswhere`-resolved tools; P4-T26 instructing an executor to rewrite AC14's criterion text, which `acceptance-criteria-tracking` rule 3 prohibits; P0-T9 stating a precondition for P4-T1 and P2-T4 without gating it; an unrecorded stage-4 command substitution in section 5; an unrecorded sequencing deviation from `spec.md` in section 3; and a misquotation of `.gitignore:144`.

Classification: 5 were round-1 misses, 3 were introduced by the round-1 revision, and 1 became reachable only because of it.

## Round 3 — 4 defects

All four were consequences of the round-2 revision. P4-T28's sanitisation instruction scope was narrower than its acceptance scope, leaving Phase 0 markdown swept but never sanitised; both P3-T14 and P4-T28 invited an artifact that quotes its own sweep patterns, which the next pass then matches; the two `.normalized.cobertura.xml` documents fell inside the acceptance scope and outside the sanitisation scope; and P4-T8 over-claimed that all seven recorded numbers are re-derived when the P0-T13 expression can re-derive only three.

## Round 4 — 3 defects

One blocking. `scripts/vscode/Invoke-Restore.ps1:27` resolves MSBuild through `vswhere` and `:32` echoes it, so P0-T3's `Output Summary:` carries an absolute Program Files path. P3-T15 commits the feature folder in Phase 3 and P4-T28 in Phase 4 was the only sweep reaching it, so the literal would have landed in an intermediate commit against the section 0 obligation. Closed by gating P0-T3 and P0-T5 at capture time. The other two were section 0's `vswhere` binding enumeration being materially incomplete, and the consequential P4-T28 justification text.

A related residual was closed between rounds 3 and 4 on the same reasoning: `dotnet --list-sdks` prints a bracketed machine-wide root that P0-T2's repo-local-only rewrite did not cover, so P0-T2 was amended to sanitise every bracketed SDK root at capture time.

## Round 5 — clearance

No defects. The reviewer independently confirmed the round-4 delta as applied, re-derived every citation the delta touched plus the sibling regions, and verified that excluding P3-T5, P3-T6 and P3-T10 from section 0's `$msbuild` reuser enumeration is correct — those three are bound by the preceding test-runner sentence, and adding them would have attached a conclusion to a premise that does not carry it. Five non-blocking observations were recorded without raising a further round: an ambiguity in what P3-T5 means by "the failure message"; `git commit` written without `-m` in P3-T15 and P4-T29; P0-T9 admitting format drift outside the two gated directories; the `MSBuild\Current\Bin` versus `Bin\amd64` spelling; and `Group-Object` degenerate-case counting.

## Orchestrator reconciliation

Independently measured against the tree at `8cca5aab`, not taken from any reviewer report:

| Fact | Plan asserts | Measured | Match |
|---|---|---|---|
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` lines | 499 | 499 | yes |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` lines | 489 | 489 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` lines | 398 | 398 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` lines | 209 | 209 | yes |
| Discarding call sites in `Initialization.cs` | 192, 288, 324 | 192, 288, 324 | yes |
| Awaited call site in `Initialization.cs` | 256 | 256 | yes |
| `QuickFiler/QuickFiler.csproj` partial block | 331-340, no wildcard | 331-340, no wildcard | yes |
| `spec.md` non-canonical evidence path | line 503 | line 503 | yes |
| `Invoke-MSTestWithCoverage.ps1` non-zero-exit throw | line 236 | line 236 | yes |
| `Invoke-Restore.ps1` MSBuild echo | line 32 | line 32 | yes |
| `.gitignore` coverage ignore | 144 with 145 | 144 with 145 | yes |
| `ConvertTo-KoverageCoberturaXml` declaration | Helpers.ps1:393 | Helpers.ps1:393 | yes |
| `dotnet --list-sdks` machine-wide root | Program Files root printed | printed | yes |

The plan's pinned diff base `2b85134b42872e405602e6064e02dc9cda6c319b` was confirmed to be current `origin/main`: `git rev-list --left-right --count origin/main...HEAD` returned `0 4`, so the branch was zero commits behind and no citation was stale from upstream drift.

## Plan state at clearance

- Single plan file in the feature folder; no timestamped sibling was created, per the Plan-Path Continuity Contract.
- 5 phases, 71 tasks: P0=14, P1=7, P2=6, P3=15, P4=29. Task identifiers sequential per phase and unchanged since round 2.
- `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: plan` returned `ok: true` with no warnings on the final state.
- Work mode `full-bug`; `spec.md` is the acceptance-criteria source with 14 criteria, all unchecked, and `user-story.md` is correctly absent.
- No acceptance criterion was checked off during preparation, and no production file was modified.
