# Orchestrator Finding: Two Execution Regimes Explain the Contradictory Narratives (Issue #743)

Timestamp: 2026-09-12T15-30
Collected by: orchestrator (preparation mode)
Method: Read of `TaskMaster.runsettings` and `.github/workflows/_mstest-coverage.yml`; static reading only
EXIT_CODE: 0

This is the disambiguation the issue record has been missing. The investigation has produced two
apparently competing explanations for the 60,000 ms expiry. They are not competing. They belong to two
different execution regimes, and which one applies is decided entirely by whether a `/Settings:` argument
is passed.

## The runsettings file enables class-level parallelism

`TaskMaster.runsettings` at the repository root contains:

```
<MSTest>
  <Parallelize>
    <Workers>0</Workers>
    <Scope>ClassLevel</Scope>
  </Parallelize>
</MSTest>
```

`Workers` of 0 means one worker per logical processor, and `Scope` of `ClassLevel` means distinct
`[TestClass]` types execute concurrently. Under this file the 21 test methods across 6 `[TestClass]`
types that acquire `UiThreadDispatcherFixture.TransactionGate` genuinely can run at the same time, and
the one-permit `SemaphoreSlim` genuinely serializes them. Gate contention is real in this regime.

## CI does not pass it

`.github/workflows/_mstest-coverage.yml` line 99 is:

```
& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"
```

There is no `/Settings:` argument. `QuickFiler.Test` declares no `[assembly: Parallelize]`. So in CI,
MSTest runs the assembly serially, no two gate-acquiring tests overlap, and gate contention cannot occur
at all. What remains in CI is the raw elapsed cost of building the pump fixture, inflated by
`/EnableCodeCoverage` instrumentation and by whatever else shares the runner.

## Why this resolves the contradiction in the issue record

- Issue #711's reproduction command passed `/Settings:TaskMaster.runsettings`. That is the
  class-level-parallel regime, where contention and a leaked transaction are live mechanisms.
- Issue #743's reproduction command, quoted in its own Environment section, is
  `vstest.console.exe` over discovered `*.Test.dll` with `/EnableCodeCoverage /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`.
  No `/Settings:`. That is the serial regime, where only elapsed cost can produce the expiry.

The two issues therefore describe two different experiments, and the differing failure counts recorded
across them, seven in one place and fourteen in another, need not be reconciled as a single population.
The research artifact records that they cannot be reconciled from in-repo evidence; this explains why.

## Consequences the plan must absorb

1. **Name the regime in every acceptance criterion that involves a run.** A criterion that says "run the
   suite N times" is ambiguous between two regimes with different mechanisms, and is therefore not
   falsifiable as written. State the exact command including the presence or absence of `/Settings:`.

2. **The fix target should be chosen for the regime that matters.** CI is the serial regime, so the
   defect that reaches CI is elapsed fixture cost. An injectable UI-marshalling seam that lets tests avoid
   constructing the full `ItemViewer` Designer tree attacks exactly that cost, which is the right fix for
   CI. Gate contention and the leak are real but belong to the local parallel regime.

3. **A deterministic reproduction is available in the parallel regime and is cheap.** Acceptance criterion
   2 forbids a sleep, a retry and a timing tolerance. A leaked-transaction reproduction is deterministic
   and needs no timing at all: it does not depend on how long anything takes, only on whether a release
   happens. That is a far better basis for criterion 2 than trying to provoke an elapsed-cost timeout,
   which is inherently timing-dependent and could not be made deterministic without a tolerance.

4. **The two claims should be split across the criteria.** Criterion 2, deterministic reproduction, is
   best satisfied in the parallel regime against the leak. Criterion 3, efficacy against the roughly 4.8
   percent base rate, must be measured in whichever regime produced the base-rate observation, and the
   research records that the base rate rests on a one observed failing run with a Clopper-Pearson 95
   percent interval spanning roughly 0.0012 to 0.2382. That interval is wide enough that the 62-run figure
   is itself uncertain, which reinforces treating the streak as a supporting signal rather than the
   primary evidence.

## Status of this finding

Established by reading configuration files, not by running anything. The claim that class-level
parallelism enables real contention follows from the configuration semantics rather than from a
measurement, and the plan should still instrument rather than assume. What is directly verified here is
narrower and is not in doubt: the runsettings file requests class-level parallelism, and the CI command
does not reference that file.

## Output Summary

`TaskMaster.runsettings` requests `ClassLevel` parallelism with one worker per processor; CI passes no
`/Settings:` and therefore runs serially. Gate contention and the leak are live only in the parallel
regime, which is what #711 reproduced; elapsed fixture cost is the only available mechanism in the serial
regime, which is what #743 and CI use. Every run-bearing acceptance criterion must name its regime
explicitly, and the deterministic reproduction required by criterion 2 is most cheaply obtained against
the leak in the parallel regime.
