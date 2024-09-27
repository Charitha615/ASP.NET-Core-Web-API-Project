-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1:3306
-- Generation Time: Sep 27, 2024 at 04:42 AM
-- Server version: 8.3.0
-- PHP Version: 8.2.18

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `myauthdb`
--

-- --------------------------------------------------------

--
-- Table structure for table `appointments`
--

DROP TABLE IF EXISTS `appointments`;
CREATE TABLE IF NOT EXISTS `appointments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `Age` int NOT NULL,
  `UserID` int NOT NULL,
  `MedicalHistory` varchar(255) NOT NULL,
  `TreatmentSchedule` varchar(255) NOT NULL,
  `Medications` varchar(255) NOT NULL,
  `Contact` varchar(255) NOT NULL,
  `AesKey` varchar(1000) NOT NULL,
  `AesIV` varchar(1000) NOT NULL,
  `CreatedAt` datetime NOT NULL,
  `DoctorID` int NOT NULL,
  `DoctorName` varchar(255) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `appointments`
--

INSERT INTO `appointments` (`Id`, `Name`, `Age`, `UserID`, `MedicalHistory`, `TreatmentSchedule`, `Medications`, `Contact`, `AesKey`, `AesIV`, `CreatedAt`, `DoctorID`, `DoctorName`) VALUES
(1, '6OGvFDwEXiuXn947Ly0W/Q==', 23, 1, 'RG50R5x5fvgU0ZkiUcnnrQ==', 'RG50R5x5fvgU0ZkiUcnnrQ==', 'qVdMIUMZsjdEbJAJcTLtVQ==', '2hAGrIi+FU/ddf++kTy2lw==', 'wR5Re0fpSqhmY2NwQngFMUEytzbk0pipbs/q1xTy6E8=', 'MXLlvUt1qJkAt3ZJpFNzyA==', '0000-00-00 00:00:00', 1, ''),
(2, '9Cqg2jo4fcvylmcy8sZNwA==', 23, 1, 'UrbdQkYXwb+Rck4gljGCDQ==', 'PE9y8rTSVpw3BMOUX5m6dg==', 'xMdHClO1FUz8XZhqgeXnUQ==', 'xMdHClO1FUz8XZhqgeXnUQ==', 'x40WBNogKLci7mlSDAYPDFM68loMEgRGkg+zDfa5IXw=', 'SO2s7qHhwWEEil3V5yPAGQ==', '0000-00-00 00:00:00', 1, 'Dr. Smith'),
(3, 'UR0i05wnQtamno210bYo/w==', 23, 1, 'FiwygRNQCOKFsnXQLVVkNA==', 'FiwygRNQCOKFsnXQLVVkNA==', 'FiwygRNQCOKFsnXQLVVkNA==', 'a3DZAkVDBgSV5FzXdsyThA==', '5GJ5J81HAW0EAU0/xyiilkj82fFiM2BBXPJ7+hx3KsU=', 'yNUt/2eiqc22dEV3wZedJw==', '0000-00-00 00:00:00', 1, 'Dr. Smith');

-- --------------------------------------------------------

--
-- Table structure for table `doctors`
--

DROP TABLE IF EXISTS `doctors`;
CREATE TABLE IF NOT EXISTS `doctors` (
  `DoctorId` int NOT NULL AUTO_INCREMENT,
  `FullName` varchar(100) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `PhoneNumber` varchar(15) NOT NULL,
  `Specialty` varchar(100) NOT NULL,
  `LicenseNumber` varchar(50) NOT NULL,
  `ExperienceYears` int NOT NULL,
  PRIMARY KEY (`DoctorId`)
) ENGINE=MyISAM AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `doctors`
--

INSERT INTO `doctors` (`DoctorId`, `FullName`, `Email`, `PhoneNumber`, `Specialty`, `LicenseNumber`, `ExperienceYears`) VALUES
(1, 'Dr. John Doe', 'john.doe@example.com', '1234567890', 'Cardiology', 'LIC12345', 10),
(2, 'Dr. John Doe', 'john.doe@example.com', '1234567890', 'Cardiology', 'LIC12345', 10),
(3, 'sda', 'hasr@gmail.com', '0866966656', 'sadas', '232ada', 22),
(4, 'Dr. John Doe', 'john.doe@example.com', '1234567890', 'Cardiology', 'LIC12345', 10),
(5, 'Charitha', 'adweb@gmail.com', '0789621706', 'sadas', '232ada', 22),
(6, 'Charitha', 'hasr@gmail.com', '12345678', 'sadas', '232ada', 1),
(7, 'Charitha', 'hasr@gmail.com', '12345678', 'sadas', '232ada', 1),
(8, 'Charitha', 'hasr@gmail.com', '12345678', 'sadas', '232ada', 1),
(9, 'Charitha', 'hasr@gmail.com', '12345678', 'sadas', '232ada', 1),
(10, 'dasda', 'hasr@gmail.com', '0789621706', 'sadas', '232ada', 222);

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
CREATE TABLE IF NOT EXISTS `users` (
  `id` int NOT NULL AUTO_INCREMENT,
  `username` varchar(255) NOT NULL,
  `email` varchar(255) NOT NULL,
  `EncryptedEmail` varchar(1000) NOT NULL,
  `PasswordHash` varchar(1000) NOT NULL,
  `PasswordSalt` varchar(1000) NOT NULL,
  `AesKey` varchar(1000) NOT NULL,
  `AesIV` varchar(1000) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`id`, `username`, `email`, `EncryptedEmail`, `PasswordHash`, `PasswordSalt`, `AesKey`, `AesIV`) VALUES
(1, 'string', 'string@gmail.com', 'qEDH9kL+C+380Lwcc+fB1YD1Su/ssKImX9D4p8pCGk5NCowiSbsP8rdE4j+uAi8Z', 'tjhTqvpaOO9Hy91QxcDYeQO59AvuAYlvnDlTwyVBe2g=', '57jk0yAVWKje8+42JlKRd15EC8vU/nDg8vEB0mxd/czjJjQ0XGOBm8Q6V7o3kv2VFOwKH5to2E5StWGUnVH0Gw==', '', '');

-- --------------------------------------------------------

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
CREATE TABLE IF NOT EXISTS `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
