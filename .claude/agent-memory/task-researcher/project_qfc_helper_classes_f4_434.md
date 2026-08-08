---
name: qfc-helper-classes-f4-434
description: "Issue #434 (F4 of epic #136) conversation-resolution research decisions: dead-file dispositions favor retain+ledger over delete, because QuickFiler.csproj is under 13-way concurrent edit"
metadata:
  type: project
---

For epic #136 (`quickfiler-per-file-coverage`), child F4 / issue #434, the CONVERSATION-RESOLUTION
cluster research (2026-08-07) settled two dead-code dispositions the same way, and the reasoning
generalizes to the other ~24 declaration-only QuickFiler files:

**Decision:** for a file with an empty coverage denominator (no type declared, or interface-only),
recommend **retain the file + request an F1-ledger `no-coverable-lines` classification**, explicitly
distinct from `ratified-exempt`. Do NOT recommend deleting the file.

**Why:** deleting requires removing its `<Compile Include>` line from `QuickFiler/QuickFiler.csproj`,
a file all thirteen sibling children of the epic edit concurrently — the epic's highest-probability
merge-conflict surface. The coverage outcome is identical either way (an empty denominator is empty
whether the file exists or not), so deletion buys zero coverage benefit for a real conflict cost.
Dead-file removal belongs in a separate hygiene issue executed after the epic fans in.

**How to apply:** when researching any QuickFiler file in this epic and finding zero coverable lines,
apply the same recommendation and say so explicitly; also tell F1's harness to key its
has-a-denominator decision on the Cobertura `<line>` child count, never on `line-rate`, so
zero-denominator files are not mis-reported as 0% failures.

Related: [[qfc-item-controller-227-r2-denial]] (maintainer precedent against blanket coverage
exemptions), [[feedback-exemption-audit-check-proven-techniques]].
