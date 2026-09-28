# User Story — quickfiler-date-time-format-missing-invariant-culture (Issue #742)

- Work Mode: full-bug
- Note: this document is a supporting narrative only. It does not carry acceptance criteria. The
  acceptance criteria for this issue live exclusively in spec.md, per this repository's full-bug work
  mode convention.

## Who is affected

An operator running the QuickFiler or EFC add-in on a Windows machine whose regional date or time format
uses a separator character other than the ones the original code assumed. This includes any locale whose
date separator is not a forward slash or whose time separator is not a colon.

## What breaks

When the operator's regional settings use a different separator, two kinds of output are affected:

- The session-metrics CSV file that QuickFiler and the EFC writer produce after a filing or move session
  is a machine-read artifact. Its date and time columns pick up the operator's regional separator instead
  of the fixed separator the file format expects, so a downstream process reading that CSV encounters an
  unexpected character in a field it treats as fixed-format.
- On-screen summary text and exception messages that include a sent date or sent time show the same
  locale-dependent separator instead of a consistent one, which is a cosmetic inconsistency rather than a
  data-corruption risk.

The numeric fields recorded alongside these date and time fields in the same output already avoid this
problem: they are rendered with a fixed, locale-independent format. The date and time fields were never
given the same protection, which is the gap this issue closes.

## What done looks like

After this fix, every date and time value written to the session-metrics CSV, and every date and time
value shown in the affected on-screen or exception summary text, renders with the same fixed separator
characters no matter what regional format the operator's machine is set to. An operator running on any
Windows regional setting sees and produces the same date and time formatting as an operator running on
the setting the code was originally written against. No behavior other than the separator character
changes: the same fields appear in the same order, and no fields are added or removed.
