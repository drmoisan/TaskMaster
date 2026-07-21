# Phase 0 — Policy and Requirement Reads (Issue #270)

Timestamp: 2026-07-07T22-05

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific rules)

Files Read:
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md
- docs/features/active/2026-07-07-outlook-crash-async-void-sectiongroupname-270/issue.md (requirements source; `## Acceptance Criteria` AC1-AC6; Work Mode: minor-audit)
- docs/research/2026-07-08-sectiongroupname-argumentexception-crash.md (research input)
- docs/features/active/2026-07-07-outlook-crash-async-void-sectiongroupname-270/plan.md (plan of record)
- TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs (production file in scope)
- TaskMaster.Test/AppGlobals/AppEventsTests.cs (test file in scope)

Output Summary: All required policy files and requirement/research inputs read in the mandated order. Work Mode confirmed as minor-audit with issue.md as the sole AC source. CLAUDE.md Bugfix Workflow (failing regression test first, then minimal fix, then full QA loop) will be followed.
