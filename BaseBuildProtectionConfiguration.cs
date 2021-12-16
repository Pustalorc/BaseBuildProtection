using System.Collections.Generic;
using JetBrains.Annotations;
using Rocket.API;

namespace Pustalorc.Plugins.BaseBuildProtection
{
    [UsedImplicitly]
    public sealed class BaseBuildProtectionConfiguration : IRocketPluginConfiguration
    {
        public string BypassPermission { get; set; }
        public HashSet<ushort> BypassedIds { get; set; }

        public BaseBuildProtectionConfiguration()
        {
            BypassPermission = "";
            BypassedIds = new HashSet<ushort>();
        }

        public void LoadDefaults()
        {
            BypassPermission = "BaseBuildProtection.Bypass";
            BypassedIds = new HashSet<ushort> { 0 };
        }
    }
}