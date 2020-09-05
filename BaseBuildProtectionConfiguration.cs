using Rocket.API;

namespace Pustalorc.Plugins.BaseBuildProtection
{
    public class BaseBuildProtectionConfiguration : IRocketPluginConfiguration
    {
        public float ExtraProtectionRadius;

        public void LoadDefaults()
        {
            ExtraProtectionRadius = 0f;
        }
    }
}