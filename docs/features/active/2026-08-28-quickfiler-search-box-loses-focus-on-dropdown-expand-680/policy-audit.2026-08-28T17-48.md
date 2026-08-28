# Policy Compliance Audit — Issue #680: QuickFiler search box loses focus on drop-down expand (re-audit cycle)

- Component: QuickFiler folder-search open/dismiss lifecycle (`BreadcrumbDropDownHost`, `QfcItemController`)
- Date: 2026-08-28T17-48
- Reviewer: feature-review agent (re-audit following remediation of review cycle 2026-08-28T16-27)
- Branch: `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680` @ `c4e96b72b38fc122a8658ecbeff245814eef09bd`
- Base: `main` (merge-base `b0c7fa18a3beb073e7b051f49e28f48159f0f179` = origin/main tip; recomputed by this reviewer via `git merge-base`, matches the PR-context artifacts)
- Scope: full branch diff vs merge-base — 13 C#/build files (7 production, 5 test, 1 csproj), 86 docs/evidence/agent-memory files (99 files, +23292/-40)
- Prior cycle: `policy-audit.2026-08-28T16-27.md` (NO-GO, 1 Blocking: R1 file-size ceiling). This cycle independently re-verifies R1 closure and re-audits the full diff.
- Template note: the MCP template asset (`resolve_policy_audit_template_asset`) is not reachable from this session; the artifact is authored against the canonical heading set of the prior accepted cycle per `.claude/skills/policy-audit-template-usage/SKILL.md`, and this fallback is recorded per that skill.

## Executive Summary

The remediation commit `c4e96b72` closes the prior cycle's sole Blocking finding: `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` is now **498 lines** at head (independently re-measured this session), reduced from 514 by a verbatim relocation of `ShowPopup` and `PublishPopupMessengerReady` into the sibling partial `BreadcrumbDropDownHost.Open.cs` (107 lines). The relocation is behavior-preserving: identical method bodies and `internal` accessibility, same partial type, both call sites (`BreadcrumbDropDownOpenLifetime.cs` lines 276 and 355) unchanged; the only textual addition is the required `using System;`. Both prior non-blocking residuals are also closed: the delivery-report addendum (CR-1) and the #680/#677 composition regression test (CR-2) are verified present and correct. Reviewer-independent verification at head: CSharpier check exit 0 (1560 files), `QuickFiler.Test` build exit 0, scoped host/dismissal/wiring/contract suites **64/64 passed** including the new composition test.

**One new Blocking finding exists**: the five TRX evidence files written by the remediation execution leak host identity — `runUser="<host>\<user>"` (machine + account name) in all five, and 1240 occurrences of raw `c:\users\<user>\...` storage/codebase paths in `p4-t4.trx`. This regresses the branch's own sanitization standard (commit `72b4b7ed` exists solely to sanitize TRX placeholders) and overwrites the previously sanitized `p2-t3.trx`. Routed to `remediation-inputs.2026-08-28T17-48.md`.

Verdicts: 1 Blocking FAIL (TRX evidence sanitization), 1 non-blocking FAIL (one modified file at 82.41% line coverage vs the 85% rules floor, dispositioned below), 3 non-blocking observations (TRX task-ID collision, future-dated artifact timestamps, canonical coverage path).

## Rejected Scope Narrowing

None detected. The caller prompt explicitly disclaimed scope instruction ("not a scope instruction — determine scope independently"). Two plan-internal statements were evaluated and judged not to be reviewer scope narrowing: the remediation plan's "Item 3 ... is not in scope for this plan" line (a statement about that plan's own task scope, not this audit's scope) and its `PREFLIGHT: VALIDATOR NOT RUN` trailer (planner handoff text). The audit scope is the full branch diff vs `b0c7fa18`.

## Untrusted Historical Data Note

The remediation plan's Provenance Note (`remediation-plan.2026-08-28T17-15.md`) documents that a prior revision of that file contained a fabricated claim of maintainer approval to defer finding R1, which was caught and fully reverted before execution. The file on disk documents the incident factually without reproducing the fabricated quote, implements the actual fix (Phase 1 relocation), and contains no deferral. No text anywhere in the branch diff was found that reads as a live instruction or approval from the user or maintainer; nothing was acted on as such. Verified: R1 is fixed, not deferred, and no follow-up issue was filed for it.

## Evidence Location Compliance

- Branch diff scanned for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`: **zero occurrences — PASS**. All evidence artifacts live under `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/<kind>/` (canonical), including the remediation cycle's new `remediation-baseline/`, `regression-testing/`, `qa-gates/`, and `other/` artifacts.
- `validate_evidence_locations.py` does not exist in this repository (confirmed by filesystem search); the scan was performed directly against `git diff --name-only b0c7fa18..HEAD`.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required — no non-canonical evidence path was supplied by any caller.

## PR-Context Summary Correction

`artifacts/pr_context.summary.txt` (refreshed for this cycle) again misclassified the branch as docs-only ("Core logic changes: 0 files"), omitting all 13 C#/build files — the known recurring generator defect. The reviewer verified the true scope via `git diff --numstat b0c7fa18..HEAD` and corrected the summary in place (dated correction block listing all 13 files in the hook-parseable `- path (+N/-N)` format), so language-coverage enforcement is live for C#.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence / isolation / determinism / speed | PASS | 19 new tests (18 from the feature plan + 1 CR-2 composition test) are headless (mocked `IItemViewer`, injected show delegates, `PredicateHarness` drain-based scheduling, no window handles, no timers, no temp files). Reviewer re-ran the scoped suites at head this session: 64/64 in 2.13 s. |
| Readability, AAA structure, documented intent | PASS | The new composition test carries an XML doc summary naming both composed mechanisms (#680 restore, #677 predicate) and explicit Arrange/Act/Assert sections with FluentAssertions reason strings (verified by direct read of the `c4e96b72` diff). Prior 18 tests verified in cycle 16-27. |
| Scenario completeness (positive/negative/edge/state) | PASS | The CR-2 test closes the previously identified composed-branch gap: non-capturing open, `AllowFocus = false`, `takeFocus: true` reopen, asserting `AutoClose` restored AND focus suppressed — the exact restore-before-guard ordering produced by the manual rebase conflict resolution. |
| No external dependencies, no temp files | PASS | No filesystem, network, or process use in any new test code. |
| Test placement | PASS | `BreadcrumbDropDownHostTests.Part3.cs` (repo-established `QuickFiler.Test/` layout mirroring production namespaces). |
| Fail-before discipline (bugfix workflow step 1) | PASS with finding | Red/green chain verified in cycle 16-27 and the red-run markdown transcriptions remain intact at head. Finding (non-blocking, see § 8.3): the remediation reused results directory `p2-t3/`, overwriting the fail-before red-run TRX (27 total / 2 predicted failures, verified by this reviewer at `8e82a2e0:.../p2-t3/p2-t3.trx`) with the remediation's 36/36 green run. The red TRX remains recoverable from git history and its markdown transcription (`p2-t3-red-run-host.2026-08-28T15-30.md`) is unmodified. |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity / minimal targeted fix | PASS | Remediation is a pure mechanical relocation (16 lines moved verbatim + 1 using directive); the underlying fix remains the framework-native `AutoClose` opt-out reviewed in cycle 16-27. Full production diff re-read this cycle. |
| **File size <= 500 lines** | **PASS — prior R1 closed** | Reviewer re-measured every branch-touched file at head this session (`awk 'END{print NR}'`): `BreadcrumbDropDownHost.cs` **498**, `BreadcrumbDropDownHost.Open.cs` **107**, `QfcItemController.EventWiring.cs` 486, `BreadcrumbDropDownHostTests.Part3.cs` 427, `QfcItemController.EventHandlers.cs` 263, all others smaller. All `.cs` files <= 500. (`QuickFiler.Test.csproj` is 509 lines; it is build configuration, not production/test/script code, and is outside the 500-line rule's stated scope.) Near-ceiling watch: `BreadcrumbDropDownHost.cs` 498/500 (2 lines headroom), `BreadcrumbDropDownHostTests.cs` 499/500 (untouched by this branch), `QfcItemController.EventWiring.cs` 486/500. |
| Behavior preservation of the relocation | PASS | Verified three ways: (1) textual — the moved comment block and both method bodies are byte-identical, same `internal` accessibility, same `public sealed partial class BreadcrumbDropDownHost`; (2) call sites — repo-wide grep finds exactly two callers, both in `BreadcrumbDropDownOpenLifetime.cs`, unchanged by the remediation commit; (3) behavioral — scoped host suite population 35 -> 36 (+1 = the new test only) with 0 failures, and reviewer rerun 64/64 at head. |
| Error handling / logging | PASS | No new error paths; unchanged from cycle 16-27. |
| Naming, comments (why not what) | PASS | Relocated members retain their issue-referenced why-comments; the new test's doc comment states the composed scenario. |
| Public surface minimal / additive | PASS | No public-surface change in the remediation; feature surface (`SearchLeave`, `IsFolderDropDownOpen`) unchanged from cycle 16-27, pinned by contract tests. |
| Match existing style / treat existing tests as spec | PASS | Relocation follows the type's established ceiling-relief partial pattern (`.Open.cs`); using-directive placed in alphabetical order; zero modifications to any pinned suite (the nine pinned #438/#400 files remain byte-identical to merge-base — re-confirmed via `git diff --name-status`, none appear in the diff). |
| Docs updated | PASS | CR-1 addendum verified: append-only (26 added lines, zero deletions in the `c4e96b72` diff of `delivery-report.2026-08-28T16-40.md`), dated, corrects exactly the two stale statements (FocusPending wrapper vs raw delegate; #677 merged into base) without rewriting the execution-time record. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | Reviewer independently ran `dotnet tool run csharpier check .` at head this session: **exit 0, 1560 files**. Executor final pass also exit 0 (`p4-t1-format.2026-08-28T19-38.md`, after one formatter-triggered loop restart recorded in the restart note). |
| Analyzer rebuild gate | PASS | `p4-t2-analyzers.2026-08-28T19-42.md`: full `/t:Rebuild` with `EnableNETAnalyzers`/`EnforceCodeStyleInBuild`, exit 0, warning set matches remediation baseline (P0-T4). |
| Nullable/type-check gate | PASS | `p4-t3-nullable.2026-08-28T19-46.md`: full `/t:Rebuild` with `TreatWarningsAsErrors=true`, exit 0. `BreadcrumbDropDownHost.Open.cs` retains `#nullable enable` and the relocated members compile under it with no new warnings (both members use non-nullable parameters/fields). |
| MSTest + Moq + FluentAssertions | PASS | The new test uses `[TestMethod]` + FluentAssertions with reason strings, in the existing `PredicateHarness` idiom. |
| No prohibited behaviors | PASS | No refactor beyond the mandated relocation; no assertion weakening; no sleeps/timing hacks (drain-based scheduling). |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| Framework/library selection (CUT1/CUT2) | PASS | MSTest + FluentAssertions; harness seams rather than external mocking where the concrete host is under test. |
| Seam-based mocking, no external boundaries | PASS | `PredicateHarness` counts the raw `_focusPending` delegate, so the composition test fails if any resolution bypasses the guard wrapper — the same property that made #677's predicate test load-bearing. |
| Deterministic (no wall clock, no sleeps, IDE/CLI parity) | PASS | No banned APIs in the new test; reviewer rerun deterministic (64/64, 2.13 s). |

## 5. Test Coverage Detail

Coverage artifacts: executor-produced raw Cobertura files `coverage/coverage-remediation-baseline-680.cobertura.xml`, `coverage/coverage-remediation-final-680.cobertura.xml`, and `coverage/coverage-remediation-final-680-rerun.cobertura.xml` (present on disk in the worktree, gitignored, with committed numeric transcriptions in `evidence/remediation-baseline/p0-t8-*` and `evidence/qa-gates/p4-t5-*`). This reviewer independently re-parsed all three raw files; every figure below is reviewer-reproduced from the XML root and per-class counters, not transcribed on trust. The canonical hook path `artifacts/csharp/coverage.xml` is not populated; per this workflow's evidence model (inspect pre-existing coverage artifacts instead of re-running), the executor Cobertura set satisfies the artifact-presence requirement. Procedural note (non-blocking, consistent with prior cycles): the canonical-path copy was not produced.

- C# repo-wide line coverage: **85.27%** final (run 1: 0.852717; same-command run 2: 0.852888; same-session baseline: 0.852841) — >= 85% floor — **PASS**
- C# repo-wide branch coverage: **79.24%** final (0.792401 run 1 / 0.792462 run 2; baseline 0.79234) — >= 75% floor — **PASS**
- C# new/changed-member coverage: the remediation adds no production members; the six feature-changed members were verified at 100% line coverage vs the 90% new-code floor in cycle 16-27, and the two relocated members remain fully covered in their new file (`BreadcrumbDropDownHost.Open.cs` 23/23 = 100% final) — **PASS**
- C# modified-file line coverage (per-file, reviewer-recomputed this session from the remediation baseline and final Cobertura):

| File | Baseline covered/total | Final covered/total | Final % | Verdict |
|---|---|---|---|---|
| `BreadcrumbDropDownHost.cs` | 296/298 | 291/293 | 99.32% | line coverage PASS |
| `BreadcrumbDropDownHost.Open.cs` | 18/18 | 23/23 | 100% | line coverage PASS |
| `BreadcrumbDropDownOpenLifetime.cs` | 317/320 | 317/320 | 99.06% | line coverage PASS |
| `QfcItemController.EventHandlers.cs` | 89/108 | 89/108 | 82.41% | line coverage FAIL vs the 85% rules floor — dispositioned non-blocking below |
| `QfcItemController.EventWiring.cs` | 321/377 | 321/377 | 85.15% | line coverage PASS |

- Disposition of the `QfcItemController.EventHandlers.cs` row (unchanged from cycle 16-27; the remediation does not touch this file): above CLAUDE.md § UT2's 80% repository floor; zero changed-line regression (19 uncovered lines before and after, all pre-existing debt in untouched members; both changed members at 100%); improved +16 covered lines vs the feature baseline. Matches the established non-blocking disposition for modified files that are >= 80%, regression-free, and improved versus baseline. No remediation entry raised for this row.
- Relocation coverage effect: `BreadcrumbDropDownHost.cs` 296/298 -> 291/293 and `.Open.cs` 18/18 -> 23/23 — the five-line delta is exactly the two relocated members changing files, with both files' percentages unchanged (99.33% -> 99.32% rounding; 100% -> 100%). Zero regression on either touched file — **PASS**.
- Run-1 repo-wide dip adjudication: run 1's line-rate (0.852717) reads 8 covered lines (~0.012 pt) below the same-session baseline (0.852841). This reviewer accepts the executor's noise adjudication on independent grounds: the delta is inside the documented cross-run nondeterminism band for this parallel coverage toolchain; the same-command no-source-change run 2 came back above baseline on both figures (0.852888 / 0.792462); and the per-file counters show zero regression on both relocation-touched files. The dip is measurement noise, not a regression.
- No-regression on changed lines: **PASS** (counter-based per-file comparison, reproduced independently).
- Coverage-exclusion policy: unchanged from cycle 16-27 — `ItemViewer.FolderSearch.cs` sits under the pre-existing ratified `[ExcludeFromCodeCoverage]` on the `ItemViewer` WinForms partial (CLAUDE.md § UT2 COM/VSTO/WinForms exemption); `IItemViewer.cs` is an interface file with no executable lines. No new exclusion added by this branch (confirmed: the diff adds no `[ExcludeFromCodeCoverage]` and no coverage-config exclude) — PASS.
- TypeScript: zero changed files on this branch — not evaluated.
- Python: zero changed files on this branch — not evaluated.
- PowerShell: zero changed `.ps1`/`.psm1` files on this branch — not evaluated.

## 6. Test Execution Metrics

| Run | Tests | Result | Source |
|---|---|---|---|
| Feature baseline full suite (P0-T10) | 6821 | 0 failed | cycle 16-27 record |
| Feature final full suite (P6-T4) | 6839 | 0 failed | cycle 16-27 record |
| Remediation baseline host suite (P0-T6) | 35 | 0 failed | `p0-t6.trx` (reviewer-parsed) |
| Remediation post-move host suite (P1-T3) | 35 | 0 failed | `p1-t3.trx` |
| Remediation new-test-green host suite (P2-T3) | 36 | 0 failed (includes the composition test, outcome Passed) | `p2-t3.trx` (reviewer-parsed: `total="36" passed="36" failed="0"`) |
| Remediation full `QuickFiler.Test` (P4-T4) | baseline+1 | 0 failed | `p4-t4.trx` (contains the composition test) |
| Remediation coverage runs (P0-T8 / P4-T5) | 6856 / 6857 | 0 failed both | evidence artifacts + reviewer re-parse of all three Cobertura XMLs |
| **Reviewer scoped rerun at head (this session)** | **64** | **0 failed** | fresh `MSBuild` build of `QuickFiler.Test.csproj` (exit 0) + `vstest.console.exe` (Extensions\TestPlatform) with the combined host/dismissal/wiring/contract filter |

## 7. Code Quality Checks

| Check | Verdict | Evidence |
|---|---|---|
| Format (CSharpier check, read-only) | PASS | Reviewer-run at head this session: exit 0, 1560 files. |
| Lint (.NET analyzers, `/t:Rebuild`) | PASS | Remediation P4-T2 exit 0, warning set = baseline. |
| Type check (nullable, `/t:Rebuild`, warnings-as-errors) | PASS | Remediation P4-T3 exit 0, no CS86xx. |
| Tests | PASS | 36/36 scoped and full-suite green in remediation evidence; 64/64 reviewer scoped rerun at head. |
| **Evidence sanitization (host paths / account / machine names)** | **FAIL (Blocking)** | Reviewer swept every file in the branch diff for the account name, machine name, and user-profile path prefixes (case-insensitive). The five TRX files written by the remediation execution leak host identity: `runUser="<host>\<user>"` in **all five** (`p0-t6.trx`, `p0-t7.trx`, `p1-t3.trx`, `p2-t3.trx`, `p4-t4.trx`), and `p4-t4.trx` additionally carries **1240** occurrences of raw `c:\users\<user>\repos\taskmaster-wt\...` storage/codebase paths. This regresses the branch's own sanitization standard: commit `72b4b7ed` exists solely to sanitize exactly these token classes in the earlier TRX set (which remains clean), and the remediation overwrote the previously sanitized `p2-t3.trx` with unsanitized content. The leak was introduced solely by `c4e96b72` (verified via `git log -S '<host>'`). Distinguished from the non-blocking partial-sanitize precedent: here no sanitization pass ran at all on the new files, the volume is three orders of magnitude larger, and a sanitized artifact regressed. Routed to `remediation-inputs.2026-08-28T17-48.md` (R2). No leak exists in any markdown evidence artifact (all use `<repo-root>`/`<FEATURE>` placeholders per the plan's D6). |
| modified-workflow-needs-green-run | PASS (not triggered) | No paths under `.github/workflows/**`, `.github/actions/**`, or `scripts/benchmarks/**` in the branch diff (re-verified this cycle). |
| Tonality | PASS | Remediation plan, addendum, and evidence artifacts use neutral, factual language; the Provenance Note reports the fabrication incident without dramatization. |

## 8. Gaps and Exceptions

1. **Blocking — TRX host-identity leak (new this cycle).** Five remediation-cycle TRX files carry the machine name, account name, and (in `p4-t4.trx`) 1240 raw user-profile paths. See § 7 row and `remediation-inputs.2026-08-28T17-48.md` R2. Mechanical fix: apply the `72b4b7ed` sanitization treatment (placeholder substitution with XML-escaped tokens) to the five files.
2. **Non-blocking — AC-1/AC-2 human verification pending (carried, by design).** Both criteria require live-Outlook `ModalMenuFilter` engagement and Win32 focus transitions that cannot be exercised headlessly. The checkpoint records the human-exception response, and the 9-item runbook exists at `runbooks/quickfiler-search-focus-hv-680.runbook.md`. Unchecked-with-documented-reason remains the correct treatment; unchanged by the remediation.
3. **Non-blocking — remediation task-ID collision overwrote the AC-3 red-run TRX.** The remediation plan reused results directory `evidence/regression-testing/p2-t3/`, so its green run (36/36) replaced the feature plan's fail-before red run (27 total, 25 passed, 2 predicted failures). The red-run markdown transcription is intact and the red TRX is recoverable at `8e82a2e0`, but at head the TRX cited by `p2-t3-red-run-host.2026-08-28T15-30.md` no longer shows the red run. Recommended fix (may ride the R2 sanitization commit): restore the sanitized red TRX from `72b4b7ed` to `p2-t3/`, move the remediation green TRX to a non-colliding directory (e.g., `r-p2-t3/`), and update `p2-t3-new-test-green.2026-08-28T19-27.md` accordingly.
4. **Non-blocking — future-dated remediation artifact timestamps.** The remediation evidence artifacts are self-stamped 2026-08-28T18-16 through T20-12, yet the commit containing them was created at 17:40 and the wall clock at this review is 17:48 — the artifact `<ts>` stamps are ahead of real time and violate the convention that `<ts>` is taken at task execution time. The content is otherwise consistent and corroborated; recording accuracy only. Recommended: note the discrepancy in the delivery-report addendum chain rather than renaming committed artifacts.
5. **Non-blocking — canonical coverage-artifact path (carried).** Executor Cobertura set used in place of `artifacts/csharp/coverage.xml`; all figures independently reproduced by this reviewer.
6. **Near-ceiling watch (informational).** `BreadcrumbDropDownHost.cs` 498/500, `BreadcrumbDropDownHostTests.cs` 499/500, `QfcItemController.EventWiring.cs` 486/500 — the next change to any of these will likely need relief first.

## 9. Summary of Changes (this cycle's delta vs cycle 16-27)

- Production: verbatim relocation of `ShowPopup(Point, bool)` (with its issue-#680 comment block) and `PublishPopupMessengerReady` from `BreadcrumbDropDownHost.cs` (514 -> 498 lines) into `BreadcrumbDropDownHost.Open.cs` (90 -> 107 lines); `using System;` added for `EventArgs.Empty`. No signature, accessibility, or behavior change; call sites untouched.
- Tests: 1 new composition test `OpenAsync_TakeFocusReopenAfterNonCapturingOpenWithPredicateFalse_RestoresAutoCloseButSuppressesFocus` in `BreadcrumbDropDownHostTests.Part3.cs` (427/500 lines).
- Docs/evidence: append-only Post-Rebase Addendum to `delivery-report.2026-08-28T16-40.md` (two corrections); remediation plan (with Provenance Note documenting the reverted fabrication incident); 22 remediation evidence artifacts including 5 TRX files (the TRX files carry the Blocking sanitization finding above); prior cycle's four review artifacts committed at `8e82a2e0`.

## 10. Compliance Verdict

**REMEDIATION REQUIRED — one Blocking finding.** The prior cycle's Blocking finding (R1, file-size ceiling) and both non-blocking residuals (CR-1, CR-2) are verified closed; the fix, its regression discipline, its coverage, and the relocation's behavior preservation are all independently verified sound at head. The sole blocker is the host-identity leak in the five remediation-cycle TRX evidence files, which requires a mechanical sanitization pass (and should carry the p2-t3 red-TRX restore) before PR. See `remediation-inputs.2026-08-28T17-48.md`.

## Appendix: Reviewer-Run Commands This Cycle

`git rev-parse HEAD` / `git merge-base HEAD origin/main`; `git diff --name-status b0c7fa18..HEAD` (full scope, all pathspecs); `awk 'END{print NR}'` on all 13 branch-touched code files; `git show c4e96b72` (full remediation diff read); repo-wide grep for `ShowPopup(`/`PublishPopupMessengerReady(` (call-site verification); `git show 8e82a2e0:.../p2-t3/p2-t3.trx` (pre-remediation red-run counters); case-insensitive diff-wide sweep for account name, machine name, and user-profile prefixes; `git log -S '<host>'` (leak attribution); independent XML re-parse of all three remediation Cobertura files (root + per-class counters); `dotnet tool run csharpier check .` (exit 0); `MSBuild.exe QuickFiler.Test/QuickFiler.Test.csproj -t:Build -p:Configuration=Debug -p:Platform=AnyCPU` (exit 0); `vstest.console.exe` (Extensions\TestPlatform) scoped rerun (64/64).
