---
name: csharpier-chain-wrap-defeats-singleline-search-gates
description: CSharpier wraps fluent/chained C# call expressions across lines, so a plan's zero-hit gate on a literal like `fsPath.Substring(3)` returns 0 hits BEFORE any work and gates nothing; always grep the exact literal during preflight
metadata:
  type: project
---

A plan's "fixed-string search returns zero hits after the fix (present pre-change, so the
gate can fail)" clause is only true if the literal occupies ONE line in the CSharpier-formatted
source. CSharpier breaks a chained call over multiple lines once the chain exceeds the print
width, so a plausible-looking literal composed from a receiver plus a chained member is
frequently absent from every single line.

Verified 2026-08-26 during #614 preflight, `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs`:

```
157            var fsPathExDividers = fsPath
158                .Substring(3)
159                .Replace($"{Path.DirectorySeparatorChar}", "");
```

`grep -Fc 'fsPath.Substring(3)'` returns **0**. The plan asserted this literal was "present
pre-change", so its zero-hit gate passed before the executor touched anything. This is exactly
the G6 case in `.claude/rules/plan-acceptance-gates.md` (a literal present only across a
line wrap), but the validator reports it only as a Warning, so it can reach an executor.

**Why:** the plan author reads a construct in a rendered file or from a research snapshot that
quotes the expression logically (`fsPath.Substring(3)`), not as the formatter emitted it.

**How to apply:** during preflight, run `grep -Fc '<literal>' <file>` for EVERY search-based
acceptance condition — zero-hit gates must return >= 1, at-least-one-hit gates must return 0.
Do not accept the plan's own "present pre-change" parenthetical as evidence. When a gate is
vacuous, the fix is either a single-line literal from the same construct (`.Substring(3)` on
its own line, or the full assignment line) or, preferably, a named test whose node ID is
stable under reformatting.

Related: [[feedback-verify-line-citations-with-numbered-output]],
[[csharpier-formats-xml-print-width]].
