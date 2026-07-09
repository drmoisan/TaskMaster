---
name: project-outlook-action-ambiguity
description: Bare `Action` (non-generic) AND bare `Exception` are ambiguous in files that `using Microsoft.Office.Interop.Outlook`; use System.Action / System.Exception
metadata:
  type: project
---

In any file that has `using Microsoft.Office.Interop.Outlook;`, the bare identifiers `Action` and `Exception` are CS0104 ambiguities between the `Microsoft.Office.Interop.Outlook.*` type and the `System.*` type. (Confirmed for `Exception` in `TaskMaster/ThisAddIn.cs` during issue #208: a bare `catch (Exception ex)` failed the analyzer build with CS0104 'Exception' is an ambiguous reference between Microsoft.Office.Interop.Outlook.Exception and System.Exception.)

**Why:** The Outlook interop namespace declares its own `Action` and `Exception` types. A generic like `Action<MailItem>` disambiguates fine (the interop Action is non-generic), but a non-generic `Action` field/param and a bare `catch (Exception)` do not.

**How to apply:** In an Outlook-interop-importing file, fully-qualify as `System.Action` (delegate seams, e.g. `Action<System.Action> _marshalToSta`) and `System.Exception` (catch clauses, e.g. `catch (System.Exception ex)`). Lambda bodies and `() => {...}` call sites are fine once the declared type is `System.*`. This ambiguity does NOT surface until the analyzer/type-check msbuild step — csharpier format does not catch it — so expect a P2 loop restart if you write a bare `Exception`/`Action` in such a file. See [[project-build-test-env]] for the broader build quirks.
