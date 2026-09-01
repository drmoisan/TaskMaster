---
name: cobertura-substitution-leaves-blobs-in-history
description: The raw-Cobertura-to-JaCoCo evidence substitution only cleans the tree, not history; check git ls-tree on the pre-substitution commit and recommend squash-merge
metadata:
  type: project
---

When a branch carries a `coverage-artifact-substitution.<ts>.md` record (precedent `d0955dc4`;
applied to #503, #646, #648), verify whether the raw `.cobertura.xml` files were **committed first and
removed later** rather than never committed. Run:

```
git log --oneline --all -- <feature>/evidence/**/p*-coverage.cobertura.xml
git ls-tree -l <the commit that ADDED them> -- <those paths>
```

At #648 both raw reports (10,595,557 + 10,595,556 = 21,191,113 bytes of blob) entered history in
`8d933975` and were removed only in `08868ba0`. The working tree is clean but the blobs stay reachable
from the branch.

**Why:** the substitution record's own stated objective is "raw Cobertura must not enter history as
evidence." That objective is only met if the branch is squash-merged. A merge commit carries both
blobs into `main` permanently and every later clone fetches them.

**How to apply:** record it as a Minor, non-blocking finding with the remedy stated as a *merge-method*
recommendation ("squash-merge drops both blobs with no history rewrite"), not as a code change. Also
note that the record's on-disk byte figures are CRLF working-copy sizes and the committed blobs are
LF-normalized and slightly smaller — the two numbers disagreeing is expected, not a discrepancy.

Related: [[jacoco-summary-substitution-is-valid-coverage-evidence]],
[[feature-evidence-cobertura-counts-as-coverage-artifact]].
