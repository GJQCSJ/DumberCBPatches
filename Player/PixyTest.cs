using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using DumberCBPatches;
using MBMModsServices;
using MBMScripts;

[BepInPlugin("com.SoapBoxHero.HealthPixyPlus", "HealthPixyPlus", "2.0.0")]
[BepInDependency("com.Memacile.SoapBoxHero.MBMModsServices", BepInDependency.DependencyFlags.HardDependency)]
public class HealthPixyPlusPlugin : BaseUnityPlugin
{
    public const string AUTHOR = "SoapBoxHero";

    public const string GUID = "com.SoapBoxHero.HealthPixyPlus";

    public static ManualLogSource? log;

    public readonly ConfigEntry<bool> AutoRestFromBreedingRoomEnabled;

    public readonly ConfigEntry<bool> AutoRestEnabled;

    public static IDictionary<int, (int roomid, int seat)> restingReturnOverrides = new Dictionary<int, (int, int)>();

    public static IDictionary<int, (int roomid, int seat)> restingReturnBlocks = new Dictionary<int, (int, int)>();

    public static IDictionary<int, (int roomid, int seat)> reservedRooms = new Dictionary<int, (int, int)>();

    public static IDictionary<int, (float health, int training, int traininglevel)> stolenStats = new Dictionary<int, (float, int, int)>();

    public static IDictionary<int, bool> FemalePreviousPixyHealth = new Dictionary<int, bool>();

    public static IList<EState> PreReadyToRestStates = new List<EState>
    {
        EState.Birth,
        EState.BirthDrained,
        EState.MilkStart
    };

    public static IList<EState> ReadyToRestStates = new List<EState>
    {
        EState.AfterBirth,
        EState.AfterBirthDrained,
        EState.MilkIdle
    };

    public static bool PreviousFloraPixyHealth = false;

    public IList<Action<Female>> EnabledActions = new List<Action<Female>>();

    private static AccessTools.FieldRef<PlayData, int> m_PixyCountRef = AccessTools.FieldRefAccess<PlayData, int>("m_PixyCount");

    private static readonly AccessTools.FieldRef<Room, int> m_AllocatableSeatCountRef = AccessTools.FieldRefAccess<Room, int>("m_AllocatableSeatCount");

    private static readonly FastInvokeHandler GetEmptySeatInRoomRef = MethodInvoker.GetHandler(AccessTools.Method(typeof(PlayData), "GetEmptySeatInRoom"));

    public static IDictionary<int, Female> Females => CharacterAccessTool.Females;

    public static PlayData? PD => ToolsPlugin.PD;

    public static (int, int) NoReturnTuple => (-1, -1);

    [HarmonyPatch(typeof(Unit), "IsDragging", MethodType.Setter)]
    [HarmonyPostfix]
    public static void OnDragUnit(Unit __instance)
    {
        if (__instance != null && __instance.IsDragging && __instance.Sector == ESector.Female)
        {
            ClearUnitState(__instance.UnitId);
        }
    }

    [HarmonyPatch(typeof(GameManager), "Load")]
    [HarmonyPostfix]
    public static void BeforeLoad()
    {
        ClearState();
    }

    [HarmonyPatch(typeof(GameManager), "Save")]
    [HarmonyPrefix]
    public static void BeforeSave()
    {
        ResetState();
    }

    public HealthPixyPlusPlugin()
    {
        log = base.Logger;
        AutoRestFromBreedingRoomEnabled = base.Config.Bind(new ConfigInfo<bool>
        {
            Section = "AutoRestFromBreedingRoom",
            Name = "Enabled",
            Description = "If enabled, forces low health event to trick pixies into moving the girl to a cage.",
            DefaultValue = true
        });
        AutoRestFromBreedingRoomEnabled.SettingChanged += Enabled_Changed;
        AutoRestEnabled = base.Config.Bind(new ConfigInfo<bool>
        {
            Section = "AutoRest",
            Name = "Enabled",
            Description = "If enabled, moves girls to a cages after milking and birth.",
            DefaultValue = true
        });
        AutoRestEnabled.SettingChanged += Enabled_Changed;
    }

    private void Enabled_Changed(object? sender, EventArgs e)
    {
        ConfigureActions();
    }

    public void ConfigureActions()
    {
        if (AutoRestFromBreedingRoomEnabled.Value)
        {
            EnabledActions.Add(RestForcibly);
            EnabledActions.Add(UndoForcedHealthReduction);
            log?.LogMessage("Enabled AutoRestFromBreedingRoom action.");
        }
        else
        {
            EnabledActions.Remove(RestForcibly);
            EnabledActions.Remove(UndoForcedHealthReduction);
            log?.LogMessage("Disabled AutoRestFromBreedingRoom action.");
        }
        if (AutoRestEnabled.Value)
        {
            EnabledActions.Add(PreRestIfAvailable);
            EnabledActions.Add(RestIfAvailable);
            EnabledActions.Add(UndoForcedRestAtIdle);
            log?.LogMessage("Enabled AutoRestEnabled action.");
        }
        else
        {
            EnabledActions.Remove(PreRestIfAvailable);
            EnabledActions.Remove(RestIfAvailable);
            EnabledActions.Remove(UndoForcedRestAtIdle);
            log?.LogMessage("Disabled AutoRestFromBreedingRoom action.");
        }
        if (AutoRestFromBreedingRoomEnabled.Value || AutoRestEnabled.Value)
        {
            if (!EnabledActions.Contains(ClearReservedRoom))
            {
                EnabledActions.Add(ClearReservedRoom);
            }
        }
        else if (!AutoRestFromBreedingRoomEnabled.Value && !AutoRestEnabled.Value && EnabledActions.Contains(ClearReservedRoom))
        {
            EnabledActions.Remove(ClearReservedRoom);
        }
    }

    private void Awake()
    {
        ConfigureActions();
        bool isEnabled;
        try
        {
            ManualLogSource manualLogSource = log;
            if (manualLogSource != null)
            {
                BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(30, 1, out isEnabled);
                if (isEnabled)
                {
                    bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Installing ");
                    bepInExInfoLogInterpolatedStringHandler.AppendFormatted("HealthPixyPlus");
                    bepInExInfoLogInterpolatedStringHandler.AppendLiteral(" Harmony patches...");
                }
                manualLogSource.LogInfo(bepInExInfoLogInterpolatedStringHandler);
            }
            new Harmony("com.SoapBoxHero.HealthPixyPlus").PatchAll(typeof(HealthPixyPlusPlugin));
            manualLogSource = log;
            if (manualLogSource != null)
            {
                BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(28, 1, out isEnabled);
                if (isEnabled)
                {
                    bepInExInfoLogInterpolatedStringHandler.AppendFormatted("HealthPixyPlus");
                    bepInExInfoLogInterpolatedStringHandler.AppendLiteral(" Harmony patches successful.");
                }
                manualLogSource.LogInfo(bepInExInfoLogInterpolatedStringHandler);
            }
            ToolsPlugin.RegisterPeriodicAction(1f, Run);
            manualLogSource = log;
            if (manualLogSource != null)
            {
                BepInExMessageLogInterpolatedStringHandler bepInExMessageLogInterpolatedStringHandler = new BepInExMessageLogInterpolatedStringHandler(38, 1, out isEnabled);
                if (isEnabled)
                {
                    bepInExMessageLogInterpolatedStringHandler.AppendLiteral("Registered ");
                    bepInExMessageLogInterpolatedStringHandler.AppendFormatted("HealthPixyPlus");
                    bepInExMessageLogInterpolatedStringHandler.AppendLiteral(" action for period of 1sec.");
                }
                manualLogSource.LogMessage(bepInExMessageLogInterpolatedStringHandler);
            }
        }
        catch
        {
            ManualLogSource manualLogSource = log;
            if (manualLogSource != null)
            {
                BepInExErrorLogInterpolatedStringHandler bepInExErrorLogInterpolatedStringHandler = new BepInExErrorLogInterpolatedStringHandler(24, 1, out isEnabled);
                if (isEnabled)
                {
                    bepInExErrorLogInterpolatedStringHandler.AppendFormatted("HealthPixyPlus");
                    bepInExErrorLogInterpolatedStringHandler.AppendLiteral(" Harmony patches failed!");
                }
                manualLogSource.LogError(bepInExErrorLogInterpolatedStringHandler);
            }
        }
    }

    public void Run()
    {
        if (IsPixiesUnavailible())
        {
            return;
        }
        foreach (Female ownedFemale in CharacterAccessTool.GetOwnedFemales())
        {
            if (IsFemalePixyHealthDisabled(ownedFemale))
            {
                continue;
            }
            foreach (Action<Female> enabledAction in EnabledActions)
            {
                enabledAction?.Invoke(ownedFemale);
            }
        }
    }

    public bool IsPixiesUnavailible()
    {
        if (PD == null || PD.FloraPixyCount <= 0 || PD.PixyCountInAction == m_PixyCountRef(GameManager.Instance.PlayerData))
        {
            return true;
        }
        if (!PD.FloraPixyHealth)
        {
            if (PreviousFloraPixyHealth)
            {
                ResetState();
            }
            PreviousFloraPixyHealth = PD.FloraPixyHealth;
            return true;
        }
        PreviousFloraPixyHealth = PD.FloraPixyHealth;
        return false;
    }

    public bool IsFemalePixyHealthDisabled(Female female)
    {
        if (!female.PixyHealth)
        {
            if (FemalePreviousPixyHealth.TryGetValue(female.UnitId, out var value) && value)
            {
                ResetFemaleState(female);
            }
            FemalePreviousPixyHealth[female.UnitId] = female.PixyHealth;
            return true;
        }
        FemalePreviousPixyHealth[female.UnitId] = female.PixyHealth;
        return false;
    }

    public void PreRestIfAvailable(Female female)
    {
        if (!restingReturnBlocks.ContainsKey(female.UnitId) && PreReadyToRestStates.Contains(female.State))
        {
            if (female.PreviousRoomIdStack.Count == 0 || female.PreviousSeatStack.Count == 0)
            {
                restingReturnBlocks.Add(female.UnitId, NoReturnTuple);
                return;
            }
            int item = female.PreviousRoomIdStack.Pop();
            int item2 = female.PreviousSeatStack.Pop();
            restingReturnBlocks.Add(female.UnitId, (item, item2));
        }
    }

    public void RestIfAvailable(Female female)
    {
        if (!restingReturnOverrides.ContainsKey(female.UnitId) && ReadyToRestStates.Contains(female.State) && TryGetSeatInRoomByType(ERoomType.SlaveCage, female, out (Room, int) result))
        {
            if (restingReturnBlocks.TryGetValue(female.UnitId, out (int, int) value))
            {
                restingReturnOverrides.Add(female.UnitId, value);
                restingReturnBlocks.Remove(female.UnitId);
                female.PreviousRoomIdStack.Push(result.Item1.UnitId);
                female.PreviousSeatStack.Push(result.Item2);
            }
            else if (female.PreviousRoomIdStack.Count == 0 || female.PreviousSeatStack.Count == 0)
            {
                restingReturnOverrides.Add(female.UnitId, NoReturnTuple);
                female.PreviousRoomIdStack.Push(result.Item1.UnitId);
                female.PreviousSeatStack.Push(result.Item2);
            }
            else
            {
                int item = female.PreviousRoomIdStack.Pop();
                int item2 = female.PreviousSeatStack.Pop();
                restingReturnOverrides.Add(female.UnitId, (item, item2));
                female.PreviousRoomIdStack.Push(result.Item1.UnitId);
                female.PreviousSeatStack.Push(result.Item2);
            }
        }
    }

    public void RestForcibly(Female female)
    {
        if (!restingReturnOverrides.ContainsKey(female.UnitId) && !stolenStats.ContainsKey(female.UnitId) && female.Room.RoomType == ERoomType.BreedingRoom && female.TrainingLevel < 5 && female.State == EState.Rest && !female.IsPregnant)
        {
            stolenStats.Add(female.UnitId, (female.Health, female.Training, female.TrainingLevel));
            restingReturnOverrides.Add(female.UnitId, (female.Room.UnitId, female.Seat));
            female.Training = 10;
            female.TrainingLevel = 1;
            female.Health = female.MaxHealth * 0.1f;
        }
    }

    public void UndoForcedHealthReduction(Female female)
    {
        if (stolenStats.TryGetValue(female.UnitId, out (float, int, int) value) && female.Room.RoomType == ERoomType.SlaveCage)
        {
            (female.Health, female.Training, female.TrainingLevel) = value;
            stolenStats.Remove(female.UnitId);
            if (restingReturnOverrides.TryGetValue(female.UnitId, out (int, int) _))
            {
                female.PreviousRoomIdStack.Clear();
                female.PreviousSeatStack.Clear();
            }
        }
    }

    public void UndoForcedRestAtIdle(Female female)
    {
        if (restingReturnOverrides.TryGetValue(female.UnitId, out (int, int) value) && female.Room.RoomType == ERoomType.SlaveCage && female.State == EState.Idle)
        {
            if (value.Item1 != NoReturnTuple.Item1)
            {
                female.PreviousRoomIdStack.Push(value.Item1);
                female.PreviousSeatStack.Push(value.Item2);
            }
            restingReturnOverrides.Remove(female.UnitId);
        }
    }

    public static void ClearReservedRoom(Female female)
    {
        if (reservedRooms.ContainsKey(female.UnitId) && female.Room.RoomType == ERoomType.SlaveCage)
        {
            reservedRooms.Remove(female.UnitId);
        }
    }

    public static void ClearState()
    {
        restingReturnBlocks.Clear();
        restingReturnOverrides.Clear();
        stolenStats.Clear();
        reservedRooms.Clear();
    }

    public static void ClearUnitState(int id)
    {
        restingReturnBlocks.Remove(id);
        restingReturnOverrides.Remove(id);
        reservedRooms.Remove(id);
        if (!Females.TryGetValue(id, out Female value))
        {
            stolenStats.Remove(id);
        }
        if (value != null)
        {
            if (stolenStats.TryGetValue(id, out (float, int, int) value2))
            {
                (value.Health, value.Training, _) = value2;
            }
            value.PreviousRoomIdStack.Clear();
            value.PreviousSeatStack.Clear();
            value.PixyIsWaiting = false;
        }
    }

    public static void ResetState()
    {
        foreach (KeyValuePair<int, Female> female in Females)
        {
            ResetFemaleState(female.Value);
        }
        restingReturnBlocks.Clear();
        restingReturnOverrides.Clear();
        stolenStats.Clear();
        reservedRooms.Clear();
    }

    public static void ResetFemaleState(Female female)
    {
        if (female != null)
        {
            female.PreviousRoomIdStack.Clear();
            female.PreviousSeatStack.Clear();
            if (restingReturnOverrides.TryGetValue(female.UnitId, out (int, int) value))
            {
                female.PreviousRoomIdStack.Push(value.Item1);
                female.PreviousSeatStack.Push(value.Item2);
                restingReturnOverrides.Remove(female.UnitId);
            }
            if (restingReturnBlocks.TryGetValue(female.UnitId, out (int, int) value2))
            {
                female.PreviousRoomIdStack.Push(value2.Item1);
                female.PreviousSeatStack.Push(value2.Item2);
                restingReturnBlocks.Remove(female.UnitId);
            }
            if (stolenStats.TryGetValue(female.UnitId, out (float, int, int) value3))
            {
                (female.Health, female.Training, _) = value3;
                stolenStats.Remove(female.UnitId);
            }
            reservedRooms.Remove(female.UnitId);
        }
    }

    public bool TryGetSeatInRoomByType(ERoomType roomType, Character character, out (Room room, int seat) result)
    {
        result = (room: new Room(0), seat: 0);
        if (PD == null)
        {
            return false;
        }
        foreach (Room room in PD.GetClosedRoomList(roomType, character.Position, isPrivate: false))
        {
            if (room == null || m_AllocatableSeatCountRef(room) == 0)
            {
                continue;
            }
            int? seat = (int?)GetEmptySeatInRoomRef(PD, room.Sector, room.Slot);
            if (!reservedRooms.Any((KeyValuePair<int, (int roomid, int seat)> reserve) => reserve.Value.roomid == room.UnitId && reserve.Value.seat == seat))
            {
                if (seat.HasValue)
                {
                    result = (room: room, seat: seat.Value);
                }
                return true;
            }
        }
        return false;
    }
}