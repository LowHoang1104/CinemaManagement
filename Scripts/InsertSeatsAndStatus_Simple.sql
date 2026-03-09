-- ============================================================
-- Script ?? insert SeatStatus và 100 Seats vào Database
-- ============================================================

-- Step 1: Insert SeatStatus data
INSERT INTO "SeatStatuses" ("SeatStatusId", "StatusName")
VALUES 
('550e8400-e29b-41d4-a716-000000000001', 'Active'),
('550e8400-e29b-41d4-a716-000000000002', 'Inactive'),
('550e8400-e29b-41d4-a716-000000000003', 'Maintenance')
ON CONFLICT DO NOTHING;

-- Step 2: Insert 100 Seats (10 rows × 10 columns)
-- Adjust the RoomId below to match your actual room ID
-- You can find it by running: SELECT "RoomId" FROM "Rooms" LIMIT 1;

INSERT INTO "Seats" (
    "SeatId",
    "RoomId",
    "SeatStatusId",
    "SeatCode",
    "RowLabel",
    "ColNumber",
"SeatType",
    "CreatedAt"
)
SELECT 
    gen_random_uuid(),
    '12345678-1234-1234-1234-123456789012', -- Replace with your actual RoomId
    '550e8400-e29b-41d4-a716-000000000001', -- Active status
    (CHR(64 + rows.row_num) || LPAD(cols.col_num::TEXT, 2, '0')),
    rows.row_num,
    cols.col_num,
    CASE 
      WHEN rows.row_num IN (1, 10) AND cols.col_num IN (1, 10) THEN 'VIP'
        WHEN rows.row_num IN (5, 6) AND cols.col_num BETWEEN 4 AND 7 THEN 'VIP'
      ELSE 'Standard'
    END,
    NOW()
FROM 
    (SELECT * FROM generate_series(1, 10) AS row_num) AS rows,
    (SELECT * FROM generate_series(1, 10) AS col_num) AS cols;

-- Verify data
SELECT COUNT(*) as "Total Seats Inserted" FROM "Seats";
SELECT "SeatStatusId", "StatusName" FROM "SeatStatuses" ORDER BY "SeatStatusId";
