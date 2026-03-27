using System;

namespace CinemaManagement.Data;

public static class SeatStatusConstants
{
    public static readonly Guid Active = new Guid("550e8400-e29b-41d4-a716-000000000001");
    public static readonly Guid Inactive = new Guid("550e8400-e29b-41d4-a716-000000000002");
    public static readonly Guid Maintenance = new Guid("550e8400-e29b-41d4-a716-000000000003");
}