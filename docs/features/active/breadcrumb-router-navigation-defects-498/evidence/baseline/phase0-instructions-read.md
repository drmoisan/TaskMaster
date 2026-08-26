# Phase 0 — Instructions and Policy Read Evidence

Timestamp: 2026-08-26T08-26

Policy Order: the order defined by `.claude/skills/policy-compliance-order/SKILL.md` and by the
"Policy Compliance Order" section of `CLAUDE.md`:

1. `CLAUDE.md` (repository root, standing instructions)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. Language-specific rules for the files in scope — C#: `.claude/rules/csharp.md`
5. `.claude/rules/tonality.md`
6. `.claude/rules/plan-acceptance-gates.md`

## Files read in P0-T1 through P0-T8

| Task | File | Lines | Read |
|---|---|---:|---|
| P0-T1 | `CLAUDE.md` | 447 | full |
| P0-T2 | `.claude/rules/general-code-change.md` | 80 | full |
| P0-T3 | `.claude/rules/general-unit-test.md` | 105 | full |
| P0-T4 | `.claude/rules/csharp.md` | 96 | full |
| P0-T5 | `.claude/rules/tonality.md` | 80 | full |
| P0-T6 | `.claude/rules/plan-acceptance-gates.md` | 116 | full |
| P0-T7 | `docs/features/active/breadcrumb-router-navigation-defects-498/spec.md` | 1261 | full (structure map plus verbatim read of Context, Post-#439 Reconciliation, Scope Decisions D1-D9, and Acceptance Criteria) |
| P0-T7 | `docs/features/active/breadcrumb-router-navigation-defects-498/issue.md` | 143 | full |
| P0-T7 | `docs/features/active/breadcrumb-router-navigation-defects-498/research/2026-08-24T09-50-breadcrumb-router-navigation-defects.md` | 1148 | full |
| P0-T8 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | 694 | full (MUST-NOT-WRITE, read-only) |
| P0-T8 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs:377-410` | 462 (file) | landed pin method only |

## P0-T1 — CLAUDE.md, quoted toolchain and constraints

The four toolchain commands, in order, quoted from the "C# Toolchain (run in this exact order)" section:

1. `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`; always via `dotnet tool run`, never a global install)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

`/t:Rebuild` constraint, quoted:

> Use `/t:Rebuild`, not `/t:Build`. Analyzer diagnostics are produced during compilation, and MSBuild's
> incremental up-to-date check compares timestamps without invalidating on a command-line `/p:` change, so a
> warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and runs no analyzers.

No-`/p:Nullable=enable` constraint, quoted:

> **Do not add `/p:Nullable=enable`.** No project in this repository carries a `<Nullable>` element and there
> is no `Directory.Build.props`, so the property is a solution-wide opt-in that conscripts every file which has
> never adopted the pragma. Forcing it produced 195 errors in `UtilitiesCS.csproj` on 2026-08-10 against zero
> errors without it, and CI omits it deliberately.

Bugfix Workflow, the three ordered steps: (1) create a failing regression test first; (2) implement the
minimal, targeted fix; (3) verify locally before review, re-running the original repro and the new regression
test and running the full toolchain in order, restarting from the start if any step changes files or fails.

## P0-T2 — general-code-change.md, quoted

500-line file-size limit, quoted:

> - No production code, test code, or reusable script file may exceed **500 lines**.
> - Exceptions: temporary throwaway scripts created and deleted within an agent session; raw text fixtures for
>   language-processing test data; Markdown documentation files.

Toolchain-loop restart rule, quoted:

> **Restart from step 1** if any stage fails or auto-fixes any files. Do not stop the loop until all seven
> stages complete without errors in a single pass.

## P0-T3 — general-unit-test.md, quoted

Coverage thresholds, quoted:

> - **Line coverage must remain >= 85% across all tiers (T1–T4).**
> - **Branch coverage must remain >= 75% across all tiers (T1–T4) for languages whose coverage tooling measures
>   branch coverage.**

Coverage Exclusion Policy, quoted:

> No production file may be excluded from coverage measurement. Every production source file is in the
> denominator of the coverage metric, regardless of whether its lines are reachable in the test environment.

Note on a divergence recorded for the orchestrator, not resolved here: `CLAUDE.md` section UT2 states
"Repository-wide line coverage must remain `>= 80%`" with a ratified COM/VSTO/WinForms testable-denominator
exemption, while `.claude/rules/general-unit-test.md` states `>= 85%` with no production-file exclusion
permitted. This plan asserts no absolute repository-wide coverage threshold: `P8-T7` gates on
non-regression against the `P0-T15` measured baseline plus a 90.00 percent changed-line floor, so the
divergence does not change any gate this plan executes.

## P0-T4 — csharp.md, C#-specific rules layering on the general policy

- Formatting is CSharpier only, always through `dotnet tool run`; `dotnet format` is prohibited because it
  loads the project model and can rewrite `.csproj` files.
- Analyzer and nullable gates both require `/t:Rebuild` locally; `/p:Nullable=enable` is prohibited.
- Test framework is MSTest; mocking is Moq; assertions prefer FluentAssertions.
- Coverage: repository-wide line coverage >= 80%; any new module, class, or method >= 90%; coverage regression
  on changed lines is a blocking finding. (This is the C# rule file's own figure; see the note under P0-T3.)
- DI seam preference order: interface seam, then injectable delegate seam, then adapter seam for static or
  third-party APIs.
- Analyzer stack is a fixed set of five packages wired file-based through `packages.config` plus explicit
  `<Analyzer Include>` items; new analyzer severities are configured at `suggestion` in `.editorconfig`
  BEFORE any `<Analyzer Include>` is wired, because the type-check step promotes `warning` to error.
- SecurityCodeScan.VS2019 is deliberately dropped (Roslyn 5.6 incompatibility producing CS8032); no CS8032
  suppression is introduced.
- Prohibited: broad refactors across unrelated files, weakening assertions to make tests pass, adding sleeps
  or retries to mask flakiness, reporting success without running the required toolchain.
- Determinism: no network, no mutable machine PATH or profile state, no implicit working-directory
  assumptions; banned in test code are `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`.

## P0-T5 — tonality.md, quoted

> All written output must use a professional tone. Professional tone means:
> - Clear, direct, and factual language.
> - Neutral businesslike phrasing.
> - Measured statements that match the available evidence.
> - Concise explanations that prioritize clarity over personality.
> - Respectful wording, even when reporting defects, regressions, or disagreements.

Jokes, banter, sarcasm, puns, hyperbole, and decorative metaphor are prohibited.

## P0-T6 — plan-acceptance-gates.md, rule table G1 through G6

| Rule | Condition | Shipped severity |
|---|---|---|
| G1 | A non-placeholder `--cov` value whose text, truncated at the first `::`, ends with `.py`. | Blocking |
| G2 | A `--cov` value containing a path separator whose text plus `.py` is a tracked file. | Blocking |
| G3 | A `--cov` value containing a path separator that resolves to neither a tracked file plus `.py` nor a tracked directory. | Warning |
| G4 | A `--cov` value supplied space-separated (`--cov <value>`) rather than with `=`. | Warning |
| G5 | A checkable search literal absent from the tracked tree and not quoted in the plan document outside the command span. | Warning |
| G6 | A checkable search literal absent from every single line of a tracked file but present in that file's four-line sliding-window join. | Warning |

G1 through G4 form a cascade over each `--cov` value; G4 is evaluated independently; G6 is evaluated before
G5. G1 and G4 are context-free; G2, G3, G5, and G6 require a repository seam and are skipped when it is
unavailable. A checkable literal excludes any pattern containing `<`, `>`, `${`, `$(`, or `%`.

## P0-T7 — feature inputs

Reading order used: `spec.md` (version 1.1, the authority), then `issue.md`, then the research artifact. The
research artifact is a dated record of HEAD `988e819b` and is SUPERSEDED in the sections the spec's
"Research sections superseded by the landed work" table names: §Q3e, §Q3d and the D6 hazards, §Q3f, §Q4d,
§Q4f, §Q6a, §Q6c. Where the two disagree, the spec governs.

### Acceptance criteria, AC-1 through AC-31

| ID | Subject | Checkbox state in `spec.md` at Phase 0 |
|---|---|---|
| AC-1 | #498 range guard in the `SegmentDoubleClick` arm; no exception escapes `_host.Raise`; posted-message count unchanged | unchecked |
| AC-2 | #498 rejected index logged at `Error`; `BreadcrumbRow.CollapseAfter` unmodified | unchecked |
| AC-3 | #498 valid index still collapses and posts; `catch (BreadcrumbMessageException)` remains the only catch at the boundary | unchecked |
| AC-4 | #499 `BindRowsAsync` sets `SelectedFolderPath` null; the two write sites unchanged | unchecked |
| AC-5 | #499 `SelectedFolderPathChanged(this, null)` raised only when the value actually changed | unchecked |
| AC-6 | #499 no auto-selection side effect; `SelectFirstRow` still not called from `BindRowsAsync` | unchecked |
| AC-7 | #440 Qfc prerequisite: exact `OrdinalIgnoreCase` first pass preserved; Efc full-path caller never reaches the fallback | unchecked |
| AC-8 | #440 Qfc prerequisite: segment-boundary suffix match resolves an archive-relative stem | unchecked |
| AC-9 | #440 Qfc prerequisite: suffix fallback accepted only when unique; decoy node returns null and logs at `Error` | unchecked |
| AC-10 | #439 Efc multi-segment ancestor chain rendered root-to-leaf | **checked** — RETIRED, inherited-and-verified, delivered by PR #605 |
| AC-11 | #440 Qfc prerequisite: Qfc D5 resolution produces a multi-segment chain against a strict provider | unchecked |
| AC-12 | #439 suggestion-row percentage still rendered after the chain resolves | **checked** — RETIRED, inherited-and-verified, delivered by PR #605 |
| AC-13 | #439 Efc filing target still the presented stem after the chain resolves | **checked** — RETIRED, inherited-and-verified, delivered by PR #605 |
| AC-14 | Decision D7 ladder rung taken is recorded in RISK-1 and its criterion met | unchecked |
| AC-15 | #440 Efc Left activates the parent segment through the landed `ActivateSegment(int)` | unchecked |
| AC-16 | #440 Efc Right expands via a SINGLE `GetImmediateSubfoldersAsync` keyed on `ActiveSegmentKey`; D9 descent mechanism recorded | unchecked |
| AC-17 | #440 Qfc implements the same Left/Right tree contract | unchecked |
| AC-18 | Decision D1 handling order: tree transition, then existing expand/collapse, then `unhandledArrow` | unchecked |
| AC-19 | Decision D1 message shapes unchanged; `FolderBreadcrumbAssetContractTests` passes unmodified | unchecked |
| AC-20 | Decision D1 selector session: `BreadcrumbSelectionSession.cs` unmodified; #400 selector tests pass unmodified | unchecked |
| AC-21 | Decision D1 supersession record present in this spec | unchecked |
| AC-22 | Decision D1 #400 residual contract AC-5 through AC-8 unchanged | unchecked |
| AC-23 | Decision D2 Efc boundaries unchanged | unchecked |
| AC-24 | Decision D2 Qfc boundaries unchanged | unchecked |
| AC-25 | #498 RED-first regression evidence | unchecked |
| AC-26 | #499 RED-first regression evidence | unchecked |
| AC-27 | #440 Qfc prerequisite RED-first regression evidence | unchecked |
| AC-28 | #440 RED-first regression evidence | unchecked |
| AC-29 | Policy: full C# toolchain passes in one clean pass | unchecked |
| AC-30 | Policy: no file outside the owned set is modified | unchecked |
| AC-31 | Policy: file size, decision D8 partial split | unchecked |

Retired inherited-and-verified criteria already checked in `spec.md`: **AC-10, AC-12, AC-13** — all three.
Their evidence pointer is the `P7-T8` artifact; no task in this plan re-implements them.

### Decisions D1 through D9

| ID | Title | Status |
|---|---|---|
| D1 | #400 AC-9 is superseded in part, and the supersession is deliberately narrow | live |
| D2 | Boundary behavior is not unified across the two surfaces | live |
| D3 | The #439 part A / part B / glyph split is moot; all three landed under #439 | RETRACTED |
| D4 | #499 clears and raises | live |
| D5 | Qfc ancestor-chain resolution, the prerequisite for #440's Qfc half | live |
| D6 | Both #439 regression hazards were fixed on `main` | RETRACTED |
| D7 | The Qfc filing-target hazard is an explicit verification gate, not an assumption (three-rung ladder) | live |
| D8 | File-size: the `BreadcrumbBridgeRouter.cs` partial split is MANDATORY | live |
| D9 | #440's Efc half consumes the landed active-segment seams | live |

## P0-T8 — catalogue of `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`

Method-count command and result:

Command: `grep -c -F "[TestMethod]" QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`
EXIT_CODE: 0
Result: `10`

Listed method names: 10 — equal to the measured count.

| # | Method | What it asserts |
|---:|---|---|
| 1 | `Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability` | With an archive root supplied to the four-argument `BindRowsAsync`, archive-relative suggestion and search targets resolve through the archive-rooted hierarchy path exactly once each; banner and trash pseudo-rows are never resolved; the rendered document orders the archive root before `Clients` (root-to-leaf lineage); `73%` still renders; and `SelectedFolderPath` after `rowSelected` equals the presented stem, not the hierarchy path. |
| 2 | `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` | A presented target already rooted with different casing from the configured archive root is passed to `ResolveLeafKeyAsync` verbatim (exactly once) and `SelectedFolderPath` equals that full target unchanged. |
| 3 | `Issue439UnresolvedChainsUseSelectableFallbackForEveryDiagnosableProviderOutcome` | Four diagnosable provider outcomes — null key, empty chain, provider exception, cancellation — each keep one selectable presented segment: `GetAncestorChainAsync` is called exactly once (only for the resolvable key), every fallback label is rendered, and the selected fallback row keeps its original presented filing target. |
| 4 | `Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget` | (Added by PR #611.) A direct `rowSelected` resolves through the FULL store-qualified hierarchy path exactly once while `SelectedFolderPath` is the archive-relative presented target and explicitly not the hierarchy path. **Treat as already covered.** |
| 5 | `Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget` | (Added by PR #611.) `segmentActivate` on segment index 1 yields `SelectedFolderPath == "Clients"` — an archive-relative value — and explicitly not the full hierarchy path. **Treat as already covered.** |
| 6 | `Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget` | (Added by PR #611.) After `segmentActivate`, `leafExpandToggle`, then `renderedChildActivate` with `childIndex` 0, `ResolveLeafKeyAsync` is called exactly once overall and `SelectedFolderPath` is the archive-relative presented target, not the hierarchy path. **Treat as already covered.** |
| 7 | `Issue439AncestorActivationQueriesAncestorKeyAndSelectsArchiveRelativeChild` | `segmentActivate` on the ancestor sets `SelectedFolderPath` to `Clients`; the subsequent `leafExpandToggle` queries `GetImmediateSubfoldersAsync` on the ANCESTOR key exactly once; `renderedChildActivate` with `childIndex` 1 selects the sibling and yields the archive-relative `Clients\South`. |
| 8 | `Issue439InvalidTypedNavigationDoesNotSelectBannerOrPseudoRows` | Syntactically valid `segmentActivate` and `renderedChildActivate` payloads targeting a banner row and a trash pseudo-row leave `SelectedFolderPath` null and make no provider call at all (`VerifyNoOtherCalls`). |
| 9 | `Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic` | Activating the archive-root segment through the `host.Raise` async-void boundary and then activating an out-of-root ancestor directly produces the exact `SelectedFolderPathChanged` sequence `("", "\External\Clients")`, a final `SelectedFolderPath` of `\External\Clients`, exactly two `PostMessageJson` calls, and exactly one resolve and one ancestor-chain call. |
| 10 | `Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection` | A slash-only archive root trims to empty in both hierarchy-conversion directions: the full target is resolved verbatim exactly once, the ancestor chain is fetched exactly once, exactly one document is navigated, and activating segment index 0 yields `SelectedFolderPath == "\Archive"`. |

### Behavior pinned by `LeafExpand_UsesBoundActiveSegmentKeyWithoutResolvingAgain`

Location: `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs:377-410`.

After `Bind()`, a `leafExpandToggle` on `row-0` posts exactly two additional outbound messages, calls
`ResolveLeafKeyAsync(LeafPath, ...)` exactly ONCE (the single call made by `Bind()` itself through
`FetchChainAsync`, with no second resolution on the expansion path), and calls
`GetImmediateSubfoldersAsync` exactly ONCE with the key captured at bind time whose `FolderPath` equals
`LeafPath`. This is the landed pin that forbids reintroducing a `ResolveLeafKeyAsync` call on the expansion
path and that establishes the single-call `GetImmediateSubfoldersAsync` shape.

## Assumptions recorded

- The execution workspace root is resolved at runtime with `git rev-parse --show-toplevel`; no absolute host
  path is recorded in any artifact of this feature.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` and
  `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs:377-410` were read read-only. The first file
  is MUST-NOT-WRITE for this feature.
