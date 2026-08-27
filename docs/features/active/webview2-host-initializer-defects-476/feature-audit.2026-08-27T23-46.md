# Feature Audit — webview2-host-initializer-defects-476

- **Branch:** `bug/webview2-host-initializer-defects-476-exec` at `d1dcabd6caa960a68899e28ed9a282eaca6ffd5e`
- **Base:** `origin/epic/quickfiler-bug-family-integration` (`69e83171`)
- **Work Mode:** `full-bug`; sole AC source is `spec.md` (`## Acceptance Criteria`, 37 checkbox items)
- **Timestamp:** 2026-08-27T23-46
- **Verdict:** 37 of 37 criteria substantiated (PASS). 0 Blocking, 0 Non-blocking, 1 Advisory.

## Method

Every criterion was evaluated against the actual branch diff and committed evidence, not the executor's claims alone. All 37 items were already checked `[x]` by the executor; this review re-verified a full sample — every criterion was traced to at least one of: direct source read at HEAD, direct diff inspection, or a committed evidence artifact whose content was read. No criterion required un-checking. No criterion is worded as being satisfied by a merge or an issue closure; item 25 is a handoff-record criterion satisfied by a committed handoff artifact, and the executor explicitly created no issue.

## Criterion Evaluation

Grouped by spec section; "Verified by" cites what this reviewer independently inspected.

| # | Criterion (short) | Verdict | Verified by |
|---|---|---|---|
| 1 | `ConditionalWeakTable` registry + gate; lookup-detach-replace under gate with `TryGetValue`/`Add`/`Remove` only | PASS | Source read `:46-51`, `:101-110` |
| 2 | Dead constructor-side unhook gone; detach on predecessor instance; misleading comment corrected | PASS | Source read `:41-45`, `:98-110`; base-vs-HEAD diff |
| 3 | Host A/B ownership regression test on one control, attachment-state assertion, fails pre-fix | PASS | Test read (`SecondHost_DetachesThePredecessorAndTakesOwnership`); red record `p2-t2` failure #7; green `p2-t3` |
| 4 | Detach tolerates null `CoreWebView2`, with test | PASS | Source `:312-318`; test `PredecessorDetach_ToleratesNullCoreWebView2`; red `p2-t2` #8 |
| 5 | `Disposed` subscription detaches host and removes registry entry | PASS | Source `:113`, `:287-301`; test `ControlDisposed_DetachesTheHost`; red `p2-t2` #9. See code-review CR-1 for a related residual on predecessor detach. |
| 6 | Internal 3-arg ctor; public 2-arg chains unchanged; no `EfcFormController` edit | PASS | Source `:70-71`, `:88-92`; `EfcFormController.cs` absent from diff |
| 7 | `NavigateToString` forward inside one `Dispatch` callback | PASS | Source `:157-167`; test + red record #6. Null-dispatcher inline fallback is spec-sanctioned (design summary and residual-risk item 2). |
| 8 | `PostMessageJson` read + guard + log-and-drop + post in one callback | PASS | Source `:193-218` (`PostCore` local function); tests |
| 9 | `DispatchValue` unused in the host file | PASS | grep: zero matches |
| 10 | Dispatcher built in `InitializeAsync` from `uiSyncContext` (V1); no `CaptureCurrent` call; no new ctor precondition | PASS | Source `:257-260`; grep shows `CaptureCurrent` appears only inside a comment (`:255-256`) — the reconciliation disclosed by the executor is accurate: it is a comment mention, not a call |
| 11 | `BreadcrumbUiDispatcher.cs` unmodified | PASS | Path absent from branch diff |
| 12 | Recording-context regression test, exactly one `Post` per call, no drain, fails pre-fix | PASS | Test read (both `_PostsExactlyOnceToTheUiContext` tests, `RecordingSynchronizationContext` never invokes callbacks); red record #5/#6 |
| 13 | `IsCoreInitialized` explicit field via `Volatile.Read`; auto-property gone | PASS | Source `:62-64`, `:137`; contract test `IsCoreInitialized_HasAnExplicitBackingField` |
| 14 | `Volatile.Write` strictly after subscription, before `CoreInitialized` raise | PASS | Source `:344-353` with reorder-forbidding comment |
| 15 | Structural test with explicit proxy statement (not a race proof) | PASS | Contract-test XML doc read; wording present verbatim |
| 16 | Interface drops 1:1 claim; Evergreen decision + `<exception>` docs | PASS | `IWebViewCoreInitializer.cs` read (`:13-19`, `:35-48`, `:63`) |
| 17 | Member signatures unchanged; no caller or Moq `Setup` modified | PASS | Interface diff shows doc-only changes around unchanged signatures; no caller file in diff; 6734/6734 green |
| 18 | Initializer exemption rationale restated accurately, no residual 1:1 claim | PASS | `WebView2CoreInitializer.cs:14-31` read; grep for `1:1` in both files: zero matches |
| 19 | `CreateEnvironmentAsync` null + whitespace guards before any SDK call | PASS | Source `:40-51`; guard tests; red record #1/#2 |
| 20 | `EnsureCoreWebView2Async` guards `control`, not `environment` | PASS | Source `:78-85` with rationale comment |
| 21 | Guard tests in existing file, assert type and `ParamName`, no csproj edit needed | PASS | Test read (three `ParamName` assertions); csproj hunk contains only the two new Viewers entries |
| 22 | All eleven Moq mock sites pass unmodified | PASS | No mock-hosting file in diff; full suite green at HEAD |
| 23 | None of the nine forbidden files modified | PASS | Empty diff over the forbidden set |
| 24 | Production diff confined to the three files | PASS | `git diff --numstat`: exactly three production paths |
| 25 | `EfcItemController` seam-bypass left unfixed; handoff recorded for orchestrator promotion | PASS | `EfcItemController.cs` absent from diff; `evidence/other/followup-promotion-handoff.2026-08-27T23-31.md` read — no issue created by executor, handoff explicit |
| 26 | `Compile Include` inserted at the anchor; ItemGroup not re-sorted | PASS | Diff hunk: two lines immediately after the `WebView2CoreInitializerTests.cs` entry (now line 173). The spec's `:159` anchor drifted because merged siblings added entries above it; the anchor's content-identity (insertion contiguous with the WebView2 group, no re-sort) is satisfied, matching the executor's disclosed reconciliation and `p5-t27` evidence |
| 27 | No `#nullable` in the initializer or interface files | PASS | Direct read: neither file carries the directive |
| 28 | New host code nullable-clean (no CS86xx) under the gate | PASS | Host file carries `#nullable enable`; `qa-3` evidence: 0 errors, 0 CS86xx, non-vacuous rebuild |
| 29 | Class-level exemption removed; remarks drop the forward-all claim | PASS | Source read; contract test `WebView2BreadcrumbHost_CarriesNoClassLevelCoverageExemption` |
| 30 | Member-level exemptions only on genuinely host-bound members | PASS | Source read: exactly `OnCoreInitializationCompleted`, `OnWebMessageReceived`, `ForwardNavigateToString`, `ForwardWebMessage` exempt in the host; reflection test pins the set. `InitializeAsync` measured per plan Decisions Record item 5 |
| 31 | SDK forwards extracted into small attributed private methods | PASS | Source read: four forwards, each minimal, each attributed with member-specific rationale |
| 32 | Initializer guards measured; two forwards exempt on the accurate ground | PASS | Source read; reflection test `WebView2CoreInitializer_ExemptsOnlyTheSdkForwards`; coverage-delta section (b) shows the type measured at 77.78% line |
| 33 | Repo coverage before/after captured; delta recorded in canonical evidence sub-paths | PASS | `evidence/baseline/coverage-baseline.cobertura.xml` + `evidence/qa-gates/coverage-postchange.cobertura.xml` + `coverage-delta.2026-08-27T23-20.md`; no `evidence/coverage/` path exists |
| 34 | MSTest + Moq + FluentAssertions with `because:`; AAA comments | PASS | Direct read of all three test files. The disclosed reconciliation stands: Moq covers the seam mock; the recording `SynchronizationContext` is a hand-written test double mandated by criterion 12 itself, so the mix is spec-required, not a deviation |
| 35 | No temp files, no delays/sleeps, no wall-clock waits, no external process/network/runtime | PASS | Reviewer grep over the three files: zero matches; test design never drains SDK-touching callbacks |
| 36 | Distinct control per test; tests pass in any order | PASS | Direct read: every host test constructs its own control; reversed-order TRX committed (`p5-t37`) |
| 37 | Single clean toolchain pass in the mandated order, no failures, no rewrites | PASS | `qa-clean-pass.2026-08-27T23-22.md`: four consecutive EXIT_CODE 0 gates post-base-merge, MD5 no-rewrite proof, non-vacuity counts. Two disclosed reconciliations verified: file-scoped format apply + repo-wide read-only check (authorized by plan Decisions Record item 9) and wrapper-driven `vstest.console.exe` invocation (the plan's own [P4-T4] command, CI-parity settings) |

## Executor's Four Disclosed Reconciliations — Reviewer Verdict

1. **`CaptureCurrent` match lives in a comment** — verified; the only occurrence is explanatory prose at `:255-256`; no call exists. Accepted.
2. **csproj anchor drift 159 → 173** — verified; caused by sibling merges adding entries above the anchor; the insertion satisfies the criterion's content-identity. Accepted.
3. **File-scoped formatter apply + wrapper test invocation** — verified against plan Decisions Record item 9 and task [P4-T1]/[P4-T4]; the repo-wide read-only check is the CI-enforced gate and passed; the wrapper preserves CI parity (`/InIsolation`, runsettings, LiveOutlook filter). Accepted.
4. **Moq plus hand-written recording context** — verified; criterion 12 itself mandates a recording `SynchronizationContext`, which is not a seam Moq serves well. Accepted.

## Advisory Finding

| ID | Severity | Finding |
|---|---|---|
| FA-1 | Advisory | The review-caller's brief described the #477 fix as "restoring the SDK's `browserExecutableFolder` parameter to the `IWebViewCoreInitializer` contract". The spec (criteria 16-17) mandates the opposite shape: unchanged member signatures with the Evergreen-only narrowing documented on the contract, which is what the code implements. The spec governs; the branch is correct. Recorded so downstream summaries do not propagate the inaccurate description. |

## Acceptance Criteria Status

- Source: `docs/features/active/webview2-host-initializer-defects-476/spec.md`
- Total AC items: 37
- Checked off (delivered): 37
- Remaining (unchecked): 0
- Items remaining: none

## Merge Readiness

The branch is ready to merge into `epic/quickfiler-bug-family-integration`. All 37 acceptance criteria are substantiated, the toolchain evidence is clean and post-dates the base merge (branch is 0 behind), ownership boundaries are respected, sibling entries are intact, and the single open coverage finding (policy-audit PA-1) is dispositioned non-blocking with structural verification. Recommended follow-ups: code-review CR-1 (predecessor `Disposed` unsubscription) as a promotion candidate, CR-2/CR-3 as inexpensive test additions.
