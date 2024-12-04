﻿using System.Text.Json.Serialization;

namespace StdioBridge.Api.Core;

public class ResponseModel
{
    [JsonPropertyName("id")] public required Guid Id { get; init; }
    [JsonPropertyName("code")] public required int Code { get; init; }
    [JsonPropertyName("data")] public object? Data { get; init; }
}