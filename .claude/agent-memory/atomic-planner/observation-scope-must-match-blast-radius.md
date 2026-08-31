---
name: observation-scope-must-match-blast-radius
description: A gate's observation must cover its command's full blast radius in space (paths), in time (later-written files), and in spelling (every form the target can take), or it passes while the defect stands
metadata:
  type: feedback
---

A gate whose observation is narrower than the thing it guards returns GREEN while the defect it exists to catch is present. Three faces of the same mistake, all found in one preflight round on issue #644:

**Space.** `dotnet tool run csharpier format .` at the repo root is file-based and processes `*.cs`, `*.xml`, and `packages.config` across the whole tree. A before/after `git status --porcelain -- <one-project>` observation cannot see a rewrite anywhere else, and the following `csharpier check .` then passes because the tree has already been normalised by the very command under test. Scope the porcelain to `-- . ':!.claude/agent-memory'` (that one exclusion is required because other agents leave unrelated modifications there; see [[agent-memory-is-tracked-scope-git-gates]]).

**Time.** A sweep placed at the end of an implementation phase cannot fail for anything written into the swept folder by later phases. On #644 the PA-7 sweep sat at the end of Phase 1 while seven further tasks each wrote an evidence artifact into the same feature folder and the commit task then staged the whole folder — so the stated exit condition was never verified for the state actually committed. Fix: add a terminal sweep in the final phase, immediately before the commit task, and state in the commit task's prose that it writes no artifact of its own so nothing lands after the sweep. If the terminal sweep's own artifact lands inside the folder it sweeps, it must run the patterns **twice** — write the artifact, then re-run with it present — because a single run cannot observe itself.

**Spelling.** A detection regex must cover every form the target actually takes in the tree. `[A-Za-z]:[\\/]Users[\\/]` matches exactly one separator character after the drive colon, so it is blind to a path written with **doubled** backslashes as regex escapes, and it requires a drive prefix, so it is blind to a bare token. Pair a shape pattern with a token pattern that derives the token at run time (`$t=[regex]::Escape((Split-Path -Leaf $env:USERPROFILE)); Select-String -Pattern "(?i)$t"`) — see [[runtime-derived-account-token-pattern]].

**Why:** each face lets the acceptance condition return the passing value regardless of executor behavior, which is the unfalsifiable-gate failure the atomic-plan contract exists to prevent.

**How to apply:** for every gate, ask three questions before writing the acceptance — what paths can this command touch, what files land after this gate runs, and what spellings can the target take. Widen the observation to the answer, not to the task's nominal subject.
