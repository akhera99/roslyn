﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Roslyn.LanguageServer.Protocol;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Class representing the workspace diagnostic request parameters
///
/// See the <see href="https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/#workspaceDiagnosticParams">Language Server Protocol specification</see> for additional information.
/// </summary>
/// <remarks>
/// Note that the first literal send needs to be a <see cref="WorkspaceDiagnosticReport"/>
/// followed by n <see cref="WorkspaceDiagnosticReportPartialResult"/> literals.
/// </remarks>
internal class WorkspaceDiagnosticParams : IPartialResultParams<SumType<WorkspaceDiagnosticReport, WorkspaceDiagnosticReportPartialResult>>
{
    /// <summary>
    /// Gets or sets the value of the Progress instance.
    /// </summary>
    [JsonPropertyName(Methods.PartialResultTokenName)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IProgress<SumType<WorkspaceDiagnosticReport, WorkspaceDiagnosticReportPartialResult>>? PartialResultToken
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the identifier for which the client is requesting diagnostics for.
    /// </summary>
    [JsonPropertyName("identifier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Identifier
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the result id of a previous diagnostics response if provided.
    /// </summary>
    [JsonPropertyName("previousResultIds")]
    public PreviousResultId[] PreviousResultId
    {
        get;
        set;
    }
}