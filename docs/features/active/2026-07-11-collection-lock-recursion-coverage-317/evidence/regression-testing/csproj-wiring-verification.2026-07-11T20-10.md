# csproj Wiring Verification (#317) — AC-3

Timestamp: 2026-07-11T20-10

Command: `rg -n "ConcurrentObservableCollectionLockRecursionTests.cs" "UtilitiesCS.Test/UtilitiesCS.Test.csproj"`

EXIT_CODE: 0

Output:
```
392:    <Compile Include="ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionLockRecursionTests.cs" />
```

Output Summary: Exactly one `<Compile Include>` line references the restored file, at line 392,
immediately following the `ConcurrentObservableCollection_Tests.cs` entry at line 391 (originally at
line 391 pre-edit). This satisfies AC-3.
