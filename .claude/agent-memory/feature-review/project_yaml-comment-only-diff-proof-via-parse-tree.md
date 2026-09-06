---
name: yaml-comment-only-diff-proof-via-parse-tree
description: Prove a "comment-only" YAML workflow diff by order-sensitive parse-tree comparison of base vs head with two parsers (PyYAML + powershell-yaml/YamlDotNet), because a comment indented deeper than a preceding plain scalar could otherwise be folded into that scalar
metadata:
  type: project
---

When a workflow diff claims to be comment-only, do not accept it from reading the diff. On #730 the
inserted 16-line `#` block sat at 12-space indent between `key:` and `restore-keys:`, which are both
at 10 spaces. A more-indented line following a plain scalar can be folded *into* that scalar, which
would have silently corrupted the `actions/cache` key to include the comment text.

**Why:** the diff looks obviously inert to a human reader, and every prior preflight round on #730
accepted it as such. Only an actual parse settles it. The result was clean here, but the failure mode
is silent — a corrupted cache key produces no error, just a permanent cache miss.

**How to apply:**
1. `git show origin/main:<path>` each base revision into scratch (via `pwsh`, not Bash — MSYS mangles
   the `rev:path` colon into `;` and flips the slashes).
2. Parse base and head with **two** parsers and deep-compare with key order preserved
   (`json.dumps(..., sort_keys=False)` / `ConvertTo-Json -Depth 40`), so a reorder is also caught:
   - PyYAML via `pwsh -NoProfile -Command "python <script>"` (no `pyproject.toml`, so `poetry run` fails)
   - `powershell-yaml` 0.4.12 (YamlDotNet) is preinstalled — the closer analogue to what Actions runs
3. Also print the resolved `key` / `restore-keys` scalars to show no comment text was absorbed.

Both parsers agreeing that the parse trees are identical is what makes "no key, value, step, or job
added, removed, or reordered" an evidenced claim rather than a restatement of the diff. Also check
that comment text asserting runtime behavior is *true* (on #730: that `nuget restore` is ungated — read
the next step for an `if:` / `cache-hit` condition).
