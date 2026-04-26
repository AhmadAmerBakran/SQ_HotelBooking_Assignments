# Mini Project Part 2 – Black-box testing for Create Booking

This document derives black-box test cases for the **Create booking** feature.

## Feature under test
A hotel room can be booked for a period (**start date – end date**) in the future provided that it is not already booked for one or more days during the desired period.

## Chosen black-box techniques
1. **Equivalence Partitioning**
2. **Boundary Value Analysis**
3. **Decision table thinking** for final outcome (booking created / rejected)

## Business rules used
- The start date must be in the **future**.
- The start date must **not be later** than the end date.
- A booking can be created only if **at least one room is available** in the requested period.
- If all rooms are occupied in the requested period, the booking must be rejected.

## Equivalence classes

### Valid classes
- V1: Start date is in the future and end date is the same date or later.
- V2: Requested period does not collide with a fully occupied period.

### Invalid classes
- I1: Start date is today.
- I2: Start date is in the past.
- I3: Start date is later than end date.
- I4: Requested period overlaps a period where all rooms are occupied.

## Boundary values
- B1: start date = today (invalid)
- B2: start date = today + 1 day (valid lower boundary for future)
- B3: start date = end date (single-day booking)
- B4: start date = end date + 1 day (invalid)
- B5: first free day immediately after the seeded occupied period

## Decision table summary
| Future start date? | Start <= End? | Any room available? | Expected result |
|---|---|---|---|
| Yes | Yes | Yes | Booking created (201) |
| Yes | Yes | No | Rejected as conflict (409) |
| No | Any | Any | Rejected as bad request (400) |
| Yes | No | Any | Rejected as bad request (400) |

## Derived concrete test cases
| ID | Technique | Test data (relative to today) | Expected |
|---|---|---|---|
| TC1 | EP | start=+30, end=+31 | 201 Created |
| TC2 | EP | start=+5, end=+6 (inside seeded fully occupied period) | 409 Conflict |
| TC3 | BVA | start=0, end=+1 | 400 Bad Request |
| TC4 | EP/BVA | start=-1, end=+2 | 400 Bad Request |
| TC5 | BVA | start=+10, end=+9 | 400 Bad Request |
| TC6 | BVA | start=+19, end=+20 (first free day after seeded conflict window) | 201 Created |

## Why these cases are sufficient for part 2
- They cover all relevant valid and invalid input partitions.
- They include boundary values around the future-date rule and the date-range rule.
- They directly support both the **Reqnroll/Cucumber** tests and the **Postman API** tests.
