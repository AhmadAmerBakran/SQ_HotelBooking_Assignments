using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests;

public class AssignmentPart1_Tests
{
    private static DateTime D(int daysFromToday) => DateTime.Today.AddDays(daysFromToday);

    private static Booking B(int roomId, DateTime start, DateTime end, bool active = true) =>
        new Booking { RoomId = roomId, StartDate = start, EndDate = end, IsActive = active };
    
    private static List<Room> Rooms(params int[] ids) =>
        ids.Select(id => new Room { Id = id, Description = $"Room {id}" }).ToList();
    
    
    // Feature 1: FindAvailableRoom - input validation

    [Theory]
    // startDate <= Today should throw exception
    [InlineData(0, 0)]
    // startDate in past should throw exception
    [InlineData(-1, 2)]
    // startDate > endDate should throw exception
    [InlineData(5, 4)]
    public async Task FindAvailableRoom_InvalidDates_ThrowsArgumentException(int startOffset, int endOffset)
    {
        // Arrange
        var bookingRepo = new Mock<IRepository<Booking>>(MockBehavior.Strict);
        var roomRepo = new Mock<IRepository<Room>>(MockBehavior.Strict);

        bookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking>());
        roomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Rooms(1));

        var sut = new BookingManager(bookingRepo.Object, roomRepo.Object);

        // Act
        Task act() => sut.FindAvailableRoom(D(startOffset), D(endOffset));

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(act);
    }
    
    // Feature 1 + 2: FindAvailableRoom (DATA-DRIVEN)
        
    [Theory]
    // Existing active booking for room 1: [Today+10, Today+15]
    // Request is for [Today+X, Today+Y]
    // expectedRoomId: 1 if available else -1
    [InlineData( 1,  2,  1)]  
    [InlineData(16, 17,  1)]  
    [InlineData(12, 14, -1)]  
    [InlineData( 9, 11, -1)]  
    [InlineData(14, 16, -1)]  
    [InlineData(10, 15, -1)]  
    [InlineData( 5, 30, -1)]  
    [InlineData( 8, 10, -1)]  
    [InlineData(15, 18, -1)]  
    
    public async Task FindAvailableRoom_Overlap_DataDriven(int reqStartOffset, int reqEndOffset, int expectedRoomId)
    {
        // Arrange
        var rooms = Rooms(1);

        var existingStart = D(10);
        var existingEnd = D(15);

        var bookings = new List<Booking>
        {
            B(roomId: 1, start: existingStart, end: existingEnd, active: true)
        };

        var bookingRepo = new Mock<IRepository<Booking>>(MockBehavior.Strict);
        var roomRepo = new Mock<IRepository<Room>>(MockBehavior.Strict);

        bookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
        roomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);

        var sut = new BookingManager(bookingRepo.Object, roomRepo.Object);

        // Act
        var roomId = await sut.FindAvailableRoom(D(reqStartOffset), D(reqEndOffset));

        // Assert
        Assert.Equal(expectedRoomId, roomId);
    }
}