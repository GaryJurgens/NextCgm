using Docker.DotNet.Models;
using Org.BouncyCastle.Pqc.Crypto.Saber;

namespace NextCgm.Helpers.Utils
{
    public class DockerOptions
    {

        public string ImageName { get; set; } = string.Empty;
        public string Tag { get; set; } = "latest";
        public string ContainerName { get; set; } = string.Empty;
        public int ContainerPort { get; set; } = 80;

        public string EndDomain { get; set; } = string.Empty;

    }
}
