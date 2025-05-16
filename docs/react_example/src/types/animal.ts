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
