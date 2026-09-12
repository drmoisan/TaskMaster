---
name: csharp-seam-default-cs0236-and-intermediate-consumers
description: Two C# seam-planning defects that survive plan review but fail preflight — a property-initializer default capturing an instance field (CS0236), and a pure-function extraction whose returned value differs from a value consumed between the rewired call sites
metadata:
  type: feedback
---

When planning C# seams, check both of these before writing the acceptance clause.

**1. An injectable-delegate seam whose default touches instance state cannot use a property initializer.**
`internal Func<...> Seam { get; set; } = (a, b) => _field.Method(a, b);` is a field initializer and produces
`error CS0236: A field initializer cannot reference the non-static field, method, or property '<Type>._field'`.
Plan it as a backing field plus a lazy default instead:
`private Func<...> _seam;` / `internal Func<...> Seam { get => _seam ?? DefaultSeam; set => _seam = value; }` / `private <T> DefaultSeam(...) => _field.Method(...);`.
A **method-group** default (`= FileIO2.WriteTextFileAsync;`) is fine because it targets a static — the CS0236 hazard is
specifically an *instance* member reference.

**Why:** #433 F7 `[P3-T3]` (seam M3 over `_metrics.TryAdd`) shipped the initializer form; `atomic-executor` preflight
compiled it against `csc.exe` and returned `PREFLIGHT: REVISIONS REQUIRED` with the literal CS0236. The task's
acceptance was unsatisfiable as written.
**How to apply:** any time a seam default names a `_`-prefixed field or any instance member, write the backing-field
form and put "a property initializer must not be used, it is CS0236" into the acceptance text so the executor cannot
regress to the shorter shape.

**2. A pure-function extraction must be checked for consumers *between* the rewired call sites.**
Extracting an inline computation into a helper and rewiring "the block at line X and the block at lines Y-Z" silently
changes behaviour when a line between X and Z reads the *pre*-transform value. In #433 `[P3-T1]`, `BuildDurationTexts`
divides elapsed seconds by `emailsLoaded`, but `WriteMetricsAsync:123` consumed the **pre-division** value to compute
`OlStartTime`; the rewire straddled it.
**Why:** no test could catch it — `Stopwatch.Elapsed` is not injectable, so the acceptance clause was the only control.
**How to apply:** before writing an extraction task, read every line in the span between the first and last rewired
call site and list the intermediate consumers. If one reads a pre-transform value, name it in the acceptance clause and
state explicitly that assigning the transformed return into that variable fails the task. Related:
[[csharp-pure-move-extraction-pattern]], [[literal-call-clauses-block-file-size-tightening]].
