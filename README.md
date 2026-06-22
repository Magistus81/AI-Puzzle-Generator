# AI Puzzle Generator

AI Puzzle Generator is an AI-assisted C# Windows Forms mini-app that turns a local image into an interactive draggable puzzle. The project was developed iteratively with Codex as the main coding and refactoring tool, and ChatGPT as planning and review support.

Repository: [https://github.com/Magistus81/AI-Puzzle-Generator](https://github.com/Magistus81/AI-Puzzle-Generator)

## Features

- Upload an image from the local file system.
- Preview the uploaded image.
- Select puzzle size: 3x3, 4x4, 5x5, 8x8, or 10x10.
- Generate and shuffle puzzle pieces.
- Drag pieces with the mouse.
- Move the last selected piece with keyboard arrow keys.
- Snap adjacent pieces or connected groups when close enough.
- Lock correctly placed pieces.
- Detect puzzle completion.
- Track elapsed time.
- Pause and resume the game.
- Hide puzzle pieces while paused.
- Save and load the top 5 completion times per puzzle size using JSON.
- Resize the puzzle panel from the bottom-right corner.

## Technologies

- C#
- Windows Forms
- .NET 8 for Windows
- Newtonsoft.Json
- Visual Studio 2022 or newer
- AI-assisted development with Codex and ChatGPT

## Screenshots

![Application start screen](docs/screenshots/start_app.jpg)

![Generated puzzle](docs/screenshots/generated_pzl.jpg)

![Completed puzzle](docs/screenshots/end.jpg)

## Project Structure

```text
AI-Puzzle-Generator/
  AI-Puzzle-Generator.sln
  README.md
  ExamReport.md
  .gitignore
  docs/
    screenshots/
      start_app.jpg
      generated_pzl.jpg
      end.jpg
  src/
    PuzzleGenerator/
      PuzzleGenerator.csproj
      Program.cs
      Controls/
        DoubleBufferedPanel.cs
      Forms/
        PuzzleForm.cs
      Models/
        PuzzlePiece.cs
        PuzzleGroup.cs
      Services/
        ScoreService.cs
        TimeFormatter.cs
```

## Build And Run

1. Open `AI-Puzzle-Generator.sln` in Visual Studio on Windows.
2. Restore NuGet packages if Visual Studio does not do it automatically.
3. Set `PuzzleGenerator` as the startup project.
4. Build and run the project.

Command-line option:

```powershell
dotnet restore src/PuzzleGenerator/PuzzleGenerator.csproj
dotnet build src/PuzzleGenerator/PuzzleGenerator.csproj
dotnet run --project src/PuzzleGenerator/PuzzleGenerator.csproj
```

The app saves completion records to `TopTimes.json` in the application output folder.

## Manual Testing Checklist

- Upload a valid image.
- Try generating without an image.
- Generate each puzzle size.
- Drag pieces and connected groups.
- Use keyboard movement.
- Pause and resume.
- Complete a small 3x3 puzzle.
- Verify `TopTimes.json` is created or updated.
- Close and reopen the app and verify top times are loaded.
