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

## Execution-time remedy when it reaches the executor (2026-09-01, #648)

Preflight does not always catch it, because the literal does not exist in the tree yet: the plan
directs the executor to CREATE it. #648's `[P1-T2]` directed
`await UiThreadDispatcherFixture.BeginTransactionAsync().ConfigureAwait(false)` and `[P1-T6]` then
asserted the token `UiThreadDispatcherFixture.BeginTransactionAsync` matched a line. At the statement's
16-column indent the expression is 121 chars, so CSharpier emits the three-line chain break — the same
shape it had already produced at the sibling call site
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:47-49`. The token then
matches nothing whatever the executor writes.

**Confirm it empirically, do not infer it.** Write the directed form, run
`dotnet tool run csharpier check <file>`, and read the complaint. If the only complaint is
`different line endings`, the content shape you wrote IS CSharpier's output, so the chain break is not
avoidable by hand-formatting and the gate is genuinely unsatisfiable in that shape.

**Remedy that preserves the directed semantics:** split the single expression into two statements so
the qualified call lands alone on one line under the width limit.

```csharp
Task<UiThreadDispatcherTransaction> gate =
    UiThreadDispatcherFixture.BeginTransactionAsync();
UiThreadDispatcherTransaction transaction = await gate.ConfigureAwait(false);
```

A one-invocation chain is not broken by CSharpier; it breaks after `=` instead, which keeps
`Receiver.Method` contiguous. Same method, same awaited task, same `ConfigureAwait(false)`. Record the
deviation and its measurement in the verifying task's artifact — the deviating task's own acceptance
was "the four structural checks pass", so the shape that makes them pass is the shape it demanded.

Related: [[feedback-verify-line-citations-with-numbered-output]],
[[csharpier-formats-xml-print-width]].
