---
name: cobertura-class-line-double-count-trap
description: Aggregating .//line under a Cobertura <class> double-counts lines that also appear under <method> rows; use class-level lines/line and cross-check the class line-rate attribute
metadata:
  type: project
---

When computing a per-file coverage figure from a Cobertura document, `SelectNodes('.//line')` under a
`<class>` node matches **both** the class-level `<lines>` block and every `<method>/<lines>` block,
so any line belonging to a method is counted once per method row that contains it.

At #670 this inflated the new file `QfcItemController.WebViewFaultBoundary.cs` from the true 12/13
(92.3077%) to 16/17 (94.1176%). The culprit was line 17, the field/property initializer, which
appears at class level and again under **four** `.ctor` method rows (the type has four constructors,
and a field initializer is emitted into every one).

**Why:** the inflated figure was still above the 90% floor, so it would have passed the gate while
being wrong — and it disagreed with the caller's independently derived figure, which is the only
reason it got caught. A wrong denominator can just as easily push a passing file below a floor.

**How to apply:**
- Use the direct child path `lines/line` on the `<class>` node, not `.//line`.
- Cross-check against the class's own `line-rate` attribute, which the tool computes correctly
  (`0.923077` here). If your count disagrees with `line-rate`, your count is wrong.
- Types with multiple constructors and any field/property initializer are the high-risk shape;
  partial classes make it worse because one `<class>` name can span several `filename` values.
- Repo-wide figures read from the ROOT `<coverage>` attributes (`lines-covered`/`lines-valid`) are
  unaffected by this and remain the right source for the repo-wide row.

Related: [[measure-every-changed-file-not-just-the-ac-named-one]],
[[jacoco-summary-substitution-is-valid-coverage-evidence]].
