---
name: pester5-string-compare-message-shape
description: Pester 5.6.1 Should -Be / -BeExactly on two strings never prints "Expected exactly ..., but got"; plans asserting that shape force a futile fix loop
metadata:
  type: project
---

When both the actual and the expected value are strings, Pester 5 (CI pins 5.6.1 in `.github/workflows/_pester.yml`)
routes `Should -Be` and `Should -BeExactly` through its string-diff message:
`Expected strings to be the same, because <text>, but they were different.` followed by lengths, the differing index,
and `Expected: '<x>'` / `But was:  '<y>'`. The `Expected exactly <x>, because ..., but got <y>.` form appears only for
non-string operands. Observed: #929 p1-t2 tree-test fail-before (Should -Be), #911 rc2-t4 correction discharge
(`-BeExactly '1.0.2'` against `''` printed `Expected: '1.0.2'` / `But was:  ''`).

**Why:** the #973 r0 plan asserted It (b)'s `($carrier -join ',') | Should -BeExactly '<list>'` failure would begin
`Expected exactly ` and contain `, but got`, and also declared every other message shape "a defect in the edited test:
fix and re-run". On a correct test that clause can never pass.

**How to apply:** in preflight, find every expected-failure message assertion and classify each operand pair. A
string-vs-string comparison asserts `Expected strings to be the same` plus `but they were different`. Integer and
collection comparisons keep `Expected <x>, because ..., but got <y>.` Also check any blanket "message of another shape is a
defect" rule against the string form.
