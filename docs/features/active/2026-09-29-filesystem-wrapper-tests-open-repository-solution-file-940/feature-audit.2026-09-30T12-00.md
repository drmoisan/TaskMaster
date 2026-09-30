# Feature Audit: filesystem-wrapper-tests-open-repository-solution-file (Issue #940)

- Review timestamp label: `2026-09-30T12-00`
- Work mode: `minor-audit` (issue.md line 12). AC source: the `## Acceptance Criteria` section of `issue.md` only (AC1 to AC8, lines 64 to 71). AC8 is evaluated as amended on 2026-09-30 by the coordinator ruling recorded in the dated note at issue.md line 73 and in the plan's Revision Log entry A; the amendment is treated as authorized per the caller's instruction.
- Blocking findings: **0**. AC status: 8 of 8 PASS.

## Scope and Baseline

- Branch head: `bd71b8160e281054657280af8a7c54eeffe5c563` (worktree loose ref). Base: `origin/main` at `66afa6372fd82fc1ffd7c81f85a1ad65eebc5817`, merged into the branch at `40e587ce20bbd41cd6915707271faae937f1a8e2` (reflog entry `merge origin/main: Merge made by the 'ort' strategy`, epoch 1790779114). The merge changed no path under `UtilitiesCS/` or `UtilitiesCS.Test/` (P2-T7 `MERGE-UCS-DIFF-EXIT: 0`).
- Diff against the base (executor's verbatim `git diff --name-only origin/main...HEAD`, 46 paths): `UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs`, `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs`, 43 paths under the feature folder, and the inherited promotion record `docs/features/potential/promoted/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file.md`. No production, project, runsettings, coverage-config or workflow path.
- Baseline (Phase 0, pre-edit tree at `231e1c0b55105aeb626bf5a6e8d0266a567cacad`, then the same source trees at the merged base): scoped run 12/12 passed; full suite 7323/7323 passed; first-party coverage 85.32% lines (56084/65736), 79.73% branches (13597/17054); per-file PDA 81/91 lines and 36/42 branches, PFA 69/75 and 6/12, DIW 123/123 and 3/4; pre-edit census PFS `GetRepositoryRoot` 6, `TaskMaster.sln` 13, `catch (IOException)` 2, DIW `GetRepositoryRoot` 5, `TaskMaster.sln` 3.
- Post-change (merged head, MEASUREMENT 3): scoped run 15/15 passed; full suite 7327/7327 passed; first-party 85.33% lines (56092/65736), 79.73% branches (13597/17054); per-file PDA 91/91 and 36/42, PFA 71/75 and 6/12, DIW 123/123 and 3/4 (reviewer-verified against the `<class>` nodes of `coverage/baseline-940.cobertura.xml` and `coverage/final3-940.cobertura.xml`).
- Reviewer tooling: Read, Grep, Glob only (no shell, per caller instruction). PR context artifacts are absent from the item worktree; scope was taken from the executor's three-dot listings, which agree with the caller's branch facts and the worktree reflog.

## Acceptance Criteria Inventory

| ID | Criterion (abridged; full text in issue.md lines 64 to 71) | State on disk |
| --- | --- | --- |
| AC1 | No test in either file locates the repository root or references `TaskMaster.sln`; both `GetRepositoryRoot` helpers and `GetSolutionFile` removed | `[x]` |
| AC2 | No test in either file catches `IOException` or any broader type | `[x]` |
| AC3 | No mutating call at the repository root or a tracked entry; each mutating member exercised only against a Moq mock, an asserted-absent owned path, or an owned entry on which it is a no-op by construction | `[x]` |
| AC4 | Every previously exercised member still exercised, delegation asserted against Moq, an owned read-only fixture or a documented non-existent-path outcome; `Create`, `Refresh`, `SetAccessControl` no-ops complete without exception; coverage of the changed wrapper and adapter lines does not regress | `[x]` |
| AC5 | No temporary file or directory; no `DoNotParallelize`, worker-count or scope change, retry or sleep | `[x]` |
| AC6 | Negative-control run shows each rewritten test fails when its delegation is deliberately broken; control reverted before the final QC pass | `[x]` |
| AC7 | The three additional root-walk sites classified in an evidence artifact; none modified | `[x]` |
| AC8 (amended 2026-09-30) | C# toolchain passes in order with no new failures relative to baseline; covered lines and branches not lower than baseline in each of the three changed wrapper and adapter files; first-party line >= 80% and branch >= 75% | `[x]` |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence and reviewer verification |
| --- | --- | --- |
| AC1 | PASS | Read both files in full: no `GetRepositoryRoot`, `GetSolutionFile`, `TaskMaster.sln` or `AppDomain` token; DIW fixture is `RootedFixturePath` plus the owned image; PFS fixtures are the owned image, its directory and prefixed missing paths. P2-T8 census: all four tokens 0 in both files (baseline 6/3/13/2 and 5/0/3/1). P2-T10 `ADDED-ROOTWALK: 0`. |
| AC2 | PASS | `catch` token 0 in both files (baseline PFS 2); read confirms no try/catch; P2-T10 `ADDED-CATCH: 0`. Exception outcomes are asserted with `Should().Throw<T>()`. |
| AC3 | PASS | `p2-t14-ac3-mutating-call-inventory.2026-09-30T11-31.md`: 34 PFS call sites (owned output directory 4, its parent 2, existing owned entry 2, missing owned path 26, mock 0) and 16 DIW call sites (all mock, all inside the three Moq tests); no call names a repository path. Reviewer re-derived the PFS classes from the file: lines 48 to 58 (owned directory and parent no-ops), 200 to 206 and 216 (missing directory), 236 to 238 (missing directory), 332 to 333 (owned image no-ops), 379 to 386 and 397 (missing file), 420 to 426 (missing file with destinations asserted absent at 414 to 417). Note CR-1 (code review): `SetAccessControl` is an idempotent DACL write admitted by the criterion text. |
| AC4 | PASS | `p2-t15-ac4-form.2026-09-30T11-32.md`: fifteen FORM rows over the four admitted forms, each backed by a `RESULT ... = Passed` line of `test-run-final.md`; `Create`, `Refresh`, `SetAccessControl` each in a `no-op-no-throw` row. Member census: every member token at or above its pre-edit count (for example `.Refresh()` 2 -> 4, `.IsReadOnly` 2 -> 3, `.MemberCount` 0 -> 2, `.Open(` 3 -> 3, `.CopyTo(` 2 -> 2, DIW `.GetFiles(` 7 -> 7). Coverage of the three carried files: PDA 81 -> 91 lines / 36 -> 36 branches, PFA 69 -> 71 / 6 -> 6, DIW 123 -> 123 / 3 -> 3 (reviewer-verified in the Cobertura class nodes). |
| AC5 | PASS | Read: no `Path.GetTemp`, `File.Create`, `File.WriteAll`, `Directory.CreateDirectory`, `DoNotParallelize`, `[Timeout`, `Thread.Sleep`, `Task.Delay`, retry. P2-T10: the ten prohibition counts 0 over 276 added lines; `OUT-OF-SET-DIFF-EXIT: 0` (neither runsettings file, `coverage.config` nor the project file changed); `RUNSETTINGS-HASH-NOW` equals the P0-T4 hash. Scoped runs under Workers 0 / ClassLevel passed twice (P1-T7, P2-T5). |
| AC6 | PASS | Eleven records `evidence/regression-testing/mutation-*.md` (C1 to C11), one per rewritten test (7 PFS tests, 4 retargeted DIW tests; the constructor test and the three Moq tests were not rewritten, as the P1-T4 edit spans show). Each: production mutation compiled (`PROD_CSC_OUT_LINES: 2`), the predicted test failed with the predicted phrase (all eleven `MESSAGE` lines read), revert with `REVERT-DIFF-EXIT: 0`, hash equal to the `PRE-EDIT-HASH-` anchor, confirming pass. P1-T31: `PRODUCTION-DIFF-EXIT: 0`, porcelain empty, five hashes equal anchors before Phase 2. P2-T10 re-confirms the three production hashes at closure. |
| AC7 | PASS | `evidence/other/root-walk-site-classification.2026-09-30T07-27.md`: four `SITE:` sections covering `RibbonControllerTests.cs` (legitimate repository-file read), `FSharpCoreHintPathAlignmentTests.cs` (legitimate repository-file read), `SortEmail_Tests.cs` string-only consumers (layout dependence without file-system I/O) and the `TrySaveAttachmentAsync` site (same defect class; follow-up F-3); every section `MODIFIED: NONE`. P2-T10 `OUT-OF-SET-DIFF-EXIT: 0` with a pathspec naming the three files; `UCS-TEST-CHANGED` excludes `SortEmail_Tests.cs`. |
| AC8 (amended) | PASS | Toolchain in order: `dotnet tool run csharpier check .` exit 0 (P2-T2); analyzer rebuild exit 0 (post-merge P2-T7a, `SKIP_CORECOMPILE_LINES: 0`); TreatWarningsAsErrors rebuild exit 0 (post-merge P2-T7b); MSTest with coverage MEASUREMENT 3 7327/7327, `NEWLY-FAILING: NONE`. Per-file rule: the three `final3` FILE lines read `NOT-LOWER=True` on lines and branches, and the reviewer confirmed the same figures from the Cobertura class nodes. Floors: 85.33% lines (>= 80) and 79.73% branches (>= 75). The comparator's in-memory negative control reads `NOT-LOWER=False` on all six lowered variants with the document hash unchanged. Caveat (non-blocking, G-1 / CR-7): the format check was not re-run post-merge; the two changed files are hash-identical to the checked state and CI's format check runs on the PR head. |

Assumptions recorded:

- The AC8 amendment is accepted as authorized on the caller's instruction; the reviewer verified that the amendment text in issue.md line 71 and line 73 matches the plan's Revision Log entry A and that `coverage-final.md` preserves the superseded MEASUREMENT 1 and 2 records with their `AC8: NOT MET` outcome, so the history of the change is auditable.
- The merged-tree format state is delegated to CI; no local evidence exists for the merged files that arrived from origin/main.

## Acceptance Criteria Check-off

All eight AC items were already checked `[x]` in `issue.md` by the executor (P2-T12 to P2-T19). Every item evaluates PASS above, so no item is unchecked by this review and no new check-off is required. `issue.md` was not modified by the reviewer.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/issue.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

## Summary

- Verdict: **PASS**. Blocking findings: **0**. All eight acceptance criteria are supported by evidence that the reviewer re-verified against the source files, the committed evidence artifacts, the on-disk Cobertura documents and the worktree's git files.
- Non-blocking items carried to the orchestrator: CR-1 to CR-7 (code review), G-1 to G-3 (policy audit), follow-ups F-1 to F-3 (seam `SetAccessControl`; constructor null-guard branch tests for `PhysicalFileInfoAdapter`; promote the `SortEmail_Tests.cs` `TrySaveAttachmentAsync` root-walk site).
- PR-time gates: CI format check on the merged head; PR context regeneration by pr-author; `validate_evidence_locations.py`.
- No remediation-inputs artifact is produced.
