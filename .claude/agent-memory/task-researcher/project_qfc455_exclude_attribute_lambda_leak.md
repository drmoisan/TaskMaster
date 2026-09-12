---
name: qfc455-exclude-attribute-lambda-leak
description: Epic #136/F13 — method-level [ExcludeFromCodeCoverage] does NOT suppress lambdas lifted out of the attributed member, but a type-level attribute does; and Cobertura <class> line-rate is inflated by exactly the per-method duplicate <line> block
metadata:
  type: project
---

Two measurement facts verified numerically against
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
(2026-08-07, during F13 / issue #455 research on `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`):

1. **`[ExcludeFromCodeCoverage]` on a METHOD does not exclude lambdas declared inside it.** The
   lambda is lifted into a compiler-generated closure method that does not inherit the attribute, so
   the collector still instruments it and it shows as permanently uncovered production code. In
   `BreadcrumbPopupUiOperations.cs`, 23 of 24 uncovered lines were this one mechanism.
   **A TYPE-level attribute does suppress the closures** — proven by `WebView2Messenger.cs`
   (type-level attribute, lambdas in its body) producing *zero* `filename=` entries in the same
   report. Remedy for a lambda-bearing exempt member: relocate it into a separate type carrying a
   type-level attribute. It must be a separate type, not a `partial` — an attribute on one partial
   declaration applies to the whole type.

2. **Issue #441's double-count is exactly the per-method `<lines>` block.** For a class element, the
   `line-rate` attribute counts the class-level `<lines>` children PLUS every per-method `<lines>`
   child. Verified: 258 class lines + 82 method lines = 340; 234 + 82 = 316; 316/340 = 0.929412 =
   the reported `line-rate`. Same for branches: 120 + 40 = 160, 106 + 33 = 139, 139/160 = 0.86875.
   Inflation here was +2.24 pts line / +1.46 pts branch.

3. Corollary: **`await` inside a `catch` block makes 100% unreachable.** Roslyn's pending-exception
   rewrite leaves the closing `}` after an unconditional `throw;` as instrumented-but-unreachable IL
   plus an uncoverable branch half.

**Why:** three separate F13 findings that are expensive to re-derive and that change what a child
should plan. A child that budgets test-writing tasks against lambda-leaked lines will burn effort on
unreachable code; a child that cites a `<class> line-rate` attribute reports an inflated number.

**How to apply:** when a QuickFiler file's uncovered lines cluster inside `[ExcludeFromCodeCoverage]`
members, do not plan tests — plan a relocation into a type-level-attributed file (which usually also
solves a 500-line problem). Always recompute rates from deduplicated class-level `<line>` nodes with
max(hits), and key the harness on `filename`, not on the Cobertura class name (one source file can
declare two types and emit a single class element named after only one of them — see
`BreadcrumbWebViewSurfaceFactory.cs`, which emits as `BreadcrumbNavigationReadiness`).

Related: [[quickfiler-percoverage-epic-136]], [[qfc-perfile-coverage-viewerqueue-434]],
[[quickfiler-interface-only-files-433]].
