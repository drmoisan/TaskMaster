# Feature Audit — Issue #656 (breadcrumb `_closeCompleted` residual)

- Timestamp: 2026-09-01T15-03
- Work mode: `full-bug` (marker at `issue.md:12`)
- AC source: `spec.md` only. `user-story.md` correctly absent for this mode.
- Baseline: `main` at `5670b3cfe6a52e3b890bf80f0cd85a20d4fe4723`
- Head: `65d2f22b5100588eae8ac4de40e48f1ac391db34`

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656/spec.md
- Total AC items: 20
- Checked off (delivered): 20
- Remaining (unchecked): 0
- Items remaining: none
```

Checkbox state re-counted directly: `^- \[x\]` matches in `spec.md` = 21, `^- \[ \]` = 4. Of those,
20 checked and 0 unchecked are AC items; the remaining 1 checked and 4 unchecked are the bug-report
template's severity radio group (`- [x] Medium`) and the `- [ ] Attached minimal logs or screenshot`
line, which are not acceptance criteria. So 20 of 20 AC boxes are checked and none is unchecked.
This matches the executor's `evidence/issue-updates/ac-status.2026-08-31T20-40.md`.

Plan checklist: `plan.2026-08-31T20-10.md` carries 62 checked and 0 unchecked task boxes.

No AC box was newly checked by this reviewer; all 20 were already checked and all 20 verify as PASS.

## AC Evaluation Table

| AC | Requirement (abbreviated) | Verification performed by this reviewer | Verdict |
|---|---|---|---|
| AC-1 | Hoisted `_host.IsOpen` local before the lock; guard is `if (_closeCompleted && !<local>)` | Read the file: line 326 `bool hostOpen = _host.IsOpen;`, line 327 `lock (_sync)`, line 333 `if (_closeCompleted && !hostOpen)`. The read precedes the lock. | PASS |
| AC-2 | No `_host`/`IBreadcrumbDropDownHost` call added or modified inside any `lock (_sync)` body | Enumerated all 12 `lock (_sync)` sites (lines 91, 103, 113, 141, 154, 244, 327, 344, 349, 363, 377, 385) and all 8 `_host.` usages (119, 200, 205, 216, 265, 266, 326, 340). The only `_host` call inside a lock body is the pre-existing line 119 in `RequestOpen`. The diff adds none. | PASS |
| AC-3 | Named test exists in `Part3.cs`, drives open, close, `SetOpen(true)`, second close, asserts two `Uncommitted` entries | Read the test body in the diff. All four steps present in order; assertion is `.Equal(new[] { Uncommitted, Uncommitted }, "...")`. | PASS |
| AC-4 | Test demonstrated failing before and passing after, both outputs recorded under `evidence/qa-gates/` | `evidence/qa-gates/red-green-comparison` records red 1/0/1 exit 1 and green 1/1/0 exit 0, with both `Timestamp:`/`Command:`/`EXIT_CODE:`/`Output Summary:` blocks embedded verbatim and the red assertion message quoted. Two method departures are declared and justified in an `AC-4 Reconciliation:` section — see the deviation note below. | PASS |
| AC-5 | `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` passes and its file unchanged | Scoped run recorded 5/5 passed including this test. `git diff --name-only origin/main...HEAD -- QuickFiler.Test` returns only `...Part3.cs`, so `BreadcrumbDropDownOpenCoordinatorTests.cs` is unchanged. Re-run by me. | PASS |
| AC-6 | `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` passes, assertion text unchanged | Same scoped run; `...Part2.cs` absent from the diff, so no assertion text could have changed. | PASS |
| AC-7 | `RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync` passes, assertion text unchanged | Same scoped run; `...Part2.cs` absent from the diff. | PASS |
| AC-8 | `CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce` passes, assertion text unchanged | Same scoped run; `...Part2.cs` absent from the diff. | PASS |
| AC-9 | `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen` passes | Same scoped run, recorded `Passed`. Also covered by the 6926/6926 full-suite run. | PASS |
| AC-10 | No file under `QuickFiler/` other than the coordinator | Re-ran `git diff --name-only origin/main...HEAD -- QuickFiler`: exactly `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`. | PASS |
| AC-11 | No `*.csproj`, `*.props`, `*.targets`, `*packages.config` in the diff | Re-ran the pathspec-scoped diff: empty output. | PASS |
| AC-12 | No file under `QuickFiler.Test/` other than `...Part3.cs` | Re-ran: exactly `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`. | PASS |
| AC-13 | Both changed files under 500 lines | `awk 'END{print NR}'`: coordinator 395 (baseline 378), test part 213 (baseline 173). Both under 500. | PASS |
| AC-14 | `dotnet tool run csharpier check .` exits 0, no file needs formatting | Re-executed in this session: `Checked 1566 files in 4543ms.`, exit 0, no file listed. | PASS |
| AC-15 | Analyzer gate `0 Error(s)`, no new warning on the changed file | `p4-t3-analyzer.log`: `0 Error(s)`, `5 Warning(s)`, zero warning lines naming `BreadcrumbDropDownOpenCoordinator.cs`. Baseline `p0-t8-analyzer.log` also `5 Warning(s)` — count unchanged. | PASS |
| AC-16 | No `Skipping target "CoreCompile"` in the analyzer log | `grep -c` over `p4-t3-analyzer.log` returns 0. The gate was non-vacuous. | PASS |
| AC-17 | Type-check gate `0 Error(s)`, no `/p:Nullable=enable`, `/t:Rebuild` | `p4-t6-typecheck.log`: `0 Error(s)`; `grep -c "Nullable=enable"` returns 0; command uses `/t:Rebuild`. | PASS |
| AC-18 | Wrapper run, zero failed tests, `/InIsolation` and `TestCategory!=LiveOutlook` in effect | `TestResults/p4-t7/coverage-run.log` lines 6948-6949: `Total tests: 6926`, `Passed: 6926`, no `Failed:` line. Both switches are unconditionally appended at line 76 of `Invoke-MSTestWithCoverage.ps1`. Count reconciles as baseline 6925 + 1 added test. | PASS |
| AC-19 | Field doc and `CloseCore` summary state the new suppression condition and why the read is outside `_sync` | Read both blocks. Field `<remarks>` at lines 46-52 states the flag is cleared only on `RequestOpen`/`Invalidate` and that suppression now additionally requires the host to report not open. `CloseCore` `<remarks>` at lines 315-323 states the same and gives the SR-4 rationale for hoisting. Content requirement met; cited line ranges are stale (CR-4). | PASS |
| AC-20 | No new `internal`/`public` member on the coordinator; no member on `IBreadcrumbDropDownHost` | Declared member count 12 at base and 12 at head. `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs` absent from the diff. | PASS |

**Totals: 20 PASS, 0 PARTIAL, 0 FAIL, 0 UNVERIFIED.**

### Declared deviation on AC-4

The executor recorded two departures from AC-4's literal check method rather than silently
substituting. Both are accepted:

1. The red run is stored under `evidence/regression-testing/`, which is the canonical fail-before
   location in `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, while AC-4 asks for it
   under `evidence/qa-gates/`. The conflict is resolved by embedding the red run's four required
   fields verbatim in the `evidence/qa-gates/red-green-comparison` artifact, so both outputs are
   present where AC-4 asks while the authoritative artifact stays where a later audit will look.
2. Both single-test runs use `vstest.console.exe` directly rather than the wrapper, because neither
   wrapper accepts a `TestCaseFilter` override (both pin it) and editing a wrapper is outside the
   authorized two-file footprint. A wrapper run would have executed the whole suite and could not
   have produced a scoped red record at all. Both wrapper protections were reproduced explicitly:
   `/InIsolation` is passed and `TestCategory!=LiveOutlook` is the first conjunct of the filter.

The substance AC-4 exists to guarantee — a recorded failing run before the fix and a recorded passing
run after — is fully delivered.

## Baseline delta assessment

The issue's Expected Behavior is: "The close request reaches `_host.Close`, because the host is
genuinely open again." Relative to `main`, the branch delivers exactly that for the state
`_closeCompleted == true && IsOpen == true`, and changes nothing else. All other guard states are
bit-identical to baseline (see the truth table in `code-review.2026-09-01T15-03.md`).

The three "Proposed Fix / Validation Ideas" in `issue.md` are addressed as follows: the reopen-path
enumeration was performed (research artifact); no bypassing production path was found, so the second
idea — routing such a path through `RequestOpen`/`Invalidate` — was correctly moot and the guard was
hardened instead; and a regression test was added with the three named must-pass tests unedited.

## Honesty of the delivered classification

The caller asked specifically whether the latent-correctness classification is honestly represented.
Assessment, in three parts:

**Honestly represented, part 1 — the issue's literal premise.** `spec.md` does not pretend the
reported scenario occurs. Its **Scope & Non-Goals** opens with an explicit heading, "Classification:
latent-correctness hardening, not an observed failure", and states that no production reopen path
bypassing both entry points exists in the tree today. That is a direct contradiction of the issue's
Steps to Reproduce, stated plainly rather than buried. The **Manual validation steps** section says
"None. The residual is not reachable through the shipped UI ... so there is no manual gesture that
exercises it." The **Post-fix monitoring** section says "No telemetry to monitor: the changed state
is unreachable from shipped UI, so there is no production signal to watch." The classification is
foregrounded, not concealed, and the change's impact is not inflated anywhere I could find.

**Honestly represented, part 2 — severity.** Both `issue.md` and `spec.md` carry `- [x] Medium` with
the same justification text. The spec neither raises nor lowers the inherited severity. Given the
spec's own finding that the scenario is not reachable through shipped UI, Medium is arguably higher
than the evidence supports; but leaving an inherited severity untouched rather than editing it
downward is the conservative and defensible choice, and the qualifying sentence
("a latent correctness gap rather than an observed user-facing failure") sits directly beneath it.
No finding.

**Not fully accurate, part 3 — the reachability conclusion is stated too strongly.** This is CR-1 in
`code-review.2026-09-01T15-03.md` and it is the one place the delivered artifacts overreach. The
enumeration proves that the *host cannot be reopened* without `RequestOpen`. The spec then reports
that as though it settled a different proposition — that `_closeCompleted == true` and
`_host.IsOpen == true` cannot both hold in production. They can: `BreadcrumbDropDownHost.Close`
returns `true` after only *scheduling* `CompleteClose`, and `CompleteClose` is what sets
`OpenState = false`, dispatched onto the same UI operations queue the coordinator posts to. So the
guard's new branch is reachable on the shipped host with no substituted implementation. The
statement in **Mitigations and rollbacks** that a rollback "restores behavior that is
observationally identical to the fixed build on every shipped path" is therefore not established.

To be precise about what this does and does not mean: the change was scoped deliberately as latent
hardening, and I am not upgrading its claimed impact or failing it for being latent. The defect is
in the recorded *reasoning* — an enumeration that answers a narrower question than the conclusion
drawn from it. The fix direction remains correct in the newly-reachable window as well.

## Independent verification of execution-evidence claims

The caller flagged one integrity issue and asked that it be treated as a reason to re-derive other
factual claims rather than accept them. Both checks below were performed from git, not from prose.

### INTEGRITY-1 — Confirmed. The executor's claim about the agent-memory file is false.

The executor reported that `.claude/agent-memory/atomic-executor/project_baseline_sha_diff_conflates_merged_base.md`
"was already in the baseline diff". It was not.

| Check | Result |
|---|---|
| `git ls-tree origin/main -- <path>` | present, blob `418576a14e3a1153c16032cf7f6329df1a472474` — the file is **tracked on the base** |
| `git diff --name-only origin/main...119a89f0` (pre-execution HEAD, 10 paths) | the path is **absent** — it was unmodified at the point execution began |
| `git log --name-status origin/main..HEAD` | appears once, as `M`, in the final commit `65d2f22b` |
| `git diff origin/main...HEAD -- <path>` | +10/-0, one appended paragraph |

So the file was tracked and unmodified on both `origin/main` and the pre-execution HEAD. It entered
the branch diff because the executor modified it during the run and it was committed afterwards in
`65d2f22b`. The caller's reading is correct.

Two mitigating observations. First, the false statement appears only in the executor's
conversational report; no committed artifact repeats it. `evidence/other/final-commit` accurately
records that "No path under `.claude/agent-memory/` and no path under `artifacts/orchestration/` was
staged" for commit `145ee256`, which is true of that commit. Second, the appended content is
factually accurate — I verified its central measurement below. The defect is a misattribution of
provenance, not fabricated content, and it is not a code defect. **Severity: Minor, non-blocking.**
The follow-up worth recording is procedural: an agent-memory write during a run is a real branch
mutation and should be reported as such, not folded into "already in the baseline".

### INTEGRITY-2 — Confirmed sound. The stale-pinned-base substitution is correct and the recorded evidence is not self-contradictory.

The plan anchored every footprint gate to `2b85134b42872e405602e6064e02dc9cda6c319b`. Verified:

| Check | Result |
|---|---|
| `git merge-base --is-ancestor 2b85134b HEAD` | true — the pinned SHA is an ancestor of HEAD |
| `git merge-base 2b85134b HEAD` | `2b85134b...` — equals the pinned SHA itself |
| `git diff --name-only 2b85134b...HEAD` vs `2b85134b..HEAD` | 335 and 335 — identical, so the three-dot form has degenerated to the two-dot form |

The executor's reasoning is therefore exactly right: because the pinned SHA is an ancestor,
`merge-base(PINNED, HEAD) == PINNED`, so `PINNED...HEAD` silently becomes `PINNED..HEAD` and
conflates everything `main` gained in the interval with this item's change set. Four footprint gates
asserting "exactly the single line" and "both outputs empty" were unsatisfiable as written. The
executor detected this, substituted `origin/main...HEAD` (a genuine merge-base of the branch), and
recorded both measurements verbatim in each footprint artifact rather than substituting silently.
That is the correct handling.

The recorded numbers reconcile exactly against their measurement point, which is the test that
matters for self-consistency:

| Recorded figure | Measured at pre-execution HEAD `119a89f0` | Match |
|---|---|---|
| `PINNED...HEAD` = 299 paths | `git diff --name-only 2b85134b...119a89f0` = **299** | yes |
| 9 under `QuickFiler/` + `QuickFiler.Test/` | **9** | yes |
| one `.csproj` | **1** | yes |
| `origin/main...HEAD` = 10 paths | `git diff --name-only origin/main...119a89f0` = **10** | yes |

The 299 figure differs from today's 335 solely because 36 further evidence and documentation files
were committed after the measurement. The `footprint-production` artifact's own stale-base listing
of four `QuickFiler/` paths reproduces exactly against the current tree. No contradiction found.

### Other re-derived claims

Because of INTEGRITY-1 I re-derived every load-bearing figure rather than accepting the executor's
summaries. All of the following reproduced exactly: the 46-path branch diff and its composition; all
three footprint pathspec results; both file line counts and their baselines; the declared member
count 12 before and after; the analyzer `0 Error(s)`, `5 Warning(s)` and zero-CoreCompile-skips; the
type-check `0 Error(s)` and absent `/p:Nullable=enable`; the full-suite 6926/6926; the CSharpier
result, re-executed live; the repository line rate 0.853732 and branch rate 0.793761 from the
Cobertura root element; the coordinator class rate 0.983193 and its 234/238 line split; and hits of 1
on both changed lines 326 and 333. **No further inaccuracy was found in any committed artifact.**

## Non-blocking follow-ups (text only — no issue filed, per instruction)

Recorded here for the maintainer's consolidated post-merge issue. None of these is a merge condition.

1. **Amend the spec's reachability conclusion (CR-1).** State that the guard's new branch is
   reachable on the production host inside the window between `Close` returning `true` and the
   scheduled `CompleteClose` setting `OpenState = false`, and withdraw or qualify the claim that a
   rollback is observationally identical on every shipped path. Optionally settle the open question:
   does any real gesture dispatch a second `CloseCore` inside that window, given that the
   selector-close event appears to be raised from within `CompleteClose` itself?
2. **Correct R-1's description of the not-open `Close` branch (CR-2).** `BreadcrumbDropDownHost.cs:256`
   returns `TryCancelPendingOpen(...)`, which can invalidate, schedule a `CompleteClose` and return
   true; it does not unconditionally return false.
3. **Add a deferred-`IsOpen` harness variant (CR-3).** Every current fake clears `IsOpen`
   synchronously inside `Close`, so no test represents the production timing in item 1. A harness
   whose `Close` returns true but defers `IsOpen = false` to the drained queue would close the gap.
4. **Refresh the stale line citations in `spec.md` (CR-4).** AC-19's `:38-46` / `:302-307` and the
   Scope section's `:258-259` / `:114-115` all drifted by the seven lines the field `<remarks>`
   added.
5. **Provenance reporting for agent-memory writes (INTEGRITY-1).** A memory file modified during a
   run is a branch mutation and should be reported as one.
6. **Reconcile the two coverage floor definitions.** `CLAUDE.md` specifies >= 80 percent repo-wide
   and >= 90 percent for new code; `.claude/rules/general-unit-test.md` and
   `.claude/rules/quality-tiers.md` specify >= 85 percent line and >= 75 percent branch uniformly.
   Both are live in the repository and they disagree. This branch clears every one of those numbers,
   so nothing turned on it here, but the conflict will eventually decide a marginal case.
7. **Optional documentation follow-up already identified in the spec.** `breadcrumb-coordinator-hub-defects-501/spec.md`
   records this residual as "shipped as designed" at `:1062` and as a known limitation at `:432-437`.
   Once this merges those records become historical. The spec correctly declines to widen this
   footprint to amend them.

## Verdict

All 20 acceptance criteria are delivered and independently verified. The full toolchain passed in a
single ordered pass, re-verified against retained raw logs and one live re-execution. Coverage clears
every floor in force in this repository. The footprint is exactly the two files the spec authorizes.
The one substantive finding, CR-1, concerns the strength of a claim in the spec rather than the
correctness of the code.

**Blocking findings across all three artifacts: 0** (0 FAIL, 0 blocking PARTIAL). No
`remediation-inputs` artifact was produced.
