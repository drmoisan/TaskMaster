# QA Gate — Step 4 Full-suite MSTest with coverage, post-base-merge pass

Timestamp: 2026-08-28T00-14

Command: `pwsh -NoProfile -File ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/postchange.cobertura.xml`

EXIT_CODE: 0

Output Summary: **Test Run Successful. Total tests: 6745, Passed: 6745, Failed: 0** in 47.72
seconds across 9 test assemblies. Repository `line-rate` **85.1494%**, `branch-rate` **79.1998%**
(`lines-covered` 54545 / `lines-valid` 64058; `branches-covered` 12965 / `branches-valid` 16370).
Both clear the uniform `>= 85%` line and `>= 75%` branch floors.

The 6745 total is 15 higher than the 6730 recorded before this base merge, which is accounted for
by sibling 476's `Viewers\WebView2BreadcrumbHostContractTests.cs` and
`Viewers\WebView2BreadcrumbHostTests.cs` arriving with the integration tip.

The wrapper applies `/TestCaseFilter:TestCategory!=LiveOutlook` and the `\.claude\` worktree
exclusion, matching the CI invocation. No `[Ignore]` attribute was added or edited.

## Recorded environmental re-run

The first attempt at this step exited `-1` after 1694 passing tests with **no failing test and no
run summary at all** — the run aborted rather than reporting a failure. Only 5 of the 9 assemblies
had reached the parallelization-start line. Three facts support the environmental reading:

1. every test line emitted before the abort read `Passed`; the log contains zero `Failed` lines;
2. no `Test Run Successful`/`Test Run Failed` summary was written, which is the signature of an
   aborted host rather than a failing assertion;
3. two sibling children (464 and 489) were building concurrently in adjacent worktrees on this
   machine, and a concurrent `Rebuild` that replaces `bin\Debug` output mid-run will abort a
   vstest session in exactly this way.

ONE re-run was taken on the byte-identical tree and is the run recorded above: 6745/6745 green.
No source file was modified between the two attempts, so the toolchain loop did not restart.

## Toolchain loop closure

Formatting, analyzers, nullable analysis, and tests all passed in this single ordered pass, and no
step rewrote a file. `git status --porcelain` is empty after the pass (the `coverage/` output is
gitignored), so no restart from step 1 was required.
