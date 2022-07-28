
using ConfigParser;

Console.WriteLine("Please create following files with data:");
Console.WriteLine(FilePath.ResultServerConfig);
Console.WriteLine(FilePath.InputServerConfig);
Console.WriteLine($"Your result will be saved in {FilePath.ResultServerConfig}");
Console.WriteLine("---------------------------------------------------------\n");

Console.Write("Provide target env (DEV/UAT/LIVE): ");
//var targetEnvInput = Console.ReadLine() ?? "";
//var targetEnv = (targetEnvInput == "" ? "DEV" : targetEnvInput).ToUpper();
Console.WriteLine("---");

try
{
    ConfigProcessor.Run();
}
catch(Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(ex);
    Console.ResetColor();
}

