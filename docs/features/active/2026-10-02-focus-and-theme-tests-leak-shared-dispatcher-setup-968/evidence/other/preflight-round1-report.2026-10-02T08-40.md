# Preflight round 1 reviewer report, verbatim (issue #968)

- Timestamp: 2026-10-02T08-40
- Source: atomic-executor preflight return, recovered by the parent parallel-orchestrator from the dead preparation child's task transcript (agent a291a7fbabf9d0229, grandchild abad2f272bbd16984). Text below is verbatim.
- Plan reviewed: plan.2026-10-02T05-42.md, blob 91b1232718a914b6b3e3904c1f8b3e7d3f754b28
- Note: defect 2 option B is superseded. The promotion record was committed in 53d975270, so re-derive Fact 11 and the INHERITED-COMMITTED expectation against the current branch instead (see preflight-round1.2026-10-02T08-40.md).

---

PREFLIGHT: REVISIONS REQUIRED

CONVERGENCE: NO FURTHER ROUNDS EXPECTED. If the deltas below are applied as written and the orchestrator settles the two orchestration items (the pwsh channel in defect 1 and the AC20 wording in defect 2), the next round should only need to confirm.

## What this round could and could not check

- **No build or test access.** I ran no build, test or formatter. I made no edits and wrote no memory. Every count below was re-derived by reading the files and by Grep against this worktree.
- **Read-only git commands run:** `log`, `status --porcelain`, `rev-parse HEAD origin/main`. HEAD, BASE and origin/main are all `94287369908cc920b21b0e3256314f988ad7d2f5`.
- **The pwsh channel is refused for this agent.** One trivial read-only probe was denied by a hook, and I did not retry it (hook discipline). The exact deny text:
  `This agent is isolated in the worktree C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a291a7fbabf9d0229, but this command runs pwsh in a plain command; what it reads or is handed as shell text cannot be shown not to run git. Refusing to run it — a worktree-isolated agent's git operations must target its own worktree. Run the plain command from C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a291a7fbabf9d0229.`
- **Validator not run.** `mcp__drm-copilot__validate_orchestration_artifacts` is not available in this session, so the G1 to G9 output was not observed.

## Defects

**1. The plan only works if Bash can run pwsh, and it has no rule for a refusal.** (Execution conventions "Payload channel"; D-10; first hit at P0-T2.)
- Almost every command-bearing task is a `pwsh -NoProfile -Command` payload. The probe above shows this agent type cannot run them.
- D-10 only covers refusals of git add/commit and file edits.
- Delta (append to the "Payload channel" bullet): `If the Bash tool refuses a pwsh invocation (a refusal whose text begins "This agent is isolated in the worktree"), the executor records PWSH CHANNEL REFUSED with the verbatim refusal in that task's artifact and stops; no rephrased or alternative invocation is attempted. The plan has no non-pwsh fallback, so the orchestrator must dispatch the executor without worktree isolation.`
- Orchestration action: run the executor as a non-isolated, serialized child.

**2. The Phase 0 commit sweeps in a staged promotion record, which then fails the footprint gates and AC20.** (Fact 11; P0-T3, P0-T17, P4-T8, P6-T9, P6-T38; spec AC20.)
- `docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md` is staged (index status `A`) but not committed.
- P0-T17 runs `git commit -m` without a pathspec, which commits the whole index, so that record goes into the commit.
- P4-T8 ("otherwise only paths from INHERITED-COMMITTED") and P6-T9 (the footprint must be exactly six code paths, and no `A` status outside FEATURE) then fail, and AC20 cannot be checked off.
- Fact 11 is also wrong. This branch has no commits above BASE; the listed SHAs (`3956fa351` and the rest) belong to another branch. `INHERITED-COMMITTED:` will therefore read `NONE`. The footprint gate is still satisfiable and not vacuous, because it still asserts exactly the six code paths.
- Recommended delta (option B). It changes AC20's wording, so it needs orchestrator sign-off. It widens the exclusion by one documentation file and leaves the production-code guarantee unchanged.
  - Fact 11 becomes: `HEAD equals BASE at preflight; the branch carries no commit above BASE, so INHERITED-COMMITTED is expected to be NONE. The promotion record docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md is staged (index status A) but not committed.`
  - P0-T3 Commands: append `; git -C WORKTREE diff --cached --name-status`.
  - P0-T3 Acceptance: append `INHERITED-STAGED: lists every cached name-status line verbatim or NONE; every listed path is under FEATURE or under docs/features/potential/promoted/ with a leaf containing focus-and-theme-tests-leak-shared-dispatcher-setup (otherwise INHERITED STAGED SET OUT OF SCOPE: stop).`
  - P0-T17: add the command `git -C WORKTREE show --name-status --format= HEAD`, and append to Acceptance `PHASE0-COMMIT-PATHS: lists only FEATURE paths and INHERITED-STAGED: paths (the commit carries the whole index).`
  - P4-T8 and P6-T9: replace `INHERITED-COMMITTED:` with `INHERITED-COMMITTED: or INHERITED-STAGED:` in the inherited-path clauses.
  - D-9: append `Paths staged at P0-T3 form INHERITED-STAGED: and are treated as inherited by every footprint gate.`
  - Write Set "must not touch", after `docs/features/potential/`, insert: `(exception: the promotion record listed by P0-T3 as INHERITED-STAGED: is committed unchanged by P0-T17)`.
  - Spec AC20 (amendment 1.2): `- [ ] AC20: No production code change: the diff against the merge base, after excluding the paths already committed on the branch before the plan's first task and the promotion record staged before it (recorded at Phase 0 as the inherited committed set and the inherited staged set), lists only paths under `QuickFiler.Test/` and this feature's documentation folder.`
- Fallback (option A, no AC change): make every commit pathspec-limited, as `git -C WORKTREE commit -m "<message>" -- <the task's add pathspecs>`, and leave the record staged for the orchestrator. The record then reaches the branch after AC20 has been checked off, so a later review would fail AC20 instead.

**3. The `lock (FieldLock)` count is 6 after the change, not 5.** (Delivered-source summary line 234, P2-T6, P4-T2, P6-T19.)
- F-FIELDS line 2 contains the literal `lock (FieldLock)`, so the gate counts 66, 79, 94, the F-FIELDS comment, ENSURE and SCOPE.
- As written, P2-T6 records a CENSUS mismatch.
- Delta: replace that F-FIELDS line with `        // dispatcher into a null field. Both are read and written only while FieldLock is held.` All stated counts (5) then hold.

**4. The P5-T2 nesting gate cannot pass for the plan's own test 4.** (P5-T2, P6-T18.)
- The gate requires every `EnsureUiThreadDispatcher()` line to come before every pin `Dispose()` line.
- Test 4 takes `freshPin` after `pinA` and `pinB` have been disposed, so as written the gate stops the run with `NESTING VIOLATION`.
- Delta: the replacement P5-T2 Acceptance is given under defect 5.

**5. Test 4 cannot fail for the property AC4 claims ("Ownership flag is cleared on the last release").**
- If `_fixtureInstalledParked = false;` were omitted, the fresh pin still lands on a null field, sets the flag again and reverts on release. The test passes either way.
- This is a test-quality defect in a touched file, so it is in scope under the related-defect directive.
- Delta, replacing N1 test 4. Shown at in-file indentation; it still passes before and after the fix:
```
        /// <summary>
        /// Specification test: passes before and after the fix. After a full two-pin cycle inside the
        /// same transaction, a fresh single pin on the null baseline must still seed the parked
        /// dispatcher and its release must still restore null. A second transaction then installs
        /// that parked instance as its own value, and a pin taken and released under it must leave
        /// the value in place: had the earlier cycle's last release left the install-ownership flag
        /// set, this release would revert a value the fixture did not seed.
        /// </summary>
        [TestMethod]
        [Timeout(GateTimeoutMs)]
        public async Task EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores()
        {
            // Arrange
            Dispatcher parked;
            UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                .BeginTransactionAsync()
                .ConfigureAwait(false);
            try
            {
                transaction.Install(null);
                IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                parked = UiThreadDispatcherFixture.Current;
                pinA.Dispose();
                pinB.Dispose();

                // Act
                IDisposable freshPin = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                Dispatcher afterFreshPin = UiThreadDispatcherFixture.Current;
                freshPin.Dispose();
                Dispatcher afterFreshRelease = UiThreadDispatcherFixture.Current;

                // Assert
                afterFreshPin
                    .Should()
                    .NotBeNull(
                        because: "a pin on a null field seeds the parked dispatcher whatever earlier cycles did"
                    );
                afterFreshRelease
                    .Should()
                    .BeNull(
                        because: "the fresh pin is the only live pin, so its release reverts the seeding"
                    );
            }
            finally
            {
                transaction.Dispose();
            }

            // Act (second transaction): the parked instance is now a transaction value, not a seeding
            UiThreadDispatcherTransaction foreignTransaction = await UiThreadDispatcherFixture
                .BeginTransactionAsync()
                .ConfigureAwait(false);
            try
            {
                foreignTransaction.Install(parked);
                IDisposable foreignPin = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                foreignPin.Dispose();
                Dispatcher afterForeignRelease = UiThreadDispatcherFixture.Current;

                // Assert
                afterForeignRelease
                    .Should()
                    .BeSameAs(
                        parked,
                        because: "the last release cleared the install-ownership flag, so a pin that seeded nothing leaves a transaction value in place"
                    );
            }
            finally
            {
                foreignTransaction.Dispose();
            }
        }
```
- Knock-on count changes:
  - **Line 459:** becomes `EnsureUiThreadDispatcher()` 10 (tests 1, 2 and 3 two each, test 4 four), bare `EnsureDispatcher` 15 lines, plus `foreignTransaction.Install(parked);` 1. Its last sentence becomes `Within each transaction, its .Install( line precedes every pin acquired under it, and every pin is disposed before that transaction's first Dispose();.`
  - **Unchanged (capital T in `foreignTransaction`, case-sensitive match):** `transaction.Install(null);` 3 and `transaction.Dispose();` 4.
  - **P1-T3:** append `"foreignTransaction.Install(parked);"` to the PC tokens. Acceptance PC tokens become `10, 1, 3, 1, 1, 4, 4, 1, 3, 1, 4, 1, 2, 2, 0, 0, 0, 1, 1`.
  - **P5-T1:** `PRIMARY_LINES: 16` (pin-count tests 10), `CROSS_LINES: 31` → `32` (pin-count tests 15), `CONTROL_LINES: 28`, and "the thirteen test-side lines". The CROSS-only total stays sixteen.
  - **Line 832:** becomes `23 before the change, 28 after N1 adds five`.
  - **P6-T18:** becomes `INVOCATIONS-CLASSIFIED: 13 of 13 nested`.
  - **Spec:** in "Functions/classes" item 4, append `then, in a second transaction that installs the parked instance captured in the first, one pin taken and released leaves that value in place (the discriminating check for the flag reset)`. In the Test Strategy census sentence, change "nine in the new pin-count test class" to "ten".
- P5-T2 Acceptance, replacing the whole bullet: `for each of R1SPAN, R2SPAN, R3SPAN, T1SPAN, T2SPAN, T3SPAN and T4SPAN the NEST output shows, for every transaction variable in the span (one per span; two in T4SPAN, transaction then foreignTransaction), by ascending line number: that transaction's BeginTransactionAsync() line, then its single .Install( line, then the pins taken under it, where each pin's EnsureUiThreadDispatcher() line precedes that pin's first Dispose() line (ensureScope, pinA, pinB, freshPin, foreignPin), and every such pin Dispose() line precedes that transaction's first Dispose() line; in T4SPAN the foreignTransaction BeginTransactionAsync() line follows the transaction.Dispose(); line; R4SPAN shows no EnsureUiThreadDispatcher() line and two transactionA.Dispose(); lines, the second inside a finally; the artifact records per method NESTED: YES and INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE, and records INVOCATIONS-CLASSIFIED: 13 of 13 nested (three in the fixture tests, ten in the pin-count tests). Any other ordering is NESTING VIOLATION: stop and report.`

**6. The indentation statement is wrong for N1 and T1.** (Line 135.)
- N1 and T1 are shown four spaces deeper than in-file. T1 has 8 spaces in the plan, while P1-T3 requires 4.
- Delta: `Every block below except N1 and T1 is shown at its in-file indentation (four, eight, twelve, sixteen or twenty leading spaces) and is written exactly as shown. N1 and T1 carry one extra four-space Markdown indent on every line, which the executor removes: N1's using and namespace lines start in column 1, and T1 starts with four spaces.`

**7. A backslash `WORKTREE` path breaks every `git -C` call.** (Execution conventions "Tokens".)
- Bash strips unquoted backslashes, so the delegation's backslash path fails in `git -C`.
- Delta (append): `In every git -C argument WORKTREE is written with forward slashes, because the Bash channel removes unquoted backslashes; inside a pwsh payload's double-quoted Set-Location argument the backslash form is used.`

**8. AC11's own grep is never recorded.** (P0-T12, P2-T6, P6-T21.)
- AC11 names a grep for `installed nothing carries`. The plan records only the substitute token `A scope that installed nothing`.
- Delta: append `"installed nothing carries"` to the FIX token list (baseline 0, post 0, with a note that the baseline is vacuous because the phrase wraps across lines). P6-T21 also requires `installed nothing carries` 0.

**9. P3-T9 names the wrong file list.**
- Delta: replace `(the P0-T12 lists, FT extended with "Issue #480 shared arrange helper" for TS)` with `(the P0-T12 lists, the TS list extended with "Issue #480 shared arrange helper")`.

**10. Tests that relied on the leaked dispatcher have no defined outcome.** (P6-T5, CMD-COVERAGE-POST, Risks.)
- The two deleted theme-test calls left the parked dispatcher installed for the rest of the run. Every later transaction restored it, because R2 and R3 restore their captured previous value.
- Production QuickFiler code reads `UiThread.Dispatcher` in about 35 places. A test that depended on the leak would now throw `The UI dispatcher has not been captured`.
- The plan would either restart on the wrong files (D-13) or stop with `NEW FAILURE OUTSIDE SCOPE`. Under the related-defect directive, such a test is in scope.
- Delta, CMD-COVERAGE-POST (append): `foreach ($r in @($trx.SelectNodes("//t:UnitTestResult", $ns))) { $o = $r.GetAttribute("outcome"); if ($o -ne "Passed" -and $o -ne "NotExecuted") { $m = $r.SelectSingleNode("t:Output/t:ErrorInfo/t:Message", $ns); Write-Output ("MESSAGE " + $r.GetAttribute("testName") + " :: " + $(if ($m) { $m.InnerText -replace "\s+", " " } else { "(no message)" })) } }`
- Delta, P6-T5 Acceptance (insert after the `NEW-FAILURES:` clause): `Any NEW-FAILURES: name whose MESSAGE contains The UI dispatcher has not been captured is recorded as LEAK-DEPENDENT TEST EXPOSED: followed by the name, and stops the run for re-planning under the related-defect directive; it is neither a D-13 restart nor NEW FAILURE OUTSIDE SCOPE.`
- Delta, Risks: add a matching bullet.

## Checked and correct

**Ordering and satisfiability:**
- The expect-fail P1-T5 is not inside any exit-0 gate.
- P1-T6 runs only tests 2 to 4, and I traced all three as passing on the unmodified fixture.
- The read-only formatter baseline (P0-T9) runs before the first write-mode format (P4-T1).

**Line and token citations:**
- Line totals 342, 470, 497 and 440 are correct.
- The R4 header (221 to 224), the 16-space close at 270, and the R4SPAN, R4HEAD and R4TAIL baseline and post-change counts all check out.
- Every other FIX/FAT/TS/FT token count checks out, except `lock (FieldLock)` (defect 3).
- Census figures 20/9/23 and the csproj lines 200, 201, 203 and 212 are correct.
- The TestSupport and fixture-tests hunk bounds hold.

**Concurrency design:**
- After the change, nothing in QuickFiler.Test holds a pin across a gate release. Every pin sits inside a transaction with no `Install` between taking and releasing it.
- Nothing in QuickFiler.Test writes the field outside a transaction:
  - It contains no `UiThread.Init` or `ResetForTesting` call.
  - The only `Exchange`/`CompareExchange` writers are inside the fixture.
  - `EmailMoveMonitorTests` only reads.
- Option (a) for R4 is sound. Its `NotBeSameAs(liveA)` assertion also passes when the original value is null.
- Under these conditions, test 1's final "last release nulls" assertion is deterministic.
- R5 and the #882 round-trips have no try/finally, but nothing between acquiring and disposing can throw, so they are not defects.

**Commands and evidence:**
- Every command targets this worktree: through the PREFIX `Set-Location` plus `SetCurrentDirectory`, through `Invoke-Restore.ps1` (which resolves the solution relative to its own script folder), and through the runner's fixed coverage output (kept under the ignored `coverage\` folder).
- SDK, tool restore and NuGet restore all run before the first msbuild.
- Only trx-derived summaries and JaCoCo projections are committed, and every evidence path is under `FEATURE/evidence/<kind>/`.
- Plan and spec prose meets the tonality rules.

## Plan state

- Phase 0, nothing executed. Next five tasks: [P0-T1], [P0-T2], [P0-T3], [P0-T4], [P0-T5].
- Acceptance criteria in `spec.md`: 24 total, 0 checked off.

Files reviewed:
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a291a7fbabf9d0229\docs\features\active\2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968\plan.2026-10-02T05-42.md`
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a291a7fbabf9d0229\docs\features\active\2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968\spec.md`
