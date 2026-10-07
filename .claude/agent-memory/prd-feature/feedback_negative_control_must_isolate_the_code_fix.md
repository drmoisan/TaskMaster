---
name: negative-control-must-isolate-the-code-fix
description: When a spec pairs a code fix with declarative hardening (config/binding redirect), pin the negative control's environment so the hardening cannot satisfy it
metadata:
  type: feedback
---

When a bug spec ships BOTH a code fix and a secondary declarative hardening (a `<bindingRedirect>`,
an analyzer severity bump, a runsettings entry), the negative-control criterion must name the exact
environment it runs in so the hardening cannot silently satisfy it. Otherwise the control flips to
passing after the hardening lands, and the positive test becomes vacuous without anyone noticing.

**Why:** on issue #879 the fix was an eager `AssemblyResolve` installer, plus a `netstandard`
`<bindingRedirect>` in `TaskMaster/app.config` as hardening. A child-`AppDomain` negative control that
inherited the production config would have bound `netstandard 2.1.0.0` through the redirect with the
installer absent, so it would have stopped failing on an unfixed build. The spec pins both child
domains' `ConfigurationFile` to the TEST assembly's `.dll.config` (no `netstandard` entry, and out of
scope to change) and adds a separate AC asserting that config declares no such redirect — the
guarantee is checked, not assumed.

**Staged fail-before when one It covers two halves (seen on #973, 2026-10-02).** Pester stops an It at
its first failing `Should`, so a single red run of a test that asserts both "zero stale pairs" and
"unverifiable set equals the deliberate remainder" can only show the first failure. When the fix has
two independent halves (a config sweep and a package install), require two fail-before artifacts:
one before any edit (shows the first assertion's diagnostic list), and one after half A lands but
before half B (first assertion passes, second fails naming half B's subject). The second artifact is
the negative control proving half B is load-bearing; without it, "the sweep alone turns the test
green" would be indistinguishable from the full fix. Give every assertion that can be the first
failure a `-Because` that joins the observed values, or the red artifact carries no list.

**How to apply:** for any AC of the form "X still fails without the fix", write down (a) which config
file the control reads, (b) which handlers/attributes are installed, (c) an assertion that each of
those preconditions actually holds, and (d) an in-file comment stating that a passing control means
isolation was lost. Related: [[feedback_ac_gates_verify_satisfiability]],
[[reference_suggestion_severity_invisible_to_msbuild]].
