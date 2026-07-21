# SvgImageSelector.ImagePath Judgment Call Decision

Timestamp: 2026-07-19T02-40

## Context

`SVGControl/SvgImageSelector.cs`'s public `ImagePath` property (`get`/`set` accessor pair) is the
single most consequential judgment call in this feature (per plan §"Scope Invariants" and
research). The property is:

```csharp
public string ImagePath
{
    get
    {
        if (_absoluteImagePath == null)
        {
            return "(none)";
        }
        else
        {
            return _relativeImagePath;
        }
    }
    set
    {
        // entire body commented out — see file for the full commented block
    }
}
```

## The dead-setter nuance

The `set` accessor's entire body is commented out (a pre-existing condition, not introduced by
this feature). This means:

- `_relativeImagePath` is **never assigned** anywhere in the class (confirmed by the pre-existing
  `CS0649` compiler warning — "Field 'SvgImageSelector._relativeImagePath' is never assigned to,
  and will always have its default value null" — which remains present, unrelated to nullable
  reference types, both before and after this feature's changes).
- `_absoluteImagePath` is likewise never assigned, so the `get` accessor's
  `if (_absoluteImagePath == null)` branch is **always taken today** (in the current, unmodified
  runtime behavior), meaning the `else` branch returning `_relativeImagePath` is currently
  unreachable in practice — but it is still live code that must type-check under the pragma, and
  a genuine consumer could reach it in a future world where the setter is un-commented and starts
  assigning both fields.

Once `#nullable enable` is applied, the `else` branch's `return _relativeImagePath;` raises CS8603
("possible null reference return") because `_relativeImagePath` is `string?` and the method's
return type `ImagePath { get; }` is non-nullable `string`.

## Rejected alternative: `?? "(none)"` fallback

A `return _relativeImagePath ?? "(none)";` fallback was considered and **rejected**. This would
change observable behavior on the `else` branch: today, if `_absoluteImagePath` is non-null (a
future state reachable once the setter is un-commented) but `_relativeImagePath` is null (e.g., a
partial/inconsistent assignment), the getter currently returns `null` (implicitly, before nullable
was introduced) rather than the literal string `"(none)"`. Introducing `?? "(none)"` here would
silently substitute a different return value for callers on that path — a genuine, if narrow,
behavior change that this annotation-only feature (AC3) must not introduce. The `"(none)"` literal
already has a specific meaning in this class (returned when `_absoluteImagePath == null`); reusing
it for a different, distinct null-state (`_relativeImagePath == null` while
`_absoluteImagePath != null`) would conflate two different conditions under one sentinel value.

## Applied resolution: null-forgiving `_relativeImagePath!`

The `else` branch now reads:

```csharp
else
{
    // The `set` accessor below is currently entirely commented out (a functional no-op),
    // so _relativeImagePath is never actually assigned by this class today. The
    // null-forgiving operator preserves the pre-existing behavior of returning whatever
    // _relativeImagePath currently holds (including null) rather than introducing a
    // `?? "(none)"` or other fallback, which would change the observable return value on
    // this path.
    return _relativeImagePath!;
}
```

This is a compile-time-only annotation (the null-forgiving operator `!` has no runtime effect); it
preserves the exact pre-existing return value on this path — including `null`, if that state is
ever reached — with no new fallback expression and no new guard/throw statement introduced.

## Exact location applied

`SVGControl/SvgImageSelector.cs:88` — `return _relativeImagePath!;` inside the `ImagePath`
property's `get` accessor, `else` branch (line number as of the final `csharpier`-formatted
state of the file).
