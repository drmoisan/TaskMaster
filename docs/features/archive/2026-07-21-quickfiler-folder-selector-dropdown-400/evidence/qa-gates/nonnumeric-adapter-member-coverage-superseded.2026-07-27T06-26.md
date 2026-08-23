# P9-T41 and P9-T42 Supersession Record

Timestamp: 2026-07-27T06:26:00-04:00

The initial P9-T41 CSharpier and P9-T42 analyzer-build evidence were completed before a coverage-risk review identified that the five-argument production navigation wrapper created a capturing lambda. The prior Cobertura accounting includes compiler-generated display-class/lambda members; the existing null-guard tests return before invoking that closure, which can leave a changed generated member uncovered.

The bounded P9-T40 correction replaces the capturing lambda with the named `NavigationBinder` delegate and `BindProductionNavigation` method group while preserving the existing five-argument runtime wrapper path. This changes the P9-T41 scoped source state. Therefore the initial P9-T41 and P9-T42 evidence remains historical only and is superseded for successor final QA. P9-T41 and P9-T42 must be rerun after the P9-T40 correction; P9-T43 and later tasks must not use the superseded state.
