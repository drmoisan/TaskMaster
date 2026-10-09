---
name: review-residuals-index
description: One-line PASS/0-blocking pointers for a cluster of closed-issue reviews, kept for lookup if any issue number resurfaces (e.g. as a follow-up or reopened)
metadata:
  type: project
---

Each of these was a full policy-audit/code-review/feature-audit cycle that closed PASS with 0 blocking
findings. Consolidated here only because the individual issue is unlikely to resurface; if one does,
search git history / the issue tracker for the full artifact set under
`docs/features/active/<issue>/` or `docs/features/archive/<issue>/` first — this line is a pointer,
not the full record.

- **#442**: AC-19 stays unchecked (ratified deviation); residuals CR-1/CR-2/CR-3 filed as #645; PA-2
  agent-memory paths; post-442 baseline 85.1255/79.2096.
- **#444**: 3 ACs deferred-pending-PR-body (472-10/482-11/482-12); OB-1 merge-up owed vs #493 fan-in;
  NavigationTests at 498/500 lines; raw Cobertura XMLs survive in executor worktree for re-parse.
- **#446**: AC28-vs-AC18 contradiction owed a maintainer amendment (71.0% ceiling); Actions.cs carve-out
  is bound by COM loaders, not MessageBox.
- **#449**: untracked #584 promotion doc owed a non-child route; unused usings in a base test file;
  AC-supersession-via-plan-provision pattern validated.
- **#457**: CR-1 rollup-rebuild drift vs the merge path; AC15 potential_to_issue owed at epic close;
  post-457 baseline 0.855355/0.790134.
- **#468**: dual-floor coverage rows (80% PASS / 85% FAIL non-blocking) hook-verified; #623 baseline was
  stale (2437); AC-27/28 deferred to default-branch merge.
- **#476**: 90% floor treated as non-binding for exemption-narrowing entrants; CR-1 Disposed-subscription
  retention promotion; post-476 baseline 85.1435/79.2018.
- **#484**: F1 ApplyReadEmailFormat TOCTOU (promoted); F4 OneDrive silent-skip; D-1/D-2 plan-provision AC
  divergences accepted.
- **#488**: TRX host tokens (runUser+storage) partial-sanitize accepted as precedent, non-blocking; a
  21.4MB Cobertura plus C6-stale promotions owed at fan-in; #670 filed.
- **#501**: "no compliant test placement" premise failed on inspection (HubCoverageTests at 478/500 lines
  unexamined by the plan); a redundant `Abandon` call counted as coverage without real assertion power;
  full-suite logs were left uncommitted; post-501 baseline 85.1448/79.2202.
- **#511** (rescope re-audit): residuals CR-1 stale RCA narrative + CR-2 AC-vs-deleted-TRX wording; the
  PR must not claim to close #511/#571 — #592/#594/#597 carry the real defects.
- **#553** (CI split, cycle 2): 18/18 AC; reviewer self-dispatched a ci.yml run to cure green-run head
  drift; the branch rebase made ALL caller-supplied SHAs stale.
- **#614** (cycle 2): exited NO-GO/1 blocking — RC-1 widened filing guard admitted an archive-root-exact
  row that `RequireArchiveRelativeStem` throws on; a post-Hide async-void crash was found; CR-1 closed.
- **#635**: Markdown-only evidence audit; drift-invariant classification identities held across a 3rd
  commit; hook payload key is `output`; the session-cwd artifact mirror was needed again.
- **#670**: 14/14 AC earned, none unchecked. Strong RED step (behavioural mutation `_ = ex;` + DLL
  mtime-vs-build-start staleness check). TRX sanitisation used XML-**escaped** `&lt;user&gt;`, which is
  why absence and well-formedness both pass — `runUser` included. Residuals: CR-1/CR-2 the sink
  property is settable and unguarded, so a null or throwing sink faults the guard's task and
  reinstates the unobserved-fault defect (LATENT, shared with `EfcFormController.BoundaryErrorSink`
  from #464 — fix both together or neither); PA-1 AC10 stage 4 substituted
  `Invoke-MSTestWithCoverage.ps1` for literal `vstest /EnableCodeCoverage`; PA-2 the 4th test landed
  in `InitializationTests.cs`, outside the spec's file table and AC11's enumeration, because
  `Part3.cs` finished at 498/500. Post-670 baseline 85.3771/79.3997.
- **#663**: 15/15 AC confirmed on evidence; strongest RED-first record seen (verbatim `but found True`
  FluentAssertions text for exactly 3 tests) and the 6927+7=6934 arithmetic independently corroborates
  "no existing test removed". Residuals: raw Cobertura deleted per plan so coverage is executor-attested
  (honest FAIL row on canonical-artifact presence, thresholds PASS at 85.3726/79.4078); AltGr and
  Alt+Shift still claimed via the `Keys.None`/`Keys.Menu` arms but that is pre-existing, not a
  regression; `IsAltKeyCommand` left with zero compiled consumers deliberately; AC-15 manual validation
  deferred so the user-facing Alt+M outcome is unconfirmed. `quality-tiers.yml` does NOT exist at repo
  root, so no tier-dependent gate (property tests, mutation score) can be evaluated for any project.
- **#729**: 21/21 AC PASS, 0 blocking, none unchecked. Post-729 baseline 0.853836/0.794529 (base
  0.85386/0.794589). The interesting reviewing moves: (a) a `TimeProvider` seam had to be an explicit
  **overload pair**, not an optional param — `WaitAsync` is a method group at
  `StoreRehookCoordinator.cs:102` and an optional param removes the only candidate (CS0123); verify by
  grepping call sites *and* by the zero-`CS0123` count in the analyzer log. (b) `condition-coverage`
  moving `50% (1/2)` -> `100% (2/2)` on a null-conditional line proves a previously-unexercised null
  branch is now deterministically taken — here it proved the pre-existing `timer?.Dispose()` leak path
  in `NonBlockingDelay.cs:81` is now demonstrated by the suite (still pre-existing on base, char-for-char;
  out-of-scope observation). (c) To falsify a "these are the only N classes that capture+restore+assert
  on `Console.Out`" claim, intersect `grep -rln Console.SetOut` with `grep -rln StringWriter` and inspect
  the intersection — the one-way `Console.SetOut(new DebugTextWriter())` initializer pattern is the noise
  (23 of 28 files). Residuals, all doc-accuracy and non-blocking: `spec.md` Write Set heading claims
  "every file this plan's diff creates" but lists only the 27 code entries of 81; two plan claims that
  P8-T22 stages the #743 promotion record (it was committed in the branch's FIRST commit `3fc7fafe`);
  P7-T9 writes no evidence artifact; AC18's wording doesn't cover the promoted record that AC16 mandates.
- **#825** (ETL deadline mechanics): 35/35 AC PASS, 0 blocking. Post-825 processed Cobertura
  85.6686/79.8366; new-and-changed 20/22 = 90.91%. Reusable moves: (a) **a widened `T?` return type
  can produce zero nullable warnings because every consumer sits in a nullable-DISABLED file** — the
  green gate is NOT evidence the propagation was checked. Grep `#nullable` per consumer file; here
  only `DfDeedle.cs` was enabled (and guarded), while `OlTableExtensions_Tests.cs` enables only
  `#nullable enable annotations` in 5 narrow scoped regions that miss its `EtlAsync` call site, so
  `data[0,0]` on a now-nullable value is silently unchecked. (b) When an AC's literal wording is
  unsatisfiable, an **occurrence-count transition (2 -> 1) plus an anchored diff** is a sound
  substitute and is more discriminating than the unachievable zero-hit form — grade PASS-with-disclosed-
  substitution, not PARTIAL, provided it's disclosed in the plan AND the check-off text AND an evidence
  artifact. (c) A vacuous AC verification (a search that already returned no hit before the change)
  must be replaced by the substance clause; check the pre-change state before crediting such a search.
  (d) `ArmingBarrierTimeProvider.Armed` is a **latch**, so it drops a signal when two timers arm inside
  one await window — threading a provider one hop further inserts a new first signal and breaks any
  test that consumes signals in a fixed order; the failure mode is a HANG (assertion inside the `try`,
  gates released only in the `finally`), not a clean failure. (e) `await barrier.Armed` with no
  `[Timeout]` turns the regression it exists to catch into a hang. Residuals, all non-blocking:
  `TimeOutTask.cs` 966 lines and `OlTableExtensions_Tests.cs` ~1822 lines both still over the 500 cap
  (only the former has a recorded follow-up); `spec.md` says 968 lines / 43-line reduction where the
  measured values are 966 / 45 (`issue.md` and `file-size-accounting.md` are correct).
- **#882** (TransactionGate bounded acquisition, parallel run bugs-2026-09-28): 12/12 AC PASS, 0 blocking,
  no-Bash review. Test-only C# change (both files in QuickFiler.Test), so the only coverage figure is a
  QuickFiler.Test-scoped first-party observation (24.42/23.20, plan D1) — written as an honest FAIL row,
  non-blocking, no remediation-inputs; the +12 covered-line delta was cross-package run noise (UtilitiesCS
  +15 / QuickFiler -3, identical denominators). Moves worth reusing: (a) the counter-balance test passing
  in the SAME process as a new failed-probe test is live proof the acquisitions increment sits after the
  wait; (b) verify overload-introduction safety by grepping for a method-group use (`= Type.Method;`),
  not only for call sites; (c) FluentAssertions generic `NotThrow<T>()` only fails on T — a spec-mandated
  shape, so Informational only. Residuals: planner-owned P4-T22 comma-operator payload collapse
  (executor corrected in-band), P4-T24 HEAD-listing clause unsatisfiable after mid-plan commits, and the
  caller's inventory omitted two pre-existing `.claude/agent-memory/orchestrator` paths on the diff.
- **#930** (UiThread dispatcher-exit null guard + ILGlobals dead statics + doc-comment counts, parallel
  run bugs-2026-09-28, no-Bash minor-audit): 6/7 AC PASS, AC7 PARTIAL and UNCHECKED, 0 blocking, 1
  non-blocking. First-party 85.31/79.71 -> 85.32/79.73 (same-session, D3 four shell-icon classes excluded
  identically). The only finding: two evidence lines carried `C:\Program Files\...\vstest.console.exe`
  ("Using vstest.console:" runner output transcribed verbatim), which the plan's sanitize gate (account,
  host, root, `<drive>:\Users\`) cannot match — see [[sweep-drive-letter-paths-not-just-identity-patterns]].
  Also: Phase 0 `Timestamp:` values were estimates later replaced by file mtimes (disclosed, no figure
  affected; Informational); `<Analyzer Include>` HintPath skew (Meziantou 3.0.235 vs packages.config
  3.0.290; MSTest.Analyzers 4.4.0 vs 4.4.1) is on origin/main and breaks a cold analyzer Rebuild with
  CS0006 until the older packages are nuget-installed; `quality-tiers.yml` still absent at repo root; the
  session-cwd `pr_context.summary.txt` was a stale `.md`-only pair (documentationandmemories) so the hook's
  language checks were disarmed there, and the three artifacts had to be mirrored into the session cwd
  again. Unused `using System.Collections.Generic;` left in ILGlobals.cs after the Dictionary field
  deletion (Informational).
- **#928** (scoped coverage-runner threshold skip, parallel run bugs-2026-09-28, no-Bash minor-audit): cycle 1
  1 blocking (AC6: entry-point changed line uncredited by breakpoint coverage), cycle-1 exit PASS 7/7 AC,
  0 blocking after relocating the gate into the path-loaded part file (details in
  [[928-review-residuals]] and [[pester-breakpoint-coverage-binds-to-first-parsefile-copy]]). Residuals P-1
  to P-4 owed by the orchestrator (P-4: the bundled PoshQC coverage document never covers `scripts/`).
- **#942** (EngineToggleStateCoordinator `CompletePrime` report-then-clear reorder, parallel run
  bugs-2026-09-28, no-Bash full-bug): 14/14 AC PASS, 0 blocking, first-party 85.32/79.73 -> 85.31/79.72
  (DIRECT route, same four shell-icon classes excluded both stages), coordinator 143/143 and 37/38 at both
  stages with the moved `TryRemove` line hits=1, verified by reading the gitignored
  `coverage/final-942.cobertura.xml` class element directly. Fail-before was a real run against the
  hash-identical base file (24/1, `BeSameAs` text). Residuals, all non-blocking: hazard B (registration
  racing removal on a synchronous non-success prime; promoted as #944 per the session branch name), a
  throwing error sink would now leave the marker registered (log4net sink never throws; `try/finally`
  is a documented non-goal), `artifacts/csharp/coverage.xml` unpopulated (runner writes `coverage/`),
  `quality-tiers.yml` still absent. Hook note: a stale session-cwd `pr_context.summary.txt` listing only
  `.csproj`/`.yml`/`.md` disarms the C# check (`^\.cs$` does not match `.csproj`); artifact tokens were
  advertised with the 3-`..` traversal form from [[928-review-residuals]], no mirror written.
- **#956** (SortEmail six-way partial split + `YesNoToAllPromptSession` seam, parallel run bugs-2026-09-28,
  full-bug, Bash limited to `git diff`/`git log`): 17/17 AC PASS, 0 blocking, 6 non-blocking. First-party
  85.33/79.71 -> 85.36/79.75; TrySave class 63/66 with exactly L49/L154/L155 at 0 hits (coordinator-ruled
  exemptions, reviewer concurred); session 20/20. Details and the Cobertura-Grep attribute-order lesson in
  [[956-review-residuals]].

## Moved from MEMORY.md on 2026-10-09 (index size consolidation)

- **#928**: cycle 1 AC6 uncredited line; exit PASS 7/7 after part-file relocation; no-Bash review reads gitignored JaCoCo `<sourcefile>` nodes; 3-`..` hook path; bundled PoshQC coverage never covers `scripts/`. [[928-review-residuals]]
- **#929**: PASS 7/7; AC met by tests of a pre-existing rule accepted; labels led UTC clock 38-72 min; `.bak` residue follow-up. [[929-review-residuals]]
- **#940**: PASS 8/8; per-file coverage ruling verified at Cobertura `<class>` nodes; worktree reflog epochs as no-shell clock; stale session-cwd coverage.xml handled with an honest FAIL line. [[940-review-residuals]]
- **#944**: PASS 18/18; keyed TryRemove safe under single-writer-under-lock; Cobertura root epoch as clock. [[944-review-residuals]]
- **#945**: PASS 8/8; attribute-exempt overloads, per-file 24/25 as new-code figure; SortEmail.cs 1454-line pre-existing breach. [[945-review-residuals]]
- **#947**: PASS 7/7; empty `catch (Exception)` around a sink accepted via SafeLog precedent; `_notifyUnavailable` follow-up. [[947-review-residuals]]
- **#948**: PASS 16/16; lock-free check-then-record safe via marker serialisation; Glob hides git-ignored files. [[948-review-residuals]]
- **#950**: PASS 16/17, AC17 PENDING CI by ruling; attribute-excluded production file PASS on no-regression limb. [[950-review-residuals]]
- **#959**: cycles 1 and 2 PASS 25/27; overwritten projections hide a 1-branch repo-wide drift; check plan Status line vs task boxes (CR-8). [[959-review-residuals]]
- **#964**: PASS 8/8 both cycles; pure-move split isolated a pre-existing uncovered arm (non-blocking Minor). [[964-review-residuals]]
- **#968**: AWAITING_CI 31/32; caller-classed pending AC = one awaiting_ci blocker; hook-safe wording validator-confirmed. [[968-review-residuals]]
- **#927**: PASS 17/20 + 3 PENDING-CI; -0.01pp C# noise with zero prod change; 7-cell trap applies to every table. [[927-review-residuals]]
- Older one-liners (#565, #584, #645, #707, #730, #731, #735, #736, #751, #752, #799): see each `project_<N>-review-residuals.md`.
