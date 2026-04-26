using HotelBooking.BddTests.Support;
using System.Net;
using System.Net.Http.Json;
using HotelBooking.Core;
using HotelBooking.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Xunit;

namespace HotelBooking.BddTests.StepDefinitions;

[Binding]
public sealed class CreateBookingStepDefinitions
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private Booking? _bookingRequest;
    private HttpResponseMessage? _response;
    private string _responseBody = string.Empty;

    public CreateBookingStepDefinitions(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Given("the hotel booking API is running")]
    public void GivenTheHotelBookingApiIsRunning()
    {
        Assert.NotNull(_client);
    }

    [Given("the booking database is reset to the seeded state")]
    public void GivenTheBookingDatabaseIsResetToTheSeededState()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HotelBookingContext>();
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize(dbContext);
    }

    [Given("I prepare a booking request with start offset (.*) days and end offset (.*) days for customer (.*)")]
    public void GivenIPrepareABookingRequestWithOffsets(int startOffset, int endOffset, int customerId)
    {
        _bookingRequest = new Booking
        {
            StartDate = DateTime.Today.AddDays(startOffset),
            EndDate = DateTime.Today.AddDays(endOffset),
            CustomerId = customerId
        };
    }

    [When("I submit the create booking request")]
    public async Task WhenISubmitTheCreateBookingRequest()
    {
        Assert.NotNull(_bookingRequest);
        _response = await _client.PostAsJsonAsync("/Bookings", _bookingRequest);
        _responseBody = await _response.Content.ReadAsStringAsync();
    }

    [Then("the response status code should be (.*)")]
    public void ThenTheResponseStatusCodeShouldBe(int expectedStatusCode)
    {
        Assert.NotNull(_response);
        Assert.Equal((HttpStatusCode)expectedStatusCode, _response!.StatusCode);
    }

    [Then("the response body should contain \"(.*)\"")]
    public void ThenTheResponseBodyShouldContain(string expectedText)
    {
        Assert.Contains(expectedText, _responseBody);
    }
}
