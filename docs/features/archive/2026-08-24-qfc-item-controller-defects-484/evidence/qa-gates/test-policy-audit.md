# Test-policy audit across the five owned test files

Timestamp: 2026-08-26T14-10
Task: [P7-T9]

## 1. Banned wait primitives

Commands (run from the worktree root):

```
grep -c "Thread.Sleep" <the five owned test files>
grep -c "Task.Delay" <the five owned test files>
```

EXIT_CODE: 0

| File | `Thread.Sleep` matches | `Task.Delay` matches |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 0 | 0 |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 0 | 0 |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 0 | 0 |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | 0 | 0 |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 0 | 0 |

Both searches return **zero** matching lines across all five files.

## 2. Frameworks and libraries used by every test added by this feature

| Concern | Library | Evidence |
|---|---|---|
| Test framework | **MSTest** | Every added test carries `[TestMethod]`, except the #485 URI-rejection test, which carries `[DataTestMethod]` with three `[DataRow]` attributes. All come from `Microsoft.VisualStudio.TestTools.UnitTesting`. |
| Mocking | **Moq** | Added tests use `Mock<IItemViewer>`, `Mock<IQfcKeyboardHandler>`, `Mock<IMailItemActions>`, `Mock<MailItem>`, `Mock<IAttachment>`, `Mock.Of<T>`, and the `Verify` / `VerifyRemove` / `VerifySet` APIs. |
| Assertions | **FluentAssertions** | Every assertion in every added test is a `.Should()` chain. A repository-wide search for `Assert.` across the five owned test files returns **zero** matching lines, so no MSTest `Assert.*` call is used. |
| Temporary files | **none created** | A search for `GetTempPath`, `GetTempFileName`, `Path.GetTemp`, `File.Create`, `File.WriteAll`, `new FileStream`, and `Directory.CreateDirectory` across the five owned test files returns **zero** matching lines. No added test creates a temporary file. |

Determinism: the #484 timer test arms its `System.Threading.Timer` with `Timeout.Infinite` for both
due time and period so the callback can never fire, and observes disposal through an
`ObjectDisposedException` from `Timer.Change` rather than by waiting. The #481 headless test saves and
restores the ambient `SynchronizationContext` in `try`/`finally`, calls no `Show()`, and starts no
message pump or worker thread.

## 3. Additive-only audit of `QfcItemController.TestSupport.cs` (constraint C1)

Commands:

```
git show 61edc19befcf6c4e95b5acd32542f2dcdab41b78:QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs | grep -nE '^\s+(internal|private|public|protected)'
grep -nE '^\s+(internal|private|public|protected)' QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs
```

EXIT_CODE: 0

### Member declarations at `<BASE_SHA>` (20)

```
25:    internal sealed class HarnessController : QfcItemController
27:        internal HarnessController()
35:    internal static class QfcItemControllerTestSupport
37:        internal static void SetField(QfcItemController controller, string name, object value)
49:        internal static object GetField(QfcItemController controller, string name)
66:        internal static object InvokeNonPublic(
87:        internal static void EnsureSynchronizationContext()
102:        internal static Mock<IUiDispatcher> BuildSyncDispatcher()
143:        internal static void InjectThemes(
166:        internal static Theme BuildColorTheme(Color mouseOver, Color clicked, Color back)
184:        internal static Dictionary<string, Theme> BuildThemeDictionary(
201:        internal static Theme BuildDispatchableTheme(IUiDispatcher dispatcher)
221:        private static Dispatcher _dedicatedDispatcher;
222:        private static readonly object _dedicatedDispatcherLock = new object();
238:        internal static void EnsureUiThreadDispatcher()
257:        private static Dispatcher GetDedicatedDispatcher()
297:        internal static Dispatcher StartRunningDispatcher()
323:        internal static void ShutdownDispatcher(Dispatcher dispatcher)
334:    public class QfcItemController_TestSupportSmokeTests
337:        public void InjectThemes_ThenActiveThemeRead_ReturnsInjectedInstance()
```

### Member declarations in the delivered file (27)

```
28:    internal sealed class HarnessController : QfcItemController
30:        internal HarnessController()
38:    internal static class QfcItemControllerTestSupport
40:        internal static void SetField(QfcItemController controller, string name, object value)
52:        internal static object GetField(QfcItemController controller, string name)
69:        internal static object InvokeNonPublic(
90:        internal static void EnsureSynchronizationContext()
105:        internal static Mock<IUiDispatcher> BuildSyncDispatcher()
146:        internal static void InjectThemes(
169:        internal static Theme BuildColorTheme(Color mouseOver, Color clicked, Color back)
187:        internal static Dictionary<string, Theme> BuildThemeDictionary(
204:        internal static Theme BuildDispatchableTheme(IUiDispatcher dispatcher)
224:        private static Dispatcher _dedicatedDispatcher;
225:        private static readonly object _dedicatedDispatcherLock = new object();
241:        internal static void EnsureUiThreadDispatcher()
260:        private static Dispatcher GetDedicatedDispatcher()
300:        internal static Dispatcher StartRunningDispatcher()
326:        internal static void ShutdownDispatcher(Dispatcher dispatcher)
338:        internal static Mock<IItemViewer> BuildExecutingViewer()
363:        internal static IReadOnlyDictionary<string, IAttachment> BuildContentIdMap(
383:        internal static void InjectFilingCollaborators(
410:        internal static CancellationToken CancelledToken()
422:        internal static System.Threading.Timer BuildNeverFiringTimer() =>
431:        internal static void DriveSaveParameters(
447:        internal static void RaiseProtected(Control control, string handler, object args) =>
458:    public class QfcItemController_TestSupportSmokeTests
461:        public void InjectThemes_ThenActiveThemeRead_ReturnsInjectedInstance()
```

### Conclusion

Every one of the 20 declarations present at `<BASE_SHA>` is still present in the delivered file with a
character-identical signature; only the line numbers moved, by the three lines the earlier additions
pushed down. Nothing was renamed, removed, reordered, or re-signatured.

The only differences are **seven added members**, all appended after the last pre-existing member of
`QfcItemControllerTestSupport` as constraint C2 rule 7 requires:

| Added member | Phase | Purpose |
|---|---|---|
| `BuildExecutingViewer` | 1 (#480) | shared arrange helper |
| `BuildContentIdMap` | 2 (#485) | shared arrange helper |
| `InjectFilingCollaborators` | 3 (#483) | shared arrange helper |
| `CancelledToken` | 3 (#483) | shared arrange helper |
| `BuildNeverFiringTimer` | 4 (#484) | shared arrange helper |
| `DriveSaveParameters` | 4 (#484) | shared arrange helper |
| `RaiseProtected` | 5 (#481) | shared arrange helper |

No test method was added to this file; the only `[TestMethod]` it carries is the pre-existing
`InjectThemes_ThenActiveThemeRead_ReturnsInjectedInstance` smoke test. The additive-only clause of
constraint C1 therefore holds, and the seventeen sibling test files that consume
`QfcItemControllerTestSupport` or `HarnessController` — including
`QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` — are unaffected.

Output Summary: Zero `Thread.Sleep` and zero `Task.Delay` matches across all five owned test files.
Every added test uses MSTest attributes, Moq mocks, and FluentAssertions assertions, with zero MSTest
`Assert.*` calls and zero temporary-file APIs. All 20 `<BASE_SHA>` members of
`QfcItemController.TestSupport.cs` survive unchanged; the only differences are seven appended helper
members.
