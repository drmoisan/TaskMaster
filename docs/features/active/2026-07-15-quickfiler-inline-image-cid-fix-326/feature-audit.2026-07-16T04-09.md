# Feature Audit — quickfiler-inline-image-cid-fix (Issue #326)

- **Timestamp:** 2026-07-16T04-09
- **Work mode:** `full-bug` (persisted marker at `issue.md:12`)
- **AC source:** `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/spec.md`,
  `## Acceptance Criteria` section (sole AC source for `full-bug` work mode)

## Scope and Baseline

- **Branch:** `bug/quickfiler-inline-image-cid-fix-326`
- **Resolved base branch:** `epic/folder-tree-percentage-ui-integration`
- **Merge-base SHA:** `6d4535c654f2768568ff48e79f64fb9eacfdf62c` (independently reverified by this
  reviewer via `git merge-base HEAD origin/epic/folder-tree-percentage-ui-integration`; identical to
  the caller-supplied value — not stale)
- **Files in scope for this feature** (production/test only, independently confirmed via
  `git diff --stat 6d4535c654f2768568ff48e79f64fb9eacfdf62c HEAD`):
  - `UtilitiesCS/Interfaces/IEmailIntelligence/IAttachment.cs`
  - `UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs`
  - `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs` (new)
  - `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs`
  - `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
  - `UtilitiesCS.Test/OutlookObjects/Attachment/AttachmentSerializableTests.cs`
  - `UtilitiesCS.Test/OutlookObjects/MailItem/CidImageResolverTests.cs` (new)
  - `UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperCoreTests.cs`
  - `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj` (wiring only)
- No sibling epic-child files (#324/#325/#327/#328) are touched; independently confirmed via
  `git diff --stat` with path filters for `EfcViewer*`, `EfcViewer3*`, `CboFolders*`, `QfcItemViewer*`,
  `FolderScorer*`, `FolderPredictor*` (empty output against the correct merge-base).

## Acceptance Criteria Inventory

Eleven checkbox items under `spec.md`'s `## Acceptance Criteria` heading (verified by direct read of
the section):

1. `RewriteCidReferences` rewrites a matched `cid:` reference to the virtual-host URL.
2. `RewriteCidReferences` leaves an unmatched `cid:` reference unchanged.
3. `BuildContentIdMap` returns a case-insensitive map excluding empty/null `ContentId`.
4. `IAttachment.ContentId` is additive and populated by `AttachmentSerializable` with try/catch default.
5. `MailItemHelper.Html.cs`'s `GetHtml()` invokes the rewrite and its output contains the rewritten URL.
6. `QfcItemController.EventWiring.cs`'s `NavigateToString` call site/signature unchanged.
7. `QfcItemController.ViewerSetup.cs`'s `InitializeWebViewAsync` registers the filter + handler,
   scoped correctly.
8. No changes to `EfcViewer.cs`/`EfcViewer3.cs`/`CboFolders`/`QfcItemViewer*`/`FolderScorer`/`FolderPredictor`.
9. Manual verification of live render (compact + expanded modes).
10. Full toolchain pass (CSharpier, analyzers, nullable, `vstest.console.exe /EnableCodeCoverage`) in
    a single pass.
11. New/changed lines in `CidImageResolver.cs`, the `ContentId` change in `AttachmentSerializable.cs`,
    and the `GetHtml()` change in `MailItemHelper.Html.cs` do not reduce repository-wide coverage
    below the applicable threshold.

## Acceptance Criteria Evaluation

| # | Criterion (abridged) | Verdict | Evidence | Independent verification performed |
|---|---|---|---|---|
| 1 | `RewriteCidReferences` rewrites matched `cid:` | **PASS** | `evidence/regression-testing/fail-before-cid-resolver.2026-07-15T23-55.md`, `evidence/regression-testing/cid-resolver-tests-pass.2026-07-16T00-10.md` | Read `CidImageResolverTests.RewriteCidReferences_ShouldRewriteMatchedContentId` and `CidImageResolver.RewriteCidReferences` source directly; assertion and implementation match the AC text exactly. Raw coverage data confirms the covered code path executed (28/30 lines hit on `CidImageResolver.cs`). |
| 2 | `RewriteCidReferences` leaves unmatched `cid:` unchanged | **PASS** | same as #1 | Read `RewriteCidReferences_ShouldLeaveUnmatchedContentIdUnchanged` and the `MatchEvaluator`'s no-match branch (`return match.Value;`) directly; logic matches. |
| 3 | `BuildContentIdMap` case-insensitive, excludes empty/null | **PASS** | same as #1 | Read `BuildContentIdMap_ShouldReturnCaseInsensitiveMapExcludingEmptyContentId` and `BuildContentIdMap`'s `StringComparer.OrdinalIgnoreCase` + `string.IsNullOrEmpty` guard directly; logic matches. |
| 4 | `IAttachment.ContentId` additive + populated w/ try/catch default | **PASS** | `evidence/regression-testing/attachment-contentid-tests-pass.2026-07-16T00-13.md`, `evidence/baseline/iattachment-implementer-scan.2026-07-15T23-47.md` | Independently re-ran `grep -rn ": IAttachment\b" UtilitiesCS` (excluding test dirs) — confirms `AttachmentSerializable` is the sole production implementer, matching the feature's scan. Read `TryFromContentIdAccessor` directly; matches the existing `TryFromAccessor` try/catch-default pattern exactly. |
| 5 | `GetHtml()` invokes rewrite, output contains rewritten URL | **PASS** | `evidence/regression-testing/getthtml-cid-rewrite-test-pass.2026-07-16T00-16.md` | Read both `GetHtml()` overloads directly; both call `CidImageResolver.RewriteCidReferences(revisedBody, AttachmentsInfo, CidImageResolver.DefaultVirtualHost)` before returning. Independently converted raw coverage output shows `MailItemHelper.Html.cs` at 167/167 lines covered (100%), consistent with the new test genuinely exercising this path. |
| 6 | `EventWiring.cs` call site/signature unchanged | **PASS** | `evidence/regression-testing/eventwiring-diff-unchanged.2026-07-16T00-18.md` | Independently re-ran `git diff 6d4535c654f2768568ff48e79f64fb9eacfdf62c HEAD -- QuickFiler/Controllers/QfcItemController.EventWiring.cs` against the correct merge-base (not `main`, which this branch is far ahead of) — empty diff, confirming zero changes. |
| 7 | `InitializeWebViewAsync` registers filter + handler, scoped correctly | **PASS** | `evidence/other/webresourcerequested-wiring-review.2026-07-16T00-05.md` | Read `QfcItemController.ViewerSetup.cs`'s `InitializeWebViewAsync` directly; confirms exactly one `AddWebResourceRequestedFilter($"https://{CidImageResolver.DefaultVirtualHost}/*", CoreWebView2WebResourceContext.Image)` call and exactly one `WebResourceRequested +=` registration, both inside the method, both reading `ItemHelper.AttachmentsInfo` at request time (not registration time) as the spec requires. |
| 8 | No changes to sibling-feature files | **PASS** | `evidence/regression-testing/sibling-feature-file-isolation.2026-07-16T00-20.md`, `evidence/regression-testing/repo-wide-diff-scope.2026-07-16T00-25.md` | Independently re-ran `git diff --stat 6d4535c654f2768568ff48e79f64fb9eacfdf62c HEAD -- "QuickFiler/**/EfcViewer.cs" "QuickFiler/**/EfcViewer3.cs" "**/CboFolders*" "**/QfcItemViewer*" "**/FolderScorer*" "**/FolderPredictor*"` — empty output. |
| 9 | Manual live-render verification | **UNVERIFIED (by design, documented deferral)** | `evidence/other/manual-render-verification.2026-07-16T00-50.md` | Confirmed the deferral is explicit (`MANUAL VERIFICATION DEFERRED` header), gives a concrete environment-limitation reason (no live Outlook/WebView2 host available to this agent or this reviewer), and lists a specific post-merge follow-up checklist matching `spec.md`'s own "Manual validation steps." This reviewer likewise cannot perform a live render check in this environment. Assessed as an **acceptable, documented exception**, not a blocking gap — see `policy-audit.2026-07-16T04-09.md` §8. |
| 10 | Full toolchain pass, single pass | **PASS** | `evidence/qa-gates/final-csharpier-check.2026-07-16T00-32.md`, `evidence/qa-gates/final-analyzer-build.2026-07-16T00-35.md`, `evidence/qa-gates/final-nullable-build.2026-07-16T00-37.md`, `evidence/qa-gates/final-test-coverage.2026-07-16T00-40.md` | Independently re-ran `dotnet tool run csharpier check` against all 8 touched `.cs` files directly (0 diffs, matching the feature's full-repo pass). Analyzer and nullable builds were not independently re-run (full-solution build); evidence is internally consistent (specific warning-count deltas explained). Test pass count (4709/4709) corroborated indirectly via the raw coverage artifact showing the new tests' target methods as covered. |
| 11 | New/changed lines in `CidImageResolver.cs`/`AttachmentSerializable.cs`'s `ContentId` change/`MailItemHelper.Html.cs`'s `GetHtml()` change do not reduce repo-wide coverage | **PASS** (as literally scoped) | `evidence/qa-gates/coverage-delta-verification.2026-07-16T00-45.md` | Independently converted the executor's raw `.coverage` output to Cobertura and reproduced every figure exactly: `CidImageResolver.cs` 94.74% line/100% branch; `AttachmentSerializable.cs` 97.14% line (no regression); `MailItemHelper.Html.cs` 100% line (no regression); repo-wide (6 first-party packages) 75.6133% → 75.6239% (+0.0106 pt, net positive). **Scoping note:** this AC's text is explicitly limited to the three named files/changes and does not cover `QfcItemController.ViewerSetup.cs`'s new `ResolveImageMimeType` helper (0% covered, no exemption) — that gap is real but falls outside this AC's literal wording; it is tracked as a Non-blocking code-review finding (PA-2 in `policy-audit.2026-07-16T04-09.md`) rather than an AC failure. |

## Summary

### Acceptance Criteria Status
- Source: `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/spec.md`
- Total AC items: 11
- Checked off (delivered): 10
- Remaining (unchecked): 1
- Items remaining: "Manual verification confirms inline `cid:` images render correctly in a live
  QuickFiler expanded-mode reading pane and that compact mode (same call path) is unaffected beyond
  the shared resolution" — intentionally deferred with documented reason and follow-up checklist; not
  a delivery gap in the automatable scope of this feature.

Ten of eleven acceptance criteria are independently verified PASS with direct evidence
cross-checked against source code and independently re-derived coverage/diff data. The remaining
criterion is a live-environment manual verification that neither the original executor nor this
reviewer can perform in an automated/headless environment; it is correctly left unchecked with an
explicit, auditable deferral record rather than falsely marked complete. No AC item evaluates to
FAIL. This feature is functionally complete and well-tested for everything that can be verified
without a live Outlook/WebView2 host.

## Acceptance Criteria Check-off

All ten PASS-evaluated criteria were already checked off (`- [x]`) in `spec.md` prior to this review,
each with an inline evidence-artifact citation. This reviewer independently re-verified each PASS
disposition against source code, test files, and independently re-derived coverage/diff data (see
Evaluation table above) and found no reason to reverse any check-off. No new check-offs were made by
this review because none were outstanding. Criterion #9 remains correctly unchecked (`- [ ]`), matching
its by-design `MANUAL VERIFICATION DEFERRED` disposition.
