import { Injectable, signal } from '@angular/core';
import { Language, TranslationKey, translations } from '../i18n/translations';

const STORAGE_KEY = 'app-language';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  readonly language = signal<Language>(this.loadLanguage());

  constructor() {
    this.applyDocumentLanguage(this.language());
  }

  setLanguage(language: Language): void {
    this.language.set(language);
    localStorage.setItem(STORAGE_KEY, language);
    this.applyDocumentLanguage(language);
  }

  translate(key: TranslationKey, params?: Record<string, string | number>): string {
    let text: string = translations[this.language()][key] ?? key;

    if (params) {
      for (const [name, value] of Object.entries(params)) {
        text = text.replace(`{{${name}}}`, String(value));
      }
    }

    return text;
  }

  private loadLanguage(): Language {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored === 'en' || stored === 'uk' ? stored : 'uk';
  }

  private applyDocumentLanguage(language: Language): void {
    document.documentElement.lang = language;
    document.title = translations[language]['app.title'];
  }
}
