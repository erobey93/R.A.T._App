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

// Error handling helper
export class ApiError extends Error {
  constructor(
    message: string,
    public statusCode?: number,
    public errors?: string[]
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

// Axios interceptor setup
axios.interceptors.response.use(
  response => response,
  error => {
    if (error.response) {
      const { data, status } = error.response;
      throw new ApiError(
        data.message || 'An error occurred',
        status,
        data.errors
      );
    }
    throw new ApiError('Network error');
  }
);
