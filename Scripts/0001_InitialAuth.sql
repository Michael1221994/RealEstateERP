CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260903123330_InitialAuth') THEN
    CREATE TABLE users (
        "Id" uuid NOT NULL,
        "FullName" character varying(200) NOT NULL,
        "Username" character varying(100) NOT NULL,
        "Email" character varying(200),
        "Phone" character varying(30),
        "PasswordHash" character varying(255) NOT NULL,
        "Role" character varying(30) NOT NULL,
        "IsActive" boolean NOT NULL,
        "LastLoginAt" timestamp without time zone,
        "CreatedAt" timestamp without time zone NOT NULL,
        "UpdatedAt" timestamp without time zone,
        CONSTRAINT "PK_users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260903123330_InitialAuth') THEN
    CREATE UNIQUE INDEX "IX_users_Username" ON users ("Username");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260903123330_InitialAuth') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260903123330_InitialAuth', '8.0.11');
    END IF;
END $EF$;
COMMIT;

