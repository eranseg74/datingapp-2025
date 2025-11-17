import { Directive, inject, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { AccountService } from '../../core/services/account-service';

@Directive({
  selector: '[appHasRole]',
})
export class HasRole implements OnInit {
  @Input() appHasRole: string[] = [];
  private accountService = inject(AccountService);
  // The ViewContainerRef represents a container where one or more views can be attached to a component.
  private viewContainerRef = inject(ViewContainerRef);
  // The TemplateRef represents an embedded template that can be used to instantiate embedded views. To instantiate embedded views based on a template, use the ViewContainerRef method createEmbeddedView().
  // In order to access a TemplateRef instance we need to place a directive on an <ng-template> element (or directive prefixed with *). The TemplateRef for the embedded view is injected into the constructor of the directive, using the TemplateRef token
  private templateRef = inject(TemplateRef);

  ngOnInit(): void {
    // Checking if the current user has a role that is in the appHasRole array. If so, the user is allowed to see the component
    if (this.accountService.currentUser()?.roles?.some((r) => this.appHasRole.includes(r))) {
      // If the user can see the component, we instantiate an embedded view and inserts it into this container. The templateRef refers to the HTML template that defines the view.
      this.viewContainerRef.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainerRef.clear();
    }
  }
}
