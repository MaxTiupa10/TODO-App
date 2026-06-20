import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  Subject,
  Subscription,
  catchError,
  debounceTime,
  distinctUntilChanged,
  finalize,
  of,
  switchMap,
  tap
} from 'rxjs';
import { Category, CreateTask, Task, UpdateTask } from '../../models';
import { TaskService } from '../../services/task.service';
import { CategoryService } from '../../services/category.service';
import { LanguageService } from '../../services/language.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { TranslationKey } from '../../i18n/translations';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.css'
})
export class TasksComponent implements OnInit, OnDestroy {
  listTypes: { key: string; labelKey: TranslationKey }[] = [
    { key: 'myday', labelKey: 'tasks.list.myday' },
    { key: 'important', labelKey: 'tasks.list.important' },
    { key: 'planned', labelKey: 'tasks.list.planned' },
    { key: 'completed', labelKey: 'tasks.list.completed' },
    { key: 'assignedtome', labelKey: 'tasks.list.assignedtome' },
    { key: 'tasks', labelKey: 'tasks.list.tasks' }
  ];
  selectedListType = 'myday';
  tasks: Task[] = [];
  categories: Category[] = [];
  totalCount = 0;
  totalPages = 0;
  pageNumber = 1;
  pageSize = 10;
  search = '';
  dateFrom = '';
  dateTo = '';
  selectedCategoryId: number | null = null;
  initialLoading = true;
  searching = false;
  listAnimationKey = 0;
  error = '';

  showForm = false;
  editingTask: Task | null = null;
  formTitle = '';
  formDescription = '';
  formCategoryId: number | null = null;
  formIsCompleted = false;
  formIsImportant = false;
  formDeadline = '';

  private readonly searchChanges$ = new Subject<string>();
  private readonly reloadTasks$ = new Subject<void>();
  private readonly subscriptions = new Subscription();

  constructor(
    private taskService: TaskService,
    private categoryService: CategoryService,
    private languageService: LanguageService
  ) {}

  ngOnInit() {
    this.loadCategories();

    this.subscriptions.add(
      this.reloadTasks$.pipe(switchMap(() => this.fetchTasks())).subscribe({
        next: result => this.applyTaskResult(result),
        error: () => {
          this.error = this.languageService.translate('tasks.error.load');
          this.initialLoading = false;
          this.searching = false;
        }
      })
    );

    this.subscriptions.add(
      this.searchChanges$
        .pipe(
          debounceTime(250),
          distinctUntilChanged(),
          tap(() => {
            this.pageNumber = 1;
            this.searching = true;
          })
        )
        .subscribe(() => this.reloadTasks$.next())
    );

    this.reloadTasks$.next();
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: categories => (this.categories = categories),
      error: () => (this.error = this.languageService.translate('tasks.error.loadCategories'))
    });
  }

  loadTasks(options: { searching?: boolean } = {}) {
    if (options.searching) {
      this.searching = true;
    }
    this.reloadTasks$.next();
  }

  onSearchChange(value: string) {
    this.search = value;
    this.searchChanges$.next(value);
  }

  onCategoryChange(categoryId: number | null) {
    this.selectedCategoryId = categoryId;
    this.pageNumber = 1;
    this.loadTasks({ searching: true });
  }

  onDateFromChange(value: string) {
    this.dateFrom = value;
    this.normalizeDateRange();
    this.pageNumber = 1;
    this.loadTasks({ searching: true });
  }

  onDateToChange(value: string) {
    this.dateTo = value;
    this.normalizeDateRange();
    this.pageNumber = 1;
    this.loadTasks({ searching: true });
  }

  clearFilters() {
    this.search = '';
    this.dateFrom = '';
    this.dateTo = '';
    this.selectedCategoryId = null;
    this.pageNumber = 1;
    this.searching = true;
    this.reloadTasks$.next();
  }

  selectListType(listType: string) {
    this.selectedListType = listType;
    this.pageNumber = 1;
    this.loadTasks({ searching: true });
  }

  get currentListTitle(): string {
    const key = `tasks.list.${this.selectedListType}` as TranslationKey;
    return this.languageService.translate(key);
  }

  trackByTaskId(_index: number, task: Task): number {
    return task.id;
  }

  isOverdue(task: Task): boolean {
    if (task.isCompleted || !task.deadline) return false;
    return new Date(task.deadline).getTime() < Date.now();
  }

  goToPage(page: number) {
    if (page < 1 || page > this.totalPages) return;
    this.pageNumber = page;
    this.loadTasks({ searching: true });
  }

  openCreateForm() {
    this.editingTask = null;
    this.formTitle = '';
    this.formDescription = '';
    this.formCategoryId = null;
    this.formIsCompleted = false;
    this.formIsImportant = false;
    this.formDeadline = '';
    this.showForm = true;
  }

  openEditForm(task: Task) {
    this.editingTask = task;
    this.formTitle = task.title;
    this.formDescription = task.description ?? '';
    this.formCategoryId = task.categoryId ?? null;
    this.formIsCompleted = task.isCompleted;
    this.formIsImportant = task.isImportant;
    this.formDeadline = task.deadline ? this.toDateTimeLocalValue(task.deadline) : '';
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
        isImportant: this.formIsImportant,
        deadline: this.toIsoString(this.formDeadline),
        categoryId: this.formCategoryId ?? undefined
      };

      this.taskService.updateTask(this.editingTask.id, dto).subscribe({
        next: () => {
          this.closeForm();
          this.loadTasks();
        },
        error: () => (this.error = this.languageService.translate('tasks.error.update'))
      });
    } else {
      const dto: CreateTask = {
        title: this.formTitle,
        description: this.formDescription || undefined,
        isImportant: this.formIsImportant,
        deadline: this.toIsoString(this.formDeadline),
        categoryId: this.formCategoryId ?? undefined
      };

      this.taskService.createTask(dto).subscribe({
        next: () => {
          this.closeForm();
          this.loadTasks();
        },
        error: () => (this.error = this.languageService.translate('tasks.error.create'))
      });
    }
  }

  deleteTask(task: Task) {
    if (!confirm(this.languageService.translate('tasks.deleteConfirm', { title: task.title }))) return;

    this.taskService.deleteTask(task.id).subscribe({
      next: () => this.loadTasks(),
      error: () => (this.error = this.languageService.translate('tasks.error.delete'))
    });
  }

  toggleComplete(task: Task) {
    const dto: UpdateTask = {
      title: task.title,
      description: task.description,
      isCompleted: !task.isCompleted,
      isImportant: task.isImportant,
      deadline: task.deadline ?? undefined,
      categoryId: task.categoryId
    };

    this.taskService.updateTask(task.id, dto).subscribe({
      next: () => this.loadTasks(),
      error: () => (this.error = this.languageService.translate('tasks.error.update'))
    });
  }

  toggleImportant(task: Task) {
    const dto: UpdateTask = {
      title: task.title,
      description: task.description,
      isCompleted: task.isCompleted,
      isImportant: !task.isImportant,
      deadline: task.deadline ?? undefined,
      categoryId: task.categoryId
    };

    this.taskService.updateTask(task.id, dto).subscribe({
      next: () => this.loadTasks(),
      error: () => (this.error = this.languageService.translate('tasks.error.update'))
    });
  }

  private fetchTasks() {
    this.error = '';
    this.normalizeDateRange();

    return this.taskService
      .getTasks(
        this.pageNumber,
        this.pageSize,
        this.selectedCategoryId ?? undefined,
        this.search || undefined,
        this.selectedListType,
        this.dateFrom || undefined,
        this.dateTo || undefined
      )
      .pipe(
        catchError(() => {
          this.error = this.languageService.translate('tasks.error.load');
          this.tasks = [];
          this.totalCount = 0;
          this.totalPages = 0;
          this.listAnimationKey++;
          return of(null);
        }),
        finalize(() => {
          this.initialLoading = false;
          this.searching = false;
        })
      );
  }

  private normalizeDateRange() {
    if (this.dateFrom && this.dateTo && this.dateFrom > this.dateTo) {
      [this.dateFrom, this.dateTo] = [this.dateTo, this.dateFrom];
    }
  }

  private applyTaskResult(result: { items: Task[]; totalCount: number; totalPages: number } | null) {
    if (!result) return;

    this.listAnimationKey++;
    this.tasks = result.items;
    this.totalCount = result.totalCount;
    this.totalPages = result.totalPages;
  }

  private toDateTimeLocalValue(iso: string): string {
    const date = new Date(iso);
    if (isNaN(date.getTime())) return '';

    const pad = (value: number) => value.toString().padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
  }

  private toIsoString(dateTimeLocal: string): string | undefined {
    if (!dateTimeLocal) return undefined;

    const date = new Date(dateTimeLocal);
    if (isNaN(date.getTime())) return undefined;

    return date.toISOString();
  }
}
