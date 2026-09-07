# Phase 0 — Full-suite baseline test run with coverage

Timestamp: 2026-09-07T01-00
Task: [P0-T9]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<worktree>`,
`<vs-install>`, `<user>` and `<machine>` tokens.

## Assembly discovery

Discovery ran `Get-ChildItem -Path . -Recurse -Filter '*.Test.dll'` from `<worktree>` and kept only
paths matching `\bin\Debug\` and not matching `\obj\`, `\ref\`, or a dot-claude directory segment.

One mechanical detail is load-bearing and is recorded here because P8-T4 repeats this discovery.
This worktree is itself located beneath a dot-claude directory segment, so the dot-claude exclusion
must be evaluated against the path **relative to the worktree root**, not against the absolute path.
Evaluated against the absolute path the exclusion matches every candidate and the discovery returns
an empty assembly list, which would make the test gate vacuous. Evaluated against the relative path
it performs its intended function of rejecting stale build outputs inside any nested agent worktree.

18 files matched `*.Test.dll` in total; 9 survived the filter:

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

Command: dotnet-coverage collect --output coverage\baseline.cobertura.xml --output-format cobertura --settings coverage.config -- `<vstest>` `<the nine assemblies above>` /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p0-baseline /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"

`<vstest>` was resolved inline by vswhere to
`<vs-install>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`. The working directory was
`<worktree>`.

The `/TestCaseFilter:` value carries the extension
`&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser`
because P0-T8 recorded the verdict `SHELL_ICON_EXCLUSION: REQUIRED`.

EXIT_CODE: 0
ExpectedExitCode: 0

## Counts read from the produced TRX

TRX: `coverage\trx\p0-baseline\<user>_<machine>_2026-09-07_00_57_32_net481.trx`, the only TRX in that
directory.

- total: 7023
- executed: 7023
- passed: 7023
- failed: 0
- notExecuted (skipped): 0
- aborted: 0
- timeout: 0

Total run time 54.4729 seconds. The console reported `Test Run Successful.` A successful vstest run
prints no `Failed:` and no `Skipped:` line, so the failed and skipped figures above are read from
the TRX counters rather than from console text.

## PREEXISTING_FAILURE_SET

PREEXISTING_FAILURE_SET: empty.

No test failed in this run. In particular the issue #780 sporadic failure
`TryAddValuesAsync_UpdatesExistingValue` did not occur in this run, and the shell-icon classes
identified by P0-T8 were excluded by the filter and did not execute.

The `BASELINE NOT GREEN` condition defined by this task's acceptance is therefore **not** recorded.
The baseline is green, so P8-T4's acceptance of a failed count of 0, and AC14's requirement of a
clean final run, are both reachable without a separate remediation.

## Coverage document

The collector wrote `coverage\baseline.cobertura.xml` (38,559,748 bytes). The `coverage` directory is
gitignored, so a sanitised copy was written to
`docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/coverage-baseline.cobertura.xml`
and placed in the index with `git add --intent-to-add`. `git ls-files --error-unmatch` succeeds for
that path.

Document-level attributes, identical in the collector output and in the evidence copy:

- line-rate: 0.8582705126774282
- lines-covered: 169184
- lines-valid: 197122
- branch-rate: 0.6605220330495744
- branches-covered: 21105
- branches-valid: 31952

The `line-rate` attribute is a fraction between 0 and 1, not a percentage.

### Sanitisation applied to the evidence copy

The collector emits each `class` element's `filename` attribute as an **absolute** path containing
the executing account name. Committing that verbatim would embed absolute host paths in an evidence
artifact. The evidence copy therefore has the worktree root prefix removed, which makes every
`filename` attribute repository-relative with backslash separators. No other transformation was
applied; the element structure, all `line` elements, all `hits` values and every rate attribute are
byte-for-byte as the collector produced them, and the document-level attributes above were re-read
from the sanitised copy and match.

Two consequences are worth recording:

- The sanitised form is exactly the form P0-T10 and P8-T5 assume when they match `class` elements by
  `filename` suffix, so the aggregation rule applies unchanged. Suffix matching would also have
  worked against the unsanitised absolute form, so the sanitisation does not alter any measurement.
- Verification after sanitisation found 0 residual occurrences of the account name, 0 of the machine
  name and 0 of any drive-rooted user path in the evidence copy. No path referencing the ancestor
  checkout or the user profile was present in the document at all; every absolute path was the
  worktree root.

Output Summary: The full-suite baseline run exited 0. TRX counters are total 7023, executed 7023,
passed 7023, failed 0, skipped 0. The document-level Cobertura line-rate is 0.8582705126774282 as a
decimal fraction, over 169184 covered of 197122 valid lines. PREEXISTING_FAILURE_SET is empty and
the `BASELINE NOT GREEN` condition is not recorded: the baseline is green.
