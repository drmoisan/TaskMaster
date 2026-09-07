# Policy Audit — webview2-host-initializer-defects-476

- **Feature:** webview2-host-initializer-defects (#476; also addresses #458, #477)
- **Branch under review:** `bug/webview2-host-initializer-defects-476-exec`
- **HEAD:** `d1dcabd6caa960a68899e28ed9a282eaca6ffd5e`
- **Base:** `origin/epic/quickfiler-bug-family-integration` (tip `69e83171`; merge base equals the tip, branch is 0 behind — verified with `git merge-base` in the review worktree)
- **Work Mode:** `full-bug` (`issue.md` marker verified); AC source is `spec.md` only
- **Reviewer:** feature-review agent
- **Timestamp:** 2026-08-27T23-46
- **Overall verdict:** PASS — 0 Blocking, 1 Non-blocking, 1 Advisory

## 1. Review Scope and Provenance

The audit scope is the full branch diff against the resolved base: 89 files — 6 owned code files, 1 test project file, and 82 documentation/evidence files, all inside `docs/features/active/webview2-host-initializer-defects-476/`. Verified directly with `git diff --numstat origin/epic/quickfiler-bug-family-integration...HEAD`. No path outside the feature's ownership (the six named files, the feature folder, and the `Viewers\WebView2*` project-entry prefix) is touched:

- Forbidden production files (`Viewers/Breadcrumb*`, `Controllers/Efc*`, `Controllers/QfcItemController*`, `Viewers/ToolStrip*`, `EfcFormController.cs`, `WebView2Messenger.cs`, and the rest of the spec's forbidden list): empty diff, verified.
- Merged siblings' project entries survived intact and unreordered: feature 493's `Controllers\QfcItemController.UiThreadDispatcherFixture.cs` / `...FixtureTests.cs` (csproj lines 159-160) and feature 444's eight `Controllers\QfcCollectionController*` entries (lines 122-129), verified by direct read of `QuickFiler.Test/QuickFiler.Test.csproj` at HEAD.
- `spec.md` and `plan.2026-08-24T09-38.md` diffs contain checkbox flips only (0 non-flip changed lines, verified mechanically).

PR context artifacts were absent in the review worktree; a summary and appendix were hand-authored at `artifacts/pr_context.summary.txt` / `artifacts/pr_context.appendix.txt` (gitignored) from `git diff --numstat` per the manual-fallback procedure.

## 2. Rejected Scope Narrowing

None detected. The caller directed a full feature-versus-base audit and supplied a base reference that matches the recomputed merge base. No instruction attempted to narrow the diff, waive a gate, or exclude a language. One caller-brief factual inaccuracy (its description of the #477 fix shape) is recorded in the feature audit; it is a description error, not a narrowing attempt.

## 3. Toolchain Gate Verification

Verified by reading the committed evidence artifacts (the toolchain was not rerun, per review protocol). All four gates were re-executed by the executor after the base merge at `9cb2c4f6`, so they describe the tree under review:

| # | Gate | Evidence artifact (under `evidence/qa-gates/`) | EXIT_CODE | Notes |
|---|---|---|---|---|
| 1 | Formatting | `qa-1-csharpier.2026-08-27T23-13.md` | 0 | Applied file-scoped to the six owned files; verified read-only repo-wide. Deviation from the repo-wide apply command is authorized and reasoned in plan Decisions Record item 9 and recorded at check-off per [P5-T38]. |
| 2 | Analyzers | `qa-2-analyzers-rebuild.2026-08-27T23-14.md` | 0 | `/t:Rebuild`; non-vacuity measured: zero `Skipping target "CoreCompile"` lines against 36 `csc.exe` invocations. |
| 3 | Nullable / type check | `qa-3-nullable-rebuild.2026-08-27T23-15.md` | 0 | 0 errors, 0 CS86xx; same non-vacuity measurement. |
| 4 | Tests + coverage | `qa-4-tests-coverage.2026-08-27T23-17.md` | 0 | 6734/6734 passed, 0 failed, 0 skipped; wrapper drives `vstest.console.exe` with `/InIsolation` and the CI runsettings. |

`qa-clean-pass.2026-08-27T23-22.md` proves the four steps ran consecutively with MD5-verified no-rewrite invariance across the pass. Verdict: PASS.

## 4. Coverage Verification

Figures below were independently re-read by this reviewer from the root `<coverage>` elements of the two committed Cobertura documents (`evidence/baseline/coverage-baseline.cobertura.xml`, `evidence/qa-gates/coverage-postchange.cobertura.xml`) and cross-checked against `coverage-delta.2026-08-27T23-20.md`.

- C# repository line coverage 85.1435% against the 85% floor and branch coverage 79.2018% against the 75% floor: PASS.
- C# baseline-to-post delta: line 85.1302% to 85.1435% (+0.0133 pp), branch 79.1973% to 79.2018% (+0.0045 pp); both rates rose; changed-line no-regression coverage gate: PASS (all three in-scope production files were absent from the baseline document, so no measured line could regress).
- C# newly-measured-member 90% line coverage floor from plan task [P4-T5]: FAIL — dispositioned non-blocking (finding PA-1, Section 6).
- PowerShell coverage gate: PASS — satisfied vacuously.
  The branch diff contains zero PowerShell files, so there is nothing for that gate to measure on this branch.
- Python coverage gate: PASS — satisfied vacuously; zero Python files in the branch diff.
- TypeScript coverage gate: PASS — satisfied vacuously; zero TypeScript files in the branch diff.

The canonical committed C# coverage evidence for this branch is `docs/features/active/webview2-host-initializer-defects-476/evidence/qa-gates/coverage-postchange.cobertura.xml`. No coverage XML was created outside the feature folder by this review.

## 5. Coverage Threshold Conflict Between Policy Documents

Recorded as required. `CLAUDE.md` UT2 sets a repository-wide line floor of 80% with a 90% target for newly added code and states no branch floor. `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` set a uniform 85% line floor and a 75% branch floor across tiers T1-T4. The two documents therefore disagree on the repository line floor (80 versus 85) and on the existence of a branch floor. The stricter of each pair was applied throughout this audit, and the measured figures (85.1435% line, 79.2018% branch) clear both variants, so the conflict changes no verdict on this branch. The contradiction is a standing repository-policy defect, already recorded in the executor's Decisions Record item 8 and Phase 5 summary (finding PA-2, Advisory — repo-level, not introduced by this branch).

## 6. Newly Measured Member Floor — Disposition (PA-1, Non-blocking)

Plan task [P4-T5] imposed a blocking 90% line floor on the eleven members newly entering measurement when the class-level `[ExcludeFromCodeCoverage]` attributes were narrowed to member level. Aggregate 86/99 = 86.87%; four members fall short: `WebView2BreadcrumbHost.NavigateToString` 62.50% (lines 161-163), `WebView2BreadcrumbHost.DetachCore` 66.67% (lines 316-318), `WebView2CoreInitializer.CreateEnvironmentAsync` 83.33% (lines 55-56), `WebView2CoreInitializer.EnsureCoreWebView2Async` 66.67% (lines 85-86).

Structural verification, performed by direct read of both files at HEAD:

| Uncovered lines | What they are | Reviewer assessment |
|---|---|---|
| Host 161-163 | Opening brace, `ForwardNavigateToString(html);`, `return;` — the inline fallback taken when no dispatcher is installed | Line 162 executes `WebView2.NavigateToString`, which on an uninitialized control throws `InvalidOperationException` in-process (proven by the [P2-T2] red run, failure #6). It is therefore executable in a unit test without the Evergreen runtime, but only by asserting a third-party wrapper throw, and the throw makes line 163 unreachable, capping the member at 7/8 = 87.5% — still below 90%. Not ordinary testable first-party logic. |
| Host 316-318 | Brace, `core.WebMessageReceived -= OnWebMessageReceived;`, brace — guarded on `_control.CoreWebView2 != null` | `CoreWebView2` is non-settable and non-null only after live-runtime initialization. Genuinely unreachable in a policy-compliant unit test. |
| Initializer 55-56 | `return ForwardCreateEnvironmentAsync(cacheFolder, options);` and closing brace | Executing it calls `CoreWebView2Environment.CreateAsync`, which begins work when called: it starts the external Evergreen runtime and creates a user-data folder on disk. Both are prohibited for unit tests. Genuinely unreachable. |
| Initializer 85-86 | `return ForwardEnsureCoreWebView2Async(control, environment);` and closing brace | Executing it starts core initialization against the external runtime. Genuinely unreachable. |

No uncovered line in the four members is ordinary testable first-party logic. The disposition is **non-blocking** on four grounds: (1) `CLAUDE.md` UT2's 90% floor binds "new modules, classes, or methods **added**"; these four members are pre-existing and entered measurement only because a class-level exemption was correctly narrowed to member level — reading the added-code floor to penalize exemption-narrowing would deter measurement-honesty improvements; (2) every residual uncovered line is an SDK-reaching statement or its enclosing brace, structural to the plan-mandated extract-the-forward design, and the one theoretically reachable line (162) cannot bring its member to the floor; (3) the repository floors and the changed-line no-regression gate all pass, and both repository rates rose; (4) the shortfall was honestly measured, named per member with figures, and escalated by the executor rather than hidden or papered over with a widened exemption. No `[ExcludeFromCodeCoverage]` attribute was accepted as a dodge for a testability problem: the four short members remain measured, and the review confirmed the exemption set is exactly the four genuinely host-bound host members plus the two initializer forwards, enforced by reflection tests (`WebView2BreadcrumbHost_ExemptsOnlyHostBoundMembers`, `WebView2CoreInitializer_ExemptsOnlyTheSdkForwards`). Recommendation: record maintainer ratification of this residual in the epic's coverage-uplift tracking; optional follow-up tests are listed in the code review (CR-2, CR-3).

## 7. Coverage Exclusion and Exemption Policy Compliance

- No `exclude` entry matching a production source path was added to any coverage configuration; no coverage configuration file is in the diff. PASS.
- Exemption direction of travel is narrowing: two class-level `[ExcludeFromCodeCoverage]` attributes were removed and replaced by member-level attributes on six genuinely host-bound members, each carrying an accurate member-specific rationale (verified by direct read; the false "1:1 forward" rationale is gone from both files). Both types now appear in measurement for the first time. PASS.
- The two SDK event handlers' event-argument types have no public constructors (consistent with the committed rationale), and the two forwards reach the SDK directly. The exemptions rest on genuine host-boundedness, not convenience. PASS.

## 8. Evidence Location Compliance

The branch diff was scanned for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, and `artifacts/coverage/`: zero occurrences. All 82 evidence and documentation files live under the canonical `docs/features/active/webview2-host-initializer-defects-476/` tree (`evidence/baseline/`, `evidence/qa-gates/`, `evidence/regression-testing/`, `evidence/other/`, plus `research/`, `issue.md`, `spec.md`, plan). The named validator script `validate_evidence_locations.py` does not exist in this repository (verified by filesystem search), so the scan was performed manually with `git diff --name-only` filters. No script files (`.ps1`, `.py`, `.sh`) exist anywhere under `evidence/` in the diff (extension scan returned zero matches). PASS.

## 9. File Size Audit

All six owned files are under the 500-line ceiling at HEAD: `WebView2BreadcrumbHost.cs` 368, `WebView2CoreInitializer.cs` 103, `IWebViewCoreInitializer.cs` 66, `WebView2BreadcrumbHostTests.cs` 440, `WebView2BreadcrumbHostContractTests.cs` 201, `WebView2CoreInitializerTests.cs` 173 (measured with `wc -l`). PASS. The largest test file sits at 440 lines; the plan pre-empted the ceiling by splitting the host tests into two files (Decisions Record item 6).

## 10. Test Policy Compliance

Verified by direct read of all three test files, corroborated by `p5-t35`/`p5-t36`/`p5-t37` evidence:

- Framework: MSTest attributes throughout; Moq for the `IWebViewCoreInitializer` seam; FluentAssertions with `because:` arguments on every substantive assertion; explicit `// Arrange` / `// Act` / `// Assert` comments. PASS.
- Banned constructs: grep across the three files for `Task.Delay`, `Thread.Sleep`, `DateTime.Now`, `Random`, temp-file APIs, `BackgroundWorker`, `.Show(`/`.ShowDialog` returned zero matches. `[Timeout(60000)]` attributes are watchdogs, not waits. PASS.
- No external dependencies: the recording `SynchronizationContext` never drains posted callbacks, and the mocked seam returns completed tasks, so no WebView2 runtime, process, network, or disk I/O is reached. PASS.
- Independence: every host test constructs its own `WebView2` control on `WinFormsPumpHost`, so the process-wide owner registry cannot couple tests; a reversed-order full-feature run is committed (`p5-t37-reversed-order.trx`, all passed). PASS.
- Location: all tests live in the `QuickFiler.Test` project tree mirroring the production layout (`Viewers/`, `Controllers/`), matching the repository's established per-project test convention; nothing is colocated in production directories. PASS.
- Fail-before evidence: `p2-t2-fail-before-all-tests.2026-08-27T20-21.md` records all eleven behavioural tests failing at assertion time (11/11 failed, zero compile errors) against the unfixed code, satisfying the bugfix-workflow red-first requirement. PASS.

## 11. Artifact and Path Hygiene

The feature folder was scanned for the account name, the machine name, and absolute user-profile paths: the only matches are the sanitized placeholders `C:\Users\<user>` (one baseline artifact) and `C:\Users\USER` (TRX files); no real account or host name appears. PASS.

## 12. Findings Summary

| ID | Severity | Finding | Disposition |
|---|---|---|---|
| PA-1 | Non-blocking | Plan [P4-T5] 90% line floor on newly measured members fails for four members (86.87% aggregate) | Dispositioned non-blocking per Section 6; structural to the mandated design; recommend maintainer ratification record |
| PA-2 | Advisory | Standing 80/90 versus 85/75 coverage-threshold contradiction between `CLAUDE.md` and `.claude/rules/` | Repo-level policy defect, pre-existing; both variants are met by this branch; already escalated by the executor |

Blocking: 0. Non-blocking: 1. Advisory: 1.
