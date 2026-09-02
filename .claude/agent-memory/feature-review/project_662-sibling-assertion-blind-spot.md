---
name: 662-sibling-assertion-blind-spot
description: "#662: the consistency-looking assertion Should().Be(other) survives the relaxation it appears to guard; only the sibling Should().BeFalse catches it. Verify a test's pinning power by evaluating BOTH sides under the prohibited edit."
metadata:
  type: project
---

When a test pins "two sites must agree", the equality assertion is NOT the assertion that catches
a relaxation — both sides usually flip together, so it still passes.

`QuickFiler.Test/Controllers/EfcFormControllerTests.cs:453`
(`IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically`) makes two
assertions per row:

- `:462` `creationPath.Should().Be(filingPath, "both sites must classify {row} alike")`
- `:463` `creationPath.Should().BeFalse("{row} is rejected at both sites")`

Under the prohibited edit (widening `EfcSelectionGuard.BannerRejectionPrefix` from `"==="` to the
producers' `"===="`), for the row `"==="` both `creationPath` and `filingPath` become `true`, so
**`:462` still passes** and only **`:463` fails**. The assertion that *reads* like the consistency
guard has no pinning power at all.

**Why:** the equality assertion measures agreement between two expressions that share the relaxed
term. Only an absolute assertion on the value itself constrains it.

**How to apply:** when auditing whether a test actually pins an invariant, mentally apply the
prohibited edit and evaluate **every** assertion, not the one whose message matches the invariant's
name. Report which specific assertion goes red. The same check exposes "the new test is redundant
with the existing one" claims as false — #662's new
`BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates` asserts `BeFalse` on all four
inputs directly and does not share the blind spot.

Corollary from the same review: a guard constant that looks like an inconsistent copy can be
load-bearing. `EfcSelectionGuard`'s three-character prefix is the **only** term rejecting a
three-equals row at either EFC site, because `MinimumCreationLength` is 3 and so `3 >= 3` lets the
length rule accept it. "Unify the arity" would have been a behavioural regression. Trace the
dispatch path (`EfcFormController.cs:745-749` filing, `:1151-1153` creation) before calling any
two-valued constant pair an inconsistency.

Related: [[review-residuals-index]], [[red-first-equivalence-patterns]].
