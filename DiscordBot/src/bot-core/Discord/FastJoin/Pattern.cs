using System.Globalization;
using System.Net;
using System.Text;
using System.Web;

namespace Manito.Discord.FastJoin
{
    public static class FastJoinPattern
    {
        public readonly static ulong BermudaId = 719890;
        public readonly static string Pattern = $"steam://rungameid/{BermudaId}//";
        private static string Join(string addr) => $"{Pattern}" + HttpUtility.UrlEncode($"+connect {addr}");
        public static string Join(DnsEndPoint addr) => Join($"{addr.Host}:{addr.Port}");
        public static string Join(IPEndPoint addr) => Join($"{addr.Address}:{addr.Port}");

    }
}