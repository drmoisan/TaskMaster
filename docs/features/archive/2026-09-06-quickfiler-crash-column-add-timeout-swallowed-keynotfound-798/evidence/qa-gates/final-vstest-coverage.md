# P8-T4 — Final QC toolchain step 4 of 4: full-suite test run with coverage

Timestamp: 2026-09-07T05-44
Toolchain pass: 1

Host-specific absolute paths, user account names and machine names are redacted to `<worktree>`,
`<vs-install>`, `<user>` and `<machine>` tokens.

## Assembly discovery

Discovery repeated the P0-T9 method exactly: `Get-ChildItem -Path . -Recurse -Filter '*.Test.dll'`
from `<worktree>`, keeping only paths matching `\bin\Debug\` and not matching `\obj\`, `\ref\`, or a
dot-claude directory segment.

The dot-claude exclusion is evaluated against the path **relative to the worktree root**, not against
the absolute path. This worktree is itself located beneath a dot-claude directory segment, so
evaluating the exclusion against the absolute path would match every candidate, return an empty
assembly list, and make this gate vacuous by reporting zero assemblies. Evaluated against the
relative path it performs its intended function of rejecting stale build outputs inside any nested
agent worktree.

18 files matched `*.Test.dll` in total; 9 survived the filter, the same nine as P0-T9:

```
QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
SVGControl.Test\bin\Debug\SVGControl.Test.dll
Tags.Test\bin\Debug\Tags.Test.dll
TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
TaskTree.Test\bin\Debug\TaskTree.Test.dll
TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

## Command

Command: dotnet-coverage collect --output coverage\final.cobertura.xml --output-format cobertura --settings coverage.config -- `<vstest>` `<the nine assemblies above>` /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p8-final /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"

`<vstest>` was resolved inline by vswhere to
`<vs-install>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`. The working directory was
`<worktree>`.

The `/TestCaseFilter:` value carries the extension
`&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser`
because P0-T8 recorded the verdict `SHELL_ICON_EXCLUSION: REQUIRED`. That exclusion is environmental
and those classes are covered by CI.

`/InIsolation` is present and is mandatory. Without it vstest runs in-process and never loads each
assembly's configuration file, so binding redirects are ignored and assemblies fail to load; the
signature is an empty error message with a sub-millisecond duration across many tests, which is an
assembly-load failure rather than a regression.

EXIT_CODE: 0
ExpectedExitCode: 0

## Counts read from the produced TRX

TRX: `coverage\trx\p8-final\<user>_<machine>_2026-09-07_05_42_38_net481.trx`, the only TRX in that
directory.

- total: 7048
- executed: 7048
- passed: 7048
- failed: 0
- notExecuted (skipped): 0
- error: 0
- aborted: 0
- timeout: 0
- inconclusive: 0
- ResultSummary outcome: Completed

Total run time 58.3973 seconds. The console reported `Test Run Successful.` A successful vstest run
prints no `Failed:` and no `Skipped:` line, so the failed and skipped figures above are read from the
TRX counters rather than inferred from absent console text.

Comparison with the P0-T9 baseline: total rose from 7023 to 7048, an increase of 25, which is the
count of tests this change adds. Passed rose by the same 25. Failed and skipped are 0 on both sides.

## Issue #780 rerun accounting

Rerun count: **0**.

`DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` is a known sporadic failure under
high-worker coverage runs, tracked as issue #780. This task's acceptance requires a failed count of 0,
so a failure of that test would require a rerun of the whole task rather than acceptance. It did not
fail in this run, and the failed count is 0, so no rerun was needed and none was performed.

## Coverage document

The collector wrote `coverage\final.cobertura.xml` (38,698,976 bytes). The `coverage` directory is
gitignored, so a sanitised copy was written to
`docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml`
(38,176,529 bytes) and placed in the index with `git add --intent-to-add`.
`git ls-files --error-unmatch` succeeds for that path.

Document-level attributes, read from the sanitised evidence copy:

- line-rate: 0.8586876295435013
- lines-covered: 169857
- lines-valid: 197810
- branch-rate: 0.6611926319075866
- branches-covered: 21178
- branches-valid: 32030
- complexity: 36886

The `line-rate` attribute is a fraction between 0 and 1, not a percentage.

### Sanitisation applied to the evidence copy

The collector emits each `class` element's `filename` attribute as an **absolute** path containing
the executing account name. The evidence copy therefore has the worktree root prefix and its trailing
separator removed, which makes every `filename` attribute repository-relative with backslash
separators. No other transformation was applied: element structure, every `line` element, every
`hits` value and every rate attribute are as the collector produced them, and the document-level
attributes above were read from the sanitised copy.

That single transformation serves two purposes at once. It satisfies the no-absolute-host-paths rule,
and it produces exactly the repository-relative backslash form that P8-T5 matches against by
`filename` suffix. A forward-slash suffix would match no element in this document, would record every
path as `NOT INSTRUMENTED`, and would fail the first two clauses of P8-T6 against a correct change.

Residual host-token verification against the sanitised copy:

- occurrences of the account name: 0
- occurrences of the machine name: 0
- occurrences of any `C:\Users\` drive-rooted path: 0
- occurrences of the worktree root string: 0

Output Summary: The full-suite final run exited 0. TRX counters are total 7048, executed 7048, passed
7048, failed 0, skipped 0. The document-level Cobertura line-rate is 0.8586876295435013 as a decimal
fraction, over 169857 covered of 197810 valid lines, up from the baseline 0.8582705126774282 over
169184 of 197122. The issue #780 sporadic test did not fail and the rerun count is 0.
