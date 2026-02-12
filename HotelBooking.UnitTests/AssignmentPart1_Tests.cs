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

    
    
    // Feature 1: CreateBooking - succeeds and calls AddAsync (MOCK + VERIFY)

    [Fact]
    public async Task CreateBooking_WhenRoomAvailable_ReturnsTrue_AndCallsAddAsync()
    {
        // Arrange
        var rooms = Rooms(1);
        var bookings = new List<Booking>(); // if there's no active bookings => room available
        
        var bookingRepo = new Mock<IRepository<Booking>>(MockBehavior.Strict);
        var roomRepo = new Mock<IRepository<Room>>(MockBehavior.Strict);

        bookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
        roomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);

        bookingRepo
            .Setup(r => r.AddAsync(It.IsAny<Booking>()))
            .Returns(Task.CompletedTask);

        var sut = new BookingManager(bookingRepo.Object, roomRepo.Object);

        var request = new Booking
        {
            StartDate = D(5),
            EndDate = D(6),
            CustomerId = 123
        };

        // Act
        var ok = await sut.CreateBooking(request);

        // Assert
        Assert.True(ok);

        bookingRepo.Verify(r => r.AddAsync(It.Is<Booking>(b =>
            b.IsActive == true &&
            b.RoomId == 1 &&
            b.StartDate == request.StartDate &&
            b.EndDate == request.EndDate
        )), Times.Once);
        }

    [Fact]
    public async Task CreateBooking_WhenNoRoomAvailable_ReturnsFalse_AndDoesNotCallAddAsync()
    {
        // Arrange: 1 room with active booking that overlaps request
        var rooms = Rooms(1);

        var existing = B(1, D(10), D(15), active: true);

        var bookingRepo = new Mock<IRepository<Booking>>(MockBehavior.Strict);
        var roomRepo = new Mock<IRepository<Room>>(MockBehavior.Strict);

        bookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking> { existing });
        roomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);

        var sut = new BookingManager(bookingRepo.Object, roomRepo.Object);

        var request = new Booking
        {
            StartDate = D(12),
            EndDate = D(13),
            CustomerId = 5
        };

        // Act
        var ok = await sut.CreateBooking(request);

        // Assert
        Assert.False(ok);
        bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
    }


    // Feature : GetFullyOccupiedDates
    
    [Fact]
    public async Task GetFullyOccupiedDates_WhenStartAfterEnd_ThrowsArgumentException()
    {
        // Arrange
        var bookingRepo = new Mock<IRepository<Booking>>(MockBehavior.Strict);
        var roomRepo = new Mock<IRepository<Room>>(MockBehavior.Strict);

        roomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Rooms(1, 2));
        bookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking>());

        var sut = new BookingManager(bookingRepo.Object, roomRepo.Object);

        // Act
        Task act() => sut.GetFullyOccupiedDates(D(10), D(5));

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_TwoRooms_ReturnsDatesWhereBothRoomsBooked_Inclusive()
    {
        // Arrange
        var rooms = Rooms(1, 2);

        // Room 1 booking: [Today+10, Today+14]
        // Room 2 booking: [Today+12, Today+16]
        // Fully occupied dates in [Today+10, Today+16] are:
        // Today+12, Today+13, Today+14 (inclusive logic)
        var bookings = new List<Booking>
        {
            B(1, D(10), D(14), true),
            B(2, D(12), D(16), true),
        };

        var bookingRepo = new Mock<IRepository<Booking>>(MockBehavior.Strict);
        var roomRepo = new Mock<IRepository<Room>>(MockBehavior.Strict);

        roomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        bookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);

        var sut = new BookingManager(bookingRepo.Object, roomRepo.Object);

        // Act
        var result = await sut.GetFullyOccupiedDates(D(10), D(16));

        // Assert
        var dates = result.Select(x => x.Date).Distinct().OrderBy(x => x).ToList();

        Assert.Equal(3, dates.Count);
        Assert.Contains(D(12).Date, dates);
        Assert.Contains(D(13).Date, dates);
        Assert.Contains(D(14).Date, dates);
    }
    
}