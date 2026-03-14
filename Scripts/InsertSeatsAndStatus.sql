-- Insert SeatStatus data
INSERT INTO "SeatStatuses" ("SeatStatusId", "StatusName")
VALUES 
('550e8400-e29b-41d4-a716-000000000001', 'Active'),
('550e8400-e29b-41d4-a716-000000000002', 'Inactive'),
('550e8400-e29b-41d4-a716-000000000003', 'Maintenance');

-- Insert 100 Seats for Room 1
-- Get the first room ID (adjust as needed)
DO $$
DECLARE
  v_room_id UUID;
  v_seat_status_id UUID := '550e8400-e29b-41d4-a716-000000000001';
  v_row INT;
  v_col INT;
  v_seat_code VARCHAR(10);
BEGIN
  -- Get first room ID
  SELECT "RoomId" INTO v_room_id FROM "Rooms" LIMIT 1;
  
  IF v_room_id IS NULL THEN
  RAISE NOTICE 'No rooms found. Please create a room first.';
    RETURN;
  END IF;
  
  -- Insert 100 seats (10 rows x 10 columns)
  FOR v_row IN 1..10 LOOP
    FOR v_col IN 1..10 LOOP
      v_seat_code := CHR(64 + v_row) || LPAD(v_col::TEXT, 2, '0');
      
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
    VALUES (
    gen_random_uuid(),
        v_room_id,
        v_seat_status_id,
        v_seat_code,
        v_row,
        v_col,
        CASE 
          WHEN v_row IN (1, 10) AND v_col IN (1, 10) THEN 'VIP'
          WHEN v_row IN (5, 6) AND v_col BETWEEN 4 AND 7 THEN 'VIP'
          ELSE 'Standard'
END,
        NOW()
      );
    END LOOP;
  END LOOP;
  
  RAISE NOTICE 'Inserted 100 seats successfully!';
END $$;

-- Verify inserted data
SELECT COUNT(*) as "TotalSeats" FROM "Seats";
SELECT * FROM "SeatStatuses" ORDER BY "SeatStatusId";
