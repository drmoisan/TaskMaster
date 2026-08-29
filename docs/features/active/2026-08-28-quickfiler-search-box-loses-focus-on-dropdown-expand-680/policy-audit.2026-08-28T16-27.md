# Policy Compliance Audit — Issue #680: QuickFiler search box loses focus on drop-down expand

- Component: QuickFiler folder-search open/dismiss lifecycle (`BreadcrumbDropDownHost`, `QfcItemController`)
- Date: 2026-08-28T16-27
- Reviewer: feature-review agent
- Branch: `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680` @ `79a8500a2ffffc6449ffc0bbabe9acc66558f91f`
- Base: `main` (merge-base `b0c7fa18a3beb073e7b051f49e28f48159f0f179`; branch rebased onto this exact commit, so merge-base equals the origin/main tip)
- Scope: full branch diff vs merge-base — 12 C#/build files (7 production, 4 test, 1 csproj), 44 docs/evidence/agent-memory files
- Template note: the MCP template asset (`resolve_policy_audit_template_asset`) is not reachable from this session; the artifact is authored against the canonical heading set enumerated in `.claude/skills/policy-audit-template-usage/SKILL.md` and this fallback is recorded per that skill.

## Executive Summary

The branch delivers the #680 fix (suppress WinForms `ModalMenuFilter` menu-mode entry for search-driven popup opens via `ToolStripDropDown.AutoClose = false`, plus controller-owned dismissal) with a strict fail-before/pass-after regression discipline, a clean four-step toolchain final pass, and coverage evidence that this reviewer independently re-parsed and reproduced from the raw Cobertura files. The post-rebase composition with issue #677's `MayTakeFocus` guard was independently re-verified in this review by a fresh build and a 55/55 scoped test run at head. One Blocking policy finding exists: `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` is 514 lines at head, exceeding the 500-line file ceiling; the violation was introduced by this branch's additions on top of the post-#677 base and was not re-audited after the rebase. Remediation inputs are provided.

Verdicts: 1 Blocking FAIL (file-size ceiling), 1 non-blocking FAIL (one modified file at 82.41% line coverage vs the 85% rules floor, dispositioned below), 0 other failures.

## Rejected Scope Narrowing

None detected. The caller prompt explicitly disclaimed scope instruction ("not a scope instruction — determine scope independently"). The plan's `DIRECTIVE: PREFLIGHT VALIDATION ONLY` trailer is planner/executor handoff text, not reviewer scope narrowing. The audit scope is the full branch diff vs `b0c7fa18`.

## Evidence Location Compliance

- Branch diff scanned for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`: **zero occurrences — PASS**. All evidence artifacts live under `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/<kind>/` (canonical).
- `validate_evidence_locations.py` does not exist in this repository (confirmed by filesystem search); the scan above was performed directly against `git diff --name-only b0c7fa18..HEAD`.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required — no non-canonical evidence path was supplied by any caller.

## PR-Context Summary Correction

`artifacts/pr_context.summary.txt` misclassified the branch as docs-only ("Core logic changes: 0 files"), omitting all 12 C#/build files. This is the known recurring generator defect. The reviewer verified the true scope via `git diff --numstat b0c7fa18..HEAD` and corrected the summary in place (dated correction block listing all 12 files in the hook-parseable `- path (+N/-N)` format), so language-coverage enforcement is live for C#.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence / isolation / determinism / speed | PASS | 18 new tests are headless (mocked `IItemViewer`, injected show delegates, no window handles, no timers, no temp files). Reviewer re-ran the scoped suites at head: 55/55 in 1.47 s. |
| Readability, AAA structure, documented intent | PASS | Every new test carries an XML doc summary and explicit Arrange/Act/Assert sections (verified by direct read of all four test files). |
| Scenario completeness (positive/negative/edge/state) | PASS | Open-state true/false controls, latch consumed-exactly-once, already-closed no-spurious-intent, gesture-after-non-capturing, close-restores-default; additive-contract shape tests. |
| No external dependencies, no temp files | PASS | No filesystem, network, or process use in any new test. |
| Test placement | PASS | Repo-established layout (`QuickFiler.Test/` mirrors production namespaces); consistent with the existing suite convention for this legacy solution. |
| Fail-before discipline (bugfix workflow step 1) | PASS | Red runs committed as TRX + markdown: `p2-t3.trx` (2 predicted failures, 25/27 pass), `p2-t10.trx` (3 predicted failures, 9/12 pass); green after fix: `p3-t6.trx` 27/27, `p3-t9.trx` 47/47. Reviewer parsed all five TRX counters directly. |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity / minimal targeted fix | PASS | Framework-native opt-out (`AutoClose`) keyed off the existing `takeFocus` intent; no reflection into framework internals, no popup rewrite. Production diff is +103/-5 across 7 files. |
| **File size <= 500 lines** | **FAIL (Blocking)** | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` is **514 lines at head** (measured by `(Get-Content).Count`). Merge-base: 498. This branch adds +17/-1. The executor's P6-T6 audit recorded 479 — accurate pre-rebase, stale after the rebase composed #677's additions into the same file; no post-rebase size re-audit occurred. All other changed files are within limit (next largest: `QfcItemController.EventWiring.cs` 486, `BreadcrumbDropDownOpenLifetime.cs` 460). See `remediation-inputs.2026-08-28T16-27.md`. |
| Error handling / logging | PASS | No new error paths; dismissal routes through the existing tested cancel pipeline (`SetFolderDroppedDown(false)` → `CancelSelector`). |
| Naming, comments (why not what) | PASS | New members and the latch field carry issue-referenced why-comments; comment on the composed lambda cites both #680 and #677 mechanisms. |
| Public surface minimal / additive | PASS | `SearchLeave` and `IsFolderDropDownOpen` are additive `IItemViewer` members pinned by contract tests; `ShowPopup` change is `internal`. |
| Match existing style / treat existing tests as spec | PASS | Nine pinned #438/#400 suite files verified byte-identical to merge-base by this reviewer (`git diff b0c7fa18..HEAD -- <9 files>` empty); pinned suites green (75/75 TRX + reviewer rerun). |
| Docs updated | PASS with note | Spec, delivery report, rollout notes, HV runbook all present. Note: two delivery-report statements are stale post-rebase (see code-review CR-1). |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | Executor final pass `PRE_FORMAT_CHECK_EXIT: 0` (`p6-t1-format.2026-08-28T16-20.md`); reviewer independently ran `dotnet tool run csharpier check .` at head: exit 0, 1560 files. |
| Analyzer rebuild gate | PASS | `p6-t2-analyzers.2026-08-28T16-20.md` exit 0; post-rebase analyzer rebuild exit 0 recorded in the orchestrator checkpoint (`artifacts/orchestration/orchestrator-state.json`, `rebase_onto_main.verification`). |
| Nullable/type-check gate | PASS | `p6-t3-nullable.2026-08-28T16-20.md` exit 0; post-rebase nullable rebuild exit 0 per checkpoint; `BreadcrumbDropDownHost.Open.cs` carries `#nullable enable` and builds clean. |
| MSTest + Moq + FluentAssertions | PASS | All 18 new tests use `[TestClass]`/`[TestMethod]`, Moq mocks, FluentAssertions with reason strings. |
| No prohibited behaviors (broad refactor, weakened assertions, sleeps) | PASS | Diff footprint confined to the search open/dismiss lifecycle; no assertion weakening in pinned suites (byte-identical). |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| Framework/library selection (CUT1/CUT2) | PASS | MSTest + Moq + FluentAssertions throughout. |
| Seam-based mocking, no external boundaries | PASS | Injected show-delegate seam observes `AutoClose` at show time; controller seam uses `Mock<IItemViewer>` + reflection field injection mirroring the established `SearchFocusRegressionTests` pattern. |
| Deterministic (no wall clock, no sleeps, IDE/CLI parity) | PASS | No banned APIs in new test code (verified by read); scoped run deterministic across executor and reviewer runs. |

## 5. Test Coverage Detail

Coverage artifacts: executor-produced raw Cobertura files `coverage/coverage-baseline-680.cobertura.xml` and `coverage/coverage-final-680.cobertura.xml` (present on disk in the worktree, gitignored per DR-6, with committed numeric transcriptions in `evidence/baseline/p0-t10-*` and `evidence/qa-gates/p6-t4-*`/`p6-t5-*`). This reviewer independently re-parsed both raw files; every figure below is reviewer-reproduced, not transcribed on trust. The canonical hook path `artifacts/csharp/coverage.xml` is not populated; per this workflow's evidence model ("if coverage artifacts already exist from the executor run, inspect them instead of re-running"), the executor Cobertura pair satisfies the artifact-presence requirement. Procedural note (non-blocking, consistent with prior review cycles): the canonical-path copy was not produced.

- C# repo-wide line coverage: **85.279%** final (baseline 85.269%, delta +0.010 pt) — >= 85% floor — **PASS**
- C# repo-wide branch coverage: **79.2235%** final (baseline 79.2133%) — >= 75% floor — **PASS**
- C# new/changed-member coverage: all six changed members at **100%** line coverage vs the 90% new-code floor (reviewer corroborated via per-file counters below) — **PASS**
- C# modified-file line coverage (per-file, final Cobertura, reviewer-recomputed; matches the executor's p6-t5 table exactly):

| File | Baseline covered/total | Final covered/total | Final % | Verdict |
|---|---|---|---|---|
| `BreadcrumbDropDownHost.cs` | 279/281 | 287/289 | 99.31% | line coverage PASS |
| `BreadcrumbDropDownHost.Open.cs` | 14/14 | 18/18 | 100% | line coverage PASS |
| `BreadcrumbDropDownOpenLifetime.cs` | 317/320 | 317/320 | 99.06% | line coverage PASS |
| `QfcItemController.EventHandlers.cs` | 73/92 | 89/108 | 82.41% | line coverage FAIL vs the 85% rules floor — dispositioned non-blocking below |
| `QfcItemController.EventWiring.cs` | 319/375 | 321/377 | 85.15% | line coverage PASS |

- Disposition of the `QfcItemController.EventHandlers.cs` row: the file is above CLAUDE.md § UT2's 80% repository floor; uncovered-line count is unchanged from baseline (19 before, 19 after — every one of this branch's added lines is covered, so there is zero changed-line regression); covered-line count improved +16; both changed members in the file (`TextBoxSearch_KeyDown`, `TextBoxSearch_Leave`) are at 100%. The 19 uncovered lines are pre-existing debt in untouched members. This matches the established non-blocking disposition for modified files that are >= 80%, regression-free, and improved versus baseline. No remediation entry is raised for this row.
- No-regression on changed lines: **PASS** (counter-based comparison, all five measured files final >= baseline; reproduced independently).
- Coverage-exclusion policy: `ItemViewer.FolderSearch.cs` is excluded via the pre-existing ratified `[ExcludeFromCodeCoverage]` on the `ItemViewer` primary partial (CLAUDE.md § UT2 COM/VSTO/WinForms exemption); `IItemViewer.cs` is an interface file with no executable lines. No new exclusion was added by this branch — PASS.
- TypeScript: zero changed files on this branch — not evaluated.
- Python: zero changed files on this branch — not evaluated.
- PowerShell: zero changed `.ps1`/`.psm1` files on this branch — not evaluated.

## 6. Test Execution Metrics

| Run | Tests | Result | Source |
|---|---|---|---|
| Baseline full suite (P0-T10) | 6821 | 0 failed (`BASELINE_FAILURE_SET: none`) | `evidence/baseline/p0-t10-coverage-baseline.2026-08-28T14-55.md` |
| Red run A (host seam) | 27 | 2 failed (both predicted) | `p2-t3.trx` (reviewer-parsed) |
| Red run B (dismissal) | 12 | 3 failed (all predicted) | `p2-t10.trx` (reviewer-parsed) |
| Green run A | 27 | 0 failed | `p3-t6.trx` |
| Green run B | 47 | 0 failed | `p3-t9.trx` |
| Pinned #438/#400 suites | 75 | 0 failed | `p4-t2.trx` |
| Final full suite (P6-T4) | 6839 | 0 failed | `evidence/qa-gates/p6-t4-coverage-final.2026-08-28T16-20.md` |
| Post-rebase QuickFiler.Test (orchestrator) | 1236 | 0 failed | checkpoint `rebase_onto_main.verification` (gitignored; corroborated by the next row) |
| **Reviewer post-rebase scoped rerun at head** | **55** | **0 failed** | fresh `msbuild` rebuild of `QuickFiler.Test.csproj` + `vstest.console.exe` with the combined host/dismissal/wiring/contract/pinned-controller filter, this session |

## 7. Code Quality Checks

| Check | Verdict | Evidence |
|---|---|---|
| Format (CSharpier check, read-only) | PASS | Reviewer-run at head: exit 0. |
| Lint (.NET analyzers, `/t:Rebuild`) | PASS | Executor final pass exit 0; post-rebase rerun exit 0 per checkpoint. |
| Type check (nullable, `/t:Rebuild`, warnings-as-errors) | PASS | Executor final pass exit 0; post-rebase rerun exit 0 per checkpoint. |
| Tests | PASS | 6839/6839 executor final; 55/55 reviewer scoped rerun at head. |
| Evidence sanitization (host paths / account / machine names) | PASS | Reviewer swept every file in the branch diff for the account name, machine name, and `C:\Users\` prefix: zero real leaks. The only two `C:\Users\` occurrences are placeholdered documentation text (`C:\Users\<user>\...`, `C:\Users\<account>\...`) inside agent-memory files describing the sanitization rule itself. All five TRX files parse as well-formed XML with escaped placeholders (`&lt;repo-root&gt;` etc.) and zero raw unescaped placeholder tokens — the commit 72b4b7ed escaping fix is correct and complete. |
| modified-workflow-needs-green-run | PASS (not triggered) | No paths under `.github/workflows/**`, `.github/actions/**`, or `scripts/benchmarks/**` in the branch diff. |
| Tonality | PASS | Reviewed artifacts use neutral, factual language. |

## 8. Gaps and Exceptions

1. **Blocking — file-size ceiling.** `BreadcrumbDropDownHost.cs` at 514 lines (> 500). Introduced by this branch's +16 net lines on the post-#677 base (498). Routed to `remediation-inputs.2026-08-28T16-27.md`.
2. **Non-blocking — AC-1/AC-2 human verification pending.** Both criteria require live-Outlook `ModalMenuFilter` engagement and Win32 focus transitions, which cannot be exercised headlessly. This is handled per the repository's human-exception route: the checkpoint records an `exception` response (`human_interaction.requirements[0]`, id `manual-live-outlook-verification-680`), and a 9-item runbook exists at `runbooks/quickfiler-search-focus-hv-680.runbook.md` covering both DR-8 composition risks (HV-7 post-handoff outside-click; HV-9 row-click on a non-capturing popup). Unchecked-with-documented-reason is the correct treatment; not a delivery gap.
3. **Non-blocking — composed-branch predicate-false test gap.** No automated test opens the popup non-capturing and then issues a `takeFocus: true` reopen with `MayTakeFocus == false` to assert `AutoClose` is still restored. The restore is structurally unconditional (it precedes the guarded `FocusPending()` call inside the scheduled lambda) and the guard itself is proven un-bypassed by `AlreadyOpenRefocus_PredicateFalse_DoesNotFocusPending` (green at head in the reviewer rerun). Recommended follow-up test in code-review CR-2.
4. **Non-blocking — stale post-rebase documentation.** Delivery report asserts the pre-#677 base state; superseded by the rebase. Code-review CR-1.
5. **Non-blocking — canonical coverage-artifact path.** Executor Cobertura pair used in place of `artifacts/csharp/coverage.xml`; figures independently reproduced by the reviewer.

## 9. Summary of Changes

- Production: `AutoClose = takeFocus` write in `ShowPopup` before the show delegate; `takeFocus` threaded through `BreadcrumbDropDownOpenLifetime.ShowCurrentSurface`; `AutoClose = true` restore as the first `CompleteAll` operation in `FinishClose` and inside the already-open `takeFocus: true` scheduled lambda (composed with #677's `FocusPending()` guard wrapper by manual rebase conflict resolution — verified correct); additive `SearchLeave`/`IsFolderDropDownOpen` on `IItemViewer` with forwarding implementations; controller Escape branch, `TextBoxSearch_Leave` dismissal with a one-shot Down-arrow handoff suppression latch; wiring subscribe/detach symmetry.
- Tests: 18 new (6 host-seam, 6 dismissal, 2 wiring, 4 additive-contract), 2 csproj `Compile Include` entries.
- Docs/evidence: spec, plan v1.4, delivery report, rollout notes, HV runbook, issue mirror, 22 evidence markdown artifacts, 5 sanitized TRX files, agent-memory updates.

## 10. Compliance Verdict

**REMEDIATION REQUIRED — one Blocking finding.** The fix itself, its regression discipline, its coverage, and its post-rebase composition with #677 are verified sound; the sole blocker is the 500-line ceiling violation on `BreadcrumbDropDownHost.cs` (514 lines), which requires a small relocation refactor before PR. All other findings are non-blocking and dispositioned above.

## Appendix A: Test Inventory

New tests (18), all verified present and green at head:

`QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs`: ShowPopup_NonFocusingOpen_RunsTheShowDelegateWithAutoCloseFalse; ShowPopup_GestureOpen_RunsTheShowDelegateWithAutoCloseTrue; Close_AfterANonFocusingOpen_RestoresAutoCloseTrue; OpenAsync_TakeFocusReopenOnANonFocusingOpen_RestoresAutoCloseTrue; ShowPopup_GestureOpenAfterANonFocusingCycle_RunsTheShowDelegateWithAutoCloseTrue; ShowPopup_TwoConsecutiveNonFocusingOpens_ShowOnceWithAutoCloseFalse.

`QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs`: TextBoxSearchKeyDown_EscapeWhileDropDownOpen_RoutesExactlyOneCloseIntent; TextBoxSearchKeyDown_EscapeWhileDropDownClosed_RoutesNoIntentAndLeavesKeyUnhandled; TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent; TextBoxSearchLeave_WhileDropDownClosed_RoutesNoIntent; TextBoxSearchLeave_AfterDownArrowHandoff_SuppressesExactlyOneClose; TextBoxSearchKeyDown_DownArrow_StillOpensAndFocusesTheDropDown.

`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs`: WireIntentEvents_SubscribesSearchLeave; UnwireIntentEvents_DetachesSearchLeave.

`QuickFiler.Test/Viewers/ItemViewerSearchDismissalContractTests.cs`: IItemViewer_DeclaresSearchLeaveAsPlainEventHandler; IItemViewer_DeclaresIsFolderDropDownOpenAsReadOnlyBool; IItemViewer_ExistingSearchAndDropDownMemberShapes_AreUnchanged; ItemViewer_ImplementsSearchLeaveAndIsFolderDropDownOpen.

## Appendix B: Toolchain Commands Reference

1. Format: `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`)
2. Analyze: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. Type-check: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. Test: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` (full-suite runs via `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which applies `/TestCaseFilter:TestCategory!=LiveOutlook`)

Reviewer-run commands this cycle: `git diff --numstat b0c7fa18..HEAD`; `git diff b0c7fa18..HEAD -- <9 pinned files>` (empty); `dotnet tool run csharpier check .` (exit 0); `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU` (exit 0); `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"<host|dismissal|wiring|contract|pinned-controller filter>"` (55/55); independent XML re-parse of both Cobertura files and all five TRX files; full-diff sanitization sweep.
