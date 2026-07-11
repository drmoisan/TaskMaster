---
name: project-swordfish-epic-f5-blocked-on-old-scodictionary
description: RESOLVED — swordfish-removal epic F5 (#308) once WI-0-halted on the OLD ScoDictionary Swordfish base (ScoDictionaryNew was a decoy); remediation child #315/PR #316 deleted it and F5 then completed
metadata:
  type: project
---

RESOLVED (2026-07-11). Historical record of the swordfish-removal epic's terminal-feature blocker and its resolution.

**What happened:** F5 (swordfish-interface-project-teardown, #308) removes the vendored `UtilitiesSwordfish` project. Its WI-0 preflight (plan P0-T2 / spec AC-1) HALTs if any live category-A Swordfish reference remains in first-party production `*.cs`. At integration tip `db6dc0e9`, F1 (swordfish-dictionary-lineage, #306) had introduced a clean `ScoDictionaryNew<TKey,TValue>` (first-party base) but had NOT re-based or retired the OLD `ScoDictionary<TKey,TValue>` in `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs`, which still derived from `Swordfish.NET.Collections.ConcurrentObservableDictionary` (via `using Swordfish.NET.Collections;`, the only in-scope import for that name — the enclosing namespace `UtilitiesCS.ReusableTypeClasses` does NOT reach the descendant first-party namespace). F5 correctly HALTed at WI-0 rather than absorbing that out-of-scope migration.

**Resolution:** A separate remediation child, issue #315 (legacy-scodictionary-removal, PR #316), deleted `SCODictionary.cs` outright and retired the legacy class, merged into `epic/swordfish-removal-integration` at commit `90c1ac03`. On the F5 re-attempt (integration tip `1b65f7a7`), WI-0 CLEARED and F5 completed the teardown.

**How to apply (durable lesson):** The presence of a `*New`-suffixed clean type (like `ScoDictionaryNew`) does NOT mean a lineage migration is finished — the OLD type can still bind to the vendored base. When verifying a "remove vendored library" gate, grep for the OLD `class <Name><` base declaration and its `using <Vendor>;` import in the legacy file, not just the existence of the clean replacement. In legacy non-SDK projects (no `ImplicitUsings`/`global using`), unqualified base-type name resolution is driven entirely by explicit `using` directives, so a single stale `using` can silently keep a live vendored binding. See [[remediation-loop-strict-handoff]] and [[epic-children-need-full-lifecycle-and-prs]] for epic-child execution rules.
