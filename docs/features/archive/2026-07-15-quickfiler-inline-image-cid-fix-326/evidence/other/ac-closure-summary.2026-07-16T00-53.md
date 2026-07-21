# Acceptance Criteria Closure Summary — P5-T3

- **Timestamp:** 2026-07-16T00-53
- **AC source:** `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/spec.md`,
  `## Acceptance Criteria` section (sole AC source for this full-bug plan).

| # | AC Bullet (abridged) | Status | Backing Evidence |
|---|---|---|---|
| 1 | `RewriteCidReferences` rewrites matched `cid:` reference | [x] | `evidence/regression-testing/fail-before-cid-resolver.2026-07-15T23-55.md`; `evidence/regression-testing/cid-resolver-tests-pass.2026-07-16T00-10.md` |
| 2 | `RewriteCidReferences` leaves unmatched `cid:` reference unchanged | [x] | same as above |
| 3 | `BuildContentIdMap` case-insensitive map, excludes empty `ContentId` | [x] | same as above |
| 4 | `IAttachment.ContentId` additive + populated via `PropertyAccessor`, try/catch default | [x] | `evidence/regression-testing/attachment-contentid-tests-pass.2026-07-16T00-13.md`; `evidence/baseline/iattachment-implementer-scan.2026-07-15T23-47.md` |
| 5 | `GetHtml()` invokes `RewriteCidReferences`, output contains rewritten URL | [x] | `evidence/regression-testing/getthtml-cid-rewrite-test-pass.2026-07-16T00-16.md` |
| 6 | `QfcItemController.EventWiring.cs` call site/signature unchanged | [x] | `evidence/regression-testing/eventwiring-diff-unchanged.2026-07-16T00-18.md` |
| 7 | `InitializeWebViewAsync` registers `AddWebResourceRequestedFilter` + `WebResourceRequested`, scoped correctly | [x] | `evidence/other/webresourcerequested-wiring-review.2026-07-16T00-05.md` |
| 8 | No changes to `EfcViewer*`/`CboFolders`/`QfcItemViewer*`/`FolderScorer`/`FolderPredictor` | [x] | `evidence/regression-testing/sibling-feature-file-isolation.2026-07-16T00-20.md`; `evidence/regression-testing/repo-wide-diff-scope.2026-07-16T00-25.md` |
| 9 | Manual verification of live render (compact + expanded) | [ ] deferred | `evidence/other/manual-render-verification.2026-07-16T00-50.md` (`MANUAL VERIFICATION DEFERRED`) |
| 10 | Full toolchain pass (CSharpier, analyzers, nullable, vstest coverage), single pass | [x] | `evidence/qa-gates/final-csharpier-check.2026-07-16T00-32.md`; `evidence/qa-gates/final-analyzer-build.2026-07-16T00-35.md`; `evidence/qa-gates/final-nullable-build.2026-07-16T00-37.md`; `evidence/qa-gates/final-test-coverage.2026-07-16T00-40.md` |
| 11 | New/changed lines do not reduce repo-wide coverage below threshold | [x] | `evidence/qa-gates/coverage-delta-verification.2026-07-16T00-45.md` |

## Totals

- Total AC items: 11
- Checked off (delivered): 10
- Remaining (unchecked): 1 — item 9, deferred pending a human-performed live render check (see
  `evidence/other/manual-render-verification.2026-07-16T00-50.md`).

## Documented plan deviations (escalated, not silently resolved)

1. **`CidImageResolver` accessibility.** P2-T5 specified `internal static class CidImageResolver`;
   P2-T9 requires `QfcItemController.ViewerSetup.cs` (a different assembly, `QuickFiler`, with no
   `InternalsVisibleTo` grant from `UtilitiesCS`) to call `CidImageResolver.DefaultVirtualHost` and
   `CidImageResolver.BuildContentIdMap` directly. Resolved by making `CidImageResolver` `public`
   instead of `internal` (the minimal in-scope-file fix; no `AssemblyInfo.cs` change, which would have
   been an out-of-plan-scope production file). See
   `evidence/other/webresourcerequested-wiring-review.2026-07-16T00-05.md` for the full note.
2. **`QuickFiler` package coverage micro-regression (-0.25 pt), non-blocking.** New
   `ResolveImageMimeType` helper and the `WebResourceRequested` lambda closure are not covered by any
   unit test; the repository-wide testable-denominator coverage did not regress overall. See
   `evidence/qa-gates/coverage-delta-verification.2026-07-16T00-45.md` for the full analysis and
   disposition.
3. **P3-T6's literal `git diff --stat main`** produces a much larger diff than the plan anticipated,
   because this feature branch is based on `origin/epic/folder-tree-percentage-ui-integration` (which
   already contains other merged epic children), not directly on `main`. The corrected comparison
   against the actual divergence point confirms the diff is scoped exactly to this feature's files.
   See `evidence/regression-testing/repo-wide-diff-scope.2026-07-16T00-25.md`.
