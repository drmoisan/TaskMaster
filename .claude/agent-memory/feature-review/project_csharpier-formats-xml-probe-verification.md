---
name: csharpier-formats-xml-probe-verification
description: CSharpier 1.3.0 formats XML in TaskMaster (no .csharpierrc so width=100, .csharpierignore lacks *.xml); reproduce a "formatter mandated this layout" claim with a scratch probe file instead of accepting it
metadata:
  type: project
---

CSharpier 1.3.0 in TaskMaster formats `*.xml`, not just `*.cs`. `.csharpierignore` excludes
`**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`, `*.csproj`, `*.props`,
`*.targets` — but **not** `*.xml` generally. There is no `.csharpierrc` anywhere in the repo, so the
default print width of **100 columns** applies to ribbon XML.

**Why:** #503 cycle 1 asked the executor to collapse three `<button>` elements in
`TaskMaster/Ribbon/RibbonExplorer.xml` back to single-line form to reduce line count. The executor
reported it "not remediable" and escalated. That is a claim a reviewer must not take on trust — it is
exactly the shape of an excuse for skipping requested work.

**How to apply:** reproduce it, do not reason about it. Write a minimal `customUI` document into the
scratchpad containing the collapsed element next to a short sibling, then run
`csharpier check <probe>.xml`. In #503 this returned `Error - Was not formatted` and CSharpier's
"Expected" block expanded **only** the 116-character element while leaving the 78-character sibling
single-line — proving the multi-line form is formatter-mandated rather than incidental churn. The
merge-base single-line `<button>` is 78 chars; adding `getEnabled="EngineCommand_GetEnabled"` makes
it 116, over the 100 limit.

Corollary: a line-count reduction target for ribbon XML is unreachable while `csharpier check .`
must return 0. The only real route is splitting the resource, which is its own issue.

Related: [[project_two-vstest-binaries-binding-redirect]], [[project_nullable_build_gate_is_vacuous]].
