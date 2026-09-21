# Nikolay-Tonchev-employees

This app provides a solution to the following task: given a list of employee project
assignments, find the pair of employees who have worked together on common
projects for the longest total period of time.

## Tech stack

- Backend: .NET / C#
- Frontend: React
- Input: CSV file, loaded from the file system

## How it works

1. Load and parse the CSV. The CSV should be in the format: EmpID, ProjectID, DateFrom, DateTo. In should work with and without the header row.
2. Parse `DateFrom` / `DateTo` against a list of supported formats (see
   below); `NULL` is resolved to the current date.
3. Group assignments by `ProjectID`.
4. Within each project group, remove duplicate assignments (same
   `EmpID` + `DateFrom` + `DateTo` appearing more than once).
5. For every pair of employees sharing a project, compute the overlap
   between their date ranges.
6. Sum each pair's overlap across every project they share.
7. Return project pair(s) with the largest total.

## Supported date formats

- `yyyy-MM-dd`
- `dd/MM/yyyy`
- `MM-dd-yyyy`
- `yyyy.MM.dd`
- `dd-MMM-yyyy`

## Overlap / day-count convention

Overlap for a shared date range is calculated inclusively, so both the
start and end date of the overlapping period count as days worked together.

## Running it

The .NET Web API app is set to use SpaProxy, so running the API project will run the React on as well. You will however need to run `npm install` in order to install the required dependencies.

## Project structure

```
Nikolay-Tonchev-employees/
├── WebAPI/        
├── Client/        
└── README.md
```
