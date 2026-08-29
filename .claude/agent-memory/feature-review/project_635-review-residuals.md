---
name: 635-review-residuals
description: Issue #635 reflective-caller audit review — GO/0 blocking; how a Markdown-only evidence-audit branch was verified, and the drift-invariant acceptance-condition pattern worth reusing
metadata:
  type: project
---

Issue #635 (`bug/issue-468-residual-reflective-caller-risk-635`, head `73bd8082`) reviewed
2026-08-29: **GO, 0 blocking, 15/15 AC PASS**. Markdown-only diff, 32 paths, `NON_MD_COUNT: 0`.

**Why:** the item is an evidence-producing audit discharging issue #468 AC-16. Its claims are negative
results, so the review risk was an acceptance condition that could not have failed — not a code defect.

**How to apply:**

- **The drift-invariant acceptance-condition pattern is validated and worth reusing.** Partition B's
  hit total moved 2229 (spec base) -> 2337 (execution) -> **2474 (my review head)** purely from prose
  accretion, while both acceptance identities (`CAT_D + CAT_E = TOTAL`, `CAT_G = 0`) held at all three
  commits. Expressing the condition as a total classification with one empty category, never a hit
  count, is what made the evidence survive. A count-based condition would have been red twice over.
- **Asserted vs non-asserted figures is the right split.** `SCOPE_FILES 683`, `AC16_SIX_EXTENSION_SCOPE
  153`, `TRACKED_CS 1599`, Partition C `31` all reproduced exactly at review head, because the
  Partition A pathspec excludes `docs/*` and `.claude/*` — the only two trees this branch writes into.
  Drift in a deliberately non-asserted reference value is not a blocking finding.
- **Verify the non-vacuity control actually discriminates.** P1-T2 ran the identical pathspec for a
  present token and returned 13 hits / 4 files, two of them (`QuickFiler/Notes/notes_interface_hierarchy`,
  extensionless; `QuickFiler.csproj.bak`) unreachable by any extension-based search. That is what makes
  a zero a measurement rather than an artefact.
- **Three claims had narrower supporting measurement than the claim; all held under the broader test I
  ran.** (1) inventory used `Delegate.CreateDelegate`/`Activator.CreateInstance` vs the spec's bare
  `CreateDelegate`/`Activator.` — bare forms also prod=0; (2) `dynamic` late binding never enumerated as
  a mechanism, and it is the one class in the same family as the stated limit (runtime-assembled names)
  — verified absent (1 prod comment hit, 0 in test tree); (3) assembly `ComVisible(false)` asserted to
  bind every type — verified no per-type `ComVisible(true)`/`ClassInterface`/`ProgId`. Run the broader
  form before accepting a narrowed pattern.
- **AC-9 six-vs-eight:** spec names six variable-arg sites, mechanical derivation yields eight (7
  `GetField(` + 1 `GetMethod(`), so no six-subset is identifiable. Recording (not editing the approved
  spec, not silently picking a subset) was correct; AC discharged by superset. **Owed: maintainer
  amendment of the spec baseline figure at merge.**
- **The `//` ordering row is real:** `QuickFiler/Legacy/QuickFileController.cs:20` contains
  `MethodBase.GetCurrentMethod()` but its first non-whitespace token is `//`, so ordered tests put it in
  L3 not L1. Verified at character level.
- **cwd hazard recurred** (see [[review-worktree-differs-from-session-cwd-mirror-artifacts]]): hook
  exits 0 from the review worktree but **fails from the session cwd** `TaskMaster-wt/2026-08-29T00-11`,
  which lacks the feature folder. Mirrored the 3 artifacts there; both cwds then exit 0. Hook payload
  key is **`output`**, not `agent_output`.
- **Coverage gate correctly not exercised:** no `artifacts/pr_context.summary.txt` -> `changedLanguages`
  empty -> only the 3 path checks run. Zero changed files in every coverage language, so no coverage
  artifact was emitted; emitting one would have fabricated a measurement and exposed an unrelated
  repo-wide threshold.
