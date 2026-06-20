import { Pipe, PipeTransform } from '@angular/core';
import { TranslationKey } from '../i18n/translations';
import { LanguageService } from '../services/language.service';

@Pipe({
  name: 't',
  standalone: true,
  pure: false
})
export class TranslatePipe implements PipeTransform {
  constructor(private languageService: LanguageService) {}

  transform(key: TranslationKey, params?: Record<string, string | number>): string {
    this.languageService.language();
    return this.languageService.translate(key, params);
  }
}
