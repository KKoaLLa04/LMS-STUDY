import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-oc-category-chips',
  standalone: true,
  templateUrl: './category-chips.component.html',
  styleUrl: './category-chips.component.scss',
})
export class CategoryChipsComponent {
  @Input({ required: true }) categories: string[] = [];
  @Input() activeCategory = '';
  @Output() activeCategoryChange = new EventEmitter<string>();
}
