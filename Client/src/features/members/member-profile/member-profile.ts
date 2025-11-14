import {
  Component,
  HostListener,
  inject,
  OnDestroy,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import { EditableMember, Member } from '../../../types/member';
import { DatePipe } from '@angular/common';
import { MemberService } from '../../../core/services/member-service';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastService } from '../../../core/services/toast-service';
import { AccountService } from '../../../core/services/account-service';
import { TimeAgoPipe } from '../../../core/pipes/time-ago-pipe';

@Component({
  selector: 'app-member-profile',
  imports: [DatePipe, FormsModule, TimeAgoPipe],
  templateUrl: './member-profile.html',
  styleUrl: './member-profile.css',
})
export class MemberProfile implements OnInit, OnDestroy {
  @ViewChild('editForm') editForm?: NgForm;
  // The host in the HostListener is the browser that will listen to events. In this case we want to stop the system from proceeding when the user closes the application by clicking on the x button or the home icon. The browser will prompt a warning message waiting for the user to respond if to return to the page or leave without saving the changes
  @HostListener('window:beforeunload', ['$event']) notify($event: BeforeUnloadEvent) {
    if (this.editForm?.dirty) {
      $event.preventDefault();
    }
  }
  // Inporting the account service so we can set the current user parameter there whenever the profile is updated so the name in the navbar will change accordingly
  private accountService = inject(AccountService);
  protected memberService = inject(MemberService);
  private toast = inject(ToastService);
  // private route = inject(ActivatedRoute);
  // protected member = signal<Member | undefined>(undefined);
  protected editableMember: EditableMember = {
    // Initializing the editableMember here. If we will do it in the constructor the values will be set to empty string but will not change. We need to set them here and then set them again in the ngOnInit, after getting the values from the from the members detail (route.parent) to the member signal. Because of the zoneless detection it does not automatically update the fields so we have to do it this way
    displayName: '',
    description: '',
    city: '',
    country: '',
  };

  ngOnInit(): void {
    // this.route.parent?.data.subscribe((data) => {
    //   this.member.set(data['member']);
    // });

    this.editableMember = {
      displayName: this.memberService.member()?.displayName || '',
      description: this.memberService.member()?.description || '',
      city: this.memberService.member()?.city || '',
      country: this.memberService.member()?.country || '',
    };
  }

  updateProfile() {
    if (!this.memberService.member()) return;
    const updateMember = { ...this.memberService.member(), ...this.editableMember };
    this.memberService.updateMember(this.editableMember).subscribe({
      next: () => {
        // Getting a copy of the current user in the account service
        const currentUser = this.accountService.currentUser();
        if (currentUser && updateMember.displayName !== currentUser.displayName) {
          currentUser.displayName = updateMember.displayName;
          // Updating the current user in the account service
          this.accountService.setCurrentUser(currentUser);
        }
        this.toast.success('Profile updated successfully');
        this.memberService.editMode.set(false);
        this.memberService.member.set(updateMember as Member);
        this.editForm?.reset(updateMember);
      },
    });
  }

  ngOnDestroy(): void {
    if (this.memberService.editMode()) {
      this.memberService.editMode.set(false);
    }
  }
}
