# quickfiler-conversation-fast-list-empty (Plan)

- **Issue:** #255
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-07T12-46
- **Status:** Ready for executor preflight
- **Version:** 1.0
- **Work Mode:** minor-audit (small path, C# bug fix)

**Requirements source:** `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/issue.md`, section `## Acceptance Criteria` (AC1–AC5). This is the sole minor-audit AC source. `spec.md` and `user-story.md` are not required and must not be present in the active folder.

**Fail-closed evidence rule:** This plan includes explicit baseline artifact tasks, final-QA artifact tasks, and a coverage-comparison task for C# (coverage is mandatory per repo policy). If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing or incomplete, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Each evidence-producing task records its exact canonical artifact path under `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/<kind>/`. Do not mark an evidence-backed task complete without the artifact present and its required fields populated (`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`).

**Canonical evidence root (non-overridable):** `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/`. Any non-canonical evidence path (e.g., `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`) is rejected and replaced by the canonical `<FEATURE>/evidence/<kind>/` path.

---

## Confirmed Root Cause (verified by code reading)

**Symptom:** On expanding an item in the QuickFiler "Quick File" dialog, the conversation ("fast list" / `TopicThread`) panel shows `"The fast list is empty"` while the conversation count badge shows a non-zero value (8 in the screenshot).

**Confirmed cause — the deferred conversation-info UI publish is never triggered in the `loadAll == false` initialization path.**

Trace (file:line references):

1. The item viewer initializes conversation display at `QuickFiler/Controllers/QfcItemController.Initialization.cs:248` via `await PopulateConversationAsync(_tokenSource, Token, loadAll: false)`.
2. `PopulateConversationAsync` (`QuickFiler/Controllers/QfcItemController.Conversation.cs:94-109`) calls `LoadConversationResolverAsync` → `DoLoadConversationResolverCoreAsync` (`QfcItemController.Conversation.cs:79-92`) → `ConversationResolver.LoadAsync(..., loadAll: false, SetTopicThread)`.
3. In `ConversationResolver.LoadAsync` (MailItemHelper overload `QuickFiler/Helper Classes/ConversationResolver.cs:126-160`; the MailItem overload `ConversationResolver.cs:86-124` is identical in shape), the `loadAll == false` branch executes only `await resolver.LoadDfAsync(token, loadAll)` and then subscribes `resolver.PropertyChanged += resolver.Handler_PropertyChanged` (`ConversationResolver.cs:154-156`). It never calls `LoadConversationInfoAsync`.
4. `LoadConversationInfoAsync` (`QuickFiler/Helper Classes/ConversationResolver.Loading.cs:140-151`) is the **sole** code path that invokes `UpdateUI(pair.Expanded)` — i.e. `SetTopicThread` → `_itemViewer.SetConversationItems(...)` (`QuickFiler/Controllers/QfcItemController.Conversation.cs:207-219`) → `TopicThread.SetObjects(...)` (`QuickFiler/Viewers/ItemViewer.WebViewThread.cs:23`).
5. The intended deferred trigger — `Handler_PropertyChanged` reacting to the `Df` PropertyChanged event to run `BackgroundInitInfoItemsAsync` → `LoadConversationInfoAsync` (`ConversationResolver.Loading.cs:304-315` and `ConversationResolver.cs:210-226`) — cannot fire, because the `Df` assignment occurs inside `LoadDfAsync` (`ConversationResolver.Loading.cs:252`) **before** the handler is subscribed. This ordering is intentional per the inline comment at `ConversationResolver.cs:118` and `:154` ("Subscribe after LoadDfAsync so initial dataframe assignment does not trigger background initialization.").
6. The `UpdateUI` property-change branch of `Handler_PropertyChanged` (`ConversationResolver.Loading.cs:316-324`) is guarded by `if (FullyLoaded)`; `FullyLoaded` only becomes true after `BackgroundInitInfoItemsAsync` completes (which never runs), and `UpdateUI` is assigned before subscription (`ConversationResolver.cs:142-143`), so this path is also dead in the `loadAll == false` flow.

**Net effect:** In the `loadAll == false` path, `TopicThread` is never populated, so it renders `EmptyListMsg = "The fast list is empty"` (`QuickFiler/Viewers/ItemViewer.Designer.cs:417`). The count badge is populated independently from `Count.SameFolder` through `RenderConversationCountAsync` (`QfcItemController.Conversation.cs:104-108, 180-205`), which reads `Df` directly and shows the true count. This is the reported divergence (count 8, list empty). This corresponds to suspected cause (a): async UI publish ordering vs. viewer binding — specifically, the deferred publish is never triggered.

**Not the cause (ruled out by reading):** the `SentOn != ""` filter and `FilterConversation` filters in `LoadDfAsync` (`ConversationResolver.Loading.cs:246-250`) reduce `Df.Expanded`/`Df.SameFolder` together; since `Count.SameFolder` (the badge) is a subset of `Df.Expanded`, a non-zero badge implies `Df.Expanded` is non-empty, so the empty list is not caused by the dataframe filters. The genuinely-empty behavior (single-item fallback + Junk E-mail path) lives in `LoadConversationInfo` (`ConversationResolver.Loading.cs:37-73`) and must be preserved.

## Fix Scope (target 1–3 production files, confined to the conversation-display pipeline)

Primary candidate (resolver-level, preferred): trigger the deferred conversation-info load/publish in the `loadAll == false` branch of `ConversationResolver.LoadAsync` so that `LoadConversationInfoAsync` runs and publishes `UpdateUI(pair.Expanded)`.
- `QuickFiler/Helper Classes/ConversationResolver.cs` (the two `LoadAsync` `loadAll == false` branches), and/or
- `QuickFiler/Helper Classes/ConversationResolver.Loading.cs` (publish/handler path).

Alternative candidate (controller-level): after the resolver loads in the `loadAll == false` path, publish the resolved conversation info to the fast list through the controller's testable `SetTopicThread`/`_uiDispatcher` seam in `QuickFiler/Controllers/QfcItemController.Conversation.cs` (`PopulateConversationAsync`).

The implementation engineer selects the smallest change that fixes the confirmed cause while preserving the genuinely-empty behavior. Do not perform unrelated refactors.

## Regression Test Seam (deterministic, no live Outlook, no temp files)

Add the regression test to an **already-wired** test file so no `QuickFiler.Test.csproj` `<Compile Include>` change is needed:
- Controller-level (preferred, avoids the static `UiThread.Dispatcher`): `QuickFiler.Test/Controllers/QfcItemController.ConversationTests.cs`, reusing the existing `SeamController` / `ViewerController` and `QfcItemControllerTestSupport.BuildSyncDispatcher()` harness. Drive `PopulateConversationAsync(loadAll: false)` with a seam-returned resolver whose `ConversationInfo.Expanded` is a multi-item list and assert `IItemViewer.SetConversationItems(...)` is invoked with a non-empty list (fails before the fix, passes after).
- Resolver-level alternative: `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs`, reusing the existing COM-mock helpers (`CreateMailItem`, `CreateConversationTable`, `CreateResolverGlobals`). If exercising `UpdateUI` at the resolver level, the test must remain deterministic and must not depend on the static WPF `UiThread.Dispatcher` (which is null in unit tests); keep the smallest seam necessary.

If, contrary to the above, a new test file is created, it MUST be wired with an explicit `<Compile Include="...">` entry in `QuickFiler.Test/QuickFiler.Test.csproj` (legacy `packages.config` project with no source globbing).

Test constraints (repo policy): MSTest (`[TestClass]`/`[TestMethod]`), Moq, FluentAssertions; no live Outlook process, no `BackgroundWorker`/real form, no temp files, deterministic.

## Acceptance Criteria Mapping

- **AC1** (fast list populated on expand, not "empty"): P1-T2, P1-T3, P2-T4.
- **AC2** (row count consistent; empty message only when genuinely empty): P1-T2, P1-T4.
- **AC3** (root cause documented + deterministic MSTest/Moq/FluentAssertions regression, fail-before/pass-after, no live Outlook/temp files): Root Cause section above, P1-T1, P1-T2, P1-T5, P2-T4.
- **AC4** (fix confined to pipeline, no unrelated refactors, preserve genuinely-empty case): P1-T3, P1-T4.
- **AC5** (full C# toolchain passes; coverage on changed lines not regressed): P0-T5, P2-T1, P2-T2, P2-T3, P2-T4, P2-T5.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read policy files in required order and record `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/baseline/phase0-instructions-read.md` with fields `Timestamp:`, `Policy Order:`, and the explicit list of files read: `CLAUDE.md`; `.claude/rules/general-code-change.md`; `.claude/rules/general-unit-test.md`; `.claude/rules/csharp.md`; `.claude/skills/atomic-plan-contract/SKILL.md`; `.claude/skills/acceptance-criteria-tracking/SKILL.md`; `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Binary outcome: artifact exists with all three fields populated.
- [x] [P0-T2] Record branch and commit baseline (current branch name and `HEAD` SHA) in `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/baseline/baseline-branch-commit.md` with `Timestamp:`. Binary outcome: artifact exists with branch and commit SHA.
- [x] [P0-T3] Capture CSharpier formatting baseline. Command: `dotnet tool run csharpier --check .`. Write `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/baseline/baseline-csharpier.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Binary outcome: artifact exists with all four fields.
- [x] [P0-T4] Capture analyzer + nullable build baseline. Commands: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/baseline/baseline-build.md` with `Timestamp:`, `Command:` (both commands), `EXIT_CODE:` (per command), `Output Summary:` (warning/error counts). Binary outcome: artifact exists with all four fields.
- [x] [P0-T5] Capture MSTest coverage baseline for the QuickFiler test assembly. Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`. Write `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/baseline/baseline-tests-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric headline values: total pass/fail counts, repository/assembly line coverage percent, and baseline coverage percent for the files in Fix Scope (`ConversationResolver.cs`, `ConversationResolver.Loading.cs`, `QfcItemController.Conversation.cs`). Binary outcome: artifact exists with numeric coverage values (no placeholders).

### Phase 1 — Constrained Small-Path Implementation

- [x] [P1-T1] Record the confirmed root cause (as verified above, with file:line references) in `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/regression-testing/root-cause.md` with `Timestamp:`. Binary outcome: artifact exists and names the `loadAll == false` deferred-publish gap in `ConversationResolver.LoadAsync` (`ConversationResolver.cs:126-160` / `:86-124`) and the sole publisher `LoadConversationInfoAsync` (`ConversationResolver.Loading.cs:140-151`).
- [x] [P1-T2] [expect-fail] Add a single deterministic regression test (MSTest + Moq + FluentAssertions) in an already-wired test file per the Regression Test Seam section. The test drives the `loadAll == false` conversation-population path for a multi-item conversation and asserts the fast list is populated with a non-empty list (controller-level: `IItemViewer.SetConversationItems(...)` invoked with a non-empty `IList`; resolver-level: the `UpdateUI`/info-load publish occurs). The test must not touch a live Outlook process, `BackgroundWorker`, a real form, the static `UiThread.Dispatcher`, or temp files. Binary outcome: exactly one new `[TestMethod]` added in an already-wired file (no new file, or if unavoidable, csproj `<Compile Include>` added).
- [x] [P1-T3] [expect-fail] Build the test assembly and run the new regression test to confirm it FAILS against pre-fix behavior. Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:<NewTestMethodName>`. Write `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/regression-testing/regression-fail-before.md` with `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), `Output Summary:` (the failing assertion showing the fast list was not populated). Binary outcome: fail-before artifact exists showing the test failed for the expected reason.
- [x] [P1-T4] Apply the minimal fix confined to the conversation-display pipeline (1–3 files from the Fix Scope) so the `loadAll == false` path triggers the conversation-info load and `UpdateUI`/`SetTopicThread` publish, while preserving the genuinely-empty behavior (single-item fallback and Junk E-mail path in `LoadConversationInfo`, `ConversationResolver.Loading.cs:37-73`). No unrelated refactors. Binary outcome: only Fix-Scope production files are modified; the genuinely-empty path is unchanged in behavior.
- [x] [P1-T5] Re-run the regression test to confirm it PASSES after the fix. Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:<NewTestMethodName>`. Write `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/regression-testing/regression-pass-after.md` with `Timestamp:`, `Command:`, `EXIT_CODE:` (0), `Output Summary:` (test passed; fast list populated). Binary outcome: pass-after artifact exists showing the test passed.

### Phase 2 — Final QC Loop

- [x] [P2-T1] Run CSharpier formatting. Command: `dotnet tool run csharpier .`. Write `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/qa-gates/qc-csharpier.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. If files change, restart the loop from P2-T1. Binary outcome: artifact exists; formatting clean in the final pass.
- [x] [P2-T2] Run analyzer build. Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/qa-gates/qc-analyzers.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (analyzer diagnostic counts). Binary outcome: artifact exists; `EXIT_CODE: 0`.
- [x] [P2-T3] Run nullable type-check build. Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/qa-gates/qc-nullable.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Binary outcome: artifact exists; `EXIT_CODE: 0`.
- [x] [P2-T4] Run the full MSTest suite with coverage. Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`. Write `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/qa-gates/qc-tests-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric post-change values: total pass/fail counts and line coverage percent for the Fix-Scope files. Binary outcome: artifact exists with numeric coverage values; all tests pass. If any step P2-T1..P2-T4 changed files or failed, restart the loop from P2-T1.
- [x] [P2-T5] Verify coverage delta. Compare baseline (P0-T5) vs post-change (P2-T4) for the Fix-Scope files and changed lines. Write `docs/features/active/2026-07-07-quickfiler-conversation-fast-list-empty-255/evidence/qa-gates/coverage-delta.md` with `Timestamp:`, `Command:` (reference to P0-T5 and P2-T4 artifacts), `EXIT_CODE:`, `Output Summary:` reporting: baseline coverage percent, post-change coverage percent, and changed-line coverage. Binary outcome: artifact exists and confirms no coverage regression on changed lines (per `.claude/rules/csharp.md` and CLAUDE.md coverage policy); otherwise the outcome is remediation-required, not PASS.
