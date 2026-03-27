-- TaskTracker: create database and tasks table, runs once in MySQL
-- usage: mysql -u root < setup.sql (im using root b/c im admin and dont have a user, no password either)

CREATE DATABASE IF NOT EXISTS tasktracker
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE tasktracker;

CREATE TABLE IF NOT EXISTS tasks (
  id INT AUTO_INCREMENT PRIMARY KEY,
  title VARCHAR(255) NOT NULL,
  description VARCHAR(255),
  is_completed BOOLEAN NOT NULL DEFAULT FALSE,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
