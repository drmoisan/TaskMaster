# Code Review: Issue #614 post-remediation cycle 2

**Review Date:** 2026-08-27
**Reviewer:** Codex feature-reviewer-c3-elevated
**Feature Folder:** `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`
**Feature Folder Selection Rule:** Explicit canonical folder supplied by the orchestrator and corroborated by `artifacts/pr_context.summary.txt`.
**Base Branch:** `main` / `origin/main` at `8b70208032519d82fe838009a5ce280f18b277f9`
**Head Branch:** `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614` at `8188cff9537125255bdd0415ce4b9b701c138c99`
**Merge Base:** `c279d40bddacdba00c29a9724d1b5b17f9ebbc90`
**Review Type:** Full post-remediation re-review; 147 files, 13,007 insertions, 243 deletions.

## Executive Summary

The complete feature-to-main diff was reviewed without language or acceptance-criteria narrowing. The C# implementation closes the original rooted-path leak through a shared archive-stem contract, guards both filing-boundary overloads, hardens Outlook-to-filesystem conversion, validates archive and OneDrive roots explicitly, and separates filing-selection rules from folder-creation rules. Remediation cycle 2 correctly removes the cycle-1 rooted-selection widening that could pass a rooted value to `ResolvePaths`, retains one- and two-character relative filing stems, and adds a cross-assembly composition test. No implementation blocker or regression was found in the cycle-2 code.

The canonical final test artifact combines two runs with different expected exit codes. The evidence parser therefore reports the authoritative 6,586/6,586 successful run as a normalized failure in the fresh PR context. This is an auditability defect, not a failure of the underlying C# run. The change description also retains one inaccurate follow-up reference. After this review and before documentation remediation or PR authoring, the user approved a `scope_change` accepting exactly these two findings as risks and excluding them from remaining remediation scope. PR readiness is therefore **Pass with Accepted Risk**; neither finding is treated as corrected, and the normalized evidence row remains failed.

**What changed:**

- `ArchiveStemContract` centralizes rootedness and separator-bounded archive-relative conversion.
- `BreadcrumbBridgeRouter` rejects out-of-root row and hierarchy selections and preserves prior selection state on rejection.
- `EmailFilerConfig`, `EfcDataModel`, and `FolderConverter` enforce the contract at consumer boundaries.
- `ArchiveRootPathGuard` and `ResolveOneDriveRoot` replace silent fallback with explicit redacted failure.
- `EfcSelectionGuard` now rejects rooted filing inputs, accepts one- and two-character relative stems, and retains the three-character minimum only for folder creation.
- Targeted MSTest suites and fail-before/pass-after evidence cover the named defect set.

**Top 3 risks:**

1. Canonical PR context normalizes the cycle-2 final test gate as failed because one evidence file carries incompatible per-run expectation metadata.
2. The change description assigns the unguarded archive-root-resolution crash to #637, although the promoted follow-up for that distinct defect is #638.
3. The deliberate fail-fast archive-root and OneDrive behavior remains unverified against a live Outlook profile; the required manual record explicitly documents non-execution and automated counterparts.

**PR readiness recommendation:** **Pass with Accepted Risk** — proceed to PR authoring with both findings disclosed. Do not edit either affected file solely for these findings. Strict completion validation may still reject the unchanged artifacts; if it does, preserve and report that blocker.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major — accepted risk | `docs/features/.../evidence/qa-gates/final-test-coverage.2026-08-26T22-27.md` | top-level `EXIT_CODE: 0`; later `ExpectedExitCode: 1` | The artifact records the authoritative successful run and a separate expected-failure raw-preservation run in one file. The evidence contract permits one expectation per file; canonical PR context consequently pairs exit 0 with expectation 1 and reports `Normalized result: fail`. | The technically available correction is to keep only the authoritative run in the final-test artifact, move the preservation run to its own schema-valid artifact, and refresh PR context. The user-approved scope change excludes that remediation; preserve and disclose the normalized failure. | PR authoring and review consume the normalized evidence rows. A passing underlying run does not make the canonical row pass. The user accepted that auditability consequence at `2026-08-27T02:11:56.137Z`. | `artifacts/pr_context.summary.txt:407-412`; evidence-and-timestamp-conventions, one-expectation-per-file rule; the artifact itself records 6,586/6,586 for the authoritative run; checkpoint requirement `issue-614-approved-documentation-findings-scope-change`. |
| Minor — accepted risk | `docs/features/.../change-description.2026-08-26.md` | line 313 | The root-unresolvable `EfcDataModel.MoveToFolderAsync` read is described as deferred to #637. #637 covers producer-side `SelectRow` normalization; the distinct archive-root-resolution crash was promoted as #638. | The technically available correction is to change only this root-resolution reference to #638 while preserving the correct #637 reference on line 315. The user-approved scope change excludes that remediation; preserve and disclose the inaccurate reference. | The current text sends maintainers to the wrong follow-up issue and conflicts with the checkpoint's later reachability adjudication. The user accepted that documentation consequence at `2026-08-27T02:11:56.137Z`. | `docs/features/potential/promoted/...efc-unguarded-archive-root-read-crashes-ui-thread.md:1-8,54`; `...breadcrumb-selectrow...md:1-10`; change description lines 313 and 315; checkpoint requirement `issue-614-approved-documentation-findings-scope-change`. |
| Info | `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | lines 21, 118-215 | The alternative-folder prompt cluster remains a closed production call cycle; its D5f fix is covered but the UI option has no external production entry point. | Retain as a separately promoted cleanup; do not widen #614. | This is pre-existing dead code, not a regression introduced by cycle 2. | Repository reference scan for `AlternativeFolderPrompt`, `AskUserForAlternatives`, and `BuildAlternativesDictionary`; prior review residual. |
| Info | `TaskMaster/AppGlobals/AppOlObjects.cs`; `QuickFiler/Controllers/EfcDataModel.cs` | archive-root property and lines 289/310/328 | Explicit archive-root resolution can throw through unguarded EFC reads. The behavior is intentional for AC13, but the UI-bound handling gap is real and promoted as #638. | Address through #638; do not fold it into this evidence-only remediation. | The condition predates cycle 2 and is now independently tracked with the correct scope. | Promoted issue #638 document and orchestrator reachability adjudication. |
| Info | `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` | lines 20-39, 193-272 | The constructor/field injection seam is not used by tests or production; testability comes from the static resolver's delegate argument. | Consider removing the unused constructor/field in a separate cleanup. | This is maintainability debt and does not defeat AC14 behavior or coverage. | Reference scan shows no call to the internal constructor; resolver tests invoke `ResolveOneDriveRoot` directly. |
| Info | `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | lines 77-127 and 268 | The legacy folder-name validator applies fewer rules than the new segment validator, and one `ArgumentException` uses `nameof(fsPath)` for a local rather than a public parameter. | Consolidate validators or remove the dead cluster; use `nameof(olBranchPath)` if the live exception contract is revised. | Both are non-blocking residuals carried from the initial full review. | Direct inspection of `FindInvalidSegmentRule`, `IsLegalFolderName`, and the throw at line 268. |
| Info | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | lines 1000-1069 | The structurally similar `SortEmail.ResolvePaths` overloads remain outside the new boundary contract. | Track separately; downstream conversion still rejects out-of-root input. | This is an existing parallel path not added or modified by #614. | Reference scan finds three live calls and no `RequireArchiveRelativeStem` in these overloads. |

No implementation Blocker was found. The Major evidence finding remains open and disclosed, but the recorded user-approved scope change removes it from remediation scope and accepts its PR-context consequence.

## Implementation Audit

### C# implementation audit

#### What changed well

- `ArchiveStemContract.TryMakeArchiveRelative` is pure, separator-bounded, ordinal case-insensitive, and returns no caller input on failure.
- Producer and consumer defenses are explicit: hierarchy activation commits only a valid non-empty stem, out-of-root direct rows are rejected, and both `EmailFilerConfig.ResolvePaths` overloads validate before concatenation.
- Cycle 2 restores agreement between `IsValidFilingSelection` and `RequireArchiveRelativeStem` while preserving the separate creation-length rule.
- `FolderConverter` validates only derived segments and permits legitimate punctuation in the caller-supplied OneDrive root.
- New files are explicitly included in the legacy non-SDK project files.

#### Type safety and API notes

- `ArchiveStemContract` and `EfcSelectionGuard` use `#nullable enable`; nullable input is guarded before dereference.
- Public contract methods use explicit exception parameter names and XML documentation. The one local-name `ParamName` residual is recorded above.
- No `record`, `init`, unsafe code, or new suppression was introduced.
- Analyzer and nullable rebuild evidence record zero errors at the cycle-2 code head.

#### Error handling and logging

- Failure messages with potentially identifying Outlook or filesystem values are redacted.
- Router rejection leaves selection unchanged and emits fixed diagnostic text.
- Boundary failures use `ArgumentException`; unresolvable configuration roots use `InvalidOperationException` after logging.
- The unguarded UI propagation case is explicitly separated into promoted issue #638.

## Test Quality Audit

The test design is broad and deterministic. It includes fail-before evidence for the original filing-boundary defect and producer segment activation, positive and negative contract cases, case-variant and separator-boundary cases, redaction assertions, no-root and cross-store cases, and an explicit composition test proving every accepted filing guard value survives `EmailFilerConfig.ResolvePaths`.

### Reviewed test and QA artifacts

- `evidence/regression-testing/revert-expect-fail.2026-08-26T22-18.md` and `revert-pass-after.2026-08-26T22-20.md` — prove the cycle-1 rooted-selection regression fails before and passes after the partial revert.
- `evidence/regression-testing/rc4-getstem.2026-08-26T22-21.md` — 42/42 targeted tests pass and covers the previously untested fallback branch.
- `evidence/regression-testing/p4-t1-integration.2026-08-26T22-22.md` — 6,109 tests across the three affected assemblies pass with `LiveOutlook` excluded.
- `evidence/qa-gates/final-test-coverage.2026-08-26T22-27.md` — underlying authoritative result is 6,586/6,586 with 84.8841% line and 78.8692% branch coverage; evidence metadata defect recorded above.
- `evidence/qa-gates/coverage-delta.2026-08-26T22-28.md` — deletion-adjusted and retained-line gates pass; `EfcSelectionGuard` and `ArchiveStemContract` remain at 100% line/branch for the reported measures.
- `evidence/qa-gates/manual-validation.2026-08-26T18-55.md` — all five live-Outlook steps are explicitly NOT EXECUTED with reasons and named automated counterparts, satisfying the criterion's recorded-non-execution clause.

### Quality assessment prompts

- **Determinism:** No changed test contains `Thread.Sleep`, `Task.Delay`, wall-clock reads, randomness, temporary-file creation, process-environment mutation, network access, or a live Outlook dependency.
- **Isolation:** Pure contracts and resolver seams are tested without filesystem, COM, or process state; collaborator boundaries use Moq where needed.
- **Speed:** The authoritative full suite completed successfully and the targeted integration set passed independently; the #592 timeout appears only in a separately described diagnostic collection.
- **Diagnostics:** FluentAssertions because-messages and exception redaction checks identify the exact violated contract.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Cycle-2 redaction sweep reports only fabricated `example.com`, `testuser`, and `Contoso` placeholders. |
| No unsafe subprocess or command construction | N/A | No subprocess construction was added in the C# change. |
| Input validation at boundaries | PASS | `RequireArchiveRelativeStem`, `TryMakeArchiveRelative`, segment validation, and root resolvers cover the named boundaries. |
| Error handling remains explicit | PASS with follow-up | Explicit redacted failures are present; UI propagation is tracked as #638. |
| Configuration / path handling is safe | PASS | Prefix anchoring, separator boundaries, case-insensitive matching, UNC/short-root tests, and redaction checks are present. |

## Research Log

No external research was required. Repository policy, canonical PR context, source diff, feature evidence, and promoted follow-up documents were sufficient.

## Verdict

The C# implementation and cycle-2 partial revert are technically sound at head `8188cff9537125255bdd0415ce4b9b701c138c99`; the prior blocking composition defect is closed, and no new implementation blocker was found. Canonical PR context still represents the final test gate as failed, and the change description still names #637 where #638 is accurate. The recorded user-approved `scope_change` accepts exactly those two consequences and excludes their documentation remediation. The branch is ready for PR authoring with both risks disclosed; no remediation inputs or plan are required by this adjudication. If strict completion validation refuses the unchanged findings, that refusal remains a lifecycle blocker rather than authorization to implement the waived changes.
