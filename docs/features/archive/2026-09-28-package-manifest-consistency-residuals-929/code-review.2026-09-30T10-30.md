# Code Review — package-manifest-consistency-residuals (Issue #929)

- Artifact timestamp label: 2026-09-30T10-30 (caller-assigned)
- Branch: `bug/package-manifest-consistency-residuals-929`; head `5ce3c8c3b`; source diff base `231e1c0b5`
- Review method: Read, Grep and Glob over the item worktree (Bash forbidden by the caller); no command executed
- Companion artifacts: `policy-audit.2026-09-30T10-30.md`, `feature-audit.2026-09-30T10-30.md`
- Total blocking findings: **0**
- Total non-blocking findings: **7** (code-level; the policy audit carries five further procedural or evidence-hygiene items)

## Executive Summary

The change is small, targeted and matches the bugfix workflow: a tree-reading regression test file was authored and observed red before the fixes and green after, the fixes are the minimal edits the issue names (two deleted `<Import>` lines, two corrected `bindingRedirect` lines, one workflow input rename with its documentation), and the only production PowerShell edit is a three-line comment refresh. The two added in-memory detector tests and the four tree tests follow the conventions of the sibling suites (`$PSScriptRoot`-resolved reads, Arrange/Act/Assert comments, `-Because` on every assertion, no mocks, no files written). All toolchain gates passed on the final iteration.

No blocking defect was found. Seven non-blocking observations are recorded below; none changes behaviour and none is required before merge. Five are cosmetic or style; two are pre-existing conditions outside the diff that the review surfaces for follow-up (a wall-clock wait in a QuickFiler.Test timing test, and tracked `.csproj.bak` copies still carrying the removed token).

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Minor (non-blocking) | `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` (outside the diff) | line 189 (`RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`), wait at line 172/192 | Test relies on a real five-second `Task.Wait(TimeSpan.FromSeconds(5))`; it failed once in local iteration 1 under the full parallel run and passed in the baseline and iteration 2 on the same tree. A second, different timing test (`QfcItemController.UiThreadDispatcherFixtureTests.cs` line 206) failed once on CI run 36722780748. | List for follow-up: replace the wall-clock wait with the deterministic completion signal the determinism rule requires, and examine the dispatcher-fixture test for the same class. Not a change for this branch. | `.claude/rules/general-unit-test.md` bans real wall-clock waits in test code; both tests are pre-existing and no `.cs` file changed here. | `evidence/qa-gates/p2-t7-mstest-coverage.iter1.2026-09-28T20-01.md`; `evidence/qa-gates/p2-t3-pester.iter2.2026-09-28T20-01.md` (run-level conclusion) |
| Minor (non-blocking) | `QuickFiler.Test/QuickFiler.Test.csproj.bak`, `QuickFiler/QuickFiler.csproj.bak` (outside the diff) | whole files | Two of eight tracked `*.csproj.bak` copies still carry the `altcover` token after the live imports were removed. MSBuild does not read `.bak`, so AC1's project-file scope is satisfied. | List for follow-up: delete the eight tracked `.bak` copies (plan decision D12 deliberately left them). | Tracked backup copies of project files are stale duplicates that future censuses have to exclude by pathspec. | Grep `altcover` (case-insensitive) over the worktree excluding `docs/`; `evidence/baseline/p0-t1-worktree-anchor.2026-09-28T20-01.md` BAK-TRACKED list |
| Minor (non-blocking) | `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md` | line 301 | Sources entry still reads "Private key generation and App ID location (steps 10-12)" although step 10 now records the Client ID and states the App ID is not needed. | Reword to "Client ID location" when the runbook is next touched. | Comment/citation text drifted from the instruction it cites; the instruction itself (steps 10, 22, YAML sample) is correct. | Read of lines 99-106, 129-132, 154, 298-321 |
| Minor (non-blocking) | `.github/workflows/dependabot-repair.yml` | line 14 | Header comment line is roughly 150 characters; the surrounding block wraps near 100. | Re-wrap the two-line credential sentence when the file is next edited. | Readability only; actionlint does not police comment width. | Read of lines 13-16 |
| Minor (non-blocking) | `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` | lines 92-110 | Test 2 asserts two assemblies (`Fizzler`, `System.Runtime.CompilerServices.Unsafe`) inside one `It` via `foreach`; a Fizzler failure stops the block before the Unsafe assertions run. | Optional: split into two `It` blocks or use a `-ForEach` data-driven `It`, so each assembly reports independently. | `.claude/rules/powershell.md`: one behaviour per `It`. The census in test 1 is a deliberate single assertion over a set and is acceptable as written. | Read of the file |
| Info (non-blocking) | `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` | lines 65-90 | Test 1 enumerates every top-level directory that holds a `packages.config` and exactly one `.csproj`; directories with zero or several project files are silently skipped. On this tree every manifest directory has exactly one project file, so nothing is skipped today. | Optional hardening: assert the skipped-directory count is zero, or assert the pair count equals the number of `packages.config` files found, so a future multi-project directory cannot drop out of the census unnoticed. | The `Should -BeGreaterThan 9` floor guards against an empty census but not against partial coverage of the tree. | Read of lines 67-74 |
| Info (non-blocking) | feature folder evidence (`Timestamp:` fields) | all executor artifacts | Labels lead the embedded UTC clock readings by 38 to 72 minutes (for example P2-T3 iter2 labelled `10-58` with RUN-START `13:46:37Z`, i.e. 09:46 local), so they are sequence labels rather than clock readings. | None for this branch; future executors should stamp from the clock. | Evidence hygiene; the gates demonstrably ran (JUNIT-WRITTEN follows RUN-START in every artifact; CI run ids are third-party facts). | `evidence/regression-testing/p1-t2-tree-test-fail-before.2026-09-28T20-01.md`, `p1-t11-...`, `evidence/qa-gates/p2-t3-pester.iter2.2026-09-28T20-01.md`; worktree `artifacts/pester/powershell-coverage.xml` report name `Pester (09/30/2026 09:47:03)` |

## Scope Reviewed

| Path | Lines / shape | Reviewed how |
|---|---|---|
| `QuickFiler.Test/QuickFiler.Test.csproj` | 2 deletions (570 to 568) | Grep confirms no `altcover` token remains in any `*.csproj`, `*.config`, `*.props`, `*.targets`; P1-T4 quotes the two deleted lines |
| `SVGControl/app.config` | 2 changed lines (15, 19) | Read in full; compared with `SVGControl.csproj` lines 58 and 82 and `packages.config` lines 4 and 10 |
| `.github/workflows/dependabot-repair.yml` | lines 13-14, 51 | Read in full (173 lines) |
| `.github/workflows/README.md` | line 116 | Grep |
| 911 runbook | Part B heading, steps 10 and 22, YAML sample line 154 | Read lines 96-159 and 296-321 |
| `scripts/dependencies/ConsistencyVerifier.psm1` | comment lines 221-223 | Read lines 180-320 and 424-480 |
| `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | comments at 57-59 and 300; `It` blocks at 240-263; 337 lines | Read lines 1-30, 50-79, 210-329; Grep of every `Describe`/`Context`/`It` line |
| `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` | new, 152 lines | Read in full |
| `docs/features/potential/promoted/2026-09-28-package-manifest-consistency-residuals.md` | promoted record | present on disk; content is the issue body |
| Feature folder | issue.md, plan (56 of 56), 61 evidence `.md`, 2 JaCoCo projections, 2 trx summaries | Glob (69 files); 30 artifacts read in full |

## File-by-File Review

### `QuickFiler.Test/QuickFiler.Test.csproj`

The two removed elements were `Exists()`-guarded imports of `..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.{props,targets}`. No manifest declares altcover, the restore never creates that folder (P0-T5 `ALTCOVER-RESTORED: False`), and CI has no cache fallback that could supply it, so the imports were inert and their removal cannot alter the build graph. The project has no `<Error Condition="!Exists(...altcover...)">` guard to remove alongside (P0-T17 census; plan tree facts). Correct and minimal.

### `SVGControl/app.config`

Both `bindingRedirect` elements now name the assembly version the `<Reference Include>` declares (`Fizzler, Version=1.3.1.0`; `System.Runtime.CompilerServices.Unsafe, Version=6.0.3.0`) and the `oldVersion` upper bound moves with it. The executor additionally verified the restored DLLs' `AssemblyName.Version` (P0-T18: `FIZZLER_ASM=1.3.1.0`, `UNSAFE_ASM=6.0.3.0`) and that `Invoke-BindingRedirectReconciliation` reports zero repairs after the edit (P1-T5). Hand-editing was the right call: the repair script only reconciles redirects for packages it upgraded (plan decision D2).

### `.github/workflows/dependabot-repair.yml`

`client-id` is the input `actions/create-github-app-token` documents as recommended; the runbook's Sources entry (line 318) records that `app-id` remains accepted as legacy, so the rename is behaviour-preserving for the action while removing the deprecation warning that the issue's linked run shows. The secret name is deliberately unchanged (decision D3), so no maintainer action is required for the merge and the workflow's degraded mode (token step fails, job stops before pushing) is unchanged. The header comment was updated to describe the Client ID semantics. Nothing in the `run:` blocks changed, so the `ci-workflows.md` exit-code rule is not engaged.

### `.github/workflows/README.md` and the 911 runbook

README line 116 and runbook steps 10 and 22 now say the secret holds the App's Client ID and that the numeric App ID is not stored. The runbook's YAML sample matches the workflow line for line. One stale citation phrase remains (Findings Table, row 3).

### `scripts/dependencies/ConsistencyVerifier.psm1`

Three comment lines inside the `.DESCRIPTION` of `Find-PackageAbsentFromManifest` now state that issue 929 removed the last live instance and that the shape survives as an in-memory fixture. Line count held at 499 (P2-T1 iter2). No executable line changed; the function body (lines 236-249) is byte-identical to the base per the caller's diff, and the CI JaCoCo documents show the same 158/160 covered lines before and after with the same two uncovered lines (430, 475), which is consistent with an unchanged body.

### `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1`

The two added `It` blocks sit in the existing `Context 'Package absent from the manifest'` and reuse the existing `$script:GuardedUnmanifestedProject` (two guarded altcover imports plus a declared `Contoso.Widgets` reference) and `$script:AgreeingProject` fixtures against `$script:Manifest`. The positive test pins three facts: FindingCount 2, every finding Kind `Import`, every PackageFolder `altcover.8.6.45`. The negative test filters to the Import kind and guards the empty subset with `ExaminedCount -gt 0`, the same guard pattern the sibling tests use. Comments at lines 57-59 and 300 no longer describe the fixture as live. Fixtures are here-strings; no file is written.

### `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`

- Header (lines 1-12): `Set-StrictMode -Version Latest`; module import by `$PSScriptRoot`; the two document paths and the `client-id` pattern are script-scoped constants; the header comment states the no-disk-write and no-`AC<digit>` rules.
- `Get-TokenStepBlock` (14-43): re-implements the step-block slicing of `DependabotConfig.Tests.ps1` (a step runs from its `- name:` line to the next), returns the first step whose `uses:` line contains the token, and returns an empty `string[]` otherwise; the comma-return preserves array shape under StrictMode.
- `Get-DependentAssemblyBlock` (45-60): single-line regex over the config text with `[regex]::Escape` on the assembly name; returns the first matching `<dependentAssembly>` block.
- Test 1 (65-90): census over top-level directories; `ExaminedCount` summed and asserted greater than zero before the zero-findings assertion, so a detector that never fired cannot pass vacuously; the failure message lists each offending Import as `<leaf>: line <n> <folder>` (the P1-T2 message shows exactly that shape).
- Test 2 (92-110): for each of the two assemblies, extracts the `Version=` from the `<Reference Include>` and asserts `newVersion` equality and `oldVersion` suffix. See Findings Table row 5 for the granularity note.
- Test 3 (112-125): exactly one `client-id:` line reading a secret and zero `app-id:` lines inside the token step.
- Test 4 (127-151): extracts the secret name from the workflow, asserts the runbook's YAML sample passes `client-id` from that exact secret and no `app-id`, and asserts Part D (the secret-creation section) mentions `Client ID` and the secret name. The coupling to the workflow's own text is what makes AC6's "names that secret exactly as the workflow names it" a checked property rather than a duplicated literal.

Residual risk recorded by the plan (A2): test 4 reads the runbook at its active-folder path; the eventual feature-promotion move of the 911 folder must update `$script:RunbookPath` in the same change.

## Positive Observations

- Fail-before / pass-after evidence is complete and specific: four red tests with messages quoting the exact defect values, then three green and one red at the intermediate step, then four green.
- Decision D1 avoided adding a production rule the detector already had, keeping the production diff to comments.
- Every gate substitution mandated by the coordinator's PowerShell ruling is recorded in the artifact it affects with a `GATE-SUBSTITUTION:` line.
- Committed coverage evidence is in the permitted projection form; the raw documents were left gitignored under `coverage/` and `artifacts/`, which let this review read the per-line JaCoCo data directly.
- The two P2 loop iterations are both recorded, including the failed one, with an attribution paragraph rather than a silent retry.

## Blocking Count

Blocking: 0. Non-blocking: 7 in this artifact. No remediation inputs are produced.
