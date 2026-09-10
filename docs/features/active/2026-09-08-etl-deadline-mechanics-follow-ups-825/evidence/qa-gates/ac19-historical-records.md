# AC19 — The Historical TableEtlInvoker Record Is Untouched

Timestamp: 2026-09-09T17-09

Command: git diff $b -- UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs, with $b re-derived from evidence/baseline/base-commit.md per D3, its output counted for added or removed lines matching TableEtlInvoker

EXIT_CODE: 0

TableEtlInvokerDiffLines: 0

Output Summary: The anchored diff for UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs
contains zero added and zero removed lines matching TableEtlInvoker, so the historically accurate
past-tense mention survives this feature's edits to that file.

Re-derived line number. The mention sits at line 242 of the post-change file and reads
`/// existing caller does now that the TableEtlInvoker static has been removed.` Its pre-change
position was line 212; P3-T7 and P3-T8 added lines above it, so the current number is recorded here
rather than the pre-change one.

The distinction the criterion turns on is between a stale claim and a historical record. The
sentence P6-T2 corrected in UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs names TableEtlInvoker in
the present tense as an existing seam, and no declaration of that member exists anywhere in the
solution, so it was a false statement about the current tree. The sentence above names the same
identifier in the past tense, as something that was removed, which is true and remains true. Every
occurrence under docs/features/** is likewise a historical record and is not rewritten by this
feature.
