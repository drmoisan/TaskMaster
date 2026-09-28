---
name: winforms-control-field-installs-synccontext-and-deadlocks-await
description: A WinForms control constructed in an MSTest field initializer installs WindowsFormsSynchronizationContext on the test thread, so the first genuine await in the code under test hangs forever; clear the context in TestInitialize
metadata:
  type: project
---

Constructing any `System.Windows.Forms` control (e.g. `new TableLayoutPanel()`) in an MSTest test
class field initializer installs a `WindowsFormsSynchronizationContext` on the test thread. The
first *genuinely* asynchronous await in the code under test — one that actually yields, such as
`await Task.Run(...)` — then captures that context and posts its continuation back to the test
thread, which no unit-test host pumps. The await never resumes and vstest hangs with no output.

**Why:** observed on issue #871 P4-T8 (QuickFiler.Test, `QfcQueueEnqueueTests`). Three earlier
tests in the same class passed because none reached a genuine await: argument-guard cases throw
before the first await, and the rest are synchronous. The fourth case, which drove the real enqueue
flow, hung indefinitely and took a 10-minute tool timeout with it.

**How to apply:**
- Diagnose with `"/Blame:CollectHangDump;TestTimeout=90000"` appended to the vstest command. The
  console names "The test running when the crash occurred", which distinguishes "my new test hangs"
  from "an interaction with an existing test".
- Fix with a `[TestInitialize]` that calls `SynchronizationContext.SetSynchronizationContext(null)`.
  Field initializers run before `[TestInitialize]`, so this removes the context the control
  installed before any test body runs, and continuations complete on the thread pool. No sleep, no
  pump, no wall-clock wait — it stays inside the repo test policy.
- `FormatterServices.GetUninitializedObject(typeof(SomeControl))` does NOT install the context,
  because no constructor runs. Only a real `new` on a control does.

Related: [[project_configcontroller_sta_pump_deadlock]],
[[project_winformspumphost_tests_load_flaky]], [[project_uithread_dispatcher_static_swap_race]].
