# NexHire Build Fix Notes

இந்த version-ல் தற்போதைய Visual Studio error list-இல் இருந்த direct compile errors சரிசெய்யப்பட்டுள்ளன.

## சரிசெய்யப்பட்ட direct errors

- `JobConfiguration.cs` line 55/56-ல் இருந்த தவறான `builder.Entity<T>()` calls நீக்கப்பட்டன.
- `JobSeekerConfiguration.cs` line 63-ல் இருந்த தவறான `builder.Entity<T>()` call நீக்கப்பட்டது.
- Child entity indexes தனித்தனி EF Core configuration classes-க்கு மாற்றப்பட்டன:
  - `CandidateSkillConfiguration.cs`
  - `JobRequiredSkillConfiguration.cs`
  - `JobPreferredSkillConfiguration.cs`

`EntityTypeBuilder<Job>` மற்றும் `EntityTypeBuilder<JobSeekerProfile>`-ல் `Entity<T>()` method கிடையாது. அது `ModelBuilder`-க்கு உரியது. அதனால் child entities தனித்தனி `IEntityTypeConfiguration<T>` classes-ல் configure செய்யப்பட்டுள்ளன.

## Visual Studio-ல் செய்ய வேண்டியது

1. பழைய extracted project folder-ஐ close செய்யவும்.
2. இந்த ZIP-ஐ புதிய folder-ல் extract செய்யவும்.
3. `NexHire.sln` open செய்யவும்.
4. `NexHire.API`-ஐ Startup Project ஆக set செய்யவும்.
5. `Build > Clean Solution` செய்யவும்.
6. `Build > Rebuild Solution` செய்யவும்.

பழைய cache இருந்தால் solution close செய்து `.vs`, `bin`, `obj` folders delete செய்து மீண்டும் open செய்யவும்.
