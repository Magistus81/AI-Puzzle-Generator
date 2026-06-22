# AI Puzzle Generator

A C# Windows Forms desktop puzzle application that turns a user-selected local image into a draggable jigsaw-style puzzle.

The project is organized as an AI-assisted WinForms mini-app for an exam submission, with the gameplay logic kept close to the working desktop application.

## Features

- Upload an image from the local file system.
- Preview the uploaded image.
- Select puzzle size: 3x3, 4x4, 5x5, 8x8, or 10x10.
- Generate, shuffle, drag, and place puzzle pieces.
- Move the last selected piece with the keyboard arrow keys.
- Snap adjacent pieces or groups when close enough.
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

## How To Build And Run

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

The app saves completion records to `TopTimes.json` in the application output folder when results are recorded or the form closes.

## Project Structure

```text
AI-Puzzle-Generator/
  AI-Puzzle-Generator.sln
  README.md
  ExamReport.md
  .gitignore
  docs/
    screenshots/
      placeholder.txt
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

## AI-Assisted Development Note

Codex was used as the main AI tool for implementation support, project structuring, refactoring, documentation, and build validation. ChatGPT can be used as a planning and review assistant for explaining the approach, checking exam wording, and preparing presentation notes.

## Screenshot Placeholders

Add screenshots to `docs/screenshots/` after running the application, for example:

- `docs/screenshots/upload-preview.png`
- `docs/screenshots/generated-puzzle.png`
- `docs/screenshots/completed-puzzle.png`

## Manual Test Checklist

- Upload a valid image.
- Try generating without an image.
- Generate each puzzle size.
- Drag pieces.
- Use keyboard movement.
- Pause and resume.
- Complete a small 3x3 puzzle.
- Verify `TopTimes.json` is created or updated.
- Close and reopen the app and verify top times are loaded.
"# AI-Puzzle-Generator" 
