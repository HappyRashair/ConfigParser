
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

Console.Write("Provide target app (CND/CND-API/CS/CS-API/MW/SS/DESKTOP): ");
//var targetEnvInput = Console.ReadLine() ?? "";
//var targetEnv = (targetEnvInput == "" ? "DEV" : targetEnvInput).ToUpper();
Console.WriteLine("---");


var configProcessor = new ConfigProcessor(verboseLogging: false);
try
{
    configProcessor.Run();
}
catch (Exception ex)
{
    CWrapper.WriteRed(ex.ToString());
}

