import { Component, ElementRef, EventEmitter, HostListener, Input, OnInit, Output, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { OcIconComponent } from '../icon/icon.component';
import { AuthService } from '../../../core/auth/auth.service';
import { CurrentUser } from '../../../core/auth/models/auth.model';

@Component({
  selector: 'app-oc-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, OcIconComponent],
  templateUrl: './client-header.component.html',
  styleUrl: './client-header.component.scss',
})
export class ClientHeaderComponent implements OnInit {
  @Input() searchQuery = '';
  @Output() searchQueryChange = new EventEmitter<string>();

  private readonly authService = inject(AuthService);
  private readonly elementRef = inject(ElementRef<HTMLElement>);

  readonly isLoggedIn = signal(false);
  readonly currentUser = signal<CurrentUser | null>(null);
  readonly menuOpen = signal(false);
  readonly isAdminOrTeacher = signal(false);

  ngOnInit(): void {
    this.isLoggedIn.set(this.authService.isLoggedIn());
    if (this.isLoggedIn()) {
      const role = this.authService.getRole()?.toLowerCase();
      this.isAdminOrTeacher.set(role === 'admin' || role === 'teacher');
      this.authService.getCurrentUser().subscribe((user) => this.currentUser.set(user));
    }
  }

  onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchQueryChange.emit(value);
  }

  toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }

  onLogout(): void {
    this.closeMenu();
    this.authService.logout();
  }

  avatarInitials(): string {
    const name = this.currentUser()?.fullName?.trim();
    if (!name) return '?';
    const parts = name.split(/\s+/);
    const initials = parts.length > 1 ? parts[0][0] + parts[parts.length - 1][0] : parts[0].slice(0, 2);
    return initials.toUpperCase();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.menuOpen() && !this.elementRef.nativeElement.contains(event.target as Node)) {
      this.closeMenu();
    }
  }
}
