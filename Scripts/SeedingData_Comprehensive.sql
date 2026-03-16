-- ============================================================
-- COMPREHENSIVE SEEDING DATA SCRIPT
-- Cinema Management Database - Full Data
-- ============================================================

-- ============================================================
-- 1. SEED SEATSTATUS
-- ============================================================
INSERT INTO "SeatStatuses" ("SeatStatusId", "StatusName")
VALUES 
('550e8400-e29b-41d4-a716-000000000001', 'Active'),
('550e8400-e29b-41d4-a716-000000000002', 'Inactive'),
('550e8400-e29b-41d4-a716-000000000003', 'Maintenance')
ON CONFLICT DO NOTHING;

-- ============================================================
-- 2. SEED ROLES
-- ============================================================
INSERT INTO "Roles" ("RoleId", "Name")
VALUES 
('550e8400-e29b-41d4-a716-000000000101', 'Admin'),
('550e8400-e29b-41d4-a716-000000000102', 'User'),
('550e8400-e29b-41d4-a716-000000000103', 'Staff')
ON CONFLICT DO NOTHING;

-- ============================================================
-- 3. SEED USERS (5 users)
-- ============================================================
INSERT INTO "Users" ("UserId", "Email", "FullName", "Phone", "PasswordHash", "Status", "CreatedAt")
VALUES 
('550e8400-e29b-41d4-a716-000000000201', 'admin@cinema.vn', 'Admin User', '0901234567', 'hashed_password_123', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000202', 'user1@cinema.vn', 'Nguy?n V?n A', '0912345678', 'hashed_password_123', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000203', 'user2@cinema.vn', 'Tr?n Th? B', '0923456789', 'hashed_password_123', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000204', 'user3@cinema.vn', 'Ph?m V?n C', '0934567890', 'hashed_password_123', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000205', 'user4@cinema.vn', 'Lê Th? D', '0945678901', 'hashed_password_123', 1, NOW())
ON CONFLICT DO NOTHING;

-- ============================================================
-- 4. ASSIGN ROLES TO USERS
-- ============================================================
INSERT INTO "UserRoles" ("UserId", "RoleId")
VALUES 
('550e8400-e29b-41d4-a716-000000000201', '550e8400-e29b-41d4-a716-000000000101'), -- Admin has Admin role
('550e8400-e29b-41d4-a716-000000000202', '550e8400-e29b-41d4-a716-000000000102'), -- Users have User role
('550e8400-e29b-41d4-a716-000000000203', '550e8400-e29b-41d4-a716-000000000102'),
('550e8400-e29b-41d4-a716-000000000204', '550e8400-e29b-41d4-a716-000000000102'),
('550e8400-e29b-41d4-a716-000000000205', '550e8400-e29b-41d4-a716-000000000102')
ON CONFLICT DO NOTHING;

-- ============================================================
-- 5. SEED CINEMAS (5 cinemas in different cities)
-- ============================================================
INSERT INTO "Cinemas" ("CinemaId", "Name", "Address", "Status", "CreatedAt")
VALUES 
('550e8400-e29b-41d4-a716-000000000301', 'Galaxy Nguy?n Trãi', '116 Nguy?n Trãi, Hà N?i', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000302', 'Galaxy Tây H?', '83 Tây H?, Hà N?i', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000303', 'Galaxy Hai Bà Tr?ng', '190 Hai Bà Tr?ng, Hà N?i', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000304', 'Galaxy Crescent Mall', '50 T? H?u, Qu?ng Ninh', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000305', 'Galaxy Fiesta', '393 Nguy?n H?u C?nh, Bình D??ng', 1, NOW())
ON CONFLICT DO NOTHING;

-- ============================================================
-- 6. SEED ROOMS (3 rooms per cinema = 15 rooms)
-- ============================================================
INSERT INTO "Rooms" ("RoomId", "CinemaId", "Name", "TotalRows", "TotalCols", "Status", "CreatedAt")
VALUES 
-- Cinema 1 (Galaxy Nguy?n Trãi)
('550e8400-e29b-41d4-a716-000000000401', '550e8400-e29b-41d4-a716-000000000301', 'Phòng 1', 10, 10, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000402', '550e8400-e29b-41d4-a716-000000000301', 'Phòng 2', 10, 10, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000403', '550e8400-e29b-41d4-a716-000000000301', 'Phòng 3', 10, 10, 1, NOW()),
-- Cinema 2 (Galaxy Tây H?)
('550e8400-e29b-41d4-a716-000000000404', '550e8400-e29b-41d4-a716-000000000302', 'Phòng 1', 10, 10, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000405', '550e8400-e29b-41d4-a716-000000000302', 'Phòng 2', 10, 10, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000406', '550e8400-e29b-41d4-a716-000000000302', 'Phòng 3', 10, 10, 1, NOW()),
-- Cinema 3 (Galaxy Hai Bà Tr?ng)
('550e8400-e29b-41d4-a716-000000000407', '550e8400-e29b-41d4-a716-000000000303', 'Phòng 1', 10, 10, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000408', '550e8400-e29b-41d4-a716-000000000303', 'Phòng 2', 10, 10, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000409', '550e8400-e29b-41d4-a716-000000000303', 'Phòng 3', 10, 10, 1, NOW()),
-- Cinema 4 (Galaxy Crescent Mall)
('550e8400-e29b-41d4-a716-000000000410', '550e8400-e29b-41d4-a716-000000000304', 'Phòng 1', 10, 10, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000411', '550e8400-e29b-41d4-a716-000000000304', 'Phòng 2', 10, 10, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000412', '550e8400-e29b-41d4-a716-000000000304', 'Phòng 3', 10, 10, 1, NOW()),
-- Cinema 5 (Galaxy Fiesta)
('550e8400-e29b-41d4-a716-000000000413', '550e8400-e29b-41d4-a716-000000000305', 'Phòng 1', 10, 10, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000414', '550e8400-e29b-41d4-a716-000000000305', 'Phòng 2', 10, 10, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000415', '550e8400-e29b-41d4-a716-000000000305', 'Phòng 3', 10, 10, 1, NOW())
ON CONFLICT DO NOTHING;

-- ============================================================
-- 7. SEED SEATS (100 seats per room = 1500 seats total)
-- ============================================================
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
    rooms."RoomId",
    '550e8400-e29b-41d4-a716-000000000001',
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
    (SELECT "RoomId" FROM "Rooms") as rooms,
    (SELECT * FROM generate_series(1, 10) AS row_num) AS rows,
    (SELECT * FROM generate_series(1, 10) AS col_num) AS cols
WHERE NOT EXISTS (
    SELECT 1 FROM "Seats" WHERE "RoomId" = rooms."RoomId"
);

-- ============================================================
-- 8. SEED MOVIES (10 movies)
-- ============================================================
INSERT INTO "Movies" (
    "MovieId", "Title", "Description", "Genre", "Director", "Actors", 
    "Language", "DurationMin", "ReleaseDate", "PosterUrl", "AgeRating", "Status", "CreatedAt"
)
VALUES 
('550e8400-e29b-41d4-a716-000000000501', 'Mùi Ph?', 
    'Câu chuy?n v? th? h? tr?, m?ng m? và nh?ng b??c ngo?t trong cu?c s?ng.', 
    'Tình c?m, Hài h??c', 'Beta ??o Di?n', 'Di?n viên A, Di?n viên B', 
    'Ti?ng Vi?t', 111, '2024-03-08', 'https://via.placeholder.com/300x440?text=Mui+Pho', 13, 1, NOW()),

('550e8400-e29b-41d4-a716-000000000502', 'Kung Fu Panda 4', 
    'Chú g?u trúc Po ti?p t?c hành trình b?o v? Thung l?ng Hòa Bình.', 
    'Ho?t hình, Hành ??ng', 'Mike Mitchell', 'Jack Black, Angelina Jolie', 
    'Ti?ng Anh', 94, '2024-03-22', 'https://via.placeholder.com/300x440?text=Kung+Fu+Panda', 7, 1, NOW()),

('550e8400-e29b-41d4-a716-000000000503', 'Dune: Part Two', 
    'Paul Atreides ti?p t?c cu?c chi?n trên hành tinh sa m?c Arrakis.', 
    'Khoa h?c vi?n t??ng', 'Denis Villeneuve', 'Timothée Chalamet, Zendaya', 
    'Ti?ng Anh', 166, '2024-02-28', 'https://via.placeholder.com/300x440?text=Dune+Part+Two', 13, 1, NOW()),

('550e8400-e29b-41d4-a716-000000000504', 'The Fall Guy', 
    'M?t stuntman c? g?ng gi?i quy?t bí ?n v? cái ch?t c?a m?t b?n.', 
    'Hành ??ng, Hài h??c', 'David Leitch', 'Ryan Gosling, Emily Blunt', 
    'Ti?ng Anh', 126, '2024-04-12', 'https://via.placeholder.com/300x440?text=The+Fall+Guy', 13, 1, NOW()),

('550e8400-e29b-41d4-a716-000000000505', 'Godzilla x Kong: The New Empire', 
    'Hai quái v?t huy?n tho?i ??i ??u nhau trong cu?c chi?n vô t?n.', 
    'Khoa h?c vi?n t??ng, Hành ??ng', 'Adam Wingard', 'Rebecca Hall, Brian Cranston', 
    'Ti?ng Anh', 114, '2024-03-29', 'https://via.placeholder.com/300x440?text=Godzilla+Kong', 13, 1, NOW()),

('550e8400-e29b-41d4-a716-000000000506', 'Captain America: Brave New World', 
    'Steve Rogers tr? l?i ?? b?o v? ??t n??c kh?i m?i ?e d?a m?i.', 
    'Siêu anh hùng, Hành ??ng', 'Julius Onah', 'Anthony Mackie, Harrison Ford', 
 'Ti?ng Anh', 120, '2025-02-14', 'https://via.placeholder.com/300x440?text=Captain+America', 13, 1, NOW()),

('550e8400-e29b-41d4-a716-000000000507', 'Inside Out 2', 
    'Riley b??c vào tu?i thi?u niên v?i nh?ng c?m xúc m?i.', 
    'Ho?t hình, Tâm lý', 'Kelsey Mann', 'Amy Poehler, Phyllis Smith', 
    'Ti?ng Anh', 96, '2024-06-14', 'https://via.placeholder.com/300x440?text=Inside+Out+2', 7, 1, NOW()),

('550e8400-e29b-41d4-a716-000000000508', 'Deadpool & Wolverine', 
    'Hai anh hùng l?p d? k?t h?p ?? c?u th? gi?i.', 
    'Siêu anh hùng, Hành ??ng, Hài h??c', 'Shawn Levy', 'Ryan Reynolds, Hugh Jackman', 
    'Ti?ng Anh', 128, '2024-07-26', 'https://via.placeholder.com/300x440?text=Deadpool+Wolverine', 16, 1, NOW()),

('550e8400-e29b-41d4-a716-000000000509', 'Transformers: Rise of the Beasts', 
    'Autobots ti?p t?c cu?c chi?n ch?ng l?i Decepticons.', 
    'Khoa h?c vi?n t??ng, Hành ??ng', 'Steven Caple Jr.', 'Anthony Ramos, Dominique Fishback', 
    'Ti?ng Anh', 127, '2023-06-09', 'https://via.placeholder.com/300x440?text=Transformers', 13, 1, NOW()),

('550e8400-e29b-41d4-a716-000000000510', 'Oppenheimer', 
    'Câu chuy?n v? nhà khoa h?c Julius Robert Oppenheimer và bom nguyên t?.', 
    'L?ch s?, Chính k?ch', 'Christopher Nolan', 'Cillian Murphy, Emily Blunt', 
    'Ti?ng Anh', 180, '2023-07-21', 'https://via.placeholder.com/300x440?text=Oppenheimer', 13, 1, NOW())
ON CONFLICT DO NOTHING;

-- ============================================================
-- 9. SEED SHOWTIMES (3-4 showtimes per movie = 35 showtimes)
-- ============================================================
INSERT INTO "ShowTimes" (
    "ShowTimeId", "MovieId", "RoomId", "StartAt", "EndAt", "BasePrice", "Format", "Status", "CreatedAt"
)
VALUES 
-- Movie 1: Mùi Ph?
('550e8400-e29b-41d4-a716-000000000601', '550e8400-e29b-41d4-a716-000000000501', '550e8400-e29b-41d4-a716-000000000401', '2024-03-10 10:00:00+00', '2024-03-10 11:51:00+00', 120000, '2D PH? ?? VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000602', '550e8400-e29b-41d4-a716-000000000501', '550e8400-e29b-41d4-a716-000000000401', '2024-03-10 13:00:00+00', '2024-03-10 14:51:00+00', 120000, '2D PH? ?? VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000603', '550e8400-e29b-41d4-a716-000000000501', '550e8400-e29b-41d4-a716-000000000401', '2024-03-10 16:00:00+00', '2024-03-10 17:51:00+00', 130000, '3D PH? ?? VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000604', '550e8400-e29b-41d4-a716-000000000501', '550e8400-e29b-41d4-a716-000000000401', '2024-03-10 19:00:00+00', '2024-03-10 20:51:00+00', 130000, '3D PH? ?? VI?T', 1, NOW()),

-- Movie 2: Kung Fu Panda 4
('550e8400-e29b-41d4-a716-000000000605', '550e8400-e29b-41d4-a716-000000000502', '550e8400-e29b-41d4-a716-000000000402', '2024-03-10 09:00:00+00', '2024-03-10 10:34:00+00', 100000, '2D L?NG TI?NG VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000606', '550e8400-e29b-41d4-a716-000000000502', '550e8400-e29b-41d4-a716-000000000402', '2024-03-10 14:00:00+00', '2024-03-10 15:34:00+00', 100000, '2D L?NG TI?NG VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000607', '550e8400-e29b-41d4-a716-000000000502', '550e8400-e29b-41d4-a716-000000000402', '2024-03-10 17:30:00+00', '2024-03-10 19:04:00+00', 110000, '3D L?NG TI?NG VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000608', '550e8400-e29b-41d4-a716-000000000502', '550e8400-e29b-41d4-a716-000000000402', '2024-03-10 21:00:00+00', '2024-03-10 22:34:00+00', 110000, '3D L?NG TI?NG VI?T', 1, NOW()),

-- Movie 3: Dune: Part Two (thêm nhi?u su?t)
('550e8400-e29b-41d4-a716-000000000609', '550e8400-e29b-41d4-a716-000000000503', '550e8400-e29b-41d4-a716-000000000403', '2024-03-10 10:30:00+00', '2024-03-10 12:46:00+00', 150000, '2D PH? ?? VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000610', '550e8400-e29b-41d4-a716-000000000503', '550e8400-e29b-41d4-a716-000000000403', '2024-03-10 14:00:00+00', '2024-03-10 16:16:00+00', 150000, '2D PH? ?? VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000611', '550e8400-e29b-41d4-a716-000000000503', '550e8400-e29b-41d4-a716-000000000403', '2024-03-10 17:00:00+00', '2024-03-10 19:16:00+00', 160000, 'IMAX 2D', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000612', '550e8400-e29b-41d4-a716-000000000503', '550e8400-e29b-41d4-a716-000000000403', '2024-03-10 20:00:00+00', '2024-03-10 22:16:00+00', 160000, 'IMAX 2D', 1, NOW()),

-- Movie 4: The Fall Guy
('550e8400-e29b-41d4-a716-000000000613', '550e8400-e29b-41d4-a716-000000000504', '550e8400-e29b-41d4-a716-000000000404', '2024-03-10 11:00:00+00', '2024-03-10 12:46:00+00', 120000, '2D PH? ?? VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000614', '550e8400-e29b-41d4-a716-000000000504', '550e8400-e29b-41d4-a716-000000000404', '2024-03-10 15:00:00+00', '2024-03-10 16:46:00+00', 120000, '2D PH? ?? VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000615', '550e8400-e29b-41d4-a716-000000000504', '550e8400-e29b-41d4-a716-000000000404', '2024-03-10 18:30:00+00', '2024-03-10 20:16:00+00', 130000, '3D PH? ?? VI?T', 1, NOW()),

-- Movie 5: Godzilla x Kong
('550e8400-e29b-41d4-a716-000000000616', '550e8400-e29b-41d4-a716-000000000505', '550e8400-e29b-41d4-a716-000000000405', '2024-03-10 09:30:00+00', '2024-03-10 11:24:00+00', 130000, '2D PH? ?? VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000617', '550e8400-e29b-41d4-a716-000000000505', '550e8400-e29b-41d4-a716-000000000405', '2024-03-10 13:00:00+00', '2024-03-10 14:54:00+00', 130000, '2D PH? ?? VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000618', '550e8400-e29b-41d4-a716-000000000505', '550e8400-e29b-41d4-a716-000000000405', '2024-03-10 16:30:00+00', '2024-03-10 18:24:00+00', 140000, 'IMAX 3D', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000619', '550e8400-e29b-41d4-a716-000000000505', '550e8400-e29b-41d4-a716-000000000405', '2024-03-10 19:00:00+00', '2024-03-10 20:54:00+00', 140000, 'IMAX 3D', 1, NOW()),

-- Movie 6-10 (simplified - 2 showtimes each)
('550e8400-e29b-41d4-a716-000000000620', '550e8400-e29b-41d4-a716-000000000506', '550e8400-e29b-41d4-a716-000000000406', '2024-03-10 14:00:00+00', '2024-03-10 15:40:00+00', 130000, '2D PH? ?? VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000621', '550e8400-e29b-41d4-a716-000000000506', '550e8400-e29b-41d4-a716-000000000406', '2024-03-10 19:00:00+00', '2024-03-10 20:40:00+00', 140000, '3D PH? ?? VI?T', 1, NOW()),

('550e8400-e29b-41d4-a716-000000000622', '550e8400-e29b-41d4-a716-000000000507', '550e8400-e29b-41d4-a716-000000000407', '2024-03-10 10:00:00+00', '2024-03-10 11:36:00+00', 110000, '2D L?NG TI?NG VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000623', '550e8400-e29b-41d4-a716-000000000507', '550e8400-e29b-41d4-a716-000000000407', '2024-03-10 16:00:00+00', '2024-03-10 17:36:00+00', 110000, '2D L?NG TI?NG VI?T', 1, NOW()),

('550e8400-e29b-41d4-a716-000000000624', '550e8400-e29b-41d4-a716-000000000508', '550e8400-e29b-41d4-a716-000000000408', '2024-03-10 13:00:00+00', '2024-03-10 14:48:00+00', 130000, '2D PH? ?? VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000625', '550e8400-e29b-41d4-a716-000000000508', '550e8400-e29b-41d4-a716-000000000408', '2024-03-10 18:30:00+00', '2024-03-10 20:18:00+00', 140000, '3D PH? ?? VI?T', 1, NOW()),

('550e8400-e29b-41d4-a716-000000000626', '550e8400-e29b-41d4-a716-000000000509', '550e8400-e29b-41d4-a716-000000000409', '2024-03-10 11:00:00+00', '2024-03-10 12:47:00+00', 130000, '2D PH? ?? VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000627', '550e8400-e29b-41d4-a716-000000000509', '550e8400-e29b-41d4-a716-000000000409', '2024-03-10 17:00:00+00', '2024-03-10 18:47:00+00', 140000, '3D PH? ?? VI?T', 1, NOW()),

('550e8400-e29b-41d4-a716-000000000628', '550e8400-e29b-41d4-a716-000000000510', '550e8400-e29b-41d4-a716-000000000410', '2024-03-10 15:00:00+00', '2024-03-10 18:00:00+00', 150000, '2D PH? ?? VI?T', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000629', '550e8400-e29b-41d4-a716-000000000510', '550e8400-e29b-41d4-a716-000000000410', '2024-03-10 19:00:00+00', '2024-03-10 22:00:00+00', 150000, '2D PH? ?? VI?T', 1, NOW())
ON CONFLICT DO NOTHING;

-- ============================================================
-- 10. SEED SHOWTIMESEATS (Mark some seats as booked)
-- ============================================================
-- Sample: Mark 10 random seats as booked for first 5 showtimes
INSERT INTO "ShowTimeSeats" ("ShowTimeId", "SeatId", "Status")
SELECT 
    st."ShowTimeId",
    s."SeatId",
    1 -- Status 1 = Booked
FROM "ShowTimes" st
JOIN "Seats" s ON s."RoomId" = st."RoomId"
WHERE st."ShowTimeId" IN (
    '550e8400-e29b-41d4-a716-000000000601',
  '550e8400-e29b-41d4-a716-000000000602',
    '550e8400-e29b-41d4-a716-000000000603',
    '550e8400-e29b-41d4-a716-000000000604',
    '550e8400-e29b-41d4-a716-000000000605'
)
AND s."SeatCode" IN ('A01', 'A02', 'A03', 'B01', 'B02', 'C01', 'C02', 'D01', 'D02', 'E01')
ON CONFLICT DO NOTHING;

-- ============================================================
-- 11. SEED BOOKINGS (5 bookings)
-- ============================================================
INSERT INTO "Bookings" (
    "BookingId", "BookingCode", "UserId", "ShowTimeId", "TotalAmount", "Status", "CreatedAt"
)
VALUES 
('550e8400-e29b-41d4-a716-000000000701', 'BK001', '550e8400-e29b-41d4-a716-000000000202', '550e8400-e29b-41d4-a716-000000000601', 240000, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000702', 'BK002', '550e8400-e29b-41d4-a716-000000000203', '550e8400-e29b-41d4-a716-000000000602', 360000, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000703', 'BK003', '550e8400-e29b-41d4-a716-000000000204', '550e8400-e29b-41d4-a716-000000000603', 260000, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000704', 'BK004', '550e8400-e29b-41d4-a716-000000000205', '550e8400-e29b-41d4-a716-000000000604', 390000, 1, NOW()),
('550e8400-e29b-41d4-a716-000000000705', 'BK005', '550e8400-e29b-41d4-a716-000000000202', '550e8400-e29b-41d4-a716-000000000605', 200000, 1, NOW())
ON CONFLICT DO NOTHING;

-- ============================================================
-- 12. SEED TICKETS (10 tickets = 2 per booking)
-- ============================================================
INSERT INTO "Tickets" (
    "TicketId", "BookingId", "ShowTimeId", "SeatId", "TicketCode", "UnitPrice", "Status", "CreatedAt"
)
SELECT 
    gen_random_uuid(),
    b."BookingId",
    b."ShowTimeId",
    s."SeatId",
 'TK' || LPAD(ROW_NUMBER() OVER (ORDER BY b."BookingId")::TEXT, 6, '0'),
    st."BasePrice",
    true,
    NOW()
FROM "Bookings" b
JOIN "ShowTimes" st ON st."ShowTimeId" = b."ShowTimeId"
JOIN "Seats" s ON s."RoomId" = st."RoomId"
WHERE s."SeatCode" IN ('A01', 'A02', 'A03', 'B01', 'B02', 'C01', 'C02', 'D01', 'D02', 'E01')
LIMIT 10
ON CONFLICT DO NOTHING;

-- ============================================================
-- 13. SEED PAYMENTS (5 payments = 1 per booking)
-- ============================================================
INSERT INTO "Payments" (
    "PaymentId", "BookingId", "Amount", "Method", "Status", "PaidAt"
)
VALUES 
('550e8400-e29b-41d4-a716-000000000801', '550e8400-e29b-41d4-a716-000000000701', 240000, 'Credit Card', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000802', '550e8400-e29b-41d4-a716-000000000702', 360000, 'Debit Card', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000803', '550e8400-e29b-41d4-a716-000000000703', 260000, 'E-Wallet', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000804', '550e8400-e29b-41d4-a716-000000000704', 390000, 'Bank Transfer', 1, NOW()),
('550e8400-e29b-41d4-a716-000000000805', '550e8400-e29b-41d4-a716-000000000705', 200000, 'Credit Card', 1, NOW())
ON CONFLICT DO NOTHING;

-- ============================================================
-- VERIFICATION QUERIES
-- ============================================================
SELECT '=== SEEDING COMPLETE ===' as Status;
SELECT COUNT(*) as "Total SeatStatuses" FROM "SeatStatuses";
SELECT COUNT(*) as "Total Roles" FROM "Roles";
SELECT COUNT(*) as "Total Users" FROM "Users";
SELECT COUNT(*) as "Total Cinemas" FROM "Cinemas";
SELECT COUNT(*) as "Total Rooms" FROM "Rooms";
SELECT COUNT(*) as "Total Seats" FROM "Seats";
SELECT COUNT(*) as "Total Movies" FROM "Movies";
SELECT COUNT(*) as "Total ShowTimes" FROM "ShowTimes";
SELECT COUNT(*) as "Total Bookings" FROM "Bookings";
SELECT COUNT(*) as "Total Tickets" FROM "Tickets";
SELECT COUNT(*) as "Total Payments" FROM "Payments";
SELECT COUNT(*) as "Total ShowTimeSeats" FROM "ShowTimeSeats" WHERE "Status" > 0;
