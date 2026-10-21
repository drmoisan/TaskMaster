# Fail-Before Exception Dossier — Finding 2 (P2-T12)

Timestamp: 2026-09-03T02-19
Task: [P2-T12]
Finding: #735 finding 2 — unguarded globals dereference in `ClearSpamManagerAsync`.

## WhyFailingRunImpossible

The defective statements cannot be reached by any deterministic unit test. `ClearSpamManagerAsync`
installs a `WindowsFormsSynchronizationContext` on the calling thread, then blocks on a modal
`MessageBox.Show` confirmation dialog that no automated run can answer, and only past that point
does it reach the statements that dereferenced the globals chain. Those statements themselves call
`SpamBayes.CreateSpamClassifiersAsync()` and `classifier.Serialize()`, which are disk-backed
classifier creation and serialization paths, and then a live engine restart. Reaching the defect
would therefore require a message pump, an answered modal dialog, and filesystem access, each of
which the repository unit-test policy prohibits outright.

The method additionally sits inside `RibbonController`'s pre-existing, already-ratified type-level
`[ExcludeFromCodeCoverage]`, so even a run that somehow reached it would produce no coverage signal.

A failing-run artifact for this finding is therefore not merely absent; it is structurally
impossible to produce. This dossier records that impossibility rather than fabricating a run.

## Alternative proof

The whole decision the fix extracts is covered by the nine tests in the new gate fixture, recorded
in `evidence/regression-testing/gate-tests.2026-09-02T12-04.md` (total 9, passed 9, failed 0).

The defect was: the three links of the globals chain are each independently null in the window
between ribbon construction and the completion of add-in initialization, and the method dereferenced
all three unguarded. The three not-ready tests cover exactly those three null states —
`RunAsync_WhenAutoFileAccessorReturnsNull_NotifiesOnceAndDoesNotInvokeReset` (the container itself
absent), `RunAsync_WhenManagerIsNull_NotifiesOnceAndDoesNotInvokeReset` (the classifier manager
unset), and `RunAsync_WhenEnginesAccessorReturnsNull_NotifiesOnceAndDoesNotInvokeReset` (the engines
facade absent) — and each asserts that the deferred reset is never invoked and that exactly one
not-ready notice is emitted. Under the pre-fix code each of those three states produced a
`NullReferenceException` instead.

Three further tests pin the surrounding contract so the guard cannot be weakened later:
`RunAsync_WithNullReset_ThrowsArgumentNullExceptionBeforeProbingAccessors` proves a caller defect is
not masked as "not ready";
`RunAsync_WhenAllDependenciesAvailable_InvokesResetWithResolvedManagerAndEngines` proves the working
path still runs and receives the resolved dependencies by identity; and
`RunAsync_WhenResetFaults_PropagatesUnchangedAndDoesNotNotify` proves the gate suppresses invocation
rather than errors.

The residual roughly ten lines that remain inside `ClearSpamManagerAsync` are validated by the
manual-verification dossier at
`evidence/other/manual-verification-clear-spam-manager.2026-09-02T12-04.md`, and no coverage credit
is claimed for them anywhere in this change.

## Negative-claim fields

SearchScope: `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/` and all of
its canonical sub-kinds, searched recursively — `baseline/`, `regression-testing/`, `qa-gates/`,
`other/`, `issue-updates/`. The feature root
`docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/` was also searched, as this
feature is single-version and has no `v1/` scope.

SearchPatterns: `fail-before*.md`, `*finding2*`, `*ClearSpamManager*`, `*.trx` (inspected for any
recorded failure attributable to `ClearSpamManagerAsync` or to the globals dereference).

SearchResult: no failing run for finding 2 exists anywhere under the feature's evidence tree. The
only fail-before run recorded in this cycle is
`evidence/regression-testing/fail-before-finding1.2026-09-02T12-04.md`, which belongs to finding 1,
and (later in this plan) `fail-before-finding3.2026-09-02T12-04.md`, which belongs to finding 3.
Neither concerns finding 2. This dossier is the finding 2 substitute.

Command: filesystem enumeration of the feature evidence tree with the patterns listed above; no
command-line tool was required to establish a negative over a tree this executor wrote in full.
EXIT_CODE: 0

Output Summary: A fail-before run for finding 2 is structurally impossible — the defective
statements require a message pump, an answered modal dialog and disk access, all prohibited by unit
test policy, and they sit inside a pre-existing type-level coverage exemption. The whole extracted
decision is instead covered by the nine passing gate tests, and the residual lines are validated by
the manual-verification dossier. No failing run for this finding exists anywhere under the feature
evidence tree.
