
using ConfigParser;

Console.WriteLine("Please create following files with data:");
Console.WriteLine(FilePath.ResultServerConfig);
Console.WriteLine(FilePath.InputServerConfig);
Console.WriteLine($"Your result will be saved in {FilePath.ResultServerConfig}");
Console.WriteLine("---------------------------------------------------------\n");

Console.Write("Provide target env (DEV/UAT/LIVE): ");
var targetEnvInput = Console.ReadLine() ?? "";
var targetEnv = (targetEnvInput == "" ? "DEV" : targetEnvInput).ToUpper();
Console.WriteLine("---");

Console.Write("Provide target app (CND/CND-API/CS/CS-API/MW/SS/DESKTOP): ");
var targetAppInput = Console.ReadLine() ?? "";
var targetApp = (targetAppInput == "" ? "CND-API" : targetAppInput).ToUpper();
Console.WriteLine("---");


Console.Write("Verbose logging (y/n): ");
var verboseLoggingInput = Console.ReadLine() ?? "";
var verboseLogging = verboseLoggingInput == "y";
Console.WriteLine("---");

var configProcessor = new ConfigProcessor(targetEnv, targetApp, verboseLogging);
try
{
    configProcessor.Run();
}
catch (Exception ex)
{
    CWrapper.WriteRed(ex.ToString());
}

