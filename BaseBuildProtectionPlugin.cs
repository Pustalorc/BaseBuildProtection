using Pustalorc.Plugins.BaseClustering;
using Rocket.Core.Plugins;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
#if DecayPatch
using System.Collections.Generic;
using Pustalorc.ImperialPlugins.Decay;
using System.Linq;
#endif

namespace Pustalorc.Plugins.BaseBuildProtection
{
    public sealed class BaseBuildProtectionPlugin : RocketPlugin
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
            if (!shouldAllow)
                return;

            shouldAllow = CheckValidDeployPosAndOwner(point, owner, group);
        }

        private void OnBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point,
            ref float angleX, ref float angleY, ref float angleZ, ref ulong owner, ref ulong group,
            ref bool shouldAllow)
        {
            if (!shouldAllow)
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
                Logging.Write(this, "Clustering feature on BaseClustering plugin is not loaded. Will not perform any more code.");
                UnloadPlugin();
                return true;
            }

            var bestCluster = bClusterDirectory.FindBestCluster(point);

            if (bestCluster == null) return true;

            var commonOwner = bestCluster.CommonOwner;
            var commonGroup = bestCluster.CommonGroup;

#if DecayPatch
            var decayPlugin = AdvancedDecayPlugin.Instance;

            // ReSharper disable once InvertIf
            // A dumb invert. We always return same thing.
            if (decayPlugin != null)
            {
                var baseDecayConfig = decayPlugin.BaseDecayConfiguration.Instance;

                var allTcs = new HashSet<ushort>(baseDecayConfig.CustomBaseSettings.SelectMany(k => k.ToolCupboardItemIds).Concat(baseDecayConfig.ToolCupboardItemIds));

                var baseTcs = bestCluster.Buildables.Where(l => allTcs.Contains(l.AssetId)).ToList();

                // ReSharper disable once InvertIf
                // A dumb invert. We always return same thing.
                if (baseTcs.Count > 0)
                {
                    commonOwner = baseTcs.Where(l => l.Owner != CSteamID.Nil.m_SteamID).GroupBy(l => l.Owner)
                        .OrderByDescending(l => l.Count()).Select(g => g.Key).FirstOrDefault();
                    commonGroup = baseTcs.Where(l => l.Group != CSteamID.Nil.m_SteamID).GroupBy(l => l.Group)
                        .OrderByDescending(l => l.Count()).Select(g => g.Key).FirstOrDefault();
                }
            }
#endif

            return commonOwner == owner || group != CSteamID.Nil.m_SteamID && group == commonGroup;
        }
    }
}