export type StudyType = 'Genetics' | 'Health' | 'Behavior' | 'Growth' | 'Other';

export interface Study {
  id: string;
  name: string;
  description: string;
  startDate: string;
  estimatedEndDate: string;
  type: StudyType;
  status: 'Active' | 'Completed' | 'Planned';
}

export interface Animal {
  id: string;
  name: string;
  age: number;
  sex: 'Male' | 'Female';
  imageUrl: string;
  group?: 'Control' | 'Experimental';
}

export interface DataPoint {
  id: string;
  name: string;
  type: 'text' | 'number' | 'boolean' | 'date' | 'select';
  options?: string[]; // For select type
  required: boolean;
}

export interface StudyData {
  id: string;
  studyId: string;
  animalId: string;
  dataPointId: string;
  value: string | number | boolean | Date;
  timestamp: string;
}

export interface StudyStats {
  average?: number;
  minimum?: number;
  maximum?: number;
  count: number;
}
