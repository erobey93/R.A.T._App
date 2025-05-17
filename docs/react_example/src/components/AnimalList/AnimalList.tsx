import { useState } from 'react';
import { useAnimals } from '../../hooks/useAnimals';
import { Animal } from '../../types/animal';
import { deleteAnimal } from '../../api/animalApi';

export const AnimalList = () => {
  const [species, setSpecies] = useState<string>('');
  const [sex, setSex] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');
  
  const { animals, loading, error, refresh } = useAnimals({ 
    species, 
    sex, 
    searchTerm 
  });

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this animal?')) {
      try {
        await deleteAnimal(id);
        refresh(); // Reload the list
      } catch (err) {
        alert('Failed to delete animal');
      }
    }
  };

  if (error) {
    return (
      <div className="error-container">
        <h2>Error</h2>
        <p>{error}</p>
        <button onClick={refresh}>Try Again</button>
      </div>
    );
  }

  return (
    <div className="animal-list">
      <h1>Animals</h1>
      
      {/* Filters */}
      <div className="filters">
        <select 
          value={species} 
          onChange={e => setSpecies(e.target.value)}
          className="filter-select"
        >
          <option value="">All Species</option>
          <option value="Rat">Rat</option>
          <option value="Mouse">Mouse</option>
        </select>
        
        <select 
          value={sex} 
          onChange={e => setSex(e.target.value)}
          className="filter-select"
        >
          <option value="">All Sexes</option>
          <option value="Male">Male</option>
          <option value="Female">Female</option>
        </select>
        
        <input
          type="text"
          value={searchTerm}
          onChange={e => setSearchTerm(e.target.value)}
          placeholder="Search by name or ID..."
          className="search-input"
        />
      </div>

      {/* Results */}
      {loading ? (
        <div className="loading">Loading...</div>
      ) : animals.length === 0 ? (
        <div className="no-results">
          <p>No animals found</p>
        </div>
      ) : (
        <table className="animals-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Name</th>
              <th>Species</th>
              <th>Sex</th>
              <th>Date of Birth</th>
              <th>Variety</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {animals.map(animal => (
              <tr key={animal.id}>
                <td>{animal.id}</td>
                <td>{animal.name}</td>
                <td>{animal.species}</td>
                <td>{animal.sex}</td>
                <td>{new Date(animal.dateOfBirth).toLocaleDateString()}</td>
                <td>{animal.variety || '-'}</td>
                <td>
                  <button 
                    onClick={() => window.location.href = `/animals/${animal.id}`}
                    className="action-button view"
                  >
                    View
                  </button>
                  <button 
                    onClick={() => window.location.href = `/animals/${animal.id}/edit`}
                    className="action-button edit"
                  >
                    Edit
                  </button>
                  <button 
                    onClick={() => handleDelete(animal.id)}
                    className="action-button delete"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};
