# Change Log

## Current Version

v1.1.1

- Updated `System.Text.Json` from 10.0.11 to 10.0.12
- Test dependencies updated: Touchstone 0.1.12 -> 0.2.0, Microsoft.NET.Test.Sdk 18.9.0 -> 18.10.1, NUnit 4.6.1 -> 5.0.0, NUnit3TestAdapter 6.2.0 -> 6.3.0
- No API or behavior changes

## Previous Versions

v1.1.0

- Added `JsonFlattener.TryFlatten` and `XmlFlattener.TryFlatten`, which return `false` and the underlying exception for malformed or null input, so callers can tell malformed input apart from a valid document with no values
- Added `Flatten(input, includeNullItems, throwOnError)` overloads that let the parse exception propagate when `throwOnError` is `true`
- `Flatten(input, includeNullItems)` behavior is unchanged (malformed input still returns an empty collection)
- README: corrected the XML example's namespace and key paths (child elements are keyed relative to the root element)

v1.0.0

- Initial release