using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Patch;

namespace Poll.N.Quiz.Settings.Synchronizer;

internal static class JsonPatchExtensions
{
    internal static string ApplyPatchTo(this string jsonPatchString, string jsonString)
    {
        var jsonPatch = JsonSerializer.Deserialize<JsonPatch>(jsonPatchString);

        if(jsonPatch is null)
            throw new InvalidOperationException("Invalid JsonPatch for event"); //TODO: log this properly

        var jsonNode = JsonNode.Parse(jsonString);

        if(jsonNode is null)
            throw new InvalidOperationException("Invalid JsonNode for event"); //TODO: log this properly

        return jsonPatch.ApplyTo(jsonNode).ToJsonString();
    }

    internal static JsonNode ApplyPatches(this JsonNode jsonNode, params IEnumerable<JsonPatch> jsonPatches)
    {
        var resultSettingsJsonNode = jsonNode;

        foreach (var jsonPatch in jsonPatches)
        {
            resultSettingsJsonNode = jsonPatch.ApplyTo(resultSettingsJsonNode);
        }

        return resultSettingsJsonNode;
    }

    private static JsonNode ApplyTo(this JsonPatch jsonPatch, JsonNode jsonNode)
    {
        var patchResult = jsonPatch.Apply(jsonNode);

        if(!patchResult.IsSuccess || patchResult.Result is null)
            throw new InvalidOperationException("Failed to apply patch to json node:" + patchResult.Error); //TODO: log this properly

        return patchResult.Result;
    }
}
