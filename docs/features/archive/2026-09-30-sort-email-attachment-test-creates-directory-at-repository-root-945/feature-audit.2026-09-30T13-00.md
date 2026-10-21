# Feature Audit: sort-email-attachment-test-creates-directory-at-repository-root (#945)

- Review timestamp: 2026-09-30T13-00
- Reviewer: feature-review agent
- Work mode: `minor-audit` (marker at `issue.md` line 12); AC source is the `## Acceptance Criteria` section of `issue.md`
- Verdict: **PASS**, 8 of 8 acceptance criteria verified

## Executive Summary

All eight acceptance criteria are supported by evidence on disk. Source-level criteria (AC1 to AC4)
were re-derived by reading `SortEmail.cs` and `SortEmail_Tests.cs`. Run-level criteria (AC5 to AC7)
are evidence-attested from the committed evidence tree because this review executed no test, build or
coverage run and the raw coverage documents are not committed. AC8 is evidence-attested from the
recorded footprint and corroborated by the branch reflog and the branch ref (head
`cb502eeec14dc32ba087149bd90693ef4a5fc0a0`). No criterion is left unchecked.

## Scope and Baseline

- Audit scope: full branch diff against the merge base `039cf779110df3313b3324299d019cabfccce980` (`origin/main` at merge). Footprint: `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`, `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs`, and the feature folder. Inherited paths: five `.claude/agent-memory/**` files and the promoted potential-entry file.
- Baseline: Phase 0 measurements taken after the mid-run merge of `origin/main` (reflog: merge at 12:07 local, baseline at 12-23 local): scoped class 14 tests; repository run 7330 of 7330; first-party coverage 85.34% lines, 79.73% branches; `UtilitiesCS` package 38827/43423 lines and 9413/11271 branches.
- Coverage artifacts: the raw Cobertura documents are not committed; the figures below come from `evidence/baseline/coverage-baseline.md` and `evidence/qa-gates/coverage-final.md`.
- No caller narrowing of scope was detected.

## Acceptance Criteria Inventory

Source: `issue.md` `## Acceptance Criteria`.

- AC1: `internal static` overload with `Action<string> createDirectory`, used in place of the direct call, passed through the retry.
- AC2: two-parameter method reduced to one delegation; no caller edited; no static settable delegate.
- AC3: rewritten success test with a rooted literal path, recording delegate, ordered assertion, result true, one save.
- AC4: new IOException propagation test; no test throws `UnauthorizedAccessException`.
- AC5: negative control failing on the recorded-event assertion; production restored byte-identical.
- AC6: scoped run 15 total, 15 passed, 0 failed.
- AC7: full toolchain in one pass; `SortEmail.cs` uncovered-line delta at most 0; package and repository rates within 0.10 percentage points.
- AC8: no source file changed other than the two named.

## Acceptance Criteria Evaluation

| AC | Status | Evidence and basis |
|---|---|---|
| AC1 | **PASS** | Re-derived. `SortEmail.cs` line 913 declares `internal static async Task<bool> TrySaveAttachmentAsync(this Attachment attachment, string filePathSave, Action<string> createDirectory)`; line 921 `createDirectory(Path.GetDirectoryName(filePathSave));`; line 961 `return await TrySaveAttachmentAsync(attachment, filePathSave, createDirectory);`. No remaining direct `Directory.CreateDirectory` in the core (the only occurrence in the file is the wrapper lambda at line 902, per Grep). |
| AC2 | **PASS** | Re-derived. Lines 894 to 904: single `return` of the three-argument call with `path => System.IO.Directory.CreateDirectory(path)`. Production call sites at lines 819, 864 and 879 use the two-argument form and are unchanged; no `static` delegate field or property was added (Grep for `createDirectory` returns only the parameter uses at lines 908 to 961). |
| AC3 | **PASS** | Re-derived. `SortEmail_Tests.cs` lines 238 to 266: literal `C:\Sortemail945Sandbox\attachments`, `Path.Combine` with a literal segment, recording delegate `path => events.Add("mkdir:" + path)`, assertions `saved.Should().BeTrue()`, `events.Should().Equal("mkdir:" + dir, "save:" + path)`, `Verify(..., Times.Once)`. The remaining `GetRepositoryRoot()` uses (lines 196, 223, 295, 320) are outside this test. A Grep for `Directory.`, `File.` write and `GetTemp` in the file returned no match. |
| AC4 | **PASS** | Re-derived. Lines 272 to 289: delegate `path => throw new IOException("disk failure")`, `await act.Should().ThrowAsync<IOException>()`, `Verify(x => x.SaveAsFile(It.IsAny<string>()), Times.Never)`. No `UnauthorizedAccessException` or `ShowDialog` appears in `SortEmail_Tests.cs`. |
| AC5 | **PASS** | Evidence-attested. `evidence/regression-testing/negative-control-createdirectory-removed.md`: mutation removed `createDirectory(Path.GetDirectoryName(filePathSave));`, `VSTEST_EXIT_CODE: 1`, 2 of 2 failed; the success test message is `Expected events to be equal to {"mkdir:...", "save:..."}, but {"save:..."} contains 1 item(s) less`, which is the recorded-event assertion. Sandbox absent before and after. Restore: `p1-t16` and `p1-t17` (2 of 2 pass after restore, with the incremental-build caveat recorded); `p2-t10` records the source SHA-256 equal to the post-fix hash. A runtime-red run of the pre-fix code was not performed; a compile-red dossier (CS1501) is recorded as the substitute, with the reason stated. |
| AC6 | **PASS** | Evidence-attested. `evidence/regression-testing/test-run-final.md`: `COUNTERS total=15 executed=15 passed=15 failed=0`; baseline `test-run-baseline.md` 14. Both named tests reported Passed. |
| AC7 | **PASS** | Evidence-attested. `toolchain-pass.md`: CSharpier format (0 rewritten) and check exit 0, analyzer `/t:Rebuild` exit 0 (0 skipped CoreCompile), nullable `/t:Rebuild` exit 0 (0 skipped CoreCompile), DIRECT coverage route exit 0, one iteration, `LOOP: CLEAN PASS`. `coverage-final.md`: `SORTEMAIL-UNCOVERED-DELTA: 0`; `UtilitiesCS` line 0.894157 to 0.894019 (deficit 0.0138 points), branch 0.835152 to 0.834886 (0.0266), repository line 0.853384 to 0.853293 (0.0091); all within the 0.10 tolerance; COMPARABILITY A (equal denominators). Final first-party 85.33% lines and 79.71% branches. 7331 of 7331 tests passed. |
| AC8 | **PASS** | Evidence-attested and corroborated. `p2-t10-scope-boundary.2026-09-30T12-40.md` lists exactly `SortEmail_Tests.cs` and `SortEmail.cs` in the scoped source pair and no path in the tooling-config pair (`TaskMaster.runsettings`, `scripts`, `config`, `coverage.config`). The branch reflog shows a single implementation commit (`fix(tests): inject directory creation ...`) after the main merge and the branch ref resolves to `cb502eeec`. The `.claude/agent-memory/**` paths and the promoted potential file are inherited and are not source files. |

## Acceptance Criteria Check-off

All eight items in `issue.md` were already `[x]`. Each was evaluated PASS above, so all remain checked.
No item was unchecked and no criterion text was edited.

| AC | Checkbox state after review |
|---|---|
| AC1 | `[x]` (confirmed) |
| AC2 | `[x]` (confirmed) |
| AC3 | `[x]` (confirmed) |
| AC4 | `[x]` (confirmed) |
| AC5 | `[x]` (confirmed) |
| AC6 | `[x]` (confirmed) |
| AC7 | `[x]` (confirmed) |
| AC8 | `[x]` (confirmed) |

Newly checked off by this review: none.

The `## Next Step` item "Move to active fix folder / branch" in `issue.md` is unchecked. It is not an acceptance
criterion under the `minor-audit` rule and was left unchanged.

## Summary

### Acceptance Criteria Status
- Source: `docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/issue.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

Non-blocking observations carried from the policy audit and code review: `SortEmail.cs` is 1454 lines
(pre-existing breach of the 500-line limit; 1429 before this item); both overloads are attribute-exempt
from coverage; the 6-line and 3-branch drift inside the `UtilitiesCS` package is unattributed but inside
the ratified tolerance; four shell-icon and OS-browser test classes are excluded from the local run and
run in CI; raw coverage documents are not committed, so coverage figures are evidence-attested.

No remediation is required.
