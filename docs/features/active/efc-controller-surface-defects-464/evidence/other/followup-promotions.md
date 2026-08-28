# [P11-T14] Follow-up promotions

Timestamp: 2026-08-28T02-12
Task: [P11-T14]
Command: `gh issue list --repo drmoisan/TaskMaster --state all --search "<term>"` for each item;
`ls docs/features/potential/promoted/`; tool-availability check for the promotion MCP tool
EXIT_CODE: 0

## Promotion route actually available in this execution environment

`spec.md` §`Follow-ups to promote as separate issues` lists six items. The repository's promotion
lifecycle (`feature-promotion-lifecycle`) runs a potential document under `docs/features/potential/`
through the `mcp__drm-copilot__potential_to_issue` MCP tool, which creates the GitHub issue and writes
the promotion receipt.

**That route is not available to this executor, for two independent reasons, both recorded so the claim
is auditable rather than asserted:**

1. **The promotion MCP tool is not in this agent's tool set.** The only MCP tools exposed to this session
   are `run_poshqc_format`, `run_poshqc_analyze`, `run_poshqc_test` and `run_poshqc_analyze_autofix`.
   There is no `potential_to_issue` tool and no other promotion tool.
2. **Writing a potential document would breach this plan's own scope gate.** `[P11-T17]` requires that
   `git diff --name-only <BASE>` list only the nine C1 writable paths, the three `EfcViewer3.*`
   deletions, and paths under `docs/features/active/efc-controller-surface-defects-464/`. A new file
   under `docs/features/potential/` is outside that allowlist and would make `[P11-T17]` fail, and
   `[P9-T2]`'s allowlist likewise excludes it.

`gh` is installed and authenticated (`drmoisan`), so raw `gh issue create` is technically possible. It
was **deliberately not used**: it bypasses the potential-document stage the lifecycle requires, invents a
promotion path this plan does not authorise, and the orchestrator that delegated this batch owns fan-in
and follow-up promotion. Creating issues outside the lifecycle also risks duplicates, which the
lifecycle's receipt mechanism exists to prevent.

Each row below therefore records **the reason creation is unavailable and the potential-document path**,
which is the branch `[P11-T14]`'s acceptance explicitly provides for. **This is an outstanding handoff
item, not a completed promotion**, and it is reported as such at plan completion.

### Duplicate check

Before recording, `gh issue list --state all --search` was run for each item. Every search returned an
empty result set (`[]`), so none of the six is already tracked and none of the recorded rows would be a
duplicate when the orchestrator promotes it.

## The six items

| # | Item | Source | Existing issue | Status | Potential-document path to create |
|---|---|---|---|---|---|
| 1 | Delete the seventeen other uncompiled `QuickFiler/Viewers/*.cs` orphans, plus `QuickFiler/Legacy/**` and `QuickFiler/Notes/**`, all wholly uncompiled. Repository hygiene, not a bug fix. | `spec.md:1145-1147` | none found | **NOT CREATED** — promotion tool unavailable | `docs/features/potential/2026-08-28-quickfiler-uncompiled-viewer-orphans-and-legacy-trees.md` |
| 2 | Decide the intended `KbdActions<>` indexer-setter contract (upsert versus assign-if-present) and align it. Belongs with the owner of `KbdActions.cs` (feature #444). | `spec.md:1148-1149` | none found | **NOT CREATED** | `docs/features/potential/2026-08-28-kbdactions-indexer-setter-contract-decision.md` |
| 3 | Consolidate the fifth banner-prefix constant at `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:16` with `BreadcrumbRowBuilder.BannerPrefix`. | `spec.md:1150-1151` | none found | **NOT CREATED** | `docs/features/potential/2026-08-28-consolidate-banner-prefix-constants.md` |
| 4 | Fix the shared `ProcessCmdKey` over-claim in the QFC twin (`QfcFormViewer.cs:56-73` / `QfcFormKeyHandler.cs:18`), which this feature deliberately does not touch. | `spec.md:1152-1153` | none found | **NOT CREATED** | `docs/features/potential/2026-08-28-qfc-twin-processcmdkey-alt-chord-over-claim.md` |
| 5 | Correct `484/spec.md`'s `ToggleNavigation(bool)` retention rationale (R-1) and `444/spec.md`'s `CharActions` reachability claim (R-2). | `spec.md:1154-1155` | none found | **NOT CREATED** | `docs/features/potential/2026-08-28-upstream-spec-corrections-484-r1-and-444-r2.md` |
| 6 | Resolve the coverage-threshold discrepancy between CLAUDE.md (80% repository-wide / 90% new code) and `.claude/rules/general-unit-test.md` (85% line / 75% branch). | `spec.md:1156-1157` | none found | **NOT CREATED** | `docs/features/potential/2026-08-28-coverage-threshold-discrepancy-claude-md-vs-rules.md` |

## A seventh item this batch discovered — the RC7 residual

Not in `spec.md`'s list of six, because it was discovered on this execution base after `spec.md` was
written. It is recorded here so it is promoted rather than lost, and it is the item the base-drift
addendum instructs be "reported and promoted, not absorbed".

| # | Item | Status | Potential-document path to create |
|---|---|---|---|
| 7 | `QuickFiler/Controllers/EfcSelectionGuard.cs` declares `BannerPrefix` as three `=` characters, a **third** arity variant, while both row producers (`BreadcrumbRowBuilder.cs:19` and `FolderSuggestionTree.cs:16`) use four. The code comment near `QuickFiler/Controllers/EfcFormController.cs:325` still describes a four-`=` rejection that `EfcSelectionGuard` does not implement. | **NOT CREATED** — same reasons | `docs/features/potential/2026-08-28-efcselectionguard-banner-prefix-arity-and-stale-comment.md` |

**Why this feature did not fix it, stated for the promoted item.** `EfcSelectionGuard.cs` belongs to
merged sibling **#614** and is outside this feature's owned set; `[P9-T2]` confirms this feature's diff
does not contain it. Widening `EfcSelectionGuard.BannerPrefix` to four characters would *relax* a filing
guard #614 deliberately tightened — a three-`=` row would become filable — in a file this feature does not
own, on a merged sibling's behaviour, to gain nothing a user can observe, since no producer emits a
three-`=` row. `EfcSelectionGuardTests.cs` asserts only on a four-`=` banner, so the change would pass
tests while silently relaxing a merged guard. Item 3 above is the correct home for the consolidation, and
this item is the correct home for the stale comment.

## No follow-up was implemented inside this feature's diff

Verified rather than asserted:

| Follow-up | Path it would have to touch | In this feature's diff? |
|---|---|---|
| 1 | `QuickFiler/Viewers/*.cs` orphans, `QuickFiler/Legacy/**`, `QuickFiler/Notes/**` | **no** — `[P9-T2]`'s 98-path result contains only the 9 C1 paths, the 3 authorised `EfcViewer3.*` deletions and this feature's documentation |
| 2 | `QuickFiler/Controllers/KbdActions.cs` | **no** — `[P9-T3]` records zero paths matching `KbdActions` under both bases |
| 3 | `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs`, `BreadcrumbRowBuilder.cs` | **no** — `[P9-T3]` records zero paths matching `BreadcrumbRowBuilder`; `FolderSuggestionTree.cs` is absent from the 98-path result |
| 4 | `QuickFiler/Viewers/QfcFormViewer.cs`, `QuickFiler/Controllers/QfcFormKeyHandler.cs` | **no** — `[P9-T3]` records zero for both under both bases |
| 5 | `docs/features/active/qfc-item-controller-defects-484/spec.md`, `.../quickfiler-keyboard-action-defects-444/spec.md` | **no** — neither appears in the 98-path result |
| 6 | `CLAUDE.md`, `.claude/rules/general-unit-test.md` | **no** — neither appears in the 98-path result; policy files are read-only to this agent |
| 7 | `QuickFiler/Controllers/EfcSelectionGuard.cs` | **no** — absent from the 98-path result; the addendum forbids editing it |

The three EFC deletions this feature **did** perform (`EfcViewer3.*`, the dead conversation-expanded
handler, the `RegisterActions`/`ToggleExpansion` dead path) are remedies RC11, RC4 and RC6 inside the
approved scope, not absorptions of follow-up item 1.

Output Summary: All six `spec.md` follow-up items, plus a seventh discovered on this base (the RC7
`EfcSelectionGuard` three-`=` `BannerPrefix` residual and the stale four-`=` comment at
`EfcFormController.cs:325`), are recorded with their source citation and the potential-document path that
should be created. **None was created as a GitHub issue**: the `potential_to_issue` MCP tool is not in
this agent's tool set, and writing a potential document under `docs/features/potential/` would breach the
`[P9-T2]` and `[P11-T17]` scope gates. A `gh issue list --state all --search` duplicate check returned an
empty set for all seven, so none is already tracked. Promotion is an **outstanding handoff item for the
orchestrator**, reported at plan completion. It is verified path-by-path that **no follow-up was
implemented inside this feature's diff**.
