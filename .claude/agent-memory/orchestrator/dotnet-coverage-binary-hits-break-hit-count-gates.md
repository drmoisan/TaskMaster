---
name: dotnet-coverage-binary-hits-break-hit-count-gates
description: dotnet-coverage Cobertura writes line hits as 0 or 1, so a plan gate comparing hit counts between lines is unsatisfiable; prove both branch outcomes from the branch row instead
metadata:
  type: project
---

The post-processed Cobertura from the TaskMaster coverage route reports `hits` as a binary covered flag: on 948 (2026-10-02) the maximum over all 133464 line elements was 1. A 948 plan clause "guard-line hits strictly greater than record-line hits" (to prove the skip branch ran) could never pass and stopped execution at P3-T10, after three preflight rounds had cleared it.

**Why:** preflight reads plans, not collector output; nobody had observed the success-case value.

**How to apply:** when a plan must prove both outcomes of an `if`, require the line's branch row (`condition-coverage` covered equal to total, total at least 2) and make a missing branch row fail. Show a negative control: the same predicate on a partially covered branch line in the same class evaluates False. Correcting it under a standing authority took three short confirming rounds (0.6 defects, 0.7 wording, clear); see [[delta-application-is-itself-a-defect-source]] and [[preflight-catches-vacuous-gates]].
