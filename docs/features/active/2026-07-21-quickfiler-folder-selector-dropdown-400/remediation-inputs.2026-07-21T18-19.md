# Remediation Inputs: QuickFiler Folder Selector Drop-Down (#400)

**Timestamp:** 2026-07-21T18-19Z
**Authoritative base:** main at df5ad49c909f6b739edef45d0336151f44e827a6
**Reviewed head:** bug/quickfiler-folder-selector-dropdown-400 at b38a87751669f3522928dd01ac0f4f97b82572ed
**Work mode:** full-bug
**Review outcome:** REMEDIATION_REQUIRED

## Authoritative inputs

1. docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md
2. docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/plan.2026-07-21T10-41.md
3. docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/policy-audit.2026-07-21T18-19.md
4. docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/code-review.2026-07-21T18-19.md
5. docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/feature-audit.2026-07-21T18-19.md
6. artifacts/pr_context.summary.txt
7. artifacts/pr_context.appendix.txt
8. docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-delta.2026-07-21T17-49.md
9. docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-accounting-scope-change.2026-07-21T18-01.md

## Required fixes

### 1. Gate popup attachment and initial replay on document readiness

**Primary files:**

- QuickFiler/Viewers/BreadcrumbDropDownHost.cs
- QuickFiler/Viewers/ItemViewer.Breadcrumb.cs, only if the existing ready-event contract must change
- QuickFiler/Viewers/BreadcrumbMessengerHub.cs, only if replay ownership must change
- QuickFiler/Resources/FolderBreadcrumb.html, only if an explicit page-ready protocol is selected
- Corresponding focused QuickFiler.Test/Viewers test files

**Defect:** CreateProductionSurfaceAsync calls NavigateToString and returns WebView2Messenger immediately. PopupMessengerReady then causes cached state replay before the popup document is known to have registered its window.chrome.webview message listener.

**Expected behavior:**

1. A popup messenger is not exposed, attached, replayed, shown, or focused until the specific NavigateToString document is ready to receive host messages.
2. The first open receives the latest cached render, theme, and selector state exactly once per message type.
3. Navigation failure or disposal before readiness closes the selector uncommitted, restores the original selection, disposes partial resources, returns focus once, and does not attach a messenger.
4. Reopen after a successful initialization reuses the one ready surface and does not replay duplicate subscriptions.
5. The closed-surface behavior is not regressed. If the same readiness defect is confirmed in its existing setup, apply the same bounded readiness contract there and cover it deterministically.

**Required deterministic verification:**

- Add a failure-first test that keeps a readiness/factory task incomplete and proves PopupMessengerReady, cached PostJson calls, show, and focus do not occur before completion.
- Complete readiness and prove one attach, one render, one theme, one selector-state replay, one show, and one focus action.
- Complete readiness with failure and prove rollback, cleanup, and no attachment/callback duplication.
- Do not require a live browser, display, user interaction, wall-clock delay, temporary file, network, or external process.

### 2. Serialize and invalidate asynchronous popup surface creation

**Primary files:**

- QuickFiler/Viewers/BreadcrumbDropDownHost.cs
- QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleTests.cs
- QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs or BreadcrumbDropDownIntegrationTests.cs when composition assertions are required

**Defect:** EnsureSurfaceAsync checks fields before awaiting the factory but has no shared in-flight task, lifecycle generation, cancellation, or post-await reset/disposal guard.

**Expected behavior:**

1. Concurrent OpenAsync calls share exactly one initialization operation and create at most one surface/ToolStripControlHost.
2. Reset invalidates a pending operation. Its later completion is disposed and cannot attach, raise PopupMessengerReady, show, focus, or call selection callbacks for the reset lifecycle.
3. Dispose invalidates a pending operation. Its later success or failure performs bounded cleanup without mutating disposed host state or invoking callbacks.
4. A new open after Reset starts exactly one fresh initialization and can succeed.
5. Initialization failure remains observable through LastInitializationException for the current lifecycle, without a stale failure overwriting a later successful lifecycle.
6. All operations remain deterministic on the expected WinForms synchronization context; do not add blocking waits or sleep-based coordination.

**Required deterministic verification:**

- Use TaskCompletionSource-controlled factories for concurrent open, reset-before-completion, dispose-before-completion, stale failure, and fresh-open-after-reset cases.
- Assert factory counts, host item counts, messenger-ready counts, disposal counts, show/focus/cancel counts, and final IsOpen/PopupMessenger/LastInitializationException state.

### 3. Complete missing semantic composition tests

**Primary files:**

- QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs
- QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs
- QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs only for static contract changes

**Expected behavior and tests:**

1. Open the selector, move pending selection away from original, simulate native outside/automatic close, and prove original committed identity is restored with zero SelectionChanged publication for the pending row and one focus return.
2. Open a populated selector and route selectorKey up through the inbound coordinator path. Prove pending-only movement, separator skipping, first-selectable clamping, no committed change, and no duplicate transition.
3. Preserve existing Left/Right, Enter, Escape, mouse activation, probability, issue #398 atomicity, and invalid-message behavior.
4. Static HTML token tests may remain for accessibility/markup contracts, but they must not be cited as runtime proof of document readiness or DOM focus.

### 4. Reconcile coverage accounting without weakening policy

**Primary artifacts/documents:**

- spec.md AC-18
- remediation plan
- new final coverage-delta and acceptance-verification evidence

**Expected behavior:**

1. Preserve repository-wide line coverage at or above 80%, changed-line no-regression, and at least 90% for all measurable new/changed selector types and members.
2. Do not widen class-level exclusions, modify coverage.config, or move host-neutral logic under an exclusion.
3. Keep direct WebView2/WinForms adapter methods limited to direct third-party calls and readiness coordination that cannot be deterministically executed as live UI under the unit-test policy.
4. Reconcile the literal AC-18 wording with the already recorded scope_change by incorporating the bounded nonnumeric adapter rule into the authoritative requirement and plan. Do not lower any numeric threshold or add an exception/waiver.
5. Produce numeric baseline, post-change, changed/new-line, per-type, and per-measurable-member evidence. Enumerate every nonnumeric surface and its deterministic seam tests.
6. If the implementation makes any excluded adapter contain host-neutral logic, move that logic to an instrumentable type and meet the numeric threshold there.

### 5. Re-run final QA and acceptance reconciliation

Run one uninterrupted final C# toolchain pass:

1. csharpier format .
2. msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
3. msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
4. pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final.<UTC-minute>.cobertura.xml

If any command fails or changes files, restart at formatting and write a fresh complete final-pass evidence set. Evidence files must contain Timestamp, Command, numeric EXIT_CODE, and Output Summary. Recompute the coverage delta against df5ad49c909f6b739edef45d0336151f44e827a6.

After the final pass, reevaluate all 19 criteria. Only PASS criteria may be checked in spec.md. AC-6, AC-7, AC-12, AC-13, AC-14, AC-15, AC-16, AC-18, and AC-19 remain unchecked until their audit findings are fully resolved and evidenced.

## Structural and evidence constraints

- Preserve the full-bug work mode and the complete issue #400 semantic contract.
- Use MSTest, FluentAssertions, and Moq or focused fakes at external boundaries.
- No modified production or test source file may exceed 500 lines.
- Every new C# file must have exactly one legacy project Compile include.
- Store all new execution evidence under docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/baseline, evidence/regression-testing, or evidence/qa-gates as appropriate.
- Use actual UTC-minute timestamps in evidence filenames.
- Include failure-first evidence for each confirmed defect before implementing its fix.
- Keep each implementation batch within the repository C# change budget and delegate through the typed C# engineer route required by the orchestrator.

## Do not do

1. Do not modify production or tests outside the folder-selector, breadcrumb host, or directly affected test/project surfaces.
2. Do not add a live UI, manual validation, screenshot, sleep, temporary file, network, Outlook, or external-process acceptance step.
3. Do not weaken coverage thresholds, widen exclusions, change coverage filters, or silently omit nonnumeric methods.
4. Do not treat source-string HTML tests as proof of runtime listener ordering or focus.
5. Do not suppress analyzer, nullable, test, or coverage failures.
6. Do not mark a task, acceptance criterion, or QA command complete when its evidence is missing or contradictory.
7. Do not replace the breadcrumb architecture, add packages/settings, or change public IItemViewer signatures.
8. Do not silently skip any planned command.

## Completion gate

Remediation is complete only after the plan is executed, all required tests and ordered QA gates pass, updated numeric coverage evidence satisfies the reconciled requirements, the nine currently unchecked criteria are reevaluated from current evidence, all required artifacts validate, and independent feature review returns PASS.
