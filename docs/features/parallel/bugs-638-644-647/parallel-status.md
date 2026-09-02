# Parallel Status: bugs-638-644-647

Generated projection of `artifacts/orchestration/parallel-orchestrator-state.json`. Regenerated in full at each documented boundary; never hand-authored and never an input to scheduling.

## Run

| field | value |
| --- | --- |
| parallel_slug | `bugs-638-644-647` |
| mode | `open` |
| max_concurrency | 3 |
| current_cohort | 13 |
| recolor_generation | 10 |
| last_updated | 2026-09-02T10:09:45Z |
| next_step | `PARALLEL_RUN_CLOSED` |

Mode is `open`, so this run never auto-completed. It was terminated through `/parallel-close` at 2026-09-02T10:09:45Z and accepts no further admission.

The close gate was decided against durable state re-derived first, not against the recorded values: all fourteen pull requests were confirmed `MERGED` through `gh`, and every recorded merge commit and merge time matched, so the checkpoint needed no rewrite. No item was `in_flight`, so the gate permitted the close. The gate keys on item `state` alone and never on `merge_status`, which is why the seven item worktrees still on disk under deferred cleanup did not stand in its way; forcing a removal to unblock a close would have been the wrong remedy, and none was performed. The close itself is non-destructive: it closed no pull request, removed no worktree, and changed no item state. Items that never started would have kept their recorded states, since a close records that the run stopped admitting rather than that its items were withdrawn.

Every unordered pair of items conflicts — the conflict graph is complete on fourteen vertices with all ninety-one edges present and no independent pair — so every cohort is a singleton and the run executes serially.

`max_concurrency` was raised from 2 to 3 at 2026-09-01T12:00:28Z on operator instruction. The value is recorded on the checkpoint only; the run manifest still declares 2 and was deliberately not rewritten, because the manifest is static input authored by `parallel-planner` and is read-only to `parallel-orchestrator`. The change has no observable effect on this run, and the cap has never been the binding constraint. Under the per-edge cohort barrier `max_concurrency` is a pure throughput throttle governing how many *independent* lanes advance at once, and an all-pairs conflict graph has exactly one lane, so the barrier — not the cap — permits exactly one in-flight item at a time. Reaching genuine three-way concurrency would require the conflict graph to lose edges, which must not be obtained by narrowing a declared blast radius or reinterpreting an edge; the `parallel-orchestrate` skill prohibits doing either in order to widen a launch batch. The durable remedy is upstream, in the two derivation causes named below.

A recurring cause of that completeness is worth naming, because it is not real contention. Every C# plan in this repository cites `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and its siblings as the mandated coverage command it will *run*, not as a file it will *write*, and `config/blast-radius.json` lists `mandate_reads` exclusions for `.claude/rules/**`, `artifacts/**` and `.claude/agent-memory/**` but not for `scripts/vscode/**`. Those citations therefore survive derivation and become genuine `path_overlap` edges. Twenty-one of the ninety-one edges rest on a `scripts/vscode/` path. The relation is designed to fail closed and no radius was narrowed to work around it; the correction belongs upstream in the push-down source of `config/blast-radius.json`.

A second, independent cause has now overtaken it in one respect. Nineteen edges rest on a `.claude/agent-memory/` path, and those are not derivation artifacts at all: `.claude/agent-memory/**` is a `mandate_reads` exclusion, so the library never derives such a path from plan text. Every one of them was added by the reconciliation step that compares the derived radius against the item branch's actual diff. They are real contention over a genuinely shared file, and they exist only because that reconciliation is performed.

The run was briefly idle with all four original items terminal. Ten admissions then made it schedulable again. `/parallel-add 646` admitted a fifth item, `/parallel-add 656` admitted a sixth and deferred it behind 646, `/parallel-add 285` admitted a seventh and advanced the coloring to generation 3, `/parallel-add 633` admitted an eighth and advanced it to generation 4, `/parallel-add 670` admitted a ninth and advanced it to generation 5, `/parallel-add 678` admitted a tenth and advanced it to generation 6, `/parallel-add 287` admitted an eleventh and advanced it to generation 7, `/parallel-add 648` admitted a twelfth and advanced it to generation 8, `/parallel-add 662` admitted a thirteenth and advanced it to generation 9, and `/parallel-add 663` admitted a fourteenth and advanced it to generation 10. Item 285 was launched at 2026-09-01T11:59:21Z from cohort index 4, the lowest current-generation index, so its per-edge barrier held no prior-cohort neighbour: all four of its original conflicting neighbours 637, 638, 644 and 647 are terminal and carry no current-generation cohort assignment. Items 287, 633, 646, 648, 656, 662, 663, 670 and 678 remain ineligible: each conflicts with item 285, which sits in a strictly prior cohort and is neither `merged` nor `worktree_removed`.

Item 285 then shipped as pull request 715, merged at 2026-09-01T13:02:41Z as merge commit `09eae2e8`, which advanced `main` from `2b85134b`. Its child returned 51 of 51 plan tasks complete with zero blocking findings, zero remediation cycles, and 12 of 12 acceptance criteria checked. CI was confirmed by reading the five required check conclusions against the final head `637d4deb` after they settled, not from the child's report and not from a watcher exit code; the head was re-confirmed unchanged after settling, so the green conclusion belongs to the commit that merged. Item 287 launched immediately afterwards into cohort index 5, its barrier having been re-probed against the live checkpoint and returned `allow`.

Two things about item 285 are deliberately unfinished rather than overlooked. Its worktree cleanup is DEFERRED: the tree carries a modified `MEMORY.md` and an untracked sibling note under `.claude/agent-memory/orchestrator/`, and a removal would destroy both. Only the untracked file was visible on the first inspection; the `MEMORY.md` modification appeared on a re-read moments later, which is the practical case for re-deriving durable state rather than trusting a snapshot. Separately, four non-blocking findings on `UtilitiesCS/Threading/TimeOutTask.cs` are recorded on the item under `deferred_followups` and named in the pull request body; they could not be promoted from the item branch because acceptance criterion 12 asserts the branch diff contains only the two source paths and the feature folder, and an intake artifact under `docs/features/potential/` would have falsified an already-evidenced criterion. The consolidated issue is filed from a separate branch after merge.

Item 287 then shipped as pull request 716, merged at 2026-09-01T14:19:51Z as merge commit `06b1e02e`. Its child reported 47 of 47 plan tasks, the full C# toolchain green, MSTest 6912 of 6912 passing, repository coverage 85.297 percent line and 79.293 percent branch with no regression and 100 percent on new code, and 16 of 16 acceptance criteria checked. That child stopped without a completion report and was NOT restarted: durable state showed the pull request already open and the three review artifacts already committed carrying `Overall verdict: PASS` and zero Blocking findings, so only the CI wait remained, which is parent work. Its later report confirmed that diagnosis in full.

A change landed on `main` between the two merges that bears directly on this run's central constraint. Pull request 714 added `scripts/vscode/**` to `config/blast-radius.json`, the exact upstream correction identified above as the cause of roughly twenty-one of the ninety-one conflict edges. It takes effect on radii DERIVED AFTER it landed. It does not retroactively change this run: every recorded radius and every recorded edge here was derived before it, and re-deriving them to reduce the edge count is precisely what the `parallel-orchestrate` skill forbids. Recoloring after a membership change belongs to the mutation protocol and after a drift event to drift detection; neither applies to a configuration change. A future run planned against the corrected config should partition more favourably, and the residual `.claude/agent-memory/` edges will remain because those are real contention rather than a derivation artifact.

Item 633 launched into cohort index 6 from `main` at `06b1e02e`. Its branch needed correcting first: the checkpoint recorded head `064ed05b`, the remote was at `e1bd7235`, and the local ref still matched the checkpoint, because `main` had been merged into the item branch out of band between sessions. Ancestry was confirmed and the local ref fast-forwarded to the remote tip before launch.

Item 633 then shipped as pull request 717, merged at 2026-09-01T16:02:21Z as merge commit `8996b287`. Its child reported 47 of 47 plan tasks, MSTest 6912 of 6912 passing, and repository coverage of 85.297 percent line and 79.293 percent branch with 100 percent on new code.

Its Major non-blocking finding was verified rather than accepted on its label. NB-1 records that the `FilerQueue` worker loop clears `_consumerRunning` on exactly one path, so an exception escaping the loop leaves the flag permanently set and `WhenDrainedAsync()` never completes — a consequence the reviewer notes is worse after the change than before, converting a delayed or lost undo push into a session-long hang. The parent traced reachability independently: `FilerQueueItem`'s constructor rejects a null list and null elements but PERMITS an empty one, so `Helpers.First()` in the catch handler can indeed throw; however there is exactly one production caller, its list comes from `PackageItems()` which returns a single-element list unless conversation mode is active, the `First()` sits in the CATCH handler and so runs only after something else already threw, and the second route the reviewer named has no production caller at all. The trigger is therefore a compound condition the dominant path cannot reach, which supports the non-blocking classification with more specific evidence than the review itself states. It is the highest-priority entry of the consolidated follow-up issue rather than a merge blocker.

Item 646 launched into cohort index 7 from `main` at `8996b287`. Its branch needed the same correction as 633: the checkpoint and the local ref both sat at `9f578b3c` while the remote was at `3c4afd8c`, because `main` had again been merged into the item branch out of band. Ancestry was confirmed and the local ref fast-forwarded before launch. Two consecutive items have now shown this, so the remote-versus-local comparison is a standing pre-launch check rather than an incident response.

Item 646 then shipped as pull request 718, merged at 2026-09-01T17:09:50Z as merge commit `c7b4f08f`. Two of its child's claims were verified independently rather than accepted. Its history rewrite dropped roughly 52 MB of blobs and it asserted those blobs never reached `origin` because the push was a fast-forward; `git merge-base --is-ancestor 3c4afd8c 9b16bf67` confirms the pushed tip descends from the previously recorded remote tip, so no remote commit was overwritten and a force push would have been required had the rewritten commits already been pushed. And its pull request closes only its own issue: `closingIssuesReferences` on the live pull request contains exactly one entry.

That second check matters because the PR-context bundle has now been wrong on three consecutive items in the same coupled way. It reports the GitHub CLI unavailable while `gh` 2.87.3 answers every query, and it offers an auto-close list containing unrelated already-closed issues plus tokens that are not issues at all — `#ISO-8601` scraped from timestamp prose on item 633, `#CR-1` scraped from evidence prose on item 646. The pairing is what makes it dangerous: one defect invents issue numbers and the other disables the verification that would catch them, so a child that trusts the bundle either closes an unrelated issue or, following the skill's fallback for unavailable validation, closes nothing and leaves its own issue open. This is a collector defect rather than item work and belongs in its own issue.

Item 648 launched into cohort index 8 from `main` at `c7b4f08f`, after the same pre-launch fast-forward the previous two items needed. Three consecutive occurrences make the remote-versus-local comparison a standing check rather than an incident response.

Item 648 then shipped as pull request 719, merged at 2026-09-01T18:26:50Z as merge commit `5670b3cf`, with issue 648 confirmed CLOSED and COMPLETED.

Its F-1 finding forced an explicit merge-method decision, because roughly 21.2 MB of raw Cobertura blobs sit in that branch history: the squash method would drop them, the merge-commit method makes them permanent on `main`. The merge-commit method was kept, on three verified grounds. The decisive one is that `enforce-epic-merge-gate.ps1` line 377 returns an allow decision unless the command matches BOTH the merge subcommand AND the merge-commit flag, so a squash invocation falls out of the gate's scope and would run with no `ci_green` requirement and no `pr_number` match at all — choosing a merge method to avoid a side effect would silently discard the authorization the gate exists to enforce. Second, the accumulation is systemic rather than introduced here: `origin/main` already carries 218 tracked `.cobertura.xml` files, counted directly. The child reported 281, which is wrong. Third, all four sibling items used the merge-commit method, so keeping it preserves comparability. The child also declined to rewrite the history because the three audit artifacts cite those commit SHAs as their own evidence, and erasing a finding's evidence to remove the finding is the wrong trade.

The PR-context bundle failed for a fourth consecutive time and more severely. Its scraped auto-close list carried 18 entries: nine were not issue numbers at all, seven were closed and unrelated, and one — issue 584 — was OPEN and explicitly out of scope, so emitting the list would have closed live work. `closingIssuesReferences` on the merged pull request contains exactly one entry, issue 648.

Item 656 launched into cohort index 9 from `main` at `5670b3cf`, after the same pre-launch fast-forward the previous three items needed.

Item 656 shipped as pull request 720, merged at 2026-09-01T19:29:30Z as merge commit `43dcc800`, with issue 656 confirmed CLOSED and COMPLETED. Review returned zero blocking findings on the first pass and 20 of 20 acceptance criteria passing.

Two findings from that item are recorded because they generalize past it. The first is a plan defect: the item's footprint acceptance gates pinned a base SHA that predated the branch's reconciliation merge with `main`. Because a pinned SHA that is an ancestor of `HEAD` makes `merge-base(pinned, HEAD)` equal to the pinned SHA itself, the three-dot diff form silently degenerates to the two-dot form and reported 299 paths rather than the true 51. Three-dot syntax is therefore not by itself protection against a stale base, and three acceptance criteria would have failed over files the change never touched. The parent re-measured against `origin/main` and confirmed 51 paths of which exactly two are code. That correction has been added as a standing caution to every later item's kickoff.

The second is a reachability correction. The item's research had described the change as a production no-op while the reviewer asserted the residual was reachable on the shipped host; both were partly right. `BreadcrumbUiDispatcher.Dispatch` runs its action inline when the current boundary owns the call, so on the normal gesture path the close completes synchronously and the new guard behaves identically to the old one. Off that boundary the dispatch is asynchronous and the new branch is genuinely reachable with no substituted seam. Neither party established that a real gesture produces the off-boundary interleaving. The risk direction is one-way — the narrowed guard can only allow a close to proceed in a window where the host genuinely reports open, never suppress one that previously succeeded — and the finding is not merge-method-dependent.

A separate check found that eleven issues previously recorded as delivered but still open across six earlier bug families are now all closed, so that backlog is cleared.

Item 662 launched into cohort index 10 from `main` at `43dcc800`, after the same pre-launch fast-forward the previous four items required.

Item 662 shipped as pull request 721, merged at 2026-09-01T21:37:11Z as merge commit `9ca9e99a`, with issue 662 confirmed CLOSED and COMPLETED. Review returned zero blocking findings and 10 of 10 acceptance criteria passing.

The stale pinned base defect recorded against item 656 recurred here, which establishes it as a pattern rather than an isolated authoring error. This item's footprint gate pinned an ancestor SHA and reported 22 paths against an asserted union of four, before any edit existed. The child re-anchored to the merge base with `origin/main` and measured the true footprint as four code files. Two consecutive items have now carried the same unsatisfiable gate construction, so the caution issued to later children states it as expected rather than unusual.

The reviewer recorded an honest FAIL row on per-file coverage for `EfcFormController.cs` at 25.5 percent and dispositioned it non-blocking rather than reclassifying it as a pass. That handling is correct and the evidence remains legible: the file's diff is entirely comment lines, its coverage counters are unchanged from baseline, and it carries an Outlook interop dependency that places it inside the ratified exemption class.

The underlying defect is latent rather than live: no producer emits a row of the length that would trigger it, so nothing user-visible was reachable, and the change is a maintainability correction.

The PR-context bundle failed for a sixth consecutive item, this time scraping two issue numbers cited only as precedent in the requirements plus a token taken from evidence-convention prose. All were verified and discarded, and `closingIssuesReferences` holds exactly one entry.

Item 663 launched into cohort index 11 from `main` at `9ca9e99a`, after the same pre-launch fast-forward that all six preceding items required.

Item 663 shipped as pull request 722, merged at 2026-09-01T23:29:44Z as merge commit `988d35a8`, with issue 663 confirmed CLOSED and COMPLETED. Review returned zero blocking findings and 15 of 15 acceptance criteria passing, each re-executed by the reviewer rather than accepted from the executor's checkboxes.

This item established the mechanism behind a tooling defect that seven consecutive items had reported only by its symptoms, and the mechanism is materially worse than the symptoms suggested. The pull-request context tooling classifies changed C# source files as documentation and reports zero core-logic files. The reviewer loaded the hook directly and confirmed that this leaves the changed-language set empty, which causes C# coverage enforcement to be skipped without any report that it was skipped. Until now the defect was understood as producing a polluted issue-closing list that an operator could discard. It also disables a quality gate silently, on every C# item. Reachability is live in the review tooling; there is no product-runtime reachability. This is now the highest-priority repository-level finding on the run.

Two smaller observations were recorded rather than acted on. Evidence timestamps are not clock-derived — declared times run roughly forty minutes ahead of an independent clock, and two commits twenty-one minutes apart by declaration are seven and a half minutes apart by commit date — but the gates do not rest on them, because the red-before-green ordering is established by content and the test-count arithmetic holds across four runs. Separately, one acceptance criterion cited retained locals at lines 64 to 67 that now sit at 61 to 64, because the guard collapsed from four lines to one; the criterion matches on content, so nothing turned on the stale line numbers.

The underlying defect is latent rather than live, and the change strictly narrows the set of key chords the handler claims. One member of the delivered predicate for a previously shipped issue is currently deletable without failing any test, which this item's own suite demonstrates is worth pinning; that is recorded as a follow-up rather than fixed here.

Item 670 launched into cohort index 12 from `main` at `988d35a8`, after the same pre-launch fast-forward that all seven preceding items required.

Item 670 shipped as pull request 723, merged at 2026-09-02T01:10:37Z as merge commit `807fb0bb`, with issue 670 confirmed CLOSED and COMPLETED. Review returned zero blocking findings and 14 of 14 acceptance criteria passing. Coverage moved from 85.3866 percent to 85.3771 percent with covered lines increasing by five, and the new file measured 92.31 percent, clearing both the 80 and the 85 percent floors. That is a genuine pass, and the child correctly declined to manufacture a failing row where none was warranted.

This item established a SECOND and independently sufficient mechanism by which the pull-request context tooling disables C# coverage enforcement. The first, traced on item 663, is that changed C# sources are classified as documentation. The second is that the tooling enumerates only the ten changed paths with the highest churn, and this item's two coverage evidence documents run to roughly 194 thousand lines each, which displaces every source path from that list. The coverage validation script parses only the churn-annotated lines, so in both cases it receives an empty language set and returns a silent pass. The consequence deserves stating plainly: the more thorough an item's committed evidence, the more certainly the gate that would judge it stops running. Reachability is live in the review tooling on every C# item, with no product-runtime reachability.

The stale pinned base defect recurred for a third consecutive item. Anchored at the pinned ancestor, the footprint gate reported 17 contaminating paths from sibling deliveries before any work existed, and one project-file clause was unsatisfiable outright. The child re-anchored to the true merge base and left the plan text unmodified.

The child also recorded a claim about a disposal exception that its reviewer contradicted. It verified rather than defended, found the reviewer correct and its own search scoped to the wrong file, and retracted the claim in its checkpoint. The requirement documents were accurate throughout and no code changed as a result.

Item 678, the final item of the run, launched into cohort index 13 from `main` at `807fb0bb`, after the same pre-launch fast-forward that all eight preceding items required.

Item 678 shipped as pull request 724, merged at 2026-09-02T06:17:18Z as merge commit `01820205`, with issue 678 confirmed CLOSED and COMPLETED. Coverage was read from the post-processed Cobertura document rather than from any summary tool — 85.3967 percent line and 79.4522 percent branch, 55086 covered of 64506 valid, with all 34 changed lines covered — which is the correct response to a coverage gate that cannot be relied on to run.

The context bundle named issue 427 in its auto-close list, and 427 is OPEN. It is the parent report for the duplicate-scoring behaviour, of which this item delivers only the scoped consumer-side portion, so emitting that entry would have retired live work. The child verified each scraped number individually and emitted a closing keyword for 678 alone; 427 was confirmed still OPEN after the merge. This is the second occurrence of the bundle naming an OPEN out-of-scope issue, after item 648 named 584. Two instances across nine items establish that the pollution is not confined to already-closed numbers, and that per-number verification is load-bearing rather than precautionary.

**Every item of this run is now merged.** The run is `open` mode, so it does not auto-complete: it remains a standing queue and terminates only through `/parallel-close`. No completion is asserted here.

Before the 285 launch the item branch had to be freed. The planner's preparation worktree at `.claude/worktrees/agent-a21c202f574d31539` still held `bug/timeouttask-runwithtimeout-exception-type-mismatch-285` checked out, and git refuses a second checkout of the same branch. It was freed with `git checkout --detach` rather than `git worktree remove`: a preparation worktree appears in no `items[]` record, so both worktree-removal gates fail closed on it. Idleness was established before detaching — the tree was clean and the worktree index mtime was unchanged across two samples and roughly 6.6 hours stale — rather than inferred from the lock, whose named process is this session's own long-finished preparation subagent.

Two of the four admissions overlapped in time with another mutation and had to be recomputed because of it. `/parallel-add 656` re-read the checkpoint after its preparation child returned and found that 646 had landed meanwhile. `/parallel-add 285` re-read and found that 656 had landed meanwhile — a sixth item, five new conflict edges, and `recolor_generation` advanced from 1 to 2. Deciding either against the state read before its preparation began would have admitted it alongside a conflicting item.

Item 647 was launched only after its committed plan blob was verified against the run kickoff Integrity table (`cccd1a0435a2e0d0b791645a426f5a0a7cb1369a`) and its branch tip was confirmed identical locally and on `origin` at `e2a94c08`. Its per-edge barrier was satisfied because all three conflicting neighbours were terminal or merged. Because its branch was cut from an older `main`, the child reconciled it against `origin/main` `9b6aff2e` before running any plan task; that merge was clean at `0cb9e6a2`.

### Item 646 — admission

Item 646 was admitted by `/parallel-add 646` at 2026-09-01T00:44:13Z as an `ADMIT_CURRENT_COHORT` decision with **no recolor**, so `recolor_generation` is stamped unchanged at 1.

**A delivery pre-check ran before any preparation was delegated**, and confirmed the defect is genuinely outstanding rather than already shipped under a sibling issue. The guard site on `origin/main` at `QuickFiler/Controllers/QfcHomeController.Metrics.cs` builds the filtered array `lines` and then awaits `MetricsFileWriter` unconditionally, with no length guard between the two; the sibling EFC path at `QuickFiler/Controllers/EfcHomeController.Metrics.cs` does carry the corresponding `dataLines.Length == 0` early return. A bare-number search of `origin/main` returned only documentation commits and no delivering fix, and GitHub issue 646 is `OPEN`.

**The admission decision turned on state that changed during preparation.** When the add began, item 647 was `in_flight` with pull request 712 open and two checks still running, and 647's declared radius already named both files item 646 must edit. On that state the decision would have been `DEFER_AND_RECOLOR`. Pull request 712 merged at 2026-09-01T00:05:33Z while the preparation child was still running, which emptied the pinned set and left no non-terminal member in any current-generation cohort. The decision was therefore recomputed against re-derived durable state rather than against the earlier reading, and resolved to a no-conflict admission requiring no recompute.

**Item 646 takes a new cohort index rather than joining index 3.** It conflicts with item 647, which occupies current-generation index 3, and a cohort must remain an independent set in the conflict graph, so the admitted item takes index 4 and `current_cohort` advances to 4.

**No in-flight item's cohort or state changed**, which is trivially satisfied here because the pinned set was empty at decision time: every pre-existing item was already terminal. No existing `items[]` record, no existing `cohorts[]` entry, and no existing `conflict_edges[]` entry was modified by the admission.

**The declared radius was widened beyond the library derivation, in the fail-closed direction.** `Get-BlastRadius` returned 32 paths from the preflight-cleared plan and `issue.md`. Comparing that set against the item branch's actual three-dot diff against `origin/main` showed two files the plan text could not have revealed: `.claude/agent-memory/orchestrator/MEMORY.md` and a new sibling memory file, both from a memory commit the preparation child made on the item branch. They will land on `main` through this item's pull request, so they are declared. They are recorded as two exact paths rather than a `.claude/agent-memory/**` subtree glob deliberately: a glob over that subtree would contend with essentially every future item, whereas the exact `MEMORY.md` path contends only with another item editing that same shared index, which is a real contention and correct to report.

**Preparation cleared preflight in two rounds**, meeting the two-round target. Round one caught a blocker that would have made two gates unsatisfiable: two tasks passed the solution-level `"/p:Platform=Any CPU"` to a project-level build, and `QuickFiler.Test.csproj` keys on the literal `Debug|AnyCPU`, so the space-bearing value matches no `PropertyGroup` and the build fails outright. The plan also required a scaffolded `## Acceptance Criteria` section to be authored into `issue.md`, without which the minor-audit integrity check fails.

### Item 656 — admission

Item 656 was admitted by `/parallel-add 656` at 2026-09-01T01:27:03Z as a `DEFER_AND_RECOLOR` decision, so `recolor_generation` increments by exactly one, from 1 to 2.

**A delivery pre-check ran before any preparation was delegated.** GitHub issue 656 is `OPEN`, and a bare-number search of `origin/main` returned one substantive commit, `2434f07f fix(breadcrumb): enforce close, lifetime, broadcast and lease invariants`. That commit does not deliver the item; its body **promotes** it, recording 655 and 656 as real issues rather than prose. The guard sites confirm the residual is outstanding: in `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` on `main`, `_closeCompleted` is set `true` only on the successful-close path in `CloseCore` (`:335`), is read as an early-return suppressor (`:316`), and is cleared in exactly two places, `RequestOpen` (`:114`) and `Invalidate` (`:352`). Residual scope was settled from the delivering feature's own `spec.md`, which records SR-4 as a deliberate known limitation shipped as designed and assigns closure to feature 488's host paths.

**The admission decision turned on a concurrent mutation.** When this add began, every item in the run was terminal and the pinned set was empty, which would have produced a no-conflict admission with no recompute. While the preparation child ran, `/parallel-add 646` admitted item 646 into cohort index 4 in state `scheduled`. Item 656 conflicts with 646, and 646 is an unstarted member of the current cohort, so the deferred branch applies: `max_concurrency` is 2, and the next slot-filling batch would otherwise have launched both from cohort 4 concurrently. The decision was recomputed against the re-read checkpoint rather than against the state observed before preparation.

**The recolor ran over the unstarted subgraph only.** Its input was `{646, 656}` with the single edge `646~656`, passed to the canonical Welsh-Powell entry point `bash .claude/lib/bash/compute-cohorts.sh --keys "646 656" --edges "646:656"`, which returned `[[646],[656]]`. The pinned set was empty, so no conflict edge joins an unstarted item to a pinned item and the pinned-barrier offset is not applied; the lowest returned index therefore equals `current_cohort` exactly. The absolute indices were written verbatim: 646 stays at index 4 and 656 takes index 5. `current_cohort` remains 4, the lowest current-generation index still holding a non-terminal item.

**No in-flight item's cohort or state changed.** This is trivially satisfied because the pinned set was empty at decision time. The four pre-existing items are all `merged` and therefore exempt from current-generation cohort membership. Item 646 was a legitimate subject of the recolor because it is unstarted, and it retained index 4.

**The declared radius was widened beyond the library derivation, in the fail-closed direction.** `Get-BlastRadius` returned 94 paths from the preflight-cleared plan and `spec.md`. Comparing that set against the item branch's actual three-dot diff against `origin/main` showed six files the plan text could not have revealed: `MEMORY.md` plus one sibling note in each of the `atomic-executor`, `atomic-planner`, and `task-researcher` agent-memory trees, from memory commits three preparation-chain children made on the item branch. They will land on `main` through this item's pull request, so they are declared. They are recorded as six exact paths rather than a `.claude/agent-memory/**` subtree glob, on the same reasoning recorded for item 646. The widening was re-tested against every item before it was written: it added no shared surface and changed no conflict edge and no reason kind.

**Preparation cleared preflight in two rounds**, meeting the two-round target. Round one returned ten enumerated defects, four blocking, in one exhaustive pass. Three were substantive rather than cosmetic: the plan read `lines-covered` and `lines-valid` from a Cobertura `class` node, where neither attribute exists, which made the coverage acceptance unsatisfiable; no task created the `TestResults` directories the msbuild file logger writes into, and neither creates intermediate directories; and a standing-guard test run omitted the `LiveOutlook` exclusion.

**The remedy reconciles SR-4 rather than overriding it.** SR-4 rejected the refinement `if (_closeCompleted && !_host.IsOpen) return true;` because it would read `_host.IsOpen` under the `_sync` lock. `RequestOpen` already performs that read under `_sync` at `:112`, so the rejection's ground was the addition of a new instance, not the absence of any. Hoisting the read above the lock satisfies that constraint literally, leaves all five standing guards unedited, and holds the production footprint at one file — which matters because `BreadcrumbDropDownHost.cs` (498 lines) and `BreadcrumbItemViewerLifecycleCoordinator.cs` (497) both sit against the 500-line cap.

**Research corrected the issue's own attribution, and the item is hardening rather than a live failure.** No reopen path bypassing both clearing entry points exists today. The only production statement that opens the drop-down host is `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:268`, and the chain reaching it is closed through `RequestOpen`, which clears `_closeCompleted` immediately beforehand. The issue's claim that the bypassing paths live in the ItemViewer breadcrumb lifecycle host surface is not confirmed, and `spec.md` records the correction. This matches the issue's own Medium and latent rating; it is a latent correctness gap, not an observed user-facing failure.

### Item 285 — admission

Item 285 was admitted by `/parallel-add 285` at 2026-09-01T05:23:49Z as a `DEFER_AND_RECOLOR` decision, so `recolor_generation` increments by exactly one, from 2 to 3.

**A delivery pre-check ran before any preparation was delegated, and this time the OPEN issue state was corroborated rather than misleading.** Six prior families on this repository shipped work while leaving their issues `OPEN`, so an open issue is treated as weak evidence. Here the evidence agrees: `git log origin/main --grep="fix(285)"` returned nothing, the bare-number search returned only the promotion commit `a93a5d64` and its merge `9e039276`, and the guard site still carries the defect. In `UtilitiesCS/Threading/TimeOutTask.cs` on `main`, the private `RunWithTimeout<T1, TResult>` overload taking `this Func<T1, TResult>` (declared at line 177) awaits `Task.Run(() => function(arg1), combinedToken.Token)` against a linked `CancellationTokenSource(milliseconds)` timer, while its only specific handler is `catch (TimeoutException)` at line 200. A timer-driven cancellation raises `TaskCanceledException`, which is an unrelated type, so the `maxAttempts` retry ladder can never run for a genuine timeout.

**The counts the acceptance criteria assert were verified exhaustively, not from a truncated grep.** Over the complete 993-line file on `main` there are 9 `catch (TaskCanceledException)` clauses (lines 65, 130, 268, 351, 429, 498, 581, 663, 744), 4 `catch (TimeoutException)` (200, 272, 818, 914), 10 `catch (System.Exception e)`, zero exception filters, and zero occurrences of `OperationCanceledException`. Those reconcile exactly with the specification's post-fix expectations of 9, 3, 10 and 1, with the three surviving `TimeoutException` clauses being precisely the ones the specification names.

**One correction was carried into the specification rather than restated from the issue.** The issue asserts the exception "propagates unhandled to the caller". It does not: the general `catch (System.Exception e)` at line 220 catches it, logs it, and rethrows only when `strict` is true. The user-visible defect is that the retry never happens and a non-strict caller silently receives `default!`.

**Preparation was terminated by a session rate limit and was resumed rather than restarted.** The first child committed and pushed promotion, research and `spec.md` to the item branch at `8b73479f` before dying. Diagnosing from the branch rather than the checkpoint showed the gap was exactly the atomic plan and preflight clearance, so the resumed delegation was scoped to those two steps alone. Restarting would have rebuilt three artifacts and produced a second plan file in violation of the plan-path continuity contract. The stale worktree was detached to free the branch rather than removed, because both worktree-removal gates fail closed on a path carrying no `items[]` record.

**Preparation cleared preflight in two rounds**, meeting the two-round target, with round one returning ten findings. One round-one finding was **rejected on evidence** rather than applied: it demanded a `.dotnet-sdk/` carve-out in three terminal footprint gates on the premise that `.gitignore` did not match that directory, but `.gitignore` line 350 (`.dotnet*/`) does match, and `git check-ignore -v` confirms it. Applying the finding would have weakened three gates. The planner also caught that one of the nine accepted findings would have silently weakened AC12, and repaired it.

**The admission decision turned on a concurrent mutation.** When this add began, the run held five items, `recolor_generation` was 1, and item 646 was the only unstarted item. While the preparation child ran, `/parallel-add 656` admitted a sixth item and advanced the generation to 2. The decision was recomputed against the re-read checkpoint. Item 285 conflicts with 646, an unstarted `scheduled` member of the current cohort at index 4, on the exact shared path `.claude/agent-memory/orchestrator/MEMORY.md` and on module `QuickFiler.Test`, so the deferred branch applies.

**The recolor ran over the unstarted subgraph only.** Its input was `{285, 646, 656}` with the three edges `285~646`, `285~656` and `646~656` — a triangle — passed to `bash .claude/lib/bash/compute-cohorts.sh --keys "285 646 656" --edges "285:646 285:656 646:656"`, which returned the three singleton classes `[[285],[646],[656]]`. The pinned set was empty, so no conflict edge joins an unstarted item to a pinned item, the pinned-barrier offset is not applied, and the lowest returned index equals `current_cohort` exactly. The absolute indices were written verbatim: 285 to index 4, 646 to index 5, 656 to index 6.

**Items 646 and 656 changed cohort index, and that is correct.** Both are UNSTARTED and are therefore vertices of the recolored subgraph; the pinning invariant protects `in_flight` items only. The newly admitted item landing ahead of both is the deterministic output of the coloring, not a scheduling preference, and it is applied verbatim rather than re-based. The safety property is unaffected: 285, 646 and 656 pairwise conflict and occupy three distinct cohorts, so the per-edge barrier prevents any two from running concurrently.

**No in-flight item's cohort or state changed.** This is trivially satisfied because the pinned set was empty at decision time, re-verified through `git worktree list --porcelain`, `git branch` and `gh pr view` rather than read from the checkpoint. The four merged items are exempt from current-generation cohort membership and were not touched.

**The declared radius was widened beyond the library derivation, in the fail-closed direction.** `Get-BlastRadius` returned 68 paths from the preflight-cleared plan and `spec.md`. The item branch's three-dot diff against `origin/main` showed three further files: `.claude/agent-memory/orchestrator/MEMORY.md` and two sibling notes, from a memory commit the resumed preparation child made at `21a47aac`. They are invisible to the plan-text derivation because `.claude/agent-memory/**` is a `mandate_reads` exclusion, but they will land on `main` through this item's pull request, so they are declared as three exact paths rather than a subtree glob, on the same reasoning recorded for items 646 and 656. That widening is what produces the `285~646` edge, and the contention is real: both items edit the same shared memory index.

**The plan file is `plan.2026-09-01T00-30.md` rather than `plan.md`, for a reason worth recording.** The hook `enforce-feature-folder-order.ps1` unconditionally requires `user-story.md` to exist before any `plan.md` write in an active feature folder, and never reads the work-mode marker. For a `full-bug` item, `user-story.md` must be absent, and its presence is itself an integrity failure. The hook therefore blocks the canonical filename for every `full-bug` item. The timestamped form is the repository's prevailing convention (49 of roughly 50 active plans use it) and exactly one plan file exists for this cycle. The hook defect belongs upstream.

### Item 633 — admission

Item 633 was admitted by `/parallel-add 633` at 2026-09-01T07:19:32Z as a `DEFER_AND_RECOLOR` decision, advancing `recolor_generation` from 3 to 4.

**A delivery pre-check ran before any preparation was delegated.** GitHub issue 633 is `OPEN`, no `fix(633)` commit exists on `origin/main`, and a bare-number search returned only four unrelated commits. The guard sites were then read directly out of `origin/main` rather than inferred from commit subjects: `QuickFiler/Controllers/QfcItemController.MailActions.cs` still calls `_homeController.FilerQueue.Enqueue(filer, helpers)` followed by `await Task.CompletedTask`, so the method only enqueues, and `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` still runs `BackGroundMoveAsync` through `MoveEmailsAsync`, then `WriteMetrics`, then `CleanupBackground()` with nothing observing the filer queue between them. The residual-scope question was settled from the delivering family's own documents rather than from a second source read: `docs/features/active/qfc-collection-controller-defects-468/spec.md:1040` records this defect as a deferred observation explicitly out of scope for all seven of that feature's issues, and that feature's follow-up promotion record maps row 7 to issue 633. The defect was excluded from the 468 delivery rather than shipped under it.

**The preparation child died on a session rate limit after clearing preflight but before committing.** It was resumed rather than restarted, per the standard dead-child recovery. Diagnosis came from the branch and the worktree, not from the checkpoint: the feature folder held `issue.md`, `spec.md` with 25 acceptance criteria, the research artifact and a nine-phase 1,642-line plan, all uncommitted, with the item branch still at the `origin/main` tip. The plan was independently confirmed through the plan validator before anything was accepted. Only the terminal commit-and-push step was missing, and only that step was performed; no plan or specification content was authored by the parent.

**Nothing was written to the checkpoint until the decision existed.** The candidate never entered `items[]` in state `proposed`, because that state cannot be represented validly and because holding a write open across a long preparation step is a wide window for a lost update against the concurrent adds also running on this run. A baseline checkpoint validation was taken before the operation began, so any later failure would be attributable to this operation rather than pre-existing.

**The candidate conflicts with all seven pre-existing items**, each verdict corroborated by an exact set intersection of the two `paths` lists rather than read from the library's reported pair. It conflicts with item 285, the sole member of the current cohort at index 4, so the deferred branch applied.

**The pinned set was empty at decision time.** Items 638, 644, 647 and 637 are terminal, and 285, 646 and 656 were all still `scheduled` — re-verified through `git worktree list --porcelain`, `git branch` and `gh pr list` as having no execution worktree, no pull request and `merge_status` `not_started`. No conflict edge therefore joined an unstarted item to a pinned item, the pinned-barrier offset was not applied, and the lowest returned index equals `current_cohort` exactly.

**The unstarted subgraph is a complete graph on four vertices**, so the canonical Welsh-Powell coloring returned four singleton classes in the order 285, 633, 646, 656, and the absolute indices are 4, 5, 6 and 7, written verbatim. Items 646 and 656 each moved down one index. That is permitted because both are unstarted rather than pinned, and no in-flight item's cohort or state changed — trivially, because there was no in-flight item.

**The declared radius needed no widening, and the reason is worth recording because it differs from every sibling admission.** `Get-BlastRadius` returned 99 paths, and the item branch's three-dot diff against `origin/main` carries exactly the four feature-folder files, all covered by the folder glob, so the reconciliation against the branch diff found no escape. Items 646 and 656 both had to be widened for `.claude/agent-memory` files their preparation children committed. This item's child died before making that commit, and the parent could not make it on the child's behalf: the pre-implementation gate's operand exemption covers only `docs/features` and `artifacts/orchestration` paths. Those memory notes remain uncommitted in the preparation worktree.

**Two derived entries are recorded rather than silently accepted.** The derivation returned `QuickFiler/**/*.cs`, a module-wide glob that will contend with every future QuickFiler item, and it returned unexpanded `FEATURE/` and `TIMESTAMP` plan-template placeholders. Both were left verbatim, because narrowing a declared radius is prohibited. Neither changes scheduling here, since the conflict graph over this run is already complete.

### Item 670 — admission

Item 670 was admitted by `/parallel-add 670` at 2026-09-01T08:18:56Z as a `DEFER_AND_RECOLOR` decision, advancing `recolor_generation` from 4 to 5.

**A delivery pre-check ran before any preparation was delegated, and it resolved in the opposite direction from the six prior families.** Those families shipped work while leaving their issues `OPEN`, so an `OPEN` state is treated as weak evidence. Here every check agrees the work is outstanding. The three discarding call sites are unchanged on `origin/main` at `QuickFiler/Controllers/QfcItemController.Initialization.cs` lines 192, 288 and 324, with line 256 the sole awaited call; line 345 carries a commented-out occurrence that is not a call site. The bare-number search returned `d9ed9eb2`, which is the *promotion* commit rather than a fix, and its body states the position outright: the faulted task "is confirmed unobserved at three of its four production call sites, and the D5 guard is delivered unweakened".

**This is the promoted-follow-up case, and it inverts the usual residual-scope rule.** Issue 488 carved 670 out as its own issue to discharge its research §3.5 criterion, finishing at 145 of 145 plan tasks and 54 of 54 acceptance criteria. A follow-up that a delivering commit promoted to its own issue is *not* residual scope for the parent — the promotion is what discharged it — so the parent is complete and the promoted item is admitted on its own merits.

**Preparation was terminated by a session rate limit and was resumed rather than restarted.** The first child had already committed and pushed four commits to the item branch: the feature folder and research, `spec.md` with 14 acceptance criteria, the atomic plan with a passing plan validator, and a revision closing 11 preflight defects. Diagnosis came from the branch rather than the checkpoint, and showed the gap was exactly one thing — the confirming preflight round. That was verified rather than assumed: every `PREFLIGHT: ALL CLEAR` then present in the repository belonged to an unrelated feature folder, none to 670. The resumed delegation was scoped to that round alone. The stale worktree was detached to free the branch rather than removed, because both worktree-removal gates fail closed on a path carrying no `items[]` record.

**Preflight took five rounds, which misses the two-round target, and the cause is recorded rather than smoothed over.** 27 defects closed across the rounds: 11, 9, 4, 3, 0. Rounds 3 through 5 were almost entirely sibling invalidation — each round's own fix invalidated an assumption in a neighbouring task that the preceding review had no reason to examine. Round 2's replacement of a two-outcome gate taxonomy left a later task gating against a set that no longer existed, and its two new path-sweep tasks left an earlier task deriving too few substitution tokens. Of round 2's nine defects, 5 were round-1 misses, 3 were introduced by the round-1 revision, and 1 became reachable only because of it. Round 4 was the first to bundle the consequential sibling fix into its own delta, and round 5 then cleared with zero defects. Four blocking defects were genuine unsatisfiable-gate conditions, including an acceptance condition asserting a case-insensitive `Select-String` returns zero matches for `throw` while the guard body that same task dictates contains `Token.ThrowIfCancellationRequested()`.

**The admission decision turned on a concurrent mutation, for the third time in this run.** When this add began the run held seven items at generation 3. While preparation ran, `/parallel-add 633` admitted an eighth item and advanced the generation to 4. The decision was recomputed against a freshly re-read checkpoint rather than the pre-preparation reading, under explicit guards that aborted the write if `recolor_generation`, `current_cohort`, the `in_flight` set, or the unstarted set had moved again. Item 670 conflicts with item 285, the sole member of the current cohort at index 4, so the deferred branch applied.

**The recolor preserved every existing assignment.** The unstarted subgraph `{285, 633, 646, 656, 670}` is complete on five vertices, so `bash .claude/lib/bash/compute-cohorts.sh` returned five singleton classes in ascending key order. The pinned set was empty — re-verified through `git worktree list --porcelain`, `git branch` and `gh pr view` — so no conflict edge joins an unstarted item to a pinned item, the pinned-barrier offset was not applied, and the lowest returned index equals `current_cohort` exactly. Indices 4, 5, 6, 7 and 8 were written verbatim. Unlike the 285 and 633 admissions, no pre-existing item changed index: 285, 633, 646 and 656 keep 4, 5, 6 and 7, and the new item appends at 8. A control run of the coloring without the candidate confirmed that before the write.

**No in-flight item's cohort or state changed**, trivially so because the pinned set was empty at decision time, and confirmed afterwards by re-reading all four pre-existing unstarted records and finding their `state` and `merge_status` unchanged.

**The declared radius was widened beyond the library derivation, in the fail-closed direction.** `Get-BlastRadius` returned 103 paths from the preflight-cleared plan and `spec.md`. The item branch's three-dot diff against `origin/main` carried 10 further tracked files, all under `.claude/agent-memory/`, spanning the `atomic-executor`, `atomic-planner` and `orchestrator` trees, from memory commits three preparation-chain children made. They are invisible to a plan-text derivation and will land on `main` through this item's pull request, so they are declared, as 10 exact paths rather than a subtree glob, on the same reasoning recorded for items 646, 656 and 285. The final radius is 113 paths. The escape scales with the delegation chain rather than with the item: three participating agent trees produced roughly one `MEMORY.md` plus siblings each.

**The re-test after widening earned its place.** It changed no conflict verdict, but it did change reason kinds: `path_overlap` was newly reported against items 285 and 647, which had previously contended by module alone. A widening that silently gains contention is worth knowing about before it reaches the checkpoint rather than after.

**One defect in this operation's own tooling was caught and corrected before it was reported as fact.** The edge `detail` strings are corroborated by an exact set intersection of the two `paths` lists rather than read from the library's reported pair. For the `285 ~ 670` edge that intersection holds exactly one element, and a single-element PowerShell pipeline result collapses to a scalar string, so indexing it returned the string's first *character* and the detail was written as `. ~ .`. That looked like a bare `.` path token in both radii — which would have been a serious derivation artifact capable of matching every path — so it was investigated rather than accepted. No item carries such a token; the intersection is `.claude/agent-memory/orchestrator/MEMORY.md`, and the two affected fields were corrected. The checkpoint was re-validated afterwards.

### Item 678 — admission

Item 678 was admitted by `/parallel-add 678` at 2026-09-01T08:42:00Z as a `DEFER_AND_RECOLOR` decision, advancing `recolor_generation` from 5 to 6.

**A delivery pre-check ran before any preparation was delegated, and both guard sites were read out of `origin/main` rather than inferred.** GitHub issue 678 is `OPEN`, no `fix(678)` commit exists, and the bare-number search returned only the promotion commit `b8668ea0` and one unrelated digit collision. The producer site `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` constructs a `FolderPredictor`, awaits `InitAsync(helper, FromField)`, reads `(score, topFolder)` off it and then discards the initialised object; `QfcPreScoredItem` carries only the mail item and the folder string, so the predictor never leaves the pre-filter. The consumer site `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` unconditionally rebuilds a predictor through `_folderPredictorFactory` and awaits `fp.InitAsync(ItemHelper, FromField)` a second time. The producer half of issue 427 landed under the 446 family; this consumer half did not.

**Research corrected two premises the parent's own delegation had asserted.** The parent's first delegation named `QfcHighConfidencePreFilter.FilterAsync` as the live producer. It is not: that path is dormant, and the live producer is the streaming dequeue confidence gate. The research also established that there are two distinct re-scoring legs — the first page and every subsequent page — so the carrier must be threaded through both, and that the `Times.Never` assertions in `QfcHomeControllerRunAsyncHighConfidenceTests.cs` are disabled-mode assertions to be preserved rather than rewritten. The resumed planning delegation was instructed to plan against the research reading wherever it disagrees with the issue body. The scope is materially larger than the issue's `Low` severity implies: 23 acceptance criteria across carrier, producer, both consumer legs, the single-initialisation invariant and preserved behaviour.

**Preparation was terminated by a session rate limit and was resumed rather than restarted — at a death point not previously seen on this run.** The item branch existed with an empty `git log origin/main..HEAD` and a dirty worktree, which matches the documented "died before committing" case. What it did not match was the file listing: the worktree held `issue.md`, an 837-line research artifact **and** `plan.2026-08-31T21-12.md`, which reads as a completed preparation. Opening the plan settled it — 44 lines of the untouched scaffold that `new_active_feature_folder` emits, with placeholder tasks such as `[P0-T1] Link approved spec: <spec link>` and zero `### Phase` headings. Plan authoring had never run. Scoping the resume from the file list alone would have sent a scaffold to preflight. The parent committed the survivors as `2ed1a8c7`, including the scaffold — deliberately, because it is the canonical plan path and committing it is what makes the resumed planner revise in place rather than author a timestamped sibling — then detached the stale worktree to free the branch, because both worktree-removal gates fail closed on a path carrying no `items[]` record. The resumed delegation was scoped to plan authoring and preflight alone.

**Preflight took four rounds, which misses the two-round target, and the cause is recorded rather than smoothed over.** 28 defects closed across the rounds: 19, 7, 2, 0. Rounds 1 and 2 were exhaustive rather than first-defect stops. The overrun came from scope: the authoring pass and round 1 both scoped the citation set to the files the plan *names*, rather than to the wider set its mandated edits force the compiler to touch. Three of round 2's seven defects became visible only after tracing outward, and the sweep then mandated found seven more of the same classes, including two files no token grep could surface because their `CreateGate` lambdas are untyped at the call site. The highest-value catch was of that kind: `QfcFormControllerTests.cs` is 827 lines and was absent from the file-size census entirely, which left one task's at-or-below-baseline comparison with no operand — a gate that could not pass however the executor behaved. Round 2's own fix then created round 3's defect, by scoping a loop-termination carve-out to "this restart rule", which does not reach the task that records the loop's result.

**The declared radius needed no widening, and the reason differs from the sibling admissions.** `Get-BlastRadius` returned 116 paths, and the item branch's three-dot diff against `origin/main` is exactly three files, all inside the feature folder and all already covered by the derived set. Items 646, 656, 285 and 670 each had to be widened for `.claude/agent-memory` files their preparation children committed. This item's child wrote no agent memory at all — it reported that `.claude/agent-memory/` is tracked, so writing there would either breach its commit constraint or leave the worktree dirty for the execution child — so the escape route that required manual path additions on four siblings was never opened. Two derived entries are recorded rather than silently accepted: `.config/dotnet-tools.json`, a repository-root shared surface, and `QuickFiler.Test/QuickFiler.Test.csproj`, which contends with nearly every QuickFiler item. Neither was narrowed, because narrowing a declared radius is prohibited.

**The candidate conflicts with all nine pre-existing items**, each verdict corroborated by an exact set intersection of the two `paths` lists rather than read from the library's reported pair. It conflicts with item 285, the sole member of the current cohort at index 4, so the deferred branch applied.

**The admission decision turned on a concurrent mutation, for the fourth time in this run.** When this add began the run held eight items at generation 4. While preparation ran, `/parallel-add 670` admitted a ninth item and advanced the generation to 5. The decision was recomputed against a freshly re-read checkpoint rather than the pre-preparation reading, under explicit guards that aborted the write if `recolor_generation`, the `in_flight` set, the unstarted set, or the presence of any drift event had moved between the decision and the write.

**The recolor preserved every existing assignment.** The unstarted subgraph `{285, 633, 646, 656, 670, 678}` is complete on six vertices — all fifteen pairs conflict — so `bash .claude/lib/bash/compute-cohorts.sh` returned six singleton classes in ascending key order. The pinned set was empty, re-verified through `git worktree list --porcelain`, `git branch` and `gh pr view`, so no conflict edge joins an unstarted item to a pinned item, the pinned-barrier offset was not applied, and the lowest returned index equals `current_cohort` exactly. Indices 4 through 9 were written verbatim. As at generation 5, no pre-existing item changed index: 285, 633, 646, 656 and 670 keep 4, 5, 6, 7 and 8, and the new item appends at 9.

**No in-flight item's cohort or state changed**, trivially so because the pinned set was empty at decision time, and confirmed afterwards by re-reading every pre-existing record and finding `state` and `merge_status` unchanged.

### Item 287 — admission

Item 287 was admitted by `/parallel-add 287` at 2026-09-01T10:00:00Z as a `DEFER_AND_RECOLOR` decision, so `recolor_generation` increments by exactly one, from 6 to 7.

**A delivery pre-check ran before any preparation was resumed.** GitHub issue 287 is `OPEN`, a `fix(287)` search of `origin/main` returned nothing, and a bare-number search returned only the promotion commit `a93a5d64` that created issues 285 through 287, plus unrelated digit collisions. The guard site settles it directly: `StoreWrapperController.Launch` on `origin/main` still carries a single `if (readiness.State != StoreLaunchReadinessState.Ready)` branch that shows one message — "Store settings are not available yet. Please try again after startup completes." — for every non-`Ready` state. `StoreLaunchReadinessState` declares three members (`Ready`, `ModelUnavailable`, `StoresUnavailable`), so the remedy of distinguishing a transient case from a genuine failure is non-vacuous rather than a copy edit with no discriminating input.

**Preparation was resumed, not restarted.** A prior `/parallel-add 287` had died after applying its second round of preflight deltas. Its work was durably committed and pushed: eight commits carrying `issue.md` with the `- Work Mode: full-bug` marker, `spec.md` with twenty acceptance criteria, a research artifact, and an 81 KB six-phase plan that passes the plan validator. Only the confirming preflight round was missing, which was proved by grepping the item's own feature folder for `PREFLIGHT: ALL CLEAR` and finding nothing rather than inferred from the last commit subject. Death was established from durable state — an index mtime unchanged across two samples and a clean worktree — not from a notification. The stale preparation worktree was freed by detaching its `HEAD`, never by `git worktree remove`, which both removal gates refuse for a path in no `items[]` record. The resumed child was scoped to preflight alone and revised the existing plan in place, so no timestamped sibling plan was created.

**Preflight cleared in two rounds**, and the resumed child recorded the clearance as a committed evidence artifact at `evidence/other/preflight-clearance.2026-09-01T05-51.md` rather than leaving it in an untracked checkpoint, which is why the previous attempt's clearance had evaporated. That prior clearance had also been unsound: it was pinned to a plan blob byte-identical to the one the resumed child started from, and a fresh transitive review of those exact bytes found two blocking defects. Both rounds' defects were genuine pre-existing misses rather than self-inflicted by a preceding round's fix; the common cause was a citation set scoped to the files the plan names rather than to the transitive set its edits force.

**The admission decision was recomputed after preparation returned, not carried forward.** The pinned set was empty at decision time and `gh` reports no pull request on any of the seven item branches, so `highest_pinned_cohort` was undefined and the pinned-barrier offset reduced to the `current_cohort` floor: the lowest returned index equals `current_cohort` = 4. The candidate conflicts with every one of the ten existing items, including 285, the sole member of the current cohort at index 4, so the no-conflict admit branch did not apply.

**The recolor placed the new item ahead of five incumbents, which is correct rather than a defect.** The induced unstarted subgraph over `{285, 287, 633, 646, 656, 670, 678}` is complete — all twenty-one pairs conflict — so `bash .claude/lib/bash/compute-cohorts.sh` returned seven singleton colour classes in ascending key order. Because 287 sorts below the five later incumbents, the offset places it at index 5 and shifts 633, 646, 656, 670 and 678 each up by one, to 6, 7, 8, 9 and 10; 285 keeps index 4. Those five moves are legitimate: the pinning invariant protects `in_flight` items only, and every moved item is unstarted. Re-basing the indices to keep the incumbents ahead would have been the actual defect. This is the first admission on this run whose new item did not land last, because it is the first whose key is not the largest in the unstarted set.

**No in-flight item's cohort or state changed**, which is trivially satisfied because no item is in flight. The four merged items are exempt from current-generation cohort membership and none of their records was touched.

**The declared radius needed no widening.** `Get-BlastRadius` returned 92 paths from the preflight-cleared plan and `spec.md`, and the item branch's three-dot diff against `origin/main` is exactly five files, all inside the feature folder and all covered by the derived feature-folder glob. The preparation child wrote no agent memory — the escape route that forced manual path additions on items 646, 656 and 670 — because the delegation prompt named that consequence explicitly and the child declined. That reproduces the item 678 result and confirms the suppression is a usable lever rather than a coincidence, at the cost that the child's reusable findings live only in its report.

### Item 647 — delivery history

Item 647 shipped as pull request 712, merged at `2b85134b`. Delivery was 89 of 89 plan tasks with zero unmet acceptance conditions, a closing feature review of 0 Blocking and 18 Non-blocking findings with a GO recommendation, and 21 of 21 acceptance criteria checked off in `spec.md` and independently re-verified on disk by the reviewer rather than accepted from the executor. Zero remediation cycles were required.

**CI was confirmed green before the merge, and the head SHA moved once, deliberately.** The child owed agent-memory commits after CI had already started, and committed them immediately rather than after a green run, superseding run `33452814634`. That superseded run reports `conclusion: cancelled`, and `gh run watch --exit-status` returned `0` on it — an exit code is not a pass signal for a cancelled run, and only the `conclusion` field is. The authoritative green result is run `33452909264` against the final head `070bd5fd`, whose five required checks (`actionlint`, `build-analyzers`, `build-nullable`, `format-check`, `mstest-coverage`) all pass. That conclusion was re-read directly through `gh pr checks` at the parent before `merge_status` advanced to `ci_green`.

**Neither PR-creation gate fired.** `local_execution_overrides` stayed empty for this item, so no user authorization was needed, in contrast with item 644. `PR_AUTHOR_RECEIPT_STALE` did not fire.

**Worktree cleanup succeeded**, unlike item 637's. The worktree was clean but locked by the finished child, so it was unlocked first and then removed with a plain removal; forcing was neither needed nor used.

**One repository defect surfaced during delivery.** `artifacts/orchestration/orchestrator-state.json` is tracked in git on `main`, added by unrelated commit `e8e628f0`, despite `.gitignore` listing `artifacts/`. Any orchestrator writing its checkpoint therefore dirties a tracked file and pollutes its own change footprint. The child contained it locally with `git update-index --skip-worktree` and committed nothing to that path. The file should be untracked upstream; this run did not do so, because it is outside the item's scope. The same defect recurred during the preparation of items 646 and 285 and was contained the same way.

**Five follow-up issues were opened** through the MCP promotion surface: 707, 708 and 709 for the plan's deferred non-goals, and 710 and 711 for review residuals — a coverage regression in `QfcHomeController.Metrics.cs` caused by an untestable static logger, and 14 load-sensitive `QuickFiler.Test` pump-host tests that fail under coverage on a loaded machine and pass on re-run. The latter is CI-flakiness debt.

**Item 637 completed out-of-band, on codex, after the operator was rate-limited on Claude.** Ground truth for its completion was rederived from `git`/`gh` per the cache doctrine rather than trusted from any checkpoint. See the delivery-history section below for the full reconciliation.

### Item 648 — admission

Item 648 was admitted by `/parallel-add 648` at 2026-09-01T11:20:00Z as a `DEFER_AND_RECOLOR` decision, advancing `recolor_generation` from 7 to 8.

**A delivery pre-check ran before any preparation work was considered.** GitHub issue 648 is `OPEN`, and a bare-number search of `origin/main` returned exactly one hit: `98113b09 docs(quickfiler): capture the #493 R-1 residual as issue #648`. Its body reads "Issue #648 tracks the ungated reflection swap", which is a promotion record rather than a delivery claim, so the hit is the one bare-number shape that means genuine outstanding work. No `fix(648)` commit exists. The guard site on `origin/main` at `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` still resolves the private static by raw reflection, writes it with `field.SetValue(null, dispatcher)`, and restores it with an unconditional `field.SetValue(null, original)` in a plain `finally`, acquiring neither `FieldLock` nor the transaction gate. The prerequisite the issue depends on has landed: `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` is present on `main` with the `Exchange`, `CompareExchange` and `ReleaseTransactionGate` surface the remedy calls for.

**Preparation was not re-run, because it had already completed.** Branch `bug/wpfuidispatchertests-ungated-static-swap-648` existed and was pushed, carrying the active feature folder, an 837-line research artifact, and a 332-line three-phase minimal-audit plan revised across three preflight rounds. The abandoned child's checkpoint records `completed_steps` through `S4_preflight` and `preflight_round_4: PREFLIGHT: ALL CLEAR / CONVERGENCE: NO FURTHER ROUNDS EXPECTED`. What had never happened was the checkpoint write: the item was absent from `items[]` entirely, which is the deferred-write posture a `/parallel-add` holds while preparation runs. The admission therefore resumed at the decision step rather than restarting an expensive preparation.

**The radius was re-derived by the parent and then widened.** `Get-BlastRadius` over the preflight-cleared plan and `issue.md` reproduced the abandoned child's recorded `path_count` of 91, its two modules, and its empty `shared_surfaces` and `contracts` exactly. Reconciling that against the item branch's three-dot diff against `origin/main` found ten uncovered paths, all agent-memory files the preparation child committed from four agent trees. They were added as ten exact paths rather than a `.claude/agent-memory/**` glob, and the widened 101-path radius was re-tested against all eleven existing items: no conflict verdict and no reason-kind set changed.

**The candidate conflicts with every existing item**, on both `path_overlap` and `module_overlap` in all eleven pairs. Item 285 is an unstarted `scheduled` member of the current cohort at index 4, so the deferred branch applies. The pinned (`in_flight`) set was empty, so no pinned-conflict offset applied and the lowest returned index equals `current_cohort`. The unstarted subgraph `{285, 287, 633, 646, 648, 656, 670, 678}` is complete on eight vertices, so `compute-cohorts.sh` returned eight singleton classes in ascending-key order; with the offset, 648 lands at index 8 and items 656, 670 and 678 each shift up by one. No in-flight item moved, because there were none.

### Item 662 — admission

Item 662 was admitted by `/parallel-add 662` at 2026-09-01T10:16:33Z as a `DEFER_AND_RECOLOR` decision, advancing `recolor_generation` from 8 to 9.

**A delivery pre-check ran before any preparation work was considered, and all three guard sites confirm the defect outstanding on `origin/main`.** GitHub issue 662 is `OPEN`, and a bare-number search of `origin/main` returned no hit at all — neither a delivering `fix` commit nor a promotion record — which is the rarer, unambiguous case in a repository where issue state is otherwise decoupled from delivery. The guard sites settle it directly. `QuickFiler/Controllers/EfcSelectionGuard.cs:15` still declares `private const string BannerPrefix = "==="` (three characters) and tests it with `StartsWith` at `:49` and `:75`. Both producers declare four characters, at `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19` and `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:16`. The comment on the `SelectedFolder` property in `QuickFiler/Controllers/EfcFormController.cs` asserts that `IsValidSelection` keeps its four-character rejection as a second guard, which is behaviour the three-character guard does not implement. The defect is latent rather than live — no producer emits a three-character row — but the divergence and the inaccurate comment are the condition that produced issue 465 defect D.

**Preparation was not re-run, because it had already completed.** Branch `bug/efcselectionguard-banner-prefix-arity-and-stale-comment-662` existed, was pushed, and matched `origin` exactly at head `0cf48433`, carrying fourteen commits: the minor-audit feature folder, a research artifact, a 427-line three-phase plan, and three preflight rounds. Round 3 is recorded as a **committed evidence artifact**, `evidence/other/preflight-round-3-clearance.md`, carrying `PREFLIGHT: ALL CLEAR` and `CONVERGENCE: NO FURTHER ROUNDS EXPECTED` over all 52 tasks — so the clearance was greppable in the committed tree rather than surviving only in a dead child's returned text. The abandoned child's checkpoint corroborates it with `completed_steps` through `S4_preflight` and `next_step: S5_atomic_execution`, and its worktree was clean and unlocked. What had never happened was the checkpoint write: the item was absent from `items[]` entirely, which is the deferred-write posture a `/parallel-add` holds while preparation runs. The admission therefore resumed at the decision step and delegated no child at all. Minor-audit folder integrity was re-verified: `issue.md` carries `- Work Mode: minor-audit` and an explicit `## Acceptance Criteria` section, and neither `spec.md` nor `user-story.md` is present. The committed plan was re-run through the plan validator gate and passed.

**The radius was re-derived by the parent and then widened.** `Get-BlastRadius` over the preflight-cleared plan and `issue.md` returned 69 paths across the modules `QuickFiler`, `QuickFiler.Test`, `TaskMaster.Test` and `UtilitiesCS`, with empty `shared_surfaces` and `contracts`. Reconciling that against the item branch's three-dot diff against `origin/main` found ten uncovered paths, all agent-memory files the preparation child committed from three agent trees — five under `atomic-planner`, three under `orchestrator`, two under `task-researcher`. They were added as ten exact paths rather than a `.claude/agent-memory/**` glob, on the reasoning recorded for item 646.

**The re-test after widening changed four edges, which is why it is run rather than assumed.** Against the narrow 69-path radius the pairs `285 ~ 662`, `646 ~ 662`, `648 ~ 662` and `656 ~ 662` contended by `module_overlap` alone; against the widened 79-path radius all four additionally carry `path_overlap`, through the shared `orchestrator` and `atomic-planner` memory indexes. No conflict **verdict** changed — twelve of twelve pairs conflict either way — so the schedule is unaffected, but the recorded reasons would have been wrong had the pre-widening values been written.

**The candidate conflicts with every existing item**, twelve of twelve. Item 285 is an unstarted `scheduled` member of the current cohort at index 4, so the deferred branch applies. The pinned (`in_flight`) set was empty — durably confirmed from `git worktree list --porcelain`, `git branch`, and a `gh pr list` returning no open pull request — so no pinned-conflict offset applied and the lowest returned index equals `current_cohort`. The unstarted subgraph `{285, 287, 633, 646, 648, 656, 662, 670, 678}` is complete on nine vertices with all 36 pairs contending, so `compute-cohorts.sh` returned nine singleton classes in ascending-key order; with the offset, 662 lands at index 10 and items 670 and 678 each shift up by one. **No in-flight item moved, because there were none**, and no merged item was touched.

**The write was guarded.** It would have aborted rather than proceeded had `recolor_generation`, the item count, or the mutation count moved between the decision and the write. All three were unmoved, so no concurrent mutation landed during this admission.

### Item 663 — admission

Item 663 was admitted by `/parallel-add 663` at 2026-09-01T11:25:00Z as a `DEFER_AND_RECOLOR` decision, advancing `recolor_generation` from 9 to 10.

**A delivery pre-check ran before any preparation work was considered, and the twin guard sites confirm the defect outstanding on `origin/main`.** GitHub issue 663 is `OPEN`, and a bare-number search of `origin/main` returned three hits, all of them false positives: the digits `663` occur inside the commit SHAs `8663db03` and `29c9f789...666373851...` and inside the test count `4663/4663`. No commit references the issue at all, so neither the delivering-commit shape nor the promotion-record shape is present. The guard sites settle it directly. `QuickFiler/Viewers/QfcFormViewerDark.cs` and `QuickFiler/Viewers/QfcFormViewerExpanded.cs` both gate on `QfcFormKeyHandler.IsAltKeyCommand(keyData)` alone and `return true` for the whole predicate, swallowing every Alt chord the predicate accepts. The non-twin `QuickFiler/Viewers/QfcFormViewer.cs` carries an additional conjunct on the same call, which is the asymmetry the issue reports as the over-claim.

**Preparation was not re-run, because it had already completed.** Branch `bug/qfc-twin-processcmdkey-alt-chord-over-claim-663` existed, was pushed, and matched `origin` exactly at head `366d5102`, with `origin/main` an ancestor throughout. It carries ten commits: the active feature folder, a research artifact, a 1,008-line seven-phase plan, `spec.md`, and the preflight rounds. Clearance is recorded as a **committed evidence artifact**, `evidence/other/preflight-rounds.2026-09-01T07-05.md`, carrying `PREFLIGHT: ALL CLEAR` and `CONVERGENCE: NO FURTHER ROUNDS EXPECTED` at round 7 with the defect ledger 12 → 5 → 11 → 10 → 3 → 1 → 0. What had never happened was the checkpoint write: the item was absent from `items[]` entirely, which is the deferred-write posture a `/parallel-add` holds while preparation runs. The admission therefore resumed at the decision step and delegated no child at all. The child's worktree was clean and its last commit landed at 11:08Z. Work-mode integrity was re-verified from the branch: `issue.md` carries `- Work Mode: full-bug`, `spec.md` is present, and `user-story.md` is absent, which is the documented `full-bug` shape. The committed plan was re-run through the plan validator gate and returned `ok:true`.

**The radius was derived by the parent and then widened.** `Get-BlastRadius` over the preflight-cleared plan and `spec.md` returned 69 paths across the modules `QuickFiler` and `QuickFiler.Test`, with empty `shared_surfaces` and `contracts`. Reconciling that against the item branch's three-dot diff against `origin/main` found thirteen uncovered paths: eleven agent-memory files the preparation child committed from three agent trees — three under `atomic-planner`, six under `orchestrator`, two under `task-researcher` — one promoted potential entry at `docs/features/potential/promoted/2026-08-31-invoke-mstest-single-assembly-strictmode-count-throw.md`, and one evidence artifact named in the derived radius only by its folder glob. They were added as thirteen exact paths rather than a `.claude/agent-memory/**` glob, on the reasoning recorded for item 646. The escaped paths were separately resolved through `Get-BlastRadiusFromObservedPaths` and contribute no additional module and no shared surface.

**The re-test after widening changed one edge, which is why it is run rather than assumed.** Against the narrow 69-path radius the pair `662 ~ 663` contended by `module_overlap` alone; against the widened 82-path radius it additionally carries `path_overlap`, through the shared `atomic-planner` memory index. No conflict **verdict** changed — thirteen of thirteen pairs conflict either way — and no pre-existing edge was affected, so the schedule is unaffected; but the recorded reason would have been wrong had the pre-widening value been written.

**The candidate conflicts with every existing item**, thirteen of thirteen. Item 285 is an unstarted `scheduled` member of the current cohort at index 4, so the deferred branch applies. The pinned (`in_flight`) set was empty — durably confirmed from `git worktree list --porcelain`, `git branch`, and the per-item pull-request state — so no pinned-conflict offset applied and the lowest returned index equals `current_cohort`. The unstarted subgraph `{285, 287, 633, 646, 648, 656, 662, 663, 670, 678}` is complete on ten vertices with all 45 pairs contending, so `compute-cohorts.sh` returned ten singleton classes in ascending-key order; with the offset, 663 lands at index 11 and items 670 and 678 each shift up by one. **No in-flight item moved, because there were none**, and no merged item was touched.

**The write was guarded.** It would have aborted rather than proceeded had `recolor_generation`, `current_cohort`, the unstarted set, or the pinned set moved between the decision and the write. All four were unmoved, so no concurrent mutation landed during this admission.

## Items

Cohort column projects the current-generation (`generation == 10`) assignment. The four merged items are exempt from current-generation cohort membership and therefore carry no cohort index; their earlier assignments remain visible in the Cohorts table below.

| issue_num | feature_folder | cohort | state | merge_status | pr_url | merge_commit_sha |
| --- | --- | --- | --- | --- | --- | --- |
| 638 | `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638` | — | merged | worktree_removed | https://github.com/drmoisan/TaskMaster/pull/700 | fa2ddefacf2c08abe18f3e3250d77da804534637 |
| 644 | `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644` | — | merged | worktree_removed | https://github.com/drmoisan/TaskMaster/pull/702 | 69aa28dd1154684b622904b9958ecaa2c6aa17d0 |
| 647 | `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647` | — | merged | worktree_removed | https://github.com/drmoisan/TaskMaster/pull/712 | 2b85134b42872e405602e6064e02dc9cda6c319b |
| 637 | `docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637` | — | merged | merged | https://github.com/drmoisan/TaskMaster/pull/706 | 9b6aff2e886eb86af5dfc131ebee7a2ebe1a5b6c |
| 646 | `docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646` | — | merged | merged | https://github.com/drmoisan/TaskMaster/pull/718 | c7b4f08f6d80296840f9a351042cb2113892e95f |
| 656 | `docs/features/active/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656` | — | merged | merged | https://github.com/drmoisan/TaskMaster/pull/720 | 43dcc800e5c75ab1d1033f0eac0e4b61ac919b59 |
| 285 | `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285` | — | merged | merged | https://github.com/drmoisan/TaskMaster/pull/715 | 09eae2e85cd586c092fb1977a76cd9e895ec0a3b |
| 633 | `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633` | — | merged | merged | https://github.com/drmoisan/TaskMaster/pull/717 | 8996b28746d32f9f5996a037e0ca76be78b7684d |
| 670 | `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670` | — | merged | merged | https://github.com/drmoisan/TaskMaster/pull/723 | 807fb0bb6e5e49f43efa6b256b05960bf078ca19 |
| 678 | `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678` | — | merged | merged | https://github.com/drmoisan/TaskMaster/pull/724 | 018202053b4eb5f1227d7f788dc58914b38326ad |
| 287 | `docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287` | — | merged | merged | https://github.com/drmoisan/TaskMaster/pull/716 | 06b1e02e5d545b4dfae398cdbf9ae10a3f98ac72 |
| 648 | `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648` | — | merged | merged | https://github.com/drmoisan/TaskMaster/pull/719 | 5670b3cfe6a52e3b890bf80f0cd85a20d4fe4723 |
| 662 | `docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662` | — | merged | merged | https://github.com/drmoisan/TaskMaster/pull/721 | 9ca9e99a86428717891a4b54fed70f573a0a2d65 |
| 663 | `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663` | — | merged | merged | https://github.com/drmoisan/TaskMaster/pull/722 | 988d35a8f8eb7436cc46a9f6424db917ed93807a |

### Item Lifecycle Timestamps

| issue_num | branch_name | worktree_created_at | pr_opened_at | ci_green_at | merged_at | worktree_removed_at |
| --- | --- | --- | --- | --- | --- | --- |
| 638 | `bug/efc-unguarded-archive-root-read-crashes-ui-thread-638` | 2026-08-29T16:09:08Z | 2026-08-29T17:25:51Z | 2026-08-29T17:30:04Z | 2026-08-29T17:30:10Z | 2026-08-29T17:30:53Z |
| 644 | `bug/qfc-unregister-navigation-count-mismatch-orphan-644` | 2026-08-30T02:05:00Z | 2026-08-30T15:10:00Z | 2026-08-30T15:30:00Z | 2026-08-30T11:13:25Z | 2026-08-30T15:38:00Z |
| 647 | `bug/fileio2-write-retry-reports-success-on-final-failure-647` | 2026-08-31T22:36:00Z | 2026-09-01T00:01:00Z | 2026-09-01T00:12:00Z | 2026-09-01T00:05:33Z | 2026-09-01T00:16:00Z |
| 637 | `bug/breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637` | 2026-08-30T15:42:00Z | 2026-08-31T17:27:43Z | 2026-08-31T17:43:29Z | 2026-08-31T17:39:04Z | — |
| 646 | `bug/qfc-metrics-flush-writes-empty-session-file-646` | 2026-09-01T16:05:00Z | 2026-09-01T16:55:00Z | 2026-09-01T17:08:00Z | 2026-09-01T17:09:50Z | — |
| 656 | `bug/breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656` | 2026-09-01T18:29:00Z | 2026-09-01T19:20:00Z | 2026-09-01T19:28:00Z | 2026-09-01T19:29:30Z | — |
| 285 | `bug/timeouttask-runwithtimeout-exception-type-mismatch-285` | 2026-09-01T11:59:21Z | 2026-09-01T12:47:00Z | 2026-09-01T13:01:00Z | 2026-09-01T13:02:41Z | — |
| 633 | `bug/qfc-unsynchronized-undo-handoff-after-batch-move-633` | 2026-09-01T14:25:00Z | 2026-09-01T15:48:00Z | 2026-09-01T16:01:00Z | 2026-09-01T16:02:21Z | — |
| 670 | `bug/qfc-initializewebviewasync-fault-is-unobserved-670` | 2026-09-01T23:35:00Z | 2026-09-02T00:55:00Z | 2026-09-02T01:08:00Z | 2026-09-02T01:10:37Z | — |
| 678 | `bug/quickfiler-carry-folder-predictor-to-item-controller-678` | 2026-09-02T01:15:00Z | 2026-09-02T05:55:00Z | 2026-09-02T06:15:00Z | 2026-09-02T06:17:18Z | — |
| 287 | `bug/storewrapper-dialog-imprecise-for-genuine-failure-287` | 2026-09-01T13:07:00Z | 2026-09-01T14:05:00Z | 2026-09-01T14:18:00Z | 2026-09-01T14:19:51Z | — |
| 648 | `bug/wpfuidispatchertests-ungated-static-swap-648` | 2026-09-01T17:12:00Z | 2026-09-01T18:12:00Z | 2026-09-01T18:25:00Z | 2026-09-01T18:26:50Z | — |
| 662 | `bug/efcselectionguard-banner-prefix-arity-and-stale-comment-662` | 2026-09-01T19:35:00Z | 2026-09-01T21:20:00Z | 2026-09-01T21:36:00Z | 2026-09-01T21:37:11Z | — |
| 663 | `bug/qfc-twin-processcmdkey-alt-chord-over-claim-663` | 2026-09-01T21:40:00Z | 2026-09-01T23:15:00Z | 2026-09-01T23:28:00Z | 2026-09-01T23:29:44Z | — |

Items 646, 656, 285, 633, 670, 678, 287, 648, 662 and 663 additionally record the admission-lifecycle timestamps `proposed_at`, `admitted_at`, `prepared_at`, and `scheduled_at` — item 646 all at 2026-09-01T00:44:13Z, item 656 all at 2026-09-01T01:27:03Z, item 285 all at 2026-09-01T05:23:49Z, item 633 all at 2026-09-01T07:19:32Z, item 670 all at 2026-09-01T08:18:56Z, item 678 all at 2026-09-01T08:42:00Z, item 287 all at 2026-09-01T10:00:00Z, item 648 all at 2026-09-01T11:20:00Z, item 662 all at 2026-09-01T10:16:33Z, item 663 all at 2026-09-01T11:25:00Z. Within each item those four values coincide because the checkpoint write is deliberately deferred until preparation returns and the admission decision has produced a cohort index: an item in state `proposed` cannot be represented validly, since invariant 9 requires a non-empty `paths` list and invariant 13 requires every non-withdrawn item to occupy exactly one current-generation cohort. The deferral also makes a rejected candidate free to discard, since nothing was written on its behalf.

### Item 644 — delivery history

Item 644 shipped as pull request 702, merged at `69aa28dd`. Its path to merge is recorded here because three of the obstacles were process defects rather than defects in the work.

**Delivery.** 58 of 58 plan tasks checked off. Closing feature-review returned zero Blocking findings with a GO verdict (`policy-audit`, `code-review`, `feature-audit` dated `2026-08-30T13-10`). All four toolchain gates were re-run from a clean worktree bootstrap — csharpier check, analyzer `/t:Rebuild`, nullable `/t:Rebuild`, vstest — each exit 0, at 1254 of 1254 tests passing. Footprint stayed exactly the six code paths plus this item's own feature folder. The third remediation cycle was never spent. All five CI checks passed on the pull request before the merge.

**Two interruptions, neither a defect in the work.** The first child died mid `S5_atomic_execution`; the second terminated on an API rate limit at `remediation.cycle_2.execute`. Both were resumed rather than restarted, because their work was durably committed on the item branch. Death was established from durable state — stale checkpoint, untouched feature folder, no build processes — never from a notification.

**Branch hygiene.** Three session-level commits carrying `.claude/agent-memory/**` and `docs/features/potential/**` had been committed onto the item branch because the session worktree was checked out on it. They were preserved onto `docs/parallel-session-notes-2026-08-29` and the item branch reset; `save/644-session-tip-2026-08-29` holds the pre-reset tip. This was required, not cosmetic: plan task `[P4-T8]` and AC-14 assert the repository-wide anchored diff carries no path outside the six code paths and the feature folder, and the backlog files would have failed that clause. A later agent-memory-only commit was tolerated because that span excludes `.claude/agent-memory` by pathspec.

**The PR-creation overrides gate.** `Get-OrchestratorStatePrCreationReadinessError` requires `local_execution_overrides` to be an empty list, and this run recorded three: `diff_anchor_substitution`, `p4_t6_rerun_after_contention`, and `p4_t6_comparison_clause_undecidable_at_measured_noise_floor`. That field's only function is to block PR creation for a run that deviated from its approved plan, and no drain or adjudication procedure for it exists anywhere under `.claude/` — it appears only in `OrchestratorState.psm1` and `OrchestratorStateRoutingContract.psm1`. The child refused to clear it and escalated; this orchestrator also refused, because an agent's ratification is not user consent. The user was offered plan reconciliation or an explicit exception, and authorized the exception directly. The three records were cleared from the live field and preserved verbatim under `local_execution_overrides_archived.entries`, alongside a `local_execution_overrides_exception` record naming the authorizing party. Nothing was deleted, and the archived record is retained at `artifacts/orchestration/orchestrator-state.644.json`.

**What the exception did not cover.** CI was not waived: all five checks were confirmed green through `gh pr checks` and `gh pr view` before the merge. AC-16 remains a PARTIAL with accepted residual risk and is described that way in the pull-request body, never as a clean pass — the reviewer independently verified that the `[ExcludeFromCodeCoverage]` attribute on the sole changed production file was pre-existing at the anchor, so the coverage argument is not circular.

### Item 637 — delivery history

Item 637 shipped as pull request 706, merged at `9b6aff2e`. Its path to merge is recorded here because it completed entirely outside this session, on codex, and the reconciliation is not a routine cohort advance.

**Delivery.** PR 706 ("fix(quickfiler): normalize rooted breadcrumb selection paths") cites `Related: #637, #614, #439` and its changed files are rooted in this item's own feature folder plus `BreadcrumbBridgeRouter.Selection.cs`, `EfcDataModel.FilingStem.cs`, and the split Issue #439 fixture, confirming it is this item's delivery. The PR body reports CSharpier format/check passed, analyzer and nullable rebuilds passed, coverage-enabled MSTest passed at 6,894 tests with 0 failures and 85.3389% line coverage, and post-remediation policy, code, and feature reviews passed with zero blocking findings.

**Branch substitution.** The item was originally tracked on `bug/breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`, which this checkpoint's `worktree_path` still names (`.claude/worktrees/agent-af95f0a8159ff28fa`, parked at `23a0c934`). Delivery instead landed on a continuation worktree/branch pair created against the same agent id — worktree `.claude/worktrees/agent-af95f0a8159ff28fa-wt/2026-08-31T08-39`, branch `agent-af95f0a8159ff28fa-wt-2026-08-31T08-39` — which is why `pr_number`/`pr_url` could not be discovered from the originally tracked branch and had to be located by searching merged PRs against `main` and matching on content and feature-folder path.

**CI ordering.** `ci_green_at` (17:43:29Z, the last CI check's completion) is later than `merged_at` (17:39:04Z): the merge on codex preceded full CI completion rather than following it, unlike this run's Claude-executed items (638, 644), where CI green was confirmed before the merge command ran.

**GitHub issue #637 was not auto-closed by the merge** (the PR's `closingIssuesReferences` was empty) and remains `OPEN`. The merge itself, not the issue state, is treated as the authoritative completion signal per the cache doctrine.

**Neither worktree was removed, and cleanup is now deliberately deferred rather than merely unattempted.** It was assessed on 2026-08-31. The tracked worktree `.claude/worktrees/agent-af95f0a8159ff28fa` is dirty: 17 uncommitted modifications spanning every `.csproj` plus `.claude/state/powershell-batch-budget.default.json`. That diff is not this item's delivery — the tracked branch is already an ancestor of `origin/main` — but a local repair of stale analyzer include paths after a Dependabot bump (Meziantou.Analyzer 3.0.156 to 3.0.194, Roslynator.Analyzers 4.16.0 to 5.0.0) together with a UTF-8 BOM strip on each `.csproj`, which is the open issue #597 surface. A plain removal correctly refuses a dirty tree and was not forced, because discarding another context's uncommitted build repair is not this run's decision to make. The sibling continuation worktree `agent-af95f0a8159ff28fa-wt/2026-08-31T08-39`, which carried the actual delivery, is absent from `items[]` and is therefore unremovable through either gate, both of which fail closed on a path with no matching record. `merge_status` accordingly stays `merged` rather than advancing to `worktree_removed`.

## Cohorts

| index | generation | item_keys |
| --- | --- | --- |
| 0 | 0 | 638 |
| 1 | 0 | 644 |
| 2 | 0 | 647 |
| 1 | 1 | 644 |
| 2 | 1 | 637 |
| 3 | 1 | 647 |
| 4 | 1 | 646 |
| 4 | 2 | 646 |
| 5 | 2 | 656 |
| 4 | 3 | 285 |
| 5 | 3 | 646 |
| 6 | 3 | 656 |
| 4 | 4 | 285 |
| 5 | 4 | 633 |
| 6 | 4 | 646 |
| 7 | 4 | 656 |
| 4 | 5 | 285 |
| 5 | 5 | 633 |
| 6 | 5 | 646 |
| 7 | 5 | 656 |
| 8 | 5 | 670 |
| 4 | 6 | 285 |
| 5 | 6 | 633 |
| 6 | 6 | 646 |
| 7 | 6 | 656 |
| 8 | 6 | 670 |
| 9 | 6 | 678 |
| 4 | 7 | 285 |
| 5 | 7 | 287 |
| 6 | 7 | 633 |
| 7 | 7 | 646 |
| 8 | 7 | 656 |
| 9 | 7 | 670 |
| 10 | 7 | 678 |
| 4 | 8 | 285 |
| 5 | 8 | 287 |
| 6 | 8 | 633 |
| 7 | 8 | 646 |
| 8 | 8 | 648 |
| 9 | 8 | 656 |
| 10 | 8 | 670 |
| 11 | 8 | 678 |
| 4 | 9 | 285 |
| 5 | 9 | 287 |
| 6 | 9 | 633 |
| 7 | 9 | 646 |
| 8 | 9 | 648 |
| 9 | 9 | 656 |
| 10 | 9 | 662 |
| 11 | 9 | 670 |
| 12 | 9 | 678 |
| 4 | 10 | 285 |
| 5 | 10 | 287 |
| 6 | 10 | 633 |
| 7 | 10 | 646 |
| 8 | 10 | 648 |
| 9 | 10 | 656 |
| 10 | 10 | 662 |
| 11 | 10 | 663 |
| 12 | 10 | 670 |
| 13 | 10 | 678 |

Generation 10 is the current generation. It carries only the ten unstarted items, because the recolor is a pure function over the unstarted subgraph and the four merged items are exempt from current-generation membership. Their earlier rows are retained above so the schedule stays traceable by generation.

Each generation's indices are absolute checkpoint indices written verbatim, never re-based to zero. Generation 10 begins at index 4 rather than 0 because the lowest index the recolor may return is `current_cohort`, which is 4; the pinned-barrier offset above `highest_pinned_cohort` was not applied at generation 10, because the pinned set was empty and no unstarted item therefore conflicted with a pinned one.

Generations 5 and 6 are the two recolors in this run that moved no existing item. Generations 3, 4, 7, 8, 9 and 10 each shifted previously admitted items down an index, which is permitted because those items are unstarted rather than pinned. At generations 5 and 6 the coloring happened to place the new item last, so the incumbents retained their indices and the new item appended. That is an output of the deterministic coloring, not a scheduling preference. The stability had a structural cause: the unstarted subgraph is complete, so every vertex must receive a distinct colour, and Welsh-Powell's descending-degree ordering with an ascending-key tie-break assigns them in ascending key order — so a new item whose key is the largest in the unstarted set lands last.

Generation 7 is the first admission on this run where that condition did not hold, and it is the clearest illustration that "last" was never a property of being new. Item 287's key is smaller than five of the six incumbents, so the same ascending-key ordering places it second, at index 5, and shifts 633, 646, 656, 670 and 678 up to 6 through 10. Only 285, whose key is smaller still, keeps its index. The five displaced items are all unstarted, so the pinning invariant is untouched, and conflicting items remain in distinct cohorts, so the per-edge barrier still prevents any two of them from running concurrently.

Generation 10 is the same mechanism read in the opposite direction. Item 663's key is larger than seven of the nine incumbents and smaller than two, so ascending-key ordering places it eighth, at index 11, leaving 285, 287, 633, 646, 648, 656 and 662 on their existing indices and shifting only 670 and 678 up to 12 and 13. The displacement is therefore predictable before the write from the candidate's rank among the unstarted keys, and its size is a property of the key rather than of the admission.

## Conflict Edges

| a | b | reason | detail |
| --- | --- | --- | --- |
| 638 | 644 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 638 | 647 | `path_overlap` | TaskMaster/AppGlobals/AppOlObjects.cs ~ TaskMaster/AppGlobals/AppOlObjects.cs |
| 644 | 647 | `path_overlap` | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 ~ scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 |
| 637 | 638 | `path_overlap` | QuickFiler/Controllers/EfcDataModel.cs ~ QuickFiler/Controllers/EfcDataModel.cs |
| 637 | 644 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 637 | 647 | `path_overlap` | TaskMaster/AppGlobals/AppOlObjects.cs ~ TaskMaster/AppGlobals/AppOlObjects.cs |
| 637 | 646 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 638 | 646 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 644 | 646 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 646 | 647 | `path_overlap` | QuickFiler/Controllers/QfcHomeController.Metrics.cs ~ QuickFiler/Controllers/QfcHomeController.Metrics.cs |
| 637 | 656 | `path_overlap` | QuickFiler/Properties/AssemblyInfo.cs:5 ~ QuickFiler/Properties/AssemblyInfo.cs:5 |
| 638 | 656 | `path_overlap` | QuickFiler/Properties/AssemblyInfo.cs:5 ~ QuickFiler/Properties/AssemblyInfo.cs:5 |
| 644 | 656 | `path_overlap` | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 ~ scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 |
| 646 | 656 | `path_overlap` | scripts/vscode/Invoke-MSTestWithCoverage.ps1 ~ scripts/vscode/Invoke-MSTestWithCoverage.ps1 |
| 647 | 656 | `path_overlap` | scripts/vscode/Install-RepoDotNetSdk.ps1 ~ scripts/vscode/Install-RepoDotNetSdk.ps1 |
| 285 | 637 | `path_overlap` | .github/workflows/_mstest-coverage.yml ~ .github/workflows/_mstest-coverage.yml |
| 285 | 638 | `path_overlap` | TaskMaster/AppGlobals/AppOlObjects.cs ~ TaskMaster/AppGlobals/AppOlObjects.cs |
| 285 | 644 | `module_overlap` | QuickFiler.Test |
| 285 | 646 | `path_overlap` | .claude/agent-memory/orchestrator/MEMORY.md ~ .claude/agent-memory/orchestrator/MEMORY.md |
| 285 | 647 | `path_overlap` | TaskMaster/AppGlobals/AppOlObjects.cs ~ TaskMaster/AppGlobals/AppOlObjects.cs |
| 285 | 656 | `path_overlap` | scripts/vscode/Install-RepoDotNetSdk.ps1 ~ scripts/vscode/Install-RepoDotNetSdk.ps1 |
| 285 | 633 | `path_overlap` | scripts/vscode/Install-RepoDotNetSdk.ps1 ~ scripts/vscode/Install-RepoDotNetSdk.ps1 |
| 633 | 637 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 633 | 638 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 633 | 644 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 633 | 646 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 633 | 647 | `path_overlap` | scripts/vscode/Install-RepoDotNetSdk.ps1 ~ scripts/vscode/Install-RepoDotNetSdk.ps1 |
| 633 | 656 | `path_overlap` | QuickFiler/Properties/AssemblyInfo.cs:5 ~ QuickFiler/Properties/AssemblyInfo.cs:5 |
| 285 | 670 | `path_overlap` | .claude/agent-memory/orchestrator/MEMORY.md ~ .claude/agent-memory/orchestrator/MEMORY.md |
| 633 | 670 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 637 | 670 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 638 | 670 | `path_overlap` | QuickFiler.Test/Controllers/EfcFormControllerTests.cs ~ QuickFiler.Test/Controllers/EfcFormControllerTests.cs |
| 644 | 670 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 646 | 670 | `path_overlap` | .claude/agent-memory/orchestrator/MEMORY.md ~ .claude/agent-memory/orchestrator/MEMORY.md |
| 647 | 670 | `path_overlap` | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 ~ scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 |
| 656 | 670 | `path_overlap` | .claude/agent-memory/atomic-executor/MEMORY.md ~ .claude/agent-memory/atomic-executor/MEMORY.md |
| 285 | 678 | `module_overlap` | QuickFiler.Test |
| 633 | 678 | `path_overlap` | .config/dotnet-tools.json ~ .config/dotnet-tools.json |
| 637 | 678 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 638 | 678 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 644 | 678 | `path_overlap` | QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs ~ QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs |
| 646 | 678 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 647 | 678 | `path_overlap` | scripts/vscode/Invoke-MSTestWithCoverage.ps1 ~ scripts/vscode/Invoke-MSTestWithCoverage.ps1 |
| 656 | 678 | `path_overlap` | scripts/vscode/Invoke-MSTestWithCoverage.ps1 ~ scripts/vscode/Invoke-MSTestWithCoverage.ps1 |
| 670 | 678 | `path_overlap` | QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs ~ QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs |
| 287 | 638 | `path_overlap` | scripts/vscode/Install-RepoDotNetSdk.ps1 ~ scripts/vscode/Install-RepoDotNetSdk.ps1 |
| 287 | 644 | `path_overlap` | scripts/vscode/Invoke-MSTestWithCoverage.ps1 ~ scripts/vscode/Invoke-MSTestWithCoverage.ps1 |
| 287 | 647 | `path_overlap` | evidence/baseline/file-line-counts.md ~ evidence/baseline/file-line-counts.md |
| 287 | 637 | `path_overlap` | evidence/baseline/phase0-instructions-read.md ~ evidence/baseline/phase0-instructions-read.md |
| 287 | 646 | `path_overlap` | scripts/vscode/Invoke-MSTestWithCoverage.ps1 ~ scripts/vscode/Invoke-MSTestWithCoverage.ps1 |
| 287 | 656 | `path_overlap` | scripts/vscode/Install-RepoDotNetSdk.ps1 ~ scripts/vscode/Install-RepoDotNetSdk.ps1 |
| 285 | 287 | `path_overlap` | scripts/vscode/Install-RepoDotNetSdk.ps1 ~ scripts/vscode/Install-RepoDotNetSdk.ps1 |
| 287 | 633 | `path_overlap` | scripts/vscode/Install-RepoDotNetSdk.ps1 ~ scripts/vscode/Install-RepoDotNetSdk.ps1 |
| 287 | 670 | `path_overlap` | coverage/baseline.cobertura.xml ~ coverage/baseline.cobertura.xml |
| 287 | 678 | `path_overlap` | scripts/vscode/Invoke-MSTestWithCoverage.ps1 ~ scripts/vscode/Invoke-MSTestWithCoverage.ps1 |
| 638 | 648 | `path_overlap` | .github/workflows/_mstest-coverage.yml:83 ~ .github/workflows/_mstest-coverage.yml:83 |
| 644 | 648 | `path_overlap` | .claude/settings.local.json ~ .claude/settings.local.json |
| 647 | 648 | `path_overlap` | scripts/vscode/Install-RepoDotNetSdk.ps1 ~ scripts/vscode/Install-RepoDotNetSdk.ps1 |
| 637 | 648 | `path_overlap` | .github/workflows/_mstest-coverage.yml ~ .github/workflows/_mstest-coverage.yml |
| 646 | 648 | `path_overlap` | .claude/agent-memory/orchestrator/MEMORY.md ~ .claude/agent-memory/orchestrator/MEMORY.md |
| 648 | 656 | `path_overlap` | .claude/agent-memory/atomic-planner/MEMORY.md ~ .claude/agent-memory/atomic-planner/MEMORY.md |
| 285 | 648 | `path_overlap` | .claude/agent-memory/orchestrator/MEMORY.md ~ .claude/agent-memory/orchestrator/MEMORY.md |
| 633 | 648 | `path_overlap` | .github/workflows/_format-check.yml ~ .github/workflows/_format-check.yml |
| 648 | 670 | `path_overlap` | .claude/agent-memory/atomic-planner/MEMORY.md ~ .claude/agent-memory/atomic-planner/MEMORY.md |
| 648 | 678 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 287 | 648 | `path_overlap` | scripts/vscode/Install-RepoDotNetSdk.ps1 ~ scripts/vscode/Install-RepoDotNetSdk.ps1 |
| 285 | 662 | `path_overlap` | .claude/agent-memory/orchestrator/MEMORY.md ~ .claude/agent-memory/orchestrator/MEMORY.md |
| 287 | 662 | `module_overlap` | TaskMaster.Test |
| 633 | 662 | `path_overlap` | QuickFiler/**/*.cs ~ QuickFiler/Controllers/EfcFormController.cs |
| 637 | 662 | `path_overlap` | QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs ~ QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs |
| 638 | 662 | `path_overlap` | QuickFiler.Test/Controllers/EfcFormControllerTests.cs ~ QuickFiler.Test/Controllers/EfcFormControllerTests.cs |
| 644 | 662 | `module_overlap` | QuickFiler |
| 646 | 662 | `path_overlap` | .claude/agent-memory/orchestrator/MEMORY.md ~ .claude/agent-memory/orchestrator/MEMORY.md |
| 647 | 662 | `module_overlap` | QuickFiler |
| 648 | 662 | `path_overlap` | .claude/agent-memory/atomic-planner/MEMORY.md ~ .claude/agent-memory/atomic-planner/MEMORY.md |
| 656 | 662 | `path_overlap` | .claude/agent-memory/atomic-planner/MEMORY.md ~ .claude/agent-memory/atomic-planner/MEMORY.md |
| 662 | 670 | `path_overlap` | .claude/agent-memory/atomic-planner/MEMORY.md ~ .claude/agent-memory/atomic-planner/MEMORY.md |
| 662 | 678 | `module_overlap` | QuickFiler |
| 285 | 663 | `path_overlap` | .claude/agent-memory/orchestrator/MEMORY.md ~ .claude/agent-memory/orchestrator/MEMORY.md |
| 287 | 663 | `path_overlap` | scripts/vscode/Install-RepoDotNetSdk.ps1 ~ scripts/vscode/Install-RepoDotNetSdk.ps1 |
| 633 | 663 | `path_overlap` | .github/workflows/_format-check.yml ~ .github/workflows/_format-check.yml |
| 637 | 663 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 638 | 663 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 644 | 663 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |
| 646 | 663 | `path_overlap` | .claude/agent-memory/orchestrator/MEMORY.md ~ .claude/agent-memory/orchestrator/MEMORY.md |
| 647 | 663 | `path_overlap` | scripts/vscode/Install-RepoDotNetSdk.ps1 ~ scripts/vscode/Install-RepoDotNetSdk.ps1 |
| 648 | 663 | `path_overlap` | .claude/agent-memory/atomic-planner/MEMORY.md ~ .claude/agent-memory/atomic-planner/MEMORY.md |
| 656 | 663 | `path_overlap` | .claude/agent-memory/atomic-planner/MEMORY.md ~ .claude/agent-memory/atomic-planner/MEMORY.md |
| 662 | 663 | `path_overlap` | .claude/agent-memory/atomic-planner/MEMORY.md ~ .claude/agent-memory/atomic-planner/MEMORY.md |
| 663 | 670 | `path_overlap` | .claude/agent-memory/atomic-planner/MEMORY.md ~ .claude/agent-memory/atomic-planner/MEMORY.md |
| 663 | 678 | `path_overlap` | QuickFiler.Test/QuickFiler.Test.csproj ~ QuickFiler.Test/QuickFiler.Test.csproj |

The ten edges introduced by item 287 are the strongest single illustration of the `mandate_reads` gap described above: seven of the ten rest on a `scripts/vscode/` path — `Install-RepoDotNetSdk.ps1` on five and `Invoke-MSTestWithCoverage.ps1` on four, counting the one edge whose intersection holds both. Those are mandated commands the plan cites as things it will run, not files it will write, so seven of this item's ten edges encode tooling citation rather than product contention. The remaining three rest on shared baseline-evidence and coverage artifacts (`evidence/baseline/file-line-counts.md`, `evidence/baseline/phase0-instructions-read.md`, `coverage/baseline.cobertura.xml`). Six of the ten also carry `module_overlap`, which for this item is genuine: it edits `UtilitiesCS` and `TaskMaster` and their test projects.

Not one of item 287's edges rests on a `.claude/agent-memory/` path, matching item 678 and distinguishing both from items 646, 656, 285, 670 and 662. In each of these two cases the preparation child wrote no agent memory, so no reconciliation-added path entered the radius and every edge is plan-derived.

Item 663 introduced thirteen edges, one to every other item in the run, and every one of them carries `path_overlap` — the first admission on this run to produce no `module_overlap`-only edge. Six rest on a `.claude/agent-memory/` path and exist only because the derived radius was reconciled against the item branch's diff; two rest on `scripts/vscode/Install-RepoDotNetSdk.ps1`, the mandated bootstrap script that plans cite as a command they run rather than a file they write. Each recorded `detail` is a verified member of the two radii's exact path intersection rather than the library's reported pair, and the intersection sizes range from two to nine, which is the variation that distinguishes a working harness from the single-element collapse recorded for `285 ~ 670`. The `662 ~ 663` row is the edge the widening changed: against the narrow radius it carried `module_overlap` alone.

Item 662 introduced twelve edges, one to every other item in the run, of which five rest on a `.claude/agent-memory/` path and exist only because the derived radius was reconciled against the item branch's diff. Four carry `module_overlap` as their sole reason. The `633 ~ 662` detail records the glob pair `QuickFiler/**/*.cs ~ QuickFiler/Controllers/EfcFormController.cs`, because item 633's radius carries that module-wide glob and the two radii share no exact path; the remaining seven `path_overlap` details are verified members of the exact intersection rather than the library's reported pair. The `637 ~ 662` row is the clearest instance of why that corroboration is required: the library reported `**/evidence/**/*.md ~ .claude/agent-memory/atomic-planner/MEMORY.md`, a pair that glob cannot match, and the recorded path `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` is a verified member of the intersection instead.

The nine edges introduced by item 678 all carry `module_overlap`, and eight of the nine also carry `path_overlap`. Not one rests on a `.claude/agent-memory/` path, which distinguishes this admission from items 646, 656, 285 and 670: its preparation child wrote no agent memory, so no reconciliation-added path entered the radius and every edge here is a plan-derived contention. Two of the nine rest on `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, the mandated coverage command that plans cite as a command they run rather than a file they write — the `mandate_reads` gap described above.

The `285 ~ 678` row was the second edge in the run whose sole reason is `module_overlap`, joining `285 ~ 644`: the two radii share no exact path, but both name the `QuickFiler.Test` module. Item 662 added four more — `287 ~ 662`, `644 ~ 662`, `647 ~ 662` and `662 ~ 678` — bringing the total to six. In each the two radii share no exact path but name a module in common, and the `detail` records that module rather than a path pair, because there is no path pair to record.

All eight edges introduced by item 670 carry both `path_overlap` and `module_overlap`, and the three joining it to items 637 and 638 additionally carry `contract_dependency`. Three of the eight rest on a `.claude/agent-memory/` path and exist only because the derived radius was reconciled against the item branch's diff; without that step those three edges would be absent from the graph and the declared radius would under-report what the item lands on `main`.

The `285 ~ 670` detail is recorded here after a correction. The intersection of those two radii holds exactly one path, and a single-element PowerShell pipeline result collapses to a scalar string, so the first attempt indexed the string and wrote `. ~ .`. That was investigated rather than accepted, because a bare `.` path token in a declared radius would match essentially everything and would be a far more serious finding than a formatting slip. No item carries such a token; the value above is the verified intersection and the checkpoint was re-validated after the correction.

All seven edges introduced by item 633 carry both `path_overlap` and `module_overlap`, and each recorded `detail` is a verified member of the two radii's exact path intersection rather than the library's reported pair. That distinction mattered here: item 633's derived radius carries the module-wide glob `QuickFiler/**/*.cs`, which is exactly the condition under which the reported pair becomes unreliable.

The `285 ~ 644` row was the first edge in the run whose sole reason is `module_overlap`: the two radii share no exact path, but both name the `QuickFiler.Test` module. Every other row records an exact path present in both radii, corroborated by set intersection rather than read from the contention library's reported pair, because that reported pair is unreliable once either radius carries a glob. The `637 ~ 656` row is a direct instance: the library reported the pair `**/evidence/**/*.md ~ QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs:89`, which that glob cannot match, and the recorded path is a verified member of the two radii's exact intersection instead. The verdict itself was trustworthy in every case; only the reported pair was not.

Before any fresh verdict was trusted, the harness was replayed against the previously recorded edges and reproduced each one exactly on the `conflict` value and the reason-kind set, with no extra edges reported. The item 633 admission replayed all twenty-one edges recorded at that point and matched every one. That replay is what validates the calling convention, since its two known failure modes — an unresolved module import and a boolean test of the returned hashtable — fail in opposite directions, so neither an all-clear nor an all-conflict result is self-validating.

## Mutations

| op | item_key | at | prior_state | new_state | disposition | recolor_generation |
| --- | --- | --- | --- | --- | --- | --- |
| `add` | 637 | 2026-08-30T01:40:19Z | — | `scheduled` | — | 1 |
| `add` | 646 | 2026-09-01T00:44:13Z | — | `scheduled` | — | 1 |
| `add` | 656 | 2026-09-01T01:27:03Z | — | `scheduled` | — | 2 |
| `add` | 285 | 2026-09-01T05:23:49Z | — | `scheduled` | — | 3 |
| `add` | 633 | 2026-09-01T07:19:32Z | — | `scheduled` | — | 4 |
| `add` | 670 | 2026-09-01T08:18:56Z | — | `scheduled` | — | 5 |
| `add` | 678 | 2026-09-01T08:42:00Z | — | `scheduled` | — | 6 |
| `add` | 287 | 2026-09-01T10:00:00Z | — | `scheduled` | — | 7 |
| `add` | 648 | 2026-09-01T11:20:00Z | — | `scheduled` | — | 8 |
| `add` | 662 | 2026-09-01T10:16:33Z | — | `scheduled` | — | 9 |
| `add` | 663 | 2026-09-01T11:25:00Z | — | `scheduled` | — | 10 |
| `close` | — | 2026-09-02T10:09:45Z | — | — | — | 10 |

The `close` row is the run's final mutation, and nothing may be appended after it. It is the only run-scoped entry in the table: `item_key` is null because a close acts on the run rather than on an item, and `new_state` is null for the same reason. It stamps `recolor_generation` unchanged at 10 and rewrote no cohort, because run termination changes no cohort assignment and is therefore a non-recompute operation. Counting from generation 0, the eleven admissions contributed nine recomputes and the close contributed none, which is exactly the recorded value.

Every `add` row carries a null `prior_state`, which is the documented shape for an `add`. The accompanying `prepared` to `scheduled` transition is not recorded in the mutation entry; it is recorded as an item-state update in `items[]` with the lifecycle timestamps.

The `recolor_generation` column distinguishes the two admission branches. Items 637 and 646 stamp the generation unchanged because neither admission required a recompute. Items 656, 285, 633, 670, 678, 287, 648, 662 and 663 increment it, to 2, 3, 4, 5, 6, 7, 8, 9 and 10 respectively, because all nine admissions were deferred, and a deferred add is a recompute by definition. The column is monotonically non-decreasing in append order, which is the property that makes a lost update between two concurrent mutations detectable after the fact.

Four of the eleven admissions were recomputed after a concurrent mutation landed during their preparation, so this is a normal case on this run rather than an edge case. The 648, 662 and 663 admissions are a different shape again: preparation had already completed and been abandoned before the checkpoint write, so the resume began at the decision step, re-derived the radius from the committed plan, and guarded the write on `recolor_generation`, `current_cohort`, the unstarted set and the pinned set all being unmoved. That shape is now the most common on this run, and in the 662 and 663 cases a committed preflight-clearance artifact on the item branch reduced the diagnosis to reading one file, where the 648 case had required opening the dead child's untracked checkpoint. Each of those admissions deferred its checkpoint write until preparation returned, then re-read the checkpoint and re-derived the decision inputs. The 670, 678 and 287 admissions additionally guarded the write itself: each aborted rather than wrote if `recolor_generation`, `current_cohort`, the item count or the mutation count had moved between the decision and the write. The 287 admission found the checkpoint unmoved across a preparation lasting roughly forty-five minutes, which is the first such window on this run to close without a concurrent mutation; the guard was still evaluated rather than skipped, because a quiet window is only knowable after the fact.

## Drift Events

None recorded.
