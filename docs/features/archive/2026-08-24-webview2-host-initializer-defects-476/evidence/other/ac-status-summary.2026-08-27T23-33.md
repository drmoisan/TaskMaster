# Acceptance Criteria Status Summary ([P5-T39])

Timestamp: 2026-08-27T23-33

- **AC source:** `docs/features/active/webview2-host-initializer-defects-476/spec.md`, section
  `## Acceptance Criteria`. Work Mode is `full-bug`, so `spec.md` is the sole AC source; no
  `user-story.md` exists and none is required.
- **Total criteria: 37**
- **Satisfied (checked off): 37**
- **Unsatisfied (left unchecked): 0**
- 37 + 0 = 37.

Counts verified mechanically after the check-offs: `grep -c "^- \[x\]"` over the AC section returns
37 and `grep -c "^- \[ \]"` returns 0.

---

## Per-criterion table

| # | Section | Short label | State | Evidence pointer |
| --- | --- | --- | --- | --- |
| 1 | #458 | `ConditionalWeakTable` registry + static gate + lookup-detach-replace | [x] | `evidence/regression-testing/p2-t3-predecessor-detach-green.2026-08-27T20-23.md`; source `WebView2BreadcrumbHost.cs:45-51, 97-108` |
| 2 | #458 | Dead constructor-side unhook removed; detach on predecessor instance | [x] | `evidence/regression-testing/p2-t3-predecessor-detach-green.2026-08-27T20-23.md`; source `WebView2BreadcrumbHost.cs:99-107` |
| 3 | #458 | Host-A/host-B ownership regression test, no reflected handler count | [x] | `evidence/regression-testing/p2-t2-fail-before-all-tests.2026-08-27T20-21.md`, `p2-t3-predecessor-detach-green.2026-08-27T20-23.md` |
| 4 | #458 | Detach tolerates null `CoreWebView2`, with a test | [x] | `evidence/regression-testing/p2-t4-null-core-tolerance-green.2026-08-27T20-25.md`; source `WebView2BreadcrumbHost.cs:314-319` |
| 5 | #458 | `_control.Disposed` detaches and removes the registry entry | [x] | `evidence/regression-testing/p2-t5-disposed-self-detach-green.2026-08-27T20-26.md`; source `:111, :288-301` |
| 6 | #476-1 | Internal 3-arg ctor; public 2-arg chains with unchanged signature | [x] | `evidence/other/p2-t1-seam-declarations.2026-08-27T20-20.md`; `evidence/qa-gates/change-inventory.2026-08-27T23-23.md` |
| 7 | #476-1 | `NavigateToString` forwards inside one `Dispatch` callback | [x] | `evidence/regression-testing/p2-t6-navigate-marshalled-green.2026-08-27T20-27.md` |
| 8 | #476-1 | `PostMessageJson` read + guard + post inside one `Dispatch` callback | [x] | `evidence/regression-testing/p2-t7-post-marshalled-green.2026-08-27T20-29.md` |
| 9 | #476-1 | `DispatchValue` unused | [x] | `evidence/other/p2-t10-no-dispatchvalue.2026-08-27T20-32.md`; re-measured 0 matches at `evidence/other/p5-t11-capturecurrent-absent.2026-08-27T23-27.md` |
| 10 | #476-1 | Dispatcher built in `InitializeAsync` from `uiSyncContext`; no `CaptureCurrent` call | [x] | `evidence/regression-testing/p2-t8-dispatcher-install-green.2026-08-27T20-30.md`; `evidence/other/p5-t11-capturecurrent-absent.2026-08-27T23-27.md` |
| 11 | #476-1 | `BreadcrumbUiDispatcher.cs` unmodified | [x] | `evidence/qa-gates/change-inventory.2026-08-27T23-23.md` (path absent) |
| 12 | #476-1 | Recording-context regression test, one `Post` per call | [x] | `evidence/regression-testing/p2-t2-fail-before-all-tests.2026-08-27T20-21.md`, `p2-t6-...`, `p2-t7-...` |
| 13 | #476-2 | `IsCoreInitialized` explicit field via `Volatile.Read`; auto-property gone | [x] | `evidence/regression-testing/p2-t11-volatile-field-green.2026-08-27T20-34.md` |
| 14 | #476-2 | `Volatile.Write` after subscription, before `CoreInitialized?.Invoke` | [x] | `evidence/other/p2-t12-publication-order.2026-08-27T20-34.md`; source `:345-349` |
| 15 | #476-2 | Structural reflection test with an explicit proxy statement | [x] | `evidence/regression-testing/p2-t11-volatile-field-green.2026-08-27T20-34.md`; XML doc at `WebView2BreadcrumbHostContractTests.cs:29-39` |
| 16 | #477-1 | Interface drops the 1:1 claim; Evergreen decision and `<exception>` docs | [x] | `evidence/other/p2-t15-interface-documentation.2026-08-27T20-38.md`; `IWebViewCoreInitializer.cs:13-19, 35-48, 63` |
| 17 | #477-1 | Signatures unchanged; no caller and no Moq `Setup` modified | [x] | `evidence/other/p5-t18-caller-expressions-unchanged.2026-08-27T23-27.md`; `evidence/qa-gates/change-inventory.2026-08-27T23-23.md`; `evidence/qa-gates/qa-2-analyzers-rebuild.2026-08-27T23-14.md` |
| 18 | #477-1 | Initializer exemption rationale restated; no residual 1:1 claim | [x] | `evidence/other/p2-t14-initializer-rationale.2026-08-27T20-37.md` |
| 19 | #477-2 | `CreateEnvironmentAsync` null and whitespace guards before any SDK call | [x] | `evidence/regression-testing/p2-t13-guards-green.2026-08-27T20-35.md` |
| 20 | #477-2 | `EnsureCoreWebView2Async` guards `control`, not `environment` | [x] | `evidence/regression-testing/p2-t13-guards-green.2026-08-27T20-35.md`; source `WebView2CoreInitializer.cs:78-86` |
| 21 | #477-2 | Guard tests in the existing file, asserting type and `ParamName` | [x] | `evidence/regression-testing/p1-t1-guard-tests-red.2026-08-27T20-08.md`, `p2-t13-guards-green.2026-08-27T20-35.md` |
| 22 | #477-2 | All eleven Moq mock sites pass unmodified | [x] | `evidence/qa-gates/qa-4-tests-coverage.2026-08-27T23-17.md`; `evidence/qa-gates/change-inventory.2026-08-27T23-23.md` |
| 23 | Scope | None of the nine named files modified | [x] | `evidence/qa-gates/change-inventory.2026-08-27T23-23.md` |
| 24 | Scope | Production diff confined to the three in-scope files | [x] | `evidence/qa-gates/change-inventory.2026-08-27T23-23.md` |
| 25 | Scope | `EfcItemController` follow-up left unfixed and handed to the orchestrator | [x] | `evidence/other/followup-promotion-handoff.2026-08-27T23-31.md`; `spec.md` Cross-Feature Notes `:627-633` |
| 26 | Scope | `Compile Include` inserted after the anchor; ItemGroup not re-sorted | [x] | `evidence/other/p5-t27-csproj-insertion.2026-08-27T23-28.md` |
| 27 | Nullable | No `#nullable` in the initializer or the interface | [x] | `evidence/other/p2-t16-nullable-participation.2026-08-27T20-39.md`; re-measured 0 matches 2026-08-27T23-27 |
| 28 | Nullable | New host code nullable-clean under `TreatWarningsAsErrors` | [x] | `evidence/qa-gates/qa-3-nullable-rebuild.2026-08-27T23-15.md` (0 errors, 0 `CS86xx`) |
| 29 | Coverage | Class-level exemption removed; remarks drop the 1:1 claim | [x] | `evidence/regression-testing/p3-t1-class-exemption-removed.2026-08-27T20-42.md` |
| 30 | Coverage | Member-level exemptions only on genuinely host-bound members | [x] | `evidence/regression-testing/p3-t2-member-exemptions.2026-08-27T20-44.md`; `evidence/qa-gates/coverage-delta.2026-08-27T23-20.md` section (c) |
| 31 | Coverage | SDK forwards extracted into small attributed private methods | [x] | `evidence/regression-testing/p3-t2-member-exemptions.2026-08-27T20-44.md`, `p3-t3-initializer-exemptions.2026-08-27T20-46.md` |
| 32 | Coverage | Initializer guards measured; two forwards exempt on the accurate ground | [x] | `evidence/regression-testing/p3-t3-initializer-exemptions.2026-08-27T20-46.md`; `evidence/other/p2-t14-initializer-rationale.2026-08-27T20-37.md` |
| 33 | Coverage | Repository figure before and after captured, delta recorded | [x] | `evidence/baseline/baseline-4-tests-coverage.2026-08-27T20-05.md`, `baseline-perfile-coverage.2026-08-27T20-06.md`, `evidence/qa-gates/qa-4-tests-coverage.2026-08-27T23-17.md`, `coverage-delta.2026-08-27T23-20.md` |
| 34 | Test policy | MSTest + Moq + FluentAssertions with `because:`, AAA comments | [x] | `evidence/other/p5-t35-test-policy-audit.2026-08-27T23-29.md` |
| 35 | Test policy | No temp file, no `Task.Delay`/`Thread.Sleep`, no external dependency | [x] | `evidence/other/p5-t36-determinism-audit.2026-08-27T23-30.md` |
| 36 | Test policy | Distinct control per test; tests pass in any order | [x] | `evidence/other/p5-t37-test-independence.2026-08-27T23-30.md`; `evidence/regression-testing/p5-t37/p5-t37-reversed-order.trx` |
| 37 | Toolchain | Single clean pass in the mandated order, no failures, no rewrites | [x] | `evidence/qa-gates/qa-clean-pass.2026-08-27T23-22.md` |

## Gap statements

No criterion was left unchecked, so no gap statement is required by the acceptance. Four
reconciliations are recorded below instead, because each criterion was checked off on a reading that
differs in some detail from the criterion's literal text. None of them is a substitute for evidence;
each names what was measured and what was not.

### R1 — criterion 10, `CaptureCurrent` raw search count

The plan's evidence sentence for `[P5-T11]` anticipated a zero-match `Select-String -SimpleMatch`
result for `CaptureCurrent` in `WebView2BreadcrumbHost.cs`. The observed raw count is **1**. The
single match is inside a `//` comment at `:255` explaining why the member is deliberately not used;
the comment-stripped count is **0**. The criterion is about a call, and there is no call. Checked off
on the comment-stripped reading plus direct source inspection, recorded in
`evidence/other/p5-t11-capturecurrent-absent.2026-08-27T23-27.md`.

### R2 — criterion 26, the `:159` line-number anchor

`QuickFiler.Test/QuickFiler.Test.csproj:159` denoted the
`Controllers\WebView2CoreInitializerTests.cs` entry when the spec was written. That entry has since
drifted to line 170 (by `BASELINE_SHA`) and then to line 173 (after the integration base merge at
`9cb2c4f6` inserted three sibling entries above it). The two new entries sit immediately after that
anchor at lines 174 and 175, the ItemGroup was not re-sorted, and the hunk adds exactly two lines and
removes none. Checked off on the anchor the line number denotes, not on the integer. Recorded in
`evidence/other/p5-t27-csproj-insertion.2026-08-27T23-28.md`.

### R3 — criterion 37, the formatter and test commands

Two deviations from the criterion's literal command list, both recorded and both deliberate:

1. **Formatting was applied file-scoped and verified repository-wide.** The criterion names
   `dotnet tool run csharpier format .`. `[P4-T1]` ran `csharpier format` against this feature's six
   files and then the read-only repository-wide `dotnet tool run csharpier check .`, per Decisions
   Record item 9. A repository-wide apply would rewrite any pre-existing deviation in a file on this
   feature's forbidden list, manufacturing a scope violation from the toolchain rather than from the
   change. The read-only full-scope check is what CI enforces; it exited 0 and reported zero files.
   MD5 digests taken before the apply, after the apply, and after the test run are identical for all
   six files, so the formatter rewrote nothing.
2. **Tests ran through the coverage wrapper rather than a bare `vstest.console.exe`.** The criterion
   names `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. `[P4-T4]` ran
   `scripts\vscode\Invoke-MSTestWithCoverage.ps1`, which drives `vstest.console.exe` over all nine
   discovered `*.Test.dll` under one `dotnet-coverage collect` with `/InIsolation` and
   `/TestCaseFilter:TestCategory!=LiveOutlook`. A bare invocation would omit the LiveOutlook filter
   and launch a real Outlook process, and would emit a binary `.coverage` file rather than the
   Cobertura the coverage criteria require. The wrapper satisfies the toolchain step while keeping the
   run comparable with the Phase 0 baseline.

The second reconciliation the plan's `[P5-T38]` contemplates — pre-existing failures accepted under
the `[P4-T4]` baseline-comparison clause — was **not needed**: the run reported `Failed: 0` and exited
0, so that clause was never invoked.

### R4 — criterion 34, the recording double versus Moq

Three of the fifteen new tests use `Mock<IWebViewCoreInitializer>`. Five use a hand-written
`RecordingSynchronizationContext` rather than a Moq mock, because criterion 12 mandates "a
**recording** `SynchronizationContext`" that "never drains the posted action". That behavioural
contract is the spec's own requirement, so the double is the mandated design rather than a freely
chosen substitute for Moq. Recorded in
`evidence/other/p5-t35-test-policy-audit.2026-08-27T23-29.md`.

---

## Blocking finding carried out of this plan

The 37 acceptance criteria are all satisfied, but one **plan-level blocking threshold is not met** and
is reported here rather than absorbed. `[P4-T5]` requires newly measured members to reach 90% line
coverage. Four do not:

| Member | Line coverage | Uncovered lines |
| --- | --- | --- |
| `WebView2BreadcrumbHost.NavigateToString` | 62.50% (5/8) | 161, 162, 163 |
| `WebView2BreadcrumbHost.DetachCore` | 66.67% (6/9) | 316, 317, 318 |
| `WebView2CoreInitializer.CreateEnvironmentAsync` | 83.33% (10/12) | 55, 56 |
| `WebView2CoreInitializer.EnsureCoreWebView2Async` | 66.67% (4/6) | 85, 86 |

Aggregate over the eleven enumerated newly measured members: 86/99 = 86.87%, also below 90%.

Every uncovered line above is a statement that reaches the WebView2 SDK. Covering any of them
requires the external Evergreen WebView2 runtime, which `.claude/rules/general-unit-test.md` forbids a
unit test from depending on. The shortfall is structural to the design this plan mandates — extract
the SDK body into an `[ExcludeFromCodeCoverage]` forward and leave the call statement inside the
measured member — and no task in this plan authorizes a redesign. Full detail, including the
per-member table and the line-by-line account, is in
`evidence/qa-gates/coverage-delta.2026-08-27T23-20.md` section (d).

No spec acceptance criterion asserts the 90% new-code floor, so no criterion is left unchecked on its
account. It is a policy-level finding for the orchestrator and the feature reviewer.

## Coverage-threshold conflict between policy documents

Recorded as Decisions Record item 8 and `[P4-T5]` require.

| Document | Repository line floor | Repository branch floor | New-code floor |
| --- | --- | --- | --- |
| `CLAUDE.md` §UT2 | >= 80% | not stated | >= 90% |
| `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md` | >= 85% | >= 75% | not stated |

The two documents disagree on the repository-wide line floor (80 versus 85) and on whether a branch
floor exists at all. This plan applied the stricter of each pair, so the conflict changed no verdict:
the post-change repository figures are **85.1435% line** and **79.2018% branch**, which clear both
line floors and the branch floor. The Phase 0 baseline was 85.1302% and 79.1973%, so both moved
upward, by +0.0133 and +0.0045 percentage points respectively. Both figures are measured on the
unfiltered repository-wide Cobertura denominator, the same denominator the baseline used, so the
deltas are like-for-like. The margin above the 85% line floor is 0.1435 percentage points and remains
thin; the conflict should be resolved in the policy documents rather than re-adjudicated per feature.
