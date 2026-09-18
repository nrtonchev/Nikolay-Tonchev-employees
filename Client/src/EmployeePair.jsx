import { useRef, useState } from "react";

const ENDPOINT = "http://localhost:5124/api/extract-data-from-file";

export default function EmployeePairs() {
    const inputRef = useRef(null);

    const [status, setStatus] = useState("idle"); // idle | loading | done | error
    const [rows, setRows] = useState([]);
    const [error, setError] = useState("");
    const [fileName, setFileName] = useState("");

    function openPicker() {
        inputRef.current?.click();
    }

    async function handleFileSelected(event) {
        const file = event.target.files?.[0];

        if (!file) return;

        setFileName(file.name);
        setStatus("loading");
        setError("");
        setRows([]);

        const body = new FormData();
        body.append("file", file);

        try {
            const response = await fetch(ENDPOINT, { method: "POST", body });

            if (!response.ok) {
                const message = await response.text();
                throw new Error(message || `Request failed with ${response.status}`);
            }

            setRows(await response.json());
            setStatus("done");
        } catch (problem) {
            setError(problem.message);
            setStatus("error");
        } finally {
            event.target.value = "";
        }
    }

    const pair = rows.length > 0 ? rows[0] : null;
    const totalDays = rows.reduce((sum, row) => sum + row.daysWorked, 0);

    return (
        <main >
            <h1>Longest-working employee pair</h1>
            <p style={{paddingBottom: 10}}>
                Select a CSV with data in the following format: EmpID, ProjectID, DateFrom, DateTo.
            </p>

            <input
                ref={inputRef}
                type="file"
                accept=".csv"
                onChange={handleFileSelected}
                hidden
            />

            <button className="fileButton" type="button" onClick={openPicker} disabled={status === "loading"}>
                {status === "loading" ? "Analyzing…" : "Choose a file"}
            </button>

            {fileName && <p style={{padding: 10}}>{fileName}</p>}

            {status === "loading" && (
                <p aria-live="polite">
                    Reading the file and comparing every pair…
                </p>
            )}

            {status === "error" && (
                <p role="alert" className="error">
                    {error}
                </p>
            )}

            {status === "done" && rows.length === 0 && (
                <p>No two employees in this file shared a project.</p>
            )}

            {status === "done" && pair && (
                <>
                    <h2 style={{padding: 10}}>
                        Employees {pair.employeeOneId} and {pair.employeeTwoId} — {totalDays} days
                        together
                    </h2>

                    <table>
                        <thead>
                        <tr>
                            <th scope="col">EmployeeOneId</th>
                            <th scope="col">EmployeeTwoId</th>
                            <th scope="col">ProjectId</th>
                            <th scope="col">Days worked</th>
                        </tr>
                        </thead>
                        <tbody>
                        {rows.map((row) => (
                            <tr key={row.projectId}>
                                <td>{row.employeeOneId}</td>
                                <td>{row.employeeTwoId}</td>
                                <td>{row.projectId}</td>
                                <td>{row.daysWorked}</td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                </>
            )}
        </main>
    );
}