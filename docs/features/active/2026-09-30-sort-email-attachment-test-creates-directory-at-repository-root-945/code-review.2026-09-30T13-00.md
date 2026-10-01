# Code Review: sort-email-attachment-test-creates-directory-at-repository-root (#945)

- Review timestamp: 2026-09-30T13-00
- Reviewer: feature-review agent
- Branch: `bug/sort-email-attachment-test-creates-directory-945`, head `cb502eeec14dc32ba087149bd90693ef4a5fc0a0`
- Base: merge base `039cf779110df3313b3324299d019cabfccce980`
- Work mode: `minor-audit`; acceptance criteria source is `issue.md` `## Acceptance Criteria`
- Verification basis: Read, Grep and Glob only. Source statements are re-derived from the files; test and coverage outcomes are evidence-attested from the feature evidence tree.

## Executive Summary

Verdict: **PASS**. 0 blocking findings, 5 non-blocking findings.

The change replaces a direct `System.IO.Directory.CreateDirectory` call in `TrySaveAttachmentAsync`
with a call through an injected `Action<string>` delegate. The two-parameter method is kept as a thin
delegation that supplies the real call, so the three production call sites
(`SortEmail.cs` lines 819, 864, 879) are untouched. The test is rewritten so that it reaches no file
system API, and a second test covers the delegate-failure path. The design is the smallest seam
permitted by `.claude/rules/csharp.md` (injectable delegate seam, second in order of preference) and
adds no static mutable state.

Code-level checks performed:

- `SortEmail.cs` lines 888 to 961: the wrapper is non-async and returns the core task, so no extra state machine and no change to exception flow. The core calls `createDirectory(Path.GetDirectoryName(filePathSave))` inside the existing `try`; the `catch` filter remains `UnauthorizedAccessException`, so an `IOException` from the delegate propagates, which is exactly what the new test asserts. The retry branch passes `createDirectory` to the recursive call (line 961), so a retry uses the same seam instead of silently reverting to the real call.
- `SortEmail_Tests.cs` lines 236 to 289: `Path.Combine` and the rooted literal are pure string operations; the recording delegate appends to a local list; the save callback records the path. Ordered assertion `events.Should().Equal("mkdir:" + dir, "save:" + path)` makes the mkdir-before-save order a checked property, and the negative control proved the assertion discriminates (failure message contains the missing `mkdir:` entry).
- No `Directory.`, `File.` write, `GetTemp*`, `Thread.Sleep` or `Task.Delay` in the test file.
- Naming, XML documentation, Arrange-Act-Assert comments and FluentAssertions use match the surrounding file.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low (non-blocking) | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | whole file (1454 lines) | File exceeds the 500-line limit. It was 1429 lines before this item (caller-supplied) and is 1454 now; the item adds 25 lines. The breach is pre-existing, not introduced by this item. | Owe a follow-up that splits attachment saving, message saving and cleanup out of `SortEmail.cs`. Do not fold it into this fix. | `.claude/rules/general-code-change.md` File Size Limit; a bug-fix seam should not carry a file-split refactor. | Grep line count 1454 at head; caller baseline 1429. |
| Low (non-blocking) | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | lines 893 and 912 | Both overloads carry `[ExcludeFromCodeCoverage]`. The new core overload is newly written code that is attribute-exempt, although its success and IOException paths are now unit-tested. Only the catch block, which calls `YesNoToAll.ShowDialog`, justifies the exemption. | Extract the read-only prompt behind a seam in a follow-up so the attribute can be removed and the core measured. | The attribute is per-method and reviewable, so it is not a Blocking exclusion under the Coverage Exclusion Policy; the scope decision in `issue.md` retained it deliberately. | `SortEmail.cs` lines 893, 912, 936; `coverage-final.md` `SORTEMAIL-UNCOVERED-DELTA: 0`. |
| Nit | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | line 921 | `createDirectory` is not null-guarded. A null delegate would raise `NullReferenceException` inside the `try`, which is not caught by the `UnauthorizedAccessException` filter and so propagates. | None required; the method is `internal` and its only callers pass a lambda or a test delegate. Add a guard only if the overload becomes public. | `.claude/rules/csharp.md` prefers `internal` for non-public APIs; risk is confined to the assembly and its test project. | `SortEmail.cs` lines 913 to 921. |
| Nit | `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` | line 238 | The sandbox literal is a Windows drive path (`C:\...`). It is valid for this Windows-only net48 solution and never touches disk, but a reader may mistake it for a real location. | None required; the explanatory comment at lines 236 to 237 already states the intent. | Determinism and isolation are met; portability is not a repository goal for this project. | Test file lines 236 to 238; `SANDBOX-EXISTS-AFTER: False` in `test-run-final.md`. |
| Low (non-blocking) | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | lines 894 to 904 | The two-parameter wrapper, including its lambda `path => System.IO.Directory.CreateDirectory(path)`, has no direct test. It is correct by inspection and attribute-exempt; its only risk is a typo in the default, which a build would not catch. | Accept; an integration test that calls it would create a directory, which this item exists to prevent. | Testing the real default requires the real file system, which the repository test policy prohibits. | `SortEmail.cs` lines 899 to 903. |

## Acceptance Criteria Inventory

Source: `issue.md` `## Acceptance Criteria` (work mode `minor-audit`). Eight items, AC1 to AC8, all
checked `[x]` by the executor before this review.

## Acceptance Criteria Evaluation

| AC | Code-level result | Basis |
|---|---|---|
| AC1 | PASS | Overload declared `internal static Task<bool> TrySaveAttachmentAsync(this Attachment, string, Action<string>)` (lines 913 to 917); calls `createDirectory(Path.GetDirectoryName(filePathSave))` (line 921); recursive call passes `createDirectory` (line 961). Re-derived. |
| AC2 | PASS | Wrapper (lines 894 to 904) is a single `return TrySaveAttachmentAsync(attachment, filePathSave, path => System.IO.Directory.CreateDirectory(path));`. Call sites at lines 819, 864, 879 unchanged; no static delegate or shared mutable seam. Re-derived. |
| AC3 | PASS | Test at lines 245 to 266: rooted literal via const and `Path.Combine`, recording delegate, ordered `events` assertion, `saved` true, `Times.Once`. No file-system API. Re-derived. |
| AC4 | PASS | Test at lines 272 to 289: delegate throws `IOException`, `ThrowAsync<IOException>`, `Times.Never` for `SaveAsFile`. A Grep shows no `UnauthorizedAccessException` in `SortEmail_Tests.cs`. Re-derived. |
| AC5 | PASS | `negative-control-createdirectory-removed.md` records exit 1 with both tests failing on the expected messages; the restore is recorded with an equal SHA-256 (`p1-t17`, `p2-t10`). Evidence-attested. |
| AC6 | PASS | `test-run-final.md`: total 15, passed 15, failed 0; baseline 14. Evidence-attested. |
| AC7 | PASS | `toolchain-pass.md` one clean pass; `coverage-final.md` deltas 0.0138, 0.0266 and 0.0091 percentage points (band 0.10), `SortEmail.cs` uncovered delta 0. Evidence-attested. |
| AC8 | PASS | Footprint in `p2-t10-scope-boundary` lists exactly the two source files plus feature-folder paths; inherited paths are `.claude/agent-memory/**` and the promoted potential entry. The branch reflog shows one implementation commit after the main merge. Evidence-attested and corroborated. |

## Follow-ups (not filed)

1. Split `SortEmail.cs` below the 500-line limit.
2. Extract the read-only `ShowDialog` prompt from `TrySaveAttachmentAsync` behind a seam, then remove `[ExcludeFromCodeCoverage]` from both overloads and add tests for the `UnauthorizedAccessException` branches.
3. Promote the same injectable-delegate approach to other `SortEmail` helpers that still call `Directory` or `File` APIs directly, if any test reaches them (none found in this review).

## Verdict

**PASS.** No blocking findings; no remediation inputs produced.
