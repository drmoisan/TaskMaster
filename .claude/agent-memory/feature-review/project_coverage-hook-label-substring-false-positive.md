---
name: coverage-hook-label-substring-false-positive
description: validate-feature-review-coverage.ps1's C# label regex matches "csharp"/"dotnet" as unanchored substrings, so words like "csharpier" or narrative prose anywhere in policy-audit.md can silently become coverage-row candidates subject to the narrowing-phrase check
metadata:
  type: project
---

`.claude/hooks/validate-feature-review-coverage.ps1`'s `Test-LanguageCoverageRow` label
pattern for CSharp is `(C#|CSharp|csharp|\.NET|dotnet)`, unanchored and case-insensitive. This
matches as a **substring**, not a whole word: "csharpier" (the formatter tool name) contains
"csharp" and therefore counts as a C#-labeled line; any line mentioning `dotnet-coverage`,
`dotnet tool run`, etc. counts too. Combined with the coverage-keyword filter
(`coverage|lcov|line[s]?\s+hit|pester`), this means **any line anywhere in policy-audit.md**
that happens to mention "csharpier"/"dotnet"/"C#" together with "coverage" becomes subject to
the narrowing-phrase check (`out of scope`, `not applicable`, `N/A`, `UNVERIFIED`,
`informational only`, `context only`, `out of plan scope`), even in prose far from the
intended coverage-verdict row (e.g., a sentence explaining `dotnet-coverage merge -f
cobertura`'s branch-rate conversion quirk).

**How to apply:** Before finalizing a policy-audit.md with a FAIL/PASS C# coverage verdict
plus surrounding disposition prose, run a quick Python/grep simulation of the hook's own
label+coverage+narrowing regex against the drafted file (see the one-liner pattern used in
#309's review) to confirm zero unintended narrowing-phrase hits before treating the artifact
as final. Word wrapping in markdown source matters: the hook scans by literal newline, so a
paragraph manually wrapped at ~80 chars is checked line-by-line, not as one logical sentence
— wrapping a narrowing word onto its own line (away from the C#+coverage co-occurrence) is a
legitimate way to avoid a false trip, but verify this deliberately rather than relying on luck.

Related: [[coverage-hook-forces-fail-below-floor-despite-exemption]],
[[pr-context-summary-misclassifies-cs]].
