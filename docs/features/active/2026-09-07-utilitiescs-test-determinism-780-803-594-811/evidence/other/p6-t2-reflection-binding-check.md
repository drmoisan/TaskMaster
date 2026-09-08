# P6-T2 — Reflection binding re-verification

Timestamp: 2026-09-08T10-01
Task: [P6-T2]
Command: Grep over `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` for `EtlByRowAsync` and over `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs` for the same, after all Phase 1, 5 and 6 edits
EXIT_CODE: 0

REFLECTION_BINDING: UNCHANGED

A reflection binding is invisible to the compiler: a signature change that breaks it produces no
build error and instead fails at run time with a null `MethodInfo`. This task re-checks it after
every edit rather than relying on the P5-T10 build.

## The reflection call

`UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs`, in
`EtlByRowAsync_PrivateHelper_ReturnsConvertedRows` (test method opens at line 1098; the call spans
lines 1122-1135, originally cited as 1124-1137 and now two lines earlier because P6-T1 removed the
four `Returns(120)` tolerance lines above it):

```csharp
var asyncRows = await InvokeStaticAsync<IAsyncEnumerable<object[]>>(
    "EtlByRowAsync",
    new[]
    {
        typeof(Outlook.Table),
        typeof(Dictionary<string, Func<object, string>>),
        typeof(Dictionary<string, int>),
        typeof(CancellationToken),
    },
    mockTable.Object,
    converters,
    columnDictionary,
    CancellationToken.None
);
```

The explicit four-type array selects the four-parameter overload by exact signature match.

## The four-parameter overload it binds to (unchanged)

`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs:173-178`:

```csharp
private static async Task<IAsyncEnumerable<object[]>> EtlByRowAsync(
    Table table,
    Dictionary<string, Func<object, string>>? objectConverters,
    Dictionary<string, int> columnDictionary,
    CancellationToken token
)
```

Its parameter list is `Table table, Dictionary<string, Func<object, string>>? objectConverters,
Dictionary<string, int> columnDictionary, CancellationToken token` — exactly the four types the
reflection array names, in the same order. P1-T1 did not edit this overload (decision D9, D14).

## The seven-parameter overload that was edited

`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs:231-239`:

```csharp
private static async Task<object[,]> EtlByRowAsync(
    Table table,
    Dictionary<string, Func<object, string>>? objectConverters,
    Dictionary<string, int> columnDictionary,
    CancellationToken token,
    int timeout,
    TimeProvider? timeProvider,
    ProgressTracker? progress = null
)
```

Its list ends `TimeProvider? timeProvider, ProgressTracker? progress = null`, which is exactly what
the acceptance condition requires. P1-T1 replaced `int attempts` with `TimeProvider? timeProvider`
in this overload only.

A third overload also carries the name — the public extension
`EtlByRowAsync(this IAsyncEnumerable<Row>, ...)` at line 218 — but it takes five parameters of
different types and cannot be selected by the four-type array.

## Acceptance evaluation

- The four-parameter overload's parameter list is unchanged and matches the reflection array
  type-for-type and in order. PASS
- The seven-parameter overload's list ends `TimeProvider? timeProvider, ProgressTracker? progress = null`.
  PASS
- `REFLECTION_BINDING: UNCHANGED` recorded. PASS

## Output Summary

The reflection-bound four-parameter `EtlByRowAsync` overload is untouched by this change, so the
binding in `EtlByRowAsync_PrivateHelper_ReturnsConvertedRows` still resolves. The `TimeProvider`
parameter was added only to the seven-parameter overload, which no test binds by reflection. The
P6-T5 run confirms the test passes at run time, which is the behavioural counterpart to this
static check.
