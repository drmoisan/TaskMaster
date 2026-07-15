# quickfiler-inline-image-cid-fix (Plan)

- **Issue:** #326
- **Parent (optional):** none (child of epic `folder-tree-percentage-ui`, manifest child 9004)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-15T16-53
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-bug (spec.md required and present; user-story.md absent by design; enforces
  spec-driven expectations and the full QA loop per `atomic-plan-contract`)
- **Feature folder (`<FEATURE>`):** `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326`
- **Workspace root:** `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a1e77dc4a849cd790`
  (all commands below are run from this root unless otherwise noted; `<TS>` placeholders MUST be
  substituted with the real ISO-8601 timestamp (`yyyy-MM-ddTHH-mm`) at the moment each artifact is
  written, per `evidence-and-timestamp-conventions`).
- **AC source:** `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/spec.md`
  `## Acceptance Criteria` (sole AC source for this full-bug plan).

## Evidence location note

All evidence artifacts in this plan resolve to
`docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/<kind>/` per the
Non-Overridable Evidence Path Clause in `evidence-and-timestamp-conventions`. No non-canonical path
(e.g. `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`) is used anywhere in this plan.
If any upstream instruction had supplied a non-canonical evidence path, this planner would reject it
and record `EVIDENCE_LOCATION_OVERRIDE_REJECTED: <supplied path> replaced with <canonical path>`; no
such override was supplied in this delegation, so no rejection record is required.

**Fail-closed evidence rule:** If any required baseline artifact, regression artifact, or QA artifact
is missing or incomplete, the plan's overall outcome MUST be treated as remediation-required, never
PASS.

## Production files in scope (per spec.md §Proposed Fix)

1. `UtilitiesCS/Interfaces/IEmailIntelligence/IAttachment.cs` — additive `ContentId` property.
2. `UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs` — populate `ContentId`.
3. `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs` — **new file**, host-neutral pure logic.
4. `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs` — invoke the rewrite inside `GetHtml()`.
5. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` — extend the already-`[ExcludeFromCodeCoverage]`
   `InitializeWebViewAsync` with `AddWebResourceRequestedFilter` + `WebResourceRequested` glue.

No other production file may be touched. `QfcItemController.EventWiring.cs`'s
`_itemViewer.NavigateToString(ItemHelper.Html)` call site/signature, `EfcViewer.cs`, `EfcViewer3.cs`,
`CboFolders`, any `QfcItemViewer*` Designer file, `FolderScorer`, and `FolderPredictor` are explicitly
out of scope (spec.md §Scope & Non-Goals).

---

### Phase 0 — Baseline Capture & Policy Read

- [ ] [P0-T1] Read `CLAUDE.md` in full (policy reading order position 1) at
      `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a1e77dc4a849cd790/CLAUDE.md`.
      Acceptance: file read in this execution session; its Policy Compliance Order section is quoted
      verbatim in the Phase 0 evidence artifact produced by P0-T5.
- [ ] [P0-T2] Read `.claude/rules/general-code-change.md` (policy reading order position 2).
      Acceptance: file read; its Mandatory Toolchain Loop section quoted in the P0-T5 evidence
      artifact.
- [ ] [P0-T3] Read `.claude/rules/general-unit-test.md` (policy reading order position 3).
      Acceptance: file read; its Coverage Requirements section quoted in the P0-T5 evidence artifact.
- [ ] [P0-T4] Read `.claude/rules/csharp.md` (policy reading order position 4, C#-specific).
      Acceptance: file read; its Toolchain section quoted in the P0-T5 evidence artifact.
- [ ] [P0-T5] Write the Phase 0 policy-read evidence artifact to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/baseline/phase0-instructions-read.md`
      containing at minimum `Timestamp:`, `Policy Order:` (the 4-item list from P0-T1..P0-T4 in
      order), and an explicit list of the four file paths read. Acceptance: the file exists at the
      exact path above and contains all three required fields.
- [ ] [P0-T6] Record baseline git state (current branch name and `HEAD` short SHA via
      `git rev-parse --abbrev-ref HEAD` and `git rev-parse --short HEAD`) to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/baseline/git-baseline-state.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:`
      line stating the branch name and SHA.
- [ ] [P0-T7] Run the baseline CSharpier format check: `dotnet tool run csharpier check .` (repo
      root). Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/baseline/baseline-csharpier-check.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass
      or the count of files needing formatting).
- [ ] [P0-T8] Run the baseline analyzer build:
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
      Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/baseline/baseline-analyzer-build.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build
      succeeded/failed, warning/error counts).
- [ ] [P0-T9] Run the baseline nullable build:
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
      Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/baseline/baseline-nullable-build.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P0-T10] Run the baseline full test pass with coverage:
      `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`.
      Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/baseline/baseline-test-coverage.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:`
      line with the numeric total test pass/fail counts and the numeric baseline line-coverage
      percentage for `UtilitiesCS.dll` and `QuickFiler.dll`.
- [ ] [P0-T11] Grep the repository for other production implementers of `IAttachment` (pattern
      `: IAttachment` across `**/*.cs`, excluding `UtilitiesCS.Test/**` and `QuickFiler.Test/**`) and
      record the result to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/baseline/iattachment-implementer-scan.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:` (the exact grep pattern/scope), and
      `Output Summary:` stating the count and names of production implementers found (expected: only
      `AttachmentSerializable`).

---

### Phase 1 — Regression Test First (must fail before the fix)

- [ ] [P1-T1] [expect-fail] Write
      `UtilitiesCS.Test/OutlookObjects/MailItem/CidImageResolverTests.cs`, namespace
      `UtilitiesCS.Test.OutlookObjects.MailItem`, `[TestClass]`, containing exactly these three
      `[TestMethod]`s using MSTest + FluentAssertions (no Moq required — `IAttachment` fakes are
      created via `new AttachmentSerializable() { ContentId = ..., AttachmentData = ... }`):
      1. `RewriteCidReferences_ShouldRewriteMatchedContentId` — Arrange an HTML string containing
         `<img src="cid:logo1">` and an `AttachmentSerializable` instance with
         `ContentId = "logo1"`; Act by calling
         `CidImageResolver.RewriteCidReferences(html, new IAttachment[] { attachment }, "cid.quickfiler.local")`;
         Assert the result `.Should().Contain("src=\"https://cid.quickfiler.local/logo1\"")` and
         `.Should().NotContain("cid:logo1")`.
      2. `RewriteCidReferences_ShouldLeaveUnmatchedContentIdUnchanged` — Arrange HTML containing
         `<img src="cid:unknown">` with no attachment whose `ContentId` matches; Assert the result
         `.Should().Contain("cid:unknown")` unchanged.
      3. `BuildContentIdMap_ShouldReturnCaseInsensitiveMapExcludingEmptyContentId` — Arrange a
         collection of `AttachmentSerializable` fakes with `ContentId` values `"LOGO1"`, `""`, and
         `null`; Act by calling `CidImageResolver.BuildContentIdMap(...)`; Assert the returned map
         contains exactly one entry keyed `"logo1"` (case-insensitive lookup on `"LOGO1"` succeeds)
         and excludes the empty/null entries.
      Acceptance: the file exists at the exact path above and contains exactly these three named
      `[TestMethod]`s.
- [ ] [P1-T2] Edit `UtilitiesCS.Test/UtilitiesCS.Test.csproj` to insert
      `<Compile Include="OutlookObjects\MailItem\CidImageResolverTests.cs" />` (this repo's legacy
      `packages.config`-based csproj requires explicit `<Compile Include>` wiring; there is no glob).
      Acceptance: the csproj contains the new `<Compile Include>` line exactly once.
- [ ] [P1-T3] [expect-fail] Run
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` and confirm the
      build fails, with compiler errors referencing the not-yet-existing `CidImageResolver` type and
      the not-yet-existing `IAttachment.ContentId` / `AttachmentSerializable.ContentId` member.
      Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/regression-testing/fail-before-cid-resolver.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, a non-zero `EXIT_CODE:`, and
      `Output Summary:` quoting the specific `CS0246`/`CS0117`-class compiler errors naming
      `CidImageResolver` and `ContentId`. This is the auditable fail-before evidence for AC-1
      through AC-3.

---

### Phase 2 — Minimal Targeted Fix

- [ ] [P2-T1] Edit `UtilitiesCS/Interfaces/IEmailIntelligence/IAttachment.cs` to add
      `string ContentId { get; set; }` to the `IAttachment` interface, alphabetically positioned
      between `OlObjectClass Class { get; set; }` and `string DisplayName { get; set; }` per the existing (fully alphabetical) member ordering.
      Acceptance: the interface declares exactly one new member, `string ContentId { get; set; }`,
      and no existing member signature changes.
- [ ] [P2-T2] Edit `UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs` to add a public
      auto-implemented `string ContentId { get; set; }` property (in the "Serialized Standard
      Attachment Properties" region) and populate it inside the
      `AttachmentSerializable(Attachment a, bool imageBytesOnly = true)` constructor via a new
      private helper method `TryFromContentIdAccessor(Attachment attachment, out string contentId)`
      that wraps `attachment.PropertyAccessor.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x3712001F")`
      in try/catch (`catch (System.Exception)`), defaulting `ContentId` to `null` on failure, mirroring
      the existing `TryFromAccessor` pattern for `PR_ATTACH_DATA_BIN`. Acceptance:
      `AttachmentSerializable` compiles, exposes a settable `ContentId` property, and the constructor
      populates it from the named proptag with a try/catch default to `null` on read failure.
- [ ] [P2-T3] Add `[TestMethod] ContentId_ShouldPopulateFromMockedPropertyAccessor_WhenPropertyPresent`
      to `UtilitiesCS.Test/OutlookObjects/Attachment/AttachmentSerializableTests.cs`, mocking
      `Mock<PropertyAccessor>().Setup(x => x.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x3712001F")).Returns("logo1")`
      and asserting the constructed `AttachmentSerializable.ContentId` equals `"logo1"`. Acceptance:
      the test method exists and, once P2-T2 lands, passes.
- [ ] [P2-T4] Add `[TestMethod] ContentId_ShouldDefaultToNull_WhenPropertyAccessorThrows` to the same
      test file, mocking `Mock<PropertyAccessor>().Setup(x => x.GetProperty(It.IsAny<string>())).Throws<System.Runtime.InteropServices.COMException>()`
      and asserting the constructed `AttachmentSerializable.ContentId` is `null`. Acceptance: the test
      method exists and, once P2-T2 lands, passes.
- [ ] [P2-T5] Create `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs`, namespace
      `UtilitiesCS`, `internal static class CidImageResolver` (host-neutral, no COM/WebView2 types),
      exposing:
      - `public const string DefaultVirtualHost = "cid.quickfiler.local";`
      - `public static IReadOnlyDictionary<string, IAttachment> BuildContentIdMap(IReadOnlyCollection<IAttachment> attachments)`
        returning a `Dictionary<string, IAttachment>(StringComparer.OrdinalIgnoreCase)` keyed by each
        attachment's non-null/non-empty `ContentId`.
      - `public static string RewriteCidReferences(string html, IReadOnlyCollection<IAttachment> attachments, string virtualHost)`
        using `Regex.Replace` with pattern `src=(['"])cid:([^'"]+)\1` (case-insensitive,
        `RegexOptions.IgnoreCase`) and a `MatchEvaluator` that looks up the captured id in
        `BuildContentIdMap(attachments)`; on a match, replaces with
        `src="https://{virtualHost}/{Uri.EscapeDataString(id)}"`; on no match, returns the original
        matched text unchanged.
      Acceptance: the file exists at the exact path above, contains both named public members with
      the signatures above, and P1-T1's three tests pass when run (verified in Phase 3).
- [ ] [P2-T6] Edit `UtilitiesCS/UtilitiesCS.csproj` to insert
      `<Compile Include="OutlookObjects\MailItem\CidImageResolver.cs" />` immediately after the
      existing `<Compile Include="OutlookObjects\MailItem\MailItemHelper.Html.cs" />` line (this
      repo's legacy `packages.config`-based csproj requires explicit `<Compile Include>` wiring; there
      is no glob). Acceptance: the csproj contains the new `<Compile Include>` line exactly once,
      immediately following the sibling entry.
- [ ] [P2-T7] Edit `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs`'s `GetHtml()` method
      to call `CidImageResolver.RewriteCidReferences(revisedBody, AttachmentsInfo, CidImageResolver.DefaultVirtualHost)`
      on the computed `revisedBody` before returning it, leaving `GetHtml(string htmlBody)`'s
      pre-existing (out-of-scope) parameter-ignoring behavior untouched except for applying the same
      rewrite call to its own `revisedBody` result. Acceptance: both overloads' return values pass
      through `CidImageResolver.RewriteCidReferences`; no other line in the file changes; the
      `EmailHeader` splice logic is unchanged.
- [ ] [P2-T8] Add `[TestMethod] GetHtml_ShouldRewriteCidReferenceToVirtualHostUrl_WhenAttachmentContentIdMatches`
      to `UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperCoreTests.cs`, following the existing
      `GetHtml_ShouldInjectEmailHeaderIntoBodyMarkup` pattern: mock `InteropMailItem.HTMLBody` to
      return `<html><head></head><body><img src="cid:logo1"></body></html>`, use
      `SetLazyField(helper, "_attachmentsInfo", new IAttachment[] { new AttachmentSerializable() { ContentId = "logo1" } })`,
      call `helper.GetHtml()`, and assert the result
      `.Should().Contain("src=\"https://cid.quickfiler.local/logo1\"")`. Acceptance: the test method
      exists and, once P2-T7 lands, passes.
- [ ] [P2-T9] Edit `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`'s
      `InitializeWebViewAsync` (already `[ExcludeFromCodeCoverage]`) to, after
      `EnsureCoreWebView2Async` completes, call
      `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2.AddWebResourceRequestedFilter($"https://{UtilitiesCS.CidImageResolver.DefaultVirtualHost}/*", CoreWebView2WebResourceContext.Image)`
      and register a `WebResourceRequested` handler that, at request time, reads
      `ItemHelper.AttachmentsInfo`, calls `UtilitiesCS.CidImageResolver.BuildContentIdMap(...)`, looks
      up the requested URL's last path segment, and on a match calls
      `e.Response = _webViewEnvironment.CreateWebResourceResponse(new MemoryStream(match.AttachmentData), 200, "OK", $"Content-Type: {mimeType}")`;
      on no match, declines to set `e.Response` (falls through to WebView2's default not-found
      behavior) rather than throwing. Acceptance: `InitializeWebViewAsync` registers exactly one
      `AddWebResourceRequestedFilter` call scoped to `https://{DefaultVirtualHost}/*` with
      `CoreWebView2WebResourceContext.Image`, and exactly one `WebResourceRequested` handler that
      resolves attachments via `ItemHelper` at request time (not at registration time); the method
      remains `[ExcludeFromCodeCoverage]`.
- [ ] [P2-T10] Grep `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` for
      `AddWebResourceRequestedFilter` and `WebResourceRequested` and confirm both appear exactly once
      inside `InitializeWebViewAsync`, with the filter argument containing
      `CidImageResolver.DefaultVirtualHost` and `CoreWebView2WebResourceContext.Image`. Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/other/webresourcerequested-wiring-review.<TS>.md`.
      Acceptance: artifact contains the grep command, its output, and confirms exactly one match of
      each pattern at the expected scope. This is the code-review confirmation for the AC covering
      `InitializeWebViewAsync`'s host-bound glue (not unit-testable, `[ExcludeFromCodeCoverage]`).

---

### Phase 3 — Targeted Verification (maps to spec.md Acceptance Criteria)

- [ ] [P3-T1] Run
      `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:RewriteCidReferences_ShouldRewriteMatchedContentId,RewriteCidReferences_ShouldLeaveUnmatchedContentIdUnchanged,BuildContentIdMap_ShouldReturnCaseInsensitiveMapExcludingEmptyContentId`
      and confirm all three pass. Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/regression-testing/cid-resolver-tests-pass.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`
      stating `3/3 passed, 0 failed`. Satisfies the first two AC bullets of spec.md.
- [ ] [P3-T2] Run
      `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:ContentId_ShouldPopulateFromMockedPropertyAccessor_WhenPropertyPresent,ContentId_ShouldDefaultToNull_WhenPropertyAccessorThrows`
      and confirm both pass. Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/regression-testing/attachment-contentid-tests-pass.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`
      stating `2/2 passed, 0 failed`. Satisfies the `IAttachment.ContentId`/`AttachmentSerializable`
      AC bullet of spec.md.
- [ ] [P3-T3] Run
      `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:GetHtml_ShouldRewriteCidReferenceToVirtualHostUrl_WhenAttachmentContentIdMatches`
      and confirm it passes. Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/regression-testing/getthtml-cid-rewrite-test-pass.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`
      stating `1/1 passed, 0 failed`. Satisfies the `MailItemHelper.Html.cs GetHtml()` AC bullet of
      spec.md.
- [ ] [P3-T4] Run `git diff main -- QuickFiler/Controllers/QfcItemController.EventWiring.cs` and
      confirm the output is empty. Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/regression-testing/eventwiring-diff-unchanged.<TS>.md`.
      Acceptance: artifact contains the command and confirms zero diff lines. Satisfies the
      `QfcItemController.EventWiring.cs` call-site-unchanged AC bullet of spec.md.
- [ ] [P3-T5] Run
      `git diff --stat main -- QuickFiler/**/EfcViewer.cs QuickFiler/**/EfcViewer3.cs "**/CboFolders*" "**/QfcItemViewer*" "**/FolderScorer*" "**/FolderPredictor*"`
      and confirm the output is empty. Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/regression-testing/sibling-feature-file-isolation.<TS>.md`.
      Acceptance: artifact contains the command and confirms zero matched files changed. Satisfies
      the "no changes to EfcViewer/CboFolders/QfcItemViewer*/FolderScorer/FolderPredictor" AC bullet
      of spec.md.
- [ ] [P3-T6] Run `git diff --stat main` (repo root, no path filter) and confirm the output lists
      exactly these files: `UtilitiesCS/Interfaces/IEmailIntelligence/IAttachment.cs`,
      `UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs`,
      `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs`,
      `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs`, `UtilitiesCS/UtilitiesCS.csproj`,
      `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`,
      `UtilitiesCS.Test/OutlookObjects/MailItem/CidImageResolverTests.cs`,
      `UtilitiesCS.Test/OutlookObjects/Attachment/AttachmentSerializableTests.cs`,
      `UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperCoreTests.cs`,
      `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, plus `spec.md` under the feature folder (checked off
      in Phase 5). Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/regression-testing/repo-wide-diff-scope.<TS>.md`.
      Acceptance: artifact contains the full `git diff --stat main` output and confirms no other
      production or test file appears in it.

---

### Phase 4 — Final QA Loop (Full C# Toolchain)

Loop behavior: if any task in this phase fails, or if any command changes files (e.g. CSharpier
reformats a file), restart this phase from P4-T1. Do not proceed to Phase 5 until all five tasks in
this phase complete without errors in a single pass.

- [ ] [P4-T1] Run `dotnet tool run csharpier check .`. If it reports a diff, run
      `dotnet tool run csharpier format .` and restart this phase from P4-T1. Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/qa-gates/final-csharpier-check.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`
      confirming zero formatting diffs.
- [ ] [P4-T2] Run the post-change analyzer build:
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
      Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/qa-gates/final-analyzer-build.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`
      confirming zero analyzer errors/warnings-as-errors.
- [ ] [P4-T3] Run the post-change nullable/TreatWarningsAsErrors build:
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
      Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/qa-gates/final-nullable-build.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`
      confirming zero nullable warnings/errors.
- [ ] [P4-T4] Run the full post-change test pass with coverage:
      `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`.
      Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/qa-gates/final-test-coverage.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with
      the numeric total test pass/fail counts (including all 7 new tests from Phases 1–2) and the
      numeric post-change line-coverage percentage for `UtilitiesCS.dll` and `QuickFiler.dll`. Any
      pre-existing failure must match the baseline's known pre-existing failure set exactly (zero new
      failures).
- [ ] [P4-T5] Compare the baseline coverage percentages from P0-T10 against the post-change coverage
      percentages from P4-T4, and separately record `CidImageResolver.cs`'s own line/branch coverage
      from the `.coverage` output produced by P4-T4. Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/qa-gates/coverage-delta-verification.<TS>.md`.
      Acceptance: artifact states baseline coverage %, post-change coverage %, and
      `CidImageResolver.cs` new-code coverage %, with an explicit PASS/FAIL statement confirming (a)
      no regression on repository-wide testable-denominator line/branch coverage versus baseline, and
      (b) `CidImageResolver.cs` new-code coverage is at or above the applicable threshold
      (>= 85% line / >= 75% branch per `.claude/rules/general-unit-test.md`; >= 90% per CLAUDE.md's
      new-module target). If either figure is unavailable, this task's outcome MUST be recorded as
      remediation-required, not PASS.

---

### Phase 5 — Acceptance Criteria Closure & Evidence Commit

- [ ] [P5-T1] Edit
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/spec.md` to check off each
      bullet under `## Acceptance Criteria` that is fully satisfied by automated evidence (all bullets
      except the live-render manual-verification bullet), appending an inline evidence-artifact
      reference (relative path) to each checked item, citing the specific Phase 1–4 task and artifact
      that satisfies it. Acceptance: every automatable AC checkbox is `- [x]` and cites its backing
      evidence artifact path(s).
- [ ] [P5-T2] Record the manual-verification AC bullet's status to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/other/manual-render-verification.<TS>.md`,
      either (a) documenting the outcome of an actually-performed live QuickFiler expanded-mode render
      test against a real inline-`cid:`-image message and confirming compact mode is unaffected, or
      (b) if manual execution is deferred beyond this automated pass, recording an explicit
      `MANUAL VERIFICATION DEFERRED` header with the reason, rather than checking off that AC bullet
      as if it were verified. Acceptance: the artifact exists and states one of the two outcomes
      explicitly; the corresponding spec.md AC bullet is checked off only if outcome (a) was recorded.
- [ ] [P5-T3] Write a closure-summary evidence artifact to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/other/ac-closure-summary.<TS>.md`
      listing every bullet under spec.md's `## Acceptance Criteria`, each mapped to its exact backing
      evidence artifact path(s) from Phases 1–4 (or to P5-T2's deferred-verification record).
      Acceptance: the artifact exists and every AC bullet has at least one mapped, existing evidence
      artifact path or an explicit deferred-verification note.
- [ ] [P5-T4] Run `git status --porcelain` and confirm all code changes and evidence artifacts are
      staged/committed (empty output, or output limited to files intentionally left untracked by repo
      convention). Record to
      `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/other/clean-worktree-confirmation.<TS>.md`.
      Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`
      confirming the working tree is clean (or documenting the specific, expected exceptions).

---

## Acceptance Criteria Coverage Map (for preflight cross-check)

- AC "`RewriteCidReferences` rewrites a matched `cid:` reference" → P1-T1 (fail-before), P2-T5
  (implementation), P3-T1 (pass verification).
- AC "`RewriteCidReferences` leaves an unmatched `cid:` reference unchanged" → P1-T1, P2-T5, P3-T1.
- AC "`BuildContentIdMap` returns a case-insensitive map excluding empty `ContentId`" → P1-T1, P2-T5,
  P3-T1.
- AC "`IAttachment.ContentId` additive + populated by `AttachmentSerializable` with try/catch default"
  → P2-T1, P2-T2, P2-T3, P2-T4, P3-T2.
- AC "`GetHtml()` invokes `RewriteCidReferences` and output contains rewritten URL" → P2-T7, P2-T8,
  P3-T3.
- AC "`QfcItemController.EventWiring.cs` call site/signature unchanged" → P3-T4.
- AC "`InitializeWebViewAsync` registers `AddWebResourceRequestedFilter` + `WebResourceRequested`
  scoped correctly" → P2-T9, P2-T10.
- AC "No changes to `EfcViewer.cs`/`EfcViewer3.cs`/`CboFolders`/`QfcItemViewer*`/`FolderScorer`/
  `FolderPredictor`" → P3-T5, P3-T6.
- AC "Manual verification of live render (compact + expanded)" → P5-T2.
- AC "Full toolchain pass (CSharpier, analyzers, nullable, vstest coverage) in a single pass" →
  P4-T1 through P4-T4.
- AC "New/changed lines do not reduce repository-wide coverage below threshold" → P4-T5.
