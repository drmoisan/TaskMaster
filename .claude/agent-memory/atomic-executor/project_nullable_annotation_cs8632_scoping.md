---
name: nullable-annotation-cs8632-scoping
description: In nullable-disabled TaskMaster projects a `Type?` reference annotation emits CS8632 (a new warning that fails no-new-warnings gates); scope it with `#nullable enable annotations`
metadata:
  type: project
---

Production projects like `TaskMaster.csproj` have LangVersion=preview but NO `<Nullable>` element, so the nullable annotation context is disabled. Adding a nullable-reference annotation (e.g. `System.Func<object, Task>? Prop { get; set; }`, or `Timer? t`) in such a file emits **CS8632** ("annotation should only be used in a #nullable annotations context"). CS8632 is a warning, so the analyzer build (`EnableNETAnalyzers`) still succeeds, but it is a NEW warning that fails a plan's "no new warnings vs baseline" gate.

**Why:** discovered on issue #270 — a `Func<object,Task>?` seam property added 2 CS8632 warnings. The repo already tolerates CS8632 in several production files (ApplicationGlobals.cs, NonBlockingDelay.cs, EngineInitTimingProbe.cs use bare `?`), but adding more still trips a strict no-new-warnings check.

**How to apply:** wrap just the annotated declaration(s) in a narrow annotations-only context:
```
#nullable enable annotations
    internal System.Func<object, System.Threading.Tasks.Task>? MyCollaborator { get; set; }
#nullable restore annotations
```
This keeps the `?` annotation valid (no CS8632) under the analyzer build AND correct under the `-p:Nullable=enable` type-check build, with zero side effects on the rest of the file (do NOT use a whole-file `#nullable enable` — it would surface CS8625/CS8618 on other members like `OlToDoItems = null;`). Confirmed both gates clean on #270.

Related: the forced-nullable solution build fails on ~84 pre-existing vendored errors (Swordfish 50, SVGControl 34) before reaching TaskMaster; prove no-regression by identical-diagnostic-set comparison, not by a green solution build. See [[project_repo_sdk_and_nullable_rebuild]].
