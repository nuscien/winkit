# [Trivial.Web](https://trivial.kingcean.net/web/controller)

ASP.NET Core extensions.

## Import

Add following namespace to your code file to use.

```csharp
using Trivial.Web;
```

## Nullable reference types

Nullable reference types are enabled for both target frameworks. Reference-type parameters and results that may be absent are annotated as nullable. This includes missing request values and bodies, invalid or absent tokens, file results when a source is missing, and action results for ignored exceptions. Required request/response objects and successful result values remain non-nullable where the implementation requires them.

## Request

Some extension methods to get the field from HTTP request.

## Action result

Some extension methods to convert some modes to `ActionResult`.
