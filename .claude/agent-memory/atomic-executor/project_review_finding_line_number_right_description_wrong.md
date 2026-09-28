---
name: review-finding-line-number-right-description-wrong
description: A code-review finding can cite the correct uncovered line and describe that line incorrectly; executing its stated recommendation verbatim then satisfies nothing — re-derive what the cited line actually is before writing the fix
metadata:
  type: project
---

A review finding names a location and a remedy. The two are independently fallible. Verify the
location against the tree and re-derive what sits there before writing to the remedy's
description.

Observed on 2026-09-20, issue #911, finding R-C2-4. The finding read: "`Invoke-PackageReferenceSync`
line 410 — the non-zero-fix summary branch is untested ... Add one test that supplies a seam
producing at least one hint-path repair and asserts the returned `FixedCount`." Line 410 was
genuinely uncovered and the predicted post-fix figure (105 of 127, 82.68 percent) was exactly
right. But line 410 is the `else` arm — `Write-Information 'Sync-PackageReferences: All HintPaths
are up to date'`. The non-zero arm is 406-407 and it was **already covered** by an existing
end-to-end test asserting `FixedCount` of 1; neither 406 nor 407 appeared in the baseline
`UNCOVERED=` list. Executing the recommendation as written would have added a near-duplicate of
an existing test, left 410 uncovered, and left the file at 104 of 127 while the artifact claimed
the ceiling had been reached.

**Why:** a reviewer reads the coverage tool's uncovered-line list and the source separately, and
the narrative joining them is written from memory of the function's shape. The line number comes
from a tool; the label comes from a human. Only the first is measured.

**How to apply:** when a finding cites a line, open that line with numbered output before
editing, and check the sibling branch too. The cheap discriminator is the baseline `UNCOVERED=`
list itself: if the branch the finding *names* is absent from that list, it is already covered
and the finding means the other branch. Write the test that covers the cited line, then state the
discrepancy in the evidence artifact rather than silently adopting either the finding's wording
or your own — gate rule 2 forbids an artifact asserting a property that does not hold, and
"covers the non-zero-fix branch" would have been exactly that.

Related: [[feedback_verify_line_citations_with_numbered_output]],
[[feedback_never_predict_an_observation_into_an_artifact]],
[[gates_can_pass_for_reasons_unrelated_to_correctness]].
