# Phase 3 — Green run ([P3-T3])

Timestamp: 2026-09-01T22-56

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug`

EXIT_CODE: 0

## Discovered assemblies

```
Discovered 9 test assemblies.
```

## Runner summary block, transcribed verbatim

```
Test Run Successful.
Total tests: 6934
     Passed: 6934
 Total time: 26.7337 Seconds
```

The standard error stream is 0 bytes.

## Acceptance reading 1 — none of the three Phase 2 reds appears in the failing list

Verbatim failing-test list:

```
(empty)
```

Zero console lines match the vstest failing-test line form `^\s*Failed\s+\S`. The three names listed in
`[P2-T3]` — `ClaimsAltChord_WithAltM_ReturnsFalse`, `ClaimsAltChord_WithAltF4_ReturnsFalse` and
`ClaimsAltChord_WithAltLeft_ReturnsFalse` — are therefore all absent from it. Each appears in the run
transcript as a `Passed` line under `QfcFormKeyHandlerTests`.

## Acceptance reading 2 — every remaining failing name is in BASELINE_FAILURE_SET

The failing list is empty, so the condition holds vacuously and no name lies outside
BASELINE_FAILURE_SET, which `[P0-T12]` recorded as the empty set.

## Acceptance reading 3 — no failing name belongs to `QfcFormKeyHandlerTests`

Holds: the failing list is empty.

## Acceptance reading 4 — `ExecutingAssembly_ContainsNoFormDerivedType` is not failing

Holds. The run transcript carries `Passed ExecutingAssembly_ContainsNoFormDerivedType [1 ms]`, and the
failing list is empty.

## Acceptance reading 5 — the total-count arithmetic

`Total tests:` = **6934**. `[P0-T12]` baseline total 6927 plus seven = **6934**. The figures are equal.

## Acceptance reading 6 — the not-run figure

The runner printed **no** `Skipped:` line of its own and **no** single-line
`Failed! - Failed: N, Passed: N, Skipped: N, Total: N` summary. The `[P0-T12]` fallback rule therefore
applies, read the same way it was read there:

not-run = `Total tests:` − `Passed:` − `Failed:` = 6934 − 6934 − 0 = **0**.

BASELINE_NOT_RUN from `[P0-T12]` = 0. The two figures are **equal**.

## What the derivation establishes

By the derivation stated in the plan's reading guide, four observations together establish that a named
`[TestMethod]` both ran and passed: it is declared in a compiled test file; no entry in the failing list
carries both its name and the declaring type `QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests`; the
run's `Total tests:` equals the Phase 0 baseline plus seven; and the run's not-run figure is unchanged
from BASELINE_NOT_RUN. All four hold on this run. Therefore all seven new methods, the four existing
`IsAltKeyCommand_*` methods and `ExecutingAssembly_ContainsNoFormDerivedType` all ran and all passed.

The fourth observation is load-bearing rather than decorative: a skipped or inconclusive test is absent
from the failing list and is still counted in `Total tests:`, so the first three observations alone would
be satisfied by a method that never executed.

Corroborating `Passed` transcript lines for the eleven `QfcFormKeyHandlerTests` methods and the structural
guard, quoted verbatim from the run:

```
Passed IsAltKeyCommand_WithAltKey_ReturnsTrue [< 1 ms]
Passed IsAltKeyCommand_WithAltPlusOtherKey_ReturnsTrue [< 1 ms]
Passed IsAltKeyCommand_WithControlKey_ReturnsFalse [< 1 ms]
Passed IsAltKeyCommand_WithNone_ReturnsFalse [< 1 ms]
Passed ClaimsAltChord_WithBareAltFlagAndHandler_ReturnsTrue [< 1 ms]
Passed ClaimsAltChord_WithMenuKeyCodeAndAltFlag_ReturnsTrue [< 1 ms]
Passed ClaimsAltChord_WithAltM_ReturnsFalse [< 1 ms]
Passed ClaimsAltChord_WithAltF4_ReturnsFalse [< 1 ms]
Passed ClaimsAltChord_WithAltLeft_ReturnsFalse [< 1 ms]
Passed ClaimsAltChord_WithoutAltFlag_ReturnsFalse [< 1 ms]
Passed ClaimsAltChord_WithNullHandler_ReturnsFalse [< 1 ms]
Passed ExecutingAssembly_ContainsNoFormDerivedType [1 ms]
```

The run separately reports the five Email Filer `ClaimsAltChord_*` methods on
`QuickFiler.Test/Controllers/EfcViewerTests.cs` as passed. Two of them,
`ClaimsAltChord_WithAltM_ReturnsFalse` and `ClaimsAltChord_WithNullHandler_ReturnsFalse`, share a bare
name with a new method, which is why the derivation above is stated in terms of the declaring type. Both
name-sharing pairs appear twice in the transcript, once per fixture, and all four instances passed.

Output Summary: The green run exited 0 with 6934 of 6934 tests passed and an empty failing list. The
`Total tests:` figure equals the `[P0-T12]` baseline of 6927 plus seven. The not-run figure, derived by
the same arithmetic route `[P0-T12]` pinned, is 0 and equals BASELINE_NOT_RUN. The three tests that
failed in `[P2-T3]` now pass, and no test regressed. Fail-before and pass-after are both established from
observed runs.
