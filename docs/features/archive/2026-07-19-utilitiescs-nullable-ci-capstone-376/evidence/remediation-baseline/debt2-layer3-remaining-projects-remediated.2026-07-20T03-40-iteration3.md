# P2-T21/P2-T22 Loop Iteration 3 — Layer 5: TaskMaster.Test.csproj and UtilitiesCS.Test.csproj

Timestamp: 2026-07-20T03-40

## Context

After iteration 2's TaskMaster.csproj / QuickFiler.Test.csproj fixes, the next full solution-wide
rebuild surfaced own-diagnostics in `TaskMaster.Test.csproj` (13 CS8632 sites) and
`UtilitiesCS.Test.csproj` (16 CS8632 sites, 3 CS8625 sites, 3 CS0067 sites) — 35 diagnostics
total, all in test projects that had never previously been reached by a passing rebuild.

## Remediation applied

### CS8632 (29 sites total across both projects)

All fixed via the established `#nullable enable annotations` / `#nullable restore annotations`
narrow-bracket pattern, scoped to the smallest reasonable declaration/statement (or, where two or
three `?` sites are immediately adjacent, one bracket spanning the adjacent group): field
declarations, constructor parameter lists, method signatures, local variable declarations, and one
inline cast. Files touched:
`TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs`,
`TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs`,
`TaskMaster.Test/AppGlobals/EngineInitTimingProbeTests.cs` (2 separate brackets),
`TaskMaster.Test/AppGlobals/StoreRehookCoordinatorTests.cs` (2 separate brackets),
`TaskMaster.Test/AppGlobals/TestableApplicationGlobals.cs`,
`TaskMaster.Test/OutlookObjects/Store/StoresWrapperTests.cs`,
`UtilitiesCS.Test/TestHelpers/ManualFireTimerWrapper.cs`,
`UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` (5 separate brackets covering 6
sites), `UtilitiesCS.Test/OutlookObjects/Conversation/ConversationHelper_ExtendedTests.cs` (7
separate brackets), `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` (3 separate brackets).
None of these edits changes runtime behavior: the annotations context affects only whether `?`
syntax is permitted at compile time, not code generation or flow-analysis warnings.

### CS8625 (3 sites, `UtilitiesCS.Test`)

Null-forgiving operator (`null` -> `null!`) at three deliberate-null test call sites that exercise
production guard-clause/defensive-null-check behavior:
`UtilitiesCS.Test/EmailIntelligence/EmailTokenizer_Tests.cs(62,41)` (`obj: null!` — tests
`ArgumentNullException`), `UtilitiesCS.Test/EmailIntelligence/SubjectMapEntry_Tests.cs(244,86)`
(`tokens: null!`), `UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs(166,31)`
(`progress: null!` — the production method already defensively checks `if (null != progress)`).
No behavior change: `null!` is still `null` at runtime.

### CS0067 (3 sites, `UtilitiesCS.Test`)

Narrow `#pragma warning disable CS0067` / `restore` bracket with rationale, since none of the
three `PropertyChanged` events can be deleted (each is required by an implemented interface —
`IOlObjects : INotifyPropertyChanged` or `ISmartSerializable<T> : INotifyPropertyChanged` —
confirmed via grep of the interface declarations). Files:
`UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs(171)`,
`UtilitiesCS.Test/ReusableTypeClasses/SmartSerializable_Tests.cs(824)`,
`UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableBase_Tests.cs(652)`.

All fixes fall within the three authorized patterns. No diagnostic required a behavior change; no
escalation was necessary.

## Verification

Command: `MSBuild.exe TaskMaster.Test/TaskMaster.Test.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true`
EXIT_CODE: 0 — Build succeeded, 0 Warning(s), 0 Error(s).

Command: `MSBuild.exe UtilitiesCS.Test/UtilitiesCS.Test.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true`
EXIT_CODE: 0 — Build succeeded, 0 Error(s). One residual warning (CS2002, "Source file
'PercentageFormatterTests.cs' specified multiple times") is a pre-existing duplicate `<Compile>`
item in the `.csproj`, unrelated to this remediation, does not block the build, and is not fixed
by this feature (out of scope: fixing it would require editing the `.csproj`'s item list, a
separate, unrelated maintenance concern).

## Next step

Proceed to the next full solution-wide rebuild gate iteration (P2-T23).
