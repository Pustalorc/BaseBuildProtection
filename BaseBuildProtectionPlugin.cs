using Pustalorc.Plugins.BaseClustering;
using Pustalorc.Plugins.BaseClustering.API.Statics;
using Rocket.Core.Plugins;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

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
            if (BaseClusteringPlugin.Instance == null)
            {
                Logging.Write(this, "Base Clustering is not loaded. Will not perform any more code.");
                return;
            }

            var bestCluster =
                BaseClusteringPlugin.Instance.Clusters.FindBestCluster(point,
                    Configuration.Instance.ExtraProtectionRadius);

            if (bestCluster == null) return;

            shouldAllow = bestCluster.CommonOwner == owner ||
                          group != CSteamID.Nil.m_SteamID && group == bestCluster.CommonGroup;
        }

        private void OnBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point,
            ref float angleX, ref float angleY, ref float angleZ, ref ulong owner, ref ulong group,
            ref bool shouldAllow)
        {
            if (BaseClusteringPlugin.Instance == null)
            {
                Logging.Write(this, "Base Clustering is not loaded. Will not perform any more code.");
                return;
            }

            var bestCluster =
                BaseClusteringPlugin.Instance.Clusters.FindBestCluster(point,
                    Configuration.Instance.ExtraProtectionRadius);

            if (bestCluster == null) return;

            shouldAllow = bestCluster.CommonOwner == owner ||
                          group != CSteamID.Nil.m_SteamID && group == bestCluster.CommonGroup;
        }
    }
}