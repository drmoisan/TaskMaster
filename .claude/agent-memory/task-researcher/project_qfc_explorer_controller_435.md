---
name: qfc-explorer-controller-435
description: Issue #435 (epic #136 child F6) research on QfcExplorerController.cs — zero irreducible remainder found; QuickFiler CAN Moq internal interfaces (IVT is in QfcHighConfidencePreFilter.cs, not AssemblyInfo.cs)
metadata:
  type: project
---

`QuickFiler/Controllers/QfcExplorerController.cs` (#435, epic #136 child F6) was researched
2026-08-07 and found to have **no irreducible remainder** — its `[ExcludeFromCodeCoverage]` should be
removed outright, with zero exemption proposed to F1's ledger.

**Why:** every COM dependency reaches the file through the already-injected `IApplicationGlobals` /
`IFilerHomeController` (both public and Moq-able), and both construction sites are already behind
`Func<>` factory seams in sibling-owned files (`QfcHomeController.cs:180`, F7;
`EfcHomeControllerDependencyFactories.cs:155`, F8), so the constructor signature must be preserved
and no sibling production file needs editing. Five of six `IQfcExplorerController` members have zero
production callers.

**How to apply:** three constraints found here generalize to the rest of epic #136 and are worth
re-checking before proposing any QuickFiler seam:

1. **`InternalsVisibleTo("DynamicProxyGenAssembly2")` IS compiled into the QuickFiler assembly, so
   Moq CAN mock `internal` QuickFiler interfaces.** Look for assembly-level attributes in ordinary
   controller files, not just `Properties/AssemblyInfo.cs`: the attribute is declared at
   `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11`, compiled via `QuickFiler.csproj:322`.
   `QuickFiler/Controllers/QfcHomeController.cs:18` likewise declares
   `InternalsVisibleTo("QuickFiler.Test")`. An `internal` interface seam is therefore permitted and,
   under the repo seam hierarchy (interface seam > injectable delegate > adapter), preferred over a
   delegate property where an interface expresses the collaboration better.

   > This corrects the original 2026-08-07 research claim, which asserted the opposite after searching
   > only `Properties/AssemblyInfo.cs` and `Legacy/`. Verified by the orchestrator against source.
   > Lesson that generalizes: `grep -r InternalsVisibleTo <project>/` across ALL files, then confirm
   > each hit is in the csproj `<Compile Include>` list — assembly-level attributes are not required
   > to live in `AssemblyInfo.cs`.
2. **`QuickFiler.csproj` and `QuickFiler.Test.csproj` are shared, unassigned, non-SDK projects** with
   explicit `<Compile Include>` lists. Every child adding a file must edit them; this is the epic's
   most likely integration-branch conflict source and is not covered by the Feature File Assignments.
3. **`Outlook.Views` is the only interop type in this area with no `Mock<>` precedent anywhere in the
   repo** (`Explorer`, `View`, `CommandBars`, `MailItem`, `MAPIFolder`, `Selection`, `TableView` all
   have proven precedent). Route around it rather than depending on its mockability.

Two latent defects were recorded for promotion to their own issues rather than fixed in a coverage
child: `ExplConvView_Cleanup` is `throw new NotImplementedException()` on a public interface member,
and `OpenQFItem` calls `ActiveExplorer()` a second time at line 140 instead of reusing the field
captured in the constructor.

Related: [[promote-latent-defects-to-issues]]
