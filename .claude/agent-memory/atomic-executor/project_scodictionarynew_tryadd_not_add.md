---
name: scodictionarynew-tryadd-not-add
description: Retargeting Sco* stand-in tests to ScoDictionaryNew needs .TryAdd not .Add(key,value); build-fail otherwise
metadata:
  type: project
---

When a swordfish-removal feature retargets a SmartSerializable/generic stand-in test from the retired `ScoDictionary<TKey,TValue>` to the successor `ScoDictionaryNew<TKey,TValue>`, the seeding call `.Add(key, value)` fails to compile (CS1061: no `Add`).

**Why:** the retired `ScoDictionary` exposed a dictionary-style `Add(key,value)`, but `ScoDictionaryNew` derives from `ConcurrentObservableDictionary<,>`, which exposes `TryAdd(key, value)` / `AddOrUpdate(...)` instead — no two-arg `Add`. The `ScDictionary` stand-ins already in the same test files use `.TryAdd(...)`, confirming the successor API.

**How to apply:** on #315 (legacy-scodictionary-removal) this surfaced only at the analyzer build (not preflight) because both types existed until the delete phase. When retargeting positive round-trip stand-ins to `ScoDictionaryNew` in any epic child (F2/#307, F4, F5/#308), change `.Add("k", v)` to `.TryAdd("k", v)` in the same edit to avoid a build-fail/loop-restart cycle. See [[project_swordfish_removal_epic_incidental_coverage_sideeffect]] for the parallel coverage side effect.
