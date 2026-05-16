import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-rating',
  standalone:true,
  imports: [CommonModule],
  templateUrl: './rating.html',
  styleUrl: './rating.css',
})
export class Rating {
  @Input() rating = 0;
  @Input() count = 0;
  @Input() showCount = true;
  @Input() size = 14;
 
  get stars(): ('full' | 'half' | 'empty')[] {
    return Array.from({ length: 5 }, (_, i) => {
      const diff = this.rating - i;
      if (diff >= 1) return 'full';
      if (diff >= 0.5) return 'half';
      return 'empty';
    });
  }
}
