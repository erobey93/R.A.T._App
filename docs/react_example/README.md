# React TypeScript Example for R.A.T. App

This is an example React TypeScript project structure showing how to consume the R.A.T. App API.

## Project Structure

```
ratapp-client/
├── src/
│   ├── api/
│   │   └── animalApi.ts      # API client functions
│   ├── components/
│   │   ├── AnimalList/       # List of animals
│   │   ├── AnimalDetails/    # Single animal view
│   │   └── AnimalForm/       # Create/Edit form
│   ├── types/
│   │   └── animal.ts         # TypeScript interfaces
│   ├── hooks/
│   │   └── useAnimals.ts     # Custom hook for data fetching
│   └── App.tsx               # Main component
├── package.json
└── tsconfig.json
```

## Example Files

### 1. types/animal.ts
```typescript
export interface Animal {
  id: number;
  name: string;
  species: string;
  sex: string;
  dateOfBirth: string;
  line: string;
  variety?: string;
  color?: string;
  markings?: string;
  earType?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}
```

### 2. api/animalApi.ts
```typescript
import axios from 'axios';
import { Animal, ApiResponse } from '../types/animal';

const API_URL = 'http://localhost:7042/api';

export const getAnimals = async (filters?: {
  species?: string;
  sex?: string;
  searchTerm?: string;
}): Promise<Animal[]> => {
  const response = await axios.get<ApiResponse<Animal[]>>(`${API_URL}/animal`, { 
    params: filters 
  });
  return response.data.data;
};

export const getAnimal = async (id: number): Promise<Animal> => {
  const response = await axios.get<ApiResponse<Animal>>(`${API_URL}/animal/${id}`);
  return response.data.data;
};

export const createAnimal = async (animal: Omit<Animal, 'id'>): Promise<Animal> => {
  const response = await axios.post<ApiResponse<Animal>>(`${API_URL}/animal`, animal);
  return response.data.data;
};

export const updateAnimal = async (id: number, animal: Animal): Promise<Animal> => {
  const response = await axios.put<ApiResponse<Animal>>(`${API_URL}/animal/${id}`, animal);
  return response.data.data;
};

export const deleteAnimal = async (id: number): Promise<void> => {
  await axios.delete(`${API_URL}/animal/${id}`);
};
```

### 3. hooks/useAnimals.ts
```typescript
import { useState, useEffect } from 'react';
import { Animal } from '../types/animal';
import { getAnimals } from '../api/animalApi';

interface UseAnimalsOptions {
  species?: string;
  sex?: string;
  searchTerm?: string;
}

export const useAnimals = (options?: UseAnimalsOptions) => {
  const [animals, setAnimals] = useState<Animal[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadAnimals = async () => {
      try {
        setLoading(true);
        const data = await getAnimals(options);
        setAnimals(data);
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load animals');
      } finally {
        setLoading(false);
      }
    };

    loadAnimals();
  }, [options?.species, options?.sex, options?.searchTerm]);

  return { animals, loading, error };
};
```

### 4. components/AnimalList/AnimalList.tsx
```typescript
import { useState } from 'react';
import { Animal } from '../../types/animal';
import { useAnimals } from '../../hooks/useAnimals';

export const AnimalList = () => {
  const [species, setSpecies] = useState<string>('');
  const [sex, setSex] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');
  
  const { animals, loading, error } = useAnimals({ species, sex, searchTerm });

  if (error) {
    return <div className="error">Error: {error}</div>;
  }

  return (
    <div>
      <h1>Animals</h1>
      
      {/* Filters */}
      <div className="filters">
        <select value={species} onChange={e => setSpecies(e.target.value)}>
          <option value="">All Species</option>
          <option value="Rat">Rat</option>
          <option value="Mouse">Mouse</option>
        </select>
        
        <select value={sex} onChange={e => setSex(e.target.value)}>
          <option value="">All Sexes</option>
          <option value="Male">Male</option>
          <option value="Female">Female</option>
        </select>
        
        <input
          type="text"
          value={searchTerm}
          onChange={e => setSearchTerm(e.target.value)}
          placeholder="Search..."
        />
      </div>

      {/* Results */}
      {loading ? (
        <div className="loading">Loading...</div>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Species</th>
              <th>Sex</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {animals.map(animal => (
              <tr key={animal.id}>
                <td>{animal.name}</td>
                <td>{animal.species}</td>
                <td>{animal.sex}</td>
                <td>
                  <button onClick={() => /* View details */}>View</button>
                  <button onClick={() => /* Edit animal */}>Edit</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};
```

## Getting Started

1. Create a new React TypeScript project:
```bash
npx create-react-app ratapp-client --template typescript
cd ratapp-client
```

2. Install dependencies:
```bash
npm install axios @types/axios
```

3. Copy the example files into your project structure.

4. Update the API_URL in animalApi.ts to match your API endpoint.

5. Import and use the components in your App.tsx:
```typescript
import { AnimalList } from './components/AnimalList/AnimalList';

function App() {
  return (
    <div className="App">
      <AnimalList />
    </div>
  );
}

export default App;
```

This example provides a foundation for building a React TypeScript frontend that consumes the R.A.T. App API. It includes:
- Type safety with TypeScript
- API client with axios
- Custom hooks for data fetching
- Component structure
- Error handling
- Loading states
- Filtering capabilities
