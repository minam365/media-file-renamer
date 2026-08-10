using Inamsoft.MediaFileRenamer.Services.FileRenamers.Rules;

namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;

public sealed class RenamingPipeline
{
    private readonly IReadOnlyList<IRenamingRule> _rules;

    public RenamingPipeline(IEnumerable<IRenamingRule> rules)
    {
        _rules = rules.ToList();
    }

    public string Execute(IRenamingContext context)
    {
        string name = context.OriginalName;

        foreach (var rule in _rules)
            name = rule.Apply(context, name);

        return name + context.File.Extension;
    }
}
