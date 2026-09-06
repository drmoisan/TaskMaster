---
name: appending-a-second-pass-must-not-qualify-schema-fields
description: Relabelling "Output Summary:" to "Output Summary (pass 1):" when appending a re-run section deletes the machine-checkable field; the completeness gate reports the artifact INCOMPLETE.
metadata:
  type: project
---

When a contingency forces a gate to run twice and you append a "pass 2" section to the existing
artifact, do not qualify the required schema field names to disambiguate the passes.

**Why:** `evidence-and-timestamp-conventions` requires the literal fields `Timestamp:`, `Command:`,
`EXIT_CODE:` and `Output Summary:`. Rewriting the original line to `Output Summary (pass 1):` and
adding `Output Summary (final, ...):` leaves the file with ZERO occurrences of the exact literal, so
a completeness audit reports the artifact incomplete even though it reads as complete to a human.
The parenthetical is the whole defect: an exact-literal scan does not tolerate it.

**How to apply:** keep exactly one unqualified `Output Summary:` line that covers BOTH passes, and
label the narrative sections instead ("Pass 1 result:", "## Pass 2 — the re-run"). Same rule for the
other three fields. If a second `EXIT_CODE:` is genuinely needed, put the re-run in its own artifact
rather than qualifying the field in a shared one — the schema is per-file, so one artifact carries
one field set.

Caught on issue #735 by the P5-T10 completeness gate, on the `file-line-counts` artifact after the
P4-T3 branch B extraction forced P4-T1 and P4-T2 to be re-run. The gate did its job; the point is
that the mistake is invisible on reading and only a literal scan finds it.

Related: [[project_artifact_output_summary_breaks_its_own_exact_count_gate]].
