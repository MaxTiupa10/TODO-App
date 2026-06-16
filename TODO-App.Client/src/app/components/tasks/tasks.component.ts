import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Category, CreateTask, Task, UpdateTask } from '../../models';
import { TaskService } from '../../services/task.service';
import { CategoryService } from '../../services/category.service';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.css'
})
export class TasksComponent implements OnInit {
  tasks: Task[] = [];
  categories: Category[] = [];
  totalCount = 0;
  totalPages = 0;
  pageNumber = 1;
  pageSize = 10;
  search = '';
  selectedCategoryId: number | null = null;
  loading = false;
  error = '';

  showForm = false;
  editingTask: Task | null = null;
  formTitle = '';
  formDescription = '';
  formCategoryId: number | null = null;
  formIsCompleted = false;

  constructor(
    private taskService: TaskService,
    private categoryService: CategoryService
  ) {}

  ngOnInit() {
    this.loadCategories();
    this.loadTasks();
  }

  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: categories => (this.categories = categories),
      error: () => (this.error = 'Failed to load categories')
    });
  }

  loadTasks() {
    this.loading = true;
    this.error = '';

    this.taskService
      .getTasks(
        this.pageNumber,
        this.pageSize,
        this.selectedCategoryId ?? undefined,
        this.search || undefined
      )
      .subscribe({
        next: result => {
          this.tasks = result.items;
          this.totalCount = result.totalCount;
          this.totalPages = result.totalPages;
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load tasks';
          this.loading = false;
        }
      });
  }

  applyFilters() {
    this.pageNumber = 1;
    this.loadTasks();
  }

  clearFilters() {
    this.search = '';
    this.selectedCategoryId = null;
    this.pageNumber = 1;
    this.loadTasks();
  }

  goToPage(page: number) {
    if (page < 1 || page > this.totalPages) return;
    this.pageNumber = page;
    this.loadTasks();
  }

  openCreateForm() {
    this.editingTask = null;
    this.formTitle = '';
    this.formDescription = '';
    this.formCategoryId = null;
    this.formIsCompleted = false;
    this.showForm = true;
  }

  openEditForm(task: Task) {
    this.editingTask = task;
    this.formTitle = task.title;
    this.formDescription = task.description ?? '';
    this.formCategoryId = task.categoryId ?? null;
    this.formIsCompleted = task.isCompleted;
    this.showForm = true;
  }

  closeForm() {
    this.showForm = false;
    this.editingTask = null;
  }

  saveTask() {
    if (!this.formTitle.trim()) return;

    if (this.editingTask) {
      const dto: UpdateTask = {
        title: this.formTitle,
        description: this.formDescription || undefined,
        isCompleted: this.formIsCompleted,
        categoryId: this.formCategoryId ?? undefined
      };

      this.taskService.updateTask(this.editingTask.id, dto).subscribe({
        next: () => {
          this.closeForm();
          this.loadTasks();
        },
        error: () => (this.error = 'Failed to update task')
      });
    } else {
      const dto: CreateTask = {
        title: this.formTitle,
        description: this.formDescription || undefined,
        categoryId: this.formCategoryId ?? undefined
      };

      this.taskService.createTask(dto).subscribe({
        next: () => {
          this.closeForm();
          this.loadTasks();
        },
        error: () => (this.error = 'Failed to create task')
      });
    }
  }

  deleteTask(task: Task) {
    if (!confirm(`Delete task "${task.title}"?`)) return;

    this.taskService.deleteTask(task.id).subscribe({
      next: () => this.loadTasks(),
      error: () => (this.error = 'Failed to delete task')
    });
  }

  toggleComplete(task: Task) {
    const dto: UpdateTask = {
      title: task.title,
      description: task.description,
      isCompleted: !task.isCompleted,
      categoryId: task.categoryId
    };

    this.taskService.updateTask(task.id, dto).subscribe({
      next: () => this.loadTasks(),
      error: () => (this.error = 'Failed to update task')
    });
  }
}
