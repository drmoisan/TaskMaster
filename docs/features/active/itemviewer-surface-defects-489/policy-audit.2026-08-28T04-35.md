# Policy Audit — itemviewer-surface-defects (Issue #489) — remediation cycle 1 exit-gate reaudit

- Timestamp: 2026-08-28T04-35 (UTC)
- Branch: `bug/itemviewer-surface-defects-489` at `923cd3ce`
- Base: `epic/quickfiler-bug-family-integration`, merge base `69e83171`; feature Phase 0 base `cecd7813`; remediation base `d77ac212`
- Prior cycle artifacts: `policy-audit.2026-08-28T03-13.md`, `code-review.2026-08-28T03-13.md`, `feature-audit.2026-08-28T03-13.md`, `remediation-inputs.2026-08-28T03-13.md` (NO-GO, 1 Blocking: RC-1)
- Audit scope: the full branch diff `69e83171..923cd3ce` (187 files; 25 code/project paths), with deep verification of the remediation delta `d77ac212..923cd3ce`. PR context artifacts were regenerated at this HEAD before review (the prior summary's head `74d02ad2` was stale).

## Verdict

**PASS — zero Blocking findings.** RC-1 is cured and verified independently at source, at commit granularity, at runtime, and in coverage. The four disclosed execution deviations are adjudicated non-blocking (§ 7). Remediation cycle 1 closes.

## Rejected Scope Narrowing

None detected. The caller briefing requested an exit-gate reaudit of remediation cycle 1 and did not attempt to narrow the audit below the full branch-versus-base diff; the full 25-path code scope was re-derived from `git diff --numstat 69e83171..HEAD` and re-audited.

## 1. Scope integrity

- The full-branch code/project path set at HEAD is **exactly 25 paths**, independently re-derived by this reviewer and identical to the P0-T2 scope lock (0 added, 0 removed). The remediation deepened two entries already in the set (`QfcItemController.EventWiring.cs` +1, `QfcItemController.EventWiringTests.Part2.cs` +24) and added no path.
- The remediation commit series `7ad2bd17..HEAD` touches only those two scoped source files plus the feature folder — verified: `git diff --name-only` returns zero paths outside `docs/features/active/itemviewer-surface-defects-489/` other than the two source files.
- `git diff d77ac212..HEAD -- "*.csproj"` is empty. The new test compiles through the pre-existing `<Compile Include>` for `Part2.cs`.
- The shared `info/exclude` file for this git directory was inspected: it contains only pre-existing agent-infrastructure patterns; the remediation added nothing there.
- The tree at HEAD is clean (`git status --porcelain` empty).

## 2. Toolchain gates (verified from committed evidence, with independent re-execution where cheap)

| Gate | Pre-remediation baseline | Post-remediation | Independently verified by this reviewer |
|---|---|---|---|
| csharpier check | 0 unformatted | 0 unformatted, exit 0 | Yes — `dotnet tool run csharpier check` on both changed files: `Checked 2 files`, exit 0 |
| Analyzer rebuild | 5 warnings / 0 errors | 5 / 0, exit 0 | Yes — committed 11775-line log re-grepped: `5 Warning(s)`, `0 Error(s)` |
| CoreCompile skips (non-vacuity) | 0 | 0 | Yes — 0 occurrences of the skip literal in both committed rebuild logs |
| Nullable rebuild | exit 0, 0 CS86xx | exit 0, 0 CS86xx | Yes — 0 `CS86` occurrences in the committed 11889-line log; `5 Warning(s)`, `0 Error(s)` |
| `QuickFiler.Test` vstest | 1121 / 0 / 0 | 1122 / 0 / 0 | Yes — committed TRX counters re-parsed: `total="1122" executed="1122" passed="1122" failed="0"`; plus a live filtered rerun of both `UnwireIntentEvents` tests at HEAD binaries: 2/2 Passed, exit 0 |
| Repo-wide vstest | 6741 / 0 / 0 | 6742 / 0 / 0 | From committed P4-T7 evidence; total moved by exactly the one added test |
| Scope lock | 25 paths | 25 paths, set identical | Yes — re-derived from the merge base (§ 1) |

The RED-first ordering of the Bugfix Workflow is verified at commit granularity: commit `71e363ea` adds the regression test together with the committed failing TRX (`rem1-p1-t3.trx`: `total="1" executed="1" passed="0" failed="1"`), and the production fix lands in the subsequent commit `6d6a0ee1` with the passing TRX. The failing TRX itself contains `add_PicturesChanged` (1 occurrence) and `remove_PicturesChanged` (0 occurrences) — the 17-add / 16-remove imbalance observed at runtime by Moq's invocation ledger, not inferred from a grep.

## 3. Coverage (C#)

C# coverage verdict: **PASS** — repo-wide line coverage 85.1617 percent (>= 85 floor) and branch coverage 79.2335 percent (>= 75 floor), independently re-parsed by this reviewer from the coverage runner's canonical post-processed output document on disk (`line-rate="0.851617"`, `branch-rate="0.792335"`, `lines-covered="54420"`, `lines-valid="63902"`, 9 first-party packages; document mtime matches the recorded P4-T7 gate run).

- Changed-line no-regression: the single production line this remediation adds (`EventWiring.cs:481`) reports `hits="1"` in the same document, re-parsed independently (exactly one class entry for the file; both `<line number="481">` records read `hits="1"`). No new production file was added by the remediation; the feature's new-file coverage verdicts from the prior cycle stand unchanged.
- Denominator arithmetic: `lines-valid` moved 63901 -> 63902, exactly the one added coverable statement; test lines are excluded from the denominator by the runner's test-module exclude. Line rate moved 0.851567 -> 0.851617 (+0.005 percentage points), a no-regression outcome.
- Comparison shape: both figures are in the Koverage-post-processed shape (first-party packages, repo-relative filename attributes), like-for-like with the P0-T5 remediation baseline. The raw-shape figure (0.7051 at lines-valid 82070) is not a comparable basis and was not used.
- The hook-canonical file `artifacts/csharp/coverage.xml` was deliberately not emitted by the executor or by this reviewer: this repository has two legitimate denominators (post-processed first-party approximately 85.16 percent; raw all-instrumented approximately 70.5 percent), and materializing the raw shape at that path would force a false failure. Coverage verification was performed from the runner's own canonical output as reported above; the C# verdict on that basis is PASS. This is the identical disposition the prior cycle recorded.
- TypeScript, Python, and PowerShell have zero changed files on this branch, so no coverage row is mandatory for them.

## 4. Evidence Location Compliance

- All remediation evidence lands under `docs/features/active/itemviewer-surface-defects-489/evidence/<kind>/` (`qa-gates/`, `regression-testing/`, `remediation-baseline/`, `other/`) — the canonical location. Zero violations.
- The branch diff was scanned for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, and `artifacts/coverage/`: none exist. The repository does not carry `validate_evidence_locations.py`; the scan was performed manually against `git diff --name-only 69e83171..HEAD`.
- No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events: no instruction supplied a non-canonical evidence path.

## 5. General code change policy

- **Bugfix workflow:** followed exactly — failing regression test first (committed with its failing TRX), minimal one-line targeted fix second, full toolchain loop third (one iteration, all stages green, `RestartsTriggered: 0`).
- **Minimality:** the production diff is one line, mirroring the sixteen detachments already present, placed to mirror the wire order. No opportunistic refactor.
- **File size:** `EventWiring.cs` 484/500; `EventWiringTests.Part2.cs` 105/500 (both re-measured by this reviewer at HEAD). Full-branch file-size picture unchanged from the prior cycle: `BreadcrumbDropDownIntegrationTests.cs` remains at exactly 500/500 (untouched by the remediation; carried residual CR-4); the two Designer files above 500 are pre-existing generated files with deletion-only diffs on this branch, dispositioned in the prior cycle.
- **Error handling / logging:** not implicated — the fix adds no control flow; the member's existing `_itemViewer is null` guard covers the null-viewer teardown path, and `-=` on a never-attached handler is a no-op, so the line is safe on every teardown path.

## 6. C# and unit test policy

- The new test uses MSTest (`[TestMethod]`), Moq (`VerifyRemove(..., Times.Once())`), Arrange–Act–Assert with explicit section comments, and a doc comment stating the RC-1 scenario and expected outcome. No FluentAssertions shape is applicable to a `VerifyRemove` assertion; MSTest/Moq-native verification is the correct form here.
- Independence and determinism: the test constructs its own mocks and harness controller, touches no filesystem, network, or wall clock, and passed in this reviewer's isolated filtered rerun alongside the sibling balance test.
- The merged sibling's test `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` is untouched (absent from the remediation diff; file remains 499 lines) and passes unmodified against the 17-detachment code — confirmed in the committed GREEN and full-suite TRX and in this reviewer's live rerun. Its name is now slightly stale; adjudicated acceptable in `code-review.2026-08-28T04-35.md` (RCV-1).

## 7. Audit of the four disclosed execution deviations

1. **P3-T1 first-attempt gate failure (grep counted 2, expected 1).** Verified in the committed artifact: the addendum's Output Summary originally restated the machine-checkable literal, the line-counting gate failed, the summary was rephrased, and the failed attempt is retained. This is the gate working as designed and honest recording. Non-blocking; a positive signal.
2. **Backslash consumed by an unquoted coverage output argument.** The stray root file the mangled run produced was deleted before either recorded run; this reviewer confirmed no stray `coveragecoverage*` file exists at the repository root and the tree is clean. Operational false start, fully disclosed. Non-blocking.
3. **P4-T7 run twice.** The first run was detached and its exit code uncapturable; an acceptance clause requiring `EXIT_CODE: 0` cannot be satisfied by an unobserved exit code, so re-running in the foreground was the correct reading. Both runs are recorded, both cleared the floor (0.85157 and 0.851617 versus 0.851567), the denominator was identical across runs, and the jitter allowance was not consumed. Non-blocking.
4. **P4-T11 commit granularity — the one literal plan-text miss.** The plan says "as a single commit"; the caller's standing instruction said commit after each phase, and the executor followed the instruction, producing an eight-commit series. Adjudication: the substantive intent of P4-T11 — every required path committed on the branch and traceable — is met and was verified by this reviewer directly: all four required paths appear in the series (`EventWiring.cs` in `6d6a0ee1`, `Part2.cs` in `71e363ea`, `spec.md` and the handoff record in `77596b58`), no path outside the scope lock plus the feature folder appears anywhere in `7ad2bd17..HEAD`, and the final tree is clean. The conflict between plan text and standing instruction was disclosed in the artifact rather than papered over, which is the conduct the halt-and-notify rule is after. The per-phase series is also strictly better for auditability here: it is what made the RED-before-fix ordering verifiable at commit granularity (§ 2). Non-blocking; no remediation required. Residual note: future remediation plans should carry the caller's commit-granularity convention so the plan text and the instruction cannot diverge.

## 8. Findings

### RC-1 — CLOSED (was Blocking)

The invariant is restored: `WireIntentEvents()` performs 17 subscriptions and `UnwireIntentEvents()` performs 17 detachments. Evidence relied on: (a) source re-count at HEAD — 17 live `+=` in the wire member (an 18th textual match is a commented-out line) and 17 `-=` in the unwire member, with the added detachment at `EventWiring.cs:481` mirroring the attach at `:94`; (b) commit-granularity RED-first proof (§ 2); (c) this reviewer's live rerun of both `UnwireIntentEvents` tests at HEAD binaries — 2/2 passed; (d) line 481 `hits="1"` in the canonical coverage document; (e) the handoff record's dated addendum with `ObligationDischargedInBranch: true` (exactly 1 occurrence, at line 124), and the spec's dated amendment reconciling the disposition-table row and strengthening the issue #486 handoff criterion. The prior cycle's dangling-obligation state (a handoff to a sibling that had already merged) no longer exists.

### Non-blocking findings (carried forward, unchanged by the remediation)

- **PA-1 (Info):** the repository-wide 85/75-versus-80/90 threshold wording conflict remains; this branch clears the stricter of each pair, so no gate outcome depends on it.
- **PA-2 (Info):** E1 stale `<Analyzer Include>` HintPaths (pre-existing on the default branch) remains cleared for this worktree only via gitignored `packages/` copies; it and the other out-of-scope findings (O1–O8, E2–E4, reframed O3, #489 D4 residual, #490 D5, deferred #490 D1 clear-insertion) owe promotion to real issues after merge per the feature-promotion lifecycle.
- **RCV-1 (Info, new this cycle):** deliberate staleness of the sibling test name — recorded and adjudicated in the code review.

## 9. Acceptance criteria cross-check

All 62 `spec.md` acceptance criteria are checked (`- [x]` 62, `- [ ]` 0, re-counted at HEAD) and the one criterion amended by the remediation (the issue #486 handoff criterion) was strengthened, not weakened, and re-verifies as PASS against the addendum evidence. Detail in `feature-audit.2026-08-28T04-35.md`. No criterion was checked off or altered by this reviewer.
