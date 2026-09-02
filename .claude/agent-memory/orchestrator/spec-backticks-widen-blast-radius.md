---
name: spec-backticks-widen-blast-radius
description: Get-BlastRadius harvests backtick-delimited paths from spec.md as well as the plan, so a backticked comparison path widens the computed radius and can create a false cohort conflict in a parallel run
metadata:
  type: project
---

`Get-BlastRadius` in `.claude/lib/blast-radius/BlastRadius.psm1` takes BOTH
`-PlanText` and `-SpecText`. It calls `Get-PlanPaths` on the plan and
`Get-PathFromLine` over EVERY line of the spec, harvesting backtick-delimited
inline-code tokens and classifying the path-shaped ones into the radius `paths`
set. `.claude/lib/blast-radius/BlastRadiusExtraction.psm1` is the extractor.

So a path you backtick in `spec.md` purely as a comparison, a precedent, or an
evidence citation enters the computed blast radius exactly as if the fix touched
it.

**Why:** in a parallel run the radius is what decides cohort conflicts. A spec
that backticks, say, the whole `scripts/vscode` surface while citing a script it
never edits can serialize itself against an unrelated item for no reason. The
read-by-mandate filter (`Get-NonMandateReadEntry`) drops policy-rule citations,
but it does not drop ordinary source paths.

**How to apply:** in `spec.md`, backtick only the files the change will actually
modify, plus feature-folder artifacts (which are covered by the feature-folder
glob anyway). Write every comparison, precedent, and out-of-scope citation in
bare prose, and put a short "path-notation convention" note near the top of the
spec so a later editor does not "fix" the formatting. This is not cosmetic and
it is not the agent inventing a rule: it was verified against the module. Cite
the module in that note so the claim is falsifiable rather than folklore.
Related: [[parallel-epic-children-name-collisions]].
