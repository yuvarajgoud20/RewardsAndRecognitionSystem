select top 10 * from [dbo].[Nominations]


CREATE PROC dbo.sp_GetNominationData
AS BEGIN

SELECT Description,Achievements FROM [dbo].[Nominations]

END

sp_helptext 'sp_GetNominationData'



sp_helptext 