# P4-T17 — Code commit and post-commit scope assertion

Timestamp: 2026-09-13T03-22

Command: `git -C . add` with exactly the seven pathspecs the plan names, followed by a single `git -C . commit -m "fix(#838): surface table acquisition timeout and cancellation instead of null"`, then `git -C . rev-parse HEAD`, then `git -C . status --porcelain --untracked-files=all -- . ":(exclude).claude/agent-memory" ":(exclude)docs/features/potential"`.

The seven staged pathspecs were:

```
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.Failures.cs
UtilitiesCS/UtilitiesCS.csproj
UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncFailureContractTests.cs
UtilitiesCS.Test/UtilitiesCS.Test.csproj
UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/
```

No analyzer-path change is staged, because P0-T14 edited no tracked file under any branch: its containment was performed entirely inside the gitignored package directory and its before and after listings were byte-identical.

EXIT_CODE: 0

CODE-COMMIT-SHA: 3d680a4fcc498833bda95429331677b6c08d3e01

The commit reports 17 files changed, 363 insertions and 2 deletions. Four of the six code and project files were already committed at earlier phase boundaries in this run, so this commit carries their residual state together with the sixteen qa-gate evidence artifacts written in Phase 4.

## Post-commit porcelain listing, verbatim

```
 M docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/plan.2026-09-12T16-09.md
```

Output Summary: all three acceptance clauses hold. The commit exited 0, the new head identifier is recorded as `CODE-COMMIT-SHA:`, and the post-commit porcelain listing contains no entry whose path lies outside `docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/`. The single entry is this plan file, modified because the executor has been checking off completed tasks as it goes.

The listing does not hold `spec.md`, `issue.md`, `user-story.md` or the research record as untracked entries, which is the state P0-T8 predicted: its artifact recorded that the five feature documents were already TRACKED at the base commit, all five appearing in the merge-base-to-HEAD name-listing diff and none as an untracked porcelain entry. The exact membership of this listing is deliberately not asserted by the plan and no entry count is asserted, because the membership is state-dependent; what is asserted, and what holds, is that nothing outside the feature folder appears. An entry outside it would have meant the commit left a tracked source, test or project file dirty, or created a file this plan does not own.
