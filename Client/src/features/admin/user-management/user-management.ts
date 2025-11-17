import { Component, ElementRef, inject, OnInit, signal, ViewChild } from '@angular/core';
import { AdminService } from '../../../core/services/admin-service';
import { User } from '../../../types/user';

@Component({
  selector: 'app-user-management',
  imports: [],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css',
})
export class UserManagement implements OnInit {
  @ViewChild('rolesModal') rolesModal!: ElementRef<HTMLDialogElement>;
  private adminService = inject(AdminService);
  protected users = signal<User[]>([]);
  protected availableRoles = ['Member', 'Moderator', 'Admin'];
  protected selectedUser: User | null = null;

  ngOnInit(): void {
    this.getUserWithRoles();
  }

  getUserWithRoles() {
    this.adminService.getUserWithRoles().subscribe({
      next: (users) => {
        users.sort((u1: User, u2: User) => {
          return u1.email.localeCompare(u2.email);
        });
        return this.users.set(users);
      },
    });
  }

  openRolesModal(user: User) {
    this.selectedUser = user;
    this.rolesModal.nativeElement.showModal();
  }

  toggleRole(event: Event, role: string) {
    if (!this.selectedUser) return;
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      this.selectedUser.roles.push(role);
    } else {
      // If not checked, remove the role from the roles list
      this.selectedUser.roles = this.selectedUser.roles.filter((r) => r !== role);
    }
  }

  updateRoles() {
    // If there is no selected user than the whole function is not relevant
    if (!this.selectedUser) return;
    // Updating the roles for the selected user. The update function returns the updated roles for the selected user. We are using the roles list to update the users signal in this component so we will be able to update the UI by looping on all the users until we find a match with the selected user id and then updating its roles
    this.adminService.updateUserRoles(this.selectedUser.id, this.selectedUser.roles).subscribe({
      next: (updateRoles) => {
        this.users.update((users) =>
          users.map((u) => {
            if (u.id === this.selectedUser?.id) u.roles = updateRoles;
            return u;
          })
        );
        this.rolesModal.nativeElement.close();
      },
      error: (error) => console.error('Failed to update roles', error),
    });
  }
}
