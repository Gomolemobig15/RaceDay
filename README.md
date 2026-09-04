# RaceDay

## Project Overview

RaceDay is a web-based event management system designed for South African road running, walking, and cycling events.

The system is designed to allow event organisers to create and manage events, categories, routes, and participant results. Participants can register for events, enrol in event categories, and view their results.

This project is being developed progressively. Part 1 focuses on the planning, database design, API endpoint planning, documentation, and CI/CD validation.

## User Roles

### Organiser

The Organiser is responsible for managing sporting events on the RaceDay system.

An Organiser can:

- Create events
- Update event information
- Delete events
- Create and manage event categories
- View participant enrolments
- Record and update participant results
- Manage event routes

### Participant

The Participant uses the system to participate in events.

A Participant can:

- Register for an account
- Log into the system
- View available events
- View event categories
- Enrol in an event category
- View their enrolments
- View their own results
- Update their profile information

## Part 1 – Design and Planning

Part 1 focuses on designing and planning the RaceDay system before development of the ASP.NET API.

The main deliverables for Part 1 are:

- Entity Relationship Diagram (ERD)
- API Endpoint Plan
- SQL Server database script
- README documentation
- GitHub Actions validation workflow
- Sample database data
- GitHub version control history

## Database Design

The RaceDay database contains the following main entities:

- Users
- Venues
- Events
- Categories
- Enrolments
- Results
- Routes

### Main Relationships

The database relationships include:

- One Organiser can manage many Events.
- One Venue can be used by many Events.
- One Event can contain many Categories.
- One Participant can have many Enrolments.
- One Category can have many Enrolments.
- One Enrolment can have zero or one Result.
- One Event can have zero or one Route.

The database uses primary keys and foreign keys to maintain relationships between entities.

Additional constraints such as `NOT NULL`, `UNIQUE`, `DEFAULT`, and `CHECK` constraints are used to improve data integrity.

## Database Script

The SQL database script is located in:

`docs/RaceDayDatabase.sql`

The script contains:

- Database creation
- Table creation
- Primary keys
- Foreign keys
- Data constraints
- Sample Organisers
- Sample Participants
- Sample Events
- Sample Categories
- Sample Enrolments
- Sample Results
- Sample Routes

The SQL script is designed to be tested using SQL Server Management Studio (SSMS).

## API Endpoint Plan

The planned API endpoints are documented in:

`docs/APIEndpointPlan.pdf`

The endpoint plan covers functionality such as:

- User registration
- User login
- User profiles
- Event management
- Category management
- Participant enrolments
- Results
- Event routes

The API will be developed in a later part of the project.

## Repository Structure

The planned repository structure is:

RaceDay
├── docs
│   ├── ERD.pdf
│   ├── APIEndpointPlan.pdf
│   └── RaceDayDatabase.sql
├── README.md
└── .github
    └── workflows
        └── validate.yml
