# P0-T7 Baseline encoding observation for the 11 Write Set configs

Timestamp: 2026-10-02T03-08
Command: git -C <execution-worktree-root> ls-files --eol -- <the 11 paths, one invocation>; pwsh -NoProfile -Command '(Get-Content -LiteralPath <execution-worktree-root>/<path> -AsByteStream -TotalCount 3) -join [char]44' (once per path, 11 invocations); Grep pattern `^` count mode and Grep pattern `\r$` count mode, glob `**/app.config`, path `<execution-worktree-root>` (glob substitution as recorded in p0-t4; the 11 Write Set files are read from the 17-file result)
EXIT_CODE: 0

CMD-EOL (first field is the index terminator and is recorded, not gated; the second field is the gated working-tree terminator):

```text
i/lf  w/crlf  QuickFiler/app.config
i/lf  w/crlf  QuickFiler.Test/app.config
i/lf  w/crlf  SVGControl.Test/app.config
i/lf  w/crlf  Tags/app.config
i/lf  w/crlf  TaskMaster/app.config
i/lf  w/crlf  TaskTree/app.config
i/lf  w/crlf  TaskVisualization/app.config
i/lf  w/crlf  TaskVisualization.Test/app.config
i/lf  w/crlf  ToDoModel/app.config
i/lf  w/crlf  ToDoModel.Test/app.config
i/lf  w/crlf  UtilitiesCS.Test/app.config
```

Every line carries `attr/text=auto`. The 11 paths were passed in one `git ls-files --eol` invocation rather than 11; the output is one line per path, identical in form to the per-path call.

CMD-BOM:

```text
BOM-BYTES QuickFiler/app.config=239,187,191
BOM-BYTES QuickFiler.Test/app.config=239,187,191
BOM-BYTES SVGControl.Test/app.config=239,187,191
BOM-BYTES Tags/app.config=239,187,191
BOM-BYTES TaskMaster/app.config=239,187,191
BOM-BYTES TaskTree/app.config=239,187,191
BOM-BYTES TaskVisualization/app.config=239,187,191
BOM-BYTES TaskVisualization.Test/app.config=239,187,191
BOM-BYTES ToDoModel/app.config=239,187,191
BOM-BYTES ToDoModel.Test/app.config=239,187,191
BOM-BYTES UtilitiesCS.Test/app.config=239,187,191
```

CMD-LINECOUNT (`^` count) paired with `\r$` count:

```text
LINECOUNT QuickFiler/app.config lines=243 cr=243 equal
LINECOUNT QuickFiler.Test/app.config lines=367 cr=367 equal
LINECOUNT SVGControl.Test/app.config lines=227 cr=227 equal
LINECOUNT Tags/app.config lines=239 cr=239 equal
LINECOUNT TaskMaster/app.config lines=430 cr=430 equal
LINECOUNT TaskTree/app.config lines=239 cr=239 equal
LINECOUNT TaskVisualization/app.config lines=239 cr=239 equal
LINECOUNT TaskVisualization.Test/app.config lines=363 cr=363 equal
LINECOUNT ToDoModel/app.config lines=323 cr=323 equal
LINECOUNT ToDoModel.Test/app.config lines=363 cr=363 equal
LINECOUNT UtilitiesCS.Test/app.config lines=383 cr=383 equal
```

All eleven counts equal the plan's expected values.

Acceptance: 11 `w/crlf` lines, 11 BOM-BYTES lines (all `239,187,191`, so the Phase 1 preservation reference is `239,187,191` for every path), 11 equal count pairs.

Output Summary: All 11 Write Set configs are UTF-8 BOM (239,187,191) with CRLF in the working tree (w/crlf), and every line carries a CR (line count equals CR count). Matches the plan's expected line counts.
