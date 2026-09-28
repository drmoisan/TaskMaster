# Final QA Gate 6 — No Raw Test-Result or Coverage Document in Evidence (issue #742, [P5-T6])

Timestamp: 2026-09-14T02-25

Command: `pwsh -NoProfile -Command '$m = git status --porcelain --untracked-files=all -- docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/evidence | Select-String -Pattern "\.trx$|\.cobertura\.xml$"; $m; if ($m) { exit 0 } else { exit 1 }'`

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary: the command printed no line and exited 1. No file carrying the `.trx` extension and
no Cobertura coverage XML file appears, tracked or untracked, anywhere under this feature's evidence
directory.

Acceptance: satisfied, read per this plan's zero-match reading convention. The command is written so
that exit 0 means a prohibited file was found and exit 1 means none was, which is why the expected
exit code here is 1 rather than 0.

## Where the raw documents went instead

Both coverage-collecting tasks wrote their raw output to `coverage\coverage.cobertura.xml` under the
repository's `coverage\` directory, transcribed the needed figures into a Markdown artifact, and
deleted the raw file:

- [P0-T8] — figures in `../baseline/vstest-coverage-baseline.2026-09-12T16-09.md`
- [P5-T4] — figures in `vstest-coverage-final.2026-09-12T16-09.md`

`coverage\` is ignored by `.gitignore`'s `coverage/*` entry, so neither the Cobertura document nor
the runner's own `coverage\test-results\mstest-coverage-run.trx` could be committed even if a
deletion step were missed. The two direct `vstest.console.exe` invocations in [P1-T4] and [P4-T4]
used the console logger and produced no trx at all.

This satisfies the maintainer decision on issue #671 and the `## Committed Test Evidence Format`
section of `CLAUDE.md`: committed test evidence is a projection of a tool's output, never the tool's
raw document.

## Search scope for this negative claim

- SearchScope: `docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/evidence`, recursively, including untracked files (`--untracked-files=all`)
- SearchPatterns: `\.trx$` and `\.cobertura\.xml$`
- SearchResult: none
