import { Injectable, signal, effect } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private getInitialTheme(): ThemeMode {
    if (typeof window !== 'undefined') {
      const savedTheme = localStorage.getItem('theme') as ThemeMode;
      if (savedTheme === 'light' || savedTheme === 'dark') {
        return savedTheme;
      }
      
      const systemPrefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      return systemPrefersDark ? 'dark' : 'light';
    }
    return 'light';
  }

  private themeSignal = signal<ThemeMode>(this.getInitialTheme());
  currentTheme = this.themeSignal.asReadonly();

  constructor() {
    effect(() => {
      const mode = this.themeSignal();
      if (typeof window !== 'undefined') {
        if (mode === 'dark') {
          document.documentElement.classList.add('dark');
        } else {
          document.documentElement.classList.remove('dark');
        }
        localStorage.setItem('theme', mode);
      }
    });
  }

  toggleTheme(): void {
    this.themeSignal.update((prev) => (prev === 'light' ? 'dark' : 'light'));
  }

  setTheme(mode: ThemeMode): void {
    this.themeSignal.set(mode);
  }
}