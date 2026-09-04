CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL,
    FinishTime TIME NOT NULL,
    Position INT,
    ResultStatus NVARCHAR(20) NOT NULL DEFAULT 'Finished',
    RecordedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Results_Enrolment
        FOREIGN KEY (EnrolmentID)
        REFERENCES Enrolments(EnrolmentID),

    CONSTRAINT UQ_Results_Enrolment
        UNIQUE (EnrolmentID)
);
GO
