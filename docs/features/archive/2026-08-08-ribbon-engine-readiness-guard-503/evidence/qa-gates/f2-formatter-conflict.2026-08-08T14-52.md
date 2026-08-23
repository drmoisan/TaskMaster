# F2 IS UNSATISFIABLE AS SPECIFIED — CSharpier Formats XML and Mandates the Multi-Line Form

Timestamp: 2026-08-08T14-52
Discovered at: [P3-T2] (repo-wide `csharpier check .`)
Status: **F2 NOT REMEDIATED. Escalated to the orchestrator. Not fixed by this executor.**

## What happened

P2-T1 collapsed the three `TriageSet*` `<button>` elements to single-line form exactly as pinned in plan section 5.2. Every Phase 2 gate passed on its own terms: 524 lines (P2-T2), well-formed CustomUI (P2-T3), `EMBEDDED_TRIAGESETA_SINGLELINE=True` (P2-T4), 8/8 ribbon-XML tests green (P2-T5).

The **repo-wide formatting gate at P3-T2 then failed**:

```text
Error .\TaskMaster\Ribbon\RibbonExplorer.xml - Was not formatted.
  ----------------------------- Expected: Around Line 448 -----------------------------
          <group id="TriageGroup" imageMso="Filter" label="Triage">
            <button
              id="TriageSetA"
  ----------------------------- Actual: Around Line 448 -----------------------------
          <group id="TriageGroup" imageMso="Filter" label="Triage">
            <button id="TriageSetA" onAction="TriageSetA_Click" getEnabled="EngineCommand_GetEnabled" label="Set A" />
            <button id="TriageSetB" onAction="TriageSetB_Click" getEnabled="EngineCommand_GetEnabled" label="Set B" />

Checked 1498 files in 3581ms.
EXIT_CODE=1
```

CSharpier's **Expected** output is the six-line form. The formatter is demanding precisely the layout F2 asked to remove.

## Root cause — measured, not inferred

Plan section 3 rule 6 asserts "CSharpier does not format XML." **That assertion is false for this toolchain.** Measurements:

| Fact | Measurement |
|---|---|
| CSharpier version | **1.3.0** — the 1.x line formats XML, not only C# |
| `.csharpierignore` contents | excludes `**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`, `*.csproj`, `*.props`, `*.targets`. **`*.xml` in general, and `RibbonExplorer.xml` in particular, are NOT excluded** |
| CSharpier default print width | 100 columns (no `.csharpierrc` present; no `max_line_length` in `.editorconfig`) |
| Merge-base single-line form, `<button id="TriageSetA" onAction="TriageSetA_Click" label="Set A" />` | **78 characters** — fits within 100, so single-line was formatter-clean |
| Single-line form **with** the added attribute | **116 characters** — exceeds 100, so CSharpier mandates the multi-line form |

The arithmetic is decisive. Adding `getEnabled="EngineCommand_GetEnabled"` (37 characters including the trailing space) to a 78-character line yields 115-116 characters. There is no attribute ordering, and no permitted alternative callback name (AC5, AC6, and AC8 pin the value `EngineCommand_GetEnabled`), that brings the line under 100.

## Consequence — the F2 finding rests on a factual error

`remediation-inputs.2026-08-08T14-26.md` §F2 states that "approximately 12 lines came from reformatting three previously single-line `<button>` elements into multi-line form, which is **incidental churn with no functional purpose**", and `code-review.2026-08-08T14-15.md` states that "12 of those lines buy nothing."

Both statements are incorrect. Those 12 lines are **mandated by the repository's own formatter** once the functionally required attribute is added. The implementation cycle did not gratuitously reformat those elements; it produced the only layout that passes `csharpier check .`.

F2's required outcome — "restore the three reformatted `<button>` elements to their original single-line form **while retaining their `getEnabled` attribute**" — is therefore **mechanically unsatisfiable** while the mandatory format gate must pass. Its acceptance gate of "line count at or below 527" is likewise unreachable: 539 is the formatter-mandated minimum for a file carrying all eight attributes.

## Action taken

The P2-T1 collapse was **reverted** with `git checkout -- TaskMaster/Ribbon/RibbonExplorer.xml`, restoring the formatter-mandated 539-line form. Re-running the gate confirms the tree is clean:

```text
Checked 1498 files in 3682ms.
EXIT_CODE=0
```

Phase 3 restarts from P3-T1 per its own loop semantics ("If any task fails ... fix the cause and restart the phase from P3-T1"). The cause was the P2-T1 edit; reverting it is the fix that restores gate compliance.

## Why the alternative fixes were rejected

| Candidate fix | Rejected because |
|---|---|
| Add `*.xml` or `RibbonExplorer.xml` to `.csharpierignore` | `.csharpierignore` is not in the plan's section 4 scope lock. It is also a **gate-weakening change**: suppressing a formatter check so a change can pass is the pattern `.claude/rules/csharp.md` Prohibited Behaviors forbids. It would additionally un-format every other XML file in the repository. |
| Raise CSharpier's print width via a new `.csharpierrc` | Out of scope, and would reformat the entire repository, breaking the AC15 zero-line-diff guarantee on the three protected paths. |
| Keep the collapse and accept a red format gate | Violates CLAUDE.md CUT3 and `.claude/rules/csharp.md`, which require the format step to pass, and fails P3-T2's and P3-T12's own binary outcomes. |
| Shorten the line some other way | Impossible. The callback value is pinned by AC5/AC6/AC8; the control ids are pinned by `EngineCommandCatalog`. |

Per the executor directive, a newly discovered defect is **recorded for the orchestrator to promote, not fixed**. No `.csharpierignore`, `.csharpierrc`, or formatter configuration was modified.

## Superseded Phase 2 evidence

These artifacts record real measurements taken while the collapse was present. They remain valid as the record of the attempt and are the proof of the conflict, but they no longer describe the tree:

- `evidence/qa-gates/f2-xml-line-count.2026-08-08T14-52.md` — recorded 524 lines. The tree is now **539**.
- `evidence/other/phase2-build.2026-08-08T14-52.md` — recorded `EMBEDDED_TRIAGESETA_SINGLELINE=True`. It is now **False** again.
- `evidence/qa-gates/f2-xml-wellformed.2026-08-08T14-52.md` and `evidence/regression-testing/f2-ribbon-xml-tests.2026-08-08T14-52.md` — their conclusions (well-formed CustomUI; AC5/AC6/AC7/AC8 green) hold for the reverted tree as well, and were re-confirmed by the Phase 3 restart, but they were measured against the collapsed form.

Plan tasks P2-T1 through P2-T5 are un-checked in the plan file, because the outcome they certify is not present in the tree.

## Recommendation for the orchestrator

1. Close F2 as **not remediable as specified**, citing the print-width arithmetic above.
2. If the 500-line overage on `RibbonExplorer.xml` is to be addressed at all, the only viable route is **splitting the resource** into multiple embedded files — a separate, larger change that `spec.md` AC25 already declines and that should be its own issue.
3. Correct plan section 3 rule 6 ("CSharpier does not format XML") in any future plan; it is false for CSharpier 1.3.0 and caused this cycle to pin an unsatisfiable edit.
4. Note that F1 — the substantive finding, and the one the code review called "the most substantive" — **is fully remediated and proven** by the recorded mutate-build-fail-restore cycle.

## Net effect on the repository

`TaskMaster\Ribbon\RibbonExplorer.xml` takes a **zero-line diff** from this remediation cycle. The only source change this cycle contributes is the F1 fix in `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs`.
