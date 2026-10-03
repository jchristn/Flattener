# Change Log

## Current Version

v1.1.0

- Added `JsonFlattener.TryFlatten` and `XmlFlattener.TryFlatten`, which return `false` and the underlying exception for malformed or null input, so callers can tell malformed input apart from a valid document with no values
- Added `Flatten(input, includeNullItems, throwOnError)` overloads that let the parse exception propagate when `throwOnError` is `true`
- `Flatten(input, includeNullItems)` behavior is unchanged (malformed input still returns an empty collection)
- README: corrected the XML example's namespace and key paths (child elements are keyed relative to the root element)

## Previous Versions

v1.0.0

- Initial release