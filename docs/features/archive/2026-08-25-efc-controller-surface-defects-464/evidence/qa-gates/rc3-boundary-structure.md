# [P6-T15] RC3 source-structure and instrument evidence

Timestamp: 2026-08-28T01-11
Task: [P6-T15]
Command: source inspection of the delivered `QuickFiler/Controllers/EfcFormController.cs` and
`QuickFiler/Controllers/EfcItemController.cs` with `grep -c -F` fixed-string counts and `grep -n` line
numbering; plus `grep -rn` over `QuickFiler.Test/` for the token `log4net`. No build or test invoked.
EXIT_CODE: 0

## `QuickFiler/Controllers/EfcFormController.cs`

### Zero-hit fixed-string search for `throw;`

| Measure | Command | Value |
|---|---|---|
| Delivered count | `grep -c -F "throw;" QuickFiler/Controllers/EfcFormController.cs` | **0** |
| Pre-change count (recorded in `[P0-T16]`) | same command at `BASELINE_SHA` | **5**, at `:425`, `:441`, `:457`, `:517` and `:530` |

The five `throw;` statements are gone. Each of the five `catch` blocks now ends with the
`BoundaryErrorSink` call and rethrows nothing.

### The five extracted `internal async Task` members and their one-line wrappers

| `async void` wrapper | Wrapper body (verbatim) | Extracted member declaration |
|---|---|---|
| `ButtonCancel_Click` (`:460`) | `await ButtonCancelClickAsync();` (`:461`) | `internal async Task ButtonCancelClickAsync()` (`:463`) |
| `ButtonOK_Click` (`:478`) | `await ButtonOkClickAsync();` (`:478`, same line) | `internal async Task ButtonOkClickAsync()` (`:480`) |
| `ButtonRefresh_Click` (`:495`) | `await ButtonRefreshClickAsync();` (`:496`) | `internal async Task ButtonRefreshClickAsync()` (`:498`) |
| `ButtonCreate_Click` (`:513`) | `await ButtonCreateClickAsync();` (`:514`) | `internal async Task ButtonCreateClickAsync()` (`:516`) |
| `ButtonDelete_Click` (`:575`) | `await ButtonDeleteClickAsync();` (`:576`) | `internal async Task ButtonDeleteClickAsync()` (`:578`) |

Every wrapper body is a single `await` of its extracted member. All five extracted members are
`internal`. A search of `QuickFiler/Interfaces/` for the five names returns **0** matching lines, so none
appears on any interface.

Count of `BoundaryErrorSink(ex.Message, ex);` calls in the file: **5**, one per boundary.

### Verbatim body of the default `BoundaryErrorSink` delegate

Declared at `EfcFormController.cs:137-138`:

```csharp
internal System.Action<string, System.Exception> BoundaryErrorSink { get; set; } =
    (message, exception) => logger.Error(message, exception);
```

The delegate body is **exactly one** `logger.Error(message, exception)` call and nothing else. `logger`
is the pre-existing static logger declared at `EfcFormController.cs:123-125`
(`log4net.LogManager.GetLogger(...)`), which this feature did not add or modify. This is the source
inspection the amended `spec.md` criterion at `spec.md:959` names.

The property is `internal` and appears on no interface.

## `QuickFiler/Controllers/EfcItemController.cs`

### `ThrowInitializationFailure`

Declared at `:745`:

```csharp
internal static void ThrowInitializationFailure(System.Exception initializationException)
```

Modifiers: `internal static`. A search of `QuickFiler/Interfaces/` for the name returns **0** matching
lines. Delivered body (`:747-753`) captures through
`System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(initializationException).Throw()`, which
rethrows the same instance with its original stack trace intact.

### Single-statement failure branch of `WebView2Control_CoreWebView2InitializationCompleted`

```csharp
if (!e.IsSuccess)
{
    ThrowInitializationFailure(e.InitializationException);
}
```

The branch (`:761-764`) is a single statement calling the extracted member with the event argument's
initialization exception.

### Zero-hit fixed-string search for `throw (e.InitializationException)`

| Measure | Command | Value |
|---|---|---|
| Delivered count | `grep -c -F "throw (e.InitializationException)" QuickFiler/Controllers/EfcItemController.cs` | **0** |
| Pre-change count | same command at `BASELINE_SHA` | **1**, at `:777` of the plan-cited pre-change text |

## Instrument substitution — recorded, not concealed

`spec.md` requires the boundary to be verified through the controller's **boundary error sink**, whose
default delegate is verified by source inspection to be exactly one `logger.Error(message, exception)`
call on the pre-existing static logger. That verification is the section above.

The sink is an injectable seam rather than a direct assertion on `log4net.ILog`. It follows 484's
`MoveFailureNotifier` seam-and-default shape, which the upstream constraints briefing directs this
feature to mirror rather than reinvent: an `internal` property with a production default, replaced by
every failure-path test.

### Correction to plan decision D10's stated reason

Plan decision D10 and the `[P6-T15]` acceptance text both assert that
`QuickFiler.Test/QuickFiler.Test.csproj` **carries no log4net reference** and that the only occurrence of
the token under `QuickFiler.Test/` is a binding redirect at `QuickFiler.Test/app.config:78`. **That
premise is false on this execution base**, and this artifact records the truth rather than repeating the
plan's stale claim.

Measured on the delivered tree:

| Location | Content |
|---|---|
| `QuickFiler.Test/QuickFiler.Test.csproj:214-215` | `<Reference Include="log4net, Version=3.3.2.0, ...">` with `<HintPath>..\packages\log4net.3.3.2\lib\net462\log4net.dll</HintPath>` |
| `QuickFiler.Test/packages.config:16` | `<package id="log4net" version="3.3.2" targetFramework="net481" />` |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs:6-8` | `using log4net;`, `using log4net.Appender;`, `using log4net.Repository.Hierarchy;` |
| `QuickFiler.Test/app.config:78` | the binding redirect the plan cites |

The reference was introduced by commit `33bcd218` ("fix(614): route breadcrumb router selection through
the archive stem contract"), a **merged sibling**, and reached this branch through the base merge at
`25924673`. It is not this feature's addition, and constraint C1 was not breached: this feature added no
reference to any project file.

**Consequence, stated plainly.** D10's *reason* for choosing a sink over a direct log4net assertion is
now stale — a log4net appender assertion would in fact compile in this assembly today. D10's
*instruction* is unaffected and was followed: the sink is the instrument `spec.md:959` names by name,
its default delegate is exactly one `logger.Error(message, exception)` call as the criterion requires,
and it matches the 484 seam shape the upstream briefing makes binding. No implementation change is made
on the strength of this correction, because the plan governs what to do and it specifies the sink.

This deviation from the plan's stated rationale is recorded here for `[P11-T15]`.

Output Summary: PASS. `EfcFormController.cs` contains 0 occurrences of `throw;` against a pre-change
count of 5; all five `async void` handlers are one-line wrappers over `internal` `async Task` members
that appear on no interface; the default `BoundaryErrorSink` delegate is exactly one
`logger.Error(message, exception)` call on the pre-existing static logger.
`EfcItemController.cs` contains 0 occurrences of `throw (e.InitializationException)` against a pre-change
count of 1; `ThrowInitializationFailure` is declared `internal static` and captures through
`ExceptionDispatchInfo`; the handler's failure branch is a single statement. Plan decision D10's stated
premise that the test project carries no log4net reference is recorded as **false on this base** — a
merged sibling added the reference — while the delivered instrument, the boundary error sink, is exactly
what the `spec.md` criterion names.
