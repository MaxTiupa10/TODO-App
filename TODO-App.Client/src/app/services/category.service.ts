import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Category, CreateCategory } from '../models';

const API_URL = 'http://localhost:5000/api/categories';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<Category[]>(API_URL);
  }

  create(dto: CreateCategory) {
    return this.http.post<Category>(API_URL, dto);
  }

  update(id: number, name: string) {
    return this.http.put(`${API_URL}/${id}`, { name });
  }

  delete(id: number) {
    return this.http.delete(`${API_URL}/${id}`);
  }
}
