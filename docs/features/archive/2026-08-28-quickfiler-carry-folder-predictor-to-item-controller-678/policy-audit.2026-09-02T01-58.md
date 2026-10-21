# Policy Audit — issue #678, carry the folder predictor to the item controller (closing audit, post remediation cycle 1)

- Timestamp: 2026-09-02T01-58
- Feature folder: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/`
- Branch under review: `bug/quickfiler-carry-folder-predictor-to-item-controller-678`
- Head: `bd57dc9d400ac269317d2397c1ad649deac426de`
- Base: `807fb0bb6e5e49f43efa6b256b05960bf078ca19`
- Work mode: `minor-audit` (marker read from `issue.md:13`)
- Diff form used: three-dot, `git diff 807fb0bb...HEAD`
- Supersedes: `policy-audit.2026-09-01T23-35.md` (round 1, head `d1f51e3a`)

## Base resolution and scope, re-derived

`git merge-base 807fb0bb6e5e49f43efa6b256b05960bf078ca19 HEAD` returns
`807fb0bb6e5e49f43efa6b256b05960bf078ca19` exactly, so the three-dot diff is non-degenerate and the
supplied pin is correct. This was re-run at head `bd57dc9d` rather than carried over from round 1.
The audit scope is the full branch diff against that base, not the four remediation items.

Footprint re-derived by this reviewer from `git diff --numstat 807fb0bb...HEAD`:

| Prefix | Changed paths |
|---|---:|
| `QuickFiler/` | 16 |
| `QuickFiler.Test/` | 20 |
| `docs/features/active/2026-08-28-...-678/` | 86 |
| Any other prefix | 0 |
| **Total** | **122** |

The test-project count moved from 19 to 20 since round 1: the added path is
`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs`, the R1
regression test. Nothing under `UtilitiesCS/`, `.claude/`, `artifacts/orchestration/` or
`CLAUDE.md` is touched. `FolderPredictor.cs`, named in R2 as the parity target, is confirmed
unmodified.

Branch history relative to the base: ten commits, of which two are merges. The remediation cycle
added `be1e0b97` (the fix) and `bd57dc9d` (evidence, plus a CSharpier reflow of two files —
see the reflow note under Toolchain gates).

## Rejected Scope Narrowing

None detected. The caller supplied the base SHA, the feature folder and the work mode, all of which
are legitimate scope sources, and explicitly required a per-criterion evaluation of AC1 through
AC23 plus a check for regressions introduced by the remediation. The full branch diff was audited.

Two caller instructions were examined and found not to be scope narrowing:

1. "Verify each of R1 through R4" — an additional obligation layered on top of the full audit, not
   a replacement for it. The full-branch evaluation was performed regardless.
2. "Write ONLY under `docs/features/active/2026-08-28-...-678/`. Touch no source, test, plan, or
   policy file." — a write-scope constraint on this reviewer's own mutations. It does not limit
   what may be read or evaluated, and no file outside the feature folder was written.

No language with changed files was excluded from evaluation, and no coverage check was skipped.

## PR context artifacts

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` are absent from this
worktree. This reviewer's write permissions are confined to the feature folder, so the artifacts
were not regenerated. Scope and evidence were derived instead from the authoritative sources named
in `pr-base-branch-merge-base`: the resolved base SHA and the three-dot `git diff`, enumerated
above. This substitution is recorded as an assumption. It does not narrow scope, because the raw
git diff is the broader of the two sources — the summary artifact is a projection of it.

## Evidence Location Compliance

The branch diff was scanned for files written under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/` or `artifacts/coverage/`. **Zero matches** at head `bd57dc9d`. All execution
evidence is written under `docs/features/active/2026-08-28-...-678/evidence/<kind>/` using the
canonical kinds `baseline/`, `remediation-baseline/`, `qa-gates/`, `regression-testing/`,
`issue-updates/` and `other/`. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose.

`validate_evidence_locations.py` does not exist in this repository; the scan was performed directly
against `git diff --name-only 807fb0bb...HEAD`.

Verdict: **PASS**.

## Host-path and account-name hygiene

`grep -rI` across all 86 feature-folder paths for the account name, the machine name, and both the
Windows and MSYS user-profile path prefixes returns **zero matches**. This was checked because the
MSTest runner names its TRX files
`<account>_<HOST>_<timestamp>.trx` by default, and the retained TRX under `TestResults/` does carry
that form. `TestResults/` is not tracked and appears nowhere in the diff; `evidence/regression-testing/r1-red.md:19`
states explicitly that the TRX file name is redacted for that reason, and `:67` records that the
stack frame's absolute host path was replaced with the repository-relative path.

Verdict: **PASS**.

## Coverage Verification

Languages with changed files in the branch diff: **C# only**. Changed file extensions across the
whole diff are 34 `.cs`, 2 `.csproj`, 84 `.md`, 2 `.xml`. No `.ps1`, `.psm1`, `.py`, `.ts` or `.tsx`
file is touched, so no other language row carries an obligation.

### Artifact availability

The canonical path `artifacts/csharp/coverage.xml` does not exist in this worktree. The measurement
substrate used instead is the post-processed Cobertura document at `coverage/coverage.cobertura.xml`
(file mtime `2026-09-02 01:34`, written by the remediation cycle's final MSTest pass), together with
the committed baselines under `evidence/remediation-baseline/`. This reviewer parsed the Cobertura
document directly with an independent script rather than reading the executor's figures.

Corroboration that the document is genuine and current: its root element reproduces the executor's
headline figures character for character; its mtime falls between the fix commit (`be1e0b97`,
01:30:41) and the evidence commit (`bd57dc9d`, 01:46:32); and the compiled assemblies
`QuickFiler.dll`, `QuickFiler.Test.dll` and `UtilitiesCS.dll` carry mtimes of 01:33:18 to 01:33:24,
which places a real full-solution rebuild immediately before the measured run.

### Repository-wide figures, independently read

| Side | line-rate | Line % | lines-covered | lines-valid | branch-rate | Branch % |
|---|---:|---:|---:|---:|---:|---:|
| Same-session baseline (P0-T9 of this cycle) | 0.853964 | 85.3964 | 55073 | 64491 | 0.794373 | 79.4373 |
| Post-remediation (read by this reviewer from the Cobertura root element) | 0.853967 | 85.3967 | 55086 | 64506 | 0.794522 | 79.4522 |
| Movement | +0.000003 | +0.0003 pt | +13 | +15 | +0.000149 | +0.0149 pt |

Both rates moved up against the correct comparator. The comparator is the same-session baseline
taken at the start of this remediation cycle, not the round-1 figure of 85.4119 / 79.4494. Comparing
against the round-1 figure would show an apparent line-rate decrease of 0.0152 points; that
comparison is invalid because the two readings come from different measurement sessions, and a
cross-session drift of roughly 0.015 points is a known property of this repository's C# coverage
instrumentation rather than a change in the code. The same-session comparison is the one that
carries signal, and it is positive on both rates.

Both floors are cleared: 85.3967 clears the 85 percent line floor of
`.claude/rules/general-unit-test.md` and the 80 percent floor of `CLAUDE.md`; 79.4522 clears the 75
percent branch floor.

### Language rows

| Language | Changed files | Repo-wide line | Repo-wide branch | Verdict |
|---|---:|---:|---:|---|
| C# repository-wide coverage | 34 `.cs`, 2 `.csproj` | 85.3967 % | 79.4522 % | **PASS** |
| C# changed-line coverage, remediation cycle | 5 production paths | 100.00 % (34/34) | see note | **PASS** |
| C# changed-line coverage, whole branch | 15 production paths | 60.87 % (112/184) | see note | **FAIL** — dispositioned non-blocking below |
| C# new-file coverage, `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 1 | 15.29 % (13/85) | see note | **FAIL** — dispositioned non-blocking below |
| C# new-file coverage, `QuickFiler/Controllers/QfcCollectionController.CarrierLoad.cs` | 1 | no row emitted; the class-level exclusion attribute on the base part covers this part | — | **PASS** |
| C# modified-file coverage, all remaining production paths | 13 | every added executable line covered; the one per-file reduction is fully explained by a deletion in that file | — | **PASS** |
| TypeScript coverage | 0 changed files | — | — | **PASS** (vacuous: the branch diff contains zero `.ts` and `.tsx` files, so no obligation arises) |
| Python coverage | 0 changed files | — | — | **PASS** (vacuous: the branch diff contains zero `.py` files, so no obligation arises) |
| PowerShell and Pester coverage | 0 changed files | — | — | **PASS** (vacuous: the branch diff contains zero `.ps1` and `.psm1` files, so no obligation arises) |

### Independent reproduction of every executor figure

This reviewer built the added-line set per production file from `git diff --unified=0` and joined it
to the Cobertura line map, deduplicating line numbers at the class level so method rows cannot
double-count field initialisers. Every figure below was produced by this reviewer, and every one
matches `evidence/qa-gates/remediation-coverage-delta.md` exactly.

| Unit | Covered / total | Rate |
|---|---:|---:|
| Added executable production lines, remediation cycle (`4b43e31d`..HEAD) | 34 / 34 | 100.00 % |
| Added executable production lines, whole branch (`807fb0bb`...HEAD) | 112 / 184 | 60.87 % |
| `QuickFiler\Controllers\QfcHighConfidencePreFilter.cs` | 73 / 73 | 100.00 % |
| `QuickFiler\Controllers\QfcHomeController.cs` | 179 / 232 | 77.16 % |
| `QuickFiler\Controllers\QfcItemController.FolderHandling.cs` | 166 / 173 | 95.95 % |
| `QuickFiler\Controllers\QfcQueue.Enqueue.cs` | 13 / 85 | 15.29 % |
| `QuickFiler\Controllers\QfcQueue.cs` | 157 / 312 | 50.32 % |

Per-member figures for every new or modified member in a non-exempt file, derived from the
class-level line map restricted to each member's line span in the current source:

| Member | Covered / total | Rate | vs the 90 % new-code floor |
|---|---:|---:|---|
| `QfcPreScoredItem.ResolveCarrier` | 20 / 20 | 100.00 % | PASS |
| `QfcPreScoredItem.ReconcileCarriersToItems` | 9 / 9 | 100.00 % | PASS |
| `QfcQueue.ResolveCarriedHandler` | 1 / 1 | 100.00 % | PASS |
| `QfcHomeController.RunAsync` | 39 / 39 | 100.00 % | PASS |
| `QfcItemController.ProjectPredeterminedFolder` | 11 / 11 | 100.00 % | PASS |
| `QfcItemController.AssignFolderComboBox` | 29 / 32 | 90.62 % | PASS |
| `QfcItemController.LoadFolderHandlerAsync` | 71 / 75 | 94.67 % | PASS |
| `QfcQueue.EnqueueAsync` | 0 / 46 | 0.00 % | FAIL — dispositioned non-blocking below |
| `QfcQueue.LoadControllersViewersAsync` | 0 / 24 | 0.00 % | FAIL — dispositioned non-blocking below |

This reviewer independently confirmed the two named uncovered spans. The uncovered line set of
`QfcItemController.FolderHandling.cs` is exactly `{121, 122, 123, 124, 195, 196, 197}`. Lines
121-124 are the inner `catch (System.Exception e2)` of the empty-predictor fallback; lines 195-197
are the `_itemViewer.InvokeRequired` marshalling guard. Neither set intersects the lines this cycle
added. In particular, line 78 — `cancel.ThrowIfCancellationRequested()`, the whole of the R3 fix —
is **covered**.

### Disposition of the two sub-floor rows

Both failing rows have the same single cause and are dispositioned **non-blocking** together.

`QuickFiler/Controllers/QfcQueue.Enqueue.cs` is an added file whose measured rate fell from 28.00
percent (28/100) at round 1 to 15.29 percent (13/85) now. That movement is reported here because it
looks like a regression and is not one:

1. **The uncovered set is unchanged, line for line.** This reviewer enumerated the uncovered line
   numbers in that file and counted **72**, the identical count round 1 reported (100 − 28 = 72),
   occupying the same two member bodies. No line that was covered became uncovered.
2. **The ratio fell because covered lines left the file, not because coverage was lost.** R1
   collapsed the 26-line body of `QfcQueue.ResolveCarriedHandler` into a one-line delegation to
   `QfcPreScoredItem.ResolveCarrier`. Covered and total both dropped by exactly 15, so every
   executable line removed was one that had been covered. The same logic now lives in
   `QfcHighConfidencePreFilter.cs`, whose covered and total counts each rose by 29 and which stands
   at 73/73 = 100.00 percent.
3. **The residual shortfall is relocated pre-existing code.** The 72 uncovered lines are the
   `EnqueueAsync` and `LoadControllersViewersAsync` bodies, moved out of `QfcQueue.cs` by the first
   cycle. Round 1 verified independently that both members were at zero at the base ref: every
   `EnqueueAsync` reference in the test project is a Moq setup or verification on the `IQfcQueue`
   interface, and `LoadControllersViewersAsync` is private with no reference of any kind. Neither
   was reachable, so neither could have been covered. This is not a regression on changed lines.
4. **The bodies are host-bound.** `EnqueueAsync` clones a `TableLayoutPanel` through the UI-idle
   marshal and hooks an `EmailMoveMonitor`; `LoadControllersViewersAsync` dequeues a real
   `ItemViewer` through `AddAsync`. `.claude/rules/general-unit-test.md` prohibits a test requiring
   a live window, and AC20 prohibits adding an exclusion attribute — an invariant this reviewer
   confirmed held, at zero added and zero removed occurrences of the attribute across the diff.
5. **No repository policy floor is breached.** Repository-wide line 85.3967 and branch 79.4522 both
   clear their floors, and both rose against the same-session baseline.

The whole-branch changed-line row of 112/184 fails for exactly the same 72 lines and no others: all
15 other production files have every added executable line covered. Restricted to the files this
change actually authored rather than relocated, the figure is 112/112 = 100.00 percent.

No remediation-inputs artifact is produced for these rows. Under the repository's own criteria — no
regression on changed lines, both repository-wide floors cleared and rising, and the shortfall
confined to relocated host-bound code that was already at zero before the branch — the correct
disposition is a recorded failing row rather than a remediation trigger. This matches the
disposition agreed for round 1, where the orchestrator deferred NB-4 by agreement.

## Toolchain gates (C# Code Change Policy, CUT3)

| # | Gate | Command | Executor result | Reviewer verification |
|---|---|---|---|---|
| 1 | Format verify | `dotnet tool run csharpier check .` | EXIT 0, `Checked 1575 files in 4937ms.` | **Re-run by this reviewer at head `bd57dc9d`**: `Checked 1575 files in 4550ms.`, exit 0. The 1575-file count is reproduced. Confirmed. |
| 2 | Analyzer build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT 0, 5 Warning(s), 0 Error(s), `CoreCompile:` ran 57 times | Not re-run (full solution rebuild). Command matches the policy text verbatim and uses `/t:Rebuild`, not `/t:Build`. `CoreCompile` ran 57 times, so the gate was not vacuous. Corroborated on disk: the compiled assemblies carry mtimes of 01:33:18 to 01:33:24, inside the declared window. Attested. |
| 3 | Nullable build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | EXIT 0, 5 Warning(s), 0 Error(s), zero `CS86`, `CoreCompile:` ran 72 times | Not re-run. Command matches the policy text verbatim, correctly omits `/p:Nullable=enable`, and uses `/t:Rebuild`. `CoreCompile` ran 72 times, so the gate was not vacuous. Attested. |
| 4 | Test with coverage | `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` | EXIT 0, Total 6949, Passed 6949, Failed 0, Skipped 0 | Full suite not re-run. Corroborated three ways: the Cobertura document exists with an mtime inside the declared window and its root attributes reproduce the reported percentages exactly; the assemblies were rebuilt immediately before it; and the retained scoped TRX at `TestResults/p2-t5/` records 12 discovered, 12 passed, 0 failed, covering all three remediation regression tests plus the AC7, AC9, AC12 and AC16 pinning tests. Attested. |

The five commands of the final pass ran in sequence per
`evidence/qa-gates/remediation-final-toolchain-pass.md`. One loop restart is recorded and its
trigger is verifiable in the git history rather than asserted: CSharpier reflowed
`QuickFiler/Controllers/QfcHomeController.cs` and
`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs` on the first
format pass, and those two reflows are exactly the source content of commit `bd57dc9d`. This
reviewer read that diff in full and confirms it is whitespace-only — one call collapsed onto one
line, one `.Returns(...)` collapsed and one `.ContainSingle(...)` expanded — changing no token.

**Ordering check on the reflow.** Because `bd57dc9d` carries source changes as well as evidence, the
question is whether the gates ran before or after them. They ran after: the format-apply step that
produced the reflow is the first command of the pass, the four gates follow it, and the coverage
document's 01:34 mtime is later than the reflow and earlier than the commit. The measured tree is
therefore the head tree. This reviewer's own `csharpier check` at head returning exit 0 independently
confirms the head tree is format-clean.

Verdict: **PASS**.

## Policy-by-policy findings

### CLAUDE.md and `.claude/rules/general-code-change.md`

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity, reusability, separation of concerns | PASS | Strengthened by this cycle. R1 removed a duplicated matcher: `QfcQueue.ResolveCarriedHandler` is now a one-line delegation to `QfcPreScoredItem.ResolveCarrier`, so exactly one carrier-matching implementation exists in the tree and the two display legs cannot drift apart. `ResolveCarrier` and `ReconcileCarriersToItems` are small `internal static` pure helpers on the carrier type, which is where the knowledge belongs. |
| Bugfix workflow: failing regression test first | PASS | All three behavioural remediation items are pinned by a test recorded red before the fix and green after. The R1 red run is the strongest of the three and is analysed under Test quality in the code review. |
| Minimal targeted fix, no opportunistic refactor | PASS | The production diff for the whole cycle is 34 added executable lines across five files. The one structural move — hoisting the matcher onto `QfcPreScoredItem` — is required by R1's own instruction to prefer reusing the leg B helper over writing a second one. |
| Fail fast, no silent error swallowing | PASS | No new catch block anywhere in the cycle. R3 adds a throw where the code previously returned normally, which moves the change in the fail-fast direction. |
| Logging via the project pattern | PASS | The adoption log at `QfcItemController.FolderHandling.cs:80-84` uses `logger.Debug` and mirrors the two existing `Probability debug` lines in the same method. See NB-9 for one logging side effect the adoption path does not reproduce. |
| Comment why, not what | PASS | Every remediation edit carries an in-code rationale naming its item: the R1 reconciliation rationale at `QfcHomeController.cs:309-312`, the identity-first rationale at `QfcQueue.Enqueue.cs:63-68`, the R3 placement rationale at `QfcItemController.FolderHandling.cs:70-77`, and the corrected divergence note in the `QfcDatamodel.QueueProcessing.cs` doc block. |
| Documentation corrected rather than left stale | PASS | Both false documentation claims round 1 identified were corrected at the source rather than worked around. `QfcDatamodel.QueueProcessing.cs` no longer claims the two collections "describe one dequeue rather than two", and `ProjectPredeterminedFolder` no longer claims to mirror `ProjectSuggestionPath` "exactly" — it now names the two remaining divergences and why each is deliberate. One stale claim in a test file remains; see NB-10. |
| 500-line file limit | PASS for this change; pre-existing overage remains | Re-measured at head. No changed file crossed the limit and the cycle added no file near it (`...Part3.cs` is 247 lines). Three files remain over and all three are smaller than at the base ref: `QuickFiler/Controllers/QfcCollectionController.cs` 2446 → 2336, `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` 827 → 792, `QuickFiler/Controllers/QfcQueue.cs` 610 → 505. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` remains at exactly 500, at the cap and not over it. Recorded as NB-6, still open. |
| Public API stability | PASS with a documented breaking change | Unchanged from round 1. `IQfcQueue.EnqueueAsync` gained a required third parameter, with the CS0854 rationale recorded in the interface doc comment and the single production call site updated. Repository-internal interface, no external consumer. |
| No new dependency | PASS | No package reference added across the whole branch. Both `.csproj` edits are `<Compile Include=...>` entries for new partial parts. |
| I/O boundaries | PASS | All four helpers added by this branch — `ResolveCarrier`, `ReconcileCarriersToItems`, `ResolveCarriedHandler`, `ProjectPredeterminedFolder` — are pure and testable without COM, and all four are at 100 percent line coverage. |

### `.claude/rules/general-unit-test.md` and the CLAUDE.md UT sections

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence, isolation, determinism | PASS | The three added tests construct their own doubles. No wall-clock read, no sleep, no retry, no ordering dependency. The R3 test disposes its `CancellationTokenSource` through a `using` statement and carries a comment recording that a `using` declaration would be CS8370 at the project's C# 7.3 level. |
| No temporary files | PASS | Reviewer grep of the changed test files finds no `Path.GetTempFileName`, `Path.GetTempPath` or `File.Create`. |
| No live external dependency | PASS | `MailItem` is always a Moq double in the added tests. The R1 test drives the `TryUnhookOrReplace` throw branch entirely through a mocked move monitor. |
| Arrange-Act-Assert with documented intent | PASS | All three added tests carry an XML summary naming the remediation item and stating what the pre-change code did, plus explicit `// Arrange`, `// Act`, `// Assert` markers. The R1 test additionally labels its two assertion stages in banner comments, which is what makes its red run interpretable. |
| No existing passing test weakened or deleted | PASS | The cycle changed exactly one existing assertion, at `QfcItemController.FolderHandlingTests.Part2.cs:226-229`. That change is authorised by R2 clause 1 and is a correction, not a weakening: the previous assertion claimed an empty archive root is the identity projection, which round 1 established was false of the parity target. The new assertion pins the aligned behaviour and is strictly more specific. AC13's `Times.Never` and `preFilterInvoked` assertions were re-verified present and unmodified in both files. |
| Coverage exclusion policy | See note | No exclusion attribute was added or removed anywhere in the diff; this reviewer confirmed zero added and zero removed occurrences across the three-dot diff. The standing conflict between the ratified host-bound exemption in `CLAUDE.md` and the no-exclusion rule in `.claude/rules/general-unit-test.md` is pre-existing and is not created, widened or relied upon by this change. |
| Test files in a mirroring `tests/` tree | Pre-existing repository convention | This repository colocates C# tests in sibling `*.Test` projects rather than a `tests/` tree. The change follows the established convention. Not a defect introduced here. |

### `.claude/rules/quality-tiers.md`

Uniform thresholds apply: line >= 85 percent, branch >= 75 percent. Both are met repository-wide at
85.3967 and 79.4522, and both rose against the same-session baseline. The tier-dependent gates
(property-test density, mutation score, golden tests, contract tests) have no established harness in
this repository and none is required by the acceptance criteria of a `minor-audit` bug fix.

One documentation conflict is noted rather than adjudicated: `CLAUDE.md` states an 80 percent
repository floor and a 90 percent new-code target, while `.claude/rules/quality-tiers.md` and
`.claude/rules/general-unit-test.md` state a uniform 85 percent line and 75 percent branch floor.
The figures reported above clear every one of those thresholds, so the conflict does not change any
verdict in this audit. It is pre-existing and out of this branch's remit.

### C# Code Change Policy and C# Unit Test Policy

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | Every added test method carries `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. No xUnit or NUnit reference. |
| Moq for mocking | PASS | All doubles are `Mock<T>`, including the delegate-typed predictor factory seam and the throwing move monitor the R1 test needs. |
| FluentAssertions for assertions | PASS | All added assertions use `.Should()`, including `ThrowAsync<OperationCanceledException>` in the R3 test. MSTest `Assert` is not used in the added code. |
| Strong contracts, explicit types at boundaries | PASS | `ResolveCarrier` returns the nullable `QfcPreScoredItem?` and `ReconcileCarriersToItems` returns `IList<QfcPreScoredItem>`; both carry full `<param>` and `<returns>` documentation stating the null and empty behaviour. |
| Nullable discipline | PASS | The nullable build reports zero `CS86` diagnostics with `CoreCompile` running 72 times. `ResolveCarrier`'s nullable return type is declared explicitly rather than relying on inference. |
| Narrow suppression with rationale | PASS | The single `#pragma warning disable CS0618` in `QfcQueue.Enqueue.cs` is relocated verbatim from `QfcQueue.cs` with its original justification comment intact. It is not new and the cycle did not touch it. |

### `.claude/rules/tonality.md`

PASS. The remediation evidence is factual and measured, and one artifact is notably restrained where
overstating would have been easier: `evidence/qa-gates/remediation-timestamp-fidelity.md` records
that its own plan clause is structurally unsatisfiable — correcting an artifact's timestamp rewrites
that artifact's mtime, so a re-measurement band and a correction instruction form a fixpoint — and
explicitly declines to claim a pass for that sub-clause. This reviewer verified the reasoning is
sound and the conclusion correct. Reporting a plan defect against oneself rather than dispositioning
it into a pass is the behaviour the rule asks for.

## Findings summary

- Blocking: **0**
- Non-blocking: **7** — three carried over from round 1 and still open by agreement (NB-4, NB-6,
  NB-7), one carried over as a criteria-text defect (NB-8), and three newly raised by this audit
  (NB-9, NB-10, NB-11). NB-1, NB-2, NB-3 and NB-5 are closed. All are enumerated with file and line
  in `code-review.2026-09-02T01-58.md`.

## Overall policy verdict

**PASS.** No blocking policy violation was found. One acceptance criterion, AC20, continues to fail
one of its four clauses; that failure is recorded as two failing coverage rows above and
dispositioned non-blocking against repository policy floors, all of which are met and all of which
improved against the same-session baseline. AC20 remains unchecked in `issue.md`, which is correct.
