---
name: excludefromcodecoverage-attribute-ruling
description: Ruling precedent for the recurring CLAUDE.md-vs-rules conflict over class-level [ExcludeFromCodeCoverage] on TaskMaster production files; not Blocking, with the two-limb reasoning
metadata:
  type: project
---

Class-level `[ExcludeFromCodeCoverage]` on TaskMaster production files is **Not Blocking**. Ruled
this way at #731 for `QuickFiler/Controllers/QfcCollectionController.cs:21` and
`QuickFiler/Controllers/QfcDatamodel.cs:25`.

**Why:** `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy opens "No production file
may be excluded from coverage measurement" and tells feature-review to treat a production-path
exclusion as Blocking. CLAUDE.md § UT2 grants a maintainer-ratified COM/VSTO/WinForms exemption and
says it "is applied via `[ExcludeFromCodeCoverage]` attributes in source code." Two independent
limbs resolve it:
1. **Authority** — `policy-compliance-order` puts CLAUDE.md at level 1, `.claude/rules/*` at level 3,
   and CLAUDE.md's clause is the more specific one (names the mechanism, the code class, the
   ratifying authority).
2. **Operative text** — the rules file's Blocking clause enumerates coverage-config `exclude` glob
   entries (`dist/**`, `src/**`, `node_modules/**`, `jest.config.cjs`) and says "any `exclude` entry
   that matches a production source path." A C# source attribute is not an `exclude` entry. Only the
   preambles collide.

**How to apply:** Verify provenance first — `git log -1 -S "[ExcludeFromCodeCoverage]" -- <path>`.
In TaskMaster these came from `a564add0` (2026-06-13, `refactor(coverage): exempt COM/VSTO/WinForms
code from coverage metric (#197)`), so they are a pre-existing repository condition. State
explicitly whether you are ruling Blocking *for the change under review* or as a pre-existing
condition — that distinction decides whether it enters this issue's remediation loop or a separate
documentation-reconciliation issue. Same family as the unreconciled 80/90-vs-85/75 divergence in
[[build-ci-coverage-gate-fidelity-epic-outcome]].

**Always record the material consequence rather than waiving it.** An uninstrumented file means a
changed line there has *no* coverage observation — neither covered nor uncovered. At #731 that left
finding 4's `Volatile.Read` edit with only a self-disclaimed structural proxy test. Execution can
still be proven separately (the issue-#286 tests drive the enclosing method), so distinguish "not
measured" from "not exercised".
