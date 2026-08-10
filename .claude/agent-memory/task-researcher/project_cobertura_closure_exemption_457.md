---
name: cobertura-closure-exemption-457
description: "#457 research: dotnet-coverage drops [ExcludeFromCodeCoverage] members entirely (no <method> element), so member absence is the exemption signal; async members are the trap because their bodies move to <Member>d__N"
metadata:
  type: project
---

Issue #457 (`[ExcludeFromCodeCoverage]` does not suppress nested lambdas) research, 2026-08-10. Recommended
Candidate 1c: a pre-merge post-processing filter in `scripts/vscode/`.

Key verified findings (from the RAW cobertura at
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`
— most committed cobertura files are POST-processed and have already merged closure classes away; raw ones are
identifiable by absolute `filename` values and hundreds of `&lt;&gt;c` hits):

- An `[ExcludeFromCodeCoverage]` member emits **no** `<method>` element at all. Absence, not zero hits, is the
  exemption record. This is the only exemption signal recoverable from Cobertura XML.
- Closure types are **separate sibling `<class>` elements** sharing the declaring type's `filename`; method names
  embed the declaring member (`<Member>b__N_M`, `<Member>b__K`, `<Member>g__Local|N_M`).
- **Trap:** async/iterator members ALSO have no plain `<method>` element — their bodies move to
  `Type.<Member>d__N` with only `MoveNext`. A naive "member absent ⇒ exempt" rule deletes covered lambdas in
  non-exempt async members. The presence set must admit `<Member>d__N` class names.
- Local functions live inside the DECLARING type's `<class>`, not a closure type, so class-level filtering misses
  them.

**Why:** these three shape facts are what make the fix implementable from the XML alone rather than requiring
assembly-metadata or C#-source parsing, and the async trap is the single thing most likely to be missed.

**How to apply:** if #457 is revisited or a similar coverage-arithmetic issue arises, start from the raw artifact
above rather than re-running collection, and check the async `d__` case before accepting any absence-based rule.
Ordering is a hard constraint: `Merge-CoberturaClassesByFilename` merges closure classes into the declaring type
by filename and keeps only the primary's `<methods>`, destroying the linkage — any filter must run before it.

Related: [[qfc-item-controller-227-r2-denial]] (exemption-boundary precedent).
