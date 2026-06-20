import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LanguageService } from '../../services/language.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { LanguageSwitcherComponent } from '../language-switcher/language-switcher.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe, LanguageSwitcherComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  username = '';
  password = '';
  error = '';
  loading = false;

  constructor(
    private auth: AuthService,
    private router: Router,
    private languageService: LanguageService
  ) {}

  submit() {
    this.error = '';
    this.loading = true;

    this.auth.login({ username: this.username, password: this.password }).subscribe({
      next: () => this.router.navigate(['/tasks']),
      error: () => {
        this.error = this.languageService.translate('login.error');
        this.loading = false;
      },
      complete: () => (this.loading = false)
    });
  }
}
