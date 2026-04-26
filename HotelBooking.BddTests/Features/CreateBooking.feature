@create-booking
Feature: Create booking
  As a hotel customer
  I want to create a booking for a future period
  So that I can reserve a room when at least one room is available

  Background:
    Given the hotel booking API is running

  @equivalence-partitioning @happy-path
  Scenario: Create booking succeeds for a valid future period with availability
    Given the booking database is reset to the seeded state
    And I prepare a booking request with start offset 30 days and end offset 31 days for customer 1
    When I submit the create booking request
    Then the response status code should be 201

  @equivalence-partitioning @all-rooms-occupied
  Scenario: Create booking is rejected when all rooms are occupied in the requested period
    Given the booking database is reset to the seeded state
    And I prepare a booking request with start offset 5 days and end offset 6 days for customer 1
    When I submit the create booking request
    Then the response status code should be 409
    And the response body should contain "All rooms are occupied"

  @boundary-value @invalid-range
  Scenario Outline: Create booking is rejected for invalid date input
    Given the booking database is reset to the seeded state
    And I prepare a booking request with start offset <startOffset> days and end offset <endOffset> days for customer 1
    When I submit the create booking request
    Then the response status code should be 400
    And the response body should contain "The start date cannot be in the past or later than the end date."

    Examples:
      | startOffset | endOffset |
      | 0           | 1         |
      | -1          | 2         |
      | 10          | 9         |

  @boundary-value @first-free-day-after-occupied-period
  Scenario: Create booking succeeds on the first day after the fully occupied seeded period
    Given the booking database is reset to the seeded state
    And I prepare a booking request with start offset 19 days and end offset 20 days for customer 2
    When I submit the create booking request
    Then the response status code should be 201
