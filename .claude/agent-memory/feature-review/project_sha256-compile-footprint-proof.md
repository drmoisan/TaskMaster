---
name: sha256-compile-footprint-proof
description: SHA-256 hashing used as stronger-than-mtime proof of compile state and footprint binding in two reviewed PRs
metadata:
  type: project
---

Consolidated from individual review-residuals findings:

- **#644**: SHA-256 beats file mtime as proof a file was actually recompiled/unchanged since a given
  point — mtime can be touched by tooling (formatters, IDE saves) without a real content change, so
  when adjudicating "was this rebuilt after the fix," hash the artifact rather than trusting its
  timestamp.
- **#647**: A footprint SHA-256 table (one hash per changed file, captured at a specific plan task and
  re-checked at the final gate) binds every later gate's pass/fail verdict to the same HEAD content —
  if a file's hash changed between the table's capture and a later gate, that gate's verdict is stale
  and must be re-run, not trusted.
- **#464**: 7 promotions were owed after that review (see RC7 EfcSelectionGuard "===" arity finding);
  the review also hit a shared info/exclude trap — commit coverage artifacts on the feature branch
  itself rather than relying on a shared/global exclude list, or the artifact silently reads as absent
  for that PR.

**How to apply:** When a plan or executor claims a file was rebuilt, formatted, or unchanged at a
specific point, prefer a SHA-256 comparison over an mtime comparison if both are available. When a
plan uses a footprint hash table to bind gates to HEAD, re-verify the table's hashes still match the
current git blob before trusting any gate that cites it.
