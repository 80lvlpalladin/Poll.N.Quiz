// ReSharper disable once CheckNamespace
namespace Poll.N.Quiz.Settings.Web.Models;


public sealed record Environment(string Name);

public sealed record Service(string Name, IEnumerable<Environment> Environments);
