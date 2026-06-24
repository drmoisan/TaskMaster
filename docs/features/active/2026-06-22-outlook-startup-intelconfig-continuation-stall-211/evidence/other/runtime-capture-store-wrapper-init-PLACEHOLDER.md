# Runtime Capture — [store-wrapper-init] + [phase-net] (issue #211, Phase 3.6) — PLACEHOLDER

Timestamp: <maintainer fills ISO-8601 of the capture run, e.g. 2026-06-25T09-15>

Status: MAINTAINER-GATED / RUNTIME. This artifact is NOT CI-automatable. It requires a live Outlook
process and a slow cold start. See `coldstart-store-wrapper-init-capture-instructions-2026-06-24T16-30.md`
for the procedure.

Run conditions (maintainer fills):
- Debugger attached: no
- Outlook fully closed before run: <yes/no>
- Approximate total startup wall time observed: <seconds>

---

## [store-wrapper-init] lines (paste every line captured)

```
<paste each [store-wrapper-init] store=... totalMs=... threadId=... line here>
```

---

## [phase-net] lines (one per phase, in startup order)

```
[phase-net] phase=IntelConfig grossMs=<...> storeWrapperInitMs=<...> netMs=<...>
[phase-net] phase=OlObjects    grossMs=<...> storeWrapperInitMs=<...> netMs=<...>
[phase-net] phase=ToDo         grossMs=<...> storeWrapperInitMs=<...> netMs=<...>
[phase-net] phase=AutoFile     grossMs=<...> storeWrapperInitMs=<...> netMs=<...>
[phase-net] phase=Engines      grossMs=<...> storeWrapperInitMs=<...> netMs=<...>
[phase-net] phase=Events       grossMs=<...> storeWrapperInitMs=<...> netMs=<...>
```

---

## Maintainer interpretation (fill after capture)

- Which phase showed the largest `storeWrapperInitMs`? <phase>
- Did that phase's `netMs` stay small (store-init dominated the phase)? <yes/no>
- Did the dominant store appear as a single large-`totalMs` `[store-wrapper-init]` line? <store name + totalMs>
- Does the dominant phase shift relative to prior captures? <observation>

This placeholder is intentionally empty of measured values; it is filled only by the maintainer's
runtime capture and is not produced by CI or local toolchain runs.
