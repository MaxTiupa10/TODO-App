import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { CreateTask, PagedResult, Task, UpdateTask } from '../models';

const API_URL = 'http://localhost:5000/api/tasks';

@Injectable({ providedIn: 'root' })
export class TaskService {
  constructor(private http: HttpClient) {}

  getTasks(
    pageNumber = 1,
    pageSize = 10,
    categoryId?: number,
    search?: string,
    listType?: string,
    dateFrom?: string,
    dateTo?: string
  ) {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    if (categoryId != null) params = params.set('categoryId', categoryId);
    if (search) params = params.set('search', search);
    if (listType) params = params.set('listType', listType);
    if (dateFrom) params = params.set('dateFrom', dateFrom);
    if (dateTo) params = params.set('dateTo', dateTo);

    return this.http.get<PagedResult<Task>>(API_URL, { params });
  }

  getTask(id: number) {
    return this.http.get<Task>(`${API_URL}/${id}`);
  }

  createTask(dto: CreateTask) {
    return this.http.post<Task>(API_URL, dto);
  }

  updateTask(id: number, dto: UpdateTask) {
    return this.http.put(`${API_URL}/${id}`, dto);
  }

  deleteTask(id: number) {
    return this.http.delete(`${API_URL}/${id}`);
  }
}
