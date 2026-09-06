---
name: excludefromcodecoverage-misses-this-capturing-lambdas
description: '[ExcludeFromCodeCoverage] removes a member''s own lines but NOT lambdas that capture `this`; those are lifted into members of the declaring class and still appear in Cobertura with hits=0'
metadata:
  type: project
---

`[ExcludeFromCodeCoverage]` on a method does **not** uniformly remove every line of that method's
source span from the Cobertura document. The rule is about where the compiler lifts each lambda:

- A lambda that captures **a local** is lifted into a compiler-generated **display type**. The
  attribute reaches it; the token never appears in the document at all.
- A lambda that captures **`this`** is lifted into a separate **instance member of the declaring
  class**, named `<EnclosingMember>b__<N>_<M>`. The attribute on the declaring member does **not**
  reach a member emitted beside it, so each such lambda appears as its own `<method>` element
  carrying one line with `hits="0"`.

**Why:** measured on issue #736. `AppOlObjects.ResolveValidatedArchiveRootPath()` carried the
attribute and passed three `this`-capturing delegate literals as arguments; its body lines vanished
but lines 89/90/91 stayed at `hits=0`, which made a `>= 90%` new-file gate unpassable in every state
until the set was derived and removed explicitly. In the same run,
`EfcFormController.ShowModelessFaultNotice`'s `FormClosed += (s, a) => notice.Dispose();` captured a
local, was lifted into a display type, and disappeared completely.

**How to apply:** before writing a coverage floor over a file containing an excluded COM/UI wrapper,
check the argument list for `this`-capturing lambdas and expect one uncovered line each. Derive the
set mechanically from the `<EnclosingMember>b__` name prefix rather than by eye. **In the XML the
angle brackets are entity-encoded**, so a raw text search must use `&lt;Name&gt;b__`; reading the
attribute through the XML DOM (`$m.name`) decodes them and matches the plain prefix. The trailing
`74_0` ordinal is compiler-assigned from the member's position in the class and shifts when
unrelated members are added, so never assert it verbatim.

Related: [[project_exempt_forward_extraction_leaves_call_site_uncovered]],
[[project_excludefromcodecoverage_partial_class_cs0579]],
[[project_cobertura_filename_maps_to_several_class_nodes]].
