# Surface factory owner-thread scope review

- Timestamp: `2026-07-23T14-08Z`
- Command: `compare the P8-T28 ledger with the current single-file diff; recompute source, test, assertion, seam, authorized-path, and protected-file inventories; run git diff --check`
- EXIT_CODE: `0`
- Output Summary: `The correction changed one existing test file, retained 13 cases and 52 assertions, removed all static fixture helpers, passed both 13-case modes, remained at 480 lines, preserved the 62-path set and protected hashes, and introduced no prohibited mechanism.`

## Scope and source

| Measurement | Before | After |
|---|---|---|
| File | `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | same |
| SHA-256 | `A59DDA03D17572E9597B9146AD1E84AF8FE7A919DE5A7B611DBEDB38E9B9B356` | `3FE231161F91AB05FE28F4E99AE047B5D56B95FC8C09EF263B2FC4FB39676D38` |
| Physical lines | `479` | `480` |
| Outer static helpers | `6` | `0` |
| Fixture static/mutable shared state | `0` | `0` |
| `[TestMethod]` | `10` | `10` |
| `[DataTestMethod]` | `1` | `1` |
| `[DataRow]` | `3` | `3` |
| Discovered cases | `13` | `13` |
| `.Should()` calls | `52` | `52` |
| Assertion lines | `44` | `44` |
| Ordered test-name SHA-256 | `DFCD8BB714DB88473F702E9E8122F15BCF4EB8B749F5A0CE9F36321DD2266981` | same |
| Ordered assertion-line SHA-256 | `0FA3A31B15FE6825B716DEB28E0CFAE58CE8014891AA6BA901FDD0ABD2034BEC` | `863B270048BD7A660EC9F4E752C3B02A61F79FAC00077A7CD8FB4141BCDD5351` |

The assertion-line hash changed because async helper names, local fixture identifiers,
and context access expressions changed. No `.Should()` call was added, removed,
conditionalized, or weakened. The ordered test-name hash is identical.

## Synchronization design

- Each test creates a distinct `SurfaceFactoryFixture`, which is also that fixture's
  synchronization context and owns its queue, error queue, recorder, control, messenger,
  completion sources, operation factory, failure capture, and drain behavior.
- `Post` only enqueues and signals; it never invokes a callback on the posting thread.
- `Drain` calls `EnsureCreatorThread` and fails if another thread attempts to drain.
- Each callback runs through `Invoke`, which temporarily installs the fixture context on
  the creator thread and restores the previous context in `finally`.
- `TrackingControl : Panel` is constructed inside `Invoke`, preventing automatic
  installation of an unpumped `WindowsFormsSynchronizationContext`.
- Worker completion and initialization failure first drain exactly the create and
  initialize callbacks, then complete or fault the source and drain the remaining work.
- Completion waiting uses only operation completion and queue signaling. There is no
  sleep, delay, timeout, retry, or wall-clock threshold.
- Unexpected callback and operation exceptions propagate through `GetAwaiter().GetResult`.

`OperationRecorder` now records managed thread IDs. Its existing `OffBoundary` assertions
therefore prove actual creator-thread entry instead of ambient-context identity.

## Production seam preservation

The current source retains direct use of:

- `BreadcrumbPopupUiOperations`;
- `BreadcrumbUiDispatcher`;
- `BreadcrumbWebViewSurfaceFactory`;
- `BreadcrumbPopupUiOperations.CreateDispatchedReadiness`.

The pre-edit lexical `BreadcrumbNavigationReadiness` token became `var`, but the exact
`CreateDispatchedReadiness` production call and all four associated behavior assertions
remain. No production seam was removed or replaced.

## Verification

| Gate | Result |
|---|---|
| CSharpier format/check | 1 file, exit `0`, no byte delta |
| Analyzer build | exit `0`, zero errors |
| Nullable build | exit `0`, zero compiler/nullable errors |
| Uninstrumented exact class | `13/13` passed |
| Instrumented exact class | `13/13` passed |
| `git diff --check` | exit `0` |

The initialization-failure case retained original-exception identity, one error record,
exact `create`, `initialize`, `cleanup` order, and one control disposal in both test
modes.

## Authorized and protected inventory

| Item | Current value |
|---|---|
| Authorized C# paths | `62` |
| `StringComparer.OrdinalIgnoreCase` path-set SHA-256 | `E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD` |
| `coverage.config` SHA-256 | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` |
| `.csharpierignore` SHA-256 | `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25` |
| `SpamBayes.Actions.cs` SHA-256 | `99AFDEB968CD88ED657807E17CD1EE804D0043AEF3879E4D30C2259ED73945DA` |

There is no project, production, package, configuration, runsettings, filter, threshold,
or exclusion delta. The changed test file contains no `Thread.Sleep`, `Task.Delay`,
timeout, retry, `[DoNotParallelize]`, or `[Ignore]`.

## Independent review

Verdict: `PASS`

Severity counts: Blocker `0`, Major `0`, Medium `0`, Low `0`.

The fresh reviewer inspected the implementation rather than relying on this artifact and
reported no finding. The review independently confirmed:

- `Post` only enqueues and signals;
- `Drain` enforces creator-thread execution;
- the common monitor prevents a lost wake-up between queue insertion, operation
  completion, and waiting;
- the `Panel` is constructed while the fixture context is installed;
- all asynchronous paths are explicitly pumped;
- thread affinity is measured by managed thread ID;
- 13 cases, 52 `.Should()` calls, and 44 assertion lines remain;
- there is no static/shared fixture state or anti-masking mechanism;
- the 480-line source hash and all protected hashes match;
- both 13/13 pass artifacts and the Cobertura file are present and valid.

