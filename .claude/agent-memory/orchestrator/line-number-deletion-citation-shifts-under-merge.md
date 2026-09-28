---
name: line-number-deletion-citation-shifts-under-merge
description: A plan task that removes a line BY LINE NUMBER from a shared .csproj is silently retargeted by an upstream merge that inserts lines above it, and the usual "count falls by exactly one" gate cannot detect the wrong-line deletion
metadata:
  type: feedback
---

When a plan instructs a **deletion by line number** in a file a sibling item also edits, re-derive that
line number against the post-merge tree before delegating. Do not trust the count gate to catch it.

**Why:** on item 872 the plan said "the line to remove is the single-line self-closing element at line
496 whose text is `<Compile Include="Threading\ProgressTrackerAsync_Tests.cs" />`". The fix for issue
877 (PR 880) inserted a three-line `<Compile>` element for a shared `TestSupport` source higher in the
same `<ItemGroup>` of `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. Every item below it shifted down by
three, so line 496 became `ProgressTracker_Tests.cs` — a live test file — and line 499 became the
intended target.

The acceptance condition was *"exactly one line is removed, and the count of lines matching
`<Compile Include=` falls by exactly one relative to the P0-T12 baseline."* That gate passes just as
well when the WRONG item is deleted: one line removed either way, count down by one either way. The
gate is not vacuous — it can fail — but it is blind to the only failure mode that matters here. The
neighbour clause ("lines 495 and 497 are unchanged") is blind for the same reason: it names the
neighbours of the stale position, which are themselves stale.

A legacy non-SDK project compiles nothing it does not list, so the wrong deletion surfaces as a
silently missing test class, not a build error. See [[merging-main-invalidates-plan-base-anchor]] for
the sibling case where the merge breaks the diff *anchor* rather than a line citation.

**How to apply:**

1. After merging main, grep the shared project file for the quoted item TEXT and compare the returned
   line number against the plan's citation. The text is invariant under insertion above it; the line
   number is not.
2. Correct the citation and its neighbour citations in the plan, and add an instruction that the
   executor locates the item **by its quoted text** and confirms the line number matches before
   removing it. Text-first with a line-number confirmation is falsifiable; line-number-first is not.
3. Treat any count-only gate over a shared file as insufficient on its own. Pair it with an identity
   assertion — a zero-occurrence check on the removed identifier (`TrackerReferences: 0`) plus an
   anchored `--name-status` diff — so that removing the wrong item fails something.
4. Sweep the whole plan, not just the deletion task: the same number was pinned in three places here
   (the baseline task P0-T13, the edit task P1-T12, and the verification task P2-T10).
