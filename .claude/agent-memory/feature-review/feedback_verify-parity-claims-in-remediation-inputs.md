---
name: verify-parity-claims-in-remediation-inputs
description: Never write a "for parity with the N sibling projects" justification into remediation inputs without first measuring it on disk; on #418 that claim was false and the executor correctly refused the directed change.
metadata:
  type: feedback
---

Before directing a build-configuration change in `remediation-inputs.<ts>.md` — especially an added
`<Reference>`, `packages.config` entry, or binding redirect — **measure every parity/consistency claim
against the working tree**. Do not infer sibling-project convention from one sampled project or from
what "should" be true given a transitive dependency graph.

**Why:** On issue #418 cycle 2, this agent wrote "Add `Fizzler 1.3.1` on the same pattern for parity
with the eight sibling test projects" into the remediation inputs. Every clause was false:
`grep -rn "Fizzler" --include=*.csproj .` returns only `SVGControl.csproj` and `UtilitiesCS.csproj`,
both **production**; **zero** test projects reference it and none carries `Fizzler.dll`. Worse, the
on-disk package is `Fizzler.1.3.1` (both production references declare `Version=1.3.1.0`) while
`SVGControl.Test/app.config` redirects `Fizzler` to `1.3.0.0`. Complying would have deployed a
`1.3.1.0` assembly into a project redirecting to an absent `1.3.0.0` — activating a stale redirect
that is inert today only because the file is missing. That is the *same defect class as issue #418
itself*. The atomic-executor refused the directive, documented the refutation, and was right. Had it
complied, a reviewer error would have shipped as a code defect.

The trap was that the reasoning felt sound: `ExCSS` and `Fizzler` are both transitive dependencies of
`Svg`, and the `ExCSS` half of the directive *was* correct. Symmetry of the dependency graph is not
evidence of symmetry in the checked-in project files.

**How to apply:**
- Any sentence in remediation inputs of the form "for parity with N siblings" / "as every other project
  does" / "matching the existing convention" must be preceded by the `grep`/`ls` that establishes it,
  and the command should appear in the Evidence cell.
- Check three things separately for a reference addition: (1) which projects declare the reference,
  (2) which outputs actually contain the DLL, (3) whether any `app.config` redirect names a version
  that differs from the on-disk package. Item (3) is what turns a harmless no-op into a live defect.
- When an executor refuses a directive and documents why, **verify the refutation independently rather
  than re-issuing the directive**, and record the correction against the reviewer artifact in the next
  cycle's code review and remediation inputs. Silent withdrawal loses the lesson.

Related: [[project_svgcontrol-stale-binding-redirect-out-of-scope]] for the sibling stale-redirect
class, and [[project_vstest-argument-order-transitive-dep]] for why legacy `packages.config` projects
need explicit references at all (no transitive copy-local, so ordinal position on the vstest command
line decides pass/fail).
