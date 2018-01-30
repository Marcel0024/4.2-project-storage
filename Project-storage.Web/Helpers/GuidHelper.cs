using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Project_storage.Web.Helpers
{
    public class GuidHelper
    {
        public static Guid GenerateGuid()
        {
            // Get 16 cryptographically random bytes
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] data = new byte[16];
            rng.GetBytes(data);

            // Mark it as a version 4 GUID
            data[7] = (byte)((data[7] | (byte)0x40) & (byte)0x4f);
            data[8] = (byte)((data[8] | (byte)0x80) & (byte)0xbf);

            return new Guid(data);
        }

        public static string ToString(Guid guid)
        {
            return guid.ToString("N");
        }
    }
}
