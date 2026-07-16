# Remediation Inputs — quickfiler-inline-image-cid-fix (Issue #326)

- **Timestamp:** 2026-07-16T04-09
- **Triggered by:** `policy-audit.2026-07-16T04-09.md` (Compliance Verdict: PARTIAL — remediation
  required, driven by the mandatory canonical-coverage-artifact-absence rule).
- **Pointer to audit artifacts:** `policy-audit.2026-07-16T04-09.md`,
  `code-review.2026-07-16T04-09.md`, `feature-audit.2026-07-16T04-09.md` (all in this feature folder,
  same timestamp).

## Context

This feature's underlying work (the `cid:` inline-image resolution fix, its tests, and its toolchain
evidence) is sound and independently verified. Remediation is triggered by one unconditional policy
gate (coverage-artifact absence) plus two small, addressable code-quality gaps. This is a
proportionate, small remediation, not a rework of the feature.

## Enumerated Fix List

1. **Produce the canonical C# coverage artifact at the repo-canonical path.**
   - **File/path:** `artifacts/csharp/coverage.xml` (repo root; currently absent — only archived-feature
     copies exist under `docs/features/archive/.../evidence/...`).
   - **Expected behavior:** After running the repo's standard coverage command
     (`dotnet test --collect:"XPlat Code Coverage"` or the repo's `vstest.console.exe .../EnableCodeCoverage`
     + `dotnet-coverage merge -f cobertura` conversion step), a Cobertura-format XML must exist at
     `artifacts/csharp/coverage.xml` covering the full first-party solution (not just `UtilitiesCS.Test`/
     `QuickFiler.Test`), so the repo's `validate-feature-review-coverage.ps1` hook and future audits can
     read repo-wide/branch coverage directly instead of relying on ad hoc raw-`.coverage` conversion.
   - **Verification command:** `find . -iname coverage.xml -path "./artifacts/*"` should return exactly
     `./artifacts/csharp/coverage.xml`; then re-run this feature's coverage verification against that
     canonical artifact and confirm the same per-package figures reported in
     `evidence/qa-gates/coverage-delta-verification.2026-07-16T00-45.md`.
   - **Note:** This is a repo-process/CI gap, not a defect introduced by this feature; treat as a
     process remediation, not a rework of feature code.

2. **Add a unit test for `ResolveImageMimeType`.**
   - **File:** `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (production, ~line 112);
     add the test to `QuickFiler.Test` (mirroring `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`'s
     test-tree location, e.g. `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` if that
     file exists, or a new sibling test file following repo test-file-location conventions).
   - **Expected behavior:** A new `[TestMethod]` (or `[DataRow]`-parameterized test) asserting
     `ResolveImageMimeType(".jpg")` → `"image/jpeg"`, `".jpeg"` → `"image/jpeg"`, `".png"` → `"image/png"`,
     `".gif"` → `"image/gif"`, `".bmp"` → `"image/bmp"`, an unrecognized extension (e.g. `".pdf"`) →
     `"application/octet-stream"`, and `null` → `"application/octet-stream"`.
   - **Verification command:** `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:<new test name(s)>`
     — confirm pass, then re-verify `ResolveImageMimeType`'s Cobertura line-rate rises from 0/8 to 8/8
     (100%).
   - **Note:** This method has no COM/WebView2 dependency and requires no mocking; it is a pure switch
     expression on a nullable string.

3. **Optional, non-required alternative for the `CidImageResolver` visibility deviation (recommend, not mandatory).**
   - **File:** `UtilitiesCS/Properties/AssemblyInfo.cs`.
   - **Expected behavior:** Add `[assembly: InternalsVisibleTo("QuickFiler")]` alongside the existing
     `InternalsVisibleTo("UtilitiesCS.Test")` and `InternalsVisibleTo("ToDoModel.Test")` grants, then
     revert `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs`'s class declaration from
     `public static class CidImageResolver` to `internal static class CidImageResolver`.
   - **Verification command:** full C# toolchain re-run (`csharpier`, analyzer build, nullable build,
     `vstest.console.exe`) confirming zero new errors/warnings after the visibility change, plus
     confirming `QfcItemController.ViewerSetup.cs` still compiles against the now-internal type.
   - **Note:** This is a code-quality preference, not a functional defect. It is listed here for
     completeness because it was raised as a documented deviation, but it does not block merge on its
     own and may be deferred to a separate, later cleanup if the team prefers not to touch
     `AssemblyInfo.cs` in this cycle.

## Do-Not-Do List

- Do not widen the scope of this remediation to touch `QfcItemController.EventWiring.cs`,
  `EfcViewer.cs`, `EfcViewer3.cs`, `CboFolders`, any `QfcItemViewer*` Designer file, `FolderScorer`, or
  `FolderPredictor` — these remain explicitly out of scope per `spec.md` §Scope & Non-Goals.
- Do not attempt to fabricate or force a live Outlook/WebView2 manual-render verification in this
  remediation cycle; AC #9 remains a legitimate, documented, environment-blocked deferral (see
  `feature-audit.2026-07-16T04-09.md`).
- Do not weaken or delete any existing passing test to make coverage numbers look better.
- Do not attempt to "fix" the repo-wide C# coverage shortfall (75.62%, below the 85%/75% uniform tier
  floor) as part of this remediation — that is a pre-existing, repo-wide condition tracked across
  numerous other issues in this repository and is out of scope for a single-issue bugfix remediation.
- Do not introduce `[ExcludeFromCodeCoverage]` on `ResolveImageMimeType` as a shortcut to "pass"
  coverage — it has no COM/WebView2 dependency and does not qualify for any of the repo's ratified
  exemption categories; write the test instead.
- Do not touch `.github/workflows/**`, `scripts/benchmarks/**`, or `.github/actions/**` — none of
  these are in scope for this feature and none were touched by the original change.

## Severity Classification Reference

None of the above items are Blocking in the sense of a functional or correctness defect. Item 1
(canonical artifact) is a mandatory policy-gate trigger per this audit's unconditional rule and must
be resolved (or explicitly re-dispositioned by a maintainer) before this audit's PARTIAL verdict can
become PASS. Items 2 and 3 are Non-blocking code-quality improvements. See
`code-review.2026-07-16T04-09.md`'s Findings Table for the full severity/rationale/evidence detail
backing each item.
