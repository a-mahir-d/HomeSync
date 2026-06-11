import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-home',
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  public langService = inject(LanguageService);
  private router = inject(Router);

  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }
}
