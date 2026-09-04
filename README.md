CREATE TABLE Enrolments
(
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Confirmed',
    RaceNumber NVARCHAR(20),

    CONSTRAINT FK_Enrolments_Participant
        FOREIGN KEY (ParticipantID)
        REFERENCES Users(UserID),

    CONSTRAINT FK_Enrolments_Category
        FOREIGN KEY (CategoryID)
        REFERENCES Categories(CategoryID),

    CONSTRAINT UQ_Enrolments_Participant_Category
        UNIQUE (ParticipantID, CategoryID)
);
GO
