import { HttpClient } from "@angular/common/http";
import { inject, Injectable, signal } from "@angular/core";
import { firstValueFrom } from "rxjs";

export type SupportedLang = 'en' | 'tr';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  private http = inject(HttpClient);
  private currentLangSignal = signal<SupportedLang>('en');
  private translationsSignal = signal<Record<string, string>>({});

  currentLang = this.currentLangSignal.asReadonly();
  
  constructor() {
    if (typeof window !== 'undefined') {
      const savedLang = localStorage.getItem('lang') as SupportedLang;
      const browserLang = navigator.language.split('-')[0] as SupportedLang;
      const initialLang = savedLang || (browserLang === 'tr' ? 'tr' : 'en');
      
      this.setLanguage(initialLang);
    } else {
      this.loadTranslations('en');
    }
  }

  async setLanguage(lang: SupportedLang): Promise<void> {
    this.currentLangSignal.set(lang);
    if (typeof window !== 'undefined') {
      localStorage.setItem('lang', lang);
    }
    await this.loadTranslations(lang);
  }

  private async loadTranslations(lang: SupportedLang): Promise<void> {
    try {
      const dictionary = await firstValueFrom(
        this.http.get<Record<string, string>>(`/i18n/${lang}.json`)
      );
      this.translationsSignal.set(dictionary);
    } catch (error) {
      console.error(`Failed to load translations layout for language: ${lang}`, error);
    }
  }

  translate(key: string): string {
    const keys = key.split('.');
    let currentStructure: any = this.translationsSignal();

    for (const k of keys) {
      if (currentStructure && Object.prototype.hasOwnProperty.call(currentStructure, k)) {
        currentStructure = currentStructure[k];
      } else {
        return key;
      }
    }

    return typeof currentStructure === 'string' ? currentStructure : key;
  }
}