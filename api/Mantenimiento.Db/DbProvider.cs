using Cbsdg.Data.Core;

namespace Mantenimiento.Db;

public partial class DbProvider(string conStr) : ProviderBase(conStr)
{
}
