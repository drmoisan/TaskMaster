# Final QA loop, toolchain step 4 — confirming CI-verbatim run (issue #826, [P7-T5])

Timestamp: 2026-09-09T19-42

[P7-T4] is the measured run; this run confirms that the CI-shaped invocation passes and that the two runs
agree on result counts.

Command:

```
<vstest.console.exe> <the nine test assemblies> /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p7-t5.trx" /ResultsDirectory:coverage/826-raw/p7-t5 /TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser
```

`<vstest.console.exe>` is resolved through `vswhere`. Run as one `pwsh -NoProfile -Command` block
carrying the plan's C2 preamble branch guard. This is the CLAUDE.md step-4 shape,
`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`, with two additions recorded below.

The nine assemblies are `QuickFiler.Test`, `SVGControl.Test`, `Tags.Test`, `TaskMaster.Test`,
`TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test` and `VBFunctions.Test`,
each as `<project>\bin\Debug\<project>.dll`.

Literal filter string used:

```
TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser
```

EXIT_CODE: 0

## TRX counters

Read from `coverage/826-raw/p7-t5/p7-t5.trx`, `ResultSummary/Counters`:

| Counter | This run | [P7-T4] | Equal |
|---|---|---|---|
| total | 7192 | 7192 | yes |
| executed | 7192 | 7192 | yes |
| passed | 7192 | 7192 | yes |
| failed | 0 | 0 | yes |
| notExecuted | 0 | 0 | yes |

The total, passed and notExecuted counts equal the corresponding [P7-T4] counts, and failed is 0.

## `/InIsolation` and the TestCaseFilter are environmental necessities, not scope reductions

Both are recorded properties of this host rather than weakenings of the gate:

- `/InIsolation` runs the tests in a separate process, which this repository's local runs require for the
  test assemblies to load correctly.
- The filter excludes the shell-icon classes (`ShellUtilities`, `SysImageListHelper`, `OSBrowser`), which
  stall `vstest` on this machine through `SHGetFileInfo`, and the `LiveOutlook` category, which requires a
  live Outlook process. CI runs the suite unfiltered on a clean runner and covers those classes there.

No test that this feature touches, and no test in the write-set projects other than those categories, is
excluded by the filter.

Output Summary: the CI-verbatim invocation exits 0 with 7192 of 7192 tests passing and 0 failed, and its
TRX counters agree exactly with the measured [P7-T4] run. The raw TRX and the `.coverage` attachment stay
under the gitignored `coverage/826-raw/` directory and are not committed, because the attachment filename
embeds the account name and the machine name and the TRX records them in `runUser=` and `computerName=`.
