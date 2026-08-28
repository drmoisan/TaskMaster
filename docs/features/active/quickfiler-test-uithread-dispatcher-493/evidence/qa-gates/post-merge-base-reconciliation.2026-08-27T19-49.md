# Base Reconciliation and Content-Loss Gate

Timestamp: 2026-08-27T19-49
Task: Resume verification — reconcile the moved epic integration base before fan-in
Command: `git fetch origin epic/quickfiler-bug-family-integration`; `git merge --no-edit origin/epic/quickfiler-bug-family-integration`; `git rev-list --left-right --count origin/epic/quickfiler-bug-family-integration...HEAD`; `git diff --numstat origin/epic/quickfiler-bug-family-integration..HEAD | awk '$1==0 && $2>0'`
EXIT_CODE: 0
Output Summary: The merge completed with no conflict. Behind count is 0 and ahead count is 7. The
recorded merge commit is `3c6ed27b`. The pure-deletion query printed no rows, so no file on this
branch loses content that the base gained.

## Why reconciliation was required

This branch was 6 ahead and 11 behind at resume. Sibling feature 442 had merged into the integration
branch as PR #649, moving the tip to `4f238289`. Three siblings (444, 476, 501) remain in flight
against the same branch, so the base can move again before fan-in completes; this sequence is
re-run immediately before PR creation and again immediately before merge.

## Content-loss invariant

The invariant enforced here is **no file may lose content the base gained**. It is not the stricter
and unsatisfiable requirement that the pure-deletion query print nothing: a feature that legitimately
deletes code would fail that wording no matter how correct it is.

| Check | Result |
| --- | --- |
| Behind count after recorded merge | 0 |
| Files with 0 additions and >0 deletions vs base | none |
| Merge conflicts | none |

Because the behind count is 0 after a real merge commit, every base commit is an ancestor of HEAD,
so no base content can be absent by omission. The pure-deletion set is separately empty, so no file
is a deletion-only change requiring justification against feature intent. Both halves of the
invariant hold.

Reported honestly: this feature does delete code — `SemaphoreSlim UiThreadDispatcherGate` and
`SwapUiThreadDispatcher` are removed from `QfcItemController.InitializationTests.Part2.cs`, and the
private parked-dispatcher machinery is removed from `QfcItemController.TestSupport.cs`. Both files
shrank on net (489 to 440 and 418 to 393). Neither is a pure deletion, because both also gained the
replacement calls into the shared fixture, so neither appears in the query above.

## Project-file region deviation (disclosed)

The epic checkpoint's `csproj_region_partition` assigns feature 493 the region `none`, on the stated
evidence that "plan declares no Compile Include entry". The delivered change does add two entries to
`QuickFiler.Test/QuickFiler.Test.csproj`. The deviation is disclosed rather than concealed:

- **Why it is unavoidable.** `QuickFiler.Test.csproj` is a legacy non-SDK project with explicit
  `<Compile Include>` items. The plan creates two genuinely new files, so without the two entries
  they are not compiled at all and every regression test in them silently disappears. The planning
  record's "no Compile Include entry" premise was simply incomplete.
- **Where they were placed.** Lines 158-159, immediately after
  `Controllers\QfcItemController.TestSupport.cs`, inside the contiguous `QfcItemController.*` family
  block that spans lines 149-169.
- **Collision risk against siblings.** Feature 444 owns `Controllers\Qfc*` per the partition, but its
  declared entries are all `QfcCollectionController*`, which occupy lines 122-128 — roughly thirty
  lines away with unrelated context between. 501 owns `Viewers\Breadcrumb*` and 476 owns
  `Viewers\WebView2*`, neither of which is in this item group's `Controllers\` range. No sibling
  insertion point overlaps lines 158-159.
- **Correction to the partition's stated premise.** The partition describes both item groups as
  "alphabetically ordered". That is not accurate for `QuickFiler.Test.csproj`: the group is grouped by
  class family and is not sorted (for example `QfcCollectionControllerTests.cs` precedes
  `QfcCollectionController.TestSupport.cs`, and the `QfcHomeController*` block follows
  `QfcStreamingDequeue*`). Placement therefore followed the file's actual family-block convention,
  which is also what keeps the insertion clear of every sibling's region.
