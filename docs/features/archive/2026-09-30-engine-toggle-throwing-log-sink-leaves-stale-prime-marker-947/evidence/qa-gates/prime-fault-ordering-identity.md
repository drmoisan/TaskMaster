# QA Gate: PrimeFaultOrdering Byte Identity and Ribbon Test-Tree Scope (P2-T11)

Timestamp: 2026-10-01T18-10
Task: P2-T11
Command: git diff --exit-code 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85 -- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs
EXIT_CODE: 0

Output Summary:
- PrimeFaultOrdering anchored diff: exit 0 (byte-identical to BASE-SHA).
- PrimeFaultOrdering CMD-HASH: AA88DC05B45CE2E0D935014778779C5B500025BCFD237AEE05A184CED7D6F8DB = BASE-HASH-PFO: True.
- POSITIVE-CONTROL-DIFF-EXIT: 1 (`git diff --quiet BASE-SHA -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`; the comparison detects a change where one exists).
- `git diff --exit-code BASE-SHA -- TaskMaster.Test/Ribbon`: exit 1 (see LITERAL-CLAUSE-DIVERGENCE).
- `git status --porcelain --untracked-files=all -- TaskMaster.Test/Ribbon`: no output (see LITERAL-CLAUSE-DIVERGENCE).
- Paired BASE-SHA diff `git diff --name-status BASE-SHA -- TaskMaster.Test/Ribbon`: exactly one line, `A	TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs`.
- Result: the PrimeFaultOrdering identity and positive-control clauses hold as written; the Ribbon-scope intent (no tracked file under TaskMaster.Test/Ribbon changed; the only new file is the partial) holds when evaluated against the BASE-SHA diff.

LITERAL-CLAUSE-DIVERGENCE:
- The plan was written for an uncommitted run (plan Commits section, D-9). Phases 0 and 1 were committed by the orchestrator (433d5c2e2, caeb82c40) before this phase, so the partial is tracked at HEAD (`git ls-files` count 1).
- Clause "the TaskMaster.Test/Ribbon diff exits 0": observed exit 1. Its only difference is the added partial, which the anchored diff now sees because the partial is tracked. The name-status listing above shows no `M` or `D` line, so no pre-existing tracked file under TaskMaster.Test/Ribbon changed.
- Clause "the porcelain span prints exactly one line, `?? ...ThrowingSink.cs`": observed zero lines, because the partial is committed and unmodified. The paired BASE-SHA diff lists the partial as `A` and lists no other new file, which is the observation that clause was written to make.
- No criterion was weakened. The two literal clauses are recorded as not met in their literal form, and they are reported to the orchestrator.

## CMD-HASH

```
HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = EFC6F0DB3766495223014357FEAEE8D9AF530FA5A4C73717E42454D4FB3A05BF
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = BEB9C785536242293C3069E05BE54A355E2E030AE1AC06398C08AEAC188361FE
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = AA88DC05B45CE2E0D935014778779C5B500025BCFD237AEE05A184CED7D6F8DB
```

## Raw command outputs

```
PFO_DIFF_EXIT=0
POSITIVE-CONTROL-DIFF-EXIT: 1
RIBBON_DIFF_EXIT=1
RIBBON-NAME-STATUS-BEGIN
A	TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs
RIBBON-NAME-STATUS-END
PORCELAIN-BEGIN
PORCELAIN-END
TRACKED-AT-HEAD: 1
```
