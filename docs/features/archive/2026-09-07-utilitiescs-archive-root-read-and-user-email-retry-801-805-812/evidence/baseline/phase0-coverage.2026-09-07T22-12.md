# Phase 0 — Baseline Coverage Conversion (P0-T12)

Timestamp: 2026-09-08T06-51

Command: `pwsh -NoProfile -Command 'Get-ChildItem -Path coverage/plan812/p0-t11 -Filter *.coverage -Recurse | Resolve-Path -Relative'`

EXIT_CODE: 0

Command: `dotnet-coverage merge coverage/plan812/p0-t11/**/*.coverage --output coverage/plan812/p0-t11/coverage.cobertura.xml --output-format cobertura`

EXIT_CODE: 0

Command: `pwsh -NoProfile -Command '[xml]$c = Get-Content "coverage/plan812/p0-t11/coverage.cobertura.xml"; "{0} {1} {2}" -f $c.coverage."line-rate", $c.coverage."lines-covered", $c.coverage."lines-valid"'`

EXIT_CODE: 0

Output Summary:

Enumerated attachments, count: 2, which is at least 1. Both attachments are the same collected data written to two locations by the test platform, and `dotnet-coverage merge` reported including both.

Enumerated repository-relative paths, with two token classes redacted:

- `.\coverage\plan812\p0-t11\0f953e32-2c6c-4b11-a2b0-aba11a33b18f\[REDACTED-ACCOUNT]_[REDACTED-MACHINE]_2026-09-08.06_36_15.coverage`
- `.\coverage\plan812\p0-t11\[REDACTED-ACCOUNT]_[REDACTED-MACHINE]_2026-09-08_06_35_42\In\[REDACTED-MACHINE]\[REDACTED-ACCOUNT]_[REDACTED-MACHINE]_2026-09-08.06_36_15.coverage`

The redaction is mandated by P0-T12, which states that the repository-relative form is not sufficient on its own and requires each account and machine token to be replaced with a fixed placeholder while the remaining path structure and the count stay intact. The reason the task gives is the reason recorded here: the test platform names each `.coverage` attachment `<account>_<machine>_<timestamp>.coverage` and places one of them under a directory named on the same pattern, so the account token sits inside the relative path's own leaf and directory segments, not only in the absolute prefix. Transcribing the relative form verbatim would therefore write the account token into a committed evidence artifact, which D4 forbids and which the P6-T15 contents sweep of the evidence subtree would report as a non-zero match. The paths are recorded above with those two token classes replaced so that the enumeration and its count remain auditable while the sweep can still reach zero. The unredacted values remain readable in the git-ignored `coverage/plan812/p0-t11/` tree.

Root Cobertura figures read from `coverage/plan812/p0-t11/coverage.cobertura.xml`:

- `line-rate`: 0.7363901154028049
- Same value as a percentage: 73.639%
- `lines-covered`: 166609
- `lines-valid`: 226251

BASELINE-REPOSITORY-LINE-PERCENT: 73.639

PRE-EXISTING SUB-80 BASELINE: YES

The measured baseline repository line percentage of 73.639 is below 80. It is a pre-existing repository condition observed before this plan modified any source file, and it is not attributable to this change. The branch at the time of measurement carried only the preparation commit, the merge of `origin/main`, and one plan-correction commit; no file in this plan's Write Set had been edited.

The same three numeric values are also appended to the `Output Summary:` section of `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/baseline/phase0-vstest.2026-09-07T22-12.md`, because the baseline test-step artifact is required to carry the numeric coverage headline itself.
