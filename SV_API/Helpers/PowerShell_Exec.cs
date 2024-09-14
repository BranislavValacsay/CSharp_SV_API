using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace sp_api.Helpers
{
    public class PowerShell_Exec
    {
        public async Task<string> StartScript(string script) //was string
        {

                string PowerShellDirectory = $"D:\\Repos\\PowerShell\\";

                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = PowerShellDirectory + script;
                process.StartInfo.CreateNoWindow = true;               
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();

                string output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();
                process.Close();

                return output;

        }

    }
}
