# Coms-simulator
Comm Simulation is a hybrid strategy and narrative simulation game where the player commands a squad in Afghanistan across multiple days. The game blends combat operations, hearts & minds missions, dynamic NPC interactions, and side missions with branching story arcs.

Players must balance squad health, morale, and supplies while responding to evolving threats and opportunities. NPCs provide dialogue, optional side missions, and secret intel that influence the campaign outcome.

Features

Multi-day campaign: Play across 7 simulated days, with evolving events and challenges.

Dynamic story arcs: Player decisions impact mission outcomes, local reputation, and squad status.

NPC interactions: Five unique NPCs (Elder Rahim, Trader Amir, Medic Fatima, Militia Zahir, Scout Leyla) provide:

Multi-level 5-stage dialogue trees

Optional side missions

Trust-based secret branches and intel

Side missions: Optional tasks from NPCs with success/failure outcomes affecting trust, morale, supplies, and campaign flags.

Branching campaigns: Different story outcomes depending on player choices and mission results.

Squad management: Track health, morale, and supplies; manage risk vs reward in operations.

Ambient events and messages: Random events, soldier reports, and communications for immersive gameplay.

Gameplay Mechanics

Daily Briefing: Each day begins with a briefing presenting key missions or story decisions.

Mission Assignments: Choose from combat operations, civic engagement, or balanced strategies. Mission outcomes are influenced by squad stats, zone hostility, and chosen strategy.

NPC Dialogue: Interact with NPCs to gather intelligence, unlock secret missions, and gain trust. Dialogue decisions affect local reputation and unlock branching outcomes.

Side Missions: Optional missions offered by NPCs. Success or failure affects squad morale, trust, and campaign progress.

Events & Reports: Receive random messages, soldier reports, and events that impact operations or provide intel.

Day Summary: At the end of each day, review squad health, morale, supplies, and zone hostility levels.

Installation

Clone or download the repository.

Open the project in Visual Studio or Visual Studio Code with the C# extension.

Build the project targeting .NET 6+.

Run the console application (Program.cs).

Controls

Console Input:

Enter the number corresponding to your choice during story or dialogue prompts.

For yes/no prompts (side missions), enter y or n.

Game Stats:

Health: Squad survivability.

Morale: Squad performance and willingness to take risks.

Supplies: Resources needed for missions and side operations.

Trust: NPC trust gained or lost based on player decisions.

Local Reputation: Influence over local civilians and villages.

NPCs & Side Missions
NPC	Role	Sample Side Mission
Elder Rahim	Village Elder	Protect the Village
Trader Amir	Merchant	Escort Supplies
Medic Fatima	Medic	Provide Medical Aid
Militia Zahir	Local Militia	Train Local Militia
Scout Leyla	Scout	Scout Enemy Positions

Trust and reputation influence available dialogue choices, secret intel, and mission rewards.

Winning & Losing

Victory: Complete the campaign with squad health > 0 and maintain local support.

Failure: Squad wiped out or local support critically reduced.

Branching outcomes: Player decisions and side mission success determine narrative endings and secret rewards.

Future Enhancements

Add additional side missions per NPC with hidden objectives.

Expand secret story branches tied to squad performance and NPC trust.

Implement randomized events per day for replayability.

Add combat mini-simulations for deeper mission interaction.

Credits

Lead Developer: [Your Name]

Inspired By: Modern tactical narrative simulations and hybrid strategy games.

License

This project is open-source for educational and personal use. Commercial use requires permission.
