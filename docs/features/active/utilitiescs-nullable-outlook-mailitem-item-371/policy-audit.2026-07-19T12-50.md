# Policy Audit — utilitiescs-nullable-outlook-mailitem-item (Issue #371)

- Timestamp: 2026-07-19T12-50
- Reviewer: feature-review agent
- Branch: `bug/utilitiescs-nullable-outlook-mailitem-item-371`
- Base (merge-base): `dffadd5a102884dd811ed5731477de18417594f1` (epic integration tip at branch point; contains merged #363/#364)
- HEAD: `0be4b0b63b544bf7be4a0c4d2feac0b257e81d29`
- Work mode: full-feature
- Scope command: `git diff --stat dffadd5a102884dd811ed5731477de18417594f1..HEAD -- UtilitiesCS UtilitiesCS.Test`

## Executive Summary

Overall verdict: PASS.

The branch delivers per-file `#nullable enable` remediation across exactly 30 production `.cs`
files under `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}/`, with no
test-file, project-file, solution-file, `.claude/rules/*`, or workflow changes. All hard
constraints listed for this child were independently verified against the diff and evidence
artifacts. No FAIL findings; no blocking-PARTIAL findings. Coverage shows no regression on changed
lines. The documented toolchain deviation (pragma-only nullable gate, no `/p:Nullable=enable`) is
present in the plan and spec and is not accompanied by any edit to policy files.

## Scope Confirmation

- Changed production files (30, all under the declared cluster): verified via `git diff --name-only`.
  Distribution matches spec (MailItem 12, Item 9, Conversation 2, Attachment 2, Table 5).
- No `UtilitiesCS.Test` files changed (annotation-only; no new/modified tests).
- Non-source changed files: `.claude/agent-memory/atomic-executor/*` (executor memory, not a policy
  document) and the feature `docs/` folder (plan, spec, user-story, evidence). No policy documents
  under `.claude/rules/` were modified.

## Rejected Scope Narrowing

None. The caller supplied the authoritative full-branch scope against the resolved merge-base and
did not attempt to narrow coverage or skip any toolchain check. The audit was performed against the
full branch diff.

## Section 1 — General Code Change Policy

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 1.1 | Annotation-only; no behavior change / refactor / API redesign | PASS | Diff is pragma + `?`/`!` annotations applied in place; no algorithmic edits. `dynamic item = itemObj;` byte-unchanged (diff shows context only). ETL/EtlAsync/GetTableInViewAsync public signatures reverted to non-null (2f6f3fec) to preserve behavior-compatibility. |
| 1.2 | File-size limit (500 lines) | PASS (pre-existing exception documented) | Only `OutlookItem.cs` exceeds 500 (504 lines). Pre-existing at baseline (503), flagged not fixed in `evidence/other/maintainer-flags.2026-07-19T10-50.md`. Next largest touched file `OlTableExtensions.Etl.cs` = 474. No new violation. |
| 1.3 | No new banned/forbidden constructs | PASS | Diff introduces none of `[NotNullWhen]/[MaybeNullWhen]/[NotNullIfNotNull]/[MemberNotNull]/[MaybeNull]/[AllowNull]/[DisallowNull]/[DoesNotReturn]`, `init`, `record`, or `record struct` (net481 constraints). Verified by grep on added (`^+`) lines. |
| 1.4 | No new dependencies | PASS | No `.csproj`/package changes. |
| 1.5 | Toolchain loop completed in final pass | PASS | See Section 7. |

Banned-API note: `DateTime.Now` occurs in `AttachmentHelper.cs`, `AttachmentSerializable.cs`, and
`OlTableExtensions.Etl.cs`, but every occurrence is a pre-existing context line (present at
merge-base; diff shows no `+`/`-` on those lines). The repo bans these APIs in test code; these are
pre-existing production lines untouched by this annotation-only child. Not a finding.

## Section 2 — C# Code Change Policy

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 2.1 | CSharpier formatting clean (final) | PASS | `evidence/qa-gates/final-csharpier.2026-07-19T10-50.md`. |
| 2.2 | Analyzer / codestyle build clean (final) | PASS | `evidence/qa-gates/final-analyzer-build.2026-07-19T10-50.md`: EXIT 0, 0 errors, 0 warnings in the 30 in-scope files (remaining 16 warnings pre-existing, in test/other projects). |
| 2.3 | Nullable / type-check gate (pragma-only deviation) | PASS | See Section 3 and Section 7. |
| 2.4 | No project/solution `<Nullable>` element introduced | PASS | `git diff` shows `UtilitiesCS.csproj` and `TaskMaster.sln` unmodified; `grep <Nullable>` returns no matches (exit 1). |
| 2.5 | Partial-class groups opted-in as a unit | PASS | `MailItemHelper` (5), `ConvHelper` (2), `OlTableExtensions` (4) each verified as one build unit in `batch-g/h/i-nullable-build` evidence (0 CS86xx per group; no inconsistent CS8618). |

## Section 3 — Documented Toolchain Deviation

The plan and spec document a deliberate, child-scoped deviation: the nullable/type-check gate is
`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
/p:TreatWarningsAsErrors=true` with `/p:Nullable=enable` never passed. Verified:

- The deviation is stated explicitly in `spec.md` (Toolchain Note, Constraints & Risks) and
  `issue.md`, not silently substituted. Rationale: the global flag would surface the whole epic's
  pre-existing debt and drown this child's signal. Resolution of the rules-vs-convention conflict is
  deferred to the Wave-2 CI capstone child.
- `.claude/rules/*` was NOT edited to accommodate the deviation (`git diff` shows zero changes under
  `.claude/rules`).
- The plan-literal solution-wide TWAE Rebuild halts (exit 1) on a pre-existing, out-of-scope
  `SVGControl` CS0649 build error. Independently verified as genuinely pre-existing: at the
  merge-base, `SVGControl/SvgImageSelector.cs` declares `private string? _relativeImagePath;` /
  `_absoluteImagePath;` that are never assigned (CS0649). `SVGControl` is untouched by #371 (empty
  diff) and is outside `UtilitiesCS/OutlookObjects/`. This matches the condition documented for the
  already-merged #363/#364 children.
- Substitution methodology is sound: because `UtilitiesCS.csproj` has no `<Nullable>` element, CS86xx
  diagnostics arise only from files carrying a `#nullable enable` pragma, and CS86xx severity is
  warning/error-independent (a `warning CS86xx` in a non-TWAE build corresponds 1:1 to an
  `error CS86xx` under TWAE). The isolated in-scope gate
  `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild ... /p:BuildProjectReferences=false` (EXIT 0)
  therefore yields an accurate CS86xx count without the out-of-scope SVGControl halt. This is a
  faithful workaround for a pre-existing out-of-scope blocker, not a mechanism to hide a regression:
  the final full-solution gate (P10-T3) additionally reports total UtilitiesCS CS86xx = 0, including
  out-of-scope nullable-enabled consumers.

## Section 4 — Evidence Location Compliance

All feature evidence is written under
`docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/{baseline,qa-gates,regression-testing,other}/`,
the canonical `<FEATURE>/evidence/<kind>/` location. No files were written to `artifacts/baselines/`,
`artifacts/qa/`, `artifacts/coverage/`, or `artifacts/evidence/`. No evidence-location violations
found. Verdict: PASS.

## Section 5 — Coverage Verification

Changed language with files in the branch diff: C# (`.cs`) only. No TypeScript, Python, or
PowerShell files changed (their coverage checks are not applicable because zero files of those
languages changed on this branch).

Coverage artifact model: the canonical `artifacts/csharp/coverage.xml` is not present in this
worktree; the authoritative per-feature coverage evidence is the Cobertura pair
`evidence/baseline/coverage-baseline.2026-07-19T10-50.cobertura.xml` and
`evidence/qa-gates/final-coverage.2026-07-19T10-50.cobertura.xml`, compared in
`evidence/qa-gates/coverage-delta.2026-07-19T10-50.md`.

C# coverage verdict: PASS.

- In-scope OutlookObjects line coverage: Baseline 87.07% (3319/3812); Post-change 87.07% (3320/3813);
  Change +0.00% (clears the 85% line floor).
- Repo-wide UtilitiesCS line coverage: 65.30% (unchanged, flat vs baseline). This figure sits below
  85% because it includes COM/VSTO/WinForms-bound denominators that carry a ratified maintainer
  coverage exemption (CLAUDE.md testable-denominator rule). This is a pre-existing repository-wide
  condition, not introduced or worsened by this child (delta +0.00002, flat). The applicable gate for
  this annotation-only child is changed-line no-regression (AC4), which passes.
- Changed-line no-regression: PASS. No in-scope file regressed; per-file rates identical or
  improved (`AttachmentHelper.cs` 94.7%, `OutlookItem.cs` 82.5%, `MailItemHelper.cs` 97.3%,
  `MailItemHelper.Properties.cs` 81.7%, `AttachmentSerializable.cs` 97.1%).
- New/changed-code coverage: the one non-COM-bound file, `CidImageResolver.cs`, holds at 94.7%
  (36/38), unchanged from baseline, backed by `UtilitiesCS.Test/OutlookObjects/MailItem/CidImageResolverTests.cs`.
- Tests: 4511/4511 passing (no tests added or removed).

## Section 6 — Unit Test Policy

No unit tests were added or modified (annotation-only). Existing suites remain green
(batch-a..i regression evidence; final 4511/4511). COM/VSTO-bound files respect the ratified
coverage exemption; the sole non-exempt file `CidImageResolver.cs` retains real, non-trivial test
coverage. Verdict: PASS.

## Section 7 — Final Toolchain Pass

| Stage | Command (final) | Result | Evidence |
|---|---|---|---|
| Format | `csharpier .` | PASS | `final-csharpier.2026-07-19T10-50.md` |
| Analyze / codestyle | `msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | PASS (EXIT 0) | `final-analyzer-build.2026-07-19T10-50.md` |
| Type-check (nullable, pragma-only) | isolated `UtilitiesCS.csproj /t:Rebuild ... /p:BuildProjectReferences=false` (plan-literal solution TWAE halts on out-of-scope SVGControl) | PASS (EXIT 0; 0 CS86xx in-scope and total) | `final-nullable-build.2026-07-19T10-50.md` |
| Test (with coverage) | `vstest.console.exe UtilitiesCS.Test ... /EnableCodeCoverage` | PASS (4511/4511) | `coverage-delta`, batch regression evidence |

## Appendix A — Independent Verification Log

- Diff scope: 30 in-scope `.cs`, 0 test files, `.csproj`/`.sln` unmodified — confirmed.
- `#nullable enable` on line 1 of all 30 files — confirmed 30/30.
- `SVGControl` CS0649 pre-existing at merge-base and untouched by #371 — confirmed.
- `dynamic item = itemObj;` in `OlToDoTable.EnsureItemValues` byte-unchanged — confirmed.
- Upstream contracts present at HEAD: `Initializer.GetOrLoad`, `LazyExtension.ToLazy`,
  `PrettyPrint.PrettyText`, `ArrayExtensions.ToStringArray/To2D`, `FilePathHelper`
  (`UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs`) — confirmed.
- Batch-I self-correction (2f6f3fec): ETL/EtlAsync/EtlPrepAsync tuples and `GetTableInViewAsync`
  reverted to non-null; HEAD signatures non-null; final full-solution CS86xx = 0 — confirmed.
- net481-forbidden attributes/keywords in added lines — none found.
- No `.claude/rules/*` or `.github/workflows/**` changes — confirmed.
