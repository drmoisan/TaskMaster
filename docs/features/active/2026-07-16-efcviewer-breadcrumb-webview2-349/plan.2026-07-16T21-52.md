# efcviewer-breadcrumb-webview2 — Plan

- **Issue:** #349
- **Parent:** Epic `folder-tree-breadcrumb-redesign` (child 9102, wave 1, band C4, `depends_on: [9101]`)
- **Owner:** drmoisan
- **Branch:** feature/efcviewer-breadcrumb-webview2-349 (cut from epic/folder-tree-breadcrumb-redesign-integration)
- **Last Updated:** 2026-07-17T00-30
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature

## Requirements Sources

- `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/spec.md` (authoritative AC)
- `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/user-story.md` (authoritative AC)
- `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/issue.md`
- `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/research/2026-07-16T22-30-efcviewer-breadcrumb-webview2-research.md`

**All work must comply with the repository policies (CLAUDE.md, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`). Do not duplicate their content here.**

## Upstream Dependency Note (9101 — live Outlook folder-hierarchy provider)

This feature CONSUMES the 9101 provider contract; it does not implement it. During epic execution
9101 merges into `epic/folder-tree-breadcrumb-redesign-integration` BEFORE this feature runs, so the
provider types (`IFolderHierarchyProvider`, `FolderSegmentInfo` with `FullPath`, `DisplayName`,
`HasSubfolders` — assumed shape per research §C.3) are expected to be present in `UtilitiesCS` at
execution time. Verified at planning time (2026-07-17): the types do NOT yet exist on this branch,
which is why Phase 0 contains a hard dependency gate (P0-T6) that records the actual merged surface
and HALTS execution if the provider is absent. If the merged shape deviates from §C.3, the single
re-alignment point is the row-builder/router input; P0-T6 records the actual namespace and member
shape that Phases 2 and 5 must code against. No hierarchy may be re-derived from suggestion rows.

## Scope Lock (files created / modified)

New host-neutral source (non-exempt, target >= 90% line coverage) — `UtilitiesCS` is a legacy
`packages.config` project; every new `.cs` file MUST be wired with an explicit `<Compile Include>`
item in `UtilitiesCS/UtilitiesCS.csproj` in the same task that creates it:
- CREATE `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSegment.cs`
- CREATE `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`
- CREATE `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs`
- CREATE `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs`
- CREATE `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs`
- CREATE `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`
- CREATE `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`

New QuickFiler source — `QuickFiler` is likewise legacy `packages.config` (explicit
`<Compile Include>` in `QuickFiler/QuickFiler.csproj` required per file):
- CREATE `QuickFiler/Viewers/IBreadcrumbWebHost.cs` (interface-only seam)
- CREATE `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs` (non-exempt, >= 90%)
- CREATE `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (non-exempt, >= 90%)
- CREATE `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` (coverage-exempt adapter, in-code justification)

New tests — both test projects are legacy `packages.config` (explicit `<Compile Include>` required):
- CREATE `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowBuilderTests.cs` (+ item in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`)
- CREATE `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs` (+ item)
- CREATE `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs` (+ item)
- CREATE `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs` (+ item)
- CREATE `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` (+ item in `QuickFiler.Test/QuickFiler.Test.csproj`)
- CREATE `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` (+ item)

Modified WinForms / controller (coverage-exempt, build + manual QA only):
- MODIFY `QuickFiler/Viewers/EfcViewer.cs` (temporary repro instrumentation added in Phase 1, removed in Phase 8; WebView2 control exposure)
- MODIFY `QuickFiler/Viewers/EfcViewer.Designer.cs` (TreeListView -> WebView2 swap, delete `olvColumnFolder`/`olvColumnPercent`)
- MODIFY `QuickFiler/Controllers/EfcFormController.cs` (wiring only — NO new testable logic; class stays wholly `[ExcludeFromCodeCoverage]`)
- MODIFY `QuickFiler/Viewers/EfcViewer3.Designer.cs` (mechanical Designer-only control swap; NO behavioral wiring — dead code per research §B.2)
- MODIFY `UtilitiesCS/UtilitiesCS.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj` (`<Compile Include>` wiring only)

Constraints applying to every new/touched file: net48-safe types only (no `record`, no `record struct`,
no `init` accessors — plain classes / `readonly struct` with explicit constructors); `#nullable enable`
in new files; no banned APIs (`DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`,
`Task.Delay`) — initialization waiting is gated on `CoreWebView2InitializationCompleted` plus the
pending-message queue, never a delay; every file stays under the 500-line cap (`EfcFormController.cs`
is pre-existing over-limit and must not materially grow); no new NuGet packages
(WebView2 1.0.3912.50 and Newtonsoft.Json 13.0.4 are already referenced where consumed —
Newtonsoft-consuming code lives ONLY in `UtilitiesCS`, which already references it; `QuickFiler`
gains no Newtonsoft reference).

## Evidence Location Invariant

All evidence artifacts are written ONLY under
`docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/<kind>/`
using kinds `baseline/`, `qa-gates/`, `regression-testing/`, `repro/`, and `other/`.
Writing to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or any other non-canonical
path is a policy violation. Timestamps use `yyyy-MM-ddTHH-mm`. (The Phase 9 JaCoCo export to
`artifacts/csharp/coverage.xml` is a machine-consumed feature-review gate input, not an evidence
artifact, and is the one permitted `artifacts/` output.)

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture, Policy Review, and 9101 Dependency Gate

- [ ] [P0-T1] Read the policy files in policy-compliance order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record the read in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact exists containing `Timestamp:`, `Policy Order:`, and the explicit list of the four files read.
- [ ] [P0-T2] Run `dotnet tool run csharpier . --check` (or `csharpier . --check`) at repo root and record the result in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/baseline/phase0-baseline-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P0-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/baseline/phase0-baseline-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (warning/error counts).
- [ ] [P0-T4] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record the result in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/baseline/phase0-baseline-nullable.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P0-T5] Run baseline tests with coverage via `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` and record the numeric baseline coverage headline in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/baseline/phase0-baseline-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric baseline line-coverage and branch-coverage percentages and total passed/failed test counts.
- [ ] [P0-T6] Verify the merged 9101 provider surface is present: search `UtilitiesCS/` for the `IFolderHierarchyProvider` interface and `FolderSegmentInfo` type (expected members per research §C.3: `GetAncestorChainAsync(string, CancellationToken)`, `GetImmediateSubfoldersAsync(string, CancellationToken)`, `FullPath`/`DisplayName`/`HasSubfolders`) and record the actual namespace, file path, and member shape in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/other/phase0-9101-provider-gate.md`; if the provider types are ABSENT, record `DEPENDENCY GATE FAILED: 9101 provider not merged`, HALT execution (do not start Phase 1), and report blocked state to the orchestrator
  - Acceptance: artifact contains `Timestamp:`, the search commands used, the resolved type locations and member shape (or the explicit gate-failure record); execution proceeds past Phase 0 only when both types are present.

### Phase 1 — Percentage-Obscuring Defect Runtime Reproduction

- [ ] [P1-T1] Add a temporary log4net diagnostic to `QuickFiler/Viewers/EfcViewer.cs` that logs, on `Form.Shown`, `FolderListBox.ClientSize.Width`, `olvColumnFolder.Width`, `olvColumnPercent.Width`, `CurrentAutoScaleDimensions`, and `DeviceDpi`, marked with the exact comment `// TEMP repro instrumentation (#349) — removed in P8-T3`
  - Acceptance: instrumentation compiles inside the already-exempt Form class; the marker comment is present; solution builds.
- [ ] [P1-T2] [expect-fail] Capture the runtime reproduction of the percentage-obscuring defect: launch the EfcViewer against live Outlook on the user's normal display and store (a) a screenshot of the suggestion list with the obscured/missing percent and (b) the P1-T1 diagnostic log line in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/repro/` as `percent-obscured-repro.<yyyy-MM-ddTHH-mm>.md` (embedding or referencing the sibling screenshot file); if a live Outlook session is structurally unavailable to the executor, instead write a fail-before exception dossier `fail-before-exception.<yyyy-MM-ddTHH-mm>.md` under `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/regression-testing/` with `WhyFailingRunImpossible:` and an alternative geometry proof (Designer widths vs expected runtime client width per research §D.2)
  - Acceptance: the repro artifact records `Timestamp:`, capture method, and the observed geometry values demonstrating the pre-fix defect (expected: `olvColumnFolder.Width (3200) > FolderListBox.ClientSize.Width`), OR the schema-valid exception dossier exists; the defect state is documented BEFORE any fix is applied.
- [ ] [P1-T3] Run the full C# toolchain loop in order (`csharpier .` -> `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` -> `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` -> `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`), restarting from step 1 on any failure or file change, and record the green pass in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/phase1-toolchain.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:` per step, `EXIT_CODE:` per step, `Output Summary:`; all four steps green in a single pass.

### Phase 2 — Breadcrumb Row Model and Collapse/Expand State Machine

- [ ] [P2-T1] Create `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSegment.cs` defining the pure segment model (`FullPath`, `DisplayName`, `HasSubfolders`; net48-safe class or `readonly struct` with explicit constructor, `#nullable enable`, no WinForms/COM/WebView2 types) and add a matching `<Compile Include="OutlookObjects\Folder\BreadcrumbSegment.cs" />` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file exists, csproj item present, `msbuild TaskMaster.sln` compiles the type into `UtilitiesCS.dll`.
- [ ] [P2-T2] Create `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` defining the per-row model and state machine: `RowId`, `Kind` enum `{ Suggestion, Banner, TrashPseudoRow }`, ordered `Segments`, `Probability` (`double?`), collapse-after-segment state (`CollapseAfter(int segmentIndex)` on a non-leaf segment hides all segments after it and marks the now-terminal segment with a re-expand affordance; `ReExpand()` restores the full breadcrumb), leaf expand state with a children list (`SetLeafChildren`, `ToggleLeafExpanded` valid only when the leaf `HasSubfolders`), `LeftArrow()`/`RightArrow()` transitions, `VisibleSegments()` projection, and documented no-op rules (banner and pseudo-rows never collapse/expand; leaf without subfolders is a toggle no-op); add the matching `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file exists (< 500 lines), csproj item present, compiles; transitions mutate only row view-state and never alter `Segments`, `Probability`, or the filing target path.
- [ ] [P2-T3] Create `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` that builds `BreadcrumbRow` instances from suggestion rows plus 9101 ancestor chains: maps an ordered root-to-leaf `FolderSegmentInfo` chain (actual 9101 type per the P0-T6 record) to `BreadcrumbSegment`s anchored at the predicted leaf, joins `Probability` by full-path equality, classifies `"===="`-prefixed rows as `Kind.Banner` (non-interactive) and the `"Trash to Delete"` pseudo-row as `Kind.TrashPseudoRow` (selectable, no segments/affordance), and preserves presented row order; add the matching `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file exists, csproj item present, compiles; builder consumes the 9101 segment type directly (no prefix-matching over suggestion rows anywhere in the file).
- [ ] [P2-T4] Create `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowBuilderTests.cs` (MSTest + FluentAssertions, Arrange–Act–Assert) covering: chain-to-row construction anchored at the leaf, probability join (matched, unmatched, null), banner classification, trash pseudo-row classification, empty chain, single-segment chain, and preserved row order; add the matching `<Compile Include>` item to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: file exists, csproj item present, all builder tests pass under `vstest.console.exe`.
- [ ] [P2-T5] Create `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs` covering: collapse-after-segment hides downstream segments and exposes the re-expand affordance at the now-terminal segment, re-expand restores the full breadcrumb, leaf toggle with/without subfolders (no-op when `HasSubfolders == false`), `LeftArrow`/`RightArrow` transitions and their no-op rules, banner/pseudo-row no-ops, `VisibleSegments()` projection for every state, and state-transition sequences (collapse -> re-expand -> leaf expand); add the matching `<Compile Include>` item to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: file exists, csproj item present, all state tests pass; positive, negative, edge, and state-transition scenarios are each present.
- [ ] [P2-T6] Run the full C# toolchain loop in order (`csharpier .` -> analyzers `msbuild` -> nullable `msbuild` -> `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`), restarting from step 1 on any failure or file change, and record the green pass in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/phase2-toolchain.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:` per step, `EXIT_CODE:` per step, `Output Summary:`; all four steps green in a single pass.

### Phase 3 — Bridge Message Contracts and JSON Codec

- [ ] [P3-T1] Create `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs` defining the net48-safe bridge contract types (plain classes with explicit constructors, `#nullable enable`): inbound `{ type: "segmentDoubleClick" | "leafExpandToggle" | "arrowKey" | "rowSelected", rowId, segmentIndex?, key? }` and outbound `{ type: "render" | "subfolderResult" | "focusSearch", requestId?, ... }` with `subfolderResult` carrying the correlating `requestId` and the child-segment payload; add the matching `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file exists (< 500 lines), csproj item present, compiles; no `record`/`init` usage.
- [ ] [P3-T2] Create `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` implementing serialize/deserialize over Newtonsoft.Json 13.0.4: `SerializeOutbound(...)` producing the discriminated JSON, and `DeserializeInbound(string json)` that fails fast on malformed JSON, unknown `type`, or missing required fields by throwing a specific exception after logging a specific log4net error (no silent swallow, no broad catch without rethrow); add the matching `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file exists, csproj item present, compiles in `UtilitiesCS` (the only project consuming Newtonsoft for this feature); malformed input raises the documented specific exception.
- [ ] [P3-T3] Create `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs` covering: JSON round-trip for every inbound and outbound message type, `requestId` correlation on `subfolderResult`, and malformed-input negatives (invalid JSON, unknown `type`, missing `rowId`, wrong field types) each asserting the specific exception via FluentAssertions; add the matching `<Compile Include>` item to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: file exists, csproj item present, all codec tests pass; every message type has a round-trip test and at least four malformed negatives exist.
- [ ] [P3-T4] Run the full C# toolchain loop in order (`csharpier .` -> analyzers `msbuild` -> nullable `msbuild` -> `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`), restarting from step 1 on any failure or file change, and record the green pass in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/phase3-toolchain.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:` per step, `EXIT_CODE:` per step, `Output Summary:`; all four steps green in a single pass.

### Phase 4 — Breadcrumb HTML Renderer with CSS Percentage-Visibility Fix

- [ ] [P4-T1] Create `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` holding the inline CSS and JS string constants for the generated document: the research §D.4 flex layout (`.row { display: flex; align-items: center; }`, `.crumb { flex: 1 1 auto; min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }`, `.pct { flex: 0 0 auto; margin-left: auto; white-space: nowrap; }`), dark/light theme CSS blocks, and the bridge JS (`window.chrome.webview.postMessage` emitters for segment double-click, leaf affordance activation, left/right/up/down arrow keys, and row selection; `window.chrome.webview.addEventListener('message', ...)` applying `render`/`subfolderResult` updates); add the matching `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file exists (< 500 lines), csproj item present, compiles; percent CSS class is a fixed non-shrinking flex item and only the crumb class may truncate.
- [ ] [P4-T2] Create `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` generating the full HTML document (and per-row update fragments) from `BreadcrumbRow` collections plus a theme flag, enforcing the renderer invariants: percent markup (via the existing `PercentageFormatter.FormatPercent`) is ALWAYS emitted as the trailing `.pct` flex item; the plus/minus affordance is emitted only when the relevant segment's `HasSubfolders` is true (plus when collapsed, minus when expanded); banner rows are rendered non-interactive (no handlers, no affordance); folder display names are HTML-encoded; collapsed rows render the re-expand plus at the now-terminal segment; add the matching `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file exists (< 500 lines), csproj item present, compiles; renderer consumes only the pure row model and `BreadcrumbDocumentAssets` (no I/O, no WebView2 types).
- [ ] [P4-T3] Create `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs` asserting the renderer invariants: trailing fixed percent item on every row (including collapsed rows), affordance gating on `HasSubfolders` (present/absent, plus/minus by state), HTML-encoding of hostile folder names (`<script>`, `&`, quotes), non-interactive banner markup, trash pseudo-row rendered selectable without affordance, dark vs light theme output, collapsed-state fragment output, and empty row list; add the matching `<Compile Include>` item to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: file exists, csproj item present, all renderer tests pass; the percent-trailing and encoding invariants each have explicit assertions.
- [ ] [P4-T4] Run the full C# toolchain loop in order (`csharpier .` -> analyzers `msbuild` -> nullable `msbuild` -> `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`), restarting from step 1 on any failure or file change, and record the green pass in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/phase4-toolchain.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:` per step, `EXIT_CODE:` per step, `Output Summary:`; all four steps green in a single pass.

### Phase 5 — Bridge Router and Host Seam

- [ ] [P5-T1] Create `QuickFiler/Viewers/IBreadcrumbWebHost.cs` defining the narrow host seam exactly per the spec (`void NavigateToString(string html)`, `void PostMessageJson(string json)`, `event EventHandler<string> MessageReceived`, `bool IsCoreInitialized`), `#nullable enable`, interface-only file; add a matching `<Compile Include>` item to `QuickFiler/QuickFiler.csproj`
  - Acceptance: file exists, csproj item present, compiles; interface has exactly the four spec members.
- [ ] [P5-T2] Create `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs` — a small pure class that buffers outbound bridge payloads while the host reports `IsCoreInitialized == false` and flushes them in order on initialization completion (no polling, no timers, no `Task.Delay`); add a matching `<Compile Include>` item to `QuickFiler/QuickFiler.csproj`
  - Acceptance: file exists (< 500 lines), csproj item present, compiles; class holds no WebView2 types (only the `IBreadcrumbWebHost` seam or plain delegates).
- [ ] [P5-T3] Create `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` — the non-exempt router constructed from the 9101 `IFolderHierarchyProvider` (actual type per P0-T6), `IBreadcrumbWebHost`, `BreadcrumbMessageCodec`, `BreadcrumbHtmlRenderer`, and `BreadcrumbOutboundQueue`, implementing: `BindRowsAsync` (build `BreadcrumbRow`s from suggestion rows via `GetAncestorChainAsync` + `BreadcrumbRowBuilder`, render, deliver via `NavigateToString`/`render` message); inbound routing for `segmentDoubleClick` (collapse-after-segment + re-render), `leafExpandToggle` (issue `GetImmediateSubfoldersAsync` with a correlated `requestId`, post `subfolderResult` on success; on provider failure or cancellation leave the row collapsed/state unchanged and log a specific error), `arrowKey` (Right expand / Left collapse; Up at the top row posts the outbound `focusSearch` message), and `rowSelected` (track the selected full path, never for banner rows); a `SelectedFolderPath` property plus selection-changed event for the controller; a `SelectFirstRow()` entry point for the SearchText down-arrow path; an `ApplyTheme(bool darkMode)` re-render entry point; and pre-initialization queueing of every outbound payload through `BreadcrumbOutboundQueue`; add a matching `<Compile Include>` item to `QuickFiler/QuickFiler.csproj`
  - Acceptance: file exists (< 500 lines), csproj item present, compiles; router contains no WebView2/WinForms/COM types (host reached only via `IBreadcrumbWebHost`) and derives no hierarchy from suggestion-row prefix matching.
- [ ] [P5-T4] Create `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` (MSTest + Moq + FluentAssertions) testing the router against `Mock<IFolderHierarchyProvider>` and `Mock<IBreadcrumbWebHost>`: bind-and-render delivers the generated document; `segmentDoubleClick` collapses and re-renders; `leafExpandToggle` issues the subfolder query and posts a `requestId`-correlated `subfolderResult`; `arrowKey` Right/Left transitions; Up-at-top posts `focusSearch`; `rowSelected` updates `SelectedFolderPath` and raises the event; banner rows are never selectable; `SelectFirstRow` posts the select-first payload; `ApplyTheme` re-renders with the dark document; add a matching `<Compile Include>` item to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: file exists (< 500 lines), csproj item present, all router happy-path/interaction tests pass.
- [ ] [P5-T5] Create `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` covering the negative/edge paths: outbound payloads issued while `IsCoreInitialized == false` are queued and flushed in order on initialization completion; provider failure (faulted task) leaves the row collapsed with state unchanged and logs; canceled provider call leaves state unchanged; malformed inbound JSON is rejected via the codec's specific exception without corrupting router state; duplicate initialization completion is idempotent (pooled-viewer re-init); add a matching `<Compile Include>` item to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: file exists, csproj item present, all queue/error-path tests pass; no test uses `Thread.Sleep`/`Task.Delay` or temp files.
- [ ] [P5-T6] Run the full C# toolchain loop in order (`csharpier .` -> analyzers `msbuild` -> nullable `msbuild` -> `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`), restarting from step 1 on any failure or file change, and record the green pass in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/phase5-toolchain.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:` per step, `EXIT_CODE:` per step, `Output Summary:`; all four steps green in a single pass.

### Phase 6 — WebView2 Host Adapter, Designer Swap, and Controller Wiring

- [ ] [P6-T1] Create `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` implementing `IBreadcrumbWebHost` as a 1:1 SDK-forwarding adapter over the Designer-owned `Microsoft.Web.WebView2.WinForms.WebView2` control, marked `[ExcludeFromCodeCoverage]` with an in-code justification (precedent `QuickFiler/Viewers/WebView2CoreInitializer.cs`): initialization through the existing `IWebViewCoreInitializer` seam awaiting `UiSyncContext` BEFORE `EnsureCoreWebView2Async` (pattern `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, cache folder `%LocalAppData%\WindowsFormsWebView2`), `IsCoreInitialized` driven by `CoreWebView2InitializationCompleted`, `MessageReceived` raised from `CoreWebView2.WebMessageReceived` (`e.WebMessageAsJson`), `PostMessageJson` forwarding to `PostWebMessageAsJson`, and idempotent event hookup safe for pooled-viewer re-initialization (`EfcViewerQueue`); add a matching `<Compile Include>` item to `QuickFiler/QuickFiler.csproj`
  - Acceptance: file exists, csproj item present, compiles; the attribute and justification comment are present; no polling or delay-based waiting appears.
- [ ] [P6-T2] Replace the `FolderListBox` `BrightIdeasSoftware.TreeListView` with a `Microsoft.Web.WebView2.WinForms.WebView2` control in the same TableLayoutPanel cell with `Tlp.SetColumnSpan(..., 14)` and `Dock = Fill` in `QuickFiler/Viewers/EfcViewer.Designer.cs`, deleting `olvColumnFolder`/`olvColumnPercent` and the TreeListView field declaration, and expose the new control to the controller from `QuickFiler/Viewers/EfcViewer.cs` (already-exempt Form)
  - Acceptance: both files compile; no `BrightIdeasSoftware` reference remains in `EfcViewer.Designer.cs`; the solution builds (`ObjectListView.Official` stays referenced for other viewers).
- [ ] [P6-T3] Rewire `QuickFiler/Controllers/EfcFormController.cs` (wiring-only; the class stays wholly `[ExcludeFromCodeCoverage]` and gains NO new testable logic) per the research §B.5 touch points: construct `WebView2BreadcrumbHost` + `BreadcrumbBridgeRouter` where `ConfigureFolderTreeView` wired the TreeListView; route `BindFolderRows` through `Router.BindRowsAsync` (including the `ActionDeleteAsync` `"Trash to Delete"` pseudo-row rebind and `RefreshSuggestionsAsync`/`SearchText_TextChanged` paths); derive `SelectedFolder` from `Router.SelectedFolderPath` keeping `IsValidSelection`'s `"===="` banner rejection intact; handle the router's `focusSearch` event by focusing `SearchText` (Up-at-top parity); make the `'F'` keyboard action and `SearchText_DownArrow` focus the WebView2 control (down-arrow additionally calling `Router.SelectFirstRow()`); and route `DarkMode_Changed` to `Router.ApplyTheme(...)`; remove the now-dead `FolderListBox_KeyDown`/`FolderListBox_SelectionChanged`/`FolderSuggestionTree` wiring from this controller
  - Acceptance: solution compiles; `EfcFormController.cs` contains delegation/wiring statements only (no new branching logic beyond guard clauses), does not materially grow, and no longer references `BrightIdeasSoftware` for the folder list.
- [ ] [P6-T4] Run the full C# toolchain loop in order (`csharpier .` -> analyzers `msbuild` -> nullable `msbuild` -> `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`), restarting from step 1 on any failure or file change, and record the green pass in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/phase6-toolchain.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:` per step, `EXIT_CODE:` per step, `Output Summary:`; all four steps green in a single pass (exempt wiring compiles; no host-neutral test regresses).

### Phase 7 — EfcViewer3 Mechanical Designer-Only Handling

- [ ] [P7-T1] Perform the mechanical Designer-only control swap in `QuickFiler/Viewers/EfcViewer3.Designer.cs`: replace the dead-code `FolderListBox` `BrightIdeasSoftware.TreeListView` with a `Microsoft.Web.WebView2.WinForms.WebView2` control in the same layout cell, delete its two `OLVColumn` declarations, and add NO event wiring, NO controller wiring, and NO behavioral code (EfcViewer3 is dead code — sole runtime instantiation of an Efc viewer is `new EfcViewer()` at `QuickFiler/Helper Classes/EfcViewerQueue.cs:83`); leave `QuickFiler/Viewers/EfcViewer3.cs` behaviorally untouched (compile-fix edits only if the field type change requires them)
  - Acceptance: `EfcViewer3.Designer.cs` compiles with the swapped control and no `BrightIdeasSoftware` folder-list reference; no new event subscriptions or handlers are added in either EfcViewer3 file.
- [ ] [P7-T2] Verify and record the no-behavioral-wiring invariant for EfcViewer3: search `QuickFiler/` for `EfcViewer3` construction sites and for controller/event references to its `FolderListBox`, and write the results to `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/other/efcviewer3-mechanical-swap-verification.md`
  - Acceptance: artifact contains `Timestamp:`, the search commands, and results confirming zero runtime construction of `EfcViewer3` and zero non-Designer wiring of its folder-list control.
- [ ] [P7-T3] Run the full C# toolchain loop in order (`csharpier .` -> analyzers `msbuild` -> nullable `msbuild` -> `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`), restarting from step 1 on any failure or file change, and record the green pass in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/phase7-toolchain.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:` per step, `EXIT_CODE:` per step, `Output Summary:`; all four steps green in a single pass.

### Phase 8 — Manual Verification, Instrumentation Removal, and AC Check-Off

- [ ] [P8-T1] Verify the percentage-visibility fix at runtime: launch the EfcViewer against live Outlook, resize to minimum form width, confirm the percent text node is fully within each row's client rect (JS-side rect check plus screenshot), and record the pass-after evidence in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/regression-testing/percent-visible-pass-after.<yyyy-MM-ddTHH-mm>.md` (with sibling screenshot); if a live Outlook session is unavailable, record the gap in the same artifact as remediation-required (manual verification outstanding) — do not record a pass
  - Acceptance: artifact contains `Timestamp:`, verification method, and either the observed pass (percent fully visible at minimum width, paired with the P1-T2 fail-before evidence) or an explicit remediation-required status.
- [ ] [P8-T2] Verify behavior parity at runtime against live Outlook and record the checklist in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/other/manual-parity-verification.<yyyy-MM-ddTHH-mm>.md`: Up-at-top focuses `SearchText`; SearchText down-arrow enters the list and selects the first row; `"Trash to Delete"` pseudo-row is selectable after delete; `"===="` banner rows are non-interactive and rejected as filing targets; `'F'` focuses the breadcrumb control; dark-mode toggle re-themes the document; leaf expand lists real Outlook subfolders (including one not among ranked suggestions); selection feeds filing via `SelectedFolder`; if a live session is unavailable, record remediation-required per item
  - Acceptance: artifact contains `Timestamp:` and a per-item PASS / remediation-required verdict for all eight parity items.
- [ ] [P8-T3] Remove the temporary repro instrumentation (the `// TEMP repro instrumentation (#349)` log line added by P1-T1) from `QuickFiler/Viewers/EfcViewer.cs`
  - Acceptance: a search for `TEMP repro instrumentation (#349)` across the repository returns zero hits; solution compiles.
- [ ] [P8-T4] Scan all new/touched C# files from the Scope Lock for banned APIs (`DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`) and record the search commands and zero-hit results in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/banned-api-scan.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with zero banned-API hits in new/touched code and tests.
- [ ] [P8-T5] Map every acceptance criterion in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/spec.md` and `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/user-story.md` to its verifying test, evidence artifact, or manual-verification record in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/other/ac-verification-map.md`
  - Acceptance: artifact lists each of the 12 ACs with a PASS/PARTIAL/BLOCKED verdict and a concrete evidence pointer (test name, evidence path, or manual-QA record).
- [ ] [P8-T6] Check off each verified acceptance criterion (per `acceptance-criteria-tracking`: evidence before check-off, one at a time, text preserved) independently in BOTH `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/spec.md` and `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/user-story.md`, leaving unmet items unchecked
  - Acceptance: every AC with a PASS verdict in the P8-T5 map is `[x]` in both files; PARTIAL/BLOCKED items remain `[ ]` with the gap documented in the map.

### Phase 9 — Final QA Loop and Coverage Verification

Run tasks P9-T1 through P9-T4 as one toolchain pass in order; if any step fails or changes any file, restart the loop from P9-T1 until all four pass in a single pass. These command tasks are unconditional — `SKIPPED` is not a valid outcome for any of them.

- [ ] [P9-T1] Run `dotnet tool run csharpier .` (or `csharpier .`) at repo root and record the result in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/phase9-final-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; no formatting changes remain (loop restarted if any file changed).
- [ ] [P9-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/phase9-final-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; zero analyzer errors.
- [ ] [P9-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record the result in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/phase9-final-nullable.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; zero nullable/warning-as-error failures.
- [ ] [P9-T4] Run `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` and record numeric post-change coverage in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/phase9-final-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric post-change line/branch coverage and per-file coverage for every new non-exempt module (`BreadcrumbSegment`, `BreadcrumbRow`, `BreadcrumbRowBuilder`, `BreadcrumbMessages`, `BreadcrumbMessageCodec`, `BreadcrumbDocumentAssets`, `BreadcrumbHtmlRenderer`, `BreadcrumbOutboundQueue`, `BreadcrumbBridgeRouter`).
- [ ] [P9-T5] Compute and record the coverage delta/threshold verification in `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/phase9-coverage-delta.md`, comparing baseline (P0-T5) vs post-change (P9-T4) and reporting new-code coverage per module
  - Acceptance: artifact records baseline coverage, post-change coverage, and new/changed-code coverage; every new non-exempt module meets `>= 90%` line coverage; the repository floor (`>= 80%` line per CLAUDE.md; plan to the stricter spec bars of 85% line / 75% branch where the baseline supports them) is not regressed; no changed line loses coverage; if any threshold is unmet the recorded outcome is remediation-required (never PASS).
- [ ] [P9-T6] Export the first-party post-change C# coverage as JaCoCo XML to `artifacts/csharp/coverage.xml` (feature-review gate input consumed by `scripts/`-side validation; NOT an evidence artifact) scoped to first-party assemblies
  - Acceptance: `artifacts/csharp/coverage.xml` exists with a JaCoCo root element and first-party scope; the numeric evidence of record remains `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/qa-gates/phase9-final-tests-coverage.md`.

## Acceptance-Criteria Traceability (spec.md §Definition of Done = user-story.md §Acceptance Criteria)

| # | Acceptance criterion (abbreviated) | Delivering / verifying tasks |
|---|---|---|
| 1 | Single-line breadcrumb per suggestion in live `EfcViewer` via WebView2 | P2-T2/T3, P4-T2, P6-T1/T2/T3, P8-T2 |
| 2 | Leaf-only plus/minus affordance gated on `HasSubfolders` | P2-T2, P4-T1/T2/T3, P8-T2 |
| 3 | Non-leaf double-click collapse + plus re-expand | P2-T2/T5, P4-T2/T3, P5-T3/T4, P8-T2 |
| 4 | Real immediate subfolders via 9101 provider; no prefix-matching | P0-T6, P2-T3, P5-T3/T4/T5, P8-T2 |
| 5 | Percent always visible; repro first, CSS fix after | P1-T1/T2, P4-T1/T2/T3, P8-T1 |
| 6 | JS<->.NET bridge (postMessage/WebMessageReceived; PostWebMessageAsJson/NavigateToString) | P3-T1/T2/T3, P5-T1/T2/T3/T4/T5, P6-T1 |
| 7 | EfcViewer3 mechanical Designer-only swap, no behavioral wiring | P7-T1/T2 |
| 8 | No third-party tree/list control, no WPF/ElementHost | P6-T2, P7-T1, P8-T5 (verified: no package/reference additions) |
| 9 | Scoring unchanged; feature-324 percent plumbing reused as-is | P2-T3, P4-T2 (uses `PercentageFormatter.FormatPercent`), P8-T5 |
| 10 | Behavior parity (focusSearch, trash row, banners, 'F', dark mode) | P2-T3, P5-T3/T4, P6-T3, P8-T2 |
| 11 | Unit tests (MSTest+Moq+FluentAssertions), >= 90% new modules, no new logic in `EfcFormController` | P2-T4/T5, P3-T3, P4-T3, P5-T4/T5, P6-T3, P9-T4/T5 |
| 12 | Full toolchain single pass; no banned APIs | P8-T4, P9-T1/T2/T3/T4 |

## Test Plan

- Unit (MSTest + Moq + FluentAssertions; host-neutral, deterministic, no temp files/COM/network/sleeps):
  - Row-builder construction from 9101 ancestor chains (anchored leaf, probability join, banner/trash kinds, empty/single-segment edges, order preservation).
  - Row state machine (collapse-after-segment, re-expand, leaf toggle gated on `HasSubfolders`, arrow transitions, banner/pseudo-row no-ops, `VisibleSegments()` projections, transition sequences).
  - Codec round-trips for every message type plus malformed negatives (invalid JSON, unknown type, missing/wrong-typed fields) with specific-exception assertions.
  - Renderer invariants (trailing fixed percent item, affordance gating, HTML-encoding, non-interactive banners, dark/light themes, collapsed fragments).
  - Router against `Mock<IFolderHierarchyProvider>` + `Mock<IBreadcrumbWebHost>` (bind/render, collapse, `requestId`-correlated subfolder query, arrows incl. `focusSearch`, selection, pre-init queue flush, provider failure/cancel, idempotent re-init).
- Build + manual QA (coverage-exempt): `WebView2BreadcrumbHost`, both Designer swaps, `EfcViewer` Form edits, `EfcFormController` wiring — verified by compilation plus the Phase 8 runtime checks.
- Runtime evidence: pre-fix percent-obscuring repro (`evidence/repro/`, [expect-fail], P1-T2); post-fix percent visibility and eight-item parity checklist (P8-T1/T2).
- Coverage evidence contract:
  - Baseline: `evidence/baseline/phase0-baseline-tests-coverage.md`
  - Post-change: `evidence/qa-gates/phase9-final-tests-coverage.md`
  - Delta/threshold: `evidence/qa-gates/phase9-coverage-delta.md`
  - Review-gate input (non-evidence): `artifacts/csharp/coverage.xml` (JaCoCo, first-party)

## Open Questions / Notes

- The 9101 contract shape used here (`IFolderHierarchyProvider` + `FolderSegmentInfo`) is the research §C.3 assumed surface; P0-T6 records the actual merged shape and is the single re-alignment point. If the merged shape deviates, the only permitted adaptation is a narrow mapping at the row-builder/router input (one adapter class), added under a plan revision — not ad-hoc scope growth.
- The leaf `HasSubfolders` must be answerable cheaply per rendered row (snapshot-backed); this dependency note is routed to the 9101 contract review per spec §Constraints.
- EfcViewer3 disposition is fixed to the mechanical Designer-only swap (not removal) for determinism; removal remains available to a future cleanup issue (the epic non-goal forbids unification, not removal).
- Newtonsoft-consuming types live only in `UtilitiesCS` so `QuickFiler` gains no new package reference; the router consumes typed codec outputs.
- `EfcFormController.cs` is pre-existing over the 500-line cap; this plan reduces it to wiring for the folder list and must not materially grow it. Dead code paths removed by P6-T3 (`FolderListBox_KeyDown` etc.) shrink it.
- Pooled-viewer lifecycle (`EfcViewerQueue`): host adapter event hookup is idempotent (P6-T1) and the router tolerates duplicate init-completion (P5-T5), matching the `cid:`-handler precedent of rebuild-at-request-time.
