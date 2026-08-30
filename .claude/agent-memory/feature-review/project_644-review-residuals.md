---
name: 644-review-residuals
description: "#644 navigation key ledger, both cycles: PASS/0 blocking; AC-16 PARTIAL twice (two-decimal >= undecidable, 0.028pt noise vs 0.011pt shortfall); fixing a defect class instance-by-instance left a 4th instance nobody swept for; escalated analyzer HintPath skew disproved via CI-at-merge-base"
metadata:
  type: project
---

Issue #644 (`bug/qfc-unregister-navigation-count-mismatch-orphan-644`) reviewed 2026-08-29T23-06.
Verdict PASS, **0 blocking**, 17/18 ACs checked, AC-16 left unchecked at PARTIAL.

**Why: the run-E/run-F experiment is the cleanest noise proof I have seen in this repo.** Two
Cobertura runs on a *byte-identical* tree, same command, same machine, no intervening edit:
54793 vs 54811 covered of an invariant 64221 valid. That is 18 covered lines = 0.0280 pt spread,
against a 0.0109 pt shortfall the gate was asked to adjudicate, and the two runs **straddle** the
baseline (85.3303%). Verdict was PARTIAL, not FAIL: a FAIL asserts a regression, and the evidence
supports that no more than its converse. Reusable adjudication line: *a comparison whose sign is
determined by which of two equally valid measurements of an identical tree happens to be taken is
not adjudicating the property it was written to adjudicate.* Corroborates
[[csharp-coverage-constants-nondeterministic]] (#511's ~0.015 pt band) by a cleaner experiment.
Residual risk was called acceptable because the **absolute** floors (85% line / 75% branch) are
decidable at this resolution and pass on both runs with 0.32 pt and 4.29 pt margin.

**`[ExcludeFromCodeCoverage]` reasoning — the half that is wrong.** "The changed file is excluded,
therefore this change cannot move the repository figure" is sound for *direct* movement (the file
appears in 0 of 558 `<class>` entries; `lines-valid` invariant at 64221 across an 18/-9 edit is the
mechanical proof) and **overstated for total movement**. Exclusion removes the type's own lines, not
its effect on measured callees. Here `KbdActions.cs` and `KaStringAsync.cs` carry no exclusion, and
the change alters how often `Remove` is called plus adds six tests that exercise them. Always grep
the collaborators for the attribute before accepting a "cannot move the figure at all" claim.

**Provenance check that must be run whenever the excluded-class argument appears:** was the
attribute added or widened by this change? Here no — `git show <anchor>:<file> | grep -n
ExcludeFromCodeCoverage` returned line 21 and the anchored diff had no such hunk. If it had been
added, the whole argument is circular and that is a material finding.

**Withdrawn finding, worth remembering:** fully qualified `System.Action` in
`QuickFiler.Test/Controllers/**` is the **house convention** (20+ occurrences across 8 files;
`Microsoft.Office.Interop.Outlook` also defines `Action`). Do not raise it as a style inconsistency.

**Environment traps hit on this review:**
- The review worktree has **no repo-local .NET SDK** — `dotnet tool run csharpier check .` returns
  "The repo-local .NET SDK is missing". Toolchain gates cannot be independently re-run; verify from
  committed evidence.
- The PowerShell scratchpad **production-file cap of 3 was already full**, so no `.ps1` sim script
  could be written. Use inline `pwsh -NoProfile -Command "...\$var..."` (escape `$` for bash).
  Heredoc into `pwsh -Command -` is refused by the worktree-isolation guard as "too complex".
- Session cwd `TaskMaster-wt/2026-08-29T00-11` lacked the feature folder **and** carried a stale
  `artifacts/pr_context.summary.txt` from #638. Mirror all three artifacts *and* the summary, then
  simulate from both cwds. See [[review-worktree-differs-from-session-cwd-mirror-artifacts]].

**Residuals owed at merge / follow-up (none blocking):**
- CR-1 stale XML doc at `QfcCollectionControllerNavigationDigitsTests.cs` lines 192-195 still
  describes "replays the recorded width 1 ... loop bound has grown to ten" — a mechanism this fix
  deleted. That test is the #472-supersession proof, so its doc mattering more than usual.
- PA-7 absolute host path `C:/Users/DanMoisan/repos/...` at `research/research.2026-08-29T07-55.md`
  line 5, committed. Redact before merge; see [[_shared_no_absolute_host_paths]].
- PA-4 overclaim at `p4-t6` lines 136-138; PA-5 recommend an **append-only** SUPERSEDED footer on
  `p4-t6` rather than an edit (correcting forward was endorsed as the right call).
- PA-6: `[P4-T8]` was checked `[x]` although the plan routed its literal-clause failure to
  REMEDIATION-REQUIRED. Substance sound, bookkeeping inconsistent with how the same run handled
  AC-16. Pattern to watch: a checked box with a disclosed failure inside it is easy to miss.
- CR-3 promotion candidate: under a ledger, a `false` from `KbdActions.Remove` becomes an
  unambiguous out-of-band-mutation signal instead of expected noise, and is still discarded.
- Recommended new issue: reformulate the repo coverage no-regression gate with an explicit tolerance
  above the measured noise floor, or as a same-session per-file changed-lines comparison.

## Cycle-2 exit reaudit (2026-08-30T01-46, head `85a1939f`) — PASS / 0 blocking

Elective cycle over CR-1 + PA-7 only. Both **verified closed**. AC-16 re-adjudicated from the raw
facts and independently landed on **PARTIAL again** — an orchestrator override is a decision to
proceed, never a measurement, so it cannot convert an undecidable `>=` into a checked box.

**The lesson worth keeping: a class of defect was fixed instance-by-instance and the class was never
swept.** CR-1 was found at one location; a third instance surfaced only during remediation planning;
my class-scoped sweep then found a **fourth** (line 179, a `.BeEmpty(...)` because-message reading
"the recorded registration width is replayed") that appears in **no** predecessor artifact, plus
CR-2 still standing at line 145. The cycle's acceptance clauses were anchored on *specific literal
fragments* (`grown loop bound reaches`, `regardless of group count`) and its two sweep tasks were
scoped to host identity, so nothing ever swept the file for the class as a class. **When a fix
deletes a named mechanism, grep the whole repo for that mechanism's prose, not for the one string the
finding quoted.** Here: `grep -rn --include=*.cs -iE "recorded (registration )?width|loop bound"`.
Also check past-tense uses are left alone — two `loop bound` hits were correct history.

**Escalated analyzer HintPath skew — judged NON-blocking, and the executor's premise was wrong.**
`UtilitiesCS.csproj` / `VBFunctions.csproj` `Import`+`Error Condition` name Meziantou 3.0.174 while
`Analyzer Include` HintPaths name Meziantou 3.0.156 + Roslynator 4.16.0; `packages.config` declares
3.0.174 / 4.16.1 and **no packages.config anywhere declares the older pair**. Executor claimed "a
cold worktree fails with 10 CS0006". Disproved the *consequence* without disputing the skew:
`gh run list --branch main --limit 6` shows six consecutive `success`, the newest being the merge
base itself. The coldest available environment does not reproduce it, so the local failure had a
local cause (partially-populated gitignored `packages/`). Recipe: **when an escalation asserts "cold
build fails", check CI at the merge base before accepting it.** Disposition: pre-existing, identical
at base, neither csproj in the diff, promote separately.

**Compile-proof substitute for expensive msbuild gates.** Remediation was comment/string-literal
only. Rather than re-run two full-solution rebuilds, prove the edit is *in* the tested binary by
mtime ordering: source `01:25:39` < built test dll `01:34:38` < TRX run start `01:36:52`. Then note
that neither analyzers nor nullable flow analysis reads XML-doc or `because:` string contents. Cheap
and honest; do not just cite the executor's artifact.

**csharpier CAN be re-run here** (contrary to the cycle-1 note above): `dotnet tool run csharpier
check .` returned exit 0 / `Checked 1562 files`. The cycle-1 "no repo-local .NET SDK" trap did not
recur in this worktree. Re-test rather than assuming.

**PowerShell scratchpad production-file cap still full** — could not write a sim script. Dot-source
the hook inline instead, and note `powershell -NoProfile` needs **`-ExecutionPolicy Bypass`** or the
dot-source dies with `UnauthorizedAccess` and the hook function is silently undefined (`OK=` blank,
which reads like a pass). Hook simulated OK=True from **both** cwds after mirroring.

**Runtime-derived host sweep that satisfied the "don't spell the tokens" constraint:** write
`$env:USERNAME; $env:COMPUTERNAME; $env:USERPROFILE` to a scratchpad file, then `grep -rniIf <file>`.
Zero hits across all 66 feature-folder files. Bash refuses `ACCT=$(...)` + grep in one command as
"too complex" under worktree isolation — split it.

Cycle-2 residuals: CR-2 + CR-6 (one defect, 34 lines apart, fix together), CR-3/CR-4/CR-5,
PA-1/PA-2/PA-3/PA-4/PA-5/PA-6 all standing, PA-8 analyzer skew to promote.

**Supersession-vs-revert verification recipe that worked:** read the superseded commit's own diff
(`git show 9494ca35 -- <file>`), identify what the earlier fix actually changed (here: a live
`Digits` re-read inside the loop), then find the earlier fix's regression test and confirm it
survives *unchanged and passing*. `UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_...`
fails under a revert of #472 and passes here, which settles the claim empirically rather than by
reading prose.
