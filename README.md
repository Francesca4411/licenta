# Study Management - Licenta

Platforma web multi-utilizator pentru planificarea invatarii, dezvoltata in ASP.NET Core MVC.

## Functionalitati principale

- Dashboard cu statistici rapide si recomandari.
- Gestionare materii si evaluari (examene/proiecte).
- Calendar saptamanal pentru sesiuni de studiu.
- Modul Study Sessions (planificate/finalizate).
- Timer Pomodoro cu statistici si streak.
- Profil utilizator cu indicatori de progres.
- AI Tutor (integrare API extern).
- Panou Admin pentru vizualizare utilizatori si roluri.

## Tehnologii

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core
- SQLite
- ASP.NET Core Identity
- Razor Views + CSS/Bootstrap

## Cerinte

- .NET SDK 10
- (Optional) cheia API pentru modulul AI Tutor

## Rulare locala

1. Cloneaza repository-ul:
   - `git clone https://github.com/Francesca4411/licenta.git`
2. Intra in proiect:
   - `cd licenta/StudyManagement`
3. Restaureaza pachetele:
   - `dotnet restore`
4. Ruleaza aplicatia:
   - `dotnet run`
5. Deschide URL-ul afisat in terminal (ex. `http://localhost:5261`).

La pornire, migrarile EF Core sunt aplicate automat.

## Configurare AI Tutor (optional)

In fisierul `.env` din `StudyManagement`, poti seta:

- `AiTutor__ApiKey`
- `AiTutor__Model`
- `AiTutor__ApiUrl`
- `AiTutor__AnthropicVersion`
- `AiTutor__MaxTokens`
- `AiTutor__Temperature`

## Structura proiectului

- `StudyManagement/Controllers` - logica de request/response
- `StudyManagement/Models` - entitati si view models
- `StudyManagement/Services` - logica de business
- `StudyManagement/Views` - interfata Razor
- `StudyManagement/Data` - context EF Core + initializare
- `StudyManagement/Migrations` - migrari baza de date

## Note

- `.env` nu este versionat (pentru protectia credentialelor).
- Baza de date de demo poate fi inclusa in repository, in functie de setarile `.gitignore`.
