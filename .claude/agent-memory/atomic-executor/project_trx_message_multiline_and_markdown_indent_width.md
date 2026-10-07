---
name: trx-message-multiline-and-markdown-indent-width
description: CMD-VSTEST "MESSAGE line contains X and matches Y" gates break on multi-line trx messages (Moq Verify); plan line-width claims often count the 4-space Markdown indent
metadata:
  type: project
---

Two preflight checks found on the #947 plan (2026-10-01), both easy to miss by reading.

1. **Multi-line trx failure messages.** The CMD-VSTEST payload family (the #944/#947 plans) prints
`MESSAGE <name> :: $msg.InnerText`. A trx `ErrorInfo/Message` can span several physical lines: for a
Moq `Verify(..., Times.Exactly(2), "reason")` failure the custom reason and the
`Expected invocation on the mock exactly 2 times, but was 1 times` text sit on separate lines.
A gate saying "the MESSAGE line contains <reason> and matches <count pattern>" is then unsatisfiable
for a line-oriented reader. The #944 run never exercised this path, because its Moq re-prime tests
passed before the fix. **Why:** that run's only recorded MESSAGE came from FluentAssertions and fit
on one line, so the shape looked proven when it was not.
**How to apply:** when a fail-before gate asserts two fragments of one message, require the
payload to collapse line breaks, for example `-replace "\s*\r?\n\s*", " / "`.

2. **Line-width claims in plans.** Delivered-source blocks carry 4 extra Markdown-indent spaces,
and planners sometimes count them when they claim that a statement is over 100 characters and
CSharpier wraps it. The repo has no `.csharpierrc` and no `max_line_length`, so the print width is
the default 100. Recount at the in-file indentation before accepting or reporting a wrap claim.
Related: [[csharpier-chain-wrap-defeats-singleline-search-gates]].
