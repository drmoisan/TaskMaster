# quickfiler-coverage-filesize-evidence-debt (Issue #727)

- Date captured: 2026-09-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-coverage-filesize-evidence-debt/ (Issue #727)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #727
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/727
- Last Updated: 2026-09-02
## Summary

Six coverage-, file-size-, and evidence-process debt findings surfaced across the `bugs-638-644-647` parallel-orchestration run's feature reviews. None are code-behavior defects — they are test/coverage debt, a policy gap, and process-tooling hygiene gaps — consolidated into one issue rather than six for the same orchestration-overhead reason as the companion code-defects issue filed alongside this one.

## Environment

- OS/version: Windows 11 Pro (repo default)
- Python version: n/a — C#/.NET Framework 4.8.1 WinForms VSTO add-in plus PowerShell toolchain scripts
- Command/flags used: n/a — findings are from code review and coverage-report inspection
- Data source or fixture: n/a

## Steps to Reproduce

Not applicable — each sub-finding is a static review/coverage-report finding. See "Actual Behavior."

## Expected Behavior

Each sub-finding's expected behavior is stated inline below.

## Actual Behavior

**1. `QfcFormController.EventHandlers.cs` coverage debt (pre-existing).** Sits at 49.41% line coverage, below the modified-file floor. Pre-existing at 45.38% before item #633 touched it; #633 improved it by 4 points with zero uncovered changed lines, but did not close the gap — the remainder is untouched Outlook-interop and WinForms handler code needing a dedicated coverage-uplift pass. *(Source: item #633 review, PR #717.)*

**2. `StoreWrapperController` absent from the Cobertura coverage report.** Entirely absent from the report in both the pre- and post-#287 XML, despite only 2 of its members carrying `[ExcludeFromCodeCoverage]`. Pre-existing, not introduced or fixed by #287. Needs investigation into whether this is a build/instrumentation gap rather than a genuine 0% score. *(Source: item #287 review, PR #716.)*

**3. 500-line file-size ceiling violations across five files (all pre-existing).** Independently flagged across three separate PRs' reviews; none of them regressions introduced by the flagging PR:

| File | Lines | Flagged by |
|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2336 | item #678 |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | 792 | item #678 |
| `QuickFiler/Controllers/QfcQueue.cs` | 505 | item #678 |
| `UtilitiesCS/Threading/TimeOutTask.cs` | 1011 (993 at baseline) | item #285 |
| `QuickFiler/Controllers/EfcFormController.cs` | 1189 | item #662 |

All five shrank or held steady in the PRs that flagged them; none crossed the limit because of those changes. *(Sources: PR #724 item #678 for the first three; PR #715 item #285 for `TimeOutTask.cs`; PR #721 item #662 for `EfcFormController.cs`.)*

**4. COM-bound async members structurally cannot satisfy the 90% new-code coverage floor (policy gap).** `QfcQueue.EnqueueAsync` (0/46 lines) and `LoadControllersViewersAsync` (0/24 lines) cannot reach the >= 90% new-code floor without either a live Outlook window (prohibited by unit-test policy) or an `[ExcludeFromCodeCoverage]` exclusion (prohibited by the same acceptance criterion's own fourth clause). Item #678 recorded this AC as PARTIAL and left it honestly unchecked rather than dispositioning it as a pass — the criterion is self-contradictory as authored for this class of member. The same underlying tension recurs in item #663's spec-authored `AC-15`/`MANUAL_CHECK_DEFERRED` pattern and in finding 2 above (#287's absent-from-coverage-report `StoreWrapperController`). Needs a policy decision: a defined exemption class for COM-bound async members (parallel to the existing VSTO/WinForms exemption classes already ratified elsewhere in policy), or an accepted-permanently-partial disposition. *(Source: item #678 review AC20, PR #724; corroborating context items #287 and #663.)*

**5. Evidence/process hygiene tooling gaps (three, docs/tooling-only).** Several evidence artifacts across the run carry `Timestamp:` fields inconsistent with their own quoted build banners and commit dates — the underlying gates did in fact run (confirmed via recorded elapsed times and artifact mtimes), so no delivery is invalidated, but the stamps can't establish inter-gate ordering and should be trustworthy (item #648). `CLAUDE.md` references `.globalconfig` twice as an analyzer-severity source; that file does not exist — `.editorconfig` is the actual source, a documentation-only fix (item #648). The artifact-hygiene/host-identifier-redaction sweep (tracked generally under issue #671) excludes the plan file from its own residual scan, so a plan file can carry unredacted host identifiers the sweep is supposed to catch everywhere else (item #662). *(Sources: PR #719 item #648; PR #721 item #662.)*

**6. Cobertura evidence files accumulating unbounded in git history.** `origin/main` currently carries well over 200 tracked `.cobertura.xml` files, each up to ~10.6 MB, growing with every feature that runs the coverage toolchain and commits its evidence. Item #648's review additionally found that squash-merging (vs. merge-commit) a PR carrying such files makes the blobs permanently unreachable rather than removing the growth — this run's own orchestrator deliberately chose merge-commit over squash for exactly this reason on later items, but the underlying accumulation is unaddressed regardless of merge method. Several later items already work around this locally by committing package-level JaCoCo projections instead of raw Cobertura output (items #646, #646's PR #718) — this may be the right repo-wide convention to formalize. *(Source: item #648 review, PR #719.)*

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — coverage percentages and file line counts cited inline above are from each originating item's own committed coverage evidence and `wc -l`/file-size audits.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: none of these findings represent live incorrect behavior — they are debt, a policy self-contradiction, and process-hygiene gaps that make evidence and coverage numbers harder to trust over time, not defects that misbehave today.

## Suspected Cause / Notes

Each finding traces to a specific PR/item, cited inline above. Finding 4 (COM-bound coverage floor) is the one item here that needs a maintainer policy decision rather than a mechanical fix — flag it for that discussion specifically rather than assuming a "just write more tests" resolution, since the analysis shows that path is blocked by policy itself (no live-Outlook window, no exclusion attribute).

## Proposed Fix / Validation Ideas

- [ ] Dedicated coverage-uplift pass on `QfcFormController.EventHandlers.cs`
- [ ] Investigate why `StoreWrapperController` is absent from the Cobertura report (build/instrumentation gap vs. genuine 0%)
- [ ] Split the five over-500-line files listed above
- [ ] Maintainer decision on a COM-bound-async coverage exemption class (or accept permanently-partial AC disposition) — affects `QfcQueue`, `StoreWrapperController`, and item #663's spec pattern
- [ ] Fix evidence-artifact timestamp accuracy; correct the `.globalconfig` → `.editorconfig` reference in `CLAUDE.md`; extend the host-identifier-redaction sweep to include plan files
- [ ] Define and adopt a Cobertura evidence retention/pruning policy, or formalize the JaCoCo-projection convention already used ad hoc by some items, repo-wide

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
