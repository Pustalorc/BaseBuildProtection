
using Pustalorc.Plugins.BaseClustering;
using Pustalorc.Plugins.BaseClustering.API.Statics;
using Rocket.API;
using Rocket.Core.Plugins;
using SDG.Unturned;
using Steamworks;
using System.Linq;
using UnityEngine;
#if DecayPatch
using Pustalorc.ImperialPlugins.Decay;
#endif

namespace Pustalorc.Plugins.BaseBuildProtection
{
    public class BaseBuildProtectionPlugin : RocketPlugin<BaseBuildProtectionConfiguration>
    {
        protected override void Load()
        {
            BarricadeManager.onDeployBarricadeRequested += OnBarricadeDeploy;
            StructureManager.onDeployStructureRequested += OnStructureDeploy;

            Logging.PluginLoaded(this);
        }

        protected override void Unload()
        {
            BarricadeManager.onDeployBarricadeRequested -= OnBarricadeDeploy;
            StructureManager.onDeployStructureRequested -= OnStructureDeploy;

            Logging.PluginUnloaded(this);
        }

        private void OnStructureDeploy(Structure structure, ItemStructureAsset asset, ref Vector3 point,
            ref float angleX, ref float angleY, ref float angleZ, ref ulong owner, ref ulong group,
            ref bool shouldAllow)
        {
            shouldAllow = CheckValidDeployPosAndOwner(point, owner, group);
        }

        private void OnBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point,
            ref float angleX, ref float angleY, ref float angleZ, ref ulong owner, ref ulong group,
            ref bool shouldAllow)
        {
            shouldAllow = CheckValidDeployPosAndOwner(point, owner, group);
        }

        private bool CheckValidDeployPosAndOwner(Vector3 point, ulong owner, ulong group)
        {
            if (BaseClusteringPlugin.Instance == null)
            {
                Logging.Write(this, "Base Clustering is not loaded. Will not perform any more code.");
                return true;
            }

            var bestCluster = BaseClusteringPlugin.Instance.Clusters.FindBestCluster(point, Configuration.Instance.ExtraProtectionRadius);

            if (bestCluster == null) return true;

            var commonOwner = bestCluster.CommonOwner;
            var commonGroup = bestCluster.CommonGroup;

#if DecayPatch
            var config = AdvancedDecayPlugin.Instance.Configuration.Instance.Decays.LastOrDefault(k =>
                new RocketPlayer(commonOwner.ToString()).HasPermission(k.Permission));
            var tcList = AdvancedDecayPlugin.Instance.defaultRustConfiguration.Instance.ToolCupboardItemIds;

            if (config != null)
                tcList.AddRange(config.RustDecaySettings.ToolCupboardIds);

            var baseTcs = bestCluster.Buildables.Where(l => tcList.Contains(l.AssetId));

            var changedOwner = false;
            do
            {
                if (!baseTcs.Any()) break;

                var newOwner = baseTcs.Where(l => l.Owner != CSteamID.Nil.m_SteamID).GroupBy(l => l.Owner)
                    .OrderByDescending(l => l.Count()).Select(g => g.Key).FirstOrDefault();
                changedOwner = newOwner != commonOwner;
                commonOwner = newOwner;

                commonGroup = baseTcs.Where(l => l.Group != CSteamID.Nil.m_SteamID).GroupBy(l => l.Group)
                    .OrderByDescending(l => l.Count()).Select(g => g.Key).FirstOrDefault();
                config = AdvancedDecayPlugin.Instance.Configuration.Instance.Decays.LastOrDefault(k =>
                    new RocketPlayer(commonOwner.ToString()).HasPermission(k.Permission));

                if (config != null)
                    tcList.AddRange(config.RustDecaySettings.ToolCupboardIds);

                baseTcs = bestCluster.Buildables.Where(l => tcList.Contains(l.AssetId));
            } while (changedOwner);
#endif

            return commonOwner == owner || (group != CSteamID.Nil.m_SteamID && group == commonGroup);
        }
    }
}