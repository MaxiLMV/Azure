using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ProjectM;

namespace VCreate.Data
{
	// Token: 0x0200001B RID: 27
	internal static class Characters
	{
		// Token: 0x04000494 RID: 1172
		public static ReadOnlyDictionary<string, PrefabGUID> Named = new ReadOnlyDictionary<string, PrefabGUID>(new Dictionary<string, PrefabGUID>(StringComparer.OrdinalIgnoreCase)
		{
			{
				"CHAR_ArchMage_FlameSphere",
				new PrefabGUID(2138173476)
			},
			{
				"CHAR_ArchMage_Summon",
				new PrefabGUID(805231073)
			},
			{
				"CHAR_ArchMage_VBlood",
				new PrefabGUID(-2013903325)
			},
			{
				"CHAR_Bandit_Bomber",
				new PrefabGUID(-1128238456)
			},
			{
				"CHAR_Bandit_Bomber_Servant",
				new PrefabGUID(-450600397)
			},
			{
				"CHAR_Bandit_Bomber_VBlood",
				new PrefabGUID(1896428751)
			},
			{
				"CHAR_Bandit_Deadeye",
				new PrefabGUID(-1030822544)
			},
			{
				"CHAR_Bandit_Deadeye_Chaosarrow_VBlood",
				new PrefabGUID(763273073)
			},
			{
				"CHAR_Bandit_Deadeye_Frostarrow_VBlood",
				new PrefabGUID(1124739990)
			},
			{
				"CHAR_Bandit_Deadeye_Servant",
				new PrefabGUID(-2086044081)
			},
			{
				"CHAR_Bandit_Foreman_VBlood",
				new PrefabGUID(2122229952)
			},
			{
				"CHAR_Bandit_GraveDigger_VBlood_UNUSED",
				new PrefabGUID(936169687)
			},
			{
				"CHAR_Bandit_Hunter",
				new PrefabGUID(-1301144178)
			},
			{
				"CHAR_Bandit_Hunter_Servant",
				new PrefabGUID(-370708253)
			},
			{
				"CHAR_Bandit_Leader_VBlood_UNUSED",
				new PrefabGUID(-175381832)
			},
			{
				"CHAR_Bandit_Leader_Wolf_Summon",
				new PrefabGUID(-671059374)
			},
			{
				"CHAR_Bandit_Miner_Standard_Servant",
				new PrefabGUID(1112903312)
			},
			{
				"CHAR_Bandit_Miner_VBlood_UNUSED",
				new PrefabGUID(276934707)
			},
			{
				"CHAR_Bandit_Mugger",
				new PrefabGUID(2057508774)
			},
			{
				"CHAR_Bandit_Mugger_Servant",
				new PrefabGUID(1727426580)
			},
			{
				"CHAR_Bandit_Prisoner_Villager_Female",
				new PrefabGUID(1069072707)
			},
			{
				"CHAR_Bandit_Prisoner_Villager_Male",
				new PrefabGUID(286320185)
			},
			{
				"CHAR_Bandit_Stalker",
				new PrefabGUID(-309264723)
			},
			{
				"CHAR_Bandit_Stalker_Servant",
				new PrefabGUID(1453520986)
			},
			{
				"CHAR_Bandit_Stalker_VBlood",
				new PrefabGUID(1106149033)
			},
			{
				"CHAR_Bandit_StoneBreaker_VBlood",
				new PrefabGUID(-2025101517)
			},
			{
				"CHAR_Bandit_Thief",
				new PrefabGUID(923140362)
			},
			{
				"CHAR_Bandit_Thief_Servant",
				new PrefabGUID(-872078546)
			},
			{
				"CHAR_Bandit_Thief_VBlood_UNUSED",
				new PrefabGUID(2139023341)
			},
			{
				"CHAR_Bandit_Thug",
				new PrefabGUID(-301730941)
			},
			{
				"CHAR_Bandit_Thug_Servant",
				new PrefabGUID(1466015976)
			},
			{
				"CHAR_Bandit_Tourok_VBlood",
				new PrefabGUID(-1659822956)
			},
			{
				"CHAR_Bandit_Trapper",
				new PrefabGUID(-589412777)
			},
			{
				"CHAR_Bandit_Trapper_Servant",
				new PrefabGUID(2112911542)
			},
			{
				"CHAR_Bandit_Wolf",
				new PrefabGUID(-1554428547)
			},
			{
				"CHAR_Bandit_Woodcutter_Standard_Servant",
				new PrefabGUID(51737727)
			},
			{
				"CHAR_Bandit_Worker_Gatherer",
				new PrefabGUID(1743532914)
			},
			{
				"CHAR_Bandit_Worker_Gatherer_Servant",
				new PrefabGUID(48283616)
			},
			{
				"CHAR_Bandit_Worker_Miner",
				new PrefabGUID(-2039670689)
			},
			{
				"CHAR_Bandit_Worker_Woodcutter",
				new PrefabGUID(1309418594)
			},
			{
				"CHAR_BatVampire_VBlood",
				new PrefabGUID(1112948824)
			},
			{
				"CHAR_ChurchOfLight_Archer",
				new PrefabGUID(426583055)
			},
			{
				"CHAR_ChurchOfLight_Archer_Servant",
				new PrefabGUID(-915884427)
			},
			{
				"CHAR_ChurchOfLight_Cardinal_VBlood",
				new PrefabGUID(114912615)
			},
			{
				"CHAR_ChurchOfLight_CardinalAide",
				new PrefabGUID(1745498602)
			},
			{
				"CHAR_ChurchOfLight_Cleric",
				new PrefabGUID(-1464869978)
			},
			{
				"CHAR_ChurchOfLight_Cleric_Servant",
				new PrefabGUID(1218339832)
			},
			{
				"CHAR_ChurchOfLight_EnchantedCross",
				new PrefabGUID(-1449314709)
			},
			{
				"CHAR_ChurchOfLight_Footman",
				new PrefabGUID(2128996433)
			},
			{
				"CHAR_ChurchOfLight_Footman_Servant",
				new PrefabGUID(-1719944550)
			},
			{
				"CHAR_ChurchOfLight_Knight_2H",
				new PrefabGUID(-930333806)
			},
			{
				"CHAR_ChurchOfLight_Knight_2H_Servant",
				new PrefabGUID(17367048)
			},
			{
				"CHAR_ChurchOfLight_Knight_Shield",
				new PrefabGUID(794228023)
			},
			{
				"CHAR_ChurchOfLight_Knight_Shield_Servant",
				new PrefabGUID(-694328454)
			},
			{
				"CHAR_ChurchOfLight_Lightweaver",
				new PrefabGUID(1185952775)
			},
			{
				"CHAR_ChurchOfLight_Lightweaver_Servant",
				new PrefabGUID(-383158562)
			},
			{
				"CHAR_ChurchOfLight_Miner_Standard",
				new PrefabGUID(924132254)
			},
			{
				"CHAR_ChurchOfLight_Miner_Standard_Servant",
				new PrefabGUID(-1988959460)
			},
			{
				"CHAR_ChurchOfLight_Overseer_VBlood",
				new PrefabGUID(-26105228)
			},
			{
				"CHAR_ChurchOfLight_Paladin",
				new PrefabGUID(1728773109)
			},
			{
				"CHAR_ChurchOfLight_Paladin_HomePos",
				new PrefabGUID(-502558061)
			},
			{
				"CHAR_ChurchOfLight_Paladin_Servant",
				new PrefabGUID(1649578802)
			},
			{
				"CHAR_ChurchOfLight_Paladin_VBlood",
				new PrefabGUID(-740796338)
			},
			{
				"CHAR_ChurchOfLight_Priest",
				new PrefabGUID(1406393857)
			},
			{
				"CHAR_ChurchOfLight_Priest_Servant",
				new PrefabGUID(-1728284448)
			},
			{
				"CHAR_ChurchOfLight_Rifleman",
				new PrefabGUID(1148936156)
			},
			{
				"CHAR_ChurchOfLight_Rifleman_Servant",
				new PrefabGUID(-268935837)
			},
			{
				"CHAR_ChurchOfLight_SlaveMaster_Enforcer",
				new PrefabGUID(891705701)
			},
			{
				"CHAR_ChurchOfLight_SlaveMaster_Enforcer_Servant",
				new PrefabGUID(-2114140065)
			},
			{
				"CHAR_ChurchOfLight_SlaveMaster_Sentry",
				new PrefabGUID(-240536861)
			},
			{
				"CHAR_ChurchOfLight_SlaveMaster_Sentry_Servant",
				new PrefabGUID(-442412464)
			},
			{
				"CHAR_ChurchOfLight_SlaveRuffian",
				new PrefabGUID(-1875351031)
			},
			{
				"CHAR_ChurchOfLight_SlaveRuffian_Cover",
				new PrefabGUID(-1387838833)
			},
			{
				"CHAR_ChurchOfLight_SlaveRuffian_Servant",
				new PrefabGUID(-1416355128)
			},
			{
				"CHAR_ChurchOfLight_SmiteOrb",
				new PrefabGUID(1917502536)
			},
			{
				"CHAR_ChurchOfLight_Sommelier_BarrelMinion",
				new PrefabGUID(-1917548708)
			},
			{
				"CHAR_ChurchOfLight_Sommelier_VBlood",
				new PrefabGUID(192051202)
			},
			{
				"CHAR_ChurchOfLight_Villager_Female",
				new PrefabGUID(-1224027101)
			},
			{
				"CHAR_ChurchOfLight_Villager_Female_Servant",
				new PrefabGUID(1157537604)
			},
			{
				"CHAR_ChurchOfLight_Villager_Male",
				new PrefabGUID(-2025921616)
			},
			{
				"CHAR_ChurchOfLight_Villager_Male_Servant",
				new PrefabGUID(-1786031969)
			},
			{
				"CHAR_CopperGolem",
				new PrefabGUID(1107541186)
			},
			{
				"CHAR_CreatureDeer_Mutated",
				new PrefabGUID(-575831311)
			},
			{
				"CHAR_CreatureMoose_Mutated",
				new PrefabGUID(1570140219)
			},
			{
				"CHAR_Critter_Rat",
				new PrefabGUID(-2072914343)
			},
			{
				"CHAR_Critter_Silkworm",
				new PrefabGUID(-1587402408)
			},
			{
				"CHAR_Critter_VerminNest_Rat",
				new PrefabGUID(-372256748)
			},
			{
				"CHAR_Cultist_Pyromancer",
				new PrefabGUID(2055824593)
			},
			{
				"CHAR_Cultist_Slicer",
				new PrefabGUID(1807491570)
			},
			{
				"CHAR_Cursed_Bear_Spirit",
				new PrefabGUID(1105583702)
			},
			{
				"CHAR_Cursed_Bear_Standard",
				new PrefabGUID(-559819989)
			},
			{
				"CHAR_Cursed_MonsterToad",
				new PrefabGUID(575918722)
			},
			{
				"CHAR_Cursed_MonsterToad_Minion",
				new PrefabGUID(-38041784)
			},
			{
				"CHAR_Cursed_Mosquito",
				new PrefabGUID(-744966291)
			},
			{
				"CHAR_Cursed_MountainBeast_SpiritDouble",
				new PrefabGUID(-935560085)
			},
			{
				"CHAR_Cursed_MountainBeast_VBlood",
				new PrefabGUID(-1936575244)
			},
			{
				"CHAR_Cursed_Nightlurker",
				new PrefabGUID(-2046268156)
			},
			{
				"CHAR_Cursed_ToadKing_VBlood",
				new PrefabGUID(-203043163)
			},
			{
				"CHAR_Cursed_ToadSpitter",
				new PrefabGUID(1478790879)
			},
			{
				"CHAR_Cursed_Witch",
				new PrefabGUID(-56441915)
			},
			{
				"CHAR_Cursed_Witch_Exploding_Mosquito",
				new PrefabGUID(-1399273168)
			},
			{
				"CHAR_Cursed_Witch_VBlood",
				new PrefabGUID(-910296704)
			},
			{
				"CHAR_Cursed_Wolf",
				new PrefabGUID(-218175217)
			},
			{
				"CHAR_Cursed_Wolf_Spirit",
				new PrefabGUID(407089231)
			},
			{
				"CHAR_Cursed_WormTerror",
				new PrefabGUID(658578725)
			},
			{
				"CHAR_Farmland_Wolf",
				new PrefabGUID(-578677530)
			},
			{
				"CHAR_Farmlands_Cow",
				new PrefabGUID(721166952)
			},
			{
				"CHAR_Farmlands_Farmer",
				new PrefabGUID(-1342764880)
			},
			{
				"CHAR_Farmlands_Farmer_Servant",
				new PrefabGUID(516718373)
			},
			{
				"CHAR_Farmlands_HostileVillager_Base",
				new PrefabGUID(-2007601567)
			},
			{
				"CHAR_Farmlands_HostileVillager_Female_FryingPan",
				new PrefabGUID(729746981)
			},
			{
				"CHAR_Farmlands_HostileVillager_Female_Pitchfork",
				new PrefabGUID(1576267559)
			},
			{
				"CHAR_Farmlands_HostileVillager_Male_Club",
				new PrefabGUID(-164116132)
			},
			{
				"CHAR_Farmlands_HostileVillager_Male_Shovel",
				new PrefabGUID(-864975423)
			},
			{
				"CHAR_Farmlands_HostileVillager_Male_Torch",
				new PrefabGUID(-81727312)
			},
			{
				"CHAR_Farmlands_HostileVillager_Male_Unarmed",
				new PrefabGUID(-1353870145)
			},
			{
				"CHAR_Farmlands_HostileVillager_Werewolf",
				new PrefabGUID(-951976780)
			},
			{
				"CHAR_Farmlands_Militia_Summon",
				new PrefabGUID(-213868361)
			},
			{
				"CHAR_Farmlands_Nun_Servant",
				new PrefabGUID(-1788957652)
			},
			{
				"CHAR_Farmlands_Pig",
				new PrefabGUID(-1356006948)
			},
			{
				"CHAR_Farmlands_Ram",
				new PrefabGUID(947731555)
			},
			{
				"CHAR_Farmlands_Sheep",
				new PrefabGUID(1012307512)
			},
			{
				"CHAR_Farmlands_SheepOld",
				new PrefabGUID(1635167941)
			},
			{
				"CHAR_Farmlands_SmallPig",
				new PrefabGUID(1420480270)
			},
			{
				"CHAR_Farmlands_Villager_Female",
				new PrefabGUID(525027204)
			},
			{
				"CHAR_Farmlands_Villager_Female_Servant",
				new PrefabGUID(1532829342)
			},
			{
				"CHAR_Farmlands_Villager_Female_Sister",
				new PrefabGUID(1772642154)
			},
			{
				"CHAR_Farmlands_Villager_Female_Sister_Servant",
				new PrefabGUID(-444945115)
			},
			{
				"CHAR_Farmlands_Villager_Male",
				new PrefabGUID(1887807944)
			},
			{
				"CHAR_Farmlands_Villager_Male_Servant",
				new PrefabGUID(1426964824)
			},
			{
				"CHAR_Farmlands_Woodcutter_Standard",
				new PrefabGUID(-893091615)
			},
			{
				"CHAR_Farmlands_Woodcutter_Standard_Servant",
				new PrefabGUID(-1659842473)
			},
			{
				"CHAR_Forest_AngryMoose",
				new PrefabGUID(2097040330)
			},
			{
				"CHAR_Forest_Bear_Dire_Vblood",
				new PrefabGUID(-1391546313)
			},
			{
				"CHAR_Forest_Bear_Standard",
				new PrefabGUID(1043643344)
			},
			{
				"CHAR_Forest_Deer",
				new PrefabGUID(1897056612)
			},
			{
				"CHAR_Forest_Moose",
				new PrefabGUID(-831097925)
			},
			{
				"CHAR_Forest_Wolf",
				new PrefabGUID(-1418430647)
			},
			{
				"CHAR_Forest_Wolf_VBlood",
				new PrefabGUID(-1905691330)
			},
			{
				"CHAR_Geomancer_Golem_Guardian",
				new PrefabGUID(-2092246077)
			},
			{
				"CHAR_Geomancer_Golem_VBlood",
				new PrefabGUID(-1317534496)
			},
			{
				"CHAR_Geomancer_Human_VBlood",
				new PrefabGUID(-1065970933)
			},
			{
				"CHAR_Gloomrot_AceIncinerator",
				new PrefabGUID(1756241788)
			},
			{
				"CHAR_Gloomrot_AceIncinerator_Servant",
				new PrefabGUID(-1897484769)
			},
			{
				"CHAR_Gloomrot_Batoon",
				new PrefabGUID(-1707267769)
			},
			{
				"CHAR_Gloomrot_Batoon_Servant",
				new PrefabGUID(657708566)
			},
			{
				"CHAR_Gloomrot_Iva_VBlood",
				new PrefabGUID(172235178)
			},
			{
				"CHAR_Gloomrot_Monster_VBlood",
				new PrefabGUID(1233988687)
			},
			{
				"CHAR_Gloomrot_Purifier_VBlood",
				new PrefabGUID(106480588)
			},
			{
				"CHAR_Gloomrot_Pyro",
				new PrefabGUID(-322293503)
			},
			{
				"CHAR_Gloomrot_Pyro_Servant",
				new PrefabGUID(1304434816)
			},
			{
				"CHAR_Gloomrot_Railgunner",
				new PrefabGUID(1732477970)
			},
			{
				"CHAR_Gloomrot_Railgunner_Servant",
				new PrefabGUID(-1070366200)
			},
			{
				"CHAR_Gloomrot_RailgunSergeant_HomePos",
				new PrefabGUID(-1499025256)
			},
			{
				"CHAR_Gloomrot_RailgunSergeant_Minion",
				new PrefabGUID(1626314708)
			},
			{
				"CHAR_Gloomrot_RailgunSergeant_VBlood",
				new PrefabGUID(2054432370)
			},
			{
				"CHAR_Gloomrot_SentryOfficer",
				new PrefabGUID(1401026468)
			},
			{
				"CHAR_Gloomrot_SentryOfficer_Servant",
				new PrefabGUID(-1213645419)
			},
			{
				"CHAR_Gloomrot_SentryTurret",
				new PrefabGUID(-1082044089)
			},
			{
				"CHAR_Gloomrot_SpiderTank_Driller",
				new PrefabGUID(709450349)
			},
			{
				"CHAR_Gloomrot_SpiderTank_Gattler",
				new PrefabGUID(-884401089)
			},
			{
				"CHAR_Gloomrot_SpiderTank_LightningRod",
				new PrefabGUID(1655577903)
			},
			{
				"CHAR_Gloomrot_SpiderTank_Zapper",
				new PrefabGUID(-2018710724)
			},
			{
				"CHAR_Gloomrot_Tazer",
				new PrefabGUID(674807351)
			},
			{
				"CHAR_Gloomrot_Tazer_Servant",
				new PrefabGUID(-924080115)
			},
			{
				"CHAR_Gloomrot_Technician",
				new PrefabGUID(820492683)
			},
			{
				"CHAR_Gloomrot_Technician_Labworker",
				new PrefabGUID(-825299465)
			},
			{
				"CHAR_Gloomrot_Technician_Labworker_Servant",
				new PrefabGUID(-1034892278)
			},
			{
				"CHAR_Gloomrot_Technician_Servant",
				new PrefabGUID(-775762125)
			},
			{
				"CHAR_Gloomrot_TheProfessor_VBlood",
				new PrefabGUID(814083983)
			},
			{
				"CHAR_Gloomrot_TractorBeamer",
				new PrefabGUID(-293507834)
			},
			{
				"CHAR_Gloomrot_TractorBeamer_Servant",
				new PrefabGUID(565869317)
			},
			{
				"CHAR_Gloomrot_Villager_Female",
				new PrefabGUID(1216169364)
			},
			{
				"CHAR_Gloomrot_Villager_Female_Servant",
				new PrefabGUID(-1192403515)
			},
			{
				"CHAR_Gloomrot_Villager_Male",
				new PrefabGUID(-732208863)
			},
			{
				"CHAR_Gloomrot_Villager_Male_Servant",
				new PrefabGUID(-2085282780)
			},
			{
				"CHAR_Gloomrot_Voltage_VBlood",
				new PrefabGUID(-1101874342)
			},
			{
				"CHAR_Harpy_Dasher",
				new PrefabGUID(-1846851895)
			},
			{
				"CHAR_Harpy_Dasher_SUMMON",
				new PrefabGUID(1635780151)
			},
			{
				"CHAR_Harpy_FeatherDuster",
				new PrefabGUID(-1407234470)
			},
			{
				"CHAR_Harpy_Matriarch_VBlood",
				new PrefabGUID(685266977)
			},
			{
				"CHAR_Harpy_Scratcher",
				new PrefabGUID(1462269123)
			},
			{
				"CHAR_Harpy_Sorceress",
				new PrefabGUID(1224283123)
			},
			{
				"CHAR_IceElemental",
				new PrefabGUID(302393064)
			},
			{
				"CHAR_Illusion_Mosquito",
				new PrefabGUID(-303396552)
			},
			{
				"CHAR_IronGolem",
				new PrefabGUID(763796308)
			},
			{
				"CHAR_Manticore_HomePos",
				new PrefabGUID(980068444)
			},
			{
				"CHAR_Manticore_VBlood",
				new PrefabGUID(-393555055)
			},
			{
				"CHAR_Mantrap_Dull",
				new PrefabGUID(-878541676)
			},
			{
				"CHAR_Mantrap_Nest",
				new PrefabGUID(2016963774)
			},
			{
				"CHAR_Mantrap_Standard",
				new PrefabGUID(173817657)
			},
			{
				"CHAR_Militia_BellRinger",
				new PrefabGUID(-1670130821)
			},
			{
				"CHAR_Militia_BellRinger_Servant",
				new PrefabGUID(-1433235567)
			},
			{
				"CHAR_Militia_BishopOfDunley_VBlood",
				new PrefabGUID(-680831417)
			},
			{
				"CHAR_Militia_Bomber",
				new PrefabGUID(847893333)
			},
			{
				"CHAR_Militia_Bomber_Servant",
				new PrefabGUID(232701971)
			},
			{
				"CHAR_Militia_ConstrainingPole",
				new PrefabGUID(85290673)
			},
			{
				"CHAR_Militia_Crossbow",
				new PrefabGUID(956965183)
			},
			{
				"CHAR_Militia_Crossbow_Servant",
				new PrefabGUID(1481842114)
			},
			{
				"CHAR_Militia_Crossbow_Summon",
				new PrefabGUID(2036785949)
			},
			{
				"CHAR_Militia_Devoted",
				new PrefabGUID(1660801216)
			},
			{
				"CHAR_Militia_Devoted_Servant",
				new PrefabGUID(-823557242)
			},
			{
				"CHAR_Militia_EyeOfGod",
				new PrefabGUID(-1254618756)
			},
			{
				"CHAR_Militia_Glassblower_VBlood",
				new PrefabGUID(910988233)
			},
			{
				"CHAR_Militia_Guard",
				new PrefabGUID(1730498275)
			},
			{
				"CHAR_Militia_Guard_Servant",
				new PrefabGUID(-1447279513)
			},
			{
				"CHAR_Militia_Guard_Summon",
				new PrefabGUID(1050151632)
			},
			{
				"CHAR_Militia_Guard_VBlood",
				new PrefabGUID(-29797003)
			},
			{
				"CHAR_Militia_Heavy",
				new PrefabGUID(2005508157)
			},
			{
				"CHAR_Militia_Heavy_Servant",
				new PrefabGUID(-1773935659)
			},
			{
				"CHAR_Militia_Hound",
				new PrefabGUID(-249647316)
			},
			{
				"CHAR_Militia_Hound_VBlood",
				new PrefabGUID(-1373413273)
			},
			{
				"CHAR_Militia_HoundMaster_VBlood",
				new PrefabGUID(-784265984)
			},
			{
				"CHAR_Militia_InkCrawler",
				new PrefabGUID(2090982759)
			},
			{
				"CHAR_Militia_Leader_VBlood",
				new PrefabGUID(1688478381)
			},
			{
				"CHAR_Militia_Light",
				new PrefabGUID(-63435588)
			},
			{
				"CHAR_Militia_Light_Servant",
				new PrefabGUID(169329980)
			},
			{
				"CHAR_Militia_Light_Summon",
				new PrefabGUID(1772451421)
			},
			{
				"CHAR_Militia_Longbowman",
				new PrefabGUID(203103783)
			},
			{
				"CHAR_Militia_Longbowman_LightArrow_Vblood",
				new PrefabGUID(850622034)
			},
			{
				"CHAR_Militia_Longbowman_Servant",
				new PrefabGUID(-242295780)
			},
			{
				"CHAR_Militia_Longbowman_Summon",
				new PrefabGUID(1083647444)
			},
			{
				"CHAR_Militia_Miner_Standard",
				new PrefabGUID(-1072754152)
			},
			{
				"CHAR_Militia_Miner_Standard_Servant",
				new PrefabGUID(-1363137425)
			},
			{
				"CHAR_Militia_Nun",
				new PrefabGUID(-700632469)
			},
			{
				"CHAR_Militia_Nun_VBlood",
				new PrefabGUID(-99012450)
			},
			{
				"CHAR_Militia_Scribe_VBlood",
				new PrefabGUID(1945956671)
			},
			{
				"CHAR_Militia_Torchbearer",
				new PrefabGUID(37713289)
			},
			{
				"CHAR_Militia_Torchbearer_Servant",
				new PrefabGUID(986768339)
			},
			{
				"CHAR_Militia_Undead_Infiltrator",
				new PrefabGUID(-614820237)
			},
			{
				"CHAR_Monster_LightningPillar",
				new PrefabGUID(-1977168943)
			},
			{
				"CHAR_Mount_Horse",
				new PrefabGUID(1149585723)
			},
			{
				"CHAR_Mount_Horse_Gloomrot",
				new PrefabGUID(1213710323)
			},
			{
				"CHAR_Mount_Horse_Spectral",
				new PrefabGUID(2022889449)
			},
			{
				"CHAR_Mount_Horse_Vampire",
				new PrefabGUID(-1502865710)
			},
			{
				"CHAR_Mutant_Bear_Standard",
				new PrefabGUID(1938756250)
			},
			{
				"CHAR_Mutant_FleshGolem",
				new PrefabGUID(823276204)
			},
			{
				"CHAR_Mutant_RatHorror",
				new PrefabGUID(-375581934)
			},
			{
				"CHAR_Mutant_Spitter",
				new PrefabGUID(1092792896)
			},
			{
				"CHAR_Mutant_Wolf",
				new PrefabGUID(572729167)
			},
			{
				"CHAR_NecromancyDagger_SkeletonBerserker_Armored_Farbane",
				new PrefabGUID(-825517671)
			},
			{
				"CHAR_Paladin_DivineAngel",
				new PrefabGUID(-1737346940)
			},
			{
				"CHAR_Paladin_FallenAngel",
				new PrefabGUID(-76116724)
			},
			{
				"CHAR_Pixie",
				new PrefabGUID(1434914085)
			},
			{
				"CHAR_Poloma_VBlood",
				new PrefabGUID(-484556888)
			},
			{
				"CHAR_RockElemental",
				new PrefabGUID(20817667)
			},
			{
				"CHAR_Scarecrow",
				new PrefabGUID(-1750347680)
			},
			{
				"CHAR_Spectral_Guardian",
				new PrefabGUID(304726480)
			},
			{
				"CHAR_Spectral_SpellSlinger",
				new PrefabGUID(2065149172)
			},
			{
				"CHAR_Spider_Baneling",
				new PrefabGUID(-764515001)
			},
			{
				"CHAR_Spider_Baneling_Summon",
				new PrefabGUID(-1004061470)
			},
			{
				"CHAR_Spider_Broodmother",
				new PrefabGUID(342127250)
			},
			{
				"CHAR_Spider_Forest",
				new PrefabGUID(-581295882)
			},
			{
				"CHAR_Spider_Forestling",
				new PrefabGUID(574276383)
			},
			{
				"CHAR_Spider_Melee",
				new PrefabGUID(2136899683)
			},
			{
				"CHAR_Spider_Melee_Summon",
				new PrefabGUID(2119230788)
			},
			{
				"CHAR_Spider_Queen_VBlood",
				new PrefabGUID(-548489519)
			},
			{
				"CHAR_Spider_Range",
				new PrefabGUID(2103131615)
			},
			{
				"CHAR_Spider_Range_Summon",
				new PrefabGUID(1974733695)
			},
			{
				"CHAR_Spider_Spiderling",
				new PrefabGUID(1078424589)
			},
			{
				"CHAR_Spider_Spiderling_VerminNest",
				new PrefabGUID(1767714956)
			},
			{
				"CHAR_Spiderling_Summon",
				new PrefabGUID(-18289884)
			},
			{
				"CHAR_StoneGolem",
				new PrefabGUID(-779411607)
			},
			{
				"CHAR_SUMMON_Wolf",
				new PrefabGUID(1825512527)
			},
			{
				"CHAR_TargetDummy_Footman",
				new PrefabGUID(1479720323)
			},
			{
				"CHAR_Trader_Dunley_Gems_T02",
				new PrefabGUID(194933933)
			},
			{
				"CHAR_Trader_Dunley_Herbs_T02",
				new PrefabGUID(233171451)
			},
			{
				"CHAR_Trader_Dunley_Knowledge_T02",
				new PrefabGUID(281572043)
			},
			{
				"CHAR_Trader_Dunley_RareGoods_T02",
				new PrefabGUID(-1594911649)
			},
			{
				"CHAR_Trader_Farbane_Gems_T01",
				new PrefabGUID(-1168705805)
			},
			{
				"CHAR_Trader_Farbane_Herbs_T01",
				new PrefabGUID(-375258845)
			},
			{
				"CHAR_Trader_Farbane_Knowledge_T01",
				new PrefabGUID(-208499374)
			},
			{
				"CHAR_Trader_Farbane_RareGoods_T01",
				new PrefabGUID(-1810631919)
			},
			{
				"CHAR_Trader_Legendary_T04",
				new PrefabGUID(-1292194494)
			},
			{
				"CHAR_Trader_Silverlight_Gems_T03",
				new PrefabGUID(-1990875761)
			},
			{
				"CHAR_Trader_Silverlight_Herbs_T03",
				new PrefabGUID(1687896942)
			},
			{
				"CHAR_Trader_Silverlight_Knowledge_T03",
				new PrefabGUID(-915182578)
			},
			{
				"CHAR_Trader_Silverlight_RareGoods_T03",
				new PrefabGUID(739223277)
			},
			{
				"CHAR_Treant",
				new PrefabGUID(-1089337069)
			},
			{
				"CHAR_Undead_ArmoredSkeletonCrossbow_Dunley",
				new PrefabGUID(-861407720)
			},
			{
				"CHAR_Undead_ArmoredSkeletonCrossbow_Farbane",
				new PrefabGUID(-195077008)
			},
			{
				"CHAR_Undead_Assassin",
				new PrefabGUID(-1365627158)
			},
			{
				"CHAR_Undead_BishopOfDeath_VBlood",
				new PrefabGUID(577478542)
			},
			{
				"CHAR_Undead_BishopOfShadows_VBlood",
				new PrefabGUID(939467639)
			},
			{
				"CHAR_Undead_CursedSmith_FloatingWeapon_Base",
				new PrefabGUID(-1099451233)
			},
			{
				"CHAR_Undead_CursedSmith_FloatingWeapon_Mace",
				new PrefabGUID(-55245645)
			},
			{
				"CHAR_Undead_CursedSmith_FloatingWeapon_Slashers",
				new PrefabGUID(769910415)
			},
			{
				"CHAR_Undead_CursedSmith_FloatingWeapon_Spear",
				new PrefabGUID(233127264)
			},
			{
				"CHAR_Undead_CursedSmith_FloatingWeapon_Sword",
				new PrefabGUID(-2020619708)
			},
			{
				"CHAR_Undead_CursedSmith_VBlood",
				new PrefabGUID(326378955)
			},
			{
				"CHAR_Undead_FlyingSkull",
				new PrefabGUID(-236166535)
			},
			{
				"CHAR_Undead_GhostAssassin",
				new PrefabGUID(849891426)
			},
			{
				"CHAR_Undead_GhostBanshee",
				new PrefabGUID(-1146194149)
			},
			{
				"CHAR_Undead_GhostBanshee_TombSummon",
				new PrefabGUID(414648299)
			},
			{
				"CHAR_Undead_GhostGuardian",
				new PrefabGUID(-458883491)
			},
			{
				"CHAR_Undead_GhostMilitia_Crossbow",
				new PrefabGUID(-85729652)
			},
			{
				"CHAR_Undead_GhostMilitia_Crossbow_Summon",
				new PrefabGUID(348038236)
			},
			{
				"CHAR_Undead_GhostMilitia_Light",
				new PrefabGUID(-1618703048)
			},
			{
				"CHAR_Undead_GhostMilitia_Light_Summon",
				new PrefabGUID(1684831595)
			},
			{
				"CHAR_Undead_Ghoul_Armored_Farmlands",
				new PrefabGUID(2105565286)
			},
			{
				"CHAR_Undead_Ghoul_TombSummon",
				new PrefabGUID(937597711)
			},
			{
				"CHAR_Undead_Guardian",
				new PrefabGUID(-1967480038)
			},
			{
				"CHAR_Undead_Infiltrator_AfterShadow",
				new PrefabGUID(-558928562)
			},
			{
				"CHAR_Undead_Infiltrator_VBlood",
				new PrefabGUID(613251918)
			},
			{
				"CHAR_Undead_Leader_Vblood",
				new PrefabGUID(-1365931036)
			},
			{
				"CHAR_Undead_Necromancer",
				new PrefabGUID(-572568236)
			},
			{
				"CHAR_Undead_Necromancer_TombSummon",
				new PrefabGUID(2025660438)
			},
			{
				"CHAR_Undead_Priest",
				new PrefabGUID(-1653554504)
			},
			{
				"CHAR_Undead_Priest_VBlood",
				new PrefabGUID(153390636)
			},
			{
				"CHAR_Undead_RottingGhoul",
				new PrefabGUID(-1722506709)
			},
			{
				"CHAR_Undead_ShadowSoldier",
				new PrefabGUID(678628353)
			},
			{
				"CHAR_Undead_SkeletonApprentice",
				new PrefabGUID(-1789347076)
			},
			{
				"CHAR_Undead_SkeletonCrossbow_Base",
				new PrefabGUID(597386568)
			},
			{
				"CHAR_Undead_SkeletonCrossbow_Farbane_OLD",
				new PrefabGUID(1250474035)
			},
			{
				"CHAR_Undead_SkeletonCrossbow_GolemMinion",
				new PrefabGUID(1706319681)
			},
			{
				"CHAR_Undead_SkeletonCrossbow_Graveyard",
				new PrefabGUID(1395549638)
			},
			{
				"CHAR_Undead_SkeletonGolem",
				new PrefabGUID(-1380216646)
			},
			{
				"CHAR_Undead_SkeletonMage",
				new PrefabGUID(-1287507270)
			},
			{
				"CHAR_Undead_SkeletonSoldier_Armored_Dunley",
				new PrefabGUID(952695804)
			},
			{
				"CHAR_Undead_SkeletonSoldier_Armored_Farbane",
				new PrefabGUID(-837329073)
			},
			{
				"CHAR_Undead_SkeletonSoldier_Base",
				new PrefabGUID(-603934060)
			},
			{
				"CHAR_Undead_SkeletonSoldier_GolemMinion",
				new PrefabGUID(343833814)
			},
			{
				"CHAR_Undead_SkeletonSoldier_Infiltrator",
				new PrefabGUID(-1642110920)
			},
			{
				"CHAR_Undead_SkeletonSoldier_TombSummon",
				new PrefabGUID(-259591573)
			},
			{
				"CHAR_Undead_SkeletonSoldier_Unholy_Minion",
				new PrefabGUID(-1779239433)
			},
			{
				"CHAR_Undead_SkeletonSoldier_Withered",
				new PrefabGUID(-1584807109)
			},
			{
				"CHAR_Undead_UndyingGhoul",
				new PrefabGUID(1640311129)
			},
			{
				"CHAR_Undead_ZealousCultist_Ghost",
				new PrefabGUID(128488545)
			},
			{
				"CHAR_Undead_ZealousCultist_VBlood",
				new PrefabGUID(-1208888966)
			},
			{
				"CHAR_Unholy_Baneling",
				new PrefabGUID(-1823987835)
			},
			{
				"CHAR_Unholy_DeathKnight",
				new PrefabGUID(1857865401)
			},
			{
				"CHAR_Unholy_FallenAngel",
				new PrefabGUID(-1928607398)
			},
			{
				"CHAR_Unholy_SkeletonApprentice_Summon",
				new PrefabGUID(722671522)
			},
			{
				"CHAR_Unholy_SkeletonWarrior_Summon",
				new PrefabGUID(1604500740)
			},
			{
				"CHAR_Vampire_Withered",
				new PrefabGUID(-1117581429)
			},
			{
				"CHAR_Vampire_WitheredBatMinion",
				new PrefabGUID(-989999571)
			},
			{
				"CHAR_VampireMale",
				new PrefabGUID(38526109)
			},
			{
				"CHAR_Vermin_DireRat_VBlood",
				new PrefabGUID(-2039908510)
			},
			{
				"CHAR_Vermin_GiantRat",
				new PrefabGUID(-1722278689)
			},
			{
				"CHAR_Vermin_WickedRat_Rare",
				new PrefabGUID(-19165577)
			},
			{
				"CHAR_VHunter_Jade_VBlood",
				new PrefabGUID(-1968372384)
			},
			{
				"CHAR_VHunter_Leader_VBlood",
				new PrefabGUID(-1449631170)
			},
			{
				"CHAR_Villager_CursedWanderer_VBlood",
				new PrefabGUID(109969450)
			},
			{
				"CHAR_Villager_Tailor_VBlood",
				new PrefabGUID(-1942352521)
			},
			{
				"CHAR_Wendigo_VBlood",
				new PrefabGUID(24378719)
			},
			{
				"CHAR_Werewolf",
				new PrefabGUID(-1554760905)
			},
			{
				"CHAR_WerewolfChieftain_Human",
				new PrefabGUID(-1505705712)
			},
			{
				"CHAR_WerewolfChieftain_ShadowClone",
				new PrefabGUID(-1699898875)
			},
			{
				"CHAR_WerewolfChieftain_VBlood",
				new PrefabGUID(-1007062401)
			},
			{
				"CHAR_Winter_Bear_Standard",
				new PrefabGUID(2041915372)
			},
			{
				"CHAR_Winter_Moose",
				new PrefabGUID(-779632831)
			},
			{
				"CHAR_Winter_Wolf",
				new PrefabGUID(134039094)
			},
			{
				"CHAR_Winter_Yeti_VBlood",
				new PrefabGUID(-1347412392)
			}
		});

		// Token: 0x04000495 RID: 1173
		public static Dictionary<int, string> NameFromPrefab = Characters.Named.ToDictionary((KeyValuePair<string, PrefabGUID> x) => x.Value.GuidHash, (KeyValuePair<string, PrefabGUID> x) => x.Key);
	}
}
