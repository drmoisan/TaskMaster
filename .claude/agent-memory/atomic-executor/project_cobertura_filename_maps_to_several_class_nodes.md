---
name: cobertura-filename-maps-to-several-class-nodes
description: A Cobertura filename attribute matches one class node per nested type, and each node repeats its <line> elements at method and class level, so "locate the class node" and a naive line count are both wrong
metadata:
  type: project
---

When a plan says "locate the class node whose `filename` attribute ends in `X.cs` and intersect its
`<line number=...>` elements with the added-line set", the singular is misleading in
`dotnet-coverage` Cobertura output.

**Why:** the `filename` attribute is per-source-file, but `<class>` nodes are per-CLR-type. One
`.cs` file with nested types emits several class nodes carrying the identical `filename`. Measured
in `coverage/p4-t5.cobertura.xml` for `UtilitiesCS/Threading/UiThread.cs`: three nodes —
`UtilitiesCS.UiThread`, `UtilitiesCS.UiThread.SynchronizationContextAwaiter`, and
`UtilitiesCS.UiThread.SynchronizationContextAwaiter.<>c`. Separately, WITHIN one class node each
`<line>` appears twice, once under a `<method>` and once in the class-level `<lines>` roll-up, so a
raw `grep -c '<line number='` roughly doubles the count.

**How to apply:** union the `<line>` elements across EVERY node whose `filename` ends in the target
path, then `sort -n -u` on `(number, hits)` before intersecting with the added-line set. Extract a
node with `awk -v s=<startline> 'NR>=s { print; if (NR>s && /<\/class>/) exit }'`. Also expect added
lines that carry no `<line>` element at all — accessor headers, closing braces of a block, and a
field declaration with no initializer are not emitted sequence points; they are not coverable and
must be excluded from the changed-line denominator rather than counted as misses. Related:
[[async-state-machine-emits-no-method-element]].
