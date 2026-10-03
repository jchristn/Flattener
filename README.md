![alt tag](https://raw.githubusercontent.com/jchristn/Flattener/refs/heads/main/assets/icon.ico)

# Flattener

[![NuGet Version](https://img.shields.io/nuget/v/Flattener.svg?style=flat)](https://www.nuget.org/packages/Flattener/) [![NuGet](https://img.shields.io/nuget/dt/Flattener.svg)](https://www.nuget.org/packages/Flattener)

## Description

Flattener converts JSON or XML into flat key-value collections with dot notation.

## New in v1.1.1

- Dependency update: `System.Text.Json` 10.0.12
- No API or behavior changes

## New in v1.1.0

- `TryFlatten(input, out result, out error, includeNullItems)` on `JsonFlattener` and `XmlFlattener` reports malformed or null input instead of silently returning an empty collection
- `Flatten(input, includeNullItems, throwOnError)` overload surfaces the underlying `JsonException` / `XmlException`
- Existing `Flatten(input, includeNullItems)` behavior is unchanged

## Simple Examples

### JSON

```csharp
using System.Collections.Specialized;
using Flattener;

string json = @"{ ""person"": { ""name"": ""John"", ""age"": 30 } }";
NameValueCollection flattened = JsonFlattener.Flatten(json);

// By default, null values are excluded
// To include nulls:
NameValueCollection withNulls = JsonFlattener.Flatten(json, includeNullItems: true);

// Accessing values
// For a key with a single value:
string name = flattened.Get("person.name");       // Returns "John"
// For a key that might have multiple values:
string[] skills = flattened.GetValues("skills");  // Returns array of values or null

foreach (string key in flattened.AllKeys)
{
    string[] values = flattened.GetValues(key);
    foreach (string value in values)
    {
        Console.WriteLine($"{key} = {value ?? "null"}");
    }
}
```

### XML

```csharp
using System.Collections.Specialized;
using Flattener;

string xml = @"<User id=""123""><n>Alice</n><Skills><Skill>C#</Skill></Skills></User>";
NameValueCollection flattened = XmlFlattener.Flatten(xml);

// By default, empty elements are excluded
// To include empty elements:
NameValueCollection withEmpties = XmlFlattener.Flatten(xml, includeNullItems: true);

// Accessing values
// XML attributes are prefixed with @
string id = flattened.Get("User.@id");           // Returns "123"
string name = flattened.Get("n");                // Returns "Alice" (child elements are keyed relative to the root)

// For repeated elements (array-like):
string[] skills = flattened.GetValues("Skills.Skill");

foreach (string key in flattened.AllKeys)
{
    string[] values = flattened.GetValues(key);
    foreach (string value in values)
    {
        Console.WriteLine($"{key} = {value ?? "null"}");
    }
}
```

### Detecting malformed input

`Flatten(input)` returns an empty collection for malformed input, which looks the same as a valid document with no values. When the difference matters, for example to fail a request or record the error on a trace span, use `TryFlatten` or the `throwOnError` overload:

```csharp
using System;
using System.Collections.Specialized;
using Flattener;

if (JsonFlattener.TryFlatten(json, out NameValueCollection flattened, out Exception error))
{
    // flattened holds the key-value pairs (possibly zero for a valid empty document)
}
else
{
    // error is a JsonException (malformed input) or ArgumentNullException (null input);
    // flattened is an empty collection, never null
    Console.WriteLine($"Invalid JSON: {error.Message}");
}

// Or let the parse exception propagate:
NameValueCollection strict = XmlFlattener.Flatten(xml, includeNullItems: false, throwOnError: true);
```

`TryFlatten` doesn't throw for null or malformed input. `Flatten(..., throwOnError: true)` throws `System.Text.Json.JsonException` (JSON) or `System.Xml.XmlException` (XML) for malformed input, and `ArgumentNullException` for null input.

## Version history

Refer to CHANGELOG.md.