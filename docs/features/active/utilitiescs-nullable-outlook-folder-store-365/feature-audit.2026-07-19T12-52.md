# Feature Audit — utilitiescs-nullable-outlook-folder-store (Issue #365)

- Timestamp: 2026-07-19T12-52
- Reviewer: feature-reviewer
- Work mode: full-feature -> AC sources are `spec.md` and `user-story.md` (both carry AC1-AC7; identical text)
- Branch: `feature/utilitiescs-nullable-outlook-folder-store-365`

## Scope and Baseline

- Baseline (resolved merge-base): `dffadd5a` (epic integration tip, PR #382 merge). Verified `git merge-base HEAD dffadd5a == dffadd5a` (clean, no divergence).
- Head: `e00a2c21`.
- Diff: 63 production `.cs` files under `UtilitiesCS/OutlookObjects/Folder/` (incl. `MsgToMime/`) and `UtilitiesCS/OutlookObjects/Store/`; no test files; no csproj/props/targets/packages.config/sln edits; feature docs + evidence + one evidence `.gitignore`.
- Upstream Wave-0 dependencies (#363, #364) confirmed landed at baseline (`evidence/baseline/baseline-upstream-dependency-note.md`; all 6 upstream files carry `#nullable enable`), so annotation choices at cross-file call sites were made against real upstream contracts.
- Each AC verdict below was verified independently against the diff and the evidence, not by trusting the pre-checked `[x]` boxes.

## Acceptance Criteria Inventory

| ID | Criterion (abridged) |
|---|---|
| AC1 | Every CS86xx-emitting Folder/Store file carries `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma gate with `TreatWarningsAsErrors`. |
| AC2 | No project-level `<Nullable>` element in `UtilitiesCS.csproj`; no `/p:Nullable=enable` global flag in verification. |
| AC3 | No behavior change; existing `UtilitiesCS.Test` suite still passes. |
| AC4 | No coverage regression on changed lines; COM-bound coverage-exempt files annotated without new tests. |
| AC5 | Public signatures remain behavior-compatible; annotations reflect actual null behavior. |
| AC6 | No `System.Diagnostics.CodeAnalysis` post-condition attribute added; no new `record`/`record struct`/`init`. |
| AC7 | Each partial-class group remediated in the same commit/batch with consistent shared-member nullable shape. |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence and independent verification |
|---|---|---|
| AC1 | PASS | 63/63 changed production files carry `#nullable enable` at HEAD (per-file `git show`). The 18 verify-only files confirmed clean at baseline. Full-solution `/t:Rebuild /p:TreatWarningsAsErrors=true` exits 1 only from 2 pre-existing `CS0649` in vendored `SVGControl` (not in this branch's diff -> definitionally pre-existing, sibling #368) plus 15 pre-existing `CS0618`/`CS0168` in non-Folder/Store files. Scoped `UtilitiesCS.csproj /t:Rebuild /p:BuildProjectReferences=false` is a valid CS86xx/CS87xx signal (Roslyn single-pass; SVGControl is a ProjectReference failing before UtilitiesCS compiles) and reports zero nullable diagnostics for the cluster. Late CS8766 fix (`95b289c5`) confirmed as a one-line nullable-return widening. Evidence: `final-nullable-pragma-gate.md`, `baseline-nullable-pragma-gate.md`. |
| AC2 | PASS | Independently verified: `grep -ci "<Nullable" UtilitiesCS/UtilitiesCS.csproj` = 0; `git diff dffadd5a HEAD -- UtilitiesCS/UtilitiesCS.csproj` is empty. No verification command in evidence uses `/p:Nullable=enable`. |
| AC3 | PASS | `evidence/qa-gates/final-tests-coverage.md`: 4511/4511 passed, exit 0. Independently confirmed no test files changed in the diff, so the pass reflects the unmodified suite against annotated production code. Coverage Cobertura present at HEAD-consistent counts. |
| AC4 | PASS | Changed-line coverage 96.97% (96/99); the 3 uncovered changed lines are `?`/`!` edits to statements already uncovered at baseline (no covered line became uncovered). Repo-wide flat/slightly up (+0.01% line). No new tests or runtime guards added to COM-exempt files (no test files changed; verified). Evidence: `final-coverage-delta.md`, reconciled to Cobertura root. |
| AC5 | PASS | Spot-checked signatures are additive-only: `FolderNavigator.GetOutlookFolder` -> `Folder?` (with `!` at the one forced call site); `StoreDisableService(..., IStoreRehookService? rehook = null)`; `OlFolderlist_GetAllRet` -> `string[]?`. No parameter added/removed, nothing renamed. Two non-null initializers (`DisabledStoreRow` strings -> `= string.Empty`; `FolderTree._roots -> = new()`) are behavior-neutral (see code-review MINOR notes). Evidence: `final-signature-compat.md`. |
| AC6 | PASS | Independently grepped the diff's added lines: zero post-condition attributes (`NotNullWhen`/`MaybeNullWhen`/etc.), zero new `record`/`record struct`/`init`. The only `record` token in `OutlookFolderHierarchyReader.cs` is a lambda parameter name; the `sealed record StoreRehookResult` is pre-existing in a verify-only file. Evidence: `final-no-postcondition-attrs-and-records.md`. |
| AC7 | PASS | `StoresWrapper.cs` + `StoresWrapper.Filtering.cs`: both in a single commit (`5f1778de`). `FolderPredictor.cs` + `FolderPredictor.IFolderSearchHandler.cs`: both partials pragma-enabled together in F3a (`14f5fab9`, P4-T1); member annotations (all shared members live in `FolderPredictor.cs`) in F3d (`49bc28dd`). Full-solution rebuild confirms partial-class nullability consistency. Substantive requirement met. Note: the `final-ac7-partial-group-check.md` artifact misattributes both FolderPredictor partials to a single F3d commit — a documentation nit corrected in the code review, not an AC failure. |

## Acceptance Criteria Check-off

- Authoritative full-feature AC sources `spec.md` and `user-story.md` already carry AC1-AC7 as `[x]`, consistent with the PASS verdicts above; no source-file check-off change is required.
- `issue.md` retains its AC section as `[ ]`; for `full-feature` mode `issue.md` is not the authoritative AC source, so it is left unmodified.

## Acceptance Criteria Status

- Source: `spec.md`, `user-story.md`
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none

## Residuals for the Maintainer (informational, non-blocking)

1. Analyzer package/version mismatch on the integration branch: committed `UtilitiesCS.csproj`/`packages.config` reference older analyzer DLL paths than the bumped Meziantou/BannedApi/Sonar package versions. The executor worked around it locally by placing older-referenced DLLs into the gitignored `packages/` folder. Independently confirmed no tracked file changed for this workaround (`git diff dffadd5a HEAD -- UtilitiesCS/UtilitiesCS.csproj packages.config` is empty). Pre-existing and outside this feature's scope; flag for repo-wide remediation.
2. `FolderWrapper .cs` filename contains a literal trailing space before `.cs`. Pre-existing naming defect; not renamed (out of annotation-only scope). Recommend a dedicated cleanup issue.
3. Two Designer-generated files (`DisabledStoresViewer.Designer.cs`, `StoreWrapperViewer.Designer.cs`) remain non-opted-in per repo convention, which conflicts with the epic manifest's file list. Flagged by the executor for the maintainer; not resolved here (epic-manifest edit out of scope).
4. Repo-wide C# coverage (65.31% line / 61.35% branch) is below the 85%/75% floors — pre-existing legacy VSTO/COM debt, dispositioned non-blocking via the ratified CLAUDE.md exemption (see policy audit). The canonical `artifacts/csharp/coverage.xml` is absent; equivalent numeric evidence is in the feature Cobertura.
5. Global `/p:Nullable=enable`-vs-per-file-pragma rules-versus-convention conflict is deferred to the Wave-2 CI capstone child, per spec. Not actionable here.

## Summary

All seven acceptance criteria are independently verified as PASS. The change is annotation-only, behavior-preserving, and scoped exactly to the Folder/Store cluster. Toolchain evidence (format, analyzers, nullable gate, 4511/4511 tests) is consistent with the diff. No blocking findings in any of the three review artifacts.

- Overall AC result: 7 PASS / 0 PARTIAL / 0 FAIL / 0 UNVERIFIED.
- `blocking_count` (FAIL + blocking-PARTIAL across all three artifacts): **0**. (The C# repo-wide coverage FAIL is dispositioned non-blocking per the policy audit and is excluded from `blocking_count`.)
