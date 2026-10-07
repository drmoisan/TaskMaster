---
name: project-959-r2-pre-existing-evidence-ordering-and-no-git-channel-seams
description: #959 preflight round 2 seams - an "unless PRE-EXISTING-EVIDENCE names it" exemption is always true for an artifact written by an earlier task; evidence-field checks must subtract Clause A minus the run's own Phase 0 artifacts; with no Bash a caller's origin/main claim is recorded as the caller's statement; cite .gitignore by content
metadata:
  type: project
---

Seams found while applying the #959 preflight round 2 delta (plan revision 1.2, 2026-10-02).

1. Ordering of "pre-existing" fields. P0-T2 records `PRE-EXISTING-EVIDENCE:` as every path already under FEATURE/evidence/, but P0-T1 has already written phase0-instructions-read.md by then. A rule of the form "subtract the P0-T1 artifact from INHERITED unless PRE-EXISTING-EVIDENCE names it" therefore fires on every run and exempts the artifact it meant to check. Fix: the recording task excludes the artifacts this run wrote before it, and the rule says why.

2. Evidence-field checks versus Clause A. When Clause A (paths already changed at P0-T3) is captured after P0-T1 and P0-T2 wrote their artifacts, those two artifacts are inside Clause A, so a field check that subtracts Clause A whole skips them. Subtract Clause A minus the run's own Phase 0 artifacts, and run the check twice: once mid-Phase 6 and once as the penultimate command (after every late artifact, before the final sweep). The check should demand exactly the fields the AC names (three here: Timestamp, Command, EXIT_CODE), not the four of the artifact-fields convention, or tasks that never name `Output Summary:` become unsatisfiable.

3. No Bash in the planner session. A caller-supplied reason such as "origin/main changed unrelated lines of .gitignore after SHA (#961)" cannot be verified without a git channel; write it as the caller's statement and make the plan independent of it (content-only citation, excluded from the CITED-TREE pathspec). Remove the file from PATHS-CITED and amend the definition sentence ("by line, together with every path cited by content other than X"), or the definition contradicts itself.

4. ANCHOR MOVED must be observable. A convention that says "MERGE-BASE is derived once and never re-derived" still needs a late task to re-issue `git merge-base HEAD origin/main` (no fetch) and compare it with the recorded value; phrase it as an observation compared with the anchor so it does not contradict the convention.

5. Compile-red spans: a test file written in task N against a seam landed in task N+2 is a span even when the build task is two tasks later; name the first green build task after verifying the intermediate task only formats.

**Why:** each of these produced a preflight finding on round 2 that round 1 could have avoided.

**How to apply:** when a plan has Phase 0 artifacts written before the Clause A capture, when a caller supplies git-history reasons, and whenever an "unless field X names it" exemption refers to a field recorded after the exempted path was created.
