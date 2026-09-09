---
name: cs1769-forces-reflection-for-outlook-returning-apis
description: A UtilitiesCS method returning Task<Outlook.X> cannot be awaited from UtilitiesCS.Test at all (CS1769, embedded interop type as generic argument) — a plan that says "call it on a mocked Explorer" is unimplementable as written and needs a reflective helper
metadata:
  type: project
---

Any `UtilitiesCS` API whose return type is `Task<T>` where `T` is an Outlook interop type — for
example `OlTableExtensions.GetTableInViewAsync`, which returns `Task<Outlook.Table>` — **cannot be
awaited from `UtilitiesCS.Test`**. The build fails with:

    error CS1769: Type 'Task<Table>' from assembly 'UtilitiesCS, ...' cannot be used across
    assembly boundaries because it has a generic type argument that is an embedded interop type.

**Why:** both projects embed the Outlook interop types rather than referencing a shared PIA, so
`Outlook.Table` is a distinct type per assembly and cannot cross the boundary as a generic type
argument. This is the same CS1769 constraint that `DfDeedle.DefaultTableEtl` is declared over
`object` to avoid, and it is why the four existing `GetTableInViewAsync` binding sites in
`OlTableExtensions_Tests.cs` all go through the reflective `InvokeAsyncResult` helper rather than
calling the method directly. Reflection is *required*, not stylistic.

**How to apply:**
- Bind by name plus an explicit parameter-`Type[]`, invoke, cast the result to the **non-generic**
  `Task`, `await` that, then read `task.GetType().GetProperty("Result").GetValue(task)`. The
  non-generic `Task` and `object` both cross the boundary fine.
- `typeof(Outlook.Explorer)` in a `Type[]` is fine; only the *generic argument* position is
  rejected.
- The explicit `Type[]` is a hidden coupling to the signature. A plan that adds a parameter to the
  production method in a later phase silently breaks every reflective binding, including ones the
  plan does not list — budget a micro-action to update them, the same way the plan already budgets
  one for the direct call sites.
- Declare the array once as a `private static Type[] SignatureTypes => ...` in a new test file so a
  future signature change is a one-place edit.

Encountered on issue #825 (2026-09-09): the plan's P2-T1 said the new test "calls
GetTableInViewAsync on a mocked Explorer", which does not compile; the fix was a local reflective
helper mirroring `InvokeAsyncResult`.

Related: [[project_test_file_name_vs_partial_class_name]], [[project_outlook_action_ambiguity]]
