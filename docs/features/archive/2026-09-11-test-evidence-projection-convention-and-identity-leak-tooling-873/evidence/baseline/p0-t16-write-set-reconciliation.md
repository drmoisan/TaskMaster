# P0-T16 — Footprint Declaration Reconciliation

Timestamp: 2026-09-13T05-05
Task: [P0-T16]

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; <read spec.md, isolate the "## Write Set" section, count lines that are exactly one backticked token, count non-blank non-entry lines, count occurrences of each named test file>'
EXIT_CODE: 0

Verification was performed by reading `spec.md` and by counting over its own text, not by consulting
the plan.

## Before state

The `## Write Set` section occupies the lines between the `## Write Set` heading and the following
`## Identifier Corrections` heading.

BACKTICKED_ENTRY_COUNT_BEFORE: 22
PROSE_LINE_COUNT_BEFORE: 0
MAIN_TESTS_ENTRY_COUNT_BEFORE: 1
ASSEMBLYDISCOVERY_ENTRY_COUNT_BEFORE: 1

`tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` and
`tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` were each already
present exactly once as backtick-delimited paths, so no entry was added; adding either again would
have taken the section to twenty-four.

AC22 states a test-file count of seven: its closing clause reads `A review of the seven test files in
the Write Set confirms no test creates, writes or deletes a file on disk and no fixture is loaded
from a path.` No criterion states a test-file count of four. A search of the whole spec for
`four test files` returns no match; the only occurrence of the word four in a count role is in the
Assumptions section, which decomposes the seven test files as four new and three existing.

## Change made

Exactly one sentence was added immediately beneath the Write Set list, where no prose was present
before. No Write Set entry was added, removed or reworded. The added sentence is:

```
Two of the test-file entries above are forced by the builder signature change rather than chosen: `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` pins the complete argument array handed to the plain wrapper seam as an exact four-element assertion, which becomes a six-element array once the plain builder gains the results-directory and log-file-name switches, and `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` declares an explicit five-parameter mock body for `Invoke-DotnetCoverageCollection`, which stops binding once that function gains two parameters.
```

## After state

BACKTICKED_ENTRY_COUNT_AFTER: 22
PROSE_LINE_COUNT_AFTER: 1
MAIN_TESTS_ENTRY_COUNT_AFTER: 1
ASSEMBLYDISCOVERY_ENTRY_COUNT_AFTER: 1

## Output Summary

The Write Set section lists exactly twenty-two entries, unchanged by this task. Both named entries are
present exactly once each. AC22 states a test-file count of seven. One sentence naming both forcing
mechanisms — the exact-array assertion in the first file and the explicit five-parameter mock body in
the second — is now present beneath the Write Set list. The only edit this task made to `spec.md` is
that single added sentence plus the blank line separating it from the list.

EXIT_CODE: 0
