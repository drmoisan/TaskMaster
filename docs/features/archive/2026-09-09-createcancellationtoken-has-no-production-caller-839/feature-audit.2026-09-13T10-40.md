# Feature Audit — Issue #839 (createcancellationtoken-has-no-production-caller)

- Timestamp: 2026-09-13T10-40
- Branch: bug/createcancellationtoken-has-no-production-caller-839
- Head: 36c65b1875f89c0e3d885dc52324b60959ae3dbb
- Merge base with main: 2405a829d6afd3b12eb7c228d57158a97cb4e2ca
- Work mode: `full-bug` (issue.md:12 `- Work Mode: full-bug`; spec.md:9; user-story.md:6). AC source: `spec.md` only. `user-story.md` is narrative, carries no checkboxes (reviewer read), and is not treated as an AC source; its presence is not a defect.

## Scope and Baseline

Baseline: merge base 2405a829d6afd3b12eb7c228d57158a97cb4e2ca (recorded at evidence/baseline/base-anchor.md:17 as byte-equal to the plan's BASE-SHA literal). At the baseline, `QfcHomeController.cs` is exactly 500 lines, `Init()`'s first statement is the datamodel-loader call, and `CreateCancellationToken()` has zero production invocations on `QfcHomeController` (pre-fix-facts.md:16, 26, 28).

Branch diff (caller-verified, 43 paths):

- `QuickFiler/Controllers/QfcHomeController.cs` (+1/-2)
- `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` (+71/-0)
- 41 Markdown documents and evidence projections under `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/`

Reviewer method: Read, Grep and Glob only (no shell, no git, by caller directive). Where a criterion depends on a git-derived fact (byte-identity of an untouched file; the name-only path list), the evidence is the caller's verified listing plus the executor's anchored-diff artifact, and that reliance is stated in the row.

Assumptions recorded:

1. The caller's pasted anchored diff and 43-path name-only listing are accurate for head 36c65b187. Cross-checks: the live files match the post-image of the diff line for line; the executor's scope-and-footprint.md (37 paths at [P3-T23]) plus the six artifacts written afterwards (commit-1.md, commit-2.md, ac-status-summary.md, evidence-sanitization.md, post-checkoff-revalidation.md, scope-and-footprint.md itself) reconcile to 43.
2. The plan-file check-off marks and the three commits (commit-1 SHA 3b6cd70b4 recorded at commit-1.md:8; commit-2 and the check-off commit reported in the executor's return) are the only history above the base; the reviewer cannot enumerate commits without git.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/spec.md`, lines 211-222, under `## Acceptance Criteria`. Twelve checkbox items, all `[x]` on arrival.

| ID | Criterion (abbreviated) | State on arrival |
|---|---|---|
| AC1 | `CreateCancellationToken();` is the first statement of `Init()`, precedes the datamodel loader; nothing else added or reordered | [x] |
| AC2 | File at most 500 lines; the removed line is one of the two identified dead comments; no executable line removed | [x] |
| AC3 | New test `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt` asserts the enumerated seven conditions with FluentAssertions | [x] |
| AC4 | Fail-before projection exists with non-zero exit, names the test failing on the not-null assertion, has Timestamp and Command; no raw TRX/XML | [x] |
| AC5 | Pass-after projection exists, exit 0, zero failed, lists the new test and `Init_InitializesCorrectly` as passed | [x] |
| AC6 | Same projection lists `Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource` as passed; CleanupTests.cs byte-identical to base | [x] |
| AC7 | New test calls `_controller.Cleanup()` after assertions; no Sleep/Delay/timer/filesystem/Outlook object; doc comment names #839, the ordering rule and the QfcFormViewer debt | [x] |
| AC8 | `Init_InitializesCorrectly` unchanged (five loaders, four `Assert.AreEqual`); only a trailing `Cleanup()` permitted | [x] |
| AC9 | Exactly five `CreateCancellationToken()` invocations across QuickFiler/ and QuickFiler.Test/; family total seven | [x] |
| AC10 | Name-only diff lists only Write Set paths; named out-of-scope files absent; no .xml/.trx/.coverage added | [x] |
| AC11 | Coverage projections exist; per-file before/after percentages recorded; after >= before; inserted statement covered | [x] |
| AC12 | Remedy (d) follow-up enumerated in Rollout & Follow-up naming five symbols; not filed from this branch; no promotion artifact in the diff | [x] |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence relied on | Notes |
|---|---|---|---|
| AC1 | PASS | Reviewer read of QfcHomeController.cs:86-107: line 88 `CreateCancellationToken();` immediately after the opening brace, line 89 `_datamodel = QfcDataModelLoader(Globals, this.Token);`; remaining statements at lines 90-106 are in the base order. Caller's anchored diff hunk `@@ -85,6 +85,7 @@` shows exactly one added line in `Init()`. production-file-gates.md:13-24, 29. | No other statement added or reordered: the production numstat is 1 added / 2 deleted and the second hunk is at base lines 462-469, outside `Init()`. |
| AC2 | PASS (with recorded deviation) | Reviewer read: file ends at line 499. file-size-audit.md:10 `QFC_LINES=499`. Caller's diff second hunk removes `//public QfcFormViewer FormViewer { get => _formViewer; }` and one blank line; pre-fix-facts.md:29 confirms that comment was base line 465, one of the two candidates named in the criterion. | Deviation on a literal reading: two lines were removed, not one. The second is a blank line whose removal is formatter-canonical (base lines 464 and 466 were both blank; CSharpier collapses consecutive blanks, and the format pass reported `FORMAT_CHANGED_OWNED_PATCH=False`, final-format.md:14, 29). The single non-blank line removed is the identified dead comment; no executable line was removed; the file is under 500. The criterion's substantive conditions hold; the count deviation is documented by plan Decision D1 and is accepted. |
| AC3 | PASS | Reviewer read of QfcHomeControllerTests.cs:172-230. Method name and namespace `QuickFiler.Controllers.Tests.QfcHomeControllerTests` (line 20, 23). Assertions: line 224 `capturedSource.Should().NotBeNull()`; 225 `formControllerToken.Should().Be(capturedSource.Token)`; 226 `_controller.TokenSource.Should().BeSameAs(capturedSource)`; 227-228 datamodel token equals and `CanBeCanceled` true; 229-230 queue token equals and `CanBeCanceled` true. test-file-gates.md:23-27 token counts (1, 3, 1, 2). | All seven enumerated conditions present, all FluentAssertions. |
| AC4 | PASS | Reviewer read of init-token-source-fail-before.md: `Timestamp: 2026-09-13T05-47` (line 3), `Command:` (lines 4-5), `EXIT_CODE: 1`, `ExpectedExitCode: 1` (lines 6-7), `FAILED_Init_CreatesTokenSourceBeforeAnyLoaderObservesIt=1` (line 18), `NOTNULL_MESSAGE_LINES=1` and the verbatim message `Expected capturedSource not to be <null>.` (lines 21, 27). Raw-output condition: no .trx/.xml/.coverage in the 43-path diff (caller-verified; scope-and-footprint.md:70). | The run was taken against the unfixed assembly built at [P1-T2] with the test already compiled in (`Total tests VALUE=1`), which is the correct fail-before arrangement. |
| AC5 | PASS | init-token-source-pass-after.md: `EXIT_CODE: 0` (line 6), `Total tests VALUE=1394`, `Passed VALUE=1394`, `Failed VALUE=0` (lines 11-13), `PASSED_Init_CreatesTokenSourceBeforeAnyLoaderObservesIt=1` and `PASSED_Init_InitializesCorrectly=1` (lines 17-18), both names listed in the passed block (lines 28-29). | Whole QuickFiler.Test assembly on the post-fix tree; population = baseline 1393 + 1. |
| AC6 | PASS | init-token-source-pass-after.md:19 `PASSED_Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource=1`, line 30 lists it. Byte-identity: `QfcHomeControllerCleanupTests.cs` is absent from the caller's 43-path name-only listing and from the caller's numstat (only two source files), and sibling-and-followup-gates.md:15 records an empty `git diff --stat` for that path against the base. | The byte-identity half rests on the caller's git listing and the executor's stat diff; the reviewer could not run git. Both sources agree. |
| AC7 | PASS | Reviewer read: `_controller.Cleanup();` at line 233 after the assertions (224-230); no `Thread.Sleep`, `Task.Delay`, timer, filesystem or Outlook interop object in lines 172-234 (the `Mock<Outlook.Application>` at line 43 is pre-existing `Setup()` code, unchanged). Doc comment lines 165-171 contains `Issue #839`, "creates the cancellation token source before the datamodel loader runs", and "Init() constructs a real QfcFormViewer, which neither test replaces" naming `Init_InitializesCorrectly`. test-file-gates.md:28-33. | All four AC7 conditions verified directly from the source. |
| AC8 | PASS | Reviewer read of lines 112-163: five loader replacements (122, 125, 129, 133, 136) and four `Assert.AreEqual` (151, 152, 157, 158) present. Caller's anchored diff for the test file is a single pure-insertion hunk after line 164; numstat 71/0 (test-file-gates.md:44-48). `Assert.AreEqual(` count 10 unchanged from base (test-file-gates.md:31). | No trailing `Cleanup()` was added (Decision D2); the permitted-but-optional edit was not made, which is compliant. |
| AC9 | PASS | Reviewer grep of `CreateCancellationToken\(\)` over `QuickFiler/**/*.cs` and `QuickFiler.Test/**/*.cs`: EfcHomeController.cs:62, 126, 162 (invocations), 399 (declaration); QfcHomeController.cs:88 (invocation), 466 (declaration); QfcHomeControllerMetricsTests.cs:124 (invocation). Five invocations, two declarations, family total seven. family-count.md:13-34 reports the identical set. | Baseline was six family / four invocations (pre-fix-facts.md:18-27); post-change seven / five, exactly as the criterion requires. |
| AC10 | PASS | Caller's verified name-only listing: 43 paths, all matching the three Write Set entries. scope-and-footprint.md:11, 23-58 (37 paths at that point, all Write Set), lines 62-66 (each named out-of-scope file checked absent), line 70 (no .xml/.trx/.coverage). Reviewer's reconciliation of 37 + 6 later artifacts = 43. | The reviewer could not run `git diff --name-only`; the verdict rests on the caller's listing and the executor's anchored artifact, which agree. No path under `docs/features/potential/` appears (scope-and-footprint.md:79). |
| AC11 | PASS | coverage-baseline.md exists (Timestamp 2026-09-13T05-42) with `QFC_LINE_PCT=77.91` (line 16); coverage-comparison.md exists (Timestamp 2026-09-13T06-14) with before 77.91 / after 77.99 as percentages (line 12), after >= before, and `LINE88_HITS=1` for the inserted statement, whose text is quoted at line 24 as `CreateCancellationToken();`. | The +1 valid / +1 covered movement is fully explained by the diff. Method parity between the two sides is documented (coverage-comparison.md:38-45). |
| AC12 | PASS | Reviewer read of spec.md:236: the Rollout & Follow-up bullet names `RibbonController.LoadQuickFiler()`, `QfcHomeController.Init()`, `IQfcHomeController.Init()`, `CreateCancellationToken()` and `Init_InitializesCorrectly`, and records the ordering constraint. No promotion artifact in the diff (no `docs/features/potential/` path in the caller's listing; sibling-and-followup-gates.md:29; scope-and-footprint.md:79). | Informational: `IQfcHomeController.cs` exists twice (Controllers/ declares `Init()` at line 12; Interfaces/ does not). The follow-up should name the Controllers path (code-review Info finding). Not an AC12 defect; the five symbols are named as required. |

Summary of verdicts: 12 PASS, 0 PARTIAL, 0 FAIL, 0 UNVERIFIED.

## Additional Verification Beyond the Acceptance Criteria

- Reachability claim (latent, not user-visible): reviewer grep for word-bounded `LoadQuickFiler` across `.cs`, `.xml`, `.vb`, `.ps1`, `.json` in the item worktree returned only the declaration at RibbonController.cs:97 plus archived coverage documents under `docs/features/archive/`. RibbonController.cs:104-108 is the only `.Init()` call on a `QfcHomeController` in production. The spec's characterisation holds.
- Guard sites: `_tokenSource is null` at QfcFormController.Actions.cs:38, 75, 131 (reviewer grep), unchanged, matching the spec's inverse constraint.
- Precedent: `EfcHomeController` calls its identical factory at lines 62, 126 and 162 (reviewer grep).
- Toolchain: the four-step C# toolchain passed on pass number 1 with `/t:Rebuild` and `CSC_TASK_LINES=18` on both msbuild gates (toolchain-final-pass.md; final-analyzers.md:11; final-nullable.md:11, 14).
- CI parity of the test invocation: `.github/workflows/_mstest-coverage.yml:99` passes `/EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` and no settings file; the plan's Decision D16 aligned the local runs with that, and both sides of every comparison were measured under the aligned method.
- Hygiene: reviewer grep of the feature folder for the account name and absolute path patterns returned zero hits; the executor's sanitisation gate agrees with a positive control (evidence-sanitization.md:9-18).

## Acceptance Criteria Check-off

All twelve criteria were already `[x]` in spec.md on arrival (ac-status-summary.md:11-21 records the twelve individual transitions at [P3-T14] through [P3-T25], each against a named artifact). The reviewer evaluated each independently above and found every one PASS, so no check-off state was changed and no criterion was unchecked. No new criterion was added; no criterion text was modified.

Newly checked off by this review: none (all were already checked and each check-off is confirmed as evidence-backed).

Left unchecked by this review: none.

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/spec.md
- Total AC items: 12
- Checked off (delivered): 12
- Remaining (unchecked): 0
- Items remaining: none

## Summary

Verdict: **PASS**. 12 of 12 acceptance criteria verified against the diff and the evidence artifacts; every spec.md check-off is supported by a concrete, readable projection. One recorded deviation (AC2: the diff removes the identified dead comment plus one adjacent blank line rather than exactly one line; formatter-canonical, no executable line removed, file at 499). Zero blocking findings. No remediation-inputs artifact is required. The change is ready to merge.

Follow-ups owed by the caller after merge, none of which conditions this verdict: file remedy (d) naming `QuickFiler/Controllers/IQfcHomeController.cs`; file the runsettings-parallelism item; file the tracked-`.trx` cleanup item (see code-review Info findings).
