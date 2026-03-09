-- Thêm d? li?u Cinema test
INSERT INTO "Cinemas" ("CinemaId", "Name", "Address", "Status", "CreatedAt") VALUES
(gen_random_uuid(), 'Beta Thanh Xuân', '56 Nguyen Trai, Thanh Xuân, Hà N?i', 1, NOW()),
(gen_random_uuid(), 'Beta M? ?ình', '25 Lê ??c Th?, M? ?ình, Hà N?i', 1, NOW()),
(gen_random_uuid(), 'Beta Quang Trung', '123 Quang Trung, Gò V?p, TP.HCM', 1, NOW()),
(gen_random_uuid(), 'Beta Golden Land', '275 Nguy?n Trãi, Thanh Xuân, Hà N?i', 1, NOW())
ON CONFLICT DO NOTHING;

-- Thêm d? li?u Room test cho t?ng cinema
WITH cinema_data AS (
    SELECT "CinemaId", "Name" FROM "Cinemas" WHERE "Status" = 1
)
INSERT INTO "Rooms" ("RoomId", "CinemaId", "Name", "TotalRows", "TotalCols", "Status", "CreatedAt")
SELECT 
    gen_random_uuid(),
    cd."CinemaId",
    'Room ' || r.room_num,
    8, -- TotalRows
    10, -- TotalCols  
    1,
    NOW()
FROM cinema_data cd
CROSS JOIN generate_series(1, 3) AS r(room_num)
ON CONFLICT DO NOTHING;

-- Thêm phim test
INSERT INTO "Movies" ("MovieId", "Title", "DurationMin", "Description", "PosterUrl", "Director", "Actors", "Genre", "Language", "ReleaseDate", "Status", "CreatedAt") VALUES
(gen_random_uuid(), 'Avengers: Endgame', 180, 'Cu?c chi?n cu?i cùng c?a các siêu anh hùng', 'https://via.placeholder.com/300x440?text=Avengers', 'Russo Brothers', 'Robert Downey Jr., Chris Evans', 'Hành ??ng, Khoa h?c vi?n t??ng', 'Ti?ng Anh', '2024-01-15', 1, NOW()),
(gen_random_uuid(), 'Spider-Man: No Way Home', 148, 'Peter Parker ph?i ??i m?t v?i nh?ng k? thù t? các v? tr? khác', 'https://via.placeholder.com/300x440?text=Spider-Man', 'Jon Watts', 'Tom Holland, Zendaya', 'Hành ??ng, Phiêu l?u', 'Ti?ng Anh', '2024-02-01', 1, NOW()),
(gen_random_uuid(), 'Black Widow', 134, 'Natasha Romanoff ??i m?t v?i quá kh? ?en t?i', 'https://via.placeholder.com/300x440?text=Black-Widow', 'Cate Shortland', 'Scarlett Johansson, Florence Pugh', 'Hành ??ng, Phiêu l?u', 'Ti?ng Anh', '2024-01-20', 1, NOW())
ON CONFLICT DO NOTHING;

-- Thêm ShowTimes test
WITH movie_room_data AS (
    SELECT 
        m."MovieId",
        r."RoomId",
        r."Name" as room_name,
        c."Name" as cinema_name
    FROM "Movies" m
    CROSS JOIN "Rooms" r
    INNER JOIN "Cinemas" c ON r."CinemaId" = c."CinemaId"
    WHERE m."Status" = 1 AND r."Status" = 1 AND c."Status" = 1
)
INSERT INTO "ShowTimes" ("ShowTimeId", "MovieId", "RoomId", "StartAt", "EndAt", "BasePrice", "Format", "Status", "CreatedAt")
SELECT 
  gen_random_uuid(),
    mrd."MovieId",
    mrd."RoomId",
    (CURRENT_DATE + INTERVAL '1 day') + (st.hour_offset || ' hours')::INTERVAL,
    (CURRENT_DATE + INTERVAL '1 day') + (st.hour_offset || ' hours')::INTERVAL + INTERVAL '2 hours 30 minutes',
    CASE 
        WHEN st.hour_offset::INT < 12 THEN 80000
        WHEN st.hour_offset::INT < 18 THEN 100000
        ELSE 120000
    END,
    CASE 
        WHEN st.hour_offset::INT % 2 = 0 THEN '2D PH? ?? VI?T'
        ELSE 'IMAX 2D'
    END,
    1,
    NOW()
FROM movie_room_data mrd
CROSS JOIN (VALUES ('9'), ('12'), ('15'), ('18'), ('21')) AS st(hour_offset)
ON CONFLICT DO NOTHING;

-- Thêm User test
INSERT INTO "Users" ("UserId", "Email", "FullName", "Phone", "Status", "CreatedAt") VALUES
(gen_random_uuid(), 'test@example.com', 'Test User', '0123456789', 1, NOW())
ON CONFLICT ("Email") DO NOTHING;