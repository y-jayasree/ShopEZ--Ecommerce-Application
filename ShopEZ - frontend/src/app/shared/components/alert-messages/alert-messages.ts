import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-alert-messages',
  standalone:true,
  imports: [CommonModule],
  templateUrl: './alert-messages.html',
  styleUrl: './alert-messages.css',
})
export class AlertMessages {
  @Input() message = '';
  @Input() type: 'success' | 'danger' | 'info' | 'warning' = 'info';
  @Output() dismiss = new EventEmitter<void>();
}
