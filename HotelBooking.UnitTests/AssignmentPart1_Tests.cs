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
}