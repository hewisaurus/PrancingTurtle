-- MySQL dump 10.13  Distrib 5.7.12, for Win64 (x86_64)
--
-- Host: 172.16.21.14    Database: PrancingTurtle
-- ------------------------------------------------------
-- Server version	5.5.5-10.1.26-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Dumping data for table `AbilityRole`
--

LOCK TABLES `AbilityRole` WRITE;
/*!40000 ALTER TABLE `AbilityRole` DISABLE KEYS */;
INSERT INTO `AbilityRole` VALUES (2,1816129489,'Shrapnel Bomb','Saboteur',1,3),(4,716211738,'Caltrop Charge','Saboteur',1,3),(5,-65335436,'Cadenza','Bard',3,3),(7,-3372885,'Verse of Agony','Bard',3,3),(8,-76157891,'Puncture','Assassin',1,3),(9,1508652891,'Serrated Blades','Assassin',1,3),(10,522887827,'Twilight Force','Nightblade',1,3),(11,-84779344,'Death from the Shadows','Nightblade',1,3),(12,1621415636,'Causal Treatment','Physician',2,3),(13,913581242,'Simultaneous Treatment','Physician',2,3),(14,-67659735,'Power Core','Tactician',2,3),(15,-60891805,'Curative Blast','Tactician',2,3),(17,147622222,'Guarded Steel','Riftstalker',4,3),(18,777208711,'Splinter Shot','Ranger',1,3),(19,280691459,'Quick Shot','Ranger',1,3),(20,298432740,'Swift Shot','Marksman',1,3),(21,1900535668,'Silver Tip Munitions','Marksman',1,3),(22,-74491293,'Hundred Blades','Bladedancer',1,3),(23,93296395,'Keen Strike','Bladedancer',1,3),(24,-520878,'Scourge','Inquisitor',1,1),(25,1244719305,'Bolt of Depravity','Inquisitor',1,1),(26,-41217628,'Siphon Vitality','Defiler',1,1),(27,-77294840,'Foul Growth','Defiler',1,1),(28,-82235525,'Symbol of the Sun','Purifier',2,1),(29,1773854348,'Ward of Flame','Purifier',2,1),(30,2037559528,'Strike of Judgment','Justicar',4,1),(31,-43974202,'Hammer of Duty','Justicar',4,1),(32,120311091,'Frozen Wrath','Shaman',1,1),(33,1592331833,'Massive Blow','Shaman',1,1),(34,1051526167,'Raging Storm','Stormcaller',1,2),(35,775964852,'Icicle','Stormcaller',1,2),(36,753806880,'Reaping Harvest','Paragon',1,4),(37,432347214,'Rising Waterfall','Paragon',1,4),(38,125069375,'Volcanic Bomb','Archon',3,2),(40,1675671410,'Earthen Barrage','Archon',3,2),(41,-65009691,'Cutting Slash','Beastmaster',3,4),(42,1695164184,'Flesh Rip','Beastmaster',3,4),(43,-63848263,'Lifegiving Veil','Chloromancer',2,2),(44,140899038,'Lifebound Veil','Chloromancer',2,2),(46,327693484,'Void Life','Chloromancer',2,2),(47,463405171,'Mass Casualty Response','Liberator',2,4),(48,648535309,'Group Assistance','Liberator',2,4),(49,187176626,'Viral Stream','Reaver',1,4),(50,-69883250,'Dire Blow','Reaver',1,4),(51,1973816291,'Reckless Strike','Void Knight',4,4),(52,-24490029,'Devouring Blow','Void Knight',4,4),(53,721551111,'Shattered Reflection','Arbiter',4,2),(54,1871751762,'Galvanic Strike','Arbiter',4,2),(55,138235363,'Glacial Insignia','Oracle',3,1),(56,2024510576,'Tainted Emblems','Oracle',3,1),(57,-75936232,'Charged Pulse','Tempest',1,4),(58,-65901037,'Double Pulse','Tempest',1,4),(59,1588471451,'Fireball','Pyromancer',1,2),(60,33805137,'Cinder Burst','Pyromancer',1,2),(61,1853130752,'Cornered Beast','Champion',1,4),(62,1310074166,'Titan\'s Strike','Champion',1,4),(63,1889768018,'Pacifying Strike','Paladin',4,4),(64,-36055070,'Light\'s Benediction','Paladin',4,4),(65,-51332742,'Pool of Restoration','Warden',2,1),(66,740001041,'Healing Flood','Warden',2,1),(67,489397491,'Healing Invocation','Sentinel',2,1),(68,1778238839,'Healing Breath','Sentinel',2,1),(69,71003279,'Tyranny of Death','Cabalist',1,1),(70,230857555,'Curse of Anarchy','Cabalist',1,1),(71,-59428155,'Fae Mimicry','Druid',1,1),(72,1673825006,'Combined Effort','Druid',1,1),(73,-55162170,'Piercing Thrust','Warlord',1,4),(74,-72730493,'King of the Hill','Warlord',1,4),(75,1980969236,'Rift Strike','Riftblade',1,4),(76,115904938,'Flamespear','Riftblade',1,4),(77,324479987,'Plague Bolt','Necromancer',1,2),(78,1206347165,'Necrosis','Necromancer',1,2),(79,646858846,'Dark Touch','Warlock',1,2),(80,726155736,'Defile','Warlock',1,2),(81,103100092,'Crystalline Missiles','Elementalist',1,2),(82,509409020,'Lightning Strike','Elementalist',1,2),(83,1575003627,'Haunting Pain','Dominator',1,2),(84,400474527,'Mana Wrench','Dominator',1,2),(85,-66963788,'Lucent Slash','Harbinger',1,2),(86,-19153798,'Storm\'s Fury','Harbinger',1,2),(88,1903703850,'Hailstorm','Stormcaller',1,2),(91,456744392,'Flourish','Chloromancer',2,2),(94,1351023610,'Bloom','Chloromancer',2,2),(95,-79401539,'Infernal Torrent','Tactician',1,3),(97,-65354533,'Reckoning','Justicar',4,1),(99,479185146,'Curse of Anarchy','Cabalist',1,1),(102,-7363976,'Well of Souls','Cabalist',1,1),(104,-50511165,'Planar Spout','Berserker',1,5),(107,73694550,'Ethereal Corruption','Berserker',1,5),(110,567842125,'Icy Cleave','Berserker',1,5),(113,554245225,'Air Cutter','Dervish',1,5),(116,1359367436,'Whirling Dervish','Dervish',1,5),(119,416085614,'Savage Twister','Dervish',1,5),(122,863725018,'Rippling Ether','Typhoon',1,5),(125,2018384954,'Hurricane','Typhoon',1,5),(128,1445013743,'Cloudburst','Typhoon',1,5),(131,532566663,'Precision Bolt','Vulcanist',1,5),(134,219739954,'Skill Shot','Vulcanist',1,5),(137,221254520,'Vorpal Salvo','Vulcanist',1,5),(140,447478480,'Earthquake','Titan',4,5),(143,808942338,'Creeping Vines','Titan',4,5),(146,148960385,'Upheaval','Titan',4,5),(149,34570762,'Serenity','Preserver',2,5),(152,819089832,'Nurture','Preserver',2,5),(155,1941821258,'Inundate','Preserver',2,5),(158,875787762,'Planar Vortex','Riftstalker',4,3),(161,-90294916,'Power of the Planes','Riftstalker',4,3),(164,950094923,'Rift Disturbance','Riftstalker',4,3),(167,-83987170,'Wrath of the Planes','Riftstalker',4,3),(170,1292238302,'Rift Barrier','Riftstalker',4,3),(173,-33845295,'Flame Volley','Pyromancer',1,2),(176,-13369196,'Malicious Poison','Assassin',1,3),(179,118334667,'Virulent Poison','Assassin',1,3),(182,206273970,'Nysyr\'s Rebuke','Inquisitor',1,1),(183,676876950,'Vaporize','Maelstrom',1,5),(186,651084970,'Scald','Maelstrom',1,5),(189,1489968671,'Excoriate','Maelstrom',1,5),(192,-36318438,'Rune of Castigation','Runeshaper',1,1),(195,165459395,'Rune of Smiting','Runeshaper',1,1),(198,861676331,'Rune of Soul Binding','Runeshaper',1,1),(199,269407301,'Essence Strike','Titan',4,5),(201,2019895836,'Urgent Care','Physician',2,3),(203,1689666296,'Urgent Care','Physician',2,3),(205,773681108,'Maintenance Therapy','Physician',2,3),(207,802497716,'Maintenance Therapy','Physician',2,3),(209,329052316,'Maintenance Therapy','Physician',2,3),(211,1889486536,'Group Therapy','Physician',2,3),(213,1621415636,'Causal Treatment','Physician',2,3),(215,1137809741,'Causal Treatment','Physician',2,3),(217,-38581808,'Active Treatment','Physician',2,3),(218,732148812,'Fury Blast','Vulcanist',1,5),(219,2032225032,'Molten Wave','Vulcanist',1,5),(220,-58177305,'Bear\'s Fury','Vulcanist',1,5),(221,1659673058,'Hammer of Faith','Justicar',4,1),(222,1531469073,'Even Justice','Justicar',4,1),(223,-1029779,'Bolt of Radiance','Justicar',4,1),(224,436858804,'Bolt of Radiance','Justicar',4,1),(225,-73435607,'Atrophy','Warlock',1,2),(226,1282932639,'Defile','Warlock',1,2);
/*!40000 ALTER TABLE `AbilityRole` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `BossFight`
--

LOCK TABLES `BossFight` WRITE;
/*!40000 ALTER TABLE `BossFight` DISABLE KEYS */;
INSERT INTO `BossFight` VALUES (19,'Skelf Brothers',11,0,1,NULL,0,38000000,'Hidrac'),(20,'Ungolok',11,0,0,NULL,0,95100000,NULL),(21,'Drekanoth of Fate',11,0,0,NULL,0,120000000,'Drekanoth of Fate,Drekanoth du Destin,Drekanoth des Schicksals'),(23,'Finric',11,0,0,NULL,0,150000000,NULL),(24,'Bulf',12,0,1,NULL,0,76000000,NULL),(25,'Jinoscoth',12,0,1,NULL,0,219400000,NULL),(27,'Threngar',12,0,0,NULL,0,530000000,NULL),(28,'Izkinra',12,0,1,NULL,0,119200000,'Warmaster Shaddoth,Kriegsmeister Shaddoth,Maître de guerre Shaddoth'),(29,'The Yrlwalach',12,0,0,NULL,0,320000000,'The Yrlwalach,L\'Yrlwalach,Der Yrlwalach'),(30,'Johan',13,0,0,NULL,0,257300000,'Johan,Johann'),(31,'P.U.M.P.K.I.N.',13,0,1,NULL,0,297000000,'P.U.M.P.K.I.N.,PATATOR,GOLDJUNGE'),(32,'Crucia',13,0,1,'Resistance,Widerstand',1,320000000,NULL),(33,'Raid Boss Practice Dummy',14,0,0,NULL,0,0,NULL),(34,'Expert Boss Practice Dummy',14,0,0,NULL,0,0,NULL),(35,'Normal Practice Dummy',14,0,0,NULL,0,0,NULL),(37,'Murdantix',15,0,0,NULL,1,490300000,NULL),(38,'Matron Zamira',15,0,0,NULL,1,414600000,'Matron Zamira,Matrone Zamira'),(39,'Sicaron',15,0,0,NULL,1,1360000000,NULL),(40,'Rune King Molinar',15,0,1,NULL,1,311900000,'Rune King Molinar,Roi runique Molinar,Runenkönig Molinar'),(41,'Estrode',15,0,0,NULL,1,515000000,NULL),(42,'Vladmal Prime',15,0,0,NULL,1,340000000,NULL),(43,'Grugonim',15,0,0,NULL,1,968000000,NULL),(44,'Inquisitor Garau',15,0,0,NULL,1,366000000,'Inquisitor Garau,Inquisiteur Garau'),(45,'Inwar Darktide',15,0,1,NULL,1,172000000,'Inwar Darktide,Inwar Noirflux,Inwar Dunkelflut'),(46,'Akylios',15,0,1,NULL,1,315000000,NULL),(47,'Soulrender Zilas',15,0,0,NULL,1,383000000,'Soulrender Zilas,Étripeur d\'âmes Zilas,Seelenreißer Zilas'),(49,'Raid-Ready Practice Dummy - 52',14,0,0,NULL,0,0,NULL),(52,'Murdantix',16,0,0,NULL,0,0,NULL),(55,'Matron Zamira',16,0,0,NULL,0,0,NULL),(58,'Sicaron',16,0,0,NULL,0,0,NULL),(61,'Rune King Molinar',16,0,0,NULL,0,0,NULL),(64,'Estrode',16,0,0,NULL,0,0,NULL),(67,'Vladmal Prime',16,0,0,NULL,0,0,NULL),(70,'Grugonim',16,0,0,NULL,0,0,NULL),(73,'Inquisitor Garau',16,0,0,NULL,0,0,NULL),(76,'Inwar Darktide',16,0,0,NULL,0,0,NULL),(79,'Akylios',16,0,0,NULL,0,0,NULL),(82,'Soulrender Zilas',16,0,0,NULL,0,0,NULL),(85,'Expert Boss Practice Dummy: Level 67',14,0,0,NULL,0,0,NULL),(86,'Anrak the Foul',17,0,1,NULL,0,0,'Anrak the Foul,Anrak l\'ignoble'),(89,'Thalguur',17,0,0,NULL,0,0,NULL),(92,'Guurloth',17,0,0,NULL,0,0,NULL),(95,'Uruluuk',17,0,0,NULL,0,0,NULL),(98,'Dungeon Practice Dummy',14,0,0,NULL,0,0,NULL),(101,'Pagura',20,0,1,NULL,0,0,NULL),(104,'Lord Arak',20,0,0,NULL,0,0,NULL),(107,'Fauxmire',20,0,0,NULL,0,0,NULL),(110,'The Enigma',20,0,0,NULL,0,0,NULL),(113,'Lady Envy',20,0,0,NULL,0,0,NULL),(116,'Dark Genesis',20,0,0,NULL,0,0,NULL),(119,'Pillars of Justice',20,0,0,NULL,0,0,NULL),(122,'Lady Justice',20,0,0,NULL,0,0,NULL),(125,'The Arisen Arak',20,0,0,NULL,0,0,NULL),(128,'Pure Evil',20,0,0,NULL,0,0,NULL),(129,'Duke Eblius',21,0,0,NULL,0,0,NULL),(132,'Hericius',21,0,0,NULL,0,0,NULL),(135,'Azaphrentus',21,0,0,NULL,0,0,NULL),(138,'Fyragnos',21,0,0,NULL,0,0,NULL),(141,'Nightstalker Caelon',21,0,0,NULL,0,0,NULL),(144,'Lord Fionn',21,0,1,NULL,0,0,NULL),(145,'Beruhast',22,0,0,NULL,0,0,NULL),(147,'Ereandorn',22,0,0,NULL,0,0,NULL),(149,'General Silgen',22,0,0,NULL,0,0,NULL),(151,'High Priest Arakhurn',22,0,0,NULL,0,0,NULL),(153,'Seething Core',22,0,0,NULL,0,0,NULL),(155,'Beligosh',23,0,0,NULL,0,0,NULL),(157,'TarJulia',23,0,0,NULL,0,0,NULL),(159,'Council of Fate',23,0,1,NULL,0,0,NULL),(161,'Malannon',23,0,0,NULL,0,0,NULL);
/*!40000 ALTER TABLE `BossFight` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `BossFightDifficulty`
--

LOCK TABLES `BossFightDifficulty` WRITE;
/*!40000 ALTER TABLE `BossFightDifficulty` DISABLE KEYS */;
INSERT INTO `BossFightDifficulty` VALUES (1,86,1,58000000,'Anrak the Foul,Anrak l\'ignoble,Anrak der Üble'),(4,86,4,111000000,'Anrak the Foul,Anrak l\'ignoble,Anrak der Üble'),(7,86,7,139120000,'Anrak the Foul,Anrak l\'ignoble,Anrak der Üble'),(10,92,1,86580000,'Guurloth'),(13,92,4,115440000,'Guurloth'),(16,92,7,145780000,'Guurloth'),(19,89,1,148000000,'Thalguur'),(22,89,4,197580000,'Thalguur'),(25,89,7,218670000,'Thalguur'),(28,95,1,130980000,'Uruluuk'),(31,95,4,174344000,'Uruluuk'),(34,95,7,295500000,'Uruluuk'),(35,20,1,76000000,'Ungolok'),(38,20,4,95000000,'Ungolok'),(41,20,7,118700000,'Ungolok'),(44,19,1,27000000,'Hidrac'),(47,19,4,32600000,'Hidrac'),(50,19,7,41900000,'Hidrac'),(53,23,1,108000000,'Finric'),(56,23,4,142000000,'Finric'),(59,23,7,168000000,'Finric'),(62,21,1,86400000,'Drekanoth of Fate,Drekanoth du Destin,Drekanoth des Schicksals'),(65,21,4,104000000,'Drekanoth of Fate,Drekanoth du Destin,Drekanoth des Schicksals'),(68,21,7,135000000,'Drekanoth of Fate,Drekanoth du Destin,Drekanoth des Schicksals'),(71,24,4,76000000,'Bulf'),(74,25,4,199000000,'Jinoscoth'),(77,27,4,475000000,'Threngar'),(80,28,4,107000000,'Warmaster Shaddoth,Kriegsmeister Shaddoth,Maître de guerre Shaddoth'),(83,29,4,317000000,'The Yrlwalach,L\'Yrlwalach,Der Yrlwalach'),(86,37,4,490300000,'Murdantix'),(89,38,4,414600000,'Matron Zamira,Matrone Zamira'),(92,39,4,1360000000,'Sicaron'),(95,40,4,311900000,'Rune King Molinar,Roi runique Molinar,Runenkönig Molinar'),(98,41,4,515000000,'Estrode'),(101,42,4,340000000,'Vladmal Prime'),(104,43,4,968000000,'Grugonim'),(107,44,4,366000000,'Inquisitor Garau,Inquisiteur Garau'),(110,45,4,172000000,'Inwar Darktide,Inwar Noirflux,Inwar Dunkelflut'),(113,46,4,315000000,'Akylios'),(116,47,4,383000000,'Soulrender Zilas,Étripeur d\'âmes Zilas,Seelenreißer Zilas'),(119,101,4,193000000,'Pagura'),(122,104,4,340000000,'Lord Arak,Seigneur Arak,Fürst Arak'),(125,107,4,0,'Fauxmire,Falsimir'),(128,110,4,0,'The Enigma'),(131,113,4,0,'Lady Envy,Dame Enie,Lady Neid'),(134,116,4,0,'Dark Genesis'),(137,119,4,0,'Vis'),(140,122,4,695000000,'Lady Justice,Dame Justice,Justitia'),(143,125,4,0,'Arisen Arak'),(146,128,4,0,'Pure Evil,Mal incarné,Das Pure Böse'),(149,122,7,868000000,'Lady Justice,Dame Justice,Justitia'),(150,129,4,65800000,'Duke Eblius,Duc Eblius,Herzog Eblius'),(153,132,4,161980000,'Hericius'),(156,135,4,123550000,'Azaphrentus'),(159,138,4,111300000,'Fyragnos'),(162,141,4,67900000,'Nightstalker Caelon,Guetteur nocturne Caelon,Nachtpirscher Caelon'),(165,144,4,134680000,'Lord Fionn,Seigneur Fibon,Fürst Fionn'),(168,129,1,32900000,'Duke Eblius,Duc Eblius,Herzog Eblius'),(171,129,7,86100000,'Duke Eblius,Duc Eblius,Herzog Eblius'),(174,132,1,80850000,'Hericius'),(177,132,7,210700000,'Hericius'),(180,135,1,80850000,'Azaphrentus'),(183,135,7,161980000,'Azaphrentus'),(186,138,1,55300000,'Fyragnos'),(189,138,7,145040000,'Fyragnos'),(192,141,1,33950000,'Nightstalker Caelon,Guetteur nocturne Caelon,Nachtpirscher Caelon'),(195,141,7,88410000,'Nightstalker Caelon,Guetteur nocturne Caelon,Nachtpirscher Caelon'),(198,144,1,69300000,'Lord Fionn,Seigneur Fibon,Fürst Fionn'),(201,144,7,176400000,'Lord Fionn,Seigneur Fibon,Fürst Fionn'),(203,147,4,1000000000,'Ereandorn'),(205,145,4,1000000000,'Beruhast'),(207,149,4,1050000000,'General Silgen'),(209,151,4,1000000000,'High Priest Arakhurn'),(211,153,4,625000000,'Seething Core'),(213,155,4,1130000000,'Beligosh'),(215,157,4,800000000,'TarJulia'),(217,159,4,316000000,'Count Pleuzhal'),(219,161,4,1800000000,'Malannon');
/*!40000 ALTER TABLE `BossFightDifficulty` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `BossFightSingleTargetDetail`
--

LOCK TABLES `BossFightSingleTargetDetail` WRITE;
/*!40000 ALTER TABLE `BossFightSingleTargetDetail` DISABLE KEYS */;
INSERT INTO `BossFightSingleTargetDetail` VALUES (1,37,'Murdantix'),(4,39,'Sicaron'),(7,41,'Estrode'),(13,20,'Ungolok'),(16,23,'Finric'),(19,25,'Jinoscoth'),(22,32,'Crucia'),(25,43,'Grugonim'),(28,38,'Matron Zamira,Matrone Zamira'),(31,21,'Drekanoth of Fate,Drekanoth du Destin,Drekanoth des Schicksals'),(34,29,'The Yrlwalach,L\'Yrlwalach,Der Yrlwalach'),(37,30,'Johan,Johann'),(40,86,'Anrak the Foul,Anrak l\'ignoble,Anrak der Ãœble'),(43,89,'Thalguur'),(46,92,'Guurloth'),(49,95,'Uruluuk'),(51,101,'Pagura'),(54,113,'Lady Envy'),(57,104,'Lord Arak'),(59,122,'Lady Justice,Dame Justice,Justitia'),(62,116,'Dark Genesis,Genèse sombre,Dunkler Ursprung');
/*!40000 ALTER TABLE `BossFightSingleTargetDetail` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `Instance`
--

LOCK TABLES `Instance` WRITE;
/*!40000 ALTER TABLE `Instance` DISABLE KEYS */;
INSERT INTO `Instance` VALUES (11,'The Rhen of Fate',10,1,1,1,'RoF',NULL,1,3,0),(12,'Mount Sharax',20,1,1,1,'MS',NULL,1,3,0),(13,'Tyrant\'s Forge',20,1,1,1,'TF',NULL,1,3,0),(14,'Practice Dummies',-1,1,0,1,'PD',NULL,1,1,0),(15,'Hammerknell Fortress',20,1,1,1,'HK',NULL,2,3,0),(16,'Hammerknell Fortress (Level 50)',20,0,0,0,'',NULL,1,1,0),(17,'Intrepid Gilded Prophecy',10,1,1,1,'IGP',NULL,2,3,0),(20,'Mind of Madness',20,1,1,1,'MoM','mom.jpg',3,3,1),(21,'Comet of Ahnket',10,1,1,1,'CoA','coa.jpg',3,3,1),(22,'Intrepid Rise Of The Phoenix',10,1,1,1,'iROTP',NULL,1,4,0),(23,'Tartaric Depths',10,1,1,1,'TD',NULL,1,4,0);
/*!40000 ALTER TABLE `Instance` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `PlayerClass`
--

LOCK TABLES `PlayerClass` WRITE;
/*!40000 ALTER TABLE `PlayerClass` DISABLE KEYS */;
INSERT INTO `PlayerClass` VALUES (1,'Cleric',NULL,NULL),(2,'Mage',NULL,NULL),(3,'Rogue',NULL,NULL),(4,'Warrior',NULL,NULL),(5,'Primalist',NULL,NULL);
/*!40000 ALTER TABLE `PlayerClass` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `RoleIcon`
--

LOCK TABLES `RoleIcon` WRITE;
/*!40000 ALTER TABLE `RoleIcon` DISABLE KEYS */;
INSERT INTO `RoleIcon` VALUES (1,'Damage','raid_icon_role_dps.png',4),(2,'Healing','raid_icon_role_heal.png',2),(3,'Support','raid_icon_role_support.png',3),(4,'Tank','raid_icon_role_tank.png',1);
/*!40000 ALTER TABLE `RoleIcon` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `Shard`
--

LOCK TABLES `Shard` WRITE;
/*!40000 ALTER TABLE `Shard` DISABLE KEYS */;
INSERT INTO `Shard` VALUES (1,'Deepwood','US','PvE'),(2,'Faeblight','US','PvE-RP'),(3,'Greybriar','US','PvE'),(4,'Hailol','US','PvE'),(5,'Laethys','US','PvE'),(6,'Seastone','US','PvP'),(7,'Wolfsbane','US','PvE'),(8,'BloodIron','EU','PvP'),(9,'Brisesol','EU','PvE'),(10,'Brutwacht','EU','PvE'),(11,'Gelidra','EU','PvE'),(12,'Typhiria','EU','PvE'),(13,'Zaviel','EU','PvE'),(157,'PTS','PTS','PvE');
/*!40000 ALTER TABLE `Shard` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2017-10-16 20:00:47
