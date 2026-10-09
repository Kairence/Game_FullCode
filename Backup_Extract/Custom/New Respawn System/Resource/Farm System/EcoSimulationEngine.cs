using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Multis;
using Server.Mobiles; // VirtualCitizen
using Server.Misc;    // If VirtualCitizen is here

namespace Server.Misc
{
    public enum SimulationSystemType
    {
        None = 0,
        Farming,
        Fishing,
        Mining,
        Retail,
        Lumberjacking,
        Husbandry
    }

    public class EnvironmentAuraState
    {
        public bool HasWater;
        public bool HasHeat;
        public bool IsProtected;
        public bool IsPestControlled;
        public bool IsIndoor;
    }

    public static class EcoSimulationEngine
    {
        public static List<BaseAddon> ActiveSimulations = new List<BaseAddon>();

        public static void Register(BaseAddon addon)
        {
            if (addon != null && !ActiveSimulations.Contains(addon))
            {
                ActiveSimulations.Add(addon);
            }
        }

        public static void Unregister(BaseAddon addon)
        {
            if (addon != null && ActiveSimulations.Contains(addon))
            {
                ActiveSimulations.Remove(addon);
            }
        }

        public static void ProcessGlobalTick()
        {
            // Called by MasterTickEngine
            foreach (var sim in new List<BaseAddon>(ActiveSimulations))
            {
                if (sim == null || sim.Deleted)
                {
                    ActiveSimulations.Remove(sim);
                    continue;
                }

                if (sim is PrivateFarmAddon farm)
                {
                    bool needsLabor = false;
                    foreach (AddonComponent c in farm.Components)
                    {
                        if (c is FarmWaterBarrelComponent w)
                        {
                            w.Charges--;
                            if (w.Charges <= 0) needsLabor = true;
                        }
                        else if (c is FarmPestControlKegComponent p)
                        {
                            p.Charges--;
                            if (p.Charges <= 0) needsLabor = true;
                        }
                    }
                    
                    // Virtual Citizen will handle labor if needed
                }
            }
        }

        // ==============================================================================
        // [AI Virtual Citizen AI Worker logic]
        // ==============================================================================
        public static void ExecuteAIWork(Server.Misc.VirtualCitizen agent, SimulationSystemType jobType)
        {
            if (agent == null || agent.House == null) return;

            if (jobType == SimulationSystemType.Farming)
            {
                foreach (var sim in ActiveSimulations)
                {
                    if (sim is PrivateFarmAddon farm && farm.MobileOwner != null && farm.MobileOwner != null)
                    {
                        farm.JobBoardLaborerAssigned(agent);
                        return;
                    }
                }
            }
        }
    }
}
