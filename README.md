INSERT INTO Events
(
    OrganiserID,
    VenueID,
    EventName,
    Description,
    EventType,
    EventDate,
    StartTime,
    RegistrationDeadline,
    Status
)
VALUES
(
    1,
    1,
    'Joburg Spring Run',
    'Road running event in Johannesburg.',
    'Running',
    '2026-10-10',
    '07:00',
    '2026-10-01',
    'Open'
),
(
    2,
    2,
    'Cape Active Cycle Day',
    'Road cycling event in Cape Town.',
    'Cycling',
    '2026-11-15',
    '06:30',
    '2026-11-01',
    'Open'
),
(
    1,
    3,
    'Durban Beachfront Walk',
    'Community walking event along the Durban beachfront.',
    'Walking',
    '2026-12-05',
    '08:00',
    '2026-11-25',
    'Open'
);
GO
