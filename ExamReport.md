# Exam Report: AI Puzzle Generator

## Project Idea and Requirements

AI Puzzle Generator is a Windows Forms desktop application that lets the user upload an image and generate a puzzle from it. The player can choose a puzzle size, move pieces with the mouse, adjust the last selected piece with the keyboard, pause the game, and save completion records.

Main requirements:

- Keep the project as a C# Windows Forms desktop application.
- Preserve the established gameplay behavior.
- Organize the application into a clean Visual Studio project structure.
- Store puzzle pieces and groups as model classes.
- Move JSON score persistence into a service.
- Move time formatting into a helper service.
- Provide project documentation and manual testing evidence for an exam submission.

## System Architecture - Modules

### UI Layer

The UI layer is implemented in `PuzzleForm`. It creates the form controls, handles user events, displays the uploaded image preview, shows timer and top-time labels, and keeps the WinForms layout simple and consistent.

### Image Processing and Puzzle Generation

This module scales the uploaded image to the puzzle panel, splits it into equal rectangular pieces, assigns each piece a correct position, and creates the `PictureBox` controls used during play.

### Puzzle Piece Movement and Snapping Logic

Mouse drag logic moves a selected piece and its connected group. Adjacent pieces are detected by grid position and screen position. If two pieces are close enough, their groups are merged. Correctly placed groups are locked in position.

### Game State, Timer and Pause/Resume

The form tracks whether the game is active or paused. A WinForms timer updates elapsed time once per second. Pausing stops the timer and hides all puzzle pieces; resuming shows the pieces again and restarts the timer.

### Score Persistence

`ScoreService` loads and saves the top completion times from `TopTimes.json` using Newtonsoft.Json. It also keeps only the best five times for each puzzle size.

### Testing and Validation

Testing is mainly manual because the application behavior depends on WinForms UI interactions, image selection dialogs, drag movement, and visual completion. Build validation is done with `dotnet build`.

## Development Process per Module

### UI Layer

The form layout and event handlers are kept in `PuzzleForm` to avoid unnecessary redesign. The entry point is placed in `Program.cs`, matching a normal Visual Studio WinForms project.

### Image Processing and Puzzle Generation

The image scaling and splitting logic is kept focused inside the puzzle generation workflow. The generated puzzle pieces use the separate `PuzzlePiece` model.

### Puzzle Piece Movement and Snapping Logic

Drag, keyboard movement, snapping, group merging, and placement checks were preserved in the form code because they are tightly connected to UI controls and mouse events.

### Game State, Timer and Pause/Resume

The timer and pause/resume behavior is retained. Time display strings were moved into `TimeFormatter` so repeated formatting logic is not duplicated in the form.

### Score Persistence

The JSON loading, saving, and top-five score update behavior is handled by `ScoreService`. This keeps file persistence separate from UI code while still using a simple local `TopTimes.json` file.

### Testing and Validation

Manual tests were defined around the main user workflows. The project was also prepared for Visual Studio build and command-line build with the .NET SDK.

## Testing Strategy

Manual test checklist:

- Upload a valid image.
- Try generating without an image.
- Generate each puzzle size.
- Drag pieces.
- Use keyboard movement.
- Pause and resume.
- Complete a small 3x3 puzzle.
- Verify `TopTimes.json` is created or updated.
- Close and reopen the app and verify top times are loaded.

Additional validation:

- Build the solution in Visual Studio.
- Build the project with `dotnet build`.
- Check that no required feature was removed during refactoring.

## Challenges and Tool Comparison

The main challenge was preserving gameplay behavior while improving the project structure. Most game logic was intentionally kept in `PuzzleForm` because a deeper architectural rewrite would increase risk and was not required for the exam goal.

Codex was used as the main AI tool for implementation support, refactoring, project setup, documentation draft, and build validation. ChatGPT was used as a planning and review assistant for checking the module breakdown and preparing exam-friendly explanations.

Compared with manual-only development, AI assistance helped organize the project faster and identify clean module boundaries. The developer still needed to verify that the application stayed a WinForms desktop app and that required features were preserved.

## Working System Evidence

Suggested evidence to add before submission:

- Screenshot of uploaded image preview.
- Screenshot of generated puzzle pieces.
- Screenshot of pause/resume behavior.
- Screenshot or JSON snippet showing saved top times.
- Build output from Visual Studio or `dotnet build`.

Screenshots should be placed in `docs/screenshots/`.

## Repository Link placeholder

Repository link: TODO
