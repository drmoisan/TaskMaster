---
name: graph-usings-removable-and-models-name-collisions-973
description: The six `using Microsoft.Graph.*` directives in UtilitiesCS are all REMOVABLE (verified 2026-10-03 against Microsoft.Graph 6.7.0 XML docs); Microsoft.Graph.Models exports non-generic List/KeyValuePair/Application/Folder/OutlookItem/Message/Store(TermStore) so importing it into Outlook-interop files is a CS0104 hazard; the package must stay because UtilitiesCS.Test reads Microsoft.Graph.xml as a fixture; reflog is the fallback clock when Bash is disabled
metadata:
  type: project
---

Verified 2026-10-03 for the #973 fold-in (maintainer directive to strip unused Graph usings).

1. **All six directives bind nothing.** `StoreWrapper.cs:7` (Models.TermStore), `Triage_OlLogic.cs:10` (Models),
   `CategoryClassifierGroup.cs:11-12`, `ManagerAsyncLazy.cs:18`, `FolderMinimalWrapper.cs:6` (four
   request-builder namespaces with 1-3 types each). Checked every type name and extension call in each
   file against `packages/Microsoft.Graph.6.7.0/lib/netstandard2.0/Microsoft.Graph.xml`; the only static
   class in `Microsoft.Graph.Models` is `DateTimeTimeZoneExtensions`. `Microsoft.Graph.Core` contributes
   zero types to those namespaces. The maintainer's "Triage_OlLogic may genuinely use Models" caveat was
   false: `Explorer`, `View`, `MailItem` have no Graph counterpart.
2. **Collision hazard worth remembering:** `Microsoft.Graph.Models` exports NON-GENERIC `List` and
   `KeyValuePair` (different arity, so `List<T>` is safe) and `Application`, `Folder`, `OutlookItem`,
   `Message`, `Attachment`, `Recipient`, `Group`; `Models.TermStore` exports `Store`, `Set`, `Term`,
   `Group`. Any file that also imports `Microsoft.Office.Interop.Outlook` and uses those simple names
   gets CS0104. Never "widen" a Graph using to `Microsoft.Graph.Models`.
3. **The Graph package cannot go with the usings.** `UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs`
   `GetLargeTextFixture` walks up to `packages/` and opens `Microsoft.Graph.*/lib/netstandard2.0/Microsoft.Graph.xml`
   as a multi-megabyte fixture; plus References in UtilitiesCS(.Test).csproj and redirects in 2 configs.
4. **Unused usings are invisible to every gate here** (no `GenerateDocumentationFile`, `.editorconfig`
   suggestion ceiling, CS8019 hidden) — see [[console-out-and-rs0030-promotion-826]]. Only a Grep proves
   removal; the Rebuild gates prove nothing broke.
5. **Clock fallback when Bash is disabled:** read `.git/worktrees/<id>/logs/HEAD` (or the branch reflog)
   last line; its epoch is the latest clock reading available and is a lower bound for a filename stamp.
   State the derivation in the file.

**Why:** the spec/plan for #973 said "no .cs file changes" (AC16, plan P0-T15/P4-T4/P4-T12); the fold-in
contradicts that and the planner must amend before execution. `CategoryClassifierGroup.cs` is 539 lines
(over the 500 limit) before and after — pre-existing, do not attribute to the item.

**How to apply:** when asked whether a `using` is removable, enumerate the namespace's top-level types
from the shipped XML docs and grep the file's identifiers against them; name the static classes
explicitly; then rely on the Grep (red before / 0 after) plus the compile gates as the fail-able checks.
