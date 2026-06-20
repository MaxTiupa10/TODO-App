-- Mark existing active tasks as overdue
UPDATE "Tasks"
SET "Deadline" = '2026-06-17 09:00:00+00'
WHERE "Id" = 1 AND "IsCompleted" = false;

UPDATE "Tasks"
SET "Deadline" = '2026-06-18 14:00:00+00'
WHERE "Id" = 4 AND "IsCompleted" = false;

UPDATE "Tasks"
SET "Deadline" = '2026-06-19 08:00:00+00'
WHERE "Id" = 8 AND "IsCompleted" = false;

UPDATE "Tasks"
SET "Deadline" = '2026-06-16 20:00:00+00'
WHERE "Id" = 15 AND "IsCompleted" = false;

UPDATE "Tasks"
SET "Deadline" = '2026-06-10 23:59:00+00'
WHERE "Id" = 16 AND "IsCompleted" = false;

UPDATE "Tasks"
SET "Deadline" = '2026-06-19 12:00:00+00'
WHERE "Id" = 18 AND "IsCompleted" = false;

-- Add new overdue tasks
INSERT INTO "Tasks" (
    "Id", "Title", "Description", "Deadline", "IsCompleted", "IsImportant", "CategoryId", "UserId", "CreatedAt"
)
OVERRIDING SYSTEM VALUE
VALUES
(21, 'Сплатити штраф за паркування', 'Термін сплати минув 5 днів тому', '2026-06-15 17:00:00+00', false, true, 1, 1, NOW()),
(22, 'Передзвонити лікарю', 'Потрібно записатися на повторний прийом', '2026-06-18 11:30:00+00', false, false, 3, 1, NOW()),
(23, 'Здати показники лічильників', 'Останній день подачі був учора', '2026-06-19 23:59:00+00', false, true, 2, 1, NOW())
ON CONFLICT ("Id") DO UPDATE SET
    "Title" = EXCLUDED."Title",
    "Description" = EXCLUDED."Description",
    "Deadline" = EXCLUDED."Deadline",
    "IsCompleted" = EXCLUDED."IsCompleted",
    "IsImportant" = EXCLUDED."IsImportant",
    "CategoryId" = EXCLUDED."CategoryId";

SELECT setval(
    pg_get_serial_sequence('"Tasks"', 'Id'),
    COALESCE((SELECT MAX("Id") FROM "Tasks"), 1),
    true
);

SELECT "Id", "Title", "Deadline", "IsCompleted"
FROM "Tasks"
WHERE "Deadline" < NOW() AND "IsCompleted" = false
ORDER BY "Deadline";
