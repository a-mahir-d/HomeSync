import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ThemeService } from '../../services/theme.service';
import { LanguageService } from '../../services/language.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  imports: [CommonModule, RouterLink],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  public themeService = inject(ThemeService);
  public langService = inject(LanguageService);
  public authService = inject(AuthService);
  public router = inject(Router);

  isMobileMenuOpen = signal<boolean>(false);

  toggleMobileMenu(): void {
    this.isMobileMenuOpen.update(state => !state);
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  toggleLanguage(): void {
    const nextLang = this.langService.currentLang() === 'en' ? 'tr' : 'en';
    this.langService.setLanguage(nextLang);
  }

  logoutAndRedirect() {
    this.authService.logout();
    this.router.navigate(['/']);
  }
}
