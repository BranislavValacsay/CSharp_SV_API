using LARS.Interface;
using Microsoft.EntityFrameworkCore;

namespace LARS.Data
{
    public class Stepper : IStepperService
    {
        private readonly LarsContext _context;

        public Stepper(LarsContext context)
        {
            _context = context;
        }

        public async Task<bool> Lifecycle(string guid, string operation)
        {

            var serverRequest = await _context.RequestServers.FirstOrDefaultAsync(x => x.Guid == guid);
            if (serverRequest == null) { return false; }
            switch (operation)
            {
                case "Approval":

                    break;

                case "vServerCreationStart":

                    break;
                case "vServerCreationFinish":

                    break;
                case "vServerCreationFail":

                    break;
                case "vServerCreationRestart":

                    break;


                case "DscCreationStart":

                    break;
                case "DscCreationFinish":

                    break;
                case "DscCreationFail":

                    break;
                case "DscCreationRestart":

                    break;

                case "DscInjectionStart":

                    break;
                case "DscInjectionFinish":

                    break;
                case "DscInjectionFail":

                    break;
                case "DscInjectionRestart":

                    break;

                case "SystemDiskRenameStart":

                    break;
                case "SystemDiskRenameFinish":

                    break;
                case "SystemDiskRenameFail":

                    break;
                case "SystemDiskRenameRestart":

                    break;

                case "StartVirtualServerStart":
                    break;
                case "StartVirtualServerFinish":
                    break;
                case "StartVirtualServerError":
                    break;
                case "StartVirtualServerRestart":
                    break;

                case "RequestComplete":
                    break;

                default:
                    break;
            }

            _context.Entry(serverRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
