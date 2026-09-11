---
name: rerecord-observed-radius-after-merge
description: End-to-end recipe for re-recording an item's blast radius from its merged branch diff — the anchor that does not return empty, the exact Get-BlastRadiusFromObservedPaths signature, the line-slice checkpoint swap, and the three escape classes actually observed
metadata:
  type: reference
---

After merging an item, re-record its `blast_radius` from the merged branch diff. Four mechanics, each
of which cost a round the first time.

**1. Anchor to the PRE-MERGE main tip, never to the current one.** `git diff --name-only A...B` is
`merge-base(A,B)..B`. Once the item is merged, its head is an ancestor of `origin/main`, so the
merge-base IS the head and `origin/main...<head>` returns EMPTY. Use the tip main had immediately
before this merge — the previous item's merge commit — as the left operand. Keep a running note of
it; it is the `merge_commit_sha` of whatever merged last.

**2. The library signature is not what the name suggests.** Four corrections, all found by trial:

- the parameter is `-ObservedPaths`, NOT `-Paths`;
- `-Config` is MANDATORY and takes the PARSED object, so pass
  `(Get-Content -Raw config/blast-radius.json | ConvertFrom-Json)` directly;
- there is NO `Get-BlastRadiusConfig` helper — `BlastRadiusConfig.psm1` exports only `Get-Config*`
  and `Resolve-*` primitives, so do not go looking for a loader;
- `-ComputedAt` takes the stamp; there is no `-DateKind` on this function, unlike
  `Test-BlastRadiusConflict` (see [[blast-radius-powershell-calling-convention]]).

Working call, after `Import-Module <abs>/.claude/lib/blast-radius/BlastRadius.psm1 -Force`:

```
Get-BlastRadiusFromObservedPaths -ObservedPaths $observed -Config $cfg -ComputedAt '<stamp>'
```

**3. Swap the block by LINE SLICE, not by JSON re-serialization.** A `blast_radius` block runs 30 to
95 lines. Re-serializing the whole checkpoint through `ConvertTo-Json` reformats 1500 unrelated lines
and risks the PowerShell ordering traps. Instead read `[System.IO.File]::ReadAllLines`, find the
start by matching `^\s+"blast_radius": \{$` PLUS a distinctive path two or three lines below it,
find the end by matching the NEXT sibling key, splice, and `WriteAllLines`. Print the first and last
replaced line so the slice is auditable. This is exact and touches nothing else.

**4. Feed the library the code and non-doc paths, then ADD the feature-folder glob by hand.** Do not
pass 60 feature-document paths through it; represent them with the item's own
`docs/features/active/<folder>/**` glob, matching what every item on the run declares. Add the glob
to the returned `paths` list and write the result. **Never re-normalize afterwards** — that filter
strips `.claude/agent-memory` paths straight back out, which is exactly how a declared set of them
gets silently lost.

**Three escape classes actually observed on run `bugs-2026-09-06`, all benign, none a drift event:**

- an agent-memory lesson the child committed deliberately and named in its report (item 810);
- three `docs/features/potential/` follow-up entries a child wrote instead of filing issues, which is
  what its remediation cycle was closed on (item 811);
- none at all (item 812), whose branch nonetheless legitimately edited a SIBLING item's `spec.md`
  that its declared radius already covered.

Record every escape in prose. Do NOT write `drift_events[]`: it is F8-owned, this agent never writes
it, and the drift-detection command line does not exist in this repository. Check whether the escape
creates contention with any non-terminal item before calling it benign — on this run none did,
because the peers were already merged.
