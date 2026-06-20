ALTER TABLE "Tasks" ADD COLUMN IF NOT EXISTS "CompletedAt" timestamp with time zone NULL;

UPDATE "Tasks"
SET "CompletedAt" = "CreatedAt"
WHERE "IsCompleted" = true AND "CompletedAt" IS NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES
    ('20260616170000_AddTaskDeadlineAndImportant', '8.0.12'),
    ('20260620120000_AddTaskCompletedAt', '8.0.12')
ON CONFLICT ("MigrationId") DO NOTHING;

SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
\d "Tasks"
