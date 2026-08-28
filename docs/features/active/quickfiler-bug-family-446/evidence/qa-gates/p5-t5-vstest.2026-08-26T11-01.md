# [P5-T5] Test Gate — ACCEPTED PASS

Timestamp: 2026-08-26T11-01

Task: [P5-T5]
Feature: docs/features/active/quickfiler-bug-family-446

Command: `& $vstest $asm /InIsolation /EnableCodeCoverage "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /Logger:trx "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\qa-gates\p5-t5"`
EXIT_CODE: 0

Executed through `pwsh -NoProfile`. `$vstest` was resolved at run time with the plan's vswhere
prelude to `<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
(`VSTest version 18.8.0 (x64)`).

This is the second pass of the Phase 5 loop. The first pass of this task failed and is recorded in
`p5-t5-vstest.2026-08-26T10-56.md`; the correction was confined to the executor's transcription of
the discovery prelude's regular expression and changed nothing in the repository.

## Discovered assemblies

The Command-conventions discovery prelude reported `ASM_COUNT=9` and produced these nine
workspace-root-relative paths:

1. `.\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
2. `.\SVGControl.Test\bin\Debug\SVGControl.Test.dll`
3. `.\Tags.Test\bin\Debug\Tags.Test.dll`
4. `.\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll`
5. `.\TaskTree.Test\bin\Debug\TaskTree.Test.dll`
6. `.\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll`
7. `.\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll`
8. `.\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`
9. `.\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll`

**None of the nine recorded paths contains a `.claude` segment when expressed relative to the
workspace root**, satisfying that acceptance condition. The exclusion is applied to the relative
path and not the absolute one because the workspace root itself lies beneath a `.claude`
directory; an absolute-path test would exclude every assembly and produce a vacuously green run.
The count matches the nine assemblies recorded by the `[P0-T12]` baseline exactly.

## Test counts

- Total: `6501`
- Passed: `6501`
- Failed: `0`
- Skipped: `0`

Reproduced from the runner: `Test Run Successful.` / `Total tests: 6501` / `Passed: 6501` /
`Total time: 52.1997 Seconds`. The runner prints no `Failed:` or `Skipped:` row when those counts
are zero. The recorded total is greater than zero, satisfying that acceptance condition.

The TRX `ResultSummary/Counters` element independently confirms the counts:
`total="6501" executed="6501" passed="6501" failed="0" error="0" timeout="0" aborted="0"
inconclusive="0" notExecuted="0"`. Every one of the 6501 `UnitTestResult` elements carries
`outcome="Passed"`; the TRX contains zero `RunInfo` elements, so no run-level error or warning was
recorded.

## Failed-test set

(empty)

`EXIT_CODE: 0` with a recorded failed count of `0`, so this task completes on its **primary**
branch. The pre-existing-baseline reconciliation branch is not taken and is not needed: `[P0-T12]`
recorded an empty failed set, so there was no reconciliation set available in the first place.

## Relationship to the `[P0-T12]` baseline total

The baseline recorded 6482 tests; this run records 6501, a net increase of 19. The increase is the
regression tests this change set added across Phases 1 through 3 to
`QuickFiler.Test`. No test was removed and no test regressed: both runs report a failed count of
`0`.

## Test host

No `Test host process crashed` message appears in the run output, so no per-assembly
`/InIsolation` re-run was required. `/InIsolation` was passed on the single aggregate invocation
as the Command conventions require.

## Artifacts

- `docs/features/active/quickfiler-bug-family-446/evidence/qa-gates/p5-t5/p5-t5.trx`

The TRX was produced by `vstest.console.exe` under its default name, which embeds the account and
machine identifiers. It was renamed to the task-ID form used throughout this feature folder and
its contents were scrubbed before it was committed: `<repo-root>` absolute prefixes were replaced
with `REDACTED-REPO-ROOT`, the user-profile prefix with `REDACTED-USER-PROFILE`, the combined
`<account>_<HOST>` token with `REDACTED-USER_REDACTED-HOST`, the bare machine name with
`REDACTED-HOST` and the bare account name with `REDACTED-USER`. Only plain tokens were used, since
`<` is not legal in an XML attribute value and is not an entity inside CDATA. Replacement counts:
13,020 repository-root prefixes, 1 bare machine name, 2 `<account>_<HOST>` tokens and 2 bare
account names. A case-insensitive search of the scrubbed TRX for the account and machine names
returns **zero** hits.

The scrubbed TRX was re-parsed as XML and verified unchanged in substance: the `Counters` element
is byte-identical to its pre-scrub value, the `UnitTestResult` count is 6501 before and after, and
the SHA-256 of the ordered concatenation of all 6501 `testName` attributes is
`e3761b9485de9eea293020022be539e3973f3318381ae172978f264cfb5d962d` both before and after the
scrub. No counter and no test name was altered.

### Binary `.coverage` byproducts relocated out of the repository

`/EnableCodeCoverage` also emitted two 20 MB binary `.coverage` attachments, under a results
subdirectory and an attachment GUID directory whose names embedded the account and machine
identifiers. Those files contain 1,292 UTF-16LE occurrences of the account name in embedded
absolute paths. A binary coverage container cannot be text-scrubbed without corrupting its
internal offsets, so the two byproduct directories were moved out of the repository rather than
committed with host identifiers in them. Nothing is lost: they are tool byproducts that no task in
this plan cites, and the coverage figures this plan consumes come from the Cobertura XML that
`[P5-T6]` produces. The TRX retains its (now redacted) attachment reference.

## Output Summary

Full-suite gate passed: `EXIT_CODE: 0`, 9 assemblies discovered with no `.claude` segment in any
workspace-root-relative path, 6501 total, 6501 passed, 0 failed, 0 skipped. Primary branch, not
the reconciliation branch. TRX scrubbed of host identifiers, re-parsed, and verified to have
identical counters and test names.
