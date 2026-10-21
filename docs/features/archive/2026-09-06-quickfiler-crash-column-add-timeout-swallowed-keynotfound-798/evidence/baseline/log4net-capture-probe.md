# Phase 0 — Cross-assembly log4net capture probe

Timestamp: 2026-09-07T01-04
Task: [P0-T13]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<worktree>`,
`<vs-install>`, `<user>` and `<machine>` tokens.

## Question this probe answers

The AC2 timing tests in P2-T5 need to observe `[Df timing]` lines emitted by the production
`UtilitiesCS.DfDeedle` logger from a test in the separate `UtilitiesCS.Test` assembly. Whether that
is possible depends on whether the log4net repository reached from the test assembly is the same
repository that owns the production assembly's logger. If the two assemblies resolve to different
default repositories, a `MemoryAppender` attached from the test side captures nothing and any
assertion over it would be vacuous.

## Probe test

A temporary test named `Probe798_MemoryAppenderCapturesDfDeedleLogger` was appended to
`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`. It:

1. reflects the private static `logger` field of `UtilitiesCS.DfDeedle` with
   `BindingFlags.NonPublic | BindingFlags.Static` and casts it to `log4net.ILog`;
2. obtains the test assembly's default repository as
   `(log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository()`;
3. attaches a `log4net.Appender.MemoryAppender` to the logger named by `typeof(DfDeedle).FullName`
   on that repository and sets that logger's level to `log4net.Core.Level.Debug`;
4. invokes `Debug` on the reflected production `ILog` instance with the literal message `probe-798`;
5. asserts the appender's captured events contain an event whose rendered message is that literal;
6. removes the appender in a `finally` block so the process-wide log4net state is restored.

## Build

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0

The probe compiled with no analyzer error, which additionally confirms that `log4net` is on the
`UtilitiesCS.Test` reference list and that `MemoryAppender`, `Hierarchy` and `Level` are reachable
from the test assembly without a new package reference.

## Test run

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p0-log4net /TestCaseFilter:"FullyQualifiedName~Probe798_MemoryAppenderCapturesDfDeedleLogger"`
EXIT_CODE: 0
ExpectedExitCode: 0

TRX: `coverage\trx\p0-log4net\<user>_<machine>_2026-09-07_01_04_23_net481.trx`

- total: 1
- executed: 1
- passed: 1
- failed: 0
- outcome: Passed

Duration 130 ms.

## Verdict

LOG4NET_CROSS_ASSEMBLY_CAPTURE: CONFIRMED

> **SUPERSEDED.** The verdict line above and the paragraph that follows it are retained verbatim as
> the original Phase 0 record, but they are **superseded** by the addendum at the end of this file,
> "Addendum — verdict not reproducible in Phase 3". Phase 3 established that this verdict is not
> reproducible and that its supporting reasoning is incorrect. Any reader, and specifically P9-T14,
> must read the addendum before relying on the `CONFIRMED` verdict.

A `MemoryAppender` attached through the test assembly's default log4net repository captured a Debug
event emitted by the production `UtilitiesCS.DfDeedle` logger. The two assemblies resolve to the
same default repository on this toolchain.

## Consequence for P2-T5

P2-T5 implements the `CONFIRMED` branch. Its two AC2 tests,
`AddQfcColumns_EmitsDfTimingLineForEachColumnOperation` and
`HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration`, attach a `MemoryAppender` to the
logger named by `typeof(DfDeedle).FullName` and assert the **existence** of the expected
`[Df timing]` lines. They must not assert a count: log4net binds one logger per type for the whole
process, so a concurrently running test class can add events but can never remove them, which makes
an existence claim deterministic and a count assertion order-dependent. The `NOT AVAILABLE` fallback
defined in P2-T5, attaching the appender to the repository reached from the production assembly's
own logger instance, is not needed and is not implemented.

## Fallback explicitly not used

The fallback suggested in spec.md's assumptions section, asserting AC2 indirectly through the
injected column-adder, is not used, because injecting an adder replaces the whole `AddQfcColumns`
body so the AC2 instrumentation never executes and the assertion could not fail. AC14 requires only
that the fallback actually used be documented in the plan, and P2-T5 documents it.

Output Summary: The probe test compiled and passed, 1 total, 1 passed, 0 failed. The verdict is
`LOG4NET_CROSS_ASSEMBLY_CAPTURE: CONFIRMED`, so P2-T5 implements the direct `MemoryAppender`
strategy against the logger named by `typeof(DfDeedle).FullName` and asserts existence rather than
a count.

## Probe Removal

Task: [P0-T14]
Timestamp: 2026-09-07T01-05

The probe test was removed by restoring
`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` from the index, which carries the base-commit
content for that path. Two observations were then taken.

### Observation 1 — anchored diff

Command: `git diff --stat c431dc32 -- UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`
EXIT_CODE: 0
Output lines produced: 0

The command produced no output at all, so the file is byte-identical to its content at the base
commit `c431dc32`.

### Observation 2 — repository-wide literal search

Command: repository-wide search over `*.cs` for the literal `Probe798_MemoryAppenderCapturesDfDeedleLogger`
Matches: 0, across 0 files.

The probe identifier is absent from every `.cs` file in the repository.

### Supporting check

The restored file measures 882 lines, matching the base-commit value P0-T11 recorded and confirming
that the probe's 48 added lines are gone rather than merely renamed.

Both observations were taken after the removal, not predicted before it. The temporary probe leaves
no residue in the write set, and `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` enters Phase 1
in its base-commit state, so the P1-T11 acceptance condition requiring a line count strictly below
882 is measured against an unmodified starting point.

## Addendum — verdict not reproducible in Phase 3

Timestamp: 2026-09-07T02-45
Recorded by: Phase 4 executor, applying correction A1
Supersedes: the `LOG4NET_CROSS_ASSEMBLY_CAPTURE: CONFIRMED` verdict recorded above under `## Verdict`

### Status of the original verdict

The verdict `LOG4NET_CROSS_ASSEMBLY_CAPTURE: CONFIRMED` recorded above **is not reproducible**. It
must not be relied on as a standing statement about this repository's log4net behaviour, and AC14's
requirement that the log-capture check be recorded with its outcome is satisfied by this addendum
together with the original record, not by the original record alone.

The original verdict text is retained unaltered above because it is the contemporaneous Phase 0
record and this feature folder's audit trail depends on earlier tasks' citations remaining readable.
Retention is not endorsement.

### What Phase 3 observed

When Phase 3 brought the two AC2 tests to green it found that both tests captured **zero** log
events, whether or not the production instrumentation was present. The capture was inert, so the
assertion over it could not discriminate.

### Mechanism

Neither `UtilitiesCS` nor `UtilitiesCS.Test` carries a log4net `XmlConfigurator` attribute. The
default repository reached by `log4net.LogManager.GetRepository()` from either assembly is therefore
**unconfigured**: its `Configured` property is false. An unconfigured `Hierarchy` reports every level
disabled through `Hierarchy.IsDisabled`, so the production `logger.Debug(...)` call short-circuits
and emits nothing at all. Attaching a `MemoryAppender` to the logger named by
`typeof(DfDeedle).FullName` and raising that logger's `Level` to `Debug` does not change this,
because the repository-level disable is evaluated before the per-logger level.

The consequence is that the original probe's passing result does not establish what it was written to
establish. The probe asserted that an appender attached from the test assembly captures an event
emitted by the production logger. Under an unconfigured repository the production `Debug` call is a
no-op, so a capture of zero events is the expected outcome and a passing probe is not evidence that
the two assemblies share a default repository. Whether they in fact share one remains **unestablished
by execution**; Phase 3 neither confirmed nor refuted it, because the repository-level disable masks
the question the probe was asked.

### The probe cannot be re-examined

The probe test `Probe798_MemoryAppenderCapturesDfDeedleLogger` was removed by P0-T14, and the
`## Probe Removal` section above records both the anchored diff and the repository-wide literal
search confirming its removal. No copy of the probe source survives in the worktree or in the
feature folder. The probe's recorded pass therefore cannot be re-run, re-read, or diagnosed further,
and the reason it passed cannot be established from the retained evidence alone. The mechanism above
is derived from the Phase 3 observation and from the absence of a configurator attribute in either
assembly, not from re-execution of the probe.

### Strategy actually implemented — a third strategy

The strategy Phase 3 implemented is **neither** of the two branches P2-T5 documents. It is a third
strategy, and it is recorded here as such rather than being described as one of the plan's two
branches.

- **Not the `CONFIRMED` branch as written.** That branch attaches a `MemoryAppender` to the logger
  named by `typeof(DfDeedle).FullName` through the test assembly's default repository and asserts
  existence of the expected `[Df timing]` lines. Implemented alone, it captures nothing, because the
  repository is unconfigured.
- **Not the `NOT AVAILABLE` branch.** That branch attaches the appender to the repository reached
  from the production assembly's own logger instance, obtained by reflecting the private static
  `logger` field of `UtilitiesCS.DfDeedle`. It addresses a cross-assembly repository mismatch. The
  actual obstruction is not a repository mismatch but a repository-level disable, which that branch
  does not remove.
- **The third strategy, implemented.** The test helper `CaptureDfDeedleLog` in
  `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` keeps the `CONFIRMED` branch's
  appender attachment and existence assertion, and additionally makes the repository emit at all by
  setting `Configured` to true on the `Hierarchy` for the duration of the capture and calling
  `ActivateOptions()` on the `MemoryAppender` before attaching it. The previous `Configured` value
  and the logger's previous `Level` are both restored in a `finally`, so the process-wide log4net
  state the class mutates is returned to its prior value. The class carries `[DoNotParallelize]`,
  which bounds that mutation within the class.

This third strategy is recorded in the plan by correction A3, which satisfies AC14's final clause
requiring that the AC2 assertion strategy actually used be documented in the plan.

### Effect on the AC2 fail-before evidence

Because the capture was inert when `p2-ac2-timing-fail-before.md` was written, the two failures that
artifact records would have occurred whether or not the AC2 instrumentation existed. That gate did
not discriminate and does not support AC2. It is superseded as AC2 fail-before evidence by
`../regression-testing/p3-ac2-failbefore-rederived.2026-09-07T02-45.md`, produced by correction A2,
which re-derives the fail-before observation against the working capture.

Output Summary: The Phase 0 verdict `LOG4NET_CROSS_ASSEMBLY_CAPTURE: CONFIRMED` is not reproducible.
Both AC2 tests captured zero events because neither assembly configures log4net, leaving the default
repository unconfigured and every level disabled, which makes the production `Debug` call a no-op.
The probe was removed by P0-T14 and cannot be re-examined. Phase 3 implemented a third strategy,
documented above and in the plan by correction A3: set `Configured` on the repository and call
`ActivateOptions()` on the appender for the duration of the capture, restoring both in a `finally`.
