import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Category } from '../../models';
import { CategoryService } from '../../services/category.service';
import { LanguageService } from '../../services/language.service';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.css'
})
export class CategoriesComponent implements OnInit {
  categories: Category[] = [];
  newCategoryName = '';
  editingId: number | null = null;
  editingName = '';
  error = '';
  loading = false;

  constructor(
    private categoryService: CategoryService,
    private languageService: LanguageService
  ) {}

  ngOnInit() {
    this.loadCategories();
  }

  loadCategories() {
    this.loading = true;
    this.categoryService.getAll().subscribe({
      next: categories => {
        this.categories = categories;
        this.loading = false;
      },
      error: () => {
        this.error = this.languageService.translate('categories.error.load');
        this.loading = false;
      }
    });
  }

  createCategory() {
    if (!this.newCategoryName.trim()) return;

    this.categoryService.create({ name: this.newCategoryName.trim() }).subscribe({
      next: () => {
        this.newCategoryName = '';
        this.loadCategories();
      },
      error: () => (this.error = this.languageService.translate('categories.error.create'))
    });
  }

  startEdit(category: Category) {
    this.editingId = category.id;
    this.editingName = category.name;
  }

  cancelEdit() {
    this.editingId = null;
    this.editingName = '';
  }

  saveEdit(id: number) {
    if (!this.editingName.trim()) return;

    this.categoryService.update(id, this.editingName.trim()).subscribe({
      next: () => {
        this.cancelEdit();
        this.loadCategories();
      },
      error: () => (this.error = this.languageService.translate('categories.error.update'))
    });
  }

  deleteCategory(category: Category) {
    if (!confirm(this.languageService.translate('categories.deleteConfirm', { name: category.name }))) return;

    this.categoryService.delete(category.id).subscribe({
      next: () => this.loadCategories(),
      error: () => (this.error = this.languageService.translate('categories.error.delete'))
    });
  }
}
