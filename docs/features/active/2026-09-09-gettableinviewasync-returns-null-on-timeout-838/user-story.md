# gettableinviewasync-returns-null-on-timeout (User Story)

- **Issue:** #838
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12
- **Work Mode:** full-bug
- **Status:** Context only — not an acceptance-criteria source

> **Authority notice.** This document is narrative context only. It carries no acceptance criteria and
> no checkbox items. Work mode for this item is full-bug, so spec.md in this same feature folder is
> the sole authoritative acceptance-criteria source for issue #838. Any agent tracking, checking off,
> or verifying acceptance criteria must read spec.md and must not treat anything in this document as a
> criterion. Where this document and spec.md appear to differ, spec.md governs.

> **Formatting notice.** Repository paths are written here in plain prose without backticks, on
> purpose. The change footprint for this item is declared once, in the Write Set section of spec.md.
> Do not add backticked paths to this document.

## Who is affected

The person affected is the TaskMaster operator using the add-in inside the Outlook host, and
secondarily the developer who has to diagnose that operator's report.

## The narrative

Table acquisition for a mail folder runs under a millisecond deadline. When that deadline expires
without a table being produced, the acquisition method currently hands its caller a null instead of
reporting a failure. A null-forgiving suppression on the return statement keeps the compiler from
noticing, so nothing in the build or the logs marks the moment the read failed.

The consumer one frame up declares its local as a non-null table and dereferences it. The null
therefore travels past the point of failure and surfaces later as a null-reference error at the
frame-building boundary, carrying no folder name, no attempt count, and a type that does not describe
what actually happened.

Two costs follow from this. The operator sees a folder that produced no rows and cannot tell whether
the folder is genuinely empty or whether the read timed out. The developer reading the resulting error
sees a null-reference failure at a location that is not where the failure originated, and has to
reconstruct the deadline path by inspection because no signal was recorded.

The outcome sought is that the acquisition either returns a table or reports why it could not, at the
frame that still knows the retry count and the deadline budget, and using the same failure type the
adjacent step of the same pipeline already uses.

## Story 1 — a timed-out read is distinguishable from an empty one

- Given a mail folder whose table acquisition cannot complete inside its deadline
- When the operator triggers the action that reads that folder
- Then the failure is reported as a timeout that names the retry counter and the millisecond budget,
  rather than being indistinguishable from a folder that legitimately returned no rows

## Story 2 — cancellation stays quiet

- Given an operator who cancels an in-progress operation, or a host shutdown that cancels the token
- When the acquisition is abandoned as a result
- Then the outcome is reported as a cancellation and continues to take the existing quiet path, and is
  not reclassified as a timeout or an error

## Story 3 — the diagnosis lands at the right frame

- Given a developer investigating an operator report of a folder that produced no data
- When they read the captured error
- Then the error names the acquisition deadline at the frame where the deadline expired, instead of a
  null-reference failure at a later frame that omits the folder and the attempt count

## Boundaries of this story

This story covers the failure contract of the table-acquisition method only. It does not cover the
behaviour of the shared timeout helper for its other callers across the solution, and it does not
change the success path, any configuration surface, or any user-visible setting. The precise scope,
the design, and every verifiable criterion are stated in spec.md.
