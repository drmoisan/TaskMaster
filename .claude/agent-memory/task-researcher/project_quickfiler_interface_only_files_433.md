---
name: quickfiler-interface-only-files-433
description: QuickFiler interface-only .cs files emit no Cobertura class entry at all (positive control MailItemActionsAdapter); IQfcHomeController.cs exists twice (one orphan); EfcHomeController satisfies 3 IFilerHomeController members with NotImplementedException
metadata:
  type: project
---

Established 2026-08-07 while researching epic #136 child F7 (issue #433), files
`QuickFiler/Controllers/IQfcHomeController.cs` and `QuickFiler/Interfaces/IFilerHomeController.cs`.

**1. Interface-only C# files produce no Cobertura entry whatsoever.** In
`docs/features/active/2026-08-06-...-424/evidence/qa-gates/coverage-final.cobertura.xml` there is no
`<class>` element matching `QuickFiler\.[A-Za-z.]*I[A-Z]`, and the only `<class>` with
`filename="QuickFiler\Interfaces\..."` is `MailItemActionsAdapter` — a concrete class in that folder.
That adapter is the **positive control** proving the folder is instrumented, so the absence of every
interface is a property of interfaces, not of a coverage-config exclusion. Interface names do appear
in the artifact, but only inside `signature="..."` attributes of other classes' methods.

**Why:** this is the fastest way to evidence "zero executable lines" for any interface-only file in
this repo without running vstest.
**How to apply:** for any interface-only file classification question, grep a committed Cobertura
artifact for a `<class>` element with that filename, and cite a concrete class in the same folder as
the positive control. Do not settle for source reading alone.

**2. `QuickFiler.csproj` sets `TargetFrameworkVersion v4.8.1` with `LangVersion preview`.** Preview
LangVersion does NOT enable default interface implementations — the .NET Framework CLR lacks the
runtime support, so Roslyn rejects a DIM regardless. Interface-only classifications in QuickFiler are
therefore stable against future edits in a way they would not be on a .NET 8 target.

**3. Two files named `IQfcHomeController.cs` exist; one is dead.**
`QuickFiler/Controllers/IQfcHomeController.cs` (namespace `QuickFiler.Controllers`) is compiled
(`QuickFiler.csproj:304`). `QuickFiler/Interfaces/IQfcHomeController.cs` (namespace
`QuickFiler.Interfaces`, different members: `ExplCtrlr`/`FrmCtrlr`/`KbdHndlr`/`ExecuteMoves`/
`cStopWatch StopWatch`) is NOT in the csproj — it survives only in `QuickFiler.csproj.bak:244`.
**How to apply:** always use the full path when naming this file in a plan task; a name-only grep
will hit the dead one.

**4. `EfcHomeController` (epic child F8) satisfies three `IFilerHomeController` members by throwing.**
`EfcHomeController.cs:391` (`Loaded`), `:417` (`FilerQueue`), `EfcHomeController.Metrics.cs:26-29`
(`QuickFileMetrics_WRITE(string)`). `IFilerHomeController` has exactly two implementers —
`QfcHomeController` (F7, indirectly via `IQfcHomeController`) and `EfcHomeController` (F8) — so any
member addition is a cross-child change. `RibbonController.cs:42` holds the session as
`IFilerHomeController` and reads `.Loaded` at `:101`, which is a latent throw if an EFC instance is
ever assigned to that field.

**5. The three commented-out members of `IFilerHomeController` (lines 29, 34, 40) are load-bearing.**
Uncommenting any one breaks `EfcHomeController`: its `DataModel` is `internal EfcDataModel` (not
`IQfcDatamodel`), its `FormViewer` is `internal EfcViewer` (not `QfcFormViewer`), and it has no
`Iterate()` at all.

See [[qfc-home-controller-coverage-433]] and [[qfc-home-controller-metrics-433]] for the class-file
research in the same child.
