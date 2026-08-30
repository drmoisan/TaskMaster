---
name: project-644-cycle2-sweep-gate-evasion-seams
description: "#644 cycle-2 preflight seams: rewording a line out of a class-scoped detector's match set is gate evasion, not a fix; untracked prior artifacts need a SHA-256 invariance pair; porcelain ?? scope must be pathspec-bounded"
metadata:
  type: project
---

Three seams from the #644 cycle-2 remediation-plan revision round (eleven deltas, plan at
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/remediation-plan.2026-08-30T02-08.md`).

**1. Never reword a line out of a class-scoped detector's match set to hit a count.** The plan had
proposed inserting a word (`recorded key width`) into a corrected comment purely so it would stop
matching the sweep pattern `recorded (registration )?width|loop bound` and let the post-edit count
read 2. The adjudication: the pattern is a *proxy* for the defect class, and it already tolerates
past-tense hits. Once the line is corrected to past tense it belongs in the tolerated category, so
the right outcome is to enumerate it as an additional tolerated hit (2 to 3) and correct the count,
not to remove the line from every future run of the detector.
**Why:** the stated reason for the reword was "the original wording would have failed the sweep" —
which inverts gate and defect. The detector's future coverage is the asset; the count is not.
**How to apply:** when a correction leaves a line still matching a class-scoped gate, raise the
expected count and enumerate the new tolerated hit by file and line. Add a second, narrower pinned
token (here `#472 fix, it replayed the recorded width` = 1) so the gate still discriminates the
corrected form from the original text — the raw count alone stops being sensitive to that edit.

**2. A divergence from the cycle input's stated exit figure goes in the plan, not the input.**
`remediation-inputs.*.md` is a cycle-entry record and is never edited. Add a bullet to "Hard scope
limits" naming the input's figure, why it was premised on the rejected approach, and the corrected
figure with its enumeration.

**3. An anchored `git diff --name-status` cannot enforce invariance of files that are untracked at
cycle entry.** Four prior audit artifacts were `??`; an edit to any of them stays `??` and is
invisible to the name-status diff, yet the commit task stages them.
**How to apply:** capture `Get-FileHash -Algorithm SHA256` over the untracked prior-artifact set in
the Phase 0 baseline task and re-check the identical set in the containment task. This is the only
mechanism that observes their content. See [[diff-gates-need-a-commit-task]].

**4. A porcelain companion clause must bound where `??` entries may appear.** "No `M`/`A`/`D` entry
for any path other than X" leaves a stray untracked file under the co-staged pathspec (here
`QuickFiler.Test`) free to reach the commit. Require every `??` entry to be under the feature folder
and state explicitly that none is permitted under the code pathspec.

**5. `git log -1 --format=%B` yields one array element per line**, so a per-line `Select-String` for
`keyword\s+#644` cannot see a closing keyword and the issue number split across a newline. Join
first: `((git log -1 --format=%B) -join ' ') | Select-String ...`.

Related: [[project_644_ac16_referral_revision_seams]], [[project_644_pa7_redaction_plan_seams]],
[[zero-hit-grep-gates-need-carveouts]], [[wiring-gates-must-be-wiring-sensitive]].
