using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Middleware;

[JsonSerializable(typeof(ProblemDetails))]
internal partial class ProblemDetailsJsonContext : JsonSerializerContext;
