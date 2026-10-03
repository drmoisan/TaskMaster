---
name: project-959-r7-ac-by-reference-spec-correction-and-delta-backtick-seams
description: "#959 revision 1.7 (2026-10-03): when an AC incorporates a type by reference to the spec's technical sections, a seam-type change must correct those sections in place (not just report drift); reviewer deltas use double backticks as message delimiters; a self-review Grep claim of '0 lines' self-hits its own quotation"
metadata:
  type: project
---

Three seams from the #959 revision 1.6 -> 1.7 preflight round (six textual deltas, all applied verbatim).

1. **AC-by-reference forces a spec technical-section correction.** AC8 said "exact delegate types in Technical specifications". Revision 1.6 changed the seam type (CS1769: `Func<Attachment,...>` cannot cross the embedded-interop boundary; nested `TrySaveAttachmentDelegate` instead) and only *reported* the spec drift to the orchestrator. Preflight blocked that: because the AC incorporates the technical sections, leaving them stale makes the AC describe the wrong type. The fix was an in-place correction of the technical lines (D5, impacted-functions list, two signature blocks, method-group sentence, planner note) under orchestrator permission, with the line count unchanged, plus a P6-T23 precondition Grep (`Func.Attachment` zero lines, `TrySaveAttachmentDelegate` at least six lines) so the check-off task verifies the correction.
   **Why:** "AC text unchanged" is not the same as "AC meaning unchanged" when the AC points at another section.
   **How to apply:** before claiming "no AC changes" after a type/signature substitution, Grep the AC block for "in Technical specifications" / "see section" style references and correct the referenced sections in the same pass.

2. **Reviewer delta text uses double backticks as message delimiters.** The delta wrapped insert text in single backticks and used ``double`` backticks for inner code spans (and the reverse on another line). The file content was written with single backticks (the plan's and spec's convention); Markdown renders both identically and no Grep gate depends on the backtick count. Declare the interpretation in the return.

3. **Self-review "0 lines" claims self-hit.** Writing "Grep for `about 342` (0 lines)" into the self-review record makes that record the one hit. Phrase it as "exactly one line, this record's own quotation, and none elsewhere" (same trap as the #956 R1 spec-wording note).

4. **Stop-record iteration default.** A first stop record carries no `ITERATION:` row; the re-run carries `ITERATION: 2`. The artifact-filenames rule must say a file without an `ITERATION:` row counts as iteration 1, or the "highest ITERATION" glob tie-break is undefined for the first record. Check which re-runs already exist (Glob) before asserting that "both re-runs carry ITERATION: 2".
