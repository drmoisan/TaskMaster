# Feature Audit — quickfiler-test-uithread-dispatcher (#493)

- **Branch:** `bug/quickfiler-test-uithread-dispatcher-493` (HEAD `98113b09`) vs base `125c36b0` (`epic/quickfiler-bug-family-integration` tip; merge-base reviewer-verified)
- **Work mode:** `full-bug` — `spec.md` is the sole acceptance-criteria source (10 criteria, AC-1..AC-10). `user-story.md` exists in the folder but is not an AC source under `full-bug` and was not evaluated or modified. `issue.md` deliberately does not restate the criteria.
- **Reviewer timestamp:** 2026-08-27T15-07

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
| --- | --- | --- |
| AC-1 — Restore exists and is idempotent | **PASS** | `TestSupport.cs` `EnsureUiThreadDispatcher` returns `IDisposable` delegating to `UiThreadDispatcherFixture.EnsureDispatcher()`. `EnsureScope.Dispose` restores conditionally via `CompareExchange` (`ReferenceEquals` compare-then-write, `UiThreadDispatcherFixture.cs:72-84`), second `Dispose` is a guarded no-op, and a no-install call returns `EnsureScope(null)`. R2 and R3 pass (`evidence/qa-gates/quickfiler-test-run.2026-08-27T11-19.md`). |
| AC-2 — Concurrent callers cannot interleave install and restore | **PASS** | Reviewer grep: the only `typeof(UiThread)` reflection swap in the owned files is `UiThreadDispatcherFixture.cs:135`; every mutation path (`Exchange`, `CompareExchange`, `EnsureDispatcher`) holds `FieldLock` for the whole read-modify-write; transactions additionally hold `TransactionGate` from `BeginTransactionAsync` to `Dispose`; `EnsureDispatcher` never acquires `TransactionGate` (verified by inspection); ordering is `TransactionGate` → `FieldLock` with no reverse path (see code-review lock analysis). R1 and R4 pass. The unowned `WpfUiDispatcherTests.cs` mutator is outside "QuickFiler.Test's owned files" per AC-2's own wording (residual R-1, issue #648). |
| AC-3 — Bounded regression test for the #230 deadlock scenario | **PASS** | R1–R5 (plus R6) exist in `QfcItemController.UiThreadDispatcherFixtureTests.cs`; each carries `[Timeout(GateTimeoutMs)]` with `GateTimeoutMs = 60000` (line 34). The class doc comment records R1 as the primary deterministic assertion and R4 as the supporting probabilistic one, with the reason a deterministic R4 is impossible without a forbidden timed wait. |
| AC-4 — #230 local workaround removed, not duplicated | **PASS** | Branch diff shows `SemaphoreSlim UiThreadDispatcherGate` and `SwapUiThreadDispatcher` deleted from `InitializationTests.Part2.cs`; reviewer grep confirms zero remaining references to either symbol and exactly one reflection-swap implementation in owned files (the fixture). Two-phase `BeginTransactionAsync` … `Install` preserved (gate acquired at build start, `Part2.cs:53-55`; install at `Part2.cs:128`). `PumpHarness.Restore()` remains idempotent (`_restored` guard) with restore-before-release ordering enforced inside `UiThreadDispatcherTransaction.Dispose` (R4/R5 prove it). |
| AC-5 — No sleeps, delays, or wall-clock waits; no temp files | **PASS** | Reviewer grep of all four owned files: zero matches for `Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`, `Stopwatch`, `Environment.TickCount`, `SpinWait`, timed `.Wait(n)`, and the temp-file APIs. All coordination is `ManualResetEventSlim` or awaited `Task` completion. Corroborated by `evidence/qa-gates/determinism-audit.2026-08-27T11-39.md` (20/20 clean). |
| AC-6 — `FocusAndThemeTests.cs` unmodified and unregressed | **PASS** (see independent judgment below) | Byte-identity independently verified by the reviewer: `git hash-object` of the working file equals the base blob (`77c4e709…`); 497 lines. Both call sites compile: both msbuild gates exit 0 with zero `error CS`/`warning CS` lines. Both named theme tests pass (P3-T5 evidence). Zero analyzer diagnostics name the file in either log, before and after (reviewer re-verified from the retained P0-T10/P4-T2 extracts). |
| AC-7 — `UtilitiesCS/Threading/UiThread.cs` unmodified | **PASS** | Reviewer-verified: `git hash-object` of the working file equals the base blob (`8663db03…`); the file is absent from the branch diff; no `InternalsVisibleTo("QuickFiler.Test")` grant added (no `UtilitiesCS` file changed at all); zero production assemblies changed (`evidence/qa-gates/scope-lock.2026-08-27T11-46.md`, independently re-run by the reviewer). |
| AC-8 — Every owned and new file at or under 500 lines | **PASS** | Reviewer-measured (awk NR): `TestSupport.cs` 440, `InitializationTests.Part2.cs` 393, `UiThreadDispatcherFixture.cs` 278, `UiThreadDispatcherFixtureTests.cs` 346. The two `<Compile Include>` entries sit immediately after the `QfcItemController.TestSupport.cs` entry in the `Qfc*` neighbourhood (csproj diff, line 155 context). |
| AC-9 — Full C# toolchain passes in a single final pass, in order | **PASS** (evidence-verified) | The four CUT3 commands are recorded with exit 0 / green results in `evidence/qa-gates/` (csharpier-check 11-10, msbuild-analyzers 11-13, msbuild-nullable 11-16 — correctly without `/p:Nullable=enable` — and quickfiler-test-run 11-19: 1072/1072 with `/EnableCodeCoverage /InIsolation`). MSTest/Moq/FluentAssertions only (reviewer-verified in the new files). The reviewer cannot rerun builds in this session; the committed same-session gate artifacts are the verification basis, and the commands stated match CLAUDE.md § CUT3 exactly. |
| AC-10 — Fail-before evidence in the form the defect permits | **PASS** | `evidence/regression-testing/fail-before-exception.2026-08-27T10-27.md` quotes the pre-change `void` helper body verbatim and states why a red test run cannot exist; `evidence/regression-testing/fail-before-compile.2026-08-27T10-44.md` records the expected-fail analyzer build (exit 1) with three distinct `CS0029` diagnostics mapping to R1/R2/R3. Both live under `<FEATURE>/evidence/<kind>/` per the evidence conventions skill. |

**Totals: 10 PASS / 0 PARTIAL / 0 FAIL / 0 UNVERIFIED.**

## Independent AC-6 Judgment (P4-T2 discrepancy)

The plan's P4-T2 required byte-exact set equality of the msbuild-log line sets containing the token `QfcItemController.FocusAndThemeTests.cs`, before vs after. That comparison did not hold, and the executor checked AC-6 (and P4-T2) off anyway. The reviewer examined this independently, without deferring to the executor's or the orchestrator's reasoning:

1. **Independent reproduction.** From the retained extracts (`TestResults/plan-logs/p0-t10/` vs `.../p4-t2/`), the reviewer deleted exactly the two added compile-input tokens (`Controllers\QfcItemController.UiThreadDispatcherFixture.cs` and `...FixtureTests.cs`) from the final analyzer-step and nullable-step extracts and compared byte-for-byte against the baselines: **identical in both cases**. The symmetric difference is therefore exactly the two files this change adds, and nothing else. Per-line length deltas (33240→33363, 33163→33286, and the nullable-step pair) are exactly 123 characters, the length of the two path tokens plus separators.
2. **The failed gate was structurally unsatisfiable as written.** At default msbuild verbosity, every log line containing that token is a `csc.exe` invocation (or its `BuildResponseFile` echo) enumerating the project's entire source set. Any change that adds any file to `QuickFiler.Test` — which this plan's own P1 tasks mandate — makes byte-exact equality impossible. The gate's failure therefore carries no information about `FocusAndThemeTests.cs`; it is a defect in the plan's proxy measurement, disclosed in advance in the P0-T10 baseline artifact and in plan § Notes rule 2.
3. **Every clause AC-6 itself states holds, on reviewer-independent evidence.** Byte-identity: `git hash-object` equals the base blob. 497 lines: measured. Call sites compile: both gates exit 0, zero `error CS`. Theme tests pass: named in the P3-T5 passed list. Diagnostics clause: the diagnostic-bearing subset of matching log lines is empty on both sides (reviewer re-grepped the extracts: zero `error CS`/`warning CS`), which is the *absolute* condition AC-6's final sentence states, not merely non-regression.

**Judgment: AC-6 is honestly PASS.** A plan-mandated gate failing would compel PARTIAL only if the gate measured something the criterion requires; this gate measured compiler-invocation text, which AC-6 does not mention, and its failure mode is fully explained by the two added files. Marking AC-6 PARTIAL would assert a gap in the criterion where none exists. The genuine deviation — a plan task checked `[x]` whose literal acceptance text did not hold — is recorded as Non-blocking finding NB-1 in `policy-audit.2026-08-27T15-07.md` so it is visible in the PR rather than buried, and the follow-up guidance (do not gate future plans on raw compiler-invocation text; the executor's own agent-memory note `project_msbuild_log_token_search_matches_csc_command_line.md` already captures this) is attached there.

## Checkbox Reconciliation (`acceptance-criteria-tracking`)

All 10 AC checkboxes in `spec.md` were `[x]` on entry. Every criterion evaluates PASS, so **no checkbox was changed by this review**. The spec.md branch diff was verified to consist solely of the 10 `[ ]`→`[x]` flips with no criterion text modified, satisfying the preserve-text rule. No phantom criteria were added. `user-story.md` checkboxes were not touched (not an AC source under `full-bug`).

## Residual Risks (report-only, per spec § Risks)

- **R-1** — `WpfUiDispatcherTests.cs` ungated mutator (restores in `finally`; latent ordering hazard, not a no-restore recurrence): tracked as GitHub issue **#648**, verified OPEN; promotion receipts at `evidence/issue-updates/issue-r1-followup-completed.2026-08-27T14-53.md`. Non-blocking.
- **R-2** — `UtilitiesCS.Test` cross-assembly mutators: out of scope, unreachable by any lock inside `QuickFiler.Test`. Non-blocking.
- **R-3/R-5** — steady-state field value unchanged for `EmailMoveMonitorTests`; brief gate serialization of R1–R6 against pump tests. Both accepted by spec. Non-blocking.

## Baseline-Relative Outcome

- Tests: 1066/1066 → 1072/1072 (+6 = R1–R6; no test lost, renamed away, or newly failing).
- Coverage: raw whole-repo Cobertura triple byte-identical to baseline (line-rate 0.19049434489769984, lines-valid 78690); zero delta, as required for a change with zero production lines. See policy-audit § 5 for the coverage rows and the procedural FAIL disposition.
- Warnings: 5 → 5 (identical pre-existing packages.config notices); errors 0 → 0.
- File sizes: both modified files shrank (489→440, 418→393); both new files well under 500.

### Acceptance Criteria Status

- Source: `docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md` (sole source; work mode `full-bug`)
- Total AC items: 10
- Checked off (delivered): 10
- Remaining (unchecked): 0
- Items remaining: none
