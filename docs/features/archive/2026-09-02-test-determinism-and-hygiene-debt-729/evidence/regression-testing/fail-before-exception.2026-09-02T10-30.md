# Fail-before exception dossier — Finding 3 (P5-T7)

Timestamp: 2026-09-02T23-31

Scope: Finding 3, the parallel-execution hazard on process-global `Console.Out` in
`UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` and
`UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs`.

EXIT_CODE: 0

## WhyFailingRunImpossible:

The failure this change prevents is a race, not a deterministic defect. It requires a specific
interleaving of `Console.SetOut` calls across two threads running two different test classes
concurrently, and neither the interleaving nor the thread scheduling can be forced from the test
source.

Two failure modes follow from that interleaving, and both need the sibling's `Console.SetOut` to
land inside a particular window:

1. A sibling class's `Console.SetOut` lands between this class's capture of `Console.Out` and its
   act step. This class's `StringWriter` then receives nothing, the captured output stays empty,
   and the assertion on that captured text fails.
2. A sibling class captures `Console.Out` while this class's `StringWriter` is installed, and
   reinstalls that `StringWriter` after this class's `using` block has disposed it. Every later
   `Console.Write` in the process then throws `ObjectDisposedException`, so one interleaving
   cascades into failures in unrelated classes.

Both modes depend on wall-clock scheduling under
`[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`
(`UtilitiesCS.Test/Properties/AssemblyInfo.cs` lines 18-21), with `Workers = 0` resolving to the
machine's processor count. A red run could be produced only by chance, would not reproduce, and a
test that forced the interleaving would be asserting on the test harness rather than on the
behaviour being protected. No deterministic red run is therefore producible, and none is claimed.

The remedy is also not observable as a test outcome: `[DoNotParallelize]` moves each class into
MSTest's serial partition, which removes the concurrency that the race needs. Its effect is the
absence of a nondeterministic failure, which no single run can demonstrate.

## Alternative proof — two in-repo precedent classes

The hazard is not hypothetical. The repository already recognises this exact hazard and already
applies this exact remedy in two other `UtilitiesCS.Test` classes. Both carry a hazard comment
naming the same mechanism, and both were marked in response to it.

Precedent 1 — `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs` lines 14-20:

```csharp
    // [DoNotParallelize] — DataFramePrettyHelpers_RenderRowsMarkdownAndConsoleOutput
    // captures and restores Console.Out, which is process-wide state. Under the
    // class-level parallel scope set in TaskMaster.runsettings, a sibling test
    // class's Console.SetOut overrides this class's redirect mid-test, causing
    // PrettyPrint's Console.WriteLine output to land in the wrong writer.
    [DoNotParallelize]
    [TestClass]
```

Precedent 2 — `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` lines 17-21:

```csharp
    // EnumerateTable_WritesFormattedOutputAndMovesToStart redirects Console.Out,
    // which is process-wide state. Under class-level parallel execution another
    // test class can replace the writer mid-test and make the captured output empty.
    [DoNotParallelize]
    [TestClass]
```

These two classes establish that the hazard was previously observed in this assembly and that
`[DoNotParallelize]` is the accepted in-repo remedy for it. The two classes marked by this change
capture, restore, and assert on `Console.Out` on identical terms and were the only two compiled
`UtilitiesCS.Test` classes still doing so without the attribute.

## Deliberate deviation from the precedent wording

The new hazard comments reuse the precedent wording but cite
`UtilitiesCS.Test/Properties/AssemblyInfo.cs` lines 18-21 as the live source of the class-level
parallel scope, and state explicitly that `TaskMaster.runsettings` is not what takes effect. The
precedent comments' `TaskMaster.runsettings` citation is stale: the CI vstest invocation passes no
`/Settings:` argument, so the assembly-level `Parallelize` attribute is what actually governs. The
stale citation is not repeated.

Output Summary: No deterministic red run exists or can be produced for Finding 3, and none is
claimed. This dossier records why, and cites two in-repo precedent classes as the alternative
proof that the hazard is real and was previously observed. Satisfies spec.md AC15.
