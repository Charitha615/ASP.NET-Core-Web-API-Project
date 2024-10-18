-- Create the database
CREATE DATABASE IF NOT EXISTS myauthdb;

-- Use the newly created database
USE myauthdb;

-- Create the appointments table
CREATE TABLE IF NOT EXISTS appointments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255),
    Age INT,
    UserID VARCHAR(255),
    MedicalHistory VARCHAR(255),
    TreatmentSchedule VARCHAR(255),
    Medications VARCHAR(255),
    Contact VARCHAR(255),
    AesKey VARCHAR(1000),
    AesIV VARCHAR(1000),
    CreatedAt DATETIME,
    DoctorID INT,
    DoctorName VARCHAR(255)
);

-- Create the doctors table
CREATE TABLE IF NOT EXISTS doctors (
    DoctorId INT AUTO_INCREMENT PRIMARY KEY,
    FullName VARCHAR(255),
    EncryptedEmail TEXT,
    EncryptedPhoneNumber TEXT,
    EncryptedSpecialty TEXT,
    EncryptedLicenseNumber TEXT,
    ExperienceYears INT
);

-- Create the userlogs table
CREATE TABLE IF NOT EXISTS userlogs (
    LogId INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT,
    Action VARCHAR(100),
    Details TEXT,
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    IPAddress VARCHAR(306)
);

-- Create the users table
CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(255),
    email VARCHAR(255),
    EncryptedEmail VARCHAR(1000),
    PasswordHash VARCHAR(1000),
    PasswordSalt VARCHAR(1000),
    AesKey VARCHAR(1000),
    AesIV VARCHAR(1000)
);
