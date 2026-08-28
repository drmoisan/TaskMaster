---
name: stale-figure-sweep-by-changed-file-set
description: When correcting stale figures after an upstream PR lands, sweep by the PR's full changed-file set and by both the spelled-out and numeric spellings of a count — a directive's named figure list is not exhaustive.
metadata:
  type: feedback
---

When a directive asks you to correct stale figures caused by an upstream PR, derive the
sweep from the PR's actual changed-file set (`git diff --stat <merge>^1 <merge>`), not from
the list of figures the directive names. Sweep every count in BOTH spellings: spelled-out
("seven test methods") and numeric ("7 `[TestMethod]`s").

**Why:** On the #498 figure correction the directive named four stale figures across two
files. #611 had actually touched four code files, and a fifth stale figure —
`FolderPredictorTests.cs` 985 → 1043 — sat in `spec.md` referencing a third file the
directive never mentioned. Separately, an initial `[Ss]even` grep missed two spec lines
that wrote the same stale count as `7 \`[TestMethod]\`s`; only re-reading the grep context
surfaced them. Either miss would have left the document wrong after a run whose entire
purpose was to make it right.

**How to apply:** Before editing, run the changed-file diff of the upstream merge commit
and grep the plan and spec for EVERY file it touched, not just the cited ones. Grep counts
as `\b(531|983|985)\b` plus the word form. Then re-grep after editing and confirm each
surviving hit is intentional — a historical "grew X to Y" provenance note, a "version 1.0
figure" column, or a genuinely different file. On this run plan line 45's "seven new
methods" referred to `BreadcrumbBridgeRouterTests.cs`, not the corrected file, and
correctly stayed put.

Prefer converting an absolute figure over a file the feature does NOT own into a
baseline-relative assertion ([[preflight-catches-vacuous-gates]] is the related failure
mode: a gate that reads as verification but cannot fail). Absolute counts over unowned
files are the recurring staleness source in this epic.
