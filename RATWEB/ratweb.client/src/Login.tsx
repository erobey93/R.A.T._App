// src/components/Login.tsx

import React, { useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

interface LoginResponse {
    username: string;
    role: string;
}

const Login = () => {
    const [username, setUsername] = useState<string>('');
    const [password, setPassword] = useState<string>('');
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();

        try {
            const response = await axios.post<LoginResponse>('http://localhost:5000/api/auth/login', {
                username,
                password
            });

            const { username: loggedInUser, role } = response.data;

            // Redirect user based on their role
            if (role === 'Admin') {
                navigate('/admin-dashboard');
            } else if (role === 'User') {
                navigate('/user-dashboard');
            }

            alert(`Welcome ${loggedInUser}! Role: ${role}`);
        } catch (error) {
            setError('Invalid username or password.');
        }
    };

    return (
        <div className="login-form">
            <h2>Login</h2>
            <form onSubmit={handleLogin}>
                <div>
                    <label htmlFor="username">Username:</label>
                    <input
                        type="text"
                        id="username"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                    />
                </div>
                <div>
                    <label htmlFor="password">Password:</label>
                    <input
                        type="password"
                        id="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>
                {error && <p className="error-message">{error}</p>}
                <button type="submit">Login</button>
            </form>
            <div>
                <button onClick={() => navigate('/create-account')}>Create Account</button>
                <button onClick={() => navigate('/change-password')}>Change Password</button>
            </div>
        </div>
    );
};

export default Login;
