using Daemon.ApplicationModels;

namespace Daemon.ApplicationServices;

public interface IConfigurationService
{
    Task<ApiSettings> GetConfigurations();
}
