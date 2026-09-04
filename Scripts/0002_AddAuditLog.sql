START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904180537_AddAuditLog') THEN
    CREATE TABLE audit_logs (
        "Id" uuid NOT NULL,
        "ActorUserId" uuid,
        "Action" character varying(100) NOT NULL,
        "EntityType" character varying(100) NOT NULL,
        "EntityId" uuid,
        "Details" jsonb,
        "OccurredAt" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_audit_logs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_audit_logs_users_ActorUserId" FOREIGN KEY ("ActorUserId") REFERENCES users ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904180537_AddAuditLog') THEN
    CREATE INDEX "IX_audit_logs_ActorUserId" ON audit_logs ("ActorUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904180537_AddAuditLog') THEN
    CREATE INDEX "IX_audit_logs_EntityType_EntityId_OccurredAt" ON audit_logs ("EntityType", "EntityId", "OccurredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904180537_AddAuditLog') THEN
    CREATE INDEX "IX_audit_logs_OccurredAt" ON audit_logs ("OccurredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904180537_AddAuditLog') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260904180537_AddAuditLog', '8.0.11');
    END IF;
END $EF$;
COMMIT;

