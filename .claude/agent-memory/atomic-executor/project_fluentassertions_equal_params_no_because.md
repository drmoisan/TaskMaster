---
name: fluentassertions-equal-params-no-because
description: FluentAssertions .Equal(params T[]) has no because overload; a trailing reason string becomes an extra expected element and fails a GREEN-on-HEAD test
metadata:
  type: project
---

In this repo's MSTest + FluentAssertions tests, `collection.Should().Equal(...)` binds to the `Equal(params T[] elements)` overload, which has NO `because`/`becauseArgs` parameters. Passing a reason string as a trailing argument (e.g. `.Equal("A", "B", "the set must be unchanged")`) silently treats the reason as a THIRD expected element, so a 2-item collection fails with "contains 1 item(s) less".

**Why:** During #292 execution a behavior-preserving guardrail test that was supposed to be GREEN on HEAD failed for this reason, not for a real defect — masquerading as a RED result and briefly confusing the RED-before-GREEN capture.

**How to apply:** For ordered equality with FluentAssertions, either drop the reason (`.Equal(new[] { "A", "B" })`, which binds to the `IEnumerable<T> expected` overload) or move the reason to a separate assertion that supports `because` (e.g. `.HaveCount(2, "reason")`). Only string-collection `.Equal` is affected by the ambiguity; the pattern applies to any `Equal(params T[])` call where a because was intended.
