Timestamp: 2026-08-11T13-46
Inputs: P1-T2 rebuild evidence; P1-T3 through P1-T12 remeasurement and conversion evidence.
Determination: AC7 has corrected-arithmetic TaskMaster input evidence; it does not authorize a policy-threshold choice or reduction.

- P1-T2 rebuilt the solution before the measurements.
- P1-T3, P1-T6, and P1-T9 each recorded the valid zero-failure outcome class: 6,435 passed, 0 failed, and exit code 0.
- P1-T4/P1-T7/P1-T10 converted the raw Cobertura XML using `ConvertTo-KoverageCoberturaXml`.
- P1-T5/P1-T8/P1-T11 recorded corrected root and per-package observations.
- P1-T12 recorded line-rate reproducibility of 0.0176 percentage points and a zero `lines-valid` spread.
- All figures are post-#441/#478/#457 corrected-arithmetic observations.
- Measurement cannot silently choose or lower a policy threshold.
