---
name: project-489-partn-reroute-amendment-seams
description: "#489 mid-execution plan amendment: rerouting appends to PartN files requires a parent `partial` edit the caller didn't know about; spec amendment notes shift every AC line citation"
metadata:
  type: project
---

Mid-execution amendment of `plan.2026-08-25T01-04.md` (feature `itemviewer-surface-defects-489`) rerouted five new tests into `EventWiringTests.Part2.cs` / `MailActionsTests.Part2.cs` after siblings 484/444/493 grew the parents to 499/498 lines.

**Why:** the 500-line ceiling made Phases 1/7 unexecutable as authored; the repo remedy is the `InitializationTests.Part2.cs` precedent.

**How to apply:**
- Before writing any PartN continuation task, READ the parent class declaration. Both #489 parents were `public class`, NOT `partial`, despite the delegation asserting "continues the same partial test class". The plan must add a line-neutral `public class` → `public partial class` edit to the parent, and that edit surfaces in every diff gate: the parent stays in the scope-lock list (1 added / 1 deleted) even though no test is appended to it.
- PartN convention (per #424/#230): `[TestClass]` on the parent ONLY; a second `[TestClass]` on another partial declaration is a compile error. Part2 replicates the parent's exact `using` set (MailActionsTests imports `Microsoft.Office.Interop.Outlook` and not `System`, so `System.*` stays fully qualified).
- Sequential csproj appends: each new `<Compile Include>` anchors on the quoted text of the THEN-current block tail (SeamFactoryTests → EventWiringTests.Part2 → ThemeMarshallingTests → MailActionsTests.Part2), and each task's count gate is baseline+1/+2/+3/+4 in execution order (P1-T2, P1-T4, P5-T2, P7-T3).
- A dated spec-amendment note inserted below the § Acceptance Criteria Authority paragraph shifts EVERY criterion line number (+13 here). The plan's AC index table and all 62 P12 check-off citations must be renumbered; process `(line NNN,` replacements in DESCENDING original order so a new value never collides with a not-yet-replaced original. Quote the original criterion in the note WITHOUT its `- [ ]` prefix so checkbox-count gates stay at 62.
- Sibling growth also drifted in-place rename citations: MailActionsTests `SetFolderItems` sites moved `:66`/`:87` → `:67`/`:88`; re-grep every listed rename site at the current head before trusting a prepared plan's numbers (all 12 other sites were still current).
- Files removed from an intentional-growth list must be re-stated as held by the "no growth past baseline" clause, or the list edit silently loosens the gate. See [[literal-call-clauses-block-file-size-tightening]] and [[project-445-keyboard-action-plan-seams]].
- A second correction pass (2026-08-27, user-authorized) fixed four ACs + one prose line that still cited the pre-reroute file locations (path token only). Still-stale residuals deliberately left (out of authorized scope): § test-matrix rows for #486 D3 / #490 D3 / #490 D4 (spec lines ~655/664/665) name the parent files with pre-growth line counts ("374 lines, 126 spare", "184 lines, 316 spare"). Per [[feedback-spec-corrections-sweep-sibling-sections]], a full sweep would also amend those rows — flag them whenever a future pass touches this spec.
