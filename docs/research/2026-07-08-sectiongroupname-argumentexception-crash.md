# Research: `sectionGroupName` ArgumentException crash in the TaskMaster VSTO add-in

- Date: 2026-07-08
- Author: task-researcher (static analysis only; Outlook not runnable in this environment)
- Scope: root-cause confirmation for an unhandled `System.ArgumentException` (`Parameter name: sectionGroupName`, HResult 0x80070057) that terminates `outlook.exe`, observed with a ThreadPool rethrow stack (`ExceptionDispatchInfo.Throw` -> `QueueUserWorkItemCallback.ExecuteWorkItem` -> `ThreadPoolWorkQueue.Dispatch`).

This document records verified findings and clearly separates confirmed facts from inference. Every claim is grounded in a `file:line` citation or an explicitly labeled framework-behavior inference.

---

## 1. Method used

Exhaustive in-repo search (Grep/Glob/Read) for:

- Any first-party call to a `System.Configuration` member taking a `sectionGroupName` parameter (`GetSectionGroup`, `SectionGroups[...]`, `OpenExeConfiguration`, `OpenMappedExeConfiguration`, `ExeConfigurationFileMap`, `ConfigurationUserLevel`).
- All `System.Configuration` consumers (`ConfigurationManager`, `ApplicationSettingsBase`, `Settings.Default`, `GetSection`).
- All configuration files (`*.config`) and generated `Settings.Designer.cs` classes.
- log4net configuration wiring.
- The add-in startup path and all `async void` / fire-and-forget sites in the `TaskMaster` host project.

---

## 2. The throw site

### 2.1 Confirmed: no first-party code calls any `sectionGroupName` API

Grep across the entire repository for `sectionGroupName`, `GetSectionGroup`, `SectionGroups[`, `OpenExeConfiguration`, `OpenMappedExeConfiguration`, `ExeConfigurationFileMap`, and `ConfigurationUserLevel` returns **zero matches in first-party code**. There is no first-party call site that passes a `sectionGroupName` argument to `System.Configuration`.

Conclusion (confirmed): the throw does not occur in first-party code. It occurs inside a framework method in `System.Configuration`, reached transitively.

### 2.2 Confirmed: the only first-party consumer of `System.Configuration` is `ApplicationSettingsBase`

The sole first-party mechanism that reaches `System.Configuration` section / section-group navigation is the generated settings classes deriving from `System.Configuration.ApplicationSettingsBase`, accessed through `Settings.Default` (read) and `Settings.Default.Save()` (write). Seven such generated classes exist:

- `TaskMaster/Properties/Settings.Designer.cs:16`
- `QuickFiler/Properties/Settings.Designer.cs:16`
- `ToDoModel/Properties/Settings.Designer.cs:16`
- `TaskVisualization/My Project/Settings.Designer.cs:16`
- `UtilitiesCS/Properties/Settings.Designer.cs:16`
- `Tags/My Project/Settings.Designer.cs:21` (`MySettings`)
- `UtilitiesSwordfish.Test/Properties/Settings.Designer.cs:16` (test only)

The generated property accessors use the `this["Name"]` indexer (e.g. `QuickFiler/Properties/Settings.Designer.cs:31`), which routes through `ApplicationSettingsBase.get_Item` -> `LocalFileSettingsProvider` -> `ClientSettingsStore`. `Save()` routes through `LocalFileSettingsProvider.SetPropertyValues` -> `ClientSettingsStore.WriteSettings`. Both internally navigate the `userSettings` / `applicationSettings` section groups.

`Settings.Default` and `Settings.Default.Save()` are used pervasively; representative call sites:

- `TaskMaster/AppGlobals/AppOlObjects.cs:446` (`_darkMode = Properties.Settings.Default.DarkMode`, an instance-field initializer)
- `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs:43,86,137` (`Save()`)
- `TaskMaster/AppGlobals/AppStagingFilenames.cs` (many `Save()` calls)
- `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` (many `Save()` calls)
- `TaskMaster/AppGlobals/ApplicationGlobals.cs:64` (`Settings.Default.StartupTimingEnabled`)
- `TaskMaster/AppGlobals/AppEvents.cs:72` (`Settings.Default.EventsHooked`)
- `ToDoModel/Data Model/ID/IDList.cs:104-105` (set + `Save()`)

### 2.3 Refuted: log4net is not the source

log4net is configured by assembly attributes: `TaskMaster/ThisAddIn.cs:12` (`[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]`) and `QuickFiler/Legacy/QuickFileController.cs:14` (`[assembly: log4net.Config.XmlConfigurator(Watch = true)]`). `TaskMaster/log4net.config` is a standalone `<log4net>` root element (`TaskMaster/log4net.config:1`), which `XmlConfigurator` parses directly as a raw XML file. log4net does not use `System.Configuration` section-group navigation for a standalone config file and has no API parameter named `sectionGroupName`. The separately-tracked log4net defect (issue #208, `docs/features/potential/promoted/2026-06-19-log4net-startup-log-directory-not-created.md`) is a `DirectoryNotFoundException` on the relative `logs\` path, unrelated to this `ArgumentException`.

### 2.4 Inference: the framework method and the exact parameter

The message text "The parameter 'sectionGroupName' is invalid." matches the .NET Framework `System.Configuration` helper `ExceptionUtil.ParameterInvalid("sectionGroupName")` (format string `SR.Parameter_Invalid` = "The parameter '{0}' is invalid."). The only public `System.Configuration` surface whose validated parameter is literally named `sectionGroupName` is `System.Configuration.Configuration.GetSectionGroup(string sectionGroupName)` (and its supporting `SectionGroupInfo` path parsing). `ArgumentException` (not the more common `ConfigurationErrorsException`) with this parameter name is raised when the supplied section-group path is syntactically invalid — for example an empty path segment (a trailing `/`, as in `"userSettings/"`) or an illegal name character.

`ClientSettingsStore` calls `Configuration.GetSectionGroup("userSettings")` (and the application-settings equivalent) during user-scoped settings navigation, using the settings class's group name to build the path. This ties the exception to the `ApplicationSettingsBase` path. This is framework-behavior inference (the .NET Framework `System.Configuration` source is not in this repository), stated as inference, not verified from a repo line.

### 2.5 Confirmed: in-repo config files are well-formed

`TaskMaster/app.config` declares both `userSettings` and `applicationSettings` section groups with valid `name` attributes and a `TaskMaster.Properties.Settings` section in each (`TaskMaster/app.config:3-25`, values at `:454-632`). `UtilitiesCS/app.config`, `ToDoModel/app.config` likewise declare a valid `userSettings` sectionGroup. `QuickFiler/app.config`, `Tags/app.config`, and `TaskVisualization/app.config` contain **no** `<configSections>` and **no** settings sections at all (binding redirects only; e.g. `QuickFiler/app.config`), yet their `Settings` classes carry `[DefaultSettingValueAttribute]` on every property (e.g. `QuickFiler/Properties/Settings.Designer.cs:28,40,49,...`), so reads fall back to compiled defaults rather than throwing.

Important runtime caveat (inference): the host process is `outlook.exe`, so the AppDomain configuration file actually loaded is `outlook.exe.config`, not any `*.dll.config` produced from these `app.config` files. A class-library `app.config` is not auto-loaded at runtime. The content of the deployed `outlook.exe.config` and the per-user `user.config` is therefore the decisive input for the section-group path and is **not present in the repository**.

---

## 3. The async / fire-and-forget path (crash mechanism)

The stack (`ExceptionDispatchInfo.Throw` on a ThreadPool worker via `QueueUserWorkItemCallback.ExecuteWorkItem`) is the signature of `AsyncVoidMethodBuilder.SetException` / `AsyncMethodBuilderCore.ThrowAsync`: an `async void` method faulted, and because `SynchronizationContext.Current` captured at the method's entry was `null`, the captured exception was rescheduled onto the ThreadPool and rethrown, terminating the process. (A `Task.Run(...)` whose fault is discarded via `_ =` normally raises `TaskScheduler.UnobservedTaskException` on the finalizer thread and does not terminate the process by default; the observed stack is the `async void` pattern, not the unobserved-Task finalizer pattern.)

### 3.1 The documented startup load path is exception-observed (not the crash source)

- `ThisAddIn.Application_Startup` enqueues `_globals.LoadAsync(false)` via `IdleAsyncQueue.AddEntry(true, ...)` (`TaskMaster/ThisAddIn.cs:59-69`).
- `IdleAsyncQueue.OnApplicationIdle` is `async void` but wraps `await entry.actionAsync()` in a `try/catch (Exception)` that logs (`UtilitiesCS/Threading/IdleAsyncQueue.cs:60,67-88`). Faults from the awaited load are observed and logged, not crashed.
- The sequential/parallel phase awaits in `ApplicationGlobals.LoadSequentialAsync` / `LoadParallelAsync` are all awaited (`TaskMaster/AppGlobals/ApplicationGlobals.cs:119-224`), so faults propagate back into that protected `try/catch`.
- `ProcessStartupInboxItemsAfterReadinessHookup` attaches an `OnlyOnFaulted` continuation that logs (`TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs:30-56`).
- The IntelConfig `Task.Run` (`TaskMaster/AppGlobals/ApplicationGlobals.cs:437-441`) is awaited; and `IntelligenceConfig` reads embedded ResX resources, not `Settings.Default` (`UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs:230-246`), so it is not a settings-config throw source.

Conclusion: the primary startup load chain is exception-safe. The crashing fault escapes via a different, unobserved `async void`.

### 3.2 Confirmed anti-pattern: `async void` COM event handlers that rethrow

`TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs` contains two `async void` Outlook COM event handlers whose `catch` block does nothing but rethrow:

- `OlToDoItems_ItemChange` — `AppEvents.ReadinessHookup.cs:63-73`, body `catch (System.Exception) { throw; }`.
- `OlInboxItems_ItemAdd` — `AppEvents.ReadinessHookup.cs:75-85`, body `catch (System.Exception) { throw; }`.

These are subscribed to live `Items.ItemAdd` / `Items.ItemChange` COM events (`AppEvents.cs:120-121`, `:245`). A rethrow out of an `async void` invokes `AsyncVoidMethodBuilder.SetException`; if `SynchronizationContext.Current` is `null` at handler entry, the exception is rethrown on the ThreadPool and terminates the process — exactly the observed stack. The `catch (Exception) { throw; }` construct is strictly worse than no handler: it adds nothing and guarantees the rethrow.

### 3.3 Other unobserved `async void` / discard sites

- `RibbonViewer` has ~40 `public async void *_Click(...)` ribbon callbacks with no `try/catch` (e.g. `TaskMaster/Ribbon/RibbonViewer.cs:84,127,132,137,...`). Each is a process-terminating vector if it faults with no captured `SynchronizationContext`. These are user-initiated, not startup.
- `AddInUtilities.LaunchQuickFiler` / `LaunchSortEmail` discard the returned Task with `_ =` (`TaskMaster/AddInUtilities.cs:48,56`). These are unobserved fire-and-forget Tasks (VBA-invokable), which typically raise `UnobservedTaskException` rather than the observed ThreadPool async-void stack.

### 3.4 SynchronizationContext note

`ThisAddIn_Startup` calls `UiThread.Init(monitorUiThread: false)` (`TaskMaster/ThisAddIn.cs:28`). `UiThread.Init` creates a hidden WinForms `SyncContextForm`, captures the UI `SynchronizationContext` into the static field `UiThread.UiSyncContext`, and hides the form (`UtilitiesCS/Threading/UiThread.cs:30-53`). Capturing the context into a static field does not guarantee that `SynchronizationContext.Current` is installed on the STA at the entry of every later `async void` callback (VSTO add-in main threads frequently have no ambient `SynchronizationContext`). When it is absent at handler entry, an `async void` fault rethrows on the ThreadPool — consistent with the crash stack. This is inference from the framework's `async void` behavior combined with the captured-not-installed pattern in `UiThread`.

---

## 4. Reachability / reproducibility on HEAD

- **Confirmed reachable (crash mechanism):** the `async void` rethrow anti-pattern in `AppEvents.ReadinessHookup.cs:63-73,75-85` is present on HEAD and is a live process-terminating path for any exception raised inside `ToDoEvents.OlToDoItems_ItemChange` or `ProcessMailItemAsync` when no `SynchronizationContext` is installed. Likewise the ~40 unguarded `RibbonViewer` `async void` handlers.
- **Unknown (proximate config trigger):** the specific runtime condition that produces an empty/invalid `sectionGroupName` cannot be proven from static analysis. It depends on the deployed `outlook.exe.config` and the per-user `user.config`, neither of which is in the repository. The in-repo `app.config` files are well-formed (§2.5). What is known: reads with an undeclared section fall back to `[DefaultSettingValueAttribute]` and do not call `GetSectionGroup`; the `Save()` (user-scoped write) path does navigate the section group and is therefore the more likely trigger. What is unknown: whether a corrupted/version-mismatched `user.config`, a machine-level `outlook.exe.config`, or an empty settings group name is the actual invalid input.
- **Chronology note:** the earliest `Settings.Default` access is synchronous on the STA during `Application_Startup` (`ApplicationGlobals` ctor -> `LoadBasicMethod` -> `new AppOlObjects(...)` -> instance-field initializer `AppOlObjects.cs:446`). A throw there would surface synchronously on the STA, not as a ThreadPool rethrow. The ThreadPool signature therefore implicates a later `async void` path (a `Save()` or settings access reached from an event handler / ribbon callback), not the first synchronous read.

---

## 5. Two candidate defects, assessed separately

### 5a. Proximate cause — the call passing an invalid `sectionGroupName`

- Location: inside framework `System.Configuration` (`Configuration.GetSectionGroup`), reached via `ApplicationSettingsBase` (`Settings.Default` / `.Save()`). No first-party throw line exists.
- Bugfix scope assessment: **not suitable for a minimal targeted fix at this time.** The invalid input value cannot be reproduced from in-repo evidence, so a "smallest deterministic regression test that fails before the fix" (CLAUDE.md Bugfix Workflow step 1) cannot be authored against the true proximate trigger without the deployed `outlook.exe.config` / `user.config`. Attempting a proximate fix now would be speculative and risks a broad change to settings access. Recommend deferring until the offending config artifact is captured from a crashing machine, then adding a focused resilience guard around the specific settings access.

### 5b. Systemic cause — unobserved async fault converts a recoverable error into a process kill

- Location: `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs:63-73,75-85` (`async void` + `catch { throw; }`); secondarily the unguarded `RibbonViewer` `async void` handlers and the `AddInUtilities` discards.
- Bugfix scope assessment: **in scope for a minimal targeted bugfix.** This is the defect that turns a recoverable `ArgumentException` into an `outlook.exe` termination. It is deterministically testable via seams without touching COM/network/temp files (CLAUDE.md Bugfix Workflow + `.claude/rules/csharp.md` DI Seams; General Unit Test Policy).

---

## 6. Recommended minimal fix

Target the systemic cause (5b). Smallest defensible change:

1. In `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs`, replace the two `catch (System.Exception) { throw; }` blocks (lines 63-73 and 75-85) with a catch that logs via the existing `logger` (as `OnApplicationIdle` and `ProcessStartupInboxItemsAfterReadinessHookup` already do) and does not rethrow out of the `async void`. This removes the guaranteed ThreadPool-rethrow vector on the item-event path.

Optional hardening (same PR, only if kept minimal):
2. In `TaskMaster/AddInUtilities.cs:48,56`, observe the discarded Tasks (log faults on a continuation) instead of `_ =` discard.

A broader, still-small option is a single process-wide safety net that logs faults from `async void` handlers (for example a `TaskScheduler.UnobservedTaskException` subscription plus a WinForms/dispatcher unhandled-exception handler registered in `ThisAddIn_Startup`). This is defensible but touches the lifecycle-exempt `ThisAddIn` and does not by itself stop an `async void` ThreadPool rethrow, so the per-handler fix (item 1) remains the primary recommendation.

### Recommended regression test (deterministic, no COM/network/temp files)

- Target `AppEvents` via the existing test seam pattern (`TaskMaster.Test/AppGlobals/AppEventsTests.cs`). Drive the item-event handler path with a mocked/injected collaborator whose invoked method throws a synthetic exception, and assert that the handler observes and logs it (no exception escapes / no rethrow), rather than propagating. Use Moq for the collaborator and FluentAssertions for the assertion (`.Should().NotThrow()` around the invocation and a verified `logger` error), per CUT1/CUT2. This test fails against the current `catch { throw; }` and passes after item 1.
- Note: a regression test that reproduces the exact `sectionGroupName` value is not authorable from in-repo evidence (see 5a); the systemic test asserts fault containment, which is the behavior actually being fixed.

---

## Automation Feasibility

N/A. No recommended step requires third-party UI interaction. The fix and its regression test are pure C# using existing MSTest/Moq/FluentAssertions seams.

---

## Confirmed root cause

The crash is a two-layer defect. Confirmed: no first-party code calls any `System.Configuration` API taking a `sectionGroupName` parameter, so the `ArgumentException` is thrown inside framework `System.Configuration` code (message text matches `ExceptionUtil.ParameterInvalid("sectionGroupName")` from `Configuration.GetSectionGroup`), reached only through `ApplicationSettingsBase` (`Settings.Default` access / `Settings.Default.Save()`) — the sole first-party `System.Configuration` consumer in the repo. The exact invalid `sectionGroupName` runtime value cannot be pinpointed by static analysis because it depends on the deployed `outlook.exe.config` / per-user `user.config`, which are not in the repository (explicitly unknown). Confirmed and reachable on HEAD: the process termination is produced by the systemic defect — a recoverable settings/config exception escaping an `async void` handler that has no captured `SynchronizationContext`, most directly the `catch (System.Exception) { throw; }` anti-pattern in the two Outlook item-event handlers, which forces an `AsyncVoidMethodBuilder` ThreadPool rethrow matching the observed stack. The systemic defect, not the proximate config value, is what converts the error into an `outlook.exe` crash and is the actionable fix.

## Recommended fix scope (file count + files)

Small path — 1 production file (optionally 2):

1. `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs` (required — remove the `catch { throw; }` rethrow in `OlToDoItems_ItemChange` and `OlInboxItems_ItemAdd`; log instead).
2. `TaskMaster/AddInUtilities.cs` (optional — observe the `_ =` discarded Tasks at lines 48 and 56).

Regression test file: `TaskMaster.Test/AppGlobals/AppEventsTests.cs` (extend existing).
