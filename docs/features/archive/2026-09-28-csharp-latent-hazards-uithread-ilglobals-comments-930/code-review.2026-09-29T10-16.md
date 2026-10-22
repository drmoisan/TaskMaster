# Code Review: csharp-latent-hazards-uithread-ilglobals-comments (Issue #930), Remediation Cycle 1 Re-audit

- Timestamp (caller-supplied artifact stamp): 2026-09-29T10-16
- Branch: `bug/csharp-latent-hazards-uithread-ilglobals-comments-930` against `origin/main` (self-anchor `ac819907f479ee18026993054e714dc2e056142f`; cycle anchor `39845d4a3f2f38d5c018f41e15e8372d432553c5`)
- Scope: the full branch diff (six code files: four production, two test) plus the cycle-1 documentation changes inside the feature folder.
- Method: Read and Grep only (no Bash, per caller instruction). Code files were reviewed in full in the prior audit (2026-09-29T00-45); the cycle changed none of them (r1-footprint.md `OUTSIDE_TRACKED_CHANGES=0`; r1-toolchain-exemption.md `CODE_OR_CONFIG_TRACKED_CHANGES=0`). This review spot-verified the fixed defects in the files on disk and reviewed the cycle's documentation edits.

## Executive Summary

The code change set is unchanged and remains a minimal, targeted bugfix set: one boolean operand with a why-comment in `UiThread.cs`, two field deletions in `ILGlobals.cs`, two numeral removals in XML doc comments, and three discriminating tests (the two regression tests were recorded failing on unmodified source). The cycle-1 change is documentation only: seven path-prefix substitutions with the placeholder `VS-INSTALL-ROOT` in five files (two committed evidence summaries and the quotations inside the three 2026-09-29T00-45 audit artifacts). The Non-blocking finding from the prior review (absolute install path in committed evidence, contradicting AC7) is resolved: a Grep of the whole feature folder for `\b[A-Za-z]:[\\/]\S` returns zero matches, and the same pattern returns 26 matches on the raw coverage log, so the zero is not a blind pattern. Each substituted file is proven text-exact by a hash chain (expected hash computed from the pre-edit text with only the prefix replaced, equal to the post-edit hash in r1-subst-1 to r1-subst-5). No new finding was introduced by the cycle.

Counts: Blocking 0; Non-blocking 0; Informational 9 (carried forward; NB-1 resolved).

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Informational | UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs | line 3 | `using System.Collections.Generic;` appears unused after the `Dictionary<int, object> Cache` deletion. | Remove on a later touch of the file; not required for merge. | Minimal-fix rule permits leaving it; no enabled analyzer flags it (0 warnings). | Prior-audit Grep of the file for collection type names returned zero matches; analyzer Rebuild 0 warnings (final-04-analyzers.md). |
| Informational | UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs | former lines 113 and 131 | Removal of the public members `ILGlobals.Cache` and `ILGlobals.modules` is a breaking public API change with no in-repo consumer. | Name both removed public members in the PR body. | General Code Change Policy section 7: call out breaking changes clearly in the change description. | Reviewer Grep over `*.cs` for `ILGlobals\.(Cache|modules)\b|public static .*\b(Cache|modules)\b` over the worktree: zero matches; 863-build-green.md (0 errors solution-wide). |
| Informational | UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs | lines 292 to 313 | `PublicStaticFields_AreExactlyTheTwoOpCodeTables` pins the public static field surface by name, so any later public static field fails it. | None required; the XML doc states the intent. | Deliberate, documented surface pin. | Prior-audit read of the test source. |
| Informational | UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs | whole file | The file hosts two test classes; its name matches only the second. Pre-existing from #816. | Optional later split; outside this branch's scope. | File cohesion guidance; the plan placed the new test in the existing class to reuse its class-level `[DoNotParallelize]` (Decision D12). | File lines 22 to 24 and 153 to 155. |
| Informational | UtilitiesCS/UtilitiesCS.csproj; VBFunctions.csproj; SVGControl.Test.csproj | `<Analyzer Include>` items (UtilitiesCS.csproj line 1316) | Analyzer HintPath versions (Meziantou.Analyzer 3.0.235, MSTest.Analyzers 4.4.0) differ from packages.config (3.0.290, 4.4.1); pre-existing on origin/main. | Open a follow-up issue to align the `<Analyzer Include>` versions with packages.config. | Fresh worktrees fail the analyzer Rebuild with CS0006 until the older packages are installed manually. | baseline-02-analyzers.md; no tracked project file changed by the branch or the cycle. |
| Informational | evidence/baseline/*.md (Phase 0 artifacts) | `Timestamp:` fields | Several Phase 0 `Timestamp:` values were estimates later replaced by file write times; the disclosure does not enumerate them. | Enumerate the corrected artifact names in reduced-audit-handoff.md when the folder is next revised. | No figure or AC depends on a Timestamp value; the sequence is monotone. | reduced-audit-handoff.md; git history (`df86ec9e`, `699ad109`). |
| Informational | evidence/qa-gates/final-06-mstest-coverage.md; evidence/qa-gates/coverage-comparison.md | package-level delta | Six UtilitiesCS lines outside the changed files flipped missed-to-covered between runs (0.009 points). | None; recorded so the delta is not read as an effect of the change. | Within known run-to-run variance. | Prior-audit re-summation of both JaCoCo projections. |
| Informational | evidence/qa-gates/final-06-mstest-coverage.md | Decision D3 | Four shell-icon test classes are excluded from both local coverage runs; the runner exposes no filter extension point. | Candidate follow-up to add a filter or hang-timeout parameter to `Invoke-MSTestWithCoverage.ps1`. | Identical exclusion on both sides; CI runs the classes unfiltered. | plan Decision D3; final-06-mstest-coverage.md. |
| Informational | evidence (PR context) | not applicable | PR-context artifacts are absent from the review worktree; scope was derived from footprint evidence and files on disk. | None. | Environment limitation. | r1-footprint.md; footprint.md. |

Numbering note: the authoritative Informational list is I-1 to I-9 in policy-audit.2026-09-29T10-16.md, as in the prior audit. The rows above are the code-review view of that list. The surface-pin row (test lines 292 to 313) is a design observation that the policy audit folds into section 1.3; the policy audit's I-8 (ILGlobals.cs file-level line coverage 95.00% to 94.74% because two covered initializer lines were deleted, not a changed-line regression) is a coverage-arithmetic note not repeated as a row here. Neither difference changes the counts: Blocking 0, Non-blocking 0, Informational 9.

## Detailed Review

### Cycle 1 documentation edits (new in this re-audit)

- Placeholder counts read by Grep for `VS-INSTALL-ROOT`: baseline-04-mstest-coverage.md 1, final-06-mstest-coverage.md 1, code-review.2026-09-29T00-45.md 1, policy-audit.2026-09-29T00-45.md 2, feature-audit.2026-09-29T00-45.md 2. Together with the substitution records this equals the seven baseline hit rows in r1-drive-scan-baseline.md. The remaining Grep hits for the placeholder are in the remediation plan, remediation inputs and r1 evidence files, which describe the placeholder without any path.
- The two evidence summaries were re-read: line 11 (baseline) and line 12 (final) now read `Runner output: Using vstest.console: VS-INSTALL-ROOT\Common7\IDE\Extensions\TestPlatform\vstest.console.exe; Discovered 9 test assemblies.`. The adjacent figures are intact: baseline `lines 56079/65737 (85.31%), branches 13593/17052 (79.71%)`, `Total tests: 7320. Passed: 7320.`; final `lines 56084/65736 (85.32%), branches 13597/17054 (79.73%)`, `Total tests: 7322. Passed: 7322.`.
- The hash chain (r1-file-state-baseline.md, r1-subst-1 to r1-subst-5) is a sound proof of text-exactness. The expected hash is derived from the pre-edit text by applying only the path-prefix regular expression, so a post-edit hash equal to it excludes any other edit. Line counts equal the baseline values (229, 230, 64, 270, 68). The path mask is narrow (a drive letter, `Program Files`, `Microsoft Visual Studio`, two path segments, lookahead `Common7`), so it cannot rewrite unrelated text.
- The gate is non-vacuous: synthetic backslash and slash paths match, a URL does not, and the raw log yields 26 hits (baseline and final), matching the reviewer's independent control of 26.
- The cycle also edits the three prior audit artifacts in place, replacing quoted path text. That is appropriate here: the quotations were themselves committed AC7 violations, and the edit changes no verdict, count or finding text.
- issue.md: only the AC7 marker changed (hash equals EXPECTED-TEXTHASH-6, r1-ac-status.md). Reviewer read of issue.md confirms `[x]` on AC1 to AC7 (lines 44 to 50) and no other change to criterion text.
- Root-cause treatment: the prior gate enumerated identity patterns (account, host, worktree root, drive-rooted Users directory). The remediation states the invariant (no drive-rooted path anywhere in the folder) and gates on a general pattern with a positive control, which is the correct fix rather than adding one more named directory.

### UtilitiesCS/Threading/UiThread.cs (unchanged; spot-verified)

Reviewer Grep finds `&& _dispatcher is not null` at line 184 (captured-context exit) and line 199 (dispatcher exit), the same operand shape in both exits. With `_dispatcher` null and no dispatcher on the executing thread, the former `ReferenceEquals(null, null)` evaluated true; the added conjunct removes that path and can only make the exit more restrictive. The prior audit confirmed all twelve pre-existing `IsCompleted` tests still pass and the nullable Rebuild reports 0 CS86xx.

### UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs (unchanged; spot-verified)

Reviewer Grep over `*.cs` for a remaining `Cache` or `modules` reference or public static declaration returns zero matches. The two remaining public statics are `readonly` and published once from the static constructor. `using System.Reflection;` is still required; `using System.Collections.Generic;` is unused (Informational).

### QuickFiler/Viewers/*.Search.cs (unchanged; spot-verified)

Reviewer Grep of `QuickFiler/Viewers` for `(487|481) lines` returns zero matches; the explanatory sentence about the 500-line ceiling is retained, so the drift-prone numeral is gone and the rationale remains.

### Tests (unchanged)

The #889 test arranges the five conditions AC1 enumerates and asserts `observed.Should().BeFalse()` with a `thrown.Should().BeNull()` guard and `observed` initialized to true. The two #863 tests assert non-empty init-only public statics and the exact name set. All three were recorded failing before the fix and passing after. No `DoNotParallelize`, worker-count, retry, sleep or temporary-file change (`ADDED_*=0`, runsettings hash unchanged).

### Introduced-by-cycle check

Checked and none found: no tracked change outside the feature folder and `.claude/agent-memory/` (`OUTSIDE_TRACKED_CHANGES=0`, `PORCELAIN_OUTSIDE_ALLOWED=0`); no code or configuration path (`CODE_OR_CONFIG_TRACKED_CHANGES=0`, `CODE_OR_CONFIG_PORCELAIN=0`, control 24 markdown rows); no drive-rooted path in any new artifact (whole-folder Grep zero, including the plan, remediation inputs and all r1 files); no account, host or worktree-root disclosure (r1-sanitize-final.md zeros with controls). Procedural note, not a finding: the cycle's edits and the r1 evidence files are uncommitted in the working tree at review time (r1-footprint.md, `PORCELAIN_IN_FEATURE=17`), and the caller instructed this review not to commit; they must be committed in the exempt docs-only form before the PR.
