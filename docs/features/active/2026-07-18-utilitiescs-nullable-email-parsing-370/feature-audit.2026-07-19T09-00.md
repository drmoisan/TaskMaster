# Feature Audit — utilitiescs-nullable-email-parsing (Issue #370)

- Branch: `feature/utilitiescs-nullable-email-parsing-370`
- Diff base: `df2235bc1716ddf18891ff01f3f283f6da6168b9`
- Work Mode: `full-feature` (marker confirmed in `issue.md`)
- AC Source (per full-feature mode, acceptance-criteria-tracking skill): `spec.md` AND
  `user-story.md` (both consulted; `issue.md`'s copy also reviewed for consistency)
- Timestamp: 2026-07-19T09-00

## Scope and Baseline

Scope: per-file `#nullable enable` pragma opt-in nullable-reference-type remediation,
annotation-only, across the 24 `.cs` files in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/`
(14), `UtilitiesCS/EmailIntelligence/SubjectMap/` (6, excluding the Designer-generated
`SubjectMapMetrics.Designer.cs`), and `UtilitiesCS/EmailIntelligence/Ctf/` (4). Diff base is the
fork point from the epic integration branch, `df2235bc1716ddf18891ff01f3f283f6da6168b9`; 9
commits are in scope (`git log df2235bc..HEAD --oneline`). Baseline state: none of the 24 files
carried `#nullable enable`; repo-wide test suite at 5702/5702 passing, 83.7834% line / 76.3407%
branch coverage; `UtilitiesCS.csproj` carried no `<Nullable>` element.

## Acceptance Criteria Inventory

All three AC source files (`issue.md`, `spec.md`, `user-story.md`) define an identical AC1-AC6
set, all checked off (`[x]`) in all three files at the time of this review:

| AC | Text (abbreviated) |
|---|---|
| AC1 | Every `.cs` file in the cluster that emits CS86xx carries `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with `TreatWarningsAsErrors`. |
| AC2 | No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`. |
| AC3 | No behavior change to parsing/sorting logic; existing tests still pass. |
| AC4 | No coverage regression on changed lines. |
| AC5 | Public signatures of the remediated types remain behavior-compatible; nullability annotations reflect actual null behavior and are consistent with the upstream `utilitiescs-nullable-extensions` annotation contracts they consume. |
| AC6 | Non-remediated files remain non-opted-in and are not cross-blocked; the change is independently mergeable under the per-file pragma architecture. |

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence / Independent Verification |
|---|---|---|
| AC1 | PASS | Independently re-ran the scoped per-file pragma gate (`msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:TreatWarningsAsErrors=true -p:BuildProjectReferences=false`, after building the `SVGControl` dependency without `TreatWarningsAsErrors`): zero CS86xx diagnostics across the full `UtilitiesCS.csproj` compilation, matching `evidence/qa-gates/final-nullable-pragma-gate.md`. Confirmed the literal solution-wide command's failure (`msbuild TaskMaster.sln -t:Rebuild ... -p:TreatWarningsAsErrors=true`) is due solely to a pre-existing `CS0649` x2 in vendored `SVGControl/SvgImageSelector.cs`, unrelated to and unchanged by this branch (`git diff df2235bc..HEAD -- SVGControl` is empty; the last commit touching that file, `c194362d`, is confirmed via `git merge-base --is-ancestor` to be an ancestor of the diff base). Independently confirmed each of the 24 target files carries `#nullable enable` via `grep`. |
| AC2 | PASS | Independently re-grepped `UtilitiesCS/UtilitiesCS.csproj` for `Nullable` — zero matches, matching `evidence/qa-gates/final-ac2-csproj-check.md`. |
| AC3 | PASS | Independently reviewed the full diff of all 24 files (`git diff df2235bc..HEAD -- UtilitiesCS/EmailIntelligence`) — every change is a pragma line, a nullability annotation, or CSharpier reflow; no logic/control-flow change identified. `evidence/qa-gates/final-tests-coverage.md` and all 7 per-batch regression runs report 5702/5702 passing, matching the Phase-0 baseline exactly. |
| AC4 | PASS | Independently spot-checked `evidence/qa-gates/final-coverage-delta.md`'s per-file table: no file's covered/total ratio decreased between baseline and post-change; aggregate 24-file cluster coverage improved marginally (87.30% -> 87.47%). Two modified files (`SortEmail.cs`, `TesseractOcrTextExtractor.cs`) sit below an 80% per-file floor but are byte-identical to baseline (36/66 and 1/13 respectively in both baseline and post-change), i.e. zero regression — consistent with AC4's specific "no regression on changed lines" wording (not an absolute per-file floor). |
| AC5 | PASS | Independently reviewed `evidence/qa-gates/final-signature-compat.md`'s per-file table against the actual diff for a sample spanning all 7 batches; confirmed every signature-level change is additive nullability metadata (a `?` on a type, a `!` at a call site, or an unconstrained `T?` generic return) that reflects the member's actual pre-existing null-return/null-input behavior, with no renamed member, no added/removed/reordered parameter, and no non-nullability-related return-type change. Cross-referenced consumption of upstream Wave-0 (`utilitiescs-nullable-extensions`, issue #363) contracts (`NullExtensions.ThrowIfNull<T>`, `StringExtensions.IsNullOrEmpty`, `IEnumerableExtensions.Transpose<T>`) — all consumption sites compile clean under the per-file pragma, confirming contract consistency. |
| AC6 | PASS | Independently ran `git diff df2235bc..HEAD --name-only -- UtilitiesCS` — exactly the 24 target files are listed, all within the three cluster directories; `SubjectMapMetrics.Designer.cs` is absent from the diff. No other production file (including any file outside `UtilitiesCS/EmailIntelligence/{EmailParsingSorting,SubjectMap,Ctf}`) was given a `#nullable enable` pragma or any nullable-related edit. |

### Scope-Invariant Compliance (supports AC3/AC5/AC6)

- **File-size non-split guard**: independently re-measured with `wc -l` — `SortEmail.cs` (1408
  lines), `EmailTokenizer.cs` (730 lines), `SubjectMapEntry.cs` (658 lines) remain single,
  unsplit files exceeding the pre-existing 500-line general limit, as explicitly scoped out of
  this annotation-only remediation by the plan's Scope Invariants. Confirmed no additional `.cs`
  file was created by a split (24 files touched, no new file in the diff).
- **Struct non-conversion guard**: independently located `FolderStruct`
  (`EmailDataMiner.FolderExtraction.cs:18`, `internal struct FolderStruct(...)` primary
  constructor) and `SpamBayesOptions` (`EmailTokenizer.cs:707`, `public struct SpamBayesOptions`)
  — both remain plain structs; no `record`/`record struct`/`init` conversion found by grep.
- **Designer-file non-modification guard**: `git diff df2235bc..HEAD --
  UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapMetrics.Designer.cs` is empty.
- **No prohibited nullable post-condition attributes / polyfills**: independently re-grepped the
  diff and the whole repository for `NotNullWhen|MaybeNullWhen|NotNullIfNotNull|MaybeNull|
  AllowNull|DisallowNull|DoesNotReturn|MemberNotNull` and for a
  `namespace System.Diagnostics.CodeAnalysis` polyfill declaration — zero matches on both.

### Plan Execution Verification

All 54 atomic tasks across Phases 0-8 in `plan.2026-07-18T22-05.md` are checked off (`[x]`).
Every referenced evidence artifact path was confirmed to exist under `evidence/baseline/`,
`evidence/qa-gates/`, `evidence/regression-testing/`, and `evidence/other/` (41 evidence files
total, enumerated and spot-read in this review). Plan-text discrepancy noted: task P6-T4
mis-attributes `TryLoadObjectAndGetMemorySize<T>` and `FolderStruct` to
`EmailDataMiner.Transform.cs` (actual locations: `EmailDataMiner.Serialization.cs` and
`EmailDataMiner.FolderExtraction.cs` respectively) — independently confirmed via `grep`; this is
a documentation-only discrepancy with no functional impact, since all 4 Batch F files were
remediated together as the mandatory single partial-class batch regardless of which sub-bullet
named which file (self-disclosed and correctly reasoned about in `batch-f-nullable-gate.md`).

## Acceptance Criteria Check-off

All six AC items (AC1-AC6) were already checked off (`[x]`) in `issue.md`, `spec.md`, and
`user-story.md` prior to this review, and this audit's independent verification confirms each
is a genuine PASS, not merely a self-reported checkmark. No further check-off action was
required or taken by this review (per the acceptance-criteria-tracking skill's "check off only
what you find PASS and still unchecked" rule — nothing was unchecked here).

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-18-utilitiescs-nullable-email-parsing-370/issue.md`,
  `docs/features/active/2026-07-18-utilitiescs-nullable-email-parsing-370/spec.md`,
  `docs/features/active/2026-07-18-utilitiescs-nullable-email-parsing-370/user-story.md`
- Total AC items: 6 (per source file; identical across all three files)
- Checked off (delivered): 6 / 6 (all three files)
- Remaining (unchecked): 0
- Items remaining: none

## Summary

All six acceptance criteria are independently verified PASS with concrete, reproduced evidence
(re-executed msbuild gates, re-derived grep/wc-l scope scans, cross-checked coverage deltas).
All 54 plan tasks are complete and their evidence artifacts exist and are internally consistent.
Two self-reported executor findings (plan-text mis-attribution in P6-T4; CS8625 side effect on
3 pre-existing test files) were independently confirmed accurate and correctly dispositioned as
non-blocking. The transient `TaskMaster.cli.runsettings` edit was independently confirmed to
leave zero net diff (MD5 `214be06fbfaf1aee387da41e907f4fb4` matches before/after). No blocking
gaps identified. This feature is ready to merge as an independently-mergeable Wave-1 child of
the `utilitiescs-nullable-remediation` epic.
