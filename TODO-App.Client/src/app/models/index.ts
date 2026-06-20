export interface Task {
  id: number;
  title: string;
  description?: string;
  isCompleted: boolean;
  completedAt?: string;
  isImportant: boolean;
  createdAt: string;
  deadline?: string;
  categoryId?: number;
  categoryName?: string;
}

export interface CreateTask {
  title: string;
  description?: string;
  categoryId?: number;
  isImportant?: boolean;
  deadline?: string;
}

export interface UpdateTask {
  title: string;
  description?: string;
  isCompleted: boolean;
  isImportant: boolean;
  deadline?: string;
  categoryId?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface Category {
  id: number;
  name: string;
}

export interface CreateCategory {
  name: string;
}

export interface AuthResponse {
  token: string;
  username: string;
  userId: number;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
}
