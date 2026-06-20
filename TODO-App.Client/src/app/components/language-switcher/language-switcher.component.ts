import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Language } from '../../i18n/translations';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-language-switcher',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './language-switcher.component.html'
})
export class LanguageSwitcherComponent {
  constructor(public languageService: LanguageService) {}

  setLanguage(language: Language): void {
    this.languageService.setLanguage(language);
  }
}
