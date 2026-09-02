---
name: verify-the-callers-factual-correction
description: When a caller asserts the requirement docs are factually wrong and asks for your independent assessment, verify the assertion itself — at #670 the docs were right and the caller had located a different defect
metadata:
  type: feedback
---

A delegating prompt that says "the requirement documents contain a known factual imprecision, give me
your independent assessment" is still a claim to be tested, not a premise to build on. Verify the
correction with the same rigor you would apply to the original.

**Why:** at #670 the caller stated that `issue.md`/`spec.md` wrongly describe the issue-#488 D5 path
as raising `ObjectDisposedException`, citing `ItemViewer.Breadcrumb.cs:420-436` (`ThrowIfOffUiBoundary`)
and `:64` (different-provider) as raising `InvalidOperationException`, and asserted "No
`ObjectDisposedException` is raised on that path." Both cited sites were real — but they are #488
defects **D4** (UI-thread affinity) and **D3** (fail-fast on provider substitution). D5 is a third,
separate guard at `:391-393`:

    if (IsDisposed || Disposing) { throw new ObjectDisposedException(nameof(ItemViewer)); }

with an in-source comment at `:383-384` naming it "Issue #488 defect D5's `ObjectDisposedException`
throw". It is reachable: `ViewerSetup.cs:112` → `:150 InitializeBreadcrumbPipeline` →
`Breadcrumb.cs:74 EnsureBreadcrumbLifecycle` → `:361 EnsureBreadcrumbResourceOwnership` → `:393`.
The requirement documents were accurate and needed no correction.

**How to apply:**
- A `grep` for the exception type across the whole file, not just the caller's cited line range, is
  the ten-second check that settles it. The caller's range excluded the throw.
- Where several numbered defects (D3/D4/D5) landed in one file from one upstream issue, confirm
  WHICH defect a guard implements before attributing behaviour to it — adjacent guards in the same
  file routinely throw different types.
- Report the contradiction plainly and give the call chain as evidence; do not soften it into
  agreement. Then state the consequence — at #670 it was nil, because no AC depended on the
  exception type and the boundary catches `Exception`.

Related: [[verify-parity-claims-in-remediation-inputs]], [[verify-the-asserted-evidence-mechanism]].
