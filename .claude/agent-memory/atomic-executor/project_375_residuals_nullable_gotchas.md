---
name: 375-residuals-nullable-gotchas
description: #375 residuals nullable child — CS8644 inherited-interface island fix, SVGControl ProjectReference re-clean trap, MeetingItemHelper bulk-annotation pattern
metadata:
  type: project
---

Executing per-file `#nullable enable` opt-in for the residuals child (#375) surfaced three
non-obvious mechanics beyond the known pragma-gate baseline blockers ([[project_nullable_pragma_gate_mechanics]], [[project_364_nullable_gate_preexisting_blockers]]).

**CS8644 (inherited interface-nullability mismatch) from an oblivious base is not fixable with `?`/`!`/`= null!`.**
Why: `PeopleScoDictionaryNew : ScoDictionaryNew<string,string>, IPeopleScoDictionaryNew` — adding
`#nullable enable` re-evaluates the inherited ICollection/IDictionary/ISmartSerializable
implementations (provided by the still-oblivious #366 base ScoDictionaryNew/ConcurrentObservableDictionary)
and raises 22 CS8644 at the class-declaration line. Fix: keep the class-declaration/base-list line in
a `#nullable disable` region and re-enable for the member bodies (`#nullable enable` right after `{`).
The class's OWN members (CS8618 fields, CS8603 returns) stay fully checked; only the inherited-interface
check goes oblivious. Uses only nullable region pragmas — no warning-ID suppression, no WarningsNotAsErrors
change. This is the #366-undeclared-edge (spec Maintainer Decision 5). How to apply: whenever a nullable
opt-in of a class derived from an un-opted #366 Sco*/Observable base raises CS8644, use the
class-declaration-line `#nullable disable` island, not `!`.

**Isolated-gate trap: a full-solution `/t:Rebuild` cleans SVGControl.dll and then the isolated
UtilitiesCS build fails CS0006 (SVGControl.dll not found), NOT a nullable signal.**
Why: UtilitiesCS has a ProjectReference to SVGControl; the mandated full-solution TWAE Rebuild cleans
SVGControl's output but can't rebuild it (pre-existing CS0649 under TWAE), so `-p:BuildProjectReferences=false`
can't resolve the reference afterward. How to apply: after ever running the full-solution Rebuild,
re-run a no-TWAE full-solution `-t:Build` to regenerate SVGControl.dll/UtilitiesCS.dll before the next
isolated `UtilitiesCS.csproj ... -p:BuildProjectReferences=false` gate.

**MeetingItemHelper-style Lazy<T>-backed property classes: bulk `= null!` + `(...)!` beats widening the API.**
~30 `private Lazy<string> _x;` fields uninitialized in the parameterless ctor → ~218 CS8618; the
`get => _x?.Value;` getters → CS8603. Set every ctor-unset field `= null!` (scriptable), keep the public
property types non-null (they implement the oblivious IItemInfo — no CS8766), and wrap getters
`get => (_x?.Value)!;` and Lazy-factory lambda returns `(...)!;` — behavior-preserving compile-time
no-ops that return the same value (null included) and avoid cascading nullability into dozens of internal
consumers. `ToLazy`/`ToLazyValue`/`ToLazyTry` are oblivious so setters `value.ToLazy()` never cascade.
For CRLF+BOM production files, use a Python utf-8-sig read/write to bulk-apply, not git-bash sed/perl.
