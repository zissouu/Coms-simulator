using System;
using System.Collections.Generic;

class Program
{
    public static Random rand = new Random();
    static List<string> messageLog = new List<string>();
    static List<string> actionHistory = new List<string>();
    static Squad squad = new Squad();
    static List<Zone> zones = new List<Zone>();
    static StoryNode currentStoryNode;
    static int currentDay = 1;
    static int maxDays = 7;

    // Campaign state
    static HashSet<string> campaignFlags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    static int localReputation = 0;
    static int extractionReadiness = 0;

    // NPC roster
    static List<NPC> npcs = new List<NPC>
    {
        new NPC("Elder Rahim","Village Elder"),
        new NPC("Trader Amir","Merchant"),
        new NPC("Medic Fatima","Medic"),
        new NPC("Militia Zahir","Local Militia"),
        new NPC("Scout Leyla","Scout")
    };

    static void Main(string[] args)
    {
        Console.WriteLine("=== Military Comm Simulation: Hybrid Campaign with Persistent NPC Dialogue ===");

        Soldier[] squadMembers = {
            new Soldier("Private Ryan"),
            new Soldier("Corporal Davis"),
            new Soldier("Specialist Lee")
        };
        Commander commander = new Commander("Commander Smith");

        zones.Add(new Zone("Valley Outpost"));
        zones.Add(new Zone("Checkpoint Alpha"));
        zones.Add(new Zone("Mountain Pass"));
        zones.Add(new Zone("Bazaar Village"));
        zones.Add(new Zone("Supply Route"));

        messageLog.Add("You deploy to the AO. Balance combat and local interactions carefully.");

        while (currentDay <= maxDays && squad.Health > 0)
        {
            Console.WriteLine($"\n=== Day {currentDay} ===");
            StartDay(commander, squadMembers);
            DaySummary();
            NightCommunications();
            currentDay++;
        }

        EvaluateEnding();
    }

    // -------------------- Main Day Flow --------------------
    static void StartDay(Commander commander, Soldier[] squadMembers)
    {
        CheckNPCStoryTriggers();

        var choicesPrimary = new Dictionary<string, Action>();

        if (campaignFlags.Contains("intel_rare_cache") && !campaignFlags.Contains("raided_supply_cache"))
        {
            choicesPrimary["Raid the rare enemy supply cache"] = () =>
            {
                AssignMission(commander, squadMembers, "Raid", "Mountain Pass", 3, "Aggressive");
                campaignFlags.Add("raided_supply_cache");
                actionHistory.Add($"Day {currentDay}: Raided rare supply cache");
            };
        }

        choicesPrimary["Prioritize combat operations"] = () =>
            AssignMission(commander, squadMembers, "Patrol", "Checkpoint Alpha", 2, "Aggressive");

        choicesPrimary["Prioritize hearts & minds"] = () =>
            ConductCivicAction(squadMembers, "Bazaar Village");

        choicesPrimary["Balanced operations"] = () =>
        {
            AssignMission(commander, squadMembers, "Patrol", "Valley Outpost", 2, "Cautious");
            ConductCivicAction(squadMembers, "Bazaar Village");
        };

        currentStoryNode = new StoryNode($"Day {currentDay} briefing: Situation evolving.", choicesPrimary);
        TriggerStoryNode();

        AfternoonNPCInteractions(squadMembers);
        ReceiveMessage(squadMembers);
        AmbientEvents();
    }

    // -------------------- Story Node Handling --------------------
    static void TriggerStoryNode()
    {
        Console.WriteLine($"\nSTORY: {currentStoryNode.Description}");
        int index = 1;
        foreach (var choice in currentStoryNode.Choices.Keys)
        {
            Console.WriteLine($"{index}. {choice}");
            index++;
        }
        Console.Write("Choice: ");
        string input = Console.ReadLine()!;
        if (int.TryParse(input, out int selected) && selected >= 1 && selected <= currentStoryNode.Choices.Count)
        {
            var action = new List<Action>(currentStoryNode.Choices.Values)[selected - 1];
            action.Invoke();
        }
        else
        {
            Console.WriteLine("Invalid choice. Defaulting to first option.");
            new List<Action>(currentStoryNode.Choices.Values)[0].Invoke();
        }
    }

    // -------------------- NPC Interactions --------------------
    static void AfternoonNPCInteractions(Soldier[] squadMembers)
    {
        foreach (var npc in npcs)
        {
            if (rand.NextDouble() < 0.4)
            {
                Console.WriteLine($"\nAfternoon: {npc.Name} ({npc.Role}) requests interaction.");

                if (!npc.History.Contains("offered_side_mission"))
                {
                    var mission = GenerateSideMission(npc);
                    Console.WriteLine($"{npc.Name} offers: {mission.Name}");
                    Console.WriteLine(mission.Description);
                    Console.Write("Accept? (y/n): ");
                    string input = Console.ReadLine()!;
                    if (input.ToLower() == "y")
                    {
                        ExecuteSideMission(mission, squadMembers, npc);
                        mission.Completed = true;
                    }
                    npc.History.Add("offered_side_mission");
                }

                DialogueWithNPC(npc);
            }
        }
    }

    // -------------------- Persistent Branching Dialogue --------------------
    static void DialogueWithNPC(NPC npc)
    {
        if (npc.CurrentDialogue == null)
        {
            // Initialize a simple starting dialogue
            var node = new DialogueNode($"Hello, Commander. What should we do today?");
            node.Choices["Ask about enemy activity"] = () =>
            {
                Console.WriteLine($"{npc.Name}: 'Enemy scouts spotted near the pass.'");
                npc.Trust += 5;
                npc.CurrentDialogue = new DialogueNode("Any support you need from us?");
            };
            node.Choices["Discuss local village"] = () =>
            {
                Console.WriteLine($"{npc.Name}: 'Villagers are wary, but they notice your squad's efforts.'");
                npc.Trust += 3;
                npc.CurrentDialogue = new DialogueNode("We might gain their cooperation with your help.");
            };
            node.Choices["End conversation"] = () => { npc.CurrentDialogue = null; };

            npc.CurrentDialogue = node;
        }

        while (npc.CurrentDialogue != null)
        {
            var node = npc.CurrentDialogue;
            Console.WriteLine($"\n{npc.Name}: {node.Text}");
            int i = 1;
            foreach (var choice in node.Choices.Keys)
            {
                Console.WriteLine($"{i}. {choice}");
                i++;
            }

            Console.Write("Choice: ");
            string input = Console.ReadLine()!;
            if (int.TryParse(input, out int selected) && selected >= 1 && selected <= node.Choices.Count)
            {
                var action = new List<Action>(node.Choices.Values)[selected - 1];
                action.Invoke();
            }
            else
            {
                Console.WriteLine("Invalid choice. Defaulting to first option.");
                new List<Action>(node.Choices.Values)[0].Invoke();
            }

            // Randomly end conversation after a few choices
            if (rand.NextDouble() < 0.5) npc.CurrentDialogue = null;
        }
    }

    // -------------------- Soldier Banter --------------------
    static void SoldierBanter(Soldier[] squadMembers, string zone, string missionType)
    {
        string[] banters = {
            $"\"This is going to be rough at {zone}. Stay sharp.\"",
            $"\"I hope the locals cooperate this time.\"",
            $"\"Watch your six, team. {missionType} missions are risky.\"",
            $"\"Keep your eyes open for ambushes.\"",
            $"\"Let's show them what we're made of.\""
        };
        Console.WriteLine($"{squadMembers[rand.Next(squadMembers.Length)].Name} says: {banters[rand.Next(banters.Length)]}");
        actionHistory.Add($"Day {currentDay}: Soldier banter during {missionType} in {zone}");
    }

    // -------------------- Ambient Events --------------------
    static void AmbientEvents()
    {
        string[] events = {
            "A hawk circles overhead as you patrol the valley.",
            "Distant gunfire echoes across the mountains.",
            "Local children wave at your squad as you pass through the village.",
            "Wind kicks up dust along the supply route."
        };
        if (rand.NextDouble() < 0.5)
        {
            string evt = events[rand.Next(events.Length)];
            Console.WriteLine($"[AMBIENT] {evt}");
            actionHistory.Add($"Day {currentDay}: Ambient event: {evt}");
        }
    }

    // -------------------- Side Missions --------------------
    static SideMission GenerateSideMission(NPC npc)
    {
        Zone zone = zones[rand.Next(zones.Count)];
        int diff = rand.Next(1, 4);
        string name = $"Assist {npc.Name} in {zone.Name}";
        string desc = $"Help {npc.Name} with a task in {zone.Name}. Success improves trust and morale.";

        return new SideMission(
            name,
            desc,
            zone,
            diff,
            (s, n) => { Console.WriteLine($"[SUCCESS] {npc.Name} mission succeeded!"); s.Morale = Math.Min(100, s.Morale + rand.Next(5, 10)); n.Trust = Math.Min(50, n.Trust + rand.Next(5, 10)); localReputation = Math.Min(100, localReputation + rand.Next(3, 8)); },
            (s, n) => { Console.WriteLine($"[FAILURE] {npc.Name} mission failed!"); s.Health = Math.Max(0, s.Health - rand.Next(5, 15)); s.Morale = Math.Max(0, s.Morale - rand.Next(3, 10)); n.Trust = Math.Max(-50, n.Trust - rand.Next(3, 10)); localReputation = Math.Max(-100, localReputation - rand.Next(3, 6)); }
        );
    }

    static void ExecuteSideMission(SideMission mission, Soldier[] squadMembers, NPC npc)
    {
        int chance = 50 + squad.Morale / 3 - mission.Difficulty * 10 + npc.Trust / 2;
        chance = Math.Clamp(chance, 10, 90);
        bool success = rand.Next(100) < chance;

        if (success) mission.OnSuccess(squad, npc);
        else mission.OnFailure(squad, npc);

        actionHistory.Add($"Day {currentDay}: Side mission '{mission.Name}' {(success ? "Success" : "Failure")}");
    }

    // -------------------- Missions --------------------
    static void AssignMission(Commander commander, Soldier[] squadMembers, string missionType, string zoneName, int difficulty, string strategy)
    {
        Zone zone = zones.Find(z => z.Name == zoneName) ?? new Zone(zoneName);
        if (!zones.Contains(zone)) zones.Add(zone);

        Console.WriteLine($"\nMission: {missionType} in {zone.Name} (Diff {difficulty}) Strategy: {strategy}");
        SoldierBanter(squadMembers, zone.Name, missionType);
        messageLog.Add($"Commander {commander.Name} assigned {missionType} in {zone.Name} ({strategy}).");

        int baseChance = 50 + squad.Morale / 2 - zone.Hostility * 12;
        if (difficulty == 2) baseChance -= 10;
        if (difficulty == 3) baseChance -= 20;
        if (strategy == "Cautious") baseChance += 10;
        if (strategy == "Air Support") baseChance += 20;
        if (campaignFlags.Contains("enemy_alerted")) baseChance -= 12;
        if (localReputation >= 40 && missionType != "Raid") baseChance += 10;

        baseChance = Math.Clamp(baseChance, 5, 95);
        bool success = rand.Next(100) < baseChance;

        if (success)
        {
            Console.WriteLine($"[SUCCESS] {missionType} succeeded in {zone.Name}");
            squad.Morale = Math.Min(100, squad.Morale + 8);
            squad.Supplies = Math.Max(0, squad.Supplies - rand.Next(0, 10));
            zone.Hostility = Math.Max(0, zone.Hostility - 1);
        }
        else
        {
            Console.WriteLine($"[FAILURE] {missionType} failed in {zone.Name}");
            squad.Health = Math.Max(0, squad.Health - rand.Next(5, 20));
            squad.Morale = Math.Max(0, squad.Morale - rand.Next(5, 18));
            squad.Supplies = Math.Max(0, squad.Supplies - rand.Next(5, 25));
            zone.Hostility = Math.Min(3, zone.Hostility + 1);
            if (rand.NextDouble() < 0.5) campaignFlags.Add("enemy_alerted");
        }

        int si = rand.Next(squadMembers.Length);
        string report = squadMembers[si].ReportStatus(zone.Name);
        Console.WriteLine($"Incoming report: {report}");
        messageLog.Add(report);
        actionHistory.Add($"Day {currentDay}: {missionType} at {zone.Name} ({(success ? "S" : "F")})");
    }

    // -------------------- Civic Actions --------------------
    static void ConductCivicAction(Soldier[] squadMembers, string zoneName)
    {
        Zone zone = zones.Find(z => z.Name == zoneName) ?? new Zone(zoneName);
        if (!zones.Contains(zone)) zones.Add(zone);

        Console.WriteLine($"\nConducting civic action in {zone.Name}");
        squad.Supplies = Math.Max(0, squad.Supplies - rand.Next(5, 15));

        int chance = 60 + (localReputation / 3);
        chance = Math.Clamp(chance, 20, 95);
        bool success = rand.Next(100) < chance;

        if (success)
        {
            Console.WriteLine($"[SUCCESS] Positive local impact in {zone.Name}");
            localReputation = Math.Min(100, localReputation + rand.Next(5, 12));
            squad.Morale = Math.Min(100, squad.Morale + rand.Next(3, 8));
        }
        else
        {
            Console.WriteLine($"[FAILURE] Minimal impact in {zone.Name}");
            localReputation = Math.Max(-100, localReputation - rand.Next(3, 8));
            squad.Morale = Math.Max(0, squad.Morale - rand.Next(1, 5));
        }

        actionHistory.Add($"Day {currentDay}: Civic action in {zone.Name} ({(success ? "Success" : "Failure")})");
    }

    // -------------------- Messages --------------------
    static void ReceiveMessage(Soldier[] squadMembers)
    {
        int si = rand.Next(squadMembers.Length);
        string msg = squadMembers[si].ReportStatus();
        messageLog.Add(msg);
        Console.WriteLine($"Incoming message: {msg}");

        if (rand.NextDouble() < 0.3)
        {
            string ev = GenerateRandomEvent();
            messageLog.Add(ev);
            Console.WriteLine($"[EVENT] {ev}");
        }
    }

    static string GenerateRandomEvent()
    {
        string[] events = {
            "Enemy ambush reported at checkpoint!",
            "IED detected near supply route.",
            "Air support requested at grid 7B.",
            "Squad successfully cleared the area.",
            "Local informant provides intel."
        };
        return events[rand.Next(events.Length)];
    }

    // -------------------- Day Summary --------------------
    static void DaySummary()
    {
        Console.WriteLine($"\n--- End of Day {currentDay} Summary ---");
        Console.WriteLine($"Health: {squad.Health}, Morale: {squad.Morale}, Supplies: {squad.Supplies}");
        Console.WriteLine($"Local Reputation: {localReputation}, Extraction Readiness: {extractionReadiness}%");
        Console.WriteLine("Zone Hostility Levels:");
        foreach (var z in zones) Console.WriteLine($"- {z.Name}: {z.Hostility}/3");
        Console.WriteLine("-----------------------------");
    }

    static void NightCommunications()
    {
        Console.WriteLine("\nNight comms: Reviewing messages from squad...");
        foreach (var msg in messageLog) Console.WriteLine($"- {msg}");
        messageLog.Clear();
    }

    // -------------------- Multi-day Story Logic --------------------
    static void CheckNPCStoryTriggers()
    {
        foreach (var npc in npcs)
        {
            if (npc.Trust >= 30 && !npc.History.Contains("offered_rare_intel"))
            {
                Console.WriteLine($"{npc.Name} shares rare intel: Enemy cache spotted!");
                campaignFlags.Add("intel_rare_cache");
                npc.History.Add("offered_rare_intel");
            }
            else if (npc.Trust <= -30 && !npc.History.Contains("misled_team"))
            {
                Console.WriteLine($"{npc.Name} misled your squad! Ambush risk increased!");
                campaignFlags.Add("enemy_ambush_possible");
                npc.History.Add("misled_team");
            }
        }
    }

    // -------------------- Evaluate Ending --------------------
    static void EvaluateEnding()
    {
        Console.WriteLine("\n=== Campaign Outcome ===");

        if (squad.Health <= 0)
        {
            Console.WriteLine("Your squad has been wiped out. Mission failed.");
            return;
        }

        if (localReputation >= 50 && squad.Health > 60)
        {
            Console.WriteLine("Successful hybrid campaign: Local support strong, operations efficient.");
            if (campaignFlags.Contains("intel_rare_cache"))
                Console.WriteLine("Rare intel obtained: Enemy movements disrupted.");
        }
        else if (localReputation < -30)
        {
            Console.WriteLine("Campaign failure: Locals hostile, squad forced to withdraw.");
            if (campaignFlags.Contains("enemy_alerted"))
                Console.WriteLine("Enemy ambushes increased due to prior actions. Heavy losses incurred.");
        }
        else
        {
            Console.WriteLine("Mixed outcome: Some success, area remains contested.");
        }

        foreach (var npc in npcs)
        {
            if (npc.Trust >= 40)
                Console.WriteLine($"{npc.Name} ({npc.Role}) praises your efforts, will support future operations.");
            else if (npc.Trust <= -40)
                Console.WriteLine($"{npc.Name} ({npc.Role}) warns others against your team; relations damaged.");
        }

        Console.WriteLine($"\nSquad Health: {squad.Health}, Morale: {squad.Morale}, Supplies: {squad.Supplies}");
        Console.WriteLine($"Local Reputation: {localReputation}, Extraction Readiness: {extractionReadiness}%");
        Console.WriteLine("\nAction history:");
        foreach (var h in actionHistory) Console.WriteLine($"- {h}");
    }
}

// -------------------- Supporting Classes --------------------
class Commander { public string Name { get; set; } public Commander(string n) { Name = n; } }
class Soldier
{
    public string Name { get; set; }
    public Soldier(string n) { Name = n; }
    public string ReportStatus(string? zone = null)
    {
        string[] statuses = { "all clear", "under fire", "advancing", "retreating" };
        string s = statuses[Program.rand.Next(statuses.Length)];
        return zone != null ? $"{Name} reports from {zone}: {s}." : $"{Name} reports: {s}.";
    }
}
class Squad { public int Health = 100; public int Morale = 100; public int Supplies = 100; }
class Zone { public string Name; public int Hostility = 1; public Zone(string n) { Name = n; } }
class StoryNode { public string Description; public Dictionary<string, Action> Choices; public StoryNode(string d, Dictionary<string, Action> c) { Description = d; Choices = c; } }
class NPC
{
    public string Name; public string Role; public int Trust = 0; public List<string> History = new List<string>();
    public DialogueNode CurrentDialogue;
    public NPC(string n, string r) { Name = n; Role = r; }
}
class DialogueNode
{
    public string Text;
    public Dictionary<string, Action> Choices = new Dictionary<string, Action>();
    public bool Visited = false;
    public DialogueNode(string text) { Text = text; }
}
class SideMission
{
    public string Name; public string Description; public Zone TargetZone; public int Difficulty;
    public Action<Squad, NPC> OnSuccess; public Action<Squad, NPC> OnFailure;
    public bool Completed = false;
    public SideMission(string n, string d, Zone z, int diff, Action<Squad, NPC> s, Action<Squad, NPC> f)
    { Name = n; Description = d; TargetZone = z; Difficulty = diff; OnSuccess = s; OnFailure = f; }
}
