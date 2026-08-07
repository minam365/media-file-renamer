namespace Inamsoft.MediaFileRenamer.Services.FileRenamers.Rules;

public interface IRenamingRule
{
    string Apply(IRenamingContext context, string currentName);
}
