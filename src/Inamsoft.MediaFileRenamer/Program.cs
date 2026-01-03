using Inamsoft.MediaFileRenamer;
using Inamsoft.MediaFileRenamer.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

//var sourceFolderPath = args.Length > 0 ? args[0] : @"J:\Public\Kaan and Varunika - 17.08.2024\Pana FZ45";
//var sourceFolderPath = args.Length > 0 ? args[0] : @"J:\Public\Kaan's Standesamt\DCIM\100_PANA";
var sourceFolderPath = args.Length > 0 ? args[0] : @"\\INAMNAS3\Family Memories\Emin's Camera";
var sourceFilePattern = args.Length > 1 ? args[1] : "*.*";
var targetFolderPath = args.Length > 2 ? args[2] : @"J:\-RENAMED-\Emin's Camera";

//var helper = new MediaFileHelper();

//AnsiConsole.MarkupLine("[yellow]Starting file copy operation...[/]");
//AnsiConsole.WriteLine();
////AnsiConsole.Status()
////    .Start($"Renaming files and copying into ...'{targetFolderPath}'", ctx =>
////    {
////        var result = helper.RichCopyFiles(sourceFolderPath, targetFolderPath, sourceFilePattern, makeUniqueNames: true);
////    });

//var result = helper.RichCopyFiles(sourceFolderPath, targetFolderPath, sourceFilePattern, makeUniqueNames: true);

//AnsiConsole.WriteLine();
//AnsiConsole.MarkupLine($"[green]File copy operation completed.[/]");
//AnsiConsole.WriteLine();

var app = new CommandApp();
app.Configure(config =>
{
    config.SetApplicationName("Inamsoft Media File Renamer");
    config.AddCommand<CopyFilesCommand>("copy")
        .WithDescription("Copies media files from the source folder to the target folder with options for file patterns and unique naming.");
    config.AddCommand<MoveFilesCommand>("move")
        .WithDescription("Moves media files from the source folder to the target folder with options for file patterns and unique naming.");
});

return app.Run(args);