import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  username = '';
  password = '';
  error = '';
  loading = false;

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    this.error = '';
    this.loading = true;

    this.auth.register({ username: this.username, password: this.password }).subscribe({
      next: () => this.router.navigate(['/tasks']),
      error: () => {
        this.error = 'Registration failed. Username may already exist.';
        this.loading = false;
      },
      complete: () => (this.loading = false)
    });
  }
}
