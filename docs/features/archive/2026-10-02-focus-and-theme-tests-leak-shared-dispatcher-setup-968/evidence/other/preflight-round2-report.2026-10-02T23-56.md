# Preflight round 2 reviewer report, verbatim (issue #968)

- Timestamp: 2026-10-02T23-56
- Source: atomic-executor return under `DIRECTIVE: PREFLIGHT VALIDATION ONLY`, delegated by the preparation-mode orchestrator (non-isolated; the pwsh channel was refused by the pre-implementation gate, see the report).
- Plan reviewed: plan.2026-10-02T05-42.md, blob 136eb144fd5d441a97e9fb0d1c7bc60d264d1acb (commit ce51e29c9)
- Round: 2 (the single confirming round authorised by the coordinator; no further round is run in this preparation)
- Text below is verbatim (HTML entity escapes in the transport were restored to `<` and `>`).

---

PREFLIGHT: REVISIONS REQUIRED
CONVERGENCE: FURTHER ROUNDS LIKELY (the pwsh channel was refused in this round, so I could not run any plan command to see what it prints on success. One more round with a working pwsh channel is needed to confirm the deltas and to run the command-output checks that this round could only do by reading the files.)

## What I could and could not check

- **The pwsh channel was refused by a hook.** My first read-only probe (a `pwsh -NoProfile -Command` token-count run against the item worktree) was denied. I did not retry it. Deny text, verbatim:
  `PreToolUse:Bash hook error: PREIMPLEMENTATION_GATE_BLOCKED: Implementation operations require artifacts/orchestration/orchestrator-state.json to contain issue number, feature folder, route metadata, lifecycle readiness, and checkpoint state before implementation begins.`
  - The same gate will refuse every pwsh payload in this plan when the executor runs from this session tree (defect 8).
  - Because of the deny I ran no build, test, formatter or plan payload.
- **Validator not run.** The `mcp__drm-copilot__validate_orchestration_artifacts` tool is not available to me.
- **Git commands run (all read-only):**
  - `git -C <wt> log --oneline -8`: HEAD `ce51e29c9`; above BASE are `53d975270`, `d096f1250`, `4c6de5e84`, `87cca65ed` and `ce51e29c9`.
  - `git -C <wt> hash-object <plan>` returned `136eb144fd5d441a97e9fb0d1c7bc60d264d1acb`, matching the plan blob you supplied.
  - `git -C <wt> merge-base origin/main HEAD` returned `94287369908cc920b21b0e3256314f988ad7d2f5`.
  - `git -C <wt> diff --name-status 94287369… HEAD` lists 8 FEATURE paths plus the two promoted records, all with status `A`.
  - `git -C <wt> status --porcelain` was empty before the review and empty afterwards.
- **Every count and line citation below was checked with the Read and Grep tools** against the item worktree. The FluentAssertions 8.11.0 checks (`TaskFormatter` exists, so formatting a `Task` operand does not block; there is no `Should(Task)` overload) were made against the shared `packages` folder of the main checkout.

## Defects

**1. The F-SCOPE block is 43 lines, not 42, so the fixture is 375 lines after Phase 2, not 374.**
- Delivered Source F-SCOPE runs from plan line 227 to plan line 269, which is 43 lines.
- The arithmetic is 342 + 5 + 13 + 3 + 1 + 11 = 375.
- P2-T6 and P3-T9 assert 374 as an exact gate. A correct execution therefore fails P2-T6.
- Deltas:
  - Plan line 225: replace `forty-two lines replacing thirty-two:` with `forty-three lines replacing thirty-two:`.
  - Plan line 271: replace `After F-FIELDS to F-SCOPE the fixture is 374 lines before formatting;` with `After F-FIELDS to F-SCOPE the fixture is 375 lines before formatting;`.
  - P2-T6 Acceptance: replace `FIX LINES 374;` with `FIX LINES 375;`.
  - P3-T9 Acceptance: replace `LINES 374, 482, 442, 472 for FIX, FAT, TS, FT` with `LINES 375, 482, 442, 472 for FIX, FAT, TS, FT`.

**2. The four new fold span baselines are off by one.**
- `CMD-SPAN-TOKEN-COUNT` prints `SPAN: <START line>-<END line − 1>`. The existing #968 spans follow this rule: R4SPAN's END is line 285 and it prints `212-284`; ENSURE's END is line 146 and it prints `122-145`.
- The fold END lines are:
  - LIV line 166 (`/// <summary>Reads the issue #424 …`)
  - LIV line 218 (test 2's declaration)
  - DMT line 134
  - QQP line 311
- So the printed spans are `110-165`, `183-217`, `96-133` and `299-310`. P0-T13 asserts `110-166`, `183-218`, `96-134` and `299-311`, so it stops with `FOLD CENSUS MISMATCH`.
- Deltas:
  - Span anchors, `T1-LIVE` bullet: replace `Baseline \`SPAN: 110-166\`.` with `Baseline \`SPAN: 110-165\`.`
  - `HELD` bullet: replace `Baseline \`SPAN: 183-218\`.` with `Baseline \`SPAN: 183-217\`.`
  - `T-SIB` bullet: replace `Baseline \`SPAN: 96-134\`.` with `Baseline \`SPAN: 96-133\`.`
  - `GATE-LAMBDA` bullet: replace `Baseline \`SPAN: 299-311\`.` with `Baseline \`SPAN: 299-310\`.`
  - P0-T13 Acceptance: see the combined replacement under defect 3.

**3. The T1-LIVE baseline for `(await pending)` is 1, not 0.**
- LIV line 163 reads `(await pending).Should().BeEmpty();` and lies inside the span.
- P0-T13 Acceptance, combined delta for defects 2 and 3: replace
  `` `T1-LIVE` 5, 0, 3, 3, 1, 0, 0 with `SPAN: 110-166`; `HELD` 1, 0, 0 with `SPAN: 183-218`; `T-SIB` 1, 0, 0, 0, 1 with `SPAN: 96-134`; `GATE-LAMBDA` 1, 0 with `SPAN: 299-311`. ``
  with
  `` `T1-LIVE` 5, 0, 3, 3, 1, 0, 1 with `SPAN: 110-165` (the last value is LIV line 163, `(await pending).Should().BeEmpty();`); `HELD` 1, 0, 0 with `SPAN: 183-217`; `T-SIB` 1, 0, 0, 0, 1 with `SPAN: 96-133`; `GATE-LAMBDA` 1, 0 with `SPAN: 299-310` (each printed end is the END anchor line minus one, as for R4SPAN). ``

**4. The QfcDatamodel.cs baseline for `ForEachAwaitWithCancellationAsync` is 2, not 1.**
- Line 431 is a comment (`// ForEachAwaitWithCancellationAsync (System.Linq.Async) is obsolete…`) and line 440 is the call.
- P0-T13 would stop with `FOLD CENSUS MISMATCH`.
- The post-change value of 0 is correct, because both lines sit in the deleted block 417 to 465.
- Deltas:
  - Fact 14: replace `` `ForEachAwaitWithCancellationAsync` 1; `` with `` `ForEachAwaitWithCancellationAsync` 2 (the comment at 431 and the call at 440); ``.
  - P0-T13 Acceptance: replace `QDM tokens 2, 4, 0, 1, 4, 1, 1, 1, 2, 2, 7, 7, 1, 2, 1, 1, 1, 1, 2, 2, 3;` with `QDM tokens 2, 4, 0, 1, 4, 1, 1, 1, 2, 2, 7, 7, 1, 2, 1, 1, 2, 1, 2, 2, 3;`.

**5. The post-change `FakeTimeProvider` counts ignore the `ArmingFakeTimeProvider` substring.**
- The token count is an ordinal substring match, so every `ArmingFakeTimeProvider` line also counts as `FakeTimeProvider`.
- LIV after L-T1: `new ArmingFakeTimeProvider()` and `<see cref="ArmingFakeTimeProvider.Armed"/>` give 2, not 0.
- DMT after M-T: the 5 untouched lines plus the same 2 give 7, not 5.
- The prose `worker,` count is also wrong: it is 4, because the `SynchronousBackgroundWorker worker,` parameter line matches too. That value is not gated.
- Deltas:
  - Plan line 887: replace `` `worker,` 3 lines (the three callers); `Task.Yield` 0; `fake.Advance` 0; `FakeTimeProvider` 0; `` with `` `worker,` 4 lines (the three callers and the `StartHeldOpenLoader` parameter line `SynchronousBackgroundWorker worker,`); `Task.Yield` 0; `fake.Advance` 0; `FakeTimeProvider` 2 (`new ArmingFakeTimeProvider()` and the L-T1 doc cref `ArmingFakeTimeProvider.Armed`; the count is an ordinal substring match); ``.
  - P5-T3 Acceptance: replace ``LIV tokens `Task.Yield` 1 (the L-T1 doc line `<c>Task.Yield</c>` only), `fake.Advance` 0, `FakeTimeProvider` 0,`` with ``LIV tokens `Task.Yield` 0, `fake.Advance` 0, `FakeTimeProvider` 2 (the two `ArmingFakeTimeProvider` lines of L-T1),``. This replacement also carries defect 7.
  - Plan line 1014: replace ``` `Task.Yield` 1 (the M-T doc line only; the executable `await Task.Yield();` is gone); `await Task.Yield();` 0; `fake.Advance` 2 (the two untouched tests at pre-edit 241 and 280); `FakeTimeProvider` 5; ``` with ``` `Task.Yield` 0; `await Task.Yield();` 0; `fake.Advance` 2 (the two untouched tests at pre-edit 241 and 280); `FakeTimeProvider` 7 (five untouched lines plus `new ArmingFakeTimeProvider()` and the M-T doc cref `ArmingFakeTimeProvider.Armed`); ```.
  - P5-T4 Acceptance: replace ``` `fake.Advance` 2, `FakeTimeProvider` 5, `[TestMethod]` 9.``` with ``` `fake.Advance` 2, `FakeTimeProvider` 7, `[TestMethod]` 9.```.

**6. `HUNK_COUNT: 9` for QfcDatamodel.cs cannot occur.**
- Git puts two changes in one hunk when at most six unchanged lines separate them (with the default three context lines).
- The deletion at old line 363 and the replacement at old line 369 are 5 lines apart, so they form one hunk.
- The block deleted from old line 377 to 465 and the region deleted at 469 to 473 are 3 lines apart, so they also form one hunk.
- The observed count is therefore 7, or 6 if git shifts the 377 block up one line (lines 376 and 465 are identical). The merged 363/369 hunk is also neither "a pure deletion" nor "the single replacement", so that clause fails as well.
- `--numstat` reports the same edit independently of how git groups it into hunks.
- P4-T9 Acceptance: replace ``` `HUNK_COUNT:` for QfcDatamodel.cs is 9 and every `HUNK` is a pure deletion or the single one-line replacement at old line 369 (no hunk adds more than one line); ``` with ``` `HUNK_COUNT:` for QfcDatamodel.cs is recorded, not gated (git merges edits separated by at most six unchanged lines, so the nine P1 edits yield fewer hunks); the `--numstat` row for `QuickFiler/Controllers/QfcDatamodel.cs` reads `1	129` (128 removed lines plus the replaced line at old 369, whose replacement is the only added line); ```. P6-T2 restates P4-T9, so it inherits this change.

**7. A literal `Task.Yield` stays in both rewritten tests' doc comments, but AC31 says the tests "contain no `Task.Yield`".**
- The plan meets AC31 only through span gates that start at the method declaration. Grepping the whole file finds 1 hit in each of LIV and DMT, which a reviewer would read as a failed criterion.
- Deltas:
  - L-T1: replace `        /// <c>Task.Yield</c>. <see cref="ArmingFakeTimeProvider.Armed"/> proves the gate armed its` with `        /// a scheduler yield. <see cref="ArmingFakeTimeProvider.Armed"/> proves the gate armed its`.
  - M-T: replace `        /// <c>Task.Yield</c>, and the dequeue task itself is the completion signal.` with `        /// a scheduler yield, and the dequeue task itself is the completion signal.`
  - The count changes are already in the defect 5 deltas for P5-T3 and plan line 1014. Plan line 887 then reads correctly as written.

**8. No stop rule covers a pre-implementation-gate refusal of a pwsh payload or an evidence Write.**
- D-10 covers refusals of `git add`, `git commit`, `.cs` and `.csproj` edits, and spec edits. The payload-channel rule covers only the "This agent is isolated in the worktree" text.
- This round shows that `PREIMPLEMENTATION_GATE_BLOCKED` refuses a read-only pwsh payload issued from this session tree.
- D-10: replace ``A PreToolUse refusal of any `git add`, `git commit`, `.cs` edit, `.csproj` edit or spec edit is recorded verbatim`` with ``A PreToolUse refusal of any `git add`, `git commit`, `.cs` edit, `.csproj` edit, spec edit, evidence-file Write or pwsh payload (including a refusal whose text begins `PREIMPLEMENTATION_GATE_BLOCKED`) is recorded verbatim``.
- Orchestration action, not a plan edit: before dispatch, seed `artifacts/orchestration/orchestrator-state.json` in the executor's session tree. The executor is non-isolated, so its process starts in the coordinator tree and the hook reads the file from there.

Each delta was checked against the plan's own rules. Every changed token stays on one physical line. None contains a placeholder or a double quote inside a token. No acceptance criterion is weakened (defect 6 replaces an unsatisfiable hunk count with a stricter numstat check). The delta prose has no hyperbole or informal wording.

## Checked and correct

- **Round-1 deltas:**
  - Defects 1, 3, 4, 5, 6, 7, 8, 9 and 10 are applied correctly.
  - Defect 2 is correctly superseded: the INHERITED-COMMITTED set is stated by membership, and every commit (P0-T19, P6-T9, P8-T46, and the D-13 restart commit) is pathspec-limited.
- **Fixture and pin-count tests:**
  - N1 compiles under C# 7.3 (the project sets no LangVersion). `parked` is definitely assigned after the try/finally.
  - Traced on the unmodified fixture: test 1 fails with the predicted message, and tests 2 to 4 pass. All four pass on the fixed fixture.
  - Every PC token count and the T3SPAN NEST tail are correct.
  - Census after the change: PRIMARY 16, CROSS 32, CROSS-only 16, CONTROL 28, `INVOCATIONS-CLASSIFIED: 13 of 13`.
  - Theme, test-support and fixture-test files: the FAT, TS and FT counts and line totals (482, 442, 472) and the R4SPAN, R4HEAD and R4TAIL values before and after are correct. The TestSupport `HUNK_COUNT: 2` and the fixture-test hunk bounds hold.
- **D-18 design:**
  - The gate reaches its first `Delay` synchronously (QfcStreamingDequeueConfidenceGate.cs lines 243 to 255, with no earlier await on the path).
  - `ZeroAcceptanceCeiling` is 120 s and `DefaultFirstBatchDeadline` is 12 s, so neither fires at a simulated 200 or 400 ms.
  - No timer is created before the dequeue call.
  - `ReArm()` comes before `Advance`, so the re-arm signal is not racy.
  - The `ConfigureAwait(false)` on the gate's await makes the re-arm proof independent of the ambient context.
  - Scope 2 keeps the inlining rule valid, so the `loaderRelease` and `Worker_DoWork` continuations run inline and clear the flag before `ReadLivenessFlag`. Neither scope body contains an await.
  - The sensitivity edit makes the gate return after one wait, so each test fails on its `BeSameAs` re-arm assertion. FluentAssertions 8.11 formats `Task` operands without reading `.Result`, so the failure does not hang.
  - Under Workers=0 and ClassLevel, each test owns its clock and its scope is per-thread.
  - No sleep, delay, retry, Yield loop, timeout change or `[DoNotParallelize]` is added.
- **Dead-code removal (fact 15):**
  - The zero-caller proof is correct: 24 lines in 5 files, `\blog\b` 3 lines, string/reflection sweep 2 lines, and none of the four members in the interface.
  - The removal arithmetic is 5 + 49 + 40 + 1 + 24 + 2 + 1 + 3 + 3 = 128, giving 495 − 128 = 367.
  - The post-edit QfcDatamodel.cs counts in P4-T8 are correct.
  - No per-file coverage gate is placed on the `[ExcludeFromCodeCoverage]` type `QfcDatamodel`.
- **Folded-scope test files:**
  - The baseline and P4-T4 to P4-T7 counts for TD, ZB and LIV are correct (P4-T6 interim, HELD 0/1/1).
  - Line totals: TD 229 and LIV about 346. QueueProcessing stays 413, and its P5-T11 counts and `HUNK_COUNT: 2` are correct.
  - Project file: lines 155 to 229 match the plan's citations, and the T1/T2 placement is correct.
- **Ordering:**
  - P1-T5 and P5-T8 (both expect-fail) sit outside every exit-0 gate.
  - The sensitivity edit is reverted and checked with `diff --exit-code` before any later gate. P6-T2 asserts GATE-LAMBDA 1, 0 before the P6-T9 commit.
  - P0-T9 (read-only csharpier check) runs before any write-mode format.
- **Toolchain, gates and evidence:**
  - The CLAUDE.md toolchain order and commands are followed: `/t:Rebuild` for the analyzer and nullable gates, and no `/p:Nullable=enable`.
  - G7 to G9: every `git diff` is anchored, every name-listing diff has a porcelain companion, and the write-mode steps are observed through file hashes and markers.
  - Evidence lives only under `FEATURE/evidence/<kind>/`. Only trx-derived summaries and JaCoCo projections are committed; raw documents and logs stay under the gitignored `coverage/` folder.
- **Coverage of scope:**
  - The AC20 footprint is exactly the 14 code paths, with the two production paths named.
  - #972 items 1 to 5 and the liveness residual each map to tasks.
  - The PR body is directed to carry `Closes #968` and `Closes #972` (spec lines 134 and 332, and P8-T46).
  - The planner's AC-INVENTORY and AC-MAPPING records match the plan.
  - The PWSH CHANNEL REFUSED rule is present.

Defect count: 8.

Plan state: nothing executed, no box checked. Next five tasks: [P0-T1], [P0-T2], [P0-T3], [P0-T4], [P0-T5]. Acceptance criteria in `spec.md`: 32 total, 0 checked off.
