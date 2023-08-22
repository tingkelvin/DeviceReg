  USE [main]
  GO
  IF NOT EXISTS (SELECT name FROM main.sys.databases WHERE name = N'DatabaseExample')
  CREATE DATABASE [DatabaseExample]
  GO