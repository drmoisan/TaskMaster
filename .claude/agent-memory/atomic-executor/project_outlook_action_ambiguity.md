---
name: project-outlook-action-ambiguity
description: Bare `Action` (non-generic) is ambiguous in files that `using Microsoft.Office.Interop.Outlook`; use System.Action
metadata:
  type: project
---

In any QuickFiler file that has `using Microsoft.Office.Interop.Outlook;`, the bare identifier `Action` is a CS0104 ambiguity between `Microsoft.Office.Interop.Outlook.Action` and `System.Action`.

**Why:** The Outlook interop namespace declares its own `Action` type. A generic like `Action<MailItem>` disambiguates fine (the interop Action is non-generic), but a non-generic `Action` field/param/`Action<Action>` does not.

**How to apply:** When introducing a `System.Action` delegate seam (e.g., `Action<System.Action> _marshalToSta`) in an Outlook-interop-importing file, fully-qualify as `System.Action`. The default-lambda body `action => UiThread.Dispatcher.Invoke(action)` and call-site `() => {...}` lambdas are fine once the field/param type is `System.Action`. See [[project-build-test-env]] for the broader QuickFiler build quirks.
