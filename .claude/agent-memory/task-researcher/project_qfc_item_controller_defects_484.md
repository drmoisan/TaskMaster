---
name: qfc-item-controller-defects-484
description: "#484 epic child (closes #480/#481/#483/#484/#485): promoted-potential Suspected Fix sections were wrong in 5 of 5 issues; verify callers + interface declarations before planning"
metadata:
  type: project
---

Feature `qfc-item-controller-defects` (issue #484) closes five pre-existing `QfcItemController` bugs.
Research on 2026-08-24 found that **every one of the five promoted potential documents had a materially
wrong or incomplete "Suspected Fix" section**, while their *diagnoses* were accurate.

**Why:** the potentials were captured during preparation research for a different epic child (#453/F10) as
drive-by observations. They were never validated against caller sets, interface declarations, or test
reachability, because that was out of scope at capture time.

**How to apply:** when planning from a promoted potential in this repository, treat the Affected Code and
Why-This-Is-a-Defect sections as authoritative and the Suspected Fix section as an untested hypothesis.
Before accepting a suggested fix, check in this order:

1. **Enumerate callers of the defective member.** #480's potential said "some caller may have been written to
   compensate"; there are in fact ZERO production callers of the one-arg `ToggleNavigation(bool)` overload —
   all four `QfcCollectionController` sites use the two-arg overload. Removal was unconditionally safe and
   the caution was unnecessary.
2. **Check whether the member is on a public interface.** #483's potential offered "rethrow OR return a
   failure result"; the return-result option is impossible because `Task MoveMailAsync()` is declared on
   `IQfcItemController` and implemented by the out-of-scope `EfcItemController`. Half the suggested option
   space did not exist.
3. **Check whether the fix lands in a coverage-exempt member.** #485's suggested in-place `Uri.TryCreate`
   guard is code-correct but would have added zero covered lines and zero regression tests, because the
   enclosing `InitializeWebViewAsync` is `[ExcludeFromCodeCoverage]` and needs a live WebView2 runtime.
   Extraction into a pure member was required to make the fix verifiable at all.
4. **Check whether the named remedy is sufficient.** #484's "dispose the timer" is necessary but does not
   abort an in-flight callback; the callback itself dereferences four fields `Cleanup()` nulls.
5. **Recount anything the potential quantified.** #481 claimed "25 `+=` in EventWiring.cs"; the real figure
   is 22 event subscriptions (the count included two commented-out lines and one arithmetic `+=`).

Related: [[qfc-item-controller-227-r2-denial]], [[feedback-exemption-audit-check-proven-techniques]],
[[qfc227-headless-itemviewer-and-tlpcellsnapshot]].
