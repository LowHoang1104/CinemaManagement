-- ============================================================
-- Script insert SeatStatus và 100 Seats (Auto-find RoomId)
-- ============================================================

-- Step 1: Insert SeatStatus data
INSERT INTO "SeatStatuses" ("SeatStatusId", "StatusName")
VALUES 
('550e8400-e29b-41d4-a716-000000000001', 'Active'),
('550e8400-e29b-41d4-a716-000000000002', 'Inactive'),
('550e8400-e29b-41d4-a716-000000000003', 'Maintenance')
ON CONFLICT DO NOTHING;

-- Step 2: Get first available RoomId and insert 100 Seats
WITH room_data AS (
  SELECT "RoomId" FROM "Rooms" ORDER BY "RoomId" LIMIT 1
)
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
    room_data."RoomId",
    '550e8400-e29b-41d4-a716-000000000001', -- Active status
    (CHR(64 + rows.row_num) || LPAD(cols.col_num::TEXT, 2, '0')) as seat_code,
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
    (SELECT * FROM generate_series(1, 10) AS col_num) AS cols,
    room_data;

-- Step 3: Verify data
SELECT COUNT(*) as "Total Seats" FROM "Seats";
SELECT COUNT(*) as "Rows with 10 seats each" FROM (
    SELECT "RowLabel", COUNT(*) as seat_count 
    FROM "Seats" 
    GROUP BY "RowLabel"
) t WHERE seat_count = 10;

SELECT * FROM "SeatStatuses" ORDER BY "SeatStatusId";

-- Display sample seats
SELECT "SeatCode", "RowLabel", "ColNumber", "SeatType" 
FROM "Seats" 
ORDER BY "RowLabel", "ColNumber" 
LIMIT 20;
