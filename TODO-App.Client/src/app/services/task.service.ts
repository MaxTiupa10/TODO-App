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
    if (dateFrom) params = params.set('deadlineFromUtc', this.toLocalDayStartUtcIso(dateFrom));
    if (dateTo) params = params.set('deadlineToUtc', this.toLocalDayEndUtcIso(dateTo));

    return this.http.get<PagedResult<Task>>(API_URL, { params });
  }

  private toLocalDayStartUtcIso(date: string): string {
    return new Date(`${date}T00:00:00`).toISOString();
  }

  private toLocalDayEndUtcIso(date: string): string {
    return new Date(`${date}T23:59:59.999`).toISOString();
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
