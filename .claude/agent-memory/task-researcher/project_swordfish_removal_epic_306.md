---
name: swordfish-removal-epic-306
description: Swordfish-removal epic child F1 (#306) dictionary-lineage migration research findings and scoping
metadata:
  type: project
---

Epic "swordfish-removal" replaces vendored Swordfish types with Swordfish-free equivalents, split
into children: F1=dictionary lineage (#306), F2=collection/stack, F3=ScoSortedDictionary,
F5=UtilitiesSwordfish deletion + ProjectReference/.sln + IScoCollection migration. F1 must NOT touch
F2/F3/F5 scope.

**Why:** long-running multi-feature epic; scope boundaries matter to avoid cross-feature bleed.

**How to apply:** for F1 (#306), the load-bearing finding is that legacy `ScoDictionary`
(Swordfish-based, `SCODictionary.cs`) writes flat `{"key":value}` JSON with NO `$type`
(TypeNameHandling commented out at SCODictionary.cs:227). `ScoDictionaryNew` via the DEFAULT
`Static.Deserialize(fileName, folderPath)` path (GetDefaultSettings = TypeNameHandling.Auto, no
converter) round-trips the SAME flat shape, so re-pointing preserves on-disk compatibility with no
binder/converter work. The ONLY thing that breaks compatibility is the globals path
`GetSettingsJson<T>(globals)` which registers `ScoDictionaryConverter` + `PreserveReferencesHandling.All`
and emits the wrapper `{CoDictionary,RemainingObject}` shape — must be avoided for these dictionaries.
Persisted (need compat tests): DictRemap, FilteredFolderScraping, FolderRemap, SubjectMap Encoder.
In-memory only (pure type swap): SubjectMap Decoder, FolderScorer._folderNameScores (epic wrongly
lists FolderScorer scores as persisted). Ripple consumers: EmailDetails/EmailDetailsWrapper take
`IScoDictionary<string,string> dictRemap` params. PeopleScoDictionary.cs is fully commented-out (inert).
Full research: docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/research/research-dictionary-lineage.2026-07-10T20-16.md
