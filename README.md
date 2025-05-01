![alt tag](https://raw.githubusercontent.com/jchristn/flattener/main/assets/icon.ico)

# Flattener

[![NuGet Version](https://img.shields.io/nuget/v/Flattener.svg?style=flat)](https://www.nuget.org/packages/Flattener/) [![NuGet](https://img.shields.io/nuget/dt/Flattener.svg)](https://www.nuget.org/packages/Flattener)

## Description

Flattener converts nested JSON or XML into flat key-value collections with dot notation.

## New in v1.0.0

- Initial release
- JSON and XML flattening support
- Optional inclusion of null values

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
string name = flattened.Get("person.name");      // Returns "John"
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
using View.Chunking;

string xml = @"<User id=""123""><n>Alice</n><Skills><Skill>C#</Skill></Skills></User>";
NameValueCollection flattened = XmlFlattener.Flatten(xml);

// By default, empty elements are excluded
// To include empty elements:
NameValueCollection withEmpties = XmlFlattener.Flatten(xml, includeNullItems: true);

// Accessing values
// XML attributes are prefixed with @
string id = flattened.Get("User.@id");           // Returns "123"
string name = flattened.Get("User.n");           // Returns "Alice"
// For repeated elements (array-like):
string[] skills = flattened.GetValues("User.Skills.Skill");

foreach (string key in flattened.AllKeys)
{
    string[] values = flattened.GetValues(key);
    foreach (string value in values)
    {
        Console.WriteLine($"{key} = {value ?? "null"}");
    }
}
```

## Version history

Refer to CHANGELOG.md.