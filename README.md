# HotelBooking_Clean_Async
 Async version of HotelBooking_Clean


## Assignment Part 2 additions

The solution now includes:
- `AssignmentPart2_BlackBox_TestCases.md` with the derived black-box test cases
- `HotelBooking.BddTests` with Reqnroll/Cucumber scenarios for **Create booking**
- `Postman/HotelBooking_CreateBooking_AssignmentPart2.postman_collection.json` for automated API testing of **Create booking**

Note: `BookingsController.Post` was updated to return **400 Bad Request** for invalid date input instead of exposing an unhandled exception.
