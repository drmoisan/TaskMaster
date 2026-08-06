---
name: literal-call-clauses-block-file-size-tightening
description: acceptance clauses that require a literal call in two+ places remove the tightening lever and can make the 500-line limit unsatisfiable — plan the type split up front
metadata:
  type: feedback
---

When a plan's acceptance clauses require a specific call to appear **literally** in more than one location (e.g. issue #418 `[P1-T11]` required both `logger.Error` and `Trace.TraceError` inside the `catch`, and `[P1-T14]` required both in **both** constructors), those clauses forbid centralizing the duplication. If the target file is also near the 500-line limit, the plan has removed its own remedy and the executor returns `SCOPE_EXCEEDED` with no legal move.

**Why:** `.claude/rules/general-code-change.md` grants file-size exceptions only to throwaway scripts, raw text fixtures, and Markdown — a production `.cs` file has **no** waiver path. So the limit is hard, and "tighten the added code" is not a reliable escape clause when acceptance text pins the duplication. Issue #418 hit exactly this: `SvgRenderer.cs` landed at 547 lines, a genuine tightening pass (shared describe-failure helper, shared const, XML docs demoted to `//`, merged guards) only reached 547 from 603, and all three remaining levers (centralize logging, chain constructors, delete pre-existing members) each broke an acceptance clause or exceeded scope.

**How to apply:** when planning additions to a file already past roughly 400 lines, decide the type split *in the plan* rather than leaving a tighten-until-it-fits task. Prefer extracting members that are pure and cohesion-justified independently of the line count (pure path/string helpers, parsing, arithmetic) into a new `internal static` type in its own file — that survives review as a design decision rather than a line-count dodge. Remember the legacy-csproj consequence: a new source file in a non-SDK project needs an explicit `<Compile Include>` item (see [project_legacy_csproj_explicit_compile_include](project_legacy_csproj_explicit_compile_include.md)), and check whether an assembly-scope `InternalsVisibleTo` already exists before planning a redundant one.
