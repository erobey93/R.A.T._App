import { useEffect, useState } from 'react';
import './App.css';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';

// Forecast type definition
interface Forecast {
    date: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}

// Example of a new page component
const Login = () => {
    return (
        <div>
            <h2>Test Login Page</h2>
            <p>Test the new login page setup</p>
        </div>
    );
};

function App() {
    const [forecasts, setForecasts] = useState<Forecast[]>();

    useEffect(() => {
        populateWeatherData();
    }, []);

    const contents = forecasts === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        : <table className="table table-striped" aria-labelledby="tableLabel">
            <thead>
                <tr>
                    <th>Date</th>
                    <th>Temp. (C)</th>
                    <th>Temp. (F)</th>
                    <th>Summary</th>
                </tr>
            </thead>
            <tbody>
                {forecasts.map(forecast =>
                    <tr key={forecast.date}>
                        <td>{forecast.date}</td>
                        <td>{forecast.temperatureC}</td>
                        <td>{forecast.temperatureF}</td>
                        <td>{forecast.summary}</td>
                    </tr>
                )}
            </tbody>
        </table>;

    return (
        <Router>
            <div>
                <h1>My React App</h1>
                <nav>
                    <ul>
                        <li><Link to="/weather">Weather Forecast</Link></li>
                        <li><Link to="/newpage">Login</Link></li>
                    </ul>
                </nav>

                {/* Updated Routes component and Route syntax */}
                <Routes>
                    <Route path="/" element={<h2>Welcome to my app!</h2>} />
                    <Route path="/weather" element={<><h1 id="tableLabel">Weather Forecast</h1>{contents}</>} />
                    <Route path="/newpage" element={<Login />} />
                </Routes>
            </div>
        </Router>
    );

    async function populateWeatherData() {
        const response = await fetch('weatherforecast');
        if (response.ok) {
            const data = await response.json();
            setForecasts(data);
        }
    }
}

export default App;