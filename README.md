CREATE TABLE Events
(
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    VenueID INT NOT NULL,
    EventName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(1000),
    EventType NVARCHAR(20) NOT NULL,
    EventDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    RegistrationDeadline DATE NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Events_Organiser
        FOREIGN KEY (OrganiserID)
        REFERENCES Users(UserID),

    CONSTRAINT FK_Events_Venue
        FOREIGN KEY (VenueID)
        REFERENCES Venues(VenueID),

    CONSTRAINT CK_Events_Type
        CHECK (EventType IN ('Running', 'Walking', 'Cycling')),

    CONSTRAINT CK_Events_Status
        CHECK (Status IN ('Open', 'Closed', 'Completed', 'Cancelled'))
);
GO
