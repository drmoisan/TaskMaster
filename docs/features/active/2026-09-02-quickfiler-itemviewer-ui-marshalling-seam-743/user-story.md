# QuickFiler `ItemViewer` UI-marshalling seam (User Story)

- **Issue:** #743
- **Work Mode:** full-bug
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12
- **Status:** Draft
- **Version:** 1.0

> **Non-authoritative document.** This item's work mode is `full-bug`. Per the
> `acceptance-criteria-tracking` skill, the sole authoritative acceptance-criteria source for a
> `full-bug` item is the sibling spec. This file is narrative context for reviewers and carries no
> checkboxes and no acceptance criteria. It is produced because the feature-document contract requires
> both artifacts to exist. Do not track delivery against this file, and do not add checkboxes to it.

---

## Who is affected

The people affected are the repository's maintainer and the automated agents that run the C# toolchain
on his behalf. There is no end-user impact: no Outlook user sees different behaviour as a result of
this defect, and no production code path is known to be incorrect. Severity is Medium for exactly that
reason.

## The narrative

A maintainer or an agent runs the C# toolchain as CLAUDE.md defines it, ending in
`vstest.console.exe` with coverage enabled. On an idle machine the QuickFiler test assembly passes. On
a loaded machine — another build running, a coverage-instrumented multi-assembly run, idle MSBuild
node-reuse processes holding CPU — a handful of pump-hosted controller and breadcrumb-host tests stop
after approximately sixty seconds each and report a timeout rather than an assertion failure. Re-running
the identical tree with the machine idle passes.

The consequence is not a wrong answer; it is a lost signal. A gate that fails for reasons unrelated to
the change under test stops being usable as evidence. An agent that sees it has no principled way to
distinguish "this change broke something" from "this machine was busy", and the cheapest available
response — rerun until green — is precisely the habit the repository's determinism rules exist to
prevent.

The defect has now been reported three times: as #711, as part of #729, and as #592, which also
absorbed #511 and #571. Twice it was closed without being fixed, because the fix requires editing
QuickFiler production code and the owning items were scoped test-only. This item exists to stop that
happening a third time, and the production-edit prohibition is re-opened here specifically so it can be
closed.

## What "fixed" looks like from the maintainer's seat

Three things, in order of how much they matter.

First, the maintainer can point to a measurement — not an argument — that says what actually causes the
sixty-second expiry. Two mechanisms are currently live candidates: the real elapsed cost of building a
6,223-line Designer control tree with two handle-created WebView2 children, and a test-fixture
semaphore permit that is never released because MSTest stops observing a method it has timed out. The
evidence in this folder supports both and settles neither. The maintainer's first lead, which named two
specific helper identifiers, has been checked and those identifiers exist in no source file in the tree;
they were removed under issue #493 and replaced. That correction is recorded so nobody spends time
instrumenting something that is not there.

Second, a single deterministic test reproduces whichever mechanism is found, and does so without a
sleep, a retry, or a timing tolerance of any kind. This matters more than any number of green runs. A
deterministic test has no base rate, so it needs one run to be convincing, whereas the historical
failure rate of roughly one run in twenty-one means that thirty consecutive clean runs would occur by
chance about once in every four attempts even if nothing had been fixed. Sixty-two clean runs would be
needed to reject that null at the five-percent level, and sixty-two loaded full-suite runs would take
roughly fifteen hours. The statistical evidence is therefore scoped to a targeted reproduction and kept
as a supporting signal behind the deterministic one.

Third, nothing is lost in the trade. The pump-hosted tests that today provide the coverage evidence for
several de-exempted production members stay on the real Win32 message loop. The seam this item adds is
additive, and its purpose is narrow: to let the members that never needed a real message loop be tested
without paying for one. There are existing reflection-based contract tests in the assembly that assert
the two marshalling members of the viewer interface still exist, and they are the guard that keeps the
change additive rather than substitutive.

## What the maintainer explicitly does not want

The prohibitions matter as much as the goal here, because every cheap "fix" for this class of defect
destroys the thing being protected.

Raising the sixty-second bound, scaling it to the machine, or retrying is forbidden. Replacing the real
message pump in the existing pump-hosted tests with a fake synchronization context is forbidden; the
inherited recommendation this item is built on ends with a clause proposing exactly that, and the clause
is rejected while the rest of the recommendation is adopted. Removing the timeout attribute is
forbidden, because it is a deadlock guard and removing it converts a bounded, diagnosable failure into
an unbounded hang. Sharing one viewer across a test class would be the single cheapest speed-up
available and is also rejected, because it couples tests to each other's ordering.

## What remains honestly unknown

This story does not claim more than the evidence supports.

The identity of the seven tests that expired in the one recorded genuine failure cannot be recovered:
the underlying result file is not in this worktree, and the branch holding it never merged. There is no
committed artifact of an actual expiry anywhere in the tree. Whether the semaphore leak has ever
actually occurred is untested; there is weak evidence against it, since a leak should have hung one of
the four gate-taking tests that carry no timeout at all, but test ordering is not pinned so that is not
proof. An earlier report counted fourteen failing tests where a later one counted seven; those were two
different days under two different commands and they cannot be reconciled from what is in the
repository.

Each of these is carried forward as an open question rather than resolved by assumption, and the
acceptance criteria in the spec are written so that an honest negative result is recorded as such
rather than dressed up as a pass.

## Related records

Issues #711, #729, #592, #511, #571, #493 and #489. The last of these owns the viewer-interface surface
and its current disposition must be confirmed before the interface-widening part of the work proceeds.
