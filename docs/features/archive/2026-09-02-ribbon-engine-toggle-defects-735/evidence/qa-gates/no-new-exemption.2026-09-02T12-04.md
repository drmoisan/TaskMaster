# Phase 4 — No New Coverage-Exemption Attribute (P4-T9)

Timestamp: 2026-09-03T03-24
Task: [P4-T9]
Command: `git add -N .; git diff (git merge-base origin/main HEAD) -- TaskMaster TaskMaster.Test`
EXIT_CODE: 0
Merge base re-derived at run time: `a679cd082819af6788cd0fb35f4366786fab87e3`
Diff size: 1598 lines.

The `git add -N` span is required because three of the five new source files would otherwise be
invisible to a diff against the merge base: an anchored `git diff` enumerates tracked changes only,
and an untracked file is not one. Intent-to-add makes them visible to the diff without staging their
content for a commit. The staged set was checked with `git status --porcelain` immediately
afterwards and contained only this change's own paths.

## The two required zero counts

| Check | Pattern | Count |
|---|---|---|
| Exemption attribute ADDED | `^\+\s*\[ExcludeFromCodeCoverage\]` | **0** |
| Exemption attribute REMOVED | `^-\s*\[ExcludeFromCodeCoverage\]` | **0** |

Zero added means no exemption is introduced anywhere in the change. Zero removed means no existing
exemption was relocated or rewritten, which is how a widening would most plausibly appear in a diff.
Together they establish that the change adds no exemption and widens none.

## Why the check is anchored to the attribute form

Two new host-neutral classes deliberately NAME the attribute in prose, in the XML-doc paragraph that
records its absence as intentional. An unanchored search for the attribute name would match those
sentences and report a violation on files that carry no attribute at all.

Every line in the whole 1598-line diff that mentions the attribute name in any form is quoted here,
and there are exactly two, both additions and both documentation:

```
+    /// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is host-neutral
+    /// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is host-neutral
```

The first is on `TaskMaster/Ribbon/SpamManagerResetGate.cs`, the second on
`TaskMaster/Ribbon/EngineTogglePressedStateCache.cs`. Each begins with `+` followed by whitespace and
then `///`, so neither can match `^\+\s*\[ExcludeFromCodeCoverage\]`, which requires the attribute
bracket immediately after the leading whitespace. This is precisely why an unanchored search would
have been wrong.

## Coverage disposition restated

The residual lines inside `ClearSpamManagerAsync` remain inside `RibbonController`'s pre-existing,
already-ratified type-level exemption. That exemption is untouched by this change: it is neither
added, moved nor widened, which the zero removed-count confirms. No coverage credit is claimed for
those lines anywhere in this change; they are validated by the manual-verification dossier instead.

Output Summary: Both counts are zero — no `[ExcludeFromCodeCoverage]` attribute line was added and
none was removed anywhere under `TaskMaster` or `TaskMaster.Test`. The only two diff lines mentioning
the attribute at all are XML-doc sentences on the two new host-neutral classes recording its
deliberate absence, and both are quoted above.
