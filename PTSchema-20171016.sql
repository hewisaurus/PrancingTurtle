CREATE DATABASE  IF NOT EXISTS `PrancingTurtle` /*!40100 DEFAULT CHARACTER SET latin1 */;
USE `PrancingTurtle`;
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
-- Table structure for table `Ability`
--

DROP TABLE IF EXISTS `Ability`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Ability` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SoulId` int(11) DEFAULT NULL,
  `AbilityId` bigint(20) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Icon` varchar(50) DEFAULT NULL,
  `RequiredLevel` int(11) DEFAULT NULL,
  `MinimumPointsInSoul` int(11) DEFAULT NULL,
  `AddonId` varchar(50) DEFAULT NULL,
  `Description` varchar(650) DEFAULT NULL,
  `DamageType` varchar(50) DEFAULT NULL,
  `RankNumber` int(11) DEFAULT NULL,
  `RequiresWeapon` varchar(50) DEFAULT NULL,
  `Cooldown` decimal(8,2) DEFAULT NULL,
  `CastTime` varchar(50) DEFAULT NULL,
  `Channel` tinyint(1) DEFAULT NULL,
  `Duration` decimal(8,2) DEFAULT NULL,
  `Interval` decimal(4,2) DEFAULT NULL,
  `MinRange` decimal(4,1) DEFAULT NULL,
  `MaxRange` decimal(4,1) DEFAULT NULL,
  `CostManaPercent` decimal(5,2) DEFAULT NULL,
  `CostEnergy` int(11) DEFAULT NULL,
  `MinimumHeat` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AbilityId` (`AbilityId`),
  KEY `IX_Ability_AbilityId` (`AbilityId`),
  KEY `IX_Ability_Name` (`Name`),
  KEY `FK_Ability_Soul` (`SoulId`),
  CONSTRAINT `FK_Ability_Soul` FOREIGN KEY (`SoulId`) REFERENCES `Soul` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=29345 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AbilityRole`
--

DROP TABLE IF EXISTS `AbilityRole`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AbilityRole` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AbilityLogId` bigint(20) NOT NULL,
  `AbilityName` varchar(50) NOT NULL,
  `Soul` varchar(50) NOT NULL,
  `RoleIconId` int(11) NOT NULL,
  `PlayerClassId` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_AbilityRole_RoleIcon` (`RoleIconId`),
  KEY `FK_AbilityRole_PlayerClass` (`PlayerClassId`),
  KEY `AbilityLogId` (`AbilityLogId`),
  CONSTRAINT `FK_AbilityRole_PlayerClass` FOREIGN KEY (`PlayerClassId`) REFERENCES `PlayerClass` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_AbilityRole_RoleIcon` FOREIGN KEY (`RoleIconId`) REFERENCES `RoleIcon` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=227 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ApiUsers`
--

DROP TABLE IF EXISTS `ApiUsers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ApiUsers` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Website` varchar(80) NOT NULL,
  `AuthKey` varchar(45) NOT NULL,
  `EmailAddress` varchar(45) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UQ_Email` (`EmailAddress`),
  UNIQUE KEY `UQ_AuthKey` (`AuthKey`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AuthUser`
--

DROP TABLE IF EXISTS `AuthUser`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AuthUser` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Email` varchar(255) NOT NULL,
  `PasswordHash` varchar(28) NOT NULL,
  `ExtraInformation1` varchar(33) DEFAULT NULL,
  `ExtraInformation2` varchar(50) NOT NULL,
  `ShortMenuFormat` tinyint(1) NOT NULL DEFAULT '1',
  `ShowGuildMenu` tinyint(1) NOT NULL DEFAULT '1',
  `AccessFailedCount` int(11) NOT NULL,
  `LockoutEnabled` tinyint(1) NOT NULL,
  `LockoutEndDate` datetime DEFAULT NULL,
  `EmailConfirmed` tinyint(1) NOT NULL,
  `EmailConfirmationToken` varchar(50) DEFAULT NULL,
  `PasswordResetToken` varchar(50) DEFAULT NULL,
  `PreviousLoginTime` datetime DEFAULT NULL,
  `LastLoggedIn` datetime DEFAULT NULL,
  `PreviousLoginAddress` varchar(50) DEFAULT NULL,
  `LastLoginAddress` varchar(50) DEFAULT NULL,
  `TimeZone` varchar(50) NOT NULL,
  `Created` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2372 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AuthUserCharacter`
--

DROP TABLE IF EXISTS `AuthUserCharacter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AuthUserCharacter` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AuthUserId` int(11) NOT NULL,
  `CharacterName` varchar(50) NOT NULL,
  `ShardId` int(11) NOT NULL,
  `GuildId` int(11) DEFAULT NULL,
  `GuildRankId` int(11) DEFAULT NULL,
  `Removed` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `FK_AuthUserCharacter_Guild` (`GuildId`),
  KEY `FK_AuthUserCharacter_GuildRank` (`GuildRankId`),
  KEY `FK_UserCharacter_Shard` (`ShardId`),
  KEY `FK_UserCharacter_AuthUser` (`AuthUserId`),
  CONSTRAINT `FK_AuthUserCharacter_Guild` FOREIGN KEY (`GuildId`) REFERENCES `Guild` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_AuthUserCharacter_GuildRank` FOREIGN KEY (`GuildRankId`) REFERENCES `GuildRank` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_UserCharacter_AuthUser` FOREIGN KEY (`AuthUserId`) REFERENCES `AuthUser` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_UserCharacter_Shard` FOREIGN KEY (`ShardId`) REFERENCES `Shard` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=3300 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AuthUserCharacterGuildApplication`
--

DROP TABLE IF EXISTS `AuthUserCharacterGuildApplication`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AuthUserCharacterGuildApplication` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AuthUserCharacterId` int(11) NOT NULL,
  `GuildId` int(11) NOT NULL,
  `Message` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_AuthUserCharacterGuildApplication_UserCharacter` (`AuthUserCharacterId`),
  KEY `FK_AuthUserCharacterGuildApplication_Guild` (`GuildId`),
  CONSTRAINT `FK_AuthUserCharacterGuildApplication_Guild` FOREIGN KEY (`GuildId`) REFERENCES `Guild` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_AuthUserCharacterGuildApplication_UserCharacter` FOREIGN KEY (`AuthUserCharacterId`) REFERENCES `AuthUserCharacter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=2082 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AutoParserAbilityChecking`
--

DROP TABLE IF EXISTS `AutoParserAbilityChecking`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AutoParserAbilityChecking` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AbilityId` bigint(20) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=105493 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AutoParserIgnoredEncounters`
--

DROP TABLE IF EXISTS `AutoParserIgnoredEncounters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AutoParserIgnoredEncounters` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=73 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AutoParserPlayerChecking`
--

DROP TABLE IF EXISTS `AutoParserPlayerChecking`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AutoParserPlayerChecking` (
  `Id` int(11) NOT NULL AUTO_INCREMENT COMMENT 'PK',
  `PlayerId` varchar(50) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2410 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `BossFight`
--

DROP TABLE IF EXISTS `BossFight`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `BossFight` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `InstanceId` int(11) NOT NULL,
  `DpsCheck` bigint(20) NOT NULL DEFAULT '0',
  `RequiresSpecialProcessing` tinyint(1) NOT NULL DEFAULT '0',
  `UniqueAbilityName` varchar(50) DEFAULT NULL,
  `PriorityIfDuplicate` tinyint(1) NOT NULL DEFAULT '0',
  `Hitpoints` bigint(20) NOT NULL DEFAULT '0',
  `HitpointTarget` varchar(150) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_Boss_Instance` (`InstanceId`),
  CONSTRAINT `FK_Boss_Instance` FOREIGN KEY (`InstanceId`) REFERENCES `Instance` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=163 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `BossFightDifficulty`
--

DROP TABLE IF EXISTS `BossFightDifficulty`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `BossFightDifficulty` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `BossFightId` int(11) NOT NULL,
  `EncounterDifficultyId` tinyint(3) NOT NULL,
  `OverrideHitpoints` bigint(20) NOT NULL,
  `OverrideHitpointTarget` varchar(150) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_BossFightDifficulty_BossFight_idx` (`BossFightId`),
  KEY `FK_BossFightDifficulty_EncounterDifficulty_idx` (`EncounterDifficultyId`),
  CONSTRAINT `FK_BossFightDifficulty_BossFight` FOREIGN KEY (`BossFightId`) REFERENCES `BossFight` (`Id`),
  CONSTRAINT `FK_BossFightDifficulty_EncounterDifficulty` FOREIGN KEY (`EncounterDifficultyId`) REFERENCES `EncounterDifficulty` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=220 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `BossFightSingleTargetDetail`
--

DROP TABLE IF EXISTS `BossFightSingleTargetDetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `BossFightSingleTargetDetail` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `BossFightId` int(11) NOT NULL,
  `TargetName` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UQ_BossFightId` (`BossFightId`),
  KEY `IX_TargetName` (`TargetName`),
  KEY `FK_BossFightSingleTargetDetail_BossFight_idx` (`BossFightId`),
  CONSTRAINT `FK_BossFightSingleTargetDetail_BossFight` FOREIGN KEY (`BossFightId`) REFERENCES `BossFight` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=63 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Buff`
--

DROP TABLE IF EXISTS `Buff`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Buff` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `BuffGroupId` int(11) NOT NULL,
  `IsPrimary` tinyint(1) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Icon` varchar(50) DEFAULT NULL,
  `ShowByDefault` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_Buff_BuffGroup` (`BuffGroupId`),
  CONSTRAINT `FK_Buff_BuffGroup` FOREIGN KEY (`BuffGroupId`) REFERENCES `BuffGroup` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `BuffDebuffGroup`
--

DROP TABLE IF EXISTS `BuffDebuffGroup`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `BuffDebuffGroup` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `GroupName` varchar(50) NOT NULL,
  `IsBuff` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UQ_BuffDebuffGroup_Name` (`GroupName`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `BuffDebuffStacking`
--

DROP TABLE IF EXISTS `BuffDebuffStacking`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `BuffDebuffStacking` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `BuffDebuffGroupId` int(11) NOT NULL,
  `AbilityId` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_BuffDebuffStacking_BuffDebuffGroup_idx` (`BuffDebuffGroupId`),
  KEY `FK_BuffDebuffStacking_Ability_idx` (`AbilityId`),
  CONSTRAINT `FK_BuffDebuffStacking_Ability` FOREIGN KEY (`AbilityId`) REFERENCES `Ability` (`Id`),
  CONSTRAINT `FK_BuffDebuffStacking_BuffDebuffGroup` FOREIGN KEY (`BuffDebuffGroupId`) REFERENCES `BuffDebuffGroup` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `BuffGroup`
--

DROP TABLE IF EXISTS `BuffGroup`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `BuffGroup` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `IsABuff` tinyint(1) NOT NULL,
  `TrackOnUI` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `DailyStats`
--

DROP TABLE IF EXISTS `DailyStats`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DailyStats` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Date` datetime NOT NULL,
  `DamageRecords` bigint(20) NOT NULL,
  `HealingRecords` bigint(20) NOT NULL,
  `ShieldingRecords` bigint(20) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UQ_DailyStats_Date` (`Date`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `DamageDone`
--

DROP TABLE IF EXISTS `DamageDone`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DamageDone` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `SourcePlayerId` int(11) DEFAULT NULL,
  `SourceNpcName` varchar(100) DEFAULT NULL,
  `SourceNpcId` varchar(100) DEFAULT NULL,
  `SourcePetName` varchar(100) DEFAULT NULL,
  `TargetPlayerId` int(11) DEFAULT NULL,
  `TargetNpcName` varchar(100) DEFAULT NULL,
  `TargetNpcId` varchar(100) DEFAULT NULL,
  `TargetPetName` varchar(100) DEFAULT NULL,
  `EncounterId` int(11) NOT NULL,
  `AbilityId` int(11) NOT NULL,
  `TotalDamage` bigint(20) NOT NULL,
  `EffectiveDamage` bigint(20) NOT NULL,
  `CriticalHit` tinyint(1) NOT NULL,
  `SecondsElapsed` int(11) NOT NULL,
  `OrderWithinSecond` int(11) NOT NULL,
  `BlockedAmount` bigint(20) NOT NULL,
  `AbsorbedAmount` bigint(20) NOT NULL,
  `DeflectedAmount` bigint(20) NOT NULL,
  `IgnoredAmount` bigint(20) NOT NULL,
  `InterceptedAmount` bigint(20) NOT NULL,
  `OverkillAmount` bigint(20) NOT NULL,
  `Dodged` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_DamageDone_EncounterId` (`EncounterId`),
  KEY `IX_DamageDone_SourcePlayerId` (`SourcePlayerId`),
  KEY `IX_DamageDone_TargetPlayerId` (`TargetPlayerId`),
  KEY `FK_DamageDone_Ability` (`AbilityId`),
  KEY `TargetNpcName` (`TargetNpcName`),
  CONSTRAINT `FK_DamageDone_Ability` FOREIGN KEY (`AbilityId`) REFERENCES `Ability` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_DamageDone_Encounter` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_DamageDone_SourcePlayer` FOREIGN KEY (`SourcePlayerId`) REFERENCES `Player` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_DamageDone_TargetPlayer` FOREIGN KEY (`TargetPlayerId`) REFERENCES `Player` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=6505789389 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Encounter`
--

DROP TABLE IF EXISTS `Encounter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Encounter` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Date` datetime NOT NULL,
  `BossFightId` int(11) NOT NULL,
  `SuccessfulKill` tinyint(1) NOT NULL,
  `ValidForRanking` tinyint(1) DEFAULT '0',
  `Hash` varchar(28) DEFAULT NULL,
  `Duration` time NOT NULL,
  `DateUploaded` datetime DEFAULT NULL,
  `IsPublic` tinyint(1) NOT NULL,
  `GuildId` int(11) DEFAULT NULL,
  `UploaderId` int(11) DEFAULT NULL,
  `EncounterDifficultyId` tinyint(3) NOT NULL DEFAULT '4',
  `ToBeDeleted` tinyint(1) NOT NULL DEFAULT '0',
  `Removed` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `FK_Encounter_Guild` (`GuildId`),
  KEY `FK_Encounter_AuthUserCharacter` (`UploaderId`),
  KEY `FK_Encounter_BossFight` (`BossFightId`),
  KEY `FK_Encounter_EncounterDifficulty_idx` (`EncounterDifficultyId`),
  CONSTRAINT `FK_Encounter_AuthUserCharacter` FOREIGN KEY (`UploaderId`) REFERENCES `AuthUserCharacter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_Encounter_BossFight` FOREIGN KEY (`BossFightId`) REFERENCES `BossFight` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_Encounter_EncounterDifficulty` FOREIGN KEY (`EncounterDifficultyId`) REFERENCES `EncounterDifficulty` (`Id`),
  CONSTRAINT `FK_Encounter_Guild` FOREIGN KEY (`GuildId`) REFERENCES `Guild` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=197106 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EncounterActiveTime`
--

DROP TABLE IF EXISTS `EncounterActiveTime`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `EncounterActiveTime` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CharacterId` varchar(50) NOT NULL,
  `CharacterName` varchar(50) NOT NULL,
  `CharacterType` varchar(50) NOT NULL,
  `SecondActiveUp` int(11) NOT NULL,
  `SecondActiveDown` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_EncounterActiveTime_CharacterId` (`CharacterId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EncounterBuffAction`
--

DROP TABLE IF EXISTS `EncounterBuffAction`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `EncounterBuffAction` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EncounterId` int(11) NOT NULL,
  `AbilityId` int(11) NOT NULL,
  `BuffName` varchar(50) NOT NULL,
  `SourceId` varchar(50) NOT NULL,
  `SourceName` varchar(50) NOT NULL,
  `SourceType` varchar(50) NOT NULL,
  `TargetId` varchar(50) NOT NULL,
  `TargetName` varchar(50) NOT NULL,
  `TargetType` varchar(50) NOT NULL,
  `SecondBuffWentUp` int(11) NOT NULL,
  `SecondBuffWentDown` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_EncounterBuffAction_SourceId` (`SourceId`),
  KEY `IX_EncounterBuffAction_SourceType` (`SourceType`),
  KEY `IX_EncounterBuffAction_TargetId` (`TargetId`),
  KEY `IX_EncounterBuffAction_TargetType` (`TargetType`),
  KEY `IX_EncounterBuffAction_EncounterId` (`EncounterId`),
  KEY `FK_EncounterBuffAction_Ability` (`AbilityId`),
  CONSTRAINT `FK_EncounterBuffAction_Ability` FOREIGN KEY (`AbilityId`) REFERENCES `Ability` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_EncounterBuffAction_Encounter` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=648773509 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EncounterBuffEvent`
--

DROP TABLE IF EXISTS `EncounterBuffEvent`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `EncounterBuffEvent` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EncounterId` int(11) NOT NULL,
  `BuffId` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `SecondBuffWentUp` int(11) NOT NULL,
  `SecondBuffWentDown` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_EncounterBuffEvent_EncounterId` (`EncounterId`),
  KEY `FK_EncounterBuffEvent_Player` (`PlayerId`),
  KEY `FK_EncounterBuffEvent_Buff` (`BuffId`),
  CONSTRAINT `FK_EncounterBuffEvent_Buff` FOREIGN KEY (`BuffId`) REFERENCES `Buff` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_EncounterBuffEvent_Encounter` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_EncounterBuffEvent_Player` FOREIGN KEY (`PlayerId`) REFERENCES `Player` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EncounterBuffUptime`
--

DROP TABLE IF EXISTS `EncounterBuffUptime`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `EncounterBuffUptime` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EncounterId` int(11) NOT NULL,
  `BuffId` int(11) NOT NULL,
  `Uptime` decimal(5,2) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_EncounterBuffUptime_EncounterId` (`EncounterId`),
  KEY `FK_EncounterBuffUptime_Buff` (`BuffId`),
  CONSTRAINT `FK_EncounterBuffUptime_Buff` FOREIGN KEY (`BuffId`) REFERENCES `Buff` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_EncounterBuffUptime_Encounter` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EncounterDeath`
--

DROP TABLE IF EXISTS `EncounterDeath`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `EncounterDeath` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SourcePlayerId` int(11) DEFAULT NULL,
  `SourceNpcName` varchar(100) DEFAULT NULL,
  `SourceNpcId` varchar(100) DEFAULT NULL,
  `SourcePetName` varchar(100) DEFAULT NULL,
  `TargetPlayerId` int(11) DEFAULT NULL,
  `TargetNpcName` varchar(100) DEFAULT NULL,
  `TargetNpcId` varchar(100) DEFAULT NULL,
  `TargetPetName` varchar(100) DEFAULT NULL,
  `EncounterId` int(11) NOT NULL,
  `AbilityId` int(11) NOT NULL,
  `TotalDamage` bigint(20) NOT NULL,
  `OverkillValue` bigint(20) NOT NULL,
  `SecondsElapsed` int(11) NOT NULL,
  `OrderWithinSecond` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_EncounterDeath_EncounterId` (`EncounterId`),
  KEY `FK_EncounterDeath_Ability` (`AbilityId`),
  KEY `FK_EncounterDeath_SourcePlayer` (`SourcePlayerId`),
  KEY `FK_EncounterDeath_TargetPlayer` (`TargetPlayerId`),
  CONSTRAINT `FK_EncounterDeath_Ability` FOREIGN KEY (`AbilityId`) REFERENCES `Ability` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_EncounterDeath_Encounter` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_EncounterDeath_SourcePlayer` FOREIGN KEY (`SourcePlayerId`) REFERENCES `Player` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_EncounterDeath_TargetPlayer` FOREIGN KEY (`TargetPlayerId`) REFERENCES `Player` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=7563513 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EncounterDebuffAction`
--

DROP TABLE IF EXISTS `EncounterDebuffAction`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `EncounterDebuffAction` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EncounterId` int(11) NOT NULL,
  `AbilityId` int(11) NOT NULL,
  `DebuffName` varchar(50) NOT NULL,
  `SourceId` varchar(50) NOT NULL,
  `SourceName` varchar(50) NOT NULL,
  `SourceType` varchar(50) NOT NULL,
  `TargetId` varchar(50) NOT NULL,
  `TargetName` varchar(50) NOT NULL,
  `TargetType` varchar(50) NOT NULL,
  `SecondDebuffWentUp` int(11) NOT NULL,
  `SecondDebuffWentDown` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_EncounterDebuffAction_EncounterId` (`EncounterId`),
  KEY `IX_EncounterDebuffAction_SourceId` (`SourceId`),
  KEY `IX_EncounterDebuffAction_SourceType` (`SourceType`),
  KEY `IX_EncounterDebuffAction_TargetId` (`TargetId`),
  KEY `IX_EncounterDebuffAction_TargetType` (`TargetType`),
  KEY `FK_EncounterDebuffAction_Ability` (`AbilityId`),
  CONSTRAINT `FK_EncounterDebuffAction_Ability` FOREIGN KEY (`AbilityId`) REFERENCES `Ability` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_EncounterDebuffAction_Encounter` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=350602123 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EncounterDifficulty`
--

DROP TABLE IF EXISTS `EncounterDifficulty`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `EncounterDifficulty` (
  `Id` tinyint(3) NOT NULL,
  `Name` varchar(45) NOT NULL,
  `ShortName` varchar(2) NOT NULL,
  `Priority` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Name_UNIQUE` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EncounterNpc`
--

DROP TABLE IF EXISTS `EncounterNpc`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `EncounterNpc` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `EncounterId` int(11) NOT NULL,
  `NpcName` varchar(50) NOT NULL,
  `NpcId` varchar(50) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `NpcName` (`NpcName`,`NpcId`),
  KEY `EncounterId` (`EncounterId`),
  CONSTRAINT `FK_EncounterNpc_Encounter` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3510211 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EncounterNpcCast`
--

DROP TABLE IF EXISTS `EncounterNpcCast`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `EncounterNpcCast` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EncounterId` int(11) NOT NULL,
  `AbilityName` varchar(100) NOT NULL,
  `NpcId` varchar(50) NOT NULL,
  `NpcName` varchar(50) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UC_EncounterNpcCast_EncounterIdAbilityNameNpcName` (`EncounterId`,`AbilityName`,`NpcName`),
  KEY `IX_EncounterNpcCast_EncounterId` (`EncounterId`),
  KEY `IX_EncounterNpcCast_NpcId` (`NpcId`),
  KEY `IX_EncounterNpcCast_AbilityName` (`AbilityName`),
  CONSTRAINT `FK_EncounterNpcCast_Encounter` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=1405044 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EncounterOverview`
--

DROP TABLE IF EXISTS `EncounterOverview`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `EncounterOverview` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `EncounterId` int(11) NOT NULL,
  `AverageDps` bigint(20) NOT NULL DEFAULT '-1',
  `PlayerDeaths` int(11) NOT NULL,
  `AverageHps` bigint(20) NOT NULL DEFAULT '-1',
  `AverageAps` bigint(20) NOT NULL DEFAULT '-1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UC_EncounterOverview_EncounterId` (`EncounterId`),
  KEY `IX_EncounterOverview_EncounterId` (`EncounterId`),
  CONSTRAINT `FK_EncounterOverview_Encounter` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=182861 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EncounterPlayerRole`
--

DROP TABLE IF EXISTS `EncounterPlayerRole`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `EncounterPlayerRole` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `EncounterId` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Role` varchar(12) NOT NULL,
  `Class` varchar(12) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UQ_EncounterIdPlayerId` (`EncounterId`,`PlayerId`),
  KEY `EncounterId` (`EncounterId`,`PlayerId`),
  KEY `PlayerId` (`PlayerId`),
  KEY `Name` (`Name`),
  CONSTRAINT `FK_Encounter_EncounterPlayerRole` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`),
  CONSTRAINT `FK_Player_EncounterPlayerRole` FOREIGN KEY (`PlayerId`) REFERENCES `Player` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3882171 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EncounterPlayerStatistics`
--

DROP TABLE IF EXISTS `EncounterPlayerStatistics`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `EncounterPlayerStatistics` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EncounterId` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `DPS` bigint(20) NOT NULL DEFAULT '0',
  `HPS` bigint(20) NOT NULL DEFAULT '0',
  `APS` bigint(20) NOT NULL DEFAULT '0',
  `Deaths` tinyint(4) NOT NULL DEFAULT '0',
  `TopDpsAbilityId` int(11) DEFAULT NULL,
  `TopHpsAbilityId` int(11) DEFAULT NULL,
  `TopApsAbilityId` int(11) DEFAULT NULL,
  `TopDpsAbilityValue` bigint(20) NOT NULL DEFAULT '0',
  `TopHpsAbilityValue` bigint(20) NOT NULL DEFAULT '0',
  `TopApsAbilityValue` bigint(20) NOT NULL DEFAULT '0',
  `SingleTargetDPS` bigint(20) NOT NULL DEFAULT '0',
  `BurstDamage1sValue` bigint(20) NOT NULL DEFAULT '0',
  `BurstDamage1sSecond` int(11) NOT NULL DEFAULT '0',
  `BurstDamage5sValue` bigint(20) NOT NULL DEFAULT '0',
  `BurstDamage5sPerSecond` bigint(20) NOT NULL DEFAULT '0',
  `BurstDamage5sStart` int(11) NOT NULL DEFAULT '0',
  `BurstDamage5sEnd` int(11) NOT NULL DEFAULT '0',
  `BurstDamage15sValue` bigint(20) NOT NULL DEFAULT '0',
  `BurstDamage15sPerSecond` bigint(20) NOT NULL DEFAULT '0',
  `BurstDamage15sStart` int(11) NOT NULL DEFAULT '0',
  `BurstDamage15sEnd` int(11) NOT NULL DEFAULT '0',
  `BurstHealing1sValue` bigint(20) NOT NULL DEFAULT '0',
  `BurstHealing1sSecond` int(11) NOT NULL DEFAULT '0',
  `BurstHealing5sValue` bigint(20) NOT NULL DEFAULT '0',
  `BurstHealing5sPerSecond` bigint(20) NOT NULL DEFAULT '0',
  `BurstHealing5sStart` int(11) NOT NULL DEFAULT '0',
  `BurstHealing5sEnd` int(11) NOT NULL DEFAULT '0',
  `BurstHealing15sValue` bigint(20) NOT NULL DEFAULT '0',
  `BurstHealing15sPerSecond` bigint(20) NOT NULL DEFAULT '0',
  `BurstHealing15sStart` int(11) NOT NULL DEFAULT '0',
  `BurstHealing15sEnd` int(11) NOT NULL DEFAULT '0',
  `BurstShielding1sValue` bigint(20) NOT NULL DEFAULT '0',
  `BurstShielding1sSecond` int(11) NOT NULL DEFAULT '0',
  `BurstShielding5sValue` bigint(20) NOT NULL DEFAULT '0',
  `BurstShielding5sPerSecond` bigint(20) NOT NULL DEFAULT '0',
  `BurstShielding5sStart` int(11) NOT NULL DEFAULT '0',
  `BurstShielding5sEnd` int(11) NOT NULL DEFAULT '0',
  `BurstShielding15sValue` bigint(20) NOT NULL DEFAULT '0',
  `BurstShielding15sPerSecond` bigint(20) NOT NULL DEFAULT '0',
  `BurstShielding15sStart` int(11) NOT NULL DEFAULT '0',
  `BurstShielding15sEnd` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `EncounterId` (`EncounterId`,`PlayerId`),
  KEY `PlayerId` (`PlayerId`),
  KEY `TopDpsAbilityId` (`TopDpsAbilityId`),
  KEY `TopHpsAbilityId` (`TopHpsAbilityId`),
  KEY `TopApsAbilityId` (`TopApsAbilityId`),
  CONSTRAINT `FK_EncounterPlayerStatistics_ApsAbility` FOREIGN KEY (`TopApsAbilityId`) REFERENCES `Ability` (`Id`),
  CONSTRAINT `FK_EncounterPlayerStatistics_DpsAbility` FOREIGN KEY (`TopDpsAbilityId`) REFERENCES `Ability` (`Id`),
  CONSTRAINT `FK_EncounterPlayerStatistics_Encounter` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`),
  CONSTRAINT `FK_EncounterPlayerStatistics_HpsAbility` FOREIGN KEY (`TopHpsAbilityId`) REFERENCES `Ability` (`Id`),
  CONSTRAINT `FK_EncounterPlayerStatistics_Player` FOREIGN KEY (`PlayerId`) REFERENCES `Player` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1025978 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Guild`
--

DROP TABLE IF EXISTS `Guild`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Guild` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ShardId` int(11) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `GuildStatusId` int(11) NOT NULL,
  `Created` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `HideFromLists` tinyint(1) NOT NULL DEFAULT '0',
  `HideFromRankings` tinyint(1) NOT NULL DEFAULT '0',
  `HideFromSearch` tinyint(1) NOT NULL DEFAULT '0',
  `HideSessions` tinyint(1) NOT NULL DEFAULT '0',
  `HideRoster` tinyint(1) NOT NULL DEFAULT '1',
  `HideProgression` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `FK_Guild_Shard` (`ShardId`),
  KEY `FK_Guild_GuildStatus` (`GuildStatusId`),
  CONSTRAINT `FK_Guild_GuildStatus` FOREIGN KEY (`GuildStatusId`) REFERENCES `GuildStatus` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_Guild_Shard` FOREIGN KEY (`ShardId`) REFERENCES `Shard` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=580 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `GuildRank`
--

DROP TABLE IF EXISTS `GuildRank`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `GuildRank` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `Description` varchar(100) DEFAULT NULL,
  `CanUploadLogs` tinyint(1) NOT NULL,
  `CanPromoteUsers` tinyint(1) NOT NULL,
  `CanApproveUsers` tinyint(1) NOT NULL,
  `CanModifyPrivacy` tinyint(1) NOT NULL,
  `CanModifyAnySession` tinyint(1) NOT NULL DEFAULT '0',
  `DefaultWhenCreated` tinyint(1) NOT NULL,
  `DefaultWhenApproved` tinyint(1) NOT NULL,
  `RankPriority` int(11) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `GuildStatus`
--

DROP TABLE IF EXISTS `GuildStatus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `GuildStatus` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `Description` varchar(100) DEFAULT NULL,
  `Active` tinyint(1) NOT NULL,
  `Approved` tinyint(1) NOT NULL,
  `DefaultStatus` tinyint(1) NOT NULL,
  `PlayersCanApply` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `HealingDone`
--

DROP TABLE IF EXISTS `HealingDone`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `HealingDone` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `SourcePlayerId` int(11) DEFAULT NULL,
  `SourceNpcName` varchar(100) DEFAULT NULL,
  `SourceNpcId` varchar(100) DEFAULT NULL,
  `SourcePetName` varchar(100) DEFAULT NULL,
  `TargetPlayerId` int(11) DEFAULT NULL,
  `TargetNpcName` varchar(100) DEFAULT NULL,
  `TargetNpcId` varchar(100) DEFAULT NULL,
  `TargetPetName` varchar(100) DEFAULT NULL,
  `EncounterId` int(11) NOT NULL,
  `AbilityId` int(11) NOT NULL,
  `TotalHealing` bigint(20) NOT NULL,
  `EffectiveHealing` bigint(20) NOT NULL,
  `CriticalHit` tinyint(1) NOT NULL,
  `SecondsElapsed` int(11) NOT NULL,
  `OrderWithinSecond` int(11) NOT NULL,
  `OverhealAmount` bigint(20) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_HealingDone_EncounterId` (`EncounterId`),
  KEY `IX_HealingDone_SourcePlayerId` (`SourcePlayerId`),
  KEY `IX_HealingDone_TargetPlayerId` (`TargetPlayerId`),
  KEY `FK_HealingDone_Ability` (`AbilityId`),
  CONSTRAINT `FK_HealingDone_Ability` FOREIGN KEY (`AbilityId`) REFERENCES `Ability` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_HealingDone_Encounter` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_HealingDone_SourcePlayer` FOREIGN KEY (`SourcePlayerId`) REFERENCES `Player` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_HealingDone_TargetPlayer` FOREIGN KEY (`TargetPlayerId`) REFERENCES `Player` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=6148196883 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Instance`
--

DROP TABLE IF EXISTS `Instance`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Instance` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `MaxRaidSize` tinyint(4) NOT NULL DEFAULT '20',
  `Visible` tinyint(1) NOT NULL DEFAULT '1',
  `IncludeInProgression` tinyint(1) NOT NULL DEFAULT '1',
  `IncludeInLists` tinyint(1) NOT NULL DEFAULT '1',
  `ShortName` varchar(6) NOT NULL,
  `ImageFilename` varchar(20) DEFAULT NULL,
  `TierNumber` tinyint(1) NOT NULL DEFAULT '1',
  `GameVersion` tinyint(2) NOT NULL DEFAULT '1',
  `ForceShowTier` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `NewsRecentChanges`
--

DROP TABLE IF EXISTS `NewsRecentChanges`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `NewsRecentChanges` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ItemDate` datetime NOT NULL,
  `Description` text NOT NULL,
  `Created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Visible` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=184 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `NpcDeath`
--

DROP TABLE IF EXISTS `NpcDeath`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `NpcDeath` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `Deaths` bigint(20) NOT NULL,
  `AlternateNames` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=9636 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Player`
--

DROP TABLE IF EXISTS `Player`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Player` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `Shard` varchar(100) NOT NULL,
  `PlayerId` varchar(50) NOT NULL,
  `PlayerClassId` int(11) DEFAULT NULL COMMENT 'This is nullable because we won''t know what class the player is when inserting, but we can loop over later and update it.',
  PRIMARY KEY (`Id`),
  KEY `PlayerId` (`PlayerId`)
) ENGINE=InnoDB AUTO_INCREMENT=143575 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PlayerClass`
--

DROP TABLE IF EXISTS `PlayerClass`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PlayerClass` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `Color` varchar(6) DEFAULT NULL,
  `Icon` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `RoleIcon`
--

DROP TABLE IF EXISTS `RoleIcon`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `RoleIcon` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `Icon` varchar(50) NOT NULL,
  `Priority` int(11) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ScheduledTask`
--

DROP TABLE IF EXISTS `ScheduledTask`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ScheduledTask` (
  `Id` smallint(6) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `LastRun` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ScheduleMinutes` smallint(6) NOT NULL DEFAULT '60',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Session`
--

DROP TABLE IF EXISTS `Session`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Session` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Date` datetime NOT NULL,
  `Duration` time NOT NULL,
  `Name` varchar(100) NOT NULL,
  `AuthUserCharacterId` int(11) NOT NULL COMMENT 'Session creator',
  `UploadToken` varchar(50) DEFAULT NULL COMMENT 'Obsolete',
  `Filename` varchar(50) DEFAULT NULL COMMENT 'Obsolete',
  `EncountersPublic` tinyint(1) DEFAULT NULL,
  `SessionSize` bigint(20) DEFAULT NULL,
  `TotalPlayTime` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_Session_AuthUserCharacter` (`AuthUserCharacterId`),
  CONSTRAINT `FK_Session_AuthUserCharacter` FOREIGN KEY (`AuthUserCharacterId`) REFERENCES `AuthUserCharacter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=16762 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SessionEncounter`
--

DROP TABLE IF EXISTS `SessionEncounter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SessionEncounter` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SessionId` int(11) NOT NULL,
  `EncounterId` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_SessionEncounter_EncounterId` (`EncounterId`),
  KEY `FK_SessionEncounter_Session` (`SessionId`),
  CONSTRAINT `FK_SessionEncounter_Encounter` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_SessionEncounter_Session` FOREIGN KEY (`SessionId`) REFERENCES `Session` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=237258 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SessionLog`
--

DROP TABLE IF EXISTS `SessionLog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SessionLog` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SessionId` int(11) NOT NULL,
  `AuthUserCharacterId` int(11) NOT NULL,
  `GuildId` int(11) NOT NULL,
  `Token` varchar(50) DEFAULT NULL,
  `Filename` varchar(50) NOT NULL,
  `LogSize` bigint(20) DEFAULT '0',
  `TotalPlayedTime` bigint(20) DEFAULT '0',
  `LogLines` bigint(20) DEFAULT '0',
  `CreationDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `FK_SessionLog_Session` (`SessionId`),
  KEY `FK_SessionLog_AuthUserCharacter` (`AuthUserCharacterId`),
  KEY `FK_SessionLog_Guild` (`GuildId`),
  CONSTRAINT `FK_SessionLog_AuthUserCharacter` FOREIGN KEY (`AuthUserCharacterId`) REFERENCES `AuthUserCharacter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_SessionLog_Guild` FOREIGN KEY (`GuildId`) REFERENCES `Guild` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_SessionLog_Session` FOREIGN KEY (`SessionId`) REFERENCES `Session` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=16552 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SessionParseLog`
--

DROP TABLE IF EXISTS `SessionParseLog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SessionParseLog` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `SessionId` int(11) NOT NULL,
  `LogLine` varchar(200) NOT NULL,
  `Created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `FK_SessionParseLog_Session_idx` (`SessionId`),
  CONSTRAINT `FK_SessionParseLog_Session` FOREIGN KEY (`SessionId`) REFERENCES `Session` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Shard`
--

DROP TABLE IF EXISTS `Shard`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Shard` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `Region` varchar(3) NOT NULL,
  `ShardType` varchar(6) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=158 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ShieldingDone`
--

DROP TABLE IF EXISTS `ShieldingDone`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ShieldingDone` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `SourcePlayerId` int(11) DEFAULT NULL,
  `SourceNpcName` varchar(100) DEFAULT NULL,
  `SourceNpcId` varchar(100) DEFAULT NULL,
  `SourcePetName` varchar(100) DEFAULT NULL,
  `TargetPlayerId` int(11) DEFAULT NULL,
  `TargetNpcName` varchar(100) DEFAULT NULL,
  `TargetNpcId` varchar(100) DEFAULT NULL,
  `TargetPetName` varchar(100) DEFAULT NULL,
  `EncounterId` int(11) NOT NULL,
  `AbilityId` int(11) NOT NULL,
  `ShieldValue` bigint(20) NOT NULL,
  `CriticalHit` tinyint(1) NOT NULL,
  `SecondsElapsed` int(11) NOT NULL,
  `OrderWithinSecond` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ShieldingDone_EncounterId` (`EncounterId`),
  KEY `IX_ShieldingDone_SourcePlayerId` (`SourcePlayerId`),
  KEY `IX_ShieldingDone_TargetPlayerId` (`TargetPlayerId`),
  KEY `FK_ShieldingDone_Ability` (`AbilityId`),
  CONSTRAINT `FK_ShieldingDone_Ability` FOREIGN KEY (`AbilityId`) REFERENCES `Ability` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_ShieldingDone_Encounter` FOREIGN KEY (`EncounterId`) REFERENCES `Encounter` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_ShieldingDone_SourcePlayer` FOREIGN KEY (`SourcePlayerId`) REFERENCES `Player` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_ShieldingDone_TargetPlayer` FOREIGN KEY (`TargetPlayerId`) REFERENCES `Player` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=390338597 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SiteNotification`
--

DROP TABLE IF EXISTS `SiteNotification`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SiteNotification` (
  `Id` tinyint(4) NOT NULL AUTO_INCREMENT,
  `Header` varchar(100) NOT NULL,
  `Body` varchar(1000) NOT NULL,
  `ColourClass` varchar(45) NOT NULL,
  `Created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Visible` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=44 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Soul`
--

DROP TABLE IF EXISTS `Soul`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Soul` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PlayerClassId` int(11) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Icon` varchar(50) DEFAULT NULL,
  `Role` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_Soul_PlayerClass` (`PlayerClassId`),
  CONSTRAINT `FK_Soul_PlayerClass` FOREIGN KEY (`PlayerClassId`) REFERENCES `PlayerClass` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=41 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SoulAbility`
--

DROP TABLE IF EXISTS `SoulAbility`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SoulAbility` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SoulId` int(11) NOT NULL,
  `AbilityId` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_SoulAbility_Soul_idx` (`SoulId`),
  KEY `FK_SoulAbility_Ability_idx` (`AbilityId`),
  CONSTRAINT `FK_SoulAbility_Ability` FOREIGN KEY (`AbilityId`) REFERENCES `Ability` (`Id`),
  CONSTRAINT `FK_SoulAbility_Soul` FOREIGN KEY (`SoulId`) REFERENCES `Soul` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `TempAbility`
--

DROP TABLE IF EXISTS `TempAbility`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `TempAbility` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SoulId` int(11) DEFAULT NULL,
  `AbilityId` bigint(20) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Icon` varchar(50) DEFAULT NULL,
  `RequiredLevel` int(11) DEFAULT NULL,
  `MinimumPointsInSoul` int(11) DEFAULT NULL,
  `AddonId` varchar(50) DEFAULT NULL,
  `Description` varchar(650) DEFAULT NULL,
  `DamageType` varchar(50) DEFAULT NULL,
  `RankNumber` int(11) DEFAULT NULL,
  `RequiresWeapon` varchar(50) DEFAULT NULL,
  `Cooldown` decimal(8,2) DEFAULT NULL,
  `CastTime` varchar(50) DEFAULT NULL,
  `Channel` tinyint(1) DEFAULT NULL,
  `Duration` decimal(8,2) DEFAULT NULL,
  `Interval` decimal(4,2) DEFAULT NULL,
  `MinRange` decimal(4,1) DEFAULT NULL,
  `MaxRange` decimal(4,1) DEFAULT NULL,
  `CostManaPercent` decimal(5,2) DEFAULT NULL,
  `CostEnergy` int(11) DEFAULT NULL,
  `MinimumHeat` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_TempAbility_Soul` (`SoulId`),
  CONSTRAINT `FK_TempAbility_Soul` FOREIGN KEY (`SoulId`) REFERENCES `Soul` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COMMENT='This table is just for testing of importing abilities from the XML file and while parsing combatlogs.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `UserGroup`
--

DROP TABLE IF EXISTS `UserGroup`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `UserGroup` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(45) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UQ_UserGroup_Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `UserGroupMembership`
--

DROP TABLE IF EXISTS `UserGroupMembership`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `UserGroupMembership` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UserGroupId` int(11) NOT NULL,
  `AuthUserId` int(11) NOT NULL,
  `Created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreatedBy` varchar(45) NOT NULL,
  `Modified` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `ModifiedBy` varchar(45) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_UserGroupMembership_UserGroup_idx` (`UserGroupId`),
  KEY `FK_UserGroupMembership_AuthUser_idx` (`AuthUserId`),
  CONSTRAINT `FK_UserGroupMembership_AuthUser` FOREIGN KEY (`AuthUserId`) REFERENCES `AuthUser` (`Id`),
  CONSTRAINT `FK_UserGroupMembership_UserGroup` FOREIGN KEY (`UserGroupId`) REFERENCES `UserGroup` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2017-10-16 19:30:12
