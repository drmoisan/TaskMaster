# Policy Audit — quickfiler-inline-image-cid-fix (Issue #326)

- **Timestamp:** 2026-07-16T04-09
- **Branch:** `bug/quickfiler-inline-image-cid-fix-326`
- **Resolved base branch:** `epic/folder-tree-percentage-ui-integration`
- **Merge-base SHA:** `6d4535c654f2768568ff48e79f64fb9eacfdf62c` (independently reverified via
  `git merge-base HEAD origin/epic/folder-tree-percentage-ui-integration` and
  `git merge-base HEAD epic/folder-tree-percentage-ui-integration`; both returned the identical SHA
  supplied by the caller — no stale merge-base found).
- **Head SHA reviewed:** `d693caab` (branch tip at review start; working tree clean).
- **Work mode:** `full-bug` (persisted marker at `issue.md:12`). AC source: `spec.md`
  `## Acceptance Criteria` (sole source, per `full-bug` rule).

## Executive Summary

This is a small, well-scoped bugfix (issue #326) adding `cid:` inline-image resolution to
QuickFiler's WebView2 body-render path. The diff touches 8 C# files (5 production, 3 test) plus two
`.csproj` wiring edits and this feature's own documentation/evidence tree; no other language has
changed files. Independent verification (detailed below) confirms the feature's own evidence claims
are accurate: new-code coverage for `CidImageResolver.cs` is 94.7% line / 100% branch, the two other
modified production files (`AttachmentSerializable.cs`, `MailItemHelper.Html.cs`) are covered at
97.1% and 100% respectively with no regression, and the full C# toolchain (CSharpier, analyzers,
nullable/`TreatWarningsAsErrors`, MSTest+coverage) passed in a single pass. One acceptance criterion
(live manual render verification) is intentionally left unchecked; this is an acceptable, well-documented
deferral, not a gap in the automated work. Two implementation deviations from the plan are disclosed
and are found policy-compliant or low-severity (see Findings). The one unresolved, unconditional
policy gate failure is the **absence of the canonical repo-root coverage artifact**
(`artifacts/csharp/coverage.xml`), which is mandatory per this audit's own contract regardless of the
quality of the feature-local evidence; this triggers remediation per policy, though the underlying
work is otherwise sound. See `remediation-inputs.2026-07-16T04-09.md`.

## Scope and Baseline

- `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` did not exist at review
  start, contradicting the caller's claim that they had "already [been] collected." No
  `mcp__drm-copilot__collect_pr_context` (or any MCP) tool was present in this agent's tool list. Per
  `pr-context-artifacts`' refresh rule and prior practice on this repo (`pr-context-mcp-unavailable-manual-fallback`),
  both files were hand-authored directly from `git diff --numstat 6d4535c654f2768568ff48e79f64fb9eacfdf62c..HEAD`
  and a full `git diff` for the production/test files, in the bullet format the repo's coverage hook
  expects (`- <path> (+N/-N)`). This is disclosed, not silently substituted.
- Full branch diff obtained via `git diff --stat 6d4535c654f2768568ff48e79f64fb9eacfdf62c HEAD`: 35
  files changed (10 production/test/csproj files, 24 feature-folder evidence/doc files, 1 plan.md
  update, 1 spec.md update). No sibling epic child's files (#324/#325/#327/#328) are touched.
- Production files in scope (verified against `spec.md` §Proposed Fix and independently against
  `git diff`):
  1. `UtilitiesCS/Interfaces/IEmailIntelligence/IAttachment.cs` (+1, additive `ContentId` member)
  2. `UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs` (+21, populate `ContentId`)
  3. `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs` (new file, +93)
  4. `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs` (+10, invoke rewrite)
  5. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (+47, WebView2 wiring)
  6. Two `.csproj` wiring edits (`UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`, +1 each)
- Test files: `CidImageResolverTests.cs` (new, +79), `AttachmentSerializableTests.cs` (+36),
  `MailItemHelperCoreTests.cs` (+26).
- Verified unchanged (as required by `spec.md` §Scope & Non-Goals): `QfcItemController.EventWiring.cs`
  (`git diff 6d4535c654f2768568ff48e79f64fb9eacfdf62c HEAD -- QuickFiler/Controllers/QfcItemController.EventWiring.cs`
  → empty), and `EfcViewer.cs`/`EfcViewer3.cs`/`CboFolders*`/`QfcItemViewer*`/`FolderScorer*`/`FolderPredictor*`
  (`git diff --stat` with the same path filters → empty). Both independently reproduced against the
  correct merge-base by this reviewer, matching the feature's own
  `evidence/regression-testing/eventwiring-diff-unchanged.2026-07-16T00-18.md` and
  `evidence/regression-testing/sibling-feature-file-isolation.2026-07-16T00-20.md`.

## Rejected Scope Narrowing

None detected. The caller's instructions correctly identify this as an epic-child branch and supply
the correct base branch/merge-base (independently reverified above); this is legitimate base-branch
resolution, not scope narrowing. The caller's framing of the two implementation deviations and the one
by-design unchecked AC item asks this review to make an evidentiary judgment call, not to skip or
narrow any check. No caller text instructed skipping a toolchain stage, marking a changed language
"out of scope," or excluding any changed file from review. This audit covers the full branch diff
against the resolved base.

## Evidence Location Compliance

`git diff --name-only 6d4535c654f2768568ff48e79f64fb9eacfdf62c HEAD | grep -E "^artifacts/(baselines|baseline|qa|qa-gates|evidence|coverage|regression-testing|post-change)/"`
→ no matches. All feature evidence is written under
`docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/{baseline,other,qa-gates,regression-testing}/`,
the canonical `<FEATURE>/evidence/<kind>/` scheme. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition
applies; no non-canonical path was supplied by any caller instruction. **PASS.**

## 1. General Unit Test Policy Compliance

- **Independence/Isolation/Determinism:** All seven new/modified test methods are single-behavior,
  MSTest+Moq+FluentAssertions, Arrange-Act-Assert, no shared mutable state, no temp files, no network
  or external services. **PASS.**
- **Fast execution:** Individual test evidence shows sub-second to ~300ms runs. **PASS.**
- **Scenario completeness:** `CidImageResolverTests.cs` covers positive (matched rewrite), negative
  (unmatched left unchanged), and a mixed-population edge case (case-insensitive map, null/empty
  exclusion) for the two new pure functions. `AttachmentSerializableTests.cs` covers both the
  property-present and the accessor-throws (error-handling) path for `ContentId`. `MailItemHelperCoreTests.cs`
  covers the integration point (`GetHtml()` invoking the rewrite). **PASS** for the new surface;
  concurrency/state-transition scenarios are not applicable to these pure/stateless functions.
- **Test file location:** All three test files sit under `UtilitiesCS.Test/OutlookObjects/...`,
  mirroring the production tree (`UtilitiesCS/OutlookObjects/...`). **PASS.**
- **500-line file limit:** `AttachmentSerializableTests.cs` is 469 lines after this change (was ~433
  before), within the 500-line limit but with reduced headroom (Informational — see Findings).
  `CidImageResolverTests.cs` (79 lines) and `MailItemHelperCoreTests.cs` (415 lines) are comfortably
  within limit. **PASS.**
- **No temp files, no external dependencies:** Confirmed by inspection of all three test files; all
  attachment fakes are in-memory `AttachmentSerializable`/`Mock<PropertyAccessor>` instances. **PASS.**

## 2. General Code Change Policy Compliance

- **Design principles (simplicity, reusability, separation of concerns):** `CidImageResolver` is a
  small, pure, host-neutral static class with two well-named functions; I/O and COM-bound glue
  (`WebResourceRequested`) stay in `QfcItemController.ViewerSetup.cs`, not mixed into the pure resolver.
  **PASS.**
- **Bugfix workflow (failing test first, then minimal fix):** Fail-before evidence
  (`evidence/regression-testing/fail-before-cid-resolver.2026-07-15T23-55.md`) shows a genuine
  pre-fix compiler failure (`CS0117`/`CS0103` referencing the not-yet-existing `CidImageResolver`
  type and `ContentId` member) reproduced by this reviewer's read of the artifact and cross-checked
  against the corresponding pass evidence. **PASS.**
- **File size limit (500 lines):** All touched production files (`CidImageResolver.cs` 93,
  `MailItemHelper.Html.cs` 219, `AttachmentSerializable.cs` 265, `IAttachment.cs` 26,
  `QfcItemController.ViewerSetup.cs` 329 lines, independently counted with `awk 'END{print NR}'`)
  are well under the limit. **PASS.**
- **Error handling:** `TryFromContentIdAccessor` wraps the MAPI property read in try/catch and
  defaults to `null` on failure, mirroring the existing `TryFromAccessor` pattern exactly (verified
  by diff comparison). The `WebResourceRequested` handler declines to set `e.Response` on a lookup
  miss rather than throwing, matching spec.md's stated fail-non-fatal design. **PASS.**
- **Public API / compatibility:** `IAttachment.ContentId` is additive; the only production implementer
  of `IAttachment` (`AttachmentSerializable`, independently confirmed via
  `grep -rn ": IAttachment" UtilitiesCS`) already implements it. `QfcItemController.EventWiring.cs`'s
  `NavigateToString` call site is verified byte-for-byte unchanged. **PASS.**

## 3. Language-Specific Code Change Policy Compliance (C#)

- **Formatting (CSharpier):** Independently re-run by this reviewer against all eight touched `.cs`
  files: `dotnet tool run csharpier check <8 files>` → `Checked 8 files in 1463ms.`, zero diffs.
  Matches the feature's own final-pass evidence. **PASS.**
- **Linting (.NET analyzers):** Not independently re-run (full-solution `msbuild` build was not
  re-executed by this reviewer given time/CI-parity constraints); relying on
  `evidence/qa-gates/final-analyzer-build.2026-07-16T00-35.md`, which shows 0 errors, 74 warnings (a
  2-warning *reduction* from the 76-warning baseline, explained as pre-existing `SVGControl` warnings
  not re-emitted on this incremental pass) with no new warning class attributable to this feature's
  files. **UNVERIFIED (not independently re-run) but evidence is internally consistent and
  specific.**
- **Type checking (nullable / TreatWarningsAsErrors):** Same caveat; relying on
  `evidence/qa-gates/final-nullable-build.2026-07-16T00-37.md` (0 warnings, 0 errors, matches
  baseline). **UNVERIFIED (not independently re-run), evidence consistent.**
- **Public surface minimality (`.claude/rules/csharp.md` "Prefer internal for non-public APIs"):**
  `CidImageResolver` was made `public` rather than `internal` as the plan specified. See Finding
  PA-1 below. **PARTIAL.**
- **Testing tool stack (MSTest + Moq + FluentAssertions):** confirmed in all three touched test files.
  **PASS.**

## 4. Language-Specific Unit Test Policy Compliance (C#)

- **MSTest/Moq/FluentAssertions usage, `[TestClass]`/`[TestMethod]`, AAA structure:** confirmed by
  direct inspection of all three test files. **PASS.**
- **Coverage thresholds:** see the dedicated Coverage Verification section below.

## 5. Coverage Verification (Mandatory)

**Languages with changed files in the branch diff:** C# only (`.cs`: 8 files; `.csproj`: 2 files — no
`.ts`/`.tsx`, `.py`, `.ps1`/`.psm1` files changed). PowerShell/Python/TypeScript coverage rows are
correctly `N/A` because zero files of those languages changed on this branch — this is the only
context in which `N/A` is a permitted verdict per this audit's contract.

### Canonical artifact check

- `artifacts/csharp/coverage.xml` — **absent** (confirmed via `find . -iname coverage.xml`, which
  found only archived-feature copies under `docs/features/archive/...`, none at the canonical
  repo-root path).
- Per the mandatory Coverage Verification procedure: **coverage artifact absent for C#; coverage
  verification is mandatory for all languages with changed files. FAIL.** This triggers remediation
  regardless of the quality of the feature-local evidence discussed below.

### Independent verification performed (raw `.coverage` conversion, not a re-run of test generation)

A raw MSTest coverage file was already present at
`TestResults/7c9c72aa-643c-43aa-9701-2f07730bcdc3/DanMoisan_MEGALODON4_2026-07-15.23_49_29.coverage`
(produced by the executor's own P4-T4 final run; this reviewer did not re-run tests or coverage
generation). This reviewer converted it to Cobertura via
`dotnet-coverage merge <file> -f cobertura -o <scratch>/final-coverage.cobertura.xml` (a
format-conversion of an existing artifact, not new test/coverage generation) and independently
parsed the result:

| Item | Independently computed | Feature evidence claim (`coverage-delta-verification.2026-07-16T00-45.md`) | Match? |
|---|---|---|---|
| `QuickFiler` package line-rate | 72.2672% | 72.27% | Yes |
| `UtilitiesCS` package line-rate | 88.4464% | 88.45% | Yes |
| Repo-wide (6 first-party packages, class-level `<lines>`) | 40815/53971 = 75.6239% | 75.6239% | Yes, exact |
| `CidImageResolver.cs` line coverage | 28/30 (93.33%) main class + 8/8 (100%) closure = 36/38 = 94.74% | 94.7% | Yes |
| `CidImageResolver.cs` branch coverage | 100% (both class entries) | 100% | Yes |
| `ResolveImageMimeType` (new method, `QfcItemController.ViewerSetup.cs`) | 0/8 lines (0%) | "line-rate 0%" | Yes |
| `WebResourceRequested` closure (`<InitializeWebViewAsync>b__121_0`) | 0/17 lines (0%) | "line-rate 0%" | Yes |
| `AttachmentSerializable.cs` (whole file) | 136/140 = 97.14% | not separately reported by evidence, but consistent with "no regression" claim | Consistent |
| `MailItemHelper.Html.cs` (whole file) | 167/167 = 100% | not separately reported by evidence, but consistent with "no regression" claim | Consistent |

Every number the feature's own evidence reported was independently reproduced exactly (or to the
reported rounding) from the raw coverage artifact. This is strong corroboration that the evidence is
accurate and not fabricated, even though the canonical repo-root artifact copy is missing.

### Per-scope verdicts (uniform 85%/75% tier rule, `.claude/rules/general-unit-test.md` /
`quality-tiers.md` Authoritative Decision #2)

- **New code files:**
  - `CidImageResolver.cs`: 94.74% line / 100% branch. **PASS** (>= 85%/75%, and >= 90% CLAUDE.md
    new-module target).
- **Modified files:**
  - `AttachmentSerializable.cs`: 97.14% line, no regression (new lines are the covered
    `TryFromContentIdAccessor` path, verified by the two new passing tests). **PASS.**
  - `MailItemHelper.Html.cs`: 100% line, no regression. **PASS.**
  - `IAttachment.cs`: interface-only, no executable lines; correctly outside coverage measurement per
    the "type-only/interface-only modules" clarification in `general-unit-test.md`. **N/A (by
    design, not a changed-file exemption).**
  - `QfcItemController.ViewerSetup.cs`: file-level line-rate 96/140 lines newly measured in this
    review's independent pass at 68.57% for this specific file overall (a legacy, initialization-heavy
    file with substantial pre-existing uncovered wiring code, not attributable to this PR); no
    previously-covered line regressed (the entire drop is new, never-covered lines). Of the ~47
    added lines, 25 are new-and-uncovered: 17 are the `WebResourceRequested` closure body, which sits
    inside `InitializeWebViewAsync`'s pre-existing, ratified `[ExcludeFromCodeCoverage]` host-bound-glue
    exemption (Cobertura does not propagate that exclusion to the compiler-generated closure method,
    a known tooling limitation, not a policy gap); the remaining 8 are `ResolveImageMimeType`, a new,
    pure, stateless, fully-testable private method that carries **no exemption** and is **0% covered**.
    **FAIL** for this specific new method (see Finding PA-2); the closure portion is **PASS-by-exemption**.
- **Repo-wide (C#, first-party testable denominator across `UtilitiesCS`/`QuickFiler`/`Tags`/
  `TaskVisualization`/`TaskMaster`/`ToDoModel`):**
  - Baseline: 75.6133% (per feature evidence; this reviewer could not independently reproduce the
    baseline figure because the baseline `.coverage` file was no longer present in `TestResults/` at
    review time — only the final run's file remained).
  - Post-change: 75.6239% (independently reproduced exactly, see table above).
  - Change: +0.0106 pt (net neutral/slightly positive; no regression attributable to this PR).
  - **Disposition: FAIL against the repo's stated 85%/75% floor (`general-unit-test.md`), and also
    FAIL against `.claude/rules/csharp.md`'s stated 80% floor.** This is a long-standing, pre-existing,
    repo-wide condition, not something this PR introduced or could plausibly remediate within its own
    scope — the same condition has been documented across numerous prior reviews of this repository
    (see this agent's persistent memory: issues #253, #278, #283, #309, #328). This PR's own delta is
    net neutral to slightly positive. Recorded as a mandatory FAIL per this audit's letter, but
    dispositioned as a pre-existing, repo-wide, non-blocking-for-this-PR condition requiring
    org-level backlog remediation, not a defect in this change.
  - **Note — three conflicting repo-wide C# coverage floors exist in this repo's own policy documents**:
    CLAUDE.md's embedded UT2 section states `>= 80%` with a COM/VSTO exemption category;
    `.claude/rules/csharp.md` states `>= 80%` flat; `.claude/rules/general-unit-test.md` and
    `quality-tiers.md` state the uniform `>= 85% line / >= 75% branch` tier rule with "tier-specific
    lower thresholds are not used." This audit applies the newer, more specific 85%/75% uniform rule
    per this audit's own explicit instructions, but flags the three-way conflict as an
    unresolved documentation inconsistency (Informational; not attributable to this PR).
  - Branch coverage: this repo's Cobertura output records `branch-rate="1"` uniformly for all
    packages in both baseline and final XML (confirmed directly in the raw converted XML by this
    reviewer — every `<package>` and `<class>` element carries `branch-rate="1"`), which appears to be
    a tooling/instrumentation artifact of this repo's `dotnet-coverage`/MSTest configuration rather
    than a meaningful measurement (100% branch coverage across the entire first-party codebase is
    not plausible on its face). This is a pre-existing tooling limitation, not something this PR
    caused or could fix; branch-coverage verdicts in this audit rely on line-coverage as the
    practical signal.

### C# coverage row (required PASS/FAIL summary line for hook enforcement)

**C# / CSharp coverage: FAIL** — canonical `artifacts/csharp/coverage.xml` is absent (mandatory per
policy for any language with changed files); independently-verified new-code coverage for this PR's
own new/changed files is otherwise strong (CidImageResolver.cs 94.7%/100%, AttachmentSerializable.cs
97.1%, MailItemHelper.Html.cs 100%), with one isolated new-method gap (`ResolveImageMimeType`, 0%,
see Finding PA-2) and a pre-existing, non-regressing, repo-wide sub-floor condition (75.62%, below
the 85%/75% uniform tier rule). This C# coverage disposition is a full-scope FAIL verdict, not a
narrowed or skipped check.

## 6. Toolchain Execution Summary

| Stage | Command | Result (feature evidence) | Independently reproduced by this review? |
|---|---|---|---|
| Format | `dotnet tool run csharpier check .` | 0 diffs (1338 files) | Yes — reran against the 8 touched files directly; 0 diffs |
| Lint (analyzers) | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 errors, 74 warnings (pre-existing) | No — not re-run (full-solution build) |
| Type-check (nullable) | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 warnings, 0 errors | No — not re-run (full-solution build) |
| Test + coverage | `vstest.console.exe ... /EnableCodeCoverage` | 4709/4709 passed, 0 failed | Partially — raw `.coverage` output independently converted and parsed; test pass/fail counts not independently re-executed, but the presence of covered `CidImageResolver`/`MailItemHelper`/`AttachmentSerializable` lines in the raw artifact corroborates that the new tests genuinely executed |

## 7. Documented Implementation Deviations — Policy Review

### Deviation 1 — `CidImageResolver` made `public` instead of `internal`

**Verified claim:** confirmed by direct inspection — `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs`
declares `public static class CidImageResolver`, and `UtilitiesCS/Properties/AssemblyInfo.cs` grants
`InternalsVisibleTo` only to `DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, and `ToDoModel.Test` — no
grant to `QuickFiler` exists. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (assembly
`QuickFiler`, confirmed via `QuickFiler.csproj`'s `ProjectReference` to `UtilitiesCS.csproj`) does call
`CidImageResolver.DefaultVirtualHost` and `CidImageResolver.BuildContentIdMap` directly, so cross-assembly
access is genuinely required.

**Policy assessment:** `.claude/rules/csharp.md` states "Keep public API surface intentional and
minimal. Prefer `internal` for non-public APIs." The chosen fix (widen `CidImageResolver` to `public`)
is functionally correct and unblocks the feature without touching any file outside the plan's declared
scope, but it is not the narrowest fix available: adding
`[assembly: InternalsVisibleTo("QuickFiler")]` to `UtilitiesCS/Properties/AssemblyInfo.cs` (the exact
pattern already used for `UtilitiesCS.Test` and `ToDoModel.Test`) would have preserved `internal` and
kept the public surface of `UtilitiesCS.dll` unchanged for all other consumers, at the cost of touching
one additional file not listed in the plan's "Production files in scope" section. The executor's
stated rationale — avoiding an out-of-plan-scope file edit — is a defensible, disclosed, minimal-footprint
judgment call for a single-issue bugfix, not an attempt to hide the change. **Non-blocking finding**
(PA-1 below); recommend the `InternalsVisibleTo("QuickFiler")` alternative as a fast follow-up.

### Deviation 2 — `QuickFiler` package coverage micro-regression (-0.25 pt)

**Verified claim:** confirmed above in the Coverage Verification section — the drop is fully
attributable to the two new, wholly-uncovered constructs (`ResolveImageMimeType` 0/8,
`WebResourceRequested` closure 0/17), and repository-wide coverage did not regress (+0.0106 pt).

**Policy assessment:** the `WebResourceRequested` closure's lack of coverage is consistent with the
repo's existing ratified COM/WebView2-host-bound-glue exemption (the containing
`InitializeWebViewAsync` carries `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`, and the
spec's own Test Strategy section explicitly anticipates this). **No policy violation for the closure
portion.** `ResolveImageMimeType`, however, is a standalone, pure, stateless private method with no
COM/WebView2 dependency, is not decorated with `[ExcludeFromCodeCoverage]`, and has no applicable
exemption under any of the three coverage-floor documents in this repo. This is a real, avoidable,
new-code coverage gap. **Non-blocking but should be remediated finding** (PA-2 below).

## 8. Acceptance Criteria — By-Design Deferral Review

The one unchecked `spec.md` AC bullet ("Manual verification confirms inline `cid:` images render
correctly in a live QuickFiler expanded-mode reading pane...") is documented in
`evidence/other/manual-render-verification.2026-07-16T00-50.md` with an explicit
`MANUAL VERIFICATION DEFERRED` header, a concrete reason (no live Outlook/WebView2 host in this
execution environment), and a specific post-merge follow-up checklist matching `spec.md`'s own
"Manual validation steps" and "Rollout & Follow-up" sections verbatim. This matches the
`evidence-and-timestamp-conventions` skill's pattern for documenting an impossible-in-this-environment
verification rather than falsely asserting it was performed, and the AC bullet is correctly left
unchecked (not marked `[x]`) rather than being force-closed. **Assessment: acceptable, documented
exception, not a blocking gap.** This is consistent with the repo's general practice of deferring
genuinely environment-impossible manual/live verification to a human reviewer post-merge, provided
(as here) the deferral is explicit and the follow-up checklist is concrete.

## 9. Gaps and Exceptions

- **PA-1 (Non-blocking):** `CidImageResolver` is `public` rather than `internal`; recommend
  `InternalsVisibleTo("QuickFiler")` as a narrower alternative in a fast follow-up.
- **PA-2 (Non-blocking, remediation-eligible):** `ResolveImageMimeType` (new method,
  `QfcItemController.ViewerSetup.cs`) has 0% test coverage with no applicable exemption; recommend a
  small `[DataRow]`-parameterized MSTest test asserting the extension-to-MIME-type mapping (including
  the `null`/unknown-extension fallback).
- **PA-3 (mandatory FAIL, remediation trigger):** canonical `artifacts/csharp/coverage.xml` absent at
  the repo-root canonical path. The feature's own evidence is independently verified as accurate, but
  the canonical artifact copy required by this audit's contract does not exist.
- **PA-4 (pre-existing, non-blocking for this PR):** repo-wide C# coverage (75.62%) is below both the
  85%/75% uniform tier floor and the 80% floor separately stated in two other policy documents in this
  repo; this condition predates this PR and this PR's own delta is net neutral/positive.
- **PA-5 (Informational):** Three conflicting repo-wide C# coverage floors exist across CLAUDE.md,
  `.claude/rules/csharp.md`, and `.claude/rules/general-unit-test.md`/`quality-tiers.md`
  (80% w/ exemption vs. flat 80% vs. uniform 85%/75%). Not attributable to this PR; recommend
  reconciling these documents.
- **PA-6 (Informational):** `AttachmentSerializableTests.cs` is now 469 lines (94% of the 500-line
  limit); no action required now, but future additions to this file should consider splitting.
- Analyzer and nullable-build toolchain stages were not independently re-run by this reviewer (full
  solution `msbuild` build); relied on feature-provided evidence, which is internally consistent and
  specific (exact warning counts/diffs explained).

## 10. Compliance Verdict

**PARTIAL — remediation required**, driven entirely by PA-3 (mandatory canonical coverage-artifact
absence) per this audit's unconditional policy trigger. The underlying code change, tests, and
toolchain evidence are otherwise sound and independently verified. No finding in this audit rises to
a functional or correctness Blocking severity; see `code-review.2026-07-16T04-09.md` for severity
classification of individual findings and `remediation-inputs.2026-07-16T04-09.md` for the concrete
fix list.

## Appendix A: Independent Verification Commands Run By This Reviewer

```
git fetch origin epic/folder-tree-percentage-ui-integration
git merge-base HEAD origin/epic/folder-tree-percentage-ui-integration
git merge-base HEAD epic/folder-tree-percentage-ui-integration
git diff --stat 6d4535c654f2768568ff48e79f64fb9eacfdf62c HEAD
git diff --numstat 6d4535c654f2768568ff48e79f64fb9eacfdf62c HEAD
git diff 6d4535c654f2768568ff48e79f64fb9eacfdf62c HEAD -- QuickFiler/Controllers/QfcItemController.EventWiring.cs
git diff --stat 6d4535c654f2768568ff48e79f64fb9eacfdf62c HEAD -- "QuickFiler/**/EfcViewer.cs" "QuickFiler/**/EfcViewer3.cs" "**/CboFolders*" "**/QfcItemViewer*" "**/FolderScorer*" "**/FolderPredictor*"
git diff --name-only 6d4535c654f2768568ff48e79f64fb9eacfdf62c HEAD | grep -E "^artifacts/(baselines|baseline|qa|qa-gates|evidence|coverage|regression-testing|post-change)/"
grep -rn "InternalsVisibleTo" UtilitiesCS/Properties/AssemblyInfo.cs
grep -n "ProjectReference" QuickFiler/QuickFiler.csproj
dotnet-coverage merge TestResults/7c9c72aa-643c-43aa-9701-2f07730bcdc3/DanMoisan_MEGALODON4_2026-07-15.23_49_29.coverage -f cobertura -o <scratch>/final-coverage.cobertura.xml
dotnet tool run csharpier check <8 touched .cs files>
find . -iname "coverage.xml"
```

## Appendix B: Toolchain Commands Reference (repo-standard, per CLAUDE.md/`.claude/rules/csharp.md`)

1. `dotnet tool run csharpier .` (or `csharpier .`)
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
