# Phase 6 — Post-Change Coverage Conversion and Delta (P6-T6)

Timestamp: 2026-09-08T08-24

Command: `pwsh -NoProfile -Command 'Get-ChildItem -Path coverage/plan812/p6-t5 -Filter *.coverage -Recurse | Resolve-Path -Relative'`

EXIT_CODE: 0

Command: `dotnet-coverage merge coverage/plan812/p6-t5/**/*.coverage --output coverage/plan812/p6-t5/coverage.cobertura.xml --output-format cobertura`

EXIT_CODE: 0

Command: `pwsh -NoProfile -Command '[xml]$c = Get-Content "coverage/plan812/p6-t5/coverage.cobertura.xml"; "{0} {1} {2}" -f $c.coverage."line-rate", $c.coverage."lines-covered", $c.coverage."lines-valid"'`

EXIT_CODE: 0

Output Summary:

## Enumerated attachments

Enumerated attachment count: **2**, which is at least 1.

Enumerated repository-relative paths, with two token classes redacted:

- `.\coverage\plan812\p6-t5\2b2f7172-191e-42f8-8263-ca3782fce87f\[REDACTED-ACCOUNT]_[REDACTED-MACHINE]_2026-09-08.08_19_29.coverage`
- `.\coverage\plan812\p6-t5\[REDACTED-ACCOUNT]_[REDACTED-MACHINE]_2026-09-08_08_18_57\In\[REDACTED-MACHINE]\[REDACTED-ACCOUNT]_[REDACTED-MACHINE]_2026-09-08.08_19_29.coverage`

The redaction is required by this task for the same reason P0-T12 gives: the repository-relative spelling alone is not sufficient, because the test platform names each attachment and its containing directory `<account>_<machine>_<timestamp>`, so the account and machine tokens sit inside the relative path's own segments and not only in an absolute prefix. Recording either form verbatim would write a host token into a committed artifact, which D4 forbids and which the P6-T15 contents sweep would report. The path structure and the count are preserved so the enumeration stays auditable. The unredacted values remain readable in the git-ignored `coverage/plan812/p6-t5/` tree.

Attachment-count comparison, required to detect a blended collection:

- Count observed at P0-T12: **2**
- Count observed here: **2**
- The two counts are **equal**.

An increase would have indicated that `coverage/plan812/p6-t5` held attachments from more than one collection — either a scoped re-run misdirected into this tree or an earlier P6-T5 attempt whose results were not cleared — and the merge would then have blended two collections, invalidating parts (a) through (d) rather than part (c) alone. No such increase occurred. Consistently, P6-T5 recorded `PRE-CLEAR-EXISTS: False`, so no earlier attempt's tree existed, and no scoped flake re-run was performed at all because the full-suite failed count was 0.

No matched `filename` attribute value is transcribed anywhere in this artifact: `dotnet-coverage` writes `filename` as an absolute Windows path carrying the account name.

## Root figures

| Figure | P0-T12 baseline | P6-T6 post-change | Delta |
| --- | --- | --- | --- |
| `line-rate` | 0.7363901154028049 | **0.7367916823028189** | +0.0004015669000140 |
| `lines-covered` | 166609 | **166887** | +278 |
| `lines-valid` | 226251 | **226505** | +254 |

## (a) New accessor file coverage

Pattern: `(^|[\\/])FolderPredictor\.ArchiveRoot\.cs$`, matched against the trailing path segments only, because `dotnet-coverage` writes `filename` as an absolute Windows path with backslash separators and a repository-relative forward-slash spelling would match nothing.

- Matched `<class>` element count: **1**, which is at least 1.
- Sum of `<line>` elements across those class elements: **19**
- Sum of those with `hits` greater than or equal to 1: **19**
- Aggregate line rate: **1.000000**

1.000000 is greater than or equal to 0.90, so the `>= 90%` new-module target is met. The aggregate is computed as covered lines over total lines across every matching class element, so more than one matching element would not have made the reading ambiguous; here exactly one matched.

## (b) Changed-line coverage on the latch file

Pattern: `(^|[\\/])StoreWrapperController\.Display\.cs$`, matched on trailing path segments for the same reason.

- Matched `<class>` element count: **2**, which is at least 1. A partial class is emitted as one or more `<class>` elements per source file, and every match is unioned.
- Union of `<line>` numbers those elements carry: 73 lines.
- Added and modified line numbers reported by `git diff origin/main...HEAD -U0` for `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs`: 14.
- **Intersection size: 6**, which is at least 1. The intersecting line numbers are 48, 49, 50, 51, 52 and 54.
- Number of lines in that intersection whose `hits` is 0: **0**.

The intersection size condition is what makes this reading non-vacuous. P4-T2 adds the statement `_userEmailRetryAttempted = true;`, which is an executable line, so an intersection of size 0 would have meant the coverage document was not matched rather than that nothing executable changed, and the hits condition would then have been satisfied without reading a single line. The intersection is non-empty and every line in it is covered, so no gap has to be closed and this phase is not restarted.

The 14 changed lines exceed the 6 in the intersection because 8 of them are comment lines added by P5-T2 and the multi-line `if` condition's continuation lines, which Cobertura does not emit `<line>` elements for.

For completeness, the same computation over `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.ArchiveRoot.cs`, which is a new file so every one of its 81 lines is an added line: intersection size 19, of which 0 have `hits` of 0.

## (c) Repository comparison — Branch A

The comparison is recorded under exactly one named branch, and that branch is **Branch A**.

Branch selection: the post-change `lines-valid` of 226505 differs from the P0-T12 `lines-valid` of 226251 by 254. One percent of the baseline figure is 2262.51, and 254 is at most 2262.51, so the two rates were computed over comparable instrumented denominators and Branch A applies.

Branch A condition: the post-change root `line-rate` must be at least the baseline `line-rate` less 0.005.

- Threshold: 0.7363901154028049 − 0.005 = 0.7313901154028049
- Observed: 0.7367916823028189

0.7367916823028189 is greater than or equal to 0.7313901154028049, so the no-regression condition holds. The rate in fact rose rather than merely holding.

Branch B is not applicable and is not recorded.

## (d) Post-change repository line percentage

Post-change repository line percentage: **73.679**.

That figure is below 80. Per this task's condition, the reason it is not attributable to this change is cited from the P0-T12 baseline artifact, which carries the line `PRE-EXISTING SUB-80 BASELINE: YES` against a measured `BASELINE-REPOSITORY-LINE-PERCENT:` of 73.639. The sub-80 condition was therefore present before this plan edited any source file, and this change moved the figure upward by 0.040 percentage points rather than downward.

The post-change root `line-rate`, `lines-covered`, and `lines-valid` are also appended to the `Output Summary:` section of `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/qa-gates/p6-t5-vstest.2026-09-07T22-12.md`, on its `POST-CHANGE-COVERAGE-HEADLINE:` line, because the final-QC test-step artifact is required to carry the numeric coverage headline itself.
