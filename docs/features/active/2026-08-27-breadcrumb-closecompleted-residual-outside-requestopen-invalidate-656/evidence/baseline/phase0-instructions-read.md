# Phase 0 — Policy Instructions Read (Issue #656)

Timestamp: 2026-09-01T14-33
Task: [P0-T1]
Policy Order: as required by `.claude/skills/policy-compliance-order/SKILL.md` — repository standing
instructions first, then the cross-language code-change policy, then the cross-language unit-test
policy, then the language-specific rules for the files in scope (C#).

Files read, in the required order:

- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`

EXIT_CODE: 0

Output Summary: All four policy files were read in the order listed above. The first three are
auto-loaded into the session as project instructions; `.claude/rules/csharp.md` was read explicitly
from disk in this session. Controlling constraints extracted for this item: C# toolchain order is
format -> analyze -> type-check -> test with a restart on any failure or file rewrite; CSharpier is
invoked only through `dotnet tool run`; msbuild gates use `/t:Rebuild` and never `/t:Build`; the
nullable gate must not carry `/p:Nullable=enable`; tests use MSTest with FluentAssertions and Moq;
no production file may be excluded from coverage; the 500-line file-size limit applies to both files
in this item's footprint.
