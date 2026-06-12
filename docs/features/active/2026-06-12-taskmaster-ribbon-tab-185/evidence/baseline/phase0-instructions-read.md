# Phase 0 — Policy Read Evidence (Issue #185)

Timestamp: 2026-06-12T10-36

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific code/test standards)

Files Read (in order):
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md

Mode: minor-audit. AC source: docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/issue.md (## Acceptance Criteria section, AC1–AC5).

Fail-closed check: no spec.md or user-story.md present in the active feature folder (confirmed by directory listing). Condition satisfied.

Scope confirmation: only TaskMaster/Ribbon/RibbonExplorer.xml and TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs are in scope. TabFolder and TabTasks are out of scope.
