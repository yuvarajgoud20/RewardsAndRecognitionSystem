INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
VALUES 
    (NEWID(), 'Admin', 'ADMIN', NEWID()),
    (NEWID(), 'Director', 'DIRECTOR', NEWID()),
    (NEWID(), 'Manager', 'MANAGER', NEWID()),
    (NEWID(), 'TeamLead', 'TEAMLEAD', NEWID()),
    (NEWID(), 'Employee', 'EMPLOYEE', NEWID());