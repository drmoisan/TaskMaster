---
name: parallel-surface-cannot-express-ordering
description: The parallel surface expresses ordering only as blast-radius contention; any "A must land before B" requirement needs the epic surface instead
metadata:
  type: project
---

The `parallel` surface cannot express a required order between items. `depends_on` and `wave` are
prohibited keys in both the run manifest and the planner checkpoint, and cohorts are derived purely
from computed blast-radius contention.

**Why:** the surface was designed for thematically unrelated items that share no dependency edge.
Contention only guarantees that two conflicting items do not run *concurrently* — it says nothing
about which runs *first*, because cohort indices come from Welsh-Powell degree ordering
(`(-degree, item_key)` ascending), not from intent. Requesting ordering from the operator is
explicitly out of scope for the planner.

**How to apply:** when an operator frames work as lanes with sequential items, or as ordered flights
("fix the coverage gates before certifying anything against them"), that is a dependency graph.
Route it to `/epic-plan` + `/epic-orchestrate`, which model explicit `depends_on` edges and wave
layering. Do not attempt to encode the ordering as artificially widened blast radii — the skill
forbids manipulating a radius to steer the conflict graph, and widening to force serialization is the
same manipulation as narrowing to suppress it.

A second, subtler gap: items outside the run's manifest are invisible to cohort scheduling. If
another surface (an epic, a manual branch) is concurrently editing shared build, coverage, or CI
surfaces, the parallel run cannot see the contention and will schedule against gates that are
actively changing. Check for in-flight work on shared surfaces before planning a run.

See [[parallel-surface-partial-port]] for the current port status and the config blocker.
