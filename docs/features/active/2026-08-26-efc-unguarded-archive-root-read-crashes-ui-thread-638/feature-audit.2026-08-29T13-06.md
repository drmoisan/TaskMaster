# Feature Audit — Issue #638 (EFC unguarded archive-root read)

- **Branch:** `bug/efc-unguarded-archive-root-read-crashes-ui-thread-638`
- **Base / merge base:** `ecdb1c84ba8541ab67042985919cfed4df768c01`
- **Head:** `af1b36e2d93c6beeeb98bbe4998d752e1ebfd732`
- **Audit date:** 2026-08-29T13-06
- **Work mode:** `full-bug` — marker read from `issue.md:8`
- **AC source:** `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/spec.md` § Acceptance Criteria, **sole** source
- **`user-story.md`:** correctly absent. Under `full-bug`, `.claude/skills/acceptance-criteria-tracking/SKILL.md` names `spec.md` only. Its absence is not a defect and is not reported as one.

## Verdict

**PASS. 20 of 20 acceptance criteria verified. 0 blocking findings.**

All 20 criteria were arrived at `[x]` in `spec.md`. Each was re-verified against primary evidence —
source reads, the branch diff, the Cobertura file, and the committed gate artifacts — rather than
accepted on the check-off. No criterion was found checked-but-unsupported. This audit changed no
checkbox: none needed setting and none needed contesting.

## Acceptance Criteria Evaluation

| AC | Criterion (abbreviated) | Verdict | Verification performed by this audit |
|---|---|---|---|
| AC1 | `MoveToFolderAsync(string,...)` returns `false` instead of propagating when `ArchiveRootPath` throws `InvalidOperationException` | PASS | Source read at `EfcDataModel.cs:327-330`: `if (!TryGetArchiveRoot(out var olAncestor)) { return false; }`. Test `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing` present at `:46-62` asserting `moved.Should().BeFalse()`. Recorded failing pre-fix and passing post-fix in the paired regression artifacts. |
| AC2 | `OpenOlFolderAsync` completes without throwing and invokes the seam exactly once | PASS | Source read at `:356-360`: seam invoked then `return`, one call on the path. Test at `:92-110` asserts `reported.Should().ContainSingle()`, which is an exactly-one assertion, not an at-least-one assertion. Absence of a throw is proven by the `await` completing before the assertion. |
| AC3 | `OpenFsFolderAsync` completes without throwing and invokes the seam exactly once | PASS | Source read at `:380-384`, identical shape. Test at `:117-135`, same `ContainSingle()` assertion. |
| AC4 | The user-visible diagnostic contains neither a mailbox address nor the archive root path | PASS | The constant at `:264-268` is a fixed two-part string naming the rule; it interpolates nothing. Test `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress` at `:142-162` asserts the captured message contains neither `mailbox@example.com` nor the archive-path literal. The assertion targets the seam's own output, which is the correct target. |
| AC5 | The archive-root guard sits after the OneDrive `SpecialFolders` read in all three methods | PASS | Source read confirms ordering in all three: `MoveToFolderAsync` `:321-325` then `:327-330`; `OpenOlFolderAsync` `:351-354` then `:356-360`; `OpenFsFolderAsync` `:376-379` then `:380-384`. Pinned from the production side by `MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot` plus the two `Open*` equivalents, all `Times.Never()`. Pinned from the untouched side by `EfcHomeControllerLifecycleTests.cs:217` — read directly, still `SpecialFoldersAccessCount.Should().Be(2)`, and the file is absent from the branch diff. |
| AC6 | The `MailInfo is null` guard remains the first check in `MoveToFolderAsync` | PASS | Source read at `:311-314`: it is the first statement of the method body, before the `attachments` local, the conversation lookup and the OneDrive guard. `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot` at `:193-215` asserts `Times.Never()` on the archive-root getter. |
| AC7 | The success path reads `ArchiveRootPath` exactly once per call | PASS | `TryGetArchiveRoot` contains a single read at `:284`, and each caller invokes it once. `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` at `:171-186` asserts `VerifyGet(..., Times.Once())`. See CR-1 in the code review for a non-blocking robustness note about that test's stopping barrier; the `VerifyGet` itself is sound. |
| AC8 | The catch is narrowed to `InvalidOperationException`; a `COMException` still propagates | PASS | Source read at `:287`: `catch (InvalidOperationException ex)`, the only catch added. Grep of the changed hunks confirms no `catch (Exception)` and no `catch (COMException)`. `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` at `:248-262` asserts `await act.Should().ThrowAsync<COMException>()`. |
| AC9 | Both documented throw conditions are covered, not only one | PASS | Two tests inject `InvalidOperationException` carrying, respectively, the unresolvable-root text and the cross-store/renamed text, matching the constants at `ArchiveRootPathGuard.cs:13-17`. Recorded with a note: production dispatches on exception type and never inspects the message, so the two tests drive identical production statements. See CR-2. The criterion as written asks that both conditions be exercised, which they are; the note is about the strength of the evidence, not its existence. |
| AC10 | Public and internal signatures unchanged | PASS | `git diff` against the merge base shows no alteration to any of `Task<bool> MoveToFolderAsync(string, bool, bool, bool, bool)`, `Task OpenOlFolderAsync(string)` or `Task OpenFsFolderAsync(string)`; the only edits inside those methods are the inserted guard and the changed right-hand side of `OlAncestor`. All six protected test files are absent from `git diff --name-only`, and all compiled and passed in the 6870-test run. |
| AC11 | The new test file is registered in the legacy `.csproj` and its tests appear in the executed list | PASS | `<Compile Include="Controllers\EfcDataModelArchiveRootTests.cs" />` present in the diff at `QuickFiler.Test.csproj:116`. Registration proven effective, not merely present: `ARCHIVEROOT_TESTS_EXECUTED: 11` in the full-suite TRX derivation, and the total arithmetic 6859 + 11 = 6870 is consistent. |
| AC12 | Fail-before / pass-after evidence exists under `evidence/regression-testing/` | PASS | `p3-t15-regression-fail-before.md`: exit 1, 11 tests, 5 failed, one `InvalidOperationException` per failure, and the five names are exactly AC1-AC4 plus the redaction test. `p5-t1-regression-pass-after.md`: exit 0, 11 of 11. Both under the canonical sub-path. Recorded with a wording note: the fail-before run executed against code that already carried the Phase-2 seam declaration, which is structurally required for the tests to compile and is behaviourally inert until Phase 4 adds its three invocation sites. The proof of the unguarded read is unaffected. |
| AC13 | Format gate reports no unformatted files, via `dotnet tool run` | PASS | `evidence/qa-gates/p6-t2-csharpier-check.md`: exit 0, `Checked 1561 files in 4097ms.`, zero `Error <path> - Was not formatted.` lines. The recorded command is `dotnet tool run csharpier check .`, so the manifest-pinned version was used. File count rose by exactly one against the 1560-file baseline, consistent with one new `.cs` file. |
| AC14 | Analyzer gate: `0 Error(s)` and zero `Skipping target "CoreCompile"` | PASS | `evidence/qa-gates/p6-t3-msbuild-analyzers.md`: exit 0, `0 Error(s)`, `5 Warning(s)` equal to the merge-base baseline, and 0 occurrences of the literal `Skipping target "CoreCompile"` against 86 `CoreCompile` invocations in the tee'd log. The recorded command uses `/t:Rebuild` and carries both analyzer properties. |
| AC15 | Type-check gate: `0 Error(s)`, zero `CoreCompile` skips, no `/p:Nullable=enable`, no `/t:Build` | PASS | `evidence/qa-gates/p6-t4-msbuild-nullable.md`: exit 0, `0 Error(s)`, `5 Warning(s)`, 0 `Skipping target "CoreCompile"`. The recorded command line contains `/t:Rebuild` and `/p:TreatWarningsAsErrors=true` and contains neither prohibited token, matching `.github/workflows/ci.yml` character for character. |
| AC16 | Test gate reports zero failures across `QuickFiler.Test` and `TaskMaster.Test`; no `LiveOutlook` category in the change | PASS | `evidence/qa-gates/p6-t5-vstest-coverage.md`: exit 0, 9 assemblies discovered including both named ones, 6870 of 6870 passed, per-namespace TRX join giving `QuickFiler.` = 0 failed and `TaskMaster.` = 0 failed, and 0 executed `LiveOutlook` tests. Independently confirmed by grep that the new test file contains zero `TestCategory` occurrences. The run carried CI's `/InIsolation` and `/TestCaseFilter:"TestCategory!=LiveOutlook"`. |
| AC17 | Changed lines >= 90% line coverage, no changed-line regression, repo-wide figures captured for both runs and shown not lowered | PASS | Changed-line figure independently recomputed from `coverage/coverage.cobertura.xml` during this audit: 27 covered of 29 valid = **93.10 percent**, uncovered lines 366 and 390 — reproducing `p7-t2-coverage-changed-lines.md` exactly, figure and line identities. Repo-wide figures independently re-read from the Cobertura root: `line-rate=0.853335`, `branch-rate=0.79311`, 9 packages. Baseline captured under `evidence/baseline/` and, after remediation, under `evidence/remediation-baseline/`; post-change under `evidence/qa-gates/`. The `[P7-T3]` mode-mismatch remediation is adjudicated **sound** in the policy audit § 5 on four independent corroborations: mode equality, package-count equality, denominator arithmetic (`64221 - 64195 = 26` = the newly added executable lines) and numerator arithmetic (`+67`, consistent with 11 new tests). The remediation artifact claims only "not lowered", which is what this criterion requires. |
| AC18 | Change footprint is exactly the three source files plus this feature folder | PASS | `git diff --name-only` against the merge base returns 38 paths: the three source files and 35 feature-folder documents. `EfcFormController.cs`, `AppOlObjects.cs`, `ArchiveRootPathGuard.cs` and `IOlObjects.cs` are all absent. Line deltas match the criterion's premise: `EfcDataModel.cs` 423 to 485, the new test file 389, the `.csproj` +1. `git status --short` in the review worktree is empty, so nothing is staged or untracked outside the diff. |
| AC19 | `EfcDataModel.cs` remains at or under 500 lines | PASS | `awk END{print NR}` gives 485 at head and 423 at the merge base via `git show`. 15 lines of headroom remain. |
| AC20 | The three non-goals are each filed as a separate follow-up issue with the numbers recorded in Rollout & Follow-up | PASS | The three numbers #696, #697 and #698 are recorded in `spec.md` § Rollout & Follow-up in two places: the post-fix task list at `:815` and the Links list at `:833-840`, the latter mapping each number to its non-goal letter and URL. `evidence/other/p8-t2-followup-issue-dossier.md` carries exactly three sections, one per non-goal, and they correspond one-to-one to non-goals (a), (b) and (c) in `spec.md` § Scope & Non-Goals — (a) `COMException` from the getter's COM calls, (b) the log-only `async void` boundary sinks, (c) the five archive-root reads in `EfcFormController`. Each section carries a title, body, verified citations and `ProposedLabels:`. The dossier's appended `RESOLVED` section records the filing route and the number-to-non-goal mapping, which agrees with the spec. The remote existence of the three issues was not queried because the reviewing directive prohibits `gh`; see G4/G5 in the policy audit. |

### Verdict distribution

| Verdict | Count |
|---|---|
| PASS | 20 |
| PARTIAL | 0 |
| FAIL | 0 |
| UNVERIFIED | 0 |

## Scrutiny of the two orchestrator-closed criteria

The directive singled out AC17 and AC20 as closed after plan execution rather than by the executor.
Both were examined more closely than the rest.

**AC17 — sound.** The executor was right to refuse the original comparison: a `raw` merge-base figure
(14 packages, `lines-valid=82363`, every `.Test` assembly in the denominator) against a
`koverage-processed` post-change figure (9 packages, `lines-valid=64221`) measures the denominator,
not the change. The `+14.63`-point "improvement" that comparison produced was an artefact and would
have been a false pass. The remediation removes the defect at its source by re-measuring the merge
base in the same mode, in an isolated detached worktree, with `packages/` and `.dotnet-sdk/` copied
from the feature worktree so the analyzer set and SDK cannot contribute to the difference, after a
`/t:Rebuild`. It exited 0 with 6859 of 6859 passing and reached post-processing, which is precisely
the step the original baseline run never reached.

Two things raise confidence beyond the artifact's own narrative. First, the arithmetic closes
independently: the denominator grew by exactly 26 lines, and 26 is the count of newly added
executable lines in `EfcDataModel.cs` derived separately in this audit from the `git diff -U0` hunks
intersected with the Cobertura line set. Second, the artifact is appropriately modest — it states
that line counts drift between runs on a suite this size and claims only "not lowered" rather than
presenting `+0.07` as a measured improvement. That is the claim AC17's text requires. The original
`[P7-T3]` artifact was left unedited with a clearly delimited appended `RESOLVED` section, which is
the correct reconciliation form.

**AC20 — sound within the verification the directive assigns.** Both halves hold. The three numbers
are recorded in the spec's Rollout & Follow-up section, twice, with each number bound to its
non-goal letter and URL. The dossier's three sections correspond one-to-one to the spec's three
non-goals, and each carries the citations a filer would need. One residual is recorded in the policy
audit as G5: the dossier states the promoted records are retained under
`docs/features/potential/promoted/`, and no entry for these three is present in the review worktree,
whose tree is clean. Those records were created in whichever worktree the promotion tooling ran.
AC20's text requires the numbers to be recorded in the spec, which they are, so this does not affect
the verdict; it is worth confirming before the feature folder is archived.

## Baseline comparison

Behavior at `ecdb1c84`, from the spec's verified tracing and confirmed by reading the pre-change file
through `git show`:

`Globals.Ol.ArchiveRootPath` was read unguarded at three sites inside `EmailFilerConfig` object
initializers. An unresolvable archive root raised `InvalidOperationException`, which unwound through
four frames with no catch and was absorbed by `EfcFormController`'s log-only `BoundaryErrorSink`.
Because `ActionOkAsync` hides the form before awaiting the move and disposes it afterwards, the
observable result was a vanished window, no message, no filed mail, and `Dispose()` and `Cleanup()`
skipped. With no negative caching on the backing field, the failure recurred on every attempt.

Behavior at `af1b36e2`:

`MoveToFolderAsync` returns `false` before constructing `EmailFiler`; the sole caller of that
overload routes `false` to `HandleMoveResult` and then to `MoveFailureMessageAction`, so the user is
told the move failed and `ActionOkAsync` proceeds to `Dispose()` and `Cleanup()`. The two `Open*`
methods surface a redacted diagnostic through the new seam and return normally. One `logger.Warn`
entry is written per guarded failure. `COMException` and every other exception type still propagate.
`OlAncestor` is never assigned an empty or synthesized value, so the #614 store-root-leak failure
mode is not reintroduced.

The behavior change is confined to the three guarded paths. No other production file is touched, no
signature changes, no configuration key is added, and rollback is a straight revert with no
migration.

## Findings

Blocking findings: **0**.

Non-blocking observations are recorded in the policy audit § 8 (G1 through G7) and the code review
(CR-1 through CR-7). The two that bear on acceptance criteria specifically:

- **AC9's evidence is weaker than its wording suggests** (CR-2). The two throw-condition tests differ
  only by the message string of the injected exception, and production dispatches on type. They drive
  identical production statements. The criterion asks that both conditions be exercised, which they
  are; a stronger test is not constructible at the `IOlObjects` seam, and the guard's own
  message-level behavior is already pinned upstream in
  `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs`, which this change correctly
  leaves untouched. Verdict remains PASS.
- **AC12's phrase "unmodified production code" is imprecise** (G3). The fail-before run executed
  against production code carrying the Phase-2 seam declaration but not the Phase-4 guard. The seam
  is inert until Phase 4 adds its three invocation sites, which the branch diff confirms, and the
  tests could not compile without it. The five recorded `InvalidOperationException` failures are
  genuine proof of the unguarded read. Verdict remains PASS.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/spec.md
- Total AC items: 20
- Checked off (delivered): 20
- Remaining (unchecked): 0
- Items remaining: none
```

No checkbox was altered by this audit. All 20 were already `[x]` on arrival and all 20 were
independently verified against evidence.
