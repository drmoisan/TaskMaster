---
name: pr-context-bundle-autoclose-list-is-unsafe
description: collect_pr_context has three coupled defects — it invents auto-close issue numbers, falsely reports gh unavailable, and (worst) misclassifies C# sources as docs so C# coverage enforcement is silently skipped
metadata:
  type: project
---

The PR-context bundle is wrong in two coupled ways on nearly every item, and the pair is more
dangerous than either alone. Instruct every execution child to distrust both.

**Defect 1 — a fabricated auto-close list.** The bundle's author-asserted close list is scraped from
prose and contains issues the item merely CITES plus tokens that are not issues at all. Observed on
three consecutive items of run `bugs-638-644-647`:

| Item | Bundle's list | Reality |
| --- | --- | --- |
| 633 | `#468`, `#633`, `#ISO-8601` | `468` a different already-closed issue; `ISO-8601` scraped from timestamp-format prose |
| 646 | `#442`, `#646`, `#647`, `#CR-1` | `442` and `647` different already-closed issues cited as context and as explicitly out of scope; `CR-1` a code-review finding identifier |
| 287 | (same class) | — |

Emitting any of those verbatim posts a closing keyword against an unrelated issue.

**Defect 2 — a false unavailability report.** The same bundle reports
`GitHub CLI unavailable: GitHub CLI (gh) is not installed` while `gh` 2.87.3 is installed and answers
every query. The `pr-author` skill's own fallback says to emit NO `Closes` bullet when GitHub
validation is unavailable, so believing the false report silently leaves the item's issue open on
merge.

**Why the pair is worse than either half:** one defect invents issue numbers, the other disables the
verification that would catch them. A child that trusts the bundle either closes the wrong issue or
closes nothing.

**Defect 3 — it silently skips C# coverage enforcement. This is the severe one.** Established on
item 663 by the reviewer LOADING THE HOOK DIRECTLY rather than inferring from output: the tooling
classifies changed `.cs` source files as **documentation** and reports `Core logic changes: 0 files`.
That leaves `changedLanguages` **empty**, and an empty language set causes C# coverage enforcement to
be skipped — with no message saying it was skipped.

Seven consecutive items had reported only the symptoms above. The mechanism changes what the defect
IS: not a nuisance that produces a discardable list, but a **quality gate that silently does not
run** on every C# item. A passing review is therefore weaker evidence than it appears, and no amount
of care by the child compensates, because nothing surfaces the omission.

**The coverage skip has TWO independently sufficient causes.** Item 670 found the second: the
bundle enumerates only the **top ten changed paths by churn**. That item's two committed Cobertura
documents run to roughly 194,000 lines each, so they displace every `.cs` path from the list, and
`validate-feature-review-coverage.ps1` parses only those churn-annotated lines. Empty set, silent
pass — by a completely different route than the docs-misclassification.

State the consequence plainly, because it inverts the usual intuition: **the more thorough an item's
committed evidence, the more certainly the gate that would judge it stops running.** Fixing only the
file-type classification would leave this half live.

**How to apply this third defect specifically:**

- Never treat the bundle's language classification or its `Core logic changes` count as evidence.
  Derive the changed-file set yourself from the anchored diff.
- Tell each child explicitly that it cannot rely on the bundle to tell it whether coverage
  enforcement ran, and that C# coverage must be confirmed from the coverage run's own output.
- Weight this above the auto-close pollution when filing the repository-level issue. Closing the
  wrong issue is visible and reversible; an unrun coverage gate is neither.

**How to apply:**

- Put it in the kickoff prompt. Tell the child: do not trust the bundle's close list or its
  unavailability claim; verify every number with `gh issue view <n>`; close only this item's own
  issue; and note the bundle error in the PR body so a later reader does not re-derive the bad list.
- **Verify the outcome parent-side from the live pull request**, not from the child's report:
  `gh pr view <n> --json closingIssuesReferences` must contain exactly one entry, the item's own
  issue. That is observer-side and cheap.
- **Deviating from the skill's fallback is correct here when the child has direct verification**,
  and should be recorded rather than done silently. The fallback exists to prevent closing an
  UNVERIFIED issue; it is not a reason to leave a verified issue open. The 646 child made exactly
  this call and stated its reasoning.
- This is a repository-level defect in the collector, not an item defect, so it belongs in its own
  issue rather than in any item's follow-up list. See
  [[qfc-collection-468-family-shipped-issues-left-open]] for the separate, long-standing reason
  issues in this repository go un-closed.
