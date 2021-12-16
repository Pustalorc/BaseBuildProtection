using Pustalorc.Plugins.BaseClustering;
using Rocket.API;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
#if DecayPatch
using Pustalorc.ImperialPlugins.Decay.API.Utilities;
using Pustalorc.ImperialPlugins.Decay;
using System.Linq;
#endif

namespace Pustalorc.Plugins.BaseBuildProtection
{
    public sealed class BaseBuildProtectionPlugin : RocketPlugin<BaseBuildProtectionConfiguration>
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
            var config = Configuration.Instance;
            if (!shouldAllow || config.BypassedIds.Contains(asset.id))
                return;

            var player = UnturnedPlayer.FromCSteamID((CSteamID)owner);

            if (player?.HasPermission(config.BypassPermission) == true)
                return;

            shouldAllow = CheckValidDeployPosAndOwner(point, owner, group);
        }

        private void OnBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point,
            ref float angleX, ref float angleY, ref float angleZ, ref ulong owner, ref ulong group,
            ref bool shouldAllow)
        {
            var config = Configuration.Instance;
            if (!shouldAllow || config.BypassedIds.Contains(asset.id))
                return;

            var player = UnturnedPlayer.FromCSteamID((CSteamID)owner);

            if (player?.HasPermission(config.BypassPermission) == true)
                return;

            shouldAllow = CheckValidDeployPosAndOwner(point, owner, group);
        }

        private bool CheckValidDeployPosAndOwner(Vector3 point, ulong owner, ulong group)
        {
            var clusteringPlugin = BaseClusteringPlugin.Instance;

            if (clusteringPlugin == null)
            {
                Logging.Write(this, "Base Clustering is not loaded. Will not perform any more code.");
                UnloadPlugin();
                return true;
            }

            var bClusterDirectory = clusteringPlugin.BaseClusterDirectory;

            if (bClusterDirectory == null)
            {
                Logging.Write(this,
                    "Clustering feature on BaseClustering plugin is not loaded. Will not perform any more code.");
                UnloadPlugin();
                return true;
            }

#if DecayPatch
            var bestClusters = bClusterDirectory.FindBestClusters(point).ToList();

            if (bestClusters.Count <= 0)
#else
            var bestCluster = bClusterDirectory.FindBestCluster(point);

            if (bestCluster == null)
#endif
                return true;

#if DecayPatch
            foreach (var bestCluster in bestClusters)
            {
#endif

                var commonOwner = bestCluster.CommonOwner;
                var commonGroup = bestCluster.CommonGroup;

#if DecayPatch
                var decayPlugin = AdvancedDecayPlugin.Instance;

                if (decayPlugin == null)
                    continue;

                var configurationUtility = decayPlugin.ConfigurationUtility;

                if (configurationUtility == null)
                    continue;

                var baseDecayConfig = decayPlugin.BaseDecayConfiguration.Instance;

                var baseTcs = bestCluster.Buildables.GetToolCupboards(configurationUtility, baseDecayConfig);

                if (baseTcs.Count <= 0)
                    continue;

                commonOwner = baseTcs.Where(l => l.Owner != CSteamID.Nil.m_SteamID).GroupBy(l => l.Owner)
                    .OrderByDescending(l => l.Count()).Select(g => g.Key).FirstOrDefault();
                commonGroup = baseTcs.Where(l => l.Group != CSteamID.Nil.m_SteamID).GroupBy(l => l.Group)
                    .OrderByDescending(l => l.Count()).Select(g => g.Key).FirstOrDefault();
#endif
                if (!(commonOwner == owner || group != CSteamID.Nil.m_SteamID && group == commonGroup))
                    return false;

#if DecayPatch
            }
#endif

            return true;
        }
    }
}