using System.Security.Cryptography;
using System.Text;

namespace Forge.Domain.Constants;

public static class OfficialExerciseIds
{
    private const string CatalogNamespace = "Forge.OfficialExercises.v1:";

    public static Guid FromCatalogKey(string key)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(CatalogNamespace + key));

        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x30);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

        return new Guid(bytes);
    }
}
