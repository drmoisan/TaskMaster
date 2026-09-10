# ilglobals-loadopcodes-unsynchronised-static-race (Issue #824)

- Date captured: 2026-09-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ilglobals-loadopcodes-unsynchronised-static-race/ (Issue #824)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #824
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/824
- Last Updated: 2026-09-08
## Summary

An unsynchronised static-mutation race in `ILGlobals.LoadOpCodes()` is the sole reason acceptance
criterion AC4 of issue #811 is unmet. `LoadOpCodes()` reassigns a shared static opcode table to a
freshly allocated, all-default array and then fills it in a loop with no lock, no `Lazy<T>`, no
`volatile`, and no static constructor. Two MSTest classes call it from class-level-parallel test
runs with no `[DoNotParallelize]` guard, so a reader on one worker can observe the table between
the reassignment and the fill. This entry records the defect so it can be tracked and fixed under
its own issue; no source change is made on the #811 branch.

## Environment

- OS/version: Windows 11 Pro 10.0.26200 locally; GitHub Actions windows runner in CI
- Runtime: .NET Framework 4.8.1 test host, VSTest 18.9.0, MSTest 4.4.0, class-level parallelism
- Command/flags used: `vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation`
- Data source or fixture: no live Outlook or filesystem dependency; the race is between two MSTest
  classes running under `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`

## Steps to Reproduce

Read the cited lines below. The failure is intermittent and was observed once in ten consecutive
full-suite runs at 24 class-level workers; it does not reproduce deterministically on demand.

## Expected Behavior

1. `ILGlobals`'s opcode tables are either published once, immutably, before any reader can observe
   them, or every read and write is synchronised.
2. Two MSTest classes that both call `LoadOpCodes()` do not run concurrently unless the shared
   state they mutate is safe under concurrent access.
3. A test failure caused by a partially filled table is distinguishable, at the point it is first
   investigated, from a genuine `MethodBodyReader` defect.

## Actual Behavior

1. **Mechanism.** `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs:123` reassigns
   `singleByteOpCodes` to a freshly allocated all-default `OpCode[0x100]` and then fills it in a
   reflection loop at line 137. Lines 117-118 declare both opcode tables as plain mutable
   `public static` fields. There is no `lock`, no `Lazy<T>`, no `volatile`, and no static
   constructor guarding either table.

2. **Reader sites.** `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs:105` and `:110`
   read the tables with no synchronisation.

3. **Participants.** `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` calls
   `LoadOpCodes()` at lines 15, 26, 37, 47. `MethodBodyReader_Tests.cs` calls it at line 364 inside
   the private `CreateReader` helper that every test in that class routes through. Neither class
   carries `[DoNotParallelize]`, and the assembly runs at
   `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`
   (`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`). `ClassLevel` scope serialises tests
   within a class, so the second participant in any observed race must be the sibling class.

4. **Observed signature (AC4 run 7).** `GetBodyCode_ReturnsConcatenatedInstructions` failed with
   `Expected bodyCode ... to contain "ldstr"`. Only the entry at IL offset 0001 (`ldstr`, opcode
   `0x72`) degraded; `nop`, `stloc.0`, `br.s`, `ldloc.0` and `ret` all resolved correctly. The two
   spaces where the opcode name belongs are the empty `Name` of `default(OpCode)`, whose
   `OperandType` is `InlineBrTarget` (0), which drives the numeric-operand branch and stores a raw
   metadata token instead of calling `module.ResolveString(...)`. The printed value 1879067923 lies
   in the `0x70……` String-metadata-token range, so the token was read correctly and merely not
   resolved. That combination is explained only by a partially filled table read
   mid-initialisation, not by a defect in token resolution itself.

5. **Frequency.** Observed once in ten consecutive full-suite runs at 24 class-level workers. The
   base rate is not established, and two clean baseline runs do not establish absence.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: while this is open, the required `mstest-coverage` check can still fail intermittently on
unrelated pull requests, which is the class of harm issue #811 exists to remove. The observed
frequency is low (once in ten runs) and no production code path is affected, but the failure is
non-deterministic and its recorded signature could be mistaken for a genuine `MethodBodyReader`
defect by a future investigator who does not have this entry.

## Suspected Cause / Notes

Both opcode tables are declared as ordinary mutable `public static` fields with no publication
guard. `LoadOpCodes()` is called repeatedly, once per test class instantiation path, rather than
once per process, so under `ClassLevel` parallelism two test classes can interleave: one thread's
reassignment-then-fill sequence in `ILGlobals.cs:123-137` is visible to a second thread's read in
`MethodBodyReader.cs:105`/`:110` at any point during the fill, including the instant right after
reassignment when the array is still all-default.

## Proposed Fix / Validation Ideas

Candidate fixes, in order of preference:

1. Publish both tables once and immutably — a static constructor or a `Lazy<OpCode[]>` pair — and
   make `LoadOpCodes()` a no-op or remove it. `LoadOpCodes()` is idempotent and the tables are
   conceptually constant, so this removes the mutable window entirely.
2. Take a `lock` inside `LoadOpCodes()` and around the reads in `MethodBodyReader`.
3. Interim stopgap only: add `[DoNotParallelize]` to both `ILGlobals_Tests` and
   `MethodBodyReader_Tests`. This suppresses rather than eliminates the race and should not be the
   final state, by the same reasoning `spec.md` applies to the console stopgap under AC3 of
   issue #811.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
