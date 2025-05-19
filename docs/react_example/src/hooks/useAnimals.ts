import { useState, useEffect, useCallback } from 'react';
import { Animal } from '../types/animal';
import { getAnimals, ApiError } from '../api/animalApi';

interface UseAnimalsOptions {
  species?: string;
  sex?: string;
  searchTerm?: string;
}

export const useAnimals = (options?: UseAnimalsOptions) => {
  const [animals, setAnimals] = useState<Animal[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadAnimals = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getAnimals(options);
      setAnimals(data);
    } catch (err) {
      const message = err instanceof ApiError 
        ? err.message 
        : 'Failed to load animals';
      setError(message);
      setAnimals([]);
    } finally {
      setLoading(false);
    }
  }, [options?.species, options?.sex, options?.searchTerm]);

  useEffect(() => {
    loadAnimals();
  }, [loadAnimals]);

  const refresh = () => {
    loadAnimals();
  };

  return { 
    animals, 
    loading, 
    error, 
    refresh,
    // Helper methods for common operations
    getById: useCallback((id: number) => 
      animals.find(a => a.id === id), [animals]),
    filterBySpecies: useCallback((species: string) => 
      animals.filter(a => a.species === species), [animals]),
    filterBySex: useCallback((sex: string) => 
      animals.filter(a => a.sex === sex), [animals])
  };
};
